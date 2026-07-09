using Game.Modules.GModuleScene;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RegionManager
{
    public List<GridRegion> regions = new List<GridRegion>();
    public int activeRegionId = -1;
    public List<RegionType> regionTypes = new List<RegionType>();
    private string newRegionTypeName = "";
    private Color newRegionTypeColor = Color.white;
    private Vector2 regionTypeScroll;
    private int sortOrder = 0; // 排序方式
    public GridRegion ActiveRegion => GetRegion(activeRegionId);

    public GridRegion GetRegion(int id)
    {
        if (id == -1) return null;
        return regions.Find(r => r.id == id);
    }

    public GridRegion CreateRegion(string name = "New Region")
    {
        int newId = regions.Count > 0 ? regions[regions.Count - 1].id + 1 : 0;
        var region = new GridRegion(newId, name);
        regions.Add(region);
        return region;
    }

    public void DeleteRegion(int id)
    {
        regions.RemoveAll(r => r.id == id);
        if (activeRegionId == id) activeRegionId = -1;
    }

    public void DrawRegionTypeManagement(float availableWidth = 0)
    {
        EditorGUILayout.LabelField("区域类型管理", EditorStyles.boldLabel);

        // 排序功能
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("排序:", GUILayout.Width(40));
        if (GUILayout.Button("默认", GUILayout.Width(60))) sortOrder = 0;
        if (GUILayout.Button("按名称", GUILayout.Width(60))) sortOrder = 1;
        if (GUILayout.Button("按颜色", GUILayout.Width(60))) sortOrder = 2;
        EditorGUILayout.EndHorizontal();

        // 创建新区域类型
        EditorGUILayout.BeginHorizontal();
        newRegionTypeName = EditorGUILayout.TextField("名称", newRegionTypeName, GUILayout.Width(availableWidth > 0 ? availableWidth * 0.4f : 200));
        newRegionTypeColor = EditorGUILayout.ColorField("颜色", newRegionTypeColor, GUILayout.Width(availableWidth > 0 ? availableWidth * 0.3f : 150));
        if (GUILayout.Button("添加", GUILayout.Width(availableWidth > 0 ? availableWidth * 0.2f : 60)))
        {
            if (!string.IsNullOrEmpty(newRegionTypeName))
            {
                regionTypes.Add(new RegionType { name = newRegionTypeName, color = newRegionTypeColor });
                newRegionTypeName = "";
                newRegionTypeColor = Color.white;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 获取排序后的区域类型列表
        var sortedRegionTypes = GetSortedRegionTypes();

        // 区域类型列表 - 完整的管理功能
        float scrollHeight = Mathf.Min(regionTypes.Count * 25 + 10, 150);
        regionTypeScroll = EditorGUILayout.BeginScrollView(regionTypeScroll, GUILayout.Height(scrollHeight));

        for (int i = 0; i < sortedRegionTypes.Count; i++)
        {
            var regionType = sortedRegionTypes[i];
            int originalIndex = regionTypes.IndexOf(regionType);

            EditorGUILayout.BeginHorizontal();

            // 显示索引
            EditorGUILayout.LabelField($"[{originalIndex}]", GUILayout.Width(30));

            // 名称编辑
            regionType.name = EditorGUILayout.TextField(regionType.name, GUILayout.Width(availableWidth > 0 ? availableWidth * 0.25f : 100));

            // 颜色编辑
            regionType.color = EditorGUILayout.ColorField(regionType.color, GUILayout.Width(availableWidth > 0 ? availableWidth * 0.2f : 80));

            // 上下移动箭头
            EditorGUILayout.BeginVertical(GUILayout.Width(30));

            // 上移按钮
            GUI.enabled = originalIndex > 0;
            if (GUILayout.Button("↑", GUILayout.Height(12), GUILayout.Width(30)))
            {
                MoveRegionType(originalIndex, -1);
                break;
            }

            // 下移按钮
            GUI.enabled = originalIndex < regionTypes.Count - 1;
            if (GUILayout.Button("↓", GUILayout.Height(12), GUILayout.Width(30)))
            {
                MoveRegionType(originalIndex, 1);
                break;
            }

            GUI.enabled = true;
            EditorGUILayout.EndVertical();

            // 使用计数
            int usageCount = GetRegionTypeUsageCount(originalIndex);
            EditorGUILayout.LabelField($"使用:{usageCount}", GUILayout.Width(50));

            // 删除按钮
            if (GUILayout.Button("删除", GUILayout.Width(availableWidth > 0 ? availableWidth * 0.15f : 50)))
            {
                if (usageCount > 0)
                {
                    EditorUtility.DisplayDialog("无法删除", $"该区域类型正在被 {usageCount} 个区域使用，无法删除。", "确定");
                }
                else if (EditorUtility.DisplayDialog("确认删除", $"确定要删除区域类型 '{regionType.name}' 吗？", "删除", "取消"))
                {
                    regionTypes.RemoveAt(originalIndex);
                    break;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        if (regionTypes.Count == 0)
        {
            EditorGUILayout.HelpBox("暂无区域类型，请先添加区域类型。", MessageType.Info);
        }
    }

    // 移动区域类型
    private void MoveRegionType(int currentIndex, int direction)
    {
        if (direction == 0) return;

        int newIndex = currentIndex + direction;
        if (newIndex < 0 || newIndex >= regionTypes.Count) return;

        // 交换位置
        var temp = regionTypes[currentIndex];
        regionTypes[currentIndex] = regionTypes[newIndex];
        regionTypes[newIndex] = temp;
    }
    // 获取排序后的区域类型
    private List<RegionType> GetSortedRegionTypes()
    {
        var sorted = new List<RegionType>(regionTypes);

        switch (sortOrder)
        {
            case 1:
                sorted.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
                break;
            case 2:
                sorted.Sort((a, b) =>
                {
                    Color.RGBToHSV(a.color, out float h1, out float s1, out float v1);
                    Color.RGBToHSV(b.color, out float h2, out float s2, out float v2);
                    return h1.CompareTo(h2);
                });
                break;
        }

        return sorted;
    }

    // 获取区域类型使用计数
    private int GetRegionTypeUsageCount(int regionTypeId)
    {
        int count = 0;
        foreach (var region in regions)
        {
            if (region.regionTypeId == regionTypeId)
                count++;
        }
        return count;
    }

    // 保存区域数据到JSON
    public RegionDataWrapper ExportRegionData()
    {
        return new RegionDataWrapper
        {
            regions = new List<GridRegion>(regions),
            regionTypes = new List<RegionType>(regionTypes),
            activeRegionId = activeRegionId
        };
    }

    // 从JSON导入区域数据
    public void ImportRegionData(RegionDataWrapper data)
    {
        if (data != null)
        {
            regions = data.regions ?? new List<GridRegion>();
            regionTypes = data.regionTypes ?? new List<RegionType>();
            activeRegionId = data.activeRegionId;
        }
    }
}

[Serializable]
public class RegionType
{
    public string name;
    public Color color;
}

// 区域数据包装类用于JSON序列化
[Serializable]
public class RegionDataWrapper
{
    public List<GridRegion> regions = new List<GridRegion>();
    public List<RegionType> regionTypes = new List<RegionType>();
    public int activeRegionId = -1;
}
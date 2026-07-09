using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static GridTileEditorWindow;

public class LabelTypeManager
{
    public List<LabelType> LabelTypes = new List<LabelType>();
    private Vector2 labelTypeScroll;
    private string newLabelTypeName = "";
    private Color newLabelTypeColor = Color.white;
    private int sortOrder = 0;

    public void DrawLabelTypeManagement(ref int activeLabelTypeId, ref EditorMode editorMode, float availableWidth = 0)
    {
        EditorGUILayout.LabelField("标签类型管理", EditorStyles.boldLabel);

        // 排序功能
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("排序:", GUILayout.Width(40));
        if (GUILayout.Button("默认", GUILayout.Width(60))) sortOrder = 0;
        if (GUILayout.Button("按名称", GUILayout.Width(60))) sortOrder = 1;
        if (GUILayout.Button("按颜色", GUILayout.Width(60))) sortOrder = 2;
        EditorGUILayout.EndHorizontal();

        // 创建新标签类型
        EditorGUILayout.BeginHorizontal();
        newLabelTypeName = EditorGUILayout.TextField("名称", newLabelTypeName, GUILayout.Width(availableWidth > 0 ? availableWidth * 0.4f : 200));
        newLabelTypeColor = EditorGUILayout.ColorField("颜色", newLabelTypeColor, GUILayout.Width(availableWidth > 0 ? availableWidth * 0.3f : 150));
        if (GUILayout.Button("添加", GUILayout.Width(availableWidth > 0 ? availableWidth * 0.2f : 60)))
        {
            if (!string.IsNullOrEmpty(newLabelTypeName))
            {
                LabelTypes.Add(new LabelType { name = newLabelTypeName, color = newLabelTypeColor });
                newLabelTypeName = "";
                newLabelTypeColor = Color.white;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 获取排序后的标签列表
        var sortedLabelTypes = GetSortedLabelTypes();

        // 标签类型列表
        float scrollViewHeight = Mathf.Min(LabelTypes.Count * 25 + 10, 150);
        labelTypeScroll = EditorGUILayout.BeginScrollView(labelTypeScroll, GUILayout.Height(scrollViewHeight));

        for (int i = 0; i < sortedLabelTypes.Count; i++)
        {
            var labelType = sortedLabelTypes[i];
            int originalIndex = LabelTypes.IndexOf(labelType);

            EditorGUILayout.BeginHorizontal();

            // 选择按钮 - 显示下标
            bool isActive = activeLabelTypeId == originalIndex && editorMode == EditorMode.Paint;
            string selectButtonText = isActive ? $"✓[{originalIndex}]" : $"[{originalIndex}]选";
            if (GUILayout.Toggle(isActive, selectButtonText, GUILayout.Width(50)))
            {
                activeLabelTypeId = originalIndex;
                editorMode = EditorMode.Paint;
            }
            else if (isActive)
            {
                activeLabelTypeId = -1;
            }

            // 标签信息 - 显示下标但不修改名称
            EditorGUILayout.LabelField($"[{originalIndex}]", GUILayout.Width(30));
            labelType.name = EditorGUILayout.TextField(labelType.name, GUILayout.Width(availableWidth > 0 ? availableWidth * 0.25f : 100));
            labelType.color = EditorGUILayout.ColorField(labelType.color, GUILayout.Width(availableWidth > 0 ? availableWidth * 0.2f : 80));

            // 上下移动箭头
            EditorGUILayout.BeginVertical(GUILayout.Width(30));

            // 上移按钮
            GUI.enabled = originalIndex > 0; // 第一个不能上移
            if (GUILayout.Button("↑", GUILayout.Height(12), GUILayout.Width(30)))
            {
                MoveLabelType(originalIndex, -1);
                // 更新活动标签ID
                if (activeLabelTypeId == originalIndex)
                    activeLabelTypeId = originalIndex - 1;
                else if (activeLabelTypeId == originalIndex - 1)
                    activeLabelTypeId = originalIndex;
                break;
            }

            // 下移按钮
            GUI.enabled = originalIndex < LabelTypes.Count - 1; // 最后一个不能下移
            if (GUILayout.Button("↓", GUILayout.Height(12), GUILayout.Width(30)))
            {
                MoveLabelType(originalIndex, 1);
                // 更新活动标签ID
                if (activeLabelTypeId == originalIndex)
                    activeLabelTypeId = originalIndex + 1;
                else if (activeLabelTypeId == originalIndex + 1)
                    activeLabelTypeId = originalIndex;
                break;
            }

            GUI.enabled = true;
            EditorGUILayout.EndVertical();

            // 删除按钮
            if (GUILayout.Button("删除", GUILayout.Width(availableWidth > 0 ? availableWidth * 0.15f : 50)))
            {
                if (EditorUtility.DisplayDialog("确认删除", $"确定要删除标签类型 '{labelType.name}' 吗？", "删除", "取消"))
                {
                    LabelTypes.RemoveAt(originalIndex);
                    if (activeLabelTypeId == originalIndex) activeLabelTypeId = -1;
                    break;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        if (LabelTypes.Count == 0)
        {
            EditorGUILayout.HelpBox("暂无标签类型，请先添加标签类型。", MessageType.Info);
        }
    }

    // 移动标签类型
    private void MoveLabelType(int currentIndex, int direction)
    {
        if (direction == 0) return;

        int newIndex = currentIndex + direction;
        if (newIndex < 0 || newIndex >= LabelTypes.Count) return;

        // 交换位置
        var temp = LabelTypes[currentIndex];
        LabelTypes[currentIndex] = LabelTypes[newIndex];
        LabelTypes[newIndex] = temp;
    }

    private List<LabelType> GetSortedLabelTypes()
    {
        var sorted = new List<LabelType>(LabelTypes);

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

    public void DrawLabelTypeManagement(ref int activeLabelTypeId, ref EditorMode editorMode)
    {
        DrawLabelTypeManagement(ref activeLabelTypeId, ref editorMode, 0);
    }

    public void ImportLabelTypes(List<LabelType> labelTypes)
    {
        LabelTypes.Clear();
        LabelTypes.AddRange(labelTypes);
    }
}

[Serializable]
public class LabelType
{
    public string name;
    public Color color;
}
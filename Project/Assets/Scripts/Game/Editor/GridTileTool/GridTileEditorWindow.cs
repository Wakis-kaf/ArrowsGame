// 语言: C#
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Game.Modules.GModuleScene;
using static LabelTypeManager;
using Framework.Utils;

public class GridTileEditorWindow : EditorWindow
{
    private GridTile targetGrid;
    private GridTileMap currentMap;
    private Dictionary<int, GridCellMeta> metaByIndex = new Dictionary<int, GridCellMeta>();
    private int selectedIndex = -1;
    private Vector2 leftScroll, rightScroll;

    private EditorMode editorMode = EditorMode.Select;
    private BrushTool brushTool = BrushTool.Point;

    private int activeLabelTypeId = -1;
    private bool openPopupOnClick = true;
    private float brushRadius = 1.0f;
    private bool brushEraseMode = false;
    private bool isDraggingRect = false;
    private Vector3 rectStartWorld;
    private Vector3 rectCurrentWorld;
    private bool isDraggingCircle = false;
    private int lastClearedIndex = -1;
    private double lastClearedTime = 0.0;
    private const double clearSuppressSeconds = 0.25;
    private GridTile.CellSwizzle? lastSeenSwizzle = null;
    private Dictionary<int, Vector3> cachedUnswizzledLocalCenter = new Dictionary<int, Vector3>();
    private Dictionary<long, List<int>> spatialBuckets = new Dictionary<long, List<int>>();
    private float bucketSize = 1f;
    private Dictionary<int, GridCell> indexToCell = new Dictionary<int, GridCell>();
    private Vector3[] circlePreviewPoints = null;
    private int circlePreviewSegments = 0;

    // 距离裁剪设置
    private bool enableDistanceCulling = true;
    private float maxHandlerDistance = 10f;
    private bool debugCulling = false;
    private bool exportOnlyMarked = true;

    // 管理器
    private LabelTypeManager labelTypeManager;
    private SceneViewDrawer sceneViewDrawer;

    // 标签存储路径
    private const string LABEL_TYPES_SAVE_PATH = "Assets/Editor/GridTileLabelTypes.json";
    // 区域存储路径
    private const string REGION_DATA_SAVE_PATH = "Assets/Editor/GridTileRegionData.json";

    private RegionManager regionManager = new RegionManager();
    private Vector2 regionScroll;
    private bool showRegionPanel = true;

    // 布局控制
    private float leftPanelWidth = 0.35f;
    private float rightPanelWidth = 0.65f;
    private Vector2 scrollPosition;

    
  
    public enum EditorMode
    { Select = 0, Paint = 1 }

    public enum BrushTool
    { Point = 0, Rect = 1, Circle = 2 }

    [MenuItem("Tools/GridTile 编辑器")]
    public static void OpenWindow()
    {
        var w = GetWindow<GridTileEditorWindow>("GridTile 编辑器");
        w.minSize = new Vector2(800, 600);
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        Selection.selectionChanged += OnSelectionChanged;

        labelTypeManager = new LabelTypeManager();
        sceneViewDrawer = new SceneViewDrawer();

        LoadLabelTypes();
        LoadRegionData(); // 新增：加载区域数据

        if (regionManager.regions.Count == 0)
        {
            regionManager.CreateRegion("默认区域");
        }

        OnSelectionChanged();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (targetGrid == null || currentMap == null) return;

        // 检测swizzle变化
        if (lastSeenSwizzle == null || targetGrid.cellSwizzle != lastSeenSwizzle.Value)
        {
            RefreshMap();
        }

        float handleSizeBase = HandleUtility.GetHandleSize(targetGrid.transform.position) * 0.1f;

        // 计算swizzle轴
        Vector3 swizzleRight = targetGrid.transform.TransformDirection(GridTile.ReverseSwizzle(Vector3.right, targetGrid.cellSwizzle)).normalized;
        Vector3 swizzleUp = targetGrid.transform.TransformDirection(GridTile.ReverseSwizzle(Vector3.up, targetGrid.cellSwizzle)).normalized;
        Vector3 swizzleForward = targetGrid.transform.TransformDirection(GridTile.ReverseSwizzle(Vector3.forward, targetGrid.cellSwizzle)).normalized;

        if (swizzleRight.sqrMagnitude < 1e-6f) swizzleRight = targetGrid.transform.right;
        if (swizzleUp.sqrMagnitude < 1e-6f) swizzleUp = targetGrid.transform.up;
        if (swizzleForward.sqrMagnitude < 1e-6f) swizzleForward = targetGrid.transform.forward;

        Vector3 normal = swizzleForward;
        Camera cam = sceneView != null ? sceneView.camera : Camera.current;

        // 网格尺寸计算
        float gridDim = 1f;
        try { gridDim = Mathf.Max(0.01f, Mathf.Max(targetGrid.cellSize.x + targetGrid.gap.x, targetGrid.cellSize.y + targetGrid.gap.y)); }
        catch { gridDim = 1f; }

        // 鼠标射线检测
        Event ev = Event.current;
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(ev.mousePosition);
        Plane gridPlane = new Plane(normal, targetGrid.transform.position);
        bool hitPlane = gridPlane.Raycast(mouseRay, out float enter);
        Vector3 mouseWorld = hitPlane ? mouseRay.GetPoint(enter) : Vector3.zero;

        // 相机位置（用于距离计算）
        Vector3 cameraWorld = cam != null ? cam.transform.position : targetGrid.transform.position;

        bool forceShowAll = (ev != null && (ev.control || (ev.modifiers & EventModifiers.Alt) != 0));

        // 笔刷交互
        if (editorMode == EditorMode.Paint && targetGrid != null && hitPlane)
        {
            HandleBrushInteraction(ev, mouseWorld);
        }

        // 绘制所有格子
        DrawAllCells(cam, handleSizeBase, gridDim, normal, forceShowAll, cameraWorld);

        // F2快速弹窗
        if (ev != null && ev.type == EventType.KeyDown && ev.keyCode == KeyCode.F2 && selectedIndex >= 0)
        {
            ShowQuickLabelPopup(selectedIndex);
            ev.Use();
        }

        if (Event.current.type == EventType.Repaint) SceneView.RepaintAll();
    }

    private void HandleBrushInteraction(Event ev, Vector3 mouseWorld)
    {
        bool ctrlPressed = ev != null && ((ev.modifiers & EventModifiers.Control) != 0 || ev.control);
        bool isEraseNow = brushEraseMode || ctrlPressed;

        switch (brushTool)
        {
            case BrushTool.Rect:
                HandleRectBrush(ev, mouseWorld, isEraseNow);
                break;
            case BrushTool.Circle:
                HandleCircleBrush(ev, mouseWorld, isEraseNow);
                break;
        }
    }

    private void HandleRectBrush(Event ev, Vector3 mouseWorld, bool isEraseNow)
    {
        if (ev.type == EventType.MouseDown && ev.button == 0)
        {
            isDraggingRect = true;
            rectStartWorld = mouseWorld;
            rectCurrentWorld = mouseWorld;
            ev.Use();
        }
        else if (ev.type == EventType.MouseDrag && isDraggingRect)
        {
            rectCurrentWorld = mouseWorld;
            ev.Use();
            SceneView.RepaintAll();
        }
        else if (ev.type == EventType.MouseUp && isDraggingRect)
        {
            rectCurrentWorld = mouseWorld;
            ApplyRectBrush(rectStartWorld, rectCurrentWorld, isEraseNow);
            isDraggingRect = false;
            ev.Use();
        }

        if (isDraggingRect)
        {
            DrawRectPreview(rectStartWorld, rectCurrentWorld, 2f, Color.yellow);
        }
    }

    private void HandleCircleBrush(Event ev, Vector3 mouseWorld, bool isEraseNow)
    {
        DrawCirclePreview(mouseWorld, brushRadius, 64, Color.green);

        if (ev.type == EventType.MouseDown && ev.button == 0)
        {
            isDraggingCircle = true;
            ApplyCircleBrush(mouseWorld, brushRadius, isEraseNow);
            ev.Use();
        }
        else if (ev.type == EventType.MouseDrag && isDraggingCircle)
        {
            ApplyCircleBrush(mouseWorld, brushRadius, isEraseNow);
            ev.Use();
        }
        else if (ev.type == EventType.MouseUp && isDraggingCircle)
        {
            isDraggingCircle = false;
            ev.Use();
        }
    }

    private void DrawAllCells(Camera cam, float handleSizeBase, float gridDim, Vector3 normal, bool forceShowAll, Vector3 cameraWorld)
    {
        int labelTypesCount = labelTypeManager.LabelTypes.Count;
        var fontSizeLabel = Mathf.Clamp(Mathf.RoundToInt(10 + handleSizeBase * 28f), 12, 44);
        var fontSizeIndex = Mathf.Clamp(Mathf.RoundToInt(10 + handleSizeBase * 18f), 12, 30);

        var indexStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, fontSize = fontSizeIndex };
        indexStyle.normal.textColor = Color.black;
        var indexShadow = new GUIStyle(indexStyle); indexShadow.normal.textColor = new Color(0, 0, 0, 0.85f);

        var labelStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = fontSizeLabel };
        var labelShadow = new GUIStyle(labelStyle); labelShadow.normal.textColor = new Color(0, 0, 0, 0.85f);

        float maxDistSqr = maxHandlerDistance * maxHandlerDistance;
        int drawnCount = 0;
        int culledByDistance = 0;
        int culledByViewport = 0;

        int cellCount = currentMap.cells.Count;
        for (int i = 0; i < cellCount; i++)
        {
            var cell = currentMap.cells[i];
            Vector3 cellWorldPos = cell.worldCenter;
            metaByIndex.TryGetValue(cell.index, out var meta);
            bool isSelected = cell.index == selectedIndex;
            bool hasMark = meta != null && (meta.labelTypeIds != null && meta.labelTypeIds.Count > 0 || (meta.properties != null && meta.properties.Count > 0));

            // 强制显示条件
            bool forceDraw = forceShowAll || isSelected || hasMark;

            // 距离裁剪
            if (enableDistanceCulling && !forceDraw && cam != null)
            {
                Vector3 scaledWorldPos = cellWorldPos;
                if (targetGrid.transform.localScale != Vector3.one)
                {
                    var v3 = targetGrid.transform.localScale;
                    scaledWorldPos = Vector3.Scale(cellWorldPos - targetGrid.transform.position, new Vector3(1 / v3.x, 1 / v3.y, 1 / v3.z)) + targetGrid.transform.position;
                }

                float dist = Vector3.Distance(cameraWorld, scaledWorldPos);
                if (dist > maxHandlerDistance)
                {
                    culledByDistance++;
                    continue;
                }
            }

            // 视锥裁剪
            if (!forceDraw && cam != null && !IsWorldPointInViewport(cam, cellWorldPos))
            {
                culledByViewport++;
                continue;
            }

            // 绘制格子
            drawnCount++;
            DrawSingleCell(cell, meta, isSelected, cam, handleSizeBase, gridDim, normal, indexStyle, indexShadow, labelStyle, labelShadow);
        }

        // 调试日志
        if (debugCulling && Event.current != null && Event.current.type == EventType.Repaint)
        {
            Debug.Log($"[GridTileCulling] 绘制统计 - 总格子数: {cellCount}, 绘制数: {drawnCount}, 距离裁剪数: {culledByDistance}, 视锥裁剪数: {culledByViewport}");
        }
    }


    private Vector3 GetSwizzleForward()
    {
        return targetGrid.transform.TransformDirection(GridTile.ReverseSwizzle(Vector3.forward, targetGrid.cellSwizzle)).normalized;
    }

    private Vector3 GetSwizzleUp()
    {
        return targetGrid.transform.TransformDirection(GridTile.ReverseSwizzle(Vector3.up, targetGrid.cellSwizzle)).normalized;
    }

    private void ShowQuickLabelPopup(int cellIndex)
    {
        metaByIndex.TryGetValue(cellIndex, out var existingMeta);

        GridLabelQuickPopup.Show(cellIndex, existingMeta, labelTypeManager.LabelTypes, (newLabelTypeIds) =>
        {
            if (newLabelTypeIds == null || newLabelTypeIds.Count == 0)
            {
                if (metaByIndex.ContainsKey(cellIndex)) metaByIndex.Remove(cellIndex);
                lastClearedIndex = cellIndex;
                lastClearedTime = EditorApplication.timeSinceStartup;
            }
            else
            {
                var cell = GetCellByIndex(cellIndex);
                if (!metaByIndex.TryGetValue(cellIndex, out var meta))
                    meta = GridCellMeta.Create(cellIndex, cell?.GetIndex() ?? Vector2Int.zero, new List<int>());
                meta.labelTypeIds = newLabelTypeIds;
                metaByIndex[cellIndex] = meta;
            }
            Repaint();
            SceneView.RepaintAll();
        });
    }

    private Vector3 WorldToUnswizzledLocal(Vector3 world)
    {
        Vector3 local = targetGrid.transform.InverseTransformPoint(world);
        return GridTile.ReverseSwizzle(local, targetGrid.cellSwizzle);
    }

    private Vector3 UnswizzledLocalToWorld(Vector3 unswizzledLocal)
    {
        Vector3 sw = GridTile.ReverseSwizzle(unswizzledLocal, targetGrid.cellSwizzle);
        return targetGrid.transform.TransformPoint(sw);
    }

    private bool IsWorldPointInViewport(Camera cam, Vector3 worldPos)
    {
        if (cam == null) return true;

        Vector3 vp = cam.WorldToViewportPoint(worldPos);
        bool inZ = vp.z > -0.1f;
        bool inX = vp.x >= -0.2f && vp.x <= 1.2f;
        bool inY = vp.y >= -0.2f && vp.y <= 1.2f;

        return inZ && inX && inY;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        Selection.selectionChanged -= OnSelectionChanged;
        SaveLabelTypes();
        SaveRegionData(); // 新增：保存区域数据
    }
    // 保存区域数据到JSON文件
    private void SaveRegionData()
    {
        try
        {
            string directory = Path.GetDirectoryName(REGION_DATA_SAVE_PATH);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var regionData = regionManager.ExportRegionData();
            //string json = JsonUtility.ToJson(regionData, true);
            string json = Utility.Json.ToJson(regionData);
            File.WriteAllText(REGION_DATA_SAVE_PATH, json);
            AssetDatabase.Refresh();
        }
        catch (Exception e)
        {
            Debug.LogError($"保存区域数据失败: {e.Message}");
        }
    }

    // 从JSON文件加载区域数据
    private void LoadRegionData()
    {
        try
        {
            if (File.Exists(REGION_DATA_SAVE_PATH))
            {
                string json = File.ReadAllText(REGION_DATA_SAVE_PATH);
                var regionData = JsonUtility.FromJson<RegionDataWrapper>(json);
                if (regionData != null)
                {
                    regionManager.ImportRegionData(regionData);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"加载区域数据失败: {e.Message}");
        }
    }
    // 保存标签类型到JSON文件
    private void SaveLabelTypes()
    {
        try
        {
            string directory = Path.GetDirectoryName(LABEL_TYPES_SAVE_PATH);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = Utility.Json.ToJson(new LabelTypeListWrapper { labelTypes = labelTypeManager.LabelTypes });
            File.WriteAllText(LABEL_TYPES_SAVE_PATH, json);
            AssetDatabase.Refresh();
        }
        catch (Exception e)
        {
            Debug.LogError($"保存标签类型失败: {e.Message}");
        }
    }

    // 从JSON文件加载标签类型
    private void LoadLabelTypes()
    {
        try
        {
            if (File.Exists(LABEL_TYPES_SAVE_PATH))
            {
                string json = File.ReadAllText(LABEL_TYPES_SAVE_PATH);
                var wrapper = JsonUtility.FromJson<LabelTypeListWrapper>(json);
                if (wrapper != null && wrapper.labelTypes != null)
                {
                    labelTypeManager.ImportLabelTypes(wrapper.labelTypes);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"加载标签类型失败: {e.Message}");
        }
    }

    // 包装类用于JSON序列化
    [Serializable]
    private class LabelTypeListWrapper
    {
        public List<LabelType> labelTypes = new List<LabelType>();
    }

    private void OnSelectionChanged()
    {
        var go = Selection.activeGameObject;
        if (go != null)
        {
            var gt = go.GetComponent<GridTile>();
            if (gt != targetGrid)
            {
                targetGrid = gt;
                RefreshMap();
                Repaint();
            }
        }
    }

    private void RefreshMap()
    {
        selectedIndex = -1;
        GridTileMap newMap = null;

        cachedUnswizzledLocalCenter.Clear();
        spatialBuckets.Clear();
        indexToCell.Clear();

        if (targetGrid == null)
        {
            currentMap = null;
            lastSeenSwizzle = null;
            return;
        }

        var backupMeta = new Dictionary<int, GridCellMeta>(metaByIndex);

        try { newMap = targetGrid.BuildGridMap(); }
        catch (Exception e)
        {
            Debug.LogError("BuildGridMap 出错: " + e);
            newMap = null;
        }

        if (newMap == null)
        {
            currentMap = null;
            return;
        }

        metaByIndex.Clear();

        bucketSize = Mathf.Max(0.0001f, Mathf.Max(newMap.cellSize.x + newMap.gap.x, newMap.cellSize.y + newMap.gap.y));

        for (int i = 0; i < newMap.cells.Count; i++)
        {
            var cell = newMap.cells[i];
            indexToCell[cell.index] = cell;

            if (backupMeta.TryGetValue(cell.index, out var oldMeta))
            {
                metaByIndex[cell.index] = GridCellMeta.Create(
                    cell.index,
                    cell.GetIndex(),
                    oldMeta.labelTypeIds,
                    oldMeta.properties,
                    oldMeta.regionId
                );
            }

            cachedUnswizzledLocalCenter[cell.index] = cell.localCenter;

            int bx = Mathf.FloorToInt(cell.localCenter.x / bucketSize);
            int by = Mathf.FloorToInt(cell.localCenter.y / bucketSize);
            long key = ((long)bx << 32) | (uint)by;
            if (!spatialBuckets.TryGetValue(key, out var list))
            {
                list = new List<int>();
                spatialBuckets[key] = list;
            }
            list.Add(cell.index);
        }

        currentMap = newMap;
        lastSeenSwizzle = targetGrid.cellSwizzle;

        if (debugCulling)
            Debug.Log($"[GridTileCulling] 刷新地图完成 - GridTile: {targetGrid.name}, 格子数量: {currentMap.cells.Count}, 缩放: {targetGrid.transform.localScale}");
    }

    private void OnGUI()
    {
        // 动态调整布局
        float windowWidth = position.width;

        // 小窗口时的简化布局
        if (windowWidth < 900)
        {
            DrawCompactLayout();
        }
        else
        {
            DrawFullLayout();
        }
    }

    private void DrawFullLayout()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // 顶部工具栏
        DrawTopToolbar();

        EditorGUILayout.Space();

        // 中间区域：区域管理 + 标签管理
        EditorGUILayout.BeginHorizontal();

        // 左侧：区域管理
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(position.width * 0.4f));
        DrawRegionPanel();
        EditorGUILayout.EndVertical();

        // 右侧：标签管理 - 修复滚动条问题
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(position.width * 0.6f));
        labelTypeManager.DrawLabelTypeManagement(ref activeLabelTypeId, ref editorMode, position.width * 0.6f - 40);
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 底部：模式设置和格子编辑
        DrawBottomPanels();

        EditorGUILayout.EndScrollView();
    }

    private void DrawCompactLayout()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // 顶部工具栏
        DrawTopToolbar();

        EditorGUILayout.Space();

        // 区域管理（折叠式）
        showRegionPanel = EditorGUILayout.Foldout(showRegionPanel, "区域管理", true);
        if (showRegionPanel)
        {
            DrawRegionPanelCompact();
        }

        EditorGUILayout.Space();

        // 标签管理 - 修复滚动条问题
        EditorGUILayout.BeginVertical(GUI.skin.box);
        labelTypeManager.DrawLabelTypeManagement(ref activeLabelTypeId, ref editorMode, position.width - 40);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // 底部：模式设置和格子编辑
        DrawBottomPanels();

        EditorGUILayout.EndScrollView();
    }

    private void DrawTopToolbar()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("目标GridTile:", GUILayout.Width(80));
        var newTarget = (GridTile)EditorGUILayout.ObjectField(targetGrid, typeof(GridTile), true);
        if (newTarget != targetGrid)
        {
            targetGrid = newTarget;
            RefreshMap();
        }

        if (GUILayout.Button("刷新", GUILayout.Width(60))) RefreshMap();
        if (GUILayout.Button("导入", GUILayout.Width(60))) ImportJson();
        if (GUILayout.Button("导出", GUILayout.Width(60))) ExportJson();
        if (GUILayout.Button("清空", GUILayout.Width(60)))
        {
            if (EditorUtility.DisplayDialog("确认", "清空当前编辑器中的所有格子元数据？", "是", "否"))
            {
                metaByIndex.Clear();
                selectedIndex = -1;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        exportOnlyMarked = EditorGUILayout.ToggleLeft("仅导出已标记格子", exportOnlyMarked, GUILayout.Width(150));
        EditorGUILayout.LabelField("（默认开启，便于体积与导入一致性）", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
    }
    private void ImportJson()
    {
        string path = EditorUtility.OpenFilePanel("导入 Grid 元数据 JSON", Application.dataPath, "json");
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            string json = File.ReadAllText(path);
            var gm = JsonUtility.FromJson<GridMeta>(json);
            if (gm == null)
            {
                EditorUtility.DisplayDialog("导入失败", "解析 JSON 失败或格式不正确。", "确定");
                return;
            }

            if (currentMap == null)
            {
                RefreshMap();
                if (currentMap == null)
                {
                    EditorUtility.DisplayDialog("导入失败", "当前 GridMap 未构建，无法映射导入的数据。请先刷新 GridMap。", "确定");
                    return;
                }
            }

            var validIndexes = new HashSet<int>();
            foreach (var c in currentMap.cells) validIndexes.Add(c.index);

            int applied = 0, skipped = 0;
            int noneLabelCells = 0;
            int regionCells = 0;
            int missingRegionCells = 0;

            // 创建区域ID映射表（JSON区域ID -> 本地区域ID）
            Dictionary<int, int> regionIdMap = new Dictionary<int, int>();

            // 处理导入的区域映射
            if (gm.regions != null)
            {
                foreach (var importRegion in gm.regions)
                {
                    if (importRegion.id == -1)
                    {
                        regionIdMap[-1] = -1; // 默认区域
                        continue;
                    }

                    // 查找本地是否存在相同ID的区域
                    var localRegion = regionManager.regions.Find(r => r.id == importRegion.id);
                    if (localRegion != null)
                    {
                        // 使用现有的本地区域
                        regionIdMap[importRegion.id] = importRegion.id;
                    }
                    else
                    {
                        // 创建新区域，但使用导入的区域ID
                        var newRegion = new GridRegion(importRegion.id, importRegion.name)
                        {
                            color = importRegion.color,
                            regionTypeId = importRegion.regionTypeId
                        };
                        regionManager.regions.Add(newRegion);
                        regionIdMap[importRegion.id] = importRegion.id;
                    }
                }
            }

            // 处理每个区域的格子数据
            if (gm.regions != null)
            {
                foreach (var importRegion in gm.regions)
                {
                    if (importRegion.cells == null) continue;

                    foreach (var cellMeta in importRegion.cells)
                    {
                        if (!validIndexes.Contains(cellMeta.index))
                        {
                            skipped++;
                            continue;
                        }

                        // 处理标签：如果标签包含-1（None），则清空标签列表
                        List<int> finalLabelIds = new List<int>();
                        if (cellMeta.labelTypeIds != null)
                        {
                            foreach (var labelId in cellMeta.labelTypeIds)
                            {
                                if (labelId != -1) // 跳过None标签
                                {
                                    finalLabelIds.Add(labelId);
                                }
                                else
                                {
                                    noneLabelCells++;
                                }
                            }
                        }

                        // 映射区域ID
                        int localRegionId = -1;
                        if (regionIdMap.TryGetValue(importRegion.id, out int mappedRegionId))
                        {
                            localRegionId = mappedRegionId;
                        }
                        else
                        {
                            missingRegionCells++;
                        }

                        bool hasMeaningful = (finalLabelIds.Count > 0) ||
                                           (cellMeta.properties != null && cellMeta.properties.Count > 0);

                        if (hasMeaningful || finalLabelIds.Count == 0)
                        {
                            var newMeta = GridCellMeta.Create(
                                cellMeta.index,
                                GetCellByIndex(cellMeta.index)?.GetIndex() ?? Vector2Int.zero,
                                finalLabelIds,
                                cellMeta.properties,
                                localRegionId
                            );
                            metaByIndex[cellMeta.index] = newMeta;
                            applied++;

                            if (localRegionId != -1) regionCells++;
                        }
                        else
                        {
                            if (metaByIndex.ContainsKey(cellMeta.index))
                                metaByIndex.Remove(cellMeta.index);
                            skipped++;
                        }
                    }
                }
            }

            EditorUtility.DisplayDialog("导入完成",
                $"应用: {applied} 个元数据\n" +
                $"跳过: {skipped} 个（索引不匹配）\n" +
                $"None标签格子: {noneLabelCells} 个\n" +
                $"区域格子: {regionCells} 个\n" +
                $"缺失区域格子: {missingRegionCells} 个", "确定");

            RefreshMap();
            Repaint();
            SceneView.RepaintAll();

            if (debugCulling)
            {
                Debug.Log($"[GridTileImport] 导入统计 - 应用: {applied}, 跳过: {skipped}, None标签格子: {noneLabelCells}, 区域格子: {regionCells}, 缺失区域: {missingRegionCells}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("导入失败: " + e);
            EditorUtility.DisplayDialog("导入失败", e.Message, "确定");
        }
    }
    private string GetVectorIndexText(GridCell cell)
    {
        if (cell == null) return "-";
        if (currentMap.layout == GridTile.CellLayout.Hexagon) return $"({cell.axial.x},{cell.axial.y})";
        else return $"({cell.rc.x},{cell.rc.y})";
    }

    private void DrawBottomPanels()
    {
        if (targetGrid == null)
        {
            EditorGUILayout.HelpBox("请选择含有 GridTile 组件的 GameObject。", MessageType.Info);
            return;
        }

        if (currentMap == null)
        {
            if (GUILayout.Button("构建并刷新 GridMap")) RefreshMap();
            EditorGUILayout.HelpBox("当前 GridMap 尚未构建或构建失败。点击刷新尝试构建。", MessageType.Warning);
            return;
        }

        // 模式设置
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("编辑模式:", GUILayout.Width(70));
        editorMode = (EditorMode)GUILayout.Toolbar((int)editorMode, new string[] { "选择", "画笔" });

        EditorGUILayout.LabelField("活动标签:", GUILayout.Width(60));
        if (labelTypeManager.LabelTypes.Count > 0)
        {
            string[] ltChoices = new string[labelTypeManager.LabelTypes.Count + 1];
            ltChoices[0] = "无";
            for (int i = 0; i < labelTypeManager.LabelTypes.Count; i++)
                ltChoices[i + 1] = $"{i}:{labelTypeManager.LabelTypes[i].name}";
            int selectedIndex = EditorGUILayout.Popup(activeLabelTypeId + 1, ltChoices);
            activeLabelTypeId = selectedIndex - 1; // 修复：使用-1表示无标签
        }
        else
        {
            activeLabelTypeId = -1;
            EditorGUILayout.LabelField("（无）");
        }
        EditorGUILayout.EndHorizontal();

        // 画笔设置
        if (editorMode == EditorMode.Paint)
        {
            DrawBrushSettings();
        }

        // 格子列表和属性面板
        EditorGUILayout.BeginHorizontal();
        DrawCellList();
        DrawPropertyPanel();
        EditorGUILayout.EndHorizontal();

        // 设置选项
        DrawSettingsPanel();
    }

    private void DrawRegionPanel()
    {
        EditorGUILayout.LabelField("区域管理", EditorStyles.boldLabel);

        // 区域类型管理 - 现在有完整的管理功能
        regionManager.DrawRegionTypeManagement(position.width * 0.4f - 20);

        EditorGUILayout.Space();

        // 当前活动区域显示
        string activeRegionName = regionManager.activeRegionId == -1 ? "默认区域" :
            regionManager.GetRegion(regionManager.activeRegionId)?.name ?? "未知区域";
        EditorGUILayout.LabelField($"当前活动区域: {activeRegionName}", EditorStyles.helpBox);

        regionScroll = EditorGUILayout.BeginScrollView(regionScroll, GUILayout.Height(150));

        // 默认区域
        EditorGUILayout.BeginHorizontal();
        bool isDefaultActive = regionManager.activeRegionId == -1;
        if (GUILayout.Toggle(isDefaultActive, "默认区域", GUILayout.Width(80)))
        {
            regionManager.activeRegionId = -1;
        }
        EditorGUILayout.LabelField("(无区域标签的操作)", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();

        // 区域列表
        for (int i = 0; i < regionManager.regions.Count; i++)
        {
            var region = regionManager.regions[i];
            EditorGUILayout.BeginHorizontal();

            bool isActive = regionManager.activeRegionId == region.id;
            if (GUILayout.Toggle(isActive, "", GUILayout.Width(20)))
            {
                regionManager.activeRegionId = region.id;
            }

            region.name = EditorGUILayout.TextField(region.name, GUILayout.Width(80));

            // 区域类型选择 - 显示类型名称和颜色
            string[] regionTypeChoices = new string[regionManager.regionTypes.Count + 1];
            regionTypeChoices[0] = "无";
            for (int j = 0; j < regionManager.regionTypes.Count; j++)
                regionTypeChoices[j + 1] = $"[{j}]{regionManager.regionTypes[j].name}";

            int currentTypeIndex = region.regionTypeId + 1;
            int newTypeIndex = EditorGUILayout.Popup(currentTypeIndex, regionTypeChoices, GUILayout.Width(100));
            if (newTypeIndex != currentTypeIndex)
            {
                region.regionTypeId = newTypeIndex - 1;
                if (region.regionTypeId >= 0)
                    region.color = regionManager.regionTypes[region.regionTypeId].color;
            }

            // 显示区域类型颜色
            if (region.regionTypeId >= 0 && region.regionTypeId < regionManager.regionTypes.Count)
            {
                var rect = GUILayoutUtility.GetRect(16, 16, GUILayout.Width(16));
                EditorGUI.DrawRect(rect, regionManager.regionTypes[region.regionTypeId].color);
            }

            int cellCount = GetRegionCellCount(region.id);
            EditorGUILayout.LabelField($"{cellCount}格", GUILayout.Width(40));

            if (GUILayout.Button("删", GUILayout.Width(30)))
            {
                if (EditorUtility.DisplayDialog("确认删除", $"确定要删除区域 '{region.name}' 吗？", "删除", "取消"))
                {
                    regionManager.DeleteRegion(region.id);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        // 添加区域按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("添加区域"))
        {
            var newRegion = regionManager.CreateRegion($"区域{regionManager.regions.Count + 1}");
            // 默认使用第一个区域类型（如果有）
            if (regionManager.regionTypes.Count > 0)
            {
                newRegion.regionTypeId = 0;
                newRegion.color = regionManager.regionTypes[0].color;
            }
        }

        // 从类型添加区域
        if (regionManager.regionTypes.Count > 0)
        {
            if (GUILayout.Button("从类型添加"))
            {
                ShowRegionTypeSelectionPopup();
            }
        }
        EditorGUILayout.EndHorizontal();
    }
    private void ShowRegionTypeSelectionPopup()
    {
        GenericMenu menu = new GenericMenu();

        for (int i = 0; i < regionManager.regionTypes.Count; i++)
        {
            int index = i;
            menu.AddItem(new GUIContent($"{i}:{regionManager.regionTypes[i].name}"), false, () =>
            {
                regionManager.CreateRegion(regionManager.regionTypes[index].name);
            });
        }

        menu.ShowAsContext();
    }

    private void DrawSingleCell(GridCell cell, GridCellMeta meta, bool isSelected, Camera cam, float handleSizeBase, float gridDim, Vector3 normal, GUIStyle indexStyle, GUIStyle indexShadow, GUIStyle labelStyle, GUIStyle labelShadow)
    {
        Vector3 cellWorldPos = cell.worldCenter;

        // 获取标签和区域信息
        List<int> labelIds = meta != null && meta.labelTypeIds != null ? meta.labelTypeIds : new List<int>();
        string labelName = "";
        Color primaryColor = Color.white;
        string regionName = "";

        // 优先显示区域颜色和名称
        if (meta != null && meta.regionId != -1)
        {
            var region = regionManager.GetRegion(meta.regionId);
            if (region != null)
            {
                primaryColor = region.color;
                regionName = region.name;
            }
        }

        // 如果有标签，覆盖区域颜色并显示标签名称（带下标）
        if (labelIds.Count > 0)
        {
            var validLabels = new List<string>();
            foreach (var labelId in labelIds)
            {
                if (labelId == -1) // None标签
                {
                    validLabels.Add("[-1]None");
                    primaryColor = Color.gray; // None标签用灰色显示
                }
                else if (labelId >= 0 && labelId < labelTypeManager.LabelTypes.Count)
                {
                    // 显示标签下标和名称
                    validLabels.Add($"[{labelId}]{labelTypeManager.LabelTypes[labelId].name}");
                    // 标签颜色优先于区域颜色
                    primaryColor = labelTypeManager.LabelTypes[labelId].color;
                }
            }
            labelName = string.Join("+", validLabels);
        }

        // 绘制填充圆盘
        Color fill = primaryColor; fill.a = 0.18f;
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        Handles.color = fill;
        float fillRadius = handleSizeBase * 0.9f;
        Handles.DrawSolidDisc(cellWorldPos, normal, fillRadius);
        Handles.color = primaryColor;
        Handles.DrawWireDisc(cellWorldPos, normal, fillRadius);

        // 绘制中心点
        Quaternion swizzleRot = Quaternion.identity;
        try { swizzleRot = Quaternion.LookRotation(GetSwizzleForward(), GetSwizzleUp()); } catch { swizzleRot = Quaternion.identity; }

        float pickSize = handleSizeBase * 0.9f;
        Handles.color = Color.white;
        Handles.SphereHandleCap(0, cellWorldPos, swizzleRot, pickSize * 0.6f, EventType.Repaint);

        // 绘制文本
        Vector3 camUp = cam != null ? cam.transform.up.normalized : Vector3.up;
        Vector3 camRight = cam != null ? cam.transform.right.normalized : Vector3.right;
        Vector3 indexOffset = camUp * (handleSizeBase * 1.0f + gridDim * 0.25f) + camRight * (-(handleSizeBase * 1.0f + gridDim * 0.25f));
        Vector3 labelOffset = camUp * -(handleSizeBase * 0.9f + gridDim * 0.2f);
        Vector3 regionOffset = camUp * (handleSizeBase * 1.5f + gridDim * 0.3f);

        string vectorIndexText = GetVectorIndexText(cell);
        Handles.Label(cellWorldPos + indexOffset + camRight * (0.03f * handleSizeBase), vectorIndexText, indexShadow);
        Handles.Label(cellWorldPos + indexOffset, vectorIndexText, indexStyle);

        // 显示区域名称（如果有区域且没有标签，或者既有区域又有标签）
        if (!string.IsNullOrEmpty(regionName))
        {
            var regionStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = string.IsNullOrEmpty(labelName) ? primaryColor : Color.white }
            };
            Handles.Label(cellWorldPos + regionOffset, regionName, regionStyle);
        }

        // 显示标签名称（带下标）
        if (!string.IsNullOrEmpty(labelName))
        {
            labelStyle.normal.textColor = primaryColor;
            Handles.Label(cellWorldPos + labelOffset + camRight * (0.01f * handleSizeBase) + camUp * (-0.01f * handleSizeBase), labelName, labelShadow);
            Handles.Label(cellWorldPos + labelOffset, labelName, labelStyle);
        }

        // 点击事件
        Handles.color = Color.clear;
        if (Handles.Button(cellWorldPos, Quaternion.identity, pickSize, pickSize * 1.2f, Handles.SphereHandleCap))
        {
            ProcessCellClick(cell, Event.current);
        }

        // 选中高亮
        if (isSelected)
        {
            Handles.color = Color.yellow;
            Handles.DrawWireDisc(cellWorldPos, normal, fillRadius * 1.15f);
        }
    }
    private void ProcessCellClick(GridCell cell, Event ev)
    {
        var eLocal = Event.current;
        bool shiftPressed = eLocal != null && ((eLocal.modifiers & EventModifiers.Shift) != 0 || eLocal.shift);
        bool ctrlPressedLocal = eLocal != null && ((eLocal.modifiers & EventModifiers.Control) != 0 || eLocal.control);
        double now = EditorApplication.timeSinceStartup;

        if (shiftPressed)
        {
            if (metaByIndex.ContainsKey(cell.index))
            {
                metaByIndex.Remove(cell.index);
                lastClearedIndex = cell.index;
                lastClearedTime = now;
            }
            eLocal?.Use();
            Repaint();
            SceneView.RepaintAll();
            return;
        }

        if (lastClearedIndex == cell.index && (now - lastClearedTime) < clearSuppressSeconds)
        {
            eLocal?.Use();
            Repaint();
            SceneView.RepaintAll();
            return;
        }

        // 多标签画笔逻辑 - 修复：只有真正选择标签时才添加 labelTypeIds
        if (editorMode == EditorMode.Paint && activeLabelTypeId >= 0 && activeLabelTypeId < labelTypeManager.LabelTypes.Count && brushTool == BrushTool.Point)
        {
            bool eraseNow = brushEraseMode || ctrlPressedLocal;
            if (eraseNow)
            {
                if (metaByIndex.ContainsKey(cell.index))
                {
                    var meta = metaByIndex[cell.index];
                    meta.labelTypeIds.Clear();
                    // 如果既没有标签也没有属性，就删除整个元数据
                    if (meta.properties == null || meta.properties.Count == 0)
                    {
                        metaByIndex.Remove(cell.index);
                    }
                }
            }
            else
            {
                if (!metaByIndex.TryGetValue(cell.index, out var meta))
                {
                    // 创建新的元数据，但只设置区域ID，不设置标签（除非用户选择了具体标签）
                    meta = GridCellMeta.Create(cell.index, cell.GetIndex(),
                        new List<int>(), // 空的标签列表
                        new List<PropertyKV>(),
                        regionManager.activeRegionId);
                    metaByIndex[cell.index] = meta;
                }

                // 只有在选择了具体标签（不是默认的-1）时才添加标签
                if (activeLabelTypeId >= 0)
                {
                    if (!meta.labelTypeIds.Contains(activeLabelTypeId))
                        meta.labelTypeIds.Add(activeLabelTypeId);
                }

                // 更新区域ID（如果当前不是默认区域）
                if (regionManager.activeRegionId != -1)
                    meta.regionId = regionManager.activeRegionId;

                metaByIndex[cell.index] = meta;
            }

            if (lastClearedIndex == cell.index) lastClearedIndex = -1;

            eLocal?.Use();
            Repaint();
            SceneView.RepaintAll();
            return;
        }

        selectedIndex = cell.index;
        if (openPopupOnClick) ShowQuickLabelPopup(cell.index);
        eLocal?.Use();
        Repaint();
        SceneView.RepaintAll();
    }

    // 简化的画笔应用逻辑
    private void ApplyBrushToCell(GridCell cell, bool erase)
    {
        if (erase)
        {
            if (metaByIndex.ContainsKey(cell.index))
            {
                var meta = metaByIndex[cell.index];
                meta.labelTypeIds.Clear();
                // 如果既没有标签也没有属性，就删除整个元数据
                if (meta.properties == null || meta.properties.Count == 0)
                {
                    metaByIndex.Remove(cell.index);
                }
            }
        }
        else
        {
            if (activeLabelTypeId >= 0 && activeLabelTypeId < labelTypeManager.LabelTypes.Count)
            {
                if (!metaByIndex.TryGetValue(cell.index, out var meta))
                {
                    meta = GridCellMeta.Create(cell.index, cell.GetIndex(),
                        new List<int>(), // 空的标签列表
                        new List<PropertyKV>(),
                        regionManager.activeRegionId);
                    metaByIndex[cell.index] = meta;
                }

                // 只有在选择了具体标签时才添加标签
                if (activeLabelTypeId >= 0)
                {
                    if (!meta.labelTypeIds.Contains(activeLabelTypeId))
                        meta.labelTypeIds.Add(activeLabelTypeId);
                }

                // 更新区域ID（如果当前不是默认区域）
                if (regionManager.activeRegionId != -1)
                    meta.regionId = regionManager.activeRegionId;

                metaByIndex[cell.index] = meta;
            }
        }
    }
    private void DrawRectPreview(Vector3 worldA, Vector3 worldB, float lineWidth, Color col)
    {
        if (targetGrid == null) return;
        Vector3 uA = WorldToUnswizzledLocal(worldA);
        Vector3 uB = WorldToUnswizzledLocal(worldB);
        float minX = Mathf.Min(uA.x, uB.x), maxX = Mathf.Max(uA.x, uB.x);
        float minY = Mathf.Min(uA.y, uB.y), maxY = Mathf.Max(uA.y, uB.y);

        Vector3[] cornersUn = new Vector3[4]
        {
                new Vector3(minX, minY, 0f),
                new Vector3(maxX, minY, 0f),
                new Vector3(maxX, maxY, 0f),
                new Vector3(minX, maxY, 0f)
        };

        Vector3[] cornersWorld = new Vector3[4];
        for (int i = 0; i < 4; i++) cornersWorld[i] = UnswizzledLocalToWorld(cornersUn[i]);

        Handles.color = col;
        Handles.DrawAAPolyLine(lineWidth, new Vector3[] { cornersWorld[0], cornersWorld[1], cornersWorld[2], cornersWorld[3], cornersWorld[0] });
    }

    private void DrawCirclePreview(Vector3 worldCenter, float radius, int segments, Color col)
    {
        if (targetGrid == null) return;
        if (circlePreviewPoints == null || circlePreviewSegments != segments)
        {
            circlePreviewPoints = new Vector3[segments + 1];
            circlePreviewSegments = segments;
        }

        Vector3 centerUn = WorldToUnswizzledLocal(worldCenter);
        for (int i = 0; i <= segments; i++)
        {
            float ang = (float)i / segments * Mathf.PI * 2f;
            Vector3 pUn = centerUn + new Vector3(Mathf.Cos(ang) * radius, Mathf.Sin(ang) * radius, 0f);
            circlePreviewPoints[i] = UnswizzledLocalToWorld(pUn);
        }

        Handles.color = col;
        Handles.DrawAAPolyLine(2f, circlePreviewPoints);
    }

    private GridCell GetCellByIndex(int idx)
    {
        if (indexToCell.TryGetValue(idx, out var cell)) return cell;
        return null;
    }

    private void ApplyRectBrush(Vector3 worldA, Vector3 worldB, bool erase)
    {
        if (targetGrid == null || currentMap == null) return;
        Vector3 uA = WorldToUnswizzledLocal(worldA);
        Vector3 uB = WorldToUnswizzledLocal(worldB);
        float minX = Mathf.Min(uA.x, uB.x), maxX = Mathf.Max(uA.x, uB.x);
        float minY = Mathf.Min(uA.y, uB.y), maxY = Mathf.Max(uA.y, uB.y);

        foreach (var idx in QueryBucketsRange(minX, maxX, minY, maxY))
        {
            if (!indexToCell.TryGetValue(idx, out var cell)) continue;

            if (!cachedUnswizzledLocalCenter.TryGetValue(cell.index, out var uc)) uc = cell.localCenter;
            if (uc.x >= minX && uc.x <= maxX && uc.y >= minY && uc.y <= maxY)
            {
                ApplyBrushToCell(cell, erase);
            }
        }
        EditorUtility.SetDirty(this);
        SceneView.RepaintAll();
    }

    private void ApplyCircleBrush(Vector3 worldCenter, float radius, bool erase)
    {
        if (targetGrid == null || currentMap == null) return;
        Vector3 centerUn = WorldToUnswizzledLocal(worldCenter);
        float r2 = radius * radius;

        float minX = centerUn.x - radius;
        float maxX = centerUn.x + radius;
        float minY = centerUn.y - radius;
        float maxY = centerUn.y + radius;

        foreach (var idx in QueryBucketsRange(minX, maxX, minY, maxY))
        {
            if (!indexToCell.TryGetValue(idx, out var cell)) continue;

            if (!cachedUnswizzledLocalCenter.TryGetValue(cell.index, out var uc)) uc = cell.localCenter;
            float dx = uc.x - centerUn.x;
            float dy = uc.y - centerUn.y;
            if (dx * dx + dy * dy <= r2)
            {
                ApplyBrushToCell(cell, erase);
            }
        }
        EditorUtility.SetDirty(this);
        SceneView.RepaintAll();
    }
    private IEnumerable<int> QueryBucketsRange(float minX, float maxX, float minY, float maxY)
    {
        int bx0 = Mathf.FloorToInt(minX / bucketSize);
        int bx1 = Mathf.FloorToInt(maxX / bucketSize);
        int by0 = Mathf.FloorToInt(minY / bucketSize);
        int by1 = Mathf.FloorToInt(maxY / bucketSize);
        for (int bx = bx0; bx <= bx1; bx++)
            for (int by = by0; by <= by1; by++)
            {
                long k = BucketKey(bx, by);
                if (spatialBuckets.TryGetValue(k, out var list))
                {
                    foreach (var idx in list) yield return idx;
                }
            }
    }



    private static long BucketKey(int bx, int by) => ((long)bx << 32) | (uint)by;
    private void ExportJson()
    {
        if (currentMap == null)
        {
            EditorUtility.DisplayDialog("错误", "当前未构建 GridMap，无法导出。请先刷新。", "确定");
            return;
        }

        var gm = new GridMeta
        {
            gridName = targetGrid != null ? targetGrid.name : "Grid",
            layout = (int)currentMap.layout,
            orientation = (int)currentMap.orientation,
            swizzle = (int)currentMap.swizzle,
            cellSize = currentMap.cellSize,
            gap = currentMap.gap,
            gridRadius = currentMap.gridRadius,
            cellCount = currentMap.cellCount,
            regions = new List<GridRegion>()
        };

        // 按区域组织数据
        var regionCellsDict = new Dictionary<int, List<GridCellMeta>>();

        // 收集所有格子，包括未标记的
        foreach (var cell in currentMap.cells)
        {
            bool hasMeta = metaByIndex.TryGetValue(cell.index, out var meta);

            if (exportOnlyMarked && !hasMeta)
                continue;

            int regionId = hasMeta ? meta.regionId : -1;

            if (!regionCellsDict.ContainsKey(regionId))
            {
                regionCellsDict[regionId] = new List<GridCellMeta>();
            }

            // 创建格子的元数据，如果没有元数据就创建默认的
            var cellMeta = hasMeta ? meta : GridCellMeta.Create(cell.index, cell.GetIndex(), new List<int>(), new List<PropertyKV>(), -1);

            // 确保所有格子都有标签列表，如果没有标签就设置为[-1]
            if (cellMeta.labelTypeIds == null || cellMeta.labelTypeIds.Count == 0)
            {
                cellMeta.labelTypeIds = new List<int> { -1 }; // 默认None标签
            }

            regionCellsDict[regionId].Add(cellMeta);
        }

        // 创建区域对象 - 包括区域管理器中的所有区域
        foreach (var region in regionManager.regions)
        {
            if (regionCellsDict.ContainsKey(region.id) && regionCellsDict[region.id].Count > 0)
            {
                var exportRegion = new GridRegion(region.id, region.name)
                {
                    color = region.color,
                    regionTypeId = region.regionTypeId,
                    cells = regionCellsDict[region.id]
                };
                gm.regions.Add(exportRegion);
            }
            else if (!exportOnlyMarked)
            {
                // 即使没有格子也导出空区域，保持区域结构完整
                var exportRegion = new GridRegion(region.id, region.name)
                {
                    color = region.color,
                    regionTypeId = region.regionTypeId,
                    cells = new List<GridCellMeta>()
                };
                gm.regions.Add(exportRegion);
            }
        }

        // 处理默认区域的格子（regionId = -1）
        if (regionCellsDict.ContainsKey(-1) && regionCellsDict[-1].Count > 0)
        {
            var defaultRegion = new GridRegion(-1, "默认区域")
            {
                color = Color.gray,
                regionTypeId = -1,
                cells = regionCellsDict[-1]
            };
            gm.regions.Add(defaultRegion);
        }

        // 如果没有区域数据但有格子数据，创建一个默认区域
        if (gm.regions.Count == 0 && (metaByIndex.Count > 0 || !exportOnlyMarked))
        {
            var allCells = new List<GridCellMeta>();
            foreach (var cell in currentMap.cells)
            {
                GridCellMeta meta = null;
                if (metaByIndex.TryGetValue(cell.index, out var existing))
                {
                    meta = existing;
                }
                else
                {
                    meta = GridCellMeta.Create(cell.index, cell.GetIndex(), new List<int>(), new List<PropertyKV>());
                }

                // 确保所有格子都有标签列表，如果没有标签就设置为[-1]
                if (meta.labelTypeIds == null || meta.labelTypeIds.Count == 0)
                {
                    meta.labelTypeIds = new List<int> { -1 }; // 默认None标签
                }

                allCells.Add(meta);
            }

            var defaultRegion = new GridRegion(-1, "默认区域")
            {
                color = Color.gray,
                regionTypeId = -1,
                cells = allCells
            };
            gm.regions.Add(defaultRegion);
        }

        // 同时导出区域管理器中的区域类型数据
        if (regionManager.regionTypes.Count > 0)
        {
            // 在 GridMeta 中添加区域类型信息
            // 注意：这里需要修改 GridMeta 类来包含区域类型数据
            // 或者我们可以创建一个包含所有数据的包装类
        }

        string path = EditorUtility.SaveFilePanel("导出 Grid 元数据为 JSON", Application.dataPath, gm.gridName + "_grid.json", "json");
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            string json = Utility.Json.ToJson(gm);
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();

            // 同时保存区域数据到本地
            SaveRegionData();

            // 统计信息
            int totalCells = 0;
            int noneLabelCells = 0;
            int regionCells = 0;
            foreach (var region in gm.regions)
            {
                totalCells += region.cells.Count;
                if (region.id != -1) regionCells += region.cells.Count;
                foreach (var cell in region.cells)
                {
                    if (cell.labelTypeIds != null && cell.labelTypeIds.Contains(-1))
                        noneLabelCells++;
                }
            }

            EditorUtility.DisplayDialog("导出成功",
                $"已导出 {gm.regions.Count} 个区域到：\n{path}\n\n" +
                $"统计信息：\n" +
                $"总格子数: {totalCells}\n" +
                $"区域格子: {regionCells}\n" +
                $"None标签格子: {noneLabelCells}", "确定");

            if (debugCulling)
            {
                Debug.Log($"[GridTileExport] 导出完成 - 区域数: {gm.regions.Count}, 格子数: {totalCells}, 区域格子: {regionCells}, None标签格子: {noneLabelCells}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("导出失败: " + e);
            EditorUtility.DisplayDialog("导出失败", e.Message, "确定");
        }
    }
    private void DrawBrushSettings()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("画笔设置", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("工具:", GUILayout.Width(40));
        brushTool = (BrushTool)GUILayout.Toolbar((int)brushTool, new string[] { "点", "矩形", "圆形" });
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        brushEraseMode = EditorGUILayout.ToggleLeft("删除模式", brushEraseMode, GUILayout.Width(80));
        EditorGUILayout.LabelField("或按 Ctrl 临时删除", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();

        if (brushTool == BrushTool.Circle)
        {
            brushRadius = EditorGUILayout.Slider("圆形半径", brushRadius, 0.1f,
                Mathf.Max(0.5f, targetGrid != null ? Mathf.Max(targetGrid.cellSize.x, targetGrid.cellSize.y) * 5f : 5f));
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawSettingsPanel()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUILayout.BeginHorizontal();
        openPopupOnClick = EditorGUILayout.ToggleLeft("单击弹出编辑窗口", openPopupOnClick, GUILayout.Width(150));
        EditorGUILayout.LabelField("提示: 关闭后单击只选中，不弹窗编辑。", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        enableDistanceCulling = EditorGUILayout.ToggleLeft("启用距离裁剪", enableDistanceCulling, GUILayout.Width(120));
        debugCulling = EditorGUILayout.ToggleLeft("调试模式", debugCulling, GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("显示距离:", GUILayout.Width(70));
        maxHandlerDistance = EditorGUILayout.Slider(maxHandlerDistance, 1f, 200f);
        if (GUILayout.Button("10米", GUILayout.Width(40))) maxHandlerDistance = 10f;
        if (GUILayout.Button("50米", GUILayout.Width(40))) maxHandlerDistance = 50f;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }


    private void DrawRegionPanelCompact()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        // 当前活动区域
        string activeRegionName = regionManager.activeRegionId == -1 ? "默认区域" :
            regionManager.GetRegion(regionManager.activeRegionId)?.name ?? "未知区域";
        EditorGUILayout.LabelField($"活动区域: {activeRegionName}", EditorStyles.miniBoldLabel);

        regionScroll = EditorGUILayout.BeginScrollView(regionScroll, GUILayout.Height(120));

        // 区域选择
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Toggle(regionManager.activeRegionId == -1, "默认", GUILayout.ExpandWidth(true)))
        {
            regionManager.activeRegionId = -1;
        }

        foreach (var region in regionManager.regions)
        {
            var style = new GUIStyle(GUI.skin.button);
            style.normal.textColor = region.color;
            if (GUILayout.Toggle(regionManager.activeRegionId == region.id, region.name, style, GUILayout.ExpandWidth(true)))
            {
                regionManager.activeRegionId = region.id;
            }
        }
        EditorGUILayout.EndHorizontal();

        // 区域管理
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+添加区域"))
        {
            regionManager.CreateRegion($"区域{regionManager.regions.Count + 1}");
        }

        if (regionManager.activeRegionId != -1 && GUILayout.Button("-删除区域"))
        {
            var region = regionManager.GetRegion(regionManager.activeRegionId);
            if (region != null && EditorUtility.DisplayDialog("确认删除", $"确定要删除区域 '{region.name}' 吗？", "删除", "取消"))
            {
                regionManager.DeleteRegion(region.id);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private int GetRegionCellCount(int regionId)
    {
        if (regionId == -1) return 0;
        int count = 0;
        foreach (var meta in metaByIndex.Values)
        {
            if (meta.regionId == regionId) count++;
        }
        return count;
    }

    private void DrawCellList()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(position.width * leftPanelWidth));
        EditorGUILayout.LabelField("格子列表", EditorStyles.boldLabel);

        // 区域筛选
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("筛选:", GUILayout.Width(40));
        bool showAll = GUILayout.Toggle(regionManager.activeRegionId == -2, "全部", GUILayout.Width(40));
        bool showCurrent = GUILayout.Toggle(regionManager.activeRegionId != -2, "当前区域", GUILayout.Width(70));
        if (showAll) regionManager.activeRegionId = -2;
        else if (showCurrent) regionManager.activeRegionId = regionManager.activeRegionId == -2 ? -1 : regionManager.activeRegionId;
        EditorGUILayout.EndHorizontal();

        leftScroll = EditorGUILayout.BeginScrollView(leftScroll, GUILayout.Height(200));

        foreach (var cell in currentMap.cells)
        {
            // 区域筛选
            if (regionManager.activeRegionId != -2)
            {
                if (metaByIndex.TryGetValue(cell.index, out var meta))
                {
                    if (meta.regionId != regionManager.activeRegionId) continue;
                }
                else
                {
                    continue;
                }
            }

            string displayName = GetLabelNameForCell(cell);
            string indexText = GetVectorIndexText(cell);
            GUIStyle itemStyle = (cell.index == selectedIndex) ? EditorStyles.helpBox : EditorStyles.label;

            EditorGUILayout.BeginHorizontal(itemStyle);
            if (GUILayout.Button($"{displayName} ({indexText})", GUILayout.ExpandWidth(true)))
            {
                selectedIndex = cell.index;
                SceneView.RepaintAll();
            }
            if (GUILayout.Button("定位", GUILayout.Width(40)))
            {
                SceneView.lastActiveSceneView.LookAt(cell.worldCenter);
                selectedIndex = cell.index;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawPropertyPanel()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(position.width * rightPanelWidth));
        EditorGUILayout.LabelField("格子属性编辑", EditorStyles.boldLabel);

        rightScroll = EditorGUILayout.BeginScrollView(rightScroll, GUILayout.Height(200));

        if (selectedIndex < 0)
        {
            EditorGUILayout.HelpBox("请选择一个格子进行编辑", MessageType.Info);
        }
        else
        {
            bool hasMeta = metaByIndex.TryGetValue(selectedIndex, out var meta);
            var selCell = GetCellByIndex(selectedIndex);

            EditorGUILayout.LabelField($"索引: {GetVectorIndexText(selCell)}");
            EditorGUILayout.LabelField($"位置: {selCell.worldCenter:F2}");

            // 显示区域信息
            if (hasMeta && meta.regionId != -1)
            {
                var region = regionManager.GetRegion(meta.regionId);
                if (region != null)
                {
                    EditorGUILayout.LabelField($"区域: {region.name}");
                }
            }

            EditorGUILayout.Space();

            // 标签编辑
            if (labelTypeManager.LabelTypes.Count > 0)
            {
                EditorGUILayout.LabelField("标签类型:", EditorStyles.miniBoldLabel);
                List<int> currentLabelIds = hasMeta && meta.labelTypeIds != null ? new List<int>(meta.labelTypeIds) : new List<int>();

                for (int i = 0; i < labelTypeManager.LabelTypes.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    bool isSelected = currentLabelIds.Contains(i);
                    bool newSelected = EditorGUILayout.ToggleLeft($"{labelTypeManager.LabelTypes[i].name}", isSelected);

                    if (newSelected && !isSelected)
                    {
                        if (!hasMeta)
                        {
                            meta = GridCellMeta.Create(selectedIndex, selCell.GetIndex(), new List<int>(), new List<PropertyKV>());
                            hasMeta = true;
                        }
                        meta.labelTypeIds.Add(i);
                        metaByIndex[selectedIndex] = meta;
                    }
                    else if (!newSelected && isSelected)
                    {
                        if (hasMeta)
                        {
                            meta.labelTypeIds.Remove(i);
                            if (meta.labelTypeIds.Count == 0 && (meta.properties == null || meta.properties.Count == 0))
                                metaByIndex.Remove(selectedIndex);
                            else
                                metaByIndex[selectedIndex] = meta;
                        }
                    }

                    var rect = GUILayoutUtility.GetRect(16, 16, GUILayout.Width(16));
                    EditorGUI.DrawRect(rect, labelTypeManager.LabelTypes[i].color);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("全选"))
                {
                    if (!hasMeta)
                    {
                        meta = GridCellMeta.Create(selectedIndex, selCell.GetIndex(), new List<int>(), new List<PropertyKV>());
                        hasMeta = true;
                    }
                    meta.labelTypeIds.Clear();
                    for (int i = 0; i < labelTypeManager.LabelTypes.Count; i++)
                        meta.labelTypeIds.Add(i);
                    metaByIndex[selectedIndex] = meta;
                }
                if (GUILayout.Button("清空"))
                {
                    if (hasMeta)
                    {
                        meta.labelTypeIds.Clear();
                        if (meta.properties == null || meta.properties.Count == 0)
                            metaByIndex.Remove(selectedIndex);
                        else
                            metaByIndex[selectedIndex] = meta;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            // 属性编辑
            if (hasMeta)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("自定义属性:", EditorStyles.miniBoldLabel);
                if (meta.properties == null) meta.properties = new List<PropertyKV>();

                int removeAt = -1;
                for (int i = 0; i < meta.properties.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    meta.properties[i].key = EditorGUILayout.TextField(meta.properties[i].key, GUILayout.Width(100));
                    meta.properties[i].value = EditorGUILayout.TextField(meta.properties[i].value);
                    if (GUILayout.Button("×", GUILayout.Width(24))) removeAt = i;
                    EditorGUILayout.EndHorizontal();
                }
                if (removeAt >= 0) meta.properties.RemoveAt(removeAt);

                if (GUILayout.Button("添加属性"))
                    meta.properties.Add(new PropertyKV() { key = "key", value = "value" });
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private string GetLabelNameForCell(GridCell cell)
    {
        if (cell == null) return "无";

        if (metaByIndex.TryGetValue(cell.index, out var m))
        {
            List<string> infoParts = new List<string>();

            // 区域信息
            if (m.regionId != -1)
            {
                var region = regionManager.GetRegion(m.regionId);
                if (region != null)
                {
                    infoParts.Add(region.name);
                }
            }

            // 标签信息
            if (m.labelTypeIds != null && m.labelTypeIds.Count > 0)
            {
                var labelNames = new List<string>();
                foreach (var labelId in m.labelTypeIds)
                {
                    if (labelId == -1) // None标签
                    {
                        labelNames.Add("[-1]None");
                    }
                    else if (labelId >= 0 && labelId < labelTypeManager.LabelTypes.Count)
                    {
                        labelNames.Add($"[{labelId}]{labelTypeManager.LabelTypes[labelId].name}");
                    }
                }
                if (labelNames.Count > 0)
                {
                    infoParts.Add($"标签:{string.Join(",", labelNames)}");
                }
            }

            return infoParts.Count > 0 ? string.Join(" ", infoParts) : "已标记";
        }
        return "无";
    }
}
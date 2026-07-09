using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Game.Modules.GModuleScene;

public class SceneViewDrawer
{
    private Vector3[] circlePreviewPoints;
    private int circlePreviewSegments;

    public void DrawSceneView(
        SceneView sceneView,
        GridTile targetGrid,
        GridTileMap currentMap,
        Dictionary<int, GridCellMeta> metaByIndex,
        ref int selectedIndex,
        LabelTypeManager labelTypeManager,
        GridTileEditorWindow.EditorMode editorMode,
        GridTileEditorWindow.BrushTool brushTool,
        int activeLabelTypeId,
        bool brushEraseMode,
        ref bool isDraggingRect,
        ref Vector3 rectStartWorld,
        ref Vector3 rectCurrentWorld,
        ref bool isDraggingCircle,
        ref int lastClearedIndex,
        ref double lastClearedTime,
        bool openPopupOnClick,
        bool enableDistanceCulling,
        float maxHandlerDistance,
        bool debugCulling,
        Dictionary<int, Vector3> cachedUnswizzledLocalCenter,
        Dictionary<long, List<int>> spatialBuckets,
        Dictionary<int, GridCell> indexToCell,
        float brushRadius,
        System.Action<Vector3, Vector3, bool> ApplyRectBrush,
        System.Action<Vector3, float, bool> ApplyCircleBrush,
        System.Action<GridCell, Event> ProcessCellClick,
        System.Func<GridCell, string> GetVectorIndexText,
        System.Action<int> ShowQuickLabelPopup
    )
    {
        if (targetGrid == null || currentMap == null) return;

        // 基础参数计算
        float handleSizeBase = HandleUtility.GetHandleSize(targetGrid.transform.position) * 0.1f;
        Vector3[] swizzleAxes = CalculateSwizzleAxes(targetGrid);
        Vector3 normal = swizzleAxes[2];
        Camera cam = sceneView?.camera ?? Camera.current;
        float gridDim = CalculateGridDim(targetGrid);

        // 鼠标交互基础数据
        Event ev = Event.current;
        (bool hitPlane, Vector3 mouseWorld) = GetMouseWorldPosition(ev, cam, normal, targetGrid.transform.position);
        Vector3 cameraWorld = cam?.transform.position ?? targetGrid.transform.position;
        bool forceShowAll = ev != null && (ev.control || (ev.modifiers & EventModifiers.Alt) != 0);

        // 画笔交互处理
        HandleBrushInteraction(editorMode, brushTool, hitPlane, ev, mouseWorld, brushEraseMode,
            ref isDraggingRect, ref rectStartWorld, ref rectCurrentWorld,
            ref isDraggingCircle, ApplyRectBrush, ApplyCircleBrush, brushRadius, targetGrid);

        // 绘制所有格子
        DrawGridCells(currentMap, metaByIndex, ref selectedIndex, labelTypeManager, cam, cameraWorld,
            forceShowAll, enableDistanceCulling, maxHandlerDistance, debugCulling,
            handleSizeBase, normal, gridDim, GetVectorIndexText, openPopupOnClick,
            ref lastClearedIndex, ref lastClearedTime, ev, ProcessCellClick);

        // F2快速弹窗
        if (ev != null && ev.type == EventType.KeyDown && ev.keyCode == KeyCode.F2 && selectedIndex >= 0)
        {
            ShowQuickLabelPopup?.Invoke(selectedIndex);
            ev.Use();
        }

        if (Event.current.type == EventType.Repaint) SceneView.RepaintAll();
    }

    // 计算swizzle轴向量
    private Vector3[] CalculateSwizzleAxes(GridTile targetGrid)
    {
        Vector3 right = targetGrid.transform.TransformDirection(GridTile.ReverseSwizzle(Vector3.right, targetGrid.cellSwizzle)).normalized;
        Vector3 up = targetGrid.transform.TransformDirection(GridTile.ReverseSwizzle(Vector3.up, targetGrid.cellSwizzle)).normalized;
        Vector3 forward = targetGrid.transform.TransformDirection(GridTile.ReverseSwizzle(Vector3.forward, targetGrid.cellSwizzle)).normalized;

        return new[] {
            right.sqrMagnitude < 1e-6f ? targetGrid.transform.right : right,
            up.sqrMagnitude < 1e-6f ? targetGrid.transform.up : up,
            forward.sqrMagnitude < 1e-6f ? targetGrid.transform.forward : forward
        };
    }

    // 计算网格尺寸
    private float CalculateGridDim(GridTile targetGrid)
    {
        try
        {
            return Mathf.Max(0.01f, Mathf.Max(targetGrid.cellSize.x + targetGrid.gap.x, targetGrid.cellSize.y + targetGrid.gap.y));
        }
        catch
        {
            return 1f;
        }
    }

    // 获取鼠标世界位置
    private (bool hit, Vector3 worldPos) GetMouseWorldPosition(Event ev, Camera cam, Vector3 normal, Vector3 planePos)
    {
        if (ev == null) return (false, Vector3.zero);
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(ev.mousePosition);
        if (new Plane(normal, planePos).Raycast(mouseRay, out float enter))
        {
            return (true, mouseRay.GetPoint(enter));
        }
        return (false, Vector3.zero);
    }

    // 处理画笔交互
    private void HandleBrushInteraction(GridTileEditorWindow.EditorMode editorMode, GridTileEditorWindow.BrushTool brushTool,
        bool hitPlane, Event ev, Vector3 mouseWorld, bool brushEraseMode,
        ref bool isDraggingRect, ref Vector3 rectStartWorld, ref Vector3 rectCurrentWorld,
        ref bool isDraggingCircle, System.Action<Vector3, Vector3, bool> ApplyRectBrush,
        System.Action<Vector3, float, bool> ApplyCircleBrush, float brushRadius, GridTile targetGrid)
    {
        if (editorMode != GridTileEditorWindow.EditorMode.Paint || !hitPlane || ev == null) return;

        bool ctrlPressed = (ev.modifiers & EventModifiers.Control) != 0 || ev.control;
        bool isEraseNow = brushEraseMode || ctrlPressed;

        // 矩形画笔
        if (brushTool == GridTileEditorWindow.BrushTool.Rect)
        {
            if (ev.type == EventType.MouseDown && ev.button == 0)
            {
                isDraggingRect = true;
                rectStartWorld = rectCurrentWorld = mouseWorld;
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
                ApplyRectBrush?.Invoke(rectStartWorld, rectCurrentWorld, isEraseNow);
                isDraggingRect = false;
                ev.Use();
            }

            if (isDraggingRect)
            {
                DrawRectPreview(targetGrid, rectStartWorld, rectCurrentWorld, 2f, Color.yellow);
            }
        }

        // 圆形画笔
        if (brushTool == GridTileEditorWindow.BrushTool.Circle)
        {
            DrawCirclePreview(targetGrid, mouseWorld, brushRadius, 64, Color.green);

            if (ev.type == EventType.MouseDown && ev.button == 0)
            {
                isDraggingCircle = true;
                ApplyCircleBrush?.Invoke(mouseWorld, brushRadius, isEraseNow);
                ev.Use();
            }
            else if (ev.type == EventType.MouseDrag && isDraggingCircle)
            {
                ApplyCircleBrush?.Invoke(mouseWorld, brushRadius, isEraseNow);
                ev.Use();
            }
            else if (ev.type == EventType.MouseUp && isDraggingCircle)
            {
                isDraggingCircle = false;
                ev.Use();
            }
        }
    }

    private void DrawRectPreview(GridTile targetGrid, Vector3 worldA, Vector3 worldB, float lineWidth, Color col)
    {
        if (targetGrid == null) return;
        Vector3 uA = WorldToUnswizzledLocal(targetGrid, worldA);
        Vector3 uB = WorldToUnswizzledLocal(targetGrid, worldB);

        Vector3[] cornersWorld = new Vector3[4] {
            UnswizzledLocalToWorld(targetGrid, new Vector3(Mathf.Min(uA.x, uB.x), Mathf.Min(uA.y, uB.y))),
            UnswizzledLocalToWorld(targetGrid, new Vector3(Mathf.Max(uA.x, uB.x), Mathf.Min(uA.y, uB.y))),
            UnswizzledLocalToWorld(targetGrid, new Vector3(Mathf.Max(uA.x, uB.x), Mathf.Max(uA.y, uB.y))),
            UnswizzledLocalToWorld(targetGrid, new Vector3(Mathf.Min(uA.x, uB.x), Mathf.Max(uA.y, uB.y)))
        };

        Handles.color = col;
        Handles.DrawAAPolyLine(lineWidth, cornersWorld[0], cornersWorld[1], cornersWorld[2], cornersWorld[3], cornersWorld[0]);
    }

    private void DrawCirclePreview(GridTile targetGrid, Vector3 worldCenter, float radius, int segments, Color col)
    {
        if (targetGrid == null) return;
        if (circlePreviewPoints == null || circlePreviewSegments != segments)
        {
            circlePreviewPoints = new Vector3[segments + 1];
            circlePreviewSegments = segments;
        }

        Vector3 centerUn = WorldToUnswizzledLocal(targetGrid, worldCenter);
        for (int i = 0; i <= segments; i++)
        {
            float ang = (float)i / segments * Mathf.PI * 2f;
            circlePreviewPoints[i] = UnswizzledLocalToWorld(targetGrid, centerUn + new Vector3(Mathf.Cos(ang) * radius, Mathf.Sin(ang) * radius));
        }

        Handles.color = col;
        Handles.DrawAAPolyLine(2f, circlePreviewPoints);
    }

    private Vector3 WorldToUnswizzledLocal(GridTile targetGrid, Vector3 world)
    {
        return GridTile.ReverseSwizzle(targetGrid.transform.InverseTransformPoint(world), targetGrid.cellSwizzle);
    }

    private Vector3 UnswizzledLocalToWorld(GridTile targetGrid, Vector3 unswizzledLocal)
    {
        return targetGrid.transform.TransformPoint(GridTile.ReverseSwizzle(unswizzledLocal, targetGrid.cellSwizzle));
    }

    private void DrawGridCells(GridTileMap currentMap, Dictionary<int, GridCellMeta> metaByIndex,
        ref int selectedIndex, LabelTypeManager labelTypeManager, Camera cam, Vector3 cameraWorld,
        bool forceShowAll, bool enableDistanceCulling, float maxHandlerDistance, bool debugCulling,
        float handleSizeBase, Vector3 normal, float gridDim, System.Func<GridCell, string> GetVectorIndexText,
        bool openPopupOnClick, ref int lastClearedIndex, ref double lastClearedTime, Event ev,
        System.Action<GridCell, Event> ProcessCellClick)
    {
        float maxDistSqr = maxHandlerDistance * maxHandlerDistance;
        int drawnCount = 0, culledByDistance = 0, culledByViewport = 0;
        int cellCount = currentMap.cells.Count;

        for (int i = 0; i < cellCount; i++)
        {
            GridCell cell = currentMap.cells[i];
            Vector3 cellWorldPos = cell.worldCenter;
            metaByIndex.TryGetValue(cell.index, out var meta);
            bool isSelected = cell.index == selectedIndex;
            bool hasMark = meta != null && (meta.labelTypeIds?.Count > 0 || meta.properties?.Count > 0);
            bool forceDraw = forceShowAll || isSelected || hasMark;

            // 距离裁剪
            if (EnableDistanceCulling(enableDistanceCulling, !forceDraw, cam, currentMap, cellWorldPos, cameraWorld, maxHandlerDistance))
            {
                culledByDistance++;
                continue;
            }

            // 视锥裁剪
            if (!forceDraw && cam != null && !IsWorldPointInViewport(cam, cellWorldPos))
            {
                culledByViewport++;
                continue;
            }

            // 绘制格子
            drawnCount++;
            DrawGridCell(cell, meta, isSelected, labelTypeManager, handleSizeBase, normal, gridDim, cam, GetVectorIndexText, ProcessCellClick, ev);
        }

        // 调试日志
        if (debugCulling && ev != null && ev.type == EventType.Repaint)
        {
            Debug.Log($"[GridTileCulling] 绘制统计 - 总格子数: {cellCount}, 绘制数: {drawnCount}, 距离裁剪数: {culledByDistance}, 视锥裁剪数: {culledByViewport}");
        }
    }

    // 距离裁剪判断
    private bool EnableDistanceCulling(bool enableDistanceCulling, bool notForceDraw, Camera cam, GridTileMap currentMap,
        Vector3 cellWorldPos, Vector3 cameraWorld, float maxHandlerDistance)
    {
        if (!enableDistanceCulling || !notForceDraw || cam == null) return false;

        Vector3 scaledWorldPos = currentMap.gridTile.transform.localScale != Vector3.one
            ? ScaleWorldPosition(cellWorldPos, currentMap.gridTile)
            : cellWorldPos;

        return Vector3.Distance(cameraWorld, scaledWorldPos) > maxHandlerDistance;
    }

    // 缩放世界位置（处理非等比缩放）
    private Vector3 ScaleWorldPosition(Vector3 worldPos, GridTile gridTile)
    {
        Vector3 scale = gridTile.transform.localScale;
        return Vector3.Scale(worldPos - gridTile.transform.position, new Vector3(1 / scale.x, 1 / scale.y, 1 / scale.z)) + gridTile.transform.position;
    }

    private bool IsWorldPointInViewport(Camera cam, Vector3 worldPos)
    {
        Vector3 vp = cam.WorldToViewportPoint(worldPos);
        return vp.z > -0.1f && vp.x >= -0.2f && vp.x <= 1.2f && vp.y >= -0.2f && vp.y <= 1.2f;
    }

    private void DrawGridCell(GridCell cell, GridCellMeta meta, bool isSelected,
        LabelTypeManager labelTypeManager, float handleSizeBase, Vector3 normal, float gridDim,
        Camera cam, System.Func<GridCell, string> GetVectorIndexText,
        System.Action<GridCell, Event> ProcessCellClick, Event ev)
    {
        Vector3 cellWorldPos = cell.worldCenter;
        List<int> labelIds = meta?.labelTypeIds ?? new List<int>();
        (string labelName, Color primaryColor) = GetLabelInfo(labelIds, labelTypeManager);

        // 绘制填充圆盘
        float fillRadius = handleSizeBase * 0.9f;
        DrawDisc(cellWorldPos, normal, fillRadius, primaryColor);

        // 绘制中心点
        Quaternion swizzleRot = Quaternion.LookRotation(normal, Vector3.up);
        float pickSize = handleSizeBase * 0.9f;
        Handles.color = Color.white;
        Handles.SphereHandleCap(0, cellWorldPos, swizzleRot, pickSize * 0.6f, EventType.Repaint);

        // 绘制文本
        DrawCellText(cellWorldPos, cell, GetVectorIndexText, labelName, handleSizeBase, gridDim, cam, primaryColor);

        // 绘制点击区域
        Handles.color = Color.clear;
        if (Handles.Button(cellWorldPos, Quaternion.identity, pickSize, pickSize * 1.2f, Handles.SphereHandleCap))
        {
            ProcessCellClick?.Invoke(cell, ev);
        }

        // 选中高亮
        if (isSelected)
        {
            Handles.color = Color.yellow;
            Handles.DrawWireDisc(cellWorldPos, normal, fillRadius * 1.15f);
        }
    }

    // 获取标签信息（名称和主颜色）
    private (string name, Color color) GetLabelInfo(List<int> labelIds, LabelTypeManager labelTypeManager)
    {
        if (labelIds.Count == 0) return ("", Color.white);

        List<string> validLabels = new List<string>();
        Color primaryColor = Color.white;

        foreach (int labelId in labelIds)
        {
            if (labelId >= 0 && labelId < labelTypeManager.LabelTypes.Count)
            {
                validLabels.Add(labelTypeManager.LabelTypes[labelId].name);
                if (primaryColor == Color.white)
                    primaryColor = labelTypeManager.LabelTypes[labelId].color;
            }
        }

        return (string.Join("+", validLabels), primaryColor);
    }

    // 绘制圆盘（填充+边框）
    private void DrawDisc(Vector3 pos, Vector3 normal, float radius, Color color)
    {
        Color fill = color;
        fill.a = 0.18f;
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        Handles.color = fill;
        Handles.DrawSolidDisc(pos, normal, radius);
        Handles.color = color;
        Handles.DrawWireDisc(pos, normal, radius);
    }

    // 绘制单元格文本（索引和标签）
    private void DrawCellText(Vector3 cellWorldPos, GridCell cell, System.Func<GridCell, string> GetVectorIndexText,
        string labelName, float handleSizeBase, float gridDim, Camera cam, Color primaryColor)
    {
        Vector3 camUp = cam?.transform.up ?? Vector3.up;
        Vector3 camRight = cam?.transform.right ?? Vector3.right;
        Vector3 indexOffset = camUp * (handleSizeBase * 1.0f + gridDim * 0.25f) - camRight * (handleSizeBase * 1.0f + gridDim * 0.25f);
        Vector3 labelOffset = -camUp * (handleSizeBase * 0.9f + gridDim * 0.2f);

        string vectorIndexText = GetVectorIndexText(cell);

        // 索引文本样式
        var indexStyle = CreateTextStyle(10 + handleSizeBase * 18f, 12, 30, TextAnchor.MiddleLeft, Color.black);
        var indexShadow = new GUIStyle(indexStyle) { normal = { textColor = new Color(0, 0, 0, 0.85f) } };

        Handles.Label(cellWorldPos + indexOffset + camRight * (0.03f * handleSizeBase), vectorIndexText, indexShadow);
        Handles.Label(cellWorldPos + indexOffset, vectorIndexText, indexStyle);

        // 标签文本样式
        if (!string.IsNullOrEmpty(labelName))
        {
            var labelStyle = CreateTextStyle(10 + handleSizeBase * 28f, 12, 44, TextAnchor.MiddleCenter, primaryColor);
            var labelShadow = new GUIStyle(labelStyle) { normal = { textColor = new Color(0, 0, 0, 0.85f) } };

            Handles.Label(cellWorldPos + labelOffset + camRight * (0.01f * handleSizeBase) - camUp * (0.01f * handleSizeBase), labelName, labelShadow);
            Handles.Label(cellWorldPos + labelOffset, labelName, labelStyle);
        }
    }

    // 创建文本样式（复用逻辑）
    private GUIStyle CreateTextStyle(float baseSize, int min, int max, TextAnchor alignment, Color color)
    {
        return new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = alignment,
            fontSize = Mathf.Clamp(Mathf.RoundToInt(baseSize), min, max),
            normal = { textColor = color }
        };
    }
}
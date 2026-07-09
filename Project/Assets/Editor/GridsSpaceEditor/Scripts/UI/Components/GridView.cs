using System.Collections.Generic;
using System.Linq;
using GridsSpaceEditor.Core;
using GridsSpaceEditor.Data.Enums;
using GridsSpaceEditor.Data.Models;
using GridsSpaceEditor.UI.Shared;
using UnityEngine;
using UnityEditor;
using Mathf = UnityEngine.Mathf;
using System;

namespace GridsSpaceEditor.UI.Components
{
    public class GridView
    {
        private GridManager m_GridManager;
        private PortManager m_PortManager;
        private SystemData m_SystemData;
        private float m_Zoom = 1.0f;
        private Vector2 m_PanOffset = Vector2.zero;

        private bool m_IsLeftMouseDown = false;
        private bool m_IsRightMouseDown = false;
        private bool m_IsDragging = false;
        private Vector2 m_MouseDownPos;
        private Rect m_SelectionRect;
        private const float k_DragThreshold = 5f;

        private float m_BrushRadius = 1.5f;
        private bool m_UseBrush = false;

        private bool m_IsResizing = false;
        private float m_RightPanelWidth = 350f;

        public float Zoom => m_Zoom;
        public Vector2 PanOffset => m_PanOffset;
        public float RightPanelWidth
        {
            get => m_RightPanelWidth;
            set => m_RightPanelWidth = Mathf.Clamp(value, 250f, 1200f);
        }
        public bool IsResizing => m_IsResizing;

        public event System.Action OnDataChanged;
        public event System.Action OnViewportChanged;

        public GridView(GridManager gridManager, SystemData systemData)
        {
            m_GridManager = gridManager;
            m_PortManager = null;
            m_SystemData = systemData;
        }

        public void SetPortManager(PortManager portManager)
        {
            m_PortManager = portManager;
        }

        public void ResetView()
        {
            m_PanOffset = Vector2.zero;
            m_Zoom = 1.0f;
            OnViewportChanged?.Invoke();
        }

        /// <param name="isPortEditTab">端口编辑主标签：显示全部格子的端口 + 边缘热点。</param>
        /// <param name="showAllPortsInGridEdit">网格编辑标签下：为 true 时显示所有已开启端口的格子；为 false 时仅显示选中格子的端口。</param>
        public void DrawView(Rect rect, bool isPortEditTab, bool showAllPortsInGridEdit)
        {
            if (Event.current.type == EventType.Layout) return;

            GUI.BeginGroup(rect);
            EditorGUI.DrawRect(new Rect(0, 0, rect.width, rect.height), ColorPalette.Background);

            Vector2 center = rect.size / 2f + m_PanOffset;
            float cellSize = 50f * m_Zoom;

            DrawGridLines(rect, center, cellSize);
            DrawCells(rect, center, cellSize);

            // 只在端口编辑模式下显示端口
            if (isPortEditTab)
            {
                DrawPorts(center, cellSize, true);
                DrawEdgeHotspots(rect, center, cellSize);
            }

            DrawSelectionOverlay(center, cellSize);

            GUI.EndGroup();
        }

        private void DrawGridLines(Rect rect, Vector2 center, float cellSize)
        {
            Handles.BeginGUI();
            Handles.color = ColorPalette.GridLine;

            float lineOffset = m_SystemData.CenterAlignment ? cellSize * 0.5f : 0f;
            float startX = (center.x - lineOffset) % cellSize;
            float startY = (center.y + lineOffset) % cellSize;

            for (float x = startX; x < rect.width; x += cellSize)
                Handles.DrawLine(new Vector2(x, 0), new Vector2(x, rect.height));
            for (float x = startX - cellSize; x > 0; x -= cellSize)
                Handles.DrawLine(new Vector2(x, 0), new Vector2(x, rect.height));
            for (float y = startY; y < rect.height; y += cellSize)
                Handles.DrawLine(new Vector2(0, y), new Vector2(rect.width, y));
            for (float y = startY - cellSize; y > 0; y -= cellSize)
                Handles.DrawLine(new Vector2(0, y), new Vector2(rect.width, y));

            Handles.color = ColorPalette.AxisLine;
            Handles.DrawLine(new Vector2(0, center.y), new Vector2(rect.width, center.y));
            Handles.DrawLine(new Vector2(center.x, 0), new Vector2(center.x, rect.height));

            Handles.color = ColorPalette.OriginMarker;
            Handles.DrawWireDisc(center, Vector3.forward, 3f);
            Handles.EndGUI();
        }

        private void DrawCells(Rect rect, Vector2 center, float cellSize)
        {
            GUIStyle coordStyle = new GUIStyle { alignment = TextAnchor.MiddleCenter };
            coordStyle.normal.textColor = new Color(1, 1, 1, 0.4f);
            coordStyle.fontSize = Mathf.RoundToInt(10 * m_Zoom);

            foreach (var cell in m_GridManager.Cells)
            {
                float drawX = center.x + cell.Coordinates.x * cellSize;
                float drawY = center.y - cell.Coordinates.y * cellSize;

                if (m_SystemData.CenterAlignment)
                {
                    drawX -= cellSize * 0.5f;
                    drawY -= cellSize * 0.5f;
                }
                else
                {
                    drawY -= cellSize;
                }

                Rect cellRect = new Rect(drawX, drawY, cellSize, cellSize);
                if (!rect.Overlaps(new Rect(cellRect.position + rect.position, cellRect.size))) continue;

                bool isSelected = m_GridManager.SelectedCoords.Contains(cell.Coordinates);
                EditorGUI.DrawRect(cellRect, isSelected ? ColorPalette.CellSelected : ColorPalette.CellFill);

                Handles.color = isSelected ? Color.yellow : ColorPalette.CellBorder;
                Handles.DrawSolidRectangleWithOutline(cellRect, Color.clear, Handles.color);

                if (m_Zoom > 0.6f)
                    GUI.Label(cellRect, $"{cell.Coordinates.x},{cell.Coordinates.y}", coordStyle);
            }
        }

        private void DrawEdgeHotspots(Rect rect, Vector2 center, float cellSize)
        {
            if (m_PortManager == null) return;

            foreach (var cell in m_GridManager.Cells)
            {
                bool isSelected = m_GridManager.SelectedCoords.Contains(cell.Coordinates);
                if (!isSelected) continue;

                float cellDrawX = center.x + cell.Coordinates.x * cellSize;
                float cellDrawY = center.y - cell.Coordinates.y * cellSize;

                if (m_SystemData.CenterAlignment)
                {
                    cellDrawX -= cellSize * 0.5f;
                    cellDrawY -= cellSize * 0.5f;
                }
                else
                {
                    cellDrawY -= cellSize;
                }

                foreach (EdgeSide side in Enum.GetValues(typeof(EdgeSide)))
                {
                    Vector2 edgePos = GetEdgeHotspotPosition(cellDrawX, cellDrawY, cellSize, side);
                    bool hasPort = cell.Ports != null && cell.Ports.Any(p => p.Side == side);

                    Color hotspotColor = hasPort ? Color.yellow : new Color(1f, 1f, 0f, 0.15f);
                    Handles.color = hotspotColor;

                    float hotspotSize = 12f * m_Zoom;
                    Handles.DrawSolidDisc(edgePos, Vector3.forward, hotspotSize);

                    if (!hasPort)
                    {
                        Handles.color = new Color(1f, 1f, 0f, 0.5f);
                        Handles.DrawWireDisc(edgePos, Vector3.forward, hotspotSize);
                    }
                }
            }
        }

        private Vector2 GetEdgeHotspotPosition(float cellX, float cellY, float cellSize, EdgeSide side)
        {
            switch (side)
            {
                case EdgeSide.顶部:
                    return new Vector2(cellX + cellSize * 0.5f, cellY);
                case EdgeSide.底部:
                    return new Vector2(cellX + cellSize * 0.5f, cellY + cellSize);
                case EdgeSide.左侧:
                    return new Vector2(cellX, cellY + cellSize * 0.5f);
                case EdgeSide.右侧:
                    return new Vector2(cellX + cellSize, cellY + cellSize * 0.5f);
                default:
                    return new Vector2(cellX + cellSize * 0.5f, cellY + cellSize * 0.5f);
            }
        }

        private void DrawPorts(Vector2 center, float cellSize, bool showPortsForAllCells)
        {
            if (m_PortManager == null) return;

            Handles.BeginGUI();

            foreach (var cell in m_GridManager.Cells)
            {
                if (cell.Ports == null || cell.Ports.Count == 0) continue;
                if (!showPortsForAllCells && !m_GridManager.SelectedCoords.Contains(cell.Coordinates))
                    continue;

                float cellDrawX = center.x + cell.Coordinates.x * cellSize;
                float cellDrawY = center.y - cell.Coordinates.y * cellSize;

                if (m_SystemData.CenterAlignment)
                {
                    cellDrawX -= cellSize * 0.5f;
                    cellDrawY -= cellSize * 0.5f;
                }
                else
                {
                    cellDrawY -= cellSize;
                }

                foreach (var port in cell.Ports)
                {
                    Vector2 portPos = GetPortPosition(cellDrawX, cellDrawY, cellSize, port.Side);

                    float handleSize = 8f * m_Zoom;

                    bool isSelected = m_PortManager.SelectedPort == port;
                    if (isSelected)
                    {
                        Handles.color = Color.yellow;
                        Handles.DrawWireDisc(portPos, Vector3.forward, handleSize);
                        Handles.color = new Color(1f, 1f, 0f, 0.5f);
                        Handles.DrawSolidDisc(portPos, Vector3.forward, handleSize * 0.7f);
                    }
                    else
                    {
                        Handles.color = new Color(0.3f, 0.3f, 0.3f);
                        Handles.DrawWireDisc(portPos, Vector3.forward, handleSize);
                        Handles.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                        Handles.DrawSolidDisc(portPos, Vector3.forward, handleSize * 0.7f);
                    }

                    // 绘制端口标签
                    if (m_SystemData.ShowPortLabels && m_Zoom > 0.5f)
                    {
                        DrawPortLabel(port, portPos, cellSize, isSelected);
                    }
                }
            }

            Handles.EndGUI();
        }

        private void DrawPortLabel(PortInstance port, Vector2 portPos, float cellSize, bool isSelected)
        {
            string labelText = string.IsNullOrEmpty(port.PortID) ? "?" : port.PortID;
            string typeIndicator = port.IOType == PortIOType.输入 ? "[I]" : "[O]";

            GUIStyle labelStyle = new GUIStyle(EditorStyles.whiteLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(m_SystemData.PortLabelFontSize * m_Zoom),
                fontStyle = FontStyle.Bold
            };

            Vector2 labelSize = labelStyle.CalcSize(new GUIContent(typeIndicator + labelText));
            float padding = 2f * m_Zoom;

            Vector2 labelPos = new Vector2(portPos.x - labelSize.x * 0.5f, portPos.y + 10f * m_Zoom);
            Rect labelRect = new Rect(labelPos.x - padding, labelPos.y - padding, labelSize.x + padding * 2, labelSize.y + padding * 2);

            Color bgColor;
            if (isSelected)
            {
                bgColor = new Color(0.6f, 0.5f, 0f, 0.9f);
            }
            else
            {
                bgColor = port.IOType == PortIOType.输入
                    ? new Color(0f, 0.3f, 0.6f, 0.85f)
                    : new Color(0.6f, 0.1f, 0.1f, 0.85f);
            }
            EditorGUI.DrawRect(labelRect, bgColor);

            Color textColor = Color.white;
            labelStyle.normal.textColor = textColor;
            GUI.Label(labelRect, typeIndicator + labelText, labelStyle);
        }

        private Vector2 GetPortPosition(float cellX, float cellY, float cellSize, EdgeSide side)
        {
            switch (side)
            {
                case EdgeSide.顶部:
                    return new Vector2(cellX + cellSize * 0.5f, cellY);
                case EdgeSide.底部:
                    return new Vector2(cellX + cellSize * 0.5f, cellY + cellSize);
                case EdgeSide.左侧:
                    return new Vector2(cellX, cellY + cellSize * 0.5f);
                case EdgeSide.右侧:
                    return new Vector2(cellX + cellSize, cellY + cellSize * 0.5f);
                default:
                    return new Vector2(cellX + cellSize * 0.5f, cellY + cellSize * 0.5f);
            }
        }

        private void DrawSelectionOverlay(Vector2 center, float cellSize)
        {
            if (m_IsDragging && !m_UseBrush)
            {
                bool isRemoveMode = m_IsRightMouseDown || (m_IsLeftMouseDown && Event.current.shift);
                Color rectColor = isRemoveMode ? ColorPalette.BoxSelectRemove : ColorPalette.BoxSelectAdd;
                Color borderColor = isRemoveMode ? Color.red : Color.green;

                EditorGUI.DrawRect(m_SelectionRect, rectColor);
                Handles.DrawSolidRectangleWithOutline(m_SelectionRect, Color.clear, borderColor);
            }

            if (m_UseBrush && (m_IsLeftMouseDown || m_IsRightMouseDown))
            {
                Handles.color = m_IsRightMouseDown ? ColorPalette.BrushCursorRemove : ColorPalette.BrushCursorAdd;
                Handles.DrawWireDisc(Event.current.mousePosition, Vector3.forward, m_BrushRadius * cellSize);
            }
        }

        public void HandleInput(Rect rect, int selectedTab)
        {
            if (m_IsResizing) return;

            Event e = Event.current;
            Vector2 localPos = e.mousePosition - rect.min;
            Vector2 center = rect.size / 2f + m_PanOffset;
            float cellSize = 50f * m_Zoom;

            if (e.type == EventType.ScrollWheel && rect.Contains(e.mousePosition))
            {
                float oldZoom = m_Zoom;
                m_Zoom = Mathf.Clamp(m_Zoom - e.delta.y * 0.02f, 0.1f, 5f);
                m_PanOffset -= (localPos - center) * (m_Zoom / oldZoom - 1f);
                e.Use();
                OnViewportChanged?.Invoke();
            }

            if (e.type == EventType.MouseDrag && e.button == 2)
            {
                m_PanOffset += e.delta;
                e.Use();
                OnViewportChanged?.Invoke();
            }

            if (e.button == 0 || e.button == 1)
            {
                if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
                {
                    if (e.button == 0) m_IsLeftMouseDown = true;
                    else m_IsRightMouseDown = true;
                    m_MouseDownPos = localPos;
                    m_IsDragging = false;
                    e.Use();
                }
                else if (e.type == EventType.MouseDrag && (m_IsLeftMouseDown || m_IsRightMouseDown))
                {
                    if (!m_IsDragging && Vector2.Distance(m_MouseDownPos, localPos) > k_DragThreshold)
                        m_IsDragging = true;

                    if (m_IsDragging)
                    {
                        m_SelectionRect = new Rect(
                            Mathf.Min(m_MouseDownPos.x, localPos.x),
                            Mathf.Min(m_MouseDownPos.y, localPos.y),
                            Mathf.Abs(localPos.x - m_MouseDownPos.x),
                            Mathf.Abs(localPos.y - m_MouseDownPos.y)
                        );

                        if (selectedTab == 0)
                        {
                            float gridX = (localPos.x - center.x) / cellSize;
                            float gridY = (center.y - localPos.y) / cellSize;
                            bool isAdd = m_IsLeftMouseDown && !e.shift;

                            if (m_UseBrush)
                                ApplyBrush(new Vector2(gridX, gridY), isAdd);
                            else
                                ProcessBoxAction(center, cellSize, isAdd);

                            OnDataChanged?.Invoke();
                        }
                        else if (m_IsLeftMouseDown)
                        {
                            UpdateSelection(center, cellSize);
                        }
                    }
                    e.Use();
                }
                else if (e.type == EventType.MouseUp)
                {
                    if (!m_IsDragging && rect.Contains(e.mousePosition))
                    {
                        Vector2Int hoveredCoord = GetHoveredCoord(localPos, center, cellSize);

                        if (selectedTab == 0)
                        {
                            if (e.button == 0)
                            {
                                if (m_GridManager.HasCell(hoveredCoord))
                                {
                                    m_GridManager.ToggleCellSelection(hoveredCoord);
                                }
                                else
                                {
                                    m_GridManager.AddCell(hoveredCoord);
                                    m_GridManager.SelectCell(hoveredCoord);
                                    OnDataChanged?.Invoke();
                                }
                            }
                            else if (e.button == 1)
                            {
                                m_GridManager.RemoveCell(hoveredCoord);
                                OnDataChanged?.Invoke();
                            }
                        }
                        else if (e.button == 0)
                        {
                            m_GridManager.ClearSelection();
                            if (m_GridManager.HasCell(hoveredCoord))
                                m_GridManager.SelectCell(hoveredCoord);
                        }
                    }

                    m_IsLeftMouseDown = m_IsRightMouseDown = m_IsDragging = false;
                    m_SelectionRect = Rect.zero;
                    e.Use();
                }
            }
        }

        public bool HandleEdgeHotspotClick(Vector2 mousePos, Rect viewRect, bool isLeftClick)
        {
            if (m_PortManager == null) return false;
            if (m_GridManager.SelectedCoords.Count == 0) return false;

            Vector2 localPos = mousePos - viewRect.min;
            Vector2 center = viewRect.size / 2f + m_PanOffset;
            float cellSize = 50f * m_Zoom;

            foreach (var coord in m_GridManager.SelectedCoords)
            {
                var cell = m_GridManager.GetCell(coord);
                if (cell == null) continue;

                float cellDrawX = center.x + cell.Coordinates.x * cellSize;
                float cellDrawY = center.y - cell.Coordinates.y * cellSize;

                if (m_SystemData.CenterAlignment)
                {
                    cellDrawX -= cellSize * 0.5f;
                    cellDrawY -= cellSize * 0.5f;
                }
                else
                {
                    cellDrawY -= cellSize;
                }

                float hotspotSize = 12f * m_Zoom;

                foreach (EdgeSide side in Enum.GetValues(typeof(EdgeSide)))
                {
                    Vector2 edgePos = GetEdgeHotspotPosition(cellDrawX, cellDrawY, cellSize, side);

                    if (Vector2.Distance(localPos, edgePos) <= hotspotSize * 1.5f)
                    {
                        m_PortManager.SetEditingCell(cell);

                        if (isLeftClick)
                        {
                            if (!cell.Ports.Any(p => p.Side == side))
                            {
                                m_PortManager.AddPort(side);
                                OnDataChanged?.Invoke();
                                return true;
                            }
                        }
                        else
                        {
                            if (cell.Ports.Any(p => p.Side == side))
                            {
                                m_PortManager.RemovePort(side);
                                OnDataChanged?.Invoke();
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public bool HandlePortClick(Vector2 mousePos, Rect viewRect)
        {
            if (m_PortManager == null) return false;

            Vector2 localPos = mousePos - viewRect.min;
            Vector2 center = viewRect.size / 2f + m_PanOffset;
            float cellSize = 50f * m_Zoom;

            foreach (var cell in m_GridManager.Cells)
            {
                if (cell.Ports == null || cell.Ports.Count == 0) continue;

                float cellDrawX = center.x + cell.Coordinates.x * cellSize;
                float cellDrawY = center.y - cell.Coordinates.y * cellSize;

                if (m_SystemData.CenterAlignment)
                {
                    cellDrawX -= cellSize * 0.5f;
                    cellDrawY -= cellSize * 0.5f;
                }
                else
                {
                    cellDrawY -= cellSize;
                }

                foreach (var port in cell.Ports)
                {
                    Vector2 portPos = GetPortPosition(cellDrawX, cellDrawY, cellSize, port.Side);
                    float handleSize = 8f * m_Zoom;

                    if (Vector2.Distance(localPos, portPos) <= handleSize * 1.5f)
                    {
                        m_PortManager.SelectPort(port);
                        OnDataChanged?.Invoke();
                        return true;
                    }
                }
            }

            return false;
        }

        private Vector2Int GetHoveredCoord(Vector2 localPos, Vector2 center, float cellSize)
        {
            float gridX = (localPos.x - center.x) / cellSize;
            float gridY = (center.y - localPos.y) / cellSize;

            if (m_SystemData.CenterAlignment)
                return new Vector2Int(Mathf.RoundToInt(gridX), Mathf.RoundToInt(gridY));
            else
                return new Vector2Int(Mathf.FloorToInt(gridX), Mathf.FloorToInt(gridY));
        }

        private void UpdateSelection(Vector2 center, float cellSize)
        {
            m_GridManager.ClearSelection();
            foreach (var cell in m_GridManager.Cells)
            {
                Vector2 pos = new Vector2(center.x + cell.Coordinates.x * cellSize, center.y - cell.Coordinates.y * cellSize);
                if (!m_SystemData.CenterAlignment)
                {
                    pos.x += cellSize * 0.5f;
                    pos.y -= cellSize * 0.5f;
                }
                if (m_SelectionRect.Contains(pos))
                    m_GridManager.SelectCell(cell.Coordinates, true);
            }
        }

        private void ProcessBoxAction(Vector2 center, float cellSize, bool isAdd)
        {
            List<Vector2Int> coords = m_GridManager.GetCoordsInRect(m_SelectionRect, center, cellSize, m_SystemData.CenterAlignment);

            if (isAdd)
            {
                m_GridManager.AddCellsInRect(coords);
                m_GridManager.SelectCells(coords);
            }
            else
            {
                m_GridManager.RemoveCells(coords);
            }
        }

        private void ApplyBrush(Vector2 gridPos, bool isAdd)
        {
            int range = Mathf.CeilToInt(m_BrushRadius);
            List<Vector2Int> coords = new List<Vector2Int>();

            for (int x = -range; x <= range; x++)
            {
                for (int y = -range; y <= range; y++)
                {
                    Vector2Int target = new Vector2Int(Mathf.RoundToInt(gridPos.x) + x, Mathf.RoundToInt(gridPos.y) + y);
                    if (Vector2.Distance(gridPos, target) <= m_BrushRadius)
                        coords.Add(target);
                }
            }

            if (isAdd)
            {
                m_GridManager.AddCellsInRect(coords);
                m_GridManager.SelectCells(coords);
            }
            else
            {
                m_GridManager.RemoveCells(coords);
            }
        }

        public void SetUseBrush(bool use) => m_UseBrush = use;
        public bool GetUseBrush() => m_UseBrush;
        public void SetBrushRadius(float radius) => m_BrushRadius = radius;
        public float GetBrushRadius() => m_BrushRadius;

        public void HandleSplitterInput(float totalWidth, float panelHeight)
        {
            Rect splitterRect = GUILayoutUtility.GetRect(5, panelHeight, GUILayout.Width(5));
            EditorGUI.DrawRect(splitterRect, new Color(0.1f, 0.1f, 0.1f));
            EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);

            if (Event.current.type == EventType.MouseDown && splitterRect.Contains(Event.current.mousePosition))
                m_IsResizing = true;

            if (m_IsResizing)
            {
                m_RightPanelWidth = totalWidth - Event.current.mousePosition.x;
                m_RightPanelWidth = Mathf.Clamp(m_RightPanelWidth, 250f, totalWidth * 0.8f);
            }

            if (Event.current.rawType == EventType.MouseUp)
                m_IsResizing = false;
        }
    }
}

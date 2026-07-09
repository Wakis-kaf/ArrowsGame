using System;
using System.Collections.Generic;
using System.Linq;
using GridsSpaceEditor.Data.Models;
using UnityEngine;

namespace GridsSpaceEditor.Core
{
    public class GridManager
    {
        private List<GridCellData> m_Cells = new List<GridCellData>();
        private HashSet<Vector2Int> m_SelectedCoords = new HashSet<Vector2Int>();
        private string m_DefaultType = "";

        public IReadOnlyList<GridCellData> Cells => m_Cells;
        public ICollection<Vector2Int> SelectedCoords => m_SelectedCoords;

        public event Action OnDataChanged;
        public event Action OnSelectionChanged;

        public void SetDefaultType(string type) => m_DefaultType = type;

        public void SetCells(List<GridCellData> cells)
        {
            m_Cells = cells ?? new List<GridCellData>();
            NotifyDataChanged();
        }

        public GridCellData GetCell(Vector2Int coord)
        {
            return m_Cells.FirstOrDefault(c => c.Coordinates == coord);
        }

        public bool HasCell(Vector2Int coord)
        {
            return m_Cells.Any(c => c.Coordinates == coord);
        }

        public void AddCell(Vector2Int coord)
        {
            if (!HasCell(coord))
            {
                m_Cells.Add(new GridCellData
                {
                    Name = "Cell",
                    Coordinates = coord,
                    Type = m_DefaultType
                });
                NotifyDataChanged();
            }
        }

        public void AddCell(Vector2Int coord, string name, string type)
        {
            if (!HasCell(coord))
            {
                m_Cells.Add(new GridCellData
                {
                    Name = name,
                    Type = type,
                    Coordinates = coord
                });
                NotifyDataChanged();
            }
        }

        public void RemoveCell(Vector2Int coord)
        {
            m_Cells.RemoveAll(c => c.Coordinates == coord);
            m_SelectedCoords.Remove(coord);
            NotifyDataChanged();
            NotifySelectionChanged();
        }

        public void RemoveCells(IEnumerable<Vector2Int> coords)
        {
            var coordList = coords.ToList();
            m_Cells.RemoveAll(c => coordList.Contains(c.Coordinates));
            foreach (var c in coordList)
                m_SelectedCoords.Remove(c);
            NotifyDataChanged();
            NotifySelectionChanged();
        }

        public void SelectCell(Vector2Int coord, bool addToSelection = false)
        {
            if (!addToSelection)
                m_SelectedCoords.Clear();
            m_SelectedCoords.Add(coord);
            NotifySelectionChanged();
        }

        public void ToggleCellSelection(Vector2Int coord)
        {
            if (m_SelectedCoords.Contains(coord))
                m_SelectedCoords.Remove(coord);
            else
                m_SelectedCoords.Add(coord);
            NotifySelectionChanged();
        }

        public void ClearSelection()
        {
            m_SelectedCoords.Clear();
            NotifySelectionChanged();
        }

        public void SelectCells(IEnumerable<Vector2Int> coords)
        {
            m_SelectedCoords.Clear();
            foreach (var c in coords)
                m_SelectedCoords.Add(c);
            NotifySelectionChanged();
        }

        public void MoveSelection(Vector2Int offset)
        {
            var selectedCells = m_Cells.Where(c => m_SelectedCoords.Contains(c.Coordinates)).ToList();
            HashSet<Vector2Int> nextCoords = new HashSet<Vector2Int>();
            
            foreach (var cell in selectedCells)
            {
                cell.Coordinates += offset;
                nextCoords.Add(cell.Coordinates);
            }
            
            m_SelectedCoords = nextCoords;
            NotifyDataChanged();
            NotifySelectionChanged();
        }

        public void UpdateCellProperties(Vector2Int coord, string name, string type, string description)
        {
            var cell = GetCell(coord);
            if (cell != null)
            {
                cell.Name = name;
                cell.Type = type;
                cell.Description = description;
                NotifyDataChanged();
            }
        }

        public void BatchApplyTemplate(GridCellData template)
        {
            foreach (var coord in m_SelectedCoords)
            {
                var cell = GetCell(coord);
                if (cell != null)
                {
                    cell.Name = template.Name;
                    cell.Type = template.Type;
                    cell.Description = template.Description;
                }
            }
            NotifyDataChanged();
        }

        public List<Vector2Int> GetCoordsInRect(Rect rect, Vector2 center, float cellSize, bool centerAlignment)
        {
            float xMinG = (rect.xMin - center.x) / cellSize;
            float xMaxG = (rect.xMax - center.x) / cellSize;
            float yMinG = (center.y - rect.yMax) / cellSize;
            float yMaxG = (center.y - rect.yMin) / cellSize;

            int minX, maxX, minY, maxY;
            if (centerAlignment)
            {
                minX = Mathf.RoundToInt(xMinG); maxX = Mathf.RoundToInt(xMaxG);
                minY = Mathf.RoundToInt(yMinG); maxY = Mathf.RoundToInt(yMaxG);
            }
            else
            {
                minX = Mathf.FloorToInt(xMinG); maxX = Mathf.FloorToInt(xMaxG);
                minY = Mathf.FloorToInt(yMinG); maxY = Mathf.FloorToInt(yMaxG);
            }

            List<Vector2Int> result = new List<Vector2Int>();
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                    result.Add(new Vector2Int(x, y));
            }
            return result;
        }

        public void AddCellsInRect(List<Vector2Int> coords)
        {
            foreach (var coord in coords)
            {
                if (!HasCell(coord))
                {
                    m_Cells.Add(new GridCellData
                    {
                        Name = "Cell",
                        Coordinates = coord,
                        Type = m_DefaultType
                    });
                }
            }
            NotifyDataChanged();
        }

        public void ClearAll()
        {
            m_Cells.Clear();
            m_SelectedCoords.Clear();
            NotifyDataChanged();
            NotifySelectionChanged();
        }

        /// <summary>批量导入开始：清空所有数据，但不触发事件。</summary>
        public void ImportBegin()
        {
            m_Cells.Clear();
            m_SelectedCoords.Clear();
        }

        /// <summary>批量导入结束：统一触发一次数据变更事件。</summary>
        public void ImportEnd()
        {
            NotifyDataChanged();
            NotifySelectionChanged();
        }

        public void AddCellInternal(GridCellData cell)
        {
            if (cell == null) return;
            m_Cells.Add(cell);
        }

        private void NotifyDataChanged()
        {
            OnDataChanged?.Invoke();
            EditorEventBus.Publish(new CellDataChangedEvent { Cells = m_Cells });
        }

        private void NotifySelectionChanged()
        {
            OnSelectionChanged?.Invoke();
            EditorEventBus.Publish(new SelectionChangedEvent { SelectedCoords = m_SelectedCoords });
        }
    }
}

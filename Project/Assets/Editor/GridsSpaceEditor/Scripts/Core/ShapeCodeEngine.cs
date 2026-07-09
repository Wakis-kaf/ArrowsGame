using System;
using System.Collections.Generic;
using System.Linq;
using GridsSpaceEditor.Data.Enums;
using GridsSpaceEditor.Data.Models;
using UnityEngine;

namespace GridsSpaceEditor.Core
{
    /// <summary>
    /// 形状码格式：
    /// [idx:x,y]                 = 格子 (x,y)，无端口
    /// [idx:x,y]|[it:PortID,0]  = 格子 (x,y)，有一个端口在第 0 边
    /// 多条目用 ; 分隔
    /// </summary>
    public class ShapeCodeEngine
    {
        private readonly PortManager m_PortManager;
        private GridManager m_GridManager;

        public ShapeCodeEngine(PortManager portManager)
        {
            m_PortManager = portManager;
        }

        public void SetGridManager(GridManager gridManager)
        {
            m_GridManager = gridManager;
        }

        /// <summary>从当前网格生成形状码字符串</summary>
        public string Generate(List<GridCellData> cells)
        {
            if (cells == null || cells.Count == 0)
                return string.Empty;

            List<string> entries = new List<string>();
            var sortedCells = cells.OrderBy(c => c.Coordinates.y).ThenBy(c => c.Coordinates.x);

            foreach (var cell in sortedCells)
            {
                if (cell.Ports == null || cell.Ports.Count == 0)
                {
                    entries.Add($"[idx:{cell.Coordinates.x},{cell.Coordinates.y}]");
                }
                else
                {
                    foreach (var port in cell.Ports)
                    {
                        entries.Add($"[idx:{cell.Coordinates.x},{cell.Coordinates.y}]|[it:{port.PortID},{(int)port.Side}]");
                    }
                }
            }

            return string.Join(";", entries);
        }

        /// <summary>
        /// 清空网格后按形状码重建（空形状码则清空网格）。
        /// </summary>
        public void Import(string shapeCode, Action onComplete = null)
        {
            if (m_GridManager == null) return;

            m_GridManager.ImportBegin();

            if (string.IsNullOrWhiteSpace(shapeCode))
            {
                m_GridManager.ImportEnd();
                onComplete?.Invoke();
                return;
            }

            try
            {
                string[] parts = shapeCode.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                Dictionary<Vector2Int, GridCellData> cellMap = new Dictionary<Vector2Int, GridCellData>();

                foreach (string part in parts)
                {
                    bool hasPort = part.Contains("|");
                    string basePart = hasPort ? part.Split('|')[0] : part;
                    string coordStr = basePart.Replace("[idx:", "").Replace("]", "");
                    string[] xy = coordStr.Split(',');

                    Vector2Int pos = new Vector2Int(int.Parse(xy[0]), int.Parse(xy[1]));

                    if (!cellMap.TryGetValue(pos, out var cell))
                    {
                        cell = new GridCellData
                        {
                            Name = "Cell",
                            Coordinates = pos
                        };
                        cellMap[pos] = cell;
                    }

                    if (hasPort)
                    {
                        string iStr = part.Split('|')[1].Replace("[it:", "").Replace("]", "");
                        string[] idSide = iStr.Split(',');

                        var port = new PortInstance
                        {
                            PortID = idSide[0],
                            Side = (EdgeSide)int.Parse(idSide[1])
                        };

                        var template = m_PortManager.Templates.FirstOrDefault(t => t.PortID == port.PortID);
                        if (template != null)
                            ApplyTemplateDefaults(port, template);

                        cell.Ports.Add(port);
                    }
                }

                foreach (var cell in cellMap.Values)
                    m_GridManager.AddCellInternal(cell);

                m_GridManager.ImportEnd();
            }
            catch (Exception e)
            {
                Debug.LogError($"形状码解析失败: {e.Message}");
                m_GridManager.ImportEnd();
            }

            onComplete?.Invoke();
        }

        public string EncodePort(PortInstance port)
        {
            return $"[it:{port.PortID},{(int)port.Side}]";
        }

        /// <summary>
        /// 仅补全元数据默认值，不覆盖端口核心标识信息。
        /// 这样形状码导入后各端口属性不会被预设库覆盖。
        /// </summary>
        private void ApplyTemplateDefaults(PortInstance port, PortInstance template)
        {
            if (string.IsNullOrEmpty(port.InputFilter))
                port.InputFilter = template.InputFilter;
            if (string.IsNullOrEmpty(port.InputDescription))
                port.InputDescription = template.InputDescription;
            if (string.IsNullOrEmpty(port.OutputType))
                port.OutputType = template.OutputType;
            if (string.IsNullOrEmpty(port.OutputDescription))
                port.OutputDescription = template.OutputDescription;
            if (string.IsNullOrEmpty(port.PortDescription))
                port.PortDescription = template.PortDescription;
        }

        public PortInstance DecodePort(string portString)
        {
            try
            {
                string cleaned = portString.Replace("[it:", "").Replace("]", "");
                string[] parts = cleaned.Split(',');
                return new PortInstance
                {
                    PortID = parts[0],
                    Side = (EdgeSide)int.Parse(parts[1])
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>从形状码提取端口定义，加入预设库（去重）</summary>
        public void ImportPortsFromShapeCode(string shapeCode, PortManager targetManager)
        {
            if (string.IsNullOrWhiteSpace(shapeCode))
                return;

            var uniquePorts = new Dictionary<string, PortInstance>();

            string[] parts = shapeCode.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                bool hasPort = part.Contains("|");
                if (!hasPort) continue;

                string portStr = part.Split('|')[1];
                string cleaned = portStr.Replace("[it:", "").Replace("]", "");
                string[] idSide = cleaned.Split(',');

                string portId = idSide[0];
                if (!uniquePorts.ContainsKey(portId))
                {
                    var port = new PortInstance
                    {
                        PortID = portId,
                        Side = (EdgeSide)int.Parse(idSide[1])
                    };
                    uniquePorts[portId] = port;
                }
            }

            foreach (var port in uniquePorts.Values)
            {
                targetManager.CreateNewPreset(port);
            }
        }
    }
}

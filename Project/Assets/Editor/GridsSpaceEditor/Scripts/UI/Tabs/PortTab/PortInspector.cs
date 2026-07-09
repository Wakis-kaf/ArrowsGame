using System;
using System.Linq;
using GridsSpaceEditor.Core;
using GridsSpaceEditor.Data.Enums;
using GridsSpaceEditor.Data.Models;
using UnityEditor;
using UnityEngine;

namespace GridsSpaceEditor.UI.Tabs.PortTab
{
    public class PortInspector
    {
        private PortManager m_PortManager;
        private GridManager m_GridManager;
        private ShapeCodeEngine m_ShapeCodeEngine;
        private Action m_OnChanged;
        private Action m_OnRequestRepaint;

        private EdgeSide? m_SelectedSide;

        public PortInspector(PortManager portManager, GridManager gridManager, ShapeCodeEngine shapeCodeEngine, Action onChanged, Action onRequestRepaint)
        {
            m_PortManager = portManager;
            m_GridManager = gridManager;
            m_ShapeCodeEngine = shapeCodeEngine;
            m_OnChanged = onChanged;
            m_OnRequestRepaint = onRequestRepaint;
        }

        public void Draw()
        {
            if (m_GridManager.SelectedCoords.Count == 0)
            {
                EditorGUILayout.HelpBox("请先在网格视图中选择一个格子", MessageType.Info);
                return;
            }

            var firstCoord = m_GridManager.SelectedCoords.First();
            var cell = m_GridManager.GetCell(firstCoord);

            if (cell == null)
            {
                EditorGUILayout.HelpBox("选中的位置没有格子数据", MessageType.Warning);
                return;
            }

            m_PortManager.SetEditingCell(cell);

            string coordInfo = "坐标 (" + firstCoord.x + ", " + firstCoord.y + ")";
            EditorGUILayout.LabelField("=== " + coordInfo + " ===", EditorStyles.boldLabel);

            DrawSideButtons(cell);
            EditorGUILayout.Space(5);

            DrawPortTabs(cell);
        }

        private void DrawSideButtons(GridCellData cell)
        {
            EditorGUILayout.LabelField("端口开关:", EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();
            foreach (EdgeSide side in Enum.GetValues(typeof(EdgeSide)))
            {
                bool has = cell.Ports.Any(p => p.Side == side);
                GUI.backgroundColor = has ? Color.green : Color.gray;

                string label = GetSideButtonLabel(side);

                if (GUILayout.Button(label, EditorStyles.miniButton, GUILayout.Height(28)))
                {
                    if (!has)
                        m_PortManager.AddPort(side);
                    else
                        m_PortManager.RemovePort(side);
                    m_OnChanged?.Invoke();
                    m_OnRequestRepaint?.Invoke();

                    // 如果删除的是当前选中的端口，清除选中状态
                    if (has && m_SelectedSide == side)
                    {
                        m_SelectedSide = null;
                    }
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private string GetSideButtonLabel(EdgeSide side)
        {
            return side switch
            {
                EdgeSide.顶部 => "↑顶",
                EdgeSide.右侧 => "→右",
                EdgeSide.底部 => "↓底",
                EdgeSide.左侧 => "←左",
                _ => side.ToString()
            };
        }

        private void DrawPortTabs(GridCellData cell)
        {
            EditorGUILayout.LabelField("端口配置:", EditorStyles.miniLabel);

            // 四个选项卡始终显示
            EditorGUILayout.BeginHorizontal();

            foreach (EdgeSide side in Enum.GetValues(typeof(EdgeSide)))
            {
                bool hasPort = cell.Ports.Any(p => p.Side == side);
                bool isSelected = m_SelectedSide == side;

                string tabLabel = GetSideTabLabel(side);
                Color btnColor;

                if (hasPort)
                {
                    var port = cell.Ports.First(p => p.Side == side);
                    string typeIcon = port.IOType == PortIOType.输入 ? "[I]" : "[O]";
                    tabLabel = tabLabel + " " + typeIcon;
                }

                if (isSelected)
                    btnColor = Color.yellow;
                else
                    btnColor = new Color(0.35f, 0.35f, 0.35f);

                GUI.backgroundColor = btnColor;
                if (GUILayout.Button(tabLabel, EditorStyles.miniButton, GUILayout.Height(26)))
                {
                    m_SelectedSide = side;
                    if (hasPort)
                    {
                        var port = cell.Ports.First(p => p.Side == side);
                        m_PortManager.SelectPort(port);
                    }
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);

            // 绘制选中端口的编辑表单
            DrawSelectedPortEditor(cell);
        }

        private string GetSideTabLabel(EdgeSide side)
        {
            return side switch
            {
                EdgeSide.顶部 => "顶部",
                EdgeSide.右侧 => "右侧",
                EdgeSide.底部 => "底部",
                EdgeSide.左侧 => "左侧",
                _ => side.ToString()
            };
        }

        private void DrawSelectedPortEditor(GridCellData cell)
        {
            if (m_SelectedSide == null)
            {
                EditorGUILayout.HelpBox("请从上方选择一个端口进行编辑", MessageType.Info);
                return;
            }

            var port = cell.Ports.FirstOrDefault(p => p.Side == m_SelectedSide);
            if (port == null)
            {
                EditorGUILayout.HelpBox("请在上方开启端口", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            Color oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.9f, 1f, 0.9f);
            string sideName = GetSideTabLabel(port.Side);
            EditorGUILayout.LabelField("编辑: " + sideName + " [" + (port.IOType == PortIOType.输入 ? "输入" : "输出") + "]", EditorStyles.boldLabel);
            GUI.backgroundColor = oldBg;

            EditorGUILayout.Space(3);

            EditorGUI.BeginChangeCheck();

            // 预设应用
            DrawPresetDropdown(port);

            EditorGUILayout.Space(3);

            port.PortID = EditorGUILayout.TextField("端口 ID", port.PortID);
            port.IOType = (PortIOType)EditorGUILayout.EnumPopup("类型", port.IOType);

            if (port.IOType == PortIOType.输入)
            {
                port.InputFilter = EditorGUILayout.TextField("过滤器", port.InputFilter);
                port.InputDescription = EditorGUILayout.TextField("描述", port.InputDescription);
            }
            else
            {
                port.OutputType = EditorGUILayout.TextField("数据类型", port.OutputType);
                port.OutputDescription = EditorGUILayout.TextField("描述", port.OutputDescription);
            }

            port.PortDescription = EditorGUILayout.TextArea(port.PortDescription, GUILayout.Height(30));

            if (EditorGUI.EndChangeCheck())
            {
                m_OnChanged?.Invoke();
            }

            EditorGUILayout.Space(3);

            if (GUILayout.Button("保存为新预设", GUILayout.Height(22)))
            {
                m_PortManager.SaveAsNewPreset(port);
                EditorUtility.DisplayDialog("成功", "已保存为新预设", "确定");
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPresetDropdown(PortInstance port)
        {
            var templates = m_PortManager.Templates;
            if (templates == null || templates.Count == 0)
            {
                EditorGUILayout.HelpBox("预设库为空", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("应用预设:", GUILayout.Width(70));

            // 获取当前选中的预设索引
            int currentIndex = -1;
            if (!string.IsNullOrEmpty(port.PresetName))
            {
                for (int i = 0; i < templates.Count; i++)
                {
                    if (templates[i].PortID == port.PresetName)
                    {
                        currentIndex = i;
                        break;
                    }
                }
            }

            // 构建预设名称数组
            string[] presetNames = new string[templates.Count + 1];
            presetNames[0] = "(不应用预设)";
            for (int i = 0; i < templates.Count; i++)
            {
                presetNames[i + 1] = templates[i].PortID;
            }

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup(currentIndex + 1, presetNames, GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck() && newIndex != currentIndex + 1)
            {
                if (newIndex == 0)
                {
                    // 不应用预设，保持当前值
                }
                else
                {
                    var preset = templates[newIndex - 1];
                    port.SyncFrom(preset, preset.PortID);
                    m_OnChanged?.Invoke();
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}

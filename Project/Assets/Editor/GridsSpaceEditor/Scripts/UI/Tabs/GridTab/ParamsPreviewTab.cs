using System.Linq;
using GridsSpaceEditor.Core;
using GridsSpaceEditor.Data.Models;
using UnityEditor;
using UnityEngine;

namespace GridsSpaceEditor.UI.Tabs.GridTab
{
    public class ParamsPreviewTab
    {
        private GridManager m_GridManager;
        private SystemData m_SystemData;
        private bool m_OnlyShowSelectedInParams = true;

        public event System.Action OnDataChanged;

        public ParamsPreviewTab(GridManager gridManager, SystemData systemData)
        {
            m_GridManager = gridManager;
            m_SystemData = systemData;
        }

        public void Draw()
        {
            m_OnlyShowSelectedInParams = EditorGUILayout.ToggleLeft("仅显示当前选中的格子", m_OnlyShowSelectedInParams);

            var displayList = m_OnlyShowSelectedInParams
                ? m_GridManager.Cells.Where(c => m_GridManager.SelectedCoords.Contains(c.Coordinates)).ToList()
                : m_GridManager.Cells.ToList();

            if (displayList.Count == 0)
            {
                EditorGUILayout.HelpBox("无选中项。", MessageType.Info);
                return;
            }

            if (m_SystemData.Templates.Count > 0 && GUILayout.Button("批量应用模版"))
            {
                GenericMenu menu = new GenericMenu();
                foreach (var t in m_SystemData.Templates)
                {
                    var template = t;
                    menu.AddItem(new GUIContent(t.Name), false, () =>
                    {
                        m_GridManager.BatchApplyTemplate(template);
                        OnDataChanged?.Invoke();
                    });
                }
                menu.ShowAsContext();
            }

            foreach (var cell in displayList)
            {
                DrawCellInspector(cell);
            }
        }

        private void DrawCellInspector(GridCellData cell)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUI.BeginChangeCheck();
            cell.Name = EditorGUILayout.TextField("名称", cell.Name);
            cell.Description = EditorGUILayout.TextField("描述", cell.Description);

            int idx = Mathf.Max(0, m_SystemData.GridTypes.IndexOf(cell.Type));
            idx = EditorGUILayout.Popup("类型", idx, m_SystemData.GridTypes.ToArray());

            if (m_SystemData.GridTypes.Count > 0)
                cell.Type = m_SystemData.GridTypes[idx];

            if (EditorGUI.EndChangeCheck())
                OnDataChanged?.Invoke();

            EditorGUILayout.LabelField($"坐标: {cell.Coordinates}");
            EditorGUILayout.EndVertical();
        }
    }
}

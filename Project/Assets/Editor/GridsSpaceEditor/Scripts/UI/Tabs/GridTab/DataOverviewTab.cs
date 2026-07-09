using GridsSpaceEditor.Core;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GridsSpaceEditor.UI.Tabs.GridTab
{
    public class DataOverviewTab
    {
        private GridManager m_GridManager;

        public DataOverviewTab(GridManager gridManager)
        {
            m_GridManager = gridManager;
        }

        public void Draw()
        {
            EditorGUILayout.LabelField($"总数: {m_GridManager.Cells.Count}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"已选中: {m_GridManager.SelectedCoords.Count}");

            if (m_GridManager.SelectedCoords.Count > 0)
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField("选中的坐标:", EditorStyles.boldLabel);

                foreach (var coord in m_GridManager.SelectedCoords.OrderBy(c => c.y).ThenBy(c => c.x))
                {
                    EditorGUILayout.LabelField($"  ({coord.x}, {coord.y})");
                }
            }
        }
    }
}

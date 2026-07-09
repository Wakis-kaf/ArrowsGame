using System;
using GridsSpaceEditor.Data.Models;
using GridsSpaceEditor.UI.Components;
using UnityEditor;
using UnityEngine;

namespace GridsSpaceEditor.UI.Tabs.GridTab
{
    public class GridEditTab
    {
        private GridView m_GridView;
        private SystemData m_SystemData;
        private Action m_OnSystemDataChanged;

        public GridEditTab(GridView gridView, SystemData systemData, Action onSystemDataChanged)
        {
            m_GridView = gridView;
            m_SystemData = systemData;
            m_OnSystemDataChanged = onSystemDataChanged;
        }

        public void Draw()
        {
            EditorGUI.BeginChangeCheck();
            m_SystemData.ShowAllPortsInGridEdit = EditorGUILayout.ToggleLeft(
                "显示所有端口（关闭则仅显示选中格子的端口）",
                m_SystemData.ShowAllPortsInGridEdit);
            if (EditorGUI.EndChangeCheck())
                m_OnSystemDataChanged?.Invoke();

            EditorGUILayout.Space(4);

            m_GridView.SetUseBrush(EditorGUILayout.Toggle("开启圆形笔刷", m_GridView.GetUseBrush()));

            if (m_GridView.GetUseBrush())
            {
                m_GridView.SetBrushRadius(EditorGUILayout.Slider("笔刷半径", m_GridView.GetBrushRadius(), 0.5f, 10f));
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "左键点击：创建/选中\n" +
                    "右键点击：删除\n" +
                    "左键拖拽：框选添加\n" +
                    "Shift+左键拖拽：框选删除",
                    MessageType.None);
            }
        }
    }
}

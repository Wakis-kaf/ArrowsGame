using GridsSpaceEditor.Core;
using GridsSpaceEditor.Data.Models;
using GridsSpaceEditor.UI.Shared;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GridsSpaceEditor.UI.Tabs.PortTab
{
    public class PortEditorTab
    {
        private GridManager m_GridManager;
        private PortManager m_PortManager;
        private ShapeCodeEngine m_ShapeCodeEngine;
        private PortInspector m_Inspector;
        private PortLibraryPanel m_LibraryPanel;

        private string m_ShapeCode = "";
        private System.Action m_OnDataChanged;
        private System.Action m_OnRequestRepaint;

        public PortEditorTab(
            GridManager gridManager,
            PortManager portManager,
            ShapeCodeEngine shapeCodeEngine,
            System.Action onDataChanged,
            System.Action onRequestRepaint)
        {
            m_GridManager = gridManager;
            m_PortManager = portManager;
            m_ShapeCodeEngine = shapeCodeEngine;
            m_OnDataChanged = onDataChanged;
            m_OnRequestRepaint = onRequestRepaint;

            m_Inspector = new PortInspector(portManager, gridManager, shapeCodeEngine, OnPortChanged, m_OnRequestRepaint);
            m_LibraryPanel = new PortLibraryPanel(portManager, OnLibraryChanged);
        }

        public void Draw(int subTabIndex)
        {
            switch (subTabIndex)
            {
                case 0:
                    DrawInspectorSection();
                    break;
                case 1:
                    DrawLibrarySection();
                    break;
                case 2:
                    DrawShapeCodeSection();
                    break;
            }
        }

        private void DrawInspectorSection()
        {
            m_Inspector.Draw();
        }

        private void DrawShapeCodeSection()
        {
            SectionHeader.Draw("形状码引擎");

            m_ShapeCode = EditorGUILayout.TextArea(m_ShapeCode, GUILayout.Height(60));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("立即导入并同步到网格"))
            {
                ImportFromShapeCode();
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawLibrarySection()
        {
            m_LibraryPanel.Draw();
        }

        private void ImportFromShapeCode()
        {
            m_ShapeCodeEngine.Import(m_ShapeCode, () =>
            {
                m_OnDataChanged?.Invoke();
                RefreshShapeCode();
            });
        }

        public void RefreshShapeCode()
        {
            m_ShapeCode = m_ShapeCodeEngine.Generate(m_GridManager.Cells.ToList());
        }

        private void OnPortChanged()
        {
            RefreshShapeCode();
            m_OnDataChanged?.Invoke();
        }

        private void OnLibraryChanged()
        {
            RefreshShapeCode();
        }

        public void SetShapeCode(string code)
        {
            m_ShapeCode = code;
        }
    }
}

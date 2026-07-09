using UnityEditor;
using UnityEngine;

namespace GridsSpaceEditor.UI.Components
{
    public class TabToolbar
    {
        private int m_SelectedIndex = 0;
        private string[] m_TabNames;

        public int SelectedIndex => m_SelectedIndex;

        public TabToolbar(params string[] tabNames)
        {
            m_TabNames = tabNames;
        }

        public int Draw()
        {
            EditorGUI.BeginChangeCheck();
            m_SelectedIndex = GUILayout.Toolbar(m_SelectedIndex, m_TabNames);
            if (EditorGUI.EndChangeCheck())
                GUI.FocusControl(null);
            return m_SelectedIndex;
        }

        public void SetIndex(int index)
        {
            m_SelectedIndex = Mathf.Clamp(index, 0, m_TabNames.Length - 1);
        }
    }
}

using UnityEditor;
using UnityEditor.UI;

namespace Framework.Runtime.UI.Editor
{
    [CustomEditor(typeof(UContainer), true)]
    public class UContainerEditor : ScrollRectEditor
    {
        public override void OnInspectorGUI()
        {
            PreDrawInspectorGUI();
            base.OnInspectorGUI();
        }

        protected virtual void PreDrawInspectorGUI()
        {
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_HorizontalScrollBarShow");
            if (UnitUIEditorTool.SerializedProperty(serializedObject, "m_HorizontalAutoHide").boolValue)
            {
                UnitUIEditorTool.SerializedProperty(serializedObject, "m_HorizontalAutoHideTimer");
                UnitUIEditorTool.SerializedProperty(serializedObject, "m_HorizontalShowDuration");
                UnitUIEditorTool.SerializedProperty(serializedObject, "m_HorizontalHideDuration");
            }

            UnitUIEditorTool.SerializedProperty(serializedObject, "m_VerticalScrollBarShow");
            if (UnitUIEditorTool.SerializedProperty(serializedObject, "m_VerticalAutoHide").boolValue)
            {
                UnitUIEditorTool.SerializedProperty(serializedObject, "m_VerticalAutoHideTimer");
                UnitUIEditorTool.SerializedProperty(serializedObject, "m_VerticalShowDuration");
                UnitUIEditorTool.SerializedProperty(serializedObject, "m_VerticalHideDuration");
            }

            UnitUIEditorTool.SerializedProperty(serializedObject, "m_AutoTolerance");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_EnableDragEventPass");
        }
    }
}
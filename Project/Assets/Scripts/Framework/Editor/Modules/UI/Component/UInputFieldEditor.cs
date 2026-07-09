using UnityEditor;
using UnityEditor.UI;

namespace Framework.Runtime.UI.Editor
{
    [CustomEditor(typeof(UInputField))]
    public class UInputFieldEditor : InputFieldEditor
    {
        public override void OnInspectorGUI()
        {
            OnPreInspectorGUI();
            base.OnInspectorGUI();
        }

        protected virtual void OnPreInspectorGUI()
        {
            if (UnitUIEditorTool.SerializedProperty(serializedObject, "m_EnablePromptBox").boolValue)
                UnitUIEditorTool.SerializedProperty(serializedObject, "m_PromptList");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_EnableDebounce");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_DebounceTime");
        }
    }
}
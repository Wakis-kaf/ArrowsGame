using UnityEditor;
using UnityEditor.UI;

namespace Framework.Runtime.UI.Editor
{
    [CustomEditor(typeof(UCheckBox), true)]
    public class UCheckBoxEditor: ToggleEditor
    {
        public override void OnInspectorGUI()
        {
            UnitUIEditorTool.SerializedProperty(serializedObject, "checkAnimCaller");
            UnitUIEditorTool.SerializedProperty(serializedObject, "disCheckAnimCaller");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_RedPoint");
            UnitUIEditorTool.SerializedProperty(serializedObject, "IsTmpText");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_Text");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_TmpTxt");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_IsSelect");
            base.OnInspectorGUI();
        }
    }
}
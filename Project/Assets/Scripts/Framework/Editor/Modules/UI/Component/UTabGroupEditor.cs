using UnityEditor;

namespace Framework.Runtime.UI.Editor
{
    [CustomEditor(typeof(UTabGroup), true)]
    public class UTabGroupEditor : UnityEditor.Editor
    {
        // protected override void PreDrawInspectorGUI()
        // {
        //     UnitUIEditorTool.SerializedProperty(serializedObject, "m_AllowMultiSelect");
        //     UnitUIEditorTool.SerializedProperty(serializedObject, "m_Tabs");
        //     base.PreDrawInspectorGUI();
        // }
    }
}
using UnityEditor;

namespace Framework.Runtime.UI.Editor
{
    [CustomEditor(typeof(UIBaseRender))]
    public class UIBaseRenderEdiator : USpriteEditor
    {
        
        public override void  OnInspectorGUI()
        {
            base.OnInspectorGUI();
            UnitUIEditorTool.SerializedProperty(serializedObject, "enterAnimCaller");
            UnitUIEditorTool.SerializedProperty(serializedObject, "exitAnimCaller");
            UnitUIEditorTool.SerializedProperty(serializedObject, "clickAnimCaller");
            UnitUIEditorTool.SerializedProperty(serializedObject, "downAnimCaller");
            UnitUIEditorTool.SerializedProperty(serializedObject, "upAnimCaller");
            UnitUIEditorTool.SerializedProperty(serializedObject, "selectAnimCaller");
            UnitUIEditorTool.SerializedProperty(serializedObject, "disSelectAnimCaller");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_IsSelect");
        }
    }
}
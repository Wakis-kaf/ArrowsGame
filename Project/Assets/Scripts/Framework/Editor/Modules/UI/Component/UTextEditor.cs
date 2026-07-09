using UnityEditor;
using UnityEditor.UI;

namespace Framework.Runtime.UI.Editor
{
    [CustomEditor(typeof(UText))]
    public class UTextEditor : TextEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            // UnitUIEditorTool.SerializedProperty(serializedObject, "m_PointerEnterAnimationName");
            // UnitUIEditorTool.SerializedProperty(serializedObject, "m_PointerEnterAnimationName");
            // UnitUIEditorTool.SerializedProperty(serializedObject, "m_Animator");
            UnitUIEditorTool.SerializedProperty(serializedObject, "mouseEnterAnimCaller");
            UnitUIEditorTool.SerializedProperty(serializedObject, "mouseExitAnimCaller");
            EditorGUILayout.Space(); //空行
        }
    }
}
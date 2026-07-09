using TMPro.EditorUtilities;
using UnityEditor;

namespace Framework.Runtime.UI.Editor
{
    [CanEditMultipleObjects, CustomEditor(typeof(UTMPText), true)]
    public class UTMPTextEditor : TMP_EditorPanelUI
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
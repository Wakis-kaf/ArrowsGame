using UnityEditor;
using UnityEditor.UI;

namespace Framework.Runtime.UI.Editor
{
    [CustomEditor(typeof(USimpleCheckBox), true)]
    public class USimpleCheckBoxEditor : ToggleEditor
    {
        public override void OnInspectorGUI()
        {
            OnPreInspectorGUI();
            base.OnInspectorGUI();
        }

        protected virtual void OnPreInspectorGUI()
        {
            if (UnitUIEditorTool.SerializedProperty(serializedObject, "m_IsEnableTMPText").boolValue)
            {
                UnitUIEditorTool.SerializedProperty(serializedObject, "m_UTMPText");
            }
            else
            {
                UnitUIEditorTool.SerializedProperty(serializedObject, "m_UText");
            }

            // UnitUIEditorTool.SerializedProperty(serializedObject, "m_CheckAnimationName");
            // UnitUIEditorTool.SerializedProperty(serializedObject, "m_DisCheckAnimationName");
            // UnitUIEditorTool.SerializedProperty(serializedObject, "m_Animator"); 
            UnitUIEditorTool.SerializedProperty(serializedObject, "checkAnimCaller");
            UnitUIEditorTool.SerializedProperty(serializedObject, "disCheckAnimCaller");
        }
    }
}
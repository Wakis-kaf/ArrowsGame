using UnityEditor;
using UnityEditor.UI;

namespace Framework.Runtime.UI.Editor
{
    [CustomEditor(typeof(UButton), false)]
    public class UButtonEditor : ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            OnPreDrawInspectorGUI();
            base.OnInspectorGUI();
        }

        protected virtual void OnPreDrawInspectorGUI()
        {
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_UText");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_UTMPText");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_RedPoint");
            UnitUIEditorTool.SerializedProperty(serializedObject, "isEnableBindShortcuts");
            UnitUIEditorTool.SerializedProperty(serializedObject, "loneTimePressThreshold");
            UnitUIEditorTool.SerializedProperty(serializedObject, "isEnableProtect");
            UnitUIEditorTool.SerializedProperty(serializedObject, "enableTMPTxt");
            UnitUIEditorTool.SerializedProperty(serializedObject, "protectTime");
            // UnitUIEditorTool.SerializedProperty(serializedObject, "m_MouseEnterAnimationName");
            // UnitUIEditorTool.SerializedProperty(serializedObject, "m_MouseExitAnimationName");
            // UnitUIEditorTool.SerializedProperty(serializedObject, "m_MouseDownAnimationName");
            // UnitUIEditorTool.SerializedProperty(serializedObject, "m_MouseUpAnimationName");
            // UnitUIEditorTool.SerializedProperty(serializedObject, "m_ClickAnimationName");
            // UnitUIEditorTool.SerializedProperty(serializedObject, "m_ProtectAnimationName");
            UnitUIEditorTool.SerializedProperty(serializedObject, "mouseEnterAnimCaller");
            UnitUIEditorTool.SerializedProperty(serializedObject, "mouseExitAnimCaller");
            UnitUIEditorTool.SerializedProperty(serializedObject, "mouseDownAnimCaller");
            UnitUIEditorTool.SerializedProperty(serializedObject, "mouseUpAnimCaller");
            UnitUIEditorTool.SerializedProperty(serializedObject, "clickAnimCaller");
            UnitUIEditorTool.SerializedProperty(serializedObject, "clickProtectAnimCaller");
            //UnitUIEditorTool.SerializedProperty(serializedObject, "m_UIAnimator");
        }
    }
}
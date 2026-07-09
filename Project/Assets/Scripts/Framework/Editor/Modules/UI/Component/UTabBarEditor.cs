using UnityEditor;

namespace Framework.Runtime.UI.Editor
{
    [CustomEditor(typeof(USimpleTabBar), true)]
    public class UTabBarEditor : USimpleCheckBoxEditor
    {
        protected override void OnPreInspectorGUI()
        {
            base.OnPreInspectorGUI();
            // UnitUIEditorTool.SerializedProperty(serializedObject, "m_OpenAnimationName");
            // UnitUIEditorTool.SerializedProperty(serializedObject, "m_CloseAnimationName");
            //UnitUIEditorTool.SerializedProperty(serializedObject, "m_Animator");
        }
    }
}
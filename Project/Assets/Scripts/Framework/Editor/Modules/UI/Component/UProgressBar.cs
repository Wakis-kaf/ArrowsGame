using UnityEditor;
using UnityEditor.UI;

namespace Framework.Runtime.UI.Editor
{
    [CustomEditor(typeof(UProgressBar))]
    public class UProgressBarEditor : SliderEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            UnitUIEditorTool.SerializedProperty(serializedObject, "isFillModel");
            UnitUIEditorTool.SerializedProperty(serializedObject, "fillSyncTransform");
            UnitUIEditorTool.SerializedProperty(serializedObject, "isFillClampMin");
            serializedObject.ApplyModifiedProperties();
        }

    }
}
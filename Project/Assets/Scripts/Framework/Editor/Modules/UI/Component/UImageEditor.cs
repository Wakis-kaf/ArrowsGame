using UnityEditor;
using UnityEditor.UI;

namespace Framework.Runtime.UI.Editor
{
    [CustomEditor(typeof(UImage),true)]
    [CanEditMultipleObjects]
    public class UImageEditor : ImageEditor
    {
        public override void OnInspectorGUI()
        {
            OnPreInspectorGUI();
            base.OnInspectorGUI();
            OnAfterInspectorGUI();
        }

        protected virtual void OnPreInspectorGUI()
        {
        }

        protected virtual void OnAfterInspectorGUI()
        {
        }
    }
}
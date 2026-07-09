using UnityEditor;

namespace Framework.Runtime.UI.Editor
{
    [CustomEditor(typeof(USimpleCheckBoxGroup))]
    public class USimpleCheckBoxGroupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            OnPreInspectorGUI();
            base.OnInspectorGUI();
        }

        protected virtual void OnPreInspectorGUI()
        {
        }
    }
}
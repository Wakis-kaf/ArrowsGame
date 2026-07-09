using System;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Framework.Runtime.UI.Editor
{
    public class SpriteEmitEditor
    {
        private static EditorWindow Window
        {
            get
            {
                Init();
                return (EditorWindow) _instanceField.GetValue(null);
            }
        }

        private static MethodInfo _openHandler;

        //private static MethodInfo _selectHandler;
        private static FieldInfo _instanceField; //

        private static bool _hasInit;

        private static void Init()
        {
            if (!_hasInit)
            {
                Type type = Type.GetType("UnityEditor.SpriteEditorWindow,UnityEditor");
                if (type == null) return;
                _openHandler = type.GetMethod("GetWindow", BindingFlags.Public | BindingFlags.Static);
                //_selectHandler = type.GetMethod("SelectSpriteIndex", BindingFlags.NonPublic | BindingFlags.Instance);
                _instanceField = type.GetField("s_Instance", BindingFlags.Public | BindingFlags.Static);
                _hasInit = true;
            }
        }

        public static void Open(Object obj)
        {
            Selection.activeObject = obj;
            Init();
            _openHandler.Invoke(null, null);
        }
    }
}
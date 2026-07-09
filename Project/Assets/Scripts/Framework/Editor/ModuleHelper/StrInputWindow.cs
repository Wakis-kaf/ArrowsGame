using System;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor.ModuleHelpes
{
    public class StrInputWindow : EditorWindow
    {
        private string targetResult;
        private string desc;
        private Func<string, string> onCheck;
        private Action<string> onResult;

        public static void AddWindow(string title, string desc, Action<string> onResult,
            Func<string, string> onCheck = null)
        {
            Rect rect = new Rect(0, 0, 300, 200);
            StrInputWindow window =
                GetWindowWithRect(typeof(StrInputWindow), rect, true, title) as StrInputWindow;
            window.desc = desc;
            window.onCheck = onCheck;
            window.onResult = onResult;
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(20);
            EditorGUILayout.LabelField(desc);
            GUILayout.Space(10);
            targetResult = EditorGUILayout.TextField(targetResult);
            GUILayout.Space(10);
            if (GUILayout.Button("确定", GUILayout.Width(80)))
            {
                OnConfirm();
            }
        }

        private void OnConfirm()
        {
            if (onCheck != null)
            {
                string errorMsg = onCheck(targetResult);
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    EditorUtility.DisplayDialog("错误", errorMsg, "确定");
                    return;
                }
            }

            if (onResult != null)
            {
                onResult(targetResult);
            }

            Close();
        }
    }
}
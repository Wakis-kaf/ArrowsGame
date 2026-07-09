using System.Collections;
using System.Collections.Generic;
using System.IO;
using Framework.Editor.ModuleHelpes;
using Framework.Runtime;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor
{
    public class AssetHelper
    {
        [MenuItem("Assets/拷贝资源路径", false, 100)]
        public static void CopyAssetPath()
        {
            var selection = Selection.activeObject;
            string path = AssetDatabase.GetAssetPath(selection);
            Debug.Log(path);
            CopyToClipboard(path);
        }

        [MenuItem("Assets/拷贝资源Pointer路径", false, 100)]
        public static void CopyAssetPointPath()
        {
            var selection = Selection.activeObject;
            string path = AssetDatabase.GetAssetPath(selection);
            string filePath = path;
            int lastPoint = filePath.LastIndexOf(".");
            if (lastPoint != -1)
            {
                filePath = filePath.Substring(0, lastPoint);
            }
            string suffix = Path.Combine(Application.dataPath, GameConfig.NameField_ResBuildDir);
            if (filePath.StartsWith(suffix))
            {
                filePath = filePath.Replace(suffix, "");
            }
            suffix = Path.Combine("Assets/", GameConfig.NameField_ResBuildDir) + "/";
            if (filePath.StartsWith(suffix))
            {
                filePath = filePath.Replace(suffix, "");
            }
            Debug.Log(filePath);
            CopyToClipboard(filePath);
        }

        public static void CopyToClipboard(string text)
        {
            GUIUtility.systemCopyBuffer = text;
            Debug.Log("已复制到剪贴板: " + text);
        }

        // Update is called once per frame
        private void Update()
        {
        }
    }
}
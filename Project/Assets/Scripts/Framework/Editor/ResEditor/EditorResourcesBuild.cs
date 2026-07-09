using System.IO;
using UnityEditor;
using UnityEngine;

namespace  Framework.Editor.AssetBundleBuildHelp
{
    public class EditorResourcesBuild
    {
        [MenuItem("Framework/Resources/Build EditorResources FileList")]
        public static void BuildResourcesExportConfig()
        {
            string path = Application.dataPath + "/Editor/Resources/";
            if (!Directory.Exists(path))
            {
                Debug.LogWarningFormat("path{0} not exists ", path);
                return;
            }

            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

            string txt = "";
            foreach (var file in files)
            {
                if (file.EndsWith(".meta")) continue;

                string name = file.Replace(path, "");
                name = name.Substring(0, name.LastIndexOf("."));
                name = name.Replace("\\", "/");
                txt += name + "\n";
            }

            path = path + "EditorFileList.bytes";
            if (File.Exists(path)) File.Delete(path);
            File.WriteAllText(path, txt);
            AssetDatabase.Refresh();
        }
    }
}
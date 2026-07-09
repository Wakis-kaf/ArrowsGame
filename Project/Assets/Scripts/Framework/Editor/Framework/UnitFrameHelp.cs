using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System.Runtime.InteropServices;
using Framework.Runtime;
using System.IO;
using Framework.Runtime.Config;
using Framework.Runtime.UI;
using Framework.Runtime.UI.Editor;
using Framework.Utils;
using UnityEngine.EventSystems;
using EditorUtility = UnityEditor.EditorUtility;

namespace Framework.Editor.FrameHelp
{
    public class UnitFrameHelp
    {
        [MenuItem("Framework/Create UnitFrame Driver", priority = 0)]
        [MenuItem("GameObject/UnitFramework/UnitFrame Init", priority = 2)]
        public static void CreateUnitFrameDriver()
        {
            if (Transform.FindObjectOfType<GameAppShell>() == null)
            {
                new GameObject("Framework Shell").AddComponent<GameAppShell>();
            }
            if (Transform.FindObjectOfType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
            if (Transform.FindObjectOfType<UIRoot>() == null)
            {
                UnitUIEditor.CreateUIRoot();
            }
        }
        [MenuItem("Framework/Help/Clear Archives")]
        public static void ClearArchives()
        {
            string path = GameObject.FindObjectOfType<FrameworkSetting>().PlatformDir;
            string archivePath = Utility.Path.PathCombine(path, "Archives");
            if (Directory.Exists(archivePath))
            {
                Utility.FileUtil.DeleteDir(archivePath);
                UnityEngine.Debug.Log($"清除日志目录成功{archivePath}");
                return;
            }
            else
            {
                UnityEngine.Debug.LogError($"清除日志目录失败，目录不存在 {archivePath}");
            }
            
        }

        [MenuItem("Framework/Help/Open Persistent Path")]
        public static void OpenPersistentPath()
        {
            string path = Application.persistentDataPath.Replace("/", "\\"); ;
            Process.Start("explorer.exe", path);
        }

        [MenuItem("Framework/Help/Open Platform Path")]
        public static void OpenPlatformPath()
        {
            string path = GameConfigStatic.GetPlatformPathStatic();
            path = path.Replace("/", "\\"); ;
            Process.Start("explorer.exe", path);
        }

        [MenuItem("Framework/Config/Create GameConfig Template")]
        public static void CreateExpConfigTemplate()
        {
            string path = GetGameCfgPath();
            // 判断文件是否存在
            //string template = Utility.Json.ToJson(GameConfigTemplate.GetInitGameConfigTemplate());
            string template = "{}";
            Utility.FileUtil.SaveFile(path, template);
            string dirPath = Utility.FileUtil.DecodeDirPathFromFullPath(path);
            dirPath = dirPath.Replace("/", "\\");
            Process.Start("explorer.exe", dirPath);
        }

        public static string GetGameCfgPath()
        {
            return Utility.Path.PathCombine(Utility.Path.GetPersistentDataPath(), GameConfig.FieldName_GameConfigJson);
        }
    }
}
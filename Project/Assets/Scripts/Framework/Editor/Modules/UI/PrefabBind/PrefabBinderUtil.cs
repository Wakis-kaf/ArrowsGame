using Framework.Editor.ModuleHelpes;
using Framework.Runtime;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.UI.PrefabBinderHelp;
using Framework.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Framework.Runtime.UI.Editor.PrefabBinderHelp
{
    public static class PrefabBinderUtil
    {
        private static readonly Regex m_DirRegex = new Regex(@"/UI/(?<module>\S+?)/(?:\S+)/", RegexOptions.IgnoreCase);
        private static readonly Regex m_FileNameRegex = new Regex(@"^.*/(?<name>\S+)\.prefab$", RegexOptions.IgnoreCase);

        [MenuItem("Assets/UI预制体/创建或更新UI预制体定义", false, 100)]
        public static void CreateOrUpdateUIDeclare()
        {
            GameObject m_SelectedGameObject = Selection.activeGameObject;
            string m_FilePath = GetSelectedPrefabPath(m_SelectedGameObject);

            if (!TryGetScriptContext(m_SelectedGameObject, m_FilePath, out var m_Context)) return;

            if (!File.Exists(m_Context.ScriptFilePath))
            {
                string m_TemplatePath = GetUITemplatePath(m_Context.TemplateType);
                string m_Directory = Path.GetDirectoryName(m_Context.ScriptFilePath);

                if (!Directory.Exists(m_Directory))
                {
                    Directory.CreateDirectory(m_Directory);
                }

                ModuleHelperUtils.CreateAndWriteFileByTemplate(m_TemplatePath, m_Context.ScriptFilePath, "$NAME", m_Context.TargetClassName);
            }

            UpdateUIDeclare();
        }

        public static void UpdateUIDeclare()
        {
            GameObject m_SelectedGameObject = Selection.activeGameObject;
            if (m_SelectedGameObject.TryGetComponent<PrefabCreateOptions>(out var m_CreateOptions))
            {
                UpdatePrefabBinderDeclare(m_CreateOptions.UICreateOption.isCustomFileName, m_CreateOptions.UICreateOption.customFileName);
            }
            else
            {
                UpdatePrefabBinderDeclare();
            }
        }

        public static void OpenScript(bool isCustomFileName = false, string customFileName = "")
        {
            GameObject m_SelectedGameObject = Selection.activeGameObject;
            string m_FilePath = GetSelectedPrefabPath(m_SelectedGameObject);

            if (!TryGetScriptContext(m_SelectedGameObject, m_FilePath, out var m_Context, isCustomFileName, customFileName)) return;

            if (File.Exists(m_Context.ScriptFilePath))
            {
                string m_AssetPath = m_Context.ScriptFilePath.Replace(Application.dataPath, "Assets");
                Object m_ScriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(m_AssetPath);
                if (m_ScriptAsset != null)
                {
                    AssetDatabase.OpenAsset(m_ScriptAsset, 0);
                }
            }
            else
            {
                Debug.LogError("未找到脚本文件：" + m_Context.ScriptFilePath);
            }
        }

        public static void UpdatePrefabBinderDeclare(bool isCustomFileName = false, string customFileName = "")
        {
            GameObject m_SelectedGameObject = Selection.activeGameObject;
            string m_FilePath = GetSelectedPrefabPath(m_SelectedGameObject);

            if (!TryGetScriptContext(m_SelectedGameObject, m_FilePath, out var m_Context, isCustomFileName, customFileName))
            {
                return;
            }

            if (string.IsNullOrEmpty(m_Context.ScriptFilePath) || !File.Exists(m_Context.ScriptFilePath))
            {
                Debug.LogError($"未找到脚本 搜索路径: {m_Context.SearchDir} 名称 : {m_Context.TargetClassName}");
                return;
            }

            string m_FileContent = Utility.FileUtil.ReadFile(m_Context.ScriptFilePath);
            m_FileContent = UpdateFileContent(m_Context.Binder, m_Context.Options, m_FileContent, m_FilePath);
            Utility.FileUtil.SaveFile(m_Context.ScriptFilePath, m_FileContent);
            Debug.Log($"更新脚本成功: {m_Context.TargetClassName}");
        }

        private static bool TryGetScriptContext(GameObject m_Go, string m_PrefabPath, out ScriptContext m_Context, bool m_IsCustomFile = false, string m_CustomName = "")
        {
            m_Context = default;
            if (string.IsNullOrEmpty(m_PrefabPath))
            {
                Debug.LogError("当前选中的物体不是一个预制体");
                return false;
            }
            m_Go.TryGetComponent<PrefabCreateOptions>(out m_Context.Options);
            var m_DirMatch = m_DirRegex.Match(m_PrefabPath);
            var m_NameMatch = m_FileNameRegex.Match(m_PrefabPath);
            bool isDirMathSuc = (m_Context.Options != null && m_Context.Options.createOptionType != CreateOptionType.UIClass)
                || m_DirMatch.Success;

            if (!isDirMathSuc || !m_NameMatch.Success)
            {
                Debug.LogError($"路径匹配失败: {m_PrefabPath}");
                return false;
            }

            m_Context.Binder = m_Go?.GetComponent<PrefabBinder>();
            if (m_Context.Binder == null)
            {
                Debug.LogError("prefabBinder 为空");
                return false;
            }



            string m_ModuleName = m_DirMatch.Groups["module"].Value.Replace("/Prefabs", "");
            m_Context.TargetClassName = m_IsCustomFile && !string.IsNullOrEmpty(m_CustomName) ? m_CustomName : m_NameMatch.Groups["name"].Value;

            string m_SubDir = "";
            if (m_Context.Options != null && m_Context.Options.createOptionType == CreateOptionType.UIClass)
            {
                m_Context.TemplateType = m_Context.Options.UICreateOption.templateType;
                if (string.IsNullOrEmpty(m_CustomName) && m_Context.Options.UICreateOption.isCustomFileName && !string.IsNullOrEmpty(m_Context.Options.UICreateOption.customFileName))
                {
                    m_Context.TargetClassName = m_Context.Options.UICreateOption.customFileName;
                }
                m_SubDir = GetUIDirPath(m_Context.TemplateType);
            }
            else if (m_Context.Options != null && m_Context.Options.createOptionType == CreateOptionType.Reference)
            {
                if (string.IsNullOrEmpty(m_CustomName) && m_Context.Options.ReferenceCreateOption.isCustomFileName && !string.IsNullOrEmpty(m_Context.Options.ReferenceCreateOption.customFileName))
                {
                    m_Context.TargetClassName = m_Context.Options.ReferenceCreateOption.customFileName;
                }
            }

            List<string> m_SearchRoots = new List<string>
            {
                Path.Combine(Application.dataPath, "Scripts/Game/Runtime/Modules", m_ModuleName),
                Path.Combine(Application.dataPath, "Scripts/Game/Hotfix/Modules", m_ModuleName),
                Path.Combine(Application.dataPath, "Scripts/Game/Runtime/Modules"),
                Path.Combine(Application.dataPath, "Scripts/Game/Hotfix/Modules")
            };

            m_Context.ScriptFilePath = SearchFileInDirectories(m_SearchRoots, m_Context.TargetClassName + ".cs");

            if (!string.IsNullOrEmpty(m_Context.ScriptFilePath))
            {
                m_Context.SearchDir = Path.GetDirectoryName(m_Context.ScriptFilePath);
            }
            else
            {
                m_Context.SearchDir = Path.Combine(Path.Combine(Application.dataPath, "Scripts/Game/Runtime/Modules", m_ModuleName), m_SubDir);
                m_Context.ScriptFilePath = Path.Combine(m_Context.SearchDir, m_Context.TargetClassName + ".cs");
            }

            return true;
        }

        private static string SearchFileInDirectories(IEnumerable<string> m_Roots, string m_FileName)
        {
            foreach (var m_Root in m_Roots)
            {
                if (!Directory.Exists(m_Root)) continue;
                string[] m_Files = Directory.GetFiles(m_Root, m_FileName, SearchOption.AllDirectories);
                if (m_Files.Length > 0) return m_Files[0].Replace("\\", "/");
            }
            return null;
        }

        private struct ScriptContext
        {
            public PrefabBinder Binder;
            public PrefabCreateOptions Options;
            public string TargetClassName;
            public string ScriptFilePath;
            public string SearchDir;
            public UITemplateType TemplateType;
        }

        private static string GetUITemplatePath(UITemplateType m_TemplateType)
        {
            return $"{Application.dataPath}/Scripts/Framework/Editor/ModuleHelper/UICSTemplate/{m_TemplateType}Template.txt";
        }

        private static string GetUIDirPath(UITemplateType m_TemplateType)
        {
            switch (m_TemplateType)
            {
                case UITemplateType.DisplayUnit:
                case UITemplateType.Panel:
                case UITemplateType.View:
                    return "View";
                case UITemplateType.ListDisplayUnit:
                    return "View/Renders";
                case UITemplateType.Component:
                    return "View/Components";
                default:
                    return "";
            }
        }

        private static string UpdateFileContent(PrefabBinder m_PrefabBinder, PrefabCreateOptions m_CreateOption, string m_FileContent, string m_PrefabPath)
        {
            m_FileContent = UpdateDeclare(m_PrefabBinder, m_CreateOption, m_FileContent);

            if (m_CreateOption != null && m_CreateOption.createOptionType == CreateOptionType.UIClass)
            {
                m_FileContent = UpdateExtract(m_PrefabBinder, m_FileContent);
                if (m_CreateOption.UICreateOption.isOverrideAssetLinkGet)
                    m_FileContent = UpdatePrefabReference(m_PrefabBinder, m_FileContent, m_PrefabPath);
                if (m_CreateOption.UICreateOption.isOverrideOpenLayer)
                    m_FileContent = UpdatePrefabLayerGet(m_PrefabBinder, m_CreateOption, m_FileContent, m_PrefabPath);
            }
            return m_FileContent;
        }
        private static string GetPrefabBinderDeclareStr(string prefabBinderName)
        {
            return $"\t\tprivate PrefabBinder m_{prefabBinderName};\r\n        public PrefabBinder {prefabBinderName}\r\n        {{\r\n            get\r\n            {{\r\n                if(m_{prefabBinderName} == null) {{ \r\n                    m_{prefabBinderName}= gameObject.GetComponent<PrefabBinder>() ?? gameObject.AddComponent<PrefabBinder>(); \r\n                }}\r\n                return m_{prefabBinderName};\r\n            }}\r\n        }}";
        }
        private static string UpdateDeclare(PrefabBinder m_PrefabBinder, PrefabCreateOptions createOption, string fileContent)
        {
            Regex declareAreaRegex = new Regex(@"#region PrefabBinder 自动引用区域 开始((.|\n)*?)#endregion PrefabBinder 自动引用区域 结束");
            var match = declareAreaRegex.Match(fileContent);
            var nameList = m_PrefabBinder.NameList;
            StringBuilder sb = new StringBuilder();
            if (createOption.createOptionType == CreateOptionType.Reference &&
             !createOption.ReferenceCreateOption.isUseCustomGetter)
            {
                sb.AppendLine(GetPrefabBinderDeclareStr(createOption.ReferenceCreateOption.GetPrefabBinderName()));
            }
            for (int i = 0; i < nameList.Count; i++)
            {
                var m_Obj = m_PrefabBinder.GetObj<Object>(nameList[i]);
                if (m_Obj == null) continue;
                string m_FullName = m_Obj.GetType().FullName;
                string getter = "";
                string filedType = "private";
                if (createOption.createOptionType == CreateOptionType.Reference)
                {
                    string prefabBinderName = createOption.ReferenceCreateOption.GetPrefabBinderName();
                    getter = $"=>{prefabBinderName}?.GetObj<{m_FullName}>(\"{nameList[i]}\")";
                    filedType = createOption.ReferenceCreateOption.isPublicFiled ? "public" : "private";
                }

                sb.AppendLine($"\t\t{filedType} {m_FullName} {nameList[i]}{getter};");
            }
            var areaTxt = "";
            if (!match.Success)
            {
                // 直接插入
                Regex pattern = new Regex(@"public class [^{]+\{");
                match = pattern.Match(fileContent);
                if (match.Success)
                {
                    areaTxt = match.Value + "\n\t\t#region PrefabBinder 自动引用区域 开始\n" + sb.ToString() + "\n\t\t#endregion PrefabBinder 自动引用区域 结束";
                    fileContent = fileContent.Replace(match.Value, areaTxt);
                }
            }
            else
            {
                areaTxt = match.Value;
                areaTxt = areaTxt.Replace(match.Groups[1].Value, "\n" + sb.ToString() + "\n\t\t");
                fileContent = fileContent.Replace(match.Value, areaTxt);
            }
            return fileContent;
        }

        private static string UpdateExtract(PrefabBinder prefabBinder, string fileContent)
        {
            // 使用 RegexOptions.Singleline 让 . 匹配所有字符（包括换行符）
            Regex declareAreaRegex = new Regex(
                @"protected\s+override\s+void\s+AutoExtractPrefabBinderComponent\s*\(\s*PrefabBinder\s+prefabBinder\s*\)\s*\{([^{}]*(?:\{(?<depth>)[^{}]*(?:\}(?<-depth>)[^{}]*)*)*(?(depth)(?!)))\}",
                RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
            var match = declareAreaRegex.Match(fileContent);
            var nameList = prefabBinder.NameList;
            // 生成引用字符串
            StringBuilder sb = new StringBuilder();
            var areaTxt = "";
            for (int i = 0; i < nameList.Count; i++)
            {
                string fullName = prefabBinder.GetObj<Object>(nameList[i]).GetType().FullName;
                sb.AppendLine($"\t\t\tthis.{nameList[i]} = prefabBinder.GetObj<{fullName}>(\"{nameList[i]}\");");
            }
            if (match.Success)
            {
                areaTxt = match.Value;
                areaTxt = areaTxt.Replace(match.Groups[1].Value, "\n" + sb.ToString() + "\n\t\t");
                fileContent = fileContent.Replace(match.Value, areaTxt);
            }
            else
            {
                Debug.LogError("请实现重载AutoExtractPrefabBinderComponent");
            }
            return fileContent;
        }

        private static string UpdatePrefabReference(PrefabBinder prefabBinder, string fileContent, string prefabPath)
        {
            Regex declareAreaRegex = new Regex(
                @"public\s+override\s+string\s+GetAssetLink\s*\(\s*string\s+outAssetLink\s*\)\s*\{([^{}]*(?:\{(?<depth>)[^{}]*(?:\}(?<-depth>)[^{}]*)*)*(?(depth)(?!)))\}",
                RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
            var match = declareAreaRegex.Match(fileContent);
            var nameList = prefabBinder.NameList;
            // 生成引用字符串
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t\t\tstring assetPath = \"{prefabPath}\";");
            sb.AppendLine("\t\t\treturn AssetPathEncoder.EncodeEnvAssetLink(assetPath, AssetType.PrefabAsset);");
            if (match.Success)
            {
                var areaTxt = match.Value;
                areaTxt = areaTxt.Replace(match.Groups[1].Value, "\n" + sb.ToString() + "\n\t\t");
                fileContent = fileContent.Replace(match.Value, areaTxt);
            }
            else
            {
                Debug.LogError("请实现重载GetAssetLink");
            }
            return fileContent;
        }
        private static string UpdatePrefabLayerGet(PrefabBinder prefabBinder, PrefabCreateOptions createOption, string fileContent, string prefabPath)
        {
            Regex declareAreaRegex = new Regex(
                @"public\s+override\s+int\s+GetOpenLayer\s*\(\s*int\s+externalLayer\s*\)\s*\{([^{}]*(?:\{(?<depth>)[^{}]*(?:\}(?<-depth>)[^{}]*)*)*(?(depth)(?!)))\}",
                RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
            var match = declareAreaRegex.Match(fileContent);
            var nameList = prefabBinder.NameList;
            // 生成引用字符串
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t\t\treturn {createOption.UICreateOption.GetLayerGetterOverrideTxt()};");
            if (match.Success)
            {
                var areaTxt = match.Value;
                areaTxt = areaTxt.Replace(match.Groups[1].Value, "\n" + sb.ToString() + "\n\t\t");
                fileContent = fileContent.Replace(match.Value, areaTxt);
            }
            else
            {
                Debug.LogError("请实现重载 GetOpenLayer");
            }
            return fileContent;
        }

        public static string GetSelectedPrefabPath(GameObject m_Go)
        {
            var m_PrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (m_PrefabStage != null) return m_PrefabStage.assetPath;

            if (m_Go != null)
            {
                string m_Path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(m_Go);
                if (!string.IsNullOrEmpty(m_Path)) return m_Path;
                return AssetDatabase.GetAssetPath(m_Go);
            }
            return null;
        }
    }
}
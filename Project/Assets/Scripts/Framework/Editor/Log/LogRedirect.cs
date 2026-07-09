using System;
using System.Collections.Generic;
using System.Reflection;
using Framework.Runtime;
using Framework.Runtime.LogSystem;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;

namespace Framework.Editor.Log
{
    public static class LogRedirect
    {
        private static LogEditorConfig[] _logEditorConfig;
        private static string EntryScriptPath = "Assets\\Scripts\\Framework\\Runtime\\System\\Log\\DefaultHelpers\\UnityLogPrintHelper.cs";
        private static string JumpScriptPath = "Assets\\Scripts\\Framework\\Runtime\\System\\Log\\Log.cs";
        static LogRedirect()
        {
            //_logEditorConfig = GetLogEditorConfig();
            //GetLogEditorConfig();
        }

        private class LogEditorConfig
        {
            public string logScriptPath = string.Empty;
            public string logTypeName = string.Empty;
            public int instanceID = 0;

            public LogEditorConfig(string logScriptPath, System.Type logType)
            {
                this.logScriptPath = logScriptPath;
                this.logTypeName = logType.FullName + ":";
            }
        }


        private static LogEditorConfig[] GetLogEditorConfig()
        {
            string[] assetPaths = AssetDatabase.GetAllAssetPaths();
            List<LogEditorConfig> res = new List<LogEditorConfig>();
            foreach (var path in assetPaths)
            {
                if (!path.EndsWith(".cs")) continue;
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script == null) continue;
                Type classType = script.GetClass();
                if (classType == null) continue;
                if (Attribute.IsDefined(classType, typeof(LogStackTraceIgnoreAttribute)))
                {
                    res.Add(new LogEditorConfig(path, classType));
                }
            }


            return res.ToArray();
        }
        private static int EntryFileInstanceId = 26006;
        private static int JumpFileInstanceId = 16650;
        //private static int TarGetInstanceId = 26006;
        
        [OnOpenAssetAttribute(0)]
        private static bool OnOpenAsset(int instanceID, int line)
        {      //只对控制台的开启进行重定向
            if (!EditorWindow.focusedWindow.titleContent.text.Equals("Console"))
                return false;
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(EntryScriptPath);
            if (script != null)
            {
                EntryFileInstanceId = script.GetInstanceID();
            }
            script = AssetDatabase.LoadAssetAtPath<MonoScript>(JumpScriptPath);
            if (script != null)
            {
                JumpFileInstanceId = script.GetInstanceID();
            }

            if (instanceID != EntryFileInstanceId) return false;
            var statckTrack = GetStackTrace();
            if (!string.IsNullOrEmpty(statckTrack))
            {
                var fileNames = statckTrack.Split('\n');
                var fileName = GetCurrentFullFileName(fileNames);
                var fileLine = LogFileNameToFileLine(fileName);
                fileName = GetRealFileName(fileName);
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fileName), fileLine);
                return true;
            }
            return false;
        }

        private static string GetStackTrace()
        {
            var consoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
            var fieldInfo =
                consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            var consoleWindowInstance = fieldInfo.GetValue(null);

            if (null != consoleWindowInstance)
            {
                if ((object)EditorWindow.focusedWindow == consoleWindowInstance)
                {
                    // Get m_ActiveText in ConsoleWindow  
                    fieldInfo = consoleWindowType.GetField("m_ActiveText",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    string activeText = fieldInfo.GetValue(consoleWindowInstance).ToString();

                    return activeText;
                }
            }

            return "";
        }

        private static void UpdateLogInstanceID(LogEditorConfig config)
        {
            if (config.instanceID > 0)
            {
                return;
            }

            var assetLoadTmp = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(config.logScriptPath);
            if (null == assetLoadTmp)
            {
                throw new System.Exception("not find asset by path=" + config.logScriptPath);
            }

            config.instanceID = assetLoadTmp.GetInstanceID();
        }

        //private static string GetCurrentFullFileName(string[] fileNames)
        //{
        //    string retValue = "";
        //    int findIndex = -1;
        //    for (int i = fileNames.Length - 1; i >= 0; --i)
        //    {
        //        if (string.IsNullOrEmpty(fileNames[i])) continue;
        //        bool isCustomLog = false;

        //        for (int j = _logEditorConfig.Length - 1; j >= 0; --j)
        //        {
        //            if (fileNames[i].StartsWith(_logEditorConfig[j].logTypeName))
        //            {
        //                isCustomLog = true;
        //                break;
        //            }
        //        }

        //        if (isCustomLog)
        //        {
        //            findIndex = i;
        //            break;
        //        }
        //    }

        //    if (findIndex >= 0 && findIndex < fileNames.Length - 1)
        //    {
        //        retValue = fileNames[findIndex + 1];
        //    }

        //    return retValue;
        //}
         private static string GetCurrentFullFileName(string[] fileNames)
        {
            string retValue = "";
            int findIndex = -1;
            for (int i = fileNames.Length - 1; i >= 0; --i)
            {
                if (string.IsNullOrEmpty(fileNames[i])) continue;
                bool isCustomLog = false;

                //for (int j = _logEditorConfig.Length - 1; j >= 0; --j)
                //{
                MonoScript monoScript = EditorUtility.InstanceIDToObject(JumpFileInstanceId) as MonoScript;
                var fullName = monoScript.GetClass().FullName;
                if (fileNames[i].StartsWith(fullName))
                {
                    isCustomLog = true;
                    //break;
                }
                //}

                if (isCustomLog)
                {
                    findIndex = i;
                    break;
                }
            }

            if (findIndex >= 0 && findIndex < fileNames.Length - 1)
            {
                retValue = fileNames[findIndex + 1];
            }

            return retValue;
        }

        private static string GetRealFileName(string fileName)
        {
            int indexStart = fileName.IndexOf("(at ") + "(at ".Length;
            int indexEnd = ParseFileLineStartIndex(fileName) - 1;

            fileName = fileName.Substring(indexStart, indexEnd - indexStart);
            return fileName;
        }

        private static int LogFileNameToFileLine(string fileName)
        {
            int findIndex = ParseFileLineStartIndex(fileName);

            string stringParseLine = "";
            for (int i = findIndex; i < fileName.Length; ++i)
            {
                if (i < 0) continue;
                var charCheck = fileName[i];
                if (!IsNumber(charCheck))
                {
                    break;
                }
                else
                {
                    stringParseLine += charCheck;
                }
            }

            return int.Parse(stringParseLine);
        }

        private static int ParseFileLineStartIndex(string fileName)
        {
            int retValue = -1;
            for (int i = fileName.Length - 1; i >= 0; --i)
            {
                var charCheck = fileName[i];
                bool isNumber = IsNumber(charCheck);
                if (isNumber)
                {
                    retValue = i;
                }
                else
                {
                    if (retValue != -1)
                    {
                        break;
                    }
                }
            }

            return retValue;
        }

        private static bool IsNumber(char c)
        {
            return c >= '0' && c <= '9';
        }
    }
}
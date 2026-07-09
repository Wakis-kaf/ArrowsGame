﻿﻿
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor.Utils
{

    public static partial class EditorUtility
    {
        public static class EditorAssetDatabaseUtility
        {
            public static string GetCurrentAssetDirectory()
            {
                Type projectWindowUtilType = typeof(ProjectWindowUtil);
                MethodInfo getActiveFolderPath = projectWindowUtilType.GetMethod("GetActiveFolderPath",
                    BindingFlags.Static | BindingFlags.NonPublic);
                object obj = getActiveFolderPath.Invoke(null, new object[0]);
                string pathToCurrentFolder = obj.ToString();

                return pathToCurrentFolder;
            }


            public static string GetNotRepeatAssetName(string currentPath, string name)
            {
                string rootPath = Application.dataPath.Replace("Assets", "");
                string dirPath = Application.dataPath.Replace("Assets", "");
                Regex regex = new Regex(@"(\d+)$", 
                    RegexOptions.Compiled | 
                    RegexOptions.CultureInvariant);
                string fileName = name;
                dirPath = Path.Combine(rootPath, currentPath,fileName);
              
                
                int recLimit = 0;
                
                while  (File.Exists(dirPath+".asset"))
                {
                    Debug.Log($"Already Exist dir{dirPath}");
                    recLimit++;
                    Match match = regex.Match(fileName);
                    int suffixNum = 1;
                    if (match.Success)
                    {
                        int.TryParse(match.Groups[1].Value,out suffixNum) ;
                        fileName = fileName.Substring(0,  fileName.LastIndexOf(match.Groups[1].Value));
                        suffixNum++;
                    }
                    fileName = fileName + ""+suffixNum.ToString();    
                    dirPath = Path.Combine(rootPath, currentPath, fileName);

                    Debug.Log($"New Dir Path{dirPath}");
                    if (recLimit > 100)
                    {
                        Debug.LogError("Get Not Repeat Name Error! Rec over");
                        break;
                        
                    }
                }
                return fileName;
            }
        }
    }

}


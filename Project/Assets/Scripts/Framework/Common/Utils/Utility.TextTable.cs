using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Framework.Utils
{
    public static partial class Utility
    {
        public static class TextTable
        {
            public static string[][] ReadTxtTableFile(string path, string name, bool includeTitle = false)
            {
                string fileFullName = string.Empty;
                List<string[]> set = new List<string[]>();

                if (!path.EndsWith("/"))
                    fileFullName = string.Concat(path, "/", name);
                else
                {
                    fileFullName = string.Concat(path, name);
                }

                try
                {
                    StreamReader streamReader = new StreamReader(fileFullName);
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        line = line.Trim();  // 4 将每行添加到文件List中
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        if (!includeTitle && line.StartsWith("#")) continue;  // 如果是注释 就跳过
                        set.Add(line.Split(','));
                    }
                    streamReader.Close();
                    return set.ToArray();
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("read file error! message: {0}", e.Message);
                    //throw;
                    return set.ToArray();
                }
            }
        }
    }
}
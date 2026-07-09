
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Framework.Runtime;
using Framework.Utils;
using UnityEditor;
using UnityEngine;

namespace  Framework.Editor.AssetBundleBuildHelp
{
    [InitializeOnLoad]
    public class ResourcesBuild : AssetPostprocessor
    {
        private static string m_FileListContent;

        [MenuItem("Framework/Resources/Build Resources FileList")]
        public static void BuildResourcesFileList()
        {
            string path = Application.dataPath + "/Resources/";
            if (!Directory.Exists(path))
            {
                Debug.LogWarningFormat("path{0} not exists ", path);
                return;
            }

            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

            StringBuilder txt = new StringBuilder();
            foreach (var file in files)
            {
                if (file.EndsWith(".meta")) continue;

                string name = file.Replace(path, "");
                name = name.Substring(0, name.LastIndexOf("."));
                name = name.Replace("\\", "/");
                txt.AppendLine(name);
                //txt+= name.ToLower() + "\n";
            }

            path = path + "FileList.bytes";
            if (File.Exists(path)) File.Delete(path);
            File.WriteAllText(path, txt.ToLower().ToString());
            if (!Application.isPlaying)
                AssetDatabase.Refresh();
        }

        private static void WriteToContent(string resPath)
        {
            string[] files = Directory.GetFiles(resPath, "*.*", SearchOption.AllDirectories);
            string txt = "";
            foreach (var file in files)
            {
                if (file.EndsWith(".meta")) continue;
                string name = file.Replace(resPath, "");
                int dotIndex = name.LastIndexOf(".");
                if (dotIndex == -1)
                    name = name.Substring(0);
                else
                {
                    name = name.Substring(0, dotIndex);
                }

                name = name.Replace("\\", "/");
                if (name.StartsWith("/"))
                    name = name.Substring(1);
                txt += name.ToLower() + "\n";
            }

            if (!string.IsNullOrEmpty(txt))
            {
                m_FileListContent += txt;
            }
        }

        [MenuItem("Framework/Resources/Build Resources FileList Deep")]
        public static void BuildResourcesFileListDeep()
        {
            string rootPath = Application.dataPath;
            m_FileListContent = string.Empty;
            // 获取该目录下的所有文件夹
            RecursionFindResourcesDir(rootPath);
            string savePath = rootPath + "/Resources/FileList.bytes";
            if (File.Exists(savePath)) File.Delete(savePath);
            File.WriteAllText(savePath, m_FileListContent);
            m_FileListContent = string.Empty;
            if (!Application.isPlaying)
                AssetDatabase.Refresh();
        }

        private static void RecursionFindResourcesDir(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                DirectoryInfo direction = new DirectoryInfo(dirPath);
                DirectoryInfo[] directories = direction.GetDirectories(); //查找本文件夹和所有子文件夹
                for (int i = 0; i < directories.Length; i++)
                {
                    var dir = directories[i];
                    string dirName = dir.Name.ToLower();

                    if (dirName == "Editor".ToLower()) continue;
                    if (dirName == "Resources".ToLower())
                    {
                        WriteToContent(dir.FullName);
                    }

                    RecursionFindResourcesDir(dir.FullName);
                }
            }
        }
        //[MenuItem("Framework/Resources/Transfer OriginalResources To Resources")]
        //public static void DeepCopyFolderToResources()
        //{
        //    CopyFolderKeepAssetsUsingEditor.CopyFolderKeepAssetsUsing(GameConfig.NameField_ResBuildDir,"Resources/res");
        //    BuildResourcesFileListDeep();
        //}



    }
}
public class CopyFolderKeepAssetsUsingEditor
{
 
    //public static void CopyFolderKeepAssetsUsing(string fromPath,string targetPath)
    //{
    //    string copyedFolderPath = Utility.Path.PathCombine(Application.dataPath, fromPath);
    //    string tempFolderPath = Utility.Path.PathCombine(GameConfigStatic.GetPlatformPathStatic(), GameConfig.FieldName_BuildTemp, fromPath);
    //    string newFoldrPath = Utility.Path.PathCombine(Application.dataPath, targetPath);
    //    CopyDirectory(copyedFolderPath, tempFolderPath);
    //    //重新生成guids
    //    RegenerateGuids(copyedFolderPath);
    //    CopyDirectory(copyedFolderPath, newFoldrPath);
    //    AssetDatabase.DeleteAsset(copyedFolderPath);
    //    CopyDirectory(tempFolderPath, copyedFolderPath);
    //    AssetDatabase.Refresh();
    //    AssetDatabase.SaveAssets();
    //}
    private static string SplitStr(string path)
    {
        string str = "";
        for (int i = 0; i < path.Split('/').Length; i++)
        {
            if (i != 0)
            {
                string _str = "/" + path.Split('/')[i];
                str += _str;
            }
        }
        return str;
    }
    #region Copy
    public static void CopyDirectory(string sourceDirectory, string destDirectory)
    {
        //判断源目录和目标目录是否存在，如果不存在，则创建一个目录
        if (!Directory.Exists(sourceDirectory))
        {
            Directory.CreateDirectory(sourceDirectory);
        }
        if (!Directory.Exists(destDirectory))
        {
            Directory.CreateDirectory(destDirectory);
        }
        //拷贝文件
        CopyFile(sourceDirectory, destDirectory);
        //拷贝子目录       
        //获取所有子目录名称
        string[] directionName = Directory.GetDirectories(sourceDirectory);
        foreach (string directionPath in directionName)
        {
            //根据每个子目录名称生成对应的目标子目录名称
            string directionPathTemp = Path.Combine(destDirectory, directionPath.Substring(sourceDirectory.Length + 1));// destDirectory + "\\" + directionPath.Substring(sourceDirectory.Length + 1);
                                                                                                                        //递归下去
            CopyDirectory(directionPath, directionPathTemp);
        }
    }
    public static void CopyFile(string sourceDirectory, string destDirectory)
    {
        //获取所有文件名称
        string[] fileName = Directory.GetFiles(sourceDirectory);
        foreach (string filePath in fileName)
        {
            //根据每个文件名称生成对应的目标文件名称
            string filePathTemp = Path.Combine(destDirectory, filePath.Substring(sourceDirectory.Length + 1));// destDirectory + "\\" + filePath.Substring(sourceDirectory.Length + 1);
                                                                                                              //若不存在，直接复制文件；若存在，覆盖复制
            if (File.Exists(filePathTemp))
            {
                File.Copy(filePath, filePathTemp, true);
            }
            else
            {
                File.Copy(filePath, filePathTemp);
            }
        }
    }
    #endregion

    #region GUID
    private static readonly string[] kDefaultFileExtensions = {  
            // "*.meta",  
            // "*.mat",  
            // "*.anim",  
            // "*.prefab",  
            // "*.unity",  
            // "*.asset"  
            "*.*"
        };
    static public void RegenerateGuids(string _assetsPath, string[] regeneratedExtensions = null)
    {
        if (regeneratedExtensions == null)
        {
            regeneratedExtensions = kDefaultFileExtensions;
        }

        // Get list of working files  
        List<string> filesPaths = new List<string>();
        foreach (string extension in regeneratedExtensions)
        {
            filesPaths.AddRange(
                Directory.GetFiles(_assetsPath, extension, SearchOption.AllDirectories)
                );
        }

        // Create dictionary to hold old-to-new GUID map  
        Dictionary<string, string> guidOldToNewMap = new Dictionary<string, string>();
        Dictionary<string, List<string>> guidsInFileMap = new Dictionary<string, List<string>>();

        // We must only replace GUIDs for Resources present in Assets.   
        // Otherwise built-in resources (shader, meshes etc) get overwritten.  
        HashSet<string> ownGuids = new HashSet<string>();

        // Traverse all files, remember which GUIDs are in which files and generate new GUIDs  
        int counter = 0;
        foreach (string filePath in filesPaths)
        {
            EditorUtility.DisplayProgressBar("Scanning Assets folder", MakeRelativePath(_assetsPath, filePath), counter / (float)filesPaths.Count);
            string contents = string.Empty;
            try
            {
                contents = File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                Debug.LogError(filePath);
                Debug.LogError(e.ToString());
                counter++;
                continue;
            }
            IEnumerable<string> guids = GetGuids(contents);
            bool isFirstGuid = true;
            foreach (string oldGuid in guids)
            {
                // First GUID in .meta file is always the GUID of the asset itself  
                if (isFirstGuid && Path.GetExtension(filePath) == ".meta")
                {
                    ownGuids.Add(oldGuid);
                    isFirstGuid = false;
                }
                // Generate and save new GUID if we haven't added it before  
                if (!guidOldToNewMap.ContainsKey(oldGuid))
                {
                    string newGuid = Guid.NewGuid().ToString("N");
                    guidOldToNewMap.Add(oldGuid, newGuid);
                }

                if (!guidsInFileMap.ContainsKey(filePath))
                    guidsInFileMap[filePath] = new List<string>();

                if (!guidsInFileMap[filePath].Contains(oldGuid))
                {
                    guidsInFileMap[filePath].Add(oldGuid);
                }
            }

            counter++;
        }

        // Traverse the files again and replace the old GUIDs  
        counter = -1;
        int guidsInFileMapKeysCount = guidsInFileMap.Keys.Count;
        foreach (string filePath in guidsInFileMap.Keys)
        {
            EditorUtility.DisplayProgressBar("Regenerating GUIDs", MakeRelativePath(_assetsPath, filePath), counter / (float)guidsInFileMapKeysCount);
            counter++;

            string contents = File.ReadAllText(filePath);
            foreach (string oldGuid in guidsInFileMap[filePath])
            {
                if (!ownGuids.Contains(oldGuid))
                    continue;

                string newGuid = guidOldToNewMap[oldGuid];
                if (string.IsNullOrEmpty(newGuid))
                    throw new NullReferenceException("newGuid == null");

                contents = contents.Replace("guid: " + oldGuid, "guid: " + newGuid);
            }
            //File.WriteAllText(filePath, contents);
            Utility.FileUtil.SaveFile(filePath, System.Text.Encoding.UTF8.GetBytes(contents));
        }

        EditorUtility.ClearProgressBar();
    }
    private static IEnumerable<string> GetGuids(string text)
    {
        const string guidStart = "guid: ";
        const int guidLength = 32;
        int textLength = text.Length;
        int guidStartLength = guidStart.Length;
        List<string> guids = new List<string>();

        int index = 0;
        while (index + guidStartLength + guidLength < textLength)
        {
            index = text.IndexOf(guidStart, index, StringComparison.Ordinal);
            if (index == -1)
                break;

            index += guidStartLength;
            string guid = text.Substring(index, guidLength);
            index += guidLength;

            if (IsGuid(guid))
            {
                guids.Add(guid);
            }
        }
        return guids;
    }
    private static bool IsGuid(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (
                !((c >= '0' && c <= '9') ||
                  (c >= 'a' && c <= 'z'))
                )
                return false;
        }

        return true;
    }
    private static string MakeRelativePath(string fromPath, string toPath)
    {
        Uri fromUri = new Uri(fromPath);
        Uri toUri = new Uri(toPath);

        Uri relativeUri = fromUri.MakeRelativeUri(toUri);
        string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

        return relativePath;
    }
    #endregion
}


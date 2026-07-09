using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Framework.Utils
{
    public static partial class Utility
    {
        public static class FileUtil
        {
            public static void CopyDirFile(string sourceDirectory, string targetDirectory,
                Action<FileInfo, float> fileCopyCallBack = null, Func<FileInfo, bool> copyPreProcess = null,
                Action<float> copyProgressCb = null)
            {
                sourceDirectory = sourceDirectory.Replace("/", "\\");
                targetDirectory = targetDirectory.Replace("/", "\\");
                List<FileInfo> fileInfos = new List<FileInfo>(2048);
                GetFileInfoListDeep(sourceDirectory, fileInfos, copyPreProcess);
                var count = fileInfos.Count;
                var sourceDirStrLength = sourceDirectory.Length;
                for (int i = 0; i < count; i++)
                {
                    var fileInfo = fileInfos[i];
                    int index = fileInfo.FullName.IndexOf(sourceDirectory);
                    string targetPath = targetDirectory + fileInfo.FullName.Substring(index + sourceDirStrLength);
                    int dotPoint = targetPath.LastIndexOf("\\");
                    string targetDirPath = dotPoint == -1 ? targetPath : targetPath.Substring(0, dotPoint);
                    if (!Directory.Exists(targetDirPath))
                    {
                        //目标目录下不存在此文件夹即创建子文件夹
                        Directory.CreateDirectory(targetDirPath);
                    }

                    CopyFile(fileInfo.FullName, targetPath);
                    fileCopyCallBack?.Invoke(fileInfo, (float)i / count);
                    copyProgressCb?.Invoke((float)i / count);
                }

                copyProgressCb?.Invoke(1);
            }

            public static void CopyFile(string sourceFullName, string destFileName)
            {
                System.IO.File.Copy(sourceFullName, destFileName, true);
            }

            public static bool CreateDirectory(string dir)
            {
                try
                {
                    Directory.CreateDirectory(dir);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public static string DecodeDirPathFromFullPath(string fullPath)
            {
                fullPath = fullPath.Replace("\\", "/");
                string dirPath = fullPath;
                //if (fullPath.IndexOf(".", StringComparison.Ordinal) != -1)
                //{
                //int index1 = fullPath.LastIndexOf("\\");
                int index = fullPath.LastIndexOf("/");
                //int index = index1 != -1 ? index1 : index2;
                if (index != -1)
                {
                    dirPath = fullPath.Substring(0, index);
                }
                //}

                return dirPath;
            }

            public static void DeleteChildDir(string srcPath)
            {
                if (!Directory.Exists(srcPath))
                {
                    return;
                }

                try
                {
                    DirectoryInfo dir = new DirectoryInfo(srcPath);
                    FileSystemInfo[] fileinfo = dir.GetFileSystemInfos(); //返回目录中所有文件和子目录
                    foreach (FileSystemInfo i in fileinfo)
                    {
                        if (i is DirectoryInfo) //判断是否文件夹
                        {
                            DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                            subdir.Delete(true); //删除子目录和文件
                        }
                        else
                        {
                            System.IO.File.Delete(i.FullName); //删除指定文件
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    throw;
                }
            }

            public static bool DeleteDir(string dir, bool recursive = true)
            {
                if (!Directory.Exists(dir))
                {
                    return false;
                }

                try
                {
                    Directory.Delete(dir, recursive);
                    return true;
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    return false;
                }
            }

            public static void DeleteEmptyDir(string dir)
            {
                foreach (string directory in Directory.GetDirectories(dir))
                {
                    DeleteEmptyDir(directory);
                    if (Directory.GetFiles(directory).Length == 0 && Directory.GetDirectories(directory).Length == 0)
                    {
                        Directory.Delete(directory);
                    }
                }
            }

            public static void DeleteFile(string filePath)
            {
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            public static bool IsDirectoryExist(string dir)
            {
                return Directory.Exists(dir);
            }

            public static bool IsFileExist(string filePath)
            {
                return File.Exists(filePath);
            }

            public static byte[] ReadAllBytes(string getAssetBundleDirPath)
            {
                return File.ReadAllBytes(getAssetBundleDirPath);
            }

            public static string ReadFile(string fileFullName)
            {
                try
                {
                    if (!File.Exists(fileFullName)) return string.Empty;
                    StreamReader streamReader = new StreamReader(fileFullName);
                    string jsonStr = streamReader.ReadToEnd();
                    streamReader.Close();
                    return jsonStr;
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("read file error! message: {0}", e.Message);
                    //throw;
                    return String.Empty;
                }
            }

            public static string ReadFile(string path, string name)
            {
                string fileFullName = string.Empty;
                if (!path.EndsWith("/"))
                    fileFullName = string.Concat(path, "/", name);
                else
                {
                    fileFullName = string.Concat(path, name);
                }

                return ReadFile(fileFullName);
            }

            public static void SaveFile(string fullPath, byte[] content, bool isOverride = true)
            {
                FileDirCheck(fullPath);
                File.WriteAllBytes(fullPath, content);
            }

            public static void SaveFile(string fullPath, string content, bool isOverride = true)
            {
                FileDirCheck(fullPath);
                StreamWriter sw;
                FileInfo fileInfo = new FileInfo(fullPath);
                if (!fileInfo.Exists)
                {
                    // 创建文件
                    sw = fileInfo.CreateText();
                }
                else
                {
                    if (isOverride)
                    {
                        // 删除文件
                        fileInfo.Delete();
                        // 创建文件
                        sw = fileInfo.CreateText();
                    }
                    else
                    {
                        sw = fileInfo.AppendText();
                    }
                }

                //以行的形式写入信息
                sw.WriteLine(content);
                //关闭流
                sw.Close();
                //销毁流
                sw.Dispose();
            }

            public static void SaveFile(string path, string name, string content, bool isOverride = true)
            {
                StreamWriter sw;
                // 判断路径是否存在
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }

                CheckDir(path);
                SaveFile(Path.PathCombine(path, name), content, isOverride);
            }

            public static string SearchFile(string searchDirPath, string name)
            {
                // 搜索文件夹路径下与指定文件名不带扩展名完全匹配的文件
                if (!Directory.Exists(searchDirPath)) return "";
                string[] filePaths = Directory.GetFiles(searchDirPath, "*", SearchOption.AllDirectories);

                // 遍历所有匹配的文件路径，比较文件名不带扩展名是否与指定的文件名匹配
                foreach (string path in filePaths)
                {
                    string nameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(path);
                    if (nameWithoutExtension == name)
                    {
                        return path;
                    }
                }
                return "";
            }

            public static bool TryGetFile(string filePath, out byte[] datas)
            {
                try
                {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using (BinaryReader binaryReader = new BinaryReader(fileStream))
                    {
                        datas = binaryReader.ReadBytes((int)fileStream.Length);
                    }
                    return true;
                }
                catch (Exception)
                {
                    datas = default;
                    return false;
                }
            }

            public static async UniTask<byte[]> TryGetFileAsync(string filePath)
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                {
                    byte[] buffer = new byte[stream.Length];
                    await stream.ReadAsync(buffer, 0, (int)stream.Length);
                    return buffer;
                }
            }

            public static bool TrySaveFile(string filePath, byte[] datas)
            {
                try
                {
                    FileDirCheck(filePath);
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                    {
                        binaryWriter.Write(datas);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }

            public static async UniTask TrySaveStorage(string filePath, byte[] data)
            {
                // 确保目录存在
                FileDirCheck(filePath);
                using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                {
                    await stream.WriteAsync(data, 0, data.Length);
                    await stream.FlushAsync(); // 确保数据写入磁盘
                }
            }

            private static void CheckDir(string path)
            {
                if (Directory.Exists(path)) return;
                Directory.CreateDirectory(path);
            }

            private static void CopyDirFile(string sourceDirectory, string targetDirectory,
                Action<FileInfo> fileCopyCallBack, Func<FileInfo, bool> copyPreProcess, Action<float> copyProgressCb,
                float level, ref float progress)
            {
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(sourceDirectory);
                    //获取目录下（不包含子目录）的文件和子目录
                    FileSystemInfo[] fileInfoArray = dir.GetFileSystemInfos();
                    var length = fileInfoArray.Length;
                    for (int i = 0; i < length; i++)
                    {
                        var fileInfo = fileInfoArray[i];
                        if (fileInfo is DirectoryInfo) //判断是否文件夹
                        {
                            if (!Directory.Exists(targetDirectory + "\\" + fileInfo.Name))
                            {
                                //目标目录下不存在此文件夹即创建子文件夹
                                Directory.CreateDirectory(targetDirectory + "\\" + fileInfo.Name);
                            }

                            float before = progress;
                            //递归调用复制子文件夹
                            CopyDirFile(fileInfo.FullName, targetDirectory + "\\" + fileInfo.Name, fileCopyCallBack,
                                copyPreProcess, copyProgressCb, level / length, ref progress);
                            if (Math.Abs(progress - before) < 1e-5)
                            {
                                progress += level / length;
                            }
                        }
                        else
                        {
                            if (copyPreProcess != null && !copyPreProcess(fileInfo as FileInfo)) continue;
                            System.IO.File.Copy(fileInfo.FullName, targetDirectory + "\\" + fileInfo.Name, true);
                            progress += level / length;
                            copyProgressCb?.Invoke(progress);
                            fileCopyCallBack?.Invoke(fileInfo as FileInfo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("复制文件出现异常" + ex.Message);
                }
            }

            private static void FileDirCheck(string fullPath)
            {
                CheckDir(DecodeDirPathFromFullPath(fullPath));
            }

            private static void GetFileInfoListDeep(string sourceDirectory, List<FileInfo> list,
                Func<FileInfo, bool> isNotIgnoreFileInfo = null)
            {
                DirectoryInfo dir = new DirectoryInfo(sourceDirectory);
                //获取目录下（不包含子目录）的文件和子目录
                FileSystemInfo[] fileInfoArray = dir.GetFileSystemInfos();
                var length = fileInfoArray.Length;
                for (int i = 0; i < length; i++)
                {
                    var fileInfo = fileInfoArray[i];

                    if (fileInfo is DirectoryInfo) //判断是否文件夹
                    {
                        GetFileInfoListDeep(fileInfo.FullName, list, isNotIgnoreFileInfo);
                    }
                    else
                    {
                        if (isNotIgnoreFileInfo != null && !isNotIgnoreFileInfo(fileInfo as FileInfo)) continue;
                        list.Add(fileInfo as FileInfo);
                    }
                }
            }
        }
    }
}
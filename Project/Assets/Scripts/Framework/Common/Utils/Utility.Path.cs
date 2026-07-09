using System.IO;
using UnityEngine;

namespace Framework.Utils
{
    public static partial class Utility
    {
        public static class Path
        {
            // 资源路径
            public const string RESOURCES_OUTPUT_PATH = "res";

            public const string RESOURCES_BUILD_PATH = "OriginalResources";
            public const string pcName = "pc";
            public const string iosName = "ios";
            public const string androidName = "android";
            public const string webglName = "webgl";

            public static string GetDataPath()
            {
                return Application.dataPath;
            }

            public static string GetStreamingAssetsPath()
            {
                return Application.streamingAssetsPath;
            }

            public static string GetPersistentDataPath()
            { return Application.persistentDataPath; }

            public static string GetTemporaryCachePath()
            { return Application.temporaryCachePath; }

            public static string PathCombine(string path, string path2)
            {
                return System.IO.Path.Combine(path, path2);
            }

            public static string PathCombine(string path, string path2, string path3)
            {
                return System.IO.Path.Combine(path, path2, path3);
            }

            public static string PathCombine(string path, string path2, string path3, string path4)
            {
                return System.IO.Path.Combine(path, path2, path3, path4);
            }

            public static string PathCombine(params string[] paths)
            {
                return System.IO.Path.Combine(paths);
            }

            public static string PathCombineNormal(params string[] paths)
            {
                return System.IO.Path.Combine(paths).Replace("\\", "/");
            }

            /// <summary>
            /// 得到 AB 资源的输入目录
            /// </summary>
            /// <returns></returns>
            public static string GetABBuildPath()
            {
                return GetDataPath() + "/" + RESOURCES_BUILD_PATH;
            }

            /// <summary>
            /// 获得 AB 包输出路径 1\ 平台(PC/移动端等)路径 2\ 平台名称
            /// </summary>
            /// <returns></returns>
            public static string GetABOutPath()
            {
                return GetStreamingAssetsPath() + "/" + GetPlatformName();
            }

            public static string GetABOutManifestPath()
            {
                return GetABOutPath() + "/" + GetPlatformName();
            }

            /// <summary>
            /// 获取平台路径
            /// </summary>
            /// <returns></returns>
            //public static string GetPlatformStreamingPath()
            //{
            //    string path =
            //    #if UNITY_ANDROID && !UNITY_EDITOR
            //            Application.streamingAssetsPath;
            //    #elif UNITY_IPHONE && !UNITY_EDITOR
            //            "file://" + Application.streamingAssetsPath;
            //    #elif UNITY_STANDLONE_WIN || UNITY_EDITOR
            //            "file://" + Application.streamingAssetsPath;
            //    #else
            //        Application.streamingAssetsPath
            //    #endif
            //        return path;
            //}

            /// <summary>
            /// 获得平台名称
            /// </summary>
            /// <returns></returns>
            public static string GetPlatformName()
            {
#if UNITY_STANDALONE_WIN

                return  pcName;

#elif UNITY_IPHONE
            return iosName;
#elif UNITY_ANDROID
                return androidName;
#elif UNITY_WEBGL
                return webglName;
#endif
            }

            /// <summary>
            /// 返回 WWW 下载 AB 包加载路径
            /// </summary>
            /// <returns></returns>
            public static string GetWWWAssetBundlePath()
            {
                string strReturnWWWPath = string.Empty;

#if UNITY_STANDALONE_WIN
                strReturnWWWPath = "file://" + GetABOutPath();

#elif UNITY_IPHONE
            strReturnWWWPath = GetABOutPath() + "/Raw/";
#elif UNITY_ANDROID
                strReturnWWWPath = "jar:file://" + GetABOutPath();
#endif

                return strReturnWWWPath;
            }

            /// <summary>
            /// 获取规范的路径。
            /// </summary>
            /// <param name="path">要规范的路径。</param>
            /// <returns>规范的路径。</returns>
            public static string GetRegularPath(string path)
            {
                return path?.Replace('\\', '/');
            }

            /// <summary>
            /// 获取远程格式的路径（带有file:// 或 http:// 前缀）。
            /// </summary>
            /// <param name="path">原始路径。</param>
            /// <returns>远程格式路径。</returns>
            public static string GetRemotePath(string path)
            {
                string regularPath = Utility.Path.GetRegularPath(path);
                if (regularPath == null)
                    return (string)null;
                return !regularPath.Contains("://")
                    ? ("file:///" + regularPath).Replace("file:////", "file:///")
                    : regularPath;
            }

            /// <summary>
            /// 移除空文件夹。
            /// </summary>
            /// <param name="directoryName">要处理的文件夹名称。</param>
            /// <returns>是否移除空文件夹成功。</returns>
            public static bool RemoveEmptyDirectory(string directoryName)
            {
                if (string.IsNullOrEmpty(directoryName))
                    throw new System.Exception();
                try
                {
                    if (!Directory.Exists(directoryName))
                        return false;
                    string[] directories = Directory.GetDirectories(directoryName, "*");
                    int length = directories.Length;
                    foreach (string directoryName1 in directories)
                    {
                        if (Utility.Path.RemoveEmptyDirectory(directoryName1))
                            --length;
                    }

                    if (length > 0 || Directory.GetFiles(directoryName, "*").Length != 0)
                        return false;
                    Directory.Delete(directoryName);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}

/*
unity 中常见的资源路径如下：
1. Application.dataPath 此属性用于返回项目文件所在文件夹的路径。例如在editor 中就是Asset
2. Application.streamingAssetsPath 此属性用于返回流数据的缓存目录，返回路径为相对路径，适合设置一些外部数据文件的路径
3. Application.persistenDataPath 此属性用于返回一个持久化数据存储目录的路径，可以在此路径下存放一些持久化的数据文件
4. Application.temporayCachePath 此属性用于返回一个临时数据的缓存目录
相关链接: https://zhuanlan.zhihu.com/p/125109062#:~:text=%E4%BA%8C%E3%80%81Unity3D%E4%B8%AD%E7%9A%84%E8%B5%84%E6%BA%90%E8%AE%BF%E9%97%AE%E4%BB%8B%E7%BB%8D%201%E3%80%81Resources%20%E6%98%AFUnity3D%E7%B3%BB%E7%BB%9F%E6%8C%87%E5%AE%9A%E6%96%87%E4%BB%B6%E5%A4%B9%EF%BC%8C%E5%A6%82%E6%9E%9C%E4%BD%A0%E6%96%B0%E5%BB%BA%E7%9A%84%E6%96%87%E4%BB%B6%E5%A4%B9%E7%9A%84%E5%90%8D%E5%AD%97%E5%8F%ABResources%EF%BC%8C%E9%82%A3%E4%B9%88%E9%87%8C%E9%9D%A2%E7%9A%84%E5%86%85%E5%AE%B9%E5%9C%A8%E6%89%93%E5%8C%85%E6%97%B6%E9%83%BD%E4%BC%9A%E8%A2%AB%E6%89%93%E5%88%B0%E5%8F%91%E5%B8%83%E5%8C%85%E4%B8%AD%E3%80%82,%E6%96%87%E4%BB%B6%E5%A4%B9%E7%89%B9%E7%82%B9%EF%BC%9A%20%E5%8F%AA%E8%AF%BB%EF%BC%8C%E5%8D%B3%E4%B8%8D%E8%83%BD%E5%8A%A8%E6%80%81%E4%BF%AE%E6%94%B9%E3%80%82%20%E6%89%80%E4%BB%A5%E6%83%B3%E8%A6%81%E5%8A%A8%E6%80%81%E6%9B%B4%E6%96%B0%E7%9A%84%E8%B5%84%E6%BA%90%E4%B8%8D%E8%A6%81%E6%94%BE%E5%9C%A8%E8%BF%99%E9%87%8C%E3%80%82%20%E4%BC%9A%E5%B0%86%E6%96%87%E4%BB%B6%E5%A4%B9%E5%86%85%E7%9A%84%E8%B5%84%E6%BA%90%E6%89%93%E5%8C%85%E9%9B%86%E6%88%90%E5%88%B0.asset%E6%96%87%E4%BB%B6%E9%87%8C%E9%9D%A2%E3%80%82
*/
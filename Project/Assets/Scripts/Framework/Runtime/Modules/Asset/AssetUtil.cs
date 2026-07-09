using System;
using System.IO;
using System.Text.RegularExpressions;
using Framework.Misc;

namespace Framework.Runtime.MAsset
{
    public static class AssetUtil
    {
        #region 资源加载便捷处理

        public const string prefabDir = "Prefabs";
        public const string uiDir = "UI";

        #endregion 资源加载便捷处理


        public static string GetAbFullName(FileInfo fileInfoObj, string scenesName)
        {
            string fileName = scenesName + "/" + fileInfoObj.Name;
            return GetSpaceLineName(fileName);
        }

        public static string GetAbName(FileInfo fileInfoObj, string scenesName)
        {
            string fileName = scenesName + "/" + fileInfoObj.Name.Split('.')[0];
            return GetSpaceLineName(fileName);
        }

        /// <summary>
        /// 小写,以 / 为路径分割符号
        /// 会替换 "." 为 /
        /// 会替换 "\\" 为 /
        /// 会去掉.ab
        /// </summary>
        public static string GetAssetBundleHashName(string assetName)
        {
            //  内存优化
            var assetNameSB = UStringBuilderPool.GetSharedStringBuilder(assetName);
            return assetNameSB.ToLower().Replace(".ab", "").Replace("\\", "/").Replace(".", "/").ToStringAndRelease();
        }

        /// <summary>
        /// 小写,以 / 为路径分割符号
        /// 会替换 "\\" 为 /
        /// 会替换 "." 为 /
        /// </summary>
        public static string GetAssetHashPath(string assetPath)
        {
            //  内存优化
            var strSb = UStringBuilderPool.GetSharedStringBuilder(assetPath);
            return strSb.ToLower().Replace("\\", "/").Replace(".", "/").ToStringAndRelease();
        }

        /// <summary>
        /// 小写,以 / 为路径分割符号
        /// </summary>
        public static string GetAssetPath(string assetPath)
        {
            //  内存优化
            var strSb = UStringBuilderPool.GetSharedStringBuilder(assetPath);
            return strSb.ToLower().Replace("\\", "/").ToStringAndRelease();

            // assetPath = assetPath.ToLower();
            // assetPath = assetPath.Replace("\\", "/");
            // return assetPath;
        }

        public static string GetBundleRealFullName(string assetName)
        {
            assetName = assetName.ToLower();
            int lineIndex = assetName.LastIndexOf("_");
            if (lineIndex == -1) return assetName;
            int dotIndex = assetName.IndexOf(".");
            string content = assetName.Substring(lineIndex, dotIndex - lineIndex);
            assetName = assetName.Replace(content, string.Empty);
            return assetName;
        }

        public static string GetHashPath(string path)
        {
            return path.ToLower();
        }

        /// <summary>
        /// 小写,以 / 为路径分割符号
        /// 会替换 "\\" 为 /
        /// 会删除 ".prefab 结尾
        /// </summary>
        public static string GetPrefabHashPath(string assetPath)
        {
            assetPath = assetPath.ToLower();
            if (assetPath.EndsWith(".prefab"))
            {
                assetPath = assetPath.Replace(".prefab", "");
            }

            assetPath = assetPath.Replace("\\", "/");
            return assetPath;
        }

        /// <summary>
        /// 小写,以 / 为路径分割符号
        /// 会替换 "." 为 /
        /// 会补充 .prefab 为结尾
        /// </summary>
        public static string GetPrefabPath(string hashPath)
        {
            if (hashPath.EndsWith(".prefab"))
            {
                hashPath = hashPath.Replace(".prefab", "");
            }

            hashPath = hashPath.Replace(".", "/");
            return hashPath + ".prefab";
        }

        public static string GetSpaceLineName(string fileName)
        {
            Regex replaceSpace = new Regex(@"\s{1,}", RegexOptions.IgnoreCase);
            return replaceSpace.Replace(fileName, "_").Trim();
        }

        /// <summary>
        /// 小写,以 \\ 为路径分割符号
        /// </summary>
        public static string GetWindowsAssetHashPath(string assetPath, bool convertWindowPath = true)
        {
            assetPath = assetPath.ToLower();
            assetPath = assetPath.Replace("/", "\\");
            return assetPath;
        }

    }
}
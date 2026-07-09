using Framework.Runtime.LogSystem;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.U2D;

namespace Framework.Runtime.MAsset
{
    public static partial class AssetPathEncoder
    {

        public const string AssetType_MainManifest = "manifest";

        public const string AssetType_Prefab = "prefab";

        public const string AssetType_Unity = "unity";
        public const string AssetType_Any = "any";
        public const string AssetType_AAGroupAssets = "aagroupassets";
        public const string AssetType_AA = "aa";

        public const string FileExtension_ABManifest = "ab.manifest";

        public const string FileExtension_Animation = "anim";

        public const string FileExtension_AnimationController = "controller";

        public const string FileExtension_Empty = "";

        public const string FileExtension_Asset = "asset";

        public const string FileExtension_Bytes = "bytes";
        public const string FileExtension_DllBytes = "dll.bytes";

        public const string FileExtension_CSharScript = "cs";

        public const string FileExtension_Fbx = "fbx";

        public const string FileExtension_Jpg = "jpg";

        public const string FileExtension_Json = "json";

        public const string FileExtension_Lua = "lua";

        public const string FileExtension_Manifest = "manifest";

        public const string FileExtension_Mp3 = "mp3";

        public const string FileExtension_Obj = "obj";

        public const string FileExtension_Png = "png";

        public const string FileExtension_Prefab = "prefab";

        public const string FileExtension_Scene = "unity";
        public const string FileExtension_AudioMixer = "mixer";

        public const string FileExtension_SpriteAtlas = "spriteatlas";

        public const string FileExtension_Text = "txt";

        public const string FileExtension_Tga = "tga";

        public const string FileExtension_Wav = "wav";

        public const string FileExtension_Xml = "xml";

        public static List<string> HotAssetBundleConnectedResNames = new List<string>()
        {
           "version_asset",
        };

        public static List<string> HotIgnoreAsset = new List<string>
        {
            //"version_asset",
            //"gameconfig",
        };

        public static List<string> HotNotAssetBundleResNames = new List<string>()
        {
            "gameconfig",
        };

        public static Dictionary<string, AssetLoadType> AssetLoadTypeStrName2EnumTypeMap = new Dictionary<string, AssetLoadType>()
        {
            { "editor",AssetLoadType.EditorLoader},
            { "resources",AssetLoadType.ResourcesLoader},
            { "platform",AssetLoadType.PlatformLoader},
            { "web",AssetLoadType.WebLoader},
        };

        public static Dictionary<string, AssetReadType> AssetReadTypeStrName2EnumTypeMap = new Dictionary<string, AssetReadType>()
        {
            { "env",AssetReadType.Env},
            { "editor",AssetReadType.Editor},
            { "hot",AssetReadType.Hot},
            { "resources",AssetReadType.Resources},
        };

        public static Dictionary<string, string> AssetType2FileExtensionMap = new Dictionary<string, string>()
        {
            { "aa",FileExtension_Empty},
            { "aagroupassets",FileExtension_Empty},
            { "unity",FileExtension_Scene},
            { "mixer",FileExtension_AudioMixer},
            { "prefab",FileExtension_Prefab},
            { "cs",FileExtension_CSharScript},
            { "png|tex",FileExtension_Png},
            { "jpg|tex",FileExtension_Jpg},
            { "tga|tex",FileExtension_Tga},
            { "png|sprite",FileExtension_Png},
            { "jpg|sprite",FileExtension_Jpg},
            { "tga|sprite",FileExtension_Tga},
            { "spriteatlas",FileExtension_SpriteAtlas},
            { "anim",FileExtension_Animation},
            { "controller",FileExtension_AnimationController},
            { "bytes",FileExtension_Bytes},
            { "json",FileExtension_Json},
            { "txt",FileExtension_Text},
            { "xml",FileExtension_Xml},
            { "fbx",FileExtension_Fbx},
            { "obj",FileExtension_Obj},
            { "wav",FileExtension_Wav},
            { "mp3",FileExtension_Mp3},
            { "asset",FileExtension_Asset},
            { "any",FileExtension_Empty},
            { "dllbytes",FileExtension_DllBytes},


        };

        public static Dictionary<string, AssetType> AssetTypeStrName2EnumTypeMap = new Dictionary<string, AssetType>()
        {
            { AssetType_AA,AssetType.AddressableAsset},
            { AssetType_AAGroupAssets,AssetType.AddressableGroupAsset },
            { "mixer",AssetType.AudioMixerAsset },
            { "unity",AssetType.SceneAsset},
            { "prefab",AssetType.PrefabAsset},
            { "cs",AssetType.CSharpScript},
            { "png|tex",AssetType.PngTextureAsset},
            { "jpg|tex",AssetType.JpgTextureAsset},
            { "tga|tex",AssetType.TgaTextureAsset},
            { "png|sprite",AssetType.PngSpriteAsset},
            { "jpg|sprite",AssetType.JpgSpriteAsset},
            { "tga|sprite",AssetType.TgaSpriteAsset},
            { "spriteatlas",AssetType.SpriteAtlasAsset},
            { "anim",AssetType.AnimatoinClipAsset},
            { "controller",AssetType.AnimatoinControllerAsset},
            { "bytes",AssetType.BytesAsset},
            { "dllbytes",AssetType.HotCodeBytesAsset},
            { "txt",AssetType.TxtTextAsset},
            { "json",AssetType.TxtTextAsset},
            { "xml",AssetType.XmlTextAsset},
            { "fbx",AssetType.FbxAsset},
            { "obj",AssetType.ObjAsset},
            { "wav",AssetType.WavAudioClipAsset},
            { "mp3",AssetType.Mp3AudioClipAsset},
            { "asset",AssetType.ScriptObjectAsset},
            { AssetType_Any,AssetType.Any},
        };

        public static Dictionary<AssetType, Type> AssetTypeStrName2TypeMap = new Dictionary<AssetType, Type>()
        {
            { AssetType.AddressableGroupAsset,typeof(List<UnityEngine.Object>)},
            { AssetType.AddressableAsset,typeof(UnityEngine.Object)},
            { AssetType.PrefabAsset,typeof(GameObject)},
            {AssetType.PngTextureAsset,typeof(Texture2D)},
            {AssetType.JpgTextureAsset,typeof(Texture2D)},
            {AssetType.TgaTextureAsset,typeof(Texture2D)},
            {AssetType.PngSpriteAsset,typeof(Sprite)},
            {AssetType.JpgSpriteAsset,typeof(Sprite)},
            {AssetType.TgaSpriteAsset,typeof(Sprite)},
            {AssetType.SpriteAtlasAsset,typeof(SpriteAtlas)},
            {AssetType.AnimatoinClipAsset,typeof(AnimationClip)},
            {AssetType.AnimatoinControllerAsset,typeof(RuntimeAnimatorController)},
            {AssetType.TxtTextAsset,typeof(TextAsset)},
            {AssetType.BytesAsset,typeof(object)},
            {AssetType.HotCodeBytesAsset,typeof(object)},
            {AssetType.XmlTextAsset,typeof(TextAsset)},
            {AssetType.FbxAsset,typeof(GameObject)},
            {AssetType.WavAudioClipAsset,typeof(AudioClip)},
            {AssetType.Mp3AudioClipAsset,typeof(AudioClip)},
            {AssetType.ScriptObjectAsset,typeof(ScriptableObject)},
            {AssetType.AudioMixerAsset,typeof(UnityEngine.Object)},
        };

    }
    public static partial class AssetPathEncoder
    {
        public static bool CanReadHotRes = false;
        public static bool CanReadResFromEditor = false;
        public static bool CanReadResourceRes = false;

        public static string EncodeEnvAssetLink(this string assetPath, AssetType assetType = AssetType.Auto)
        {
            if (IsPathIsLinkPath(assetPath)) return assetPath;
            return $"$env:{GetAssetLink(assetPath, assetType)}";
        }
        public static string EncodeResourcesAssetLink(string assetPath, AssetType assetType = AssetType.Auto)
        {
            if (IsPathIsLinkPath(assetPath)) return assetPath;
            return $"$resources:{GetAssetLink(assetPath, assetType)}";
        }

        public static string EncodeEditorAssetLink(string assetPath, AssetType assetType = AssetType.Auto)
        {
            if (IsPathIsLinkPath(assetPath)) return assetPath;
            return $"$editor:{GetAssetLink(assetPath, assetType)}";
        }

        public static string EncodeHotAssetLink(string assetPath, AssetType assetType = AssetType.Auto)
        {
            if (IsPathIsLinkPath(assetPath)) return assetPath;
            return $"$hot:{GetAssetLink(assetPath, assetType)}";
        }
        private static string GetAssetLink(string assetPath)
        {
            if (IsPathIsLinkPath(assetPath)) return assetPath;
            int dotIndex = assetPath.IndexOf(".");
            int length = dotIndex == -1 ? assetPath.Length : dotIndex;
            string hashPath = assetPath.Substring(0, length);
            string fileExtension = string.Empty;
            if (dotIndex >= 0)
            {
                fileExtension = assetPath.Substring(dotIndex + 1);
            }
            string assetTypeStr = GetAssetFileTypeByExtension(fileExtension);
            return $"{assetTypeStr}&{hashPath}";
        }
        private static string GetAssetLink(string assetPath, AssetType assetType)
        {
            if (assetType == AssetType.Auto)
            {
                return GetAssetLink(assetPath);
            }
            if (IsPathIsLinkPath(assetPath)) return assetPath;
            int dotIndex = assetPath.IndexOf(".");
            int length = dotIndex == -1 ? assetPath.Length : dotIndex;
            string hashPath = assetPath.Substring(0, length);
            string fileExtension = string.Empty;
            if (dotIndex >= 0)
            {
                fileExtension = assetPath.Substring(dotIndex + 1);
            }
            string assetTypeStr = GetAssetTypeStrByType(assetType);
            string extension = "." + GetAssetFileExtensionByType(assetTypeStr);
            if (hashPath.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                hashPath = hashPath.Replace(extension, "");
            }
            return $"{assetTypeStr}&{hashPath}";
        }

        public static bool DecodeAssetUrl(string url, out PathUrlOption xPathOption)
        {
            /*
             输入路径$env:prefab&mainGame/ui/prefabs/a
            输出路径@resources:prefab&res/maingame/ui/prefabs/a;$env:prefab&res/maingame/ui/prefabs/a
             */
            PathUrlOption option = new PathUrlOption();
            option.isValidateSuc = false;
            if (string.IsNullOrEmpty(url))
            {
                xPathOption = option;
                return false;
            }

            var splits = url.Split(";");
            string loadOptinStr = splits[0];
            string readOptionStr = splits[1];
            Match matchLoad = Regex.Match(loadOptinStr, @"\@([^:]+):([^&]*)&([^&]*)");
            Match matchRead = Regex.Match(readOptionStr, @"\$([^:]+):([^&]+)&([^&]*)");

            if (!matchLoad.Success || !matchRead.Success)
            {
                xPathOption = option;
                return false;
            }
            if (matchRead.Success)
            {
                string readTypeStr = matchRead.Groups[1].Value; // $和:之间的字符串
                string assetTypeStr = matchRead.Groups[2].Value; // :和&之间的字符串
                string assetPath = matchRead.Groups[3].Value; // &以后的字符串
                AssetReadType readType = GetAssetReadType(readTypeStr);
                AssetType assetType = GetAssetType(readType, assetTypeStr);
                option.readType = readType;
                option.assetType = assetType;
                option.assetPath = assetPath;
                option.assetLink = readOptionStr;
            }

            if (matchLoad.Success)
            {
                string loadTypeStr = matchLoad.Groups[1].Value; // $和:之间的字符串
                string fileTypeStr = matchLoad.Groups[2].Value; // :和&之间的字符串
                string filePath = matchLoad.Groups[3].Value; // &以后的字符串
                AssetLoadType loadType = GetAssetLoadTypeLabel(loadTypeStr);
                option.fileLoadType = loadType;
                option.fileExtension = fileTypeStr;
                option.fileFullPath = filePath;
            }
            option.assetUrl = url;
            option.isValidateSuc = true;
            xPathOption = option;
            return true;
        }

        private static string DotToPathSplit(string path)
        {
            return path.Replace(".", "/");
        }

        public static string EncodeAssetUrl(string assetLink)
        {

            int lineIndex = assetLink.IndexOf("/");
            string standaredPath = lineIndex == -1 ? DotToPathSplit(assetLink) : assetLink;
            if (standaredPath.StartsWith("@")) return standaredPath; // 不对其进行加密

            // 使用正则表达式匹配三部分内容
            Match match = Regex.Match(standaredPath, @"\$([^:]+):([^&]+)&(.+)");

            if (match.Success)
            {
                string part1 = match.Groups[1].Value; // $和:之间的字符串
                string part2 = match.Groups[2].Value; // :和&之间的字符串
                string part3 = match.Groups[3].Value; // &以后的字符串
                string loadStr = EncodeLoadShortPath(standaredPath, part1, part2, part3, out string assetPath);
                string readStr = $"${part1}:{part2}&{assetPath}";
                return $"{loadStr};{readStr}";
            }
            else
            {
                Log.Error($"路径不符合格式! {assetLink}");
                return "";
            }
        }

        public static AssetLoadType GetAssetLoadTypeLabel(string loadTypeStr)
        {
            foreach (var item in AssetLoadTypeStrName2EnumTypeMap)
            {
                if (item.Key == loadTypeStr)
                {
                    return item.Value;
                }
            }
            return AssetLoadType.None;
        }

        private static string GetAssetLoadTypeLabel(AssetLoadType loadType)
        {
            foreach (var item in AssetLoadTypeStrName2EnumTypeMap)
            {
                if (item.Value == loadType)
                {
                    return item.Key;
                }
            }
            return string.Empty;
        }

        private static AssetReadType GetAssetReadType(string name)
        {
            if (AssetReadTypeStrName2EnumTypeMap.ContainsKey(name))
            {
                return AssetReadTypeStrName2EnumTypeMap[name];
            }
            return AssetReadType.None;
        }

        private static AssetType GetAssetType(AssetReadType readType, string assetTypeStr)
        {
            if (readType == AssetReadType.Hot)
            {
                if (assetTypeStr == AssetType_AAGroupAssets)
                {
                    return AssetType.AddressableGroupAsset;
                }
                return AssetType.AddressableAsset;
            }
            if (AssetTypeStrName2EnumTypeMap.ContainsKey(assetTypeStr))
            {
                return AssetTypeStrName2EnumTypeMap[assetTypeStr];
            }
            return AssetType.None;
        }

        private static bool IsHotRes(string assetPath)
        {
            return !HotIgnoreAsset.Contains(assetPath);
        }

        private static bool IsPathIsLinkPath(string path)
        {
            return path.StartsWith("$");
        }

        private static string AssetFileTypeRedirect(AssetReadType readType, string assetPath, string assetTypeStr)
        {
            if (readType == AssetReadType.Hot)
            {
                //if (IsHotAssetBundleAsset(assetPath, assetTypeStr))
                //{
                //    assetTypeStr = AssetType_AB;
                //}
            }
            return assetTypeStr;
        }

        /// <summary>
        /// 根据读取器的解析自动拼接加载器的路径
        /// </summary>
        /// <returns></returns>
        private static string EncodeLoadShortPath(string readPath, string readTypeStr, string assetTypeStr, string assetPath, out string newStandaredPath)
        {
            AssetReadType readType = GetAssetReadType(readTypeStr);
            AssetLoadType loadType = GetAssetLoadType(readType, assetPath, out AssetReadType redirectReadType);
            assetPath = SpecialCheck(assetPath, readType, loadType);
            readType = redirectReadType;
            AssetType assetType = GetAssetType(readType, assetTypeStr);
            string fileType = AssetFileTypeRedirect(readType, assetPath, assetTypeStr);
            string fileFullPath = GetAssetFilePath(loadType, readType, assetType, fileType, assetPath);
            string loadShortPath = $"@{GetAssetLoadTypeLabel(loadType)}:{fileType}&{fileFullPath}";
            newStandaredPath = assetPath;
            return loadShortPath;
        }

        private static string GetAssetFileExtensionByType(string assetType)
        {
            if (AssetType2FileExtensionMap.ContainsKey(assetType))
            {
                return AssetType2FileExtensionMap[assetType];
            }
            return "";
        }

        private static string GetAssetFilePath(
            AssetLoadType loadType,
            AssetReadType assetReadType,
            AssetType assetType,
            string fileType,
            string assetPath)
        {
            string finalPath = "";
            string originFileType = fileType;
            string extension = GetAssetFileExtensionByType(originFileType);
            if (!string.IsNullOrEmpty(extension))
            {
                extension = "." + extension;
            }
            if (assetReadType == AssetReadType.Editor)
            {
                // 编辑器下资源需要以Asset目录为更目录，
                finalPath = $"{assetPath}{extension}";
                return finalPath;
            }
            else if (assetReadType == AssetReadType.Resources)
            {
                return assetPath;
            }
            else if (assetReadType == AssetReadType.Hot)
            {
                string fileName = $"{assetPath}{extension}";
                //Debug.Log("fileName::" + fileName);
                return fileName;
            }

            return finalPath;
        }
        private static string GetAssetTypeStrByType(AssetType assetType)
        {
            foreach (var item in AssetTypeStrName2EnumTypeMap)
            {
                if (item.Value == assetType)
                {
                    return item.Key;
                }
            }
            return AssetType_Any;
        }
        private static string GetAssetFileTypeByExtension(string extension)
        {
            extension = extension.ToLower();
            foreach (var item in AssetType2FileExtensionMap)
            {
                if (item.Value == extension)
                {
                    return item.Key;
                }
            }
            return "any";
        }

        private static AssetLoadType GetAssetLoadType(AssetReadType readType, string assetPath, out AssetReadType redirectReadType)
        {
            redirectReadType = readType;
            AssetLoadType assetLoadType = AssetLoadType.None;
            // 注意只有受AB包管辖的目录才能使用Env模式
            if (readType == AssetReadType.Env) // 动态加载重定向
            {
                if (CanReadResFromEditor)
                {
                    readType = AssetReadType.Editor;
                }
                else if (CanReadHotRes)
                {
                    readType = AssetReadType.Hot;
                }
                else if (CanReadResourceRes)
                {
                    readType = AssetReadType.Resources;
                }
            }
            redirectReadType = readType;
            // 判断读取类型决定加载器
            if (readType == AssetReadType.None)
            {
                assetLoadType = AssetLoadType.None;
            }
            else if (readType == AssetReadType.Editor)
            {
                assetLoadType = AssetLoadType.EditorLoader;
            }
            else if (readType == AssetReadType.Resources)
            {
                assetLoadType = AssetLoadType.ResourcesLoader;
            }
            else if (readType == AssetReadType.Hot)
            {
                if (GameEnv.ResConfig.IsUseGameFirstRes)
                {
                    if (GameEnv.IsGameForPc())
                    {
                        assetLoadType = AssetLoadType.PlatformLoader;
                    }
                    else
                    {
                        // 如果是首包 如果为 ios、webgl、安卓 一律采用web加载
                        assetLoadType = AssetLoadType.WebLoader;
                    }
                }
                else
                {
                    // 非首包环境下默认使用 PlatformLoader,后续如果需要加入web远程下载需要配置动态调整
                    //TODO: 判断资源是否为远程资源，如果资源为远程资源，则使用WEbLoader
                    if (IsRemoteRes(assetPath))
                    {
                        assetLoadType = AssetLoadType.WebLoader;
                    }
                    else
                    {
                        assetLoadType = AssetLoadType.PlatformLoader;
                    }
                }
            }
            return assetLoadType;
        }



        private static bool IsHotAssetBundleIgnoreResByFileName(string fileName)
        {
            if (HotNotAssetBundleResNames.Contains(fileName)) return true;
            return false;
        }


        private static bool IsRemoteRes(string assetPath)
        {
            return false;
        }

        private static string SpecialCheck(string assetPath, AssetReadType readType, AssetLoadType loadType)
        {
            //return assetPath;
            if (readType != AssetReadType.Env) return assetPath;
            if (loadType == AssetLoadType.EditorLoader)
            {
                return assetPath;
            }
            else if (loadType == AssetLoadType.ResourcesLoader)
            {
                return assetPath;
            }
            else
            {
                return assetPath;
            }
        }
    }
}
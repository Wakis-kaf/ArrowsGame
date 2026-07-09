using Framework.Runtime.Base;
using Framework.Runtime.LogSystem;
using System;
using System.Collections.Generic;

namespace Framework.Runtime.MAsset
{
    /*
        short path 格式为
        // 文件加载方式
        文件加载类型：文件类型 & 路径
        文件解析类型：文件类型 & 路径
        @web:prefab&xxxxxxxxxxxxxxxxxxxxxxxxx;$env:any&b.xx.x.x
        @platform
        @editor
        @resources

     */

    public partial class AssetLoader
    {
        public const string ErrCode_None = "";
        public const string ErrCode_FileNotExist = "ErrCode_FileNotExist";
        public const string ErrCode_FileLoadFail = "ErrCode_FileLoadFail";
        public const string ErrCode_FileNotMatchType = "ErrCode_FileNotMatchType";
        public const string ErrCode_PlatformInterrupt = "ErrCode_PlatformInterrupt";
    }

    public partial class AssetLoader : UnitObject
    {
        public bool isReadFromEditor;
        public bool isReadFromRes;
        public bool isReadFromAB;
        private Dictionary<string, AssetFileVO> path2FileVO = new Dictionary<string, AssetFileVO>();
        public ResourcesAssetLoader ResourcesAssetLoader { get; private set; }
        public AddressablesAssetLoader AddressablesAssetLoader { get; private set; }
        public AssetFilePlatformLoader AssetFilePlatformLoader { get; private set; }
        public AssetFileWebLoader AssetFileWebLoader { get; private set; }
#if UNITY_EDITOR
        public EditorAssetLoader EditorAssetLoader { get; private set; }
#endif
        public AssetFileVO GetOrCreaAssetFileVO(string fullPath)
        {
            if (path2FileVO.ContainsKey(fullPath))
            {
                return path2FileVO[fullPath];
            }
            path2FileVO.Add(fullPath, new AssetFileVO(fullPath));
            return path2FileVO[fullPath];
        }

        public string[] testStrs = new string[]{
            "$env:prefab&mainGame.ui.prefabs.a",
            "$env:txt&luascrips.core.entry"
         };

        private Dictionary<Type, IAssetLoader> m_Type2Loader = new Dictionary<Type, IAssetLoader>();

        public AssetLoader()
        {
            ResourcesAssetLoader = new ResourcesAssetLoader();
            AddressablesAssetLoader = new AddressablesAssetLoader();
            AssetFilePlatformLoader = new AssetFilePlatformLoader();
            AssetFileWebLoader = new AssetFileWebLoader();
#if UNITY_EDITOR
            EditorAssetLoader = new EditorAssetLoader();
#endif
            //            ResourcesAssetLoader.CreateInstance();
            //            AddressablesAssetLoader.CreateInstance();
            //#if UNITY_EDITOR
            //            EditorAssetLoader.CreateInstance();
            //#endif

#if UNITY_EDITOR
            m_Type2Loader.Add(typeof(EditorAssetLoader), EditorAssetLoader);
#endif
            m_Type2Loader.Add(typeof(ResourcesAssetLoader), ResourcesAssetLoader);
            m_Type2Loader.Add(typeof(AddressablesAssetLoader), AddressablesAssetLoader);
        }
        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            ResourcesAssetLoader.Dispose();
            AddressablesAssetLoader.Dispose();
#if UNITY_EDITOR
            EditorAssetLoader.Dispose();
#endif
        }
        public void InitLoader()
        {
            ResourcesAssetLoader.InitLoader();
            AddressablesAssetLoader.InitLoader();
        }

        public void StartLoader()
        {
            AssetPathEncoder.CanReadHotRes = GameEnv.ResConfig.EnableHotResLoad;
            AssetPathEncoder.CanReadResourceRes = GameEnv.ResConfig.EnableResourcesResLoad;
            AssetPathEncoder.CanReadResFromEditor = GameEnv.ResConfig.EnableEditorResLoad;
            AddressablesAssetLoader.StartResLoad();
        }

        public IAssetVO LoadAssetSync(string assetLink)
        {
            return LoadAssetSyncByXpath(AssetPathEncoder.EncodeAssetUrl(assetLink));
        }

        public IAssetVO LoadAssetSyncByXpath(string xPath)
        {
            if (!AssetPathEncoder.DecodeAssetUrl(xPath, out PathUrlOption option))
            {
                Log.Error("解析XPath路径失败" + xPath);
                return null;
            }
            if (option.fileLoadType == AssetLoadType.None)
            {
                Log.Error("加载器为空！！" + xPath);
                return null;
            }
            if (option.fileLoadType == AssetLoadType.EditorLoader ||
               option.fileLoadType == AssetLoadType.ResourcesLoader)
            {
                // 读取和加载同源，所以直接使用加载就可以
                return LoadAssetFileSyncByXpath(xPath).AssetVO;
            }
            //Log.Info("使用 AssetBundlerLoader加载" + option.assetPath);
            return AddressablesAssetLoader.LoadAssetSync(typeof(UnityEngine.Object), option.assetPath);
        }

        public IAssetVO LoadAsset(string assetLink, Action<IAssetVO> onAssetLoadCb)
        {
            return LoadAssetByUrl(AssetPathEncoder.EncodeAssetUrl(assetLink), onAssetLoadCb);
        }

        public IAssetVO LoadAssetByUrl(string assetUrl, Action<IAssetVO> onAssetLoadCb)
        {
            if (!AssetPathEncoder.DecodeAssetUrl(assetUrl, out PathUrlOption option))
            {
                Log.Error("解析XPath路径失败" + assetUrl);
                onAssetLoadCb?.Invoke(null);
                return null;
            }
            if (option.fileLoadType == AssetLoadType.None)
            {
                Log.Error("加载器为空！！" + assetUrl);
                onAssetLoadCb?.Invoke(null);
                return null;
            }
            if (option.fileLoadType == AssetLoadType.EditorLoader ||
                option.fileLoadType == AssetLoadType.ResourcesLoader)
            {
                // 读取和加载同源，所以直接使用加载就可以
                return LoadAssetFileByXpath(assetUrl, onAssetLoadCb, null).AssetVO;
            }
            return AddressablesAssetLoader.LoadAssetAsync(option, onAssetLoadCb);
        }

        public IAssetFileVO LoadAssetFileSync(string assetLink)
        {
            return LoadAssetFileSyncByXpath(AssetPathEncoder.EncodeAssetUrl(assetLink));
        }

        public IAssetFileVO LoadAssetFile(string assetLink, Action<IAssetVO> onAssetLoadCb = null, Action<IAssetFileVO> assetFileVOHandler = null)
        {
            return LoadAssetFileByXpath(AssetPathEncoder.EncodeAssetUrl(assetLink), onAssetLoadCb, assetFileVOHandler);
        }

        public IAssetFileVO LoadAssetFileSyncByXpath(string xPath)
        {
            if (!AssetPathEncoder.DecodeAssetUrl(xPath, out PathUrlOption option))
            {
                Log.Error("解析XPath路径失败" + xPath);
                return null;
            }
            if (option.fileLoadType == AssetLoadType.None)
            {
                Log.Error("加载器为空！！" + xPath);
                return null;
            }
            if (option.fileLoadType == AssetLoadType.EditorLoader)
            {
                AssetFileVO assetFileVO = GetOrCreaAssetFileVO(xPath);

                assetFileVO.option = option;
#if UNITY_EDITOR
                assetFileVO.assetVO = EditorAssetLoader.LoadAssetSync(option.GetFileType(),
                    option.fileFullPath);
#endif
                return assetFileVO;
            }
            else if (option.fileLoadType == AssetLoadType.ResourcesLoader)
            {
                AssetFileVO assetFileVO = GetOrCreaAssetFileVO(xPath);
                assetFileVO.option = option;
                assetFileVO.assetVO = ResourcesAssetLoader.LoadAssetSync(
                    option.GetFileType(),
                    option.fileFullPath);
                return assetFileVO;
            }
            else if (option.fileLoadType == AssetLoadType.PlatformLoader)
            {
                AssetFileVO assetFileVO = AssetFilePlatformLoader.LoadAssetFileSync(option.GetFileType(), option.fileFullPath) as AssetFileVO;
                assetFileVO.option = option;
                return assetFileVO;
            }
            else if (option.fileLoadType == AssetLoadType.WebLoader)
            {
                AssetFileVO assetFileVO = AssetFileWebLoader.LoadAssetFileSync(option.GetFileType(), option.fileFullPath) as AssetFileVO;
                assetFileVO.option = option;
                return assetFileVO;
            }
            return null;
        }

        public IAssetFileVO LoadAssetFileByXpath(string xPath, Action<IAssetVO> onAssetLoadCb, Action<IAssetFileVO> assetFileVOLoadHandler)
        {
            if (!AssetPathEncoder.DecodeAssetUrl(xPath, out PathUrlOption option))
            {
                Log.Error("解析XPath路径失败" + xPath);
                assetFileVOLoadHandler?.Invoke(null);
                onAssetLoadCb?.Invoke(null);
                return null;
            }
            if (option.fileLoadType == AssetLoadType.None)
            {
                Log.Error("加载器为空！！" + xPath);
                assetFileVOLoadHandler?.Invoke(null);
                onAssetLoadCb?.Invoke(null);
                return null;
            }
            if (option.fileLoadType == AssetLoadType.EditorLoader)
            {
                //Log.Info("EditorLoader 加载器加载" + xPath);
                AssetFileVO assetFileVO = GetOrCreaAssetFileVO(xPath);
#if UNITY_EDITOR
                assetFileVO.option = option;
                assetFileVO.assetVO = EditorAssetLoader.LoadAssetAsync(option.GetFileType(),
                    option.fileFullPath,
                    (assetVo) =>
                    {
                        assetFileVOLoadHandler?.Invoke(assetFileVO);
                        onAssetLoadCb?.Invoke(assetVo);
                    },
                    option.GetFileLoadDefaultPriority());
#endif
                return assetFileVO;
            }
            else if (option.fileLoadType == AssetLoadType.ResourcesLoader)
            {
                //Log.Info("ResourcesLoader 加载器加载" + xPath);
                AssetFileVO assetFileVO = GetOrCreaAssetFileVO(xPath);
                assetFileVO.option = option;
                assetFileVO.assetVO = ResourcesAssetLoader.LoadAssetAsync(
                    option.GetFileType(),
                    option.fileFullPath,
                    (assetVo) =>
                    {
                        assetFileVOLoadHandler?.Invoke(assetFileVO);
                        onAssetLoadCb?.Invoke(assetVo);
                    },
                    option.GetFileLoadDefaultPriority());

                return assetFileVO;
            }
            else if (option.fileLoadType == AssetLoadType.PlatformLoader)
            {
                //Log.Info("PlatformLoader 加载器加载" + xPath);
                AssetFileVO assetFileVO = AssetFilePlatformLoader.LoadAssetFileAsync(option.GetFileType(), option.fileFullPath, (assetFileVO) =>
                {
                    (assetFileVO as AssetFileVO).option = option;
                    assetFileVOLoadHandler?.Invoke(assetFileVO);
                    onAssetLoadCb?.Invoke(null);
                }, option.GetFileLoadDefaultPriority()) as AssetFileVO;
                assetFileVO.option = option;
                return assetFileVO;
            }
            else if (option.fileLoadType == AssetLoadType.WebLoader)
            {
                //Log.Info("WebLoader 加载器加载" + xPath);
                AssetFileVO assetFileVO = AssetFileWebLoader.LoadAssetFileAsync(option.GetFileType(), option.fileFullPath, (assetFileVO) =>
                 {
                     (assetFileVO as AssetFileVO).option = option;
                     assetFileVOLoadHandler?.Invoke(assetFileVO);
                     onAssetLoadCb?.Invoke(null);
                 }, option.GetFileLoadDefaultPriority()) as AssetFileVO;
                assetFileVO.option = option;
                return assetFileVO;
            }
            return null;
        }
    }

    public struct PathUrlOption
    {
        public AssetReadType readType;
        public AssetType assetType;
        public string assetPath;
        public AssetLoadType fileLoadType;
        public string fileFullPath;
        public string fileExtension;
        public bool isValidateSuc;
        internal string assetLink;
        internal string assetUrl;

        public Type GetFileType()
        {
            if (AssetPathEncoder.AssetTypeStrName2TypeMap.ContainsKey(this.assetType))
            {
                return AssetPathEncoder.AssetTypeStrName2TypeMap[this.assetType];
            }
            return null;
        }

        public int GetFileLoadDefaultPriority()
        {
            return 0;
        }
    }

    public enum AssetReadType
    {
        None,
        Env,
        Editor,
        Hot,
        Resources
    }

    public enum AssetLoadType
    {
        None,
        EditorLoader,
        ResourcesLoader,
        WebLoader,
        PlatformLoader,
    }

    public enum AssetType
    {
        None,
        Auto,
        Any,
        AddressableAsset,
        AssetBundleManifestAsset,
        SceneAsset,
        PrefabAsset,
        CSharpScript,
        PngTextureAsset,
        JpgTextureAsset,
        TgaTextureAsset,
        PngSpriteAsset,
        JpgSpriteAsset,
        TgaSpriteAsset,
        SpriteAtlasAsset,
        AnimatoinClipAsset,
        AnimatoinControllerAsset,
        TxtTextAsset,
        BytesAsset,
        HotCodeBytesAsset,
        XmlTextAsset,
        FbxAsset,
        ObjAsset,
        WavAudioClipAsset,
        Mp3AudioClipAsset,
        ScriptObjectAsset,
        AddressableGroupAsset,
        AudioMixerAsset,
    }
}
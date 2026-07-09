using Framework.Runtime.LogSystem;
using Framework.Runtime.Module.Core;
using Framework.Runtime.UnitSystem.BIInterfaces;
using System;

namespace Framework.Runtime.MAsset
{
    public class AssetManager : ModuleUnit, IUnitUpdate, IUnitDestroy
    {

        private AssetUpdater m_AssetUpdater;
        public AssetLoader AssetLoader { get; private set; }
        public AssetFilePlatformLoader AssetFilePlatformLoader { get; private set; }
        public AssetFileWebLoader AssetFileWebLoader { get; private set; }
        public AssetUpdater AssetUpdater => m_AssetUpdater;

        public AssetManager()
        {
        }

        protected override void OnInit()
        {
            base.OnInit();
            ConstructLoaders();
        }

        private void ConstructLoaders()
        {
            m_AssetUpdater = new AssetUpdater();
            AssetLoader = new AssetLoader();
            AssetFilePlatformLoader = new AssetFilePlatformLoader();
            AssetFileWebLoader = new AssetFileWebLoader();
            //AssetFilePlatformLoader.CreateInstance();
            //AssetFileWebLoader.CreateInstance();
        }

        protected override void OnModuleConstructed()
        {
            Log.Debug("构建资源模块");
            InitLoader();
        }

        public override void OnAppUpdate(GameAppMessage appMessage)
        {
            base.OnAppUpdate(appMessage);
            if (appMessage.MessageCode == GameAppMessage.code_gameConfig_loadSuccess)
            {
                StartLoader();
            }
            if (appMessage.MessageCode == GameAppMessage.code_assetModule_loadSuccess)
            {
                StartNewestHandle();
            }
        }

        private void InitLoader()
        {
            AssetLoader.InitLoader();
        }

        private void StartLoader()
        {
            AssetLoader.StartLoader();
        }

        private void StartNewestHandle()
        {
            AssetUpdater.SetResNewsetCallback(OnResNewestCheckOver);
            AssetUpdater.CheckResNewest();
        }

        private void OnResNewestCheckOver()
        {
            GameApp.Ins.SendModuleUpdateMessage(new GameAppMessage(GameAppMessage.code_assetModule_newestSuccess));
        }
        public IAssetVO LoadEnvAsset(string assetPath, Action<IAssetVO> cb, AssetType assetType = AssetType.Auto)
        {
            string link = AssetPathEncoder.EncodeEnvAssetLink(assetPath,assetType);
            return LoadAssetAsync(link, cb);
        }
        public IAssetVO LoadResourcesAsset(string assetPath, Action<IAssetVO> cb, AssetType assetType = AssetType.Auto)
        {
            string link = AssetPathEncoder.EncodeResourcesAssetLink(assetPath, assetType);
            return LoadAssetAsync(link, cb);
        }
        public IAssetVO LoadEditorAsset(string assetPath, Action<IAssetVO> cb, AssetType assetType = AssetType.Auto)
        {
            string link = AssetPathEncoder.EncodeEditorAssetLink(assetPath, assetType);
            return LoadAssetAsync(link, cb);
        }
        public IAssetVO LoadHotAsset(string assetPath, Action<IAssetVO> cb, AssetType assetType = AssetType.Auto)
        {
            string link = AssetPathEncoder.EncodeHotAssetLink(assetPath, assetType);
            return LoadAssetAsync(link, cb);
        }
        public IAssetVO LoadAssetAsync(string assetLink, Action<IAssetVO> cb)
        {
            return AssetLoader.LoadAsset(assetLink, cb);
        }
        public IAssetVO LoadEnvAssetSync(string assetPath, AssetType assetType = AssetType.Auto)
        {
            string link = AssetPathEncoder.EncodeEnvAssetLink(assetPath, assetType);
            return LoadAssetSync(link);
        }
        public IAssetVO LoadResourcesAssetSync(string assetPath, AssetType assetType = AssetType.Auto)
        {
            string link = AssetPathEncoder.EncodeResourcesAssetLink(assetPath, assetType);
            return LoadAssetSync(link);
        }
        public IAssetVO LoadEditorAssetSync(string assetPath, AssetType assetType = AssetType.Auto)
        {
            string link = AssetPathEncoder.EncodeEditorAssetLink(assetPath, assetType);
            return LoadAssetSync(link);
        }
        public IAssetVO LoadHotAssetSync(string assetPath, Action<IAssetVO> cb, AssetType assetType = AssetType.Auto)
        {
            string link = AssetPathEncoder.EncodeHotAssetLink(assetPath, assetType);
            return LoadAssetSync(link);
        }
        public IAssetVO LoadAssetSync(string assetLink)
        {
            return AssetLoader.LoadAssetSync(assetLink);
        }
        public IAssetVO LoadAsset(string assetLink, Action<IAssetVO> onAssetLoadCb)
        {
            return AssetLoader.LoadAsset (assetLink, onAssetLoadCb);
        }
        public void OnUnitUpdate()
        {
        }

        public void OnUnitDestroy()
        {
            AssetLoader.Dispose();
        }
    }
}
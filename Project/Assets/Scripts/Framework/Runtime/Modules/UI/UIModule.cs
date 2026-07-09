using Framework.Runtime.LogSystem;
using Framework.Runtime.MAsset;
using Framework.Runtime.Module;
using Framework.Runtime.Module.Core;

using Framework.Runtime.UnitSystem.BIInterfaces;

namespace Framework.Runtime.UI
{
    public class UIModule : ModuleUnit, IUnitDestroy
    {
        public UIWindow UIWindow { get; private set; }
        public void OnUnitDestroy()
        {
        }

        protected override void DisposeUnManagedResources()
        {
            PanelManager.Ins.Dispose();
            UIWindow.Ins.Dispose();
            base.DisposeUnManagedResources();
        }
        
        protected override void OnInit()
        {
            base.OnInit();
           
        }

        protected override void OnModuleConstructed()
        {
            base.OnModuleConstructed();
            UIWindow = new UIWindow();
            BindAssetLoader();
        }

        private void BindAssetLoader()
        {
            var assetManager = GameApp.AssetManager;
            if (assetManager == null)
            {
                Log.Fatal("asset module not found!");
                return;
            }
            Log.Info("为UI模块绑定 资源加载系统成功");
            // TODO: 为 UnitUI Agent 设置
            UIAgent.SetInfoLogAgent(Log.Info);
            UIAgent.SetErrorLogAgent(Log.Error);
            UIAgent.SetAssetLoadAsyncAgent(assetManager.LoadAssetAsync);
            UIAgent.SetAssetLoadSyncAgent(assetManager.LoadAssetSync);
        }
    }
}
using Framework.Runtime;
using Framework.Runtime.LogSystem;
using Framework.Runtime.MGameModule;


namespace Game.Modules
{
    public class GameConfigDataHandler : GameModuleDataHandler
    {

        public T GetConfigTable<T>(string configName,ref T cfgTable) where T : class
        {
            if (cfgTable != null) return cfgTable;
            if (TryReadConfig<T>(configName, out var cfg))
            {
                Log.Info("读取cfg_sceneItem成功");
                cfgTable = cfg;
                return cfg;
            }
            Log.Error("读取cfg_sceneItem失败");
            return cfgTable;
        }
        public bool TryReadConfig<T>(string configName, out T readConfig) where T : class
        {
            if (GameApp.GameModuleManager.TryGetGameModule<GameConfigModule>(out GameConfigModule gameConfigModule))
            {
                if (gameConfigModule.TryDecodeConfig<T>(configName, out readConfig))
                {
                    return true;
                }
                Log.Fatal($"加载config失败{configName} ，解析错误,请检查类定义是否正确");
                readConfig = null;
                return false;
            }
            else
            {
                Log.Fatal($"加载config失败{configName} ，未注册GameConfig 模块");
                readConfig = null;
                return false;
            }
        }

        protected override void OnHandlerAwake()
        {
        }

        protected override void OnHandlerDestroy()
        {
        }

        protected override void OnHandlerStart()
        {
           
        }
    }
}
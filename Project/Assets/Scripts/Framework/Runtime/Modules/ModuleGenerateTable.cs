using Framework.Runtime.Archives;
using Framework.Runtime.MAsset;
using Framework.Runtime.MAudio;
using Framework.Runtime.MCombat;
using Framework.Runtime.MDebugger;
using Framework.Runtime.MGameModule;
using Framework.Runtime.MLanAndTheme;
using Framework.Runtime.MObjectPool;
using Framework.Runtime.MSceneUnit;
using Framework.Runtime.UI;
using System;
using System.Collections.Generic;

namespace Framework.Runtime.Module
{
    public static class ModuleGenerateTable
    {
        public static readonly Dictionary<Type, int> BuiltInGameThemeModule = new Dictionary<Type, int>()
        {
        };

        public static readonly Dictionary<Type, int> BuiltInModule = new Dictionary<Type, int>()
        {
            {typeof(PoolModule), 1000},
            {typeof(ArchiveModule), 3000},
            {typeof(AssetManager), 3500},
            //{typeof(NetModule), 5000},
            {typeof(WebRequestModule), 6000},
            //{typeof(AssetUpdaterModule), 6500},
            {typeof(InputModule), 7000},
            {typeof(AudioModule), 8000},
            {typeof(UIModule), 9000},
            {typeof(DebuggerModule), 10000},
            {typeof(GameModuleManager),2000 },
            {typeof(SceneUnitManager),12000 },
            {typeof(CombatSystem),130000 },
            {typeof(LanAndThemeModule),140000 },
            //{typeof(SceneModule), 12000},
            //{typeof(ProcedureModule), 130000},
            //{typeof(I2LModule), 140000},
        };

        public static readonly Dictionary<Type, int> BuiltInPluginModule = new Dictionary<Type, int>()
        {
        };
    }
}
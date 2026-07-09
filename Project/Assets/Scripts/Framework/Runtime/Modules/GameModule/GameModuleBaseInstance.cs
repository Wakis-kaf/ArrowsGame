using Framework.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Game.Modules
{
    public class GameModuleBaseInstance<T> :GameModuleBase where T:GameModuleBase
    {
        public static T Ins
        {
            get
            {
                GameApp.GameModuleManager.TryGetGameModule<T>(out var gameModule);
                return gameModule;
            }
        }
        public static THandler GetHandlerIns<THandler>() where THandler:GameModuleHandler
        {
            return GetIns().GetHandler<THandler>();
        }
        public static bool TryGetModuleInstance(out T gameModule)
        {
            return GameApp.GameModuleManager.TryGetGameModule(out gameModule);
        }

        public static T GetIns()
        {
            GameApp.GameModuleManager.TryGetGameModule<T>(out  var gameModule);
            return gameModule;
        }
    }
}

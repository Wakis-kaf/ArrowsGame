using Framework.Runtime;
using Framework.Runtime.MGameModule;
using Framework.Runtime.MLanAndTheme;
using Game.Modules.GModulePlayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleInput
{
    public class GameInputClientHandler : GameModuleLogicHandler
    {
        public static GameInputClientHandler Ins => GetModuleHandlerIns<GameInputClientHandler>();
        protected override void OnHandlerAwake()
        {

        }
        protected override void OnHandlerEnable()
        {

        }
        protected override void OnHandlerStart()
        {
            GameApp.InputModule.RegisterLayerMap(GameInputConstant.GameInputLayer);
        }
        protected override void OnHandlerDestroy()
        {

        }
        public InputLayer GetPlayerInputLayer()
        {
            return GameApp.InputModule.GetController(GameInputConstant.Play_Input_Layer);
        }





    }

}

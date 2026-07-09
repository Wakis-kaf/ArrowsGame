using Game.Modules.GModuleSceneUnit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Modules.GModuleFX
{
    public class GameFX : GameModuleBaseInstance<GameFX>
    {
        /// <summary>
        /// 构造函数中调用，托管对象可以在这初始化
        /// </summary>
        protected override void OnConstructed()
        {
            
        }
        /// <summary>
        /// 注册所有的处理类
        /// </summary>
        protected override void GenerateHandlers()
        {
            RegisterHandler<GameFXClientHandler>();
            RegisterHandler<GameFXDataHandler>();
            RegisterHandler<GameFXViewHandler>();
        }
        /// <summary>
        /// 当所有游戏模块刚被构建的时候回传触发
        /// </summary>
        protected override void OnModuleAwake()
        {
         
        }
        /// <summary>
        /// 当所有游戏模块已被创建成功的时候回传触发
        /// </summary>
        protected override void OnModuleStart()
        {
          
        }

        /// <summary>
        /// 当游戏模块被销毁的时候回传触发
        /// </summary>
        protected override void OnModuleDestroy()
        {
            
        }

        public FXSceneUnit GetFX(int fxId)
        {
            var fxSceenUnit = GameSceneUnitClientHandler.Ins.GameSceneUnitPool.GetSceneUnit<FXSceneUnit>(fxId);
            return fxSceenUnit;
        }
    }

}

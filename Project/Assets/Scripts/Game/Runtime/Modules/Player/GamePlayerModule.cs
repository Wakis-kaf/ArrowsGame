using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Modules.GModulePlayer
{
    public class GamePlayerModule : GameModuleBaseInstance<GamePlayerModule>
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
            RegisterHandler<GamePlayerClientHandler>();
            RegisterHandler<GamePlayerDataHandler>();
            RegisterHandler<GamePlayerViewHandler>();
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
    }

}

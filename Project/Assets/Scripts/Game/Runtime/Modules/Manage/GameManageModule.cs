using Framework.Runtime;
using Framework.Runtime.LogSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Modules.GModuleManage
{
    public class GameManageModule : GameModuleBaseInstance<GameManageModule>
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
            RegisterHandler<GameManageClientHandler>();
            RegisterHandler<GameManageDataHandler>();
            RegisterHandler<GameManageViewHandler>();
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
        protected override void OnCheckModuleLoad()
        {
            Log.Debug("存档加载中...");
            // 加载存档
            GameApp.ArchiveModule.LoadArchive<GameArchive>("GameMainArchive", OnArchiveLoaded, true);

        }
        private void OnArchiveLoaded(GameArchive archive, bool isLoad)
        {
            if (isLoad)
            {
                Log.Debug("存档加载成功");
                archive.LoadFromArchive();
                archive.IsNewCreateArchive = false;
            }
            else
            {
                Log.Error("存档加载失败，已经创建新存档");
                archive.InitArchive();
                archive.IsNewCreateArchive = true;
            }
            GameApp.ArchiveModule.SetAutoSave(archive, true);
            GameManageClientHandler.Ins.SetMainArchive(archive);
            OnModuleLoaded();
        }
    }

}

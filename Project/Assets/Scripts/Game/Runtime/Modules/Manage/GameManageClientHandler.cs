using Framework.Runtime;
using Framework.Runtime.LogSystem;
using Framework.Runtime.MGameModule;
using Framework.Runtime.MLanAndTheme;
using Framework.Runtime.UI;
using Game.Modules.GModuleArrows;
using Game.Modules.GModuleAudio;
using Game.Modules.GModuleInventory;
using Game.Modules.GModuleStage;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleManage
{
    public class GameManageClientHandler : GameModuleLogicHandler
    {
        public static GameManageClientHandler Ins => GetModuleHandlerIns<GameManageClientHandler>();
        private GameArchive m_GameMainArchive;
        public GameArchive GameMainArchive { get => m_GameMainArchive; }
        protected override void OnHandlerAwake()
        {

        }
        protected override void OnHandlerEnable()
        {

        }

        public void SetMainArchive(GameArchive archive)
        {
            m_GameMainArchive = archive;
        }
        protected override void OnHandlerStart()
        {
            MessageDispatcher.Ins.Subscribe(MessageCode.msg_mainGame_start, OnGameStart);
            MessageDispatcher.Ins.Subscribe(MessageCode.msg_return_home, OnReturnHome);
            MessageDispatcher.Ins.Subscribe(MessageCode.msg_entryGamePlay, OnEntryGamePlay);

        }

        private void OnEntryGamePlay()
        {
            GameArrowsClientHandler.Ins.StartLevel(GameArchive.Main.LevelArchive.GetCurLevelId());
        }

        private void OnReturnHome()
        {
            GameArrowsClientHandler.Ins.QuitLevel();
            PanelManager.Ins.CloseAllUIPanel();
            PanelManager.Ins.CloseAllHighUIPanel();
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_open_entry_panel);

        }

        private void OnGameStart()
        {
            GameApp.Theme2LocalManager.InitThemeMap(GameManageDataHandler.Ins.LoadThemeMap());
            GameApp.Theme2LocalManager.SetCurrentThemeType(ThemeType.Theme1);
            CheckArchive();
        }
        private void CheckArchive()
        {
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_mainArchiveLoaded, m_GameMainArchive);
            if (m_GameMainArchive.IsNewCreateArchive)
            {
                // TryAddNewGameItems();

            }
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_mainArchiveChecked, m_GameMainArchive);
            GameLoading.Ins.CloseLoading();
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_open_entry_panel);
        }
        private void TryAddNewGameItems()
        {
            // var initItems = GameManageDataHandler.Ins.GetGameMainCfg().initItems;
            // for (int i = 0; i < initItems.Length; i++)
            // {
            // var item = initItems[i];
            // GameInventoryDataHandler.Ins.StoreItem(item.initItemId, item.initItemCount);
            // }
        }


        protected override void OnHandlerDestroy()
        {

        }
    }

}

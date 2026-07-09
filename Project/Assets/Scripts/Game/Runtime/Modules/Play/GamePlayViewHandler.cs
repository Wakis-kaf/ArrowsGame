
using Framework.Runtime;
using Framework.Runtime.Config;
using Framework.Runtime.MGameModule;
using Framework.Runtime.UI;
using Game.Modules.GModuleAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Game.Modules.GModulePlay
{
    public class GamePlayViewHandler : GameModuleViewHandler
    {
        public static GamePlayViewHandler Ins => GetModuleHandlerIns<GamePlayViewHandler>();
        //public FloatTipMgr FloatTipMgr { get; private set; }
        protected override void OnHandlerAwake()
        {
            //FloatTipMgr = new FloatTipMgr();
        }
        protected override void OnHandlerEnable()
        {

        }
        protected override void OnHandlerStart()
        {
            base.OnHandlerStart();
            MessageDispatcher.Ins.Subscribe(MessageCode.msg_open_entry_panel, OnOpenEntryPanel);
            MessageDispatcher.Ins.Subscribe(MessageCode.msg_open_gameplay_panel, OnOpenGamePlayPanel);
            MessageDispatcher.Ins.Subscribe(MessageCode.msg_close_gameplay_panel, OnCloseGamePlayPanel);
            MessageDispatcher.Ins.Subscribe(MessageCode.msg_open_gameOver_panel, OnOpenGameFailPanel);
            MessageDispatcher.Ins.Subscribe(MessageCode.msg_close_gameOver_panel, OnCloseGameFailPanel);
            MessageDispatcher.Ins.Subscribe(MessageCode.msg_open_gameSuccess_panel, OnOpenGameSuccessPanel);
            MessageDispatcher.Ins.Subscribe(MessageCode.msg_close_gameSuccess_panel, OnCloseGameSuccessPanel);
            MessageDispatcher.Ins.Subscribe(MessageCode.msg_open_gameSetting_panel, OnOpenGameSettingPanel);
            MessageDispatcher.Ins.Subscribe(MessageCode.msg_close_gameSetting_panel, OnCloseGameSettingPanel);
            MessageDispatcher.Ins.Subscribe<ArrowClickPointTipWindowData>(MessageCode.msg_play_arrow_click_point_tip, PlayArrowClickPointTip);
        }

        private void OnCloseGameSettingPanel()
        {
            Panel.ClosePanel<PlaySettingPanel>();
        }

        private void OnOpenGameSettingPanel()
        {
            Panel.OpenPanel<PlaySettingPanel>();
        }

        private void OnCloseGameSuccessPanel()
        {
            Panel.ClosePanel<PlaySuccessPanel>();
        }

        private void OnOpenGameSuccessPanel()
        {
            Panel.OpenPanel<PlaySuccessPanel>();
        }

        private void OnCloseGameFailPanel()
        {
            Panel.ClosePanel<PlayFailPanel>();
        }

        private void OnOpenGameFailPanel()
        {
            Panel.OpenPanel<PlayFailPanel>();
        }

        private void OnCloseGamePlayPanel()
        {
            Panel.ClosePanel<PlayGamePanel>();
        }

        private void OnOpenGamePlayPanel()
        {
            PlayGamePanel panel = Panel.OpenPanel<PlayGamePanel>();
        }
        private ArrowClickPointTipWindow m_TipWindow;
        private void PlayArrowClickPointTip(ArrowClickPointTipWindowData data)
        {
            if (m_TipWindow == null || m_TipWindow.IsDisposed)
            {
                m_TipWindow = UIWindow.Ins.OpenWindow<ArrowClickPointTipWindow>("", GlobalConstant.LAYER_ALERT);
            }
            m_TipWindow?.OpenWindow();
            m_TipWindow.PlayTipAnim(data);
        }

        private void OnOpenEntryPanel()
        {
            Panel.OpenPanel<PlayEntryPanel>();
            GameAudioClientHandler.Ins.PlayBgm(GameAudioConstant.Bgm_GameEntry1);
        }

        private void OnWokrShowpTableStateChanged()
        {

        }

        protected override void OnHandlerDestroy()
        {

        }
    }

}

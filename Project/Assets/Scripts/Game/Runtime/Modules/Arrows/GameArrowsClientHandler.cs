using Framework.Runtime;
using Framework.Runtime.LogSystem;
using Framework.Runtime.MGameModule;
using Game.Modules.GModuleStage;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleArrows
{
    public class GameArrowsClientHandler : GameModuleLogicHandler
    {

        public static GameArrowsClientHandler Ins => GetModuleHandlerIns<GameArrowsClientHandler>();

        private LevelVO m_LevelVO;
        public LevelVO LevelVO => m_LevelVO;
        protected override void OnHandlerAwake()
        {

        }
        protected override void OnHandlerEnable()
        {

        }
        protected override void OnHandlerStart()
        {

        }
        protected override void OnHandlerDestroy()
        {

        }
        public void ReStartLevel(int levelId)
        {
            if (m_LevelVO == null)
            {
                m_LevelVO = new LevelVO();
                m_LevelVO.SetStatusChange(OnLevelStatusChange);
            }
            m_LevelVO.SetAsCurrent();
            m_LevelVO.ReStartLevel(levelId);
        }
        public void StartLevel(int levelId)
        {
            if (m_LevelVO == null)
            {
                m_LevelVO = new LevelVO();
                m_LevelVO.SetStatusChange(OnLevelStatusChange);
            }
            m_LevelVO.SetAsCurrent();
            m_LevelVO.StartLevel(levelId);

            // m_LevelVO.CheckLeveLoad();
        }
        public void QuitLevel()
        {
            if (m_LevelVO != null)
            {
                m_LevelVO.LevelQuit();
            }
        }
        private void OnLevelStatusChange(LevelStatus status)
        {
            // if (status == LevelStatus.Loaded)
            // {
            //     m_LevelVO.StartGame();

            // }
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_level_status_change, status);
        }

        private bool m_IsQuickAnimModel = false;
        public bool IsQuickAnimModel()
        {
            return m_IsQuickAnimModel;
        }
        public void SetQuickAnimModel(bool isQuickAnimModel)
        {
            m_IsQuickAnimModel = isQuickAnimModel;
        }
    }

}

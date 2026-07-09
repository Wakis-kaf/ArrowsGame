using Framework.Runtime;
using Framework.Runtime.LogSystem;
using Framework.Runtime.MGameModule;
using Framework.Utils;
using Game.Modules.GModuleArrows;
using Game.Modules.GModuleTip;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleArrowGenerateEdit
{
    public class GameArrowGenerateEditClientHandler : GameModuleLogicHandler
    {

        public static GameArrowGenerateEditClientHandler Ins => GetModuleHandlerIns<GameArrowGenerateEditClientHandler>();
        private ArrowGenerateEditLevelVO m_LevelVO = null;
        protected override void OnHandlerAwake()
        {

        }
        protected override void OnHandlerEnable()
        {

        }
        protected override void OnHandlerStart()
        {
            MessageDispatcher.Ins.Subscribe("msg_levelArrowsGenerateScene_start", OnGameStart);
        }

        private void OnGameStart()
        {
            GameLoading.Ins.CloseLoading();
            GameArrowsClientHandler.Ins.SetQuickAnimModel(true);
            GameArrowGenerateEditViewHandler.Ins.OpenArrowsGenerateEditPanel();

        }

        protected override void OnHandlerDestroy()
        {

        }

        public void LoadLevel(int levelId)
        {
            if (m_LevelVO == null)
            {
                m_LevelVO = new ArrowGenerateEditLevelVO();
                m_LevelVO.SetStatusChange(OnLevelStatusChange);
            }
            // var levelInfo = new LevelInfo();
            // levelInfo.levelId = levelId;
            // levelInfo.levelCfg = GameArrowsDataHandler.Ins.GetLevelConfig(levelInfo.levelId);
            // m_LevelVO.LevelInfo = levelInfo;
            m_LevelVO.SetAsCurrent();
            m_LevelVO.StartLevel(levelId);
            // m_LevelVO.CheckLeveLoad();
        }
        public void ReloadCurLevel()
        {
            if (LevelVO.Current == null) return;
            m_LevelVO.SetAsCurrent();
            LevelVO.Current.ReStartLevel();
            // m_LevelVO.CheckLeveLoad();
        }
        private void OnLevelStatusChange(LevelStatus status)
        {
            // if (status == LevelStatus.Loaded)
            // {
            //     m_LevelVO.StartGame();

            // }
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_level_status_change, status);
        }

        public void ExportCurLevel()
        {
            if (m_LevelVO == null) return;
            m_LevelVO.OnLevelLoaded(() =>
             {
                 CfgArrowLayout cfgArrowLayout = m_LevelVO.ToRecordArrowLayout();
                 string json = Utility.Json.ToJson(cfgArrowLayout);
                 SaveLevelJson(m_LevelVO.LevelInfo.levelId, json);
             });
        }
        private void SaveLevelJson(int levelId, string json)
        {
            string filePath = GetLevelJsonFilePath(levelId);
            Log.Info($"保存 关卡数据{levelId}到{filePath}");
            GameTip.Ins.TipCommonMsg($"导出关卡{levelId}成功,路径{filePath}");
            Utility.FileUtil.SaveFile(filePath, json);
        }
        private string GetLevelJsonFilePath(int levelId)
        {
            string assetPath = $"AddressableResources/LevelConfigs/LevelArrowConfigs/level_{levelId}.json";
            string filePath = Utility.Path.PathCombine(Application.dataPath, assetPath);
            return filePath;
        }
    }

}

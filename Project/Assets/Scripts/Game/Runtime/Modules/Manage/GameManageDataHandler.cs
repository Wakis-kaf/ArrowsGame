using Framework.Runtime.LogSystem;
using Framework.Runtime.MGameModule;
using Framework.Runtime.MLanAndTheme;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleManage
{
    public class InitItemsItem
    {
        public int initItemId;
        public int initItemCount;
    }
    public class GameConfigItem
    {

        public int upGoldNeedStart;

        public int enhanceLuckyStart;

        public int healthMax;

        public float readyTime;

        public int mainPowerCost;
        public int propUnLockLv;

        public float playerMoveSpeed;

        public float mainBgmStatrVolume;

        public List<InitItemsItem> initItems;
    }
    public class CfgGameTable
    {

        public List<GameConfigItem> gameConfig;
    }
    public class GameManageDataHandler : GameConfigDataHandler
    {

        public static GameManageDataHandler Ins => GetModuleHandlerIns<GameManageDataHandler>();
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

        private CfgThemeMap m_ThemeMap;

        public CfgThemeMap LoadThemeMap()
        {
            if (m_ThemeMap != null) return m_ThemeMap;
            if (TryReadConfig<CfgThemeMap>("cfg_themes", out m_ThemeMap))
            {
                Log.Info("读取 cfg_themes 成功");
                return m_ThemeMap;
            }
            Log.Error("读取 cfg_themes 失败");
            return null;
        }
        private CfgGameTable m_GameTable;
        public CfgGameTable GetGameCfgTable()
        {
            if (m_GameTable != null) return m_GameTable;
            if (TryReadConfig<CfgGameTable>("cfg_game", out m_GameTable))
            {
                Log.Info("读取 cfg_game 成功");
                return m_GameTable;
            }
            Log.Error("读取 cfg_game 失败");
            return null;
        }
        public GameConfigItem GetGameMainCfg()
        {
            return GetGameCfgTable().gameConfig[0];
        }
    }
}

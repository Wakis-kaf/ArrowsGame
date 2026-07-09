using Framework.Runtime.LogSystem;
using Framework.Runtime.MGameModule;
using Framework.Runtime.MLanAndTheme;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleManage
{
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
    }
}

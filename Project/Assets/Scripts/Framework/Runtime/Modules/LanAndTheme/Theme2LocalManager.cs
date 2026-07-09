using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Runtime.Module.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.Runtime.MLanAndTheme
{
    public interface IThemeAdapter
    {
        public bool IsEnableTheme { get; set; }

        public ThemeType UseThemeType { get; set; }
        public ThemeType CurrentThemeType { get; set; }

        public string ThemeItemId { get; }

        void SetCurrentThemeType(ThemeType currentThemeType);
    }

    public class Theme2LocalManager
    {
        private List<IThemeAdapter> m_Adapters;
        private CfgThemeMap m_ThemeMap;
        private ThemeType m_CurrentThemeType = ThemeType.None; // 假设你的 ThemeType 中有 Default 或 0 值作为默认
        public void InitThemeMap(CfgThemeMap themeMap)
        {
            m_ThemeMap = themeMap;
        }
        public CfgThemeItem FindThemeItem(ThemeType themeType, string themeName)
        {
            return m_ThemeMap?.FindThemeItem(themeType, themeName);
        }
        public bool TryFindThemeItem(ThemeType themeType, string themeName, out CfgThemeItem themeItem)
        {
            themeItem = FindThemeItem(themeType, themeName);
            return themeItem != null;
        }
        public ThemeType CurrentThemeType
        {
            get { return m_CurrentThemeType; }
        }

        public Theme2LocalManager()
        {
            m_Adapters = new List<IThemeAdapter>(20);
        }

        public void SetCurrentThemeType(ThemeType themeType)
        {
            if (m_CurrentThemeType == themeType) return;
            m_CurrentThemeType = themeType;
            foreach (var adapter in m_Adapters)
            {
                UpdateAdapterThemeType(adapter, CurrentThemeType);
            }
        }

        public void RegisterThemeAdapter(IThemeAdapter adapter)
        {
            if (!adapter.IsEnableTheme) return;
            if (!m_Adapters.Contains(adapter))
            {
                m_Adapters.Add(adapter);
            }
            UpdateAdapterThemeType(adapter, CurrentThemeType);
        }
        private void UpdateAdapterThemeType(IThemeAdapter adapter, ThemeType curThemeType)
        {
            if (adapter.UseThemeType == ThemeType.FollowEnv)
            {
                adapter.SetCurrentThemeType(curThemeType);
            }
            else
            {
                adapter.SetCurrentThemeType(adapter.UseThemeType);
            }
        }

        public void UnRegisterThemeAdapter(IThemeAdapter adapter)
        {
            if (!m_Adapters.Contains(adapter)) return;
            m_Adapters.Remove(adapter);
        }
    }
}
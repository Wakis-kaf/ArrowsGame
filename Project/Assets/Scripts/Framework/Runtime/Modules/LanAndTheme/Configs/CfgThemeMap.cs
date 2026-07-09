using System.Collections;
using System.Collections.Generic;
using Framework.Runtime.LogSystem;
using Sirenix.OdinInspector;
using UnityEngine;
namespace Framework.Runtime.MLanAndTheme
{
    public class CfgThemeMap
    {
        public Dictionary<int, CfgThemeGroup> themeGroups;

        public CfgThemeItem FindThemeItem(ThemeType themeType, string themeName)
        {
            if (themeGroups == null) return null;
            themeName = themeName.Trim();
            if (themeGroups.TryGetValue((int)themeType, out CfgThemeGroup themeGroup))
            {
                return themeGroup.FindThemeItem(themeName);
            }
            Log.Error($"未找到主题配置: themeType = {themeType}, themeName = {themeName}");
            return null;
        }
    }
    public class CfgThemeGroup
    {
        public int themeTypeId;
        public Dictionary<string, CfgThemeItem> themeItems;
        public CfgThemeItem FindThemeItem(string themeName)
        {
            if (themeItems == null) return null;
            themeName = themeName.Trim();
            if (themeItems.TryGetValue(themeName, out CfgThemeItem themeItem))
            {
                return themeItem;
            }
            Log.Error($"未找到主题配置: themeTypeId = {themeTypeId}, themeName = {themeName}");
            return null;
        }
    }
    public class CfgThemeItem
    {
        public string themeItemId;
        public string hexColor;
    }
}
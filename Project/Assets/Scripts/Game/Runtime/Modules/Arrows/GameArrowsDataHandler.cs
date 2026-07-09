using Framework.Runtime;
using Framework.Runtime.LogSystem;
using Framework.Runtime.MAsset;
using Framework.Runtime.MGameModule;
using Framework.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleArrows
{
    public class GameArrowsDataHandler : GameConfigDataHandler
    {

        public static GameArrowsDataHandler Ins => GetModuleHandlerIns<GameArrowsDataHandler>();
        private Dictionary<string, LevelPointPresets> m_LevelPointsLayoutMap = new Dictionary<string, LevelPointPresets>();
        private Dictionary<string, CfgArrowLayout> m_LevelArrowsLayoutMap = new Dictionary<string, CfgArrowLayout>();
        private CfgLevelTable m_LevelTable;
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

        public void GetOrLoadPointsLayout(string layoutType, Vector2 space, Action<LevelPointPresets> onPresetLoaded)
        {
            if (m_LevelPointsLayoutMap.TryGetValue(layoutType, out var preset))
            {
                onPresetLoaded?.Invoke(preset);
                return;
            }
            LoadLevelPointsLayout(layoutType, space, onPresetLoaded);
            // if (preset == null)
            // {
            //     Log.Error($"加载关卡{layoutType}的点布局失败");
            //     onPresetLoaded?.Invoke(null);
            //     return;
            // }
            // m_LevelPointsLayoutMap.Add(layoutType, preset);
        }
        private void LoadLevelPointsLayout(string layoutType, Vector2 space, Action<LevelPointPresets> onPresetLoaded)
        {
            string assetPath = $"Assets/AddressableResources/LevelConfigs/PointLayoutConfigs/{layoutType}.json";
            string assetLink = assetPath.EncodeEnvAssetLink();
            GameApp.AssetManager.LoadAssetAsync(assetLink, (assetVO) =>
            {
                LevelPointPresets preset = null;
                if (assetVO.IsLoadSuccess)
                {
                    TextAsset textAsset = assetVO.GetAsset<TextAsset>();
                    if (textAsset != null)
                    {
                        string configJsonStr = textAsset.text;
                        preset = DecodePointPresetFromJson(layoutType, space, configJsonStr);
                    }
                    assetVO.UnLoadAsync();
                }
                else
                {
                    Log.Error($"加载点布局配置文件失败,布局名称{layoutType}!!");
                }
                m_LevelPointsLayoutMap[layoutType] = preset;
                onPresetLoaded?.Invoke(preset);

            });

        }
        private LevelPointPresets DecodePointPresetFromJson(string layoutType, Vector2 space, string jsonStr)
        {
            LevelPointPresets levelPointPresets = new LevelPointPresets();
            levelPointPresets.pointLayoutName = layoutType;
            CfgPointLayout[] layouts = Utility.Json.ToObject<CfgPointLayout[]>(jsonStr);
            if (layouts == null)
            {
                Log.Error($"解析点布局配置文件失败,布局名称{layoutType}!!");
                return null;
            }
            levelPointPresets.localPositions = new Dictionary<Vector3Int, Vector3>();
            levelPointPresets.colors = new Dictionary<Vector3Int, Vector4>();
            foreach (var item in layouts)
            {
                Vector3Int index = new Vector3Int(item.x, item.y, 0);

                Vector3 position = new Vector3(item.x * space.x, item.y * space.y, 0);
                levelPointPresets.localPositions[index] = position;
                levelPointPresets.colors[index] = new Vector4(item.r, item.g, item.b, item.a);
            }
            return levelPointPresets;
        }

        public CfgLevel GetLevelConfig(int levelId)
        {
            CfgLevelTable table = GetCfgLevelTable();
            if (table == null) return null;
            if (table.levelsCfg.TryGetValue(levelId, out var level))
            {
                return level;
            }
            Log.Error($"关卡{levelId}的配置不存在,使用默认配置");
            return table.levelsCfg[-1];
        }
        public CfgLevelTable GetCfgLevelTable()
        {
            if (m_LevelTable != null) return m_LevelTable;
            if (TryReadConfig<CfgLevelTable>("cfg_level", out m_LevelTable))
            {
                Log.Info("读取 cfg_level 成功");
                return m_LevelTable;
            }
            Log.Error("读取 cfg_level 失败");
            return null;
        }

        public LevelArrowsPresure LoadLevelArrowsPresure(int presureArgId)
        {
            var levelTable = GetCfgLevelTable();
            if (levelTable == null) return null;
            if (levelTable.arrowsPresureArgsCfg.TryGetValue(presureArgId, out var presure))
            {
                return presure;
            }
            Log.Error($"关卡{presureArgId}的配置不存在");
            return null;
        }
        public CfgLevelAnimArgs GetLevelAnimArgs(int levelId, bool getDefault = true, bool defaultWithAnim = true)
        {
            var levelTable = GetCfgLevelTable();
            if (levelTable == null) return null;
            if (levelTable.levelsAnimArgsCfg.TryGetValue(levelId, out var animArgs))
            {
                return animArgs;
            }
            if (getDefault)
            {
                levelId = defaultWithAnim ? 0 : -1;
            }
            if (levelTable.levelsAnimArgsCfg.TryGetValue(levelId, out animArgs))
            {
                return animArgs;
            }
            Log.Error($"关卡{levelId}的配置不存在");
            return null;
        }

        public void GerOrLoadArrowsLayout(string arrowsLayoutName, Action<string, CfgArrowLayout> onLevelArrowLayoutLoaded)
        {
            if (m_LevelArrowsLayoutMap.TryGetValue(arrowsLayoutName, out var arrowsLayout))
            {
                onLevelArrowLayoutLoaded?.Invoke(arrowsLayoutName, arrowsLayout);
                return;
            }
            LoadLevelArrowsLayout(arrowsLayoutName, onLevelArrowLayoutLoaded);
        }
        private void LoadLevelArrowsLayout(string arrowsLayoutName, Action<string, CfgArrowLayout> onLevelArrowLayoutLoaded)
        {
            string assetPath = $"Assets/AddressableResources/LevelConfigs/LevelArrowConfigs/{arrowsLayoutName}.json";
            string assetLink = assetPath.EncodeEnvAssetLink();
            GameApp.AssetManager.LoadAssetAsync(assetLink, (assetVO) =>
            {
                CfgArrowLayout arrowLayout = null;
                if (assetVO.IsLoadSuccess)
                {
                    TextAsset textAsset = assetVO.GetAsset<TextAsset>();
                    if (textAsset != null)
                    {
                        string configJsonStr = textAsset.text;
                        arrowLayout = DecodeArrowsLayoutFromJson(arrowsLayoutName, configJsonStr);
                    }
                    assetVO.UnLoadAsync();
                }
                else
                {
                    Log.Error($"加载线条布局配置文件失败,布局名称{arrowsLayoutName}!!");
                }
                m_LevelArrowsLayoutMap[arrowsLayoutName] = arrowLayout;
                onLevelArrowLayoutLoaded?.Invoke(arrowsLayoutName, arrowLayout);

            });

        }
        private CfgArrowLayout DecodeArrowsLayoutFromJson(string arrowsLayoutName, string jsonStr)
        {

            CfgArrowLayout arrowLayout = Utility.Json.ToObject<CfgArrowLayout>(jsonStr);
            if (arrowLayout == null)
            {
                Log.Error($"解析线条布局配置文件失败,布局名称{arrowsLayoutName}!!");
            }
            return arrowLayout;
        }
    }
}

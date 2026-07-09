using Cysharp.Threading.Tasks;
using Framework.Runtime.LogSystem;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game.Modules.GModuleSceneUnit
{
    public class GameSceneUnitDataHandler : GameConfigDataHandler
    {
        private CfgSceneItemTable m_CfgSceneItemTable;
        public static GameSceneUnitDataHandler Ins => GetModuleHandlerIns<GameSceneUnitDataHandler>();

        public CfgBuildingArg GetCfgBuildingArg(int sceneId)
        {
            var table = GetCfgSceneItemTable();
            if (table == null) { return null; }
            if (table.CfgBuildingArgMap == null)
            {
                table.CfgBuildingArgMap = new Dictionary<int, CfgBuildingArg>();
                for (int i = 0; i < table.buildingItemArgs.Count; i++)
                {
                    table.CfgBuildingArgMap.Add(table.buildingItemArgs[i].id, table.buildingItemArgs[i]);
                }
            }

            if (table.CfgBuildingArgMap.ContainsKey(sceneId))
            {
                return table.CfgBuildingArgMap[sceneId];
            }
            return null;
        }
        public CfgMonsterArg GetCfgMonsterArg(int sceneId)
        {
            var table = GetCfgSceneItemTable();
            if (table == null) { return null; }
            if (table.CfgMonsterArgMap == null)
            {
                table.CfgMonsterArgMap = new Dictionary<int, CfgMonsterArg>();
                for (int i = 0; i < table.monsterItemArgs.Count; i++)
                {
                    table.CfgMonsterArgMap.Add(table.monsterItemArgs[i].id, table.monsterItemArgs[i]);
                }
            }

            if (table.CfgMonsterArgMap.ContainsKey(sceneId))
            {
                return table.CfgMonsterArgMap[sceneId];
            }
            return null;
        }

        public CfgSceneItemInfo GetCfgSceneItemInfo(string sceneName)
        {
            var table = GetCfgSceneItemTable();
            if (table == null) { return null; }
            for (int i = 0; i < table.sceneItemCfg.Count; i++)
            {
                var sceneItemCfg = table.sceneItemCfg[i];
                if (sceneItemCfg.name == sceneName)
                {
                    return sceneItemCfg;
                }
            }
            return null;
        }

        public CfgSceneItemInfo GetCfgSceneItemInfo(int sceneId)
        {
            var table = GetCfgSceneItemTable();
            if (table == null) { return null; }
            for (int i = 0; i < table.sceneItemCfg.Count; i++)
            {
                var sceneItemCfg = table.sceneItemCfg[i];
                if (sceneItemCfg.id == sceneId)
                {
                    return sceneItemCfg;
                }
            }
            return null;
        }

        public CfgSceneItemTable GetCfgSceneItemTable()
        {
            if (m_CfgSceneItemTable != null) return m_CfgSceneItemTable;
            if (TryReadConfig<CfgSceneItemTable>("cfg_sceneItem", out m_CfgSceneItemTable))
            {
                Log.Info("读取cfg_sceneItem成功");
                return m_CfgSceneItemTable;
            }
            Log.Error("读取cfg_sceneItem失败");
            return null;
        }
        protected override void OnCheckHandlerLoad()
        {
            var tbl = GetCfgSceneItemTable();
            OnHandlerLoaded();
        }
        protected override void OnHandlerAwake()
        {
        }

        protected override void OnHandlerDestroy()
        {
        }

        protected override void OnHandlerEnable()
        {
        }

        protected override void OnHandlerStart()
        {
        }
    }
}
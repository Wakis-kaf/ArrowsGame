using CustomLitJson.Extensions;
using Game.Modules.GModuleScene;
using System.Collections.Generic;

namespace Game.Modules.GModuleSceneUnit
{
    public static class ArgParamType
    {
        public const int Context = 0;
        public const int Dynamic_NumBaseRoot = 20101;
        public const int NumAttr_BaseRoot = 10101;
        public const int NumAttr_RoleAdd = 10202;
        public const int NumAttr_BuffAdd = 10402;
        public const int NumAttr_BuffPercent = 10403;
    }

    public class CfgBuildingArg
    {
        public int id;
        public string name;
        public string skillName;
        public bool isShowUnlock;
        public bool isMovEle;
        public int movOrder;
        public bool isShowAttr;
        public bool pauseInAttrShow;
        public bool disactiveShow;
        public CfgEnhance[] enhCfg;


        public CfgLevel[] lvCfg;





        public CfgEnhance GetEnhanceCfgByLevel(int enhanceLevel)
        {
            for (int i = 0; i < enhCfg.Length; i++)
            {
                if (enhCfg[i].enhLv == enhanceLevel)
                {
                    return enhCfg[i];
                }
            }
            return null;
        }

        public CfgLevel GetLevelCfgByLevel(int level)
        {
            for (int i = 0; i < lvCfg.Length; i++)
            {
                if (lvCfg[i].lv == level)
                {
                    return lvCfg[i];
                }
            }
            return null;
        }

        public int GetMaxLevel()
        {
            int level = -1;
            for (int i = 0; i < lvCfg.Length; i++)
            {
                if (lvCfg[i].lv > level)
                {
                    level = lvCfg[i].lv;
                }
            }
            return level;
        }
    }

    public class CfgMonsterArg
    {
        public bool isShowHp;
        public string barOffset;
        public CfgLevel[] lvCfg;
        public int id;
        public string name;
        public string skillName;

        public CfgLevel GetLevelCfgByLevel(int level)
        {
            for (int i = 0; i < lvCfg.Length; i++)
            {
                if (lvCfg[i].lv == level)
                {
                    return lvCfg[i];
                }
            }
            return null;
        }
    }

    public class CfgSceneItemInfo
    {
        public int id;
        public int type;
        public string name;
        public string clsType;
        public string modelSize;
        public string znName;
        public string pbName;
        public string pbPath;
        public string desc;
        public string iconPath;
        public float modelScale;
    }

    public class CfgSceneItemTable
    {


        [JsonIgnore]
        public Dictionary<int, CfgBuildingArg> CfgBuildingArgMap;
        [JsonIgnore]
        public Dictionary<int, CfgMonsterArg> CfgMonsterArgMap;
        public List<CfgSceneItemInfo> sceneItemCfg;
        public Dictionary<int, CfgUnitBookInfo> cfgUnitBookMap;
        public List<CfgBuildingArg> buildingItemArgs;
        public List<CfgMonsterArg> monsterItemArgs;

    }
    public class CfgUnitBookInfo
    {
        public int id;
        public float coverModelScale;
        public string coverModelPos;
        public float infoModelScale;
        public string infoModelPos;
        // public CfgReward[] unlockRewards;
    }

    public class CfgEnhance
    {
        public CfgArgs[] args;
        public int enhLv;
    }

    public class CfgLevel
    {
        public CfgArgs[] args;
        public int qual;
        public int lv;
        public float minHit;
        public float maxHit;
        public float doorPctDamage;
        public T GetData<T>(string key, T defaultValue = default)
        {
            foreach (var item in args)
            {
                if (item.argKey == key)
                {
                    return item.GetData<T>();
                }
            }
            return defaultValue;
        }
    }
}
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Game.Modules.GModuleArrows
{
    public class CfgPointLayout
    {
        public int x;
        public int y;
        // public float pX;
        // public float pY;
        public int r;
        public int g;
        public int b;
        public int a;
    }
    public class CfgLevelTable
    {
        public Dictionary<int, CfgLevel> levelsCfg;
        public Dictionary<int, LevelArrowsPresure> arrowsPresureArgsCfg;
        public Dictionary<int, CfgLevelAnimArgs> levelsAnimArgsCfg;
    }
    public class CfgArrowLayout
    {
        public List<LevelArrowNode> arrowNodes;
        public LevelArrowsPresure presureArg;

    }
    public class CfgLevel
    {
        public string pointLayoutName;
        public string arrowsLayoutName;
        public int arrowsLayoutGenerateType;
        public int arrowsGenerateArgId;
        public int customSeed;
        public float pointSpaceX;
        public float pointSpaceY;
        public bool isHideUnOccupiedPoint;
        public bool isPointColorful;
    }
    public class CfgLevelAnimArgs
    {
        public float pointEntryAnimTotalTime = 1f;
        public float arrowEntryAnimTotalTime = 0.5f;
        public bool usePointDelay = true;
        public float worldPointDetectRadius = 0.8f;
        public float worldPointDetectInnerRadius = 0.25f;
        public float arrowPointOccupiedRadius = 0.1f;
        public float arrowPointUnOccupiedRadius = 0.2f;
        public float arrowPointSuccessRadius = 0.3f;
    }
}
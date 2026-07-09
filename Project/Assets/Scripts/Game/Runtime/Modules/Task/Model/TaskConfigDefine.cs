using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Modules.GModuleTask
{
    public class CfgGameTaskTable
    {
        public List<CfgGameTaskChain> tasksCfg;
    }
    public class CfgTaskNode
    {
        public int taskId;
        public string taskName;
        public string taskSummary;
        public string taskDescription;
        public string taskTarget;
        public string[] openChains;
        public string taskParams;
    }
    public class CfgGameTaskChain
    {
        public int taskChainId;
        public string taskChainName;
        public List<CfgTaskNode> taskNodes;
    }
}


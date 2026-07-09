using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Modules.GModuleGuid
{
    public class CfgGuide
    {
        public List<CfgGuideNode> guideNodes;
        public Dictionary<string,CfgGuideTask> taskEntryTrigger;
        public Dictionary<string,CfgGuideTask> taskExitTrigger;
        public Dictionary<string,CfgGuideTask> taskStayTrigger;
        public Dictionary<string,CfgGuideTask> taskDoneTrigger;
        //public List<CfgGuideTask> taskTriggers = new List<CfgGuideTask>();
        //public List<CfgGuideTask> taskExitTriggerConditions = new List<CfgGuideTask>();
        //public List<CfgGuideTask> taskExitTriggers = new List<CfgGuideTask>();

        //public List<CfgGuideTask> taskStayTriggerConditions = new List<CfgGuideTask>();
        //public List<CfgGuideTask> taskStayTriggers = new List<CfgGuideTask>();
        //public List<CfgGuideTask> taskDoneTriggerConditions = new List<CfgGuideTask>();
        //public List<CfgGuideTask> taskDoneTriggers = new List<CfgGuideTask>();
    }
   
    public class CfgGuideNode
    {
        // 引导的id;
        public int guideId;
        //只引导一次
        public bool guideOne;
        // 引导的名称
        public string guideName;
        // 是否强制引导
        public string guidType;
        // 后置引导id;
        public int postId;
        // 引导优先级
        public int priority;
        // 中断后重新触发的子进度索引;
        public int restartSubIndex;
        // 保存进度的子进度索引;
        public bool saveSubIndex;
        // 存出的子引导任务;
        public List<CfgSubTask> subTaskList;
    }
    public class CfgSubTask
    {
        public string taskName;
        public bool saveIndex;

    }
    public class CfgGuideTask
    {
        // 引导名称
        public string taskName;
        public List<CfgGuideTrigger> triggers;
      

    }
    public class CfgGuideTrigger
    {
        public int triggerIndex;
        public CfgGuideData[] conditions;
        public CfgGuideData[] handlers;
    }
    //public class CfgGuideTriggerHandler
    //{
    //    public string handlerType;
    //    public CfgArgs[] handlerArgs;
    //}
    //public class CfgGuideTriggerCondition
    //{
    //    public string conditionType;
    //    public CfgArgs[] condArgs;
    //}
    public class CfgGuideData
    {
        public string type;
        public List<CfgArgs> dataParams;
        public string GetArgVal(string argKey)
        {
            var arg = FindArg(argKey);
            if (arg != null)
            {
                return arg.argVal;
            }
            return "";
        }
      
        public T GetParam<T>(string argKey, T defaultValue = default)
        {
            var arg = FindArg(argKey);
            if (arg == null) return defaultValue;
            return DataManager.ParseStringToType<T>(arg.argVal, defaultValue);
        }
        public CfgArgs FindArg(string argKey)
        {
            for (int i = 0; i < dataParams.Count; i++)
            {
                if (string.Compare( dataParams[i].argKey , argKey,true) == 0)
                {
                    return dataParams[i];
                }
            }
            return null;
        }
        
    }
    //public class GuideParams
    //{
    //    public string paramName;
    //    public string paramType;
    //    public string stringValue;
    //    public bool boolValue;
    //    public string doubleValue;
    //    public int intValue;
    //    public string[] stringArrayValue;
    //    public string[][] stringArray2dValue;
    //    public int[] intArrayValue;
    //    public int[][] intArray2dValue;
    //    public double[] doubleArrayValue;
    //    public double[][] doubleArray2dValue;
    //}
}

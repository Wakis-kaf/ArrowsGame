
using System;
using System.Collections.Generic;


namespace Game.Modules.GModuleGuid
{
    public delegate bool GuideConditionAgent(GuideCondition condition, CfgGuideData guideData);
    public delegate bool GuideTriggerAgent(GuideHandler trigger, CfgGuideData guideData);
    public struct GuideTriggerReference
    {
        public GuideTriggerAgent OnTrigger;
        //public GuideTriggerAgent OnStaying;
        //public GuideTriggerAgent OnDone;
    }
    public static class GuideFactoryTable
    {
        /// <summary>
        /// 注意Type需要继承GuideTrigger
        /// </summary>
        public static Dictionary<string, Type> TriggerFactoryTable = new Dictionary<string, Type>
        {

        };
        /// <summary>
        /// 注意Type需要继承GuideCondition
        /// </summary>
        public static Dictionary<string, Type> ConditionFactoryTable = new Dictionary<string, Type>
        {

        };
        public static void Clear()
        {
            m_Type2TriggerReference.Clear();
            m_Type2ConditionAgentMap.Clear();
        }
        private static Dictionary<string, GuideTriggerAgent> m_Type2TriggerReference = new Dictionary<string, GuideTriggerAgent>();
        private static Dictionary<string, GuideConditionAgent> m_Type2ConditionAgentMap = new Dictionary<string, GuideConditionAgent>();
        public static bool TryGetGuideTriggerReference(string type,out GuideTriggerAgent guideTriggerReference)
        {
            return m_Type2TriggerReference.TryGetValue(type, out guideTriggerReference);
        }
        public static void RegisterGuideConditionType<T>(string typeName) where T : GuideCondition
        {
            Type type = typeof(T);
            if (ConditionFactoryTable.ContainsKey(typeName))
            {
                ConditionFactoryTable[typeName] = type;
            }
            else
            {
                ConditionFactoryTable.Add(typeName, type);
            }
        }
        public static void RegisterGuideTriggerType<T>(string typeName) where T : GuideHandler
        {
            Type type = typeof(T);
            if (TriggerFactoryTable.ContainsKey(typeName))
            {
                TriggerFactoryTable[typeName] = type;
            }
            else
            {
                TriggerFactoryTable.Add(typeName, type);
            }
        }
        public static void RegisterGuideTriggerReference(string type, GuideTriggerAgent guideTriggerReference)
        {
            if(m_Type2TriggerReference.ContainsKey(type))
            {
                m_Type2TriggerReference.Add(type, guideTriggerReference);
            }
            else
            {
                m_Type2TriggerReference[type] = guideTriggerReference;
            }
        }
        public static bool TryGetGuideConditionAgent(string type, out GuideConditionAgent guideConditionAgent)
        {
            return m_Type2ConditionAgentMap.TryGetValue(type, out guideConditionAgent);
        }
        public static void RegisterGuideConditionAgent(string type, GuideConditionAgent agent)
        {
            if (m_Type2ConditionAgentMap.ContainsKey(type))
            {
                m_Type2ConditionAgentMap.Add(type, agent);
            }
            else
            {
                m_Type2ConditionAgentMap[type] = agent;
            }
        }
        static GuideFactoryTable()
        {
            //RegisterGuideConditionAgent("Cond_ReturnTrue", Cond_ReturnTrue);
            ////RegisterGuideConditionAgent("Cond_PlayerNotSleep", Cond_PlayerNotSleep);
            //RegisterGuideConditionAgent("Cond_IsSceneLoded", Cond_IsSceneLoded);
            //RegisterGuideConditionAgent("Cond_IsGuidShow", Cond_IsGuidShow);
            //RegisterGuideConditionAgent("Cond_IsInputZero", Cond_IsInputZero);
            //RegisterGuideConditionAgent("Cond_IsPlayerNotSleep", Cond_IsPlayerNotSleep);
            //RegisterGuideConditionAgent("Cond_PlayerNearBuild", Cond_PlayerNearBuild);
            //RegisterGuideConditionAgent("Cond_ItemEnough", Cond_ItemEnough);
            //RegisterGuideConditionAgent("Cond_IsNotLvUping", Cond_IsNotLvUping);



            //RegisterGuideTriggerReference("Trig_ShowDialog", Trig_ShowDialog);
            //RegisterGuideTriggerReference("Trig_ShowHighlight", Trig_ShowHighlight);
            //RegisterGuideTriggerReference("Trig_CloseGuide", Trig_CloseGuide);
            //RegisterGuideTriggerReference("Trig_GamePause", Trig_GamePause);
            //RegisterGuideTriggerReference("Trig_GameResume", Trig_GameResume);
            //RegisterGuideTriggerReference("Trig_ShowInputGuide", Trig_ShowInputGuide);
            //RegisterGuideTriggerReference("Trig_EntryGuideChapter", Trig_EntryGuideChapter);
            //RegisterGuideTriggerReference("Trig_GuideSleep", Trig_EntryDialog);


        }

    
       
        

      


    }
}

using BehaviorDesigner.Runtime.Tasks.Unity.UnityAnimator;
using Framework.Runtime;
using Framework.Runtime.LogSystem;
using Framework.Runtime.MGameModule;
using Framework.Runtime.UI;
using Framework.Runtime.UnitSystem.Base;
using Framework.Runtime.UnitSystem.BIInterfaces;
using Framework.Utils;

using Game.Modules.GModuleInventory;
using Game.Modules.GModuleTip;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleGuid
{
    public class GameGuideClientHandler : GameModuleLogicHandler,IUnitUpdate
    {
        
        private GuideMachine m_GuideMachine;
        public GuideMachine GuideMachine => m_GuideMachine;
        public static GameGuideClientHandler Ins => GetModuleHandlerIns<GameGuideClientHandler>();
        private HashSet<string> m_EvtRecevied;
        protected override void OnHandlerAwake()
        {
            m_GuideMachine = new GuideMachine();
        }
        protected override void OnHandlerStart()
        {
            m_EvtRecevied = new HashSet<string>();
            MessageDispatcher.Ins.OnMsgDispatch+=OnMsgDispatch;
            InitGuideConfig();
            InitGuideTable();
        }

        private void OnMsgDispatch(string msgCode)
        {
            m_EvtRecevied.Add(msgCode);
        }

        private void InitGuideConfig()
        {
            CfgGuide cfgGuide = GetModuleInHandler<GameGuideDataHandler>().GetCfgGuide();
            if (cfgGuide == null) return;
            CommonConditionAgents.InjectAgents();
            CommonTriggerAgents.InjectAgents();
            //m_GuideMachine.InitMachineByConfig(cfgGuide,this);
        }
        protected override void OnHandlerDestroy()
        {
            
        }

        public void OnUnitUpdate()
        {
            m_GuideMachine.UpdateMachine();
            m_EvtRecevied?.Clear();
        }
        private void InitGuideTable()
        {
            GuideFactoryTable.Clear();
            //GuideFactoryTable.RegisterGuideConditionAgent("Cond_ReturnTrue", Cond_ReturnTrue);
            //GuideFactoryTable.RegisterGuideTriggerReference("Trig_ShowDialog", Trig_ShowDialog);

        }

       
    }

}

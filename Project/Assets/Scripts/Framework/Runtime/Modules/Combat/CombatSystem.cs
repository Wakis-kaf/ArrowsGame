using Framework.Runtime.Module.Core;
using Framework.Runtime.UnitSystem.BIInterfaces;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Framework.Runtime.MCombat
{
    public enum EffectTagType
    {
        CombatorStatusTag,
        CombatorActionTag
    }
    public struct EffectTag
    {
        public string tagName;
        public EffectTagType effectTagType;
    }

    public class CombatSystem : ModuleUnit, IUnitUpdate
    {
        private List<Combator> combators = new List<Combator>();
        private Queue<CombatEvent> eventQueue = new Queue<CombatEvent>();
        private List<ICombatEventReceiver> m_GlobalEventReceivers = new List<ICombatEventReceiver>();
        private CombatEffectManager m_CombatEffectManager;
        public CombatEventPool CombateEventPool { get; private set; }
        public CombatProtoPool CombatProtoPool { get; private set; }
        public LoopManager LoopManager { get; private set; }
        public CombatSystem()
        {
            m_CombatEffectManager = new CombatEffectManager();
            CombateEventPool = new CombatEventPool();
            CombatProtoPool = new CombatProtoPool();
            LoopManager = new LoopManager();
        }

        public static CombatSystem Ins => GameApp.CombatSystem;
        protected override void OnModuleConstructed()
        {
            base.OnModuleConstructed();
            LoopManager.Init();
        }
        public void Start()
        {
            LoopManager.Start();
        }
        public void Stop()
        {
            ClearAll();
            LoopManager.Stop();
            
        }


        public CombatEvent CreateCombatEvent(ICombatEventReceiver sender)
        {
            var cbEvent = CombateEventPool.GetCombatEvent();
            cbEvent.SetSender(sender);
            return cbEvent;
        }

        public Combator CreateCombator()
        {
            Combator combator = new Combator();
            combators.Add(combator);
            return combator;
        }

        public CombatEvent DispatchEvent(string code, ICombatEventReceiver sender)
        {
            var cbEvent = CreateCombatEvent(sender);
            cbEvent.SetCode(code);
            return DispatchEvent(cbEvent);
        }

        public CombatEvent DispatchEvent(CombatEvent combatEvent)
        {
            combatEvent.needClear = false;
            eventQueue.Enqueue(combatEvent);
            return combatEvent;
        }
        public void DispatchEventSync(CombatEvent combatEvent)
        {
            combatEvent.needClear = false;
            HandleEventSync(combatEvent);
            return ;
        }

        public CombatEvent HandleEvent(CombatEvent combatEvent)
        {
            return combatEvent;
        }

        public CombatEvent ReceiveEvent(CombatEvent combatEvent)
        {
            return combatEvent;
        }

       
        private void HandleTag()
        {
            for (int i = 0; i < combators.Count; i++)
            {
                var tagCount = combators[i].TagCount;
                for (int j = 0; j < tagCount; j++)
                {
                    EffectTag tag = combators[i].GetTagAt(j);
                    HnadleTag(combators[i], tag);
                }
                combators[i].CheckEffect();
            }
        }
        private void HnadleTag(Combator combator,EffectTag effectTag)
        {
            m_CombatEffectManager.CheckEffect(combator, effectTag);
        }
        private void HandleEventSync(CombatEvent evt)
        {
            evt.needClear = true;
            if (evt.isGlobal)
            {
                for (int i = 0; i < m_GlobalEventReceivers.Count; i++)
                {
                    if (m_GlobalEventReceivers[i].IsActive())
                        m_GlobalEventReceivers[i].ReceiveEvent(evt);
                }
            }

            // 处理私有
            evt.SubscribeReceive();

            if (evt.needBackCall)
            {
                // 事件回传
                evt.CallBack();
            }

            if (evt.needClear)
            {
                evt.Dispose();
            }
        }
        private void HandleEvents()
        {
            while (eventQueue.Count > 0)
            {
                var evt = eventQueue.Dequeue();
                HandleEventSync(evt);
            }
        }
        private void UpdateCombators()
        {
            for (int i = 0; i < combators.Count; i++)
            {
                combators[i].Update();
            }
        }

        public void OnUnitUpdate()
        {
            UpdateCombators();
            HandleEvents();
            HandleTag();
        }
        public void ClearAll()
        {
            LoopManager.ClearAll();
            while (eventQueue.Count>0)
            {
                var evt = eventQueue.Dequeue();
                evt.Dispose();
            }
            m_GlobalEventReceivers.Clear();
            for (int i = combators.Count-1; i >= 0; i--)
            {
                DisposeCombator(combators[i]);
            }
            
        }
        public void DisposeCombator(Combator combator)
        {
            if (combator == null || !combators.Contains(combator)) return;
            combator.ClearAll();
            combators.Remove(combator);
            combator.Dispose();
        }

    
    }
}
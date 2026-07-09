using Framework.Runtime.Base;
using System;

using System.Collections.Generic;

namespace Framework.Runtime.MCombat
{
    public class Combator : UnitObject, ICombatEventReceiver, ICombatEventDispatcher
    {
        private List<Ability> m_ChildAbilities = new List<Ability>();
        private Action<CombatEvent> m_EventReceiveListeners;
        private List<Ability> m_MoutAbilities = new List<Ability>();
        private List<ICombatEventReceiver> m_MoutRecivers = new List<ICombatEventReceiver>();
        private List<EffectTag> m_Tags = new List<EffectTag>();
        private CombatEvent m_TickCombatEvent;

        // 挂载的事件接收
        private Dictionary<int, List<AbilityLink>> m_Type2Linkes = new Dictionary<int, List<AbilityLink>>();

        public Combator()
        {
            Context = new DataManager();
            AttributeBox = new AttributeBox();
            CombatEffectManager = new CombatEffectManager();
            m_TickCombatEvent = CreateEvent(CombatCode.OnAbilityUpdate);
            m_TickCombatEvent.needBackCall = false;
        }

        public bool Active { get; set; } = true;
        public AttributeBox AttributeBox { get; private set; }
        public CombatEffectManager CombatEffectManager { get; private set; }
        public DataManager Context { get; private set; }
        public int TagCount => m_Tags.Count;

        public void ClearAll()
        {
            //m_ChildAbilities.Clear();
            //m_MoutAbilities.Clear();
            ClearMoutedAbilites();
            ClearChildAbilities();
            m_MoutRecivers.Clear();
            m_Tags.Clear();
            AttributeBox.ClearAll();
            CombatEffectManager.ClearAll();
            Context.ClearAll();
            Active = true;
            m_EventReceiveListeners = null;
        }
        public void AddEventReceiveListener(Action<CombatEvent> cb)
        {
            m_EventReceiveListeners += cb;
         
        }

        public void BirthAbility(Ability ability)
        {
            if (!m_ChildAbilities.Contains(ability))
            {
                m_ChildAbilities.Add(ability);
                ability.BindMaster(this);
            }
        }

        public void CheckEffect()
        {
            for (int j = 0; j < m_Tags.Count; j++)
            {
                CombatEffectManager.CheckEffect(this, m_Tags[j]);
            }
        }
        public void ClearChildAbilities()
        {
            for (int i = 0; i < m_ChildAbilities.Count; i++)
            {
                m_ChildAbilities[i].TryDestroySync();
            }
            m_ChildAbilities.Clear();
      
        }
        public void ClearMoutedAbilites()
        {
            // 销毁所有挂载的能力
            for (int i = 0; i < m_MoutAbilities.Count; i++)
            {
                m_MoutAbilities[i].TryDestroySync();
            }
            m_MoutAbilities.Clear();
        }

        public CombatEvent CreateEvent(string code)
        {
            CombatEvent combatEvent = CombatSystem.Ins.CreateCombatEvent(this);
            combatEvent.SetCode(code);
            combatEvent.needBackCall = true; // 需要回传
            //combatEvent.SetContext(EventContextName.SendComabtor, this);
            return combatEvent;
        }

        public void DegisterLink(AbilityLink abilityLink)
        {
            if (m_Type2Linkes.TryGetValue(abilityLink.LinkType, out var list))
            {
                list.Remove(abilityLink);
            }
        }

        public void DegisterLinks(List<AbilityLink> abilityLinks)
        {
            for (int i = 0; i < abilityLinks.Count; i++)
            {
                DegisterLink(abilityLinks[i]);
            }
        }

        public string GetAbilityDebugInfo()
        {
            string info = $"当前子特性数量{m_MoutAbilities.Count}";
            for (int i = 0; i < m_MoutAbilities.Count; i++)
            {
                info += m_MoutAbilities[i].GetDebugInfo();
            }
            return info;
        }

        public List<AbilityLink> GetAbilityLinksByType(int type)
        {
            if (m_Type2Linkes.TryGetValue(type, out var list))
            {
                return new List<AbilityLink>(list);
            }

            return new List<AbilityLink>();
        }

        public EffectTag GetTagAt(int index)
        {
            if (index < m_Tags.Count) return m_Tags[index];
            return default;
        }

        public CombatEvent HandleEvent(CombatEvent combatEvent)
        {
            for (int i = 0; i < m_MoutRecivers.Count; i++)
            {
                if (!m_MoutRecivers[i].IsActive()) continue;
                m_MoutRecivers[i].HandleEvent(combatEvent);
            }
            return combatEvent;
        }

        public bool HasMoutAbility(Type ability)
        {
            Type type = ability.GetType();
            for (int i = 0; i < m_MoutAbilities.Count; i++)
            {
                if (!m_MoutRecivers[i].IsEnabled()) continue;
                var childType = m_MoutAbilities[i].GetType();
                if (childType == type || childType.IsSubclassOf(type))
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasMoutAbility(Ability ability)
        {
            return m_MoutAbilities.Contains(ability);
        }

        public bool IsActive()
        {
            return Active && IsEnabled();
        }
        public bool IsEnabled()
        {
            return !IsDisposed;
        }
        public void MoutAbility(Ability ability)
        {
            if (!m_MoutAbilities.Contains(ability))
            {
                m_MoutAbilities.Add(ability);
                ability.BindMouter(this);
            }
        }

        public void MoutReceiver(ICombatEventReceiver receiver)
        {
            if (!m_MoutRecivers.Contains(receiver))
            {
                m_MoutRecivers.Add(receiver);
            }
        }

        public CombatEvent ReceiveEvent(CombatEvent combatEvent)
        {
            for (int i = 0; i < m_MoutRecivers.Count; i++)
            {
                if (m_MoutRecivers[i].IsActive())
                {
                    m_MoutRecivers[i].ReceiveEvent(combatEvent);
                }
            }
            m_EventReceiveListeners?.Invoke(combatEvent);
            return combatEvent;
        }

        public void RegisterLink(AbilityLink abilityLink)
        {
            if (m_Type2Linkes.TryGetValue(abilityLink.LinkType, out var list))
            {
                list.Add(abilityLink);
            }
            else
            {
                m_Type2Linkes.Add(abilityLink.LinkType, new List<AbilityLink>() { abilityLink });
            }
        }

        public void RegisterLinks(List<AbilityLink> abilityLinks)
        {
            for (int i = 0; i < abilityLinks.Count; i++)
            {
                RegisterLink(abilityLinks[i]);
            }
        }

        public void RemoveChildAbility(Ability ability)
        {
            //m_ChildAbilities.Remove(ability);
        }

        public void RemoveEventReceiveListener(Action<CombatEvent> cb)
        {
            m_EventReceiveListeners -= cb;
        }

        public void RemoveMoutAbility(Ability ability)
        {
            //if (m_MoutAbilities.Contains(ability))
            //{
            //    m_MoutAbilities.Remove(ability);
            //}
            // 如果当前是在死亡销毁期间
        }

        public void RemoveMoutedReceiver(ICombatEventReceiver receiver)
        {
            //m_MoutRecivers.Remove(receiver);
        }

        public CombatEvent SendEvent(CombatEvent combatEvent)
        {
            CombatSystem.Ins.DispatchEvent(combatEvent);
            return combatEvent;
        }

        public CombatEvent SendEventSelf(string evt)
        {
            var combatEvent = CreateEvent(evt);
            combatEvent.AddTarget(this);
            return SendEvent(combatEvent);
        }

        public virtual CombatEvent SendEventToTarget(string code, ICombatEventReceiver target)
        {
            CombatEvent combatEvent = CreateEvent(code);
            combatEvent.AddTarget(target);
            return SendEvent(combatEvent);
        }

        public virtual CombatEvent SendEventToTargets(string code, IEnumerable<ICombatEventReceiver> targets)
        {
            CombatEvent combatEvent = CreateEvent(code);
            foreach (var target in targets)
            {
                combatEvent.AddTarget(target);
            }
            return SendEvent(combatEvent);
        }

        public void SetEventReceiveListener(Action<CombatEvent> cb)
        {
            this.m_EventReceiveListeners = cb;
        }

        public void TryEnableAllChildAbility()
        {
            for (int i = 0; i < m_ChildAbilities.Count; i++)
            {
                if (!m_ChildAbilities[i].IsEnabled()) continue;
                m_ChildAbilities[i].TryEnable();
            }
        }

        public bool TryGetMoutAbility<T>(out T result) where T : Ability
        {
            for (int i = 0; i < m_MoutAbilities.Count; i++)
            {
                if (m_MoutAbilities[i] is T tRes && tRes.IsEnabled())
                {
                    result = tRes;
                    return true;
                }
            }
            result = null;
            return false;
        }

        public bool TryGetMoutAbility(Type ability, out Ability result)
        {
            Type type = ability.GetType();
            for (int i = 0; i < m_MoutAbilities.Count; i++)
            {
                if (!m_MoutAbilities[i].IsEnabled()) continue;
                var childType = m_MoutAbilities[i].GetType();
                if (childType == type || childType.IsSubclassOf(type))
                {
                    result = m_MoutAbilities[i];
                    return true;
                }
            }
            result = null;
            return false;
        }

        public void Update()
        {
            TickUpdate();
            m_TickCombatEvent.ResetError();
        }

        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            Context.ClearAll();
            AttributeBox.ClearAll();
            CombatEffectManager.ClearAll();
            DisposeAbilities(m_ChildAbilities);
            DisposeAbilities(m_MoutAbilities);
            m_ChildAbilities.Clear();
            m_MoutAbilities.Clear();
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            m_Type2Linkes.Clear();
            m_TickCombatEvent = null;
            m_MoutRecivers.Clear();
            m_Tags.Clear();
        }

        private void DisposeAbilities(List<Ability> abilities)
        {
            for (int i = 0; i < abilities.Count; i++)
            {
                abilities[i].Dispose();
            }
        }

        private void TickUpdate()
        {
            UpdateMouters();
            HandleEvent(m_TickCombatEvent);
        }
        private void UpdateMouters()
        {
            
            for (int i = m_MoutAbilities.Count - 1; i >= 0; i--)
            {
                if (!m_MoutAbilities[i].IsEnabled())
                {
                    m_MoutAbilities.RemoveAt(i);
                }
            }
            for (int i = m_ChildAbilities.Count - 1; i >= 0; i--)
            {
                if (!m_ChildAbilities[i].IsEnabled())
                {
                    m_ChildAbilities.RemoveAt(i);
                }
            }
            for (int i = m_MoutRecivers.Count - 1; i >= 0; i--)
            {
                if (!m_MoutRecivers[i].IsEnabled())
                {
                    m_MoutRecivers.RemoveAt(i);
                }
            }
        }
    }
}
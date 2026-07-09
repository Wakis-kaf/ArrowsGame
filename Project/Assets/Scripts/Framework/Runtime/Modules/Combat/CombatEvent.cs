using Framework.Runtime.MObjectPool.Core;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

namespace Framework.Runtime.MCombat
{
    public class CombatEvent : IPoolElement
    {
        public string eventCode;
        public bool isGlobal = false;
        public bool needBackCall;
        public bool needClear = true;
        public ICombatEventReceiver sender;
        private List<ICombatEventReceiver> m_CallbakcReceivers = new List<ICombatEventReceiver>();
        private int m_RetCode32 = (int)CombateEventRetCode.Success;
        private List<ICombatEventReceiver> m_SubscribeReceivers = new List<ICombatEventReceiver>();
        private List<ICombatEventReceiver> m_Targets = new List<ICombatEventReceiver>();
        public void OnGetFromPool()
        {
            isGlobal = false;
            eventCode = "";
            needBackCall = false;
            sender = null;
            m_Targets.Clear();
            m_CallbakcReceivers.Clear();
            m_SubscribeReceivers.Clear();
            DataManager.ClearAll(false);
            Proto = null;
            needClear = true;
            m_RetCode32 = (int)CombateEventRetCode.Success;
        }

        public void OnPutToPool()
        {
            Proto?.Dispose();
        }
        public CombatEvent()
        {
            DataManager = new DataManager();
        }

        public DataManager DataManager { get; private set; }
        public bool IsInPool { get; set; }
        public Pool Pool { get; set; }
        public CombatProto Proto { get; set; }
        public int RetCode32 => m_RetCode32;

        public void AddSubscribeListener(ICombatEventReceiver receiver)
        {
            if (!m_SubscribeReceivers.Contains(receiver) && receiver != null)
            {
                m_SubscribeReceivers.Add(receiver);
            }
        }
        public List<ICombatEventReceiver> GetTargets()
        {
            return m_Targets;
        }
        public void AddTarget(ICombatEventReceiver target)
        {
            if (m_Targets.Contains(target)) return;
            m_Targets.Add(target);
            AddSubscribeListener(target);
            AddCallBackListener(target);
        }

        public void CallBack()
        {
            if (!needBackCall) return;
            for (int i = 0; i < m_CallbakcReceivers.Count; i++)
            {
                if (m_CallbakcReceivers[i].IsEnabled())
                {
                    m_CallbakcReceivers[i].HandleEvent(this);
                }
                
            }
        }

        public bool CheckRetNoError()
        {
            return IsSuccess();
        }

        public void Dispose()
        {
            (Pool as CombatEventPool)?.PutCombatEvent(this);
        }

        public T GetContext<T>(string name, T defalutValue = default)
        {
            return DataManager.GetData<T>(name, defalutValue);
        }

        public bool IsCode(string code)
        {
            return eventCode == code;
        }

        public bool IsCodeAndFromr(string code, ICombatEventReceiver target)
        {
            return IsCode(code) && IsEventFrom(target);
        }

        public bool IsCodeAndFromSender(string code, ICombatEventReceiver target)
        {
            return IsCode(code) && IsEventFromSender(target);
        }

        public bool IsCodeAndHasTarget(string code, ICombatEventReceiver target)
        {
            return IsCode(code) && IsEventHasTarget(target);
        }

        public bool IsEventFrom(ICombatEventReceiver target)
        {
            if (target is Combator combator)
            {
                if (sender is Ability ability)
                {
                    return ability.Mouter == combator || ability.Master == combator;
                }
                else
                {
                    return sender == target;
                }
            }
            if (target is Ability targetAbility)
            {
                if (sender is Combator sendCombator)
                {
                    return targetAbility.Mouter == sendCombator || targetAbility.Master == sendCombator;
                }
                else
                {
                    return sender == target;
                }
            }
            return sender == target;
        }

        public bool IsEventFromSender(ICombatEventReceiver target)
        {
            return target == sender;
        }

        public bool IsEventHasTarget(ICombatEventReceiver taret)
        {
            return m_Targets.Contains(taret);
        }

        public bool IsSuccess()
        {
            return (RetCode32 & (int)CombateEventRetCode.Success) > 0;
        }

        public void OnCreateInPool()
        {
        }

        public void OnDestroyByPool()
        {
        }

       

        public void OnPrewarmInPool()
        {
        }

        public void RemoveCallBackListener(ICombatEventReceiver receiver)
        {
            m_CallbakcReceivers.Remove(receiver);
        }

        public void RemoveSubscribeListener(ICombatEventReceiver receiver)
        {
            m_SubscribeReceivers.Remove(receiver);
        }

        public void ResetError()
        {
            m_RetCode32 = (int)CombateEventRetCode.Success;
        }

        public string SetCode(string code)
        {
            eventCode = code;
            return eventCode;
        }

        public void SetContext<T>(string name, T value)
        {
            DataManager.SetData<T>(name, value);
        }

        public int SetRet(CombateEventRetCode combateEventRetCode)
        {
            // 任何非成功原因都会导致成功计0
            m_RetCode32 = ((m_RetCode32 >> 1) << 1) | (int)combateEventRetCode;
            return RetCode32;
        }

        public void SetSender(ICombatEventReceiver sender)
        {
            this.sender = sender;
            AddSubscribeListener(sender);
            AddCallBackListener(sender);
        }

        public void SetTargets(IEnumerable<ICombatEventReceiver> targets)
        {
            foreach (var target in targets)
            {
                AddTarget(target);
            }
        }

        public void SubscribeReceive()
        {
            for (int i = 0; i < m_SubscribeReceivers.Count; i++)
            {
                if (m_SubscribeReceivers[i].IsActive())
                {
                    m_SubscribeReceivers[i].ReceiveEvent(this);
                }
            }
        }

        public bool TryGetContext<T>(string name, out T result, T defalutValue = default)
        {
            return DataManager.TryGetData<T>(name, out result, defalutValue);
        }

        private void AddCallBackListener(ICombatEventReceiver receiver)
        {
            if (!m_CallbakcReceivers.Contains(receiver))
            {
                m_CallbakcReceivers.Add(receiver);
            }
        }
    }
}
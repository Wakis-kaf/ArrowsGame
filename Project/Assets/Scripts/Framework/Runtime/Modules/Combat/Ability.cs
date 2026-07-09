using Framework.Runtime.Base;
using Game.Modules;
using System.Collections.Generic;
using System.Text;

namespace Framework.Runtime.MCombat
{
    public abstract class Ability :UnitObject, ICombatEventReceiver, ICombatEventDispatcher
    {
        protected DataManager m_Data;
        protected AttributeBox m_AttrBox;

        private List<AbilityLink> m_AbilityLinks;

        private string m_Description;

        private int m_Id;

        private Combator m_Master;

        private Combator m_Mouter;

        private string m_Name;

        private int m_Relationship;

        private AbilityStatus m_Status = AbilityStatus.Init;

        public Ability()
        {
            m_Data = new DataManager();
            m_AttrBox = new AttributeBox();
            m_FrameSendsEvent = new HashSet<string>();
        }

        public enum AbilityStatus
        {
            Init,
            Sleepy, // 未生效
            Awaked, // 已唤醒，未激活
            Enabled, // 启用中
            Destroyed, // 已经销毁
        }

        public List<AbilityLink> AbilityLinks => m_AbilityLinks;
        public DataManager Context => m_Data;
        public AttributeBox AttributeBox => m_AttrBox;
        public virtual string Description { get => m_Description;  set => m_Description = value; }
        public virtual int AbilityId { get => m_Id;  set => m_Id = value; }
        public virtual string Name { get => m_Name; set => m_Name = value; }
        public Combator Master { get => m_Master; protected set => m_Master = value; } //技能释放者
        public Combator Mouter { get => m_Mouter; protected set => m_Mouter = value; } // 挂载对象
        public int Relationship { get => m_Relationship; protected set => m_Relationship = value; }
        private HashSet<string> m_FrameSendsEvent;
        public bool HasFrameSend(string code)
        {
            return m_FrameSendsEvent.Contains(code);
        }
        public void ClearFrameSends()
        {
            m_FrameSendsEvent.Clear();
        }
        public void BindMaster(Combator master)
        {
            if (Master != null && master != Master)
            {
                Master.RemoveChildAbility(this);
            }
            Master = master;
            Master.BirthAbility(this);
            DoBirth(SendEventSelf(CombatCode.OnAbilityBirth));
        }

        public void BindMouter(Combator mouter)
        {
            if (Mouter != null && mouter != Mouter)
            {
                Mouter.RemoveMoutedReceiver(this);
            }
            Mouter = mouter;
            Mouter?.MoutReceiver(this);
            DoAwake(SendEventSelf(CombatCode.OnAbilityAwake));
        }
        public virtual CombatEvent SendEventToTarget(string code, ICombatEventReceiver target)
        {
            CombatEvent combatEvent = CreateEvent(code, this);
            combatEvent.AddTarget(target);
            return SendEvent(combatEvent);
        }
        public virtual CombatEvent SendEventToTargets(string code, IEnumerable<ICombatEventReceiver> targets)
        {
            CombatEvent combatEvent = CreateEvent(code, this);
            foreach (var target in targets)
            {
                combatEvent.AddTarget(target);
            }
            return SendEvent(combatEvent);
        }

        public virtual CombatEvent SendEvent(CombatEvent combatEvent)
        {
            CombatSystem.Ins.DispatchEvent(combatEvent);
            m_FrameSendsEvent.Add(combatEvent.eventCode);
            return combatEvent;
        }
        public void SendEventSync(CombatEvent combatEvent)
        {
            CombatSystem.Ins.DispatchEventSync(combatEvent);
            m_FrameSendsEvent.Add(combatEvent.eventCode);
            return ;
        }

        public virtual List<AbilityLink> GeneateAbilityLinks()
        {
            return new List<AbilityLink>();
        }

        public string GetDebugInfo()
        {
            StringBuilder sb = new StringBuilder($"当前类型 {GetType().Name}");
            sb.AppendLine($"当前状态:{m_Status}");
            sb.AppendLine($"当前名称:{Name}");
            sb.AppendLine($"当前描述:{Description}");
            sb.AppendLine($"当前关系:{Relationship}");
            return sb.ToString();
        }

        public virtual string GetName()
        {
            return "";
        }

        public virtual CombatEvent HandleEvent(CombatEvent combatEvent)
        {
            ClearFrameSends();
            CheckAbilityLifeTimeEvent(combatEvent);
            if (combatEvent.IsCode(CombatCode.OnAbilityUpdate) 
                && combatEvent.IsEventFrom(this)&&
                combatEvent.IsEventFrom(this.Mouter))
            {
                return combatEvent;
            }
            return OnHandleEvent(combatEvent);
        }
        public virtual CombatEvent ReceiveEvent(CombatEvent combatEvent)
        {
            return OnReceiveEvent(combatEvent);
        }

        public virtual bool IsActive()
        {
            return IsStatus(AbilityStatus.Enabled) && IsEnabled();
        }
        public virtual bool IsEnabled()
        {
            return m_Mouter != null && m_Mouter.IsActive() && !IsStatus(AbilityStatus.Destroyed);
        }

        public bool IsStatus(AbilityStatus status)
        {
            return status == m_Status;
        }

     

        public CombatEvent TryDestroy()
        {
            if (IsStatus(AbilityStatus.Destroyed)) return null;
            CombatEvent combatEvent = SendEventSelf(CombatCode.TryDestroyAbility);
            return combatEvent;
        }
        public void TryDestroySync()
        {
            if (IsStatus(AbilityStatus.Destroyed)) return;
            SendEventSelfSync(CombatCode.TryDestroyAbility);
            return ;
        }

        public CombatEvent TryDisable()
        {
            if (!IsActive()) return null;
            CombatEvent combatEvent = SendEventSelf(CombatCode.TryDisableAbility);
            return combatEvent;
        }

        public CombatEvent TryEnable()
        {
            if (IsActive()) return null; // 已经开启
            CombatEvent combatEvent = SendEventSelf(CombatCode.TryEnableAbility);
            return combatEvent;
        }

        protected virtual CombatEvent CheckAbilityLifeTimeEvent(CombatEvent combatEvent)
        {
            if (combatEvent.IsCode(CombatCode.TryEnableAbility) && combatEvent.IsEventFrom(this) && !IsActive())
            {
                bool sendCode = true;
                if (combatEvent.IsSuccess())
                {
                    if (combatEvent.IsEventFromSender(this))
                    {
                        DoEnable(combatEvent);
                        sendCode = false;
                    }
                }
                if (sendCode && combatEvent.IsEventFromSender(this))
                {
                    var sendEvent = SendEventSelf(CombatCode.OnAbilityEnableFail);
                }
            }

            if (combatEvent.IsCode(CombatCode.TryDisableAbility) && combatEvent.IsEventFrom(this) && IsActive())
            {
                bool sendCode = true;
                if (combatEvent.CheckRetNoError())
                {
                    if (combatEvent.IsEventFromSender(this))
                    {
                        DoDisable(combatEvent);
                        sendCode = false;
                    }
                }
                if (sendCode && combatEvent.IsEventFromSender(this))
                {
                    var sendEvent = SendEventSelf(CombatCode.OnAbilityDisableFail);
                }
            }

            if (combatEvent.IsCode(CombatCode.TryDestroyAbility) && combatEvent.IsEventFrom(this))
            {
                bool sendCode = true;
                if (combatEvent.CheckRetNoError())
                {
                    if (combatEvent.IsEventFromSender(this))
                    {
                        DoDistroy(combatEvent);
                        sendCode = false;
                    }
                }
                if (sendCode && combatEvent.IsEventFromSender(this))
                {
                    var sendEvent = SendEventSelf(CombatCode.OnAbilityDisableFail);
                }
            }
            if (combatEvent.IsCode(CombatCode.OnAbilityUpdate) && combatEvent.IsEventFrom(this))
            {
                if (combatEvent.IsEventFromSender(this.Mouter))
                {
                    OnAbilityUpdate(combatEvent);
                }
            }
            return combatEvent;
        }

        protected CombatEvent CreateEvent(string code, ICombatEventReceiver sender)
        {
            CombatEvent combatEvent = CombatSystem.Ins.CreateCombatEvent(sender);
            combatEvent.SetCode(code);
            combatEvent.needBackCall = true; // 需要回传
            combatEvent.AddSubscribeListener(this.Mouter);
            combatEvent.AddSubscribeListener(this.Master);
           
            return combatEvent;
        }

        public CombatEvent SendEventSelf(string code)
        {
            var combatEvent = CreateEvent(code, this);
            combatEvent.AddTarget(this);
            return SendEvent(combatEvent);
        }
        public void SendEventSelfSync(string code)
        {
            var combatEvent = CreateEvent(code, this);
            combatEvent.AddTarget(this);
            SendEventSync(combatEvent);
        }
   
        /// <summary>
        /// 当能力挂载到目标(Mount)对象上时回调
        /// </summary>
        /// <param name="awakeEvent"></param>
        protected virtual void OnAbilityAwake(CombatEvent awakeEvent)
        {
            m_AbilityLinks = GeneateAbilityLinks();
        }

        /// <summary>
        /// 当能力初始化,添加到父战斗者身上(Master)的时候回调
        /// </summary>
        /// <param name="birthEvent"></param>
        protected virtual void OnAbilityBirth(CombatEvent birthEvent)
        {
        }

       
        /// <summary>
        /// 当能力销毁的时候回调
        /// </summary>
        /// <param name="destroyEvent"></param>
        protected virtual void OnAbilityDestroy(CombatEvent destroyEvent)
        {
        }

       
        /// <summary>
        /// 当能力禁用的时候回调
        /// </summary>
        /// <param name="disableEvent"></param>
        protected virtual void OnAbilityDisable(CombatEvent disableEvent)
        {
            
        }

       
        /// <summary>
        /// 当能力开启的时候回调
        /// </summary>
        /// <param name="enableEvent"></param>
        protected virtual void OnAbilityEnable(CombatEvent enableEvent)
        {
        }

       
        /// <summary>
        /// 当能力每帧更新的时候回调
        /// </summary>
        /// <param name="updateEvent"></param>
        protected virtual void OnAbilityUpdate(CombatEvent updateEvent)
        {

        }
        /// <summary>
        /// 当挂载战斗实体或者父战斗实体收到消息后广播得到的事件
        /// 也可以是挂载到全局事件中，收到的全局事件
        /// </summary>
        /// <param name="combatEvent"></param>
        /// <returns></returns>
        protected virtual CombatEvent OnReceiveEvent(CombatEvent combatEvent)
        {
            return combatEvent;
        }
        /// <summary>
        /// 当能力参与目标事件，目标事件轮询更新完成后
        /// 最终回传执行最终逻辑
        /// </summary>
        /// <param name="combatEvent"></param>
        /// <returns></returns>
        protected virtual CombatEvent OnHandleEvent(CombatEvent combatEvent)
        {
            return combatEvent;
        }

        protected void SwitchStatus(AbilityStatus status)
        {
            m_Status = status;
        }

        private void DoAwake(CombatEvent awakeEvent)
        {
            if (!IsStatus(AbilityStatus.Sleepy)) return;
            SwitchStatus(AbilityStatus.Awaked);
            OnAbilityAwake(awakeEvent);
        }

        // 当接受事件的时候
        private void DoBirth(CombatEvent birthEvent)
        {
            if (!IsStatus(AbilityStatus.Init)) return;
            SwitchStatus(AbilityStatus.Sleepy);
            OnAbilityBirth(birthEvent);
        }

        private void DoDisable(CombatEvent disableEvent)
        {
            if (!IsStatus(AbilityStatus.Enabled)) return;
            SwitchStatus(AbilityStatus.Awaked);
            OnAbilityDisable(disableEvent);
            Mouter.DegisterLinks(AbilityLinks);
            SendEventSelf(CombatCode.OnAbilityDisableSuccess);
        }

        private void DoDistroy(CombatEvent destroyEvent)
        {
            if (IsStatus(AbilityStatus.Destroyed)) return;
            //DoDisable(destroyEvent);
            SwitchStatus(AbilityStatus.Destroyed);
            SendEventSelf(CombatCode.OnAbilityDestroySuccess);
            OnAbilityDestroy(destroyEvent);
            Master.RemoveChildAbility(this);
            Mouter.RemoveMoutedReceiver(this);
            Mouter.RemoveMoutAbility(this);
        }

        private void DoEnable(CombatEvent enableEvent)
        {
            if (!IsStatus(AbilityStatus.Awaked)) return;
            SwitchStatus(AbilityStatus.Enabled);
            SendEventSelf(CombatCode.OnAbilityEnableSuccess);
            Mouter.RegisterLinks(AbilityLinks);
            OnAbilityEnable(enableEvent);
        }
        public static T GetModHandlerIns<T>() where T : GameModuleHandler
        {
            return GameApp.GameModuleManager.GetGlobalHandlerInstance<T>();
        }
        public NumberAttribute GetNumAttr(string code)
        {
            return AttributeBox.GetNumberAttribute(code);
        }
        public int GetNumAttrIntValue(string code)
        {
            return (int)AttributeBox.GetNumberAttribute(code).FinalValue;
        }
        public float GetNumAttrFloatValue(string code)
        {
            return (float)AttributeBox.GetNumberAttribute(code).FinalValue;
        }
        public NumberAttribute GetMouterNumAttr(string code) {
            return Mouter.AttributeBox.GetNumberAttribute(code);
        }
        public NumberAttribute GetMasterNumAttr(string code)
        {
            return Master.AttributeBox.GetNumberAttribute(code);
        }
        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            TryDestroySync();
            //var evt = CreateEvent(CombatCode.TryDestroyAbility,this);
            //evt.needBackCall = false;
            //DoDistroy(evt);
        }
    }
}
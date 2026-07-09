using Framework.Runtime.Base;

using System;
using System.Collections.Generic;

namespace Framework.Runtime.UnitSystem.Base
{
    public enum UnitHandleType
    {
        UnitRegister = 101,
        UnitDeRegister = 102,
        UnitEnable = 103,
        UnitDisable = 104,
    }

    public static class UnitHandleEvent
    {
        public static string UnitDeRegister = "UnitDeRegister";
        public static string UnitDisable = "UnitDisable";
        public static string UnitEnable = "UnitEnable";
        public static string UnitRegister = "UnitRegister";
    }

    public abstract class UnitBehaviourHandler<T> : UnitObject, IUnitBehaviourHandler
    {
        private Dictionary<UnitHandleType, Action<T>> m_handleType2HandleActionMap =
            new Dictionary<UnitHandleType, Action<T>>();

        public UnitBehaviourHandler()
        {
            RegisterInitialHandlers(m_handleType2HandleActionMap);
        }

        /// <summary>
        /// 单元处理器优先级，优先级越高处理器越先处理，销毁时越后销毁
        /// </summary>
        public virtual int Priority => 1;

        public virtual void OnAssignUnitDeRegister(T assignUnit)
        {
        }

        public virtual void OnAssignUnitDisable(T assignUnit)
        {
        }

        public virtual void OnAssignUnitEnable(T assignUnit)
        {
        }

        public virtual void OnAssignUnitRegister(T assignUnit)
        {
        }

        public virtual void OnUnitHandleEventTrigger(IBehaviourUnit behaviourUnit, UnitHandleType handleType)
        {
            if (m_handleType2HandleActionMap.ContainsKey(handleType) && behaviourUnit is T tUnitItem)
                m_handleType2HandleActionMap[handleType]?.Invoke(tUnitItem);
        }

        public virtual void OnUnitModuleFixedUpdate()
        {
        }

        public virtual void OnUnitModuleLateUpdate()
        {
        }

        public virtual void OnUnitModuleQuit()
        {
        }

        public virtual void OnUnitModuleUpdate()
        {
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            m_handleType2HandleActionMap.Clear();
        }

        protected virtual void RegisterInitialHandlers(Dictionary<UnitHandleType, Action<T>> handlerDict)
        {
            handlerDict.Add(UnitHandleType.UnitRegister, OnAssignUnitRegister);
            handlerDict.Add(UnitHandleType.UnitDeRegister, OnAssignUnitDeRegister);
            handlerDict.Add(UnitHandleType.UnitEnable, OnAssignUnitEnable);
            handlerDict.Add(UnitHandleType.UnitDisable, OnAssignUnitDisable);
        }
    }
}
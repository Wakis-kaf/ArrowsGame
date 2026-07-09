using Framework.Runtime.LogSystem;
using Framework.Runtime.SystemEvent;

//using Framework.Runtime.SystemEvent.FrameEvent;
using Framework.Runtime.UnitSystem.Base;
using Framework.Runtime.UnitSystem.BIInterfaces;

//using Framework.Runtime.UnitSystem.Event;
using Framework.Utils;

using System;
using System.Collections.Generic;

namespace Framework.Runtime.UnitSystem
{
    public  class UnitManager
    {
        public static UnitManager Instance => GameApp.Ins.UnitManager;
        public  Type[] HandlerTypes = new Type[]
        {
            typeof(UnitBehaviourAwakeHandler),
            typeof(UnitBehaviourControllerHandler),
            typeof(UnitBehaviourDestroyHandler),
            typeof(UnitBehaviourDisableHandler),
            typeof(UnitBehaviourEnableHandler),
            typeof(UnitBehaviourFixedUpdateHandler),
            typeof(UnitLateUpdateHandler),
            typeof(UnitBehaviourStartHandler),
        };

        private  Queue<int> m_CacheUnitOptType;
        private  Queue<IBehaviourUnit> m_CacheUnits;
        private  bool m_IsDisposeAllIng = false;
        private  bool m_IsDisposed = false;
        private  bool m_IsEnable = false;
        private  Action<IBehaviourUnit> m_UnitAddEvent;
        private  Action<IBehaviourUnit, UnitHandleType> m_UnitHandleEvents;
        private  List<IUnitBehaviourHandler> m_UnitHandlers;
        private  Action m_UnitModuleQuit;
        private  Action<IBehaviourUnit> m_UnitRemoveEvent;
        private  List<IBehaviourUnit> m_Units;

         public UnitManager()
        {
            m_CacheUnits = new Queue<IBehaviourUnit>();
            m_CacheUnitOptType = new Queue<int>();
        }

        public  void AddUnitDeregisterListener(Action<IBehaviourUnit> listener)
        {
            if (listener == null) return;
            m_UnitRemoveEvent += listener;
        }

        public  void AddUnitRegisterListener(Action<IBehaviourUnit> listener, bool pullFromHistory = false)
        {
            if (listener == null) return;
            if (pullFromHistory)
                for (var i = 0; i < m_Units.Count; i++)
                    listener.Invoke(m_Units[i]);

            m_UnitAddEvent += listener;
        }

        public  void AppUpdate(GameAppMessage appMessage)
        {
        }

        public  void Close()
        {
            RemoveMessageListeners();
            DisposeManagedResources();
            DisposeUnManagedResources();
        }

        public  IBehaviourUnit[] GetAllUnit()
        {
            return m_Units.ToArray();
        }

        public  void Init()
        {
            m_IsDisposed = false;
            m_Units = new List<IBehaviourUnit>(1024);
            m_UnitHandlers = new List<IUnitBehaviourHandler>(64);
            m_IsDisposeAllIng = false;
            UnitHandlerInit();
            BindMessageListeners();
        }

        public  void OnFixedUpdate()
        {
            FixedUpdateUnitHandlers();
        }

        public  void OnLateUpdate()
        {
            LateUpdateUnitHandlers();
        }

        public  void OnModuleShutdown()
        {
            m_UnitModuleQuit?.Invoke();
        }

        public  void OnUnitDeRegister(IBehaviourUnit unit)
        {
            if (!m_IsEnable)
            {
                m_CacheUnits.Enqueue(unit);
                m_CacheUnitOptType.Enqueue(1);
                return;
            }
            FunctionUtility.SafeCall(DeRegisterUnit,unit);
        }

        public  void OnUnitRegister(IBehaviourUnit unit)
        {
            if (!m_IsEnable)
            {
                m_CacheUnits.Enqueue(unit);
                m_CacheUnitOptType.Enqueue(0);
                return;
            }
             FunctionUtility.SafeInvoke(RegisterUnit,unit);
        }

        public  void RemoveUnitDeRegisterListener(Action<IBehaviourUnit> listener)
        {
            m_UnitRemoveEvent -= listener;
        }

        public  void RemoveUnitRegisterListener(Action<IBehaviourUnit> listener)
        {
            m_UnitAddEvent -= listener;
        }

        public  void Start()
        {
            m_IsEnable = true;
            DispatchCache();

            GameApp.Ins.LoopManager.AddLoop(OnUpdate);
            GameApp.Ins.LoopManager.AddFixedLoop(OnFixedUpdate);
            GameApp.Ins.LoopManager.AddLateLoop(OnLateUpdate);
        }

        public  void UnitDisable(IBehaviourUnit unit)
        {
            if (!m_IsEnable)
            {
                m_CacheUnits.Enqueue(unit);
                m_CacheUnitOptType.Enqueue(3);
                return;
            }
            m_UnitHandleEvents?.Invoke(unit, UnitHandleType.UnitDisable);
        }

        public  void UnitEnable(IBehaviourUnit unit)
        {
            if (!m_IsEnable)
            {
                m_CacheUnits.Enqueue(unit);
                m_CacheUnitOptType.Enqueue(2);
                return;
            }
            m_UnitHandleEvents?.Invoke(unit, UnitHandleType.UnitEnable);
        }

        private  void BindHandleEvent()
        {
            var length = m_UnitHandlers.Count;
            for (var i = 0; i < length; i++)
            {
                var handler = m_UnitHandlers[i];
                // 绑定事件
                m_UnitModuleQuit += handler.OnUnitModuleQuit;
                m_UnitHandleEvents += handler.OnUnitHandleEventTrigger;
            }
        }

        private  void BindMessageListeners()
        {
            MessageDispatcher.Ins.Subscribe<IBehaviourUnit>(MessageCode.msg_unit_register, OnUnitRegister);
            MessageDispatcher.Ins.Subscribe<IBehaviourUnit>(MessageCode.msg_unit_degister, OnUnitDeRegister);
            MessageDispatcher.Ins.Subscribe<IBehaviourUnit>(MessageCode.msg_unit_enable, UnitEnable);
            MessageDispatcher.Ins.Subscribe<IBehaviourUnit>(MessageCode.msg_unit_disable, UnitDisable);
        }

        private  void DeRegisterUnit(IBehaviourUnit unit)
        {
            // 移出队列
            var index = m_Units.IndexOf(unit);
            if (index == -1) return;
            // 触发事件
            m_UnitHandleEvents?.Invoke(unit, UnitHandleType.UnitDeRegister);
            if (!m_IsDisposeAllIng)
            {
                int lastIndex = m_Units.Count - 1;
                m_Units[index] = m_Units[lastIndex];
                m_Units.RemoveAt(lastIndex);
            }
            m_UnitRemoveEvent?.Invoke(unit);
        }

        private  void DispatchCache()
        {
            while (m_CacheUnits.Count > 0)
            {
                var unit = m_CacheUnits.Dequeue();
                var optType = m_CacheUnitOptType.Dequeue();
                if (optType == 0)
                {
                    OnUnitRegister(unit);
                }
                else if (optType == 1)
                {
                    OnUnitDeRegister(unit);
                }
                else if (optType == 2)
                {
                    UnitEnable(unit);
                }
                else if (optType == 3)
                {
                    UnitDisable(unit);
                }
            }
        }

        private  void DisposeManagedResources()
        {
            m_IsDisposeAllIng = true;
            var length = m_Units.Count;
            for (var i = length - 1; i >= 0; i--)
            {
                if (i >= m_Units.Count) continue;
                try
                {
                    m_Units[i].Dispose();
                }
                catch (Exception e)
                {
                    Log.Fatal($"unit dispose  error! {m_Units[i]?.Type} : {e}");
                }
            }

            length = m_UnitHandlers.Count;
            for (var i = length - 1; i >= 0; i--)
                try
                {
                    m_UnitHandlers[i].Dispose();
                }
                catch (Exception e)
                {
                    Log.Fatal($"unit handler dispose  error! {m_UnitHandlers[i]?.Type} : {e}");
                    throw;
                }

            m_IsDisposeAllIng = false;
        }

        private  void DisposeUnManagedResources()
        {
            m_IsDisposed = true;
            m_Units.Clear();
            m_UnitHandlers.Clear();
            m_Units = null;
            m_UnitHandlers = null;
            m_UnitHandleEvents = null;
            m_UnitModuleQuit = null;
            m_UnitAddEvent = null;
            m_UnitRemoveEvent = null;
        }

        private  void FixedUpdateUnitHandlers()
        {
            var count = m_UnitHandlers.Count;
            for (var i = 0; i < count; i++)
            {
                var item = m_UnitHandlers[i];
                try
                {
                    item.OnUnitModuleFixedUpdate();
                }
                catch (Exception e)
                {
                    Log.Fatal($"Handler fixed update error! {item.Type} : {e.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// 按照优先等级降序排序处理器///
        /// </summary>
        private  int HandlerCompare(IUnitBehaviourHandler one, IUnitBehaviourHandler two)
        {
            if (one.Priority < two.Priority)
                return 1;
            return -1;
        }

        private  void LateUpdateUnitHandlers()
        {
            var count = m_UnitHandlers.Count;
            for (var i = 0; i < count; i++)
            {
                var item = m_UnitHandlers[i];
                try
                {
                    item.OnUnitModuleLateUpdate();
                }
                catch (Exception e)
                {
                    Log.Fatal($"Handler late update error! {item.Type} : {e.Message}");
                    throw;
                }
            }
        }

        private  void OnUpdate()
        {
            UpdateUnitHandlers();
        }

        private  void OuUnitEnable(FrameworkEventArgs args)
        {
            // 触发事件
            UnitEnabledEventArgs enabledEventArgs = args as UnitEnabledEventArgs;
            m_UnitHandleEvents?.Invoke(enabledEventArgs.behaviourUnit, UnitHandleType.UnitEnable);
        }

        private  IBehaviourUnit RegisterUnit<T>(T unit) where T: IBehaviourUnit
        {
            if (m_IsDisposed) return default;
            if (ReferenceEquals(unit, null)) return default;
            // 加入队列
            m_Units.Add(unit);
            // 触发事件
            m_UnitHandleEvents?.Invoke(unit, UnitHandleType.UnitRegister);
            m_UnitAddEvent?.Invoke(unit);
            return unit;
        }

        private  void RegisterUnitHandlers(Type[] unitHandlerTypes)
        {
            for (var i = 0; i < unitHandlerTypes.Length; i++)
            {
                var handlerType = unitHandlerTypes[i];
                if (handlerType.IsGenericType) continue;
                var handler = Utility.ReflectionUtil.CreateInstance<IUnitBehaviourHandler>(handlerType);
                m_UnitHandlers.Add(handler);
            }

            m_UnitHandlers.Sort(HandlerCompare);
            BindHandleEvent();
        }

        private  void RemoveMessageListeners()
        {
            MessageDispatcher.Ins.Unsubscribe<IBehaviourUnit>(MessageCode.msg_unit_register, OnUnitRegister);
            MessageDispatcher.Ins.Unsubscribe<IBehaviourUnit>(MessageCode.msg_unit_degister, OnUnitDeRegister);
            MessageDispatcher.Ins.Unsubscribe<IBehaviourUnit>(MessageCode.msg_unit_enable, UnitEnable);
            MessageDispatcher.Ins.Unsubscribe<IBehaviourUnit>(MessageCode.msg_unit_disable, UnitDisable);
        }

        private  void UnitHandlerInit()
        {
            // 获取所有实现了 UnitHandler接口的类并创建其实例
            //var unitHandlers = Utility.ReflectionUtil.GetSubClassOfRawGeneric(typeof(UnitBehaviourHandler<>));
            var unitHandlers = HandlerTypes;
            RegisterUnitHandlers(unitHandlers);
        }

        private  void UpdateUnitHandlers()
        {
            var count = m_UnitHandlers.Count;
            for (var i = 0; i < count; i++)
            {
                var item = m_UnitHandlers[i];
                FunctionUtility.SafeCall(item.OnUnitModuleUpdate);
                //try
                //{
                //    item.OnUnitModuleUpdate();
                //}
                //catch (Exception e)
                //{
                //    Log.Fatal($"Handler update error! {item.Type} \n: {e}");
                //    throw;
                //}
            }
        }
    }
}
using Framework.Runtime;
using Framework.Runtime.UnitSystem.Base;
using Framework.Runtime.UnitSystem.MonoBase;
using Framework.Utils;
using System;
using System.Collections.Generic;

namespace Game.Modules
{
    public class GameModuleBase: BehaviourUnit
    {
        public GameModuleBase()
        {
            m_Type2HandlerCacheMap = new Dictionary<Type, GameModuleHandler>();
            m_Handlers = new List<GameModuleHandler>();
            OnConstructed();
        }
        //private bool m_IsSelfLoaded = false;
        //private bool m_IsHandlersLoaded = false;
        public bool IsLoaded { get; protected set; } = false;
        public bool IsHandlersLoaded { get; protected set; } = false;
        //public bool IsLoaded => m_IsSelfLoaded && m_IsHandlersLoaded;

        public void AwakeModule()
        {
            OnModuleAwake();
            GenerateHandlers();
            AwakeModuleHandlers();
        }
        public void CheckModuleHandlerLoad(Action allHandlerLoadedCb)
        {
            m_AllHandlerLoadedCb = allHandlerLoadedCb;
            for (int i = 0; i < m_Handlers.Count; i++)
            {
                FunctionUtility.SafeCall<Action>(m_Handlers[i].CheckLoad, CheckAllHandlerLoaded);
            }
            CheckAllHandlerLoaded();
        }
        private void CheckAllHandlerLoaded()
        {
            bool isReady = true;
            for (int i = 0; i < m_Handlers.Count; i++)
            {
                if (!m_Handlers[i].IsLoaded)
                {
                    isReady = false;
                }
            }
            if (isReady)
            {
                this.OnModuleHandlerLoaded();
            }
        }
        public void CheckModuleLoad(Action onModuleLoadedCb)
        {
            this.m_OnModuleLoadedCb = onModuleLoadedCb;
            OnCheckModuleLoad();
        }

        public void DestroyModule()
        {
            DestoryModuleHandlers();
            OnModuleDestroy();
        }

        public void RegisterHandler<T>() where T : GameModuleHandler
        {
            if (!TryGetHandler<T>(out T handler))
            {
                T handlerInstance = Utility.ReflectionUtil.CreateInstance<T>();
                handlerInstance.BindOwnGameModule(this);
                if (handlerInstance != null)
                {
                    RegiserHandler(handlerInstance);
                }
            }
        }

        public void StartModule()
        {
            FunctionUtility.SafeCall(OnModuleStart);
            FunctionUtility.SafeCall(StartModuleHandlers);
        }
        public void EnableModule()
        {
            FunctionUtility.SafeCall(OnModuleEnable);
            FunctionUtility.SafeCall(EnableModuleHandlers);
        }
        public T GetHandler<T>() where T : GameModuleHandler
        {
            Type type = typeof(T);
            if (m_Type2HandlerCacheMap.TryGetValue(type, out var findHandler))
            {
                return findHandler as T;
            }
            for (int i = 0; i < m_Handlers.Count; i++)
            {
                if (m_Handlers[i].GetType() == type)
                {
                    m_Type2HandlerCacheMap.Add(type, m_Handlers[i]);
                    return m_Handlers[i] as T;
                }
            }
            return null;
        }
        public bool TryGetHandler<T>(out T handler) where T : GameModuleHandler
        {
            handler = GetHandler<T>();
            return handler != null;
           
        }

        protected virtual void GenerateHandlers()
        {
        }

        protected virtual void OnCheckModuleLoad()
        {
            OnModuleLoaded();
        }
        
        private void OnModuleHandlerLoaded()
        {
            this.IsHandlersLoaded = true;
            m_AllHandlerLoadedCb?.Invoke();
            //CheckAllModuleLoaded();
        }

        protected virtual void OnConstructed()
        {
        }

        protected virtual void OnModuleAwake()
        {
        }

        protected virtual void OnModuleDestroy()
        {
        }
     
        protected virtual void OnModuleLoaded()
        {
            this.IsLoaded = true;
            this.m_OnModuleLoadedCb?.Invoke();
        }
        

        protected virtual void OnModuleStart()
        {
        }
        protected virtual void OnModuleEnable()
        {
        }

        private List<GameModuleHandler> m_Handlers;
        private Action m_OnModuleLoadedCb;
        private Action m_AllHandlerLoadedCb;
        private Dictionary<Type, GameModuleHandler> m_Type2HandlerCacheMap;

        private void AwakeModuleHandlers()
        {
            for (int i = 0; i < m_Handlers.Count; i++)
            {
                m_Handlers[i].DoHandlerAwake();
            }
        }

        private void DestoryModuleHandlers()
        {
            for (int i = 0; i < m_Handlers.Count; i++)
            {
                m_Handlers[i].DoHandlerDestroy();
            }
        }

        private void RegiserHandler(GameModuleHandler handler)
        {
            Type type = handler.GetType();
            m_Type2HandlerCacheMap.Add(type, handler);
            m_Handlers.Add(handler);
            GameApp.GameModuleManager.RegisterGlobalHandler(handler);
        }

        private void StartModuleHandlers()
        {
            for (int i = 0; i < m_Handlers.Count; i++)
            {
                FunctionUtility.SafeCall(m_Handlers[i].DoHandlerStart);
            }
        }
        private void EnableModuleHandlers()
        {
            for (int i = 0; i < m_Handlers.Count; i++)
            {
                FunctionUtility.SafeCall(m_Handlers[i].DoHandlerEnable);
            }
        }
    }
}
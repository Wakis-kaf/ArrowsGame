using Framework.Runtime;
using Framework.Runtime.UnitSystem.Base;
using System;

namespace Game.Modules
{
    public class GameModuleHandler: BehaviourUnit
    {
        private GameModuleBase m_OwnerModule;
        public GameModuleBase OwnerModule => m_OwnerModule;
        public static T GetModuleHandlerIns<T>() where T : GameModuleHandler
        {
            return GameApp.GameModuleManager?.GetGlobalHandlerInstance<T>();
        }
        
        public bool TryGetModuleInHandler<T>(out T handler) where T : GameModuleHandler
        {
            return OwnerModule.TryGetHandler<T>(out handler);
        }
        public T GetModuleInHandler<T>() where T : GameModuleHandler
        {
            return OwnerModule.GetHandler<T>();
        }
        public void BindOwnGameModule(GameModuleBase ownerModule)
        {
            m_OwnerModule = ownerModule;
        }
        private Action m_OnLoadedCb;
        public bool IsLoaded { get; private set; } = false;
        public void CheckLoad(Action onLoadedCb)
        {
            m_OnLoadedCb = onLoadedCb;
            OnCheckHandlerLoad();
        }
        protected virtual  void OnCheckHandlerLoad()
        {
            OnHandlerLoaded();
        }

        protected void OnHandlerLoaded()
        {
            IsLoaded = true;
            m_OnLoadedCb?.Invoke();
        }

        public void DoHandlerAwake()
        {
            OnHandlerAwake();
        }

        public void DoHandlerDestroy()
        {
            OnHandlerDestroy();
        }

        public void DoHandlerStart()
        {
            OnHandlerStart();
        }
        public void DoHandlerEnable()
        {
            OnHandlerEnable();
        }

        protected virtual void OnHandlerAwake()
        {
        }
        /// <summary>
        /// 所有模块均已经加载好且Handler都已注册好后调用
        /// </summary>
        protected virtual void OnHandlerEnable()
        {
        }
        /// <summary>
        /// 游戏全部加载成功，Loading结束后进入主游戏回调
        /// </summary>
        protected virtual void OnHandlerStart()
        {
        }
        protected virtual void OnHandlerDestroy()
        {
        }

        
    }
}
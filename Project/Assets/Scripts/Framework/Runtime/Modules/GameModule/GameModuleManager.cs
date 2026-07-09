using Framework.Runtime.LogSystem;
using Framework.Runtime.MAsset;
using Framework.Runtime.Module.Core;
using Framework.Utils;
using Game.Modules;
using HybridCLR;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.MGameModule
{
    public class GameModuleManager : ModuleUnit
    {
        private List<string> m_HotUpdateAssemblyNames = new List<string>()
        {
            "Assets/AddressableResources/HotCode/HotDlls/GameRuntime.dll.bytes"
        };
        public  List<Type> GameModuleTypeList = new List<Type>();
        private List<GameModuleBase> m_GameModules;
        private List<GameModuleHandler> m_GameModuleHandlers;
        private Dictionary<Type, GameModuleHandler> m_GlobalHandlerGetCache;
        private Action m_OnAllGameModuleReadyCb;
        private Dictionary<Type, GameModuleBase> m_Type2GameModuleCacheMap;
        private HashSet<IAssetVO> m_LoadingAssets = new HashSet<IAssetVO>();
        private bool m_IsAllLoadingAssetPushed = false;
       
        public void LoadGameModule(Action onGameModuleReadyCb)
        {
            this.m_OnAllGameModuleReadyCb = onGameModuleReadyCb;
            if(GameEnv.hotCodeModel == HotCodeModel.None)
            {
                m_IsAllLoadingAssetPushed = true;
                CheckLoadingAssetAllDown();
            }
            else
            {
                // 等待加载原数据
                LoadHotUpdateAssemply();
                LoadMetadataForAOTAssemblies();
                m_IsAllLoadingAssetPushed = true;
                CheckLoadingAssetAllDown();
            }
          
        }
        private void LoadHotUpdateAssemply()
        {
            for (int i = 0; i < m_HotUpdateAssemblyNames.Count; i++) { 
                string hotAssemplyName = m_HotUpdateAssemblyNames[i];
                string link = AssetPathEncoder.EncodeEnvAssetLink(hotAssemplyName, AssetType.HotCodeBytesAsset);
                IAssetVO assetVO = GameApp.AssetManager.LoadAssetAsync(link, OnHotAssemplyLoaded);
                MarkLoadingAsset(assetVO);
            }
            
        }
        private void MarkLoadingAsset(IAssetVO assetVO)
        {
            if (assetVO == null) return;
            m_LoadingAssets.Add(assetVO);
        }
        private void OnLoadingAssetComplete(IAssetVO assetVO)
        {
            m_LoadingAssets.Remove(assetVO);
            assetVO.UnLoadAsync();
            CheckLoadingAssetAllDown();
        }
        private void CheckLoadingAssetAllDown()
        {
            if (!m_IsAllLoadingAssetPushed) return;
            if (m_LoadingAssets.Count == 0)
            {
                MessageDispatcher.Ins.Dispatch(MessageCode.msg_gamemodules_loaded);
                GenerateGameModules();
                CheckModuleLoad();
                
            }
        }
        private void CheckModuleLoad()
        {
            for (int i = 0; i < m_GameModules.Count; i++)
            {
               FunctionUtility.SafeCall<Action>(m_GameModules[i].CheckModuleLoad, CheckAllModuleReady);
            }
            CheckAllModuleReady();
        }
        private void CheckModuleHandlerLoad()
        {
            for (int i = 0; i < m_GameModules.Count; i++)
            {
                FunctionUtility.SafeCall<Action>(m_GameModules[i].CheckModuleHandlerLoad, CheckAllModuleHandlerReady);
            }
            CheckAllModuleHandlerReady();
        }
        private void OnHotAssemplyLoaded(IAssetVO assetVo)
        {
            
            Log.Info($"加载程序集完成{assetVo.assetPath}  IsLoadSuccess：{assetVo.IsLoadSuccess}");
            if (!assetVo.IsLoadSuccess)
            {
                OnLoadingAssetComplete(assetVo);
                Log.Error("加载热更新程序集错误" + assetVo.assetPath);
                return;
            }
            var bytes = assetVo.GetAsset<TextAsset>().bytes;
            Utility.AssemblyUtil.LoadAssemblyByBytes(bytes);
            OnLoadingAssetComplete(assetVo);


        }
        private  void LoadMetadataForAOTAssemblies(string aotMetadataDllLabelName = "AOTMetadataDll")
        {
            // 列表可用于释放资源
            List<TextAsset> aotMetadataDll = new List<TextAsset>();
            string aotMetadataDllPath = AssetPathEncoder.EncodeHotAssetLink(aotMetadataDllLabelName,AssetType.AddressableGroupAsset);
            IAssetVO assetVO = GameApp.AssetManager.LoadAsset(aotMetadataDllPath, OnMetadataDllLoaded);
            MarkLoadingAsset(assetVO);
            
        }

        private  void OnMetadataDllLoaded(IAssetVO assetVo)
        {
            
            if (!assetVo.IsLoadSuccess)
            {
                OnLoadingAssetComplete(assetVo);
                Log.Error($"加载元数据 {assetVo.assetPath} 失败") ;
                return;
            }
            List<UnityEngine.Object> objects = assetVo.GetAsset<List<UnityEngine.Object>>();
            for (int i = 0; i < objects.Count; i++)
            {
                TextAsset textAsset = objects[i] as TextAsset;
                HomologousImageMode mode = HomologousImageMode.SuperSet;
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(textAsset.bytes, mode);
            }
            OnLoadingAssetComplete(assetVo);

        }

        public void RegisterGameModule<T>() where T : GameModuleBase
        {
            RegisterGameModule(typeof(T));
        }

        public void RegisterGameModule(Type gameModuelTye)
        {
            if (!TryGetGameModule(out GameModuleBase gameModule))
            {
                Log.Debug($"注册游戏模块 {gameModuelTye.Name}");
                GameModuleBase gameModuleInstance = Utility.ReflectionUtil.CreateInstance(gameModuelTye) as GameModuleBase;
                if (gameModuleInstance != null)
                {
                    Log.Debug($"创建游戏模块实例 {gameModuelTye.Name}");
                    RegiserModule(gameModuleInstance);
                    FunctionUtility.SafeCall(gameModuleInstance.AwakeModule);
                    
                }
                else
                {
                    Log.Error($"创建游戏模块实例失败{gameModuelTye}");
                }
            }
        }

        public void StartGameModule()
        {
            DoStartGameModules();
        }

        public bool TryGetGameModule<T>(out T gameModule) where T : GameModuleBase
        {
            Type type = typeof(T);
            if (m_Type2GameModuleCacheMap.TryGetValue(type, out var findGameModule))
            {
                gameModule = findGameModule as T;
                return true;
            }
            for (int i = 0; i < m_GameModules.Count; i++)
            {
                if (m_GameModules[i].GetType() == type)
                {
                    m_Type2GameModuleCacheMap.Add(type, m_GameModules[i]);
                    gameModule = m_GameModules[i] as T;
                    return true;
                }
            }
            gameModule = null;
            return false;
        }

        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            for (int i = 0; i < m_GameModules.Count; i++)
            {
                m_GameModules[i].DestroyModule();
            }
            m_Type2GameModuleCacheMap.Clear();
            m_GameModules.Clear();
        }

        protected override void OnInit()
        {
            base.OnInit();
            m_Type2GameModuleCacheMap = new Dictionary<Type, GameModuleBase>();
            m_GameModules = new List<GameModuleBase>();
            m_GameModuleHandlers = new List<GameModuleHandler>();
            m_GlobalHandlerGetCache = new Dictionary<Type, GameModuleHandler>();
        }

        private void CheckAllModuleReady()
        {
            bool isReady = true;
            for (int i = 0; i < m_GameModules.Count; i++)
            {
                if (!m_GameModules[i].IsLoaded)
                {
                    Log.Debug($"{m_GameModules[i].GetType().Name}正在加载中");
                    isReady = false;
                }
            }
            if (isReady )
            {
                Log.Debug($"所有游戏模块均已加载完成!");
                this.OnAllGameModuleReady();
            }
        }
        private void OnAllGameModuleReady()
        {
            CheckModuleHandlerLoad();
        }
        private void OnAllGameModuleAllHandlerReady()
        {
            DoEnableGameModules();
            if (this.m_OnAllGameModuleReadyCb != null)
            {
                FunctionUtility.SafeCall(this.m_OnAllGameModuleReadyCb);
                m_OnAllGameModuleReadyCb = null;
            }
        }
        private void CheckAllModuleHandlerReady()
        {
            bool isReady = true;
            for (int i = 0; i < m_GameModules.Count; i++)
            {
                if (!m_GameModules[i].IsHandlersLoaded)
                {
                    isReady = false;
                }
            }
            if (isReady)
            {
                this.OnAllGameModuleAllHandlerReady();
            }
        }
        private void DoEnableGameModules()
        {
            for (int i = 0; i < m_GameModules.Count; i++)
            {
                m_GameModules[i].EnableModule();
            }
        }
        private void DoStartGameModules()
        {
            for (int i = 0; i < m_GameModules.Count; i++)
            {
                m_GameModules[i].StartModule();
            }
        }

        private void GenerateGameModules()
        {
            var modules = GameModuleTypeList;
            for (int i = 0; i < modules.Count; i++)
            {
                RegisterGameModule(modules[i]);
            }
        }

        private void RegiserModule(GameModuleBase gameModule)
        {
            Type type = gameModule.GetType();
            m_Type2GameModuleCacheMap.Add(type, gameModule);
            m_GameModules.Add(gameModule);
        }

        public void RegisterGlobalHandler(GameModuleHandler handler)
        {
            if (!m_GameModuleHandlers.Contains(handler))
            {
                m_GameModuleHandlers.Add(handler);
            }
        }
        public T GetGlobalHandlerInstance<T>() where T : GameModuleHandler
        {
            Type type = typeof(T);
            if(m_GlobalHandlerGetCache.TryGetValue(type,out var handler) && handler!=null)
            {
                return handler as T;
            }
            for (int i = 0; i < m_GameModuleHandlers.Count; i++) { 

                var handlerType = m_GameModuleHandlers[i].GetType();
                handler = m_GameModuleHandlers[i];
                if (handlerType == type)
                {
                    m_GlobalHandlerGetCache.Add(handlerType, handler);
                    return handler as T;
                }
            }
            //m_GlobalHandlerGetCache.Add(type, null);
            return null;
        }
    }
}
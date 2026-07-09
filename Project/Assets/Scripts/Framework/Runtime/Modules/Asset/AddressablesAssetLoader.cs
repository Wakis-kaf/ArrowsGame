using Framework.Runtime.Base;
using Framework.Runtime.LogSystem;
using Framework.Runtime.Memory;
using Framework.Runtime.UI;
using Framework.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Framework.Runtime.MAsset
{
    public class AddressablesAssetLoader : UnitObject, IAssetLoader
    {
        private struct AssetLoadLambaCb : ILambadaCallback
        {
            internal AssetVO assetVo;
            internal Action<IAssetVO> cb;

            public ILambadaPool Pool { get;  set; }

            public void OnGet()
            {
                assetVo = null;
                cb = null;
            }

            public void OnPut()
            {
                
            }

            public void OnAssetLoadCompleted(AsyncOperationHandle<IList<UnityEngine.Object>> handle)
            {
                assetVo.AddAssetLoadCallback(cb);
                assetVo.SetAsset(handle.Result);
                Pool?.Put(this);
            }
            public void OnAssetLoadCompleted(UnityEngine.Object obj)
            {
                assetVo.AddAssetLoadCallback(cb);
                assetVo.SetAsset(obj);
                Pool?.Put(this);
            }
            public void OnAssetLoadCompleted(AsyncOperationHandle<UnityEngine.Object> handle)
            {
                assetVo.AddAssetLoadCallback(cb);
                assetVo.SetAsset(handle.Result);
                Pool?.Put(this);
            }
        }

        private LambadaPool<AssetLoadLambaCb> AssetLoadLambaPool = new LambadaPool<AssetLoadLambaCb>();
        public bool IsAssetExist(string path)
        {
            return true;
        }

        public IAssetVO LoadAssetAsync(string path, Action<IAssetVO> cb, int priority = 0)
        {
            return LoadAssetAsync(typeof(UnityEngine.Object), path, cb, priority);
        }

        public IAssetVO LoadAssetAsync(PathUrlOption pathOption, Action<IAssetVO> cb)
        {
            string path = pathOption.fileFullPath;
            Type assetType = pathOption.GetFileType();
            AssetVO assetVo = CreateAssetVO(assetType, pathOption.fileFullPath);
            if (pathOption.assetType == AssetType.AddressableGroupAsset)
            {
                try
                {
                    var assetLoad = AssetLoadLambaPool.Get();
                    assetLoad.assetVo = assetVo;
                    assetLoad.cb = cb;
                    Addressables.LoadAssetsAsync<UnityEngine.Object>(path, null).Completed += assetLoad.OnAssetLoadCompleted;
                    return assetVo;
                }
                catch (Exception e)
                {
                    Log.Error($"Addressables 加载{path}失败 ，原因{e.Message}");
                    assetVo.AddAssetLoadCallback(cb);
                    assetVo.SetAsset(null);
                    return assetVo;   
                }
              
            }
            else
            {
                return LoadAssetAsync(assetType, path, cb);
            }         
        }

        public IAssetVO LoadAssetAsync(Type type, string path, Action<IAssetVO> cb, int priority = 0)
        {
            //Log.Info($"AddressableAssetLoader : {path} {type}");
            AssetVO assetVo = CreateAssetVO(type, path);
            try
            {
                var assetLoad = AssetLoadLambaPool.Get();
                assetLoad.assetVo = assetVo;
                assetLoad.cb = cb;
                if (type == typeof(Sprite))
                {
                    Addressables.LoadAssetAsync<Sprite>(path).Completed += (handle) =>
                    {
                        assetLoad.OnAssetLoadCompleted(handle.Result);
                    };
                }
                else {
                    Addressables.LoadAssetAsync<UnityEngine.Object>(path).Completed += assetLoad.OnAssetLoadCompleted;
                }
                return assetVo;
            }
            catch (Exception e)
            {
                Log.Error($"Addressables 加载{path}失败 ，原因{e.Message}");
                assetVo.AddAssetLoadCallback(cb);
                assetVo.SetAsset(null);
                return assetVo;
            }
             
          
        }

        private AssetVO CreateAssetVO(Type type, string path)
        {
            AssetVO assetVo = new AssetVO(type, path, UnLoadAssetSync, UnLoadAssetAsync, null);
            return assetVo;
        }

        public IAssetVO LoadAssetSync(Type type, string path)
        {
            //Log.Info($"AddressableAssetLoader : {path}");
            Log.Error($"AddressableAssetLoader 不允许同步加载!{path}");
            return null;
        }

        public void UnLoadAllAsync()
        {
        }

        public void UnLoadAllSync()
        {
        }

        public void UnLoadAssetAsync(IAssetVO assetVO)
        {
            if (assetVO.IsLoadSuccess)
            {
#if !UNITY_EDITOR
                FunctionUtility.SafeCall(Addressables.Release, assetVO.GetUnRefAsset());
#endif
            }
            assetVO.Dispose();
        }

        public void UnLoadAssetSync(IAssetVO assetVO)
        {
            if (assetVO.IsLoadSuccess)
            {

#if !UNITY_EDITOR
                FunctionUtility.SafeCall(Addressables.Release, assetVO.GetUnRefAsset());
#endif
            }
            assetVO.Dispose();
        }

        public void InitLoader()
        {
        }

        public void StartResLoad()
        {
            // TODO:这里做其他事情
            OnAssetModuleLoaded();
        }

        private void OnAssetModuleLoaded()
        {
            GameApp.Ins.SendModuleUpdateMessage(new GameAppMessage(GameAppMessage.code_assetModule_loadSuccess));
        }

        public void CloseLoader()
        {
        }
    }
}
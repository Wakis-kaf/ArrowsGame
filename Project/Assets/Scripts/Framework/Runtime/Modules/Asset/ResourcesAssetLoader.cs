using Framework.Runtime.Base;
using Framework.Runtime.LogSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.Runtime.MAsset
{
    public class ResourcesAssetLoader : UnitObject, IAssetLoader
    {
        private AssetPool m_AssetPool;
        private HashSet<string> m_ResourcesHashNames;

        public ResourcesAssetLoader()
        {
            m_AssetPool = new AssetPool();
            m_ResourcesHashNames = new HashSet<string>(1024);
            m_AssetPool.SetAsyncLoadingVOCreator(AsyncLoadingVOCreator);
            m_AssetPool.SetAsyncLoadingUpdateHandler(AsyncLoadingUpdateHandler);
            m_AssetPool.SetAssetLoadSyncHandler(AssetLoadSyncHandle);
            m_AssetPool.SetAssetUnLoadAsyncHandler(AssetUnLoadAsyncHandler);
            m_AssetPool.SetAssetLoadAsyncInitHandler(AssetLoadAsyncInitHandler);
            m_AssetPool.SetAssetUnLoadSyncHandler(AssetUnLoadSyncHandler);
        }

        #region 外部接口

        public void InitLoader()
        {
            ReadFileList();
            GameApp.Ins.GameAppShell.StartCoroutine(PoolUpdateCoroutine());
        }

        public bool IsAssetExist(string assetPath)
        {
            string assetHashName = AssetUtil.GetAssetHashPath(assetPath);
            return m_ResourcesHashNames.Contains(assetHashName);
        }

        public IAssetVO LoadAssetAsync(Type type, string path, Action<IAssetVO> cb, int priority)
        {
            AssetVO assetVo = CreateAssetVO(type, path);
            if (!IsAssetExist(path))
            {
                Log.Error("Resources加载失败，资源不存在 " + path);
                cb?.Invoke(assetVo);
                return assetVo;
            }
            m_AssetPool.GetAssetAsync(type, path, (loadingVo) =>
            {
                assetVo.SetAsset(loadingVo.Asset);
                assetVo.AddAssetLoadCallback(cb);
                //assetVo.onAssetLoadedCallback?.Invoke(loadingVo.Asset);
                //cb?.Invoke(assetVo);
            }, priority);
            return assetVo;
        }

        public IAssetVO LoadAssetSync(Type type, string path)
        {
            AssetVO assetVo = CreateAssetVO(type, path);
            if (!IsAssetExist(path))
            {
                return assetVo;
            }
            var asset = m_AssetPool.GetAssetSync(type, path);
            assetVo.SetAsset(asset);
            //assetVo.asset = asset;
            //assetVo.onAssetLoadedCallback?.Invoke(asset);
            // 设置回收回调
            return assetVo;
        }

        public void UnLoadAllAsync()
        {
            m_AssetPool.UnLoadAllAsync();
        }

        public void UnLoadAllSync()
        {
            m_AssetPool.UnLoadAllSync();
        }


        public void UnLoadAssetAsync(IAssetVO assetVO)
        {
            m_AssetPool.UnLoadAsync(assetVO.assetPath);
            assetVO.Dispose();
        }

        public void UnLoadAssetSync(IAssetVO assetVO)
        {
            m_AssetPool.UnLoadSync(assetVO.assetPath);
            assetVO.Dispose();
        }

        private AssetVO CreateAssetVO(Type type, string path)
        {
            AssetVO assetVo = new AssetVO(type, path, UnLoadAssetSync, UnLoadAssetAsync, m_AssetPool.OnAssetGet);
            return assetVo;
        }

        #endregion 外部接口

        public void CloseLoader()
        {
            m_ResourcesHashNames.Clear();
            m_AssetPool.Close();
        }

        private void AssetLoadAsyncInitHandler(AssetLoadingVO loadingVo)
        {
            Log.Debug("Resources加载" + loadingVo.hashPath);
            loadingVo.loadingRequest = Resources.LoadAsync(loadingVo.hashPath);
        }

        private Object AssetLoadSyncHandle(AssetLoadingVO loadingVo)
        {
            string hashPath = loadingVo.hashPath;
            if (!IsAssetExist(hashPath))
            {
                Debug.LogError($"ResourcesLoadMgr No Find File {hashPath}");
                return null;
            }
            return Resources.Load(hashPath, loadingVo.loadType);
        }

        private bool AssetUnLoadAsyncHandler(AssetLoadingVO loadingVo)
        {
            if (loadingVo.Asset is GameObject || loadingVo.Asset is Component) return false;
            Resources.UnloadAsset(loadingVo.Asset);
            return true;
        }

        private bool AssetUnLoadSyncHandler(AssetLoadingVO loadingVo)
        {
            if (loadingVo.Asset is GameObject || loadingVo.Asset is Component) return false;
            Resources.UnloadAsset(loadingVo.Asset);
            return true;
        }

        private bool AsyncLoadingUpdateHandler(AssetLoadingVO loadingVo)
        {
            if (loadingVo.loadingRequest is ResourceRequest request)
            {
                if (request.isDone)
                {
                    loadingVo.Asset = request.asset;
                    return true;
                }
                return false;
            }
            else
            {
                loadingVo.Asset = null;
                return true;
            }
        }

        private AssetLoadingVO AsyncLoadingVOCreator(string hashPath)
        {
            return new AssetLoadingVO(hashPath);
        }

        private IEnumerator PoolUpdateCoroutine()
        {
            while (GameApp.Ins.GameApplicationMainState == GameAppMainState.Playing)
            {
                yield return null;
                m_AssetPool.Update();
            }
        }

        private void ReadFileList()
        {
            // 读取ResourcesList 文件
            TextAsset textAsset = Resources.Load<TextAsset>("FileList");
            if (textAsset == null)
            {
                Log.Error("Resources File List Not Exist");
                return;
            }
            string txt = textAsset.text;
            txt = txt.Replace("\r\n", "\n");
            foreach (var line in txt.Split('\n'))
            {
                if (string.IsNullOrEmpty(line)) continue;
                string hashName = AssetUtil.GetAssetHashPath(line);
                m_ResourcesHashNames.Add(hashName);
            }
        }
    }
}
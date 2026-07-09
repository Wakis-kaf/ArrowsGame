#if UNITY_EDITOR

using Framework.Runtime.Base;
using Framework.Runtime.LogSystem;
using System;
using System.Collections;
using UnityEditor;
using UnityEditor.VersionControl;
using static Sirenix.OdinInspector.Editor.UnityPropertyEmitter;
using Object = UnityEngine.Object;

namespace Framework.Runtime.MAsset
{
    public class EditorAssetLoader : UnitObject, IAssetLoader
    {
        private AssetPool m_AssetPool;

        public EditorAssetLoader()
        {
            m_AssetPool = new AssetPool();
            m_AssetPool.SetAsyncLoadingVOCreator(AsyncLoadingVOCreator);
            m_AssetPool.SetAsyncLoadingUpdateHandler(AsyncLoadingUpdateHandler);
            m_AssetPool.SetAssetLoadSyncHandler(AssetLoadSyncHandle);
            m_AssetPool.SetAssetUnLoadAsyncHandler(AssetUnLoadAsyncHandler);
            m_AssetPool.SetAssetUnLoadSyncHandler(AssetUnLoadSyncHandler);
            GameApp.Ins.GameAppShell.StartCoroutine(LoadAssetCoroutine());
        }

        private IEnumerator LoadAssetCoroutine()
        {
            while (GameApp.Ins.GameApplicationMainState == GameAppMainState.Playing)
            {
                yield return null;
                m_AssetPool.Update();
            }
        }

        private bool AssetUnLoadSyncHandler(AssetLoadingVO loadingVo)
        {
            loadingVo.Asset = null;
            return true;
        }

        private bool AssetUnLoadAsyncHandler(AssetLoadingVO loadingVo)
        {
            loadingVo.Asset = null;
            return true;
        }

        private Object AssetLoadSyncHandle(AssetLoadingVO loadingVo)
        {
            string hashPath = loadingVo.hashPath;
            if (string.IsNullOrEmpty(hashPath))
            {
                Log.Error($"LoadAssetByXpath from editor  Sync path error !{hashPath} is empty");
                return null;
            }
            return AssetDatabase.LoadAssetAtPath(hashPath, loadingVo.loadType);
        }

        private bool AsyncLoadingUpdateHandler(AssetLoadingVO loadingVo)
        {
            Object asset = AssetLoadSyncHandle(loadingVo);
            if (asset != null)
            {
                loadingVo.Asset = asset;
            }
            loadingVo.Asset = null;
            return true;
        }

        private AssetLoadingVO AsyncLoadingVOCreator(string hashPath)
        {
            return new AssetLoadingVO(hashPath);
        }

        public IAssetVO LoadAssetSync(string path)
        {
            return LoadAssetSync(typeof(UnityEngine.Object), path);
        }

        public IAssetVO LoadAssetSync(Type type, string path)
        {
            AssetVO assetVo = CreateAssetVO(type, path);
            if (!IsAssetExist(path, type))
            {
                Log.Error("EditorAsset加载失败，资源不存在" + path);
                return assetVo;
            }
            var asset = m_AssetPool.GetAssetSync(type, path);
            assetVo.SetAsset(asset);

            //assetVo.asset = asset;
            //assetVo.onAssetLoadedCallback?.Invoke(asset);
            // 设置回收回调
            return assetVo;
        }

        private AssetVO CreateAssetVO(Type type, string path)
        {
            AssetVO assetVo = new AssetVO(type, path, UnLoadAssetSync, UnLoadAssetAsync, m_AssetPool.OnAssetGet);
            return assetVo;
        }

        public IAssetVO LoadAssetAsync(Type type, string path, Action<IAssetVO> cb, int priority)
        {
            AssetVO assetVo = CreateAssetVO(type, path);
            if (!IsAssetExist(path, type))
            {
                Log.Error("EditorAsset加载失败，资源不存在" + path);
                cb?.Invoke(assetVo);
                return assetVo;
            }
            m_AssetPool.GetAssetAsync(type, path, (loadingVo) =>
            {
                // 编辑器下是同步加载，所以统一延后一帧
                GameApp.Ins.GameAppShell.StartCoroutine(AssetAsyncLoadDelay(loadingVo, assetVo, cb));
            }, priority);
            return assetVo;
        }

        private IEnumerator AssetAsyncLoadDelay(AssetLoadingVO loadingVo, IAssetVO assetVo, Action<IAssetVO> cb)
        {
            yield return null;
            assetVo.SetAsset(loadingVo.Asset);
            assetVo.AddAssetLoadCallback(cb);
            //assetVo.onAssetLoadedCallback?.Invoke(loadingVo.Asset);
            //cb?.Invoke(assetVo);
        }

        public void UnLoadAssetSync(IAssetVO assetVO)
        {
            m_AssetPool.UnLoadSync(assetVO.assetPath);
            assetVO.Dispose();
        }

        public void UnLoadAssetAsync(IAssetVO assetVO)
        {
            m_AssetPool.UnLoadAsync(assetVO.assetPath);
            assetVO.Dispose();
        }

        public void UnLoadAllSync()
        {
            m_AssetPool.UnLoadAllSync();

        }

        public void UnLoadAllAsync()
        {
            m_AssetPool.UnLoadAllAsync();
        }

        public void CloseLoader()
        {
            m_AssetPool.Close();
        }
        private bool IsAssetExist(string assetPath, Type type)
        {
            // 只能同步加载判断是否有
            return m_AssetPool.GetAssetSync(type, assetPath) != null;
        }
        public bool IsAssetExist(string assetPath)
        {
            // 只能同步加载判断是否有
            return m_AssetPool.GetAssetSync(typeof(Object), assetPath) != null;
        }
    }
}

#endif
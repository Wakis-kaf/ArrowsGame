using Framework.Runtime.LogSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.Runtime.MAsset
{
    public enum AssetStatus
    {
        Init = 0,
        Loading = 1,
        LoadSuccess = 2,
        LoadFail = 3,
        LoadCancel = 4,
        UnLoading,
        //UnLoaded,
    }

    public class AssetLoadingVO
    {
        public System.Action<AssetLoadingVO> asyncLoadCallBack;
        public System.Action asyncUnLoadCallBack;
        public string hashPath;
        public object loadingRequest;
        public int loadPriority; // 加载优先级
        public Type loadType;
        public AssetStatus status;
        public int waitingCount;
        private Object m_Asset;
        private int m_ReferCount = 0;

        public AssetLoadingVO(string path)
        {
            hashPath = path;
        }

        public Object Asset
        {
            set
            {
                m_Asset = value;
                if (value != null)
                {
                    asyncLoadCallBack?.Invoke(this);
                    asyncLoadCallBack = null;
                    waitingCount = 0;
                }
            }
            get
            {
                return m_Asset;
            }
        }

        public int ReferCount => m_ReferCount;

        public void AddReferCount(int step)
        {
            m_ReferCount = m_ReferCount + step;
            m_ReferCount = Mathf.Max(0, m_ReferCount);
        }

        public bool HasAsset()
        {
            return m_Asset != null;
        }

        public bool IsInit()
        {
            return status == AssetStatus.Init;
        }

        public bool IsLoaded()
        {
            return IsLoadSuccess() || IsLoadFail();
        }

        public bool IsLoadFail()
        {
            return status == AssetStatus.LoadFail;
        }

        public bool IsLoading()
        {
            return status == AssetStatus.Loading;
        }

        public bool IsLoadSuccess()
        {
            return status == AssetStatus.LoadSuccess;
        }

        public bool IsUnLoading()
        {
            return status == AssetStatus.UnLoading;
        }

        public void OnUnLoaded()
        {
            m_ReferCount = 0;
            waitingCount = 0;
        }
    }

    public class AssetPool
    {
        private Action<AssetLoadingVO> m_AssetLoadAsyncInitHandler;
        private Func<AssetLoadingVO, Object> m_AssetLoadSyncHandler;
        private Dictionary<string, AssetLoadingVO> m_AssetPool = new Dictionary<string, AssetLoadingVO>();
        private Func<AssetLoadingVO, bool> m_AssetUnLoadAsyncHandler;
        private Func<AssetLoadingVO, bool> m_AssetUnLoadSyncHandler;
        private Func<AssetLoadingVO, bool> m_AsyncLoadingUpdateHandler;
        private Func<string, AssetLoadingVO> m_AsyncLoadingVOCreator;
        private List<AssetLoadingVO> m_AsyncLoadingVOList = new List<AssetLoadingVO>(128);
        private Action m_OnAssetEmptyHandler;

        public int allReferCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < m_AsyncLoadingVOList.Count; i++)
                {
                    count += m_AsyncLoadingVOList[i].ReferCount;
                }

                return count;
            }
        }

        public int LoadingCount
        {
            get
            {
                return m_AsyncLoadingVOList.Count;
            }
        }

        public void Close()
        {
            UnLoadAllSync();
            m_AssetPool.Clear();
            m_AsyncLoadingVOList.Clear();
            m_AsyncLoadingVOCreator = null;
            m_AsyncLoadingUpdateHandler = null;
            m_AssetLoadSyncHandler = null;
            m_AssetUnLoadAsyncHandler = null;
            m_AssetUnLoadSyncHandler = null;
            m_AssetLoadAsyncInitHandler = null;
            m_OnAssetEmptyHandler = null;
        }

        public void GetAssetAsync(string hashPath, Action<AssetLoadingVO> prefabLoadedCallback,
            int loadPriority = 0)
        {
            GetAssetAsync(typeof(Object), hashPath, prefabLoadedCallback, loadPriority);
        }

        public void GetAssetAsync(Type type, string hashPath, Action<AssetLoadingVO> assetLoadedCallback, int loadPriority = 0)
        {
            hashPath = AssetUtil.GetHashPath(hashPath);
            var loadingVo = GetLoadingVO(hashPath);
            if (loadingVo.IsLoaded())
            {
                assetLoadedCallback?.Invoke(loadingVo);
                return;
            }
            if (loadingVo.IsLoading())
            {
                loadingVo.loadPriority = loadPriority;
                loadingVo.waitingCount += 1;
                loadingVo.asyncLoadCallBack += assetLoadedCallback;
            }
            if (loadingVo.IsUnLoading())
            {
                // 直接同步卸载
                UnLoadAsyncToSync(hashPath);
            }

            if (loadingVo.IsInit())
            {
                m_AssetLoadAsyncInitHandler?.Invoke(loadingVo);
                loadingVo.status = AssetStatus.Loading;
                loadingVo.hashPath = hashPath;
                loadingVo.loadType = type;
                loadingVo.loadPriority = loadPriority;
                loadingVo.waitingCount = 1;
                loadingVo.asyncLoadCallBack += assetLoadedCallback;
            }
        }

        public Object GetAssetSync(string hashPath)
        {
            return GetAssetSync(typeof(Object), hashPath);
        }

        public Object GetAssetSync(Type type, string hashPath)
        {
            hashPath = AssetUtil.GetHashPath(hashPath);
            var loadingVo = GetLoadingVO(hashPath);
            if (loadingVo == null)
            {
                Log.Error($"loadingVo null error {hashPath}");
                return null;
            }
            if (loadingVo.IsLoaded())
            {
                return loadingVo.Asset;
            }
            if (loadingVo.IsLoading())
            {
                // 异步加载转同步
                return GetAssetAsyncToSync(hashPath);
            }
            if (loadingVo.IsUnLoading())
            {
                // 异步卸载转同步卸载
                UnLoadAsyncToSync(hashPath);
            }
            if (loadingVo.IsInit())
            {
                loadingVo.loadType = type;
                Object asset = m_AssetLoadSyncHandler?.Invoke(loadingVo);
                if (asset != null)
                {
                    SetLoadSuc(loadingVo);
                    loadingVo.Asset = asset;
                    return loadingVo.Asset;
                }
                else
                {
                    SetLoadFail(loadingVo);
                    loadingVo.Asset = null;
                    return null;
                }
            }
            return null;
        }

        public void OnAssetGet(IAssetVO assetVO)
        {
            string assetPath = assetVO.assetPath;
            var loadingVo = GetLoadingVO(assetPath);
            if (loadingVo == null)
            {
                return;
            }
            // 触发计数
            loadingVo.AddReferCount(1);
        }

        public void SetAssetEmptyHandler(Action assetEmptyHandler)
        {
            m_OnAssetEmptyHandler = assetEmptyHandler;
        }

        public void SetAssetLoadAsyncInitHandler(Action<AssetLoadingVO> assetLoadAsyncInitHandler)
        {
            m_AssetLoadAsyncInitHandler = assetLoadAsyncInitHandler;
        }

        public void SetAssetLoadSyncHandler(Func<AssetLoadingVO, Object> assetLoadSyncHandler)
        {
            m_AssetLoadSyncHandler = assetLoadSyncHandler;
        }

        public void SetAssetUnLoadAsyncHandler(Func<AssetLoadingVO, bool> assetUnLoadAsyncHandler)
        {
            m_AssetUnLoadAsyncHandler = assetUnLoadAsyncHandler;
        }

        public void SetAssetUnLoadSyncHandler(Func<AssetLoadingVO, bool> assetUnLoadSyncHandler)
        {
            m_AssetUnLoadSyncHandler = assetUnLoadSyncHandler;
        }

        public void SetAsyncLoadingUpdateHandler(Func<AssetLoadingVO, bool> asyncLoadingUpdateHandler)
        {
            m_AsyncLoadingUpdateHandler = asyncLoadingUpdateHandler;
        }

        public void SetAsyncLoadingVOCreator(Func<string, AssetLoadingVO> asyncLoadingVOCreator)
        {
            m_AsyncLoadingVOCreator = asyncLoadingVOCreator;
        }

        public void UnLoadAllAsync()
        {
            var keys = m_AssetPool.Keys;
            for (int i = 0; i < keys.Count; i++)
            {
                UnLoadAsync(keys.ElementAt(i));
            }
        }

        public void UnLoadAllSync()
        {
            var keys = m_AssetPool.Keys;
            for (int i = 0; i < keys.Count; i++)
            {
                UnLoadSync(keys.ElementAt(i));
            }
        }

        public void UnLoadAsync(string hashPath)
        {
            var loadingVo = FindLoadingVO(hashPath);
            if (loadingVo == null || !loadingVo.IsLoaded()) return;
            loadingVo.AddReferCount(-1);
            if (loadingVo.ReferCount == 0)
            {
                // 设为卸载
                loadingVo.status = AssetStatus.UnLoading;
                // 清空相关数据
                loadingVo.waitingCount = 0;
                loadingVo.asyncLoadCallBack = null;
            }
        }

        public void UnLoadSync(string hashPath)
        {
            var loadingVo = FindLoadingVO(hashPath);
            if (loadingVo == null || !loadingVo.IsLoaded()) return;
            loadingVo.AddReferCount(-1);
            if (loadingVo.ReferCount == 0)
            {
                if (m_AssetUnLoadSyncHandler != null && m_AssetUnLoadSyncHandler.Invoke(loadingVo))
                {
                    SetUnLoaded(loadingVo);
                }
            }
        }

        public void Update()
        {
            m_AsyncLoadingVOList.Sort(LoadSortCMP);
            for (int i = 0; i < m_AsyncLoadingVOList.Count; i++)
            {
                var loadingVo = m_AsyncLoadingVOList[i];
                if (loadingVo.IsLoaded() || loadingVo.status == AssetStatus.Init) continue;
                if (loadingVo.IsLoading() && m_AsyncLoadingUpdateHandler != null)
                {
                    if (m_AsyncLoadingUpdateHandler.Invoke(loadingVo))
                    {
                        if (loadingVo.Asset != null)
                        {
                            SetLoadSuc(loadingVo);
                        }
                        else
                        {
                            SetLoadFail(loadingVo);
                        }
                    }
                }
                if (loadingVo.IsUnLoading() && m_AssetUnLoadAsyncHandler != null)
                {
                    if (m_AssetUnLoadAsyncHandler.Invoke(loadingVo))
                    {
                        SetUnLoaded(loadingVo);
                    }
                }
            }
            int count = m_AsyncLoadingVOList.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var vo = m_AsyncLoadingVOList[i];
                if (vo.status == AssetStatus.Init)
                {
                    m_AssetPool.Remove(m_AsyncLoadingVOList[i].hashPath);
                    m_AsyncLoadingVOList.RemoveAt(i);
                }
            }

            // 如果当前全部都处于
            if (m_AsyncLoadingVOList.Count == 0)
            {
                m_OnAssetEmptyHandler?.Invoke();
            }
        }

        private AssetLoadingVO FindLoadingVO(string path)
        {
            path = AssetUtil.GetHashPath(path);
            if (m_AssetPool.TryGetValue(path, out var vo))
            {
                return vo;
            }
            return null;
        }

        private Object GetAssetAsyncToSync(string hashPath)
        {
            var loadingVo = GetLoadingVO(hashPath);
            if (loadingVo == null || !loadingVo.IsLoading()) return null;
            Object asset = m_AssetLoadSyncHandler?.Invoke(loadingVo);
            if (asset != null)
            {
                SetLoadSuc(loadingVo);
                loadingVo.Asset = asset;
                return loadingVo.Asset;
            }
            else
            {
                SetLoadFail(loadingVo);
                loadingVo.Asset = null;
                return null;
            }
        }

        private AssetLoadingVO GetLoadingVO(string path)
        {
            path = AssetUtil.GetHashPath(path);
            if (m_AssetPool.ContainsKey(path))
            {
                return m_AssetPool[path];
            }
            var loadingVo = m_AsyncLoadingVOCreator?.Invoke(path);
            if (loadingVo != null)
            {
                m_AssetPool.Add(path, loadingVo);
                m_AsyncLoadingVOList.Add(loadingVo);
            }
            return loadingVo;
        }

        private int LoadSortCMP(AssetLoadingVO vo1, AssetLoadingVO vo2)
        {
            // 处理 null 情况
            if (vo1 == null && vo2 == null) return 0;
            if (vo1 == null) return 1; // null 排在后面
            if (vo2 == null) return -1; // null 排在后面

            return vo2.loadPriority.CompareTo(vo1.loadPriority);
        }

        private void SetLoadFail(AssetLoadingVO loadingVo)
        {
            loadingVo.status = AssetStatus.LoadFail;
            loadingVo.asyncUnLoadCallBack = null;
        }

        private void SetLoadSuc(AssetLoadingVO loadingVo)
        {
            loadingVo.status = AssetStatus.LoadSuccess;
            loadingVo.asyncUnLoadCallBack = null;
            loadingVo.loadingRequest = null;
        }

        private void SetUnLoaded(AssetLoadingVO loadingVo)
        {
            loadingVo.status = AssetStatus.Init;
            loadingVo.waitingCount = 0;
            loadingVo.OnUnLoaded();
            loadingVo.asyncLoadCallBack = null;
            loadingVo.asyncUnLoadCallBack?.Invoke();
            loadingVo.asyncUnLoadCallBack = null;
            loadingVo.loadingRequest = null;
        }

        private void UnLoadAsyncToSync(string hashPath)
        {
            var loadingVo = FindLoadingVO(hashPath);
            if (loadingVo == null || !loadingVo.IsUnLoading()) return;
            //if (m_AssetUnLoadSyncHandler != null && m_AssetUnLoadSyncHandler.Invoke(loadingVo))
            //{
            SetUnLoaded(loadingVo); // 直接算卸载完成就行，防止内存重复卸载
            //}
        }
    }
}
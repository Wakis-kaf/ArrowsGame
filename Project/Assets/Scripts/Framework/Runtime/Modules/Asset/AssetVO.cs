using Framework.Runtime.LogSystem;
using Framework.Runtime.UnitSystem.Base;
using System;
using UnityEngine;

namespace Framework.Runtime.MAsset
{
    /*
     * 负责提供一个更轻便灵活的资源加载接口
     *
     */

    public class AssetVO : BehaviourUnit, IAssetVO
    {
        public AssetVO(Type type,
            string path,
            Action<IAssetVO> unLoadSync,
            Action<IAssetVO> unLoadAsync,
            Action<IAssetVO> onAssetGet) : base()
        {
            this.m_AssetType = type;
            this.m_AssetPath = path;
            this.m_UnLoadSync = unLoadSync;
            this.m_UnLoadAsync = unLoadAsync;
            this.m_OnAssetGet = onAssetGet;
        }

        public AssetVO(object asset)
        {
            this.SetAsset(asset);
        }

        public string assetPath { get => m_AssetPath; }
        public bool IsLoaded => m_IsLoaded;
        public bool IsLoadSuccess => m_IsLoadSuccess;
        public void SetAssetLoadCallback(Action<IAssetVO> cb)
        {
            if (this.IsLoaded)
            {
                cb?.Invoke(this);
                return;
            }
            onAssetLoadCallback = cb;
        }

        public void AddAssetLoadCallback(Action<IAssetVO> cb)
        {
            if (this.IsLoaded)
            {
                cb?.Invoke(this);
                return;
            }
            onAssetLoadCallback -= cb;
            onAssetLoadCallback += cb;
        }

        public object GetAsset()
        {
            if (IsDisposed)
            {
                Log.Error("尝试访问已经回收的资源!!");
                return null;
            }
            if (m_Asset != null)
            {
                if (m_WaitingCount > 0)
                {
                    m_WaitingCount = m_WaitingCount - 1;
                    m_WaitingCount = Mathf.Max(m_WaitingCount, 0);
                }
                else
                {
                    m_OnAssetGet?.Invoke(this);
                }
            }
            return m_Asset;
        }

        public T GetAsset<T>()
        {
            if (IsDisposed)
            {
                Log.Error("尝试访问已经回收的资源!!");
                return default;
            }
            if (m_Asset != null && m_Asset is T typeAsset)
            {
                if (m_WaitingCount > 0)
                {
                    m_WaitingCount = m_WaitingCount - 1;
                    m_WaitingCount = Mathf.Max(m_WaitingCount, 0);
                }
                else
                {
                    m_OnAssetGet?.Invoke(this);
                }
                return typeAsset;
            }
            return default;
        }

        public GameObject GetInstance(Transform parent = null)
        {
            if (GetAsset() is GameObject prefab)
            {
                return GameObject.Instantiate(prefab, parent);
            }
            return null;
        }

        public object GetUnRefAsset()
        {
            return m_Asset;
        }

        public void RemoveAssetLoadCallback(Action<IAssetVO> cb)
        {
            onAssetLoadCallback -= cb;
        }

        public void SetAsset(object asset)
        {
            this.m_Asset = asset;
            this.m_IsLoaded = true;
            this.m_IsLoadSuccess = this.m_Asset != null;
            this.onAssetLoadCallback?.Invoke(this);
            this.onAssetLoadCallback = null;
        }

        public void UnLoadAsync()
        {
            if (this.IsDisposed) return;
            m_WaitingCount = m_WaitingCount > 0 ? m_WaitingCount - 1 : m_WaitingCount;
            m_WaitingCount = Mathf.Max(m_WaitingCount, 0);
            m_UnLoadAsync?.Invoke(this);
        }

        public void UnLoadSync()
        {
            if (this.IsDisposed) return;
            m_WaitingCount = m_WaitingCount > 0 ? m_WaitingCount - 1 : m_WaitingCount;
            m_WaitingCount = Mathf.Max(m_WaitingCount, 0);
            m_UnLoadSync?.Invoke(this);
        }

        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            UnLoadSync();
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            m_Asset = null;
            m_AssetType = null;
            m_IsLoaded = false;
            m_IsLoadSuccess = false;
            m_WaitingCount = 0;
            m_AssetPath = "";
            m_UnLoadAsync = null;
            m_UnLoadSync = null;
            m_OnAssetGet = null;
            onAssetLoadCallback = null;
        }

        private object m_Asset;
        private string m_AssetPath;
        private Type m_AssetType;
        private bool m_IsLoaded;
        private bool m_IsLoadSuccess;
        private Action<IAssetVO> m_OnAssetGet;
        private Action<IAssetVO> m_UnLoadAsync;
        private Action<IAssetVO> m_UnLoadSync;
        private int m_WaitingCount = 1;
        private Action<IAssetVO> onAssetLoadCallback;
    }
}
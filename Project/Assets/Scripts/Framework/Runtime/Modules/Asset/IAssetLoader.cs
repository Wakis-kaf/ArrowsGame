using System;
using UnityEngine;

namespace Framework.Runtime.MAsset
{
    public interface IAssetLoader
    {
        public bool IsAssetExist(string path);

        public IAssetVO LoadAssetAsync(Type type, string path, Action<IAssetVO> cb, int priority);

        public IAssetVO LoadAssetSync(Type type, string path);

        public void UnLoadAllAsync();

        public void UnLoadAllSync();

        public void UnLoadAssetAsync(IAssetVO assetVO);

        public void UnLoadAssetSync(IAssetVO assetVO);
    }

    public interface IAssetVO : IDisposable
    {
        public string assetPath { get; }

        public bool IsLoaded { get; }

        public bool IsLoadSuccess { get; }

        public void AddAssetLoadCallback(Action<IAssetVO> cb);
        public void SetAssetLoadCallback(Action<IAssetVO> cb);

        public object GetAsset();

        public T GetAsset<T>();

        public GameObject GetInstance(Transform parent = null);

        public object GetUnRefAsset();

        public void RemoveAssetLoadCallback(Action<IAssetVO> cb);

        public void SetAsset(object asset);

        public void UnLoadAsync();

        public void UnLoadSync();
    }
}
using Framework.Runtime.Base;
using System;
using UnityEngine.Scripting;

namespace Framework.Runtime.Storage
{
    /// <summary>
    /// 主要负责不同平台的资源缓存和读取 包括GameConfig.json、资源包以及玩家数据 例如微信缓存
    /// </summary>
    public class PlatformStorage : UnitObject, IPlatformStorageHelper
    {
        public static PlatformStorage Instance => GameApp.Ins.PlatformStorage;
        private IPlatformStorageHelper m_PlatformStorageHelper;

        [Preserve]
        public PlatformStorage()
        {
            //#if UNITY_STANDALONE_WIN
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_ANDROID
            SetStorageHelper(new IOStorageHelper());
#elif UNITY_WXGAME
            SetStorageHelper(new WXPlaftormStorageHelper());

#endif
        }

        public bool IsStorageFileExistSync(string path)
        {
            return m_PlatformStorageHelper.IsStorageFileExistSync(path);
        }

        public void SetStorageHelper(IPlatformStorageHelper helper)
        {
            m_PlatformStorageHelper = helper;
        }

        public bool TryGetStorageSync(string path, out byte[] value)
        {
            return m_PlatformStorageHelper.TryGetStorageSync(path, out value);
        }

        public void TrySaveStorage(string path, byte[] value, Action<object> sucCb = null, Action<object> failCb = null)
        {
            m_PlatformStorageHelper.TrySaveStorage(path, value, sucCb, failCb);
        }

        public void AppUpdate(GameAppMessage appMessage)
        {
            m_PlatformStorageHelper.AppUpdate(appMessage);
        }

        public void Init()
        {
            m_PlatformStorageHelper.Init();
        }

        public void Start()
        {
            m_PlatformStorageHelper.Start();
        }

        public virtual bool IsStorageDirectoryExistSync(string dirPath)
        {
            return m_PlatformStorageHelper.IsStorageDirectoryExistSync(dirPath);
        }

        public virtual void IsStorageDirectoryExist(string dirPath, Action<object> sucCb = null, Action<object> failCb = null)
        {
            m_PlatformStorageHelper.IsStorageDirectoryExist(dirPath, sucCb, failCb);
        }

        public virtual bool CreateStorageDirectorySync(string dirPath)
        {
            return m_PlatformStorageHelper.CreateStorageDirectorySync(dirPath);
        }

        public bool TrySaveStorageSync(string path, byte[] value)
        {
            return this.m_PlatformStorageHelper.TrySaveStorageSync(path, value);
        }

        public void TryDeleteDirectory(string dirPath, bool recursive = true, Action<object> sucCb = null, Action<object> failCb = null)
        {
            this.m_PlatformStorageHelper.TryDeleteDirectory(dirPath, recursive, sucCb, failCb);
        }

        public void TryGetStorage(string path, Action<object> sucCb = null, Action<object> failCb = null)
        {
            m_PlatformStorageHelper.TryGetStorage(path, sucCb, failCb);
        }
    }
}
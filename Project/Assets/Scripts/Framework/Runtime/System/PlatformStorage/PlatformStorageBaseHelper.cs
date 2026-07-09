using System;

namespace Framework.Runtime.Storage
{
    public abstract class PlatformStorageBaseHelper : IPlatformStorageHelper
    {
        public virtual void AppUpdate(GameAppMessage appMessage)
        {
        }

        public abstract bool CreateStorageDirectorySync(string dirPath);

        public virtual void Init()
        {
        }

        public abstract void IsStorageDirectoryExist(string dirPath, Action<object> sucCb = null, Action<object> failCb = null);

        public abstract bool IsStorageDirectoryExistSync(string dirPath);

        public abstract bool IsStorageFileExistSync(string path);

        public virtual void Start()
        {
        }

        public abstract void TryDeleteDirectory(string dirPath, bool recursive = true, Action<object> sucCb = null, Action<object> failCb = null);

        public abstract void TryGetStorage(string path, Action<object> sucCb = null, Action<object> failCb = null);

        public abstract bool TryGetStorageSync(string path, out byte[] value);

        public abstract void TrySaveStorage(string path, byte[] value, Action<object> sucCb = null, Action<object> failCb = null);

        public abstract bool TrySaveStorageSync(string path, byte[] value);
    }
}
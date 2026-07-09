using System;

namespace Framework.Runtime.Storage
{
    public interface IPlatformStorageHelper
    {
        void AppUpdate(GameAppMessage appMessage);

        bool CreateStorageDirectorySync(string dirPath);

        void Init();

        void IsStorageDirectoryExist(string dirPath, Action<object> sucCb = null, Action<object> failCb = null);

        bool IsStorageDirectoryExistSync(string dirPath);

        bool IsStorageFileExistSync(string path);

        void Start();

        void TryDeleteDirectory(string dirPath, bool recursive = true, Action<object> sucCb = null, Action<object> failCb = null);

        void TryGetStorage(string path, Action<object> sucCb = null, Action<object> failCb = null);

        bool TryGetStorageSync(string path, out byte[] value);

        void TrySaveStorage(string path, byte[] value, Action<object> sucCb = null, Action<object> failCb = null);

        bool TrySaveStorageSync(string path, byte[] value);
    }
}
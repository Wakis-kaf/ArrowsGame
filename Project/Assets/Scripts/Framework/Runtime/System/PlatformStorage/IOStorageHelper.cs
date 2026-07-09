using Framework.Runtime.LogSystem;
using Framework.Utils;

using System;

namespace Framework.Runtime.Storage
{
    public class IOStorageHelper : PlatformStorageBaseHelper
    {
        public override bool CreateStorageDirectorySync(string dirPath)
        {
            dirPath = GetUserDataPath(dirPath);
            return Utility.FileUtil.CreateDirectory(dirPath);
        }

        public string GetUserDataPath(string path)
        {
            return Utility.Path.PathCombine(GameEnv.Path.platformDir, path);
        }

        public override void IsStorageDirectoryExist(string dirPath, Action<object> sucCb = null, Action<object> failCb = null)
        {
            dirPath = GetUserDataPath(dirPath);
            if (Utility.FileUtil.IsDirectoryExist(dirPath))
            {
                sucCb?.Invoke(true);
            }
        }

        public override bool IsStorageDirectoryExistSync(string dirPath)
        {
            dirPath = GetUserDataPath(dirPath);
            return Utility.FileUtil.IsDirectoryExist(dirPath);
        }

        public override bool IsStorageFileExistSync(string path)
        {
            path = GetUserDataPath(path);
            return Utility.FileUtil.IsFileExist(path);
        }

        public override void TryDeleteDirectory(string dirPath, bool recursive = true, Action<object> sucCb = null, Action<object> failCb = null)
        {
            dirPath = GetUserDataPath(dirPath);
            if (Utility.FileUtil.DeleteDir(dirPath, recursive))
            {
                sucCb?.Invoke(null);
            }
            else
            {
                failCb?.Invoke(null);
            }
        }

        public override async void TryGetStorage(string path, Action<object> sucCb = null, Action<object> failCb = null)
        {
            path = GetUserDataPath(path);
            try
            {
                byte[] datas = await Utility.FileUtil.TryGetFileAsync(path);
                sucCb?.Invoke(datas);
            }
            catch (System.Exception e)
            {
                Log.Error($"异步加载文件失败:{path} {e.Message}");
                failCb?.Invoke(null);
            }
        }

        public override bool TryGetStorageSync(string path, out byte[] value)
        {
            path = GetUserDataPath(path);
            return Utility.FileUtil.TryGetFile(path, out value);
        }

        public override async void TrySaveStorage(string path, byte[] value, Action<object> sucCb = null, Action<object> failCb = null)
        {
            path = GetUserDataPath(path);

            try
            {
                await Utility.FileUtil.TrySaveStorage(path, value);
                sucCb?.Invoke(true);
                Log.Debug($"存档保存成功{path}");
            }
            catch (System.Exception e)
            {
                Log.Error($"存档保存失败: {e.Message}");
                failCb?.Invoke(null);
            }
            //if (Utility.FileUtil.TrySaveFile(path, value))
            //{
            //    sucCb?.Invoke(true);
            //    return;
            //}
            //failCb?.Invoke(null);
            //return;
        }

        public override bool TrySaveStorageSync(string path, byte[] value)
        {
            path = GetUserDataPath(path);
            return Utility.FileUtil.TrySaveFile(path, value);
        }
    }
}
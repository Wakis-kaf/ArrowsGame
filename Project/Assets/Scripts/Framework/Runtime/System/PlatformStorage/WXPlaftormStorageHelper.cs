#if UNITY_WXGAME

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Framework.Runtime.LogSystem;
using UnityEngine.Windows;
using WeChatWASM;

namespace Framework.Runtime.Storage
{
    public class WXPlaftormStorageHelper : PlatformStorageBaseHelper
    {
        public override bool TryGetStorageSync(string path, out byte[] value)
        {
            try
            {
                value = default;
                string dir = path.Substring(0, path.LastIndexOf("/"));
                if (!IsStorageDirectoryExistSync(dir)) return false;
                if (!IsStorageFileExistSync(path)) return false;
                var fs = WX.GetFileSystemManager();
                path = GetUserDataPath(path);
                Log.Info("WXPlaftormStorageHelper 同步加载文件" + path);
                value = fs.ReadFileSync(path);
                return value != null;
            }
            catch (Exception e)
            {
                Log.Info($"WXPlaftormStorageHelper 同步加载文件失败{path} {e}");
                value = default;
                return false;
            }
        }

        public override void TryGetStorage(string path,
            Action<object> sucCb = null,
            Action<object> failCb = null)
        {
            try
            {
                var fs = WX.GetFileSystemManager();
                path = GetUserDataPath(path);
                Log.Info("WXPlaftormStorageHelper 异步 加载文件" + path);
                ReadFileParam param = new ReadFileParam();
                param.filePath = path;

                if (sucCb != null)
                {
                    param.success = (obj) => sucCb?.Invoke(obj.binData);
                }
                if (failCb != null)
                {
                    param.fail = (obj) => failCb?.Invoke(null);
                }
                fs.ReadFile(param);
            }
            catch (Exception e)
            {
                failCb?.Invoke(null);
            }
        }

        public string GetUserDataPath(string path)
        {
            return $"{WX.env.USER_DATA_PATH}/{path}";
        }

        public override bool IsStorageFileExistSync(string path)
        {
            try
            {
                path = GetUserDataPath(path);
                var fs = WX.GetFileSystemManager();
                string res = fs.AccessSync(path);
                Log.Info($"判断微信文件是否存在 {path} ,rew{res}");
                if (res == "access:ok")
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Info($"同步判断微信文件是否存在失败 {path} ,res{e}");
                return false;
            }
        }

        public override bool TrySaveStorageSync(string path, byte[] value)
        {
            try
            {
                path = GetUserDataPath(path);

                var fs = WX.GetFileSystemManager();
                string res = fs.WriteFileSync(path, value);
                if (res == "ok")
                {
                    Log.Info("WXPlaftormStorageHelper 保存文件 成功");
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Info("WXPlaftormStorageHelper 保存文件 失败" + e.Message);
                return false;
            }
        }

        public override void TrySaveStorage(string path, byte[] value, Action<object> sucCb = null, Action<object> failCb = null)
        {
            try
            {
                path = GetUserDataPath(path);
                //Log.Info("WXPlaftormStorageHelper 保存文件" + path);
                var fs = WX.GetFileSystemManager();
                WriteFileParam writeFileParam = new WriteFileParam();
                writeFileParam.filePath = path;
                writeFileParam.data = value;
                writeFileParam.fail = failCb;
                writeFileParam.success = sucCb;
                fs.WriteFile(writeFileParam);
            }
            catch (Exception e)
            {
                Log.Info($"WXPlaftormStorageHelper 保存文件失败 {e}");
                failCb?.Invoke(null);
            }
        }

        public override bool CreateStorageDirectorySync(string dirPath)
        {
            dirPath = GetUserDataPath(dirPath);
            try
            {
                if (IsStorageDirectoryExistSync(dirPath)) return true;
                var fs = WX.GetFileSystemManager();
                string res = fs.MkdirSync(dirPath, true);
                Log.Info($"创建微信目录成功 {dirPath} ,res {res}");
                return true;
            }
            catch (Exception e)
            {
                Log.Info($"创建微信目录 {dirPath} 失败 {e.Message}");
                return false;
            }
        }

        public override bool IsStorageDirectoryExistSync(string dirPath)
        {
            try
            {
                dirPath = GetUserDataPath(dirPath);
                var fs = WX.GetFileSystemManager();
                string res = fs.AccessSync(dirPath);
                Log.Info($"判断微信目录是否存在 {dirPath} ,rew{res}");
                if (res == "access:ok")
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Info($"微信目录不存在{dirPath} ,{e}");
                return false;
            }
        }

        public override void IsStorageDirectoryExist(string dirPath, Action<object> sucCb = null, Action<object> failCb = null)
        {
            try
            {
                dirPath = GetUserDataPath(dirPath);
                var fs = WX.GetFileSystemManager();
                AccessParam accessParam = new AccessParam();
                accessParam.fail = failCb;
                accessParam.success = sucCb;
                accessParam.path = dirPath;
                fs.Access(accessParam);
                Log.Info($"判断微信目录是否存在 {dirPath} ");
            }
            catch (Exception e)
            {
                failCb?.Invoke(e);
            }
        }

        public override void TryDeleteDirectory(string dirPath, bool recursive = true, Action<object> sucCb = null, Action<object> failCb = null)
        {
            try
            {
                if (!IsStorageDirectoryExistSync(dirPath))
                {
                    Log.Info($"删除目录失败,目录不存在{dirPath}");
                    failCb?.Invoke(null);
                    return;
                }
                dirPath = GetUserDataPath(dirPath);
                var fs = WX.GetFileSystemManager();
                RmdirParam param = new RmdirParam();
                param.dirPath = dirPath;
                param.recursive = recursive;
                if (sucCb != null)
                {
                    param.success = (obj) => sucCb?.Invoke(obj);
                }
                if (failCb != null)
                {
                    param.fail = (obj) => failCb?.Invoke(obj);
                }
                fs.Rmdir(param);
            }
            catch (Exception e)
            {
                Log.Info($"删除目录失败{dirPath} ,{e}");
                failCb?.Invoke(null);
            }
        }
    }
}

#endif
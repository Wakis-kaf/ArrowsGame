using Framework.Runtime.LogSystem;
using Framework.Runtime.Module.Core;
using Framework.Runtime.Storage;
using Framework.Runtime.UnitSystem.BIInterfaces;
using Framework.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Framework.Runtime.Archives
{
    public struct SaveTask
    {
        public Archive archive;
        public bool isOverride;
        public Action beforeSaveCb;
        public Action sucCb;
        public Action failCb;
    }
    public static class ArchiveTypeCode
    {
        public const int SystemData = 1;
        public const int GameArchive = 0;
    }

    public static class ArchiveStatusCode
    {
        public const int NoError = 0;
        public const int Damaged = -1;
    }
    public sealed partial class ArchiveModule : ModuleUnit, IUnitDestroy, IUnitUpdate
    {
        public class ArchiveInfo
        {
            public string systemVersion; // 系统版本
            public string gameVersion; // 游戏版本
            public string dirPath; // 存档文件夹全路径
            public List<string> archiveFileList = new List<string>(); //    存档文件列表
            public string createTime; // 存储时间
            public string updateTime; // 创建时间
            public int archiveType; // 存档路径
            public string guidID;
            public string typeName;
            public string archiveName;
            public int archiveStatus;
            public string GetInfo()
            {
                return $"【systemVersion:{systemVersion}】;【gameVersion:{gameVersion}】;【dirPath:{dirPath}】；【createTime:{createTime}】；【updateTime:{updateTime}】；【archiveType:{archiveType}】；【guidID:{guidID}】;【typeName:{typeName}】;【archiveName:{archiveName}】;【archiveStatus:{archiveStatus}】";
            }
        }

        private class ArchiveInfoMap
        {
            public List<ArchiveInfo> archiveInfos = new List<ArchiveInfo>();
            private Dictionary<string, ArchiveInfo> m_GuidID2InfoMap = new Dictionary<string, ArchiveInfo>();

            public void CheckMap()
            {
                for (int i = 0; i < archiveInfos.Count; i++)
                {
                    m_GuidID2InfoMap.Add(archiveInfos[i].guidID, archiveInfos[i]);
                }
            }

            public bool Contains(string archiveId)
            {
                return m_GuidID2InfoMap.ContainsKey(archiveId);
            }

            public List<ArchiveInfo> GetAllArchiveInfo(int archiveType)
            {
                List<ArchiveInfo> res = new List<ArchiveInfo>();
                for (int i = 0; i < archiveInfos.Count; i++)
                {
                    if (archiveInfos[i].archiveType == archiveType)
                        res.Add(archiveInfos[i]);
                }

                return res;
            }

            public void SaveArchive(Archive archive, string dirPath, string archiveFullPath)
            {
                ArchiveInfo info = null;
                bool isExist = true;
                if (!m_GuidID2InfoMap.TryGetValue(archive.Id, out info))
                {
                    info = new ArchiveInfo();
                    isExist = false;
                }

                info.archiveType = archive.ArchiveType;
                info.createTime = archive.CreateTime.ToString();
                info.updateTime = archive.UpdateTime.ToString();
                info.typeName = archive.Type.FullName;
                info.archiveName = archive.Name;
                info.systemVersion = archiveSystemVersion;
                info.dirPath = dirPath;
                info.guidID = archive.Id;
                if (!info.archiveFileList.Contains(archiveFullPath))
                {
                    info.archiveFileList.Add(archiveFullPath);
                }

                if (!isExist)
                {
                    archiveInfos.Add(info);
                    m_GuidID2InfoMap.Add(archive.Id, info);
                    SortArchiveInfos();
                }
            }
            private void SortArchiveInfos()
            {
                archiveInfos.Sort(ArhiveInfoSort);
            }
            private int ArhiveInfoSort(ArchiveInfo info1, ArchiveInfo info2)
            {
                DateTime time1 = DateTime.Parse(info1.createTime);
                DateTime time2 = DateTime.Parse(info2.createTime);
                return time2.CompareTo(time1);
            }
            public bool TryGetNoErrorInfo(string fullName, string archiveFileName, out ArchiveInfo info)
            {
                info = null;
                for (int i = 0; i < archiveInfos.Count; i++)
                {
                    var archiveInfo = archiveInfos[i];
                    if (archiveInfo.typeName == fullName &&
                        archiveInfo.archiveName == archiveFileName
                        && archiveInfo.archiveStatus == ArchiveStatusCode.NoError)
                    {
                        info = archiveInfo;
                        return true;
                    }
                }

                return false;
            }

            public bool ClearAllArchive(int archiveType)
            {
                for (int i = archiveInfos.Count - 1; i >= 0; i--)
                {
                    if (archiveInfos[i].archiveType == archiveType)
                    {
                        DeleteArchive(archiveInfos[i], false);
                    }
                }
                SortArchiveInfos();
                return true;
            }

            public void DeleteArchive(ArchiveInfo archiveInfo, bool isUpdate = true)
            {
                archiveInfos.Remove(archiveInfo);
                m_GuidID2InfoMap.Remove(archiveInfo.guidID);
                if (isUpdate)
                {
                    SortArchiveInfos();
                }

            }
        }

        private ArchiveManager m_ArchiveManager;
        private Dictionary<Type, object> m_LoadCallback = new Dictionary<Type, object>();
        private Dictionary<Type, object> m_SaveCallback = new Dictionary<Type, object>();
        private string archiveMapJsonName = "archiveMap.json";
        private ArchiveInfoMap m_ArchiveInfoMap;
        private string m_MapInfoJsonPath;
        private string m_SaveDirPath;
        private LinkedList<SaveTask> saveTasks = new LinkedList<SaveTask>();
        public string relativePath = "Archives"; // 相对路径
        public static string archiveSystemVersion = "1.0"; //  版本号
        public SaveDirPath savePath = SaveDirPath.PersistencePath;
        public SaveMode saveMode = SaveMode.Json;
        private List<Archive> m_AutoUpdateArchives = new List<Archive>();
        public string MapInfoJsonPath
        {
            get
            {
                if (string.IsNullOrEmpty(m_MapInfoJsonPath))
                {
                    m_MapInfoJsonPath = Path.Combine(GetSaveDirPath(), archiveMapJsonName);
                }

                return m_MapInfoJsonPath;
            }
        }

        public List<ArchiveInfo> GetAllArchiveInfo(int archiveType)
        {
            return m_ArchiveInfoMap.GetAllArchiveInfo(archiveType);
        }

        public override void OnAppUpdate(GameAppMessage appMessage)
        {
            base.OnAppUpdate(appMessage);
            if (appMessage.MessageCode == GameAppMessage.code_gameConfig_loadSuccess)
            {
                // 读取文档存档文件
                ReadArchiveMapJson();
                GameSettingPrefers.Init();
            }
        }
        public void SetAutoSave(Archive archive, bool isAutoSave)
        {
            if (isAutoSave && !m_AutoUpdateArchives.Contains(archive))
            {
                m_AutoUpdateArchives.Add(archive);
            }
            else if (!isAutoSave)
            {
                m_AutoUpdateArchives.Remove(archive);
            }
        }
        /// <summary>
        /// 注册存档读保存回调事件
        /// </summary>
        /// <param name="readCallBack"></param>
        /// <typeparam name="T"></typeparam>
        public void RegisterSave<T>(Action<T> saveCallBack) where T : Archive
        {
            var type = typeof(T);
            if (!m_SaveCallback.ContainsKey(type))
            {
                m_SaveCallback.Add(type, saveCallBack);
            }
            else
            {
                Action<T> action = (Action<T>)m_SaveCallback[type];
                action += saveCallBack;
            }
        }

        /// <summary>
        /// 触发存档保存事件
        /// </summary>
        /// <param name="archive"></param>
        /// <typeparam name="T"></typeparam>
        private void InvokeSave<T>(T archive) where T : Archive
        {
            if (m_SaveCallback.TryGetValue(typeof(T), out object actonObj))
            {
                Action<T> action = (Action<T>)actonObj;
                action?.Invoke(archive);
            }
        }

        protected override void OnModuleConstructed()
        {
            m_ArchiveManager = new ArchiveManager();
            m_ArchiveManager.SetHelper(ArchiverSerializerFactory.CreatSerializer(saveMode));
            if (GameEnv.ArchiveConfig.enableArchiveEncrypt)
            {
                m_ArchiveManager.SetEntryptor(ArchiverEntryprorFactory.CreatEncryptor(saveMode));
            }
            GameApp.Ins.GameAppShell.StartCoroutine(this.ArchiveSaveCotoutine());
        }

        private void ReadArchiveMapJson()
        {
            string mapJsonPath = GetMapInfoJsonPath();
            Log.Debug($"存档模块配置文件读取中，读取路径 {mapJsonPath} ");
            if (PlatformStorage.Instance.TryGetStorageSync(mapJsonPath, out byte[] jsonBytes))
            {
                Log.Debug($"存档模块配置文件本地读取成功");
                m_ArchiveInfoMap = Utility.Json.ToObject<ArchiveInfoMap>(UTF8Encoding.UTF8.GetString(jsonBytes));
                if (m_ArchiveInfoMap == null)
                {
                    ClearUnusedArchive();
                    m_ArchiveInfoMap = new ArchiveInfoMap();
                }
            }
            else
            {
                ClearUnusedArchive();
                m_ArchiveInfoMap = new ArchiveInfoMap();
            }
            m_ArchiveInfoMap.CheckMap();
        }

        private void ClearUnusedArchive()
        {
            var dir = GetSaveDirPath();
        }

        public void OnUnitDestroy()
        {
            GameSettingPrefers.Save();
            SaveMapInfo();
        }

        /// <summary>
        /// 移除存档回调事件
        /// </summary>
        /// <param name="saveCallBack"></param>
        /// <typeparam name="T"></typeparam>
        public void RemoveSave<T>(Action<T> saveCallBack) where T : Archive
        {
            var type = typeof(T);
            if (m_SaveCallback.ContainsKey(type))
            {
                Action<T> action = (Action<Archive>)m_SaveCallback[type];
                action -= saveCallBack;
            }
        }

        /// <summary>
        /// 注册存档读取回调事件
        /// </summary>
        /// <param name="loadCallBack"></param>
        /// <typeparam name="T"></typeparam>
        public void RegisterLoad<T>(Action<T> loadCallBack) where T : Archive
        {
            var type = typeof(T);
            if (!m_LoadCallback.ContainsKey(type))
            {
                m_LoadCallback.Add(type, loadCallBack);
            }
            else
            {
                Action<T> action = (Action<T>)m_LoadCallback[type];
                action += loadCallBack;
            }
        }

        /// <summary>
        /// 移除存档读取事件
        /// </summary>
        /// <param name="loadCallBack"></param>
        /// <typeparam name="T"></typeparam>
        public void RemoveLoad<T>(Action<T> loadCallBack) where T : Archive
        {
            var type = typeof(T);
            if (m_LoadCallback.ContainsKey(type))
            {
                Action<T> action = (Action<T>)m_LoadCallback[type];
                action -= loadCallBack;
            }
        }

        /// <summary>
        /// 创建一个存档
        /// </summary>
        public T CreateArchive<T>(string archiveName = "", bool isOverride = false) where T : Archive
        {
            return CreateArchive(typeof(T), archiveName, isOverride) as T;
        }

        public Archive CreateArchive(Type type, string archiveName = "", bool isOverride = false)
        {
            var archive = Utility.ReflectionUtil.CreateInstance(type) as Archive; // 新建一个存档
            archive.InitInfo(archiveSystemVersion, archiveName);
            SaveArchive(archive, isOverride);
            return archive;
        }

        private IEnumerator ArchiveSaveCotoutine()
        {
            while (GameApp.Ins.GameApplicationMainState != GameAppMainState.Destroyed)
            {
                LinkedListNode<SaveTask> current = saveTasks.First;
                while (current != null)
                {
                    var task = current.Value;
                    if (!task.archive.isWriting)
                    {
                        task.beforeSaveCb?.Invoke();
                        SaveArchiveImmediate(
                            task.archive,
                            task.isOverride,
                            task.sucCb,
                            task.failCb);
                        task.archive.IsDirty = false;
                        task.archive.SaveWaiting = false;
                        saveTasks.Remove(current);
                    }
                    current = current.Next;
                }
                yield return null;
            }
        }

        public void SaveArchiveImmediate(Archive archive,
            bool isOverride = true,
            Action sucCb = null,
            Action failCb = null)
        {
            string archiveFileName = archive.GetArchiveFileName();

            if (HasArchive(archive))
            {
                if (!isOverride)
                {
                    Log.ErrorFormat("Save Archive Error! Target has already exist ! {0}", archive.Name);
                    failCb?.Invoke();
                    return;
                }
            }
            archive.isWriting = true;
            string archiveDirPath = GetArchiveSavePath(archive);
            string archiveFullPath =
                Utility.Path.PathCombine(archiveDirPath, archive.Name + ArchiveFilePrefix.GetPrefix(saveMode));
            bool isDirExist = false;
            if (PlatformStorage.Instance.IsStorageDirectoryExistSync(archiveDirPath))
            {
                isDirExist = true;
            }
            else
            {
                isDirExist = PlatformStorage.Instance.CreateStorageDirectorySync(archiveDirPath);
            }
            if (isDirExist)
            {
                SaveArchiveFile(archive, archiveDirPath, archiveFullPath, sucCb, failCb);
            }
            else
            {
                Log.Error($"存档目录不存在 {archiveDirPath}");
            }
        }

        public void SaveArchive(Archive archive, bool isOverride = true, Action sucCb = null, Action failCb = null, Action beforeSaveCb = null)
        {
            if (archive.IsDirty && !archive.SaveWaiting)
            {
                saveTasks.AddLast(new SaveTask()
                {
                    archive = archive,
                    isOverride = isOverride,
                    sucCb = sucCb,
                    failCb = failCb,
                    beforeSaveCb = beforeSaveCb,
                });
                archive.SaveWaiting = true;
            }
        }

        public void ClearAllArchive(int archiveType)
        {
            var archiveInfos = GetAllArchiveInfo(archiveType);
            if (m_ArchiveInfoMap.ClearAllArchive(archiveType))
            {
                for (int i = archiveInfos.Count - 1; i >= 0; i--)
                {
                    DeleteArchive(archiveInfos[i]);
                }
            }

            // 保存 Map 数据
            SaveMapInfo();
        }

        public void DeleteArchive(ArchiveInfo archiveInfo)
        {
            // 删除文件
            try
            {
                PlatformStorage.Instance.TryDeleteDirectory(archiveInfo.dirPath, true, (obj) =>
                {
                    Log.Info($"删除存档文件成功{archiveInfo.dirPath} {archiveInfo.archiveName}");
                }, (obj) =>
                {
                    Log.Error($"删除存档文件失败{archiveInfo.dirPath} {archiveInfo.archiveName}");
                });
                //Utility.FileUtil.DeleteDir(archiveInfo.dirPath, true);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public void LoadArchive(ArchiveInfo info, Action<Archive, bool> loadCb)
        {
            LoadArchive<Archive>(info.archiveName, loadCb);
        }

        public T LoadArchiveSync<T>(string archiveFileName, bool loadOrCreate = false) where T : Archive
        {
            if (!HasNoErrorArchive<T>(archiveFileName, out var info))
            {
                if (loadOrCreate)
                {
                    var archive = CreateArchive<T>(archiveFileName, true);
                    return archive;
                }
                return null;
            }
            string archiveFullPath = info.archiveFileList[0];
            if (PlatformStorage.Instance.TryGetStorageSync(archiveFullPath,
                out byte[] bytes))
            {
                var archive = m_ArchiveManager.DeSerialize<T>(bytes);
                if (archive != null)
                {
                    archive.OnAfterDeSerialize();
                    InvokeLoad(archive);
                    return archive;
                }
                else
                {
                    info.archiveStatus = ArchiveStatusCode.Damaged;
                    SaveMapInfo();
                    Log.Error($"存档解析失败{info.GetInfo()}");
                    return LoadArchiveSync<T>(archiveFileName, loadOrCreate);
                }
            }
            else
            {
                info.archiveStatus = ArchiveStatusCode.Damaged;
                SaveMapInfo();
                Log.Error($"存档加载失败{info.GetInfo()}");
                return LoadArchiveSync<T>(archiveFileName, loadOrCreate);
            }

        }

        /// <summary>
        /// 加载存档
        /// </summary>
        /// <param name="archiveFileName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void LoadArchive<T>(string archiveFileName,
            Action<T, bool> loadCb,
            bool loadOrCreate = false) where T : Archive
        {
            if (!HasNoErrorArchive<T>(archiveFileName, out var info))
            {
                Log.Error($" 存档不存在 ! {archiveFileName}");
                if (loadOrCreate)
                {
                    var archive = CreateArchive<T>(archiveFileName, true);
                    loadCb.Invoke(archive, false);
                    return;
                }
                loadCb.Invoke(null, false);
                return;
            }
            Action<object> onArchiveLoadFail = (data) =>
            {
                // 存档损坏
                info.archiveStatus = ArchiveStatusCode.Damaged;
                SaveMapInfo();
                // 尝试加载下一个存档
                LoadArchive<T>(archiveFileName, loadCb, loadOrCreate);
            };
            try
            {
                string archiveFullPath = info.archiveFileList[0];
                Action<object> onArchiveLoadSuc = (data) =>
                {
                    bool isLoadSuc = true;
                    if (data is byte[] bytes)
                    {
                        var archive = m_ArchiveManager.DeSerialize<T>(bytes);
                        if (archive != null)
                        {
                            archive?.OnAfterDeSerialize();
                            loadCb?.Invoke(archive, isLoadSuc);
                            InvokeLoad(archive);
                        }
                        else
                        {
                            // 加载失败
                            Log.Error($"存档解析失败{info.GetInfo()}");
                            onArchiveLoadFail(data);
                        }
                    }
                    else
                    {
                        Log.Error($"存档加载失败{info.GetInfo()}");
                        onArchiveLoadFail(data);
                    }
                };

                PlatformStorage.Instance.TryGetStorage(archiveFullPath, onArchiveLoadSuc, onArchiveLoadFail);
            }
            catch (Exception e)
            {
                Log.Error(e);
                onArchiveLoadFail(null);
                //info.archiveStatus = ArchiveStatusCode.Damaged;
                //SaveMapInfo();
                //if (loadOrCreate)
                //{
                //    var archive = CreateArchive<T>(archiveFileName, true);
                //    loadCb?.Invoke(archive, false);
                //    InvokeLoad(archive);
                //}
                //else
                //{
                //    loadCb?.Invoke(null, false);
                //}

            }
        }

        /// <summary>
        /// 加载存档
        /// </summary>
        /// <param name="archiveFileName"></param>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T LoadArchive<T>(string archiveFileName) where T : Archive
        {
            var archive = LoadArchive<T>(archiveFileName);
            return archive;
        }

        /// <summary>
        /// 触发存档读取事件
        /// </summary>
        /// <param name="archive"></param>
        /// <typeparam name="T"></typeparam>
        private void InvokeLoad<T>(T archive) where T : Archive
        {
            if (m_LoadCallback.TryGetValue(typeof(T), out object actonObj))
            {
                Action<T> action = (Action<T>)actonObj;
                action?.Invoke(archive);
            }
        }

        private void SaveMapInfo()
        {
            var dir = GetSaveDirPath();
            bool isDirExist = false;
            if (!PlatformStorage.Instance.IsStorageDirectoryExistSync(dir))
            {
                isDirExist = PlatformStorage.Instance.CreateStorageDirectorySync(dir);
                if (isDirExist)
                {
                    Log.Error("创建存档目录成功" + isDirExist);
                }
            }
            else
            {
                isDirExist = true;
            }
            if (isDirExist)
            {
                bool res = PlatformStorage.Instance.TrySaveStorageSync(GetMapInfoJsonPath(),
                    UTF8Encoding.UTF8.GetBytes(Utility.Json.ToJson(m_ArchiveInfoMap)));
                if (res)
                {
                    Log.Debug("保存存档Info文件成功" + res);
                }
                else
                {
                    Log.Error("保存存档Info文件失败");
                }
            }
            else
            {
                Log.Debug("存档目录不存在");
            }
        }

        private bool HasNoErrorArchive<T>(string archiveFileName, out ArchiveInfo info) where T : Archive
        {
            return m_ArchiveInfoMap.TryGetNoErrorInfo(typeof(T).FullName, archiveFileName, out info);
        }

        private bool HasArchive(Archive archive)
        {
            return m_ArchiveInfoMap.Contains(archive.Id);
        }

        private string GetMapInfoJsonPath()
        {
            return MapInfoJsonPath;
        }

        private string GetArchiveSavePath(Archive archive)
        {
            string dirName = string.Join("_", archive.GetArchiveFileName(), archive.Id);
            // 格式： 存档名_时间_Guid
            return Utility.Path.PathCombine(GetSaveDirPath(archive.ArchiveType), dirName);
        }

        private string GetArchiveTypeDir(int type)
        {
            switch (type)
            {
                case 1:
                    return "SystemData";

                default:
                    return "GameArchive";
            }
        }

        private string GetSaveDirPath(int type)
        {
            return Path.Combine(GetSaveDirPath(), GetArchiveTypeDir(type));
        }

        private string GetSaveDirPath()
        {
            return relativePath;
        }

        private void SaveArchiveFile(Archive archive, string archiveDirPath, string archiveFullPath, Action sucCb = null, Action failCb = null)
        {
            try
            {
                archive.OnBeforeSerialize();
                InvokeSave(archive);
                byte[] bytes = m_ArchiveManager.GetSerializeBytes(archive);
                archive.OnAfterSerialize(bytes != null);
                if (bytes != null)
                {
                    PlatformStorage.Instance.TrySaveStorage(archiveFullPath, bytes, (obj) =>
                    {
                        // 保存存档
                        m_ArchiveInfoMap.SaveArchive(archive, archiveDirPath, archiveFullPath);
#if UNITY_EDITOR
                        // 编辑器下刷新
                        AssetDatabase.Refresh();
#endif
                        // 保存 Map 数据
                        SaveMapInfo();
                        sucCb?.Invoke();
                        archive.isWriting = false;
                    }, (obj) =>
                    {
                        failCb?.Invoke();
                        archive.isWriting = false;
                    });
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                failCb?.Invoke();
                archive.isWriting = false;
            }
        }

        public void OnUnitUpdate()
        {
            for (int i = m_AutoUpdateArchives.Count - 1; i >= 0; i--)
            {
                if (m_AutoUpdateArchives[i].IsDirty)
                {
                    m_AutoUpdateArchives[i].Save();
                    m_AutoUpdateArchives[i].IsDirty = false;
                }
            }
        }
    }
}
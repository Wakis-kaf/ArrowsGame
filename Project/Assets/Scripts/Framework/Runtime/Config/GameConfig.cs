using CustomLitJson;
using Framework.Runtime.LogSystem;
using Framework.Runtime.Storage;
using Framework.Utils;
using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

namespace Framework.Runtime
{
    public enum PlatformType
    {
        Pc,
        Android,
        Webgl,
        Ipa
    }

    public enum ResRequestType
    {
        CommonUI
    }

    public static partial class GameConfig
    {
        public const string FieldName_GameConfigJson = "gameconfig.json";
        public const string FieldName_GameConfigJsonNoExtension = "gameconfig";

        public static string NameField_ResBuildDir = "AddressableResources";
    }

    public static partial class GameConfig
    {
        private static JsonData gameConfigJson;
        private static string targetPlafromtDir;
        public static bool IsGameCfgLoadedSuc { private set; get; }
        public static bool IsGameConfigJsonLoaded { private set; get; }
        private static Action OnGameConfigReadSuccessCb = null;
        private static Action OnGameConfigReadFailCb = null;

        private static void OnGameConfigReadSuccess()
        {
            OnGameConfigReadSuccessCb?.Invoke();
            ReadGameConfig();
            CheckAndSendGameCfgLoadMsg();
        }

        private static void OnGameConfigReadFail()
        {
            OnGameConfigReadFailCb?.Invoke();
        }

        public static void AppUpdate(GameAppMessage appMessage)
        {
            if (appMessage.MessageCode == GameAppMessage.code_gameSytem_start)
            {
                ReadConfig();
            }
        }

        public static string GetAssetCDNPath()
        {
            return "";
        }

        public static void Init()
        {
            InjectConfig();
        }

        public static void ReadConfig()
        {
            ClearConfig();
            if (GameEnv.ResConfig.IsStorageFirstRes)
            {
                TryReadStorageGameConfig();
            }
            else
            {
                TryReadInPackageGameConfig();
            }
        }

        private static void TryReadStorageGameConfig()
        {
            if (GameEnv.TryGetGameConfigJsonStoragePath(out string configStoragePath, out ResLoadWay resLoadWay))
            {
                DoReadGameConfig(resLoadWay, configStoragePath, null, TryReadInPackageGameConfig);
            }
            else
            {
                TryReadInPackageGameConfig();
            }
        }

        private static void TryReadInPackageGameConfig()
        {
            if (GameEnv.TryGetInPackageGameConfigJsonPath(out string path, out ResLoadWay loadWay))
            {
                DoReadGameConfig(loadWay, path, GameConfigStorageCheck);
            }
        }

        private static void GameConfigStorageCheck()
        {
            if (GameEnv.ResConfig.IsStorageFirstRes)
            {
                StorageGameConfig();
            }
        }

        private static void StorageGameConfig()
        {
            if (GameEnv.TryGetGameConfigJsonStoragePath(out string configStoragePath, out ResLoadWay resLoadWay))
            {
                string jsonStr = gameConfigJson.ToJson();
                if (PlatformStorage.Instance.TrySaveStorageSync(configStoragePath, Encoding.UTF8.GetBytes(jsonStr)))
                {
                    StorageGameConfigSuc(null);
                }
                else
                {
                    StorageGameConfigFail(null);
                }
            }
            else
            {
                StorageGameConfigFail(null);
            }
        }

        private static void StorageGameConfigSuc(object data)
        {
            Log.Debug("GameConfig本地保存成功");
            ReadGameConfig();
        }

        private static void StorageGameConfigFail(object data)
        {
            Log.Debug("GameConfig本地保存失败");
        }

        private static void DoReadGameConfig(ResLoadWay loadWay, string path, Action sucCb = null, Action failCb = null)
        {
            OnGameConfigReadSuccessCb = sucCb;
            OnGameConfigReadFailCb = failCb;
            if (loadWay == ResLoadWay.Resources)
            {
                TextAsset textAsset = Resources.Load<TextAsset>(path);
                if (textAsset == null || string.IsNullOrEmpty(textAsset.text))
                {
                    Log.Fatal($"resources 读取gameconfig 失败，错误：{path}");
                    OnGameConfigReadFail();
                    return;
                }
                Log.Debug($"resources 读取gameconfig 成功! {textAsset.text}");
                if (DecodeGameConfigStr(textAsset.text))
                {
                    OnGameConfigReadSuccess();
                }
                else
                {
                    OnGameConfigReadFail();
                }
            }
            else if (loadWay == ResLoadWay.Web)
            {
                WebGlPlatformConfigLoad(path);
            }
            else if (loadWay == ResLoadWay.IO)
            {
                // 延后一帧
                //NormalPlatformConfigLoad(path);
                GameApp.Ins.GameAppShell.StartCoroutine(NormalPlatformDelayConfigLoad(path));
                return;
            }
            else if (loadWay == ResLoadWay.Storage)
            {
                if (PlatformStorage.Instance.TryGetStorageSync(path, out byte[] bytes) &&
                    DecodeGameConfigStr(UTF8Encoding.UTF8.GetString(bytes)))
                {
                    OnGameConfigReadSuccess();
                }
                else
                {
                    OnGameConfigReadFail();
                    return;
                }
            }
        }

        private static IEnumerator NormalPlatformDelayConfigLoad(string path)
        {
            yield return null;
            NormalPlatformConfigLoad(path);
        }

        public static void Start()
        {
        }

        public static void Stop()
        {
        }

        public static bool TryDecodeFromGameCfg<T>(string name, ref T res)
        {
            if (Utility.Json.TryGetValue(gameConfigJson, name, out T getres))
            {
                res = getres;
                return true;
            }
            return false;
            //return Utility.Json.TrySetValue(gameConfigJson, name, ref res);
        }

        public static bool TryGetDebuggerGameCfg<T>(string name, ref T res)
        {
            return TryDecodeSubFromGameCfg("debuggerConfig", name, ref res);
        }

        public static bool TryGetLogGameCfg<T>(string name, ref T res)
        {
            return TryDecodeSubFromGameCfg("logConfig", name, ref res);
        }

        public static bool TryGetLuaGameCfg<T>(string name, ref T res)
        {
            return TryDecodeSubFromGameCfg("luaConfig", name, ref res);
        }

        public static bool TrySetResGameCfg<T>(string name, T res)
        {
            return TryEncodeSubFromGameCfg("resourcesConfig", name, res);
        }

        public static bool TryGetResGameCfg<T>(string name, ref T res)
        {
            return TryDecodeSubFromGameCfg("resourcesConfig", name, ref res);
        }

        private static void CheckAndSendGameCfgLoadMsg()
        {
            if (IsGameCfgLoadedSuc)
            {
                GameApp.Ins.SendSystemUpdateMessage(new GameAppMessage(GameAppMessage.code_gameConfig_loadSuccess));
            }
        }

        private static void ClearConfig()
        {
            targetPlafromtDir = "";
        }

        private static bool DecodeGameConfig(string configPath)
        {
            if (Application.isPlaying)
                Log.Debug($"读取游戏配置文件{configPath}");
            string jsonStr = string.Empty;
            if (!File.Exists(configPath))
            {
                Log.Fatal($"{configPath} not found path {configPath}!");
                return false;
            }

            jsonStr = Utility.FileUtil.ReadFile(configPath);
            return DecodeGameConfigStr(jsonStr);
        }

        private static bool DecodeGameConfigStr(string jsonStr)
        {
            var jsonData = Utility.Json.ReadJson(jsonStr);
            if (jsonData == null)
            {
                Log.Fatal($"read gameconfig.json error");
                return false;
            }

            gameConfigJson = jsonData;
            IsGameConfigJsonLoaded = true;
            IsGameCfgLoadedSuc = true;
            return true;
        }

        private static string GetGameConfigRequestUrl(string url)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (GameEnv.WebConfig.EnableUrlStamptime)
            {
                // 加上时间戳，防止缓存
                string timestamp = DateTime.Now.Ticks.ToString();
                url += "?t=" + timestamp;
            }

#endif
            return url;
        }

        private static void InjectConfig()
        {
            if (FrameworkSetting.Instance != null)
            {
                if (Application.isPlaying)
                {
                    Log.Debug("注入配置外部配置");
                    FrameworkSetting.Instance.ResetGameEnv();
                }
            }
        }

        private static void NormalPlatformConfigLoad(string path)
        {
            ClearConfig();
            if (File.Exists(path))
            {
                if (DecodeGameConfig(path))
                {
                    OnGameConfigReadSuccess();
                }
                else
                {
                    OnGameConfigReadFail();
                }
            }
            else
            {
                Log.Error($"gameconfig不存在{path}");
                OnGameConfigReadFail();
            }
        }

        private static void OnGameConfigLocalSaveSuc(object data)
        {
            Log.Debug($"保存gameconfig至本地成功");
        }

        private static void ReadCodeConfig()
        {
            // 获取代码配置
        }

        private static void ReadGameConfig()
        {
            // 获取环境配置
            bool isEnableSdk = false;
            if (TryDecodeFromGameCfg("enableSDK", ref isEnableSdk))
            {
                GameEnv.SdkConfig.IsUseSdk = isEnableSdk;
            }

            ReadGameLogConfig();
            ReadGameResConfig();
            ReadCodeConfig();
            //InjectConfig();
            SetPath();
        }

        private static void ReadGameLogConfig()
        {
            // 获取日志配置
            TryGetLogGameCfg("isReceiveUnityLog", ref GameEnv.LogConfig.isReceiveUnityLog);
            TryGetLogGameCfg("logCacheQueueCount", ref GameEnv.LogConfig.logCacheQueueCount);
            TryGetLogGameCfg("logEnableLevel", ref GameEnv.LogConfig.logEnableLevel);
            TryGetLogGameCfg("logEnablePrintLevel", ref GameEnv.LogConfig.logEnablePrintLevel);
            TryGetLogGameCfg("logEnableWriteLevel", ref GameEnv.LogConfig.logEnableWriteLevel);
            TryGetLogGameCfg("logEnablePrintTimeLevel", ref GameEnv.LogConfig.logEnablePrintTimeLevel);
            TryGetLogGameCfg("logEnablePrintTrackLevel", ref GameEnv.LogConfig.logEnablePrintTrackLevel);
            TryGetLogGameCfg("logMsgFilter", ref GameEnv.LogConfig.logMsgFilter);
            TryGetLogGameCfg("clearOldLog", ref GameEnv.LogConfig.clearOldLog);
            TryGetLogGameCfg("logFileMaxCount", ref GameEnv.LogConfig.logFileMaxCount);
            TryGetLogGameCfg("saveLogOnlyCurrent", ref GameEnv.LogConfig.saveLogOnlyCurrent);
            TryGetLogGameCfg("logWriteDirPath", ref GameEnv.LogConfig.logWriteDirPath);
            TryGetLogGameCfg("saveDirPath", ref GameEnv.LogConfig.saveDirPath);
        }

        private static void ReadGameResConfig()
        {    // 获取资源配置
            bool isUseFirstRes = false;
            if (TryGetResGameCfg("isUseGameFirstRes", ref isUseFirstRes))
            {
                GameEnv.ResConfig.IsUseGameFirstRes = isUseFirstRes;
            }
        }

        private static void SetPath()
        {
            GameEnv.Path.firstResDirPath = Utility.Path.GetStreamingAssetsPath();
        }

        private static bool TryDecodeSubFromGameCfg<T>(string subConfigName, string name, ref T res)
        {
            if (!gameConfigJson.ContainsKey(subConfigName)) return false;
            if (Utility.Json.TryGetValue(gameConfigJson[subConfigName], name, out T getData))
            {
                res = getData;
                return true;
            }
            return false;
        }

        private static bool TryEncodeSubFromGameCfg<T>(string subConfigName, string name, T res)
        {
            if (!gameConfigJson.ContainsKey(subConfigName))
            {
                gameConfigJson[subConfigName] = new JsonData();
                gameConfigJson[subConfigName].SetJsonType(JsonType.Object);
            }
            if (Utility.Json.TrySetValue(gameConfigJson[subConfigName], name, res))
            {
                GameConfigStorageCheck();
                return true;
            }
            return false;
        }

        private static void StoragePlatformConfigLoad(string configPath)
        {
        }

        private static void WebGlPlatformConfigLoad(string configUrl)
        {
            ClearConfig();
            Log.Debug($"WebGl 读取gameconfig {configUrl}");
            configUrl = GetGameConfigRequestUrl(configUrl);
            GameApp.WebRequestModule.UnityWebRequestMgr.GetText(configUrl, null, (err, jsonTxt) =>
            {
                if (!string.IsNullOrEmpty(err))
                {
                    Log.Fatal($"WebGl 读取gameconfig 失败，错误：{configUrl} {err}");
                    OnGameConfigReadFail();
                    return;
                }
                Debug.Log(jsonTxt);
                Log.Debug($"WebGl 读取gameconfig 成功! {configUrl}");
                if (DecodeGameConfigStr(jsonTxt))
                {
                    OnGameConfigReadSuccess();
                }
                else
                {
                    OnGameConfigReadFail();
                }
            });
        }
    }
}
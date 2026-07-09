#if UNITY_EDITOR

using CustomLitJson;
using System.Collections;
using System.IO;
using System.Text;
using Framework.Runtime.Archives;
using Framework.Runtime.LogSystem;
using Framework.Runtime.Storage;
using Framework.Utils;
using UnityEngine;

namespace Framework.Runtime
{
    
    public static class GameConfigStatic
    {
        private static JsonData gameConfigJson;

        
        public static string GetGameScriptPathStatic()
        {
            // 读取 extcfg.json
            string path = GetPlatformPathStatic();

            return Utility.Path.PathCombine(path, "GameScript");
        }

        
        public static string GetGameScriptChildPathStatic(string path)
        {
            return Utility.Path.PathCombine(GetGameScriptPathStatic(), path);
        }
      
        public static string platformPath;

        public static string GetPlatformPathStatic()
        {
            FrameworkSetting quickEnvSetting = GameObject.FindObjectOfType<FrameworkSetting>();
            if (quickEnvSetting != null)
            {
                return quickEnvSetting.devPlatformDir;
            }
            return Utility.Path.GetPersistentDataPath();
            //string outCfgPath = Utility.Path.PathCombine(Utility.Path.GetPersistentDataPath(), GameConfig.FieldName_GameConfigJson);
            //string inCfgPath = Utility.Path.PathCombine(Utility.Path.GetStreamingAssetsPath(), GameConfig.FieldName_GameConfigJson);
            //if (File.Exists(outCfgPath))
            //{
            //    FunctionUtility.SafeCall(DecodeGameConfig, outCfgPath);
            //    return platformPath;
            //}
            //else if (File.Exists(inCfgPath))
            //{
            //    FunctionUtility.SafeCall(DecodeGameConfig, inCfgPath);
            //    return platformPath;
            //}
            //else
            //{
            //    Debug.LogError($"gameconfig不存在");
            //    return Utility.Path.GetPersistentDataPath();
            //}
        }

        private static void DecodeGameConfig(string configPath)
        {
            if (Application.isPlaying)
                Log.Debug($"读取游戏配置文件{configPath}");
            string jsonStr = string.Empty;
            if (!File.Exists(configPath))
            {
                Log.Fatal($"{configPath} not found path {configPath}!");
                return;
            }

            jsonStr = Utility.FileUtil.ReadFile(configPath);
            DecodeGameConfigStr(jsonStr);
        }

        private static void DecodeGameConfigStr(string jsonStr)
        {
            var jsonData = Utility.Json.ReadJson(jsonStr);
            if (jsonData == null)
            {
                Log.Fatal($"read gameconfig.json error");
                return;
            }
            gameConfigJson = jsonData;
            ReadGameConfig();
        }

        public static bool TryDecodeFromGameCfg<T>(string name, ref T res)
        {
            if (Utility.Json.TryGetValue(gameConfigJson, name, out T getRes))
            {
                res = getRes;
                return true;
            }
            return false;
        }

        private static void ReadGameConfig()
        {
            // 获取环境配置
            platformPath = GameEnv.Path.platformDir;
            //TryDecodeFromGameCfg("plafromtDir", ref platformPath);
        }
    }
}

#endif
using Framework.Runtime;
using Framework.Runtime.LogSystem;
using Framework.Runtime.MAsset;
using Framework.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Modules
{
    public class GameConfigModule : GameModuleBaseInstance<GameConfigModule>
    {
        private Dictionary<string, string> m_Name2JsonStrMap;
        protected override void OnConstructed()
        {
            m_Name2JsonStrMap = new Dictionary<string, string>();
        }

          protected override void GenerateHandlers()
        {
            RegisterHandler<GameConfigClientHandler>();
            RegisterHandler<GameConfigDataHandler>();
            RegisterHandler<GameConfigServerHandler>();
            RegisterHandler<GameConfigViewHandler>();
        }
        
        /// <summary>
        /// 当所有游戏模块刚被构建的时候回传触发
        /// </summary>
        protected override void OnModuleAwake()
        {
         
        }
        /// <summary>
        /// 当所有游戏模块已被创建成功的时候回传触发
        /// </summary>
        protected override void OnModuleStart()
        {
          
        }

        /// <summary>
        /// 当游戏模块被销毁的时候回传触发
        /// </summary>
        protected override void OnModuleDestroy()
        {
            
        }
        protected override void OnCheckModuleLoad()
        {
            //  这里需要同步加载配置文件
            string groupAssetPath = AssetPathEncoder.EncodeHotAssetLink("config",AssetType.AddressableGroupAsset);
            GameApp.AssetManager.LoadAssetAsync(groupAssetPath, OnGameConfigAssetsLoaded);
        }
        private void OnGameConfigAssetsLoaded(IAssetVO assetVO)
        {
            if(assetVO.IsLoadSuccess && assetVO.GetAsset() is List<UnityEngine.Object> txtObjs)
            {
                for (int i = 0; i < txtObjs.Count; i++)
                {
                     TextAsset textAsset = txtObjs[i] as TextAsset;
                    if (textAsset != null)
                    {
                        string configName = textAsset.name;
                        string configJsonStr = textAsset.text;
                        if (!m_Name2JsonStrMap.ContainsKey(configName))
                        {
                            m_Name2JsonStrMap.Add(configName, configJsonStr);
                        }
                        
                    }
                }
                assetVO.UnLoadAsync();
                OnModuleLoaded();
            }
            else
            {
                Log.Error("加载配置文件失败!!");
            }
        }

        public  bool TryDecodeConfig<T>(string configName, out T readConfig) where T : class
        {
            string configJson = GetConfigJsonStr(configName);
            if (string.IsNullOrEmpty(configJson))
            {
                readConfig = null;
                return false;
            }
            readConfig = Utility.Json.ToObject<T>(configJson);
            return readConfig!=null;
        }
        private string GetConfigJsonStr(string configName)
        {
            m_Name2JsonStrMap.TryGetValue(configName, out string jsonStr);
            return jsonStr;
        }
    }

}

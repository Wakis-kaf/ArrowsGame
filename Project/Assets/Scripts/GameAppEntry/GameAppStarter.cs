using Framework.Runtime;
using Framework.Runtime.LogSystem;
using Framework.Runtime.MAsset;
using Framework.Runtime.MGameModule;
using Framework.Runtime.UI;
using Framework.Utils;
using Game.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class GameAppStarter : MonoBehaviour
{
    private void Awake()
    {
        // 等待框架加载完成
        GameApp.CreateInstance(gameObject);
        GameApp.Ins.StartApplication();
        //gameObject.AddComponent<GameAppShell>();
        GameApp.Ins.MessageDispatcher.Subscribe(MessageCode.msg_gamemodules_loaded, OnGameModuleLoaded);
    }

    private void OnGameModuleLoaded()
    {
        Log.Info($"message_gamemodules_loaded hotCodeModel：{GameEnv.hotCodeModel}");
#if !UNITY_HOT_HYBIRDCLR
        if (GameEnv.hotCodeModel == HotCodeModel.None) 
        {
            Log.Info($"GameModuleList：{GameModuleFactory.GameModuleList.Count}");
            GameApp.GameModuleManager.GameModuleTypeList.AddRange(GameModuleFactory.GameModuleList);
            return;
        }  else
#endif

        if (GameEnv.hotCodeModel == HotCodeModel.HybirdCLR)
        {
            Type factoryType = Utility.AssemblyUtil.GetType("GameRuntime", "Game.Modules.GameModuleFactory");
            if (factoryType == null)
            {
                Log.Error("无法获取到GameModuleFactory类");
                return;
            }

            FieldInfo fieldInfo = factoryType.GetField("GameModuleList",
                BindingFlags.Public | BindingFlags.Static);

            if (fieldInfo != null)
            {
                // 获取字段值
                List<Type> moduleList = fieldInfo.GetValue(null) as List<Type>;

                if (moduleList != null)
                {
                    foreach (Type moduleType in moduleList)
                    {
                        Console.WriteLine($"Module Type: {moduleType.FullName}");
                        Console.WriteLine($"Assembly: {moduleType.Assembly.GetName().Name}");
                    }
                }
                GameApp.GameModuleManager.GameModuleTypeList.AddRange(moduleList);
            }
        }
    }
}
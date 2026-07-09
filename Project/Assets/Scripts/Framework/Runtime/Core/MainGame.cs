using Framework.Runtime.LogSystem;
using Framework.Runtime.MSDK;
using Framework.Utils;
using System;
using UnityEngine;

namespace Framework.Runtime
{
    public class MainGame : MonoBehaviour
    {
        private bool isAssetModuleLoadSuc;
        private bool isAssetModuleNewestSuc;
        private bool isGameConfigLoadedSuc;
        private bool isStarted;
        public static MainGame Instance { private set; get; }
        public bool IsAssetModuleLoadSuc => isAssetModuleLoadSuc;

        public bool IsAssetModuleNewestSuc => isAssetModuleNewestSuc;

        public bool IsGameConfigLoadedSuc => isGameConfigLoadedSuc;

        public bool IsStarted => isStarted;

        public void AppUpdate(GameAppMessage appMessage)
        {
            bool send = false;
            if (appMessage.MessageCode == GameAppMessage.code_gameConfig_loadSuccess && !isGameConfigLoadedSuc)
            {
                isGameConfigLoadedSuc = true;
                Log.Debug("游戏配置（GameConfig） 加载完成");
                send = true;
                MessageDispatcher.Ins.Dispatch<float, float, string, Action>(MessageCode.msg_gameLoading_set, 0.5f, 20, "游戏验证完成", null);
            }
            if (appMessage.MessageCode == GameAppMessage.code_assetModule_loadSuccess && !isAssetModuleLoadSuc)
            {
                isAssetModuleLoadSuc = true;
                Log.Debug("资源模块（AssetModule） 加载完成");
                send = true;
                MessageDispatcher.Ins.Dispatch<float, float, string, Action>(MessageCode.msg_gameLoading_set, 0.5f, 40, "资源更新检查中", null);
            }
            if (appMessage.MessageCode == GameAppMessage.code_assetModule_newestSuccess && !isAssetModuleNewestSuc)
            {
                isAssetModuleNewestSuc = true;
                Log.Debug("资源更新（AssetNewest） 加载完成");
                MessageDispatcher.Ins.Dispatch<float, float, string, Action>(MessageCode.msg_gameLoading_set, 0.5f, 50, "资源更新完成", null);
                send = true;
            }
            if (send)
            {
                GameApp.Ins.SendModuleUpdateMessage(new GameAppMessage(GameAppMessage.code_mainGameState_changed));
            }

            CheckMainGameStart();
        }

        public void AwakeMainGame()
        {
        }

        public void InitMainGame()
        {
            MessageDispatcher.Ins.Dispatch<float, float, string, Action>(MessageCode.msg_gameLoading_set, 0.5f, 10, "游戏启动中", null);
        }

        public void StartMainGame()
        {
            if (isStarted) return;
            isStarted = true;
            GameApp.Ins.SendModuleUpdateMessage(new GameAppMessage(GameAppMessage.code_gameModule_start));
            Log.Info("主游戏程序启动");
            MessageDispatcher.Ins.Dispatch<float, float, string, Action>(MessageCode.msg_gameLoading_set, 0.5f, 90, "正在启动游戏", OnLoadingOver);
        }

        public void StopMainGame()
        {
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Log.Debug("创建主程序");
            transform.SetParent(GameApp.Ins.GameAppShell.transform);
        }

        private void CheckMainGameStart()
        {
            if (!isGameConfigLoadedSuc) return;
            if (!isAssetModuleLoadSuc) return;
            if (!isAssetModuleNewestSuc) return;
            if (GameEnv.SdkConfig.IsUseSdk)
            {
                FunctionUtility.SafeCall<Action<int, string>>(SuperSDKHelper.Instance.InitSdk, SdkCheck);
            }
            else
            {
                StartMainGame();
            }
        }

        private void OnAllGameModuleReady()
        {
            MessageDispatcher.Ins.Dispatch<float, float, string, Action>(MessageCode.msg_gameLoading_set, 1f, 100, "游戏加载完成", () =>
            {
                GameApp.GameModuleManager.StartGameModule();
                if (!string.IsNullOrEmpty(FrameworkSetting.Instance.onLoadingOverMsg))
                {
                    MessageDispatcher.Ins.Dispatch(FrameworkSetting.Instance.onLoadingOverMsg);
                }
                else
                {
                    MessageDispatcher.Ins.Dispatch(MessageCode.msg_mainGame_start);
                }

                //GameLoading.Ins.CloseLoading();
            });


        }


        private void OnLoadingOver()
        {
            MessageDispatcher.Ins.Dispatch<float, float, string, Action>(MessageCode.msg_gameLoading_set, 0.5f, 95, "正在加载游戏模块", null);
            GameApp.GameModuleManager.LoadGameModule(OnAllGameModuleReady);

            // 等待所有模块需要预加载的资源完成后进入游戏
        }

        private void SdkCheck(int code, string status)
        {
            Log.Debug("SDK初始化结果" + status);
            StartMainGame();
        }
    }
}
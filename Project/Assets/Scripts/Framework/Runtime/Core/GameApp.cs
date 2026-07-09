using Framework.Runtime.Archives;
using Framework.Runtime.Base;
using Framework.Runtime.LogSystem;
using Framework.Runtime.MAsset;
using Framework.Runtime.MAudio;
using Framework.Runtime.MCombat;
using Framework.Runtime.MDebugger;
using Framework.Runtime.MGameModule;
using Framework.Runtime.MLanAndTheme;
using Framework.Runtime.CameraManage;
using Framework.Runtime.MObjectPool;
using Framework.Runtime.Module;
using Framework.Runtime.MSceneUnit;
using Framework.Runtime.Storage;
using Framework.Runtime.UI;
using Framework.Runtime.UnitSystem;
using Framework.Utils;
using System;
using UnityEngine;

namespace Framework.Runtime
{
    /// <summary>
    /// 状态有一下几种： 未启动 Sleepy 运行中 Playing (系统层初始化、系统层开始，模块初始化、模块开始.... ) 暂停中 Paused 已销毁 Destroyed
    /// </summary>

    public enum GameAppMainState
    {
        Sleepy,
        Playing,
        Destroyed,
    }

    public sealed partial class GameApp : UnitObject
    {
        public static GameApp Ins { get; private set; }

        private GameAppMainState m_State = GameAppMainState.Sleepy;
        public GameAppMainState GameApplicationMainState => m_State;
        public LoopManager LoopManager { get; private set; }
        public ModuleManager ModuleManager { get; private set; }
        public PlatformStorage PlatformStorage { get; private set; }
        public UnitManager UnitManager { get; private set; }
        public MessageDispatcher MessageDispatcher { get; private set; }
        public GameAppShell GameAppShell { get; private set; }
        public CameraStackManager CameraStackManager { get; private set; }

        public GameApp(GameObject shellGameObject)
        {
            var shell = shellGameObject ?? new GameObject("GameAppShell");
            GameAppShell = shell.GetOrAddComponent<GameAppShell>();
            GameObject.DontDestroyOnLoad(GameAppShell.gameObject);
            Application.wantsToQuit += WantsQuitApp;

        }
        private bool WantsQuitApp()
        {
            QuitApplication();
            return true;
        }
        public void QuitApplication()
        {
            StopApplication();
            GameObject.Destroy(GameAppShell);
            Dispose();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif


        }

        public void SendMainGameUpdateMssage(GameAppMessage appMessage)
        {
            UpdateFromGame(appMessage);
            ModulePopupUpdate(appMessage);
        }

        public void SendModuleUpdateMessage(GameAppMessage appMessage)
        {
            Log.Debug($"收到模块更新信息:信息码 {appMessage.MessageCode}");

            UpdateFromModule(appMessage);
            SystemPopupUpdate(appMessage);
        }

        public void SendSystemUpdateMessage(GameAppMessage appMessage)
        {
            UpdateFromSystem(appMessage);
        }

        /// <summary>
        /// 分为三层，系统层(System)，模块层(Module)和业务层(MainGame)
        /// </summary>
        public void StartApplication()
        {
            if (GameApplicationMainState == GameAppMainState.Playing) return;
            BaseInit();
            SwitchState(GameAppMainState.Playing);
            SystemInit();
            SystemStart();

            ModuleInit();
            ModuleStart();

            MainGameInit();
            MainGameAwake();

            UpdateFromSystem(new GameAppMessage(GameAppMessage.code_gameSytem_start));
        }
        private void BaseInit()
        {

            LoopManager = new LoopManager();
            ModuleManager = new ModuleManager();
            PlatformStorage = new PlatformStorage();
            UnitManager = new UnitManager();
            MessageDispatcher = new MessageDispatcher();
            CameraStackManager = new CameraStackManager();
            CameraStackManager.RegisterCamera(UIRootCamera.Camera);
            CameraStackManager.SetBaseCamera(UIRootCamera.Camera);
        }
        private void BaseSet()
        {
            Application.targetFrameRate = GameEnv.frameRate;
            Application.runInBackground = GameEnv.runInBackground;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        public void StopApplication()
        {
            if (GameApplicationMainState != GameAppMainState.Playing) return;
            UpdateFromSystem(new GameAppMessage(GameAppMessage.code_gameSytem_shutdown));
            MainGameStop();
            ModuleStop();
            SystemStop();
            SwitchState(GameAppMainState.Destroyed);

        }
        public static bool IsAppRunning()
        {
            return Ins != null && Ins.GameApplicationMainState == GameAppMainState.Playing;
        }
        private void MainGameAwake()
        {
            MainGame.Instance.AwakeMainGame();
        }

        private void MainGameInit()
        {
            GameObject go = new GameObject("MainGame");
            go.AddComponent<MainGame>();
            FunctionUtility.SafeCall(MainGame.Instance.InitMainGame);
        }

        private void MainGameStop()
        {
            FunctionUtility.SafeCall(MainGame.Instance.StopMainGame);
        }

        private void MainGameUpdate(GameAppMessage appMessage)
        {
            FunctionUtility.SafeCall(MainGame.Instance.AppUpdate, appMessage);
        }

        private void ModuleInit()
        {
            FunctionUtility.SafeCall(ModuleManager.Init);
        }

        private void ModulePopupUpdate(GameAppMessage appMessage)
        {
            FunctionUtility.SafeCall(ModuleManager.AppPopupUpdate, appMessage);
            SystemPopupUpdate(appMessage);
        }

        private void ModuleStart()
        {
            FunctionUtility.SafeCall(ModuleManager.Start);
        }

        private void ModuleStop()
        {
            FunctionUtility.SafeCall(ModuleManager.ModuleManagerStop);
        }

        private void ModuleUpdate(GameAppMessage appMessage)
        {
            FunctionUtility.SafeCall(ModuleManager.AppUpdate, appMessage);
        }

        private void SwitchState(GameAppMainState newState)
        {
            m_State = newState;
        }

        private void SystemInit()
        {
            FunctionUtility.SafeCall(GameConfig.Init);
            BaseSet();
            FunctionUtility.SafeCall(PlatformStorage.Init);
            // 初始化事件系统
            FunctionUtility.SafeCall(Log.Init);
            // 初始化日志系统
            FunctionUtility.SafeCall(LoopManager.Init);
            // 初始化Unit系统
            FunctionUtility.SafeCall(UnitManager.Init);
            // 初始化开屏加载
            FunctionUtility.SafeCall(GameLoading.Ins.Init);

        }

        private void SystemPopupUpdate(GameAppMessage appMessage)
        {
            FunctionUtility.SafeCall(GameLoading.Ins.AppPopupUpdate, appMessage);
        }

        private void SystemStart()
        {
            FunctionUtility.SafeCall(PlatformStorage.Instance.Start);
            FunctionUtility.SafeCall(GameConfig.Start);
            FunctionUtility.SafeCall(Log.Start);
            FunctionUtility.SafeCall(LoopManager.Start);
            FunctionUtility.SafeCall(UnitManager.Start);
            FunctionUtility.SafeCall(GameLoading.Ins.Start);
        }

        private void SystemStop()
        {
            MessageDispatcher.Ins?.ClearAllMessage();
            FunctionUtility.SafeCall(UnitManager.Close);
            FunctionUtility.SafeCall(LoopManager.Stop);
            FunctionUtility.SafeCall(GameConfig.Stop);
            Log.Debug("Stop Log Record");
            FunctionUtility.SafeCall(Log.Stop);
            FunctionUtility.SafeCall(GameLoading.Close);

        }

        private void SystemUpdate(GameAppMessage appMessage)
        {
            FunctionUtility.SafeCall(PlatformStorage.Instance.AppUpdate, appMessage);
            FunctionUtility.SafeCall(GameConfig.AppUpdate, appMessage);
            FunctionUtility.SafeCall(Log.AppUpdate, appMessage);
            FunctionUtility.SafeCall(LoopManager.AppUpdate, appMessage);
            FunctionUtility.SafeCall(UnitManager.AppUpdate, appMessage);
            FunctionUtility.SafeCall(GameLoading.Ins.AppUpdate, appMessage);
        }

        private void UpdateFromGame(GameAppMessage appMessage)
        {
            MainGameUpdate(appMessage);
        }

        private void UpdateFromModule(GameAppMessage appMessage)
        {
            ModuleUpdate(appMessage);
            UpdateFromGame(appMessage);
        }

        private void UpdateFromSystem(GameAppMessage appMessage)
        {
            SystemUpdate(appMessage);
            UpdateFromModule(appMessage);
        }
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
        }

        public static void CreateInstance(GameObject shellGameObject = null)
        {
            if (Ins != null)
            {
                Ins = null;
            }
            Ins = new GameApp(shellGameObject);
        }
    }

    public sealed partial class GameApp
    {
        public static ArchiveModule ArchiveModule => Ins.ModuleManager.GetModuleUnit<ArchiveModule>();
        public static AssetManager AssetManager => Ins.ModuleManager.GetModuleUnit<AssetManager>();
        public static SceneUnitManager SceneUnitManager => Ins.ModuleManager.GetModuleUnit<SceneUnitManager>();
        public static AudioModule AudioModule => Ins.ModuleManager.GetModuleUnit<AudioModule>();
        public static GameModuleManager GameModuleManager => Ins.ModuleManager.GetModuleUnit<GameModuleManager>();
        public static InputModule InputModule => Ins.ModuleManager.GetModuleUnit<InputModule>();
        public static PoolModule PoolModule => Ins.ModuleManager.GetModuleUnit<PoolModule>();
        public static UIModule UIModule => Ins.ModuleManager.GetModuleUnit<UIModule>();
        public static WebRequestModule WebRequestModule => Ins.ModuleManager.GetModuleUnit<WebRequestModule>();
        public static CombatSystem CombatSystem => Ins.ModuleManager.GetModuleUnit<CombatSystem>();
        public static DebuggerModule Debugger => Ins.ModuleManager.GetModuleUnit<DebuggerModule>();
        public static Lan2LocalManager Lan2LocalManager => Ins.ModuleManager.GetModuleUnit<LanAndThemeModule>()?.Lan2LocalManager;
        public static Theme2LocalManager Theme2LocalManager => Ins.ModuleManager.GetModuleUnit<LanAndThemeModule>()?.Theme2LocalManager;
    }

    public class GameAppMessage
    {
        public const string code_assetModule_loadSuccess = "code_assetmodule_loadSuccess";
        public const string code_assetModule_newestSuccess = "code_assetModule_newestSuccess";
        public const string code_gameConfig_loadSuccess = "code_gameConfig_loadSuccess";
        public const string code_gameSytem_shutdown = "code_gamesytem_shutdown";
        public const string code_gameSytem_start = "code_gamesytem_start";
        public const string code_luaModule_loadSuccess = "code_luamodule_loadSuccess";
        public const string code_gameModule_start = "code_gameModule_start";
        public const string code_mainGameState_changed = "code_mainGameState_changed";

        public GameAppMessage(string code)
        {
            MessageCode = code;
        }

        public GameAppMessage()
        {
        }

        public string MessageCode { private set; get; }
    }
}
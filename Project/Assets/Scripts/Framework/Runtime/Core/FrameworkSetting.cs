using Framework.Runtime;
using Framework.Runtime.LogSystem;
using Framework.Runtime.UI;
using Framework.Utils;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement; // 引入场景管理命名空间
#endif

[ExecuteAlways] // 确保在非运行模式下也能响应生命周期
public class FrameworkSetting : MonoBehaviour
{
    [LabelText("项目版本号")]
    public string appVersion = "1.0.0";
    [OnValueChanged("UpdateConfig")]
    [LabelText("项目环境")]
    public AppEnv appEnv;

    [LabelText("项目阶段")]
    public AppStatus appStatus;

    [LabelText("启用开发平台路径")]
    public bool enableDevlopPlatformDir;

    [ShowIf("enableDevlopPlatformDir"), LabelText("是否相对目录")]
    public bool isRelativePlatformDir = true;

    [ShowIf("enableDevlopPlatformDir")]
    [LabelText("开发平台路径")]
    public string devPlatformDir;
    [LabelText("后台运行")]
    public bool runInBackground;


    [SerializeField, ReadOnly]
    private string platformDir;
    public string PlatformDir => platformDir;
    [FoldoutGroup("热更新设置")]
    [LabelText("脚本热更模式")]
    [OnValueChanged("UpdateConfig")]
    public HotCodeModel hotCodeModel = HotCodeModel.ToLua;



    [FoldoutGroup("资源设置")]
    [LabelText("启用Editor加载资源")]
    public bool enableEditorResLoad;

    [FoldoutGroup("资源设置")]
    [LabelText("启用Resources加载资源")]
    public bool enableResourcesResLoad;

    [FoldoutGroup("资源设置")]
    [LabelText("启用加载可热更资源")]
    public bool enableHotResLoad;

    [FoldoutGroup("资源设置")]
    [LabelText("是否本地缓存首包资源")]
    public bool enableStorageFirstRes;

    [FoldoutGroup("资源设置")]
    [LabelText("首包加载模式")]
    public FirstResMode firstResMode;

    [FoldoutGroup("资源设置")]
    [LabelText("非首包资源加载模式")]
    public ResLoadWay resLoadWay;

    [FoldoutGroup("资源设置")]
    [LabelText("是否启用资源更新检测")]
    public bool isGameResUpdateCheck = false;

    [FoldoutGroup("SDK设置")]
    [LabelText("启用平台SDK")]
    public bool enablePlatformSDK;

    [FoldoutGroup("调试器设置")]
    [LabelText("是否开发模式默认开启Debugger")]
    public bool isDefaultOpenDebugger = true;


    [FoldoutGroup("日志设置")]
    [LabelText("是否接收Unity日志")]
    public bool isReceiveUnityLog = false;
    [FoldoutGroup("存档设置")]
    [LabelText("是否开启存档加密")]
    public bool enableArchiveEncrypt = false;

    [FoldoutGroup("其他设置")]
    [LabelText("帧率设置")]
    public int frameRate = 60;
    [FoldoutGroup("其他设置")]
    [LabelText("加载完成发送消息")]
    public string onLoadingOverMsg = "msg_mainGame_start";


    private static FrameworkSetting m_Instance;

    public static FrameworkSetting Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = Object.FindAnyObjectByType<FrameworkSetting>();
                m_Instance?.UpdateConfig();
            }
            return m_Instance;
        }
    }

    private void Awake()
    {
        if (m_Instance == null)
        {
            m_Instance = this;
        }
        else if (m_Instance != this)
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }
            return;
        }

        UpdateConfig();
    }

    // 运行时：确保游戏启动时加载配置
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnLoad()
    {
        if (Instance != null)
        {
            Instance.UpdateConfig();
        }
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            // 静态切场景兜底：当前物体被激活时执行
            UpdateConfig();

            // 注销防止重复注册，然后绑定编辑器打开场景后的事件
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }
    }

    private void OnDisable()
    {
        if (!Application.isPlaying)
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
        }
    }

    // 当编辑器静态打开/切换进任何场景，且场景完全加载后触发
    private static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
    {
        if (Application.isPlaying) return;

        // 静态进入场景后，重新寻找当前场景的配置单例并强制刷新
        m_Instance = Object.FindAnyObjectByType<FrameworkSetting>();
        if (m_Instance != null)
        {
            m_Instance.UpdateConfig();
            Debug.Log($"静态进入场景 [{scene.name}]：自动同步项目环境成功。");
        }
    }
#endif

    public void ResetGameEnv()
    {
        GameEnv.AppEnv = appEnv;
        GameEnv.AppStatus = appStatus;
        GameEnv.ResConfig.FirstResMode = firstResMode;
        GameEnv.ResConfig.resLoadWay = resLoadWay;
        GameEnv.ResConfig.IsUpdateResNewset = isGameResUpdateCheck;
        GameEnv.ResConfig.EnableHotResLoad = enableHotResLoad;
        GameEnv.ResConfig.EnableEditorResLoad = enableEditorResLoad;
        GameEnv.ResConfig.IsStorageFirstRes = enableStorageFirstRes;
        GameEnv.SdkConfig.IsUseSdk = GameEnv.IsEditor() ? false : enablePlatformSDK;
        GameEnv.LogConfig.isReceiveUnityLog = isReceiveUnityLog;
        GameEnv.Path.platformDir = platformDir;
        GameEnv.EnableDevlopPlatform = enableDevlopPlatformDir;
        GameEnv.hotCodeModel = hotCodeModel;
        GameEnv.frameRate = frameRate;
        GameEnv.runInBackground = runInBackground;
        GameEnv.ArchiveConfig.enableArchiveEncrypt = enableArchiveEncrypt;
        if (GameEnv.AppStatus != AppStatus.Devlop)
        {
            GameEnv.LogConfig.logEnableLevel = LogLevel.ERROR | LogLevel.FATAL;
        }
#if UNITY_EDITOR
        if (GameEnv.hotCodeModel == HotCodeModel.ToLua)
        {
            AddDefineSymbol("UNITY_HOT_TOLUA");
            RemoveDefineSymbol("UNITY_HOT_HYBIRDCLR");
        }
        else if (GameEnv.hotCodeModel == HotCodeModel.HybirdCLR)
        {
            AddDefineSymbol("UNITY_HOT_HYBIRDCLR");
            RemoveDefineSymbol("UNITY_HOT_TOLUA");
        }
#endif
    }

    public void UpdateConfig()
    {
        Debug.Log("游戏环境更新");
        if (hotCodeModel == HotCodeModel.None)
        {
            RemoveDefineSymbol("UNITY_HOT_HYBIRDCLR");
        }
        if (hotCodeModel == HotCodeModel.HybirdCLR)
        {
            AddDefineSymbol("UNITY_HOT_HYBIRDCLR");
        }

        if (appEnv == AppEnv.WindowEditor)
        {
            firstResMode = FirstResMode.StreamingAssets;
            resLoadWay = ResLoadWay.Editor;
            enableDevlopPlatformDir = true;
            if (isRelativePlatformDir)
            {
                platformDir = Utility.Path.PathCombine(Application.dataPath, devPlatformDir);
            }
            else
            {
                platformDir = devPlatformDir;
            }
            isGameResUpdateCheck = false;
            enableResourcesResLoad = true;
            enableEditorResLoad = true;
            enableHotResLoad = false;
            enableDevlopPlatformDir = true;
            enablePlatformSDK = false;
            isReceiveUnityLog = false;
            enableStorageFirstRes = false;
#if UNITY_EDITOR
            RemoveDefineSymbol("UNITY_WXGAME");
#endif
            ResetGameEnv();
        }
        if (appEnv == AppEnv.WindowGame)
        {
            firstResMode = FirstResMode.StreamingAssets;
            resLoadWay = ResLoadWay.IO;
            isGameResUpdateCheck = true;
            enableResourcesResLoad = true;
            enableEditorResLoad = false;
            enableHotResLoad = true;
            enableDevlopPlatformDir = false;
            enableStorageFirstRes = true;
            platformDir = Framework.Utils.Utility.Path.GetPersistentDataPath();
            enablePlatformSDK = true;
            isReceiveUnityLog = true;
#if UNITY_EDITOR
            //AddDefineSymbol("UNITY_TIKTOKGAME");
#endif
            ResetGameEnv();
        }
        if (appEnv == AppEnv.AndroidGame)
        {
            firstResMode = FirstResMode.StreamingAssets;
            resLoadWay = ResLoadWay.Addressable;
            isGameResUpdateCheck = true;
            enableResourcesResLoad = true;
            enableEditorResLoad = false;
            enableHotResLoad = true;
            enableStorageFirstRes = true;
            enableDevlopPlatformDir = false;
            platformDir = Framework.Utils.Utility.Path.GetPersistentDataPath();
            enablePlatformSDK = true;
            isReceiveUnityLog = true;
            ResetGameEnv();
        }
        if (appEnv == AppEnv.TikTokMiniGame)
        {
            firstResMode = FirstResMode.StreamingAssets;
            resLoadWay = ResLoadWay.Web;
            isGameResUpdateCheck = false;
            enableResourcesResLoad = false;
            enableEditorResLoad = false;
            enableHotResLoad = true;
            enableDevlopPlatformDir = false;
            enableStorageFirstRes = false;
            platformDir = Framework.Utils.Utility.Path.GetPersistentDataPath();
            enablePlatformSDK = true;
            isReceiveUnityLog = true;
#if UNITY_EDITOR
            //AddDefineSymbol("UNITY_TIKTOKGAME");
#endif
            ResetGameEnv();
        }
        if (appEnv == AppEnv.WXMiniGame)
        {
            firstResMode = FirstResMode.Resources;
            resLoadWay = ResLoadWay.Web;
            isGameResUpdateCheck = false;
            enableResourcesResLoad = true;
            enableEditorResLoad = false;
            enableHotResLoad = false;
            enableDevlopPlatformDir = false;
            enableStorageFirstRes = false;
            platformDir = Framework.Utils.Utility.Path.GetPersistentDataPath();
            enablePlatformSDK = true;
            isReceiveUnityLog = true;
#if UNITY_EDITOR
            AddDefineSymbol("UNITY_WXGAME");
#endif
            ResetGameEnv();
        }
        if (appEnv == AppEnv.WXWebGame)
        {
            firstResMode = FirstResMode.StreamingAssets;
            resLoadWay = ResLoadWay.Web;
            isGameResUpdateCheck = false;
            enableResourcesResLoad = true;
            enableEditorResLoad = false;
            enableHotResLoad = true;
            enableDevlopPlatformDir = false;
            enableStorageFirstRes = false;
            platformDir = Framework.Utils.Utility.Path.GetPersistentDataPath();
            enablePlatformSDK = true;
            isReceiveUnityLog = true;
#if UNITY_EDITOR
            AddDefineSymbol("UNITY_WXGAME");
#endif
            ResetGameEnv();
        }
    }

    public static void AddDefineSymbol(string defineSymbol)
    {
#if UNITY_EDITOR
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

        if (!defines.Contains(defineSymbol))
        {
            defines = string.IsNullOrEmpty(defines)
                ? defineSymbol
                : defines + ";" + defineSymbol;

            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
            Debug.Log($"已添加宏定义: {defineSymbol}");
        }
        AssetDatabase.Refresh();
#endif
    }

    public static void RemoveDefineSymbol(string defineSymbol)
    {
#if UNITY_EDITOR
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

        var definesList = defines.Split(';').ToList();
        if (definesList.Contains(defineSymbol))
        {
            definesList.Remove(defineSymbol);
            defines = string.Join(";", definesList);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
            Debug.Log($"已移除宏定义: {defineSymbol}");
        }
#endif
    }
}
using Framework.Runtime.UI;
using Framework.Utils;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
namespace Framework.Runtime.MDebugger.UIView
{
    public class EnvironmentView : View
    {
        private RectTransform rtContent;
        private StringBuilder sb;
        private UText uptxtContent;
        protected override void OnInitUI()
        {
            base.OnInitUI();
            uptxtContent = GetBindObject<UText>("utxtContent");
            rtContent = GetBindObject<RectTransform>("rtContent");
            
        }

        protected override void OnShow()
        {
            sb = new StringBuilder();
            uptxtContent.text = GetEnvironmentString();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rtContent);
        }
        protected override void OnHide()
        {
            base.OnHide();

        }
        private void AppendFormat(object key, object value, object suffix = null)
        {
            string keyStr = key.ToString().Trim();
            string valStr = value.ToString().Trim();
            sb.AppendLine($"<color=red> [{keyStr}] </color> : {valStr} {suffix}");
        }
        private void AppendLine(string content = "")
        {
            sb.AppendLine(content);
        }

        private void AppendTitle(string title)
        {
            int size = UIRoot.IsPcUI() ? 28 : 22;
  
            sb.AppendLine($"<size={size}> <color=orange> {title}</color> </size>");
        }

        private string GetEnvironmentString()
        {
            string content = string.Empty;
            AppendTitle("全局配置"); 
            AppendFormat("配置是否加载成功",GameConfig.IsGameCfgLoadedSuc);
            AppendFormat("游戏环境", GameEnv.AppEnv);

            AppendLine();
            AppendTitle("全局路径配置"); 
            AppendFormat("PersistentPath", Utility.Path.GetPersistentDataPath()); 
            AppendFormat("StreamingPath",Utility.Path.GetStreamingAssetsPath()); 
            AppendFormat("PlatformPath",GameEnv.Path.platformDir);

            AppendLine();
            AppendTitle("资源配置");
#if UNITY_EDITOR
            AppendFormat("允许编辑器加载资源", GameEnv.ResConfig.EnableEditorResLoad);
#endif
            AppendLine();
            AppendTitle("日志配置"); 
            AppendFormat("IsReceiveUnityLog", GameEnv.LogConfig.isReceiveUnityLog);
            AppendFormat("LogQueueCapacity", GameEnv.LogConfig.logQueueCapacity);
            AppendFormat("LogCacheQueueCount", GameEnv.LogConfig.logCacheQueueCount);
            AppendFormat("LogEnableLevel", GameEnv.LogConfig.logEnableLevel);
            AppendFormat("LogEnablePrintLevel", GameEnv.LogConfig.logEnablePrintLevel);
            AppendFormat("LogEnableWriteLLevel", GameEnv.LogConfig.logEnableWriteLevel);
            AppendFormat("LogEnablePrintTimeLevel", GameEnv.LogConfig.logEnablePrintTimeLevel);
            AppendFormat("LogEnablePrintTrackLevel", GameEnv.LogConfig.logEnablePrintTrackLevel);
            AppendFormat("ClearOldLog", GameEnv.LogConfig.clearOldLog); 
            AppendFormat("LogFileMaxCount", GameEnv.LogConfig.logFileMaxCount);
            AppendFormat("SaveLogOnlyCurrent", GameEnv.LogConfig.saveLogOnlyCurrent); 
            AppendFormat("LogWriteDirPath", GameEnv.LogConfig.logWriteDirPath);
            AppendLine();
            AppendTitle("热更新配置");
            AppendFormat("热更新模式", GameEnv.hotCodeModel);

            AppendLine();
            AppendTitle("内存信息");
            AppendFormat("预留内存总内存", Profiler.GetTotalReservedMemoryLong() / (1024 * 1024), "MB");
            AppendFormat("已分配并使用内存",Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024),"MB");
            AppendFormat("预留内存未使用内存", Profiler.GetTotalUnusedReservedMemoryLong() / (1024 * 1024), "MB");
            AppendFormat("托管堆总预留内存", Profiler.GetMonoHeapSizeLong() / (1024 * 1024), "MB");
            AppendFormat("托管堆正使用内存",Profiler.GetMonoUsedSizeLong() / (1024 * 1024), "MB");
            
            

            


            AppendLine();
            AppendTitle("系统配置");
            AppendFormat("设备模型", SystemInfo.deviceModel);
            AppendFormat("设备类型",SystemInfo.deviceType);
            AppendFormat("设备名称", SystemInfo.deviceName);
            AppendFormat("操作系统", SystemInfo.operatingSystem);
            AppendFormat("系统内存大小",SystemInfo.systemMemorySize, "M"); 
            AppendFormat("设备唯一标识符",SystemInfo.deviceUniqueIdentifier); 
            sb.AppendLine(); 
            AppendTitle("显卡信息");
            AppendFormat("屏幕分辨率", new Vector2(Screen.width,Screen.height));
            AppendFormat("显卡标识符", SystemInfo.graphicsDeviceID);
            AppendFormat("显卡名称", SystemInfo.graphicsDeviceName); 
            AppendFormat("显卡标识符",    SystemInfo.graphicsDeviceVendorID);
            AppendFormat("显卡厂商",          SystemInfo.graphicsDeviceVendor); 
            AppendFormat("显卡版本",   SystemInfo.graphicsDeviceVersion); 
            AppendFormat("显存大小", SystemInfo.graphicsMemorySize);
            AppendFormat("显卡着色器级别", SystemInfo.graphicsShaderLevel); 
            AppendFormat("是否支持内置阴影",    SystemInfo.supportsShadows);
             sb.AppendLine();
            AppendTitle("cpu配置");
            AppendFormat("处理器数量", SystemInfo.processorCount); 
            AppendFormat("处理器类型", SystemInfo.processorType);
            return sb.ToString();
        }
    }
}
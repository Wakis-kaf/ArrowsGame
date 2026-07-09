using Framework.Runtime.Config;

using System.Collections.Generic;

namespace Framework.Runtime.UI
{
    public static class UILayerMap
    {
        public static Dictionary<int, string> CanvasLayerMap = new Dictionary<int, string>()
        {
            {GlobalConstant.LAYER_SLIENCE, "SlienceLayer"}, // 游戏中的沉默层,适合放对象池
            {GlobalConstant.LAYER_NAVIGATION, "NavigationLayer"}, // 游戏中的导航层
            {GlobalConstant.LAYER_INSTRUCTION, "InstructionLayer"}, // 角色或者世界物体跟随 UI Layer
            {GlobalConstant.LAYER_SCENE, "SceneLayer"}, // 角色或者世界物体跟随 UI Layer
            {GlobalConstant.LAYER_PANEL, "PanelLayer"}, // 游戏中的UI面板层
            {GlobalConstant.LAYER_HIGH_INSTRUCTION, "HighInstructionLayer"}, // 角色或者世界物体跟随 UI Layer
            {GlobalConstant.LAYER_ALERT, "AlertLayer"}, // 弹出框
            {GlobalConstant.LAYER_HIGH_PANEL, "HighPanelLayer"}, // 弹出框
            {GlobalConstant.LAYER_BROADCAST, "BroadcastLayer"}, // 广播层
            {GlobalConstant.LAYER_TIP, "TipLayer"}, // 弹出提示
            {GlobalConstant.LAYER_LOADING, "LoadingLayer"}, // 加载页面显示层
            {GlobalConstant.LAYER_DEBUGGER, "DebuggerLayer"},// 调试工具层
        };
    }
}
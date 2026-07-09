using Framework.Runtime.MAsset;

namespace Framework.Runtime.Config
{
    public static class GlobalConstant
    {
        public const int LAYER_EMPTY = -1;
        public const int LAYER_SLIENCE = 0;
        public const int LAYER_NAVIGATION = 1;
        public const int LAYER_INSTRUCTION = 2;
        public const int LAYER_SCENE = 4;
        public const int LAYER_PANEL = 5;
        public const int LAYER_ALERT = 6;
        public const int LAYER_HIGH_INSTRUCTION = 7;
        public const int LAYER_HIGH_PANEL = 8;
        public const int LAYER_BROADCAST = 9;
        public const int LAYER_TIP = 10;
        public const int LAYER_LOADING = 11;
        public const int LAYER_DEBUGGER = 12;
        
       
        public const int PRIORITY_ASSET_LOAD_AUDIO = 1;
        public const int PRIORITY_ASSET_LOAD_MAP = 7;
        public const int PRIORITY_ASSET_LOAD_UI = 3;
        public const int PRIORITY_ASSET_LOAD_UNIT = 6;
        public const int PRIORITY_ASSET_LUA_CODE = 999;
        /**********************************资源路径 S**********************************/

        public static string PcDebuggerPanelLink => AssetPathEncoder.EncodeEnvAssetLink("Assets/AddressableResources/UI/DebuggerConsole/Prefabs/PcDebuggerConsolePanel.prefab");
        public static string PhoneDebuggerPanelLink => AssetPathEncoder.EncodeEnvAssetLink("Assets/AddressableResources/UI/DebuggerConsole/Prefabs/PhoneDebuggerConsolePanel.prefab");
        public static string PcDebuggerLogLink => AssetPathEncoder.EncodeEnvAssetLink("Assets/AddressableResources/UI/DebuggerConsole/Prefabs/PcDebugLogView.prefab");
        public static string PhoneDebuggerLogLink => AssetPathEncoder.EncodeEnvAssetLink("Assets/AddressableResources/UI/DebuggerConsole/Prefabs/PhoneDebugLogView.prefab");

        public static string PcDebuggerEnvViewLink = AssetPathEncoder.EncodeEnvAssetLink("Assets/AddressableResources/UI/DebuggerConsole/Prefabs/PcDebugEnvironmentView.prefab");
        public static string PhoneDebuggerEnvViewLink = AssetPathEncoder.EncodeEnvAssetLink("Assets/AddressableResources/UI/DebuggerConsole/Prefabs/PhoneDebugEnvironmentView.prefab");
        public static string PATH_RESOURCES_ENVIRONMENT_VIEW => AssetPathEncoder.EncodeEnvAssetLink("Assets/AddressableResources/UI/DebuggerConsole/Prefabs/EnvironmentView.prefab");
        public static string PATH_RESOURCES_GAME_LOADING_PANEL => AssetPathEncoder.EncodeResourcesAssetLink("firstres/UI/GameLoading/Prefabs/GameLoadingPanel", AssetType.PrefabAsset);

        /**********************************资源路径 E**********************************/
    }
}
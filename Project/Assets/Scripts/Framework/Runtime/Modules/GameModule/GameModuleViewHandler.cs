using Framework.Runtime.MAsset;
using Framework.Runtime.UI;
using Game.Modules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Framework.Runtime.MGameModule
{
    public class GameModuleViewHandler : GameModuleHandler
    {
        //string path = AssetPathEncoder.EncodeEnvAssetLink("Assets/AddressableResources/UI/Test/Prefabs/TestUPanel.prefab");
        //PanelManager.Instance.OpenPanel<TestUPanel>(path);
        public static T OpenPanel<T>(string prefabPath) where T : Panel
        {
            string link  = AssetPathEncoder.EncodeEnvAssetLink(prefabPath);
            return PanelManager.Ins.OpenPanel<T>(link);
        }
        public static T OpenPanel<T>() where T : Panel
        {
            return Panel.OpenPanel<T>();
        }
        public static T FindPanel<T>() where T : Panel
        {
            return PanelManager.Ins.FindPanel<T>();
        }
        public static void ClosePanel<T>() where T : Panel
        {
            Panel.ClosePanel<T>();
        }
    }
}


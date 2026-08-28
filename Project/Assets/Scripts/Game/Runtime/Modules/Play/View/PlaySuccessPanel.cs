using Framework.Runtime.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.MAsset;
using Framework.Runtime.Config;
using System;
using Framework.Runtime;
namespace Game.Modules
{
    public class PlaySuccessPanel : Panel
    {
        #region PrefabBinder 自动引用区域 开始
        private Framework.Runtime.UI.UButton ubtnNextLevel;
        private Framework.Runtime.UI.UTMPText utmpTxtTitle;
        private Framework.Runtime.UI.UButton ubtnHome;
        private Framework.Runtime.UI.UImage imgBg;

        #endregion PrefabBinder 自动引用区域 结束

        protected override void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
        {
            this.ubtnNextLevel = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnNextLevel");
            this.utmpTxtTitle = prefabBinder.GetObj<Framework.Runtime.UI.UTMPText>("utmpTxtTitle");
            this.ubtnHome = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnHome");
            this.imgBg = prefabBinder.GetObj<Framework.Runtime.UI.UImage>("imgBg");

        }
        public override int GetOpenLayer(int externalLayer)
        {
            return externalLayer;

        }

        public override string GetAssetLink(string outAssetLink)
        {
            string assetPath = "Assets/AddressableResources/UI/Play/Prefabs/PlaySuccessPanel.prefab";
            return AssetPathEncoder.EncodeEnvAssetLink(assetPath, AssetType.PrefabAsset);

        }
        /// <summary>
        /// 子类重写，构造函数中调用
        /// </summary>
        protected override void OnInit()
        {

        }
        /// <summary>
        /// 显示对象初始化UI,当绑定的预制体加载完成后回调(子类重写)
        /// </summary>
        protected override void OnInitUI()
        {
            ubtnHome.AddClick(OnReturnHomeClick);
            ubtnNextLevel.AddClick(OnPlayNextLevelClick);
        }

        private void OnReturnHomeClick()
        {
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_return_home);

        }
        private void OnPlayNextLevelClick()
        {
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_entryGamePlay);
        }


        /// <summary>
        /// 注册页面消息，次于 OnInitUI 之后执行
        /// </summary>
        protected override void OnSubscribeMessages()
        {

        }
        /// <summary>
        /// 当绑定UI加载完成且UI显示回调(子类重写)
        /// </summary>
        protected override void OnShow()
        {

        }
        /// <summary>
        /// 当绑定UI加载完成且UI隐藏回调(子类重写)    
        /// </summary>
        protected override void OnHide()
        {

        }
        /// <summary>
        /// 当绑定UI加载完成且数据更新的时候调用(子类重写)
        /// </summary>
        /// <param name="data"></param>

        protected override void OnGUI(object data)
        {

        }
        /// <summary>
        /// 当绑定UI被删除时回调(子类重写)
        /// </summary>
        public override void OnDestroy()
        {

        }
        protected override void StartCommonPanelHideEff(Action hideCompleted)
        {
            DisvisibleUI();
            hideCompleted?.Invoke();
        }
    }
}



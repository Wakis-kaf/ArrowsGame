using Framework.Runtime.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.MAsset;
using Framework.Runtime.Config;
using System;
using Game.Modules.GModuleArrows;
namespace Game.Modules
{
    public class PlayFailPanel : Panel
    {
        #region PrefabBinder 自动引用区域 开始
        private Framework.Runtime.UI.UButton ubtnRestart;
        private Framework.Runtime.UI.UButton ubtnRevival;
        private Framework.Runtime.UI.USprite uspHeart;
        private Framework.Runtime.UI.USprite uspHeartRoot;
        private Framework.Runtime.UI.UTMPText utmpTxtTitle;
        private Framework.Runtime.UI.USprite uspContainer;

        #endregion PrefabBinder 自动引用区域 结束

        protected override void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
        {
            this.ubtnRestart = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnRestart");
            this.ubtnRevival = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnRevival");
            this.uspHeart = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspHeart");
            this.uspHeartRoot = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspHeartRoot");
            this.utmpTxtTitle = prefabBinder.GetObj<Framework.Runtime.UI.UTMPText>("utmpTxtTitle");
            this.uspContainer = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspContainer");

        }
        public override int GetOpenLayer(int externalLayer)
        {
            return externalLayer;

        }

        public override string GetAssetLink(string outAssetLink)
        {
            string assetPath = "Assets/AddressableResources/UI/Play/Prefabs/PlayFailPanel.prefab";
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
            ubtnRestart.onClick.AddListener(OnRestartClick);
            ubtnRevival.onClick.AddListener(OnRevivalClick);
        }
        private void OnRevivalClick()
        {
            CanvasGroup.interactable = false;
            LevelVO.Current.TryRevivalGame(OnRevivalSuccess, OnRevivalFail);
        }
        private void OnRevivalSuccess()
        {
            CanvasGroup.interactable = true;
            CloseWindow();
        }
        private void OnRevivalFail()
        {
            CanvasGroup.interactable = true;
        }
        private void OnRestartClick()
        {
            LevelVO.Current.ReStartGame();
            CloseWindow();
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
    }
}



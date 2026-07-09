using Framework.Runtime.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.MAsset;
using Framework.Runtime.Config;
using Framework.Runtime;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DG.Tweening;
namespace Game.Modules
{
    public class GameArrowsLoadingOption : GameLoadingOption
    {

    }
    public class GameArrowsLoadingPanel : Panel
    {
        #region PrefabBinder 自动引用区域 开始
        private Framework.Runtime.UI.UText utxtTip;
        private Framework.Runtime.UI.UProgressBar upbLoading;
        private Framework.Runtime.UI.UImage uimgLogo;

        #endregion PrefabBinder 自动引用区域 结束
        private GameLoadingOption currentOption;
        private TweenerCore<float, float, FloatOptions> loadingTweener;

        protected override void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
        {
            this.utxtTip = prefabBinder.GetObj<Framework.Runtime.UI.UText>("utxtTip");
            this.upbLoading = prefabBinder.GetObj<Framework.Runtime.UI.UProgressBar>("upbLoading");
            this.uimgLogo = prefabBinder.GetObj<Framework.Runtime.UI.UImage>("uimgLogo");

        }
        public override int GetOpenLayer(int externalLayer)
        {
            return externalLayer;

        }
        public override string GetAssetLink(string outAssetLink)
        {
            string assetPath = "Assets/AddressableResources/UI/GameLoading/Prefabs/GameArrowsLoadingPanel.prefab";
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
            base.OnGUI(data);
            if (!(data is GameLoadingOption loadingOption))
            {
                return;
            }
             ;

            if (loadingOption == currentOption)
            {
                return;
            }

            if (currentOption != null)
            {
                this.loadingTweener?.Complete();
                this.loadingTweener = null;
            }

            this.utxtTip.text = loadingOption.tipText;
            this.upbLoading.minValue = loadingOption.minValue;
            this.upbLoading.maxValue = loadingOption.maxValue;
            currentOption = loadingOption;
            this.loadingTweener = DOTween.To(ProgressGet,
                ProgressSet,
                loadingOption.targetValue,
                loadingOption.timer).OnComplete(OnTweenerOver);
        }

        private void OnTweenerOver()
        {
            currentOption.completeCb?.Invoke();
        }

        private float ProgressGet()
        {
            return this.upbLoading.value;
        }

        private void ProgressSet(float value)
        {
            this.upbLoading.value = value;
            currentOption.updateCb?.Invoke(value);
        }
        /// <summary>
        /// 当绑定UI被删除时回调(子类重写)
        /// </summary>
        public override void OnDestroy()
        {

        }
    }

}


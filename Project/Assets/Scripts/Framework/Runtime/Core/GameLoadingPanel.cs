using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Framework.Runtime.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Runtime
{
    public class GameLoadingPanel : Panel
    {
        private GameLoadingOption currentOption;
        private TweenerCore<float, float, FloatOptions> loadingTweener;
        private UProgressBar upbLoading;
        private UText utxtTip;

        public void SetProcess(float timer, float targetValue, string tip = "")
        {
        }
        protected override void OnInitUI()
        {
            base.OnInitUI();
            this.upbLoading = this.PrefabBinder.GetObj<UProgressBar>("upbLoading");
            this.utxtTip = this.PrefabBinder.GetObj<UText>("utxtTip");
        }
        protected override void OnShow()
        {
            base.OnShow();
        }
        protected override void OnHide()
        {
            base.OnHide();
        }
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
    }
}
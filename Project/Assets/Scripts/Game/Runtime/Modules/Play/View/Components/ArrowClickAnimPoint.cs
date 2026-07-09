using Framework.Runtime.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.MAsset;
using Framework.Runtime.Config;
using System;
using DG.Tweening;
using Framework.Runtime.MObjectPool.Core;

namespace Game.Modules
{
    public class ArrowClickAnimPoint : DisplayUnit, IPoolElement
    {
        #region PrefabBinder 自动引用区域 开始
        private Framework.Runtime.UI.USprite uspInner;
        private Framework.Runtime.UI.USprite uspMiddle;
        private Framework.Runtime.UI.USprite uspOuter;
        private UnityEngine.RectTransform rtPointAnimRoot;
        #endregion PrefabBinder 自动引用区域 结束

        private Action<ArrowClickAnimPoint> m_OnComplete;
        private bool m_IsAnimPlayDirty;

        public bool IsInPool { get; set; }
        public Pool Pool { get; set; }
        protected override void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
        {
            this.uspInner = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspInner");
            this.uspMiddle = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspMiddle");
            this.uspOuter = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspOuter");
            this.rtPointAnimRoot = prefabBinder.GetObj<UnityEngine.RectTransform>("rtPointAnimRoot");
        }

        public override int GetOpenLayer(int externalLayer)
        {
            return externalLayer;
        }

        public override string GetAssetLink(string outAssetLink)
        {
            string assetPath = "Assets/AddressableResources/UI/Play/Prefabs/View/ArrowClickAnimPoint.prefab";
            return AssetPathEncoder.EncodeEnvAssetLink(assetPath, AssetType.PrefabAsset);
        }

        public void PlayTipAnim(ArrowClickPointTipWindowData data, Action<ArrowClickAnimPoint> onComplete = null)
        {
            m_IsAnimPlayDirty = true;
            m_OnComplete = onComplete;
            SetData(data);
        }

        protected override void OnGUI(object data)
        {
            TryPlayTipAnim();
        }

        private void TryPlayTipAnim()
        {
            if (!m_IsAnimPlayDirty) return;
            m_IsAnimPlayDirty = false;

            ArrowClickPointTipWindowData windowData = Data as ArrowClickPointTipWindowData;
            if (windowData == null) return;

            RectTransform parentRT = rtPointAnimRoot.parent as RectTransform;
            if (parentRT == null) return;

            Canvas canvas = rtPointAnimRoot.GetComponentInParent<Canvas>();
            Camera uiCam = (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : UIRootCamera.Camera;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, windowData.showTipScreenPos, uiCam, out Vector2 localPos);
            rtPointAnimRoot.anchoredPosition = localPos;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, windowData.showTipScreenPos, uiCam, out Vector2 localCenter);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, windowData.showTipScreenPos + new Vector2(windowData.innerScreeRadius, 0f), uiCam, out Vector2 localEdge);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, windowData.showTipScreenPos + new Vector2(windowData.outerScreeRadius, 0f), uiCam, out Vector2 localOutEdge);
            float uiRadius = Vector2.Distance(localCenter, localEdge);
            float uiOutRadius = Vector2.Distance(localCenter, localOutEdge);
            float uiDiameter = uiRadius * 2f;
            float uiOutDiameter = uiOutRadius * 2f;

            var rtInner = uspInner.GetComponent<RectTransform>();
            var rtMiddle = uspMiddle.GetComponent<RectTransform>();
            var rtOuter = uspOuter.GetComponent<RectTransform>();

            rtInner.sizeDelta = new Vector2(uiDiameter, uiDiameter);
            rtMiddle.sizeDelta = new Vector2(uiDiameter, uiDiameter);
            rtOuter.sizeDelta = new Vector2(uiOutDiameter, uiOutDiameter);

            var imgInner = uspInner.GetComponent<UnityEngine.UI.Image>();
            var imgMiddle = uspMiddle.GetComponent<UnityEngine.UI.Image>();
            var imgOuter = uspOuter.GetComponent<UnityEngine.UI.Image>();

            rtInner.localScale = Vector3.one;
            rtMiddle.localScale = Vector3.one;
            rtOuter.localScale = Vector3.one;

            if (imgInner != null) imgInner.color = new Color(imgInner.color.r, imgInner.color.g, imgInner.color.b, 0f);
            if (imgMiddle != null) imgMiddle.color = new Color(imgMiddle.color.r, imgMiddle.color.g, imgMiddle.color.b, 0f);
            if (imgOuter != null) imgOuter.color = new Color(imgOuter.color.r, imgOuter.color.g, imgOuter.color.b, 0f);

            rtPointAnimRoot.DOKill();
            rtInner.DOKill();
            rtMiddle.DOKill();
            rtOuter.DOKill();
            imgInner?.DOKill();
            imgMiddle?.DOKill();
            imgOuter?.DOKill();

            Sequence seq = DOTween.Sequence();
            float totalDuration = windowData.fadeInTime + windowData.fadeOutTime;

            if (imgInner != null)
            {
                seq.Insert(0f, imgInner.DOFade(1f, windowData.fadeInTime));
                seq.Insert(windowData.fadeInTime, imgInner.DOFade(0f, windowData.fadeOutTime));
            }

            if (imgMiddle != null)
            {
                seq.Insert(0f, imgMiddle.DOFade(0.8f, windowData.fadeInTime));
                seq.Insert(windowData.fadeInTime, imgMiddle.DOFade(0f, windowData.fadeOutTime));
            }
            seq.Insert(0f, rtMiddle.DOScale(windowData.maxOuterRadiusScale * 0.65f, totalDuration).SetEase(Ease.OutQuad));

            if (imgOuter != null)
            {
                seq.Insert(0.05f, imgOuter.DOFade(0.5f, windowData.fadeInTime));
                seq.Insert(windowData.fadeInTime + 0.05f, imgOuter.DOFade(0f, windowData.fadeOutTime));
            }
            seq.Insert(0.05f, rtOuter.DOScale(windowData.maxOuterRadiusScale, totalDuration - 0.05f).SetEase(Ease.OutQuad));

            seq.OnComplete(() =>
            {
                m_OnComplete?.Invoke(this);
                m_OnComplete = null;
            });
        }
        public void OnCreateInPool()
        {

        }

        public void OnDestroyByPool()
        {

        }

        public void OnGetFromPool()
        {

        }

        public void OnPrewarmInPool()
        {

        }

        public void OnPutToPool()
        {

        }
    }
}
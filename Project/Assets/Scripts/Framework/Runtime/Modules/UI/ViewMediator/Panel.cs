using DG.Tweening;
using Framework.Runtime.Config;
using Framework.Runtime.MAsset;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Runtime.UI
{
    public class Panel : DisplayUnit
    {
        private UPanel m_Panel;

        protected float m_ShowEffectDuration = 0.15f;
        protected float m_HideEffectDuration = 0.1f;

        public UPanel UIPanel => m_Panel;

        protected override void OnStartHideEffect(Action hideCompleted)
        {
            if (!UIPanel.hideAnimCaller.CanCall())
            {
                StartCommonPanelHideEff(hideCompleted);

            }
            else
            {
                if (CanvasGroup != null)
                {
                    CanvasGroup.blocksRaycasts = false;
                    CanvasGroup.interactable = false;
                }
                UIPanel.hideAnimCaller.Call(hideCompleted);
            }


        }
        protected Sequence m_ShowSequence;
        protected virtual void InitShowSequence()
        {
            if (m_ShowSequence != null) return;
            m_ShowSequence = DOTween.Sequence();
            m_ShowSequence.Join(CanvasGroup.DOFade(1f, m_ShowEffectDuration));
            if (UIPanel.UseCommonScaleEffect)
            {
                m_ShowSequence.Join(RectTransform.DOScale(Vector3.one * 1.05f, m_ShowEffectDuration));
                m_ShowSequence.Append(RectTransform.DOScale(Vector3.one, 0.1f));
            }
            m_ShowSequence.SetAutoKill(false)  // 不自动销毁，可以重复使用
                             .SetEase(Ease.Linear).Pause().SetUpdate(true); // 设置缓动类型
        }
        protected override void OnClearHideEffect()
        {
            if (!UIPanel.UseCommonVisibleEffect)
            {
                base.OnClearHideEffect();
                return;
            }
            if (m_HideSequence == null) return;
            m_HideSequence?.Complete();
        }
        protected override void OnClearShowEffect()
        {
            if (!UIPanel.UseCommonVisibleEffect)
            {
                base.OnClearShowEffect();
                return;
            }
            if (m_ShowSequence == null) return;
            m_ShowSequence?.Complete();

        }
        protected Sequence m_HideSequence;
        protected virtual void InitHideSequence()
        {
            if (m_HideSequence != null) return;
            m_HideSequence = DOTween.Sequence();
            m_HideSequence.Join(CanvasGroup.DOFade(0f, m_HideEffectDuration));
            if (UIPanel.UseCommonScaleEffect)
            {
                m_HideSequence.Join(RectTransform.DOScale(Vector3.one * 0.8f, m_HideEffectDuration));
            }
            // 设置 Sequence 的其他参数
            m_HideSequence.SetAutoKill(false)  // 不自动销毁，可以重复使用
                             .SetEase(Ease.Linear).Pause().SetUpdate(true); // 设置缓动类型
        }

        protected virtual void StartCommonPanelHideEff(Action hideCompleted)
        {
            if (!UIPanel.UseCommonVisibleEffect)
            {
                base.OnStartHideEffect(hideCompleted);
                return;
            }
            InitHideSequence();
            if (CanvasGroup)
            {
                CanvasGroup.blocksRaycasts = false;
                CanvasGroup.interactable = false;
                CanvasGroup.alpha = 1;
            }
            RectTransform.localScale = Vector3.one;
            m_HideSequence.Restart();
            m_HideSequence.OnComplete(hideCompleted.Invoke);

        }
        protected virtual void StartCommonPanelShowEff(Action showCompleted)
        {
            if (!UIPanel.UseCommonVisibleEffect)
            {
                base.OnStartShowEffect(showCompleted);
                return;
            }
            InitShowSequence();
            if (CanvasGroup)
            {
                CanvasGroup.blocksRaycasts = true;
                CanvasGroup.interactable = true;
                CanvasGroup.alpha = 0;
            }
            RectTransform.localScale = Vector3.one;
            m_ShowSequence.Restart();
            m_ShowSequence.OnComplete(showCompleted.Invoke);

        }
        protected override void DoHide()
        {
            HideBgMask();
            base.DoHide();
        }
        protected override void DoShow()
        {
            ShowBgMask();
            base.DoShow();
        }

        protected override void OnStartShowEffect(Action showCompleteCb)
        {
            if (!UIPanel.showAnimCaller.CanCall())
            {
                StartCommonPanelShowEff(showCompleteCb);
            }
            else
            {
                if (CanvasGroup != null)
                {
                    CanvasGroup.blocksRaycasts = true;
                    CanvasGroup.interactable = true;
                }
                UIPanel.showAnimCaller.Call(showCompleteCb);
            }

        }
        public static T OpenPanel<T>() where T : Panel
        {
            return PanelManager.Ins.OpenPanel<T>("");
        }
        public static void ClosePanel<T>() where T : Panel
        {
            PanelManager.Ins.ClosePanel<T>();
        }
        public override void CloseWindow()
        {
            PanelManager.Ins.ClosePanel(this);
        }
        public override void Destroy()
        {

            PanelManager.Ins.DestroyPanel(this);
        }
        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            ClearBgMask();
            m_ShowSequence?.Kill();
            m_ShowSequence = null;
            m_HideSequence?.Kill();
            m_HideSequence = null;
        }
        public override void OnDestroy()
        {

        }
        public void DisableBgMask()
        {
            m_Panel?.DisableBgMask();
        }

        public void EnableBgMask()
        {
            m_Panel?.EnableBgMask();
        }

        public void HideBgMask()
        {
            m_Panel?.HideBgMask();
        }
        public void ClearBgMask()
        {
            m_Panel?.ClearBgMask();
        }

        public override void OnSortOrderReset(int sortOrder)
        {
            base.OnSortOrderReset(sortOrder);
            if (m_Panel != null && m_Panel.BgMask != null)
            {
                Vector3 pos = RectTransform.position;
                var maskRt = m_Panel.BgMask.rectTransform;
                maskRt.position = pos;
                Vector3 apos = RectTransform.anchoredPosition3D;
                apos.z += WindowLayer.PANEL_MASK_GAP;
                if (m_Panel.BgMaskCanvas != null)
                {
                    m_Panel.BgMaskCanvas.overrideSorting = true;
                    m_Panel.BgMaskCanvas.sortingOrder = SortOrder - 1;
                }
                maskRt.anchoredPosition3D = apos;
            }
        }

        public override void OpenWindow()
        {
            PanelManager.Ins.OpenPanel(this, CurLayer);
        }
        public bool IsInLayerTop()
        {
            return UIWindow.Ins.IsInLayerTop(this);
        }
        public override int GetOpenLayer(int externalLayer)
        {
            return externalLayer;
        }
        public void ShowBgMask()
        {
            m_Panel?.ShowBgMask();
        }

        protected override void ExtractComponent(GameObject go)
        {
            base.ExtractComponent(go);
            m_Panel = go.GetOrAddComponent<UPanel>();
        }

        public virtual UImage CreatePanelBgMask(Transform maskRoot, bool createMaskCanvas = false)
        {
            GameObject markGo = new GameObject($"BgMask[{UIPanel.gameObject.name}]");
            UImage img = markGo.AddComponent<UImage>();
            if (createMaskCanvas)
            {
                Canvas canvas = markGo.AddComponent<Canvas>();
                markGo.GetOrAddComponent<GraphicRaycaster>();
                canvas.overrideSorting = true;
            }
            RectTransform markRT = markGo.GetOrAddComponent<RectTransform>();
            markGo.SetActive(true);
            markGo.transform.SetParent(maskRoot, false);
            markRT.SetAnchor(AnchorPresets.StretchAll);
            markRT.SetOffsetZero();
            markGo.layer = 5;
            // 设置默认贴图
            img.type = Image.Type.Sliced;
            img.color = UIPanel == null ? new Color(0, 0, 0, 0.85f) : UIPanel.bgMaskColor;
            return img;
        }
    }
}
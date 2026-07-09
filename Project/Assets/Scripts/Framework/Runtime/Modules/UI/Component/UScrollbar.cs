using DG.Tweening;

using Framework.Utils;

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Runtime.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UScrollbar : Scrollbar
    {
        private CanvasGroup m_CanvasGroup;
        private Tweener m_HideTweener;
        private bool m_IsAlphaZeroDisable = true;
        private RectTransform m_RectTransform;
        private Vector2 m_RTSize;
        private Tweener m_ShowTweener;

        public CanvasGroup CanvasGroup
        {
            get
            {
                if (m_CanvasGroup == null)
                    m_CanvasGroup = GameObjectUtil.GetOrAddComponent<CanvasGroup>(gameObject);
                return m_CanvasGroup;
            }
        }

        public RectTransform RectTransform
        {
            get
            {
                if (m_RectTransform == null)
                {
                    m_RectTransform = GetComponent<RectTransform>();
                }

                return m_RectTransform;
            }
        }

        public Vector2 Size
        {
            get => m_RTSize;
            set
            {
                if (value != m_RTSize)
                {
                    RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value.x);
                    RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value.y);
                }
            }
        }

        public void Hide()
        {
            SetAlphaAndCheck(0);
        }

        public void HideSmooth(float duration = 0.1f)
        {
            if (m_ShowTweener == null)
            {
                m_ShowTweener = DOTween.To(AlphaGetter, AlphaSetter, 0f, duration).SetAutoKill(false).Play();
            }
            else
            {
                m_ShowTweener.ChangeValues(CanvasGroup.alpha, 0f, duration).Play();
            }
        }

        public void SetAlpha(float alpha)
        {
            SetAlphaAndCheck(alpha);
        }

        public void Show()
        {
            SetAlphaAndCheck(1);
        }

        public void ShowSmooth(float duration = 0.1f)
        {
            if (m_ShowTweener == null)
            {
                m_ShowTweener = DOTween.To(AlphaGetter, AlphaSetter, 1f, duration).SetAutoKill(false).Play();
            }
            else
            {
                m_ShowTweener.ChangeValues(CanvasGroup.alpha, 1f, duration).Play();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_ShowTweener?.Kill(true);
        }

        private float AlphaGetter()
        {
            return CanvasGroup.alpha;
        }

        private void AlphaSetter(float alpha)
        {
            SetAlphaAndCheck(alpha);
        }

        private void SetAlphaAndCheck(float alpha)
        {
            CanvasGroup.alpha = alpha;
            interactable = m_IsAlphaZeroDisable ? Math.Abs(alpha) < 0.001f ? false : true : interactable;
        }
    }
}
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.Runtime.UI.UIAnimae.Tweeners
{
    [System.Serializable]
    public class CanvasGroupAlphaTweener : UITweener
    {
        [ShowIf("@this.customEase == true")]
        public AnimationCurve customCurve;

        public bool customEase = false;
        public float duration = 0.1f;

        [ShowIf("@this.customEase == false")]
        public Ease easeType = Ease.Linear;
        public bool useInitAlpha = true;
        [ShowIf("@this.useInitAlpha == false")]
        public float initAlpha = 0;
        public float targetAlpha = 1;
        public CanvasGroup targetCanvasGroup;
        private Tweener m_AlphaTweener;
        private float m_OriginAlpha;

        public override bool IsComplete()
        {
            return m_AlphaTweener.IsComplete();
        }

        public override bool IsEnableAndActive(UITweenContext context = null)
        {
            return base.IsEnableAndActive(context) && targetCanvasGroup != null;
        }

        protected override bool CanAutoComplete()
        {
            return false;
        }

        protected override void OnInit(UITweenContext context = null)
        {
            if (useInitAlpha)
            {
                m_OriginAlpha = targetCanvasGroup.alpha;
            }
            else {
                m_OriginAlpha = initAlpha;
                targetCanvasGroup.alpha = initAlpha;
            }

                m_AlphaTweener = DOTween.To(AlphaGetter, AlphaSetter, targetAlpha, duration);
            if (customEase)
            {
                m_AlphaTweener.SetEase(customCurve);
            }
            else
            {
                m_AlphaTweener.SetEase(easeType);
            }
            m_AlphaTweener?.Pause().SetAutoKill(false).OnComplete(CallComplete).SetUpdate(true);
        }

        protected override void OnPause(UITweenContext context = null)
        {
            m_AlphaTweener?.Pause();
        }

        protected override void OnPlay(UITweenContext context = null)
        {
            m_AlphaTweener?.Play();
        }

        protected override void OnPlayBackwards(UITweenContext context = null)
        {
            m_AlphaTweener?.PlayBackwards();
        }

        protected override void OnPlayForward(UITweenContext context = null)
        {
            m_AlphaTweener.PlayForward();
        }

        protected override void OnRestart(UITweenContext context = null)
        {
            m_AlphaTweener?.Restart();
        }

        protected override void OnStop(UITweenContext context = null)
        {
            //m_AlphaTweener?.Rewind();
            //targetCanvasGroup.alpha = m_OriginAlpha;
        }
        protected override void OnComplete(UITweenContext context = null)
        {
            m_AlphaTweener.Complete();
        }
        protected override void OnRewind(UITweenContext context = null)
        {
            targetCanvasGroup.alpha = m_OriginAlpha;
        }

        private float AlphaGetter()
        {
            return targetCanvasGroup.alpha;
        }

        private void AlphaSetter(float color)
        {
            targetCanvasGroup.alpha = color;
        }
    }
}
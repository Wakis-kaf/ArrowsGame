using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Runtime.UI.UIAnimae.Tweeners
{
    [Serializable]
    public class SpriteColorTweener : UITweener
    {
        [ShowIf("@this.customEase == true")]
        public AnimationCurve customCurve;

        public bool customEase = false;
        public float duration = 0.1f;

        [ShowIf("@this.customEase == false")]
        public Ease easeType = Ease.Linear;

        public Color targetColor = Color.gray;
        public Image targetImage;
        private Tweener m_ColorTweener;
        private Color m_OriginColor;

        public override bool IsComplete()
        {
            return m_ColorTweener.IsComplete();
        }

        public override bool IsEnableAndActive(UITweenContext context = null)
        {
            return base.IsEnableAndActive(context) && targetImage != null;
        }

        protected override bool CanAutoComplete()
        {
            return false;
        }

        protected override void OnInit(UITweenContext context = null)
        {
            m_OriginColor = targetImage.color;
            m_ColorTweener = DOTween.To(ColorGetter, ColorSetter, targetColor, duration);
            if (customEase)
            {
                m_ColorTweener.SetEase(customCurve);
            }
            else
            {
                m_ColorTweener.SetEase(easeType);
            }
            m_ColorTweener?.Pause().SetAutoKill(false).OnComplete(CallComplete).SetUpdate(true);
        }

        protected override void OnPause(UITweenContext context = null)
        {
            m_ColorTweener?.Pause();
        }

        protected override void OnPlay(UITweenContext context = null)
        {
            if (duration <= 0)
            {
                targetImage.color = targetColor;
            }
            else
            {
                m_ColorTweener?.PlayBackwards();

            }
        }

        protected override void OnPlayBackwards(UITweenContext context = null)
        {
            if (duration <= 0)
            {
                targetImage.color = m_OriginColor;
            }
            else
            {
                m_ColorTweener?.PlayBackwards();

            }
 
        }

        protected override void OnPlayForward(UITweenContext context = null)
        {
            if (duration <= 0)
            {
                targetImage.color = targetColor;
            }
            else
            {
                m_ColorTweener?.PlayForward();

            }
        }

        protected override void OnRestart(UITweenContext context = null)
        {
             m_ColorTweener?.Restart();
        }

        protected override void OnStop(UITweenContext context = null)
        {
            //m_ColorTweener?.Complete();
            //targetImage.color = m_OriginColor;
        }
        protected override void OnComplete(UITweenContext context = null)
        {
            m_ColorTweener?.Complete();
        }
        protected override void OnRewind(UITweenContext context = null)
        {
            m_ColorTweener?.Rewind();
        }
        private Color ColorGetter()
        {
            return targetImage.color;
        }

        private void ColorSetter(Color color)
        {
            targetImage.color = color;
        }
    }
}
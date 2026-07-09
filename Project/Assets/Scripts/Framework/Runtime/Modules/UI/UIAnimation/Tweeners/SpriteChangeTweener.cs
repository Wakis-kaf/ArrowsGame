using System;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Runtime.UI.UIAnimae.Tweeners
{
    [Serializable]
    public class SpriteChangeTweener : UITweener
    {
        public Image targetImage;
        public Sprite targetSprite;
        private Sprite m_OriginSprite;

        public override bool IsEnableAndActive(UITweenContext context = null)
        {
            return base.IsEnableAndActive(context) && targetSprite != null;
        }

        protected override void OnInit(UITweenContext context = null)
        {
            m_OriginSprite = targetImage.sprite;
        }

        protected override void OnPlay(UITweenContext context = null)
        {
            targetImage.sprite = targetSprite;
        }

        protected override void OnStop(UITweenContext context = null)
        {
            
        }
        protected override void OnRewind(UITweenContext context = null)
        {
            targetImage.sprite = m_OriginSprite;
        }
        protected override void OnComplete(UITweenContext context = null)
        {
            targetImage.sprite = targetSprite;
        }
    }
}
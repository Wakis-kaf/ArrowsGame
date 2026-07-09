using UnityEngine.UI;

namespace Framework.Runtime.UI.UIAnimae.Tweeners
{
    public class TextChangeTweener : UITweener
    {
        public string targetContent;
        public Text targetText;
        private string m_OriginContent;

        public override bool IsEnableAndActive(UITweenContext context = null)
        {
            return base.IsEnableAndActive(context) && targetText != null;
        }

        protected override void OnInit(UITweenContext context = null)
        {
            m_OriginContent = targetText.text;
        }

        protected override void OnPlay(UITweenContext context = null)
        {
            targetText.text = targetContent;
        }

        protected override void OnPlayBackwards(UITweenContext context = null)
        {
            targetText.text = m_OriginContent;
        }

        protected override void OnPlayForward(UITweenContext context = null)
        {
            targetText.text = targetContent;
        }

        protected override void OnStop(UITweenContext context = null)
        {
            //targetText.text = m_OriginContent;
        }
        protected override void OnRewind(UITweenContext context = null)
        {
            targetText.text = m_OriginContent;
        }
        protected override void OnComplete(UITweenContext context = null)
        {
            targetText.text = targetContent;
        }
    }
}
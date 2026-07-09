using System;
using UnityEngine;

namespace Framework.Runtime.UI.UIAnimae.Tweeners
{
    [Serializable]
    public class AnimationControlTweener : UITweener
    {
        public string playAnimName;

        public int playSpeed = 1;

        // 使用参考 https://blog.csdn.net/Jeffxu_lib/article/details/90602531
        public Animation targetAnimation;

        private float m_Timer = 0;
        protected override void OnInit(UITweenContext context = null)
        {
            base.OnInit(context);
            m_Timer = Time.time;
        }
        public override bool IsComplete()
        {
            return IsPlaying() && Time.time - m_Timer >= targetAnimation.GetClip(playAnimName).length;
        }

        public override bool IsEnableAndActive(UITweenContext context = null)
        {
            return base.IsEnableAndActive(context) && targetAnimation != null && targetAnimation.GetClip(playAnimName) != null;
        }

        public override bool IsPlaying()
        {
            return targetAnimation.isPlaying;
        }

        protected override void OnPause(UITweenContext context = null)
        {
            targetAnimation[playAnimName].speed = 0;
        }

        protected override void OnPlay(UITweenContext context = null)
        {
            if (!IsPlaying())
            {
                ResetTimer();
            }
            targetAnimation[playAnimName].speed = playSpeed;
            targetAnimation.Play(playAnimName);
        }

        protected override void OnPlayBackwards(UITweenContext context = null)
        {
            targetAnimation[playAnimName].speed = -playSpeed;
        }

        protected override void OnPlayForward(UITweenContext context = null)
        {
            targetAnimation[playAnimName].speed = playSpeed;
        }

        protected override void OnRestart(UITweenContext context = null)
        {
            targetAnimation.Stop();
            targetAnimation[playAnimName].speed = playSpeed;
            targetAnimation.Play(playAnimName);
            ResetTimer();
        }

        protected override void OnStop(UITweenContext context = null)
        {
            targetAnimation.Stop(playAnimName);
        }
        protected override void OnComplete(UITweenContext context = null)
        {

        }
        protected override void OnRewind(UITweenContext context = null)
        { 
        }

        private void ClearTimer()
        {
            m_Timer = 0;
            GameApp.Ins.LoopManager.RemoveTimeout(CallComplete);
        }

        private void MarkTimer()
        {
            m_Timer = Time.time;
            float timeOut = targetAnimation.GetClip(playAnimName).length;
            GameApp.Ins.LoopManager.AddTimeout(CallComplete, timeOut);
        }

        private void ResetTimer()
        {
            ClearTimer();
            MarkTimer();
        }
    }
}
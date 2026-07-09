using System;

namespace Framework.Runtime.UI.UIAnimae
{
    public enum TweenerCallType
    {
        Play,
        PlayForward,
        PlayBackwards,
        Stop,
        Pause,
        Restart,
        Complete,
        Rewind,
    }

    [Serializable]
    public class UIAnimatorCaller
    {
        public TweenerCallType callType = TweenerCallType.Restart;
        public UITweenContext ctx;
        public UIAnimator targetAnimator;
        public string targetSequence;

        public bool CanCall()
        {
            return targetAnimator != null && targetAnimator.HasSequenece(targetSequence);
        }
        public UIAnimatorCaller()
        {
            ctx = new UITweenContext();
        }

        public void Call(Action cb = null)
        {
            targetAnimator?.SetComplete(targetSequence, cb);
            targetAnimator?.Call(targetSequence, callType, ctx);
        }
        public void Stop()
        {
            targetAnimator?.Call(targetSequence, TweenerCallType.Stop, ctx);
        }
        public void Complete()
        {
            targetAnimator?.Call(targetSequence, TweenerCallType.Complete, ctx);
        }
        public void Rewind()
        {
            targetAnimator?.Call(targetSequence, TweenerCallType.Rewind, ctx);
        }
    }
}
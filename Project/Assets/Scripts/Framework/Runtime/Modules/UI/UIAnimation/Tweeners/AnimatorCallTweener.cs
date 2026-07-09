using Framework.Runtime.LogSystem;
using Sirenix.OdinInspector;
using System;

namespace Framework.Runtime.UI.UIAnimae.Tweeners
{
    [Serializable]
    public class AnimatorCallTweener : UITweener
    {
        [LabelText("Call[Caller Call Pause]")]
        public TweenerCallType callTypeWhenPause;
        [LabelText("Call[Caller Call Play]")]
        public TweenerCallType callTypeWhenPlay;
        [LabelText("Call[Caller Call PlayBackwards]")]
        public TweenerCallType callTypeWhenPlayBackwards;
        [LabelText("Call[Caller Call PlayForward]")]
        public TweenerCallType callTypeWhenPlayForward;
        [LabelText("Call[Caller Call Restart]")]
        public TweenerCallType callTypeWhenRestart;
        [LabelText("Call[Caller Call Stop]")]
        public TweenerCallType callTypeWhenStop;
        [LabelText("Call[Caller Call Rewind]")]
        public TweenerCallType callTypeWhenRewind;
        [LabelText("Call[Caller Call Complete]")]
        public TweenerCallType callTypeWhenComplete;
        public UIAnimator targetAnimator;
        public string targetSequence;

        public override bool IsComplete()
        {
            return targetAnimator.FindSequence(targetSequence).IsAllComplete();
        }

        public override bool IsEnableAndActive(UITweenContext context = null)
        {
            if (targetSequence == Sequence.sequenceName)
            {
                Log.Fatal("当前Tweener未激活: ERROR: 不能设置 targetSequence 名与当前 sequenceName 名字一样，该操作会导致递归");
                return false;
            }
            return base.IsEnableAndActive(context) && targetAnimator != null && targetAnimator.FindSequence(targetSequence) != null;
        }

        protected override bool CanAutoComplete()
        {
            return false;
        }

        protected override void OnPause(UITweenContext context = null)
        {
            targetAnimator?.Call(targetSequence, callTypeWhenPause, context);
        }

        protected override void OnPlay(UITweenContext context = null)
        {
            targetAnimator?.Call(targetSequence, callTypeWhenPlay, context);
        }

        protected override void OnPlayBackwards(UITweenContext context = null)
        {
            targetAnimator?.Call(targetSequence, callTypeWhenPlayBackwards, context);
        }

        protected override void OnPlayForward(UITweenContext context = null)
        {
            targetAnimator?.Call(targetSequence, callTypeWhenPlayForward, context);
        }

        protected override void OnRestart(UITweenContext context = null)
        {
            targetAnimator?.Call(targetSequence, callTypeWhenRestart, context);
        }

        protected override void OnStop(UITweenContext context = null)
        {
            targetAnimator?.Call(targetSequence, callTypeWhenStop, context);
        }
        protected override void OnRewind(UITweenContext context = null)
        {
            targetAnimator?.Call(targetSequence, callTypeWhenRewind, context);
        }
        protected override void OnComplete(UITweenContext context = null)
        {
            targetAnimator?.Call(targetSequence, callTypeWhenComplete, context);
        }
    }
}
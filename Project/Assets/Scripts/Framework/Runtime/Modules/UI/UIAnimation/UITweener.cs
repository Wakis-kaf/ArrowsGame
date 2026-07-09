using System;

namespace Framework.Runtime.UI.UIAnimae
{
    [Serializable]
    public class UITweener
    {
        protected Action m_CompletCb;

        private UITweenSequence m_Sequence;

        private enum Status
        {
            UnInit,
            Awaked,
            Playing,
            Stoped,
        }

        public UITweenSequence Sequence => m_Sequence;

        public void BindSequence(UITweenSequence sequence)
        {
            m_Sequence = sequence;
        }

        public void Init(UITweenContext context = null)
        {
            if (!IsEnableAndActive(context)) return;
            OnInit(context);
        }

        // 判断当前tweener是否已经完成
        public virtual bool IsComplete()
        {
            return true;
        }

        public virtual bool IsEnableAndActive(UITweenContext context = null)
        {
            return true;
        }

        // 判断当前tweener是否正在播放中
        public virtual bool IsPlaying()
        {
            return false;
        }

        // 暂停播放
        public void Pause(UITweenContext context = null)
        {
            if (!IsEnableAndActive(context)) return;
            OnPause(context);
        }

        // 继续播放或者从头播放
        public void Play(UITweenContext context = null)
        {
            if (!IsEnableAndActive(context)) return;
            OnPlay(context);
            if (CanAutoComplete())
            {
                CallComplete();
            }
        }

        public void PlayBackwards(UITweenContext context = null)
        {
            if (!IsEnableAndActive(context)) return;
            OnPlayBackwards(context);
            if (CanAutoComplete())
            {
                CallComplete();
            }
        }

        public void PlayForward(UITweenContext context = null)
        {
            if (!IsEnableAndActive(context)) return;
            OnPlayForward(context);
            if (CanAutoComplete())
            {
                CallComplete();
            }
        }

        public void Restart(UITweenContext context = null)
        {
            if (!IsEnableAndActive(context)) return;
            OnRestart(context);
        }

        public void SetComplete(Action cb)
        {
            if (IsComplete())
            {
                cb?.Invoke();
            }

            m_CompletCb = cb;
        }

        // 停止播放: 即复位
        public void Stop(UITweenContext context = null)
        {
            if (!IsEnableAndActive(context)) return;
            OnStop(context);
        }
        public void Rewind(UITweenContext context = null)
        {
            if (!IsEnableAndActive(context)) return;
            OnRewind(context);
        }
        public void Complete(UITweenContext context = null)
        {
            if (!IsEnableAndActive(context)) return;
            OnComplete(context);
        }


        protected virtual bool CanAutoComplete()
        {
            return true;
        }

        protected virtual void CallComplete()
        {
            m_CompletCb?.Invoke();
        }

        protected virtual void OnInit(UITweenContext context = null)
        { }

        // 暂停播放
        protected virtual void OnPause(UITweenContext context = null)
        { }

        // 继续播放或者从头播放
        protected virtual void OnPlay(UITweenContext context = null)
        { }

        protected virtual void OnPlayBackwards(UITweenContext context = null)
        { }

        protected virtual void OnPlayForward(UITweenContext context = null)
        { }

        protected virtual void OnRestart(UITweenContext context = null)
        { }

        // 停止播放
        protected virtual void OnStop(UITweenContext context = null)
        { }
        protected virtual void OnRewind(UITweenContext context = null)
        { }
        protected virtual void OnComplete(UITweenContext context = null)
        { }
    }
}
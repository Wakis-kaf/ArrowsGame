using Framework.Runtime.LogSystem;

using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.UI.UIAnimae
{
    public enum UITweenerType
    {
        [LabelText("空白")]Empty = 0,
        [LabelText("切换图片")] ImageChange = 1,
        [LabelText("雪碧图颜色动效")] SpriteColor = 2,
        [LabelText("组件旋转动效")] TransformRotate = 3,
        [LabelText("组件缩放动效")] TransformScale = 4,
        [LabelText("组件移动动效")] TransformMove = 5,
        [LabelText("文本内容改变")] TextChange = 6,
        [LabelText("文本颜色改变")] TextColor = 7,
        [LabelText("画布透明度改变")] CanvasGroupAlpha = 8,
        [LabelText("触发动画")] CallAnimatorFunc = 9,
        [LabelText("Animation控制")] AnimationControl = 10,
        //[LabelText("超级动画")] SuperAnimation = 11,
        [LabelText("切换雪碧图")] AtlasSpriteChange = 12,
        [LabelText("GO激活切换")] GameActiveChange = 13,
    }

    public class UIAnimator : MonoBehaviour
    {
        private bool m_HasInit;
        [SerializeField] private List<UITweenSequence> m_TweenSequences = new List<UITweenSequence>();

        public void Call(string name, TweenerCallType callType, UITweenContext ctx = null)
        {
            InitCheck();
            var sequence = FindSequence(name);
            if (sequence == null) return;
            switch (callType)
            {
                case TweenerCallType.Play:
                    sequence.Play(ctx);
                    break;

                case TweenerCallType.PlayBackwards:
                    sequence.PlayBackwards(ctx);
                    break;

                case TweenerCallType.PlayForward:
                    sequence.PlayForward(ctx);
                    break;

                case TweenerCallType.Stop:
                    sequence.Stop(ctx);
                    break;

                case TweenerCallType.Restart:
                    sequence.Restart(ctx);
                    break;

                case TweenerCallType.Pause:
                    sequence.Pause(ctx);
                    break;
                case TweenerCallType.Rewind:
                    sequence.Rewind(ctx);
                    break;
                case TweenerCallType.Complete:
                    sequence.Complete(ctx);
                    break;

            }
        }

        public void Clear()
        {
            for (int i = 0; i < m_TweenSequences.Count; i++)
            {
                m_TweenSequences[i].Clear();
            }
            m_TweenSequences.Clear();
        }
        public bool HasSequenece(string name)
        {
            for (int i = 0; i < m_TweenSequences.Count; i++)
            {
                if (m_TweenSequences[i].sequenceName == name)
                {
                    return true;
                }
            }
            return false;
        }
        public UITweenSequence FindSequence(string name)
        {
            InitCheck();
            for (int i = 0; i < m_TweenSequences.Count; i++)
            {
                if (m_TweenSequences[i].sequenceName == name)
                {
                    return m_TweenSequences[i];
                }
            }
            return null;
        }

        public bool IsSequenceComplete(string name)
        {
            InitCheck();
            var sequence = FindSequence(name);
            if (sequence == null) return true;
            return sequence.IsAllComplete();
        }

        public void SetComplete(string name, Action cb)
        {
            try
            {
                InitCheck();
                var sequence = FindSequence(name);
                if (sequence == null) return;
                sequence.SetComplete(cb);
            }
            catch (Exception e)
            {
                Log.Fatal(e);
            }
        }

        private void Awake()
        {
            InitCheck();
        }

        private void InitCheck()
        {
            if (m_HasInit) return;
            m_HasInit = true;
            for (int i = 0; i < m_TweenSequences.Count; i++)
            {
                m_TweenSequences[i].Init(this);
            }
        }
    }

    public class UITweenContext
    {
        public Action cb;
    }
}
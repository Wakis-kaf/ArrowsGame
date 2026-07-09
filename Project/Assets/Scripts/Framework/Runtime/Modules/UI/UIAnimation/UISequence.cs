using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.UI.UIAnimae
{
    [Serializable]
    public class UITweenSequence
    {
        [Header("@CombinedSequenceName()")]
        public string sequenceName;
        private UIAnimator m_BindAnimator;
        private string CombinedSequenceName()
        {
            return $"序列名称:[{this.sequenceName}]";
        }
        [SerializeField] private List<UITweenerVolume> m_Volumes = new List<UITweenerVolume>();
        public UIAnimator BindAnimator => m_BindAnimator;

        public void Clear()
        {
            m_Volumes.Clear();
        }
        public bool TryFindUITweener<T>( UITweenerType type, out T tweener,string id = "") where T : UITweener
        {
            tweener = FindUITweener<T>(type,id);
            return tweener != null;
        }
        public T FindUITweener<T>(UITweenerType type, string id = "") where T : UITweener
        {
            for (int i = 0; i < m_Volumes.Count; i++)
            {
                if (m_Volumes[i].CurTweenerType == type && m_Volumes[i].Id == id
                    && m_Volumes[i].CurrentTweene is T tres)
                {
                    return m_Volumes[i].CurrentTweene as T;
                }
            }
            return null;
        }
        public UITweenerVolume FindUITweenerVolume(UITweenerType type, string id = "")
        {
            for (int i = 0; i < m_Volumes.Count; i++)
            {
                if (m_Volumes[i].CurTweenerType == type && m_Volumes[i].Id == id)
                {
                    return m_Volumes[i];
                }
            }
            return null;
        }

        public void Init(UIAnimator bindAnimator)
        {
            m_BindAnimator = bindAnimator;
            for (int i = 0; i < m_Volumes.Count; i++)
            {
                m_Volumes[i].Init(this);
            }
        }

        public bool IsAllComplete()
        {
            bool isAllComplete = true;
            for (int i = 0; i < m_Volumes.Count; i++)
            {
                if (!m_Volumes[i].CurrentTweene.IsComplete())
                {
                    isAllComplete = false;
                    break;
                }
            }
            return isAllComplete;
        }

        // 暂停播放
        public void Pause(UITweenContext context = null)
        {
            for (int i = 0; i < m_Volumes.Count; i++)
            {
                m_Volumes[i].CurrentTweene.Pause(context);
            }
        }

        public void Play(UITweenContext context = null)
        {
            for (int i = 0; i < m_Volumes.Count; i++)
            {
                m_Volumes[i].CurrentTweene.Play(context);
            }
        }

        public void PlayBackwards(UITweenContext context = null)
        {
            for (int i = 0; i < m_Volumes.Count; i++)
            {
                m_Volumes[i].CurrentTweene.PlayBackwards(context);
            }
        }

        public void PlayForward(UITweenContext context = null)
        {
            for (int i = 0; i < m_Volumes.Count; i++)
            {
                m_Volumes[i].CurrentTweene.PlayForward(context);
            }
        }

        public void Restart(UITweenContext context = null)
        {
            for (int i = 0; i < m_Volumes.Count; i++)
            {
                m_Volumes[i].CurrentTweene.Restart(context);
            }
        }

        public void SetComplete(Action cb)
        {
            for (int i = 0; i < m_Volumes.Count; i++)
            {
                m_Volumes[i].CurrentTweene.SetComplete(cb);
            }
        }

        // 停止播放
        public void Stop(UITweenContext context = null)
        {
            for (int i = 0; i < m_Volumes.Count; i++)
            {
                m_Volumes[i].CurrentTweene.Stop(context);
            }
        }
        /// <summary>
        /// 完成播放
        /// </summary>
        /// <param name="context"></param>
        public void Complete(UITweenContext context = null)
        {
            for (int i = 0; i < m_Volumes.Count; i++)
            {
                m_Volumes[i].CurrentTweene.Complete(context);
            }
        }
        /// <summary>
        /// 复位，回到起点
        /// </summary>
        /// <param name="context"></param>
        public void Rewind(UITweenContext context = null) {
            for (int i = 0; i < m_Volumes.Count; i++)
            {
                m_Volumes[i].CurrentTweene.Rewind(context);
            }
        }


        private void OnInspectorInit()
        {
            Debug.Log("OnInspectorInit");
        }
    }
}
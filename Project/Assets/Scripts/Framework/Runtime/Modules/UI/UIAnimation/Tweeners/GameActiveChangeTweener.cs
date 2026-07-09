using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Framework.Runtime.UI.UIAnimae.Tweeners
{
    [Serializable]
    public class GameActiveChangeTweener : UITweener
    {
        public GameObject targetObject;
        public bool active = true;
        private bool m_InitActive = false;
        public override bool IsComplete()
        {
            return base.IsComplete();
           
        }
        protected override void OnInit(UITweenContext context = null)
        {
            base.OnInit(context);
            RecordInit();
        }
        private void RecordInit()
        {
            if (targetObject != null)
            {
                m_InitActive = targetObject.activeSelf;
            }   
        }
        private void Resume()
        {
            if (targetObject != null)
            {
                targetObject.SetActive(m_InitActive);
            }
        }
        private void Play()
        {
            if (targetObject != null)
            {
                targetObject.SetActive(active);
            }
        }
        protected override void OnPlay(UITweenContext context = null)
        {
            base.OnPlay(context);
            Play();

        }
        protected override void OnRestart(UITweenContext context = null)
        {
            Resume();
            Play();
        }
        protected override void OnRewind(UITweenContext context = null)
        {
            Resume();
        }
        protected override void OnPlayForward(UITweenContext context = null)
        {
            Resume();
        }
        protected override void OnPlayBackwards(UITweenContext context = null)
        {
            Resume();
        }


    }
}

using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Framework.Runtime.UI.UIAnimae.Tweeners
{
    [Serializable]
    public class AtlasSpriteChangeTweener : UITweener
    {

        [SerializeField]
        private UAtlas m_Atlas;
        public UAtlas Atlas
        {
            get
            {
                return m_Atlas;
            }
            set
            {
                m_Atlas = value;
            }
        }
        public Sprite Sprite => m_Sprite;
        private Sprite m_Sprite;
        [SerializeField]
        private string m_SpriteName;
        public USprite targetSprite;
        [SerializeField]
        private string m_SpritePath;
        private string m_OriginPath;
        public string SpritePath
        {
            get
            {
                if (!string.IsNullOrEmpty(m_SpritePath))
                {
                    return m_SpritePath;
                }
                if (Atlas != null)
                {
                    m_SpritePath = Atlas.name + "." + m_SpriteName;
                }
                return m_SpritePath;
            }
        }
        public string SpriteName
        {
            get
            {
                return m_SpriteName;
            }
            set
            {
                if (!(m_SpriteName != value))
                {
                    return;
                }

                m_SpriteName = value;
                if (string.IsNullOrEmpty(m_SpriteName))
                {
                    m_Sprite = null;
                    m_SpritePath = string.Empty;
                }
                else if (Atlas != null)
                {
                    m_Sprite = Atlas.GetSprite(m_SpriteName);
                    m_SpritePath = Atlas.name + "." + m_SpriteName;
                    Debug.Log(m_SpritePath);
                }
            }
        }

        public override bool IsEnableAndActive(UITweenContext context = null)
        {
            return base.IsEnableAndActive(context) && targetSprite != null;
        }

        protected override void OnInit(UITweenContext context = null)
        {
            m_OriginPath = targetSprite.Path ;
        }

        protected override void OnPlay(UITweenContext context = null)
        {
            targetSprite.Path = m_SpritePath;
        }
        protected override void OnRestart(UITweenContext context = null)
        {
            targetSprite.Path = m_SpritePath;
        }
        protected override void OnRewind(UITweenContext context = null)
        {
            targetSprite.Path = m_OriginPath;
        }
        protected override void OnComplete(UITweenContext context = null)
        {
            targetSprite.Path = m_SpritePath;
        }
        protected override void OnStop(UITweenContext context = null)
        {
            //targetSprite.Path = m_OriginPath;
        }
        protected override void OnPlayBackwards(UITweenContext context = null)
        {
            targetSprite.Path = m_OriginPath;
        }
    }
}


using UnityEngine;
using UnityEngine.U2D;

namespace Framework.Runtime.UI
{
    [DisallowMultipleComponent]
    public class USprite2 : UImage
    {
        [SerializeField] private SpriteAtlas m_SpriteAtlas;
        [SerializeField] private string m_SpriteName;

        public SpriteAtlas SpriteAtlas
        {
            get { return m_SpriteAtlas; }
            set { m_SpriteAtlas = value; }
        }

        public string SpriteName
        {
            get { return m_SpriteName; }
            set
            {
                if (m_SpriteName != value || sprite == null || sprite.name != value)
                {
                    m_SpriteName = value;
                    if (!Application.isEditor)
                    {
                        sprite = SpriteAtlas.GetSprite(value);
                    }
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if (m_SpriteAtlas != null && !string.IsNullOrEmpty(m_SpriteName))
            {
                SpriteName = SpriteName;
            }
        }
    }
}
using Framework.Runtime.LogSystem;
using Framework.Runtime.MAsset;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Framework.Runtime.UI
{
    /// <summary>
    /// 贴图
    /// </summary>
    //public class UImage : Image, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,IPointerDownHandler,IPointerUpHandler
    public class UImage : Image, IColor, IDragEventPass
    {
        private string m_AtlasSpriteName;
        private bool m_Gray = false;
        private RectTransform m_RectTransform;
        private Vector2 m_Size;
        private string m_SpritePath;

        public string AtlasSpritePath
        {
            set
            {
                m_AtlasSpriteName = value;
            }
        }

        public bool enableDragEventPass { get; set; } = true;
        private bool m_Dimmed;
        public bool Dimmed
        {
            get
            {
                return m_Dimmed;
            }
            set
            {
                if (value != m_Dimmed)
                {
                    m_Dimmed = value;
                    if (m_Dimmed)
                    {
                        material = new Material(Shader.Find("UI/Dimmed"));
                        material.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
                    }
                    else
                    {
                        material = null;
                    }
                }
            }
        }
        public bool Gray
        {
            get
            {
                return m_Gray;
            }
            set
            {
                if (value != m_Gray)
                {
                    m_Gray = value;
                    if (m_Gray)
                    {
                        material = new Material(Shader.Find("UI/Gray"));
                        material.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
                    }
                    else
                    {
                        material = null;
                    }
                }
            }
        }

        public bool isPointerStaying { get; private set; }

        public RectTransform RectTransform
        {
            get
            {
                if (m_RectTransform == null)
                {
                    m_RectTransform = GetComponent<RectTransform>();
                }

                return m_RectTransform;
            }
        }

        public Vector2 Size
        {
            get => m_Size;
            set
            {
                if (value != m_Size)
                {
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value.x);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value.y);
                }
            }
        }

        public string SpritePath
        {
            get { return m_SpritePath; }
            set
            {
                string path = value;
                if (m_SpritePath == value)
                {
                    return;
                }

                this.sprite = null;
                if (string.IsNullOrEmpty(path))
                {
                    Log.Error($"Not Found Path key target: {m_SpritePath} | decode {path}");
                    return;
                }

                m_SpritePath = value;
                UIAgent.LoadAssetAsync(path, SpriteLoadCb);
            }
        }

        public GameObject dragEventPassTarget { get => this.gameObject; set => throw new System.NotImplementedException(); }

        private void SpriteLoadCb(IAssetVO assetVo)
        {
            var obj = assetVo.GetAsset();
            if (obj is Sprite sprite)
            {
                this.sprite = sprite;
            }
            else if (obj is Texture2D texture2D)
            {
                this.sprite = UIUtil.Texture2DToSprite(texture2D);
            }
            else if (obj is SpriteAtlas spriteAtlas)
            {
                this.sprite = spriteAtlas.GetSprite(m_AtlasSpriteName);
            }
        }
    }
}
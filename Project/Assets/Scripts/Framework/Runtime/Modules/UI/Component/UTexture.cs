using Framework.Runtime.LogSystem;
using Framework.Runtime.MAsset;
using Framework.Runtime.Module;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Runtime.UI
{
    public class UTexture : RawImage, IColor, IDragEventPass
    {
        private bool m_Gray = false;

        private string m_TexPath;

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

        public float Height
        {
            get { return rectTransform.rect.height; }
        }

        public GameObject dragEventPassTarget { get => this.gameObject; set { } }

        public string TexPath
        {
            get { return m_TexPath; }
            set
            {
                string path = value;
                // string path = AssetMapManager.Instance.GetEnvironmentAssetPath(m_SpritePathKey);
                // string key = value;
                if (m_TexPath == value)
                {
                    return;
                }

                this.texture = null;
                if (string.IsNullOrEmpty(path))
                {
                    Log.Error($"Not Found Tex Path key target: {m_TexPath} neither decode {path}");
                    return;
                }

                m_TexPath = value;
                UIAgent.LoadAssetAsync(path, OnTexLoadComplete);
            }
        }

        public float Width
        {
            get { return rectTransform.rect.width; }
        }

        private void OnDrawGizmos()
        {
        }

        private void OnTexLoadComplete(IAssetVO assetVo)
        {
            if (assetVo.GetAsset() is Texture2D texture2D)
            {
                this.texture = texture2D;
            }
        }
    }
}
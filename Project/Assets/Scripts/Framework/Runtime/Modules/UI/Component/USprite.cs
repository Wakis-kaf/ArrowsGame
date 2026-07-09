using Framework.Runtime.MAsset;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Framework.Runtime.UI
{
#if UNITY_EDITOR

    [CanEditMultipleObjects]
#endif
    [Serializable]
    public class USprite : Image, IColor, IDragEventPass
    {
        private IAssetVO m_AtlasVO;

        private static char[] SPLIT = new char[1] { '.' };

        private string m_SpritePath;

        private float m_GroupAlpha = 1f;

        private bool m_Gray;
        

        [FormerlySerializedAs("_atlas")]
        [SerializeField]
        private UAtlas m_Atlas;

        [FormerlySerializedAs("_spriteName")]
        [SerializeField]
        private string m_SpriteName;

        [SerializeField]
        private bool m_IsAutoSnap = false;

        public bool IsAutoSnap { get => m_IsAutoSnap; set => m_IsAutoSnap = value; }

        public bool isShowWhiteSource = false;

        public bool raycastConsiderAlpha = false;

        private Action<Vector2> _onNativeSize;
        public GameObject dragEventPassTarget { get => this.gameObject; set { } }
        public bool enableDragEventPass { get; set; } = true;
        public void ClearSprite()
        {
            m_Atlas = null;
            m_SpriteName = string.Empty;
            m_SpritePath = string.Empty;
            m_AtlasVO = null;
            sprite = null;
        }
        public float Width
        {
            get
            {
                return Size.x;
            }
            set
            {
                Size = new Vector2(value, Size.y);
            }
        }

        public float Height
        {
            get
            {
                return Size.y;
            }
            set
            {
                Size = new Vector2(Size.x, value);
            }
        }

        public Vector2 Size
        {
            get
            {
                return base.rectTransform.rect.size;
            }
            set
            {
                if (value != base.rectTransform.rect.size)
                {
                    base.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value.x);
                    base.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value.y);
                }
            }
        }

        public float Alpha
        {
            get
            {
                return color.a;
            }
            set
            {
                if (Math.Abs(this.color.a - value) > 0f)
                {
                    Color color = this.color;
                    color.a = value;
                    this.color = color;
                }
            }
        }
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

        public UAtlas Atlas
        {
            get
            {
                return m_Atlas;
            }
            set
            {
                if (m_Atlas != value)
                {
                    m_Atlas = value;
                    SpriteName = null;
                }
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
                    base.sprite = null;
                    m_SpritePath = string.Empty;
                }
                else if (Atlas != null)
                {
                    base.sprite = Atlas.GetSprite(m_SpriteName);
                    if (Application.isPlaying && IsAutoSnap)
                    {
                        SetNativeSize();
                    }
                }
            }
        }

        public string Path
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
            set
            {
                if (m_SpritePath == value)
                {
                    return;
                }

                m_SpritePath = value;
                if (string.IsNullOrEmpty(m_SpritePath))
                {
                    SpriteName = null;
                    return;
                }
                int lastDotIndex = m_SpritePath.LastIndexOf(".");
                if (lastDotIndex == -1)
                {
                    lastDotIndex = m_SpritePath.LastIndexOf("/");
                }
                string atlasPath = m_SpritePath.Substring(0, lastDotIndex);
                atlasPath = GetFullAtlasPath(atlasPath);
                string spriteName = m_SpritePath.Substring(lastDotIndex + 1);
                SpriteName = spriteName;
                if (Atlas == null || Atlas.name != spriteName)
                {
                    LoadAtlas(AssetPathEncoder.EncodeEnvAssetLink(atlasPath,AssetType.PrefabAsset));
                }
            }
        }

        private string GetFullAtlasPath(string atlasPath)
        {
            return $"Assets/AddressableResources/UIAtlas/{atlasPath}/{atlasPath}";
        }

        public void AddNativeSize(Action<UnityEngine.Vector2> callback)
        {
            _onNativeSize = callback;
        }

        public void RemoveNativeSize()
        {
            _onNativeSize = null;
        }

        private void LoadAtlas(string atName)
        {
            UIAgent.LoadAssetAsync(atName, LoadComplete);
        }

        private void LoadComplete(IAssetVO vo)
        {
            m_AtlasVO = vo;
            GameObject gameObject = vo.GetAsset<GameObject>();
            if (gameObject != null)
            {
                m_Atlas = gameObject.GetComponent<UAtlas>();
                RefreshSprite();
            }
        }

        public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (raycastConsiderAlpha && (color.a <= 0f || m_GroupAlpha <= 0f))
            {
                return false;
            }

            return base.IsRaycastLocationValid(screenPoint, eventCamera);
        }

        protected override void OnCanvasGroupChanged()
        {
            base.OnCanvasGroupChanged();
            if (raycastConsiderAlpha)
            {
                m_GroupAlpha = UIUtil.GetGroupAlpha(base.gameObject);
            }
        }

        protected override void Start()
        {
            base.Start();
            CheckAtlasSprite();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            base.sprite = null;
            m_Atlas = null;
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if (_onNativeSize != null)
            {
                _onNativeSize(Size);
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (base.overrideSprite != null || isShowWhiteSource)
            {
                base.OnPopulateMesh(vh);
            }
            else
            {
                vh.Clear();
            }
        }

        public override Material GetModifiedMaterial(Material baseMaterial)
        {
            Material modifiedMaterial = base.GetModifiedMaterial(baseMaterial);
            if (modifiedMaterial.HasProperty("_GraySwitch"))
            {
                modifiedMaterial.SetFloat("_GraySwitch", m_Gray ? 1f : 0f);
            }

            return modifiedMaterial;
        }

        public void CheckAtlasSprite()
        {
            string text = ((base.sprite != null) ? base.sprite.name : null);
            if (text!=null && text != m_SpriteName)
            {
                RefreshSprite();
#if UNITY_EDITOR
                if (Application.isEditor)
                {
                    UnityEditor.EditorUtility.SetDirty(base.gameObject);
                }
#endif
            }
        }

        public void RefreshSprite()
        {
            string spriteName = m_SpriteName;
            m_SpriteName = null;
            SpriteName = spriteName;
           
        }

      
    }
}
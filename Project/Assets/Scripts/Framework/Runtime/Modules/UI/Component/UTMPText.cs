using Framework.Runtime.UI.UIAnimae;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Runtime.UI
{
    public class UTMPText : TextMeshProUGUI, IPointerEnterHandler, IPointerExitHandler, IDragEventPass
    {
        public UIAnimatorCaller mouseEnterAnimCaller = new UIAnimatorCaller() { targetSequence = "PointerEnter" };
        public UIAnimatorCaller mouseExitAnimCaller = new UIAnimatorCaller() { targetSequence = "PointerExit" };

        private bool m_isPointerStaying;
        private RectTransform m_RectTransform;
        private Vector2 m_Size;

        public event Action<PointerEventData> OnPointerEnterDelegate;
        public event Action<PointerEventData> OnPointerExitDelegate;

        public Vector2 AnchorMax
        {
            set { if (value != RectTransform.anchorMax) RectTransform.anchorMax = value; }
        }
        public Vector2 AnchorMin
        {
            set { if (value != RectTransform.anchorMin) RectTransform.anchorMin = value; }
        }

        public bool enableDragEventPass { get; set; } = true;
        public bool isPoninterStaying
        {
            get => m_isPointerStaying;
            private set { if (m_isPointerStaying != value) m_isPointerStaying = value; }
        }

        public RectTransform RectTransform
        {
            get
            {
                if (m_RectTransform == null) m_RectTransform = GetComponent<RectTransform>();
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
                    RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value.x);
                    RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value.y);
                    m_Size = value;
                }
            }
        }

        public GameObject dragEventPassTarget { get => this.gameObject; set => throw new System.NotImplementedException(); }

        public void SetInstanceColor(string txtHexColor, string oulineHexColor, float thickness, float dilate)
        {
            SetInstanceColor(UIUtil.Hex2Color(txtHexColor), UIUtil.Hex2Color(oulineHexColor), thickness, dilate);
        }

        /// <summary>
        /// 修复白块问题的版本
        /// </summary>
        public void SetInstanceColor(Color txtColor, Color outlineColor, float thickness, float dilate)
        {
            // 1. 设置颜色
            this.color = txtColor;

            // 2. 使用 TMP 官方推荐的方式获取实例材质 (这会自动处理纹理和 Keywords，防止白块)
            // 访问 fontMaterial 会自动触发 TMP 内部的 Material Instance 逻辑
            Material mat = this.fontMaterial;

            if (mat != null)
            {
                // 3. 开启描边关键字（防止有些材质默认没开描边导致不显示）
                if (thickness > 0) mat.EnableKeyword(ShaderUtilities.Keyword_Outline);

                // 4. 设置属性
                mat.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
                mat.SetFloat(ShaderUtilities.ID_OutlineWidth, thickness);
                mat.SetFloat(ShaderUtilities.ID_FaceDilate, dilate);
            }

            // 5. 【核心修复】不使用 new Material，而是强制刷新
            // 在 Time.scale = 0 时，手动通知 CanvasRenderer 更新材质引用
            this.SetMaterialDirty();
            this.UpdateMeshPadding();

            // 6. 强制同步渲染状态
            if (Time.timeScale == 0)
            {
                // 只有在物体激活时才 Toggle，否则会触发无效逻辑
                if (this.gameObject.activeInHierarchy)
                {
                    // 强制重建网格数据
                    this.ForceMeshUpdate(true, true);

                    // 暴力刷新：解决某些版本 TMP 在暂停时不更新 Material 的 BUG
                    // 这种方式不需要 new，所以不会内存泄漏（由 TMP 内部管理实例销毁）
                    this.enabled = false;
                    this.enabled = true;
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEnterDelegate?.Invoke(eventData);
            isPoninterStaying = true;
            mouseEnterAnimCaller?.Call();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnPointerExitDelegate?.Invoke(eventData);
            isPoninterStaying = false;
            mouseExitAnimCaller?.Call();
        }

        public void SetRichText(string content, float size, string colorHex)
        {
            this.text = UIUtil.GetRichTextColor(UIUtil.GetRichTextSize(content, size), colorHex);
        }
    }
}
using Framework.Runtime.UI.UIAnimae;

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework.Runtime.UI
{
    public class UText : Text, IPointerEnterHandler, IPointerExitHandler, IColor, IDragEventPass
    {
        public UIAnimatorCaller mouseEnterAnimCaller = new UIAnimatorCaller() { targetSequence = "PointerEnter" };

        public UIAnimatorCaller mouseExitAnimCaller = new UIAnimatorCaller() { targetSequence = "PointerExit" };

        private bool m_Gray = false;

        private bool m_isPointerStaying;

        private RectTransform m_RectTransform;

        private Vector2 m_Size;

        public event Action<PointerEventData> OnPointerEnterDelegate;

        public event Action<PointerEventData> OnPointerExitDelegate;

        public Vector2 AnchorMax
        {
            set
            {
                if (value != RectTransform.anchorMax)
                {
                    RectTransform.anchorMax = value;
                }
            }
        }

        public Vector2 AnchorMin
        {
            set
            {
                if (value != RectTransform.anchorMin)
                {
                    RectTransform.anchorMin = value;
                }
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

        public bool isPoninterStaying
        {
            get => m_isPointerStaying;
            private set
            {
                if (m_isPointerStaying != value)
                {
                    m_isPointerStaying = value;
                }
            }
        }

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
                    RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value.x);
                    RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value.y);
                }
            }
        }

        public GameObject dragEventPassTarget { get => this.gameObject; set => dragEventPassTarget = value; }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEnterDelegate?.Invoke(eventData);
            isPoninterStaying = true;
            mouseEnterAnimCaller?.Call();
            //m_Animator?.Play(m_PointerEnterAnimationName);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnPointerExitDelegate?.Invoke(eventData);
            isPoninterStaying = false;
            mouseExitAnimCaller?.Call();
        }

        public void SetRichText(string text, float size, string color)
        {
            text = UIUtil.GetRichTextColor(UIUtil.GetRichTextSize(text, size), color);
        }
    }
}
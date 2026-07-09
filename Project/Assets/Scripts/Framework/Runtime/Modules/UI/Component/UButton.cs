using Framework.Runtime.UI.UIAnimae;
using Framework.Utils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework.Runtime.UI
{
    /// <summary>
    /// 按钮
    /// </summary>
    public class UButton : Button, IDragHandler
    {
        public UIAnimatorCaller clickAnimCaller = new UIAnimatorCaller() { targetSequence = "Click" };
        public UIAnimatorCaller clickProtectAnimCaller = new UIAnimatorCaller() { targetSequence = "ClickProtect" };

        public bool enableTMPTxt = false;

        /// <summary>
        /// 是否绑定快捷键
        /// </summary>
        public bool isEnableBindShortcuts = true;

        /// <summary>
        /// 节流,在一定时间间隔内只执行第一次回调
        /// </summary>
        public bool isEnableProtect = false;

        /// <summary>
        /// 长按阈值
        /// </summary>
        public float loneTimePressThreshold = 0.2f;

        public UIAnimatorCaller mouseDownAnimCaller = new UIAnimatorCaller() { targetSequence = "MouseDown" };

        public UIAnimatorCaller mouseEnterAnimCaller = new UIAnimatorCaller() { targetSequence = "MouseEnter" };

        public UIAnimatorCaller mouseExitAnimCaller = new UIAnimatorCaller() { targetSequence = "MouseExit" };

        public UIAnimatorCaller mouseUpAnimCaller = new UIAnimatorCaller() { targetSequence = "MouseUp" };

        /// <summary>
        /// 节流间隔,单位s
        /// </summary>
        public float protectTime = 0.2f;

        private bool m_HasBindShortcuts;

        private float m_LastPointerDownTime;

        private Action m_LastShortcutsProxy;

        private float m_LastTrottleTime;

        private RectTransform m_RectTransform;

        private Vector2 m_Size;
        [SerializeField]

        private UText m_UText;
        [SerializeField]

        private UTMPText m_UTMPText;
        [SerializeField]
        private GameObject m_RedPoint;
        private bool m_ShowRed = false;

        private event Action OnBtnClick;

        private event Action OnDragDelegate;

        private event Action OnInteractDisable;

        private event Action OnInteractEnable;

        private event Action OnLongTimePressed;

        private event Action<PointerEventData> OnPointerDownDelegate;

        private event Action<PointerEventData> OnPointerEnterDelegate;

        private event Action<PointerEventData> OnPointerExitDelegate;

        private event Action<PointerEventData> OnPointerUpDelegate;

        private event Action OnProtectClick;
        [NonSerialized, HideInInspector]
        public int clickAudioType = UIAudioType.None;
        [NonSerialized, HideInInspector]
        public int downAudioType = UIAudioType.NormalButtonClick;

        /// <summary>
        /// 是否开启交互
        /// </summary>
        public bool Interactable
        {
            get { return interactable; }
            set
            {
                if (interactable != value)
                {
                    if (value) OnInteractEnable?.Invoke();
                    else OnInteractDisable?.Invoke();
                }
            }
        }

        public bool isPointerLongPressing { get; private set; }

        public bool isPointerPressing { get; private set; }

        public bool isPointerStaying { get; private set; }

        public RectTransform RectTransform
        {
            get
            {
                if (m_RectTransform == null)
                {
                    m_RectTransform = gameObject.GetComponent<RectTransform>();
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

        public string Text
        {
            get { return enableTMPTxt ? UTMPText.text : UText.text; }
            set
            {
                if (enableTMPTxt)
                {
                    if (value != UTMPText.text)
                    {
                        UTMPText.text = value;
                    }
                }
                else
                {
                    if (value != UText.text)
                    {
                        UText.text = value;
                    }
                }
            }
        }

        private UText UText
        {
            get
            {
                if (m_UText == null)
                {
                    m_UText = transform.GetComponentInFirstChild<UText>();
                    if (m_UText == null)
                    {
                        GameObject obj = new GameObject("UText");
                        m_UText = obj.AddComponent<UText>();
                        obj.transform.SetParent(transform, false);
                        obj.layer = 5;
                    }

                    // 初始化
                    m_UText.Size = Size;
                    m_UText.color = Color.black;
                    m_UText.alignment = TextAnchor.MiddleCenter;
                    m_UText.AnchorMin = Vector2.zero;
                    m_UText.AnchorMax = Vector2.one;
                    m_UText.RectTransform.offsetMin = Vector2.zero;
                    m_UText.rectTransform.offsetMax = Vector2.zero;
                }

                return m_UText;
            }
        }

        private UTMPText UTMPText
        {
            get
            {
                if (m_UTMPText == null)
                {
                    m_UTMPText = transform.GetComponentInFirstChild<UTMPText>();
                    if (m_UTMPText == null && enableTMPTxt)
                    {
                        GameObject obj = new GameObject("UTMPText");
                        m_UTMPText = obj.AddComponent<UTMPText>();
                        obj.transform.SetParent(transform, false);
                    }
                    if (m_UTMPText == null) return null;
                    // 初始化
                    m_UTMPText.Size = Size;
                    m_UTMPText.color = Color.black;
                    m_UTMPText.horizontalAlignment = HorizontalAlignmentOptions.Center;
                    m_UTMPText.verticalAlignment = VerticalAlignmentOptions.Middle;
                    m_UTMPText.AnchorMin = Vector2.zero;
                    m_UTMPText.AnchorMax = Vector2.one;
                    m_UTMPText.RectTransform.offsetMin = Vector2.zero;
                    m_UTMPText.RectTransform.offsetMax = Vector2.zero;
                }

                return m_UTMPText;
            }
        }

        public void AddClick(Action listener)
        {
            OnBtnClick -= listener;
            OnBtnClick += listener;
        }
        public void SetRedVisible(bool visible)
        {
            m_ShowRed = visible;
            if (m_RedPoint != null)
            {
                GameObjectUtil.SetActive(m_RedPoint, visible);
            }
        }
        public void AddDrag(Action listener)
        {
            OnDragDelegate += listener;
        }

        public void AddInteractDisable(Action listener)
        {
            OnInteractDisable += listener;
        }

        public void AddInteractEnable(Action listener)
        {
            OnInteractEnable += listener;
        }

        public void AddLongTimePressed(Action listener)
        {
            OnLongTimePressed += listener;
        }

        public void AddPointerDown(Action<PointerEventData> listener)
        {
            OnPointerDownDelegate += listener;
        }

        public void AddPointerEnter(Action<PointerEventData> listener)
        {
            OnPointerEnterDelegate += listener;
        }

        public void AddPointerExit(Action<PointerEventData> listener)
        {
            OnPointerExitDelegate += listener;
        }

        public void AddPointerUp(Action<PointerEventData> listener)
        {
            OnPointerUpDelegate += listener;
        }

        public void AddProtectClick(Action listener)
        {
            OnProtectClick += listener;
        }

        public void BindShortCuts(Action actionDelegate)
        {
            if (!isEnableBindShortcuts || m_HasBindShortcuts) return;
            m_LastShortcutsProxy = actionDelegate;
            actionDelegate += ClickByShortCuts;
            m_HasBindShortcuts = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsBtnActive()) return;
            OnDragDelegate?.Invoke();
        }
        
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (IsInProtectTime(eventData))
            {
                OnProtectClick?.Invoke();
                clickProtectAnimCaller?.Call();
            }
            else if (IsBtnActive())
            {
                base.OnPointerClick(eventData);
                m_LastTrottleTime = Time.time;
                OnBtnClick?.Invoke();
                clickAnimCaller?.Call();
            }
            else
            {
                m_LastTrottleTime = 0;
            }
           
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (IsInProtectTime(eventData)) return;
            if (!IsBtnActive()) return;
            base.OnPointerDown(eventData);
            OnPointerDownDelegate?.Invoke(eventData);
            isPointerPressing = true;
            m_LastPointerDownTime = Time.time;
            mouseDownAnimCaller?.Call();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsBtnActive()) return;
            base.OnPointerEnter(eventData);
            OnPointerEnterDelegate?.Invoke(eventData);
            isPointerStaying = true;
            mouseEnterAnimCaller?.Call();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (!IsBtnActive()) return;
            base.OnPointerExit(eventData);
            OnPointerExitDelegate?.Invoke(eventData);
            isPointerStaying = false;
            mouseExitAnimCaller?.Call();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!IsBtnActive()) return;
            base.OnPointerUp(eventData);
            OnPointerUpDelegate?.Invoke(eventData);
            isPointerPressing = false;
            isPointerLongPressing = false;
            mouseUpAnimCaller?.Call();
        }

        public void RemoveClick(Action listener)
        {
            OnBtnClick -= listener;
        }

        public void RemoveDrag(Action listener)
        {
            OnDragDelegate -= listener;
        }

        public void RemoveInteractDisable(Action listener)
        {
            OnInteractDisable -= listener;
        }

        public void RemoveInteractEnable(Action listener)
        {
            OnInteractEnable -= listener;
        }

        public void RemoveLongTimePressed(Action listener)
        {
            OnLongTimePressed -= listener;
        }

        public void RemovePointerDown(Action<PointerEventData> listener)
        {
            OnPointerDownDelegate -= listener;
        }

        public void RemovePointerEnter(Action<PointerEventData> listener)
        {
            OnPointerEnterDelegate -= listener;
        }

        public void RemovePointerExit(Action<PointerEventData> listener)
        {
            OnPointerExitDelegate -= listener;
        }

        public void RemovePointerUp(Action<PointerEventData> listener)
        {
            OnPointerUpDelegate -= listener;
        }

        public void RemoveProtectClick(Action listener)
        {
            OnProtectClick -= listener;
        }

        public void RemoveShortCuts()
        {
            if (m_LastShortcutsProxy != null)
                m_LastShortcutsProxy -= ClickByShortCuts;
            m_HasBindShortcuts = false;
        }

        protected override void Awake()
        {
            base.Awake();
            AddClick(PlayClickSound);
            AddPointerDown(PlayDownSound);
            if (m_UTMPText == null && enableTMPTxt)
            {
                m_UTMPText = GetComponentInChildren<UTMPText>();
            }
            else if (m_UText == null && !enableTMPTxt)
            {
                m_UText = GetComponentInChildren<UText>();
            }
            SetRedVisible(m_ShowRed);
            if (enableTMPTxt) GameObjectUtil.SetActive(UText, false);
            else GameObjectUtil.SetActive(UTMPText, false); 
        }

  

        private void PlayClickSound()
        {
            UIAgent.PlayAudioEffect(this, this.clickAudioType);
        }
        private void PlayDownSound(PointerEventData data)
        {
            UIAgent.PlayAudioEffect(this, this.downAudioType);
        }

    

        private void ClickByShortCuts()
        {
            if (!IsBtnActive() || !isEnableBindShortcuts) return;
            onClick?.Invoke();
        }

        private bool IsBtnActive()
        {
            if (!IsActive() || !IsInteractable())
                return false;
            return true;
        }

        private bool IsInProtectTime(PointerEventData eventData)
        {
            if (!isEnableProtect) return false;
            bool isThrottle = (Time.time - m_LastTrottleTime) < protectTime;
            return isThrottle && m_LastTrottleTime > 0f && IsBtnActive();
        }

        private void Update()
        {
            if (!IsBtnActive()) return;
            if (isPointerPressing && Time.time - m_LastPointerDownTime >= loneTimePressThreshold)
            {
                isPointerLongPressing = true;
                if (m_LastPointerDownTime >= 0)
                    OnLongTimePressed?.Invoke();
                m_LastPointerDownTime = -1f;
            }
        }
    }
}
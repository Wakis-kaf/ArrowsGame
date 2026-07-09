using Framework.Runtime.UI.UIAnimae;
using Framework.Utils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework.Runtime.UI
{
    public class UCheckBox : Toggle, IAutoLayoutItem
    {
        public UIAnimatorCaller checkAnimCaller = new UIAnimatorCaller() { targetSequence = "Checked" };
        public UIAnimatorCaller disCheckAnimCaller = new UIAnimatorCaller() { targetSequence = "DisChecked" };

        [SerializeField]
        private int m_Index = 0;
        [SerializeField]
        private bool m_IsSelect = false;
        private Action<bool> m_OnSelectPreChanged;
        private Action m_OnSelectChanged;
        private Action m_OnClick;
        private Action m_OnMouseDown;
        private RectTransform m_RectTransform;
        public bool IsSelected
        {
            get => m_IsSelect;
            set
            {
                if (value)
                {
                    DoSelect();
                }
                else
                {
                    DoDeSelect();
                }
            }
        }
        [SerializeField]
        private GameObject m_RedPoint;
        private bool m_ShowRed = false;
        public bool IsTmpText = false;
        
        [SerializeField]
        private Text m_Text;
        [SerializeField]
        private TextMeshProUGUI m_TmpTxt;

        // 当取消选择的时候触发
        private Action<bool> m_ValueChanged;
        [NonSerialized,HideInInspector]
        public int downAudioType = UIAudioType.NormalButtonClick;

        public int Index
        {
            get => m_Index;
            set => m_Index = value;
        }
        public void SetRedVisible(bool visible)
        {
            m_ShowRed = visible;
            if (m_RedPoint != null)
            {
                GameObjectUtil.SetActive(m_RedPoint, visible);
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

        public string Text
        {
            get
            {
                if (IsTmpText)
                {
                    return GetTmpText()?.text;
                }
                else
                {
                    return  GetText()?.text;
                }
            }
            set
            {
                if (IsTmpText)
                {
                    GetTmpText().text = value;
                }
                else
                {
                    GetText().text = value;
                }
            }
        }
        public void AddPreSelect(Action<bool> cb)
        {
            m_OnSelectPreChanged += cb;
            m_OnSelectPreChanged?.Invoke(isOn);
        }
        public void RemovePreSelect(Action<bool> cb)
        {
            m_OnSelectPreChanged -= cb;
            
        }
        public void AddValueChanged(Action<bool> callback)
        {
            m_ValueChanged -= callback;
            m_ValueChanged += callback;
            m_ValueChanged?.Invoke(isOn);
        }
        public void RemoveValueChanged(Action<bool> callback)
        {
            m_ValueChanged -= callback;
        }

        public void SetLabel(string tabContent)
        {
            Text = tabContent;
        }

        protected override void Awake()
        {
            base.Awake();
            AddMouseDown(PlayDownSound);
            onValueChanged.AddListener(IsOnChanged);
            isOn = m_IsSelect;
            
            SetRedVisible(m_ShowRed);
        }
        private void IsOnChanged(bool isOn)
        {
            if (isOn != m_IsSelect)
            {
                if (isOn)
                {
                    DoSelect(true);
                }
                else
                {
                    DoDeSelect(true);
                }
            }
        }
        private void PlayDownSound()
        {
            UIAgent.PlayAudioEffect(this, this.downAudioType);
        }
        public void AddClick(Action clickListener)
        {
            m_OnClick-=clickListener;
            m_OnClick += clickListener;
        }
        public void RemoveClick(Action clickListener)
        {
            m_OnClick -= clickListener;
        }
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            m_OnClick?.Invoke();
            InternalToggle();
        }
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            m_OnMouseDown?.Invoke();
        }
        public void AddMouseDown(Action cb)
        {
            m_OnMouseDown -= cb;
            m_OnMouseDown += cb;
        }
        public void RemoveMouseDown(Action cb)
        {
            m_OnMouseDown -= cb;
        }
        public override void OnSubmit(BaseEventData eventData)
        {
            InternalToggle();
        }
        private void InternalToggle()
        {
            if (!IsActive() || !IsInteractable())
                return;

            if (m_IsSelect)
            {
                m_IsSelect = false;
                m_OnSelectPreChanged?.Invoke(m_IsSelect);
            }
            else
            {
                m_IsSelect = true;
                m_OnSelectPreChanged?.Invoke(m_IsSelect);
            }

            isOn = m_IsSelect;

            m_ValueChanged?.Invoke(m_IsSelect);
        }
        private Text GetText()
        {
            if (m_Text == null)
                m_Text = gameObject.GetComponentInChildren<Text>();
            return m_Text;
        }
        private TextMeshProUGUI GetTmpText()
        {
            if (m_TmpTxt == null)
                m_TmpTxt = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            return m_TmpTxt;
        }
     
        
        public void DoDeSelect(bool force = false)
        {
            if (force)
            {
                m_IsSelect = false;
            }
            if (!m_IsSelect)
            {
                StopSelect();
                PlayDeSelect();
                isOn = m_IsSelect;
                m_ValueChanged?.Invoke(m_IsSelect);
            }
        }
        public void DoSelect(bool force = false)
        {
            if (force)
            {
                m_IsSelect = true;
            }
            if ( m_IsSelect)
            {
                StopDeSelect();
                PlaySelect();
                isOn = m_IsSelect;
                m_ValueChanged?.Invoke(m_IsSelect);
            }

        }
    
        public void PlayDeSelect()
        {
            disCheckAnimCaller?.Call();
        }

        public void PlaySelect()
        {
            checkAnimCaller?.Call();
        }

        public void StopDeSelect()
        {
            disCheckAnimCaller?.Complete();
        }
        public void StopSelect()
        {
            checkAnimCaller.Complete();
        }
    }
}
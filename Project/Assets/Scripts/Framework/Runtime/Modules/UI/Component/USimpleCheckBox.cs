using Framework.Runtime.UI.UIAnimae;
using Framework.Utils;

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Runtime.UI
{
    public class USimpleCheckBox : Toggle
    {
        // [SerializeField] private string m_CheckAnimationName = "Checked"; [SerializeField]
        // private string m_DisCheckAnimationName = "DisChecked"; [SerializeField] private
        // UIAnimator2 m_Animator;
        public UIAnimatorCaller checkAnimCaller = new UIAnimatorCaller() { targetSequence = "Checked" };

        public UIAnimatorCaller disCheckAnimCaller = new UIAnimatorCaller() { targetSequence = "DisChecked" };
        [SerializeField] private bool m_IsEnableTMPText = false;
        private Action m_OnDesSelect;
        private Action m_OnSelect; // 当选择的时候触发
        private RectTransform m_RectTransform;

        private string m_Text;

        [SerializeField] private UText m_UText;

        [SerializeField] private UTMPText m_UTMPText;

        // 当取消选择的时候触发
        private Action<bool> m_ValueChanged;

        public RectTransform rectTransform
        {
            get
            {
                if (m_RectTransform == null)
                    m_RectTransform = GameObjectUtil.GetOrAddComponent<RectTransform>(gameObject);
                return m_RectTransform;
            }
        }

        public string text
        {
            get
            {
                if (uText != null && !m_IsEnableTMPText)
                {
                    return uText.text;
                }
                else if (uTMPText != null && m_IsEnableTMPText)
                {
                    return uTMPText.text;
                }

                return string.Empty;
            }
            set
            {
                if (uText != null && !m_IsEnableTMPText)
                {
                    uText.text = value;
                }
                else if (uTMPText != null && m_IsEnableTMPText)
                {
                    uTMPText.text = value;
                }
            }
        }

        private UText uText
        {
            get
            {
                if (m_UText == null)
                {
                    m_UText = GetComponentInChildren<UText>();
                }

                return m_UText;
            }
        }

        private UTMPText uTMPText
        {
            get
            {
                if (m_UTMPText == null)
                {
                    m_UTMPText = GetComponentInChildren<UTMPText>();
                }

                return m_UTMPText;
            }
        }

        public void AddDeSelect(Action deSelectCallBack)
        {
            m_OnDesSelect += deSelectCallBack;
            if (!isOn) deSelectCallBack?.Invoke();
        }

        public void AddSelect(Action selectCallBack)
        {
            m_OnSelect += selectCallBack;
            if (isOn) selectCallBack?.Invoke();
        }

        public void AddValueChanged(Action<bool> callback)
        {
            m_ValueChanged += callback;
            callback?.Invoke(isOn);
        }

        public void RemoveDeSelect(Action deSelectCallBack)
        {
            m_OnDesSelect -= deSelectCallBack;
        }

        public void RemoveSelect(Action selectCallBack)
        {
            m_OnSelect -= selectCallBack;
        }

        public void RemoveValueChanged(Action<bool> callback)
        {
            m_ValueChanged -= callback;
        }

        protected override void Awake()
        {
            base.Awake();
            // if (graphic) graphic.raycastTarget = false;
            onValueChanged.AddListener(OnToggleChanged);
            // 一开始先触发一下
            OnToggleChanged(isOn);
        }

        private void OnTargetGraphicClick()
        {
            Debug.Log(isOn);
            isOn = !isOn;
            OnToggleChanged(isOn);
        }

        private void OnToggleChanged(bool isOn)
        {
            m_ValueChanged?.Invoke(isOn);
            if (isOn)
            {
                m_OnSelect?.Invoke();
                //m_Animator?.Play(m_CheckAnimationName);
                checkAnimCaller?.Call();
            }
            else
            {
                m_OnDesSelect?.Invoke();
                //m_Animator?.Play(m_DisCheckAnimationName);
                disCheckAnimCaller?.Call();
            }
        }
    }
}
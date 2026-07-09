using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Framework.Runtime.UI
{
    public class UValueProgress : MonoBehaviour
    {
        [SerializeField]
        private UProgressBar m_ProgressBar;
        [SerializeField]
        private UTMPText m_LabelText;
        [SerializeField]
        private UInputField m_ValueIF;
        [SerializeField]
        private UButton m_ValueChangeBtn;
        [SerializeField]
        private float m_StartValue = 0;
        [SerializeField]
        private string m_Label = "";
        private void Awake()
        {
            m_ValueChangeBtn.AddClick(OnValueChangeBtnClick);
            m_ProgressBar.AddValueChanged(OnProgressBarValueChanged);
            m_ProgressBar.value = m_StartValue;
        }
        public void SetLabel(string label)
        {
            m_Label = label;
            m_LabelText.text = m_Label;

        }

        private void OnProgressBarValueChanged(float obj)
        {
            float value = m_ProgressBar.value;
            m_ValueIF.text = value.ToString();
        }
        private void OnValueChangeBtnClick()
        {
            string valueStr = m_ValueIF.text;
            if (float.TryParse(valueStr, out float value))
            {
                m_ProgressBar.value = value;
            }

        }

        public void SetValue(float value)
        {
            m_ProgressBar.value = value;
        }

        public float GetValue()
        {
            return m_ProgressBar.value;
        }
    }
}

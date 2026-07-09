using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.UI
{
    public class UCkbTabNavigation : MonoBehaviour
    {
        public Action OnHide;
        public Action OnShow;
        [SerializeField] private RectTransform m_Content;
        [SerializeField] private UCheckBoxGroup m_TabGroup;
        private Dictionary<int, IDisplayUnit> m_TabIndex2ViewRTDict = new Dictionary<int, IDisplayUnit>();
        
        public int SelectedIndex => m_TabGroup.SelectedIndex;
        public UCheckBoxGroup TagGroup => m_TabGroup;
        
        public void AddDeSelect(Action<int> listener)
        {
            m_TabGroup.AddDeSelect(listener);
        }
        private Func<int, int,int> m_SelectCheck;
        private int m_BeforeSelectIndex;
        private int m_LastSelectIndex;
        public void SetSelectCheck(Func<int,int,int> selectCheck)
        {
            m_SelectCheck = selectCheck;
            m_LastSelectIndex = SelectedIndex;
            OnTabSelectChanged(SelectedIndex);
            m_TabGroup.RemoveSelect(OnTabSelectChanged);
            m_TabGroup.AddSelect(OnTabSelectChanged);
        }
        private void OnTabSelectChanged(int selectIndex)
        {   
            if (m_SelectCheck != null)
            {
                int newSelectIndex = m_SelectCheck.Invoke(selectIndex, m_LastSelectIndex);
                if (newSelectIndex != selectIndex)
                {
                    SwitchTab(newSelectIndex);
                }
                m_LastSelectIndex = newSelectIndex;
            }
            else
            {
                m_LastSelectIndex = selectIndex;
            }
        }


        public void AddSelect(Action<int> listener)
        {
            m_TabGroup.AddSelect(listener);
        }

        public void RemoveDeSelect(Action<int> listener)
        {
            m_TabGroup.RemoveDeSelect(listener);
        }

        public void RemoveSelect(Action<int> listener)
        {
            m_TabGroup.RemoveSelect(listener);
        }

        public void SetTabLabel(int tabIndex, string tabContent)
        {
            m_TabGroup.SetTabLabel(tabIndex, tabContent);
        }

        public void SetTabNavView(int tabIndex, IDisplayUnit displayUnit, string tabContent)
        {
            DisplayUnit view = displayUnit as DisplayUnit;
            if (m_TabIndex2ViewRTDict.ContainsKey(tabIndex))
            {
                m_TabIndex2ViewRTDict[tabIndex] = view;
            }
            else
            {
                m_TabIndex2ViewRTDict.Add(tabIndex, view);
            }
            if (!view.IsModelLoaded) return;
            view.DisplayGO.transform.SetParent(m_Content, false);
            view.SetAnchoredPosition(Vector2.zero);
            RectTransform rectTransform = view.RectTransform;
            rectTransform.SetOffsetZero();
            UCheckBox tabBar = m_TabGroup.GetTabBar(tabIndex);
            // 设置tabbar 内容
            tabBar.AddValueChanged((isShow) =>
            {
                if (isShow && !view.IsShow)
                {
                    view.Show();
                }
                else if(!isShow && view.IsShow)
                {
                    view.Hide();
                }
            });
            if (!string.IsNullOrEmpty(tabContent))
                m_TabGroup.SetTabLabel(tabIndex, tabContent);
        }
        

        public void ShowTabView(int tabIndex)
        {
            m_TabGroup.SwitchTab(tabIndex);
        }

        public void SwitchTab(int tabIndex)
        {
            m_TabGroup.SwitchTab(tabIndex);
        }

        private void OnDisable()
        {
            OnHide?.Invoke();
        }

        private void OnEnable()
        {
            OnShow?.Invoke();
        }
    }
}
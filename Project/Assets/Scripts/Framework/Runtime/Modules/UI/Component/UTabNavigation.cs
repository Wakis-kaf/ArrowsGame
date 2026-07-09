using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Framework.Runtime.UI
{
    public class UTabNavigation : MonoBehaviour
    {
        public Action OnHide;
        public Action OnShow;
        [SerializeField] private RectTransform m_Content;
        [SerializeField] private UTabGroup m_TabGroup;
        private Dictionary<int, IDisplayUnit> m_TabIndex2ViewRTDict = new Dictionary<int, IDisplayUnit>();
        public int CurrentIndex => m_TabGroup.CurrentIndex;

        public void AddDeSelect(Action<int> listener)
        {
            m_TabGroup.AddDeSelect(listener);
        }

        public void AddDeSelect(int index, Action onTabSelect)
        {
            m_TabGroup.AddDeSelect(index, onTabSelect);
        }

        public void AddSelect(Action<int> listener)
        {
            m_TabGroup.AddSelect(listener);
        }

        public void AddSelect(int index, Action onTabSelect)
        {
            m_TabGroup.AddSelect(index, onTabSelect);
        }

        public void AddTabTo(int tabIndex)
        {
            m_TabGroup.AddTabTo(tabIndex);
        }

        public void RemoveDeSelect(Action<int> listener)
        {
            m_TabGroup.RemoveDeSelect(listener);
        }

        public void RemoveDeSelect(int index, Action onTabSelect)
        {
            m_TabGroup.RemoveDeSelect(index, onTabSelect);
        }

        public void RemoveSelect(Action<int> listener)
        {
            m_TabGroup.RemoveSelect(listener);
        }

        public void RemoveSelect(int index, Action onTabSelect)
        {
            m_TabGroup.RemoveSelect(index, onTabSelect);
        }

        public void SetTabLabel(int tabIndex, string tabContent)
        {
            m_TabGroup.SetTabLabel(tabIndex, tabContent);
        }
        public IDisplayUnit GetTabNavView(int tabIndex)
        {
            return m_TabIndex2ViewRTDict.TryGetValue(tabIndex, out IDisplayUnit view) ? view : null;
        }
        public void SetTabNavView(int tabIndex, IDisplayUnit viewTransform, string tabContent)
        {
            if (m_TabIndex2ViewRTDict.TryGetValue(tabIndex, out IDisplayUnit view))
            {
                m_TabIndex2ViewRTDict[tabIndex] = viewTransform;
            }
            else
            {
                m_TabIndex2ViewRTDict.Add(tabIndex, viewTransform);
            }
            //    Destroy(view.gameObject);
            //else

            viewTransform.DisplayGO.transform.SetParent(m_Content, false);
            RectTransform rectTransform = viewTransform.DisplayGO.transform as RectTransform;
            rectTransform.SetAnchor(AnchorPresets.TopLeft);
            //viewTransform.anchoredPosition = Vector2.zero;
            USimpleTabBar simpleTabBar = m_TabGroup.GetOrAddTab(tabIndex);
            // 设置tabbar 内容
            simpleTabBar.AddSelect(() =>
            {
                viewTransform.Show();
            });
            simpleTabBar.AddDeSelect(
                () =>
                {
                    viewTransform.Hide();
                });
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
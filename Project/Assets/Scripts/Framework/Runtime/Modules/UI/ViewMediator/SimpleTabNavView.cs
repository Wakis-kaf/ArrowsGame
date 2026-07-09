
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.UI
{
    public class SimpleTabNavView : View
    {
        private UTabNavigation _mUSimpleTabNavigation;
        private List<View> m_NavigationViews = new List<View>();
        private Action m_OnHideListeners;
        private Action m_OnShowListeners;

        public SimpleTabNavView()
        {
        }

        public SimpleTabNavView(UTabNavigation tabNavigation)
        {
            NavTabViewInit(tabNavigation);
        }

        public UTabNavigation SimpleTabNavigation => _mUSimpleTabNavigation;

        public static SimpleTabNavView CreateNavTabView(UTabNavigation tabNavigation)
        {
            return new SimpleTabNavView(tabNavigation);
        }

        public void AddDeSelect(int tabIndex, Action onTabSelect)
        {
            _mUSimpleTabNavigation.AddDeSelect(tabIndex, onTabSelect);
        }

        public void AddHideListener(Action listener)
        {
            m_OnHideListeners += listener;
        }

        public void AddSelect(int tabIndex, Action onTabSelect)
        {
            _mUSimpleTabNavigation.AddSelect(tabIndex, onTabSelect);
        }

        public void AddShowListener(Action listener)
        {
            m_OnShowListeners += listener;
        }

        public void NavTabViewInit(UTabNavigation tabNavigation)
        {
            if (tabNavigation == null) return;
            _mUSimpleTabNavigation = tabNavigation;
            _mUSimpleTabNavigation.OnShow += NavTabShow;
            _mUSimpleTabNavigation.OnHide += NavTabHide;
        }

        public override void OnUILoaded(GameObject gameObject)
        {
            if (gameObject.TryGetComponent<UTabNavigation>(out UTabNavigation tabNavigation))
            {
                NavTabViewInit(tabNavigation);
            }

            base.OnUILoaded(gameObject);
        }

        public void RemoveDeSelect(int tabIndex, Action onTabSelect)
        {
            _mUSimpleTabNavigation.RemoveDeSelect(tabIndex, onTabSelect);
        }

        public void RemoveHideListener(Action listener)
        {
            m_OnHideListeners -= listener;
        }

        public void RemoveSelect(int tabIndex, Action onTabSelect)
        {
            _mUSimpleTabNavigation.RemoveSelect(tabIndex, onTabSelect);
        }

        public void RemoveShowListener(Action listener)
        {
            m_OnShowListeners -= listener;
        }

        public void SetTabLabel(int tabIndex, string tabContent)
        {
            _mUSimpleTabNavigation.SetTabLabel(tabIndex, tabContent);
        }

        public T SetTabNavView<T>(string assetKey, int tabIndex, string tabContent = "Tab") where T : View
        {
            _mUSimpleTabNavigation.AddTabTo(tabIndex);
            var view = _mUSimpleTabNavigation.GetTabNavView(tabIndex);
            if (view != null && view is T)
            {
                return view as T;
            }
            // 加载资源
            return UIWindow.Ins.GetDisplayUnitAsync<T>(assetKey, (displayUnit) =>
            {
                // 设置tab
                _mUSimpleTabNavigation.SetTabNavView(tabIndex, displayUnit, tabContent);
            });
        }

        public void SwitchTab(int tabIndex)
        {
            _mUSimpleTabNavigation.SwitchTab(tabIndex);
        }

        protected virtual void OnNavTabHide()
        {
        }

        protected virtual void OnNavTabShow()
        {
        }

        private void NavTabHide()
        {
            m_OnHideListeners?.Invoke();
            OnNavTabHide();
        }

        private void NavTabShow()
        {
            m_OnShowListeners?.Invoke();
            OnNavTabShow();
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Framework.Runtime.UI
{
    public class UckbTabNavView : View
    {
        private UCkbTabNavigation m_TabNavigation;
        private Dictionary<int, IDisplayUnit> m_NavigationViews = new Dictionary<int, IDisplayUnit>();
        private Action m_OnHideListeners;
        private Action m_OnShowListeners;

        public UckbTabNavView()
        {
        }
        private Dictionary<int, string[]> m_Tab2RedsMap = new Dictionary<int, string[]>();
        public void RegisterTabReds(int tabIndex, string[] reds)
        {
            if (!m_Tab2RedsMap.ContainsKey(tabIndex))
            {
                m_Tab2RedsMap.Add(tabIndex, reds);
            }
        }
        public bool TryGetRedTabs(int tabIndex, out string[] reds)
        {
            if (m_Tab2RedsMap.TryGetValue(tabIndex, out reds))
            {
                return true;
            }
            return false;
        }
        public UckbTabNavView(UCkbTabNavigation tabNavigation)
        {
            NavTabViewInit(tabNavigation);
        }

        public UCkbTabNavigation TabNavigation => m_TabNavigation;

        public static UckbTabNavView CreateNavTabView(UCkbTabNavigation tabNavigation)
        {
            return new UckbTabNavView(tabNavigation);
        }


        public void AddHideListener(Action listener)
        {
            m_OnHideListeners += listener;
        }

        public void AddSelect(int tabIndex, Action<bool> onTabSelectChange)
        {
            m_TabNavigation.TagGroup.AddSelect(tabIndex, onTabSelectChange);
        }
        public void SetSelectCheck(Func<int, int,int> selectCheck)
        {
            m_TabNavigation.SetSelectCheck(selectCheck);
        }

        public void AddShowListener(Action listener)
        {
            m_OnShowListeners += listener;
        }

        public void NavTabViewInit(UCkbTabNavigation tabNavigation)
        {
            if (tabNavigation == null) return;
            m_TabNavigation = tabNavigation;
            m_TabNavigation.OnShow += NavTabShow;
            m_TabNavigation.OnHide += NavTabHide;
            
        }

        public override void OnUILoaded(GameObject gameObject)
        {
            if (gameObject.TryGetComponent<UCkbTabNavigation>(out UCkbTabNavigation tabNavigation))
            {
                NavTabViewInit(tabNavigation);
            }

            base.OnUILoaded(gameObject);
        }

        //public void RemoveDeSelect(int tabIndex, Action onTabSelect)
        //{
        //    m_TabNavigation.TagGroup. RemoveDeSelect(tabIndex, onTabSelect);
        //}

        public void RemoveHideListener(Action listener)
        {
            m_OnHideListeners -= listener;
        }

        public void RemoveSelect(int tabIndex, Action<bool> onTabSelectChange)
        {
            m_TabNavigation.TagGroup.RemoveSelect(tabIndex, onTabSelectChange);
        }

        public void RemoveShowListener(Action listener)
        {
            m_OnShowListeners -= listener;
        }

        public void SetTabLabel(int tabIndex, string tabContent)
        {
            m_TabNavigation.SetTabLabel(tabIndex, tabContent);
        }
        public View GetCurView() 
        {
            return GetView<View>(GetCurrentTabIndex());
        }
        public View GetViewByName(string name)
        {
            foreach (var item in m_NavigationViews)
            {
                if(item.Value.Type.Name == name)
                {
                    return item.Value as View;
                }
            }
            return null;
        }
        public T GetView<T>(int index) where T : View
        {
            m_NavigationViews.TryGetValue(index, out IDisplayUnit view);
            return view as T;
        }
        public T SetTabNavView<T>(int tabIndex,string assetKey ="", string tabContent="") where T : View
        {
            m_TabNavigation.TagGroup.AddTabTo(tabIndex);
            // 加载资源
            var displayUnit = UIWindow.Ins.GetDisplayUnitAsync<T>(assetKey, (displayUnit) =>
            {
                // 设置tab
                SetTabNavView(tabIndex, displayUnit, tabContent);
            });
            SetTabNavView(tabIndex, displayUnit, tabContent);
            return displayUnit as T;
        }
        private void SetTabNavView(int tabIndex, IDisplayUnit displayUnit, string tabContent)
        {
            if (m_NavigationViews.ContainsKey(tabIndex))
            {
                m_NavigationViews[tabIndex] = displayUnit;
            }
            else
            {
                m_NavigationViews.Add(tabIndex, displayUnit);
            }
            m_TabNavigation.SetTabNavView(tabIndex, displayUnit, tabContent);
        }
        public int GetCurrentTabIndex()
        {
            return m_TabNavigation.SelectedIndex;
        }

        public void SwitchTab(int tabIndex)
        {
            m_TabNavigation.SwitchTab(tabIndex);
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
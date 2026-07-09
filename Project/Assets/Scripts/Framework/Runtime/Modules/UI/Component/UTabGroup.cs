using Framework.Runtime.LogSystem;

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Runtime.UI
{
    public class UTabGroup : ToggleGroup
    {
        public bool autoLayout = true;
        public int columns = 0;
        public Vector2 spacing;
        [SerializeField] private USimpleTabBar m_CheckBoxPrefab;
        [SerializeField] private int m_CurrentIndex = 0;
        private bool m_IsDirty = false;
        private List<USimpleTabBar> m_TabBarList = new List<USimpleTabBar>();
        private RectTransform m_TabBarPrefabRT;
        [SerializeField] private List<string> m_Tabs = new List<string>();
        private Action<int> OnDeSelect;
        private Action<int> OnSelect;
        public int CurrentIndex => m_CurrentIndex;
        public int TabCount => m_TabBarList.Count;

        private RectTransform tabbarPrefabRT
        {
            get
            {
                if (m_TabBarPrefabRT == null)
                    m_TabBarPrefabRT = m_CheckBoxPrefab.GetComponent<RectTransform>();
                return m_TabBarPrefabRT;
            }
        }

        public void AddDeSelect(Action<int> listener)
        {
            OnDeSelect += listener;
        }

        public void AddDeSelect(int tabIndex, Action onTabSelect)
        {
            if (m_TabBarList.Count <= tabIndex)
            {
                Log.Error("switch tab index over current tabs count!");
                return;
            }

            m_TabBarList[tabIndex].AddDeSelect(onTabSelect);
        }

        public void AddSelect(Action<int> listener)
        {
            OnSelect += listener;
        }

        public void AddSelect(int tabIndex, Action onTabSelect)
        {
            if (m_TabBarList.Count <= tabIndex)
            {
                Log.Error("switch tab index over current tabs count!");
                return;
            }

            m_TabBarList[tabIndex].AddSelect(onTabSelect);
        }

        public void AddTabTo(int tabIndex)
        {
            int start = TabCount;
            int create = tabIndex - start + 1;
            for (int i = 0; i < create; i++)
            {
                CreateAndAddTab(start + i);
            }
        }

        public USimpleTabBar GetOrAddTab(int tabIndex)
        {
            var tabBar = GetTabBar(tabIndex);
            //OnDataChanged();
            return tabBar;
        }

        public void RemoveDeSelect(Action<int> listener)
        {
            OnDeSelect -= listener;
        }

        public void RemoveDeSelect(int tabIndex, Action onTabSelect)
        {
            if (m_TabBarList.Count <= tabIndex)
            {
                Log.Error("switch tab index over current tabs count!");
                return;
            }

            m_TabBarList[tabIndex].RemoveDeSelect(onTabSelect);
        }

        public void RemoveSelect(Action<int> listener)
        {
            OnSelect -= listener;
        }

        public void RemoveSelect(int tabIndex, Action onTabSelect)
        {
            if (m_TabBarList.Count <= tabIndex)
            {
                Log.Error("switch tab index over current tabs count!");
                return;
            }

            m_TabBarList[tabIndex].RemoveSelect(onTabSelect);
        }

        public void SetTabLabel(int tabIndex, string tabContent)
        {
            USimpleTabBar simpleTabBar = GetTabBar(tabIndex);
            m_Tabs[tabIndex] = tabContent;
            simpleTabBar?.SetTabLabel(tabContent);
        }

        public void SwitchTab(int tabIndex)
        {
            if (m_TabBarList.Count <= tabIndex)
            {
                Log.Error("switch tab index over current tabs count!");
                return;
            }

            m_CurrentIndex = tabIndex;
            m_TabBarList[m_CurrentIndex].isOn = true;
        }

        protected override void Awake()
        {
            base.Awake();
            // 生成Tabs
            OnDataChanged();
            SwitchTab(m_CurrentIndex);
        }

        protected virtual void CalculatePosition()
        {
            if (autoLayout == false) return;
            Vector2 pivot = tabbarPrefabRT.pivot;
            Vector2 size = tabbarPrefabRT.rect.size;

            float startX = -(pivot.x - tabbarPrefabRT.anchorMin.x) * size.x;
            float startY = -(pivot.y - 1 + tabbarPrefabRT.anchorMin.y) * size.y;
            float orginX = startX;
            int count = m_TabBarList.Count;
            for (int i = 0; i < count; i++)
            {
                if (!m_TabBarList[i].gameObject.activeSelf) continue;

                m_TabBarList[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(startX, startY);
                if (columns == 0) //一行
                {
                    startX += size.x + spacing.x;
                }
                else if (columns == 1) //一列
                {
                    startY -= size.y + spacing.y;
                }
                else //多列
                {
                    if (i % columns == columns - 1) //换行
                    {
                        startX = orginX;
                        startY -= size.y + spacing.y;
                    }
                    else
                    {
                        startX += size.x + spacing.x;
                    }
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SwitchTab(m_CurrentIndex);
        }

        private void AddTab(USimpleTabBar simpleTabBar)
        {
            m_TabBarList.Add(simpleTabBar);
            if (m_TabBarList.Count > m_Tabs.Count)
            {
                m_Tabs.Add(simpleTabBar.text);
            }
        }

        private USimpleTabBar CreateAndAddTab(int index)
        {
            var tabBar = Instantiate(m_CheckBoxPrefab, transform, false);
            bool isShow = m_CurrentIndex == index;
            tabBar.isOn = isShow; // !! 这一步一定要先于tabBar.group = this; 执行
            AddTab(tabBar);
            tabBar.group = this;
            tabBar.name = "tabBar_" + index;
            int before = m_CurrentIndex;
            tabBar.onValueChanged.AddListener((isOn) => { OnTabValueChanged(tabBar, isOn); });
            m_CurrentIndex = before;
            tabBar.isOn = m_CurrentIndex == index;
            m_IsDirty = true;
            return tabBar;
        }

        private USimpleTabBar FindTabBar(int index)
        {
            if (index < m_TabBarList.Count)
                return m_TabBarList[index];
            return null;
        }

        private USimpleTabBar GetTabBar(int index)
        {
            for (int i = m_TabBarList.Count; i <= index; i++)
            {
                CreateAndAddTab(i);
            }

            return m_TabBarList[index];
        }

        private void HideAll()
        {
            for (int i = 0; i < m_TabBarList.Count; i++)
            {
                m_TabBarList[i].gameObject.SetActive(false);
                m_TabBarList[i].isOn = m_CurrentIndex == i;
            }
        }

        private void LateUpdate()
        {
            if (m_IsDirty)
            {
                OnDataChanged();
                m_IsDirty = false;
            }
        }

        private void OnDataChanged()
        {
            //HideAll();
            int start = m_TabBarList.Count;
            int count = m_Tabs.Count - start;
            for (int i = 0; i < count; i++)
            {
                CreateAndAddTab(i + start);
            }

            for (int i = 0; i < m_Tabs.Count; i++)
            {
                string text = m_Tabs[i];
                USimpleCheckBox tabBar = GetTabBar(i);
                tabBar.text = text;
                tabBar.gameObject.SetActive(true);
                tabBar.isOn = m_CurrentIndex == i;
            }

            CalculatePosition();
        }

        private void OnTabValueChanged(USimpleTabBar simpleTabBar, bool value)
        {
            int index = m_TabBarList.IndexOf(simpleTabBar);
            //Debug.Log(index + " " + value);
            if (value)
            {
                m_CurrentIndex = index;
                OnSelect?.Invoke(index);
            }
            else
                OnDeSelect?.Invoke(index);
        }
    }
}
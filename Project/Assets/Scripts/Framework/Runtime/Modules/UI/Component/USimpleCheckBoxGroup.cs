using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Runtime.UI
{
    public class USimpleCheckBoxGroup : ToggleGroup
    {
        public bool allowMultiSelect = false;
        public bool autoLayout = true;
        public int columns = 0;
        public Vector2 spacing;
        [SerializeField] private List<string> m_CheckBoxItemList = new List<string>();
        private List<USimpleCheckBox> m_CheckBoxList = new List<USimpleCheckBox>();
        private RectTransform m_CheckBoxPrefabRT;
        [SerializeField] private USimpleCheckBox mSimpleCheckBoxPrefab;

        private RectTransform checkBoxPrefavRT
        {
            get
            {
                if (m_CheckBoxPrefabRT == null)
                    m_CheckBoxPrefabRT = mSimpleCheckBoxPrefab.GetComponent<RectTransform>();
                return m_CheckBoxPrefabRT;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            // 生成Tabs
            OnDataChanged();
        }

        protected virtual void CalculatePosition()
        {
            if (autoLayout == false) return;
            Vector2 pivot = checkBoxPrefavRT.pivot;
            Vector2 size = checkBoxPrefavRT.rect.size;

            float startX = pivot.x * size.x;
            float startY = (pivot.y - 1) * size.y;
            float orginX = startX;
            int count = m_CheckBoxList.Count;
            for (int i = 0; i < count; i++)
            {
                if (!m_CheckBoxList[i].gameObject.activeSelf) continue;

                m_CheckBoxList[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(startX, startY);
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

        private USimpleCheckBox GetCheckBox(int index)
        {
            USimpleCheckBox simpleCheckBox = null;
            if (index >= m_CheckBoxList.Count)
            {
                simpleCheckBox = Instantiate(mSimpleCheckBoxPrefab, transform);
                m_CheckBoxList.Add(simpleCheckBox);
            }

            simpleCheckBox = m_CheckBoxList[index];
            simpleCheckBox.group = allowMultiSelect ? null : this;
            return simpleCheckBox;
        }

        private void HideAll()
        {
            for (int i = 0; i < m_CheckBoxList.Count; i++)
            {
                m_CheckBoxList[i].gameObject.SetActive(false);
            }
        }

        private void OnDataChanged()
        {
            HideAll();
            for (int i = 0; i < m_CheckBoxItemList.Count; i++)
            {
                string text = m_CheckBoxItemList[i];
                USimpleCheckBox simpleCheckBox = GetCheckBox(i);
                simpleCheckBox.text = text;
                simpleCheckBox.gameObject.SetActive(true);
            }

            CalculatePosition();
        }
    }
}
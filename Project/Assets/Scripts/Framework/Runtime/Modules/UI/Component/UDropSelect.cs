
using Framework.Runtime.UI.UIAnimae;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework.Runtime.UI
{
    [Serializable]
    public class DropSelectOption
    {
        public string optionText;
        public string value;
    }

    public class UDropSelect : MonoBehaviour
    {
        public bool allowMultiSelect;

        // 是否允许多选
        public bool allSwitchOff = true;
        public UIAnimatorCaller collapseAnimCaller = new UIAnimatorCaller() { targetSequence = "Fold" };

        public UIAnimatorCaller expandAnimCaller = new UIAnimatorCaller() { targetSequence = "Expand" };

        private int m_CurrentSelect;

        /// <summary>
        /// 是否收起
        /// </summary>
        [SerializeField, ReadOnly]
        private bool m_IsCollapse = false;
        private bool m_IsViewDirty = true;

        private Action<int> m_OnDisSelect;

        private Action<int> m_OnSelect;
        private Action<bool> m_OnCollapseChanged;

        private Action<int, bool> m_OnSelectChanged;

        [SerializeField] private List<DropSelectOption> m_Options = new List<DropSelectOption>();

        [SerializeField] private UList m_OptionsList;

        private Type m_RenderType = typeof(UDropDefaultDisplayUnit);

        private List<int> m_SelectIndexList = new List<int>();

        // 能够取消勾选
        [SerializeField] private UButton m_TriggerBtn;
        public UButton triggerBtn => m_TriggerBtn;
        public int CurrentSelect
        {
            get { return m_CurrentSelect; }
            set
            {
                m_CurrentSelect = value;
                m_OptionsList.SetSelect(m_CurrentSelect, true);
            }
        }
        /// <summary>
        /// 是否收起
        /// </summary>
        public bool IsCollapse
        {
            get { return m_IsCollapse; }
        }

        public UList OptionList => m_OptionsList;
        public void AddDisSelect(Action<int> listener)
        {
            m_OnDisSelect += listener;
        }

        public void AddCollapseChanged(Action<bool> listener)
        {
            m_OnCollapseChanged -= listener;
            m_OnCollapseChanged += listener;
        }
        public void RemoveCollapseChanged(Action<bool> listener)
        {
            m_OnCollapseChanged -= listener;
        }
        public void AddSelect(Action<int> listener)
        {
            m_OnSelect -= listener;
            m_OnSelect += listener;
        }

        public void AddSelectChanged(Action<int, bool> listener)
        {
            m_OnSelectChanged += listener;
        }

        public DropSelectOption GetOption(int index)
        {
            if (m_Options.Count > index) return m_Options[index];
            return null;
        }

        public void RemoveDisSelect(Action<int> listener)
        {
            m_OnDisSelect -= listener;
        }

        public void RemoveSelect(Action<int> listener)
        {
            m_OnSelect -= listener;
        }

        public void RemoveSelectChanged(Action<int, bool> listener)
        {
            m_OnSelectChanged -= listener;
        }

        public void SetCollapse(bool isCollapse, bool showAnim = true)
        {
            m_IsCollapse = isCollapse;
            PlayAnim(showAnim);
            m_OnCollapseChanged?.Invoke(m_IsCollapse);
        }

        public void SetOptions<T>(List<T> options) where T: DropSelectOption
        {
            m_Options = options.Cast<DropSelectOption>().ToList(); 
            ResetOption();
        }

        public void SetRenderType(Type type)
        {
            if (type != null)
            {
                m_RenderType = type;
            }
        }

        protected virtual void Awake()
        {
            m_TriggerBtn.AddClick(CollapseStateChange);
            ResetOption();
        }

        private void AddOptionOutsideClose()
        {
            var uSelect = UIEventUtil.GetOrAddUSelect(transform);
            uSelect.SetSelect(OnOptionOutSideSelect);
            UIUtil.SetSelectedGameObject(gameObject);
        }

        private void CollapseStateChange()
        {
            SetCollapse(!IsCollapse);
            //IsCollapse = !IsCollapse;
        }
        private void OnItemClick(int index)
        {
            if (IsCollapse) return;

            SetCollapse(true);
        }

        private void OnItemDeSelect(int index)
        {
            m_SelectIndexList.Remove(index);
            m_OnSelectChanged?.Invoke(index, false);
            m_OnDisSelect?.Invoke(index);
        }

        private void OnItemSelect(int index)
        {
            if (m_SelectIndexList.IndexOf(index) == -1)
                m_SelectIndexList.Add(index);
            m_CurrentSelect = index;
            DropSelectOption option = m_Options[index];
            // 切换文本显示
            m_TriggerBtn.Text = option.optionText;
            m_OnSelectChanged?.Invoke(index, true);
            m_OnSelect?.Invoke(index);
        }

        private void OnOptionOutSideSelect(bool isSelect)
        {
            if (!isSelect && !IsCollapse)
            {
                SetCollapse(true);
            }
        }

        private void OnValidate()
        {
            if (m_OptionsList)
                m_OptionsList.AllowMultiSelect = allowMultiSelect;
            if (m_OptionsList)
                m_OptionsList.AllowSwitchOff = allSwitchOff;
        }

        private void PlayAnim(bool isShowAnim = false)
        {
            if (m_IsCollapse)
            {
                //m_OptionsList.ClearSelects();

                if (isShowAnim)
                {
                    collapseAnimCaller?.Call();
                }
                else
                {
                    collapseAnimCaller?.Complete();
                }
            }
            else
            {
                if (isShowAnim)
                {
                    expandAnimCaller?.Call();
                }
                else
                {
                    expandAnimCaller?.Complete();
                }
                AddOptionOutsideClose();
            }
        }

        private void ResetOption()
        {
            m_CurrentSelect = -1;
            m_SelectIndexList.Clear();
            m_OptionsList.ClearSelects();
            m_OptionsList.ListRenderType = m_RenderType;
            m_OptionsList.SetDataSources(m_Options); // 设置数据源
            m_OptionsList.AddClick(OnItemClick);
            m_OptionsList.AddSelect(OnItemSelect);
            m_OptionsList.AddDeSelect(OnItemDeSelect);
            SetCollapse(true,false);
        }
        private class UDropDefaultDisplayUnit : UListDisplayUnit
        {
            private UText m_Text;
            protected override void OnGUI(object data)
            {
                base.OnGUI(data);
                if (!(data is DropSelectOption option)) return;
                m_Text.text = option.optionText;
            }

            protected override void OnInitUI()
            {
                base.OnInitUI();
                m_Text = GetComponentInChildren<UText>("UText");
            }
        }
    }
}
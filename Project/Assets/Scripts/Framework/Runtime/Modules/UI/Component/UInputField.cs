using Framework.NodeTree.TrieTreeStructure;

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Runtime.UI
{
    public class UInputField : InputField
    {
        private bool m_ContentHasChanged;
        [SerializeField] private float m_DebounceTime = 0.2f;
        [SerializeField] private bool m_EnableDebounce;
        [SerializeField] private bool m_EnablePromptBox; // 是否支持联想框
        private bool m_IsPromptTrigger;
        private float m_LastDebounceTime;
        private Action<string> m_OnDebounceCallback;
        private Action<string> m_OnValueChanged;
        [SerializeField] private UList m_PromptList;
        private TrieTree m_TrieTree; // 前缀树

        public void AddDebounceChanged(Action<string> callback)
        {
            if (!m_EnableDebounce) return;
            m_OnDebounceCallback += callback;
        }

        public void AddValueChanged(Action<string> listener)
        {
            m_OnValueChanged += listener;
        }

        public void Clear()
        {
            text = "";
        }

        public bool InvokePrompt()
        {
            string value = text;
            return m_TrieTree.Submit(value);
        }

        public void RegisterPrompt(string prefix, Action<string, string[]> handler = null)
        {
            m_TrieTree?.Register(prefix, handler);
        }

        public void RemoveDebounceChanged(Action<string> callback)
        {
            if (!m_EnableDebounce) return;
            m_OnDebounceCallback -= callback;
        }

        public void RemovePrompt(string prefix, Action<string, string[]> handler = null)
        {
            m_TrieTree?.Remove(prefix, handler);
        }

        public void RemoveValueChanged(Action<string> listener)
        {
            m_OnValueChanged -= listener;
        }

        protected override void Awake()
        {
            base.Awake();
            m_TrieTree = new TrieTree();
            onValueChanged.AddListener(OnValueChanged);
            if (m_EnablePromptBox && m_PromptList != null)
            {
                m_PromptList.ListRenderType = typeof(UInputFieldDefaultDisplayUnit);
                m_PromptList.AddSelect(OnItemSelect);
                m_PromptList.gameObject.SetActive(false);
            }
        }

        private void OnItemSelect(int index)
        {
            string value = m_PromptList.GetData(index) as string;
            text = value;
            m_IsPromptTrigger = true;
            MoveTextEnd(false);
            m_PromptList.gameObject.SetActive(false);
        }

        private void OnValueChanged(string content)
        {
            m_ContentHasChanged = true;
            m_LastDebounceTime = Time.time;
            if (m_IsPromptTrigger)
            {
                m_IsPromptTrigger = false;
                return;
            }

            UpdatePromptBox(content);
            m_OnValueChanged?.Invoke(content);
        }

        private void Update()
        {
            if (!m_EnableDebounce || !m_ContentHasChanged) return;
            if (Time.time - m_LastDebounceTime > m_DebounceTime)
            {
                m_OnDebounceCallback?.Invoke(text);
                m_LastDebounceTime = Time.time;
                m_ContentHasChanged = false;
            }
        }

        private void UpdatePromptBox(string content)
        {
            if (!m_EnablePromptBox)
            {
                m_PromptList?.gameObject.SetActive(false);
                return;
            }

            if (m_PromptList == null) return;
            if (string.IsNullOrEmpty(content))
            {
                m_PromptList?.gameObject.SetActive(false);
                return;
            }

            m_PromptList?.gameObject.SetActive(true);
            var prompts = m_TrieTree.GetPromptList(content);
            m_PromptList.SetDataSources(prompts); // 设置提示框数据源
        }

        private class UInputFieldDefaultDisplayUnit : UListDisplayUnit
        {
            private UText m_Text;

            protected override void OnGUI(object data)
            {
                base.OnGUI(data);
                var str = data as string;
                m_Text.text = str;
            }

            protected override void OnInitUI()
            {
                base.OnInitUI();
                m_Text = GetComponentInChildren<UText>("UText");
            }
        }
    }
}
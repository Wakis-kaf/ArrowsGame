using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework.Runtime.MLanAndTheme
{
    public class LayerBlocker
    {
        private Func<string, bool> __fetch_keydown;
        private Func<string, bool> __fetch_keypushing;
        private Func<string, bool> __fetch_keyup;
        private Func<string, Vector2> __fetch_pos;
        private Func<string, float> __fetch_value;
        private Func<string, float> __fetch_value_Raw;
        private bool m_IsEnable = true;
        private Dictionary<string, Action> m_keyDown;
        private List<string> m_KeyDownKeys;
        private Dictionary<string, Action> m_keyPushing;
        private List<string> m_KeyPushingKeys;
        private Dictionary<string, Action> m_keyUp;
        private List<string> m_KeyUpKeys;
        private LayerBase sourceInput;
        private string m_LayerName = string.Empty;

        public string LayerName => m_LayerName;

        public void SetLayerName(string name)
        {
            m_LayerName = name;
        }

        public LayerBlocker(LayerBase src)
        {
            m_keyPushing = new Dictionary<string, Action>();
            m_keyDown = new Dictionary<string, Action>();
            m_keyUp = new Dictionary<string, Action>();
            SetSourceInput(src);
        }

        private bool Enable => m_IsEnable;

        public void AddKeyDown(string name, Action action)
        {
            HandleAdd(m_keyDown, name, action);
            m_KeyDownKeys = m_keyDown.Keys.ToList();
        }

        public void AddKeyPushing(string name, Action action)
        {
            HandleAdd(m_keyPushing, name, action);
            m_KeyPushingKeys = m_keyPushing.Keys.ToList();
        }

        public void AddKeyUp(string name, Action action)
        {
            HandleAdd(m_keyUp, name, action);
            m_KeyUpKeys = m_keyUp.Keys.ToList();
        }

        public bool IsKeyDown(string key)
        {
            return __fetch_keydown(key);
        }

        public bool IsKeyPushing(string key)
        {
            return __fetch_keypushing(key);
        }

        public bool IsKeyUp(string key)
        {
            return __fetch_keyup(key);
        }

        public Vector2 Pos(string key)
        {
            return __fetch_pos(key);
        }

        public void RemoveKeyDown(string name, Action action)
        {
            HandleRemove(m_keyDown, name, action);
            m_KeyDownKeys = m_keyDown.Keys.ToList();
        }

        public void RemoveKeyPushing(string name, Action action)
        {
            HandleRemove(m_keyPushing, name, action);
            m_KeyPushingKeys = m_keyPushing.Keys.ToList();
        }

        public void RemoveKeyUp(string name, Action action)
        {
            HandleRemove(m_keyUp, name, action);
            m_KeyUpKeys = m_keyUp.Keys.ToList();
        }

        public void SetEnabled(bool value)
        {
            m_IsEnable = value;
            if (value)
            {
                __fetch_value = sourceInput.Value;
                __fetch_value_Raw = sourceInput.ValueRaw;
                __fetch_keydown = sourceInput.IsKeyDown;
                __fetch_keyup = sourceInput.IsKeyUp;
                __fetch_keypushing = sourceInput.IsKeyPushing;
                __fetch_pos = sourceInput.Pos;
            }
            else
            {
                __fetch_value = __disabled_value;
                __fetch_value_Raw = __disabled_value;
                __fetch_keydown = __disabled_pushkey;
                __fetch_keyup = __disabled_pushkey;
                __fetch_keypushing = __disabled_pushkey;
                __fetch_pos = __disabled_pos;
            }
        }

        public void SetKeyDown(string name, Action action)
        {
            HandleSet(m_keyDown, name, action);
            m_KeyDownKeys = m_keyDown.Keys.ToList();
        }

        public void SetKeyPushing(string name, Action action)
        {
            HandleSet(m_keyPushing, name, action);
            m_KeyPushingKeys = m_keyPushing.Keys.ToList();
        }

        public void SetKeyUp(string name, Action action)
        {
            HandleSet(m_keyUp, name, action);
            m_KeyUpKeys = m_keyUp.Keys.ToList();
        }

        public void SetSourceInput(LayerBase src)
        {
            this.sourceInput = src;
            SetEnabled(Enable);
        }

        public void Update()
        {
            if (m_KeyDownKeys != null)
            {
                for (int i = 0; i < m_KeyDownKeys.Count; i++)
                {
                    var key = m_KeyDownKeys[i];
                    if (IsKeyDown(key))
                    {
                        m_keyDown[key]?.Invoke();
                    }
                }
            }

            if (m_KeyUpKeys != null)
            {
                for (int i = 0; i < m_KeyUpKeys.Count; i++)
                {
                    var key = m_KeyUpKeys[i];
                    if (IsKeyUp(key))
                    {
                        m_keyUp[key]?.Invoke();
                    }
                }
            }

            if (m_KeyPushingKeys != null)
            {
                for (int i = 0; i < m_KeyPushingKeys.Count; i++)
                {
                    var key = m_KeyPushingKeys[i];
                    if (IsKeyPushing(key))
                    {
                        m_keyPushing[key]?.Invoke();
                    }
                }
            }

            // InvokeOnTrue(m_KeyDownKeys, m_keyDown, IsKeyDown); InvokeOnTrue(m_KeyUpKeys, m_keyUp,
            // IsKeyUp); InvokeOnTrue(m_KeyPushingKeys, m_keyPushing, IsKeyPushing);
        }

        public float Value(string key)
        {
            return __fetch_value(key);
        }

        public float ValueRaw(string key)
        {
            return __fetch_value_Raw(key);
        }

        private Vector2 __disabled_pos(string key)
        {
            return Vector2.zero;
        }

        private bool __disabled_pushkey(string key)
        {
            return false;
        }

        private float __disabled_value(string key)
        {
            return 0;
        }

        /* 对外的API */

        private void HandleAdd(Dictionary<string, Action> dict, string name, Action action)
        {
            if (dict.ContainsKey(name))
            {
                dict[name] -= action;
                dict[name] += action;
            }
            else
            {
                dict.Add(name, action);
            }
        }

        private void HandleRemove(Dictionary<string, Action> dict, string name, Action action)
        {
            dict.Remove(name);
        }

        private void HandleSet(Dictionary<string, Action> dict, string name, Action action)
        {
            if (dict.ContainsKey(name)) dict[name] = action;
            else
            {
                dict.Add(name, action);
            }
        }

        private void InvokeOnTrue(Dictionary<string, Action> dict, Func<string, bool> cb)
        {
            for (int i = 0; i < dict.Keys.Count; i++)
            {
                var key = dict.Keys.ElementAt(i);
                if (cb(key))
                {
                    dict[key]?.Invoke();
                }
            }
        }

        private void InvokeOnTrue(List<string> keys, Dictionary<string, Action> dict, Func<string, bool> cb)
        {
            if (keys == null) return;
            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                if (cb(key))
                {
                    dict[key]?.Invoke();
                }
            }
        }
    }
}
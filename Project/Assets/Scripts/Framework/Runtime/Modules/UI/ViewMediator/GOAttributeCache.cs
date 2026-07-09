using System;
using UnityEngine;

namespace Framework.Runtime.UI
{
    public class GOAttributeCache<T>
    {
        private bool m_Update;
        private T m_Value;
        private Func<GameObject, T> m_ValueGetter;
        private Action<GameObject, T, bool> m_ValueSetter;

        public GOAttributeCache(Action<GameObject, T,bool> valueSetter, Func<GameObject, T> valueGetter)
        {
            m_ValueSetter = valueSetter;
            m_ValueGetter = valueGetter;
        }

        public T value => m_Value;

        public void Clear()
        {
            m_ValueGetter = null;
            m_ValueSetter = null;
        }

        public T GetValue(GameObject go)
        {
            if (go != null && m_ValueGetter != null)
            {
                m_Value = m_ValueGetter(go);
            }

            return m_Value;
        }

        public void SetValue(T newValue, GameObject go = null)
        {

            m_Update = true;
            bool isNewValue = newValue.Equals(m_Value);
            m_Value = newValue;
            if (go != null && m_ValueSetter != null)
            {
                m_ValueSetter(go, newValue, isNewValue);
                m_Update = false;
            }
        }

        public void UpdateValue(GameObject go)
        {
            if (m_Update)
            {
                SetValue(m_Value, go);
            }
        }
    }
}
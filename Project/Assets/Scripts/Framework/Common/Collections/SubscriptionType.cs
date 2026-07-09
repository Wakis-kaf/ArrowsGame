using System;

namespace Framework.Collections
{
    public class SubscriptionType<T>
    {
        private Action<T> m_Listener;
        private T m_Val;

        public T Value
        {
            get
            {
                return m_Val;
            }
            set
            {
                SetValue(value);
            }
        }

        public void AddValueChangeListener(Action<T> listener)
        {
            m_Listener -= listener;
            m_Listener += listener;
        }

        public void Clear()
        {
            m_Listener = null;
            m_Val = default;
        }

        public void RemoveValueChangeListener(Action<T> listener)
        {
            m_Listener -= listener;
        }

        public void SetValue(T val)
        {
            if (!Equals(m_Val, val))
            {
                m_Val = val;
                m_Listener?.Invoke(val);
            }
        }
    }
}
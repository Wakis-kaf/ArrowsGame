using Framework.Utils;
using UnityEngine;

namespace Framework.Runtime.Base
{
    public class SingletonObject<T> : UnitObject where T : class
    {
        protected static T m_Instance;
        public static T Instance => m_Instance;

        public static void CreateInstance()
        {
            if (!ReferenceEquals(m_Instance, null)) return ;
            m_Instance = Utility.ReflectionUtil.CreateInstance<T>();
            return ;
        }

        public static T CreateInstance(params object[] args)
        {
            if (!ReferenceEquals(m_Instance, null)) return m_Instance;
            m_Instance = Utility.ReflectionUtil.CreateInstance(typeof(T), args) as T;
            Debug.Log(m_Instance == null);
            return m_Instance;
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            if (ReferenceEquals(m_Instance, this))
                m_Instance = null;
        }
    }
}
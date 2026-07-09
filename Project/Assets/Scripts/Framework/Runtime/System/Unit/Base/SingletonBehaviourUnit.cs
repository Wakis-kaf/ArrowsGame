using Framework.Utils;

namespace Framework.Runtime.UnitSystem.Base
{
    public class SingletonBehaviourUnit<T> : BehaviourUnit where T : class
    {
        private static T m_Instance;

        public SingletonBehaviourUnit() : base()
        {
            m_Instance = this as T;
        }

        public static T Instance
        {
            get
            {
                if (ReferenceEquals(m_Instance, null))
                {
                    m_Instance = CreateInstance();
                }

                return m_Instance;
            }
        }

        public static T CreateInstance()
        {
            if (!ReferenceEquals(m_Instance, null)) return m_Instance;
            m_Instance = Utility.ReflectionUtil.CreateInstance<T>();
            return m_Instance;
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            //m_Instance = null;
            if (ReferenceEquals(m_Instance, this))
            {
                m_Instance = null;
            }
        }
    }
}
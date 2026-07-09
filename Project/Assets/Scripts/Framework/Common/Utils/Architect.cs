using UnityEngine;

namespace Framework.Utils
{
    /// <summary>
    /// Finally we have a persistent version of the singleton. This will survive through scene
    /// loads. Perfect for system classes which require stateful, persistent data. Or audio sources
    /// where music plays through loading screens, etc
    /// </summary>
    public abstract class PersistentSingleton<T> : SingletonMonoBehaviour<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
    }

    public class Singleton<T> where T : class
    {
        private static T m_Instance = System.Activator.CreateInstance<T>();

        public static T Instance
        {
            get
            {
                return m_Instance;
            }
        }

        public static void ClearInstance()
        {
            m_Instance = null;
        }
    }

    public class SingletonLazy<T> where T : class
    {
        private static T m_Instance;

        public SingletonLazy()
        {
            m_Instance = this as T;
        }

        ~SingletonLazy()
        {
            if (m_Instance == this)
            {
                m_Instance = null;
            }
        }

        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = System.Activator.CreateInstance<T>();
                return m_Instance;
            }
        }

        public static void ClearInstance()
        {
            m_Instance = null;
        }
    }

    /// <summary>
    /// This transforms the static instance into a basic singleton. This will destroy any new
    /// versions created, leaving the original instance intact
    /// </summary>
    public abstract class SingletonMonoBehaviour<T> : StaticInstance<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            base.Awake();
        }
    }

    /// <summary>
    /// A static instance is similar to a singleton, but instead of destroying any new instances, it
    /// overrides the current instance. This is handy for resetting the state and saves you doing it manually
    /// </summary>
    public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static bool HasInstance { get => !ReferenceEquals(Instance, null); }
        public static T Instance { get; private set; }

        protected virtual void Awake() => Instance = this as T;

        protected virtual void OnApplicationQuit()
        {
            Instance = null;
            Destroy(gameObject);
        }
    }
}
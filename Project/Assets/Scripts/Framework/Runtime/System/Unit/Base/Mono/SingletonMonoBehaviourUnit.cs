

namespace Framework.Runtime.UnitSystem.MonoBase
{

    public class SingletonMonoBehaviourUnit<T> : MonoBehaviourUnit where T : class
    {
        private static T mInstance;
        public static T Instance => mInstance;

        protected override void Awake()
        {
            if (!ReferenceEquals(mInstance, null))
            {
                Destroy(gameObject);
                return;
            }

            mInstance = this as T;
            base.Awake();
        }

        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            if (ReferenceEquals(mInstance, this))
            {
                mInstance = null;
            }
        }
    }
}
namespace Framework.Runtime.MObjectPool.Core
{
    public interface IPoolElement
    {
        bool IsInPool { get; set; }
        Pool Pool { get; set; }
        void OnCreateInPool();

        void OnDestroyByPool();

        void OnGetFromPool();

        void OnPrewarmInPool();

        void OnPutToPool();
    }
}
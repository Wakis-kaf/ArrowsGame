using System.Collections.Generic;

namespace Framework.Runtime.Memory
{
    public interface ILambadaCallback
    {
        public ILambadaPool Pool { get; set; }

        public void OnGet();

        public void OnPut();
    }

    public interface ILambadaPool
    {
        public void Put(ILambadaCallback item);
    }

    public class LambadaPool<T> : ILambadaPool where T : struct, ILambadaCallback
    {
        public Queue<ILambadaCallback> pool = new Queue<ILambadaCallback>(8);

        public T Get()
        {
            if (pool.Count > 0)
            {
                return (T)pool.Dequeue();
            }
            T item = new T();
            item.Pool = this;
            item.OnGet();
            return item;
        }

        public void Put(ILambadaCallback item)
        {
            pool.Enqueue(item);
            item.OnPut();
        }
    }
}
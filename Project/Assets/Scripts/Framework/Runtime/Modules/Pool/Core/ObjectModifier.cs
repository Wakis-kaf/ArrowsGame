namespace Framework.Runtime.MObjectPool.Core
{
    public interface IModifierData
    {
    }

    public abstract class ObjectModifier
    {
        public abstract string modifierName { get; set; }

        /// <summary>
        /// 修改器被创建的时候调用
        /// </summary>
        public virtual void OnAwake()
        {
        }

        /// <summary>
        /// 当池中元素不够的时候调用
        /// </summary>
        /// <param name="item"></param>
        public virtual T OnCreate<T>(T item, IModifierData data) where T : class
        {
            return item;
        }

        /// <summary>
        /// 当池中元素超出上限的时候调用
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual T OnDestroy<T>(T item, IModifierData data) where T : class
        {
            return item;
        }

        /// <summary>
        /// 从池中取出对象的时候调用
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual T OnGet<T>(T item, IModifierData data) where T : class
        {
            return item;
        }

        /// <summary>
        /// 当池中元素预热的时候调用
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual T OnPrewarm<T>(T item, IModifierData data) where T : class
        {
            return item;
        }

        /// <summary>
        /// 从池中放入对象的时候调用
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual T OnPut<T>(T item, IModifierData data) where T : class
        {
            return item;
        }
    }
}
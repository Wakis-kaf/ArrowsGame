using Framework.Runtime.LogSystem;
using Framework.Runtime.MObjectPool.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Runtime
{
    // 对象池对象
    public abstract class ObjectPool<T> : Pool where T : class
    {
        // 修改器列表
        private Dictionary<string, ObjectModifier> m_Name2Modifiers = new Dictionary<string, ObjectModifier>();

        // 根据不同的标签分类元素
        private Dictionary<string, Queue<T>> m_Tag2Pool = new Dictionary<String, Queue<T>>();

        /// <summary>
        /// 创建并添加修改器
        /// </summary>
        /// <param name="modifierName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public TModifier CreateAndAddObjectModifier<TModifier>(string modifierName) where TModifier : ObjectModifier
        {
            if (string.IsNullOrWhiteSpace(modifierName)) return default;
            if (m_Name2Modifiers.ContainsKey(modifierName))
            {
                Log.Error($"[ObjectPool]已经存在修改器,name{modifierName}");
                return null;
            }

            Log.Info($"[ObjectPool]添加修改器成功,name{modifierName}");
            TModifier modifier = CreateModifier<TModifier>();
            m_Name2Modifiers.Add(modifierName, modifier);
            return modifier;
        }

        /// <summary>
        /// 创建对象
        /// </summary>
        /// <param name="???"></param>
        /// <param name="res"></param>
        /// <param name="createData"></param>
        /// <returns></returns>
        public bool CreateItem(string tag, ref T res, IModifierData createData)
        {
            if (tag == "") return false;
            T item = default;
            res = ModifiersCreateProcess(item, createData);
            if (res is IPoolElement poolElement)
            {
                poolElement.OnCreateInPool();
                poolElement.OnGetFromPool();
            }

            return true;
        }

        /// <summary>
        /// 创建修改器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public TModifier CreateModifier<TModifier>() where TModifier : ObjectModifier
        {
            TModifier modifier = Activator.CreateInstance<TModifier>();
            modifier.OnAwake();
            return modifier;
        }

        /// <summary>
        /// 销毁物品
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="item"></param>
        /// <param name="destroyData"></param>
        /// <returns></returns>
        public bool DestroyItem(string tag, T item, ref T res, IModifierData destroyData)
        {
            if (tag.Equals("")) return false;
            res = ModifiersDestroyProcess(item, destroyData);
            if (res is IPoolElement poolElement)
                poolElement.OnDestroyByPool();
            return true;
        }

        public void DisposeAllItem()
        {
            var keys = m_Tag2Pool.Keys;
            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys.ElementAt(i);
                DisposeItem(key);
            }
        }

        public void DisposeItem(string tag)
        {
            if (!m_Tag2Pool.ContainsKey(tag)) return;
            Queue<T> pool = m_Tag2Pool[tag];
            T res = default;
            while (pool.Count != 0)
            {
                var item = pool.Dequeue();
                DestroyItem(tag, item, ref res, null);
            }
        }

        /// <summary>
        /// 取出对象
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="res"></param>
        /// <param name="getData"></param>
        /// <returns></returns>
        public bool GetItem(string tag, ref T res, IModifierData getData)
        {
            if (tag == "") return false;
            // 判断是否由该标签的队列，没有则创建,并且返回一个新的对象
            if (!m_Tag2Pool.ContainsKey(tag) || m_Tag2Pool[tag].Count == 0)
            {
                //Log.Error($"[ObjectPool] 当前对象池中不存在 Tag 为{tag}的对象!");
                return false;
            }

            T item = default;
            // 从对象池中取出
            item = m_Tag2Pool[tag].Dequeue();
            //Log.Info($"  取出物体!Tag: {tag} (:ObjectPool)");
            if (m_Tag2Pool[tag].Count == 0)
            {
                m_Tag2Pool.Remove(tag);
            }


            if (item is IPoolElement poolElement)
            {
                poolElement.OnGetFromPool();
                poolElement.IsInPool = false;
                poolElement.Pool = null;
            }

            item = ModifiersGetProcess(item, getData);


            res = item;

            return true;
        }

        /// <summary>
        /// 从池中取出或者创建对象
        /// </summary>
        /// <returns></returns>
        public T GetOrCreateItem(string tag, IModifierData getData = null, IModifierData createData = null)
        {
            T res = default;
            if (!GetItem(tag, ref res, getData))
            {
                CreateItem(tag, ref res, createData);
            }

            return res;
        }

        /// <summary>
        /// 预热对象
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="item"></param>
        /// <param name="count"></param>
        public void PrewarmItem(string tag, int count,
            IModifierData createData = default,
            IModifierData prewarmData = default,
            IModifierData putData = default,
            IModifierData destroyData = default)
        {
            for (int i = 0; i < count; i++)
            {
                T newItem = default;
                // 创建对象
                CreateItem(tag, ref newItem, createData);
                // 调用修改器的预热方法
                newItem = ModifiersPrewarmProcess(newItem, prewarmData);
                if (newItem is IPoolElement poolElement)
                    poolElement.OnPrewarmInPool();
                // 将对象放入池中
                newItem = PutOrDestroyItem(tag, newItem, putData, destroyData);
            }
        }

        /// <summary>
        /// 放入对象
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="item"></param>
        /// <param name="putData"></param>
        /// <returns></returns>
        public bool PutItem(string tag, T item, ref T res, IModifierData putData = default)
        {
            if (tag.Equals("")) return false;
            tag = tag.Trim();
            // 如果池子已满,并且当前池子超出上限就退出
            if (!m_Tag2Pool.ContainsKey(tag) && m_Tag2Pool.Keys.Count >= tagLimitCount)
            {
                Log.Error($"[ObjectPool]存放对象失败!Tag: {tag}，对象分类已满!");
                return false;
            }
            else if (m_Tag2Pool.ContainsKey(tag) && m_Tag2Pool[tag].Count >= poolLimitCount) // 判断队列是否已满
            {
                Log.Info($"[ObjectPool]销毁对象!Tag: {tag}");
                return false;
            }
            else if (!m_Tag2Pool.ContainsKey(tag))
            {
                m_Tag2Pool.Add(tag, new Queue<T>(poolInitSize));
            }

            T processRes = ModifiersPutProcess(item, putData);

            // 存放
            m_Tag2Pool[tag].Enqueue(processRes);

            res = processRes;
            if (res is IPoolElement poolElement)
            {
                poolElement.IsInPool = true;
                poolElement.Pool = this;
                poolElement.OnPutToPool();

            }

            //Log.Info($"[ObjectPool]存放!Tag: {tag},当前数量{m_Tag2Pool[tag].Count}");
            return true;
        }

        /// <summary>
        /// 放入或者销毁对象
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="item"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public T PutOrDestroyItem(string tag, T item, IModifierData putData = default,
            IModifierData destroyData = default)
        {
            T res = default;
            if (!PutItem(tag, item, ref res, putData))
            {
                DestroyItem(tag, item, ref res, destroyData);
            }

            return res;
        }

        public T[] ReleaseAllItem()
        {
            // 释放
            var keys = m_Tag2Pool.Keys;
            List<T> res = new List<T>();
            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys.ElementAt(i);
                var items = ReleaseItem(key);
                if (items.Length > 0)
                    res.AddRange(items);
            }

            return res.ToArray();
        }

        public T[] ReleaseItem(string tag)
        {
            if (!m_Tag2Pool.ContainsKey(tag)) return new T[0];
            Queue<T> pool = m_Tag2Pool[tag];
            List<T> items = new List<T>();
            while (pool.Count != 0)
            {
                T item = default;
                // 释放
                if (GetItem(tag, ref item, null)) items.Add(item);
            }

            return items.ToArray();
        }

        /// <summary>
        /// 移除修改器
        /// </summary>
        /// <param name="modifierName"></param>
        public void RemoveObjectModifier(string modifierName)
        {
            if (string.IsNullOrWhiteSpace(modifierName)) return;
            if (!m_Name2Modifiers.ContainsKey(modifierName))
            {
                Log.Error($"[ObjectPool]不存在修改器,name{modifierName}");
                return;
            }

            Log.Info($"[ObjectPool]移除修改器成功,name{modifierName}");
            m_Name2Modifiers.Remove(modifierName);
        }

        private T ModifiersCreateProcess(T item, IModifierData data)
        {
            T res = item;
            foreach (var modifierKV in m_Name2Modifiers)
            {
                res = (T)modifierKV.Value.OnCreate(res, data);
            }

            return res;
        }

        private T ModifiersDestroyProcess(T item, IModifierData data)
        {
            T res = item;
            foreach (var modifierKV in m_Name2Modifiers)
            {
                res = (T)modifierKV.Value.OnDestroy(res, data);
            }

            return res;
        }

        private T ModifiersGetProcess(T item, IModifierData data)
        {
            T res = item;
            foreach (var modifierKV in m_Name2Modifiers)
            {
                res = (T)modifierKV.Value.OnGet(res, data);
            }

            return res;
        }

        private T ModifiersPrewarmProcess(T item, IModifierData data)
        {
            T res = item;
            foreach (var modifierKV in m_Name2Modifiers)
            {
                res = (T)modifierKV.Value.OnPrewarm(res, data);
            }

            return res;
        }

        private T ModifiersPutProcess(T item, IModifierData data)
        {
            T res = item;
            foreach (var modifierKV in m_Name2Modifiers)
            {
                res = (T)modifierKV.Value.OnPut(res, data);
            }

            return res;
        }
    }
}
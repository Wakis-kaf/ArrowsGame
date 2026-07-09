using Framework.Runtime.LogSystem;
using Framework.Runtime.MObjectPool.Core;
using Framework.Runtime.MObjectPool.GOPool;
using Framework.Runtime.Module.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.MObjectPool
{
    public class PoolModule : ModuleUnit
    {
        private GameObjectPool m_DefaultGameObjectPool;
        private Dictionary<string, Pool> m_Name2PoolMap = new Dictionary<string, Pool>();
        private GameObject m_PoolRoot;
        public GameObjectPool GameObjectPool => m_DefaultGameObjectPool;
        public GameObject PoolRoot => m_PoolRoot;

        public Pool AddPool(string name, Pool pool)
        {
            if (m_Name2PoolMap.ContainsKey(name)) return m_Name2PoolMap[name];
            m_Name2PoolMap.Add(name, pool);
            return pool;
        }

        public GameObjectPool CreateGameObjectPool(string name)
        {
            if (HasPool(name))
            {
                Log.Error($"创建物体对象池失败! 已存在相同名字的对象池 {name}");
                return null;
            }

            GameObjectPool pool = new GameObjectPool(PoolRoot.transform, name);
            AddPool(name, pool);
            pool.Init();
            return pool;
        }

        public Pool GetPool(string name)
        {
            TryGetPool(name, out Pool res);
            return res;
        }

        public T GetPool<T>() where T : Pool
        {
            foreach (var kvp in m_Name2PoolMap)
            {
                if (kvp.Value is T target) return target;
            }

            return null;
        }

        public bool HasPool(string name)
        {
            return m_Name2PoolMap.ContainsKey(name);
        }

        public void RemovePool(string name)
        {
            if (!m_Name2PoolMap.ContainsKey(name)) return;
            m_Name2PoolMap.Remove(name);
        }

        public bool TryGetPool(string name, out Pool pool)
        {
            if (m_Name2PoolMap.ContainsKey(name))
            {
                pool = m_Name2PoolMap[name];
                return true;
            }

            pool = default;
            return false;
        }

        protected override void OnModuleConstructed()
        {
            base.OnModuleConstructed();
            InitGameObjectPool();
        }

        private void InitGameObjectPool()
        {
            m_PoolRoot = new GameObject("Pools");
            m_PoolRoot.transform.SetParent(GameApp.Ins.GameAppShell.transform);
            m_DefaultGameObjectPool = new GameObjectPool(m_PoolRoot.transform);
            AddPool("GameObjectPool", m_DefaultGameObjectPool);
            m_DefaultGameObjectPool.Init();
        }
    }
}
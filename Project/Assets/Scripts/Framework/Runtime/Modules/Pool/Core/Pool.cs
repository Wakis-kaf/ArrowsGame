using System;

namespace Framework.Runtime.MObjectPool.Core
{
    [Serializable]
    public abstract class Pool
    {
        // 池子初始化大小
        public int poolInitSize = 50;

        // 最多池子数量
        public int poolLimitCount = 10000;

        // 最多存放种类
        public int tagLimitCount = 1000;

        private string m_PoolName = "ObjectPool";

        public string poolName
        {
            get => m_PoolName;
            set => m_PoolName = value;
        }
    }
}
using System;

namespace Framework.Runtime.Base
{
    public abstract class UnitObject : IUnitObject
    {
        private bool m_IsDisposed;
        private Type m_Type;

        public UnitObject()
        {
            m_Type = GetType();
        }

        ~UnitObject()
        {
            Dispose(false);
        }

        public bool IsDisposed
        {
            get { return m_IsDisposed; }
            set { }
        }

        public Type Type => m_Type;

        /// <summary>
        /// 手动调用回收资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// 资源回收实现细节
        /// </summary>
        /// <param name="isDisposeUnManagedResources">是否回收非托管资源</param>
        protected virtual void Dispose(bool isDisposeUnManagedResources)
        {
            if (m_IsDisposed) return;
            m_IsDisposed = true;
            if (isDisposeUnManagedResources)
                DisposeUnManagedResources();
            DisposeManagedResources();
        }

        /// <summary>
        /// 回收非托管堆管理资源
        /// </summary>
        protected virtual void DisposeUnManagedResources()
        {
        }

        /// <summary>
        /// 回收托管堆管理的资源
        /// </summary>
        protected virtual void DisposeManagedResources()
        {
        }
    }
}
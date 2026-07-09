using Framework.Runtime.Base;
using Framework.Runtime.LogSystem;

using System;
using UnityEngine;

namespace Framework.Runtime.UnitSystem.MonoBase
{
    public abstract class MonoUnitObject : MonoBehaviour, IUnitObject
    {
        private bool m_Disposed;
        private Type m_Type;

        public MonoUnitObject()
        {
            m_Type = GetType();
        }

        public bool IsDisposed
        {
            get { return m_Disposed; }
            set { }
        }

        public Type Type => m_Type;

        /// <summary>
        /// 资源回收
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// 资源回收实现细节
        /// </summary>
        /// <param name="isDisposeManagedResources">是否回收托管资源</param>
        protected virtual void Dispose(bool isDisposeManagedResources)
        {
            if (m_Disposed) return;
            m_Disposed = true;
            if (isDisposeManagedResources)
            {
                GameObject.Destroy(this.gameObject);
                DisposeManagedResources();
            }

            DisposeUnManagedResources();
        }

        /// <summary>
        /// 回收托管堆资源
        /// </summary>
        protected virtual void DisposeManagedResources()
        {
        }

        /// <summary>
        /// 回收非托管堆资源
        /// </summary>
        protected virtual void DisposeUnManagedResources()
        {
        }

        protected virtual void OnDestroy()
        {
            try
            {
                Dispose(false);
            }
            catch (Exception e)
            {
                Log.FatalFormat("MonoUnit Dispose  Error {0}", e.Message);
            }
        }
    }
}
using Framework.Runtime.MObjectPool.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Runtime
{
    public class SimpleObjectPool<T>: Pool where T :class, IPoolElement
    {
        private Queue<T> m_FreeObj;
        private List<T> m_List;
        public delegate T ObjGetHandler(T obj,object data);
        public delegate T ObjPutHandler(T obj);
        public delegate T ObjCreateHandler(object data);
        private ObjGetHandler m_GetHandler;
        private ObjPutHandler m_PutHandler;
        private ObjCreateHandler m_CreateHandler;
        public SimpleObjectPool()
        {
            m_FreeObj = new Queue<T>();
            m_List = new List<T>();
        }
        public void LoopAllItem(Action<T> loopFunc)
        {
            if (loopFunc == null) return;
            for (int i = 0; i < m_List.Count; i++)
            {
                loopFunc.Invoke(m_List[i]);
            }
        }
        public void Init(ObjCreateHandler createHandler ,
            ObjGetHandler getHandler = null,
            ObjPutHandler putHandler = null)
        {
            m_CreateHandler = createHandler;
            m_GetHandler = getHandler;
            m_PutHandler = putHandler;
        }

        public T GetObject(object data = null)
        {
            T obj = null;
            if (m_FreeObj.Count > 0)
            {
                 obj = m_FreeObj.Dequeue();
                if (m_GetHandler != null)
                {
                    obj = m_GetHandler(obj,data);
                }
                if (obj != null)
                {
                    obj.IsInPool = false;
                    obj.OnGetFromPool();
                }
                return obj;
            }
          
            if (m_CreateHandler != null)
            {
                obj = m_CreateHandler(data);
                if (obj != null)
                {
                    obj.Pool = this;
                    m_List.Add(obj);
                }         
                obj?.OnCreateInPool();
                if (m_GetHandler != null)
                {
                    obj = m_GetHandler(obj, data);
                    
                }
                if (obj != null)
                {
                    obj.IsInPool = false;
                    obj?.OnGetFromPool();
                }
            }
            return obj;
        }
        public void PutObject(T obj)
        {
            if (obj.IsInPool) return;
            if (m_PutHandler != null)
            {
                obj =m_PutHandler(obj);
            }
            if (obj != null)
            {
                obj.IsInPool = true;
                obj.Pool = this;
                m_FreeObj.Enqueue(obj);
                obj?.OnPutToPool();
            }
        }

        public void ReleaseAllObject(Func<T, bool> isCondinalRelease,Action<T> onItemPutCb = null)
        {
            if(isCondinalRelease == null) { return; }
            for (int i = m_List.Count - 1; i >= 0; i--)
            {
                var item = m_List[i];
                if(!item.IsInPool && isCondinalRelease(item))
                {
                    PutObject(item);
                    onItemPutCb?.Invoke(item);
                }
            }
        }
    }
}

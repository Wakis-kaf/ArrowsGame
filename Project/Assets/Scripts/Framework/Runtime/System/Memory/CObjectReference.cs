using Framework.Runtime.LogSystem;
using Framework.Utils;

using System;
using System.Collections.Generic;

namespace Framework.Runtime.Memory
{
    public class CObjectReference
    {
        private static CObjectReference m_Instance;
        private Dictionary<int, Action> m_Id2DisposeCbMap;

        private Dictionary<int, object> m_Id2RefereneMap;

        private Dictionary<object, int> m_Referene2IdMap;

        public CObjectReference()
        {
            m_Id2RefereneMap = new Dictionary<int, object>(1024);
            m_Referene2IdMap = new Dictionary<object, int>(1024);
            m_Id2DisposeCbMap = new Dictionary<int, Action>(1024);
        }

        public static CObjectReference Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new CObjectReference();
                }
                return m_Instance;
            }
        }

        public static void Clear()
        {
            Instance?.m_Id2RefereneMap.Clear();
            Instance?.m_Referene2IdMap.Clear();
            Instance?.m_Id2DisposeCbMap.Clear();
        }

        public static void Close()
        {
            Clear();
            //Instance = null;
        }

        public static int CreateReferenceId(object obj, Action disposeCb = null)
        {
            if (obj == null)
            {
                throw new Exception("不能为一个空对象分配引用id");
            }

            if (Instance.m_Referene2IdMap.TryGetValue(obj, out var id))
            {
                AddDisposeCb(id, disposeCb);
                return id;
            }
            // 分配一个16位的id
            id = Utility.IDGenerator.GetIntGuidID();
            Instance.m_Id2RefereneMap.Add(id, obj);
            Instance.m_Referene2IdMap.Add(obj, id);
            AddDisposeCb(id, disposeCb);
            return id;
        }

        public static object GetCObject(int referId)
        {
            if (Instance.m_Id2RefereneMap.TryGetValue(referId, out var obj))
            {
                return obj;
            }
            Log.Error($"未找到id 为 {referId} 的对象! 是否改对象已经被回收？或引用ID为错误");
            return null;
        }

        public static void RemoveDisposeCb(int id, Action disposeCb)
        {
            if (Instance.m_Id2DisposeCbMap.TryGetValue(id, out var action))
            {
                action -= disposeCb;
            }
        }

        public void RemoveObject(object obj, bool isMute = false)
        {
            if (m_Referene2IdMap.TryGetValue(obj, out var id))
            {
                if (m_Id2DisposeCbMap.ContainsKey(id))
                {
                    if (!isMute)
                        m_Id2DisposeCbMap[id]?.Invoke();
                    m_Id2DisposeCbMap.Remove(id);
                }
                m_Id2RefereneMap.Remove(id);
                m_Referene2IdMap.Remove(id);
            }
        }

        public void RemoveObject(int id, bool isMute = false)
        {
            if (m_Id2DisposeCbMap.ContainsKey(id))
            {
                if (!isMute)
                    m_Id2DisposeCbMap[id].Invoke();
                m_Id2DisposeCbMap.Remove(id);
            }
            m_Id2RefereneMap.Remove(id);
            m_Referene2IdMap.Remove(id);
        }

        private static void AddDisposeCb(int id, Action cb)
        {
            if (cb != null)
            {
                if (Instance.m_Id2DisposeCbMap.TryGetValue(id, out var action))
                {
                    action += cb;
                }
                else
                {
                    Instance.m_Id2DisposeCbMap.Add(id, cb);
                }
            }
        }
    }
}
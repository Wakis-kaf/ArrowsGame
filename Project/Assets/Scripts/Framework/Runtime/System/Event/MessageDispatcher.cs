using System;
using System.Collections.Generic;
using System.Reflection;

namespace Framework.Runtime
{
    public class MessageDispatcher
    {
        private Dictionary<string, MessageBox> type2MessageBox = new Dictionary<string, MessageBox>();

        public static MessageDispatcher Ins => GameApp.Ins.MessageDispatcher;
        public event Action<string> OnMsgDispatch;

        // --- 核心分发接口 ---
        public void Dispatch(string msgId) { GetMsgBox(msgId)?.Dispatch(); OnMsgDispatch?.Invoke(msgId); }
        public void Dispatch<T>(string msgId, T a) { GetMsgBox(msgId)?.Dispatch(a); OnMsgDispatch?.Invoke(msgId); }
        public void Dispatch<T, T2>(string msgId, T a, T2 b) { GetMsgBox(msgId)?.Dispatch(a, b); OnMsgDispatch?.Invoke(msgId); }
        public void Dispatch<T, T2, T3>(string msgId, T a, T2 b, T3 c) { GetMsgBox(msgId)?.Dispatch(a, b, c); OnMsgDispatch?.Invoke(msgId); }
        public void Dispatch<T, T2, T3, T4>(string msgId, T a, T2 b, T3 c, T4 d) { GetMsgBox(msgId)?.Dispatch(a, b, c, d); OnMsgDispatch?.Invoke(msgId); }

        // --- 订阅接口 ---
        public void Subscribe(string id, Action cb, IMessageSubscriber c = null) => GetOrAddMsgBox(id).Subscribe(cb, c);
        public void Subscribe<T>(string id, Action<T> cb, IMessageSubscriber c = null) => GetOrAddMsgBox(id).Subscribe(cb, c);
        public void Subscribe<T, T2>(string id, Action<T, T2> cb, IMessageSubscriber c = null) => GetOrAddMsgBox(id).Subscribe(cb, c);
        public void Subscribe<T, T2, T3>(string id, Action<T, T2, T3> cb, IMessageSubscriber c = null) => GetOrAddMsgBox(id).Subscribe(cb, c);
        public void Subscribe<T, T2, T3, T4>(string id, Action<T, T2, T3, T4> cb, IMessageSubscriber c = null) => GetOrAddMsgBox(id).Subscribe(cb, c);

        // --- 取消订阅接口 ---
        public void Unsubscribe(string id, Action cb) => GetMsgBox(id)?.Unsubscribe(cb);
        public void Unsubscribe<T>(string id, Action<T> cb) => GetMsgBox(id)?.Unsubscribe(cb);
        public void Unsubscribe<T, T2>(string id, Action<T, T2> cb) => GetMsgBox(id)?.Unsubscribe(cb);
        public void Unsubscribe<T, T2, T3>(string id, Action<T, T2, T3> cb) => GetMsgBox(id)?.Unsubscribe(cb);
        public void Unsubscribe<T, T2, T3, T4>(string id, Action<T, T2, T3, T4> cb) => GetMsgBox(id)?.Unsubscribe(cb);

        public void UnsubscribeAll(IMessageSubscriber caller)
        {
            if (caller == null) return;
            foreach (var box in type2MessageBox.Values) box.UnsubscribeAll(caller);
        }

        private MessageBox GetMsgBox(string id) => !string.IsNullOrEmpty(id) && type2MessageBox.TryGetValue(id, out var b) ? b : null;

        private MessageBox GetOrAddMsgBox(string id)
        {
            if (type2MessageBox.TryGetValue(id, out var b)) return b;
            b = new MessageBox();
            type2MessageBox.Add(id, b);
            return b;
        }
        public void ClearAllMessage()
        {
            type2MessageBox.Clear();
        }


        // ======================== 内部逻辑类 ========================
        private class MessageBox
        {
            private struct CallbackWrap
            {
                public Delegate callback;
                public IMessageSubscriber caller;
            }

            private Dictionary<Type, List<CallbackWrap>> type2CbList = new Dictionary<Type, List<CallbackWrap>>();
            private Dictionary<IMessageSubscriber, List<Delegate>> callerToDelegates = new Dictionary<IMessageSubscriber, List<Delegate>>();
            private static readonly Dictionary<int, MethodInfo> unSubMethodCache = new Dictionary<int, MethodInfo>();

            static MessageBox()
            {
                var methods = typeof(MessageBox).GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var m in methods)
                {
                    if (m.Name == "Unsubscribe" && m.IsGenericMethod)
                    {
                        var args = m.GetParameters()[0].ParameterType.GetGenericArguments();
                        unSubMethodCache[args.Length] = m;
                    }
                }
            }

            // --- 内部订阅逻辑 ---
            private void InternalSub(Type t, Delegate del, IMessageSubscriber caller)
            {
                if (!type2CbList.TryGetValue(t, out var list))
                {
                    list = new List<CallbackWrap>();
                    type2CbList.Add(t, list);
                }

                for (int i = 0; i < list.Count; i++)
                    if (list[i].callback == del) return;

                list.Add(new CallbackWrap { callback = del, caller = caller });

                if (caller != null)
                {
                    if (!callerToDelegates.TryGetValue(caller, out var dels))
                    {
                        dels = new List<Delegate>();
                        callerToDelegates.Add(caller, dels);
                    }
                    dels.Add(del);
                }
            }

            public void Subscribe(Action cb, IMessageSubscriber c = null) => InternalSub(typeof(Action), cb, c);
            public void Subscribe<T>(Action<T> cb, IMessageSubscriber c = null) => InternalSub(typeof(Action<T>), cb, c);
            public void Subscribe<T, T2>(Action<T, T2> cb, IMessageSubscriber c = null) => InternalSub(typeof(Action<T, T2>), cb, c);
            public void Subscribe<T, T2, T3>(Action<T, T2, T3> cb, IMessageSubscriber c = null) => InternalSub(typeof(Action<T, T2, T3>), cb, c);
            public void Subscribe<T, T2, T3, T4>(Action<T, T2, T3, T4> cb, IMessageSubscriber c = null) => InternalSub(typeof(Action<T, T2, T3, T4>), cb, c);

            // --- 分发逻辑 (含 IsActive 判定) ---
            public void Dispatch()
            {
                if (type2CbList.TryGetValue(typeof(Action), out var l))
                    for (int i = l.Count - 1; i >= 0; i--)
                        if (l[i].caller == null || l[i].caller.IsActiveSubscriber) (l[i].callback as Action)?.Invoke();
            }

            public void Dispatch<T>(T a)
            {
                if (type2CbList.TryGetValue(typeof(Action<T>), out var l))
                    for (int i = l.Count - 1; i >= 0; i--)
                        if (l[i].caller == null || l[i].caller.IsActiveSubscriber) (l[i].callback as Action<T>)?.Invoke(a);
            }

            public void Dispatch<T, T2>(T a, T2 b)
            {
                if (type2CbList.TryGetValue(typeof(Action<T, T2>), out var l))
                    for (int i = l.Count - 1; i >= 0; i--)
                        if (l[i].caller == null || l[i].caller.IsActiveSubscriber) (l[i].callback as Action<T, T2>)?.Invoke(a, b);
            }

            public void Dispatch<T, T2, T3>(T a, T2 b, T3 c)
            {
                if (type2CbList.TryGetValue(typeof(Action<T, T2, T3>), out var l))
                    for (int i = l.Count - 1; i >= 0; i--)
                        if (l[i].caller == null || l[i].caller.IsActiveSubscriber) (l[i].callback as Action<T, T2, T3>)?.Invoke(a, b, c);
            }

            public void Dispatch<T, T2, T3, T4>(T a, T2 b, T3 c, T4 d)
            {
                if (type2CbList.TryGetValue(typeof(Action<T, T2, T3, T4>), out var l))
                    for (int i = l.Count - 1; i >= 0; i--)
                        if (l[i].caller == null || l[i].caller.IsActiveSubscriber) (l[i].callback as Action<T, T2, T3, T4>)?.Invoke(a, b, c, d);
            }

            // --- 取消订阅逻辑 ---
            private void InternalUnsub(Type t, Delegate del)
            {
                if (type2CbList.TryGetValue(t, out var l))
                {
                    for (int i = 0; i < l.Count; i++)
                    {
                        if (l[i].callback == del) { l.RemoveAt(i); break; }
                    }
                }

                foreach (var kv in callerToDelegates)
                {
                    if (kv.Value.Remove(del))
                    {
                        if (kv.Value.Count == 0) callerToDelegates.Remove(kv.Key);
                        break;
                    }
                }
            }

            public void Unsubscribe(Action cb) => InternalUnsub(typeof(Action), cb);
            public void Unsubscribe<T>(Action<T> cb) => InternalUnsub(typeof(Action<T>), cb);
            public void Unsubscribe<T, T2>(Action<T, T2> cb) => InternalUnsub(typeof(Action<T, T2>), cb);
            public void Unsubscribe<T, T2, T3>(Action<T, T2, T3> cb) => InternalUnsub(typeof(Action<T, T2, T3>), cb);
            public void Unsubscribe<T, T2, T3, T4>(Action<T, T2, T3, T4> cb) => InternalUnsub(typeof(Action<T, T2, T3, T4>), cb);

            public void UnsubscribeAll(IMessageSubscriber caller)
            {
                if (!callerToDelegates.TryGetValue(caller, out var dels)) return;
                for (int i = dels.Count - 1; i >= 0; i--) UnSubByType(dels[i]);
                callerToDelegates.Remove(caller);
            }

            private void UnSubByType(Delegate del)
            {
                Type t = del.GetType();
                if (t == typeof(Action)) { Unsubscribe((Action)del); return; }
                if (t.IsGenericType && unSubMethodCache.TryGetValue(t.GetGenericArguments().Length, out var m))
                    m.MakeGenericMethod(t.GetGenericArguments()).Invoke(this, new object[] { del });
            }
        }
    }
}
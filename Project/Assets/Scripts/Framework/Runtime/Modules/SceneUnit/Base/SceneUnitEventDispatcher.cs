using Framework.Runtime.UnitSystem.Base;
using System;
using System.Collections.Generic;

namespace Framework.Runtime.MSceneUnit
{
    // public partial class SceneUnitEventConstant { // 鼠标射线击中的时候发送到事件 public const string
    // ON_CAMERA_RAY_HIT = "ON_CAMERA_RAY_HIT"; }

    public class SceneUnitEvent
    {
        public string dispatcherId;
        public string dispatcherName;
        private Dictionary<string, object> m_DataUpload;
        public SceneUnitEvent(SceneUnit dispatcher, string eventName, object arg)
        {
            this.dispatcher = dispatcher;
            this.eventName = eventName;
            this.arg = arg;
            m_DataUpload = new Dictionary<string, object>();
        }

        public object arg { get;  set; }
        public SceneUnit dispatcher { get; private set; }
        public string eventName { get;  set; }

        public object GetData(string name)
        {
            return m_DataUpload.ContainsKey(name) ? m_DataUpload[name] : null;
        }

        public bool HasData(string name)
        {
            return m_DataUpload.ContainsKey(name);
        }

        public void RemoveData(string name)
        {
            if (m_DataUpload.ContainsKey(name)) m_DataUpload.Remove(name);
        }

        public void SetData(string name, object data)
        {
            if (m_DataUpload.ContainsKey(name)) m_DataUpload[name] = data;
            else
                m_DataUpload.Add(name, data);
        }
    }

    public class SceneUnitEventDispatcher : BehaviourUnit
    {
        private Dictionary<SceneUnit, EventUpload> sceneUnitToEventUpload;

        public SceneUnitEventDispatcher()
        {
            sceneUnitToEventUpload = new Dictionary<SceneUnit, EventUpload>();
        }

        // 添加C#事件监听器
        public void AddEventListener(SceneUnit sceneUnit, string eventName, Action<SceneUnitEvent> listener)
        {
            var eventUpload = GetOrCreateEventUpload(sceneUnit);

            if (eventUpload.cEventDict.TryGetValue(eventName, out var action))
            {
                action += listener;
                eventUpload.cEventDict[eventName] = action;
            }
            else
            {
                eventUpload.cEventDict.Add(eventName, listener);
            }
        }

        // 清理无效的SceneUnit引用（防止内存泄漏）
        public void CleanupInvalidReferences()
        {
            var keysToRemove = new List<SceneUnit>();

            foreach (var kvp in sceneUnitToEventUpload)
            {
                if (kvp.Key == null) // SceneUnit已被销毁
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                sceneUnitToEventUpload.Remove(key);
            }
        }

        // 分发事件
        public void DispatchEvent(SceneUnit dispatcher, SceneUnitEvent evt)
        {
            if (dispatcher == null) return;

            if (sceneUnitToEventUpload.TryGetValue(dispatcher, out var eventUpload))
            {
                // 发布C#事件
                if (eventUpload.cEventDict.TryGetValue(evt.eventName, out var action))
                {
                    action?.Invoke(evt);
                }
            }
        }

        // 检查SceneUnit是否注册了某个事件
        public bool HasSceneUnitRegister(SceneUnit sceneUnit, string eventName)
        {
            if (sceneUnitToEventUpload.TryGetValue(sceneUnit, out var eventUpload))
            {
                return eventUpload.cEventDict.ContainsKey(eventName);
            }
            return false;
        }

        // 移除特定事件的所有C#监听器
        public void RemoveAllCSEventListeners(SceneUnit sceneUnit, string eventName)
        {
            if (sceneUnitToEventUpload.TryGetValue(sceneUnit, out var eventUpload))
            {
                if (eventUpload.cEventDict.ContainsKey(eventName))
                {
                    eventUpload.cEventDict.Remove(eventName);
                }
            }
        }

        // 移除SceneUnit的所有事件监听器
        public void RemoveAllEventListeners(SceneUnit sceneUnit)
        {
            if (sceneUnitToEventUpload.ContainsKey(sceneUnit))
            {
                sceneUnitToEventUpload.Remove(sceneUnit);
            }
        }

        // 移除特定C#事件监听器
        public void RemoveEventListener(SceneUnit sceneUnit, string eventName, Action<SceneUnitEvent> listener)
        {
            if (sceneUnitToEventUpload.TryGetValue(sceneUnit, out var eventUpload))
            {
                if (eventUpload.cEventDict.TryGetValue(eventName, out var action))
                {
                    action -= listener;
                    if (action == null)
                    {
                        eventUpload.cEventDict.Remove(eventName);
                    }
                    else
                    {
                        eventUpload.cEventDict[eventName] = action;
                    }
                }
            }
        }

        // 获取或创建SceneUnit对应的事件上传器
        private EventUpload GetOrCreateEventUpload(SceneUnit sceneUnit)
        {
            if (!sceneUnitToEventUpload.TryGetValue(sceneUnit, out var eventUpload))
            {
                eventUpload = new EventUpload();
                sceneUnitToEventUpload[sceneUnit] = eventUpload;
            }
            return eventUpload;
        }

        private class EventUpload
        {
            public Dictionary<string, Action<SceneUnitEvent>> cEventDict;

            public EventUpload()
            {
                cEventDict = new Dictionary<string, Action<SceneUnitEvent>>();
            }
        }
    }
}
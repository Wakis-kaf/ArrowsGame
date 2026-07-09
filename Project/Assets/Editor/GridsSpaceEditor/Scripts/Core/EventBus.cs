using System;
using System.Collections.Generic;
using GridsSpaceEditor.Data.Models;
using UnityEngine;

namespace GridsSpaceEditor.Core
{
    public static class EditorEventBus
    {
        private static readonly Dictionary<Type, Delegate> s_EventListeners = new Dictionary<Type, Delegate>();

        public static void Subscribe<T>(Action<T> handler) where T : class
        {
            var type = typeof(T);
            if (s_EventListeners.ContainsKey(type))
                s_EventListeners[type] = Delegate.Combine(s_EventListeners[type], handler);
            else
                s_EventListeners[type] = handler;
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : class
        {
            var type = typeof(T);
            if (s_EventListeners.ContainsKey(type))
                s_EventListeners[type] = Delegate.Remove(s_EventListeners[type], handler);
        }

        public static void Publish<T>(T eventData) where T : class
        {
            var type = typeof(T);
            if (s_EventListeners.TryGetValue(type, out var handler))
                handler?.DynamicInvoke(eventData);
        }

        public static void Clear()
        {
            s_EventListeners.Clear();
        }
    }

    #region 事件定义

    public class CellDataChangedEvent
    {
        public List<GridCellData> Cells;
    }

    public class SelectionChangedEvent
    {
        public HashSet<Vector2Int> SelectedCoords;
    }

    public class PortChangedEvent
    {
        public Vector2Int CellCoord;
        public PortInstance Port;
        public string ChangeType;
    }

    public class SystemDataChangedEvent
    {
        public SystemData Data;
    }

    public class ViewportChangedEvent
    {
        public float Zoom;
        public Vector2 PanOffset;
    }

    #endregion
}

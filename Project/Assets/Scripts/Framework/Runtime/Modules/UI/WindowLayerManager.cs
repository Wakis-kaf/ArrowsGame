using Framework.Runtime.LogSystem;
using Framework.Runtime.UnitSystem.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework.Runtime.UI
{
    public class WindowLayerManager : BehaviourUnit
    {
        public static WindowLayerManager Instance => UIWindow.Ins.WindowLayerManager;
        public const int LAYER_SPACE = 1000;
        private Dictionary<IDisplayUnit, int> m_DisplayUnitLocLayer;
        private Dictionary<IDisplayUnit, RectTransform> m_DisplayUnitRTCache;
        private Dictionary<int, WindowLayer> m_LayerManagersMap;

        public WindowLayerManager()
        {
            m_LayerManagersMap = new Dictionary<int, WindowLayer>(64);
            m_DisplayUnitLocLayer = new Dictionary<IDisplayUnit, int>(128);
            m_DisplayUnitRTCache = new Dictionary<IDisplayUnit, RectTransform>(128);
            InitAllSystemLayer();
        }

        public void ClearAllPanel(int layerIndex)
        {
            WindowLayer layer = GetLayer(layerIndex);
            if (layer == null)
            {
                Log.Error($"Layer Not Found! {layerIndex}");
                return;
            }

            layer.CloseAllPanel();
        }

        public void CloseWindowPanel(IDisplayUnit displayUnit)
        {
            int layerIndex = GetLayer(displayUnit);
            if (layerIndex == -1)
            {
                displayUnit.Hide();
                return;
            }

            WindowLayer windowLayer = GetLayer(layerIndex);
            windowLayer.CloseWindowPanel(displayUnit);
        }

        public bool DestroyWindow(IDisplayUnit displayUnit)
        {
            int layerIndex = GetLayer(displayUnit);
            if (layerIndex == -1)
            {
                return false;
            }
            WindowLayer windowLayer = GetLayer(layerIndex);
            bool res = windowLayer.DestroyWindow(displayUnit);
            if (res)
            {
                if (m_DisplayUnitRTCache.ContainsKey(displayUnit))
                {
                    GameObject.Destroy(m_DisplayUnitRTCache[displayUnit].gameObject);
                }
                m_DisplayUnitRTCache.Remove(displayUnit);
                m_DisplayUnitLocLayer.Remove(displayUnit);
            }
            return res;
        }

        public RectTransform FindDisplayUnitRT(IDisplayUnit displayUnit)
        {
            if (!m_DisplayUnitRTCache.ContainsKey(displayUnit))
            {
                if (displayUnit.DisplayGO == null)
                {
                    return null;
                }

                m_DisplayUnitRTCache.Add(displayUnit, displayUnit.DisplayGO.GetComponent<RectTransform>());
            }

            return m_DisplayUnitRTCache[displayUnit]??null ;
        }
        public WindowLayer FindLayer(int layer)
        {
            // 需要判断当前Layer 是否存在
            if (m_LayerManagersMap.ContainsKey(layer))
                return m_LayerManagersMap[layer];
            return null;
        }
        public WindowLayer GetLayer(int layer)
        {
            // 需要判断当前Layer 是否存在
            if (!m_LayerManagersMap.ContainsKey(layer))
                m_LayerManagersMap.Add(layer, CreateLayer(layer));
            return m_LayerManagersMap[layer];
        }

        public int GetSortingOrder(int layer)
        {
            return layer * LAYER_SPACE;
        }

        public void PopupLoadingWindow(IDisplayUnit displayUnit, int layer)
        {
            WindowLayer windowLayer = GetLayer(layer);
            windowLayer.AddToLoadingQueue(displayUnit);
            
        }

        public void PopupWindow(IDisplayUnit displayUnit)
        {
            int layerIndex = GetLayer(displayUnit);
            if (layerIndex == -1)
            {
                Log.Warning("图层为空");
                displayUnit.Hide();
                return;
            }

            WindowLayer windowLayer = GetLayer(layerIndex);
            windowLayer.CloseWindow(displayUnit);
        }
        public bool IsInLayerTop(IDisplayUnit displayUnit, int layer)
        {
            WindowLayer windowLayer = GetLayer(layer);
            if (windowLayer == null) return false;

            int index = windowLayer.GetIndex(displayUnit);
            if(index == -1)
            {
                return false;
            }
            return index == windowLayer.ShowCount - 1 && !windowLayer.HasLoading();

        }
        public void PopWindow(IDisplayUnit displayUnit, int layer)
        {
            WindowLayer windowLayer = GetLayer(layer);
            SwitchLayer(displayUnit, layer);
            windowLayer.OpenWindow(displayUnit);
        }

        public void PopWindowPanel(IDisplayUnit displayUnit, int layer)
        {
            WindowLayer windowLayer = GetLayer(layer);
            SwitchLayer(displayUnit, layer);
            windowLayer.OpenWindowPanel(displayUnit);
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            for (int i = 0; i < m_LayerManagersMap.Keys.Count; i++)
            {
                var item = m_LayerManagersMap[m_LayerManagersMap.Keys.ElementAt(i)];
                item.Clear();
            }
            m_LayerManagersMap.Clear();
            m_DisplayUnitLocLayer.Clear();
            m_DisplayUnitRTCache.Clear();
        }

        /// <summary>
        /// 创建Layer
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private WindowLayer CreateLayer(int layer, string name = "")
        {
            Transform root = UIRoot.RootTransform;
            layer = Mathf.Max(layer, 0);
            string layerName = string.IsNullOrEmpty(name) ? "WindowLayer" + layer : name;
            GameObject newLayerObj = new GameObject(layerName);
            Vector3 scale = newLayerObj.transform.localScale;
            newLayerObj.transform.SetParent(root, false);
            WindowLayer windowLayer = newLayerObj.AddComponent<WindowLayer>();
            windowLayer.CurLayer = layer;
            windowLayer.gameObject.layer = 5;
            windowLayer.RectTransform.SetAnchor(AnchorPresets.StretchAll);
            windowLayer.RectTransform.SetPivot(PivotPresets.MiddleCenter);
            windowLayer.RectTransform.SetBottomLeft(Vector2.zero);
            windowLayer.RectTransform.SetTopRight(Vector2.zero);
            windowLayer.RectTransform.anchoredPosition3D = new Vector3(0, 0, -layer * LAYER_SPACE);
            windowLayer.RectTransform.localScale = scale;
            return windowLayer;
        }

        private int GetLayer(IDisplayUnit displayUnit)
        {
            if (m_DisplayUnitLocLayer.TryGetValue(displayUnit, out int layer))
                return layer;
            return -1;
        }

        private void InitAllSystemLayer()
        {
            foreach (var kvp in UILayerMap.CanvasLayerMap)
            {
                GetLayer(kvp.Key).gameObject.name = kvp.Value;
            }
        }

        private void SwitchLayer(IDisplayUnit displayUnit, int newLayer)
        {
            int oldLayer = -2;
            if (m_DisplayUnitLocLayer.ContainsKey(displayUnit))
            {
                oldLayer = m_DisplayUnitLocLayer[displayUnit];
            }
            else
            {
                m_DisplayUnitLocLayer.Add(displayUnit, newLayer);
            }

            if (oldLayer != newLayer)
            {
                // 从旧层中移除
                if (oldLayer != -2)
                {
                    GetLayer(oldLayer).RemoveChild(displayUnit);
                    displayUnit.OnRemoveFromLayer(oldLayer);
                }
                GetLayer(newLayer).AddChild(displayUnit);
                displayUnit.CurLayer = newLayer;
                m_DisplayUnitLocLayer[displayUnit] = newLayer;
            }
        }

        
    }
}
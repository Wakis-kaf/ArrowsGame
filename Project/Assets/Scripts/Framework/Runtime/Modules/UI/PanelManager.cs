using Framework.Runtime.Config;
using Framework.Runtime.LogSystem;
using Framework.Runtime.UnitSystem.Base;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.UI
{
    /// <summary>
    /// Panel Manager 负责和UIRoot 对游戏中的所有面板进行管理 管理UI 层级以及层级下的所有视图顺序
    /// </summary>
    public class PanelManager : BehaviourUnit
    {
        public static PanelManager Ins => GameApp.UIModule.UIWindow.PanelManager;
        private List<Panel> m_LoadingPanelList;
        private List<UPanel> m_PanelList;
        private Dictionary<UPanel, Panel> m_Upanel2PanelDict;

        public PanelManager()
        {
            m_PanelList = new List<UPanel>(128);
            m_LoadingPanelList = new List<Panel>(128);
            m_Upanel2PanelDict = new Dictionary<UPanel, Panel>(128);
        }

        public void CloseAllPanel()
        {
            for (int i = 0; i < m_LoadingPanelList.Count; i++)
            {
                ClosePanel(m_LoadingPanelList[i]);
            }
            for (int i = 0; i < m_PanelList.Count; i++)
            {
                ClosePanel(m_PanelList[i]);
            }
        }

        public void CloseAllPanel(int layerIndex)
        {
            GameApp.UIModule.UIWindow.WindowLayerManager.ClearAllPanel(layerIndex);
        }

        public void CloseAllUIPanel()
        {
            CloseAllPanel(GlobalConstant.LAYER_PANEL);
        }
        public void CloseAllHighUIPanel()
        {
            CloseAllPanel(GlobalConstant.LAYER_HIGH_PANEL);
        }

        public void ClosePanel(UPanel panel)
        {
            if (!m_Upanel2PanelDict.ContainsKey(panel))
            {
                panel.gameObject.SetActive(false);
                return;
            }

            ClosePanel(m_Upanel2PanelDict[panel]);
        }

        public void ClosePanel(Panel displayUnit)
        {
            if (!HasPanel(displayUnit))
            {
                UIAgent.Error($"panel not registered! please make sure this panel was created by method OpenPanel!{displayUnit.Type.Name}");
            }
            GameApp.UIModule.UIWindow.WindowLayerManager.CloseWindowPanel(displayUnit);
        }

        /// <summary>
        /// 创建面板，和打开面板区别是，打开面板会先从缓存中查找， 而打开面板每次都创建一个新的面板
        /// </summary>
        /// <param name="displayUnitType"></param>
        /// <param name="prefabPath"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public Panel CreatePanel(Type displayUnitType, string prefabPath, int layer = GlobalConstant.LAYER_PANEL)
        {
            // 获取displayUnit
            IDisplayUnit displayUnit = UIWindow.Ins.GetDisplayUnitAsync(displayUnitType, prefabPath,
                (unit) =>
                {
                    try
                    {
                        layer = unit.GetOpenLayer(layer);
                        // 记录面板
                        CachePanel(unit as Panel);
                        
                        // 加载完成之后调用回调
                        if (unit.IsShow)
                        {
                            GameApp.UIModule.UIWindow.WindowLayerManager.PopWindowPanel(unit, layer);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error("CreatePanel出现错误" + e.Message + " " + e.StackTrace);
                    }
                });
            if (displayUnit.DisplayGO == null)
            {
                m_LoadingPanelList.Add(displayUnit as Panel);
            }

            if (displayUnit == null)
            {
                UIAgent.Error("displayunit null error!");
                return null;
            }
            if (UIWindow.Ins.IsLoading(displayUnit))
            {
                // 如果加载中,也需要打开,但是不是立即打开
                layer = displayUnit.GetOpenLayer(layer);
                GameApp.UIModule.UIWindow.WindowLayerManager.PopupLoadingWindow(displayUnit, layer);
            }
            displayUnit.Show();
            return displayUnit as Panel;
        }

        public T CreatePanel<T>(string prefabPath, int layer = GlobalConstant.LAYER_PANEL) where T : Panel
        {
            return CreatePanel(typeof(T), prefabPath, layer) as T;
        }

        public Panel CreatePanelSync(Type displayUnitType, string prefabPath, int layer)
        {
            // 获取displayUnit
            IDisplayUnit displayUnit = UIWindow.Ins.GetDisplayUnitSync(displayUnitType, prefabPath);
            layer = displayUnit.GetOpenLayer(layer);
            // 记录面板
            CachePanel(displayUnit as Panel);
            // 加载完成之后调用回调
            GameApp.UIModule.UIWindow.WindowLayerManager.PopWindowPanel(displayUnit, layer);
            if (displayUnit == null)
            {
                UIAgent.Error("displayunit null error!");
                return null;
            }

            if (UIWindow.Ins.IsLoading(displayUnit))
            {
                // 如果加载中,也需要打开,但是不是立即打开
                GameApp.UIModule.UIWindow.WindowLayerManager.PopupLoadingWindow(displayUnit, layer);
            }

            displayUnit.Show();
            return displayUnit as Panel;
        }


        public T FindPanel<T>() where T : Panel
        {
            return FindPanel(typeof(T)) as T;
        }
        public T FindPanelByName<T>(string name) where T : Panel
        {
            return FindPanelByTypeName(name) as T;
        }
        public Panel FindPanelByTypeName(string typeName)
        {
            for (int i = 0; i < m_LoadingPanelList.Count; i++)
            {
                if (m_LoadingPanelList[i].GetType().Name == typeName)
                {
                    return m_LoadingPanelList[i];
                }
            }
            for (int i = 0; i < m_PanelList.Count; i++)
            {
                if (m_Upanel2PanelDict[m_PanelList[i]].GetType().Name == typeName)
                    return m_Upanel2PanelDict[m_PanelList[i]];
            }

            return null;
        }
        public Panel FindPanel(Type type)
        {
            for (int i = 0; i < m_LoadingPanelList.Count; i++)
            {
                if (m_LoadingPanelList[i].GetType() == type)
                {
                    return m_LoadingPanelList[i];
                }
            }
            for (int i = 0; i < m_PanelList.Count; i++)
            {
                if (m_Upanel2PanelDict[m_PanelList[i]].GetType() == type)
                    return m_Upanel2PanelDict[m_PanelList[i]];
            }

            return null;
        }

        public bool HasPanel(Panel panel)
        {
            for (int i = 0; i < m_LoadingPanelList.Count; i++)
            {
                if (m_LoadingPanelList[i] == panel)
                {
                    return true;
                }
            }
            for (int i = 0; i < m_PanelList.Count; i++)
            {
                if (m_Upanel2PanelDict[m_PanelList[i]] == panel)
                    return true;
            }

            return false;
        }

        public T OpenPanel<T>(string prefabPath ="", int layer = GlobalConstant.LAYER_PANEL) where T : Panel
        {
            return OpenPanel(typeof(T), prefabPath, layer) as T;
        }

        public Panel OpenPanel(Type displayUnitType, string prefabPath, int layer = GlobalConstant.LAYER_PANEL)
        {
            // 先从缓存中查找是否有该panel 先在打开的面板中查找是否打开过该面板
            IDisplayUnit panelUnit = FindPanel(displayUnitType);
            if (panelUnit != null)
            {
                return OpenPanel(panelUnit, layer);
            }

            var panel = CreatePanel(displayUnitType, prefabPath, layer);
            return panel;
        }
        public void ClosePanel<T>() where T : Panel
        {
             ClosePanel(typeof(T));
        }
        public void ClosePanel(Type type)
        {
            IDisplayUnit panelUnit = FindPanel(type);
            if (panelUnit is Panel panel)
            {
                 ClosePanel(panel);
            }
            else if(panelUnit!=null)
            {
                Log.Error($"{type.Name}类型面板不存在，关闭失败!");
            }

        }

        public Panel OpenPanel(IDisplayUnit displayUnit, int layer)
        {
            layer = displayUnit.GetOpenLayer(layer);
            if (displayUnit != null && UIWindow.Ins.IsLoading(displayUnit))
            {
                // 如果加载中,也需要打开,但是不是立即打开
                GameApp.UIModule.UIWindow.WindowLayerManager.PopupLoadingWindow(displayUnit, layer);
            }
            else if (!UIWindow.Ins.IsLoading(displayUnit))
            {
                GameApp.UIModule.UIWindow.WindowLayerManager.PopWindowPanel(displayUnit, layer);
                return displayUnit as Panel;
            }
            return displayUnit as Panel;
        }

        public T OpenPanelSync<T>(string prefabPath, int layer) where T : Panel
        {
            return OpenPanelSync(typeof(T), prefabPath, layer) as T;
        }

        public Panel OpenPanelSync(Type displayUnitType, string prefabPath, int layer)
        {
            IDisplayUnit panelUnit = FindPanel(displayUnitType);
            if (panelUnit != null)
            {
                return OpenPanel(panelUnit, layer);
            }

            var panel = CreatePanelSync(displayUnitType, prefabPath, layer);
            return panel;
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            m_PanelList.Clear();
            m_Upanel2PanelDict.Clear();
            m_LoadingPanelList.Clear();
        }

        private void CachePanel(Panel displayUnit)
        {
            UPanel panel = displayUnit.DisplayGO.GetComponent<UPanel>();
            m_PanelList.Add(panel);
            if (!m_Upanel2PanelDict.ContainsKey(panel))
                m_Upanel2PanelDict.Add(panel, displayUnit);
            m_LoadingPanelList.Remove(displayUnit);
        }
        public void DestroyPanel(Panel panel)
        {
            if (UIWindow.Ins.DestroyWindow(panel))
            {
                RemovePanelCache(panel);
            }
        }
        private void RemovePanelCache(Panel panel)
        {
            UPanel uPanel = panel.UIPanel;
            m_PanelList.Remove(uPanel);
            if (m_Upanel2PanelDict.ContainsKey(uPanel))
            {
                m_Upanel2PanelDict.Remove(uPanel);
            }
            m_LoadingPanelList.Remove(panel);
        }
            
    }
}
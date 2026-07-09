using Sirenix.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Runtime.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class WindowLayer : MonoBehaviour
    {
        public const int PANEL_LAYER_STEP = 100;
        public const int PANEL_MASK_GAP = 20;
        public const float VIEW_SPACE = -200;
        private Canvas m_Canvas;
        private RectTransform m_CloseRootTransform;
        private int m_CurLayer = -1;
        private Dictionary<IDisplayUnit, float> m_DisplayUnitCloseTimeMap = new Dictionary<IDisplayUnit, float>(128);
        private List<IDisplayUnit> m_HideStack = new List<IDisplayUnit>(128);
        private RectTransform m_MaskRootTransform;
        private RectTransform m_OpenRootTransform;
        private Dictionary<UPanel, UImage> m_Panel2BgMaskDict = new Dictionary<UPanel, UImage>(128);
        private Dictionary<IDisplayUnit, int> m_PrefabLoading2IndexDict = new Dictionary<IDisplayUnit, int>(128);
        private RectTransform m_RectTransform;
        private List<IDisplayUnit> m_ShowStack = new List<IDisplayUnit>(128);
        private Transform m_Transform;
        public int ShowCount => m_ShowStack.Count;
        public Canvas Canvas
        {
            get
            {
                if (m_Canvas == null)
                {
                    m_Canvas = GetComponent<Canvas>();
                    m_Canvas.overrideSorting = true;
                }

                return m_Canvas;
            }
        }
        public bool HasLoading()
        {
            return m_PrefabLoading2IndexDict.Count > 0;
        }

        public RectTransform CloseRoot
        {
            get
            {
                if (m_CloseRootTransform == null)
                {
                    m_CloseRootTransform = new GameObject("CloseRoot").AddComponent<RectTransform>();
                    m_CloseRootTransform.SetParent(Transform, false);
                    m_CloseRootTransform.SetAnchor(AnchorPresets.StretchAll);
                    m_CloseRootTransform.SetOffsetZero();
                }

                return m_CloseRootTransform;
            }
        }

        public int CurLayer
        {
            get { return m_CurLayer; }
            set
            {
                if (value != m_CurLayer)
                {
                    m_CurLayer = value;
                    Canvas.sortingOrder = m_CurLayer * WindowLayerManager.LAYER_SPACE;
                }
            }
        }

        public RectTransform MaskRoot
        {
            get
            {
                if (m_MaskRootTransform == null)
                {
                    m_MaskRootTransform = new GameObject("MaskRoot").AddComponent<RectTransform>();
                    m_MaskRootTransform.SetParent(Transform, false);
                    m_MaskRootTransform.SetAnchor(AnchorPresets.StretchAll);
                    m_MaskRootTransform.SetOffsetZero();
                    m_MaskRootTransform.SetAsFirstSibling();
                }

                return m_MaskRootTransform;
            }
        }

        public RectTransform OpenRoot
        {
            get
            {
                if (m_OpenRootTransform == null)
                {
                    m_OpenRootTransform = new GameObject("OpenRoot").AddComponent<RectTransform>();
                    m_OpenRootTransform.SetParent(Transform, false);
                    m_OpenRootTransform.SetAnchor(AnchorPresets.StretchAll);
                    m_OpenRootTransform.SetOffsetZero();
                }

                return m_OpenRootTransform;
            }
        }

        public RectTransform RectTransform
        {
            get
            {
                if (m_RectTransform == null)
                    m_RectTransform = GetComponent<RectTransform>();
                return m_RectTransform;
            }
        }

        public int SortingOrder => m_CurLayer * WindowLayerManager.LAYER_SPACE;

        public Transform Transform
        {
            get
            {
                if (m_Transform == null)
                    m_Transform = transform;
                return m_Transform;
            }
        }

        public void AddChild(IDisplayUnit displayUnit)
        {
            DisplayUnitOpenReset(displayUnit);
        }
        
        public void AddToLoadingQueue(IDisplayUnit displayUnit)
        {
            m_ShowStack.Remove(displayUnit);
            if (m_PrefabLoading2IndexDict.ContainsKey(displayUnit))
            {
                m_PrefabLoading2IndexDict[displayUnit] = m_ShowStack.Count - 1;
            }
            else
            {
                m_PrefabLoading2IndexDict.Add(displayUnit, m_ShowStack.Count - 1);
            }
            displayUnit.Show(); // 也需要编辑为show
        }

        public void Clear()
        {
            m_DisplayUnitCloseTimeMap.Clear();
            m_ShowStack.Clear();
            m_HideStack.Clear();
            m_PrefabLoading2IndexDict.Clear();
            m_Panel2BgMaskDict.Clear();
        }

        public void CloseAllPanel()
        {
            for (int i = m_ShowStack.Count - 1; i >= 0; i--)
            {
                CloseWindowPanel(m_ShowStack[i]);
            }
            foreach (var loadingItem in m_PrefabLoading2IndexDict)
            {
                var displayUnit = loadingItem.Key;
                if (displayUnit.IsShow)
                {
                    displayUnit.Hide();
                }
            }
        }

        private  bool IsInHide(IDisplayUnit displayUnit)
        {
            return m_HideStack.Contains(displayUnit);
        }
        public void CloseWindow(IDisplayUnit displayUnit)
        {
            if (IsInHide(displayUnit)) return;
            DisplayUnitCloseReset(displayUnit);
            displayUnit.Hide();
        }

        public void CloseWindowPanel(IDisplayUnit displayUnit)
        {
            UPanel panel = displayUnit.DisplayGO.GetComponent<UPanel>();
            if (panel == null)
            {
                displayUnit.Hide();
                return;
            }
            //panel.HideBgMask();
            CloseWindow(displayUnit);
        }

        public bool DestroyWindow(IDisplayUnit displayUnit)
        {
            m_DisplayUnitCloseTimeMap.Remove(displayUnit);
            m_ShowStack.Remove(displayUnit);
            m_HideStack.Remove(displayUnit);
            m_PrefabLoading2IndexDict.Remove(displayUnit);
            if(displayUnit is Panel panel)
            {
                if (m_Panel2BgMaskDict.ContainsKey(panel.UIPanel))
                {
                    Destroy(m_Panel2BgMaskDict[panel.UIPanel].gameObject);
                    m_Panel2BgMaskDict.Remove(panel.UIPanel);
                }
            }
            return true;
        }

        public UImage GetPanelBgMask(IDisplayUnit displayUnit,UPanel panel, int layer)
        {
            UImage bgMask;
            if (m_Panel2BgMaskDict.ContainsKey(panel))
            {
                bgMask = m_Panel2BgMaskDict[panel];
            }
            else
            {
                if(displayUnit is Panel panelDisplayUnit)
                {
                    bgMask= panelDisplayUnit.CreatePanelBgMask(MaskRoot, panel.Canvas != null);
                }
                else
                {
                    bgMask = CreatePanelBgMask(panel.gameObject.name, panel.Canvas != null);
                }
                m_Panel2BgMaskDict.Add(panel, bgMask);
            }
            RectTransform maskRt = bgMask.rectTransform;
            RectTransform panelRt = panel.transform as RectTransform;
            Vector3 pos = panelRt.position;
            maskRt.position = pos;
            Vector3 apos = panelRt.anchoredPosition3D;
            apos.z += PANEL_MASK_GAP;
            maskRt.anchoredPosition3D = apos;
            if (panel.Canvas != null)
            {
                Canvas canvas = bgMask.GetComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = panel.SortOrder - 1;
            }
            return bgMask;
        }

        public void OpenWindow(IDisplayUnit displayUnit)
        {
            DisplayUnitOpenReset(displayUnit);
            displayUnit.OnOpenInLayer(CurLayer);
            displayUnit.Show();
        }

        public void OpenWindowPanel(IDisplayUnit displayUnit)
        {
            DisplayUnitOpenReset(displayUnit);
            // 需要重设
            UPanel panel = displayUnit.DisplayGO.GetOrAddComponent<UPanel>();
            // 设置重设
            panel.SortOrder = displayUnit.SortOrder;
            // 显示面板遮罩
            panel.BgMask = GetPanelBgMask(displayUnit,panel, displayUnit.SortOrder);
            displayUnit.OnOpenInLayer(CurLayer);
            displayUnit.Show();
        }

        public void RemoveChild(IDisplayUnit displayUnit)
        {
            m_DisplayUnitCloseTimeMap.Remove(displayUnit);
            m_ShowStack.Remove(displayUnit);
            m_HideStack.Remove(displayUnit);
        }

        private UImage CreatePanelBgMask(string panelName, bool createMaskCanvas = false)
        {
            GameObject markGo = new GameObject($"BgMask[{panelName}]");
            UImage img = markGo.AddComponent<UImage>();
            if (createMaskCanvas)
            {
                Canvas canvas = markGo.AddComponent<Canvas>();
                markGo.GetOrAddComponent<GraphicRaycaster>();
                canvas.overrideSorting = true;
            }
            RectTransform markRT = markGo.GetOrAddComponent<RectTransform>();
            markGo.SetActive(true);
            markGo.transform.SetParent(MaskRoot, false);
            markRT.SetAnchor(AnchorPresets.StretchAll);
            markRT.SetOffsetZero();
            markGo.layer = 5;
            // 设置默认贴图
            img.type = Image.Type.Sliced;
            img.color = new Color(0, 0, 0, 0.85f);
            return img;
        }

        private void DisplayUnitCloseReset(IDisplayUnit displayUnit)
        {
            if (m_DisplayUnitCloseTimeMap.ContainsKey(displayUnit))
            {
                m_DisplayUnitCloseTimeMap[displayUnit] = Time.time;
            }
            else
            {
                m_DisplayUnitCloseTimeMap.Add(displayUnit, Time.time);
            }
            var rt = WindowLayerManager.Instance.FindDisplayUnitRT(displayUnit);
            m_ShowStack.Remove(displayUnit);
            m_HideStack.Add(displayUnit);
            if (displayUnit.IsActiveChangeParent)
            {
                rt.SetParent(CloseRoot, false);
            }
        }

        private void DisplayUnitOpenReset(IDisplayUnit displayUnit)
        {
            int index = m_ShowStack.IndexOf(displayUnit);
            if (index == -1)
            {
                m_ShowStack.Add(displayUnit);
                m_HideStack.Remove(displayUnit);
                m_DisplayUnitCloseTimeMap.Remove(displayUnit);
                if (IsLoadingDisplayUnit(displayUnit))
                {
                    m_PrefabLoading2IndexDict.Remove(displayUnit);
                }
            }
            ResetDisplaySort();
        }

        // 回收显示该对象的所有内存空间
        private void DisposeDisplayUnit(IDisplayUnit displayUnit)
        {
        }

        public int GetIndex(IDisplayUnit displayUnit)
        {
            int index = -1;
            if (m_PrefabLoading2IndexDict.ContainsKey(displayUnit))
                index = m_PrefabLoading2IndexDict[displayUnit];
            else
            {
                index = m_ShowStack.IndexOf(displayUnit);
            }

            return index;
        }

        private bool IsLoadingDisplayUnit(IDisplayUnit displayUnit)
        {
            return m_PrefabLoading2IndexDict.ContainsKey(displayUnit);
        }
        public int GetShowIndex(IDisplayUnit displayUnit)
        {
            return GetIndex(displayUnit);
        }
        private void SortDisplayUnitByPriority()
        {
            // 手动稳定排序：冒泡排序 (优先级从小到大)
            for (int i = 0; i < m_ShowStack.Count - 1; i++)
            {
                for (int j = 0; j < m_ShowStack.Count - 1 - i; j++)
                {
                    if (m_ShowStack[j].GetVisiblePriority() > m_ShowStack[j + 1].GetVisiblePriority())
                    {
                        var temp = m_ShowStack[j];
                        m_ShowStack[j] = m_ShowStack[j + 1];
                        m_ShowStack[j + 1] = temp;
                    }
                }
            }
        }
        private void ResetDisplaySort()
        {
            SortDisplayUnitByPriority();
            for (int index = 0; index < m_ShowStack.Count; index++)
            {
                var displayUnit = m_ShowStack[index];
                var rt = WindowLayerManager.Instance.FindDisplayUnitRT(displayUnit);
                Vector3 anchorPos = rt.anchoredPosition3D;
                Vector3 scale = rt.localScale;
                Vector2 offsetMax = rt.offsetMax;
                Vector2 offsetMin = rt.offsetMin;
                if (displayUnit.IsGap)
                    anchorPos.z = index * VIEW_SPACE;
                else
                {
                    anchorPos.z = 0;
                }
                if (displayUnit.IsActiveChangeParent)
                {
                    rt.SetParent(OpenRoot, false);
                }
                else
                {
                    rt.SetParent(transform, false);
                }
                    
                rt.offsetMax = offsetMax;
                rt.offsetMin = offsetMin;
                rt.anchoredPosition3D = anchorPos;
                rt.localScale = scale;
                int gapIndex = displayUnit.IsGap ? GetIndex(displayUnit) : 0;
                int sortOrder = (gapIndex + 1) * PANEL_LAYER_STEP + CurLayer * WindowLayerManager.LAYER_SPACE;
                displayUnit.SortOrder = sortOrder;
                displayUnit.CurLayer = CurLayer;
                if (index != -1)
                    rt.SetSiblingIndex(index);
                else
                    rt.SetAsLastSibling();
            }
        }

        private void Update()
        {
            // 定期清理未使用的Panel
            for (int i = m_HideStack.Count; i >= 0; i--)
            {
                if (i >= m_HideStack.Count) continue;

                var displayUnit = m_HideStack[i];
                if (!m_DisplayUnitCloseTimeMap.ContainsKey(displayUnit)) continue;
                
                if (displayUnit.IsAutoDispose)
                {
                    float lastOpenTime = m_DisplayUnitCloseTimeMap[displayUnit];
                    if (displayUnit.AutoDisposeTime > 0 && Time.time - lastOpenTime >= displayUnit.AutoDisposeTime)
                    {
                        displayUnit.Destroy();
                        m_DisplayUnitCloseTimeMap.Remove(displayUnit);
                        m_HideStack.Remove(displayUnit);
                    }
                }
                
            }
        }
    }
}
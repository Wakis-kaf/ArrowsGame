using Framework.Runtime.LogSystem;
using Framework.Runtime.MAsset;
using Framework.Runtime.Memory;
using Framework.Runtime.UnitSystem.Base;
using Framework.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.UI
{
    public class UIWindow : BehaviourUnit
    {
        public static UIWindow Ins => GameApp.UIModule.UIWindow;
        public WindowLayerManager WindowLayerManager { get; private set; }
        public PanelManager PanelManager { get; private set; }
        private struct DisplayUnitGetLambaCb : ILambadaCallback
        {
            public IDisplayUnit displayUnit;
            public Action<IDisplayUnit, IAssetVO> DisplayUnitAssetLoadedReset;
            public int layer;
            public WindowLayerManager windowLayerManager;
            public ILambadaPool Pool { get; set; }

            public void ExecuteAssetLoadedCallback(IAssetVO assetVo)
            {
                DisplayUnitAssetLoadedReset(displayUnit, assetVo);
                Pool?.Put(this);
            }

            public void ExecuteDisplayUnitGetCallback(IDisplayUnit displayUnit, IAssetVO assetVo)
            {
                Pool?.Put(this);
            }

            public void ExecuteUnitPrefabLoadedCallback(IDisplayUnit displayUnitLoaded)
            {
                if (displayUnit.IsShow)
                    windowLayerManager.PopWindow(displayUnitLoaded, layer);
                Pool?.Put(this);
            }

            public void OnGet()
            {
                displayUnit = null;
                DisplayUnitAssetLoadedReset = null;
                windowLayerManager = null;
                layer = -1;
            }

            public void OnPut()
            {
            }
        }

        private LambadaPool<DisplayUnitGetLambaCb> DisplayUnitLambaPool = new LambadaPool<DisplayUnitGetLambaCb>();

        private Dictionary<IDisplayUnit, Action<IDisplayUnit>> m_DisplayObjPrefabLoadedCallback;

        public UIWindow()
        {
            WindowLayerManager = new WindowLayerManager();
            PanelManager = new PanelManager();
            m_DisplayObjPrefabLoadedCallback = new Dictionary<IDisplayUnit, Action<IDisplayUnit>>(128);
        }

        public void Close(IDisplayUnit displayUnit)
        {
            WindowLayerManager.PopupWindow(displayUnit);
        }

        public bool DestroyWindow(IDisplayUnit displayUnit)
        {
            bool res = WindowLayerManager.DestroyWindow(displayUnit);
            if (res)
            {
                displayUnit.IsLoading = false;
                MessageDispatcher.Ins.UnsubscribeAll(displayUnit);
                RemovePrefabLoadedCb(displayUnit);
            }

            return res;
        }
        private bool TryGetPrefabLoadedCb(IDisplayUnit displayUnit,out Action<IDisplayUnit> cb)
        {
            return m_DisplayObjPrefabLoadedCallback.TryGetValue(displayUnit, out cb);
        }
        private void SetPrefabLoadedCb(IDisplayUnit displayUnit, Action<IDisplayUnit> cb)
        {
            if (!m_DisplayObjPrefabLoadedCallback.ContainsKey(displayUnit))
            {
                m_DisplayObjPrefabLoadedCallback.Add(displayUnit, cb);
            }
            else
            {
                m_DisplayObjPrefabLoadedCallback[displayUnit] = cb;
            }
        }
        private void RemovePrefabLoadedCb(IDisplayUnit displayUnit)
        {
            m_DisplayObjPrefabLoadedCallback.Remove(displayUnit);
        }
        public WindowLayer GetWindowLayer(int layerIndex)
        {
            return WindowLayerManager.GetLayer(layerIndex);
        }

        public bool IsLoading(IDisplayUnit displayUnit)
        {
            return displayUnit.IsLoading;
        }

        public T OpenWindow<T>(
            string assetLink,
            int layer = 0,
            Action<IDisplayUnit> prefabLoadedCallBack = null) where T : class, IDisplayUnit
        {
            return OpenWindow(typeof(T), assetLink, layer, prefabLoadedCallBack) as T;
        }

        /// <summary>
        /// 打开面板
        /// </summary>
        public IDisplayUnit OpenWindow(
            Type windowType,
            string assetLink,
            int layer = 0,
            Action<IDisplayUnit> prefabLoadedCallBack = null)
        {
            IDisplayUnit displayUnit = GetDisplayUnitAsync(windowType, assetLink, prefabLoadedCallBack);
            layer = displayUnit.GetOpenLayer(layer);
            return OpenWindow(displayUnit, layer);
        }
        public bool IsInLayerTop(IDisplayUnit displayUnit)
        {
            int layer = displayUnit.CurLayer;
            return WindowLayerManager.IsInLayerTop(displayUnit, layer);
        }
        public IDisplayUnit OpenWindow(IDisplayUnit displayUnit, int layer)
        {
            layer = displayUnit.GetOpenLayer(layer);
            displayUnit.CurLayer = layer;
            if (IsLoading(displayUnit))
            {
                WindowLayerManager.PopupLoadingWindow(displayUnit, layer);
                var item = DisplayUnitLambaPool.Get();
                item.displayUnit = displayUnit;
                item.windowLayerManager = WindowLayerManager;
                item.layer = layer;
                SetPrefabLoadedCb(displayUnit,item.ExecuteUnitPrefabLoadedCallback);
            }
            else
            {

                WindowLayerManager.PopWindow(displayUnit, layer);
            }

            return displayUnit;
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            WindowLayerManager.Dispose();
            WindowLayerManager = null;
            //m_PrefabLoadingMap.Clear();
            m_DisplayObjPrefabLoadedCallback.Clear();
        }

        private IDisplayUnit CreateDisplayUnitInstance(Type displayObjType)
        {
            string errorMsg = string.Empty;
            if (!typeof(IDisplayUnit).IsAssignableFrom(displayObjType))
            {
                errorMsg = $"OpenDisplayUnit Failed {displayObjType} is disallow new() with zero arg";
                UIAgent.Error(errorMsg);
                return null;
            }

            IDisplayUnit displayUnit;
            if (displayObjType.IsSubOrEqualOf(typeof(MonoBehaviour)))
            {
                var obj = new GameObject(displayObjType.ToString());
                displayUnit = (IDisplayUnit)obj.AddComponent(displayObjType);
            }
            else if (displayObjType.IsNewAbleWithArgs(Array.Empty<object>()))
            {
                displayUnit = Utility.ReflectionUtil.CreateInstance(displayObjType) as IDisplayUnit;
            }
            else
            {
                errorMsg = $"OpenDisplayUnit Failed {displayObjType} is disallow new() with zero arg";
                UIAgent.Error(errorMsg);
                return null;
            }

            if (displayUnit == null)
            {
                errorMsg = $"OpenDisplayUnit Failed {displayObjType} is create display obj error";
                UIAgent.Error(errorMsg);
                return null;
            }

            return displayUnit;
        }
        private IDisplayUnit GetDisplayUnitInstance(Type displayObjType) {
            IDisplayUnit displayUnit = CreateDisplayUnitInstance(displayObjType);
            return displayUnit;
        }
        private void OnModelLoadedSetDisplayUnit(IDisplayUnit displayUnit,GameObject go)
        {
            displayUnit.OnUILoaded(go);
            if (TryGetPrefabLoadedCb(displayUnit, out var callback))
            {
                FunctionUtility.SafeCall(callback, displayUnit);
                RemovePrefabLoadedCb(displayUnit);
            }
        }
        public IDisplayUnit CreateDisplayUnitByPrefab(Type displayObjType, GameObject prefab)
        {
            IDisplayUnit displayUnit = GetDisplayUnitInstance(displayObjType);
            IAssetVO assetVo = new AssetVO(prefab);
            DisplayUnitAssetLoadedReset(displayUnit, assetVo);

            return displayUnit;
        }

        private void DisplayUnitAssetLoadedReset(IDisplayUnit displayUnit, IAssetVO assetVo)
        {
            displayUnit.AssetVo = assetVo;
            displayUnit.IsLoading = false;
            var prefab = assetVo.GetAsset() as GameObject;
            var prefabInstance = assetVo.GetInstance(displayUnit.ParentTransform?? UIRoot.RootTransform);
            prefabInstance.name = prefab.name;
            OnModelLoadedSetDisplayUnit(displayUnit, prefabInstance);
        }
     
       
        public T GetDisplayUnitAsync<T>(string assetLink,
                    Action<IDisplayUnit> assetLoadedCallBack = null) where T : IDisplayUnit
        {
            return (T)GetDisplayUnitAsync(typeof(T), assetLink, assetLoadedCallBack);
        }

        public T CreateDisplayunit<T>( string path = "",Transform parentTransform=null) where T :IDisplayUnit
        {

            T displayUnit = UIWindow.Ins.GetDisplayUnitAsync<T>(path);
            displayUnit.ParentTransform = parentTransform;
            displayUnit.Show();
            return displayUnit;

        }
        public IDisplayUnit GetDisplayUnitAsync(
            Type displayObjType, 
            string assetLink,
            Action<IDisplayUnit> displayUnitLoadedCallBack = null)
        {
            IDisplayUnit displayUnit = CreateDisplayUnitInstance(displayObjType);
            assetLink = displayUnit.GetAssetLink(assetLink);
            displayUnit.IsLoading = true;
            var item = DisplayUnitLambaPool.Get();
            item.displayUnit = displayUnit;
            item.DisplayUnitAssetLoadedReset = DisplayUnitAssetLoadedReset;
            displayUnit.AddLoadedCb(displayUnitLoadedCallBack);
            IAssetVO assetVo = UIAgent.LoadAssetAsync(assetLink, item.ExecuteAssetLoadedCallback);
            return displayUnit;
        }
        public T GetDisplayUnitDirect<T>(GameObject go) where T :class, IDisplayUnit
        {
            return GetDisplayUnitDirect(typeof(T),go) as T;
        }
        public IDisplayUnit GetDisplayUnitDirect(Type displayObjType, GameObject go)
        {
            IDisplayUnit displayUnit = CreateDisplayUnitInstance(displayObjType);
            OnModelLoadedSetDisplayUnit(displayUnit, go);
            return displayUnit;
        }

        public IDisplayUnit GetDisplayUnitSync(Type displayObjType, string assetLink)
        {
            IDisplayUnit displayUnit = CreateDisplayUnitInstance(displayObjType);
            assetLink = displayUnit.GetAssetLink(assetLink);
            IAssetVO assetVo = UIAgent.LoadAssetSync(assetLink);
            DisplayUnitAssetLoadedReset(displayUnit, assetVo);
            return displayUnit;
        }

       
    }
}
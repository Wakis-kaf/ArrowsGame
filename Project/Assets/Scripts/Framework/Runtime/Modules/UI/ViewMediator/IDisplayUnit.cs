using Framework.Runtime.Base;
using Framework.Runtime.MAsset;
using System;
using UnityEngine;

namespace Framework.Runtime.UI
{
    /// <summary>
    /// 负责UI 的显示控制
    /// </summary>
    public interface IDisplayUnit : IUnitObject,IMessageSubscriber
    {
        public Transform ParentTransform { get; set; }
        public IAssetVO AssetVo { get; set; }
        public float AutoDisposeTime { get; set; }
        public bool IsAutoDispose { get; set; }
        public bool IsActiveChangeParent { get; set; }
        public string GetAssetLink(string outPath);
        Canvas Canvas { get; }
        CanvasGroup CanvasGroup { get; }
        public int CurLayer { get; set; }
        public int GetVisiblePriority();
        public object Data { get; set; }

        /// <summary>
        /// 实际对象
        /// </summary>
        GameObject DisplayGO { get;  }

        //public object InitOption { get; set; }
        public bool IsGap { get; set; }
        public bool IsLoading { get; set; }
        public bool IsModelLoaded { get; }
        public bool IsUIInited { get; }
        public bool IsShow { get; }
        public int SortOrder { get; set; }
        public int GetOpenLayer(int externalLayer);
       

        void CloseWindow();

        void Destroy();

        void Hide();

        /// <summary>
        /// 当在WindowLayer 中打开的时候调用
        /// </summary>
        /// <param name="layer"></param>
        void OnOpenInLayer(int layer);

        /// <summary>
        /// 当UI加载完的时候调用
        /// </summary>
        /// <param name="uiGameObject"></param>
        void OnUILoaded(GameObject uiGameObject);

        /// <summary>
        /// 从WindowLayer 中移除的时候调用
        /// </summary>
        /// <param name="layer"></param>
        void OnRemoveFromLayer(int layer);

        void OpenWindow();

        /// <summary>
        /// 设置数据源
        /// </summary>
        /// <param name="data"></param>
        void SetData(object data, bool dirtyDataGUIMute = false);

        public void SetSortOrder(int layer);

        void Show();
        void AddLoadedCb(Action<IDisplayUnit> cb);
        void RemoveLoadedCb(Action<IDisplayUnit> cb);
    }
}
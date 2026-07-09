using Framework.Runtime.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.MAsset;
using Framework.Runtime.Config;
using Framework.Runtime;
namespace Game.Modules
{

    public class ArrowClickPointTipWindowData
    {
        public Vector2 showTipScreenPos;
        public float innerScreeRadius;
        public float outerScreeRadius;
        public float fadeInTime = 0.1f;
        public float fadeOutTime = 0.3f;
        public float maxOuterRadiusScale = 1f;
    }

    public class ArrowClickPointTipWindow : View
    {
        #region PrefabBinder 自动引用区域 开始
        private UnityEngine.GameObject arrowClickAnimPointPrefab;
        private UnityEngine.RectTransform rtPointAnimRoot;

        #endregion PrefabBinder 自动引用区域 结束

        protected override void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
        {
            this.arrowClickAnimPointPrefab = prefabBinder.GetObj<UnityEngine.GameObject>("arrowClickAnimPointPrefab");
            this.rtPointAnimRoot = prefabBinder.GetObj<UnityEngine.RectTransform>("rtPointAnimRoot");

        }
        public override int GetOpenLayer(int externalLayer)
        {
            return externalLayer;

        }

        public override string GetAssetLink(string outAssetLink)
        {
            string assetPath = "Assets/AddressableResources/UI/Play/Prefabs/View/ArrowClickPointTipWindow.prefab";
            return AssetPathEncoder.EncodeEnvAssetLink(assetPath, AssetType.PrefabAsset);

        }
        private SimpleObjectPool<ArrowClickAnimPoint> m_ArrowClickAnimPointPool;
        /// <summary>
        /// 子类重写，构造函数中调用
        /// </summary>
        protected override void OnInit()
        {

        }
        /// <summary>
        /// 显示对象初始化UI,当绑定的预制体加载完成后回调(子类重写)
        /// </summary>
        protected override void OnInitUI()
        {
            m_ArrowClickAnimPointPool = new SimpleObjectPool<ArrowClickAnimPoint>();
            m_ArrowClickAnimPointPool.Init(CreateHandler, GetHandler, PutHandler);
        }

        private ArrowClickAnimPoint GetHandler(ArrowClickAnimPoint point, object data)
        {
            point.Show();
            return point;
        }

        private ArrowClickAnimPoint PutHandler(ArrowClickAnimPoint point)
        {
            point.Hide();
            return point;
        }

        private ArrowClickAnimPoint CreateHandler(object data)
        {
            var pointGo = GameObject.Instantiate(arrowClickAnimPointPrefab, rtPointAnimRoot);
            var point = UIWindow.Ins.GetDisplayUnitDirect<ArrowClickAnimPoint>(pointGo);
            return point;
        }

        /// <summary>
        /// 注册页面消息，次于 OnInitUI 之后执行
        /// </summary>
        protected override void OnSubscribeMessages()
        {

        }
        /// <summary>
        /// 当绑定UI加载完成且UI显示回调(子类重写)
        /// </summary>
        protected override void OnShow()
        {

        }
        /// <summary>
        /// 当绑定UI加载完成且UI隐藏回调(子类重写)    
        /// </summary>
        protected override void OnHide()
        {

        }
        /// <summary>
        /// 当绑定UI加载完成且数据更新的时候调用(子类重写)
        /// </summary>
        /// <param name="data"></param>

        protected override void OnGUI(object data)
        {
            while (m_TipAnimQueue.Count > 0)
            {
                ArrowClickPointTipWindowData windowData = m_TipAnimQueue.Dequeue();
                ArrowClickAnimPoint point = m_ArrowClickAnimPointPool.GetObject();
                point.Show();
                point.PlayTipAnim(windowData, OnPointTipAnimOver);
            }
        }

        private void OnPointTipAnimOver(ArrowClickAnimPoint point)
        {
            m_ArrowClickAnimPointPool.PutObject(point);
        }



        /// <summary>
        /// 当绑定UI被删除时回调(子类重写)
        /// </summary>
        public override void OnDestroy()
        {

        }
        private Queue<ArrowClickPointTipWindowData> m_TipAnimQueue = new Queue<ArrowClickPointTipWindowData>();

        public void PlayTipAnim(ArrowClickPointTipWindowData data)
        {
            m_TipAnimQueue.Enqueue(data);
            UpdateGUI();
        }

    }

}








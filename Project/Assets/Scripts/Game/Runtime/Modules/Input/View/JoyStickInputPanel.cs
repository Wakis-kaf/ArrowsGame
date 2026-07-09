using Framework.Runtime.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.MAsset;
using Framework.Runtime;
using System;
using DG.Tweening;
namespace Game.Modules
{
    public class JoyStickInputPanel : Panel
    {
        private bool m_IsGuiding;
        private string m_GuideTip;
        private Action m_GuideOverCb;
        public UIJoyStick JoyStickInput => joyStickInput;
        private UIJoyStick joyStickInput;
        #region PrefabBinder 自动引用区域 开始
		private UnityEngine.RectTransform rtHandler;
		private UnityEngine.RectTransform rtBackground;
		private UnityEngine.RectTransform rtBoundArea;
		private UnityEngine.RectTransform rtDeteactArea;
		private Framework.Runtime.UI.USprite uspHand;
		private UnityEngine.RectTransform transJoyHandler;
		private UnityEngine.RectTransform transJoyHandlerBg;
		private Framework.Runtime.UI.UTMPText utmpTxtGuideTip;
		private UnityEngine.RectTransform transGuide;

		#endregion PrefabBinder 自动引用区域 结束

        private Sequence animationSequence;
        private bool isAnimating = false;

        // 添加这两个变量来存储初始位置
        private Vector3 joyStartPos;
        private Vector3 handStartPos;

        // 可调整的参数
        [SerializeField] private float joyHandlerMoveDistance = -50f; // tanrsJoyhandler的移动距离
        [SerializeField] private float handMoveDistance = -150f;        // tanrHand的移动距离
        [SerializeField] private float moveDuration = 0.5f;           // 向下移动持续时间
        [SerializeField] private float returnDuration = 0.3f;         // 复位持续时间
        [SerializeField] private float intervalDuration = 0.3f;       // 两次动画之间的间隔


        protected override void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
        {
			this.rtHandler = prefabBinder.GetObj<UnityEngine.RectTransform>("rtHandler");
			this.rtBackground = prefabBinder.GetObj<UnityEngine.RectTransform>("rtBackground");
			this.rtBoundArea = prefabBinder.GetObj<UnityEngine.RectTransform>("rtBoundArea");
			this.rtDeteactArea = prefabBinder.GetObj<UnityEngine.RectTransform>("rtDeteactArea");
			this.uspHand = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspHand");
			this.transJoyHandler = prefabBinder.GetObj<UnityEngine.RectTransform>("transJoyHandler");
			this.transJoyHandlerBg = prefabBinder.GetObj<UnityEngine.RectTransform>("transJoyHandlerBg");
			this.utmpTxtGuideTip = prefabBinder.GetObj<Framework.Runtime.UI.UTMPText>("utmpTxtGuideTip");
			this.transGuide = prefabBinder.GetObj<UnityEngine.RectTransform>("transGuide");

		}
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
            joyStickInput = DisplayGO.GetComponent<UIJoyStick>();
            this.joyStickInput.handler = rtHandler;
            this.joyStickInput.boundaryRect = rtBoundArea;
            this.joyStickInput.detectRect = rtDeteactArea;
            this.joyStickInput.background = rtBackground;
            this.joyStickInput.inCanvas = UIRoot.RootCanvas;
            this.joyStickInput.uiCamera = UIRootCamera.Camera;
            InitializeOriginalPositions();
            this.joyStickInput.AddDragCb(OnInputChanged);
            this.joyStickInput.Init();
        }
        /// <summary>
        /// 当绑定UI加载完成且UI显示回调(子类重写)
        /// </summary>
        protected override void OnShow()
        {
            SetActive(this.transGuide, false);
        }
        /// <summary>
        /// 当绑定UI加载完成且UI隐藏回调(子类重写)    
        /// </summary>
        protected override void OnHide()
        {

        }
        public void SetGuide(string tip,Action guideOverCb=null)
        {
            m_IsGuiding = true;
            m_GuideOverCb = guideOverCb;
            m_GuideTip = tip;
            this.UpdateGUI();
        }
        public void CancelGuide()
        {
            if (!m_IsGuiding) { return; }
            m_IsGuiding = false;
            m_GuideOverCb?.Invoke();
            SetActive(this.transGuide, false);

        }
        public void UpdateGuide()
        {
            if (!m_IsGuiding) { return; }
            SetActive(this.transGuide, true);
            this.utmpTxtGuideTip.text = this.m_GuideTip;
            StartAnimation();
        }
        private void InitializeOriginalPositions()
        {
            if (transJoyHandler != null)
            {
                joyStartPos = transJoyHandler.localPosition;
            }

            if (uspHand != null)
            {
                handStartPos = uspHand.transform.localPosition;
            }
        }

        //private void StartAnimation()
        //{
        //    StopAnimation();
        //    if (isAnimating) return;

        //    isAnimating = true;
        //    var tanrsJoyhandler = transJoyHandler;
        //    var tanrHand = uspHand.transform;
        //    // 创建动画序列
        //    animationSequence = DOTween.Sequence();

        //    // 同时向下移动
        //    animationSequence.Join(tanrsJoyhandler.DOLocalMoveY(
        //        joyHandlerOriginalPos.y + joyHandlerMoveDistance,
        //        moveDuration).SetEase(Ease.OutQuad));

        //    animationSequence.Join(tanrHand.DOLocalMoveY(
        //        handOriginalPos.y + handMoveDistance,
        //        moveDuration).SetEase(Ease.OutQuad));

        //    // 同时复位
        //    animationSequence.Append(tanrsJoyhandler.DOLocalMoveY(
        //        joyHandlerOriginalPos.y,
        //        returnDuration).SetEase(Ease.InQuad));

        //    animationSequence.Join(tanrHand.DOLocalMoveY(
        //        handOriginalPos.y,
        //        returnDuration).SetEase(Ease.InQuad));

        //    // 添加间隔
        //    animationSequence.AppendInterval(intervalDuration);

        //    // 设置循环
        //    animationSequence.SetLoops(-1, LoopType.Restart);
        //    animationSequence.SetUpdate(true);

        //    // 动画完成后重置标志
        //    animationSequence.OnKill(() => isAnimating = false);
        //}
        private void StartAnimation()
        {
            var tanrsJoyhandler = transJoyHandler;
            var tanrHand = uspHand.transform;


            // 停止之前的动画
            if (animationSequence != null && animationSequence.IsActive())
                animationSequence.Kill();

            animationSequence = DOTween.Sequence();

            // 使用变量而不是硬编码值 - 这里开始修正
            animationSequence.Join(tanrsJoyhandler.DOLocalMoveY(
                joyStartPos.y + joyHandlerMoveDistance, // 使用变量
                0.5f)
                .SetUpdate(true));

            animationSequence.Join(tanrHand.DOLocalMoveY(
                handStartPos.y + handMoveDistance, // 使用变量
                0.5f)
                .SetUpdate(true));

            // 直接复位
            animationSequence.AppendCallback(() => {
                tanrsJoyhandler.localPosition = joyStartPos;
                tanrHand.localPosition = handStartPos;
            });

            animationSequence.AppendInterval(intervalDuration).SetUpdate(true);
            animationSequence.SetLoops(-1);
        }
        private void StopAnimation()
        {
            if (animationSequence != null && animationSequence.IsActive())
            {
                animationSequence.Kill();
                isAnimating = false;

                // 重置位置
                if (transJoyHandler != null)
                {
                    transJoyHandler.localPosition = joyStartPos;
                }

                if (uspHand != null)
                {
                    uspHand.transform.localPosition = handStartPos;
                }
            }
        }

        /// <summary>
        /// 当绑定UI加载完成且数据更新的时候调用(子类重写)
        /// </summary>
        /// <param name="data"></param>

        protected override void OnGUI(object data)
        {
            UpdateGuide();
        }
        /// <summary>
        /// 当绑定UI被删除时回调(子类重写)
        /// </summary>
        public override void OnDestroy()
        {

        }
        public override string GetAssetLink(string outAssetLink)
        {
			string assetPath = "Assets/AddressableResources/UI/Input/Prefabs/JoyStickInputPanel.prefab";
			return AssetPathEncoder.EncodeEnvAssetLink(assetPath, AssetType.PrefabAsset);

		}
        private void OnInputChanged(Vector2 input)
        {
            CancelGuide();
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_player_move_input,input);
        }
    }
}


























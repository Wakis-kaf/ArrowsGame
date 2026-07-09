using Cysharp.Threading.Tasks;
using DG.Tweening;
using Framework.Runtime;
using Framework.Runtime.Config;
using Framework.Runtime.MAsset;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.UI;
using System;
using System.Threading;
using UnityEngine;

namespace Game.Modules
{
    public class GuidePointOption
    {
        public bool showHand = true;
        public bool showCenterPoint = true;
        public HightLightType hightLightType = HightLightType.Circle;
        public Vector2 screenPos;
        public GameObject go;
        public float sizeScale = 1;
    }
    public class GuideDialogOption
    {
        public string content;
        public bool pauseGame = true;
        public Action closeCb;
        public bool enableQuickDisplay = false;
        public float popCharInterval = 0.02f;
    }
    public enum HightLightType
    {
        Circle = 0,
        Rect = 1,
        CircleRect = 2,
    }
    public class GuideHighlightOption
    {
        public HightLightType hightlightType;
        public Vector2 pos1;
        public Vector2 pos2;
        public Vector2 widthAndHeight;
        public float rad1;
        public float rad2;
        public float feather = 100;
        public float sizeScale = 1;
        public GameObject evtTarget;
        public bool showHand=true;
        public bool useRectSize=true;
    }

    public class GameGuidePanel : Panel
    {
        #region PrefabBinder 自动引用区域 开始
		private Framework.Runtime.UI.USprite uspAreaCenter;
		private Framework.Runtime.UI.USprite uspAreaBg;
		private UnityEngine.RectTransform transArea;
		private UnityEngine.RectTransform transHandPivot;
		private UnityEngine.RectTransform transHand;
		private Framework.Runtime.UI.UGuideMask uguidMaskBg;
		private UnityEngine.GameObject goHighlight;
		private Framework.Runtime.UI.UButton ubtnDetect;
		private Framework.Runtime.UI.UTMPText utmpTxtDialogContent;
		private UnityEngine.RectTransform transDialog;
		private Framework.Runtime.UI.USprite uspBg;
		private UnityEngine.GameObject goDialog;

		#endregion PrefabBinder 自动引用区域 结束


        [Header("移动设置")]
        public float moveDistance = 25f;
        public float durationOut = 0.2f;      // 移出时间
        public float durationIn = 0.2f;       // 移回时间
        public float pauseDuration = 0.1f;    // 停留时间

        [Header("缩放设置")]
        public float maxScale = 1.5f;
        public float areaMaxScale = 2f;
        public float scaleDuration = 0.2f;

        [Header("缓动曲线")]
        public Ease moveOutEase = Ease.OutCubic;
        public Ease moveInEase = Ease.InCubic;
        public Ease scaleOutEase = Ease.OutBack;
        public Ease areaScaleOutEase = Ease.OutBack;
        public Ease scaleInEase = Ease.InBack;
        public Ease areaScaleInEase = Ease.InBack;

        private Vector2 originalPos;
        private Vector3 originalScale = Vector3.one;
        private Vector3 originalAreaScale = Vector3.one;
        private Sequence animationSequence;


        // 弹字相关变量
        private CancellationTokenSource m_TypingCancellationTokenSource;
        private string m_FullText = "";
        private int m_CurrentCharIndex = 0;
        private bool m_IsTypingComplete = false;
        private GuideDialogOption m_CurrentOption;
        private bool m_IsQuickDisplayMode = false;
        private bool m_CanCloseByClick = false;
        private bool m_IsTypingActive = false;
        
        protected override void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
        {
			this.uspAreaCenter = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspAreaCenter");
			this.uspAreaBg = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspAreaBg");
			this.transArea = prefabBinder.GetObj<UnityEngine.RectTransform>("transArea");
			this.transHandPivot = prefabBinder.GetObj<UnityEngine.RectTransform>("transHandPivot");
			this.transHand = prefabBinder.GetObj<UnityEngine.RectTransform>("transHand");
			this.uguidMaskBg = prefabBinder.GetObj<Framework.Runtime.UI.UGuideMask>("uguidMaskBg");
			this.goHighlight = prefabBinder.GetObj<UnityEngine.GameObject>("goHighlight");
			this.ubtnDetect = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnDetect");
			this.utmpTxtDialogContent = prefabBinder.GetObj<Framework.Runtime.UI.UTMPText>("utmpTxtDialogContent");
			this.transDialog = prefabBinder.GetObj<UnityEngine.RectTransform>("transDialog");
			this.uspBg = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspBg");
			this.goDialog = prefabBinder.GetObj<UnityEngine.GameObject>("goDialog");

		}

        public override int GetOpenLayer(int externalLayer)
        {
			return GlobalConstant.LAYER_HIGH_PANEL;

		}

        public override string GetAssetLink(string outAssetLink)
        {
			string assetPath = "Assets/AddressableResources/UI/Guide/Prefabs/GameGuidePanel.prefab";
			return AssetPathEncoder.EncodeEnvAssetLink(assetPath, AssetType.PrefabAsset);

		}

        /// <summary>
        /// 子类重写，构造函数中调用
        /// </summary>
        protected override void OnInit()
        {
            // 初始化取消令牌源
            m_TypingCancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// 显示对象初始化UI,当绑定的预制体加载完成后回调(子类重写)
        /// </summary>
        protected override void OnInitUI()
        {
            this.ubtnDetect.AddClick(OnDetectClick);
            originalPos = transHandPivot.anchoredPosition;
            originalScale = transHandPivot.localScale;
            originalAreaScale = transArea.localScale;
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
            ResetAllEffects();
            //CheckResuem();
            if (m_Option != null)
            {
                m_Option.closeCb?.Invoke();
            }
        }
        private GuideDialogOption m_Option;
        /// <summary>
        /// 当绑定UI加载完成且数据更新的时候调用(子类重写)
        /// </summary>
        /// <param name="data"></param>
        protected override void OnGUI(object data)
        {
            SetActive(goDialog, false);
            SetActive(goHighlight, false);
            SetActive(transHand, false);
            if (data is GuideDialogOption option)
            {
                UpdateDialog(option);
                //CheckPause();
            }else if(data is GuideHighlightOption highlightOption)
            {
                UpdateHighlight(highlightOption);
            }else if(data is GuidePointOption pointOption)
            {
                UpdatePoint(pointOption);
            }
        }

        private void UpdatePoint(GuidePointOption option)
        {
            var size = GetHightPointSize(option.go, option.hightLightType, 75, Vector2.one * 150, option.sizeScale);
            UpdatePoint(option.showHand, option.screenPos, option.hightLightType, size, option.showCenterPoint);

        }
        private void UpdatePoint(bool isShowHand, 
            Vector2 screenPos,
            HightLightType pointType
            ,Vector2 screenSize,
            bool showCenterPoint = true)
        {
            SetActive(transHand, isShowHand);
            SetActive(uspAreaCenter, showCenterPoint);
            StopHandAnimation();
            if (isShowHand)
            {
                var screePos = screenPos;
                var anchorPos = UIUtil.GetScree2AnchorPosition(UIRootCamera.Camera, screePos, (RectTransform)transHand.parent.transform);
                this.transHand.anchoredPosition = anchorPos;
                originalPos = this.transHandPivot.anchoredPosition;
                SetHandAreaStyle(pointType, screenSize);
                PlayHandAnimation();
            }
        }
        private Vector2 GetHightPointSize(
            GameObject go,
            HightLightType hightlightType,
            float rads,
            Vector2 widthAndHeight,
            float sizeScale)
        {
            Vector2 screeSize = Vector2.one * 100;
            Vector2 size = Vector2.zero;
            if (go != null && go.transform is RectTransform rectTransform)
            {
                var rect = UIUtil.GetScreenSpaceRect(UIRootCamera.Camera, rectTransform);
                screeSize.x = rect.width;
                screeSize.y = rect.height;
            }
            else
            {
                if (hightlightType == HightLightType.Circle)
                {
                    screeSize = Vector2.one * rads * 2;
                }
                else
                {
                    screeSize = widthAndHeight;
                }
            }
            if (hightlightType == HightLightType.Circle)
            {
                float max = Mathf.Max(screeSize.x, screeSize.y);
                size = Vector2.one * max/2;
            }
            else
            {
                size = screeSize;
            }
            return size * sizeScale;
        }

        private void UpdateHighlight(GuideHighlightOption option)
        {
            Vector2 screeSize = GetHightPointSize(option.evtTarget,option.hightlightType,option.rad1,option.widthAndHeight, option.sizeScale);
            UpdatePoint(option.showHand, option.pos1, option.hightlightType, screeSize,true);
            SetActive(goHighlight, true);
            Vector2 scaledWidthAndHeight = option.widthAndHeight;
            float scaledRad1 = option.rad1;
            float scaleFactor = GetScreenScaleFactor();
            if (option.useRectSize && option.evtTarget != null && option.evtTarget.transform is RectTransform rect)
            {
                Rect screeRect = UIUtil.GetScreenSpaceRect(UIRootCamera.Camera, rect);
                scaledWidthAndHeight = new Vector2(screeRect.width, screeRect.height);
                if(option.hightlightType == HightLightType.Circle)
                {
                    scaledRad1 = Mathf.Max(screeRect.width, screeRect.height) / 2;
                }
                else
                {
                    scaledRad1 = option.rad1 * scaleFactor;
                }
            }
            else
            {
                
                scaledWidthAndHeight = option.widthAndHeight * scaleFactor;
                scaledRad1 = option.rad1 * scaleFactor;
            }
            scaledWidthAndHeight = scaledWidthAndHeight * option.sizeScale;
            scaledRad1 = scaledRad1 * option.sizeScale;
            this.uguidMaskBg.SetFeather(option.feather);

            if (option.hightlightType == HightLightType.Circle)
            {
                this.uguidMaskBg.CreateCircleMask(option.pos1, scaledRad1);
            }else if(option.hightlightType  == HightLightType.Rect){
                this.uguidMaskBg.CreateRectangleMask(option.pos1, scaledWidthAndHeight);
            }
            else if (option.hightlightType == HightLightType.CircleRect)
            {
                this.uguidMaskBg.CreateCircleRectangleMask(option.pos1, scaledWidthAndHeight, scaledRad1);
            }
            //if (option.showHand)
            //{
            //    var screePos = option.pos1;
            //    var anchorPos = UIUtil.GetScree2AnchorPosition(UIRootCamera.Camera, screePos, (RectTransform)transHand.parent.transform);
            //    this.transHand.anchoredPosition = anchorPos;
            //}


            //this.uguidMaskBg.SetTargetImage(option.evtTarget);


        }
        private float GetScreenScaleFactor()
        {
            // 你的标准分辨率（例如 1080x1920）
            Vector2 referenceResolution = UIRoot.CanvasSize;

            // 当前屏幕分辨率
            Vector2 currentResolution = new Vector2(Screen.width, Screen.height);

            // 计算宽高比差异
            float referenceAspect = referenceResolution.x / referenceResolution.y;
            float currentAspect = currentResolution.x / currentResolution.y;

            // 方式3：综合考虑宽高比（推荐）
            if (currentAspect > referenceAspect)
            {
                // 当前屏幕更宽，基于高度缩放
                return currentResolution.y / referenceResolution.y;
            }
            else
            {
                // 当前屏幕更高或比例相同，基于宽度缩放
                return currentResolution.x / referenceResolution.x;
            }
        }
        private void OnDetectClick()
        {
            // 如果正在打字且未完成
            if (!m_IsTypingComplete && m_IsTypingActive)
            {
                // 快速显示模式：点击直接显示全部文本
                if (m_IsQuickDisplayMode)
                {
                    CancelTypingEffect();
                    ShowFullText();
                    m_CanCloseByClick = true; // 现在可以点击关闭
                }
                else
                {
                    // 非快速显示模式：点击直接完成打字
                    CancelTypingEffect();
                    ShowFullText();
                    m_IsTypingComplete = true;
                    m_CanCloseByClick = true; // 打字完成后可以点击关闭
                }
            }
            else if (m_CanCloseByClick)
            {
                // 打字已完成，可以关闭面板
                CloseWindow();
            }
        }

        private void UpdateDialog(GuideDialogOption option)
        {
            SetActive(goDialog, true);
            m_Option = option;
            // 重置所有效果
            ResetAllEffects();

            m_CurrentOption = option;
            m_FullText = option.content;
            m_IsQuickDisplayMode = option.enableQuickDisplay;
            m_IsTypingComplete = false;
            m_CurrentCharIndex = 0;
            m_CanCloseByClick = m_IsQuickDisplayMode; // 快速显示模式一开始就可以点击关闭

            // 清空文本
            if (utmpTxtDialogContent != null)
            {
                utmpTxtDialogContent.text = "";
            }

            // 开始弹字效果
            StartTypingEffect().Forget();
        }

        private async UniTaskVoid StartTypingEffect()
        {
            if (string.IsNullOrEmpty(m_FullText) || m_CurrentOption == null)
                return;

            m_IsTypingActive = true;
            m_CurrentCharIndex = 0;
            m_IsTypingComplete = false;

            var token = m_TypingCancellationTokenSource.Token;

            try
            {
                while (m_CurrentCharIndex < m_FullText.Length && !token.IsCancellationRequested)
                {
                    // 每次显示一个字符
                    string displayedText = m_FullText.Substring(0, m_CurrentCharIndex + 1);
                    if (utmpTxtDialogContent != null)
                    {
                        utmpTxtDialogContent.text = displayedText;
                    }

                    m_CurrentCharIndex++;

                    // 等待间隔时间（使用UniTask延迟）
                    await UniTask.Delay(TimeSpan.FromSeconds(m_CurrentOption.popCharInterval),ignoreTimeScale:true,
                        cancellationToken: token);
                }

                if (!token.IsCancellationRequested)
                {
                    // 打字完成
                    m_IsTypingComplete = true;
                    m_CanCloseByClick = true; // 打字完成后可以点击关闭
                    m_IsTypingActive = false;

                    // 如果不是快速显示模式，打字完成后需要点击才能关闭
                    if (!m_IsQuickDisplayMode)
                    {
                        m_CanCloseByClick = true;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 任务被取消，正常处理
                //Debug.Log("Typing effect was cancelled.");
            }
            catch (Exception e)
            {
                //Debug.LogError($"Error in typing effect: {e}");
            }
            finally
            {
                m_IsTypingActive = false;
            }
        }

        private void CancelTypingEffect()
        {
            // 取消当前的打字任务
            if (m_TypingCancellationTokenSource != null && !m_TypingCancellationTokenSource.IsCancellationRequested)
            {
                m_TypingCancellationTokenSource.Cancel();
                // 创建新的CancellationTokenSource用于下次打字
                m_TypingCancellationTokenSource.Dispose();
                m_TypingCancellationTokenSource = new CancellationTokenSource();
            }
            m_IsTypingActive = false;
        }

        private void ShowFullText()
        {
            if (utmpTxtDialogContent != null)
            {
                utmpTxtDialogContent.text = m_FullText;
            }
            m_CurrentCharIndex = m_FullText.Length;
            m_IsTypingComplete = true;
            m_IsTypingActive = false;
        }

        private void ResetAllEffects()
        {
            // 停止当前的打字任务
            CancelTypingEffect();

            // 重置所有状态
            m_FullText = "";
            m_CurrentCharIndex = 0;
            m_IsTypingComplete = false;
            m_CurrentOption = null;
            m_IsQuickDisplayMode = false;
            m_CanCloseByClick = false;
            m_IsTypingActive = false;

            // 清空文本显示
            if (utmpTxtDialogContent != null)
            {
                utmpTxtDialogContent.text = "";
            }
        }

        /// <summary>
        /// 当绑定UI被删除时回调(子类重写)
        /// </summary>
        public override void OnDestroy()
        {
            ResetAllEffects();

            // 清理CancellationTokenSource
            if (m_TypingCancellationTokenSource != null)
            {
                if (!m_TypingCancellationTokenSource.IsCancellationRequested)
                {
                    m_TypingCancellationTokenSource.Cancel();
                }
                m_TypingCancellationTokenSource.Dispose();
                m_TypingCancellationTokenSource = null;
            }
        }
        public void SetHandAreaStyle(HightLightType pointType, Vector2 screenSize)
        {
            if(pointType == HightLightType.Circle)
            {
                uspAreaBg.Path = "PlayGuide.guide_frame_circle";
                // 这个是半径
                Vector2 rectSize = UIUtil.GetScreenSizeToRectSizeDelta(UIRoot.RootCanvas, screenSize);
                uspAreaBg.rectTransform.sizeDelta =  rectSize* 2;
            }
            else
            {
                uspAreaBg.Path = "PlayGuide.guide_frame_square";
                Vector2 rectSize = UIUtil.GetScreenSizeToRectSizeDelta(UIRoot.RootCanvas, screenSize);
                uspAreaBg.rectTransform.sizeDelta = rectSize;
            }
        }
        public void PlayHandAnimation()
        {
            StopHandAnimation();

            Vector2 targetPos = originalPos + new Vector2(moveDistance, -moveDistance);

            // 确保初始状态
            transArea.localScale = originalAreaScale;
            transHandPivot.localScale = originalScale;

            animationSequence = DOTween.Sequence();

            // 记录实际的目标缩放值（绝对缩放）
            Vector3 targetAreaScale = originalAreaScale * areaMaxScale;
            Vector3 targetHandScale = originalScale * maxScale;

            float scaleOutDuration = durationOut * 0.8f;
            float scaleInDuration = durationIn * 0.8f;

            // 1. 移动动画
            animationSequence.Append(transHandPivot.DOAnchorPos(targetPos, durationOut)
                .SetEase(moveOutEase));

            // 2. 缩放动画 - 使用绝对缩放值，不使用SetRelative()
            animationSequence.Join(transArea.DOScale(targetAreaScale, scaleOutDuration)
                .SetEase(Ease.OutCirc)
                .SetDelay(0.03f));

            animationSequence.Join(transHandPivot.DOScale(targetHandScale, scaleOutDuration)
                .SetEase(Ease.OutCirc)
                .SetDelay(0.03f));

            // 3. 停留
            animationSequence.AppendInterval(pauseDuration);

            // 4. 返回移动
            animationSequence.Append(transHandPivot.DOAnchorPos(originalPos, durationIn)
                .SetEase(moveInEase));

            // 5. 缩回 - 回到原始缩放值
            animationSequence.Join(transArea.DOScale(originalAreaScale, scaleInDuration)
                .SetEase(Ease.InCirc)
                .SetDelay(-0.02f));

            animationSequence.Join(transHandPivot.DOScale(originalScale, scaleInDuration)
                .SetEase(Ease.InCirc)
                .SetDelay(-0.02f));

            animationSequence.SetLoops(-1);
            animationSequence.SetId("AdvancedHandGuide");
            animationSequence.SetUpdate(true);
        }
        public void StopHandAnimation()
        {
            if (animationSequence != null && animationSequence.IsActive())
            {
                animationSequence.Kill();
            }

            if (transHandPivot != null)
            {
                transHandPivot.anchoredPosition = originalPos;
                transHandPivot.localScale = originalScale;
            }
        }
        /// <summary>
        /// 重写关闭方法，确保清理资源
        /// </summary>
        //public override void OnClose()
        //{
        //    ResetAllEffects();
        //    base.Close();
        //}
    }
}











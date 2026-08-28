using Framework.Runtime.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.MAsset;
using Framework.Runtime.Config;
using System;
using Game.Modules.GModuleArrows;
using Game.Modules.GModuleTip;
using Framework.Runtime.LogSystem;
using Framework.Runtime;
using UnityEngine.Rendering;
using DG.Tweening;
using Game.Modules.GModuleManage;
using Game.Modules.GModuleProgression;
namespace Game.Modules
{
    public class PlayGamePanel : Panel
    {
        #region PrefabBinder 自动引用区域 开始
        private UnityEngine.RectTransform rtBottomArea;
        private UnityEngine.RectTransform rtTopArea;
        private Framework.Runtime.UI.UButton ubtnMinus;
        private Framework.Runtime.UI.UButton ubtnPlus;
        private Framework.Runtime.UI.UProgressBar upbZoom;
        private Framework.Runtime.UI.UProgressBar upbProgress;
        private Framework.Runtime.UI.UTMPText utmpTxtDif;
        private Framework.Runtime.UI.UTMPText utmpTxtLevel;
        private Framework.Runtime.UI.UList ulistHearts;
        private Framework.Runtime.UI.UButton ubtnTip;
        private Framework.Runtime.UI.UButton ubtnRestart;
        private Framework.Runtime.UI.UButton ubtnReturn;
        private Framework.Runtime.UI.USprite uspBg;
        private Framework.Runtime.UI.UTMPText utmpTxtCoins;
        private Framework.Runtime.UI.UTMPText utmpTxtHearts;
        private Framework.Runtime.UI.UButton ubtnUndo;
        private Framework.Runtime.UI.UButton ubtnClear;

        #endregion PrefabBinder 自动引用区域 结束

        protected override void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
        {
            this.rtBottomArea = prefabBinder.GetObj<UnityEngine.RectTransform>("rtBottomArea");
            this.rtTopArea = prefabBinder.GetObj<UnityEngine.RectTransform>("rtTopArea");
            this.ubtnMinus = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnMinus");
            this.ubtnPlus = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnPlus");
            this.upbZoom = prefabBinder.GetObj<Framework.Runtime.UI.UProgressBar>("upbZoom");
            this.upbProgress = prefabBinder.GetObj<Framework.Runtime.UI.UProgressBar>("upbProgress");
            this.utmpTxtDif = prefabBinder.GetObj<Framework.Runtime.UI.UTMPText>("utmpTxtDif");
            this.utmpTxtLevel = prefabBinder.GetObj<Framework.Runtime.UI.UTMPText>("utmpTxtLevel");
            this.ulistHearts = prefabBinder.GetObj<Framework.Runtime.UI.UList>("ulistHearts");
            this.ubtnTip = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnTip");
            this.ubtnRestart = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnRestart");
            this.ubtnReturn = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnReturn");
            this.uspBg = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspBg");
            this.utmpTxtCoins = prefabBinder.GetObj<Framework.Runtime.UI.UTMPText>("utmpTxtCoins");
            this.utmpTxtHearts = prefabBinder.GetObj<Framework.Runtime.UI.UTMPText>("utmpTxtHearts");
            this.ubtnUndo = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnUndo");
            this.ubtnClear = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnClear");

        }
        private bool m_IsZoomDragingChange = false;
        private bool m_IsZoomDraged = false;
        private bool m_IsZoomOutChanged = false;
        public override int GetOpenLayer(int externalLayer)
        {
            return externalLayer;

        }

        public override string GetAssetLink(string outAssetLink)
        {
            string assetPath = "Assets/AddressableResources/UI/Play/Prefabs/PlayGamePanel.prefab";
            return AssetPathEncoder.EncodeEnvAssetLink(assetPath, AssetType.PrefabAsset);

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
            ubtnRestart.AddClick(OnRestartClick);
            ubtnTip.AddClick(OnTipClick);
            ubtnReturn.AddClick(OnReturnClick);
            ulistHearts.ListRenderType = typeof(UListHeartRender);
            upbZoom.AddValueChanged(OnZoomProgressChanged, false);
            upbZoom.AddBeginDraged(OnZoomProgressBeginDrag);
            upbZoom.AddEndDraged(OnZoomProgressEndDrag);
            ubtnMinus.AddClick(OnZoomMinusClick);
            ubtnPlus.AddClick(OnZoomPlusClick);
            ubtnUndo.AddClick(() => OnPropClick(GameProgressionConstant.PropUndo, LevelVO.Current.TryUseUndoProp));
            ubtnClear.AddClick(() => OnPropClick(GameProgressionConstant.PropClear, LevelVO.Current.TryUseClearProp));

        }

        private void OnZoomPlusClick()
        {
            DispatchEevent<float, bool>(MessageCode.msg_add_camera_zoom, 0.1f, true);
        }

        private void OnZoomMinusClick()
        {
            DispatchEevent<float, bool>(MessageCode.msg_add_camera_zoom, -0.1f, true);
        }

        private void OnReturnClick()
        {
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_return_home);
        }

        /// <summary>
        /// 注册页面消息，次于 OnInitUI 之后执行
        /// </summary>
        protected override void OnSubscribeMessages()
        {
            SubscribeEvent(MessageCode.msg_on_game_restart, OnGameRestart);
            SubscribeEvent(MessageCode.msg_on_game_start, OnGameStart);
            SubscribeEvent(MessageCode.msg_on_arrowLineChanged, SyncArrowNumProgressAnim);
            SubscribeEvent(MessageCode.msg_on_wrong_arrow_click, RefreshProgressionView);
            SubscribeEvent<float, float>(MessageCode.msg_on_cameraZoom_changed, OnCamerZoomChanged);
            SubscribeEvent(MessageCode.msg_on_progression_changed, RefreshProgressionView);
            SubscribeEvent<int>(MessageCode.msg_on_progression_rewarded, OnProgressionRewarded);
        }



        private void OnGameStart()
        {
            ulistHearts.SetDataSources(LevelVO.Current.GetLevelHeartInfoList());
            RefreshProgressionView();
        }

        private void OnGameRestart()
        {
            ulistHearts.SetDataSources(LevelVO.Current.GetLevelHeartInfoList());
        }

        private void OnTipClick()
        {
            OnPropClick(GameProgressionConstant.PropHint, LevelVO.Current.TryUseHintProp);
        }

        private void OnRestartClick()
        {
            LevelVO.Current.ReStartGame();
        }

        /// <summary>
        /// 当绑定UI加载完成且UI显示回调(子类重写)
        /// </summary>
        protected override void OnShow()
        {
            // StartEntryAnim();
            SyncArrowNumProgress(false);
            UpdateView();
            RefreshProgressionView();
            // SyncZoom();
            ulistHearts.SetDataSources(LevelVO.Current.GetLevelHeartInfoList());
        }
        private void UpdateView()
        {
            utmpTxtLevel.text = $"第{GameArchive.Main.LevelArchive.GetCurLevelId()}关";
        }
        private void OnPropClick(int propId, Func<bool> useProp)
        {
            if (GameProgressionService.TryConsumeProp(propId))
            {
                if (useProp()) { RefreshProgressionView(); return; }
                GameProgressionService.AddProp(propId, 1);
            }
            GameProgressionService.AcquireProp(propId, acquired =>
            {
                if (acquired && GameProgressionService.TryConsumeProp(propId) && !useProp()) GameProgressionService.AddProp(propId, 1);
                RefreshProgressionView();
            });
        }
        private void RefreshProgressionView()
        {
            if (GameArchive.Main == null || LevelVO.Current == null) return;
            utmpTxtCoins.text = $"金币 {GameArchive.Main.ProgressionArchive.Coins}";
            utmpTxtHearts.text = $"体力 {LevelVO.Current.GetHearNum()}";
            var isVisible = GameProgressionService.IsPropBarVisible(GameArchive.Main.LevelArchive.GetCurLevelId());
            ubtnTip.gameObject.SetActive(isVisible);
            ubtnUndo.gameObject.SetActive(isVisible);
            ubtnClear.gameObject.SetActive(isVisible);
            ubtnTip.Text = $"提示\\nx{GameProgressionService.GetPropCount(GameProgressionConstant.PropHint)}";
            ubtnUndo.Text = $"撤销\\nx{GameProgressionService.GetPropCount(GameProgressionConstant.PropUndo)}";
            ubtnClear.Text = $"清除\\nx{GameProgressionService.GetPropCount(GameProgressionConstant.PropClear)}";
        }
        private void OnProgressionRewarded(int levelId)
        {
            RefreshProgressionView();
        }
        private void SyncArrowNumProgressAnim()
        {
            SyncArrowNumProgress(true);
        }
        private void SyncZoom(float curZoom = -1, float targetZoom = -1)
        {
            var levelArgs = LevelVO.Current.LevelInfo.levelArgs;
            var maxZoom = levelArgs.maxZoomScale;
            var minZoom = levelArgs.minZoomScale;
            var startZoom = levelArgs.startZoomScale;
            var curVal = upbZoom.value;
            var targetValue = (float)(targetZoom - minZoom) / (float)(maxZoom - minZoom);
            upbZoom.maxValue = 1;
            upbZoom.value = targetValue;
        }

        private void OnCamerZoomChanged(float zoom, float targetZoom)
        {
            if (m_IsZoomDragingChange || m_IsZoomDraged)
            {
                return;
            }
            m_IsZoomOutChanged = true;
            SyncZoom(zoom, targetZoom);
            m_IsZoomOutChanged = false;
        }


        private void OnZoomProgressEndDrag(float zoom)
        {
            OnZoomProgressChanged(zoom);
            m_IsZoomDragingChange = false;
        }

        private void OnZoomProgressBeginDrag(float zoom)
        {
            m_IsZoomDragingChange = true;
        }
        private void OnZoomProgressChanged(float value)
        {
            if (m_IsZoomOutChanged) return;
            m_IsZoomDraged = true;
            // if (!m_IsZoomDragChange || m_IsZoomOutChanged) return;
            var levelArgs = LevelVO.Current.LevelInfo.levelArgs;
            var maxZoom = levelArgs.maxZoomScale;
            var minZoom = levelArgs.minZoomScale;
            var startzoom = levelArgs.startZoomScale;
            float add = (maxZoom - minZoom) * value;
            float finalZoom = minZoom + add;
            DispatchEevent<float, bool>(MessageCode.msg_set_camera_zoom, finalZoom, true);
            m_IsZoomDraged = false;
        }
        private void SyncArrowNumProgress(bool isAnim = false)
        {
            var total = LevelVO.Current.GetTotalArrowLineNum();
            int cost = total - LevelVO.Current.GetRemainArrowLineNum();
            float targetValue = total == 0 ? 0 : cost / (float)total;

            DOTween.Kill(upbProgress);

            if (isAnim)
            {
                DOTween.To(() => upbProgress.value, x => upbProgress.value = x, targetValue, 0.3f)
                    .SetEase(Ease.OutQuad)
                    .SetTarget(upbProgress);
            }
            else
            {
                upbProgress.value = targetValue;
            }
        }


        /// <summary>
        /// 当绑定UI加载完成且UI隐藏回调(子类重写)    
        /// </summary>
        protected override void OnHide()
        {
            SyncArrowNumProgress(false);
        }
        /// <summary>
        /// 当绑定UI加载完成且数据更新的时候调用(子类重写)
        /// </summary>
        /// <param name="data"></param>

        protected override void OnGUI(object data)
        {

        }
        /// <summary>
        /// 当绑定UI被删除时回调(子类重写)
        /// </summary>
        public override void OnDestroy()
        {

        }
        protected override void OnStartShowEffect(Action showCompleteCb)
        {
            CanvasGroup.blocksRaycasts = true;
            CanvasGroup.interactable = true;
            CanvasGroup.alpha = 0;
            CanvasGroup.DOFade(1, 0.2f).OnComplete(showCompleteCb.Invoke);
        }
        protected override void OnStartHideEffect(Action hideCompleted)
        {
            if (CanvasGroup != null)
            {
                CanvasGroup.blocksRaycasts = false;
                CanvasGroup.interactable = false;
            }
            CanvasGroup.alpha = 1;
            CanvasGroup.DOFade(0, 0.2f).OnComplete(hideCompleted.Invoke);
        }
        // private void StartEntryAnim()
        // {
        //     StopEntryAnim();
        //     CanvasGroup.alpha = 0;
        //     CanvasGroup.DOFade(1, 0.2f);
        // }
        // private void StopEntryAnim()
        // {
        //     CanvasGroup.DOKill();
        //     CanvasGroup.alpha = 1;
        // }
    }
}







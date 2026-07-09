using Framework.Runtime.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.MAsset;
using Framework.Runtime.Config;
using Game.Modules.GModuleArrows;
using Framework.Runtime.LogSystem;
using System;
using Game.Modules.GModuleArrowGenerateEdit;
namespace Game.Modules
{
    public class ArrowsGenerateOptionPanel : Panel
    {
        #region PrefabBinder 自动引用区域 开始
		private Framework.Runtime.UI.UCheckBoxGroup uckbGroupGenerateType;
		private UnityEngine.RectTransform rtGenerateTypes;
		private Framework.Runtime.UI.UButton ubtnReGenerate;
		private Framework.Runtime.UI.UButton ubtnSave;
		private Framework.Runtime.UI.UButton ubtnClose;
		private Framework.Runtime.UI.UCheckBox uckbIsEnableRootTraceHead;
		private Framework.Runtime.UI.UCheckBox uckbTryMaxLength;
		private Framework.Runtime.UI.UCheckBox uckbRetractFromTurn;
		private Framework.Runtime.UI.UValueProgress uvpbTurnTendency;
		private Framework.Runtime.UI.UInputField uifEmptyReGenerateMaxRetractNum;
		private Framework.Runtime.UI.UInputField uifMainLineGenerateMaxRetractNum;
		private Framework.Runtime.UI.UInputField uifMaxTurnsPerLine;
		private Framework.Runtime.UI.UInputField uifNormalLine;
		private Framework.Runtime.UI.UInputField uifMaxLine;
		private Framework.Runtime.UI.UInputField uifMinLine;
		private Framework.Runtime.UI.UInputField uifSeed;
		private UnityEngine.RectTransform rtGroupsVertical;
		private Framework.Runtime.UI.USprite uspBg;

		#endregion PrefabBinder 自动引用区域 结束

        protected override void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
        {
			this.uckbGroupGenerateType = prefabBinder.GetObj<Framework.Runtime.UI.UCheckBoxGroup>("uckbGroupGenerateType");
			this.rtGenerateTypes = prefabBinder.GetObj<UnityEngine.RectTransform>("rtGenerateTypes");
			this.ubtnReGenerate = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnReGenerate");
			this.ubtnSave = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnSave");
			this.ubtnClose = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnClose");
			this.uckbIsEnableRootTraceHead = prefabBinder.GetObj<Framework.Runtime.UI.UCheckBox>("uckbIsEnableRootTraceHead");
			this.uckbTryMaxLength = prefabBinder.GetObj<Framework.Runtime.UI.UCheckBox>("uckbTryMaxLength");
			this.uckbRetractFromTurn = prefabBinder.GetObj<Framework.Runtime.UI.UCheckBox>("uckbRetractFromTurn");
			this.uvpbTurnTendency = prefabBinder.GetObj<Framework.Runtime.UI.UValueProgress>("uvpbTurnTendency");
			this.uifEmptyReGenerateMaxRetractNum = prefabBinder.GetObj<Framework.Runtime.UI.UInputField>("uifEmptyReGenerateMaxRetractNum");
			this.uifMainLineGenerateMaxRetractNum = prefabBinder.GetObj<Framework.Runtime.UI.UInputField>("uifMainLineGenerateMaxRetractNum");
			this.uifMaxTurnsPerLine = prefabBinder.GetObj<Framework.Runtime.UI.UInputField>("uifMaxTurnsPerLine");
			this.uifNormalLine = prefabBinder.GetObj<Framework.Runtime.UI.UInputField>("uifNormalLine");
			this.uifMaxLine = prefabBinder.GetObj<Framework.Runtime.UI.UInputField>("uifMaxLine");
			this.uifMinLine = prefabBinder.GetObj<Framework.Runtime.UI.UInputField>("uifMinLine");
			this.uifSeed = prefabBinder.GetObj<Framework.Runtime.UI.UInputField>("uifSeed");
			this.rtGroupsVertical = prefabBinder.GetObj<UnityEngine.RectTransform>("rtGroupsVertical");
			this.uspBg = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspBg");

		}
        public override int GetOpenLayer(int externalLayer)
        {
			return GlobalConstant.LAYER_DEBUGGER;

		}

        public override string GetAssetLink(string outAssetLink)
        {
			string assetPath = "Assets/AddressableResources/UI/ArrowGenerateEdit/Prefabs/ArrowsGenerateOptionPanel.prefab";
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
            ubtnClose.AddClick(OnCloseClick);
            ubtnSave.AddClick(OnSaveClick);
            ubtnReGenerate.AddClick(OnReGenerateClick);
        }

        private void OnReGenerateClick()
        {
            OnSaveClick();
            GameArrowGenerateEditClientHandler.Ins.ReloadCurLevel();
            // OnCloseClick();

        }

        private void OnSaveClick()
        {
            SyncViewToData();
        }

        private void OnCloseClick()
        {
            CloseWindow();
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
            SyncDataToView();
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

        }
        /// <summary>
        /// 当绑定UI被删除时回调(子类重写)
        /// </summary>
        public override void OnDestroy()
        {

        }
        private void SyncViewToData()
        {
            var levelVO = LevelVO.Current;
            if (levelVO == null)
            {
                Log.Error("同步数据到视图失败，当前等级VO为空");
                return;
            }

            var presure = levelVO.LevelInfo.arrowsPresure;
            presure.minLineLength = int.Parse(uifMinLine.text);
            presure.maxLineLength = int.Parse(uifMaxLine.text);
            presure.normalLineLength = int.Parse(uifNormalLine.text);
            presure.maxTurnsPerLine = int.Parse(uifMaxTurnsPerLine.text);
            presure.runtimeSeed = int.Parse(uifSeed.text);
            presure.emptyReGenerateMaxRetractNum = int.Parse(uifEmptyReGenerateMaxRetractNum.text);
            presure.mainLineGenerateMaxRetractNum = int.Parse(uifMainLineGenerateMaxRetractNum.text);
            presure.isEnableRootTraceHead = uckbIsEnableRootTraceHead.isOn;
            presure.tryMaxLength = uckbTryMaxLength.isOn;
            presure.retractFromTurn = uckbRetractFromTurn.isOn;
            presure.turnTendency = uvpbTurnTendency.GetValue();
            levelVO.LevelInfo.levelCfg.arrowsLayoutGenerateType = uckbGroupGenerateType.SelectedIndex;
            levelVO.LevelInfo.levelCfg.customSeed = int.Parse(uifSeed.text);
        }
        private void SyncDataToView()
        {
            var levelVO = LevelVO.Current;
            if (levelVO == null)
            {
                Log.Error("同步数据到视图失败，当前等级VO为空");
                return;
            }
            var presure = levelVO.LevelInfo.arrowsPresure;
            var minLineLength = presure.minLineLength;
            uifMinLine.text = minLineLength.ToString();
            var maxLineLength = presure.maxLineLength;
            uifMaxLine.text = maxLineLength.ToString();
            var normalLineLength = presure.normalLineLength;
            uifNormalLine.text = normalLineLength.ToString();
            var maxTurnsPerLine = presure.maxTurnsPerLine;
            uifMaxTurnsPerLine.text = maxTurnsPerLine.ToString();
            var seed = presure.runtimeSeed;
            uifSeed.text = seed.ToString();
            var emptyReGenerateMaxRetractNum = presure.emptyReGenerateMaxRetractNum;
            uifEmptyReGenerateMaxRetractNum.text = emptyReGenerateMaxRetractNum.ToString();
            var mainLineGenerateMaxRetractNum = presure.mainLineGenerateMaxRetractNum;
            uifMainLineGenerateMaxRetractNum.text = mainLineGenerateMaxRetractNum.ToString();
            var isEnableRootTraceHead = presure.isEnableRootTraceHead;
            uckbIsEnableRootTraceHead.isOn = isEnableRootTraceHead;
            var tryMaxLength = presure.tryMaxLength;
            uckbTryMaxLength.isOn = tryMaxLength;
            var retractFromTurn = presure.retractFromTurn;
            uckbRetractFromTurn.isOn = retractFromTurn;
            var turnTendency = presure.turnTendency;
            uvpbTurnTendency.SetValue(turnTendency);
            uckbGroupGenerateType.SelectedIndex = (int)levelVO.LevelInfo.levelCfg.arrowsLayoutGenerateType;



        }


    }
}







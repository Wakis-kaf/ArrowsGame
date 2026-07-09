using Framework.Runtime.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.MAsset;
using Framework.Runtime.Config;
using System;
using Game.Modules.GModuleArrowGenerateEdit;
using Game.Modules.GModuleArrows;
namespace Game.Modules
{
    public class ArrowsGenerateEditPanel : Panel
    {
        #region PrefabBinder 自动引用区域 开始
		private Framework.Runtime.UI.UButton ubtnOption;
		private Framework.Runtime.UI.UInputField uifCurLevel;
		private Framework.Runtime.UI.UButton ubtnExport;
		private Framework.Runtime.UI.UButton ubtnGenerate;
		private UnityEngine.RectTransform rtBotttomArea;
		private Framework.Runtime.UI.UButton ubtnLast;
		private Framework.Runtime.UI.UButton ubtnNext;
		private Framework.Runtime.UI.UButton ubtnLoadLevel;
		private Framework.Runtime.UI.UTMPText utmpTxtCurLevel;
		private UnityEngine.RectTransform rtTopArea;
		private Framework.Runtime.UI.USprite uspBg;

		#endregion PrefabBinder 自动引用区域 结束

        protected override void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
        {
			this.ubtnOption = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnOption");
			this.uifCurLevel = prefabBinder.GetObj<Framework.Runtime.UI.UInputField>("uifCurLevel");
			this.ubtnExport = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnExport");
			this.ubtnGenerate = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnGenerate");
			this.rtBotttomArea = prefabBinder.GetObj<UnityEngine.RectTransform>("rtBotttomArea");
			this.ubtnLast = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnLast");
			this.ubtnNext = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnNext");
			this.ubtnLoadLevel = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnLoadLevel");
			this.utmpTxtCurLevel = prefabBinder.GetObj<Framework.Runtime.UI.UTMPText>("utmpTxtCurLevel");
			this.rtTopArea = prefabBinder.GetObj<UnityEngine.RectTransform>("rtTopArea");
			this.uspBg = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspBg");

		}
        public override int GetOpenLayer(int externalLayer)
        {
			return GlobalConstant.LAYER_DEBUGGER;

		}

        public override string GetAssetLink(string outAssetLink)
        {
			string assetPath = "Assets/AddressableResources/UI/ArrowGenerateEdit/Prefabs/ArrowsGenerateEditPanel.prefab";
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
            ubtnLoadLevel.AddClick(LoadLevel);
            ubtnNext.AddClick(NextLevel);
            ubtnLast.AddClick(LastLevel);
            ubtnGenerate.AddClick(ReLoadLevel);
            ubtnExport.AddClick(ExportLevel);
            ubtnOption.AddClick(OpenOptionPanel);
            uifCurLevel.text = "1";
            LoadLevel();
        }

        private void OpenOptionPanel()
        {
            GameArrowGenerateEditViewHandler.Ins.OpenArrowsGenerateOptionPanel();
        }

        private void LastLevel()
        {
            int lastId = GetCurLevelId() - 1;
            lastId = Mathf.Max(lastId, 1);
            GameArrowGenerateEditClientHandler.Ins.LoadLevel(lastId);
            uifCurLevel.text = lastId.ToString();
        }

        private void NextLevel()
        {
            int nextId = GetCurLevelId() + 1;
            GameArrowGenerateEditClientHandler.Ins.LoadLevel(nextId);
            uifCurLevel.text = nextId.ToString();
        }
        private int GetCurLevelId()
        {
            int.TryParse(uifCurLevel.text, out int levelId);
            return levelId;
        }
        private void LoadLevel()
        {
            int levelId = GetCurLevelId();
            GameArrowGenerateEditClientHandler.Ins.LoadLevel(levelId);
        }
        private void ExportLevel()
        {
            GameArrowGenerateEditClientHandler.Ins.ExportCurLevel();

        }
        private void ReLoadLevel()
        {
            GameArrowGenerateEditClientHandler.Ins.ReloadCurLevel();

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

        }
        /// <summary>
        /// 当绑定UI被删除时回调(子类重写)
        /// </summary>
        public override void OnDestroy()
        {

        }
    }
}







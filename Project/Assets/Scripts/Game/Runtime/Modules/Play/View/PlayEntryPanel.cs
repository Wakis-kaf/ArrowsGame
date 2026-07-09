using Framework.Runtime.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.MAsset;
using Framework.Runtime.Config;
using Framework.Runtime;
using Game.Modules.GModuleScene;
using Game.Modules.GModuleArrows;
using System;
using Game.Modules.GModuleManage;
namespace Game.Modules
{
    public class PlayEntryPanel : Panel
    {
        #region PrefabBinder 自动引用区域 开始
        private Framework.Runtime.UI.UButton ubtnQuit;
        private Framework.Runtime.UI.UButton ubtnBook;
        private Framework.Runtime.UI.UButton ubtnBlueprint;
        private Framework.Runtime.UI.UButton ubtnInv;
        private Framework.Runtime.UI.UButton ubtnSetting;
        private Framework.Runtime.UI.UButton ubtnStart;
        private UnityEngine.GameObject goBtnGroup;
        private Framework.Runtime.UI.USprite uspBg;
        #endregion PrefabBinder 自动引用区域 结束

        protected override void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
        {
            this.ubtnQuit = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnQuit");
            this.ubtnBook = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnBook");
            this.ubtnBlueprint = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnBlueprint");
            this.ubtnInv = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnInv");
            this.ubtnSetting = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnSetting");
            this.ubtnStart = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnStart");
            this.goBtnGroup = prefabBinder.GetObj<UnityEngine.GameObject>("goBtnGroup");
            this.uspBg = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspBg");
        }
        public override int GetOpenLayer(int externalLayer)
        {
            return externalLayer;
        }

        public override string GetAssetLink(string outAssetLink)
        {
            string assetPath = "Assets/AddressableResources/UI/Play/Prefabs/PlayEntryPanel.prefab";
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
            this.ubtnStart.AddClick(OnStartClick);
            this.ubtnSetting.AddClick(OnSettingClick);
        }

        private void OnSettingClick()
        {
            DispatchEevent(MessageCode.msg_open_gameSetting_panel);
        }

        private void OnStartClick()
        {
            CloseWindow();
            DispatchEevent(MessageCode.msg_entryGamePlay);


        }
        /// <summary>
        /// 当绑定UI加载完成且UI显示回调(子类重写)
        /// </summary>
        protected override void OnShow()
        {
            var curLvId = GameArchive.Main.LevelArchive.GetCurLevelId();
            ubtnStart.Text = $"关卡{curLvId}";
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




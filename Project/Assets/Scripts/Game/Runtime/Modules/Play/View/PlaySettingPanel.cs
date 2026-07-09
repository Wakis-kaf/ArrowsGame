using Framework.Runtime;
using Framework.Runtime.Config;
using Framework.Runtime.MAsset;
using Framework.Runtime.MAudio;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.UI;
using Game.Modules.GModuleAudio;
using Game.Modules.GModuleManage;
using Game.Modules.GModuleScene;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules
{
    public class PlaySettingPanel : Panel
    {
        #region PrefabBinder 自动引用区域 开始
        private Framework.Runtime.UI.UTMPText utmpTxtShakeOpen;
        private Framework.Runtime.UI.UProgressBar upbShake;
        private Framework.Runtime.UI.UButton ubtnVersion;
        private Framework.Runtime.UI.UTMPText utmpTxtTitle;
        private Framework.Runtime.UI.USprite uspTitle;
        private Framework.Runtime.UI.USprite uspTop;
        private Framework.Runtime.UI.UProgressBar upbEffectVolumn;
        private Framework.Runtime.UI.UProgressBar upbMusicVolumn;
        private Framework.Runtime.UI.UButton ubtnClose;

        #endregion PrefabBinder 自动引用区域 结束

        protected override void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
        {
            this.utmpTxtShakeOpen = prefabBinder.GetObj<Framework.Runtime.UI.UTMPText>("utmpTxtShakeOpen");
            this.upbShake = prefabBinder.GetObj<Framework.Runtime.UI.UProgressBar>("upbShake");
            this.ubtnVersion = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnVersion");
            this.utmpTxtTitle = prefabBinder.GetObj<Framework.Runtime.UI.UTMPText>("utmpTxtTitle");
            this.uspTitle = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspTitle");
            this.uspTop = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspTop");
            this.upbEffectVolumn = prefabBinder.GetObj<Framework.Runtime.UI.UProgressBar>("upbEffectVolumn");
            this.upbMusicVolumn = prefabBinder.GetObj<Framework.Runtime.UI.UProgressBar>("upbMusicVolumn");
            this.ubtnClose = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnClose");

        }
        private void OnShakeChanged(float val)
        {
            bool isOn = val > 0 ? true : false;
            GameArchive.Main.RoleArchive.SetShakeOpen(isOn);
            utmpTxtShakeOpen.text = isOn ? "关" : "开";
            utmpTxtShakeOpen.color = isOn ? Color.red : Color.green;
        }
        private void OnMusicVolumnChange(float value)
        {
            GameAudioClientHandler.Ins.ChangeMusicVolMultiplier(value);
            if (!upbMusicVolumn.IsDraging)
            {
                GameAudioClientHandler.Ins.SaveMusicVolume(value);
            }
        }

        private void OnEffectVolumnChange(float value)
        {

            GameAudioClientHandler.Ins.ChangeEffectVolMultiplier(value);
            if (!upbEffectVolumn.IsDraging)
            {
                GameAudioClientHandler.Ins.SaveEffectVolume(value);
            }
        }

        public override string GetAssetLink(string outAssetLink)
        {
            string assetPath = "Assets/AddressableResources/UI/Play/Prefabs/PlaySettingPanel.prefab";
            return AssetPathEncoder.EncodeEnvAssetLink(assetPath, AssetType.PrefabAsset);

        }
        public override int GetOpenLayer(int externalLayer)
        {
            return externalLayer;

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
            this.ubtnClose.AddClick(OnCloseClick);
            LoadFromArchive();
            this.upbEffectVolumn.AddValueChanged(OnEffectVolumnChange);
            this.upbMusicVolumn.AddValueChanged(OnMusicVolumnChange);
            this.upbShake.AddValueChanged(OnShakeChanged);
            upbEffectVolumn.AddEndDraged(GameAudioClientHandler.Ins.SaveEffectVolume);
            upbMusicVolumn.AddEndDraged(GameAudioClientHandler.Ins.SaveMusicVolume);
            ubtnVersion.Text = FrameworkSetting.Instance.appVersion;
            ubtnVersion.AddClick(OnVersionClick);

        }

        private void OnVersionClick()
        {
            if (GameEnv.IsInDevlopMode())
            {
                if (GameApp.Debugger.IsDebuggrPanelShowing())
                {
                    GameApp.Debugger.CloseDebuggerPanel();
                }
                else
                {
                    GameApp.Debugger.OpenDebuggerPanel();
                }

            }
        }

        private void OnCloseClick()
        {
            this.CloseWindow();
        }
        /// <summary>
        /// 当绑定UI加载完成且UI显示回调(子类重写)
        /// </summary>
        protected override void OnShow()
        {
            LoadFromArchive();

        }
        private void LoadFromArchive()
        {
            this.upbEffectVolumn.value = GameAudioClientHandler.Ins.GetEffectVolume(); ;
            this.upbMusicVolumn.value = GameAudioClientHandler.Ins.GetMusicVolume();
            this.upbShake.value = GameArchive.Main.RoleArchive.GetShakeOpen() ? 1 : 0;
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








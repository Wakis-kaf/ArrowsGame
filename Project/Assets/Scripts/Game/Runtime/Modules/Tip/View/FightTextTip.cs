// FightTextTip.cs - 简化版本
using Framework.Runtime.MAsset;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.UI;
using Framework.Utils;

using System;
using UnityEngine;
using Framework.Runtime.UI.UIAnimae;
using Framework.Runtime.UI.UIAnimae.Tweeners;
namespace Game.Modules.GModuleTip
{
	public struct FightTextTipData
	{
		public string tipValue;
		public FightTextTipType fightTextTipType;
    }
	public enum FightTextTipType
	{
		NormalDamageTip,
		CritTip,
		PoisonTip,
		NormalRecoveryTip
	}
    public class FightTextTip : Tip
    {
        #region PrefabBinder 自动引用区域 开始
		private Framework.Runtime.UI.USprite uspCritPoison;
		private Framework.Runtime.UI.UTMPText utmpTxtPoisonTip;
		private UnityEngine.GameObject goPoison;
		private Framework.Runtime.UI.USprite uspCritIcon;
		private Framework.Runtime.UI.UTMPText utmpTxtCritTip;
		private UnityEngine.GameObject goCrit;
		private Framework.Runtime.UI.UTMPText utmpTxtRecoveryTip;
		private UnityEngine.GameObject goRecovery;
		private Framework.Runtime.UI.UTMPText utmpTxtDamageTip;
		private UnityEngine.GameObject goDamage;
		private UnityEngine.GameObject goRoot;

		#endregion PrefabBinder 自动引用区域 结束

        protected override void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
        {
			this.uspCritPoison = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspCritPoison");
			this.utmpTxtPoisonTip = prefabBinder.GetObj<Framework.Runtime.UI.UTMPText>("utmpTxtPoisonTip");
			this.goPoison = prefabBinder.GetObj<UnityEngine.GameObject>("goPoison");
			this.uspCritIcon = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspCritIcon");
			this.utmpTxtCritTip = prefabBinder.GetObj<Framework.Runtime.UI.UTMPText>("utmpTxtCritTip");
			this.goCrit = prefabBinder.GetObj<UnityEngine.GameObject>("goCrit");
			this.utmpTxtRecoveryTip = prefabBinder.GetObj<Framework.Runtime.UI.UTMPText>("utmpTxtRecoveryTip");
			this.goRecovery = prefabBinder.GetObj<UnityEngine.GameObject>("goRecovery");
			this.utmpTxtDamageTip = prefabBinder.GetObj<Framework.Runtime.UI.UTMPText>("utmpTxtDamageTip");
			this.goDamage = prefabBinder.GetObj<UnityEngine.GameObject>("goDamage");
			this.goRoot = prefabBinder.GetObj<UnityEngine.GameObject>("goRoot");

		}
        public override void OnPlayStartAnimation(Action cb)
        {
			if(Data is FightTextTipData fightTextTipData)
			{
				var sequence = UIAnimator.FindSequence("TipStart");
				if(sequence!=null && (sequence.TryFindUITweener<AnimationControlTweener>(UITweenerType.AnimationControl,out var tweener)))
				{
					var animationClipType = "FightTip_Normal_FloatAnim";

                    if (fightTextTipData.fightTextTipType == FightTextTipType.NormalDamageTip)
					{
                        animationClipType = "FightTip_Normal_FloatAnim";

                    }else if(fightTextTipData.fightTextTipType == FightTextTipType.CritTip)
					{
                        animationClipType = "FightTip_Crit_FloatAnim";
                    }else if(fightTextTipData.fightTextTipType == FightTextTipType.PoisonTip)
					{
                        animationClipType = "FightTip_Poison_FloatAnim";
                    }
					tweener.playAnimName = animationClipType;

                }
                

            }
            base.OnPlayStartAnimation(cb);
        }
		

        public override string GetAssetLink(string outAssetLink)
        {
			string assetPath = "Assets/AddressableResources/UI/Tip/Prefabs/FightTextTip.prefab";
			return AssetPathEncoder.EncodeEnvAssetLink(assetPath, AssetType.PrefabAsset);

		}

        protected override void OnGUI(object data)
        {
			if(data is FightTextTipData option)
			{
				UpdateUIByOption(option);
            }
        }
        public override void BindFollow(Vector3 follow, Camera gameCamera = null, Vector3 offset = default)
        {
            base.BindFollow(follow, gameCamera, offset);
        }
		private void UpdateUIByOption(FightTextTipData option) {
			UpdatePos();
			bool showDamage = false;
			bool showCrit = false;
			bool showPoison = false;
			bool showRecovery = false;
			if(option.fightTextTipType == FightTextTipType.NormalDamageTip)
			{
				showDamage = true;
				utmpTxtDamageTip.text = option.tipValue;
            }
            else if(option.fightTextTipType == FightTextTipType.CritTip)
			{
				showCrit = true;
                utmpTxtCritTip.text = option.tipValue;
            }
            else if (option.fightTextTipType == FightTextTipType.PoisonTip)
            {
                showPoison = true;
                utmpTxtPoisonTip.text = option.tipValue;
            }
            else if (option.fightTextTipType == FightTextTipType.NormalRecoveryTip)
            {
                showRecovery = true;
                utmpTxtRecoveryTip.text = option.tipValue;
            }
			GameObjectUtil.SetActive(this.goRecovery, showRecovery);
			GameObjectUtil.SetActive(this.goDamage, showDamage);
			GameObjectUtil.SetActive(this.goCrit, showCrit);
			GameObjectUtil.SetActive(this.goPoison, showPoison);
		
		}
    }
}





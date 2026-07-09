using Framework.Runtime.MAsset;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleTip
{
    public class TipFactory 
    {
        private static TipOption bounceMsgTipOption;
        private static TipOption commonFightTextTipOption;
        private static TipOption commonTipOption;

        public static void Clear()
        {
            bounceMsgTipOption = null;
            commonFightTextTipOption = null;
            commonTipOption = null;
        }
        public static TipOption GetDefaultBounceMsgTipOption()
        {
            if (bounceMsgTipOption != null)
                return bounceMsgTipOption;

            bounceMsgTipOption = new TipOption();
            bounceMsgTipOption.tipName = "BounceMsgTip";
            bounceMsgTipOption.layer = TipConstant.LAYER_COMMON_TIP;
            bounceMsgTipOption.prefabLink = AssetPathEncoder.EncodeEnvAssetLink("Assets/AddressableResources/UI/Tip/Prefabs/BounceMsgTip.prefab");
            bounceMsgTipOption.tipType = typeof(BounceMsgTip);
            bounceMsgTipOption.popAnchorPos = new UnityEngine.Vector2(0, -200);
            bounceMsgTipOption.size = UnityEngine.Vector2.zero;
            bounceMsgTipOption.data = null;
            bounceMsgTipOption.usePoolTip = true;
            bounceMsgTipOption.autoPut = true;
            bounceMsgTipOption.isCheckPosOverlap = true;

            return bounceMsgTipOption;
        }
        public static TipOption GetDefaultCommonMsgTipOption()
        {
            if (commonTipOption != null)
                return commonTipOption;

            commonTipOption = new TipOption();
            commonTipOption.tipName = "CommonMsgTip";
            commonTipOption.layer = TipConstant.LAYER_COMMON_TIP;
            commonTipOption.prefabLink = AssetPathEncoder.EncodeEnvAssetLink("Assets/AddressableResources/UI/Tip/Prefabs/CommonMsgTip.prefab");
            commonTipOption.tipType = typeof(CommonMsgTip);
            commonTipOption.popAnchorPos = new UnityEngine.Vector2(0, -350);
            commonTipOption.size = UnityEngine.Vector2.zero;
            commonTipOption.data = null;
            commonTipOption.usePoolTip = true;
            commonTipOption.autoPut = true;
            commonTipOption.isCheckPosOverlap = true;

            return commonTipOption;
        }

        public static TipOption GetDefaultFightTextTipOption()
        {
            if (commonFightTextTipOption != null)
                return commonFightTextTipOption;
            commonFightTextTipOption = new TipOption();
            commonFightTextTipOption.layer = TipConstant.LAYER_SCENE_TIP;
            commonFightTextTipOption.tipName = "FightTextTip";
            //bounceMsgTipOption.prefabLink = AssetPathEncoder.EncodeEnvAssetLink("Assets/AddressableResources/UI/Tip/Prefabs/FightTextTip.prefab");
            commonFightTextTipOption.tipType = typeof(FightTextTip);
            commonFightTextTipOption.popAnchorPos = new UnityEngine.Vector2(0, -200);
            commonFightTextTipOption.size = UnityEngine.Vector2.zero;
            commonFightTextTipOption.data = null;
            commonFightTextTipOption.usePoolTip = true;
            commonFightTextTipOption.autoPut = true;

            return commonFightTextTipOption;
        }
    }
}
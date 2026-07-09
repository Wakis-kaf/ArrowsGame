using Framework.Runtime.Config;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Framework.Runtime.UI.PrefabBinderHelp
{
    public enum UITemplateType
    {
        DisplayUnit,
        ListDisplayUnit,
        Panel,
        View,
        Component
    }

    public enum CreateOptionType
    {
        [LabelText("无")]
        Empty,
        [LabelText("UI类型")]
        UIClass,
        [LabelText("引用类型")]
        Reference,
    }
    public enum OpenLayerType
    {
        ExternalLayer,
        NavigationLayer,
        InstructionLayer,
        SceneLayer,
        PanelLayer,
        AlertLayer,
        HighInstructionLayer,
        HighPanelLayer,
        BroadcastLayer,
        TipLayer,
        LoadingLayer,
        DebuggerLayer,
        //    public const int LAYER_EMPTY = -1;
        //public const int LAYER_NAVIGATION = 1;
        //public const int LAYER_INSTRUCTION = 2;
        //public const int LAYER_SCENE = 4;
        //public const int LAYER_PANEL = 5;
        //public const int LAYER_ALERT = 6;
        //public const int LAYER_HIGH_INSTRUCTION = 7;
        //public const int LAYER_HIGH_PANEL = 8;
        //public const int LAYER_BROADCAST = 9;
        //public const int LAYER_TIP = 10;
        //public const int LAYER_LOADING = 11;
        //public const int LAYER_DEBUGGER = 12;


    }
    [ExecuteInEditMode]
    public class PrefabCreateOptions : MonoBehaviour
    {
        [Serializable]
        public class UICreateOptionConfig
        {
            [LabelText("是否自定义视图名称")]
            public bool isCustomFileName;
            [ShowIf("@this.isCustomFileName == true")]
            public string customFileName;
            //[LabelText("是否重写资源路径获取接口")]
            public bool isOverrideAssetLinkGet = true;
            [LabelText("是否重写打开层级")]
            public bool isOverrideOpenLayer = true;
            [ShowIf("IsShowOpenLayer")]
            public OpenLayerType openLayerType = OpenLayerType.ExternalLayer;
            public UITemplateType templateType;
            private bool IsShowOpenLayer()
            {
                return this.isOverrideOpenLayer == true;
            }
            public string GetLayerGetterOverrideTxt()
            {
                switch (openLayerType)
                {
                    case OpenLayerType.ExternalLayer:
                        return "externalLayer";
                    case OpenLayerType.NavigationLayer:
                        return "GlobalConstant.LAYER_NAVIGATION";
                    case OpenLayerType.InstructionLayer:
                        return "GlobalConstant.LAYER_INSTRUCTION";
                    case OpenLayerType.SceneLayer:
                        return "GlobalConstant.LAYER_SCENE";
                    case OpenLayerType.PanelLayer:
                        return "GlobalConstant.LAYER_PANEL";
                    case OpenLayerType.AlertLayer:
                        return "GlobalConstant.LAYER_ALERT";
                    case OpenLayerType.HighInstructionLayer:
                        return "GlobalConstant.LAYER_HIGH_INSTRUCTION";
                    case OpenLayerType.HighPanelLayer:
                        return "GlobalConstant.LAYER_HIGH_PANEL";
                    case OpenLayerType.BroadcastLayer:
                        return "GlobalConstant.LAYER_BROADCAST";
                    case OpenLayerType.TipLayer:
                        return "GlobalConstant.LAYER_TIP";
                    case OpenLayerType.LoadingLayer:
                        return "GlobalConstant.LAYER_LOADING";
                    case OpenLayerType.DebuggerLayer:
                        return "GlobalConstant.LAYER_DEBUGGER";
                    default:
                        return "externalLayer";
                }
            }
        }
        [Serializable]
        public class ReferenceCreateOptionConfig
        {
            [LabelText("是否自定义视图名称")]
            public bool isCustomFileName;
            [ShowIf("@this.isCustomFileName == true")]
            public string customFileName;
            [LabelText("是否使用已有获取器")]
            public bool isUseCustomGetter = true;
            [LabelText("获取器名称")]
            public string customGetterName = "EntityPrefabBinder";
            [LabelText("是否是公共字段")]
            public bool isPublicFiled = false;
            public string GetPrefabBinderName()
            {
                return customGetterName;
            }


        }

        public CreateOptionType createOptionType = CreateOptionType.UIClass;
        [ShowIf("@this.createOptionType", CreateOptionType.UIClass)]
        public UICreateOptionConfig UICreateOption = new UICreateOptionConfig();

        [ShowIf("@this.createOptionType", CreateOptionType.Reference)]
        public ReferenceCreateOptionConfig ReferenceCreateOption = new ReferenceCreateOptionConfig();


        //[ShowIf("@this.createOptionType", CreateOptionType.UIClass)]
        //[LabelText("是否自定义视图名称")]
        //public bool isCustomFileName;

        //[ShowIf("@this.isCustomFileName == true")]
        //[ShowIf("@this.createOptionType",CreateOptionType.UIClass)]
        //[LabelText("自定义试图名称")]
        //public string customFileName;

        //[ShowIf("@this.createOptionType", CreateOptionType.UIClass)]
        //[LabelText("是否重写资源路径获取接口")]
        //public bool isOverrideAssetLinkGet = true;
        //[ShowIf("@this.createOptionType", CreateOptionType.UIClass)]
        //[LabelText("是否重写打开层级")]
        //public bool isOverrideOpenLayer = true;

        //[ShowIf("IsShowOpenLayer")]
        //public OpenLayerType openLayerType = OpenLayerType.ExternalLayer;

        //[ShowIf("@this.createOptionType", CreateOptionType.UIClass)]
        //public UITemplateType templateType;




    }
}
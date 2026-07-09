// ITipOption.cs
using Framework.Runtime.Config;
using UnityEngine;
namespace Game.Modules.GModuleTip
{
    public class TipOption
    {
        public string tipName;
        public int layer = GlobalConstant.LAYER_TIP;
        public string prefabLink;
        public System.Type tipType;
        public Vector2 popAnchorPos;
        public Vector3 worldPos;
        public Vector2 size;
        public object data;
        public bool usePoolTip = true;
        public bool autoPut;
        public bool isCheckPosOverlap;
        
    }
}
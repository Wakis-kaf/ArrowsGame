using UnityEngine;
namespace Game.Modules.GModuleBar
{

    public class BarOption
    {
        public int layer;
        public string barTypeName;
        public string prefabLink;
        public System.Type barType;
        public Vector2 popAnchorPos;
        public Vector2 size;
        public object data;
        public bool usePoolBar;
        public bool autoPut;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Modules.GModuleModelCapture
{
    public enum ModelCaptureType
    {
        CameraCanvas,
        Model
    }
    [System.Serializable]
    public struct ModelCaptureOption
    {
        public string modelPathLink;
        public GameObject model;
        public ModelCaptureType ModelCaptureType;
        public int rtWidth;
        public int rtHeight;
        public Vector3 modelLocalPos;
        public Vector3 birthPostion;
    }
    public static class GameModelCaptureConstant 
    {
        
    }

}

using UnityEngine;

namespace Framework.Runtime.UI
{
    public class FullScreeView : DisplayUnit
    {
        public override void OnOpenInLayer(int layer)
        {
            base.OnOpenInLayer(layer);
            FullScreenAdjust();
        }

        private void FullScreenAdjust()
        {
            Size = UIRoot.CanvasSize;
        }
    }
}
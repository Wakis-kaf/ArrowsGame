using UnityEngine;
using UnityEngine.UI;

namespace Framework.Runtime.UI.Utils
{
    public static class UIExtension
    {
        public static void SetSlicedImage(this Image img, Sprite sprite)
        {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
        }
    }
}
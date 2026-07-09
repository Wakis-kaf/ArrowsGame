using UnityEngine;

namespace Framework.Utils
{
    public static class RectTransformUtil
    {
        public static Vector2 ScreenPointToLocalPointInRectangle(RectTransform targetRectTransform, Vector2 screePoint, Camera camera)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRectTransform, screePoint, camera, out var localPoint);
            return localPoint;
        }

        public static Vector2 ScreenPointToLocalPointInRectangle(RectTransform targetRectTransform, float x, float y, Camera camera)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRectTransform, new Vector2(x, y), camera, out var localPoint);
            return localPoint;
        }

        public static Vector2 ScreenPointToLocalPointInRectangle(RectTransform targetRectTransform, Vector3 screePoint, Camera camera)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRectTransform, screePoint, camera, out var localPoint);
            return localPoint;
        }

        public static void SetAnchorPosition(RectTransform rt, float x, float y)
        {
            rt.anchoredPosition = new Vector2(x, y);
        }
    }
}
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Framework.Runtime.LogSystem;
using Framework.Runtime.Modules.UI.PrefabBind;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

namespace Framework.Runtime.UI
{
    public static class UIUtil
    {
        public static void RefreshLayout(RectTransform transform)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
        }
        public static void RefreshLayoutDelay(RectTransform transform)
        {
            RefreshLayoutUnitaskDelay(transform).Forget();
        }
        private static async UniTask RefreshLayoutUnitaskDelay(RectTransform transform)
        {
            await UniTask.DelayFrame(1);
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
        }
        public static void ExpandToScreenUniversal(RectTransform rect)
        {
            if (rect == null) return;

            // 1. 获取最顶层 Canvas 和它对应的 RectTransform
            Canvas rootCanvas = rect.GetComponentInParent<Canvas>().rootCanvas;
            RectTransform rootRT = rootCanvas.GetComponent<RectTransform>();

            // 2. 将锚点和中心点全部重置到正中心，消除偏移逻辑干扰
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            // 3. 关键点：将根 Canvas 的世界坐标位置赋给子物体
            // 这样无论父物体在哪，子物体都会先对齐屏幕中心
            rect.position = rootRT.position;

            // 4. 计算尺寸转换
            // 我们需要知道：根 Canvas 的 100 像素，在当前子物体的层级下是多少局部像素
            // 公式：子物体 size = 根 Canvas 世界尺寸 / 子物体层级的全局缩放
            Vector3[] rootCorners = new Vector3[4];
            rootRT.GetWorldCorners(rootCorners);
            float worldWidth = Vector3.Distance(rootCorners[0], rootCorners[3]);
            float worldHeight = Vector3.Distance(rootCorners[0], rootCorners[1]);

            // 考虑父物体的 LossyScale（全局缩放）
            Vector3 parentScale = rect.parent != null ? rect.parent.lossyScale : Vector3.one;

            rect.sizeDelta = new Vector2(worldWidth / parentScale.x, worldHeight / parentScale.y);

            // 5. 重置旋转和缩放
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
        }

        public static Rect GetScreenSpaceRect(Camera uiCamera, RectTransform rectTransform, Vector2 offset = default)
        {
            // 1. 获取局部坐标下的四个角
            Vector3[] localCorners = new Vector3[4];
            rectTransform.GetLocalCorners(localCorners);

            // 2. 将局部坐标转换为世界坐标，同时应用 offset
            // 我们只需要左下角和右上角即可计算 Rect
            // 注意：offset 是相对于 RectTransform 自身的中心/锚点的偏移
            Vector3 worldBottomLeft = rectTransform.TransformPoint(localCorners[0] + (Vector3)offset);
            Vector3 worldTopRight = rectTransform.TransformPoint(localCorners[2] + (Vector3)offset);

            // 3. 转换到屏幕空间
            Vector2 screenBottomLeft = uiCamera.WorldToScreenPoint(worldBottomLeft);
            Vector2 screenTopRight = uiCamera.WorldToScreenPoint(worldTopRight);

            // 4. 计算最终 Rect
            float width = screenTopRight.x - screenBottomLeft.x;
            float height = screenTopRight.y - screenBottomLeft.y;

            return new Rect(screenBottomLeft.x, screenBottomLeft.y, width, height);
        }
        public static Canvas AddCanvas(GameObject go, int sortingOrder, int sortingLayer = 0)
        {
            Canvas canvas = go.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = go.AddComponent<Canvas>();
            }

            canvas.enabled = true;
            if (canvas.isRootCanvas)
            {
                Transform transform = go.transform;
                while (transform != null && !(transform.parent == UIRoot.RootTransform))
                {
                    if (transform.parent == null)
                    {
                        AddChild(transform.gameObject, UIRoot.Root.gameObject);
                        break;
                    }

                    transform = transform.parent;
                }
            }

            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
            canvas.sortingLayerID = sortingLayer;
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Tangent;
            GraphicRaycaster graphicRaycaster = go.GetComponent<GraphicRaycaster>();
            if (graphicRaycaster == null)
            {
                graphicRaycaster = go.AddComponent<GraphicRaycaster>();
            }

            graphicRaycaster.enabled = true;
            CanvasRenderer component = go.GetComponent<CanvasRenderer>();
            if (component == null)
            {
                component = go.AddComponent<CanvasRenderer>();
            }

            return canvas;
        }

        public static void AddChild(GameObject child, GameObject parent)
        {
            child.transform.SetParent(parent.transform);
            child.transform.localEulerAngles = Vector3.zero;
            child.transform.localPosition = Vector3.zero;
            child.transform.localScale = Vector3.one;
        }

        public static void ApplayClickEvent(GameObject go)
        {
            if (go == null)
            {
                return;
            }

            IPointerClickHandler[] componentsInChildren = go.GetComponentsInChildren<IPointerClickHandler>();
            if (componentsInChildren != null)
            {
                PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    pointerEventData.pointerPress = go;
                    componentsInChildren[i].OnPointerClick(pointerEventData);
                }
            }
        }

        public static void AutoBangScreenSize(RectTransform trans, float border = 70f)
        {
            trans.pivot = Vector2.one * 0.5f;
            trans.anchorMin = Vector2.zero;
            trans.anchorMax = Vector2.one;
            trans.offsetMin = new Vector2(border, 0f);
            trans.offsetMax = new Vector2(0f - border, 0f);
        }

        public static void AutoSize(RectTransform trans)
        {
            trans.pivot = Vector2.one * 0.5f;
            trans.anchorMin = Vector2.zero;
            trans.anchorMax = Vector2.one;
            Vector2 vector2 = (trans.offsetMin = (trans.offsetMax = Vector2.zero));
        }

        public static Texture2D CloneTexture2D(Texture2D originTex)
        {
            Texture2D newTex;
            newTex = new Texture2D(originTex.width, originTex.height);
            Color[] colors = originTex.GetPixels(0, 0, originTex.width, originTex.height);
            newTex.SetPixels(colors);
            newTex.Apply();
            return newTex;
        }

        public static string Color2Hex(Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }

        public static RectTransform CreateGameObject(GameObject parentObj, string name = "GameObject")
        {
            GameObject gameObject = new GameObject(name);
            gameObject.layer = 5;
            RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
            rectTransform.SetParent(parentObj.transform);
            rectTransform.localEulerAngles = Vector3.zero;
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localScale = Vector3.one;
            return rectTransform;
        }

        public static void EnableEvent(GameObject go, bool enable, bool interactable = true)
        {
            CanvasGroup canvasGroup = go.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = go.AddComponent<CanvasGroup>();
            }

            canvasGroup.blocksRaycasts = enable;
            canvasGroup.interactable = interactable;
        }

        public static float GetGroupAlpha(GameObject go)
        {
            float num = 1f;
            Transform transform = go.transform;
            while (transform != null)
            {
                CanvasGroup component = transform.GetComponent<CanvasGroup>();
                if (component != null)
                {
                    num *= component.alpha;
                    if (component.ignoreParentGroups)
                    {
                        return num;
                    }
                }

                transform = transform.parent;
            }

            return num;
        }

        public static PrefabBinder GetPrefabBinder(GameObject go)
        {
            return go.GetComponent<PrefabBinder>();
        }

        public static string GetRichTextColor(string text, string colorHex)
        {
            //using (zstring.Block())
            //{
            //    return (zstring) $"<color={colorHex}>{text}</color>";
            //}
            return $"<color={colorHex}>{text}</color>";
        }

        public static string GetRichTextColor(string text, Color color)
        {
            //using (zstring.Block())
            //{
            //    return (zstring) $"<color={ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
            //}
            return $"<color={ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
        }

        public static string GetRichTextSize(string text, float fontSize = 12)
        {
            //using (zstring.Block())
            //{
            //    return (zstring) $"<size={fontSize}>{text}</size>";
            //}
            return $"<size={fontSize}>{text}</size>";
        }

        public static UPanel GetUPanel(GameObject go)
        {
            Transform transform = go.transform;
            while (transform != null)
            {
                UPanel component = transform.GetComponent<UPanel>();
                if (component != null)
                {
                    return component;
                }

                transform = transform.parent;
            }

            return null;
        }

        public static int GetUPanelOrder(GameObject go)
        {
            UPanel cPanel = GetUPanel(go);
            if (cPanel != null)
            {
                return cPanel.SortOrder;
            }

            return 0;
        }

        public static Vector2 GetWorld2AnchorPosition(Vector3 worldPosition, RectTransform target)
        {
            return GetWorld2AnchorPosition(UIRootCamera.Camera, worldPosition, target);
        }

        public static Vector2 GetWorld2AnchorPosition(Camera camera, Vector3 worldPosition, RectTransform target)
        {
            Vector3 screenPos = camera.WorldToScreenPoint(worldPosition);
            Vector2 screenPos2D = new Vector2(screenPos.x, screenPos.y);
            Vector2 anchoredPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(target, screenPos2D, camera, out anchoredPos);
            return anchoredPos;
        }
        public static Vector2 GetScreenSizeToRectSizeDelta(Canvas canvas, Vector2 screenSize)
        {
            if (canvas == null) return screenSize;

            // 获取Canvas的缩放因子
            float scaleFactor = canvas.scaleFactor;

            // 将屏幕坐标转换为局部坐标
            Vector2 localSize = screenSize / scaleFactor;


            return localSize;
        }
        public static Vector2 GetScree2ChildAnchorPosWithPivot(Camera camera, Vector2 screenPos2D, RectTransform child)
        {

            if (child == null || child.parent == null) return Vector2.zero;
            RectTransform parent = child.parent as RectTransform;
            // 1. 获取鼠标在父物体本地空间下的坐标
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPos2D, camera, out Vector2 localMousePos))
            {
                // 2. 计算 target 的锚点(Anchor)在父物体坐标系下的像素位置
                Vector2 anchorPosInParent;
                Rect parentRect = parent.rect;
                float anchorX = Mathf.Lerp(parentRect.xMin, parentRect.xMax, (child.anchorMin.x + child.anchorMax.x) * 0.5f);
                float anchorY = Mathf.Lerp(parentRect.yMin, parentRect.yMax, (child.anchorMin.y + child.anchorMax.y) * 0.5f);
                anchorPosInParent = new Vector2(anchorX, anchorY);

                // 3. 【关键：计算居中偏移】
                // 你的 target 尺寸是由所有 grid 撑开的。我们要让它的 Rect 中心对齐鼠标。
                // anchoredPosition 对应的是 Pivot。如果 Pivot 在 (0,1)，
                // 那么中心点相对于 Pivot 的偏移是 (width * 0.5, -height * 0.5)
                Rect targetRect = child.rect;
                Vector2 centerToPivotOffset = new Vector2(
                    (0.5f - child.pivot.x) * targetRect.width,
                    (0.5f - child.pivot.y) * targetRect.height
                );

                // 最终坐标 = (鼠标在父物体位置 - 锚点物理位置) - 中心到Pivot的偏移
                // 这样赋值后，鼠标会指在物体的 Rect 正中心
                return (localMousePos - anchorPosInParent) - centerToPivotOffset;
            }

            return Vector2.zero;
        }
        public static Vector2 GetScree2AnchorPosition(Camera camera, Vector2 screenPos2D, RectTransform target)
        {
            Vector2 anchoredPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(target, screenPos2D, camera, out anchoredPos);
            return anchoredPos;
        }
        public static bool IsRectOverlapping(RectTransform rect1, RectTransform rect2, Camera cam = null)
        {
            Vector3[] corners1 = new Vector3[4];
            Vector3[] corners2 = new Vector3[4];

            rect1.GetWorldCorners(corners1);
            rect2.GetWorldCorners(corners2);

            Vector2 min1 = RectTransformUtility.WorldToScreenPoint(cam, corners1[0]);
            Vector2 max1 = RectTransformUtility.WorldToScreenPoint(cam, corners1[2]);
            Rect screenRect1 = new Rect(min1.x, min1.y, max1.x - min1.x, max1.y - min1.y);

            Vector2 min2 = RectTransformUtility.WorldToScreenPoint(cam, corners2[0]);
            Vector2 max2 = RectTransformUtility.WorldToScreenPoint(cam, corners2[2]);
            Rect screenRect2 = new Rect(min2.x, min2.y, max2.x - min2.x, max2.y - min2.y);

            return screenRect1.Overlaps(screenRect2);
        }
        public static Vector2 GetRect2ScreenPosition(Camera camera, RectTransform target)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, target.position);
            return screenPoint;
        }
        public static Vector2 GetWorl2ScreenPosition(Camera camera, Vector3 worldPosition)
        {
            Vector3 screenPos = camera.WorldToScreenPoint(worldPosition);
            Vector2 screenPos2D = new Vector2(screenPos.x, screenPos.y);
            return screenPos2D;
        }
        public static Vector2 GetWorl2AnchorPosition(Camera worldCamera, Vector3 worldPosition, Camera uiCamera, RectTransform uiRect)
        {
            Vector3 screenPos = worldCamera.WorldToScreenPoint(worldPosition);
            Vector2 screenPos2D = new Vector2(screenPos.x, screenPos.y);
            return GetWorld2AnchorPosition(screenPos2D, uiCamera, uiRect);
        }
        public static Vector2 GetWorld2AnchorPosition(Vector2 screePos, RectTransform rectTransform)
        {
            return GetWorld2AnchorPosition(screePos, UIRootCamera.Camera, rectTransform);
        }

        public static Vector2 GetWorld2AnchorPosition(Vector2 screePos, Camera camera, RectTransform rectTransform)
        {
            Vector2 anchorPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screePos, camera, out anchorPos);
            return anchorPos;
        }

        public static Color Hex2Color(string hexColor)
        {
            if (!hexColor.StartsWith("#")) hexColor = "#" + hexColor;
            ColorUtility.TryParseHtmlString(hexColor, out var nowColor);
            return nowColor;
        }

        /// <summary>
        /// 从外部指定文件中加载图片
        /// </summary>
        /// <returns></returns>
        public static Texture2D LoadTextureByIO()
        {
            FileStream fs = new FileStream(@"D:\" + "图片文件名的全程（包含后缀名）比如  1.png", FileMode.Open, FileAccess.Read);
            fs.Seek(0, SeekOrigin.Begin); //游标的操作，可有可无
            byte[] bytes = new byte[fs.Length]; //生命字节，用来存储读取到的图片字节
            try
            {
                fs.Read(bytes, 0, bytes.Length); //开始读取，这里最好用trycatch语句，防止读取失败报错
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            fs.Close(); //切记关闭

            int width = 2048; //图片的宽（这里两个参数可以提到方法参数中）
            int height =
                2048; //图片的高（这里说个题外话，pico相关的开发，这里不能大于4k×4k不然会显示异常，当时开发pico的时候应为这个问题找了大半天原因，因为美术给的图是6000*3600，导致出现切几张图后就黑屏了。。。
            Texture2D texture = new Texture2D(width, height);
            if (texture.LoadImage(bytes))
            {
                Log.Info("图片加载完毕 ");
                return texture; //将生成的texture2d返回，到这里就得到了外部的图片，可以使用了
            }
            else
            {
                Log.Info("图片尚未加载");
                return null;
            }
        }

        public static void SetAnchor(RectTransform rectTransform, AnchorPresets anchorPresets)
        {
            rectTransform.SetAnchor(anchorPresets);
        }

        public static void SetFullSize(RectTransform child, RectTransform parent)
        {
            if (child == null || parent == null) return;
            Vector2 size = parent.sizeDelta;
            child.sizeDelta = size;
        }

        public static void SetFullSize(RectTransform child, Transform parent)
        {
            SetFullSize(child, parent.GetComponent<RectTransform>());
        }

        public static void SetFullSize(RectTransform child, Component parent)
        {
            SetFullSize(child, parent.GetComponent<RectTransform>());
        }
        public static void SetDimmed(Component cmp, bool value, bool includeChildren = true)
        {
            SetDimmed(cmp.gameObject, value, includeChildren);
        }
        public static void SetDimmed(GameObject go, bool value, bool includeChildren = true)
        {
            if (includeChildren)
            {
                IColor[] componentsInChildren = go.GetComponentsInChildren<IColor>(includeInactive: true);
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    componentsInChildren[i].Dimmed = value;
                }
            }
            else
            {
                IColor component = go.GetComponent<IColor>();
                if (component != null)
                {
                    component.Dimmed = value;
                }
            }
        }
        public static void SetGray(Component cmp, bool value, bool includeChildren = true)
        {
            SetGray(cmp.gameObject, value, includeChildren);
        }
        public static void SetGray(GameObject go, bool value, bool includeChildren = true)
        {
            if (includeChildren)
            {
                IColor[] componentsInChildren = go.GetComponentsInChildren<IColor>(includeInactive: true);
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    componentsInChildren[i].Gray = value;
                }
            }
            else
            {
                IColor component = go.GetComponent<IColor>();
                if (component != null)
                {
                    component.Gray = value;
                }
            }
        }

        public static void SetGroupAlpha(GameObject go, float alpha)
        {
            CanvasGroup canvasGroup = go.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = go.AddComponent<CanvasGroup>();
            }

            if (canvasGroup != null)
            {
                DOTween.Kill(canvasGroup);
            }

            canvasGroup.alpha = alpha;
        }

        public static void SetGroupAlphaInTime(GameObject go, float alpha, float time, Ease type = Ease.Linear, TweenCallback callBack = null)
        {
            CanvasGroup canvasGroup = go.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = go.AddComponent<CanvasGroup>();
            }

            if (canvasGroup != null)
            {
                DOTween.Kill(canvasGroup);
            }
            Tween t = canvasGroup.DOFade(alpha, time).SetEase(type);
            if (callBack != null)
            {
                t.OnComplete(callBack);
            }
        }

        public static void SetLayer(GameObject go, int layer)
        {
            go.layer = layer;
            Transform transform = go.transform;
            int i = 0;
            for (int childCount = transform.childCount; i < childCount; i++)
            {
                Transform child = transform.GetChild(i);
                SetLayer(child.gameObject, layer);
            }
        }

        public static void SetOffsetZero(RectTransform rectTransform)
        {
            rectTransform.SetOffsetZero();
        }

        public static void SetSelectedGameObject(GameObject go)
        {
            EventSystem.current.SetSelectedGameObject(go);
        }

        public static void SetUIEventCurSelect(GameObject go)
        {
            EventSystem.current.SetSelectedGameObject(go);
        }

        public static Texture2D SpriteToTexture2D(Sprite sprite)
        {
            if (sprite == null) return null;
            var targetTex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            var pixels = sprite.texture.GetPixels(
                (int)sprite.textureRect.x,
                (int)sprite.textureRect.y,
                (int)sprite.textureRect.width,
                (int)sprite.textureRect.height);
            targetTex.SetPixels(pixels);
            targetTex.Apply();
            return targetTex;
        }

        public static void SwapChildIndex(GameObject childA, GameObject childB)
        {
            int siblingIndex = childA.transform.GetSiblingIndex();
            int siblingIndex2 = childB.transform.GetSiblingIndex();
            childB.transform.SetSiblingIndex(siblingIndex);
            childA.transform.SetSiblingIndex(siblingIndex2);
        }

        public static Sprite Texture2DToSprite(Texture2D t2d)
        {
            Sprite s = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
            s.name = t2d.name;
            return s;
        }

        /// <summary>
        /// 将Texture2d转换为Sprite
        /// </summary>
        /// <param name="tex">参数是texture2d纹理</param>
        /// <returns></returns>
        public static Sprite TextureToSprite(Texture2D tex, float width, float height)
        {
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
            return sprite;
        }

        public static Sprite TextureToSprite(Texture2D tex)
        {
            return TextureToSprite(tex, tex.width, tex.height);
        }

        public static float WorldToScreenSpace(Camera gameCamera, float distance)
        {
            if (gameCamera == null || distance <= 0f) return 0f;

            if (gameCamera.orthographic)
            {
                return (distance / gameCamera.orthographicSize) * (UnityEngine.Screen.height * 0.5f);
            }

            UnityEngine.Vector3 cameraPos = gameCamera.transform.position;
            UnityEngine.Vector3 forward = gameCamera.transform.forward;

            float t = Mathf.Abs(forward.z) > 0.0001f ? -cameraPos.z / forward.z : 10f;
            if (t < 0f) t = 10f;

            UnityEngine.Vector3 centerWorldPos = cameraPos + forward * t;
            UnityEngine.Vector3 offsetWorldPos = centerWorldPos + gameCamera.transform.right * distance;

            UnityEngine.Vector2 centerScreenPos = gameCamera.WorldToScreenPoint(centerWorldPos);
            UnityEngine.Vector2 offsetScreenPos = gameCamera.WorldToScreenPoint(offsetWorldPos);

            return UnityEngine.Vector2.Distance(centerScreenPos, offsetScreenPos);
        }
    }
}
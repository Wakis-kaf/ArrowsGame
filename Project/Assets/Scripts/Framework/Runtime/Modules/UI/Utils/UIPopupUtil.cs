using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.GraphicsBuffer;
namespace Framework.Runtime.UI
{
    public enum PopupAxis
    {
        Top,
        Right,
        Left,
        Bottom,
    }

    public static class UIPopupUtil 
    {
        #region 点吸附

        public static bool TryGetSnapPosition(
     Vector2 pointScreenPos,
     RectTransform popRectTransform,
     Camera uiCamera,
     Vector2 snapAnchor,
     out Vector2 targetAnchorPosition,
     RectTransform clipRectTransform = null,
     Vector2 space = default, // 修改为 Vector2
     bool allowFlexSpace = true,
     bool allowWrap = false)
        {
            targetAnchorPosition = Vector2.zero;
            if (popRectTransform == null) return false;

            RectTransform parentRT = popRectTransform.parent as RectTransform;
            if (parentRT == null) return false;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, pointScreenPos, uiCamera, out Vector2 localTargetPoint))
            {
                return false;
            }

            Vector2 size = popRectTransform.rect.size;
            Vector2 pivot = popRectTransform.pivot;

            // 视觉点坐标映射
            Vector2 visualNormalized = (snapAnchor + Vector2.one) * 0.5f;
            Vector2 offsetFromPivot = new Vector2(
                (visualNormalized.x - pivot.x) * size.x,
                (visualNormalized.y - pivot.y) * size.y
            );

            // 初始计算位置：目标点 - 内部偏移 - 外部间距
            // 注意：direction 由 snapAnchor 决定，space 分量分别作用于 x 和 y
            Vector2 direction = snapAnchor.normalized;
            Vector2 currentSpace = space;
            Vector2 posWithSpace = localTargetPoint - offsetFromPivot - Vector2.Scale(direction, currentSpace);

            // 边界检测与调整
            if (AdjustPosition(ref posWithSpace, currentSpace, direction, popRectTransform, parentRT, clipRectTransform, uiCamera, allowFlexSpace, allowWrap, out Vector2 finalPos))
            {
                targetAnchorPosition = finalPos;
                return true;
            }

            targetAnchorPosition = posWithSpace;
            return true;
        }

        private static bool AdjustPosition(
            ref Vector2 pos,
            Vector2 originalSpace,
            Vector2 direction,
            RectTransform pop,
            RectTransform parent,
            RectTransform clip,
            Camera cam,
            bool allowFlex,
            bool allowWrap,
            out Vector2 finalPos)
        {
            finalPos = pos;
            Vector2 size = pop.rect.size;
            Vector2 pivot = pop.pivot;

            // 获取限制区域
            Rect boundaryRect;
            if (clip != null)
            {
                Vector3[] corners = new Vector3[4];
                clip.GetWorldCorners(corners);
                Vector2 min = parent.InverseTransformPoint(corners[0]);
                Vector2 max = parent.InverseTransformPoint(corners[2]);
                boundaryRect = new Rect(min, max - min);
            }
            else
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, Vector2.zero, cam, out Vector2 min);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, new Vector2(Screen.width, Screen.height), cam, out Vector2 max);
                boundaryRect = new Rect(min, max - min);
            }

            System.Func<Vector2, Rect> getPopRect = (p) => new Rect(p - pivot * size, size);
            Rect currentRect = getPopRect(finalPos);

            // 检查溢出
            bool xOver = currentRect.xMin < boundaryRect.xMin || currentRect.xMax > boundaryRect.xMax;
            bool yOver = currentRect.yMin < boundaryRect.yMin || currentRect.yMax > boundaryRect.yMax;

            if (xOver || yOver)
            {
                // 1. 处理 FlexSpace: 独立缩减 X 或 Y 的 space
                if (allowFlex)
                {
                    if (xOver) finalPos.x += direction.x * originalSpace.x;
                    if (yOver) finalPos.y += direction.y * originalSpace.y;

                    currentRect = getPopRect(finalPos);
                    xOver = currentRect.xMin < boundaryRect.xMin || currentRect.xMax > boundaryRect.xMax;
                    yOver = currentRect.yMin < boundaryRect.yMin || currentRect.yMax > boundaryRect.yMax;
                }

                // 2. 如果依然溢出，处理 Wrap 或直接 Clamp
                if (xOver || yOver)
                {
                    if (allowWrap)
                    {
                        // 如果允许 Wrap，原本向外的 space 变成向内（取反）
                        // 并且强制限制在边界内
                        finalPos.x = Mathf.Clamp(finalPos.x, boundaryRect.xMin + pivot.x * size.x, boundaryRect.xMax - (1 - pivot.x) * size.x);
                        finalPos.y = Mathf.Clamp(finalPos.y, boundaryRect.yMin + pivot.y * size.y, boundaryRect.yMax - (1 - pivot.y) * size.y);
                    }
                    else
                    {
                        // 不允许 Wrap 则直接硬性 Clamp
                        finalPos.x = Mathf.Clamp(finalPos.x, boundaryRect.xMin + pivot.x * size.x, boundaryRect.xMax - (1 - pivot.x) * size.x);
                        finalPos.y = Mathf.Clamp(finalPos.y, boundaryRect.yMin + pivot.y * size.y, boundaryRect.yMax - (1 - pivot.y) * size.y);
                    }
                }
            }

            return true;
        }
        #endregion
        private struct PopupInfo
        {
            public RectTransform fromRect;
            public RectTransform popRect;
            public Camera uiCamera;
            public PopupAxis axis;
            public float space;
            public RectTransform clipTransform;
            public bool flexSpace;
            public bool coverEnable;
        }
        public static bool GetPopAnchorPos(
            RectTransform fromRect,
            RectTransform popRect,
            Camera uiCamera,
            out Vector2 anchorPos,
            RectTransform clipTransform = null,
            PopupAxis axis = PopupAxis.Top,
            float space =10f,
            bool flexSpace = true,
            bool coverEnable = false,
            Vector2 offset = default
            )
        {
           Rect fromScreeRect = UIUtil.GetScreenSpaceRect(uiCamera, fromRect, offset);
           Rect popScreeRect = UIUtil.GetScreenSpaceRect(uiCamera, popRect);
           Rect clipRect = clipTransform == null ? new Rect(0, 0, Screen.width, Screen.height) :
                UIUtil.GetScreenSpaceRect(uiCamera, clipTransform);
            Rect putRect = GetPopSpaceRectInAxis(axis, fromScreeRect, popScreeRect, clipRect);
            if(CanPutInAxis(putRect, fromScreeRect,popScreeRect, axis,space,flexSpace,coverEnable,out var popScreenPos))
            {
                anchorPos = TransformAxisPos2Anhor(axis, popScreenPos, uiCamera, popRect);
                return true;
            }
            axis = GetOppositeAxis(axis);
            fromScreeRect = UIUtil.GetScreenSpaceRect(uiCamera, fromRect, -offset);
            putRect = GetPopSpaceRectInAxis(axis, fromScreeRect, popScreeRect, clipRect);
            if (CanPutInAxis(putRect, fromScreeRect, popScreeRect, axis, space, flexSpace, coverEnable, out  popScreenPos))
            {
                anchorPos = TransformAxisPos2Anhor(axis, popScreenPos, uiCamera, popRect);
                return true;
            }
            anchorPos= popRect.anchoredPosition;
            return false;
        }
        private static Vector2 TransformAxisPos2Anhor(PopupAxis axis,
            Vector2 pupScreenPos,
            Camera uiCamera,
            RectTransform popRect)
        {
            Rect popScreeRect = UIUtil.GetScreenSpaceRect(uiCamera, popRect);
            Vector2Int dir = GetSpaceDir(axis);
            float x = pupScreenPos.x + dir.x*(popScreeRect.width / 2f);
            float y = pupScreenPos.y + dir.y*(popScreeRect.height / 2f);
            Vector2 popCenterScreePos = new Vector2(x, y);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(popRect.parent as RectTransform,
                popCenterScreePos, uiCamera, 
                out var centerAnchorPos);

            return centerAnchorPos;
        }
        private static PopupAxis GetOppositeAxis(PopupAxis axis)
        {
            if (axis == PopupAxis.Top)
            {
                return PopupAxis.Bottom;
            }
            else if (axis == PopupAxis.Bottom)
            {
                return PopupAxis.Top;
            }
            else if (axis == PopupAxis.Left)
            {
                return PopupAxis.Right;
            }
            else if (axis == PopupAxis.Right)
            {
                return PopupAxis.Left;
            }
            return PopupAxis.Top;
        }
        private static Vector2Int GetSpaceDir(PopupAxis axis)
        {
            if (axis == PopupAxis.Top)
            {
                return Vector2Int.up;
            }
            else if (axis == PopupAxis.Bottom)
            {
                return Vector2Int.down;
            }
            else if (axis == PopupAxis.Left)
            {
                return Vector2Int.left;
            }
            else if (axis == PopupAxis.Right)
            {
                return Vector2Int.right;
            }
            return Vector2Int.zero;
        }
        private static bool CanPutInAxis(Rect spaceRect, 
            Rect fromRect,
            Rect popRect,
            PopupAxis axis,
            float space,
            bool flexSpace,
            bool coverEnable,
            out Vector2 putPos)
        {
            if(axis == PopupAxis.Top)
            {
                return CanPutInTop(spaceRect, fromRect, popRect, axis, space, flexSpace, coverEnable, out putPos);
            }else if (axis == PopupAxis.Bottom)
            {
                return CanPutInBottom(spaceRect, fromRect, popRect, axis, space, flexSpace, coverEnable, out putPos);
            }
            else if (axis == PopupAxis.Left)
            {
                return CanPutInLeft(spaceRect, fromRect, popRect, axis, space, flexSpace, coverEnable, out putPos);
            }
            else if (axis == PopupAxis.Right)
            {
                return CanPutInRight(spaceRect, fromRect, popRect, axis, space, flexSpace, coverEnable, out putPos);
            }
            putPos = Vector2.zero;
            return false;
        }


        private static bool CanPutInTop(Rect spaceRect, 
          Rect fromRect,
          Rect popRect,
          PopupAxis axis,
          float space,
          bool flexSpace,
          bool coverEnable,
          out Vector2 putPos)
        {
            float popHeight = popRect.height;
            float spaceHeight = spaceRect.height;
            float freeSpaceHeigth = spaceHeight - popHeight;
            bool pusAble = false;
            float x = fromRect.x + fromRect.width / 2;
            float offsetLeftX = Mathf.Max( 0 -(x-popRect.width/2),0); // 大于0说明需要向右偏差
            float offsetRightX = Mathf.Max(x + popRect.width / 2 - spaceRect.width,0); // 大于0说明需要向左偏差
            float startY = spaceRect.y;
            float y = 0;
            if (freeSpaceHeigth >= 0)
            {
                if(freeSpaceHeigth >= space)
                {
                    pusAble = true;
                    y = startY + space;
                }else if (flexSpace)
                {
                    pusAble = true;
                    y = startY + freeSpaceHeigth;
                }
            }
            else if (coverEnable)
            {
                pusAble = true;
                y = startY - freeSpaceHeigth;
            }

            if (!pusAble)
            {
                putPos = Vector2.zero;
                return false;
            }

            putPos = new Vector2(x+offsetLeftX-offsetRightX, y);
            return true;           
        }
        private static bool CanPutInBottom(Rect spaceRect,
                 Rect fromRect, Rect popRect,
         PopupAxis axis,
         float space,
         bool flexSpace,
         bool coverEnable,
         out Vector2 putPos)
        {
            float popHeight = popRect.height;
            float spaceHeight = spaceRect.height;
            float freeSpaceHeigth = spaceHeight - popHeight;
            bool pusAble = false;
            float x = fromRect.x + fromRect.width / 2;
            float offsetLeftX = Mathf.Max(0 - (x - popRect.width / 2), 0); // 大于0说明需要向右偏差
            float offsetRightX = Mathf.Max(x + popRect.width / 2 - spaceRect.width, 0); // 大于0说明需要向左偏差
            float startY = spaceRect.y+spaceRect.height;
            float y = 0;
            if (freeSpaceHeigth >= 0)
            {
                if (freeSpaceHeigth >= space)
                {
                    pusAble = true;
                    y = startY - space;
                }
                else if (flexSpace)
                {
                    pusAble = true;
                    y = startY - freeSpaceHeigth;
                }
            }
            else if (coverEnable)
            {
                pusAble = true;
                y = startY - freeSpaceHeigth;
            }

            if (!pusAble)
            {
                putPos = Vector2.zero;
                return false;
            }

            putPos = new Vector2(x + offsetLeftX - offsetRightX, y);
            return true;
        }


        private static bool CanPutInLeft(Rect spaceRect,
                 Rect fromRect, Rect popRect,
          PopupAxis axis,
          float space,
          bool flexSpace,
          bool coverEnable,
          out Vector2 putPos)
        {
            float popWidth = popRect.width;
            float spaceWidth = spaceRect.width;
            float freeSpaceHeigth = spaceWidth - popWidth;
            bool pusAble = false;
            float y = fromRect.y + fromRect.height / 2;
            float offsetBottomY = Mathf.Max(0 - (y - popRect.height / 2), 0); // 大于0说明需要向上偏差
            float offsetTopY = Mathf.Max(y + popRect.height / 2 - spaceRect.height, 0); // 大于0说明需要向下偏差
            float startX = spaceRect.x + spaceRect.width;
            float x = 0;

            if (freeSpaceHeigth >= 0)
            {
                if (freeSpaceHeigth >= space)
                {
                    pusAble = true;
                    x = startX - space;
                }
                else if (flexSpace)
                {
                    pusAble = true;
                    x = startX - freeSpaceHeigth;
                }
            }
            else if (coverEnable)
            {
                pusAble = true;
                x = startX - freeSpaceHeigth;
            }

            if (!pusAble)
            {
                putPos = Vector2.zero;
                return false;
            }

            putPos = new Vector2(x, y+ offsetBottomY- offsetTopY);
            return true;
        }
        private static bool CanPutInRight(Rect spaceRect,
                 Rect fromRect, Rect popRect,
          PopupAxis axis,
          float space,
          bool flexSpace,
          bool coverEnable,
          out Vector2 putPos)
        {
            float popWidth = popRect.width;
            float spaceWidth = spaceRect.width;
            float freeSpaceHeigth = spaceWidth - popWidth;
            bool pusAble = false;
            float y = fromRect.y + fromRect.height / 2;
            float offsetBottomY = Mathf.Max(0 - (y - popRect.height / 2), 0); // 大于0说明需要向上偏差
            float offsetTopY = Mathf.Max(y + popRect.height / 2 - spaceRect.height, 0); // 大于0说明需要向下偏差
            float startX = spaceRect.x;
            float x = 0;

            if (freeSpaceHeigth >= 0)
            {
                if (freeSpaceHeigth >= space)
                {
                    pusAble = true;
                    x = startX + space;
                }
                else if (flexSpace)
                {
                    pusAble = true;
                    x = startX + freeSpaceHeigth;
                }
            }
            else if (coverEnable)
            {
                pusAble = true;
                x = startX + freeSpaceHeigth;
            }

            if (!pusAble)
            {
                putPos = Vector2.zero;
                return false;
            }

            putPos = new Vector2(x, y + offsetBottomY - offsetTopY);
            return true;
        }

        private static Rect GetPopSpaceRectInAxis(PopupAxis axis, 
            Rect fromScreeRect,
            Rect popScreeRect,
            Rect clipRect)
        {

            float startX = 0;
            float startY = 0;
            float width = 0;
            float height = 0;

            if(axis == PopupAxis.Top)
            {
                width =clipRect.width;
                startX = clipRect.x;
                startY = fromScreeRect.y + fromScreeRect.height;
                height = clipRect.height- startY;
            }else if(axis== PopupAxis.Bottom)
            {
                width = clipRect.width;
                startX = clipRect.x;
                startY = clipRect.y;
                height = fromScreeRect.y - startY;
            }else if(axis == PopupAxis.Left)
            {
                startX = clipRect.x;
                startY = clipRect.y;
                height = clipRect.height;
                width = fromScreeRect.x-startX;
            }
            else if (axis == PopupAxis.Right)
            {
                startY = clipRect.y;
                height = clipRect.height;
                startX = fromScreeRect.x+fromScreeRect.width;
                width = clipRect.width - startX;
            }
            return new Rect(startX, startY, width, height);
        }

      

    }
}
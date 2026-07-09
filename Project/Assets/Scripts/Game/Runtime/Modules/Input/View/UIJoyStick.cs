using Framework.Runtime.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum JoystickType
{
    Fixed,      //固定式摇杆
    Floating,   //浮动式摇杆(根据点击屏幕的位置生成摇杆控制器)
    Dynamic     //动态摇杆(摇杆可以被动态拖拽)
}

public class UIJoyStick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform background = null;
    // 新增：限制移动范围的RectTransform
    public RectTransform boundaryRect = null;

    public RectTransform detectRect = null;

    public RectTransform handler = null;
    public Canvas inCanvas;
    public Vector2 input = Vector2.zero;
    public JoystickType joystickType = JoystickType.Fixed;
    public float MoveThreshold = 1;
    public Camera uiCamera;
    private Vector2 center = new Vector2(0.5f, 0.5f);
    private float deadZone = 0;
    private Vector2 fixedPosition = Vector2.zero;
    private Action<Vector2> OnDragingCb;
    private bool isDragging = false; // 新增：标记是否正在拖拽

    public float DeadZone
    {
        get { return deadZone; }
        set { deadZone = Mathf.Abs(value); }
    }

    public void AddDragCb(Action<Vector2> cb)
    {
        OnDragingCb -= cb;
        OnDragingCb += cb;
    }

    public void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        if (joystickType == JoystickType.Dynamic && magnitude > MoveThreshold)
        {
            Vector2 difference = normalised * (magnitude - MoveThreshold) * radius;
            Vector2 newPosition = background.anchoredPosition + difference;

            // 限制新位置在边界内
            newPosition = ClampPositionToBoundary(newPosition);
            background.anchoredPosition = newPosition;
        }
        if (magnitude > deadZone)
        {
            if (magnitude > 1)
                input = normalised;
        }
        else
        {
            input = Vector2.zero;
        }
        OnDragingCb?.Invoke(input);
    }

    public void Init()
    {
        if (background == null || handler == null || boundaryRect == null || detectRect == null)
        {
            return;
        }
        background.pivot = center;
        handler.anchorMin = center;
        handler.anchorMax = center;
        handler.pivot = center;
        handler.anchoredPosition = Vector2.zero;
        fixedPosition = background.anchoredPosition;
        SetMode();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 只有正在拖拽状态才处理拖拽
        if (!isDragging) return;

        Vector2 position = UIRootCamera.Camera.WorldToScreenPoint(background.position);//将ui坐标中的background映射到屏幕中的实际坐标
        Vector2 radius = background.sizeDelta / 2;
        input = (eventData.position - position) / (radius * inCanvas.scaleFactor);//将屏幕中的触点和background的距离映射到ui空间下实际的距离
        HandleInput(input.magnitude, input.normalized, radius, uiCamera);        //对输入进行限制
        handler.anchoredPosition = input * radius;                              //实时计算handle的位置
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 检查点击位置是否在detectRect范围内
        if (!IsPointInDetectRect(eventData.position))
        {
            return;
        }

        isDragging = true;

        if (joystickType != JoystickType.Fixed)
        {
            Vector2 anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
            // 限制初始位置在边界内
            anchoredPosition = ClampPositionToBoundary(anchoredPosition);
            background.anchoredPosition = anchoredPosition;
            background.gameObject.SetActive(true);
        }
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 只有正在拖拽状态才处理抬起
        if (!isDragging) return;

        isDragging = false;

        if (joystickType != JoystickType.Fixed)
            background.gameObject.SetActive(false);
        input = Vector2.zero;
        handler.anchoredPosition = Vector2.zero;
        OnDragingCb?.Invoke(input);
    }

    public void RemoveDragCb(Action<Vector2> cb)
    {
        OnDragingCb -= cb;
    }

    public void SetMode()
    {
        if (joystickType == JoystickType.Fixed)
        {
            background.anchoredPosition = fixedPosition;
            background.gameObject.SetActive(true);
        }
        else
            background.gameObject.SetActive(false);
    }

    protected virtual void Start()
    {
        Init();
    }

    /// <summary>
    /// 检查点击位置是否在detectRect范围内
    /// </summary>
    private bool IsPointInDetectRect(Vector2 screenPosition)
    {
        if (detectRect == null) return false;

        // 将屏幕坐标转换为detectRect的本地坐标
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(detectRect, screenPosition, uiCamera, out localPoint);

        // 检查点是否在矩形范围内
        Rect rect = detectRect.rect;
        return rect.Contains(localPoint);
    }

    /// <summary>
    /// 将位置限制在边界RectTransform范围内
    /// </summary>
    private Vector2 ClampPositionToBoundary(Vector2 position)
    {
        if (boundaryRect == null) return position;

        // 获取边界矩形的范围
        Vector2 boundaryMin = boundaryRect.rect.min;
        Vector2 boundaryMax = boundaryRect.rect.max;

        // 将边界转换到当前坐标空间
        Vector2 localBoundaryMin = boundaryRect.TransformPoint(boundaryMin);
        Vector2 localBoundaryMax = boundaryRect.TransformPoint(boundaryMax);

        // 将边界转换到baseRect的本地坐标空间
        Vector2 minPos = detectRect.InverseTransformPoint(localBoundaryMin);
        Vector2 maxPos = detectRect.InverseTransformPoint(localBoundaryMax);

        // 限制位置在边界内
        float clampedX = Mathf.Clamp(position.x, minPos.x, maxPos.x);
        float clampedY = Mathf.Clamp(position.y, minPos.y, maxPos.y);

        return new Vector2(clampedX, clampedY);
    }

    /// <summary>
    /// 简化的边界限制方法（如果上面的方法有问题可以使用这个）
    /// </summary>
    private Vector2 ClampPositionToBoundarySimple(Vector2 position)
    {
        if (boundaryRect == null) return position;

        // 获取背景摇杆的半径
        float backgroundRadius = background.sizeDelta.x * 0.5f;

        // 获取边界矩形的边界（在boundaryRect的本地坐标空间）
        Rect boundary = boundaryRect.rect;

        // 限制位置在边界内，考虑摇杆的半径
        float minX = boundary.xMin + backgroundRadius;
        float maxX = boundary.xMax - backgroundRadius;
        float minY = boundary.yMin + backgroundRadius;
        float maxY = boundary.yMax - backgroundRadius;

        float clampedX = Mathf.Clamp(position.x, minX, maxX);
        float clampedY = Mathf.Clamp(position.y, minY, maxY);

        return new Vector2(clampedX, clampedY);
    }

    private Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
    {
        Vector2 localPoint = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(detectRect, screenPosition, uiCamera, out localPoint);
        return localPoint;
    }

    // 新增：获取当前是否正在拖拽
    public bool IsDragging()
    {
        return isDragging;
    }

    // 新增：强制停止拖拽
    public void ForceStop()
    {
        if (isDragging)
        {
            isDragging = false;
            if (joystickType != JoystickType.Fixed)
                background.gameObject.SetActive(false);
            input = Vector2.zero;
            handler.anchoredPosition = Vector2.zero;
            OnDragingCb?.Invoke(input);
        }
    }
}
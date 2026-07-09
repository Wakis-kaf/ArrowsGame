using Framework.Runtime.UI;

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Runtime.MDebugger
{
    public class UnitFrameDebugger : MonoBehaviour
    {
        private UButton m_BtnDragTitle;
        private UButton m_BtnFpsCounter;
        private Vector2 m_DragStartOffset;
        private Vector2 m_LastFoldPosition;
        private RectTransform m_ObjExpand;
        private RectTransform m_ObjFold;
        private Action m_OnExpand; // 展开的时候触发
        private Action m_OnFold; // 折叠的时候触发
        private RectTransform m_RectTransform;
        private RectTransform m_SelfRectTransform;

        public bool IsExpand
        {
            get { return m_ObjExpand.gameObject.activeInHierarchy; }
        }

        private RectTransform rectTransform
        {
            get
            {
                if (m_RectTransform == null)
                    m_RectTransform = GetComponent<RectTransform>();
                return m_RectTransform;
            }
        }

        public void Expand()
        {
            m_ObjFold.gameObject.SetActive(false);
            m_ObjExpand.gameObject.SetActive(true);
            m_LastFoldPosition = m_ObjFold.anchoredPosition;
            m_ObjFold.anchoredPosition = Vector2.zero;
        }

        public void Fold()
        {
            m_ObjFold.gameObject.SetActive(true);
            m_ObjExpand.gameObject.SetActive(false);
            m_ObjFold.anchoredPosition = m_LastFoldPosition;
        }

        public void SetFoldFps(float fps)
        {
            if (!m_BtnFpsCounter.IsActive()) return;
            m_BtnFpsCounter.Text = fps.ToString();
        }

        private void Awake()
        {
            // 获取基本的组件
            m_ObjFold = transform.Find("ObjFold").GetComponent<RectTransform>();
            m_ObjExpand = transform.Find("ObjExpand").GetComponent<RectTransform>();
            m_ObjFold.gameObject.SetActive(true);
            m_ObjExpand.gameObject.SetActive(true);

            m_BtnDragTitle = transform.GetComponentInChild<UButton>("ubtnDragTitle");
            m_BtnDragTitle.AddDrag(OnDragPressing);
            
            m_BtnDragTitle.AddPointerDown(OnPointerDown);
            
            m_BtnFpsCounter = transform.GetComponentInChild<UButton>("ubtnFpsCounter");
            m_BtnFpsCounter.AddDrag(OnDragPressing);
            m_BtnFpsCounter.AddPointerDown(OnPointerDown);
            m_BtnFpsCounter.AddClick(Expand);
            m_SelfRectTransform = GetComponent<RectTransform>();

            // 设置初始位置
            SetToTopLeftCorner();
            m_LastFoldPosition = m_ObjFold.anchoredPosition;
            m_ObjExpand.gameObject.SetActive(false);
        }

        /// <summary>
        /// 将fold设置到父元素的左上角
        /// </summary>
        private void SetToTopLeftCorner()
        {
            if (m_ObjFold == null) return;

            RectTransform parentRect = m_ObjFold.parent as RectTransform;
            if (parentRect == null) return;

            // 设置锚点为左上角
            m_ObjFold.anchorMin = new Vector2(0, 1);
            m_ObjFold.anchorMax = new Vector2(0, 1);
            m_ObjFold.pivot = new Vector2(0, 1); // 基准点在左上角

            // 放置在左上角
            m_ObjFold.anchoredPosition = Vector2.zero;
            m_LastFoldPosition = Vector2.zero;
        }
        

        /// <summary>
        /// 限制位置在父元素范围内
        /// </summary>
        private void ClampPosition(ref Vector2 localPos)
        {
            RectTransform parentRect = m_ObjFold.parent as RectTransform;
            if (parentRect == null) return;

            Vector2 parentSize = parentRect.rect.size;
            Vector2 foldSize = m_ObjFold.rect.size;

            // 计算在父元素坐标系中的边界
            // 由于锚点在左上角，所以X范围是[0, 父宽度-fold宽度]
            // Y范围是[-(父高度-fold高度), 0] (Y轴向下为正)
            float minX = 0;
            float maxX = parentSize.x - foldSize.x;
            float minY = -parentSize.y + foldSize.y;
            float maxY = 0;

            localPos.x = Mathf.Clamp(localPos.x, minX, maxX);
            localPos.y = Mathf.Clamp(localPos.y, minY, maxY);
        }

        private void FollowMouseMove(Vector2 screenPos)
        {
            if (m_ObjFold == null) return;

            RectTransform parentRect = m_ObjFold.parent as RectTransform;
            if (parentRect == null) return;

            // 将屏幕坐标转换为父元素下的局部坐标
            Vector2 localPos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, screenPos, UIRootCamera.Camera, out localPos))
            {
                // 应用拖拽偏移量
                Vector2 targetPosition = localPos - m_DragStartOffset;

                // 限制位置在父元素范围内
                ClampPosition(ref targetPosition);
                m_ObjFold.anchoredPosition = targetPosition;
                m_LastFoldPosition = targetPosition;
            }
        }

        private void OnDragPressing()
        {
            FollowMouseMove(Input.mousePosition);
        }

        private void OnPointerDown(PointerEventData pointerEventData)
        {
            RectTransform parentRect = m_ObjFold.parent as RectTransform;
            if (parentRect != null)
            {
                Vector2 localPos;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect, pointerEventData.position, UIRootCamera.Camera, out localPos))
                {
                    // 计算点击位置与当前对象位置的偏移量
                    m_DragStartOffset = localPos - m_ObjFold.anchoredPosition;
                }
            }
        }
    }
}
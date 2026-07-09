using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Runtime.UI
{
    public class EventReceiver : MonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler,
        IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private bool m_IsDrag;
        private bool m_IsPointerHovering;
        private bool m_IsPointerPressing;

        private event Action<PointerEventData> OnBeginDragDelegate;

        private event Action<PointerEventData> OnDragDelegate;

        private event Action<PointerEventData> OnEndDragDelegate;

        private event Action<PointerEventData> OnPointerClickDelegate;

        private event Action<PointerEventData> OnPointerDownDelegate;

        private event Action<PointerEventData> OnPointerEnterDelegate;

        private event Action<PointerEventData> OnPointerExitDelegate;

        private event Action<PointerEventData> OnPointerUpDelegate;

        private event Action<PointerEventData> OnPointMoveDelegate;

        public bool IsDraging => m_IsDrag;
        public bool IsPointerHovering => m_IsPointerHovering;

        public bool IsPointerPressing => m_IsPointerPressing;

        public void AddBeginDrag(Action<PointerEventData> listener)
        {
            OnBeginDragDelegate -= listener;
            OnBeginDragDelegate += listener;
        }

        public void AddClick(Action<PointerEventData> listener)
        {
            OnPointerClickDelegate -= listener;
            OnPointerClickDelegate += listener;
        }

        public void AddDrag(Action<PointerEventData> listener)
        {
            OnDragDelegate -= listener;
            OnDragDelegate += listener;
        }

        public void AddEndDrag(Action<PointerEventData> listener)
        {
            OnEndDragDelegate -= listener;
            OnEndDragDelegate += listener;
        }

        public void AddPointerDown(Action<PointerEventData> listener)
        {
            OnPointerDownDelegate -= listener;
            OnPointerDownDelegate += listener;
        }

        public void AddPointerEnter(Action<PointerEventData> listener)
        {
            OnPointerEnterDelegate -= listener;
            OnPointerEnterDelegate += listener;
        }

        public void AddPointerExit(Action<PointerEventData> listener)
        {
            OnPointerExitDelegate -= listener;
            OnPointerExitDelegate += listener;
        }

        public void AddPointerMove(Action<PointerEventData> listener)
        {
            OnPointMoveDelegate -= listener;
            OnPointMoveDelegate += listener;
        }

        public void AddPointerUp(Action<PointerEventData> listener)
        {
            OnPointerUpDelegate -= listener;
            OnPointerUpDelegate += listener;
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            m_IsDrag = true;
            OnBeginDragDelegate?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            OnDragDelegate?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            m_IsDrag = false;
            OnEndDragDelegate?.Invoke(eventData);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            OnPointerClickDelegate?.Invoke(eventData);
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            OnPointerDownDelegate?.Invoke(eventData);
            m_IsPointerPressing = true;
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEnterDelegate?.Invoke(eventData);
            m_IsPointerHovering = true;
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            OnPointerExitDelegate?.Invoke(eventData);
            m_IsPointerHovering = false;
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            OnPointMoveDelegate?.Invoke(eventData);
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            OnPointerUpDelegate?.Invoke(eventData);
            m_IsPointerPressing = false;
        }

        public void RemoveBeginDrag(Action<PointerEventData> listener)
        {
            OnBeginDragDelegate -= listener;
        }

        public void RemoveClick(Action<PointerEventData> listener)
        {
            OnPointerClickDelegate -= listener;
        }

        public void RemoveDrag(Action<PointerEventData> listener)
        {
            OnDragDelegate -= listener;
        }

        public void RemoveEndDrag(Action<PointerEventData> listener)
        {
            OnEndDragDelegate -= listener;
        }

        public void RemovePointerDown(Action<PointerEventData> listener)
        {
            OnPointerDownDelegate -= listener;
        }

        public void RemovePointerEnter(Action<PointerEventData> listener)
        {
            OnPointerEnterDelegate -= listener;
        }

        public void RemovePointerExit(Action<PointerEventData> listener)
        {
            OnPointerExitDelegate -= listener;
        }

        public void RemovePointerMove(Action<PointerEventData> listener)
        {
            OnPointMoveDelegate -= listener;
        }

        public void RemovePointerUp(Action<PointerEventData> listener)
        {
            OnPointerUpDelegate -= listener;
        }

        public void SetBeginDrag(Action<PointerEventData> listener = null)
        {
            OnBeginDragDelegate = listener;
        }

        public void SetDrag(Action<PointerEventData> listener = null)
        {
            OnDragDelegate = listener;
        }

        public void SetEndDrag(Action<PointerEventData> listener = null)
        {
            OnEndDragDelegate = listener;
        }
    }
}
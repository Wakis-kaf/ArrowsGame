using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UScrollRectDragPass : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private IDragEventPass dragEventPass;

    private ScrollRect parentScrollRect;

    public ScrollRect ParentScrollRect
    {
        get
        {
            if (parentScrollRect == null)
            {
                parentScrollRect = GetComponentInParent<ScrollRect>();
            }
            return parentScrollRect;
        }
        set
        {
            parentScrollRect = value;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (parentScrollRect != null &&
            parentScrollRect.transform != transform
            && (dragEventPass != null && dragEventPass.enableDragEventPass)
            )
        {
            parentScrollRect.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parentScrollRect != null &&
          parentScrollRect.transform != transform
          && (dragEventPass != null && dragEventPass.enableDragEventPass)
          )
        {
            parentScrollRect.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (parentScrollRect != null &&
          parentScrollRect.transform != transform
          && (dragEventPass != null && dragEventPass.enableDragEventPass)
          )
        {
            parentScrollRect.OnEndDrag(eventData);
        }
    }

    private void Awake()
    {
        dragEventPass = gameObject.GetComponent<IDragEventPass>();
    }
}
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Runtime.UI.UIAnimae
{
    public class Tester : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public UIAnimatorCaller pressCaller;
        public UIAnimatorCaller upCaller;

        public void OnPointerDown(PointerEventData eventData)
        {
            pressCaller.Call();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            upCaller.Call();
        }
    }
}
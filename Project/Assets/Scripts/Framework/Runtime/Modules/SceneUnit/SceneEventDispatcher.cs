using Framework.Runtime.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Framework.Runtime.MSceneUnit
{
    public class SceneEventDispatcher : MonoBehaviour
    {
        private  Action m_OnMouseDown;
        public bool IgnoreUIEvent = true;
        private void OnMouseDown()
        {
            if (IgnoreUIEvent && IsPointerOverUIobject())
            {
                return; 
            }
         
            m_OnMouseDown?.Invoke();
        }
        public void SetMouseDown(Action mouseDownCb)
        {
            m_OnMouseDown = mouseDownCb;
        }
        public void AddMouseDown(Action mouseDownCb)
        {
            m_OnMouseDown -= mouseDownCb;
            m_OnMouseDown += mouseDownCb;

        }
        public void RemoveMouseDown(Action mouseDownCb)
        {
            m_OnMouseDown -= mouseDownCb;
        }
        //public static bool IsPointerOverUIobject()
        //{
        //    PointerEventData eventData = new PointerEventData(EventSystem.current);
        //    eventData.position = Input.mousePosition;
        //    List<RaycastResult> results = new List<RaycastResult>();
        //    EventSystem.current.RaycastAll(eventData, results);
        //    return results.Count > 0; 
        //}
        public static bool IsPointerOverUIobject()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            foreach (var result in results)
            {
                var graphic = result.gameObject.GetComponent<Graphic>();
                if (graphic != null && graphic.raycastTarget)
                {
                    var filter = result.gameObject.GetComponent<ICanvasRaycastFilter>();
                    if (filter != null)
                    {
                        if (filter.IsRaycastLocationValid(eventData.position, eventCamera: UIRootCamera.Camera))
                        {
                            return true; // 找到一个阻止射线穿透的UI对象
                        }
                    }
                    else
                    {
                        // 没有 ICanvasRaycastFilter 组件，graphic.raycastTarget 为 true 即可
                        return true;
                    }
                }
            }

            return false; // 没有找到阻止射线穿透的UI对象
        }

    }

}

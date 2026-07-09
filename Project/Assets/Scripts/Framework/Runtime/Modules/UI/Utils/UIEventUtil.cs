using System;
using UnityEngine;

namespace Framework.Runtime.UI
{
    public class UIEventUtil
    {
        public static EventReceiver GetEventReceiver(Transform transform)
        {
            return transform.gameObject.GetOrAddComponent<EventReceiver>();
        }

        public static USelect GetOrAddUSelect(Transform transform)
        {
            return transform.gameObject.GetOrAddComponent<USelect>();
        }

        public static USelect GetUSelect(Transform transform)
        {
            return transform.gameObject.GetComponent<USelect>();
        }

        public static void DeSelectGO()
        {
       
            UIUtil.SetSelectedGameObject(null);
        }

        public static USelect SetGOSelect(GameObject go,Action<bool> onSelectChangde = null)
        {
            var uSelect = UIEventUtil.GetOrAddUSelect(go.transform);
            uSelect.SetSelect(onSelectChangde);
            UIUtil.SetSelectedGameObject(go);
            return uSelect;
        }
        
    }
}
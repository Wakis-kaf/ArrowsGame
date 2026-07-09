using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Runtime.UI
{
    public class USelect : UIBehaviour, ISelectHandler, IDeselectHandler
    {
        public bool ignoreChildSelect = true;

        private Action<bool> _onSelect;
        protected override void OnDestroy()
        {
            base.OnDestroy();
            _onSelect = null;
        }
        /// <summary>
        /// 添加选中
        /// </summary>
        /// <param name="callback"></param>
        public void SetSelect(Action<bool> callback)
        {
            _onSelect = callback;
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            if (ignoreChildSelect)
            {
                PointerEventData pEData = eventData as PointerEventData;

                if (pEData != null && pEData.pointerCurrentRaycast.gameObject != null)
                {
                    Transform target = pEData.pointerCurrentRaycast.gameObject.transform;

                    if (target.IsChildOf(this.transform))
                    {
                        StartCoroutine(ReturnSelect());
                        return;
                    }
                }
            }

            if (_onSelect != null && GameApp.IsAppRunning())
            {
                _onSelect(false);
            }
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            if (_onSelect != null)
            {
                _onSelect(true);
            }
        }

        /// <summary>
        /// 取消选中
        /// </summary>
        public void RemoveSelect()
        {
            _onSelect = null;
        }

        private IEnumerator ReturnSelect()
        {
            yield return 0;
            UIUtil.SetUIEventCurSelect(gameObject);
            yield return 0;
        }
    }
}
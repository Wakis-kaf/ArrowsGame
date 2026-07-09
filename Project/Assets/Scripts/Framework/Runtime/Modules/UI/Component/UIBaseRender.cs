using Framework.Runtime.UI.UIAnimae;

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Runtime.UI
{
    public class UIBaseRender : USprite,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerMoveHandler,
        IPointerClickHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IDragEventPass
    {
        public UIAnimatorCaller clickAnimCaller = new UIAnimatorCaller() { targetSequence = "PointerClick" };
        public UIAnimatorCaller disSelectAnimCaller = new UIAnimatorCaller() { targetSequence = "DisSelect" };
        public UIAnimatorCaller downAnimCaller = new UIAnimatorCaller() { targetSequence = "PointerDown" };
        public UIAnimatorCaller enterAnimCaller = new UIAnimatorCaller() { targetSequence = "PointerEnter" };
        public UIAnimatorCaller exitAnimCaller = new UIAnimatorCaller() { targetSequence = "PointerExit" };
        public UIAnimatorCaller selectAnimCaller = new UIAnimatorCaller() { targetSequence = "Select" };
        public UIAnimatorCaller upAnimCaller = new UIAnimatorCaller() { targetSequence = "PointerUp" };
        [SerializeField]
        private bool m_IsSelect = false;
        private RectTransform m_RectTransform;
        private Action<bool> onSelect;

        private event Action<PointerEventData> OnPointerClickDelegate;

        private event Action<PointerEventData> OnPointerDownDelegate;

        private event Action<PointerEventData> OnPointerEnterDelegate;
        private event Action<PointerEventData> OnPointerMoveDelegate;

        private event Action<PointerEventData> OnPointerExitDelegate;

        private event Action<PointerEventData> OnPointerUpDelegate;
        public bool IsSelect { 
            get => m_IsSelect; 
            set{
                if (value) {
                    DoSelect();
                }else
                {
                    DoDeSelect();
                }
            } 
        }

        public RectTransform RectTransform
        {
            get
            {
                if (m_RectTransform == null)
                {
                    m_RectTransform = GetComponent<RectTransform>();
                }

                return m_RectTransform;
            }
        }

        public void AddClick(Action<PointerEventData> listener)
        {
            OnPointerClickDelegate -= listener;
            OnPointerClickDelegate += listener;
        }
        public void SetClick(Action<PointerEventData> listener)
        {
            OnPointerClickDelegate = listener;
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
        public void AddPointerMove(Action<PointerEventData> listener)
        {
            OnPointerMoveDelegate -= listener;
            OnPointerMoveDelegate += listener;
        }

        public void AddPointerExit(Action<PointerEventData> listener)
        {
            OnPointerExitDelegate += listener;
        }

        public void AddPointerUp(Action<PointerEventData> listener)
        {
            OnPointerUpDelegate -= listener;
            OnPointerUpDelegate += listener;
        }

        public void AddSelectChanged(Action<bool> listener)
        {
            listener?.Invoke(IsSelect);
            onSelect -= listener;
            onSelect += listener;
        }



       

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            OnPointerClickDelegate?.Invoke(eventData);
            clickAnimCaller?.Call();
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            OnPointerDownDelegate?.Invoke(eventData);
            downAnimCaller?.Call();
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEnterDelegate?.Invoke(eventData);
            enterAnimCaller?.Call();
        }
        public virtual void OnPointerMove(PointerEventData eventData)
        {
            OnPointerMoveDelegate?.Invoke(eventData);
        }
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            OnPointerExitDelegate?.Invoke(eventData);
            exitAnimCaller?.Call();
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            OnPointerUpDelegate?.Invoke(eventData);
            upAnimCaller?.Call();
        }
        public void PlayDeSelect()
        {
            disSelectAnimCaller?.Call();
        }

        public void PlaySelect()
        {
            selectAnimCaller?.Call();
        }

        public void RemoveClick(Action<PointerEventData> listener)
        {
            OnPointerClickDelegate -= listener;
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

        public void RemovePointerUp(Action<PointerEventData> listener)
        {
            OnPointerUpDelegate -= listener;
        }

        public void RemoveSelectChanged(Action<bool> listener)
        {
            onSelect -= listener;
        }
        public void DoDeSelect(bool force = false)
        {
            if (force)
            {
                m_IsSelect = false;
            }
            if (m_LastIsSelect != m_IsSelect && !m_IsSelect)
            {
                StopSelect();
                PlayDeSelect();
            }
            m_LastIsSelect = m_IsSelect;
        }
        public void DoSelect(bool force = false)
        {
            if (force)
            {
                m_IsSelect = true;
            }
            if (m_LastIsSelect != m_IsSelect && m_IsSelect)
            {
                StopDeSelect();
                PlaySelect();
            }
            m_LastIsSelect = m_IsSelect;
        }

        public void StopDeSelect()
        {
            disSelectAnimCaller?.Complete();
        }
        public void StopSelect()
        {
            selectAnimCaller.Complete();
        }
        protected override void Awake()
        {
            base.Awake();
            AddClick(OnClick);
        }
        private bool m_LastIsSelect;
        private void OnClick(PointerEventData data )
        {
            m_LastIsSelect = m_IsSelect;
            if (m_IsSelect)
            {
                m_IsSelect = false;
                onSelect?.Invoke(false);
            }
            else
            {
                m_IsSelect = true;
                onSelect?.Invoke(true);
            }

            if (m_LastIsSelect != m_IsSelect)
            {
                if (m_IsSelect)
                {
                    DoSelect();
                }
                else
                {
                    DoDeSelect();
                }
            }
            m_LastIsSelect = m_IsSelect;

        }

     
    }
}
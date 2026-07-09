using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework.Runtime.UI
{
    public class UProgressBar : Slider, IBeginDragHandler, IEndDragHandler
    {
        public RectTransform fillSyncTransform;
        public bool isFillModel = false;
        public bool isFillClampMin = true;
        private Action<float> m_OnBeginDraged;
        private Action<float> m_OnEndDraged;
        private Action<float> m_OnValueChanged;
        private Tweener m_ValueTweener;
        private bool m_IsDraging = false;
        public bool IsDraging => m_IsDraging;
        public void AddBeginDraged(Action<float> listener)
        {
            m_OnBeginDraged += listener;
        }

        public void AddEndDraged(Action<float> listener)
        {
            m_OnEndDraged += listener;
        }

        public void AddValueChanged(Action<float> listener, bool addTrigger = true)
        {
            m_OnValueChanged += listener;
            if (addTrigger)
                listener?.Invoke(value);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            m_IsDraging = true;
            m_OnBeginDraged?.Invoke(value);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            m_IsDraging = false;
            m_OnEndDraged?.Invoke(value);
        }

        public void RemoveBeginDraged(Action<float> listener)
        {
            m_OnBeginDraged -= listener;
        }

        public void RemoveEndDraged(Action<float> listener)
        {
            m_OnEndDraged -= listener;
        }

        public void RemoveValueChanged(Action<float> listener)
        {
            m_OnValueChanged -= listener;
        }

        public void SetValue(float value)
        {
            this.value = value;
        }

        public void SetValue(float value, float duration, Action cb = null)
        {
            m_ValueTweener?.Complete();
            m_ValueTweener = DOTween.To(ValueGetter, ValueSetter, value, duration).SetUpdate(true);
            if (cb != null)
            {
                m_ValueTweener.OnComplete(() => { cb?.Invoke(); });
            }

            m_ValueTweener.Play();
        }

        public void ValueSetter(float value)
        {
            SetValue(value);
        }

        protected override void Awake()
        {
            base.Awake();
            onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(float value)
        {
            m_OnValueChanged?.Invoke(value);

            if (isFillModel && fillSyncTransform != null)
            {
                float minX = 0;
                fillSyncTransform.anchorMin = Vector2.zero;
                fillSyncTransform.anchorMax = Vector2.up;
                fillSyncTransform.pivot = new Vector2(1, 0.5f);
                var pos = fillSyncTransform.anchoredPosition;
                if (isFillClampMin)
                {
                    minX = fillSyncTransform.rect.size.x;
                }
                pos.x = Mathf.Max(minX, fillRect.rect.size.x);
                fillSyncTransform.anchoredPosition = pos;
            }
        }

        private float ValueGetter()
        {
            return value;
        }
    }
}
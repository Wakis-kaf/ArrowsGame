using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System;

namespace Framework.Runtime.UI
{
    public class UIGroupTransition
    {
        private class AnimUnit
        {
            public RectTransform rect;
            public CanvasGroup canvasGroup;
            public Vector2 originalPos;
            public Vector2 dir;
            public float dist;
            public float dur;
            public float delay;
            public Ease ease;
        }

        private List<AnimUnit> m_AnimUnits = new List<AnimUnit>();

        public void AddUnit(RectTransform target, Vector2 direction, float distance, float duration, float delay = 0, Ease ease = Ease.OutQuad)
        {
            if (target == null) return;

            AnimUnit unit = new AnimUnit
            {
                rect = target,
                canvasGroup = target.GetComponent<CanvasGroup>(),
                originalPos = target.anchoredPosition,
                dir = direction.normalized,
                dist = distance,
                dur = duration,
                delay = delay,
                ease = ease
            };

            m_AnimUnits.Add(unit);
        }

        public void PlayAll(Action onComplete = null)
        {
            if (m_AnimUnits.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            float maxTotalDuration = 0;
            for (int i = 0; i < m_AnimUnits.Count; i++)
            {
                var unit = m_AnimUnits[i];
                if (unit.rect == null) continue;

                Vector2 startPos = unit.originalPos - (unit.dir * unit.dist);
                unit.rect.anchoredPosition = startPos;

                if (unit.canvasGroup != null)
                {
                    unit.canvasGroup.alpha = 0f;
                    unit.canvasGroup.DOKill();
                }

                unit.rect.DOKill();
                unit.rect.DOAnchorPos(unit.originalPos, unit.dur)
                    .SetDelay(unit.delay)
                    .SetEase(unit.ease)
                    .SetUpdate(true);

                if (unit.canvasGroup != null)
                {
                    unit.canvasGroup.DOFade(1f, unit.dur)
                        .SetDelay(unit.delay)
                        .SetUpdate(true);
                }

                maxTotalDuration = Mathf.Max(maxTotalDuration, unit.dur + unit.delay);
            }

            if (onComplete != null)
            {
                DOVirtual.DelayedCall(maxTotalDuration, () => onComplete.Invoke()).SetUpdate(true);
            }
        }

        public void PlayBackAll(Action onComplete = null)
        {
            if (m_AnimUnits.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            float maxTotalDuration = 0;
            for (int i = 0; i < m_AnimUnits.Count; i++)
            {
                var unit = m_AnimUnits[i];
                if (unit.rect == null) continue;

                Vector2 targetPos = unit.originalPos - (unit.dir * unit.dist);

                unit.rect.DOKill();
                unit.rect.DOAnchorPos(targetPos, unit.dur)
                    .SetDelay(unit.delay)
                    .SetEase(unit.ease)
                    .SetUpdate(true);

                if (unit.canvasGroup != null)
                {
                    unit.canvasGroup.DOKill();
                    unit.canvasGroup.DOFade(0f, unit.dur)
                        .SetDelay(unit.delay)
                        .SetUpdate(true);
                }

                maxTotalDuration = Mathf.Max(maxTotalDuration, unit.dur + unit.delay);
            }

            if (onComplete != null)
            {
                DOVirtual.DelayedCall(maxTotalDuration, () => onComplete.Invoke()).SetUpdate(true);
            }
        }

        public void ResetAll()
        {
            foreach (var unit in m_AnimUnits)
            {
                if (unit.rect == null) continue;

                unit.rect.DOKill();
                unit.rect.anchoredPosition = unit.originalPos;

                if (unit.canvasGroup != null)
                {
                    unit.canvasGroup.DOKill();
                    unit.canvasGroup.alpha = 1f;
                }
            }
        }

        public void Clear()
        {
            m_AnimUnits.Clear();
        }
    }
}
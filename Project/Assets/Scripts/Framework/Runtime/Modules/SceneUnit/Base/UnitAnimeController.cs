using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.MSceneUnit
{
    public class UnitAnimeController : SceneUnitComponent
    {
        private Animator m_Animator;

        private Dictionary<string, Tweener> m_FloatTweenerMap = new Dictionary<string, Tweener>();

        public Animator Animator
        {
            get
            {
                if (m_Animator == null)
                {
                    m_Animator = OwnSceneUnit.RootTransform.GetComponentInChildren<Animator>();
                }

                return m_Animator;
            }
        }

        public void SetBool(string name, bool value)
        {
            Animator.SetBool(name, value);
        }

        public void SetFloat(string name, float value)
        {
            if (m_FloatTweenerMap.TryGetValue(name, out var tweener))
            {
                tweener.ChangeEndValue(value, 0).Pause();
            }

            Animator.SetFloat(name, value);
        }

        public void SetFloatSmooth(string name, float value, float duration = 0.1f, Ease ease = Ease.Linear)
        {
            Tweener tweener = null;
            if (!m_FloatTweenerMap.TryGetValue(name, out tweener))
            {
                tweener = DOTween
                    .To(() => { return Animator.GetFloat(name); }, (curV) => { Animator.SetFloat(name, curV); }, value,
                        duration).SetEase(ease).SetAutoKill(false).Play();
                m_FloatTweenerMap.Add(name, tweener);
            }

            tweener.ChangeValues(Animator.GetFloat(name), value, duration).Play();
        }

        public void SetInt(string name, int value)
        {
            Animator.SetInteger(name, value);
        }

        public void SetTrigger(string name)
        {
            Animator.SetTrigger(name);
        }
    }
}
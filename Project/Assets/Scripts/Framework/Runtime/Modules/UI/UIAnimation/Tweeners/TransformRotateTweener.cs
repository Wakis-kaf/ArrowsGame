using DG.Tweening;
using Sirenix.OdinInspector;

using System;
using UnityEngine;

namespace Framework.Runtime.UI.UIAnimae.Tweeners
{
    [Serializable]
    public class TransformRotateTweener : UITweener
    {
        public CoordinateType coordinateType = CoordinateType.Local;

        [ShowIf("@this.customEase == true")]
        public AnimationCurve customCurve;

        public bool customEase = false;
        public float duration = 0.1f;

        [ShowIf("@this.customEase == false")]
        public Ease easeType = Ease.Linear;

        public Vector3 rotateTo = Vector3.zero;
        public Transform targetTransform;
        private Tweener m_RotTweener;
        private Vector3 m_StartAnchorPos = Vector3.zero;
        private Vector3 m_StartLocalPos = Vector3.zero;
        private Vector3 m_StartPos = Vector3.zero;

        public override bool IsComplete()
        {
            return m_RotTweener.IsComplete();
        }

        public override bool IsEnableAndActive(UITweenContext context = null)
        {
            return base.IsEnableAndActive(context) && (targetTransform != null);
        }

        protected override bool CanAutoComplete()
        {
            return false;
        }

        protected override void OnInit(UITweenContext context = null)
        {
            m_StartPos = targetTransform.position;
            m_StartLocalPos = targetTransform.localPosition;
            m_RotTweener = DOTween.To(RotGetter, RotSetter, rotateTo, duration);
            if (customEase)
            {
                m_RotTweener.SetEase(customCurve);
            }
            else
            {
                m_RotTweener.SetEase(easeType);
            }
            m_RotTweener?.Pause().SetAutoKill(false).OnComplete(CallComplete).SetUpdate(true);
        }

        protected override void OnPause(UITweenContext context = null)
        {
            m_RotTweener?.Pause();
        }

        protected override void OnPlay(UITweenContext context = null)
        {
            m_RotTweener?.Play();
        }

        protected override void OnPlayBackwards(UITweenContext context = null)
        {
            m_RotTweener?.PlayBackwards();
        }

        protected override void OnPlayForward(UITweenContext context = null)
        {
            m_RotTweener?.PlayForward();
        }
        protected override void OnComplete(UITweenContext context = null)
        {
            m_RotTweener?.Complete();
        }
        protected override void OnRewind(UITweenContext context = null)
        {
            m_RotTweener?.Rewind();
            switch (coordinateType)
            {
                case CoordinateType.World:
                    targetTransform.eulerAngles = m_StartPos;
                    break;

                case CoordinateType.Local:
                    targetTransform.localEulerAngles = m_StartLocalPos;
                    break;
            }
        }
        protected override void OnRestart(UITweenContext context = null)
        {
            m_RotTweener?.Restart();
        }

        protected override void OnStop(UITweenContext context = null)
        {
            //m_RotTweener.Complete();
            //switch (coordinateType)
            //{
            //    case CoordinateType.World:
            //        targetTransform.eulerAngles = m_StartPos;
            //        break;

            //    case CoordinateType.Local:
            //        targetTransform.localEulerAngles = m_StartLocalPos;
            //        break;
            //}
        }

        private Vector3 RotGetter()
        {
            switch (coordinateType)
            {
                case CoordinateType.World:
                    return targetTransform.eulerAngles;

                case CoordinateType.Local:
                    return targetTransform.localEulerAngles;

                default:
                    return Vector3.zero;
            }
        }

        private void RotSetter(Vector3 angles)
        {
            switch (coordinateType)
            {
                case CoordinateType.World:
                    targetTransform.eulerAngles = angles;
                    break;

                case CoordinateType.Local:
                    targetTransform.localEulerAngles = angles;
                    break;

                default:
                    return;
            }
        }
    }
}
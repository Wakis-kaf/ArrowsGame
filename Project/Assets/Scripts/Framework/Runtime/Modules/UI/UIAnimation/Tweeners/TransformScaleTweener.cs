using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Framework.Runtime.UI.UIAnimae.Tweeners
{
    [Serializable]
    public class TransformScaleTweener : UITweener
    {
        public CoordinateType coordinateType = CoordinateType.Local;

        [ShowIf("@this.customEase == true")]
        public AnimationCurve customCurve;

        public bool customEase = false;
        public float duration = 0.1f;

        [ShowIf("@this.customEase == false")]
        public Ease easeType = Ease.Linear;
        public bool useInitScaleFrom = true;
        [ShowIf("@this.useInitScaleFrom == false")]
        public Vector3 scaleFrom = Vector3.one;
        public Vector3 scaleTo = Vector3.one;
        public Transform targetTransform;
        private Tweener m_ScaleTweener;
        private Vector3 m_StartLocalScale = Vector3.zero;

        public override bool IsComplete()
        {
            return m_ScaleTweener.IsComplete();
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
            m_StartLocalScale = useInitScaleFrom?  targetTransform.localScale: scaleFrom;
            m_ScaleTweener = DOTween.To(ScaleGetter, ScaleSetter, scaleTo, duration);
            if (customEase)
            {
                m_ScaleTweener.SetEase(customCurve);
            }
            else
            {
                m_ScaleTweener.SetEase(easeType);
            }
            m_ScaleTweener?.Pause().SetAutoKill(false).OnComplete(CallComplete).SetUpdate(true);
        }

        protected override void OnPause(UITweenContext context = null)
        {
            m_ScaleTweener?.Pause();
        }

        protected override void OnPlay(UITweenContext context = null)
        {
            //targetTransform.localScale = m_StartLocalScale;
            ResetToStartScale();
            m_ScaleTweener?.Play();
        }

        protected override void OnPlayBackwards(UITweenContext context = null)
        {
            m_ScaleTweener?.PlayBackwards();
        }

        protected override void OnPlayForward(UITweenContext context = null)
        {
            //targetTransform.localScale = m_StartLocalScale;
            m_ScaleTweener?.PlayForward();
        }


        protected override void OnRestart(UITweenContext context = null)
        {
            //targetTransform.localScale = m_StartLocalScale;
            ResetToStartScale();
            m_ScaleTweener?.Restart();
        }
        protected override void OnComplete(UITweenContext context = null)
        {
            m_ScaleTweener.Complete();
        }
        protected override void OnRewind(UITweenContext context = null)
        {
            m_ScaleTweener?.Rewind();
            ResetToStartScale();
            //switch (coordinateType)
            //{
            //    case CoordinateType.World:
            //        //targetTransform.lossyScale = m_StartScale;
            //        break;

            //    case CoordinateType.Local:
            //        targetTransform.localScale = m_StartLocalScale;
            //        break;
            //}
        }
        protected override void OnStop(UITweenContext context = null)
        {
            //m_ScaleTweener.Complete();
            //switch (coordinateType)
            //{
            //    case CoordinateType.World:
            //        //targetTransform.lossyScale = m_StartScale;
            //        break;

            //    case CoordinateType.Local:
            //        targetTransform.localScale = m_StartLocalScale;
            //        break;
            //}
        }
        private void ResetToStartScale()
        {
            switch (coordinateType)
            {
                case CoordinateType.World:
                    // 注意：不能直接设置 lossyScale，需要通过父级调整
                    //SetWorldScale(targetTransform, m_StartWorldScale);
                    break;

                case CoordinateType.Local:
                    targetTransform.localScale = m_StartLocalScale;
                    break;
            }
        }
        private Vector3 ScaleGetter()
        {
            switch (coordinateType)
            {
                case CoordinateType.World:
                    return targetTransform.lossyScale;

                case CoordinateType.Local:
                    return targetTransform.localScale;

                default:
                    return Vector3.one;
            }
        }

        private void ScaleSetter(Vector3 scale)
        {
            switch (coordinateType)
            {
                case CoordinateType.World:
                    break;

                case CoordinateType.Local:
                    targetTransform.localScale = scale;
                    break;

                default:
                    return;
            }
        }
    }
}
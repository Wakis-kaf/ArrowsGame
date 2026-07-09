using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Framework.Runtime.UI.UIAnimae.Tweeners
{
    public enum CoordinateType
    {
        World,
        Local,
        Anchor
    }

    [Serializable]
    public class TransformPositionTweener : UITweener
    {
        public CoordinateType coordinateType = CoordinateType.Local;

        [ShowIf("@this.customEase == true")]
        public AnimationCurve customCurve;

        public bool customEase = false;
        public float duration = 0.1f;

        [ShowIf("@this.customEase == false")]
        public Ease easeType = Ease.Linear;

        public bool isRectTransform = false;

        [Tooltip("目标位置（绝对坐标）")]
        public Vector3 posTo = Vector3.zero;

        [ShowIf("@this.isRectTransform == true ")]
        public RectTransform targetRectTransform;

        [ShowIf("@this.isRectTransform  == false")]
        public Transform targetTransform;

        // 新增：解决与手动设置anchorPosition冲突的选项
        [Tooltip("启用此选项可以避免与手动设置anchorPosition的冲突")]
        public bool useProgressBasedAnimation = true;

        // 新增：超出目标位置时的行为控制
        [Tooltip("超出目标位置时的行为")]
        public OvershootBehavior overshootBehavior = OvershootBehavior.CompleteImmediately;

        private Tweener m_PosTweener;
        private Vector3 m_StartAnchorPos = Vector3.zero;
        private Vector3 m_StartLocalPos = Vector3.zero;
        private Vector3 m_StartPos = Vector3.zero;

        // 进度动画相关变量
        private Vector3 m_StartPosition;
        private Vector3 m_TargetPosition;
        private Vector3 m_LastExternalPosition;
        private float m_AnimationProgress;

        // 新增：移动方向检测
        private Vector3 m_MoveDirection;
        private bool m_HasCompletedDueToOvershoot = false;

        public enum OvershootBehavior
        {
            None,                   // 不处理超出，继续动画
            CompleteImmediately,     // 立即完成动画并触发回调
            StopImmediately          // 立即停止但不触发回调
        }

        public override bool IsComplete()
        {
            return m_PosTweener == null || m_PosTweener.IsComplete() || m_HasCompletedDueToOvershoot;
        }

        public override bool IsEnableAndActive(UITweenContext context = null)
        {
            return base.IsEnableAndActive(context) && (isRectTransform && targetRectTransform != null) ||
                   (!isRectTransform && targetTransform != null);
        }

        protected override bool CanAutoComplete()
        {
            return false;
        }

        protected override void OnInit(UITweenContext context = null)
        {
            if (isRectTransform)
            {
                targetTransform = targetRectTransform;
            }

            // 记录初始位置（用于回滚）
            m_StartPos = targetTransform.position;
            m_StartLocalPos = targetTransform.localPosition;
            if (isRectTransform)
                m_StartAnchorPos = targetRectTransform.anchoredPosition3D;

            if (useProgressBasedAnimation)
            {
                InitializeProgressBasedAnimation();
            }
            else
            {
                InitializeStandardAnimation();
            }
        }

        private void InitializeProgressBasedAnimation()
        {
            // 记录动画开始位置和目标位置
            m_StartPosition = GetCurrentPosition();
            m_TargetPosition = posTo;
            m_LastExternalPosition = m_StartPosition;
            m_AnimationProgress = 0f;
            m_HasCompletedDueToOvershoot = false;

            // 计算移动方向
            m_MoveDirection = (m_TargetPosition - m_StartPosition).normalized;

            // 检查是否一开始就已经超过目标位置
            if (overshootBehavior != OvershootBehavior.None && HasReachedOrPassedTarget(GetCurrentPosition()))
            {
                HandleImmediateCompletion();
                return;
            }

            // 使用基于进度的动画，从0到1
            m_PosTweener = DOTween.To(
                () => m_AnimationProgress,
                progress => {
                    if (m_HasCompletedDueToOvershoot) return;

                    m_AnimationProgress = progress;
                    UpdatePositionBasedOnProgress();
                },
                1f,
                duration
            ).SetUpdate(true);

            SetupTweenerCommonSettings();
        }

        private void InitializeStandardAnimation()
        {
            m_PosTweener = DOTween.To(PotGetter, PotSetter, posTo, duration);
            SetupTweenerCommonSettings();
        }

        private void SetupTweenerCommonSettings()
        {
            if (customEase)
            {
                m_PosTweener.SetEase(customCurve);
            }
            else
            {
                m_PosTweener.SetEase(easeType);
            }

            m_PosTweener?.Pause().SetAutoKill(false).OnComplete(CallComplete).SetUpdate(true);
        }

        private void UpdatePositionBasedOnProgress()
        {
            if (m_HasCompletedDueToOvershoot) return;

            // 检测外部位置变化
            Vector3 currentExternalPos = GetCurrentPosition();
            if (Vector3.Distance(currentExternalPos, m_LastExternalPosition) > 0.01f)
            {
                // 外部位置发生变化，重新计算起始位置，保持目标位置不变
                Vector3 positionDelta = currentExternalPos - m_LastExternalPosition;
                m_StartPosition += positionDelta;
                m_LastExternalPosition = currentExternalPos;

                // 重新计算移动方向
                m_MoveDirection = (m_TargetPosition - m_StartPosition).normalized;

                //Debug.Log($"External position changed. Adjusted start to: {m_StartPosition}");
            }

            // 检查是否超过目标位置（在设置位置之前检查）
            if (overshootBehavior != OvershootBehavior.None && HasReachedOrPassedTarget(GetCurrentPosition()))
            {
                HandleImmediateCompletion();
                return;
            }

            // 使用插值计算当前位置：从起始位置插值到目标位置
            Vector3 targetPosition = Vector3.Lerp(m_StartPosition, m_TargetPosition, m_AnimationProgress);

            // 设置位置
            SetPosition(targetPosition);
            m_LastExternalPosition = GetCurrentPosition();

        }

        /// <summary>
        /// 检查是否已经到达或超过目标位置
        /// </summary>
        private bool HasReachedOrPassedTarget(Vector3 currentPosition)
        {
            // 计算从起点到当前位置和到目标位置的向量
            Vector3 toCurrent = currentPosition - m_StartPosition;
            Vector3 toTarget = m_TargetPosition - m_StartPosition;

            // 如果当前位置在移动方向上的投影长度 >= 目标位置，说明已经到达或超过
            float currentProjection = Vector3.Dot(toCurrent, m_MoveDirection);
            float targetProjection = Vector3.Dot(toTarget, m_MoveDirection);

            bool hasReachedOrPassed = currentProjection >= targetProjection;

            if (hasReachedOrPassed)
            {
                //Debug.Log($"Target reached or passed! Current projection: {currentProjection}, Target projection: {targetProjection}");
            }

            return hasReachedOrPassed;
        }

        /// <summary>
        /// 处理立即完成逻辑
        /// </summary>
        private void HandleImmediateCompletion()
        {
            if (m_HasCompletedDueToOvershoot) return;

            m_HasCompletedDueToOvershoot = true;
            //Debug.Log($"Immediate completion due to overshoot. Current: {GetCurrentPosition()}, Target: {m_TargetPosition}");

            // 停止动画
            m_PosTweener?.Kill();

            if (overshootBehavior == OvershootBehavior.CompleteImmediately)
            {
                // 触发完成回调
                CallComplete();
            }
            else if (overshootBehavior == OvershootBehavior.StopImmediately)
            {
                // 只是停止，不触发回调
                // 保持当前位置
            }
        }

        protected override void OnPause(UITweenContext context = null)
        {
            if (m_HasCompletedDueToOvershoot) return;
            m_PosTweener?.Pause();
        }

        protected override void OnPlay(UITweenContext context = null)
        {
            if (m_HasCompletedDueToOvershoot)
            {
                //Debug.Log("Cannot play - already completed due to overshoot");
                return;
            }

            //Debug.Log("Playing position animation");
            m_PosTweener?.Play();
        }

        protected override void OnPlayBackwards(UITweenContext context = null)
        {
            if (m_HasCompletedDueToOvershoot) return;

            if (useProgressBasedAnimation)
            {
                // 对于进度动画，反向播放需要重新初始化
                m_PosTweener?.Kill();
                InitializeProgressBasedAnimation();
            }
            m_PosTweener?.PlayBackwards();
        }

        protected override void OnPlayForward(UITweenContext context = null)
        {
            if (m_HasCompletedDueToOvershoot) return;
            m_PosTweener?.PlayForward();
        }

        protected override void OnRestart(UITweenContext context = null)
        {
            if (useProgressBasedAnimation)
            {
                // 重新初始化进度动画
                m_PosTweener?.Kill();
                InitializeProgressBasedAnimation();
            }
            m_PosTweener?.Restart();
        }

        protected override void OnStop(UITweenContext context = null)
        {
            m_PosTweener?.Kill();
            m_HasCompletedDueToOvershoot = false;

            if (useProgressBasedAnimation)
            {
                // 设置到最终位置
                SetPosition(m_TargetPosition);
            }
            else
            {
                // 原有的回滚逻辑
                switch (coordinateType)
                {
                    case CoordinateType.World:
                        targetTransform.position = m_StartPos;
                        break;

                    case CoordinateType.Local:
                        targetTransform.localPosition = m_StartLocalPos;
                        break;

                    case CoordinateType.Anchor:
                        if (isRectTransform)
                        {
                            targetRectTransform.anchoredPosition = m_StartAnchorPos;
                        }
                        break;
                }
            }
        }

        protected override void CallComplete()
        {
            // 确保位置准确
            if (useProgressBasedAnimation && !m_HasCompletedDueToOvershoot)
            {
                SetPosition(m_TargetPosition);
            }
            base.CallComplete();
        }

        private Vector3 PotGetter()
        {
            switch (coordinateType)
            {
                case CoordinateType.World:
                    return targetTransform.position;

                case CoordinateType.Local:
                    return targetTransform.localPosition;

                case CoordinateType.Anchor:
                    return isRectTransform ? targetRectTransform.anchoredPosition : targetTransform.localPosition;

                default:
                    return Vector3.zero;
            }
        }

        private void PotSetter(Vector3 position)
        {
            SetPosition(position);
        }

        private void SetPosition(Vector3 position)
        {
            switch (coordinateType)
            {
                case CoordinateType.World:
                    targetTransform.position = position;
                    break;

                case CoordinateType.Local:
                    targetTransform.localPosition = position;
                    break;

                case CoordinateType.Anchor:
                    if (isRectTransform)
                    {
                        targetRectTransform.anchoredPosition = position;
                    }
                    else
                    {
                        targetTransform.localPosition = position;
                    }
                    break;

                default:
                    return;
            }
        }

        private Vector3 GetCurrentPosition()
        {
            switch (coordinateType)
            {
                case CoordinateType.World:
                    return targetTransform.position;
                case CoordinateType.Local:
                    return targetTransform.localPosition;
                case CoordinateType.Anchor:
                    return isRectTransform ? targetRectTransform.anchoredPosition3D : targetTransform.localPosition;
                default:
                    return Vector3.zero;
            }
        }

    }
}
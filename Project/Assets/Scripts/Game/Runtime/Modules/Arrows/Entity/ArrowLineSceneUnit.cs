using System;
using System.Collections.Generic;
using DG.Tweening;
using Framework.Runtime.MSceneUnit;
using UnityEngine;

namespace Game.Modules.GModuleArrows
{
    public class ArrowLineSceneUnit : SceneUnit
    {
        private UnityEngine.LineRenderer lineRenderArrow => EntityPrefabBinder?.GetObj<UnityEngine.LineRenderer>("lineRenderArrow");
        private UnityEngine.LineRenderer lineRenderLine => EntityPrefabBinder?.GetObj<UnityEngine.LineRenderer>("lineRenderLine");

        public float CornerRadius = 0.25f; //数值越小，转弯越急越紧凑，拐角就会越接近硬直角；数值越大，转弯越缓。如果觉得太滑，可以将其调小（例如从 0.45 降到 0.2 或 0.25）。
        public int CornerSubdivisions = 4; //数值越小，折线感越明显（比如设为 3 或 4 会带有一点硬朗的折角感）；数值越大，弧线越圆润。

        public LevelArrowNode ArrowNode { get; private set; }
        public const float ArrowLength = 0.7f;
        public const float ArrowAngle = 30f;
        public const float PointRadius = 0.1f;
        public const float ArrowOffsetDistance = -0.1f;
        public enum ArrowFadeType
        {
            FromHead,
            FromTail,
            BodySync
        }
        public override void OnUnitAwake()
        {
            base.OnUnitAwake();
        }

        protected override void OnSceneUnitGUI(object data)
        {
            base.OnSceneUnitGUI(data);
            SetupMainLineRenderer();
            SetupArrowRenderer();
        }
        private Tween fadeTween;
        private Gradient originalLineGradient;
        private Gradient originalArrowGradient;
        public void PlayFade(ArrowFadeType arrowFadeType, Color fadeColor, float fadeInTime, float fadeOutTime, float fadeInOverPauseTime = 0)
        {
            if (lineRenderLine == null || lineRenderArrow == null) return;

            fadeTween?.Kill();

            if (originalLineGradient == null)
            {
                originalLineGradient = new Gradient();
                originalLineGradient.SetKeys(lineRenderLine.colorGradient.colorKeys, lineRenderLine.colorGradient.alphaKeys);
            }
            if (originalArrowGradient == null)
            {
                originalArrowGradient = new Gradient();
                originalArrowGradient.SetKeys(lineRenderArrow.colorGradient.colorKeys, lineRenderArrow.colorGradient.alphaKeys);
            }

            Color origLineColor = originalLineGradient.colorKeys.Length > 0 ? originalLineGradient.colorKeys[0].color : Color.white;
            Color origArrowColor = originalArrowGradient.colorKeys.Length > 0 ? originalArrowGradient.colorKeys[0].color : Color.white;

            Sequence fadeSequence = DOTween.Sequence();

            fadeSequence.Append(DOTween.To(() => 0f, x =>
            {
                ApplyDynamicFade(arrowFadeType, origLineColor, origArrowColor, fadeColor, x);
            }, 1f, fadeInTime).SetEase(Ease.Linear));

            if (fadeOutTime > 0f)
            {
                if (fadeInOverPauseTime > 0f)
                {
                    fadeSequence.AppendInterval(fadeInOverPauseTime);
                }

                ArrowFadeType reverseType = arrowFadeType;
                if (arrowFadeType == ArrowFadeType.FromHead) reverseType = ArrowFadeType.FromTail;
                else if (arrowFadeType == ArrowFadeType.FromTail) reverseType = ArrowFadeType.FromHead;

                fadeSequence.Append(DOTween.To(() => 0f, x =>
                {
                    ApplyDynamicFade(reverseType, fadeColor, fadeColor, origLineColor, x);
                }, 1f, fadeOutTime).SetEase(Ease.Linear));
            }

            fadeTween = fadeSequence;
        }

        private void ApplyDynamicFade(ArrowFadeType type, Color startLineColor, Color startArrowColor, Color targetColor, float totalProgress)
        {
            float bodyLen = 0f;
            if (lineRenderLine.positionCount > 1)
            {
                for (int i = 0; i < lineRenderLine.positionCount - 1; i++)
                {
                    bodyLen += Vector3.Distance(lineRenderLine.GetPosition(i), lineRenderLine.GetPosition(i + 1));
                }
            }
            if (bodyLen <= 0f) bodyLen = 1.5f;

            float totalLen = bodyLen + ArrowLength;
            float arrowWeight = ArrowLength / totalLen;
            float lineWeight = bodyLen / totalLen;

            float arrowProgress = 0f;
            float lineProgress = 0f;

            if (type == ArrowFadeType.BodySync)
            {
                arrowProgress = totalProgress;
                lineProgress = totalProgress;
            }
            else if (type == ArrowFadeType.FromHead)
            {
                if (totalProgress <= arrowWeight)
                {
                    arrowProgress = totalProgress / arrowWeight;
                    lineProgress = 0f;
                }
                else
                {
                    arrowProgress = 1f;
                    lineProgress = (totalProgress - arrowWeight) / lineWeight;
                }
            }
            else if (type == ArrowFadeType.FromTail)
            {
                if (totalProgress <= lineWeight)
                {
                    lineProgress = totalProgress / lineWeight;
                    arrowProgress = 0f;
                }
                else
                {
                    lineProgress = 1f;
                    arrowProgress = (totalProgress - lineWeight) / arrowWeight;
                }
            }

            lineRenderLine.colorGradient = CreateGradient(type, startLineColor, targetColor, lineProgress);
            lineRenderArrow.colorGradient = CreateGradient(type, startArrowColor, targetColor, arrowProgress);
        }

        private Gradient CreateGradient(ArrowFadeType type, Color sourceColor, Color targetColor, float progress)
        {
            Gradient gradient = new Gradient();

            if (type == ArrowFadeType.BodySync)
            {
                Color currentColor = Color.Lerp(sourceColor, targetColor, progress);
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(currentColor, 0.0f), new GradientColorKey(currentColor, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(currentColor.a, 0.0f), new GradientAlphaKey(currentColor.a, 1.0f) }
                );
                return gradient;
            }

            float splitPoint = (type == ArrowFadeType.FromTail) ? progress : (1f - progress);

            List<GradientColorKey> colorKeys = new List<GradientColorKey>();
            List<GradientAlphaKey> alphaKeys = new List<GradientAlphaKey>();

            Color leftColor = (type == ArrowFadeType.FromTail) ? targetColor : sourceColor;
            Color rightColor = (type == ArrowFadeType.FromTail) ? sourceColor : targetColor;

            if (splitPoint <= 0.001f)
            {
                colorKeys.Add(new GradientColorKey(rightColor, 0f));
                colorKeys.Add(new GradientColorKey(rightColor, 1f));
                alphaKeys.Add(new GradientAlphaKey(rightColor.a, 0f));
                alphaKeys.Add(new GradientAlphaKey(rightColor.a, 1f));
            }
            else if (splitPoint >= 0.999f)
            {
                colorKeys.Add(new GradientColorKey(leftColor, 0f));
                colorKeys.Add(new GradientColorKey(leftColor, 1f));
                alphaKeys.Add(new GradientAlphaKey(leftColor.a, 0f));
                alphaKeys.Add(new GradientAlphaKey(leftColor.a, 1f));
            }
            else
            {
                colorKeys.Add(new GradientColorKey(leftColor, 0f));
                colorKeys.Add(new GradientColorKey(leftColor, splitPoint));
                colorKeys.Add(new GradientColorKey(rightColor, splitPoint + 0.001f));
                colorKeys.Add(new GradientColorKey(rightColor, 1f));

                alphaKeys.Add(new GradientAlphaKey(leftColor.a, 0f));
                alphaKeys.Add(new GradientAlphaKey(leftColor.a, splitPoint));
                alphaKeys.Add(new GradientAlphaKey(rightColor.a, splitPoint + 0.001f));
                alphaKeys.Add(new GradientAlphaKey(rightColor.a, 1f));
            }

            gradient.SetKeys(colorKeys.ToArray(), alphaKeys.ToArray());
            return gradient;
        }
        private void StopFade()
        {
            fadeTween?.Kill();
            lineRenderLine.positionCount = 0;
            lineRenderArrow.enabled = false;

            if (originalLineGradient != null) lineRenderLine.colorGradient = originalLineGradient;
            if (originalArrowGradient != null) lineRenderArrow.colorGradient = originalArrowGradient;
        }
        private void SetupMainLineRenderer()
        {
            lineRenderLine.positionCount = 0;
            lineRenderLine.startWidth = 0.3f;
            lineRenderLine.endWidth = 0.3f;
            lineRenderLine.numCornerVertices = 0;
            lineRenderLine.numCapVertices = 4;
            lineRenderLine.useWorldSpace = true;
            lineRenderLine.hideFlags = HideFlags.DontSaveInEditor;
        }

        private void SetupArrowRenderer()
        {
            lineRenderArrow.useWorldSpace = true;
            lineRenderArrow.hideFlags = HideFlags.DontSaveInEditor;
            lineRenderArrow.numCornerVertices = 0;
            lineRenderArrow.numCapVertices = 0;
            lineRenderArrow.enabled = false;

            float baseWidth = 2f * ArrowLength * Mathf.Tan(30f * Mathf.Deg2Rad);

            AnimationCurve widthCurve = new AnimationCurve();
            int segmentCount = 5;
            for (int i = 0; i < segmentCount; i++)
            {
                float t = (float)i / (segmentCount - 1);
                float currentWidth = baseWidth * (1f - t);
                widthCurve.AddKey(t, currentWidth);
            }
            lineRenderArrow.widthCurve = widthCurve;

            lineRenderArrow.startWidth = baseWidth;
            lineRenderArrow.endWidth = 0f;
        }

        private List<Vector3> GenerateSmoothPath(List<Vector3> sharpPath)
        {
            if (sharpPath.Count < 3) return new List<Vector3>(sharpPath);

            List<Vector3> smoothPath = new List<Vector3>();
            smoothPath.Add(sharpPath[0]);

            for (int i = 1; i < sharpPath.Count - 1; i++)
            {
                Vector3 p0 = sharpPath[i - 1];
                Vector3 p1 = sharpPath[i];
                Vector3 p2 = sharpPath[i + 1];

                Vector3 d1 = (p0 - p1).normalized;
                Vector3 d2 = (p2 - p1).normalized;

                float l1 = Vector3.Distance(p0, p1);
                float l2 = Vector3.Distance(p1, p2);

                float maxR = Mathf.Min(l1, l2) * 0.45f;
                float r = Mathf.Min(CornerRadius, maxR);

                if (r > 0.001f && Vector3.Angle(d1, d2) < 175f)
                {
                    Vector3 pStart = p1 + d1 * r;
                    Vector3 pEnd = p1 + d2 * r;

                    if (Vector3.Distance(smoothPath[smoothPath.Count - 1], pStart) > 0.001f)
                    {
                        smoothPath.Add(pStart);
                    }

                    for (int j = 1; j < CornerSubdivisions; j++)
                    {
                        float t = (float)j / CornerSubdivisions;
                        Vector3 curvePoint = (1f - t) * (1f - t) * pStart + 2f * (1f - t) * t * p1 + t * t * pEnd;
                        smoothPath.Add(curvePoint);
                    }

                    smoothPath.Add(pEnd);
                }
                else
                {
                    if (Vector3.Distance(smoothPath[smoothPath.Count - 1], p1) > 0.001f)
                    {
                        smoothPath.Add(p1);
                    }
                }
            }

            if (Vector3.Distance(smoothPath[smoothPath.Count - 1], sharpPath[sharpPath.Count - 1]) > 0.001f)
            {
                smoothPath.Add(sharpPath[sharpPath.Count - 1]);
            }

            return smoothPath;
        }

        private void ExtractFramePoints(List<Vector3> path, List<float> segLengths, float tailDist, float headDist, List<Vector3> output)
        {
            output.Clear();
            Vector3 tailPos = GetPointAtDistance(path, segLengths, tailDist);
            Vector3 headPos = GetPointAtDistance(path, segLengths, headDist);
            output.Add(tailPos);

            float accumulated = 0f;
            for (int i = 0; i < path.Count; i++)
            {
                if (accumulated > tailDist && accumulated < headDist)
                {
                    Vector3 nodePos = new Vector3(path[i].x, path[i].y, 0);
                    if (Vector3.Distance(nodePos, tailPos) > 0.01f && Vector3.Distance(nodePos, headPos) > 0.01f)
                    {
                        output.Add(nodePos);
                    }
                }
                if (i < segLengths.Count) accumulated += segLengths[i];
            }

            if (Vector3.Distance(output[output.Count - 1], headPos) > 0.01f)
            {
                output.Add(headPos);
            }
        }

        public void SetArrowData(LevelArrowNode arrowNode)
        {
            ArrowNode = arrowNode;
            if (arrowNode == null || arrowNode.PathPoints == null || arrowNode.PathPoints.Count < 2)
            {
                lineRenderLine.positionCount = 0;
                lineRenderArrow.enabled = false;
                return;
            }

            List<Vector3> originalPath = arrowNode.PathPoints;
            Vector3 firstPoint = originalPath[0];
            Vector3 secondPoint = originalPath[1];
            Vector3 startDir = (firstPoint - secondPoint).normalized;
            Vector3 extendedStart = firstPoint + startDir * PointRadius;

            List<Vector3> extendedPath = new List<Vector3>(originalPath);
            extendedPath[0] = extendedStart;

            List<Vector3> smoothedPath = GenerateSmoothPath(extendedPath);

            float bodyLength = 0f;
            List<float> segmentLengths = new List<float>();
            for (int i = 0; i < smoothedPath.Count - 1; i++)
            {
                float dist = Vector3.Distance(smoothedPath[i], smoothedPath[i + 1]);
                segmentLengths.Add(dist);
                bodyLength += dist;
            }

            List<Vector3> currentFramePoints = new List<Vector3>();
            ExtractFramePoints(smoothedPath, segmentLengths, 0f, bodyLength, currentFramePoints);

            lineRenderLine.positionCount = currentFramePoints.Count;
            for (int i = 0; i < currentFramePoints.Count; i++)
            {
                lineRenderLine.SetPosition(i, currentFramePoints[i]);
            }

            Vector3 headPos = GetPointAtDistance(smoothedPath, segmentLengths, bodyLength);
            ShowArrow(headPos, arrowNode.MoveDirection, ArrowOffsetDistance);
        }

        private Vector3 GetDirectionAtDistance(List<Vector3> path, List<float> segLengths, float targetDist, Vector3 defaultDir)
        {
            if (targetDist <= 0f)
            {
                if (path.Count >= 2) return (path[1] - path[0]).normalized;
                return defaultDir;
            }
            float current = 0f;
            for (int i = 0; i < segLengths.Count; i++)
            {
                if (targetDist <= current + segLengths[i] + 0.001f)
                {
                    return (path[i + 1] - path[i]).normalized;
                }
                current += segLengths[i];
            }
            if (path.Count >= 2) return (path[path.Count - 1] - path[path.Count - 2]).normalized;
            return defaultDir;
        }

        private void ShowArrow(Vector3 position, Vector3 direction, float offsetDistance = 0.1f)
        {
            direction.Normalize();

            Vector3 baseCenter = position + direction * offsetDistance;
            Vector3 arrowBase = new Vector3(baseCenter.x, baseCenter.y, -0.1f);

            float renderHeight = ArrowLength * 0.866025f;

            Vector3 arrowTip = arrowBase + direction * renderHeight;

            int segmentCount = 5;
            lineRenderArrow.positionCount = segmentCount;

            for (int i = 0; i < segmentCount; i++)
            {
                float t = (float)i / (segmentCount - 1);
                Vector3 segmentPos = Vector3.Lerp(arrowBase, arrowTip, t);
                lineRenderArrow.SetPosition(i, segmentPos);
            }

            lineRenderArrow.enabled = true;
        }

        public override void OnPutToPool()
        {
            base.OnPutToPool();
            lineRenderLine.positionCount = 0;
            lineRenderArrow.enabled = false;
            StopFade();
        }

        public void PlayLevelAnim(Action onAnimOver)
        {
            PlayLevelAnim(3f, 30.0f, onAnimOver);
        }

        public void PlayLevelAnim(float duration, float speed, Action onAnimOver)
        {
            if (ArrowNode == null || ArrowNode.PathPoints == null || ArrowNode.PathPoints.Count < 2)
            {
                onAnimOver?.Invoke();
                return;
            }
            StartCoroutine(SnakeMoveRoutine(duration, speed, onAnimOver));
        }

        private System.Collections.IEnumerator SnakeMoveRoutine(float duration, float speed, Action onAnimOver)
        {
            List<Vector3> originalPath = ArrowNode.PathPoints;
            Vector3 firstPoint = originalPath[0];
            Vector3 secondPoint = originalPath[1];
            Vector3 startDir = (firstPoint - secondPoint).normalized;
            Vector3 extendedStart = firstPoint + startDir * PointRadius;

            List<Vector3> extendedPath = new List<Vector3>(originalPath);
            extendedPath[0] = extendedStart;

            Vector3 lastPoint = extendedPath[extendedPath.Count - 1];
            Vector3 escapePoint = lastPoint + ArrowNode.MoveDirection.normalized * (30f * duration);
            extendedPath.Add(escapePoint);

            List<Vector3> smoothedPath = GenerateSmoothPath(extendedPath);

            float bodyLength = 0f;
            List<float> segmentLengths = new List<float>();
            for (int i = 0; i < smoothedPath.Count - 1; i++)
            {
                float dist = Vector3.Distance(smoothedPath[i], smoothedPath[i + 1]);
                segmentLengths.Add(dist);
                if (i < smoothedPath.Count - 2)
                {
                    bodyLength += dist;
                }
            }

            List<Vector3> currentFramePoints = new List<Vector3>();
            float elapsed = 0f;
            float drivenDistance = 0f;

            while (elapsed < duration)
            {
                float headDist = bodyLength + drivenDistance;
                float tailDist = drivenDistance;

                ExtractFramePoints(smoothedPath, segmentLengths, tailDist, headDist, currentFramePoints);

                lineRenderLine.positionCount = currentFramePoints.Count;
                for (int i = 0; i < currentFramePoints.Count; i++)
                {
                    lineRenderLine.SetPosition(i, currentFramePoints[i]);
                }

                Vector3 trueHeadPos = GetPointAtDistance(smoothedPath, segmentLengths, headDist);
                Vector3 trueDirection = GetDirectionAtDistance(smoothedPath, segmentLengths, headDist, ArrowNode.MoveDirection);
                ShowArrow(trueHeadPos, trueDirection, ArrowOffsetDistance);

                yield return null;

                elapsed += Time.deltaTime;
                drivenDistance += speed * Time.deltaTime;
            }

            lineRenderLine.positionCount = 0;
            lineRenderArrow.enabled = false;
            onAnimOver?.Invoke();
        }

        private Vector3 GetPointAtDistance(List<Vector3> path, List<float> segLengths, float targetDist)
        {
            if (targetDist <= 0f) return new Vector3(path[0].x, path[0].y, 0);
            float current = 0f;
            for (int i = 0; i < segLengths.Count; i++)
            {
                if (targetDist <= current + segLengths[i])
                {
                    float t = (targetDist - current) / segLengths[i];
                    Vector2 pos = Vector2.Lerp(path[i], path[i + 1], t);
                    return new Vector3(pos.x, pos.y, 0);
                }
                current += segLengths[i];
            }
            Vector2 last = path[path.Count - 1];
            return new Vector3(last.x, last.y, 0);
        }

        public void PlayMoveCollisionAnim(Vector2 collisionPosition, Action onAnimOver = null)
        {
            PlayMoveCollisionAnim(collisionPosition, 40f, onAnimOver);
        }

        public void PlayMoveCollisionAnim(Vector2 collisionPosition, float speed, Action onAnimOver = null)
        {
            if (ArrowNode == null || ArrowNode.PathPoints == null || ArrowNode.PathPoints.Count < 2)
            {
                onAnimOver?.Invoke();
                return;
            }
            StartCoroutine(CollisionMoveRoutine(collisionPosition, speed, onAnimOver));
        }

        private System.Collections.IEnumerator CollisionMoveRoutine(Vector2 collisionPosition, float speed, Action onAnimOver = null)
        {
            List<Vector3> originalPath = ArrowNode.PathPoints;
            Vector3 firstPoint = originalPath[0];
            Vector3 secondPoint = originalPath[1];
            Vector3 startDir = (firstPoint - secondPoint).normalized;
            Vector3 extendedStart = firstPoint + startDir * PointRadius;

            List<Vector3> extendedPath = new List<Vector3>(originalPath);
            extendedPath[0] = extendedStart;
            extendedPath.Add(collisionPosition);

            List<Vector3> smoothedPath = GenerateSmoothPath(extendedPath);

            float bodyLength = 0f;
            List<float> segmentLengths = new List<float>();
            for (int i = 0; i < smoothedPath.Count - 1; i++)
            {
                float dist = Vector3.Distance(smoothedPath[i], smoothedPath[i + 1]);
                segmentLengths.Add(dist);
                if (i < smoothedPath.Count - 2)
                {
                    bodyLength += dist;
                }
            }

            float collisionMoveDistance = segmentLengths[segmentLengths.Count - 1];

            List<Vector3> currentFramePoints = new List<Vector3>();
            float drivenDistance = 0f;
            bool movingForward = true;

            if (speed <= 0f)
            {
                SetArrowData(ArrowNode);
                onAnimOver?.Invoke();
                yield break;
            }

            while (true)
            {
                float headDist = bodyLength + drivenDistance;
                float tailDist = drivenDistance;

                ExtractFramePoints(smoothedPath, segmentLengths, tailDist, headDist, currentFramePoints);

                lineRenderLine.positionCount = currentFramePoints.Count;
                for (int i = 0; i < currentFramePoints.Count; i++)
                {
                    lineRenderLine.SetPosition(i, currentFramePoints[i]);
                }

                Vector3 trueHeadPos = GetPointAtDistance(smoothedPath, segmentLengths, headDist);
                Vector3 trueDirection = GetDirectionAtDistance(smoothedPath, segmentLengths, headDist, ArrowNode.MoveDirection);
                ShowArrow(trueHeadPos, trueDirection, ArrowOffsetDistance);

                if (!movingForward && drivenDistance <= 0f)
                {
                    break;
                }

                yield return null;

                if (movingForward)
                {
                    drivenDistance += speed * Time.deltaTime;
                    if (drivenDistance >= collisionMoveDistance)
                    {
                        drivenDistance = collisionMoveDistance;
                        movingForward = false;
                    }
                }
                else
                {
                    drivenDistance -= speed * Time.deltaTime;
                    if (drivenDistance <= 0f)
                    {
                        drivenDistance = 0f;
                    }
                }
            }

            SetArrowData(ArrowNode);
            onAnimOver?.Invoke();
        }
        public void PlayEntryAnim(Action<ArrowLineSceneUnit> onLineAnimOver)
        {
            PlayEntryAnim(0.5f, onLineAnimOver);
        }

        public void PlayEntryAnim(float duration, Action<ArrowLineSceneUnit> onAnimOver = null)
        {
            if (ArrowNode == null || ArrowNode.PathPoints == null || ArrowNode.PathPoints.Count < 2)
            {
                onAnimOver?.Invoke(this);
                return;
            }
            StartCoroutine(EntryMoveRoutine(duration, onAnimOver));
        }

        private System.Collections.IEnumerator EntryMoveRoutine(float duration, Action<ArrowLineSceneUnit> onAnimOver = null)
        {
            List<Vector3> originalPath = ArrowNode.PathPoints;
            Vector3 firstPoint = originalPath[0];
            Vector3 secondPoint = originalPath[1];
            Vector3 startDir = (firstPoint - secondPoint).normalized;
            Vector3 extendedStart = firstPoint + startDir * PointRadius;

            List<Vector3> extendedPath = new List<Vector3>(originalPath);
            extendedPath[0] = extendedStart;

            List<Vector3> smoothedPath = GenerateSmoothPath(extendedPath);

            float bodyLength = 0f;
            List<float> segmentLengths = new List<float>();
            for (int i = 0; i < smoothedPath.Count - 1; i++)
            {
                float dist = Vector3.Distance(smoothedPath[i], smoothedPath[i + 1]);
                segmentLengths.Add(dist);
                bodyLength += dist;
            }

            List<Vector3> currentFramePoints = new List<Vector3>();
            float elapsed = 0f;

            lineRenderLine.positionCount = 0;
            lineRenderArrow.enabled = false;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);

                float tailDist = 0f;
                float headDist = bodyLength * progress;

                ExtractFramePoints(smoothedPath, segmentLengths, tailDist, headDist, currentFramePoints);

                lineRenderLine.positionCount = currentFramePoints.Count;
                for (int i = 0; i < currentFramePoints.Count; i++)
                {
                    lineRenderLine.SetPosition(i, currentFramePoints[i]);
                }

                Vector3 trueHeadPos = GetPointAtDistance(smoothedPath, segmentLengths, headDist);
                Vector3 trueDirection = GetDirectionAtDistance(smoothedPath, segmentLengths, headDist, ArrowNode.MoveDirection);
                ShowArrow(trueHeadPos, trueDirection, ArrowOffsetDistance);

                yield return null;
            }

            SetArrowData(ArrowNode);
            onAnimOver?.Invoke(this);
        }
    }
}
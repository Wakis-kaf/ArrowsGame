using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Framework.Runtime.LogSystem;
using Framework.Runtime.MSceneUnit;
using Framework.Runtime.UI;
using Framework.Runtime.UnitSystem.MonoBase;
using UnityEngine;
namespace Game.Modules.GModuleArrows
{
    public class ArrowPointSceneUnit : SceneUnit
    {
        #region PrefabBinder 自动引用区域 开始
        private UnityEngine.SpriteRenderer srModel => EntityPrefabBinder?.GetObj<UnityEngine.SpriteRenderer>("srModel");
        private TMPro.TextMeshPro tmpTxtId => EntityPrefabBinder?.GetObj<TMPro.TextMeshPro>("tmpTxtId");
        private UnityEngine.CircleCollider2D circleCollider => EntityPrefabBinder?.GetObj<UnityEngine.CircleCollider2D>("circleCollider");

        #endregion PrefabBinder 自动引用区域 结束
        private Action<ArrowPointSceneUnit> m_OnClick;
        private MonoEvents m_MonoEvents;
        private Vector3 m_OriginalScale;

        public LevelPointNode PointNode { get; private set; }
        protected override void OnSceneUnitGUI(object data)
        {
            // m_MonoEvents = circleCollider.gameObject.GetOrAddComponent<MonoEvents>();
            // m_MonoEvents.SetMouseUp(OnModelClick);
            // 1. 记录最原始的缩放大小

        }
        protected override void OnModelLoaded(GameObject modelGamObject)
        {
            base.OnModelLoaded(modelGamObject);
            if (srModel != null)
            {
                Color c = UIUtil.Hex2Color("787576");
                srModel.color = c;
            }
            UpdateSize();
        }

        private void OnModelClick()
        {
            if (ArrowsGameCameraInput.Ins.IsDragging || ArrowsGameCameraInput.Ins.IsZooming)
            {
                Log.Info("正在点击中，取消拖拽");
                return;
            }
            Log.Info("点点击");
            m_OnClick?.Invoke(this);
        }

        public void SetPointClick(Action<ArrowPointSceneUnit> onClick)
        {
            m_OnClick = onClick;

        }
        public void UpdateSize()
        {
            if (!IsModelLoaded() || PointNode == null) return;
            var animArgs = LevelVO.Current.LevelInfo.LevelAnimArgs;
            float radius = PointNode.isOccupied && !PointNode.isOccupyRemoved ? animArgs.arrowPointOccupiedRadius : animArgs.arrowPointUnOccupiedRadius;
            var size = radius * 2 * Vector3.one;
            m_OriginalScale = size;
            srModel.transform.localScale = size;
        }

        private void OnPointDataUpdate()
        {
            UpdateSize();
        }
        public void SetPointNodeData(LevelPointNode node)
        {
            PointNode = node;
            PointNode.OnUpdate = OnPointDataUpdate;
            if (tmpTxtId.gameObject.activeSelf)
            {
                tmpTxtId.text = node.id.ToString();
            }
            UpdateSize();
            if (LevelVO.Current.LevelInfo.levelCfg.isPointColorful)
            {
                srModel.color = node.color;
                // var a = srModel.color.a;
                // var color = node.color;
                // srModel.color = new Color(color.x, color.y, color.z, a);
            }
        }
        public override void OnGetFromPool()
        {
            base.OnGetFromPool();
            srModel.transform.localScale = Vector3.zero;

            if (PointNode != null)
            {
                PointNode.OnUpdate = null;
            }
            PointNode = null;

        }
        public override void OnPutToPool()
        {
            base.OnPutToPool();

            if (PointNode != null)
            {
                PointNode.OnUpdate = null;
            }
            PointNode = null;
            srModel.transform.localScale = Vector3.zero;

            StopEntryAnim();
            StopExitAnim();
            StopArrowExitPathThroughAnim();
        }
        private Sequence m_EntrySequence;
        private Action<ArrowPointSceneUnit> m_OnPointEntryAnimOver;

        public void PlayEntryAnim(Action<ArrowPointSceneUnit> onPointEntryAnimOver = null)
        {
            StopEntryAnim();
            if (srModel == null) return;
            m_OnPointEntryAnimOver = onPointEntryAnimOver;
            int maxIndex = LevelVO.Current.LevelPointLayout.MaxY;
            int minIndex = LevelVO.Current.LevelPointLayout.MinY;
            int step = PointNode.index.y - minIndex;
            int rstep = maxIndex - minIndex + 1 - step;
            float totalTime = LevelVO.Current.LevelInfo.LevelAnimArgs.pointEntryAnimTotalTime;
            float pointDelay = totalTime / (float)(maxIndex - minIndex + 1);

            int currentX = PointNode.index.x;
            float waveCurve = Mathf.Sin(currentX * 0.6f) * 0.05f;
            float calculatedDelay = 0.2f + (rstep * pointDelay) + waveCurve;

            float microNoise = UnityEngine.Random.Range(-0.01f, 0.01f);
            float delay = Mathf.Max(0, calculatedDelay + microNoise);
            if (!LevelVO.Current.LevelInfo.LevelAnimArgs.usePointDelay)
            {
                delay = 0;
            }

            srModel.transform.localScale = Vector3.zero;
            Color startColor = srModel.color;
            startColor.a = 0f;
            srModel.color = startColor;

            float zoomInDuration = pointDelay * 4f;
            float zoomOutDuration = pointDelay * 8f;

            m_EntrySequence = DG.Tweening.DOTween.Sequence();

            m_EntrySequence.AppendInterval(delay);

            m_EntrySequence.Append(srModel.transform.DOScale(m_OriginalScale * 1.4f, zoomInDuration).SetEase(DG.Tweening.Ease.OutQuad));
            m_EntrySequence.Join(srModel.DOFade(1f, zoomInDuration).SetEase(DG.Tweening.Ease.Linear));

            m_EntrySequence.Append(srModel.transform.DOScale(m_OriginalScale * 0.93f, zoomOutDuration * 0.4f).SetEase(DG.Tweening.Ease.InOutQuad));
            m_EntrySequence.Append(srModel.transform.DOScale(m_OriginalScale * 1.04f, zoomOutDuration * 0.3f).SetEase(DG.Tweening.Ease.InOutQuad));
            m_EntrySequence.Append(srModel.transform.DOScale(m_OriginalScale, zoomOutDuration * 0.3f).SetEase(DG.Tweening.Ease.OutSine));

            m_EntrySequence.OnKill(OnEntryAnimSequenceKill);
            m_EntrySequence.OnComplete(OnEntryAnimOver);
        }

        private void OnEntryAnimOver()
        {
            m_OnPointEntryAnimOver?.Invoke(this);
        }
        private Action<ArrowPointSceneUnit> m_OnPointExitAnimOver;
        private Sequence m_ExitSequence;
        private void SetPointExitSize()
        {
            var animArgs = LevelVO.Current.LevelInfo.LevelAnimArgs;
            float radius = animArgs.arrowPointSuccessRadius;
            var size = radius * 2 * Vector3.one;
            m_OriginalScale = size;
            srModel.transform.localScale = size;
        }
        public void PlayExitAnim(Action<ArrowPointSceneUnit> onPointExitAnimOver = null)
        {
            StopExitAnim();
            if (srModel == null) return;
            SetPointExitSize();
            m_OnPointExitAnimOver = onPointExitAnimOver;

            float centerX = (LevelVO.Current.LevelPointLayout.MaxX + LevelVO.Current.LevelPointLayout.MinX) / 2f;
            float centerY = (LevelVO.Current.LevelPointLayout.MaxY + LevelVO.Current.LevelPointLayout.MinY) / 2f;

            float distanceToCenter = Vector2.Distance(
                new Vector2(PointNode.index.x, PointNode.index.y),
                new Vector2(centerX, centerY)
            );

            float baseDelay = distanceToCenter * 0.08f;
            float microNoise = UnityEngine.Random.Range(-0.01f, 0.01f);
            float delay = Mathf.Max(0, baseDelay + microNoise);

            float durationTotal = 0.2f;
            float zoomInDuration = durationTotal * 0.3f;
            float zoomOutDuration = durationTotal * 0.7f;

            m_ExitSequence = DG.Tweening.DOTween.Sequence();

            m_ExitSequence.AppendInterval(delay);

            m_ExitSequence.Append(srModel.transform.DOScale(m_OriginalScale * 1.2f, zoomInDuration).SetEase(DG.Tweening.Ease.OutQuad));
            m_ExitSequence.Join(srModel.DOFade(0f, durationTotal).SetEase(DG.Tweening.Ease.InQuad));

            m_ExitSequence.Append(srModel.transform.DOScale(Vector3.zero, zoomOutDuration).SetEase(DG.Tweening.Ease.InQuad));

            m_ExitSequence.OnKill(OnExitAnimSequenceKill);
            m_ExitSequence.OnComplete(OnExitAnimOver);

        }
        private void OnExitAnimOver()
        {
            m_OnPointExitAnimOver?.Invoke(this);
        }

        private void StopExitAnim()
        {
            m_ExitSequence?.Kill();
            m_ExitSequence = null;
        }

        private void OnExitAnimSequenceKill()
        {
            m_ExitSequence = null;
        }
        private void StopEntryAnim()
        {
            m_EntrySequence?.Kill();
            m_EntrySequence = null;
        }
        private void OnEntryAnimSequenceKill()
        {
            m_EntrySequence = null;
        }

        private DG.Tweening.Sequence m_PathThroughSequence;

        public void PlayArrowExitPathThroughAnim(int order, Action<ArrowPointSceneUnit> onCompleteCb = null)
        {
            StopArrowExitPathThroughAnim();

            if (srModel == null)
            {
                onCompleteCb?.Invoke(this);
                return;
            }

            float delay = order * 0.035f;
            float durationTotal = 0.4f;
            float scaleUpDuration = durationTotal * 0.5f;
            float scaleDownDuration = durationTotal * 0.5f;

            m_PathThroughSequence = DG.Tweening.DOTween.Sequence();

            m_PathThroughSequence.AppendInterval(delay);

            m_PathThroughSequence.Append(srModel.transform.DOScale(m_OriginalScale * 1.8f, scaleUpDuration).SetEase(DG.Tweening.Ease.OutQuad));

            m_PathThroughSequence.Append(srModel.transform.DOScale(m_OriginalScale, scaleDownDuration).SetEase(DG.Tweening.Ease.OutBack));

            m_PathThroughSequence.OnKill(() => m_PathThroughSequence = null);
            m_PathThroughSequence.OnComplete(() =>
            {
                onCompleteCb?.Invoke(this);
            });
        }

        public void StopArrowExitPathThroughAnim()
        {
            m_PathThroughSequence?.Kill();
            m_PathThroughSequence = null;
        }
    }
}



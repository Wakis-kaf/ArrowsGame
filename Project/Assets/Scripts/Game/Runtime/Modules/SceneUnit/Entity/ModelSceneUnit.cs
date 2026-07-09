using Cysharp.Threading.Tasks;
using Framework.Runtime.MSceneUnit;
using Spine.Unity;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.Modules.GModuleSceneUnit
{
    public enum SpinSkeletonUpdateType
    {
        Auto,
        Custom
    }
    /// <summary>
    /// 单纯模型展示，无作用
    /// </summary>
    public class ModelSceneUnit : SceneUnit
    {
        private bool m_IsShowCut;
        private SkeletonAnimation SkeletonAnimation
        {
            get
            {
                if(m_SkeletonAnimation == null && EntityPrefabBinder!=null)
                {
                    m_SkeletonAnimation = EntityPrefabBinder.GetObj<SkeletonAnimation>();
                }
                return m_SkeletonAnimation;
            }
        }
        private SkeletonAnimation m_SkeletonAnimation;
        private MaterialPropertyBlock propertyBlock;
        private MeshRenderer meshRenderer;
        private Color m_OriginalColor;
        private float m_OriginalFillPhase;
        private SpinSkeletonUpdateType m_SpinSkeletonUpdateType = SpinSkeletonUpdateType.Auto;
        protected override void OnModelLoaded(GameObject modelGamObject)
        {
            base.OnModelLoaded(modelGamObject);
            propertyBlock = new MaterialPropertyBlock();
            m_OriginalColor = Color.white;
            m_OriginalFillPhase = 0;
            CheckCutRendering();
        }
        public void SetSkeletonAnimUpdateType(SpinSkeletonUpdateType type)
        {
            m_SpinSkeletonUpdateType = type;
            CheckSkeletAnimation();

        }
        private void CheckSkeletAnimation()
        {
            if (SkeletonAnimation == null) return;
            SkeletonAnimation.enabled = m_SpinSkeletonUpdateType == SpinSkeletonUpdateType.Auto;
        }
        public override void OnGetFromPool()
        {
            base.OnGetFromPool();
            CancelCut();
            SetSkeletonAnimUpdateType(SpinSkeletonUpdateType.Auto);
        }
        public override void OnPutToPool()
        {
            base.OnPutToPool();
            CancelCut();
        }
        public void ShowCut()
        {
            if (m_IsShowCut) return;
            m_IsShowCut = true;
            CheckCutRendering();
        }
        public void CancelCut()
        {
            if (!m_IsShowCut) return;
            m_IsShowCut = false;
            CheckCancelCut();
        }
        private void CheckCancelCut()
        {
            if (m_IsShowCut) return;
            if (SkeletonAnimation != null)
            {
                SkeletonAnimation.enabled = true;
                if(meshRenderer!=null && propertyBlock != null)
                {
                    UpdateFillColor(Color.white);
                    UpdateFillPhase(0);
                }
                
            }
        }
        public override void OnUnitUpdate()
        {
            base.OnUnitUpdate();
            UpdateSpinAnimation();
        }
        private void UpdateSpinAnimation()
        {
            if (m_SpinSkeletonUpdateType == SpinSkeletonUpdateType.Auto || SkeletonAnimation == null) return;
            SkeletonAnimation.Update(Time.unscaledDeltaTime);
            SkeletonAnimation.LateUpdate(); // 必须调用 LateUpdate 来更新 Mesh
        }
        private void CheckCutRendering()
        {
            if (!m_IsShowCut || !IsModelLoaded()) return;
            if (SkeletonAnimation != null)
            {
                SkeletonAnimation.enabled = false;
                meshRenderer = SkeletonAnimation.GetComponent<MeshRenderer>();
                UpdateFillColor(Color.black);
                UpdateFillPhase(1);
            }
            
        }
        public void UpdateFillColor(Color color)
        {
            if (meshRenderer == null) return;

            meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_FillColor", color);
            meshRenderer.SetPropertyBlock(propertyBlock);
        }

        // 更新填充进度
        public void UpdateFillPhase(float phase)
        {
            if (meshRenderer == null) return;

            meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat("_FillPhase", Mathf.Clamp01(phase));
            meshRenderer.SetPropertyBlock(propertyBlock);
        }

        // 同时设置颜色和进度
        public void SetFillEffect(Color color, float phase, float resumTimer = -1)
        {
            if (meshRenderer == null) return;

            meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_FillColor", color);
            propertyBlock.SetFloat("_FillPhase", Mathf.Clamp01(phase));
            meshRenderer.SetPropertyBlock(propertyBlock);
            if (resumTimer > 0)
            {
                 ResumeColor(resumTimer).Forget();
            }
        }
        private async UniTask ResumeColor(float timer)
        {
            await UniTask.WaitForSeconds(timer);
            SetFillEffect(m_OriginalColor, m_OriginalFillPhase, -1);
        }


    }
}
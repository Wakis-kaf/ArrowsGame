using BehaviorDesigner.Runtime.Tasks.Unity.UnityParticleSystem;
using Cysharp.Threading.Tasks;
using Framework.Runtime.MCombat;
using Framework.Runtime.MSceneUnit;
using Spine.Unity;
using System.Threading;
using UnityEngine;

namespace Game.Modules.GModuleSceneUnit
{
    public class RoleSceneUnit : SceneUnit, ICombatEventReceiver
    {
        public Combator Combator { get; protected set; }
        public bool IsAttackEnable { get; private set; } = true;
        protected SkeletonAnimation skeletonAnimation;
        protected MaterialPropertyBlock propertyBlock;
        private MeshRenderer meshRenderer;
        private Color m_OriginalColor;
        private float m_OriginalFillPhase;
        private CancellationTokenSource m_ResumeTokenSource;
        public void DisableAttack()
        {
            IsAttackEnable = false;
        }
        private void CheckRendering()
        {
            if (!IsModelLoaded() || meshRenderer != null) return;
            skeletonAnimation = EntityPrefabBinder.GetObj<SkeletonAnimation>();
            if (skeletonAnimation != null)
            {
                meshRenderer = skeletonAnimation.GetComponent<MeshRenderer>();
            }
            m_OriginalColor = Color.white;
            m_OriginalFillPhase = 0;
            propertyBlock = new MaterialPropertyBlock();
        }
        //public virtual void DoHit(CombProto_TowerAttack hitProto)
        //{
        //}

        public void EnableAttack()
        {
            IsAttackEnable = true;
        }
        public void DisableSkelAnim()
        {
            //skeletonAnimation.enabled = false;
            skeletonAnimation.timeScale = 0;
        }
        public void EnableSkelAnim()
        {
            //skeletonAnimation.enabled = true;
            skeletonAnimation.timeScale = 1;
        }
        public float GetNumAttrFloatValue(string code)
        {
            return (float)Combator.AttributeBox.GetNumberAttribute(code).FinalValue;
        }

        public NumberAttribute GetNumberAttribute(string code)
        {
            return Combator.AttributeBox.GetNumberAttribute(code);
        }

        public int GetNumberAttrIntValue(string code)
        {
            return (int)Combator.AttributeBox.GetNumberAttribute(code).FinalValue;
        }
        protected override void OnModelLoaded(GameObject modelGamObject)
        {
            base.OnModelLoaded(modelGamObject);
            CheckRendering();
        }

        public override void OnUnitAwake()
        {
            base.OnUnitAwake();
            //Combator = CombatSystem.Ins.CreateCombator();
            //Combator.Context.SetData(CombatorAgrNames.OwnSceneUnit, this);
            //Combator.MoutReceiver(this);
        }
        public virtual CombatEvent SendEvent(CombatEvent combatEvent)
        {
            return combatEvent;
        }

        public virtual CombatEvent HandleEvent(CombatEvent combatEvent)
        {
            return combatEvent;
        }

        public virtual CombatEvent ReceiveEvent(CombatEvent combatEvent)
        {
            return combatEvent;
        }

        public virtual bool IsActive()
        {
            return false;
        }
        public virtual bool IsEnabled()
        {
            return !IsDisposed;
        }
        public void ResetFillPhase()
        {
            SetFillEffect(m_OriginalColor, 0, -1);
        }
        public void SetFillEffect(Color color, float phase, float resumTimer = -1)
        {
            if (meshRenderer == null) return;
            meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_FillColor", color);
            propertyBlock.SetFloat("_FillPhase", Mathf.Clamp01(phase));
            meshRenderer.SetPropertyBlock(propertyBlock);
            CancelResumeTask();
            if (resumTimer > 0)
            {
                m_ResumeTokenSource = new CancellationTokenSource();
                ResumeColor(resumTimer, m_ResumeTokenSource.Token).Forget();
            }
        }
        private void CancelResumeTask()
        {
            if (m_ResumeTokenSource != null)
            {
                m_ResumeTokenSource.Cancel();
                m_ResumeTokenSource.Dispose();
                m_ResumeTokenSource = null;
            }
        }
        private async UniTask ResumeColor(float timer, CancellationToken token)
        {
            await UniTask.WaitForSeconds(timer, cancellationToken: token);
            SetFillEffect(m_OriginalColor, m_OriginalFillPhase, -1);
        }
    }
}
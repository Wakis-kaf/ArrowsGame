using DG.Tweening;
using Framework.Runtime;
using Framework.Runtime.UI;
using Framework.Runtime.UI.UIAnimae;
using Framework.Runtime.UnitSystem.BIInterfaces;

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace Game.Modules.GModuleTip
{
    public class Tip : DisplayUnit
    {
        public string animatorStartName = "TipStart";
        public string animatorEndName = "TipEnd";
        private string m_TipTypeName;
        public string TipTypeName
        {
            get
            {
                if(m_TipTypeName == null)
                {
                    m_TipTypeName = GetType().Name;
                }
                return m_TipTypeName;
            }
        }
        public TipOption option;
        private Vector3 m_Follow;
        private Transform m_FollowTrans;
        private Vector3 m_Offset;
        private bool m_IsInPool = false;
        public bool IsInPool
        {
            get
            {
                return m_IsInPool;
            }
        }
        private Camera m_GameCamera;
        public override bool IsActiveChangeParent { get; set; } = true;
        public Rect GetRect()
        {
            var pos = GetAnchoredPosition();
            Rect ort = RectTransform.rect;
            Rect rect = new Rect(pos.x, pos.y, ort.width, ort.height);
            return rect;
        }
       
        public Vector2 GetSize()
        {
            return RectTransform.sizeDelta;
        }
        public Tip()
        {
            IsGap = false;
        }
        public  void PutToPool()
        {
            m_IsFollow = false;
            GameTip.GetIns().PutTip(this);
      
        }
        public bool IsOverlaps(Tip otherTip)
        { 
            return otherTip != this && GetRect().Overlaps(otherTip.GetRect());
        }
        public void SetOption(TipOption option)
        {
            this.option = option;
        }

        protected override void OnShow()
        {
            GameApp.Ins.LoopManager.AddLateLoop(UpdatePos);
            UpdatePos();
        }
        public void SetPosition(Vector2 position)
        {
            this.RectTransform.anchoredPosition3D = position;
        }
   
        
        public virtual void OnPlayStartAnimation(System.Action cb)
        {
            UpdatePos();
            var uiAnimator = this.UIAnimator;
            if (uiAnimator == null) return;
            bool waiting = true;
            uiAnimator.SetComplete(this.animatorStartName, () =>
            {
                if (!waiting && uiAnimator.IsSequenceComplete(this.animatorStartName))
                {
                    cb();
                }
            });

            waiting = false;
            uiAnimator.Call(this.animatorStartName, TweenerCallType.Restart);
        }

        public virtual void OnPlayEndAnimation(System.Action cb)
        {
            var uiAnimator = this.UIAnimator;
            if (uiAnimator == null) return;

            bool waiting = true;
            uiAnimator.SetComplete(this.animatorEndName, () =>
            {
                if (!waiting && uiAnimator.IsSequenceComplete(this.animatorEndName))
                {
                    cb();
                }
            });

            waiting = false;
            uiAnimator.Call(this.animatorEndName, TweenerCallType.Restart);
        }

        public virtual void ResetByOption(TipOption option)
        {
            
        }
        private bool m_IsFollow = false;
        public virtual void BindFollow(Transform follow, Camera gameCamera = default, Vector3 offset = default)
        {
            m_IsFollow = true;
            m_FollowTrans = follow;
            m_Follow = follow.position;
            m_Offset = offset;
            m_GameCamera = gameCamera;
            if (DisplayGO != null)
            {
                UpdatePos();
            }
        }
        public virtual void BindFollow(Vector3 follow,Camera gameCamera = default,Vector3 offset = default)
        {
            m_IsFollow = true;
            m_Follow = follow;
            m_FollowTrans = null;
            m_Offset = offset;
            m_GameCamera = gameCamera;
            if (DisplayGO != null)
            {
                UpdatePos();
            }
        }
        protected override void OnHide()
        {
            base.OnHide();
            GameApp.Ins.LoopManager.RemoveLateLoop(UpdatePos);
        }
        protected void UpdatePos()
        {
            if (!m_IsFollow|| m_GameCamera == null) return;
            SetAnchoredPosition(GetAnchorPos());

        }
        private Vector2 GetAnchorPos()
        {
            Camera gameCamera = m_GameCamera;
            if (m_FollowTrans != null)
            {
                m_Follow = m_FollowTrans.position;
            }
            Vector2 anchorPos = UIUtil.GetWorl2AnchorPosition(gameCamera,
                m_Follow + m_Offset,
                UIRootCamera.Camera,
                (RectTransform)ParentTransform);
            return anchorPos;
        }

        public virtual void OnPutToPool()
        {
            m_IsInPool = true;
        }

        public virtual void OnGetFromPool()
        {
          m_IsInPool = false;
            m_IsFollow = false;
        }
    }
}

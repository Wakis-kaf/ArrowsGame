using Framework.Runtime;
using Framework.Runtime.UI;
using Game.Modules.GModuleBar;
using Game.Modules.GModuleTip;
using System;
using UnityEditor;
using UnityEngine;
namespace Game.Modules
{

    public class Bar : DisplayUnit
    {
        public BarOption option;
        private Transform m_Follow;
        private Camera m_GameCamera;
        private Vector3 m_EntityOffset;
        public override bool IsActiveChangeParent { get; set; } = false;
        public void SetOption(BarOption option)
        {
            this.option = option;
        }
        public void BindFollow(Transform follow, Camera gameCamera,Vector3 entityOffset = default)
        {
            m_Follow = follow;
            m_GameCamera = gameCamera;
            m_EntityOffset = entityOffset;
            if (DisplayGO != null)
            {
                UpdatePos();
            }
        }
        protected override void OnShow()
        {
            GameApp.Ins.LoopManager.AddLateLoop(UpdatePos);
            UpdatePos();
        }
        protected override void OnHide()
        {
            base.OnHide();
            GameApp.Ins.LoopManager.RemoveLateLoop(UpdatePos);
        }
        private void UpdatePos()
        {
            if (m_Follow == null || m_GameCamera == null) return;
            SetAnchoredPosition(GetAnchorPos());

        }
        private Vector2 GetAnchorPos()
        {
            Camera gameCamera = m_GameCamera;
            Vector2 anchorPos = UIUtil.GetWorl2AnchorPosition(gameCamera,
                m_Follow.position + m_EntityOffset,
                UIRootCamera.Camera,
                (RectTransform)ParentTransform);
            return anchorPos;
        }
        public void PutToPool()
        {
            GameBar.GetIns().PutBar(this);
        }
        public virtual void ResetByOption(BarOption option)
        {
           
        }
    }
}
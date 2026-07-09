using Framework.Runtime;
using Framework.Runtime.UI;
using System;
using UnityEngine;

namespace Game.Modules.GModuleTip
{
    public class CommonMsgTip : Tip
    {
        public float firstMoveTime = 0.1f;
        public float pauseTime = 1f;
        public float secondMoveTime = 0.5f;
        public float moveSpeed = 500;

        private float firstMoveTimer, pauseTimer, secondMoveTimer;
        public UText utxtTip = null;
        private bool isPlaying = false;
        private Action playCompletedCb;

        // 【核心】地基高度（会被别的 Tip 顶起来）
        public float BaseY { get; set; }
        // 【核心】自身动画产生的偏移
        public float AnimOffset { get; private set; }
        private float initialX;

        protected override void OnInitUI()
        {
            base.OnInitUI();
            this.utxtTip = GetBindObject<UText>("utxtTip");
        }

        protected override void OnGUI(object data)
        {
            this.utxtTip.text = data as string;
            Canvas.ForceUpdateCanvases();
        }

        public override void ResetByOption(TipOption option)
        {
            BaseY = option.popAnchorPos.y;
            initialX = option.popAnchorPos.x;
            AnimOffset = 0;
            RectTransform.anchoredPosition = option.popAnchorPos;
        }

        public override void OnPlayStartAnimation(Action cb)
        {
            isPlaying = true;
            firstMoveTimer = firstMoveTime;
            pauseTimer = pauseTime;
            secondMoveTimer = secondMoveTime;
            playCompletedCb = cb;
            GameApp.Ins.LoopManager.AddLoop(OnUnitUpdate);
        }

        public void OnUnitUpdate()
        {
            if (!isPlaying) return;
            float deltaTime = Time.unscaledDeltaTime;

            if (firstMoveTimer > 0) { firstMoveTimer -= deltaTime; AnimOffset += deltaTime * moveSpeed; }
            else if (pauseTimer > 0) { pauseTimer -= deltaTime; }
            else if (secondMoveTimer > 0) { secondMoveTimer -= deltaTime; AnimOffset += deltaTime * moveSpeed; }
            else
            {
                isPlaying = false;
                playCompletedCb?.Invoke();
                return;
            }
            UpdateTransform();
        }

        public void UpdateTransform()
        {
            RectTransform.anchoredPosition = new Vector2(initialX, BaseY + AnimOffset);
        }

        public override void OnPlayEndAnimation(Action cb)
        {
            GameApp.Ins.LoopManager.RemoveLoop(OnUnitUpdate);
            isPlaying = false;
            cb?.Invoke();
        }

        // 获取该 Tip 当前底部的实时世界/Canvas坐标 (Y)
        public float GetCurrentBottomY() => BaseY + AnimOffset;
        // 获取该 Tip 当前顶部的实时世界/Canvas坐标 (Y)
        public float GetCurrentTopY() => BaseY + AnimOffset + GetRect().height;
    }
}
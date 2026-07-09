using System.Collections.Generic;
using UnityEngine;

namespace Game.Modules.GModuleTip
{
    public class TipPositionManager
    {
        private Dictionary<string, List<CommonMsgTip>> activeTips = new Dictionary<string, List<CommonMsgTip>>();
        public float spacing = 10f;

        public void RegisterTip(Tip tip, TipOption option)
        {
            if (!(tip is CommonMsgTip newTip)) return;

            string type = tip.TipTypeName;
            if (!activeTips.ContainsKey(type)) activeTips[type] = new List<CommonMsgTip>();

            List<CommonMsgTip> list = activeTips[type];

            // 1. 新 Tip 永远按照配置的初始位置出生
            newTip.BaseY = option.popAnchorPos.y;
            newTip.UpdateTransform();
            list.Add(newTip);

            // 2. 链式碰撞检查：从最新加入的（最后一个）往回检查到第一个
            ResolvePushing(list);
        }

        private void ResolvePushing(List<CommonMsgTip> list)
        {
            if (list.Count < 2) return;

            // 从后往前推（i 是下方，i-1 是上方）
            for (int i = list.Count - 1; i > 0; i--)
            {
                CommonMsgTip lower = list[i];
                CommonMsgTip upper = list[i - 1];

                float lowerTop = lower.GetCurrentTopY();
                float upperBottom = upper.GetCurrentBottomY();

                // 如果下方的顶部 撞到了 上方的底部
                if (lowerTop > upperBottom - spacing)
                {
                    // 计算需要把上方 Tip 顶起来的距离
                    float pushDistance = lowerTop - (upperBottom - spacing);

                    // 增加上方 Tip 的 BaseY (地基被顶高了)
                    upper.BaseY += pushDistance;
                    upper.UpdateTransform();

                    // 继续循环，下一轮会检查这个被顶高的 upper 是否撞到了它的上方
                }
                else
                {
                    // 如果这一层没撞到，那更高的层也不会被这波推挤影响，直接跳出
                    break;
                }
            }
        }

        public void UnregisterTip(Tip tip)
        {
            if (tip is CommonMsgTip msgTip && activeTips.TryGetValue(tip.TipTypeName, out var list))
            {
                list.Remove(msgTip);
            }
        }

        public void OnUpdate()
        {
            // 在每一帧中，如果下方 Tip 因为动画速度快撞到了上方，也需要触发推挤
            foreach (var list in activeTips.Values)
            {
                ResolvePushing(list);
            }
        }

        public void ClearAll()
        {
            activeTips.Clear();
        }
    }
}
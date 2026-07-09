using UnityEngine;

namespace Framework.Runtime.MLanAndTheme
{
    public class MouseKey : PushKey
    {
        private int mBindKey;

        public MouseKey(int key)
        {
            this.mBindKey = key;
        }

        public override bool IsDown
        {
            get => Input.GetMouseButtonDown(mBindKey);
        }

        public override bool IsPushing
        {
            get => Input.GetMouseButton(mBindKey);
        }

        public override bool IsUp
        {
            get => Input.GetMouseButtonUp(mBindKey);
        }

        public void ResetKey(int key)
        {
            this.mBindKey = key;
        }
    }
}
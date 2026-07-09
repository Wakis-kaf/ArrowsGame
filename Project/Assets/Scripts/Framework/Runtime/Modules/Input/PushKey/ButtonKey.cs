using UnityEngine;

namespace Framework.Runtime.MLanAndTheme
{
    public class ButtonKey : PushKey
    {
        private string mBtnKey;

        public ButtonKey(string buttonName)
        {
            mBtnKey = buttonName;
        }

        public override bool IsDown
        {
            get => Input.GetButtonDown(mBtnKey);
        }

        public override bool IsPushing
        {
            get => Input.GetButton(mBtnKey);
        }

        public override bool IsUp
        {
            get => Input.GetButtonUp(mBtnKey);
        }
    }
}
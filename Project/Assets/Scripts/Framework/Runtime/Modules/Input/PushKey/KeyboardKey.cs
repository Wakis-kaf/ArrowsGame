using UnityEngine;

namespace Framework.Runtime.MLanAndTheme
{
    public class KeyboardKey : PushKey
    {
        private KeyCode mBindKey;

        public KeyboardKey(KeyCode k)
        {
            this.mBindKey = k;
        }

        public override bool IsDown => Input.GetKeyDown(mBindKey);
        public override bool IsPushing => Input.GetKey(mBindKey);
        public override bool IsUp => Input.GetKeyUp(mBindKey);

        public void ResetKey(KeyCode k)
        {
            this.mBindKey = k;
        }
    }
}
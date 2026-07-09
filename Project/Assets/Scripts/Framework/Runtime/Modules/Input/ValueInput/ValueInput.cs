using UnityEngine;

namespace Framework.Runtime.MLanAndTheme
{
    public class AxisInput : ValueInput
    {
        private string mBindAxisName;

        public AxisInput(string name)
        {
            this.mBindAxisName = name;
        }

        public override float Value
        {
            get => Input.GetAxis(mBindAxisName);
        }

        public override float ValueRaw
        {
            get => Input.GetAxisRaw(mBindAxisName);
        }

        public void ResetName(string name)
        {
            this.mBindAxisName = name;
        }
    }

    public class ScrollInputX : ValueInput
    {
        public override float Value
        {
            get => Input.mouseScrollDelta.x;
        }
    }

    public class ScrollInputY : ValueInput
    {
        public override float Value
        {
            get => Input.mouseScrollDelta.y;
        }
    }

    public abstract class ValueInput
    {
        public abstract float Value { get; }
        public virtual float ValueRaw { get; }
    }
}
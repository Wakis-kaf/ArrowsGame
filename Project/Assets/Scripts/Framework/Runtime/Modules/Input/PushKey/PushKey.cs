namespace Framework.Runtime.MLanAndTheme
{
    public abstract class PushKey
    {
        public abstract bool IsDown { get; }
        public abstract bool IsPushing { get; }
        public abstract bool IsUp { get; }
    }
}
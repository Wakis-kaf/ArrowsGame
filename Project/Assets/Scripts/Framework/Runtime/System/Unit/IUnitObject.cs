using System;

namespace Framework.Runtime.Base
{
    public interface IUnitObject : IDisposable
    {
        public bool IsDisposed { get; set; }
        public Type Type { get; }
    }
}
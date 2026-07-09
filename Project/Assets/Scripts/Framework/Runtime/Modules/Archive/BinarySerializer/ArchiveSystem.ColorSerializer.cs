using System.Runtime.Serialization;
using UnityEngine;

namespace Framework.Runtime.Archives
{
    public class ColorSerializer : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Color q = (Color)obj;
            info.AddValue("r", q.r);
            info.AddValue("g", q.g);
            info.AddValue("b", q.b);
            info.AddValue("a", q.a);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context,
            ISurrogateSelector selector)
        {
            Color q = (Color)obj;
            q.r = info.GetSingle("r");
            q.g = info.GetSingle("g");
            q.b = info.GetSingle("b");
            q.a = info.GetSingle("a");

            obj = q;
            return obj;
        }
    }
}
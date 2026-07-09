using System.Runtime.Serialization;
using UnityEngine;

namespace Framework.Runtime.Archives
{
    public class RectSerializer : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Rect q = (Rect)obj;
            info.AddValue("x", q.x);
            info.AddValue("y", q.y);
            info.AddValue("width", q.width);
            info.AddValue("height", q.height);
            info.AddValue("xMax", q.xMax);
            info.AddValue("yMax", q.yMax);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context,
            ISurrogateSelector selector)
        {
            Rect q = (Rect)obj;
            q.x = info.GetSingle("x");
            q.y = info.GetSingle("y");
            q.width = info.GetSingle("width");
            q.xMax = info.GetSingle("xMax");
            q.yMax = info.GetSingle("yMax");

            obj = q;
            return obj;
        }
    }
}
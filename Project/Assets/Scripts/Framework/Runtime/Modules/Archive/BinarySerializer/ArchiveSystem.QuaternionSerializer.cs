using System.Runtime.Serialization;
using UnityEngine;

namespace Framework.Runtime.Archives
{
    public class QuaternionSerializer : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Quaternion q = (Quaternion)obj;

            info.AddValue("x", q.x);
            info.AddValue("y", q.y);
            info.AddValue("z", q.z);
            info.AddValue("w", q.w);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context,
            ISurrogateSelector selector)
        {
            Quaternion q = (Quaternion)obj;
            q.x = info.GetSingle("x");
            q.y = info.GetSingle("y");
            q.z = info.GetSingle("z");
            q.w = info.GetSingle("w");
            obj = q;
            return obj;
        }
    }
}
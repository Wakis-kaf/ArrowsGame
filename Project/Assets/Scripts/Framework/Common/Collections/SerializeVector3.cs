using UnityEngine;

namespace Framework.Collections
{
    [System.Serializable]
    public struct SerializeVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializeVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public SerializeVector3(Vector3 vector3)
        {
            this.x = vector3.x;
            this.y = vector3.y;
            this.z = vector3.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    public static class Vector3Extension
    {
        public static SerializeVector3[] ConvertToSerializeVector3Array(this Vector3[] array)
        {
            SerializeVector3[] res = new SerializeVector3[array.Length];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = new SerializeVector3(array[i]);
            }

            return res;
        }

        public static Vector3[] DeSerializeVector3Array(this SerializeVector3[] array)
        {
            Vector3[] res = new Vector3[array.Length];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = new Vector3(array[i].x, array[i].y, array[i].z);
            }

            return res;
        }
    }
}
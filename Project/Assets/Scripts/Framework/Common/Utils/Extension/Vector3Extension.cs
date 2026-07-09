using UnityEngine;

public static class Vector3Extension
{
    /// <summary>
    /// 将某个三维向量转换为最接近的轴向量
    /// </summary>
    /// <param name="v3"></param>
    /// <returns></returns>
    public static Vector3 GetSnapVector3(this Vector3 v3)
    {
        Vector3 origin = v3;
        Vector3Int res = Vector3Int.zero;
        if (v3 == Vector3.zero)
            return res;
        float maxAngle = int.MinValue;
        float angleY = Mathf.Abs(Vector3.Dot(Vector3.up, origin));
        int dorDir = 0;
        if (maxAngle < angleY)
        {
            maxAngle = angleY;
            res = Vector3Int.up;
            dorDir = origin.y > 0 ? 1 : -1;
        }
        float angleX = Mathf.Abs(Vector3.Dot(Vector3.right, origin));
        if (maxAngle < angleX)
        {
            maxAngle = angleX;
            res = Vector3Int.right;
            dorDir = origin.x > 0 ? 1 : -1;
        }
        float angleZ = Mathf.Abs(Vector3.Dot(Vector3.forward, origin));
        if (maxAngle < angleZ)
        {
            maxAngle = angleZ;
            res = Vector3Int.forward;
            dorDir = origin.z > 0 ? 1 : -1;
        }
        return res * dorDir;
    }

    public static bool IsNearEqual(this Vector3 v3, Vector3 target, float precision = 0.01f)
    {
        return v3.magnitude - target.magnitude < precision;
    }

    public static Vector3 Round(this Vector3 v3, float round = 1f)
    {
        if (Mathf.Abs(v3.x) < round) v3.x = 0;
        if (Mathf.Abs(v3.y) < round) v3.y = 0;
        if (Mathf.Abs(v3.z) < round) v3.z = 0;
        return v3;
    }

    public static Vector3 Round(this Vector3 v3, Vector3 round)
    {
        if (Mathf.Abs(v3.x) < round.x) v3.x = 0;
        if (Mathf.Abs(v3.y) < round.y) v3.y = 0;
        if (Mathf.Abs(v3.z) < round.z) v3.z = 0;
        return v3;
    }

    /// <summary>
    /// 转换为整数类型的Vector3
    /// </summary>
    /// <param name="v3"></param>
    /// <returns></returns>
    public static Vector3Int ToVector3Int(this Vector3 v3)
    {
        return new Vector3Int((int)v3.x, (int)v3.y, (int)v3.z);
    }
}
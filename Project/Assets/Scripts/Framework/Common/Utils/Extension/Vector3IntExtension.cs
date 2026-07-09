using UnityEngine;

public static class Vector3IntExtension
{
    // 点积：a·b = ax*bx + ay*by + az*bz
    public static int Dot(this Vector3Int a, Vector3Int b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }

    // 叉积：a × b
    public static Vector3Int Cross(this Vector3Int a, Vector3Int b)
    {
        return new Vector3Int(
            a.y * b.z - a.z * b.y,
            a.z * b.x - a.x * b.z,
            a.x * b.y - a.y * b.x
        );
    }
}
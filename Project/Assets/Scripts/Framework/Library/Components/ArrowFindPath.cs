using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class ArrowFindPath : MonoBehaviour
{
    // 移动到的目标点
    public Transform moveEndPoint;
    // 画线组件
    public LineRenderer lineGameObject;
    // 画线路径点的集合（用于计算 UV ）
    private List<Vector3> arrowList = new List<Vector3>();
    // 增量（一个点与下一个的距离）
    private Vector3 pointNumber;
    // 起点到终点分成多少份（在 LineRenderer 组件中添加的 position 数目）
    private float num;

    private void Start()
    {
        lineGameObject.transform.eulerAngles = new Vector3(90f, 0f, 0f);
        lineGameObject.startWidth = 0.5f;
        lineGameObject.endWidth = 0.5f;
        lineGameObject.alignment = LineAlignment.TransformZ;
    }

    void Update()
    {
        // 计算点数（除数为：lineGameObject.startWidth，可以避免 1：1 的材质球被拉伸）
        num = Vector3.Distance(moveEndPoint.position, transform.position) / lineGameObject.startWidth;
        // 计算增量
        pointNumber = (moveEndPoint.position - transform.position) / num;
        // 给 LineRender 组件的 Position 赋值
        for (int i = 0; i < num; i++)
        {
            // 从
            Ray ray = new Ray((transform.position + i * pointNumber) + Vector3.up * 10f, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                lineGameObject.positionCount = i + 1;
                lineGameObject.SetPosition(i, hit.point + (Vector3.up * 0.1f));
                arrowList.Add(hit.point + (Vector3.up * 0.1f));
            }
        }
        // 使 LineRenderer 的前四个值等于第四个值
        for (int i = 0; i < 4; i++)
        {
            lineGameObject.SetPosition(i, lineGameObject.GetPosition(4));
            arrowList[i] = lineGameObject.GetPosition(4);
        }
        // 使 LineRenderer 的后四个值等于倒数第四个值
        for (int i = 1; i <= 4; i++)
        {
            lineGameObject.SetPosition(lineGameObject.positionCount - i, lineGameObject.GetPosition(lineGameObject.positionCount - 4));
            arrowList[lineGameObject.positionCount - i] = lineGameObject.GetPosition(lineGameObject.positionCount - 4);
        }

        lineGameObject.material.SetTextureScale("_MainTex", new Vector2(UVTiling(arrowList.ToArray(), lineGameObject.startWidth), 1f));

        arrowList.Clear();

        // 移动效果
        lineGameObject.material.SetTextureOffset("_MainTex", new Vector2(UVOffset(lineGameObject.material.GetTextureOffset("_MainTex")), 1f));

    }

    // 计算 UV 值
    private float UVTiling(Vector3[] lineNumber, float lineWidth)
    {
        float f = 0f;
        for (int i = 0; i < lineNumber.Length - 1; i++)
        {
            f += Vector3.Distance(lineNumber[i], lineNumber[i + 1]);
        }
        return f / lineWidth;
    }

    private float UVOffset(Vector2 offset)
    {
        float f = offset.x;
        return f -= 0.05f;
    }

    // 创建一个游戏物体
    private GameObject CreateGameObject(Vector3 position)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.transform.position = position;
        obj.transform.localScale = Vector3.one * 0.1f;
        obj.GetComponent<MeshRenderer>().material.color = Color.blue;
        return obj;
    }
}
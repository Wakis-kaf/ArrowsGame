using UnityEngine;

public class ArrowUVFlow : MonoBehaviour
{
    public float scrollSpeed = 1.0f; // 移动速度
    public Vector2 direcation = Vector2.down;
    private Renderer rend;
    public string sortingLayerName = "Default";
    public int sortingOrder = 10;
    public bool isUnScaledTime = true;

    void Start()
    {
        rend = GetComponent<Renderer>();
        // 设置排序图层（必须是你在标签管理器中已经创建好的名字，默认是 "Default"）
        rend.sortingLayerName = sortingLayerName;
        // 设置层级顺序，数值越大越靠前（显示在最上面）
        rend.sortingOrder = sortingOrder;
    }

    void Update()
    {
        // 计算随时间变化的偏移值
        float offset = (isUnScaledTime?Time.unscaledTime:Time.time) * scrollSpeed;

        // _MainTex 是大多数 Shader 默认的纹理名称
        // 如果箭头方向反了，可以把 offset 传给 Y 轴，或者改为 -offset
        rend.material.SetTextureOffset("_MainTex", direcation*offset);
    }
}
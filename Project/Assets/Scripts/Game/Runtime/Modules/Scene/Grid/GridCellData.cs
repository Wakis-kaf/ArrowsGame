// GridCellData.cs
using System;
using UnityEngine;
using static Game.Modules.GModuleScene.GridTile;

namespace Game.Modules.GModuleScene
{
    [Serializable]
    public class GridCellData
    {
        public Color color = Color.white;
        public Vector3Int coordinate;  // 格子坐标 (axial 或 rc)
        public string customData = "";

        // 自定义数据 (可以是 JSON 字符串) 格子颜色
        public bool isBlocked = false;

        public string label = "";      // 格子标签

        // 是否阻挡
        public int weight = 1;         // 路径权重

        // 用于区分不同布局的坐标显示
        public string GetCoordinateString(CellLayout layout)
        {
            return layout == CellLayout.Hexagon
                ? $"({coordinate.x}, {coordinate.z})"
                : $"({coordinate.x}, {coordinate.y})";
        }
    }
}
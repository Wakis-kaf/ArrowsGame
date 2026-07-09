using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using static Game.Modules.GModuleScene.GridTile;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Game.Modules.GModuleScene
{
    /// <summary>
    /// 六边形网格系统（PointyTop/FlatTop） 关键修复：在 YZX / ZXY 等 Swizzle 下，网格与文本使用相同矩阵，预览完全一致
    /// </summary>
    [ExecuteAlways]
    public class GridTile : MonoBehaviour
    {
        [Header("布局类型")]
        [SerializeField] private CellLayout m_CellLayout = CellLayout.Hexagon;

        public CellLayout GridCellLayout { get => m_CellLayout; set => m_CellLayout = value; }

        [SerializeField] private Vector3 m_CellSize = new Vector3(1f, 1f, 0f);
        [SerializeField] private Vector3 m_Gap = new Vector3(0.1f, 0.1f, 0f);

        [SerializeField] private CellSwizzle m_CellSwizzle = CellSwizzle.XYZ;

        [ShowIf("@this.m_CellLayout==CellLayout.Hexagon")]
        [Header("六边形设置")]
        [SerializeField] private int m_GridRadius = 5;

        [ShowIf("@this.m_CellLayout==CellLayout.Hexagon")]
        [SerializeField] private HexOrientation m_Orientation = HexOrientation.PointyTop;

        [ShowIf("@this.m_CellLayout!=CellLayout.Hexagon")]
        [Header("矩形/等边 格子设置")]
        [SerializeField] private Vector3Int m_CellCount = new Vector3Int(10, 10, 1); // x=列数(横向), y=行数(纵向)

        [Header("预览设置")]
        [SerializeField] private bool m_ShowCoordinates = true;

        [SerializeField] private bool m_ShowGrid = true;
        [SerializeField] private Color m_GridColor = new Color(0.5f, 0.8f, 1f, 0.6f);
        public CellSwizzle GridCellSwizzle { get => m_CellSwizzle; set => m_CellSwizzle = value; }
        public Vector3Int GridCellCount { get => m_CellCount; set => m_CellCount = value; }
        public HexOrientation GridOrientation { get => m_Orientation; set => m_Orientation = value; }
        public bool ShowCoordinates { get => m_ShowCoordinates; set => m_ShowCoordinates = value; }
        public bool ShowGrid { get => m_ShowGrid; set => m_ShowGrid = value; }

        public enum HexOrientation
        {
            PointyTop,
            FlatTop
        }

        public enum CellSwizzle
        {
            XYZ, XZY, YXZ, YZX, ZXY, ZYX,
            XYnZ, XnYZ, nXYZ
        }

        public enum CellLayout
        {
            Rectangle,
            Isometric,
            Hexagon
        }

        public Vector3 cellSize { get => m_CellSize; set => m_CellSize = value; }
        public Vector3 gap { get => m_Gap; set => m_Gap = value; }
        public HexOrientation orientation { get => m_Orientation; set => m_Orientation = value; }
        public CellSwizzle cellSwizzle { get => m_CellSwizzle; set => m_CellSwizzle = value; }
        public int gridRadius { get => m_GridRadius; set => m_GridRadius = Mathf.Max(0, value); }

        #region Swizzle

        public Vector3 Swizzle(Vector3 position) => ApplySwizzle(position, m_CellSwizzle);

        public Vector3Int Swizzle(Vector3Int position) => Vector3Int.RoundToInt(ApplySwizzle((Vector3)position, m_CellSwizzle));

        public Vector3 Unswizzle(Vector3 position) => ReverseSwizzle(position, m_CellSwizzle);

        public Vector3Int Unswizzle(Vector3Int position) => Vector3Int.RoundToInt(ReverseSwizzle((Vector3)position, m_CellSwizzle));

        public static Vector3 ApplySwizzle(Vector3 position, CellSwizzle swizzle)
        {
            float x = position.x, y = position.y, z = position.z;

            switch (swizzle)
            {
                case CellSwizzle.XYZ: return new Vector3(x, y, z);
                case CellSwizzle.XZY: return new Vector3(x, z, y);
                case CellSwizzle.YXZ: return new Vector3(y, x, z);
                case CellSwizzle.YZX: return new Vector3(y, z, x);
                case CellSwizzle.ZXY: return new Vector3(z, x, y);
                case CellSwizzle.ZYX: return new Vector3(z, y, x);
                case CellSwizzle.XYnZ: return new Vector3(x, y, -z);
                case CellSwizzle.XnYZ: return new Vector3(x, -y, z);
                case CellSwizzle.nXYZ: return new Vector3(-x, y, z);
                default: return position;
            }
        }

        public static Vector3 ReverseSwizzle(Vector3 position, CellSwizzle swizzle)
        {
            float x = position.x, y = position.y, z = position.z;

            switch (swizzle)
            {
                case CellSwizzle.XYZ: return new Vector3(x, y, z);
                case CellSwizzle.XZY: return new Vector3(x, z, y);
                case CellSwizzle.YXZ: return new Vector3(y, x, z);
                case CellSwizzle.YZX: return new Vector3(z, x, y);  // 反向
                case CellSwizzle.ZXY: return new Vector3(y, z, x);  // 反向
                case CellSwizzle.ZYX: return new Vector3(z, y, x);
                case CellSwizzle.XYnZ: return new Vector3(x, y, -z);
                case CellSwizzle.XnYZ: return new Vector3(x, -y, z);
                case CellSwizzle.nXYZ: return new Vector3(-x, y, z);
                default: return position;
            }
        }

        private Matrix4x4 GetSwizzleMatrix()
        {
            // 只做轴与符号的线性重排，统一用作 Handles/Gizmos 的矩阵
            switch (m_CellSwizzle)
            {
                case CellSwizzle.XYZ: return Matrix4x4.identity;
                case CellSwizzle.XZY:
                    return new Matrix4x4(
                    new Vector4(1, 0, 0, 0),
                    new Vector4(0, 0, 1, 0),
                    new Vector4(0, 1, 0, 0),
                    new Vector4(0, 0, 0, 1));

                case CellSwizzle.YXZ:
                    return new Matrix4x4(
                    new Vector4(0, 1, 0, 0),
                    new Vector4(1, 0, 0, 0),
                    new Vector4(0, 0, 1, 0),
                    new Vector4(0, 0, 0, 1));

                case CellSwizzle.YZX:
                    return new Matrix4x4(
                    new Vector4(0, 1, 0, 0),
                    new Vector4(0, 0, 1, 0),
                    new Vector4(1, 0, 0, 0),
                    new Vector4(0, 0, 0, 1));

                case CellSwizzle.ZXY:
                    return new Matrix4x4(
                    new Vector4(0, 0, 1, 0),
                    new Vector4(1, 0, 0, 0),
                    new Vector4(0, 1, 0, 0),
                    new Vector4(0, 0, 0, 1));

                case CellSwizzle.ZYX:
                    return new Matrix4x4(
                    new Vector4(0, 0, 1, 0),
                    new Vector4(0, 1, 0, 0),
                    new Vector4(1, 0, 0, 0),
                    new Vector4(0, 0, 0, 1));

                case CellSwizzle.XYnZ:
                    return new Matrix4x4(
                    new Vector4(1, 0, 0, 0),
                    new Vector4(0, 1, 0, 0),
                    new Vector4(0, 0, -1, 0),
                    new Vector4(0, 0, 0, 1));

                case CellSwizzle.XnYZ:
                    return new Matrix4x4(
                    new Vector4(1, 0, 0, 0),
                    new Vector4(0, -1, 0, 0),
                    new Vector4(0, 0, 1, 0),
                    new Vector4(0, 0, 0, 1));

                case CellSwizzle.nXYZ:
                    return new Matrix4x4(
                    new Vector4(-1, 0, 0, 0),
                    new Vector4(0, 1, 0, 0),
                    new Vector4(0, 0, 1, 0),
                    new Vector4(0, 0, 0, 1));

                default: return Matrix4x4.identity;
            }
        }

        private Matrix4x4 GetWorldSwizzleMatrix()
        {
            return transform.localToWorldMatrix * GetSwizzleMatrix();
        }

        #endregion Swizzle

        #region 核心坐标转换方法（保持不变逻辑，但注意这是“局部空间未加矩阵”的计算）

        public Vector3Int WorldToCell(Vector3 worldPosition)
        {
            Vector3 localPos = transform.InverseTransformPoint(worldPosition);
            localPos = ApplySwizzle(localPos, m_CellSwizzle);

            return m_Orientation == HexOrientation.PointyTop ?
                WorldToCellPointyTop(localPos) : WorldToCellFlatTop(localPos);
        }

        public Vector3 CellToWorld(Vector3Int cellPosition)
        {
            // 这里返回世界坐标（保持兼容），内部先得出“局部未矩阵”的中心，再反向 swizzle + TransformPoint
            Vector3 localPos = m_Orientation == HexOrientation.PointyTop ?
                CellToWorldPointyTop(cellPosition) : CellToWorldFlatTop(cellPosition);

            localPos = ReverseSwizzle(localPos, m_CellSwizzle);
            return transform.TransformPoint(localPos);
        }

        public Vector3 GetCellCenterWorld(Vector3Int cellPosition) => CellToWorld(cellPosition);

        private Vector3Int WorldToCellPointyTop(Vector3 localPosition)
        {
            float sizeX = m_CellSize.x + m_Gap.x;
            float sizeY = m_CellSize.y + m_Gap.y;

            float q = (localPosition.x * Mathf.Sqrt(3f) / 3f - localPosition.y / 3f) / (sizeX * 0.5f);
            float r = localPosition.y * 2f / 3f / (sizeY * 0.5f);

            return CubeToAxial(CubeRound(new Vector3(q, r, -q - r)));
        }

        public Vector3 CellToWorldPointyTop(Vector3Int cellPosition)
        {
            // 注意：函数名沿用，但这里返回的是“局部空间坐标（未乘矩阵、未反 swizzle）”
            Vector3 cube = AxialToCube(cellPosition);
            float sizeX = m_CellSize.x + m_Gap.x;
            float sizeY = m_CellSize.y + m_Gap.y;

            float x = sizeX * (Mathf.Sqrt(3f) * cube.x + Mathf.Sqrt(3f) / 2f * cube.z);
            float y = sizeY * (3f / 2f * cube.z);

            return new Vector3(x * 0.5f, y * 0.5f, 0);
            // 返回局部中心（未加矩阵，未反 swizzle）
        }

        private Vector3Int WorldToCellFlatTop(Vector3 localPosition)
        {
            float sizeX = m_CellSize.x + m_Gap.x;
            float sizeY = m_CellSize.y + m_Gap.y;

            float q = localPosition.x * 2f / 3f / (sizeX * 0.5f);
            float r = (-localPosition.x / 3f + Mathf.Sqrt(3f) / 3f * localPosition.y) / (sizeY * 0.5f);

            return CubeToAxial(CubeRound(new Vector3(q, r, -q - r)));
        }

        public Vector3 CellToWorldFlatTop(Vector3Int cellPosition)
        {
            Vector3 cube = AxialToCube(cellPosition);
            float sizeX = m_CellSize.x + m_Gap.x;
            float sizeY = m_CellSize.y + m_Gap.y;

            float x = sizeX * (3f / 2f * cube.x);
            float y = sizeY * (Mathf.Sqrt(3f) / 2f * cube.x + Mathf.Sqrt(3f) * cube.z);

            return new Vector3(x * 0.5f, y * 0.5f, 0);
            // 返回局部中心（未加矩阵，未反 swizzle）
        }

        #endregion 核心坐标转换方法（保持不变逻辑，但注意这是“局部空间未加矩阵”的计算）

        #region 六边形坐标数学

        private Vector3 AxialToCube(Vector3Int axial) => new Vector3(axial.x, axial.z, -axial.x - axial.z);

        private Vector3Int CubeToAxial(Vector3 cube) => new Vector3Int(Mathf.RoundToInt(cube.x), 0, Mathf.RoundToInt(cube.z));

        private Vector3 CubeRound(Vector3 cube)
        {
            float rx = Mathf.Round(cube.x);
            float ry = Mathf.Round(cube.y);
            float rz = Mathf.Round(cube.z);

            float xDiff = Mathf.Abs(rx - cube.x);
            float yDiff = Mathf.Abs(ry - cube.y);
            float zDiff = Mathf.Abs(rz - cube.z);

            if (xDiff > yDiff && xDiff > zDiff)
                rx = -ry - rz;
            else if (yDiff > zDiff)
                ry = -rx - rz;
            else
                rz = -rx - ry;

            return new Vector3(rx, ry, rz);
        }

        public List<Vector3Int> GetCellsInRadius(Vector3Int center, int radius)
        {
            var results = new List<Vector3Int>();
            for (int q = -radius; q <= radius; q++)
            {
                for (int r = -radius; r <= radius; r++)
                {
                    if (Mathf.Abs(q + r) <= radius)
                        results.Add(new Vector3Int(center.x + q, 0, center.z + r));
                }
            }
            return results;
        }

        public int GetDistance(Vector3Int a, Vector3Int b)
        {
            Vector3 cubeA = AxialToCube(a);
            Vector3 cubeB = AxialToCube(b);
            return Mathf.RoundToInt((Mathf.Abs(cubeA.x - cubeB.x) + Mathf.Abs(cubeA.y - cubeB.y) + Mathf.Abs(cubeA.z - cubeB.z)) / 2f);
        }

        #endregion 六边形坐标数学

        #region 编辑器预览（网格与文本使用同一矩阵）

        private void OnDrawGizmos()
        {
            if (!m_ShowGrid) return;
            DrawHexagonGrid();
        }

        private void OnDrawGizmosSelected()
        {
            if (!m_ShowGrid) return;
            DrawHexagonGrid();
        }

        private void DrawHexagonGrid()
        {
#if UNITY_EDITOR
            // 保存状态
            Color prevGizColor = Gizmos.color;
            Matrix4x4 prevGizMat = Gizmos.matrix;
            Matrix4x4 prevHandlesMat = Handles.matrix;
            Color prevHandlesColor = Handles.color;

            // 统一矩阵（关键）
            Matrix4x4 W = GetWorldSwizzleMatrix();
            Gizmos.matrix = W;
            Handles.matrix = W;

            // 根据布局切换渲染
            if (m_CellLayout == CellLayout.Hexagon)
            {
                var cells = GetCellsInRadius(Vector3Int.zero, m_GridRadius);
                Gizmos.color = m_GridColor;
                foreach (var cell in cells)
                {
                    DrawHexagonLocal(cell);
                }
                if (m_ShowCoordinates)
                {
                    Handles.color = m_GridColor;
                    DrawCellCoordinatesLocal(cells);
                }
            }
            else if (m_CellLayout == CellLayout.Rectangle)
            {
                DrawRectangleGridLocal();
            }
            else if (m_CellLayout == CellLayout.Isometric)
            {
                DrawIsometricGridLocal();
            }

            // 恢复状态
            Gizmos.matrix = prevGizMat;
            Gizmos.color = prevGizColor;
            Handles.matrix = prevHandlesMat;
            Handles.color = prevHandlesColor;
#else
        // 非编辑器环境仅画线
        Gizmos.color = m_GridColor;
        Matrix4x4 prev = Gizmos.matrix;
        Gizmos.matrix = GetWorldSwizzleMatrix();
        if (m_CellLayout == CellLayout.Hexagon)
        {
            var cells = GetCellsInRadius(Vector3Int.zero, m_GridRadius);
            foreach (var cell in cells) DrawHexagonLocal(cell);
        }
        else if (m_CellLayout == CellLayout.Rectangle)
        {
            DrawRectangleGridLocal();
        }
        else if (m_CellLayout == CellLayout.Isometric)
        {
            DrawIsometricGridLocal();
        }
        Gizmos.matrix = prev;
#endif
        }

        private void DrawHexagonLocal(Vector3Int cellPosition)
        {
            Vector3[] vertices = GetHexagonVerticesLocal(cellPosition);
            for (int i = 0; i < 6; i++)
            {
                Gizmos.DrawLine(vertices[i], vertices[(i + 1) % 6]);
            }
        }

        public Vector3[] GetHexagonVerticesLocal(Vector3Int cellPosition)
        {
            // 局部中心（未乘矩阵、未反/正向 swizzle）
            Vector3 localCenter = m_Orientation == HexOrientation.PointyTop ?
                CellToWorldPointyTop(cellPosition) : CellToWorldFlatTop(cellPosition);

            // 以 cellSize 直接控制形状（pointyTop/flatTop 的角度偏移不同）
            Vector3[] localVertices = new Vector3[6];
            float angleOffset = m_Orientation == HexOrientation.PointyTop ? 30f : 0f;

            for (int i = 0; i < 6; i++)
            {
                float angle = 60f * i + angleOffset;
                float rad = angle * Mathf.Deg2Rad;

                float hexWidth = m_CellSize.x;
                float hexHeight = m_CellSize.y;

                localVertices[i] = localCenter + new Vector3(
                    Mathf.Cos(rad) * hexWidth * 0.5f,
                    Mathf.Sin(rad) * hexHeight * 0.5f,
                    0f
                );
            }

            return localVertices;
        }

        // ================= Rectangle =================
        private void DrawRectangleGridLocal()
        {
            float dx = m_CellSize.x + m_Gap.x;
            float dy = m_CellSize.y + m_Gap.y;

            Gizmos.color = m_GridColor;
#if UNITY_EDITOR
            if (m_ShowCoordinates) Handles.color = m_GridColor;
#endif

            int cols = Mathf.Max(0, m_CellCount.x);
            int rows = Mathf.Max(0, m_CellCount.y);
            float ox = (cols - 1) * 0.5f;
            float oy = (rows - 1) * 0.5f;

            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    Vector3 center = new Vector3((x - ox) * dx, (y - oy) * dy, 0f);
                    DrawRectLocal(center, m_CellSize.x, m_CellSize.y);
#if UNITY_EDITOR
                    if (m_ShowCoordinates)
                    {
                        GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
                        style.alignment = TextAnchor.MiddleCenter;
                        style.normal.textColor = m_GridColor;
                        Handles.Label(center, $"({x},{y})", style);
                    }
#endif
                }
            }
        }

        private void DrawRectLocal(Vector3 c, float w, float h)
        {
            Vector3 hw = new Vector3(w, 0f, 0f) * 0.5f;
            Vector3 hh = new Vector3(0f, h, 0f) * 0.5f;
            Gizmos.DrawLine(c - hw - hh, c + hw - hh);
            Gizmos.DrawLine(c + hw - hh, c + hw + hh);
            Gizmos.DrawLine(c + hw + hh, c - hw + hh);
            Gizmos.DrawLine(c - hw + hh, c - hw - hh);
        }

        // ================= Isometric (diamond) =================
        private void DrawIsometricGridLocal()
        {
            // 使用等边正方形（菱形），对角与 cellSize 控制，gap 只影响间隔
            float sx = m_CellSize.x;
            float sy = m_CellSize.y;
            float dx = (sx + m_Gap.x) * 0.5f;
            float dy = (sy + m_Gap.y) * 0.5f;

            Gizmos.color = m_GridColor;
#if UNITY_EDITOR
            if (m_ShowCoordinates) Handles.color = m_GridColor;
#endif

            int cols = Mathf.Max(0, m_CellCount.x);
            int rows = Mathf.Max(0, m_CellCount.y);
            float ox = (cols - 1) * 0.5f;
            float oy = (rows - 1) * 0.5f;

            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    // 将规则网格(中心化)映射到等距：u=(x-ox), v=(y-oy)
                    float u = (x - ox);
                    float v = (y - oy);
                    Vector3 center = new Vector3((u - v) * dx, (u + v) * dy, 0f);
                    DrawDiamondLocal(center, sx, sy);
#if UNITY_EDITOR
                    if (m_ShowCoordinates)
                    {
                        GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
                        style.alignment = TextAnchor.MiddleCenter;
                        style.normal.textColor = m_GridColor;
                        Handles.Label(center, $"({x},{y})", style);
                    }
#endif
                }
            }
        }

        private void DrawDiamondLocal(Vector3 c, float w, float h)
        {
            Vector3 d1 = new Vector3(w * 0.5f, 0f, 0f);
            Vector3 d2 = new Vector3(0f, h * 0.5f, 0f);
            Gizmos.DrawLine(c + d1, c + d2);
            Gizmos.DrawLine(c + d2, c - d1);
            Gizmos.DrawLine(c - d1, c - d2);
            Gizmos.DrawLine(c - d2, c + d1);
        }

        private void DrawCellCoordinatesLocal(List<Vector3Int> cells)
        {
#if UNITY_EDITOR
            foreach (var cell in cells)
            {
                // 与网格完全同路径：局部中心点，直接 Handles.Label（Handles.matrix 已与 Gizmos.matrix 一致）
                Vector3 localCenter = m_Orientation == HexOrientation.PointyTop ?
                    CellToWorldPointyTop(cell) : CellToWorldFlatTop(cell);

                string coordText = $"({cell.x},{cell.z})";

                GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = m_GridColor;

                Handles.Label(localCenter, coordText, style);
            }
#endif
        }

        #endregion 编辑器预览（网格与文本使用同一矩阵）

        #region 实用方法

        public Vector3Int[] GetNeighbors(Vector3Int cell)
        {
            Vector3Int[] directions = m_Orientation == HexOrientation.PointyTop ?
                new Vector3Int[] {
                new Vector3Int(1, 0, 0), new Vector3Int(1, 0, -1), new Vector3Int(0, 0, -1),
                new Vector3Int(-1, 0, 0), new Vector3Int(-1, 0, 1), new Vector3Int(0, 0, 1)
                } :
                new Vector3Int[] {
                new Vector3Int(1, 0, 0), new Vector3Int(0, 0, -1), new Vector3Int(-1, 0, -1),
                new Vector3Int(-1, 0, 0), new Vector3Int(0, 0, 1), new Vector3Int(1, 0, 1)
                };

            var neighbors = new Vector3Int[6];
            for (int i = 0; i < 6; i++)
            {
                neighbors[i] = cell + directions[i];
            }
            return neighbors;
        }

        public bool IsInGrid(Vector3Int cell, int radius) => GetDistance(Vector3Int.zero, cell) <= radius;

        #endregion 实用方法

        #region GridMap API

        /// <summary>
        /// 根据当前设置构建一份只读的 GirdMap（与Scene预览严格一致）
        /// </summary>
        public GridTileMap BuildGridMap()
        {
            Matrix4x4 W = GetWorldSwizzleMatrix();
            var map = new GridTileMap
            {
                layout = m_CellLayout,
                orientation = m_Orientation,
                swizzle = m_CellSwizzle,
                cellSize = m_CellSize,
                gap = m_Gap,
                gridRadius = m_GridRadius,
                cellCount = m_CellCount
            };

            int index = 0;
            if (m_CellLayout == CellLayout.Hexagon)
            {
                var cells = GetCellsInRadius(Vector3Int.zero, m_GridRadius);
                for (int i = 0; i < cells.Count; i++)
                {
                    var axial = new Vector2Int(cells[i].x, cells[i].z);
                    Vector3 localCenter = (m_Orientation == HexOrientation.PointyTop) ? CellToWorldPointyTop(cells[i]) : CellToWorldFlatTop(cells[i]);
                    Vector3 worldCenter = W.MultiplyPoint3x4(localCenter);
                    var cell = new GridCell
                    {
                        index = index++,
                        layout = m_CellLayout,
                        orientation = m_Orientation,
                        swizzle = m_CellSwizzle,
                        axial = axial,
                        cube = AxialToCube(cells[i]),
                        rc = new Vector2Int(-1, -1),
                        localCenter = localCenter,
                        worldCenter = worldCenter
                    };
                    map.cells.Add(cell);
                }
            }
            else if (m_CellLayout == CellLayout.Rectangle)
            {
                float dx = m_CellSize.x + m_Gap.x;
                float dy = m_CellSize.y + m_Gap.y;
                int cols = Mathf.Max(0, m_CellCount.x);
                int rows = Mathf.Max(0, m_CellCount.y);
                float ox = (cols - 1) * 0.5f;
                float oy = (rows - 1) * 0.5f;
                for (int x = 0; x < cols; x++)
                {
                    for (int y = 0; y < rows; y++)
                    {
                        Vector3 localCenter = new Vector3((x - ox) * dx, (y - oy) * dy, 0f);
                        Vector3 worldCenter = W.MultiplyPoint3x4(localCenter);
                        map.cells.Add(new GridCell
                        {
                            index = index++,
                            layout = m_CellLayout,
                            orientation = m_Orientation,
                            swizzle = m_CellSwizzle,
                            axial = new Vector2Int(-1, -1),
                            cube = Vector3.zero,
                            rc = new Vector2Int(x, y),
                            localCenter = localCenter,
                            worldCenter = worldCenter
                        });
                    }
                }
            }
            else // Isometric
            {
                float sx = m_CellSize.x; float sy = m_CellSize.y;
                float dx = (sx + m_Gap.x) * 0.5f; float dy = (sy + m_Gap.y) * 0.5f;
                int cols = Mathf.Max(0, m_CellCount.x);
                int rows = Mathf.Max(0, m_CellCount.y);
                float ox = (cols - 1) * 0.5f; float oy = (rows - 1) * 0.5f;
                for (int x = 0; x < cols; x++)
                {
                    for (int y = 0; y < rows; y++)
                    {
                        float u = (x - ox); float v = (y - oy);
                        Vector3 localCenter = new Vector3((u - v) * dx, (u + v) * dy, 0f);
                        Vector3 worldCenter = W.MultiplyPoint3x4(localCenter);
                        map.cells.Add(new GridCell
                        {
                            index = index++,
                            layout = m_CellLayout,
                            orientation = m_Orientation,
                            swizzle = m_CellSwizzle,
                            axial = new Vector2Int(-1, -1),
                            cube = Vector3.zero,
                            rc = new Vector2Int(x, y),
                            localCenter = localCenter,
                            worldCenter = worldCenter
                        });
                    }
                }
            }

            map.BuildIndices();
            map.gridTile = this;
            return map;
        }

        #endregion GridMap API
    }

    [Serializable]
    public class GridCell
    {
        public int index;                   // 连续下标
        public CellLayout layout;           // 所属渲染布局
        public HexOrientation orientation;  // Hex 取值
        public CellSwizzle swizzle;         // 当前 Swizzle
        public Vector2Int axial;            // Hex: (q,r) 轴向坐标；非 Hex 时为(-1,-1)
        public Vector3 cube;                // Hex: cube 坐标；非 Hex 时为(Vector3.zero)
        public Vector2Int rc;               // Rect/Iso: 行列索引；Hex 时为(-1,-1)
        public Vector3 localCenter;         // 预览使用的局部中心点（未单点 Swizzle；通过矩阵统一变换）
        public Vector3 worldCenter;         // 世界中心点（已包含 Transform 与 Swizzle 矩阵）

        public Vector2Int GetIndex()
        {
            if (layout == GridTile.CellLayout.Hexagon) return axial;
            else return rc;
        }

        public bool IsRcDiagonal(GridCell cell)
        {
            if (cell.layout != CellLayout.Rectangle || layout != CellLayout.Rectangle) return false;
            return Mathf.Abs(rc.x - cell.rc.x) + Mathf.Abs(rc.y - cell.rc.y) > 1;
        }
    }

    [Serializable]
    public class GridTileMap
    {
        public CellLayout layout;
        public HexOrientation orientation;
        public CellSwizzle swizzle;
        public Vector3 cellSize;
        public Vector3 gap;
        public int gridRadius;              // Hex 使用
        public Vector3Int cellCount;        // Rect/Iso 使用
        public List<GridCell> cells = new List<GridCell>();

        // 快速索引
        private Dictionary<Vector2Int, int> axialToIndex; // Hex (q,r) -> index

        private Dictionary<Vector2Int, int> rcToIndex;    // Rect/Iso (x,y) -> index

        public void BuildIndices()
        {
            if (layout == CellLayout.Hexagon)
            {
                axialToIndex = new Dictionary<Vector2Int, int>(cells.Count);
                for (int i = 0; i < cells.Count; i++) axialToIndex[cells[i].axial] = i;
            }
            else
            {
                rcToIndex = new Dictionary<Vector2Int, int>(cells.Count);
                for (int i = 0; i < cells.Count; i++) rcToIndex[cells[i].rc] = i;
            }
        }

        public int Count => cells.Count;

        public bool TryGetByIndex(int index, out GridCell cell)
        {
            if (index >= 0 && index < cells.Count) { cell = cells[index]; return true; }
            cell = null; return false;
        }

        public bool TryGetByAxial(int q, int r, out GridCell cell)
        {
            cell = null;
            if (layout != CellLayout.Hexagon || axialToIndex == null) return false;
            int i; if (!axialToIndex.TryGetValue(new Vector2Int(q, r), out i)) return false;
            cell = cells[i]; return true;
        }

        public bool TryGetByRC(int x, int y, out GridCell cell)
        {
            cell = null;
            if (layout == CellLayout.Hexagon || rcToIndex == null) return false;
            int i; if (!rcToIndex.TryGetValue(new Vector2Int(x, y), out i)) return false;
            cell = cells[i]; return true;
        }

        private Vector2Int[] hexPointTopNeighborIndex = new[] {
            new Vector2Int(1, 0),
            new Vector2Int(1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, 1),
            new Vector2Int(0, 1)
        };

        private Vector2Int[] hexFlatTopNeighborIndex = new[] {
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1)
        };

        private Vector2Int[] IsometricNeighborIndex = new[] {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1) };

        private Vector2Int[] RectNeighborIndex = new[] {
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1,0),
            new Vector2Int(-1,-1),
            new Vector2Int(0,-1),
            new Vector2Int(1,-1)};

        public GridTile gridTile;

        public List<GridCell> GetNeighbors(GridCell cell,ref List <GridCell> list)
        {
            
            if (layout == CellLayout.Hexagon)
            {
                Vector2Int[] dirs = orientation == HexOrientation.PointyTop
                    ? hexPointTopNeighborIndex
                    : hexFlatTopNeighborIndex;
                for (int i = 0; i < dirs.Length; i++)
                {
                    Vector2Int k = new Vector2Int(cell.axial.x + dirs[i].x, cell.axial.y + dirs[i].y);
                    int idx;
                    if (axialToIndex != null && axialToIndex.TryGetValue(k, out idx))
                    {
                        list.Add(cells[idx]);
                    }
                }
                return list;
            }
            else if (layout == CellLayout.Isometric)
            {
                Vector2Int[] dirs = IsometricNeighborIndex;
                for (int i = 0; i < dirs.Length; i++)
                {
                    Vector2Int k = new Vector2Int(cell.rc.x + dirs[i].x, cell.rc.y + dirs[i].y);
                    int idx;
                    if (rcToIndex != null && rcToIndex.TryGetValue(k, out idx))
                    {
                        list.Add(cells[idx]);
                    }
                }
                return list;
            }
            else
            {
                Vector2Int[] dirs = RectNeighborIndex;
                for (int i = 0; i < dirs.Length; i++)
                {
                    Vector2Int k = new Vector2Int(cell.rc.x + dirs[i].x, cell.rc.y + dirs[i].y);
                    int idx;
                    if (rcToIndex != null && rcToIndex.TryGetValue(k, out idx))
                    {
                        list.Add(cells[idx]);
                    }
                }
                return list;
            }
         
        }
    }
}
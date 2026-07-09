using System.Collections.Generic;
using UnityEngine;

namespace Game.Modules.GModuleArrows
{
    public enum ShapeType
    {
        Circle,
        Triangle,
        Square,
        Pentagon,
        Hexagon,
        Star,
        Heart,
        Infinity,
    }

    [System.Serializable]
    public struct ShapeOptions
    {
        public float width;
        public float height;
        public float gridStep;

        public ShapeOptions(float width, float height, float gridStep = 1.0f)
        {
            this.width = width;
            this.height = height;
            this.gridStep = gridStep > 0 ? gridStep : 1.0f;
        }
    }

    public static class GamePointsPresetGenerator
    {
        private static Dictionary<ShapeType, System.Func<ShapeOptions, Dictionary<Vector3Int, Vector3>>> m_ShapeGenerators;

        static GamePointsPresetGenerator()
        {
            InitializeGenerators();
        }

        private static void InitializeGenerators()
        {
            m_ShapeGenerators = new Dictionary<ShapeType, System.Func<ShapeOptions, Dictionary<Vector3Int, Vector3>>>();

            m_ShapeGenerators[ShapeType.Circle] = GenerateCircle;
            m_ShapeGenerators[ShapeType.Triangle] = GenerateTriangle;
            m_ShapeGenerators[ShapeType.Square] = GenerateSquare;
            m_ShapeGenerators[ShapeType.Pentagon] = GeneratePentagon;
            m_ShapeGenerators[ShapeType.Hexagon] = GenerateHexagon;
            m_ShapeGenerators[ShapeType.Star] = GenerateStar;
            m_ShapeGenerators[ShapeType.Heart] = GenerateHeart;
            m_ShapeGenerators[ShapeType.Infinity] = GenerateInfinity;
        }

        public static Dictionary<Vector3Int, Vector3> GetShapePoints(ShapeType shapeType, ShapeOptions options)
        {
            if (m_ShapeGenerators.TryGetValue(shapeType, out var generator))
            {
                return generator(options);
            }
            return GenerateSquare(options);
        }

        private static Dictionary<Vector3Int, Vector3> GenerateSquare(ShapeOptions opt)
        {
            var points = new Dictionary<Vector3Int, Vector3>();
            int sizeX = Mathf.RoundToInt(opt.width);
            int sizeY = Mathf.RoundToInt(opt.height);
            int hx = sizeX / 2;
            int hy = sizeY / 2;

            for (int x = -hx; x <= (sizeX % 2 == 0 ? hx - 1 : hx); x++)
            {
                for (int y = -hy; y <= (sizeY % 2 == 0 ? hy - 1 : hy); y++)
                {
                    Vector3 pos = new Vector3(x * opt.gridStep, y * opt.gridStep, 0);
                    Vector3Int index = new Vector3Int(x, y, 0);
                    points[index] = pos;
                }
            }
            return points;
        }

        private static Dictionary<Vector3Int, Vector3> GenerateCircle(ShapeOptions opt)
        {
            var points = new Dictionary<Vector3Int, Vector3>();
            int sizeX = Mathf.RoundToInt(opt.width);
            int sizeY = Mathf.RoundToInt(opt.height);
            int hx = sizeX / 2;
            int hy = sizeY / 2;
            float rx = sizeX * 0.5f;
            float ry = sizeY * 0.5f;

            for (int x = -hx; x <= (sizeX % 2 == 0 ? hx - 1 : hx); x++)
            {
                for (int y = -hy; y <= (sizeY % 2 == 0 ? hy - 1 : hy); y++)
                {
                    float nx = x + (sizeX % 2 == 0 ? 0.5f : 0f);
                    float ny = y + (sizeY % 2 == 0 ? 0.5f : 0f);
                    if ((nx * nx) / (rx * rx) + (ny * ny) / (ry * ry) <= 1.001f)
                    {
                        Vector3 pos = new Vector3(x * opt.gridStep, y * opt.gridStep, 0);
                        Vector3Int index = new Vector3Int(x, y, 0);
                        points[index] = pos;
                    }
                }
            }
            return points;
        }

        private static Dictionary<Vector3Int, Vector3> GenerateTriangle(ShapeOptions opt)
        {
            var points = new Dictionary<Vector3Int, Vector3>();
            int sizeX = Mathf.RoundToInt(opt.width);
            int sizeY = Mathf.RoundToInt(opt.height);
            int hx = sizeX / 2;
            int hy = sizeY / 2;

            Vector3 v1 = new Vector3(0, hy * opt.gridStep, 0);
            Vector3 v2 = new Vector3(-hx * opt.gridStep, -hy * opt.gridStep, 0);
            Vector3 v3 = new Vector3(hx * opt.gridStep, -hy * opt.gridStep, 0);

            for (int x = -hx; x <= (sizeX % 2 == 0 ? hx - 1 : hx); x++)
            {
                for (int y = -hy; y <= (sizeY % 2 == 0 ? hy - 1 : hy); y++)
                {
                    Vector3 p = new Vector3(x * opt.gridStep, y * opt.gridStep, 0);
                    if (IsPointInTriangle(p, v1, v2, v3))
                    {
                        Vector3Int index = new Vector3Int(x, y, 0);
                        points[index] = p;
                    }
                }
            }
            return points;
        }

        private static Dictionary<Vector3Int, Vector3> GeneratePentagon(ShapeOptions opt)
        {
            return GeneratePolygon(opt, 5);
        }

        private static Dictionary<Vector3Int, Vector3> GenerateHexagon(ShapeOptions opt)
        {
            return GeneratePolygon(opt, 6);
        }

        private static Dictionary<Vector3Int, Vector3> GeneratePolygon(ShapeOptions opt, int sides)
        {
            var points = new Dictionary<Vector3Int, Vector3>();
            int sizeX = Mathf.RoundToInt(opt.width);
            int sizeY = Mathf.RoundToInt(opt.height);
            int hx = sizeX / 2;
            int hy = sizeY / 2;

            Vector3[] vertices = new Vector3[sides];
            float angleStep = 360f / sides;
            for (int i = 0; i < sides; i++)
            {
                float angle = (i * angleStep - 90) * Mathf.Deg2Rad;
                vertices[i] = new Vector3(Mathf.Cos(angle) * hx * opt.gridStep, Mathf.Sin(angle) * hy * opt.gridStep, 0);
            }

            for (int x = -hx; x <= (sizeX % 2 == 0 ? hx - 1 : hx); x++)
            {
                for (int y = -hy; y <= (sizeY % 2 == 0 ? hy - 1 : hy); y++)
                {
                    Vector3 p = new Vector3(x * opt.gridStep, y * opt.gridStep, 0);
                    if (IsPointInPolygon(p, vertices))
                    {
                        Vector3Int index = new Vector3Int(x, y, 0);
                        points[index] = p;
                    }
                }
            }
            return points;
        }

        private static Dictionary<Vector3Int, Vector3> GenerateStar(ShapeOptions opt)
        {
            var points = new Dictionary<Vector3Int, Vector3>();
            int sizeX = Mathf.RoundToInt(opt.width);
            int sizeY = Mathf.RoundToInt(opt.height);
            int hx = sizeX / 2;
            int hy = sizeY / 2;
            int totalPoints = 10;

            Vector3[] vertices = new Vector3[totalPoints];
            for (int i = 0; i < totalPoints; i++)
            {
                float angle = (i * (360f / totalPoints) - 90) * Mathf.Deg2Rad;
                float factor = (i % 2 == 0) ? 1.0f : 0.4f;
                vertices[i] = new Vector3(Mathf.Cos(angle) * hx * opt.gridStep * factor, Mathf.Sin(angle) * hy * opt.gridStep * factor, 0);
            }

            for (int x = -hx; x <= (sizeX % 2 == 0 ? hx - 1 : hx); x++)
            {
                for (int y = -hy; y <= (sizeY % 2 == 0 ? hy - 1 : hy); y++)
                {
                    Vector3 p = new Vector3(x * opt.gridStep, y * opt.gridStep, 0);
                    if (IsPointInPolygon(p, vertices))
                    {
                        Vector3Int index = new Vector3Int(x, y, 0);
                        points[index] = p;
                    }
                }
            }
            return points;
        }

        private static Dictionary<Vector3Int, Vector3> GenerateHeart(ShapeOptions opt)
        {
            var points = new Dictionary<Vector3Int, Vector3>();
            int sizeX = Mathf.RoundToInt(opt.width);
            int sizeY = Mathf.RoundToInt(opt.height);
            int hx = sizeX / 2;
            int hy = sizeY / 2;

            float scaleX = sizeX / 34f;
            float scaleY = sizeY / 30f;

            for (int x = -hx; x <= (sizeX % 2 == 0 ? hx - 1 : hx); x++)
            {
                for (int y = -hy; y <= (sizeY % 2 == 0 ? hy - 1 : hy); y++)
                {
                    float nx = x / (scaleX > 0 ? scaleX : 1f);
                    float ny = (y - sizeY * 0.05f) / (scaleY > 0 ? scaleY : 1f);

                    float fX = nx * 0.06f;
                    float fY = ny * 0.06f;
                    float term = fX * fX + fY * fY - 0.6f;

                    if (term * term * term - fX * fX * fY * fY * fY <= 0)
                    {
                        Vector3 pos = new Vector3(x * opt.gridStep, y * opt.gridStep, 0);
                        Vector3Int index = new Vector3Int(x, y, 0);
                        points[index] = pos;
                    }
                }
            }
            return points;
        }

        private static Dictionary<Vector3Int, Vector3> GenerateInfinity(ShapeOptions opt)
        {
            var points = new Dictionary<Vector3Int, Vector3>();
            int sizeX = Mathf.RoundToInt(opt.width);
            int sizeY = Mathf.RoundToInt(opt.height);
            int hx = sizeX / 2;
            int hy = sizeY / 2;

            float a = hx;
            float aSq = a * a;

            for (int x = -hx; x <= (sizeX % 2 == 0 ? hx - 1 : hx); x++)
            {
                for (int y = -hy; y <= (sizeY % 2 == 0 ? hy - 1 : hy); y++)
                {
                    float ny = y * (hx / (hy > 0 ? hy : 1f));
                    float sumSq = x * x + ny * ny;
                    float left = sumSq * sumSq;
                    float right = 2 * aSq * (x * x - ny * ny);

                    if (left <= right)
                    {
                        Vector3 pos = new Vector3(x * opt.gridStep, y * opt.gridStep, 0);
                        Vector3Int index = new Vector3Int(x, y, 0);
                        points[index] = pos;
                    }
                }
            }
            return points;
        }

        private static bool IsPointInTriangle(Vector3 p, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            float Sign(Vector3 p1, Vector3 p2, Vector3 p3)
            {
                return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
            }

            float d1 = Sign(p, v1, v2);
            float d2 = Sign(p, v2, v3);
            float d3 = Sign(p, v3, v1);

            bool has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }

        private static bool IsPointInPolygon(Vector3 point, Vector3[] polygon)
        {
            bool isInside = false;
            for (int i = 0; i < polygon.Length; i++)
            {
                Vector3 v1 = polygon[i];
                Vector3 v2 = polygon[(i + 1) % polygon.Length];

                if (((v1.y > point.y) != (v2.y > point.y)) &&
                    (point.x < (v2.x - v1.x) * (point.y - v1.y) / (v2.y - v1.y) + v1.x))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }
    }
}
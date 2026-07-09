using UnityEngine;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
namespace Game.Modules.GModuleArrows
{
    public class LevelPointNode
    {
        public int id;
        public Vector3Int index;
        public Vector4 color;
        public Vector3 worldPosition;
        public bool isOccupied;
        public bool isOccupyRemoved;
        public Action OnUpdate;
        public void NotifiyUpdate()
        {
            OnUpdate?.Invoke();
        }

        public LevelPointNode(int id, Vector3Int index, Vector3 worldPosition, Vector4 color)
        {
            this.id = id;
            this.index = index;
            this.worldPosition = worldPosition;
            this.isOccupied = false;
            this.color = color;
        }
    }


    public class LevelPointLayout
    {
        // private Dictionary<int, LevelPointNode> nodesMap = new Dictionary<int, LevelPointNode>();
        private List<LevelPointNode> nodesList = new List<LevelPointNode>(500);
        private Dictionary<Vector3Int, LevelPointNode> m_IndexMap = new Dictionary<Vector3Int, LevelPointNode>(500);
        private int m_MinX = int.MaxValue;
        private int m_MaxX = int.MinValue;
        private int m_MinY = int.MaxValue;
        private int m_MaxY = int.MinValue;
        public int MinX => m_MinX;
        public int MaxX => m_MaxX;
        public int MinY => m_MinY;
        public int MaxY => m_MaxY;
        public float MinAreaY { get; private set; } = float.MaxValue;
        public float MaxAreaY { get; private set; } = float.MinValue;
        public float MinAreaX { get; private set; } = float.MaxValue;
        public float MaxAreaX { get; private set; } = float.MinValue;
        public void InitializeLayout(LevelPointPresets preset, Vector3 originPosition)
        {
            // nodesMap.Clear();
            nodesList.Clear();
            m_IndexMap.Clear();
            int id = 0;
            foreach (var item in preset.localPositions)
            {
                var index = item.Key;
                var pos = item.Value;
                id++;
                Vector3 worldPos = originPosition + pos;
                Vector4 color = Vector4.zero;
                if (preset != null && preset.colors.TryGetValue(index, out var findColor))
                {
                    color = findColor;
                }
                color = color * (1f / 255f);
                LevelPointNode node = new LevelPointNode(id, index, worldPos, color);

                // nodesMap.Add(id, node);
                nodesList.Add(node);
                m_IndexMap.Add(index, node);

                m_MinX = Mathf.Min(m_MinX, index.x);
                m_MaxX = Mathf.Max(m_MaxX, index.x);
                m_MinY = Mathf.Min(m_MinY, index.y);
                m_MaxY = Mathf.Max(m_MaxY, index.y);

                MinAreaY = Mathf.Min(MinAreaY, worldPos.y);
                MaxAreaY = Mathf.Max(MaxAreaY, worldPos.y);
                MinAreaX = Mathf.Min(MinAreaX, worldPos.x);
                MaxAreaX = Mathf.Max(MaxAreaX, worldPos.x);

            }
        }

        // public LevelPointNode GetNodeById(int id)
        // {
        //     if (nodesMap.TryGetValue(id, out LevelPointNode node))
        //     {
        //         return node;
        //     }
        //     return null;
        // }
        public LevelPointNode GetNodeByIndex(Vector3Int index)
        {
            if (m_IndexMap.TryGetValue(index, out LevelPointNode node))
            {
                return node;
            }
            // foreach (var node in nodesList)
            // {
            //     if (node.index == index)
            //     {
            //         return node;
            //     }
            // }
            return null;
        }

        public List<LevelPointNode> GetAllNodes() => nodesList;
        public List<LevelPointNode> GetBoardNodes()
        {
            if (nodesList.Count == 0) return new List<LevelPointNode>();

            HashSet<Vector3Int> nodeIndices = new HashSet<Vector3Int>();
            foreach (var n in nodesList)
            {
                nodeIndices.Add(n.index);
            }

            int minX = m_MinX - 1;
            int maxX = m_MaxX + 1;
            int minY = m_MinY - 1;
            int maxY = m_MaxY + 1;

            HashSet<Vector3Int> outsideEmpty = new HashSet<Vector3Int>();
            Queue<Vector3Int> queue = new Queue<Vector3Int>();
            Vector3Int[] directions = new Vector3Int[] { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

            for (int x = minX; x <= maxX; x++)
            {
                Vector3Int p1 = new Vector3Int(x, minY, 0);
                Vector3Int p2 = new Vector3Int(x, maxY, 0);
                if (!nodeIndices.Contains(p1) && outsideEmpty.Add(p1)) queue.Enqueue(p1);
                if (!nodeIndices.Contains(p2) && outsideEmpty.Add(p2)) queue.Enqueue(p2);
            }
            for (int y = minY; y <= maxY; y++)
            {
                Vector3Int p1 = new Vector3Int(minX, y, 0);
                Vector3Int p2 = new Vector3Int(maxX, y, 0);
                if (!nodeIndices.Contains(p1) && outsideEmpty.Add(p1)) queue.Enqueue(p1);
                if (!nodeIndices.Contains(p2) && outsideEmpty.Add(p2)) queue.Enqueue(p2);
            }

            while (queue.Count > 0)
            {
                Vector3Int curr = queue.Dequeue();
                foreach (var dir in directions)
                {
                    Vector3Int neighbor = curr + dir;
                    if (neighbor.x >= minX && neighbor.x <= maxX && neighbor.y >= minY && neighbor.y <= maxY)
                    {
                        if (!nodeIndices.Contains(neighbor) && outsideEmpty.Add(neighbor))
                        {
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            List<LevelPointNode> boardNodes = new List<LevelPointNode>();
            foreach (var node in nodesList)
            {
                foreach (var dir in directions)
                {
                    if (outsideEmpty.Contains(node.index + dir))
                    {
                        boardNodes.Add(node);
                        break;
                    }
                }
            }
            return boardNodes;
        }
        public List<LevelPointNode> GetAvailableNodes()
        {
            return nodesList.Where(n => !n.isOccupied).ToList();
        }
        public void SetAllNodesOccupied(bool occupied)
        {
            foreach (var node in nodesList)
            {
                node.isOccupied = occupied;
            }
        }
        public void SetAllNodesOccupyRemoved(bool occupied)
        {
            foreach (var node in nodesList)
            {
                node.isOccupyRemoved = occupied;
            }
        }

        public void SetNodesOccupied(List<Vector3Int> indexs, bool occupied)
        {
            foreach (var index in indexs)
            {
                var node = GetNodeByIndex(index);
                if (node != null)
                {
                    node.isOccupied = occupied;
                }

            }
        }
        public void SetNodesOccupied(Vector3Int index, bool occupied)
        {
            var node = GetNodeByIndex(index);
            if (node != null)
            {
                node.isOccupied = occupied;
                node.NotifiyUpdate();

            }
        }

        public LevelPointNode GetNearPoint(Vector3Int index, Vector3 direction)
        {
            Vector3Int step = Vector3Int.RoundToInt(direction);
            Vector3Int nearIndex = index + step;
            return GetNodeByIndex(nearIndex);

        }

        public int GetMaxShapeLength()
        {
            return Mathf.Max(m_MaxX - MinX + 1, m_MaxY - m_MinY + 1);
        }

        public void SetArrowOccupyRemoved(List<Vector3Int> occupiedPointIndexs)
        {
            foreach (var index in occupiedPointIndexs)
            {
                var node = GetNodeByIndex(index);
                if (node == null) continue;
                node.isOccupyRemoved = true;
                node.NotifiyUpdate();
            }
        }

        public void SetNodesOccupyRemoved(Vector3Int index, bool isOccupyRemoved)
        {
            var node = GetNodeByIndex(index);
            if (node != null)
            {
                node.isOccupyRemoved = isOccupyRemoved && node.isOccupied;
                node.NotifiyUpdate();

            }
        }
    }
}

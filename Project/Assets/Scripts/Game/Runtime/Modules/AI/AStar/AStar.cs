using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
namespace Game.Modules.GModuleAI
{
    public enum EvaluationFunctionType
    {
        Euclidean,
        Manhattan,
        Diagonal,
    }

    public class AStar
    {
        public static float FACTOR = 10; // 两个格子之间的距离
        public static float FACTOR_DIAGONAL = 14; // 斜对角距离

        private EvaluationFunctionType m_evaluationFunctionType = EvaluationFunctionType.Manhattan;
        private List<AStarNode> m_openDic = new List<AStarNode>();
        private List<AStarNode> m_closeDic = new List<AStarNode>();
        private AStarNode m_destinationNode = null;
        private AStarNode m_startNode = null;
        private List<Vector3> m_pathPosList = new List<Vector3>();
        private AstarNodeManager m_astarNodeManager = new AstarNodeManager();
        public AstarNodeManager AstarNodeManager => m_astarNodeManager;
        private int m_curVersion = -1;
        private Dictionary<string, List<Vector3>> m_findCache = new Dictionary<string, List<Vector3>>();
        private List<Dictionary<string, float>> m_findCacheKey = new List<Dictionary<string, float>>();

        public AStar()
        {
            m_astarNodeManager = new AstarNodeManager();
        }

        public void ClearNodes()
        {
            m_astarNodeManager.Clear();
        }

        public void SetNodes(AStarNode[] nodes)
        {
            m_astarNodeManager.nodeArray = nodes;
        }


        private string GetCacheKey(AStarNode start, AStarNode destination)
        {
            return $"{start.posX}_{start.posY}_{start.posY}_{destination.posX}_{destination.posY}_{destination.posZ}";
        }
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public async void FindPathAsync(Vector3 start,
            Vector3 destination,
            Action<List<Vector3>> cb,
            bool readCache = true, EvaluationFunctionType type = EvaluationFunctionType.Manhattan)
        {
            // 等待信号量，确保只有一个任务在执行
            await _semaphore.WaitAsync();
            try
            {
                var path = await UniTask.RunOnThreadPool(() =>
                {
                    return FindPath(start, destination, readCache, type);
                });

                cb?.Invoke(path);
            }
            finally
            {
                _semaphore.Release();
            }
            //await UniTask.RunOnThreadPool(() =>
            //{
            //    cb?.Invoke(FindPath(start, destination, readCache, type));
            //});
        }
        public List<Vector3> FindPath(Vector3 start,
            Vector3 destination,
            bool readCache = true, EvaluationFunctionType type = EvaluationFunctionType.Manhattan)
        {
            m_curVersion++;
            m_openDic.Clear();
            m_closeDic.Clear();
            m_evaluationFunctionType = type;
            m_startNode = GetNodeByPosition(start.x, start.y, start.z);
            m_destinationNode = GetNodeByPosition(destination.x, destination.y, destination.z);

            if (readCache)
            {
                string cacheKey = GetCacheKey(m_startNode, m_destinationNode);
                if (m_findCache.ContainsKey(cacheKey))
                {
                    return m_findCache[cacheKey];
                }
            }
            AddNodeToOpenQueue(m_startNode);

            List<Vector3> res = new List<Vector3>() {  };
            if (StartSearch())
            {
                BuildPath();
                res.Add(start);
                res.AddRange(m_pathPosList);
                res.Add(destination);
            }
            string cacheKeyObj = GetCacheKey(m_startNode, m_destinationNode);
            m_findCache[cacheKeyObj] = res;
            return res;
        }

        public AStarNode GetNodeByPosition(float x, float y, float z)
        {
            return m_astarNodeManager.FindNearestPoint(x, y, z);
        }

        private void AddNodeToOpenQueue(AStarNode node)
        {
            m_openDic.Add(node);
        }

        private void AddNodeToCloseQueue(AStarNode node)
        {
            node.version = m_curVersion;
        }

        private bool IsOpedNode(AStarNode node)
        {
            return m_openDic.Contains(node);
        }

        private bool IsClosedNode(AStarNode node)
        {
            return node.version == m_curVersion;
        }

        private void BuildPath()
        {
            m_pathPosList = new List<Vector3>();
            AStarNode node = m_destinationNode;
            m_pathPosList.Add(new Vector3(node.posX, node.posY, node.posZ));

            while (node != m_startNode)
            {
                node = node.parentNode;
                m_pathPosList.Add(new Vector3(node.posX, node.posY, node.posZ));
            }

            int length = m_pathPosList.Count;
            for (int i = 0; i < Mathf.Ceil(length / 2f); i++)
            {
                int iindex = length - i - 1;
                if (iindex != i)
                {
                    Vector3 tmp = m_pathPosList[i];
                    m_pathPosList[i] = m_pathPosList[iindex];
                    m_pathPosList[iindex] = tmp;
                }
            }
        }

        private bool StartSearch()
        {
            AStarNode node = m_startNode;

            while (m_openDic.Count != 0 && node != m_destinationNode)
            {
                m_openDic.Sort(NodeSort);
                node = m_openDic[m_openDic.Count - 1];
                m_openDic.Remove(node);
                AddNodeToCloseQueue(node);

                for (int i = 0; i < node.links.Count; i++)
                {
                    AStartNodeLink link = node.links[i];
                    AStarNode linkedNode = link.linedNode;

                    if (linkedNode.IsWalkAble() && !IsClosedNode(linkedNode))
                    {
                        UpdateNeighborNode(node, linkedNode, link.cost);
                    }
                }

                if (m_openDic.Count == 0)
                {
                    return false;
                }
            }

            return true;
        }

        private int NodeSort(AStarNode node1, AStarNode node2)
        {
            if (node1.GetF() < node2.GetF())
            {
                return 1;
            }
            else if (node1.GetF() > node2.GetF())
            {
                return -1;
            }
            else
            {
                if (node1.GetH() < node2.GetH())
                {
                    return 1;
                }
                else if (node1.GetH() > node2.GetH())
                {
                    return -1;
                }
                return 0;
            }
        }

        private void UpdateNeighborNode(AStarNode parentNode, AStarNode node, float g)
        {
            float nodeG = parentNode.GetG() + g;
            float nodeH = GetEvaluationH(node);
            float nodeF = nodeH + nodeG;

            if (IsOpedNode(node))
            {
                if (nodeG < node.GetG())
                {
                    node.SetG(nodeG);
                    node.SetH(nodeH);
                    node.parentNode = parentNode;
                }
            }
            else
            {
                node.SetG(nodeG);
                node.SetH(nodeH);
                node.parentNode = parentNode;
                AddNodeToOpenQueue(node);
            }
        }

        private float GetEvaluationH(AStarNode node)
        {
            if (m_evaluationFunctionType == EvaluationFunctionType.Manhattan)
                return GetManhattanDistance(node);
            else if (m_evaluationFunctionType == EvaluationFunctionType.Diagonal)
                return GetDiagonalDistance(node);
            else
                return Mathf.Ceil(GetEuclideanDistance(node));
        }

        private float GetManhattanDistance(AStarNode node)
        {
            return Mathf.Abs(node.posX - m_destinationNode.posX) + Mathf.Abs(node.posZ - m_destinationNode.posZ);
        }

        private float GetEuclideanDistance(AStarNode node)
        {
            return Mathf.Sqrt(Mathf.Pow((m_destinationNode.posX - node.posX) * FACTOR, 2) + Mathf.Pow((m_destinationNode.posZ - node.posZ) * FACTOR, 2));
        }

        private float GetDiagonalDistance(AStarNode node)
        {
            float x = Mathf.Abs(m_destinationNode.posX - node.posX);
            float y = Mathf.Abs(m_destinationNode.posZ - node.posZ);
            float min = Mathf.Min(x, y);
            return min * FACTOR_DIAGONAL + Mathf.Abs(x - y) * FACTOR;
        }
    }
}

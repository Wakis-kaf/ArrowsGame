using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using JetBrains.Annotations;
using System.Security.Principal;
using Framework.Runtime.LogSystem;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using Unity.IO.LowLevel.Unsafe;

namespace Game.Modules.GModuleArrows
{
    public enum NodeType { Head, Body }
    public class PointGenerateNode
    {
        public Vector3Int index;
        public Vector3Int moveDirection;
        public LevelPointNode pointNode;
        public bool isOccupied;
        public int occupiedArrowId;
        public PointGenerateNode nextNode;
        public PointGenerateNode prevNode;
        private PointGenerateNode rootNode;
        private PointGenerateNode endNode;
        public bool isHead; // 是否是线条的头部
        public bool isRootNode;
        public bool traceTriggerd;
        public Vector2Int dirAbleFlag;
        public int blackMoveFlag; // x 为 body 移动黑名单,y 为 head 移动黑名单
        public Vector3Int stepNextMoveDir;

        public int revertNum;
        public bool useCustomDir;
        public NodeType customNodeType;
        public int searchPiority;
        public int lineTurnCount;
        public bool isNextStepTurn = false;
        public bool isTractInterrucped; // 如果为true, 回溯到该点则不能再继续回溯
        public bool isRootDirTraced = false;

        public void ClearSearchDatas()
        {
            blackMoveFlag = 0;
            traceTriggerd = false;
            if (isTractInterrucped)
            {
                var a = 1;
            }
            isTractInterrucped = false;

            isRootDirTraced = false;
            searchPiority = 0;
            revertNum = 0;
        }

        public void ClearDatas()
        {
            moveDirection = Vector3Int.zero;
            dirAbleFlag = Vector2Int.zero;
            isOccupied = false;
            occupiedArrowId = -1;
            blackMoveFlag = 0;
            revertNum = 0;
            lineTurnCount = 0;
            searchPiority = 0;
            nextNode = null;
            prevNode = null;
            rootNode = null;
            isHead = false;
            isRootNode = false;
            endNode = null;
            useCustomDir = false;
            traceTriggerd = false;
            isNextStepTurn = false;
            if (isTractInterrucped)
            {
                var a = 1;
            }
            isTractInterrucped = false;
            isRootDirTraced = false;
        }
        public void ReverseChain(bool targetIsHead)
        {
            List<PointGenerateNode> nodes = new List<PointGenerateNode>();
            PointGenerateNode current = this;
            while (current != null)
            {
                nodes.Add(current);
                current = current.nextNode;
            }

            if (nodes.Count == 0) return;

            // for (int i = 0; i < nodes.Count; i++)
            // {
            //     var temp = nodes[i].nextNode;
            //     nodes[i].nextNode = nodes[i].prevNode;
            //     nodes[i].prevNode = temp;
            // }

            // int half = nodes.Count / 2;
            // for (int i = 0; i <= half; i++)
            // {
            //     int j = nodes.Count - 1 - i;
            //     if (i == j)
            //     {
            //         // nodes[i].blackMoveFlag = InvertFlag(nodes[i].blackMoveFlag);
            //         continue;
            //     }

            //     // int tempBlackI = InvertFlag(nodes[i].blackMoveFlag);
            //     // int tempBlackJ = InvertFlag(nodes[j].blackMoveFlag);
            //     // nodes[i].blackMoveFlag = tempBlackJ;
            //     // nodes[j].blackMoveFlag = tempBlackI;

            //     // Vector2Int tempDirAble = nodes[i].dirAbleFlag;
            //     // nodes[i].dirAbleFlag = nodes[j].dirAbleFlag;
            //     // nodes[j].dirAbleFlag = tempDirAble;

            //     // bool tempTrace = nodes[i].traceTriggerd;
            //     // nodes[i].traceTriggerd = nodes[j].traceTriggerd;
            //     // nodes[j].traceTriggerd = tempTrace;

            //     // int tempRevert = nodes[i].revertNum;
            //     // nodes[i].revertNum = nodes[j].revertNum;
            //     // nodes[j].revertNum = tempRevert;

            //     // bool tempUseCustom = nodes[i].useCustomDir;
            //     // nodes[i].useCustomDir = nodes[j].useCustomDir;
            //     // nodes[j].useCustomDir = tempUseCustom;

            //     // NodeType tempCustomType = nodes[i].customNodeType;
            //     // nodes[i].customNodeType = nodes[j].customNodeType;
            //     // nodes[j].customNodeType = tempCustomType;

            //     // int tempPriority = nodes[i].searchPiority;
            //     // nodes[i].searchPiority = nodes[j].searchPiority;
            //     // nodes[j].searchPiority = tempPriority;

            //     // int tempTurnCount = nodes[i].lineTurnCount;
            //     // nodes[i].lineTurnCount = nodes[j].lineTurnCount;
            //     // nodes[j].lineTurnCount = tempTurnCount;

            //     // bool tempNextTurn = nodes[i].isNextStepTurn;
            //     // nodes[i].isNextStepTurn = nodes[j].isNextStepTurn;
            //     // nodes[j].isNextStepTurn = tempNextTurn;

            //     // bool tempInterrupted = nodes[i].isTractInterrucped;
            //     // nodes[i].isTractInterrucped = nodes[j].isTractInterrucped;
            //     // nodes[j].isTractInterrucped = tempInterrupted;
            // }

            var newRoot = nodes[nodes.Count - 1];
            newRoot.isRootNode = true;
            newRoot.prevNode = null;
            newRoot.isHead = targetIsHead;
            newRoot.rootNode = newRoot;
            newRoot.endNode = nodes[0];
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                var curNode = nodes[i];
                if (curNode != newRoot)
                {
                    curNode.isRootNode = false;
                    curNode.isHead = false;
                    curNode.rootNode = newRoot;
                    curNode.endNode = null;
                }
                var preNodeIndex = i + 1;
                var prevNode = preNodeIndex < nodes.Count ? nodes[preNodeIndex] : null;
                curNode.prevNode = prevNode;
                var nextNodeIndex = i - 1;
                var nextNode = nextNodeIndex >= 0 ? nodes[nextNodeIndex] : null;
                curNode.nextNode = nextNode;
                if (curNode.nextNode != null)
                {
                    curNode.stepNextMoveDir = curNode.nextNode.index - curNode.index;
                }
                else
                {
                    curNode.stepNextMoveDir = Vector3Int.zero;
                }
                curNode.ReSyncIsTurnNext();
            }
            newRoot.ReSyncTurnNum();
            newRoot.UpdateRootMoveDirection();
        }

        private void ReSyncIsTurnNext()
        {

            if (nextNode == null)
            {
                isNextStepTurn = false;
            }
            else
            {
                isNextStepTurn = LevelArrowsBoardGeneator.IsPointNextStepTurnDir(this, nextNode.index);
            }
        }

        private int InvertFlag(int flag)
        {
            int newFlag = 0;
            if ((flag & (1 << 1)) != 0) newFlag |= (1 << 3);
            if ((flag & (1 << 3)) != 0) newFlag |= (1 << 1);
            if ((flag & (1 << 2)) != 0) newFlag |= (1 << 4);
            if ((flag & (1 << 4)) != 0) newFlag |= (1 << 2);
            return newFlag;
        }
        public void UpdateRootMoveDirection()
        {
            var rootNode = GetRootNode();
            if (rootNode.isHead)
            {
                rootNode.moveDirection = -rootNode.stepNextMoveDir;
            }
            else
            {
                var endNode = rootNode.GetEndNode();
                var endPrevNode = endNode.prevNode;
                if (endPrevNode != null)
                {
                    rootNode.moveDirection = endNode.index - endPrevNode.index;
                }
            }
        }
        public PointGenerateNode GetRootNode()
        {
            if (isRootNode)
            {
                rootNode = this;
            }
            return rootNode;
        }
        public void DisconnectNext()
        {
            if (nextNode == null)
            {
                return;
            }
            var rootNode = GetRootNode();
            rootNode.endNode = nextNode.prevNode;
            nextNode.ClearDatasSubs();
            nextNode = null;
        }
        public void AppendNextNode(PointGenerateNode nearPoint)
        {
            if (nearPoint == this) return;
            nextNode = nearPoint;
            nearPoint.prevNode = this;
            nearPoint.rootNode = GetRootNode();
            nearPoint.isOccupied = true;
            nearPoint.occupiedArrowId = occupiedArrowId;
            rootNode.endNode = nextNode.SearchEndNode();

        }
        public void ReSyncTurnNum()
        {
            var rootNode = GetRootNode();
            int lenght = 0;
            var curNode = rootNode;
            while (curNode != null)
            {
                if (curNode.isNextStepTurn)
                {
                    lenght++;
                }
                curNode = curNode.nextNode;
            }
            lineTurnCount = lenght;
        }
        public int GetSubLength()
        {
            int lenght = 0;
            var curNode = this;
            while (curNode != null)
            {
                lenght++;
                curNode = curNode.nextNode;
            }
            return lenght;
        }
        public PointGenerateNode GetHeadNode()
        {
            var rootNode = GetRootNode();
            if (rootNode.isHead)
            {
                return rootNode;
            }
            return rootNode.GetEndNode();

        }
        public PointGenerateNode GetTailNode()
        {
            var rootNode = GetRootNode();
            if (rootNode.isHead)
            {
                return rootNode.GetEndNode();
            }
            return rootNode;

        }
        public PointGenerateNode SearchEndNode()
        {
            var curNode = this;
            while (curNode != null)
            {
                if (curNode.nextNode == null)
                {
                    return curNode;
                }
                curNode = curNode.nextNode;
            }
            return curNode;
        }
        public PointGenerateNode GetEndNode()
        {
            // var curNode = this;
            // while (curNode != null)
            // {
            //     if (curNode.nextNode == null)
            //     {
            //         return curNode;
            //     }
            //     curNode = curNode.nextNode;
            // }
            // return curNode;
            return GetRootNode().endNode;

        }
        public int GetLineLength()
        {
            return GetRootNode().GetSubLength();
        }
        public void ClearSearchDatasSubs()
        {
            var curNode = this;
            while (curNode != null)
            {
                var next = curNode.nextNode;
                curNode?.ClearSearchDatas();
                curNode = next;
            }
        }
        public void ClearDatasSubs()
        {
            var curNode = this;
            while (curNode != null)
            {
                var next = curNode.nextNode;
                curNode?.ClearDatas();
                curNode = next;
            }

        }
        public void ClearDeepDatas()
        {
            var endNode = GetRootNode().GetEndNode();
            var curNode = endNode;
            while (curNode != null)
            {
                var needClearNode = curNode;
                curNode = curNode.prevNode;
                needClearNode.ClearDatas();
            }

        }

        public void SetAsEndNode()
        {
            endNode = this;
        }

        public PointGenerateNode GetPrevTurnNode()
        {
            var curNode = GetEndNode();
            while (curNode != null)
            {
                curNode = curNode.prevNode;
                if (curNode != null && curNode.isNextStepTurn)
                {
                    return curNode;
                }
            }
            return null;
        }

        public void ClearAfterTraceOccuipiedPoint()
        {
            var curNode = GetRootNode();
            PointGenerateNode targetNode = null;
            while (curNode != null)
            {
                if (curNode.isTractInterrucped)
                {
                    targetNode = curNode;
                    break;
                }
                curNode = curNode.nextNode;
            }
            if (targetNode != null)
            {
                targetNode.DisconnectNext();
            }

        }
    }
    public class ArrowGenerateNode
    {
        public int arrowId;
        public PointGenerateNode rootNode;
        public HashSet<Vector3Int> m_NeighborCheckMap = new HashSet<Vector3Int>();
        public bool hasSolveDeep;

        public void ConvertHeadToRoot()
        {
            if (rootNode.isHead)
            {
                return;
            }
            rootNode.ReverseChain(true);
            rootNode = rootNode.GetRootNode();
        }
        public void ConvertTailToRoot()
        {
            if (!rootNode.isHead)
            {
                return;
            }
            rootNode.ReverseChain(false);
            rootNode = rootNode.GetRootNode();
        }
        public PointGenerateNode GetHeadNode()
        {
            return rootNode.GetHeadNode();
        }
        public LevelArrowNode ToRuntimeArrow()
        {
            List<Vector3Int> pointIds = new List<Vector3Int>();
            List<Vector3> pathPositions = new List<Vector3>();

            PointGenerateNode current = rootNode;
            while (current != null)
            {
                if (current.pointNode != null)
                {
                    pointIds.Add(current.pointNode.index);
                    pathPositions.Add(current.pointNode.worldPosition);
                }
                current = current.nextNode;
            }

            Vector3 direction = Vector3.zero;
            if (rootNode != null)
            {
                direction = new Vector3(rootNode.moveDirection.x, rootNode.moveDirection.y, rootNode.moveDirection.z);
            }
            if (rootNode.isHead)
            {
                pointIds.Reverse();
                pathPositions.Reverse();
            }
            var arrowNode = new LevelArrowNode(arrowId, pointIds, direction, pathPositions, hasSolveDeep);
            return arrowNode;
        }

        public bool HasNeighborCheck(Vector3Int index)
        {
            return m_NeighborCheckMap.Contains(index);
        }
        public void SetNeighborCheck(Vector3Int index)
        {
            m_NeighborCheckMap.Add(index);
        }

        public void ClearNodeSearchDatas()
        {
            rootNode?.ClearSearchDatasSubs();
        }
    }

    public class Vector3IntComparer : IEqualityComparer<Vector3Int>
    {
        public bool Equals(Vector3Int x, Vector3Int y)
        {
            return x.x == y.x && x.y == y.y && x.z == y.z;
        }

        public int GetHashCode(Vector3Int obj)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + obj.x;
                hash = hash * 23 + obj.y;
                hash = hash * 23 + obj.z;
                return hash;
            }
        }
    }

    public class LevelArrowsBoardGeneator
    {

        private static readonly Vector3Int[] Directions = new Vector3Int[]
        {
            Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left
        };
        public Dictionary<int, PointGenerateNode> pointToGenerateMap;
        private List<ArrowGenerateNode> m_ArrowGenerateNodes;
        private Dictionary<int, ArrowGenerateNode> m_Id2ArrowGenerateNodes;
        private HashSet<int> m_ConfirmedLineIds = new HashSet<int>();
        private Dictionary<int, HashSet<int>> m_ArrowCollisionMap;
        // private Dictionary<int, HashSet<int>> m_ArrowBlockMap;
        private List<LevelPointNode> m_BorderNodes;
        private HashSet<PointGenerateNode> m_RootVisitedMap;
        public LevelPointLayout layout;
        public LevelArrowsPresure pressure;
        private Queue<PointGenerateNode> m_BFSGeneratePointQueue;
        private int m_CurArrowId;
        private SeedRandom m_SeedRandom = new SeedRandom();
        private Queue<PointGenerateNode> m_AvailableNeighborNodes;
        private readonly List<(NodeType type, int value)> m_PointAxisPool = new List<(NodeType type, int value)>(2);
        private readonly List<Vector3Int> m_PointStepAbleDirs = new List<Vector3Int>(4);
        private int m_LayoutMaxX;
        private int m_LayoutMinX;
        private int m_LayoutMaxY;
        private int m_LayoutMinY;
        public LevelArrowsBoardGeneator()
        {
            pointToGenerateMap = new Dictionary<int, PointGenerateNode>();
            m_BFSGeneratePointQueue = new Queue<PointGenerateNode>();
            m_ArrowGenerateNodes = new List<ArrowGenerateNode>();
            m_Id2ArrowGenerateNodes = new Dictionary<int, ArrowGenerateNode>();
            m_ConfirmedLineIds = new HashSet<int>();
            m_RootVisitedMap = new HashSet<PointGenerateNode>();
            m_ArrowCollisionMap = new Dictionary<int, HashSet<int>>();
            m_AvailableNeighborNodes = new Queue<PointGenerateNode>();
        }
        private void ResetDatas()
        {
            m_PointAxisPool.Clear();
            m_PointStepAbleDirs.Clear();
            pointToGenerateMap.Clear();
            m_BFSGeneratePointQueue.Clear();
            m_ArrowGenerateNodes.Clear();
            m_Id2ArrowGenerateNodes.Clear();
            m_ConfirmedLineIds.Clear();
            m_CurArrowId = 0;
            m_RootVisitedMap.Clear();
            m_ArrowCollisionMap.Clear();
            m_AvailableNeighborNodes.Clear();
            m_BorderNodes = layout.GetBoardNodes();
            if (pressure.isUsingCustomSeed)
            {
                m_SeedRandom.InitSeed(pressure.customSeed);
            }
            else
            {
                m_SeedRandom.InitSeed();
            }

            m_LayoutMaxX = layout.MaxX;
            m_LayoutMinX = layout.MinX;
            m_LayoutMaxY = layout.MaxY;
            m_LayoutMinY = layout.MinY;
            pressure.tryMaxLength = false;
            pressure.isEnableRootTraceHead = false;
            pressure.runtimeSeed = m_SeedRandom.Seed;
        }
        public PointGenerateNode GetPointByRealId(int id)
        {
            foreach (var item in pointToGenerateMap)
            {
                if (item.Value.pointNode.id == id)
                {
                    return item.Value;
                }
            }
            return null;

        }
        public ArrowGenerateNode GetArrowById(int arrowId)
        {
            if (m_Id2ArrowGenerateNodes.TryGetValue(arrowId, out var arrow))
            {
                return arrow;
            }

            return null;
        }
        public PointGenerateNode GetNodeByIndex(int x, int y, int z)
        {
            if (x < m_LayoutMinX || x > m_LayoutMaxX || y < m_LayoutMinY || y > m_LayoutMaxY)
            {
                return null;
            }
            if (pointToGenerateMap.TryGetValue(GetIdByIndex(x, y, z), out PointGenerateNode node))
            {
                return node;
            }
            return null;
        }
        public PointGenerateNode GetNodeByIndex(Vector3Int index)
        {
            return GetNodeByIndex(index.x, index.y, index.z);
        }
        public PointGenerateNode GetNodeById(int id)
        {

            if (pointToGenerateMap.TryGetValue(id, out PointGenerateNode node))
            {
                return node;
            }
            return null;
        }
        private int GetIdByIndex(int x, int y, int z)
        {
            int width = m_LayoutMaxX - m_LayoutMinX + 1;

            // 偏移后的坐标
            int offsetX = x - m_LayoutMinX;
            int offsetY = y - m_LayoutMinY;

            // 展平公式：X + 每一行的宽度 * Y
            return offsetX + width * offsetY;
        }

        private int GetIdByIndex(Vector3Int index)
        {
            return GetIdByIndex(index.x, index.y, index.z);
        }

        private void InitGeneratePointMap()
        {
            var points = layout.GetAllNodes();
            pointToGenerateMap.Clear();
            foreach (var point in points)
            {
                pointToGenerateMap[GetIdByIndex(point.index)] = new PointGenerateNode
                {
                    index = point.index,
                    pointNode = point,
                    isOccupied = false,
                    occupiedArrowId = -1,
                };
            }
        }


        private bool enableLog = false;
        public List<LevelArrowNode> GenerateBoardArrows()
        {
            ResetDatas();
            bool isEnableRootTraceHead = pressure.isEnableRootTraceHead;
            //  先初始化点阵
            InitGeneratePointMap();
            // 随机挑选一个点开始生成线条
            TryGenerateMainArrow();
            //  为了减少空白点,再重新对空白点进行生成
            ReTryGenerateArrowInEmptyPoint();
            pressure.isEnableRootTraceHead = true;
            // 延长头部，减少空白点数量
            TryExtendHead();
            //  延长尾部,减少空白点
            TryExtendTail();
            pressure.isEnableRootTraceHead = isEnableRootTraceHead;
            return ConvetGenerateArrowToRuntimeArrows();
        }
        private void TryExtendHead()
        {
            Stopwatch sw = Stopwatch.StartNew();
            pressure.tryMaxLength = true;
            foreach (var arrow in m_ArrowGenerateNodes)
            {
                // var arrow = GetArrowById(12);
                arrow.ConvertTailToRoot();
                var headNode = arrow.rootNode.GetHeadNode();
                if (!IsNearHasUnOccupiedNode(headNode)) { continue; }
                // if (endNode == null) continue;
                headNode.isTractInterrucped = true;
                m_BFSGeneratePointQueue.Enqueue(headNode);
                GenerateBoardArrowsByBfs(pressure.emptyReGenerateMaxRetractNum, false);
                headNode.isTractInterrucped = false;
            }
            sw.Stop();
            Log.Info($"延长头部结束,耗时{sw.ElapsedMilliseconds}毫秒");
        }
        private void TryExtendTail()
        {
            Stopwatch sw = Stopwatch.StartNew();
            pressure.tryMaxLength = true;
            foreach (var arrow in m_ArrowGenerateNodes)
            {
                // var arrow = GetArrowById(88);
                arrow.ConvertHeadToRoot();
                var tailNode = arrow.rootNode.GetTailNode();
                if (!IsNearHasUnOccupiedNode(tailNode)) { continue; }
                // if (endNode == null) continue;
                tailNode.isTractInterrucped = true;
                m_BFSGeneratePointQueue.Enqueue(tailNode);
                GenerateBoardArrowsByBfs(pressure.emptyReGenerateMaxRetractNum, false);
                tailNode.isTractInterrucped = false;
            }
            sw.Stop();
            Log.Info($"延长尾部结束,耗时{sw.ElapsedMilliseconds}毫秒");
        }
        /// <summary>
        /// 生成主线条
        /// </summary>
        private void TryGenerateMainArrow()
        {
            Stopwatch sw = Stopwatch.StartNew();
            var point = GetBorderUnVisitedUnoccupiediedNode();
            RegisterAllowSearchStartPoint(point);
            GenerateBoardArrowsByBfs(pressure.mainLineGenerateMaxRetractNum);
            sw.Stop();
            Log.Info($"初步生成线条结束,耗时{sw.ElapsedMilliseconds}毫秒");
        }
        /// <summary>
        /// 重新对空白点进行生成
        /// </summary>
        private void ReTryGenerateArrowInEmptyPoint()
        {
            Stopwatch sw = Stopwatch.StartNew();
            var unOccupiedNodeIds = GetUnOccupiedGenerateNodeIds();
            // enableLog = true;
            foreach (var unOccupiedId in unOccupiedNodeIds)
            {
                m_RootVisitedMap.Remove(GetNodeById(unOccupiedId));
            }
            var point = GetRandomUnoccupiediedPoint();
            // var point = GetPointByRealId(344);
            Log.Info($"尝试对空白点重新生成 是否有首点 {point != null} 首点id{point?.pointNode.id} ");
            RegisterAllowSearchStartPoint(point);
            pressure.tryMaxLength = true;
            int generateLineNum = GenerateBoardArrowsByBfs(pressure.emptyReGenerateMaxRetractNum);
            Log.Info($"尝试对空白点重新生成 结束,再生成线条数 {generateLineNum}");
            // pressure.mainLineGenerateMaxRetractNum = originRetractAttemp;
            sw.Stop();
            Log.Info($"空白点再生成线条结束,耗时{sw.ElapsedMilliseconds}毫秒");

        }
        private void RegisterAllowSearchStartPoint(PointGenerateNode pointNode)
        {
            if (pointNode == null) return;
            pointNode.DisconnectNext();
            pointNode.ClearDatas();
            pointNode.occupiedArrowId = m_CurArrowId;
            pointNode.isRootNode = true;
            pointNode.SetAsEndNode();
            m_RootVisitedMap.Add(pointNode);
            m_BFSGeneratePointQueue.Enqueue(pointNode);
        }


        private List<LevelArrowNode> ConvetGenerateArrowToRuntimeArrows()
        {
            List<LevelArrowNode> list = new List<LevelArrowNode>();
            foreach (var arrow in m_ArrowGenerateNodes)
            {
                list.Add(arrow.ToRuntimeArrow());
            }
            return list;
        }
        private void ClearArrowData(PointGenerateNode point)
        {
            point.ClearDeepDatas();

        }
        private int GetMoveDirFlag(Vector3Int moveDir)
        {
            if (moveDir == Vector3Int.up)
            {
                return (1 << 1);
            }
            else if (moveDir == Vector3Int.right)
            {
                return (1 << 2);
            }
            else if (moveDir == Vector3Int.down)
            {
                return (1 << 3);
            }
            else if (moveDir == Vector3Int.left)
            {
                return (1 << 4);
            }
            return 0;
        }

        private void StartBfsStartTimer()
        {

        }
        private int GenerateBoardArrowsByBfs(int maxRetractNum = -1, bool allowInertGetAblePoint = true)
        {
            int startArrowId = m_CurArrowId;
            while (m_BFSGeneratePointQueue.Count > 0)
            {
                var topPoint = m_BFSGeneratePointQueue.Dequeue();
                bool tryAttemptOver = IsLinkAttemptOver(topPoint);
                topPoint.dirAbleFlag = GetPointGenerateAbleDirFlag(topPoint, tryAttemptOver);
                var moveDir = GetNodeMoveDir(topPoint, topPoint.dirAbleFlag, out bool isHead);
                // 设置基础标记
                var rootNode = topPoint.GetRootNode();
                bool hasConfirmedLine = HasLineConfirmed(rootNode.occupiedArrowId);
                topPoint.isHead = isHead;
                topPoint.isOccupied = true;
                if (!hasConfirmedLine)
                {
                    topPoint.occupiedArrowId = m_CurArrowId;
                }

                topPoint.stepNextMoveDir = moveDir;
                var nextPointIndex = topPoint.index + moveDir;
                topPoint.isNextStepTurn = IsPointNextStepTurnDir(topPoint, nextPointIndex);
                if (topPoint.isNextStepTurn)
                {
                    rootNode.lineTurnCount++;
                }
                bool isFindNearSuc = moveDir != Vector3Int.zero;
                PointGenerateNode nearPoint = null;
                if (isFindNearSuc)
                {
                    nearPoint = GetNodeByIndex(nextPointIndex);
                    nearPoint.isOccupied = true;
                    nearPoint.occupiedArrowId = rootNode.occupiedArrowId;
                    // 将下一方向上的点加入到队列
                    topPoint.AppendNextNode(nearPoint);
                    if (enableLog)
                        Log.Info($"$节点寻路 当前查线{topPoint.occupiedArrowId} 当前节点id:{topPoint.pointNode.id} 寻路方向{moveDir}  添加下一个点到队里{nearPoint.pointNode.id}");
                }
                rootNode.UpdateRootMoveDirection();
                // 判断是否可以结束

                if (tryAttemptOver || !isFindNearSuc)
                {
                    if (enableLog)
                        Log.Info($"节点寻路tryAttemptOver：{tryAttemptOver} !isFindNearSuc: {!isFindNearSuc}");
                    if (IsLinkArrowAble(topPoint, out var arrowNode, out bool isNewArrow))
                    {
                        m_BFSGeneratePointQueue.Clear();
                        if (isNewArrow)
                        {
                            m_CurArrowId++;
                        }
                    }
                    else if (!isFindNearSuc)
                    {
                        ArrowSearchTrace(topPoint, maxRetractNum);
                    }
                    else
                    {
                        //  未成功就继续添加点
                        m_BFSGeneratePointQueue.Enqueue(nearPoint);
                    }
                }
                else
                {
                    //  未成功就继续添加点
                    m_BFSGeneratePointQueue.Enqueue(nearPoint);
                }
                // UpdateArrowCollisionMap(false);
                if (allowInertGetAblePoint)
                {
                    TryAddAblePointToQueue();
                }

            }

            return m_CurArrowId - startArrowId;

        }
        private bool IsDisablePointSearchTrace(PointGenerateNode needRevertPoint, int maxRetractNum)
        {
            if (needRevertPoint.isTractInterrucped) //  如果有阻止回溯标记,则不允许回溯
            {
                return true;
            }
            var rootPoint = needRevertPoint.GetRootNode();
            int revertCount = rootPoint.revertNum;
            bool isOverRevertLimit = maxRetractNum >= 0 && revertCount >= maxRetractNum;
            bool isDisableTraceRoot = (needRevertPoint.isRootNode && needRevertPoint.dirAbleFlag == Vector2Int.zero) && (!pressure.isEnableRootTraceHead || needRevertPoint.isRootDirTraced);
            bool isDisableTraceNormal = (!needRevertPoint.isRootNode && isOverRevertLimit);
            bool disableTrace = isDisableTraceNormal || isDisableTraceRoot;
            return disableTrace;
        }
        private void ArrowSearchTrace(PointGenerateNode topPoint, int maxRetractNum)
        {
            // 说明当前不可以移动了，,回退到上一步
            var needRevertPoint = topPoint;
            var rootPoint = topPoint.GetRootNode();
            int revertCount = rootPoint.revertNum;
            bool isConfirmedArrow = HasLineConfirmed(topPoint.occupiedArrowId);
            if (IsDisablePointSearchTrace(needRevertPoint, maxRetractNum))//  不允许回溯
            {
                if (!isConfirmedArrow)
                {
                    // 清除根节点数据,重新找点生成
                    ClearArrowData(needRevertPoint);
                    m_BFSGeneratePointQueue.Clear();
                }
                else
                {
                    // 从回溯标记点开始删除
                    topPoint.ClearAfterTraceOccuipiedPoint();
                }

            }
            else
            {
                rootPoint.revertNum++;
                if (needRevertPoint.isRootNode)
                {
                    //  尝试反向
                    needRevertPoint.useCustomDir = true;
                    needRevertPoint.customNodeType = needRevertPoint.isHead ? NodeType.Body : NodeType.Head;
                    needRevertPoint.blackMoveFlag = 0;
                    needRevertPoint.isRootDirTraced = true;
                    if (enableLog)
                        Log.Info($"根节点寻路 回溯 当前查线{needRevertPoint.occupiedArrowId} 当前节点id:{needRevertPoint.pointNode.id} 下一个方向{needRevertPoint.customNodeType}");
                }
                else
                {
                    if (!pressure.retractFromTurn)
                    {
                        needRevertPoint = topPoint.prevNode;
                    }
                    else
                    {
                        needRevertPoint = topPoint.GetPrevTurnNode() ?? topPoint.prevNode;
                    }
                    needRevertPoint.blackMoveFlag |= GetMoveDirFlag(needRevertPoint.stepNextMoveDir);
                    if (enableLog)
                        Log.Info($"节点寻路回溯 当前查线{needRevertPoint.occupiedArrowId} 当前节点id:{needRevertPoint.pointNode.id} 禁用方向{needRevertPoint.stepNextMoveDir}");
                }
                if (needRevertPoint.isNextStepTurn)
                {
                    needRevertPoint.GetRootNode().lineTurnCount--;
                }
                needRevertPoint.traceTriggerd = true;
                needRevertPoint.DisconnectNext();
                needRevertPoint.UpdateRootMoveDirection();
                m_BFSGeneratePointQueue.Enqueue(needRevertPoint);
            }
        }
        private void TryAddAblePointToQueue()
        {
            if (m_BFSGeneratePointQueue.Count == 0)
            {
                // 随机添加周围的一个可以移动的点到队列中

                string rdmPointGetWay = "GetRdmUnOccupiedPointNearArrow";

                PointGenerateNode emptyNode = GetRdmUnOccupiedPointNearArrow();
                if (emptyNode == null)
                {
                    emptyNode = GetBorderUnVisitedUnoccupiediedNode();
                    rdmPointGetWay = "GetBorderUnVisitedUnoccupiediedNode";

                }
                if (emptyNode == null)
                {
                    emptyNode = GetRandomUnoccupiediedPoint();
                    rdmPointGetWay = "GetRandomUnoccupiediedPoint";

                }
                if (emptyNode != null)
                {
                    RegisterAllowSearchStartPoint(emptyNode);
                    if (enableLog)
                        Log.Info($"随机点获取方式{rdmPointGetWay} ，当前点{emptyNode.pointNode.id} 线条{emptyNode.occupiedArrowId}");
                }
                else
                {
                    // var a = 1;
                    // emptyNode = GetRdmUnOccupiedPointNearArrow() ?? GetBorderUnVisitedUnoccupiediedNode() ?? GetRandomUnoccupiediedPoint();
                }

            }
        }

        private PointGenerateNode GetRdmUnOccupiedPointNearArrow()
        {
            while (m_AvailableNeighborNodes.Count > 0)
            {
                var node = m_AvailableNeighborNodes.Dequeue();
                if (!node.isOccupied && !IsRootVisited(node))
                {
                    return node;
                }
            }
            return null;
        }

        private int AvaliableNeighborSortCompare(PointGenerateNode x, PointGenerateNode y)
        {
            if (x.searchPiority > y.searchPiority)
            {
                return 1;
            }
            else if (x.searchPiority == y.searchPiority)
            {
                return 0;
            }
            return -1;

        }

        private void AddNeighborsToCache(PointGenerateNode point, int priority)
        {
            if (point.isOccupied) return;
            if (m_AvailableNeighborNodes.Contains(point)) return;
            point.searchPiority = priority;
            m_AvailableNeighborNodes.Enqueue(point);


        }
        private void AddNeighborsToCache(ArrowGenerateNode arrow)
        {
            if (arrow == null)
            {
                return;
            }
            // if (!IsHasSolve(arrow)) return;
            // 如果包含头部位置,优先返回头部位置点
            var headFromIndex = arrow.GetHeadNode().index + arrow.rootNode.moveDirection;
            var neighborNode = GetNodeByIndex(headFromIndex);
            if (neighborNode != null)
            {
                if (!neighborNode.isOccupied && !arrow.HasNeighborCheck(neighborNode.index))
                {
                    // Log.Info($"线条{arrowGenerateNode.arrowId} 优先返回头部节点{neighborNode.pointNode.id}");
                    AddNeighborsToCache(neighborNode, 1000);
                }
            }

            var endNode = arrow.rootNode.GetEndNode();
            var endLastNode = endNode.prevNode;
            var endDir = (endNode.index - endLastNode.index);
            var endIndex = endNode.index + endDir;
            neighborNode = GetNodeByIndex(endIndex);
            if (neighborNode != null)
            {
                if (!neighborNode.isOccupied && !arrow.HasNeighborCheck(neighborNode.index))
                {
                    // Log.Info($"线条{arrowGenerateNode.arrowId} 优先返回尾巴节点{neighborNode.pointNode.id}");
                    AddNeighborsToCache(neighborNode, 500);
                }
            }

            // 1. 遍历该箭头占用的所有点
            PointGenerateNode current = arrow.rootNode;
            while (current != null)
            {
                // 2. 检查当前节点周围的四个方向
                foreach (var dir in Directions)
                {
                    Vector3Int neighborPos = current.index + dir;

                    // 3. 检查邻居点是否存在且未被占用
                    neighborNode = GetNodeByIndex(neighborPos);
                    if (neighborNode != null)
                    {
                        if (!neighborNode.isOccupied && !arrow.HasNeighborCheck(neighborNode.index))
                        {
                            AddNeighborsToCache(neighborNode, 100);
                        }
                    }
                }
                current = current.nextNode;
            }

        }
        private PointGenerateNode GetRdmUnOccupiedPointNearArrow(ArrowGenerateNode arrowGenerateNode)
        {
            if (arrowGenerateNode == null)
            {
                return null;
            }
            // 如果包含头部位置,优先返回头部位置点
            var headFromIndex = arrowGenerateNode.GetHeadNode().index + arrowGenerateNode.rootNode.moveDirection;
            var neighborNode = GetNodeByIndex(headFromIndex);
            if (neighborNode != null)
            {
                if (!neighborNode.isOccupied && !arrowGenerateNode.HasNeighborCheck(neighborNode.index))
                {
                    // Log.Info($"线条{arrowGenerateNode.arrowId} 优先返回头部节点{neighborNode.pointNode.id}");
                    return neighborNode;
                }
            }

            var endNode = arrowGenerateNode.rootNode.GetEndNode();
            var endLastNode = endNode.prevNode;
            var endDir = (endNode.index - endLastNode.index);
            var endIndex = endNode.index + endDir;
            neighborNode = GetNodeByIndex(endIndex);
            if (neighborNode != null)
            {
                if (!neighborNode.isOccupied && !arrowGenerateNode.HasNeighborCheck(neighborNode.index))
                {
                    // Log.Info($"线条{arrowGenerateNode.arrowId} 优先返回尾巴节点{neighborNode.pointNode.id}");
                    return neighborNode;
                }
            }

            // 否则在线条占据的周围的点附近随机挑选一个未被占用的点
            List<PointGenerateNode> candidates = new List<PointGenerateNode>();

            // 1. 遍历该箭头占用的所有点
            PointGenerateNode current = arrowGenerateNode.rootNode;
            while (current != null)
            {
                // 2. 检查当前节点周围的四个方向
                foreach (var dir in Directions)
                {
                    Vector3Int neighborPos = current.index + dir;

                    // 3. 检查邻居点是否存在且未被占用
                    neighborNode = GetNodeByIndex(neighborPos);
                    if (neighborNode != null)
                    {
                        if (!neighborNode.isOccupied && !arrowGenerateNode.HasNeighborCheck(neighborNode.index))
                        {
                            candidates.Add(neighborNode);
                        }
                    }
                }
                current = current.nextNode;
            }

            // 4. 从候选中随机返回一个
            if (candidates.Count > 0)
            {
                return candidates[GetRdmRange(0, candidates.Count)];
            }

            return null;


        }

        // private PointGenerateNode FindNodeRecursive(ArrowGenerateNode arrow, HashSet<ArrowGenerateNode> visited)
        // {
        //     if (arrow == null || !visited.Add(arrow)) return null;

        //     var node = GetRdmUnOccupiedPointNearArrow(arrow);
        //     if (node != null)
        //     {
        //         arrow.SetNeighborCheck(node.index);
        //         return node;
        //     }

        //     HashSet<int> dependentArrows = GetOccBlockArrow(arrow);
        //     if (dependentArrows != null)
        //     {
        //         foreach (var nextArrowId in dependentArrows)
        //         {
        //             var nextArrow = GetArrowById(nextArrowId);
        //             if (nextArrow == null) continue;
        //             var targetNode = FindNodeRecursive(nextArrow, visited);
        //             if (targetNode != null)
        //             {
        //                 return targetNode;
        //             }
        //         }
        //     }

        //     return null;
        // }
        // private HashSet<int> GetOccDependArrow(ArrowGenerateNode arrow)
        // {
        //     if (m_ArrowBlockMap.TryGetValue(arrow.arrowId, out var blockArrows))
        //     {
        //         return blockArrows;
        //     }

        //     return null;
        // }
        private HashSet<int> GetOccBlockArrow(ArrowGenerateNode arrow)
        {
            int curId = arrow.arrowId;
            HashSet<int> occArrow = new HashSet<int>();
            foreach (var item in m_ArrowCollisionMap)
            {
                if (item.Value.Contains(arrow.arrowId))
                {
                    occArrow.Add(item.Key);
                }
            }
            return occArrow;
        }
        private HashSet<int> GetOccArrows(ArrowGenerateNode arrow)
        {
            int curId = arrow.arrowId;
            if (m_ArrowCollisionMap.TryGetValue(curId, out var arrows))
            {
                return arrows;

            }

            return null;
        }
        private HashSet<ArrowGenerateNode> m_IterativeVisiting = new HashSet<ArrowGenerateNode>(32);
        private Dictionary<ArrowGenerateNode, bool> m_IterativeResults = new Dictionary<ArrowGenerateNode, bool>(32);
        private Stack<ArrowGenerateNode> m_IterativeStack = new Stack<ArrowGenerateNode>(32);

        private bool IsHasSolveDeep(ArrowGenerateNode root)
        {
            if (root == null) return false;
            if (!IsArrowExist(root.arrowId))
            {
                return true;
            }
            m_IterativeVisiting.Clear();
            m_IterativeResults.Clear();
            m_IterativeStack.Clear();

            m_IterativeStack.Push(root);

            while (m_IterativeStack.Count > 0)
            {
                var current = m_IterativeStack.Peek();

                if (IsHasSolveDirectly(current))
                {
                    m_IterativeResults[current] = true;
                    m_IterativeStack.Pop();
                    continue;
                }

                if (m_IterativeResults.ContainsKey(current))
                {
                    m_IterativeStack.Pop();
                    continue;
                }

                if (!m_IterativeVisiting.Contains(current))
                {
                    m_IterativeVisiting.Add(current);
                    HashSet<int> occArrows = GetOccArrows(current);
                    bool pushChildren = false;

                    if (occArrows != null)
                    {
                        foreach (var nextId in occArrows)
                        {
                            var next = GetArrowById(nextId);
                            if (next == null) continue;

                            if (m_IterativeResults.ContainsKey(next)) continue;

                            if (m_IterativeVisiting.Contains(next))
                            {
                                m_IterativeResults[next] = false;
                                continue;
                            }

                            m_IterativeStack.Push(next);
                            pushChildren = true;
                        }
                    }

                    if (pushChildren) continue;
                }

                HashSet<int> children = GetOccArrows(current);
                bool allChildrenTrue = true;
                if (children != null)
                {
                    foreach (var nextId in children)
                    {
                        var next = GetArrowById(nextId);
                        if (next == null) continue;
                        if (!m_IterativeResults.TryGetValue(next, out bool childRes) || !childRes)
                        {
                            allChildrenTrue = false;
                            break;
                        }
                    }
                }

                m_IterativeResults[current] = allChildrenTrue;
                m_IterativeVisiting.Remove(current);
                m_IterativeStack.Pop();
            }

            return m_IterativeResults.TryGetValue(root, out bool result) && result;
        }

        private bool IsHasSolveDirectly(ArrowGenerateNode arrow)
        {
            if (!IsArrowExist(arrow.arrowId))
            {
                return true;
            }
            return !m_ArrowCollisionMap.ContainsKey(arrow.arrowId);
        }
        private bool IsArrowExist(int arrowId)
        {
            return m_Id2ArrowGenerateNodes.ContainsKey(arrowId);
        }
        private float GetRdmRange(float min, float max)
        {
            return m_SeedRandom.Range(min, max);

        }
        private int GetRdmRange(int min, int max)
        {
            return m_SeedRandom.Range(min, max);

        }
        private bool IsTryLinkOver(PointGenerateNode curNode)
        {
            int lenght = curNode.GetLineLength();
            int rdmLenght = GetRdmRange(pressure.minLineLength, pressure.maxLineLength);
            return lenght >= rdmLenght && IsDirMoveAble(curNode, curNode.GetRootNode().moveDirection);
        }
        private bool IsLinkAttemptOver(PointGenerateNode curNode)
        {
            int length = curNode.GetLineLength();
            if (pressure.tryMaxLength)
            {
                return false;
            }
            if (length < pressure.minLineLength)
            {
                return false;
            }
            if (curNode.traceTriggerd)
            {
                return true;
            }
            if (length >= pressure.maxLineLength)
            {
                return true;
            }
            int rdmLenght = GetRdmRange(pressure.normalLineLength, pressure.maxLineLength);
            if (length >= rdmLenght)
            {
                return true;
            }
            return false;
        }
        private bool IsLinkArrowAble(PointGenerateNode curNode, out ArrowGenerateNode arrow, out bool isNewLine)
        {
            int length = curNode.GetLineLength();
            arrow = null;
            isNewLine = !m_ConfirmedLineIds.Contains(curNode.occupiedArrowId);
            if (length < pressure.minLineLength)
            {
                return false;
            }
            if (!IsDirMoveAble(curNode.GetHeadNode(), curNode.GetRootNode().moveDirection))
            {
                return false;
            }
            ArrowGenerateNode arroNode = null;
            if (isNewLine)
            {
                arroNode = new ArrowGenerateNode
                {
                    arrowId = m_CurArrowId,
                    rootNode = curNode.GetRootNode(),
                };
                m_ArrowGenerateNodes.Add(arroNode);
                m_Id2ArrowGenerateNodes.Add(arroNode.arrowId, arroNode);
            }
            else
            {
                arroNode = GetArrowById(curNode.occupiedArrowId);
            }
            arroNode.hasSolveDeep = false;
            UpdateArrowCollisionMap(false, arroNode.arrowId, m_UnStableHitList);
            //  判断是否有解
            if (!IsHasSolveDeep(arroNode))
            {
                if (isNewLine)
                {
                    m_ArrowGenerateNodes.Remove(arroNode);
                    m_Id2ArrowGenerateNodes.Remove(arroNode.arrowId);
                    RemoveCollisionArrowId(arroNode.arrowId);
                }
                arroNode.hasSolveDeep = false;
                return false;
            }
            arrow = arroNode;
            if (isNewLine)
            {
                OnLinkNewArrow(arroNode);
            }
            arroNode.ClearNodeSearchDatas();
            arroNode.hasSolveDeep = true;
            return true;
        }
        private void RemoveCollisionArrowId(int arrowId)
        {
            m_ArrowCollisionMap.Remove(arrowId);
            for (int i = 0; i < m_UnStableHitList.Count; i++)
            {
                m_UnStableHitList[i].Remove(arrowId);
            }
            // foreach (var item in m_ArrowCollisionMap)
            // {
            //     item.Value.Remove(arrowId);
            // }
        }
        private void OnLinkNewArrow(ArrowGenerateNode arroNode)
        {
            m_ConfirmedLineIds.Add(arroNode.arrowId);
            AddNeighborsToCache(arroNode);
            // if (pressure.allowSelfCoil && DetectClosureAndGetInternalFields(arroNode, out HashSet<PointGenerateNode> internalPoints))
            // {
            //     var endNode = arroNode.rootNode.GetEndNode();
            //     var nextEndNodeIndex = endNode.index + endNode.prevNode.stepNextMoveDir;
            //     var nextEndNode = GetNodeByIndex(nextEndNodeIndex);
            //     // 允许盘绕检查,检查内部是否具有独立空间

            //     if (nextEndNode != null && internalPoints.Contains(nextEndNode))
            //     {
            //         Log.Info($"检测到线条可延长封闭区间,线条id{arroNode.arrowId} 内部点{internalPoints.Count}");
            //         foreach (var item in internalPoints)
            //         {
            //             Log.Info($"检测到线条可延长封闭区间,线条id{arroNode.arrowId} 内部点{internalPoints.Count} 索引{item.pointNode.id}");
            //         }
            //         //  开始延长线条
            //         endNode.AppendNextNode(FindMaxLengehPath(endNode, internalPoints));
            //     }

            // }
        }
        private PointGenerateNode FindMaxLengehPath(PointGenerateNode startNode, HashSet<PointGenerateNode> availableNodes)
        {
            return startNode;
        }
        private readonly HashSet<PointGenerateNode> m_LineNodesSetCache = new HashSet<PointGenerateNode>();
        private readonly HashSet<Vector3Int> m_VisitedInternalSetCache = new HashSet<Vector3Int>(new Vector3IntComparer());
        private readonly Queue<Vector3Int> m_LocalFillQueueCache = new Queue<Vector3Int>();
        private readonly HashSet<Vector3Int> m_CheckedSeedsSetCache = new HashSet<Vector3Int>(new Vector3IntComparer());

        public bool DetectClosureAndGetInternalFields(ArrowGenerateNode arrowNode, out HashSet<PointGenerateNode> internalPoints)
        {
            internalPoints = new HashSet<PointGenerateNode>();

            if (arrowNode == null || arrowNode.rootNode == null)
            {
                return false;
            }

            m_LineNodesSetCache.Clear();
            PointGenerateNode current = arrowNode.rootNode;
            while (current != null)
            {
                m_LineNodesSetCache.Add(current);
                current = current.nextNode;
            }

            if (m_LineNodesSetCache.Count < 4)
            {
                return false;
            }

            m_CheckedSeedsSetCache.Clear();
            HashSet<Vector3Int> totalInternalIndices = new HashSet<Vector3Int>(new Vector3IntComparer());

            current = arrowNode.rootNode;
            while (current != null)
            {
                foreach (var dir in Directions)
                {
                    Vector3Int seedIdx = current.index + dir;

                    if (m_CheckedSeedsSetCache.Contains(seedIdx) || totalInternalIndices.Contains(seedIdx))
                    {
                        continue;
                    }

                    var seedNode = GetNodeByIndex(seedIdx);
                    if (seedNode != null && m_LineNodesSetCache.Contains(seedNode))
                    {
                        continue;
                    }

                    m_VisitedInternalSetCache.Clear();
                    m_LocalFillQueueCache.Clear();

                    m_LocalFillQueueCache.Enqueue(seedIdx);
                    m_VisitedInternalSetCache.Add(seedIdx);
                    bool isLeaking = false;

                    while (m_LocalFillQueueCache.Count > 0)
                    {
                        Vector3Int currIdx = m_LocalFillQueueCache.Dequeue();

                        foreach (var d in Directions)
                        {
                            Vector3Int neighborIdx = currIdx + d;

                            if (neighborIdx.x < m_LayoutMinX || neighborIdx.x > m_LayoutMaxX ||
                                neighborIdx.y < m_LayoutMinY || neighborIdx.y > m_LayoutMaxY)
                            {
                                isLeaking = true;
                                break;
                            }

                            if (m_VisitedInternalSetCache.Contains(neighborIdx))
                            {
                                continue;
                            }

                            var neighborNode = GetNodeByIndex(neighborIdx);
                            if (neighborNode != null && m_LineNodesSetCache.Contains(neighborNode))
                            {
                                continue;
                            }

                            m_VisitedInternalSetCache.Add(neighborIdx);
                            m_LocalFillQueueCache.Enqueue(neighborIdx);
                        }

                        if (isLeaking)
                        {
                            break;
                        }
                    }

                    if (isLeaking)
                    {
                        foreach (var idx in m_VisitedInternalSetCache)
                        {
                            m_CheckedSeedsSetCache.Add(idx);
                        }
                    }
                    else
                    {
                        foreach (var idx in m_VisitedInternalSetCache)
                        {
                            totalInternalIndices.Add(idx);
                            m_CheckedSeedsSetCache.Add(idx);
                        }
                    }
                }
                current = current.nextNode;
            }

            foreach (var idx in totalInternalIndices)
            {
                var node = GetNodeByIndex(idx);
                if (node != null && !m_LineNodesSetCache.Contains(node))
                {
                    internalPoints.Add(node);
                }
            }

            return internalPoints.Count > 0;
        }
        private bool IsLinkSingleArrowSuc(PointGenerateNode curNode, out ArrowGenerateNode arroNode)
        {
            int lenght = curNode.GetLineLength();
            arroNode = null;
            if (lenght < pressure.minLineLength || !IsDirMoveAble(curNode.GetHeadNode(), curNode.GetRootNode().moveDirection))
            {
                return false;
            }
            arroNode = new ArrowGenerateNode
            {
                arrowId = m_CurArrowId,
                rootNode = curNode.GetRootNode(),
            };
            m_ArrowGenerateNodes.Add(arroNode);
            m_Id2ArrowGenerateNodes.Add(arroNode.arrowId, arroNode);
            m_ConfirmedLineIds.Add(arroNode.arrowId);
            return true;
        }
        private void UpdateArrowCollisionMap(bool onlyConfirmedLine = true, int unstabeArrowId = -1, List<HashSet<int>> unstabeHitList = null)
        {
            m_ArrowCollisionMap = GetArrowCollisionMap(m_ArrowCollisionMap, onlyConfirmedLine, unstabeArrowId, unstabeHitList);
        }
        private readonly Queue<int> m_CircleCheckQueuCache = new Queue<int>(16);
        private readonly HashSet<int> m_CircleCheckVisitedCache = new HashSet<int>(64);
        private bool IsInCircle(Dictionary<int, HashSet<int>> dict, int myArrowId, int occId)
        {
            m_CircleCheckQueuCache.Clear();
            m_CircleCheckVisitedCache.Clear();

            m_CircleCheckQueuCache.Enqueue(occId);
            m_CircleCheckVisitedCache.Add(occId);

            while (m_CircleCheckQueuCache.Count > 0)
            {
                int currentId = m_CircleCheckQueuCache.Dequeue();

                if (dict.TryGetValue(currentId, out HashSet<int> nextIds) && nextIds != null)
                {
                    foreach (int nextId in nextIds)
                    {
                        if (nextId == myArrowId)
                        {
                            if (enableLog)
                            {
                                Log.Info($"检测失败, 线条产生了循环遮挡 {currentId} 挡住了 目标当前线条  {myArrowId}");
                            }
                            return true;
                        }

                        if (m_CircleCheckVisitedCache.Add(nextId))
                        {
                            m_CircleCheckQueuCache.Enqueue(nextId);
                        }
                    }
                }
            }

            return false;
        }
        private readonly Dictionary<int, HashSet<int>> _hitIdCache = new Dictionary<int, HashSet<int>>();
        private List<HashSet<int>> m_UnStableHitList = new List<HashSet<int>>();
        private Dictionary<int, HashSet<int>> GetArrowCollisionMap(Dictionary<int, HashSet<int>> collisionMap = null, bool onlyConfirmedLine = true, int unstabeArrowId = -1, List<HashSet<int>> unstabeHitList = null)
        {
            if (collisionMap == null)
            {
                collisionMap = new Dictionary<int, HashSet<int>>();
            }
            else
            {
                collisionMap.Clear();
            }
            if (unstabeArrowId >= 1)
            {
                unstabeHitList.Clear();
            }

            foreach (var arrow in m_ArrowGenerateNodes)
            {
                if (!_hitIdCache.TryGetValue(arrow.arrowId, out var hitSet))
                {
                    hitSet = new HashSet<int>(layout.GetMaxShapeLength());
                    _hitIdCache[arrow.arrowId] = hitSet;
                }
                GetArrowMoveDirHitOtherId(arrow, onlyConfirmedLine, hitSet, unstabeArrowId, unstabeHitList);
                if (hitSet.Count > 0)
                {
                    collisionMap[arrow.arrowId] = hitSet;
                    if (arrow.arrowId == 99)
                    {
                        var a = 1;
                    }
                }
            }
            return collisionMap;
        }

        private Vector3Int GetNodeMoveDir(PointGenerateNode pointNode, Vector2Int moveAbleFlag, out bool isHead)
        {
            int flag = 0;
            isHead = false;

            if (!pointNode.isRootNode)
            {
                flag = moveAbleFlag.x;
            }
            else if (pointNode.useCustomDir)
            {
                if (pointNode.customNodeType == NodeType.Body)
                {
                    flag = moveAbleFlag.x;
                }
                else
                {
                    flag = moveAbleFlag.y;
                    isHead = true;
                }
            }
            else
            {
                m_PointAxisPool.Clear();
                if (moveAbleFlag.x > 0) m_PointAxisPool.Add((NodeType.Body, moveAbleFlag.x));
                if (moveAbleFlag.y > 0) m_PointAxisPool.Add((NodeType.Head, moveAbleFlag.y));

                if (m_PointAxisPool.Count > 0)
                {
                    var selected = m_PointAxisPool[GetRdmRange(0, m_PointAxisPool.Count)];
                    if (selected.type == NodeType.Body)
                    {
                        flag = moveAbleFlag.x;
                        isHead = false;
                    }
                    else
                    {
                        flag = moveAbleFlag.y;
                        isHead = true;
                    }
                }
            }


            m_PointStepAbleDirs.Clear();
            // 使用位运算检查 flag
            if ((flag & (1 << 1)) != 0) m_PointStepAbleDirs.Add(Vector3Int.up);
            if ((flag & (1 << 2)) != 0) m_PointStepAbleDirs.Add(Vector3Int.right);
            if ((flag & (1 << 3)) != 0) m_PointStepAbleDirs.Add(Vector3Int.down);
            if ((flag & (1 << 4)) != 0) m_PointStepAbleDirs.Add(Vector3Int.left);

            if (m_PointStepAbleDirs.Count > 0)
            {
                return GetNodePriorityDir(pointNode, m_PointStepAbleDirs);
            }

            return Vector3Int.zero;
        }
        private List<Vector3Int> m_MaxPriorityDirsCache = new List<Vector3Int>(4);


        private Vector3Int GetNodePriorityDir(PointGenerateNode pointNode, List<Vector3Int> ableDirs)
        {
            if (ableDirs == null || ableDirs.Count == 0)
            {
                return Vector3Int.zero;
            }

            int maxPriority = -1;
            bool isAllSamePriority = true;
            m_MaxPriorityDirsCache.Clear();
            for (int i = 0; i < ableDirs.Count; i++)
            {
                var curDir = ableDirs[i];
                var index = pointNode.index + curDir;
                int priority = CalculatePointSearchPriority(pointNode, index);
                // Log.Info($"$节点寻路优先级  当前查线{pointNode.occupiedArrowId} 当前节点id:{pointNode.pointNode.id} 寻路方向{curDir} 优先级 {priority} 是否触发{pointNode.traceTriggerd}");
                if (i == 0)
                {
                    maxPriority = priority;
                    m_MaxPriorityDirsCache.Add(curDir);
                    continue;
                }

                if (priority != maxPriority)
                {
                    isAllSamePriority = false;
                }

                if (priority > maxPriority)
                {
                    maxPriority = priority;
                    m_MaxPriorityDirsCache.Clear();
                    m_MaxPriorityDirsCache.Add(curDir);
                }
                else if (priority == maxPriority)
                {
                    m_MaxPriorityDirsCache.Add(curDir);
                }
            }

            if (isAllSamePriority)
            {
                int randomIndex = GetRdmRange(0, ableDirs.Count);
                return ableDirs[randomIndex];
            }

            if (m_MaxPriorityDirsCache.Count > 0)
            {
                int randomIndex = GetRdmRange(0, m_MaxPriorityDirsCache.Count);
                return m_MaxPriorityDirsCache[randomIndex];
            }

            return Vector3Int.zero;
        }
        private bool IsNearHasUnOccupiedNode(PointGenerateNode pointNode)
        {
            foreach (var curDir in Directions)
            {
                var index = pointNode.index + curDir;
                var neighborNode = GetNodeByIndex(index);
                if (neighborNode != null && !neighborNode.isOccupied)
                {
                    return true;
                }
            }
            return false;
        }

        private int GetNearNodeHasOccupiedNum(PointGenerateNode pointNode)
        {
            int occNum = 0;
            foreach (var curDir in Directions)
            {
                var index = pointNode.index + curDir;
                var neighborNode = GetNodeByIndex(index);
                if (neighborNode != null && neighborNode.isOccupied)
                {
                    occNum++;
                }
            }
            return occNum;
        }
        /// <summary>
        ///  计算每一步点移动的对应的每一个方向的优先级
        /// </summary>
        /// <param name="pointNode"></param>
        /// <param name="nextIndex"></param>
        /// <returns></returns>
        private int CalculatePointSearchPriority(PointGenerateNode pointNode, Vector3Int nextIndex)
        {
            int priority = 0;
            priority += CalculatePointSearchTargetPriority(pointNode, nextIndex);
            priority += CalculatePointSearchMoveriority(pointNode, nextIndex);
            return priority;
        }
        public static bool IsPointNextStepTurnDir(PointGenerateNode pointNode, Vector3Int nextIndex)
        {
            if (pointNode == null || pointNode.prevNode == null) return false;
            Vector3Int nextMoveDir = nextIndex - pointNode.index;
            Vector3Int curMoveDir = pointNode.index - pointNode.prevNode.index;
            bool isTurn = nextMoveDir.Dot(curMoveDir) == 0;
            return isTurn;
        }
        /// <summary>
        /// 计算移动路径优先级
        /// </summary>
        /// <param name="pointNode"></param>
        /// <param name="nextIndex"></param>
        /// <returns></returns>
        private int CalculatePointSearchMoveriority(PointGenerateNode pointNode, Vector3Int nextIndex)
        {
            if (pointNode.prevNode == null)
            {
                return 0;
            }

            bool isTurn = IsPointNextStepTurnDir(pointNode, nextIndex);
            bool hasTurnChance = pointNode.GetRootNode().lineTurnCount < pressure.maxTurnsPerLine;

            // 如果没有拐弯次数了，但当前方向是拐弯，直接给负分（或不加分），让直行胜出
            if (isTurn && !hasTurnChance)
            {
                return -300;
            }

            // 还有拐弯次数，或者当前是直行，再根据倾向计算概率
            float rdm = GetRdmRange(0f, 1f);
            if (rdm < pressure.turnTendency)
            {
                if (isTurn) return 150;
            }
            else
            {
                if (!isTurn) return 150;
            }

            return 0;
        }
        /// <summary>
        /// 计算下一个格子的优先级
        /// </summary>
        /// <param name="pointNode"></param>
        /// <param name="nextIndex"></param>
        /// <returns></returns>
        private int CalculatePointSearchTargetPriority(PointGenerateNode pointNode, Vector3Int nextIndex)
        {
            int priority = 0;
            var nextIndexNode = GetNodeByIndex(nextIndex);
            if (nextIndexNode != null)
            {
                priority += 10; // 有格子就加1000
                // 获取格子周围的数量
                int occNum = GetNearNodeHasOccupiedNum(nextIndexNode);
                priority += 100 * occNum;
            }
            return priority;
        }



        private Vector2Int GetPointGenerateAbleDirFlag(PointGenerateNode pointGenerateNode, bool bodyDetctHit = false)
        {
            // 如果是身体的话,那么只才采用身体判断方向
            int headDir = pointGenerateNode.isRootNode ? GetPointGenerateAbleDirFlagFromHead(pointGenerateNode) : 0;
            return new Vector2Int(GetPointGenerateAbleDirFlagFromBody(pointGenerateNode, bodyDetctHit), headDir);
        }
        private int GetPointGenerateAbleDirFlagFromHead(PointGenerateNode pointNode)
        {
            // 需要判断当前是否可以作为头部生成
            int top = IsNearPointMoveAble(pointNode, Vector3Int.up) ? (1 << 1) : 0;
            int right = IsNearPointMoveAble(pointNode, Vector3Int.right) ? (1 << 2) : 0;
            int down = IsNearPointMoveAble(pointNode, Vector3Int.down) ? (1 << 3) : 0;
            int left = IsNearPointMoveAble(pointNode, Vector3Int.left) ? (1 << 4) : 0;
            if (pointNode.isRootNode)
            {
                // 需要判断,连接该点后,线头的朝向方向是否可走   
                if (top > 0 && !IsDirMoveAble(pointNode, Vector3Int.down))
                {
                    top = 0;
                }
                if (right > 0 && !IsDirMoveAble(pointNode, Vector3Int.left))
                {
                    right = 0;
                }
                if (down > 0 && !IsDirMoveAble(pointNode, Vector3Int.up))
                {
                    down = 0;
                }
                if (left > 0 && !IsDirMoveAble(pointNode, Vector3Int.right))
                {
                    left = 0;
                }
            }

            int flag = (top | right | down | left);
            return flag & ~(pointNode.blackMoveFlag);
        }
        private int GetPointGenerateAbleDirFlagFromBody(PointGenerateNode pointNode, bool bodyDetctHit = false)
        {
            // 需要判断当前是否可以作为尾部生成
            int top = IsNearPointMoveAble(pointNode, Vector3Int.up) ? (1 << 1) : 0;
            int right = IsNearPointMoveAble(pointNode, Vector3Int.right) ? (1 << 2) : 0;
            int down = IsNearPointMoveAble(pointNode, Vector3Int.down) ? (1 << 3) : 0;
            int left = IsNearPointMoveAble(pointNode, Vector3Int.left) ? (1 << 4) : 0;
            if (bodyDetctHit)
            {
                // 需要判断,连接该点后,线头的朝向方向是否可走   
                if (top > 0 && !IsDirMoveAble(pointNode, Vector3Int.up))
                {
                    top = 0;
                }
                if (right > 0 && !IsDirMoveAble(pointNode, Vector3Int.right))
                {
                    right = 0;
                }
                if (down > 0 && !IsDirMoveAble(pointNode, Vector3Int.down))
                {
                    down = 0;
                }
                if (left > 0 && !IsDirMoveAble(pointNode, Vector3Int.left))
                {
                    left = 0;
                }
            }
            int flag = (top | right | down | left);
            return flag & ~(pointNode.blackMoveFlag);
        }
        /// <summary>
        /// 判断该方向上能不能走
        /// </summary>
        /// <param name="fromNode"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        private bool IsNearPointMoveAble(PointGenerateNode fromNode, Vector3Int dir)
        {
            var nextIndex = fromNode.index + dir;
            var node = GetNodeByIndex(nextIndex);
            if (node == null)
            {
                return false;
            }
            return !node.isOccupied;
        }
        private bool IsInCircle(PointGenerateNode fromNode, PointGenerateNode occPoint)
        {
            if (fromNode.occupiedArrowId == occPoint.occupiedArrowId) // 自身相交
            {
                return true;
            }
            // 是否处于遮挡循环链上
            // var dict = GetArrowCollisionMap();
            return IsInCircle(m_ArrowCollisionMap, fromNode.occupiedArrowId, occPoint.occupiedArrowId);


        }
        public HashSet<int> GetArrowMoveDirHitOtherId(ArrowGenerateNode pointNode, bool onlyConfirmedLine = true, HashSet<int> outHitIds = null, int unstabeArrowId = -1, List<HashSet<int>> unstabeHitList = null)
        {
            PointGenerateNode headNode = pointNode.GetHeadNode();
            var moveDir = pointNode.rootNode.moveDirection;
            if (moveDir == Vector3Int.zero)
            {
                return null;
            }
            outHitIds?.Clear();
            HashSet<int> hitIds = outHitIds ?? new HashSet<int>(layout.GetMaxShapeLength());
            int moveX = moveDir.x;
            int moveY = moveDir.y;
            int moveZ = moveDir.z;
            int currentIdxX = headNode.index.x + moveX;
            int currentIdxY = headNode.index.y + moveY;
            int currentIdxZ = headNode.index.z + moveZ;

            // Vector3Int currentIdx = headNode.index + moveDir;
            bool isUnStabeAddMark = false;

            while (currentIdxX >= layout.MinX && currentIdxX <= layout.MaxX &&
                  currentIdxY >= layout.MinY && currentIdxY <= layout.MaxY)
            {
                if (TryDetectPointOccupied(currentIdxX, currentIdxY, currentIdxZ, out PointGenerateNode occPoint) && (!onlyConfirmedLine || HasLineConfirmed(occPoint.occupiedArrowId)))
                {
                    hitIds.Add(occPoint.occupiedArrowId);
                    if (unstabeArrowId >= 0 && occPoint.occupiedArrowId == unstabeArrowId && unstabeHitList != null && !isUnStabeAddMark)
                    {
                        isUnStabeAddMark = true;
                        unstabeHitList.Add(hitIds);
                    }
                }
                currentIdxX += moveX;
                currentIdxY += moveY;
            }
            return hitIds;
        }
        // public HashSet<int> GetArrowMoveDirHitOtherId(ArrowGenerateNode pointNode, bool onlyConfirmedLine = true, HashSet<int> outHitIds = null)
        // {
        //     PointGenerateNode headNode = pointNode.GetHeadNode();
        //     var moveDir = pointNode.rootNode.moveDirection;
        //     if (moveDir == Vector3Int.zero)
        //     {
        //         return null;
        //     }
        //     outHitIds?.Clear();
        //     HashSet<int> hitIds = outHitIds ?? new HashSet<int>(layout.GetMaxShapeLength());
        //     Vector3Int currentIdx = headNode.index + moveDir;
        //     while (currentIdx.x >= layout.MinX && currentIdx.x <= layout.MaxX &&
        //           currentIdx.y >= layout.MinY && currentIdx.y <= layout.MaxY)
        //     {
        //         if (TryDetectPointOccupied(currentIdx, out PointGenerateNode occPoint) && (!onlyConfirmedLine || HasLineConfirmed(occPoint.occupiedArrowId)))
        //         {
        //             hitIds.Add(occPoint.occupiedArrowId);
        //         }
        //         currentIdx += moveDir;
        //     }
        //     return hitIds;
        // }
        private bool HasLineConfirmed(int arrowId)
        {
            return m_ConfirmedLineIds.Contains(arrowId);
        }
        private bool IsDirMoveAble(PointGenerateNode pointNode, Vector3Int dir)
        {
            Vector3Int currentIdx = pointNode.index + dir;
            if (dir == Vector3Int.zero)
            {
                return !TryDetectPointOccupied(currentIdx, out PointGenerateNode occPoint);
            }

            while (currentIdx.x >= layout.MinX && currentIdx.x <= layout.MaxX &&
                   currentIdx.y >= layout.MinY && currentIdx.y <= layout.MaxY)
            {
                if (TryDetectPointOccupied(currentIdx, out PointGenerateNode occPoint))
                {
                    if (IsInCircle(pointNode, occPoint))
                    {
                        // 自身相交
                        return false;
                    }
                }
                currentIdx += dir;
            }

            return true;
        }
        public bool TryDetectPointOccupied(int x, int y, int z, out PointGenerateNode occPoint)
        {
            var findPoint = GetNodeByIndex(x, y, z);
            occPoint = null;
            if (findPoint != null && findPoint.isOccupied)
            {
                occPoint = findPoint;
                return true;
            }
            return false;
        }
        public bool TryDetectPointOccupied(Vector3Int nodeIndex, out PointGenerateNode occPoint)
        {
            var findPoint = GetNodeByIndex(nodeIndex);
            occPoint = null;
            if (findPoint != null && findPoint.isOccupied)
            {
                occPoint = findPoint;
                return true;
            }
            return false;
        }

        private bool IsRootVisited(PointGenerateNode pointNode)
        {
            return m_RootVisitedMap.Contains(pointNode);
        }
        private List<PointGenerateNode> m_UnoccupiedPoints = new List<PointGenerateNode>(128);
        /// <summary>
        /// 从边缘地区获取一个点作为开始点
        /// </summary>
        /// <returns></returns>
        private PointGenerateNode GetBorderUnVisitedUnoccupiediedNode()
        {
            // List<PointGenerateNode> m_UnoccupiedPoints = new List<PointGenerateNode>();
            m_UnoccupiedPoints.Clear();
            foreach (var item in m_BorderNodes)
            {
                var node = GetNodeByIndex(item.index);
                if (!node.isOccupied && !IsRootVisited(node))
                {
                    m_UnoccupiedPoints.Add(node);
                }
            }
            if (m_UnoccupiedPoints.Count == 0)
            {
                return null;
            }

            int randomIndex = GetRdmRange(0, m_UnoccupiedPoints.Count);
            var point = m_UnoccupiedPoints[randomIndex];

            return point;
        }
        private PointGenerateNode GetRootUnVisitedUnoccupiediedNode()
        {
            var unoccupiedPoints = pointToGenerateMap.Values
           .Where(node => !node.isOccupied && !IsRootVisited(node))
           .ToList();

            if (unoccupiedPoints.Count == 0)
            {
                return null;
            }

            int randomIndex = GetRdmRange(0, unoccupiedPoints.Count);
            var point = unoccupiedPoints[randomIndex];

            point.occupiedArrowId = m_CurArrowId;
            point.isRootNode = true;
            point.SetAsEndNode();
            m_RootVisitedMap.Add(point);
            return point;
        }
        private PointGenerateNode GetRandomUnoccupiediedPoint()
        {
            var unoccupiedPoints = pointToGenerateMap.Values
            .Where(node => !node.isOccupied && !IsRootVisited(node))
            .ToList();

            if (unoccupiedPoints.Count == 0)
            {
                return null;
            }

            int randomIndex = GetRdmRange(0, unoccupiedPoints.Count);
            return unoccupiedPoints[randomIndex];
        }
        public List<Vector3Int> GetUnOccupiedNodeIndexs()
        {
            List<Vector3Int> list = new List<Vector3Int>(pointToGenerateMap.Values.Count);
            foreach (var item in pointToGenerateMap)
            {
                if (!item.Value.isOccupied)
                {
                    list.Add(item.Value.pointNode.index);
                }

            }
            return list;
        }
        public List<int> GetUnOccupiedGenerateNodeIds()
        {
            List<int> list = new List<int>(pointToGenerateMap.Values.Count);
            foreach (var item in pointToGenerateMap)
            {
                if (!item.Value.isOccupied)
                {
                    list.Add(item.Key);
                }

            }
            return list;
        }
        public List<int> GetOccupiedNodeIds()
        {
            List<int> list = new List<int>(pointToGenerateMap.Values.Count);
            foreach (var item in pointToGenerateMap)
            {
                if (item.Value.isOccupied)
                {
                    list.Add(item.Value.pointNode.id);
                }

            }
            return list;
        }
    }

}
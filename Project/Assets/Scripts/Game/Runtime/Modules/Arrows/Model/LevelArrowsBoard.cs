using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using CustomLitJson.Extensions;

namespace Game.Modules.GModuleArrows
{
    public class LevelArrowStatus
    {
        public const int Status_Enable = 0;
        public const int Status_Disable = 1;
        public const int Status_Passed = 2;
    }
    public class LevelArrowNode
    {
        // [JsonIgnore]
        // public const int Arrow_Status_Enable = 0;
        // [JsonIgnore]
        // public const int Arrow_Status_Disable = 1;
        [JsonSerializer]
        private int m_Status = LevelArrowStatus.Status_Enable;
        [JsonSerializer]
        private int m_Id;
        [JsonSerializer]

        private List<Vector3Int> m_OccupiedPointIndexs = new List<Vector3Int>();
        [JsonSerializer]
        private Vector3 m_MoveDirection;
        [JsonSerializer]
        private List<Vector3> m_PathPoints = new List<Vector3>();
        [JsonSerializer]
        private bool m_IsHasSolveDeep;
        [JsonIgnore]
        public int Id => m_Id;
        [JsonIgnore]
        public List<Vector3Int> occupiedPointIndexs => m_OccupiedPointIndexs;
        [JsonIgnore]
        public Vector3 MoveDirection => m_MoveDirection;
        [JsonIgnore]
        public List<Vector3> PathPoints => m_PathPoints;

        [JsonIgnore]
        public bool isHasSolveDeep => m_IsHasSolveDeep;

        public LevelArrowNode()
        {

        }
        public LevelArrowNode(int id, List<Vector3Int> pointIds, Vector3 direction, List<Vector3> positions, bool isHasSolveDeep)
        {
            this.m_Id = id;
            this.m_OccupiedPointIndexs = new List<Vector3Int>(pointIds);
            this.m_MoveDirection = direction;
            this.m_PathPoints = new List<Vector3>(positions);
            this.m_IsHasSolveDeep = isHasSolveDeep;
        }


        public void SetStatus(int status)
        {
            m_Status = status;
        }
        public int GetStatus()
        {
            return m_Status;

        }
        public bool IsEnable()
        {
            return GetStatus() == LevelArrowStatus.Status_Enable;
        }
    }

    public class LevelArrowsBoard
    {
        private List<LevelArrowNode> m_ActiveArrows = new List<LevelArrowNode>();
        private List<LevelArrowNode> m_AllArrows = new List<LevelArrowNode>();
        private Dictionary<int, LevelArrowNode> idToArrowMap = new Dictionary<int, LevelArrowNode>();

        public List<LevelArrowNode> GetActivedArrows() => m_ActiveArrows;
        public List<LevelArrowNode> GetAllArrows() => m_AllArrows;

        public LevelArrowNode GetArrowByPointId(int pointId)
        {
            idToArrowMap.TryGetValue(pointId, out var arrow);
            return arrow;
        }

        private class GridCell
        {
            public LevelPointNode rawNode;
            public int x;
            public int y;
            public bool isOccupied;
        }

        private static readonly Vector2Int[] Directions = {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
        };
        private string m_ArrowLayoutId;
        public string ArrowLayoutId => m_ArrowLayoutId;


        public bool SetupBoard(LevelPointLayout layout, LevelArrowsPresure pressure)
        {
            LevelArrowsBoardGeneator geneator = new LevelArrowsBoardGeneator();
            geneator.layout = layout;
            geneator.pressure = pressure;
            SetupBoardArrows(geneator.GenerateBoardArrows());
            layout.SetAllNodesOccupied(true);
            layout.SetNodesOccupied(geneator.GetUnOccupiedNodeIndexs(), false);
            return m_ActiveArrows.Count > 0;

        }
        public void SetArrowLayoutId(string arrowLayoutId)
        {
            m_ArrowLayoutId = arrowLayoutId;
        }
        public void ReEnableAllArrows()
        {
            m_ActiveArrows.Clear();
            foreach (var arrow in m_AllArrows)
            {
                arrow.SetStatus(LevelArrowStatus.Status_Enable);
                if (arrow.IsEnable())
                {
                    m_ActiveArrows.Add(arrow);
                }
            }
        }
        public void SetupBoardArrows(List<LevelArrowNode> arrowNodes)
        {
            m_ActiveArrows.Clear();
            m_AllArrows.Clear();
            m_AllArrows.AddRange(arrowNodes);

            foreach (var arrow in arrowNodes)
            {
                if (arrow.IsEnable())
                {
                    m_ActiveArrows.Add(arrow);
                }
                idToArrowMap.Add(arrow.Id, arrow);
            }
            // m_Arrows = arrowNodes;
            // layout.SetAllNodesOccupied(false);
            // foreach (var arrowNode in arrowNodes)
            // {
            //     foreach (var occupiedPointIndex in arrowNode.occupiedPointIndexs)
            //     {
            //         layout.SetNodesOccupied(occupiedPointIndex, true);
            //     }
            // }
        }
        public void DoBoardValidate(LevelPointLayout layout)
        {
            layout.SetAllNodesOccupied(false);
            foreach (var arrowNode in m_AllArrows)
            {
                foreach (var occupiedPointIndex in arrowNode.occupiedPointIndexs)
                {
                    layout.SetNodesOccupied(occupiedPointIndex, true);
                }
            }

            layout.SetAllNodesOccupyRemoved(true);
            foreach (var arrowNode in m_ActiveArrows)
            {
                foreach (var occupiedPointIndex in arrowNode.occupiedPointIndexs)
                {
                    layout.SetNodesOccupyRemoved(occupiedPointIndex, false);
                }
            }
        }

        public void ClearBoard()
        {
            m_ArrowLayoutId = string.Empty;
            m_ActiveArrows.Clear();
            idToArrowMap.Clear();
        }
    }
}

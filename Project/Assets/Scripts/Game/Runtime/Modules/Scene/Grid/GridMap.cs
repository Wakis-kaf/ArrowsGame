using Framework.Runtime.LogSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Modules.GModuleScene
{
    [Serializable]
    public class GridCellMeta
    {
        public int index;
        public List<int> labelTypeIds = new List<int>();
        public List<PropertyKV> properties = new List<PropertyKV>();
        public int regionId = -1;
        public Vector2Int v2Index;

        public static GridCellMeta Create(int cellIndex, Vector2Int v2Index, List<int> labelIds = null, List<PropertyKV> properties = null, int regionId = -1)
        {
            return new GridCellMeta
            {
                index = cellIndex,
                v2Index = v2Index,
                labelTypeIds = labelIds ?? new List<int>(),
                properties = properties ?? new List<PropertyKV>(),
                regionId = regionId
            };
        }
    }

    public class GridMap : MonoBehaviour
    {
        public TextAsset gridMetaDataJson;
        public GridTile gridTile;
        public GameObject goDoorOpen;
        public GameObject goDoorClose;
        public GameObject goDoorGuide;
        private GridMeta m_GM;

        public void SetDoorGuide(bool isVisible)
        {
            goDoorGuide.SetActive(isVisible);
            StartDoorGuideAnim();
        }
        private void StartDoorGuideAnim()
        {
            StopDoorGuideAnim();
        }
        private void StopDoorGuideAnim()
        {

        }
        public void SetDoorOpen(bool isOpen)
        {
            goDoorOpen.SetActive(isOpen);
            goDoorClose.SetActive(!isOpen);
        }
        public GridMeta GetGridMeta()
        {
            if (m_GM != null) return m_GM;
            if (gridMetaDataJson == null)
            {
                Log.Error("网格元数据缺失");
                return null;
            }
            var gm = JsonUtility.FromJson<GridMeta>(gridMetaDataJson.text);
            m_GM = gm;
            return gm;
        }
    }

    [Serializable]
    public class GridMeta
    {
        public Vector3Int cellCount;
        public Vector3 cellSize;
        public Vector3 gap;
        public string gridName;
        public int gridRadius;
        public int layout;
        public int orientation;
        public List<GridRegion> regions = new List<GridRegion>();
        public int swizzle;
        private Dictionary<int, GridCellMeta> m_GridCellMetaDict = new Dictionary<int, GridCellMeta>();
        public void ResetCellMetaSize(int size)
        {
            m_GridCellMetaDict.EnsureCapacity(size);
        }
        public GridCellMeta FindGridCellMetaByIndex(int index)
        {
            if (m_GridCellMetaDict.TryGetValue(index, out var meta)) return meta;
            for (int i = 0; i < regions.Count; i++)
            {
                var cells = regions[i].cells;
                for (int j = 0; j < cells.Count; j++)
                {
                    if (!m_GridCellMetaDict.ContainsKey(cells[j].index))
                    {
                        m_GridCellMetaDict.Add(cells[j].index, cells[j]);
                    }

                    if (cells[j].index == index)
                    {
                        return cells[j];
                    }
                }
            }
            return null;
        }

        public GridRegion GetRegionByType(int type)
        {
            for (int i = 0; i < regions.Count; i++)
            {
                if (regions[i].regionTypeId == type)
                {
                    return regions[i];
                }
            }
            return null;
        }

        public List<GridRegion> GetRegionsByType(int type)
        {
            List<GridRegion> result = new List<GridRegion>();
            for (int i = 0; i < regions.Count; i++)
            {
                if (regions[i].regionTypeId == type)
                {
                    result.Add(regions[i]);
                }
            }
            return result;
        }
    }

    [Serializable]
    public class PropertyKV
    { public string key; public string value; }
}
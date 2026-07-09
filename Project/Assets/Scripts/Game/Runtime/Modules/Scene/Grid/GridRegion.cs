using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Modules.GModuleScene
{
    [Serializable]
    public class GridRegion
    {
        public List<GridCellMeta> cells = new List<GridCellMeta>();
        public Color color = Color.white;
        public int id = -1;
        public string name = "New Region";
        public int regionTypeId = -1;

        public GridRegion(int id, string name = "New Region")
        {
            this.id = id;
            this.name = name;
            this.color = Color.HSVToRGB(UnityEngine.Random.Range(0f, 1f), 0.7f, 1f);
        }

        // 添加这个构造函数以确保JSON序列化正常工作
        public GridRegion()
        { }

        public GridCellMeta FindGridCellByIndex(int index)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].index == index)
                {
                    return cells[index];
                }
            }
            return null;
        }

        public List<GridCellMeta> GetGridCellMetaByTag(int tag)
        {
            List<GridCellMeta> list = new List<GridCellMeta>();
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].labelTypeIds.Contains(tag))
                {
                    list.Add(cells[i]);
                }
            }
            return list;
        }
    }

    public class GridRegionType
    {
        public const int DefaultRegion = -1;
        public const int RoomRegion = 0;
    }
}
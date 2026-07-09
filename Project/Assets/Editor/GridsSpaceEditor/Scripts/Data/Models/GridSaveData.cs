using System;
using System.Collections.Generic;

namespace GridsSpaceEditor.Data.Models
{
    [Serializable]
    public class GridSaveData
    {
        public List<GridCellData> Cells = new List<GridCellData>();
    }
}

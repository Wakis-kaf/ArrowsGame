using System;
using System.Collections.Generic;
using UnityEngine;

namespace GridsSpaceEditor.Data.Models
{
    [Serializable]
    public class GridCellData
    {
        public string Name;
        public string Type;
        public string Description;
        public Vector2Int Coordinates;
        public List<PortInstance> Ports = new List<PortInstance>();

        public GridCellData Clone()
        {
            return new GridCellData
            {
                Name = this.Name,
                Type = this.Type,
                Description = this.Description,
                Coordinates = this.Coordinates,
                Ports = new List<PortInstance>(this.Ports)
            };
        }
    }
}

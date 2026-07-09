using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleAI
{
    public class AstarNodeManager
    {
        public AStarNode[] nodeArray = new AStarNode[0];

        public AstarNodeManager()
        {
        }

        public void Clear()
        {
            nodeArray = new AStarNode[0];
        }

        public AStarNode FindNearestPoint(float x, float y, float z)
        {
            AStarNode nearestPoint = null;
            float nearestDistanceSq = float.MaxValue;

            foreach (AStarNode point in nodeArray)
            {
                float distanceSq = SquaredDistance(x, y, z, point.posX, point.posY, point.posZ);
                if (distanceSq < nearestDistanceSq)
                {
                    nearestPoint = point;
                    nearestDistanceSq = distanceSq;
                }
            }

            return nearestPoint;
        }

        private float SquaredDistance(float x, float y, float z, float x2, float y2, float z2)
        {
            float dx = x2 - x;
            float dy = y2 - y;
            float dz = z2 - z;
            return dx * dx + dy * dy + dz * dz;
        }
    }
}
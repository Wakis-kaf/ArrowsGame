using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleAI
{
    public class AStartNodeLink
    {
        public AStarNode linedNode = null;
        public float cost = 0;
    }

    public class AStarNode
    {
        public bool walkAble = true;
        public int version = -1;
        public AStarNode parentNode = null;
        public float posX = 0;
        public float posY = 0;
        public float posZ = 0;
        private float m_g = 0;
        private float m_h = 0;
        private float m_f = 0;
        public List<AStartNodeLink> links;

        public AStarNode(float x, float y, float z, float g, float h, AStarNode parent)
        {
            posX = x;
            posY = y;
            posZ = z;
            parentNode = parent;
            m_g = g;
            m_h = h;
            m_f = m_g + m_h;
        }

        public float GetH()
        {
            return m_h;
        }

        public float GetF()
        {
            return m_f;
        }

        public float GetG()
        {
            return m_g;
        }

        public void SetH(float h)
        {
            m_h = h;
            m_f = m_g + m_h;
        }

        public void SetG(float g)
        {
            m_g = g;
            m_f = m_g + m_h;
        }

        public bool IsWalkAble()
        {
            return walkAble;
        }
    }
}
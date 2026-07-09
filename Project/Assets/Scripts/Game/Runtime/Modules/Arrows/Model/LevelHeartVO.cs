using UnityEngine;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
namespace Game.Modules.GModuleArrows
{
    public enum LevelHeartVOStatus
    {
        None,
        Alive,
        Death
    }
    public class LevelHeartVO : BaseVO
    {
        private LevelHeartVOStatus m_Status = LevelHeartVOStatus.Alive;
        private bool m_IsAnim = true;
        private Action<LevelHeartVOStatus> m_OnStatusChanged;
        public int order;

        public bool IsAnim => m_IsAnim;

        public void BindStatusChanged(Action<LevelHeartVOStatus> onChanged)
        {
            m_OnStatusChanged = onChanged;
            m_OnStatusChanged?.Invoke(m_Status);
        }
        public void SetDead(bool isAnim = true)
        {
            m_Status = LevelHeartVOStatus.Death;
            m_IsAnim = isAnim;
            m_OnStatusChanged?.Invoke(m_Status);
        }
        public void SetAlive(bool isAnim = true)
        {
            m_Status = LevelHeartVOStatus.Alive;
            m_IsAnim = isAnim;
            m_OnStatusChanged?.Invoke(m_Status);
        }
        public bool IsAlive()
        {
            return m_Status == LevelHeartVOStatus.Alive;
        }
        public bool IsDeath()
        {
            return m_Status == LevelHeartVOStatus.Death;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Runtime.MCombat
{
    public class CombatEffectManager
    {
        private struct EffectorRecord
        {
            public EffectTag effectTag;
            public ICombatEffector combatEffector;
        }
        private List<EffectorRecord> m_Records;
        public CombatEffectManager()
        {
            m_Records = new List<EffectorRecord>();
        }
        public void CheckEffect(Combator combator, EffectTag effectTag)
        {
            for (int i = 0; i < m_Records.Count; i++)
            {
                var record = m_Records[i];
                var recordEffectTag = record.effectTag;
                if (recordEffectTag.tagName == effectTag.tagName && 
                    recordEffectTag.effectTagType == effectTag.effectTagType)
                {
                    record.combatEffector.DoEffect(combator, effectTag);
                }
            }
        }

        public void ClearAll()
        {
            m_Records?.Clear();
        }
    }
}

using Assets.Scripts.Framework.Runtime.Modules.Combat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Runtime.MCombat
{
    public class CombatEventPool: ObjectPool<CombatEvent>
    {
        private const string tag = "CombatEvent";
        public CombatEventPool()
        {
            CreateAndAddObjectModifier<CombatEventPoolModifier>("CombatEventPoolModifier");
        }
        public CombatEvent GetCombatEvent()
        {
            return GetOrCreateItem(tag);
        }
        public void PutCombatEvent(CombatEvent item)
        {
            PutOrDestroyItem(tag, item);
        }
    }
}

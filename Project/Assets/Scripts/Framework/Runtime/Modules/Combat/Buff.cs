using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Runtime.MCombat
{
    public abstract class Buff : Ability
    {
        protected override CombatEvent CheckAbilityLifeTimeEvent(CombatEvent combatEvent)
        {
             base.CheckAbilityLifeTimeEvent(combatEvent);
            if (combatEvent.IsCodeAndFromSender(CombatCode.OnBuffMerge, this))
            {
                OnBuffMerge(combatEvent);
            }
            return combatEvent;
        }
        public virtual void OnBuffMerge(CombatEvent combatEvent)
        {

        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Runtime.MCombat
{
    public interface ICombatEventDispatcher
    {
        public CombatEvent SendEventToTarget(string code, ICombatEventReceiver target);
        public CombatEvent SendEventToTargets(string code, IEnumerable<ICombatEventReceiver> targets);
        public CombatEvent SendEvent(CombatEvent combatEvent);
        public CombatEvent SendEventSelf(string evt);
    }
}

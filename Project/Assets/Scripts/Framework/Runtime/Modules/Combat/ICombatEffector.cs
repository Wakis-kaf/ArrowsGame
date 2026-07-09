using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Runtime.MCombat
{
    public interface ICombatEffector
    {
        public void DoEffect(Combator combator, EffectTag effectTag);
    }
}

using Framework.Runtime.MObjectPool.Core;
using Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Framework.Runtime.Modules.Combat
{
    public class CombatEventPoolModifier : ObjectModifier
    {
        public override string modifierName
        {
            get => "CombatEventPoolModifier";
            set { }
        }
        public override T OnCreate<T>(T item, IModifierData data)
        {
            return Utility.ReflectionUtil.CreateInstance<T>();
        }
    }
}

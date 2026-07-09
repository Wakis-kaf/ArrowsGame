

using Framework.Runtime.MObjectPool.Core;
using Framework.Utils;
using static Framework.Runtime.MCombat.CombatProtoPool;

namespace Framework.Runtime.MCombat
{
    public class CombatProtoPoolModifier : ObjectModifier
    {
        public override string modifierName
        {
            get => "CombatProtoPoolModifier";
            set { }
        }
        public override T OnCreate<T>(T item, IModifierData data)
        {
            if(data is ProtoGetOrCreateData createData)
            {
              return  Utility.ReflectionUtil.CreateInstance(createData.protoType) as T;
            }
            return Utility.ReflectionUtil.CreateInstance<T>();
        }
    }
}

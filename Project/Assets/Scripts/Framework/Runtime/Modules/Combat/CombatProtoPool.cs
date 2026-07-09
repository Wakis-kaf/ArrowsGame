using Assets.Scripts.Framework.Runtime.Modules.Combat;
using Framework.Runtime.MObjectPool.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Runtime.MCombat
{
    public class CombatProtoPool : ObjectPool<CombatProto>
    {
        internal class ProtoGetOrCreateData : IModifierData
        {
            public Type protoType;
        }
        public CombatProtoPool()
        {
            CreateAndAddObjectModifier<CombatProtoPoolModifier>("CombatProtoPoolModifier");
        }
        private ProtoGetOrCreateData m_GetOrCreateData = new ProtoGetOrCreateData();
        public T GetCombatProto<T>()where T : CombatProto
        {
            m_GetOrCreateData.protoType = typeof(T);
            string tag = m_GetOrCreateData.protoType.Name;
            return GetOrCreateItem(tag, m_GetOrCreateData,m_GetOrCreateData) as T;
        }
        public void PutCombatProto(CombatProto proto,string tag) 
        {
            PutOrDestroyItem(tag, proto);
        }
        public void PutCombatProto<T>(T proto) where T : CombatProto
        {
            string tag = proto.PoolTag;
            PutOrDestroyItem(tag, proto);
        }
    }
}

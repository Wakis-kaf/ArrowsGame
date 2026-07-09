using Framework.Runtime.MObjectPool.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Framework.Runtime.MCombat
{
    /// <summary>
    /// 战斗协议,传输自定义数据结构体
    /// </summary>
    public class CombatProto : IPoolElement
    {
        public bool IsInPool { get; set; }
        public Pool Pool { get; set; }
        private string m_PoolTag;
        public string PoolTag
        {
            get
            {
                if(m_PoolTag == null)
                {
                    m_PoolTag = GetType().Name;
                }
                return m_PoolTag;
            }
        }

        public virtual void OnCreateInPool()
        {
            
        }

        public virtual void OnDestroyByPool()
        {
            
        }

        public virtual void OnGetFromPool()
        {
            
        }

        public virtual void OnPrewarmInPool()
        {
            
        }

        public virtual void OnPutToPool()
        {
            
        }
        public virtual void Dispose()
        {
            (Pool as CombatProtoPool).PutCombatProto(this, PoolTag);
        }
        public static T GetProto<T>()where T : CombatProto
        {
            return GameApp.CombatSystem.CombatProtoPool.GetCombatProto<T>();
        }
        public static void PutProto<T>(T proto) where T : CombatProto
        {
            GameApp.CombatSystem.CombatProtoPool.PutCombatProto(proto);
        }
    }
}

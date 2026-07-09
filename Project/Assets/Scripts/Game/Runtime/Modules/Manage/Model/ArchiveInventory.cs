using CustomLitJson.Extensions;
using Game.Modules.GModuleInventory;
using Game.Modules.GModuleManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Modules.GModuleManage
{
    [Serializable]
    public class ArchiveInventory
    {
        [JsonIgnore]
        public GameArchive OwnArchive { get; internal set; }
        [JsonSerializer]
        private Dictionary<int, InvMeta> m_InvMeta;
        [JsonIgnore]
        public Dictionary<int, InvMeta> InvMeta => m_InvMeta;
        public ArchiveInventory()
        {
         
            m_InvMeta = new Dictionary<int, InvMeta>();
        }
        public bool TryGetInvMeta(int id,out InvMeta invMeta)
        {
            invMeta = default;
            if (m_InvMeta.ContainsKey(id))
            {
                invMeta = m_InvMeta[id];
                return true;
            }
            return false;
        }
        public void OnLoadFromArchive()
        {

        }

        public void SaveInventory(int id, InvMeta invMeta)
        {
            if (m_InvMeta.ContainsKey(id))
            {
                m_InvMeta[id] = invMeta;
            }
            else
            {
                m_InvMeta.Add(id, invMeta);
            }
            OwnArchive.MarkDirty();
        }

     
    }


}

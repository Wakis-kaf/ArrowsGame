using System;
using System.Collections.Generic;

namespace Framework.Runtime.MCombat
{
    // 提供给外部调用通道
    public class AbilityLink
    {
        public const string Data_Id = "Data_Id";
        public const string Event_OnFuncIconBeginDrag = "Event_OnFuncIconBeginDrag";
        public const string Event_OnFuncIconDraq = "Event_OnFuncIconDraq";
        public const string Event_OnFuncIconEndDrag = "Event_OnFuncIconEndDrag";
        public const string Event_OnFuncIconSelect = "OnFuncIconSelect";
        public const int LinkType_ActionSkill = 1;
        public const int LinkType_Func = 0;
        public const int LinkType_PassiveSkill = 2;
        private Dictionary<string, Action<object>> m_Cbs;
        private DataManager m_Data;
        private int m_LinkType;
        private Ability m_Owner;

        public AbilityLink()
        {
            m_Data = new DataManager();
            m_Cbs = new Dictionary<string, Action<object>>();
        }

        public DataManager Data => m_Data;
        public int LinkType => m_LinkType;
        public Ability Owner => m_Owner;

        public void AddListener(string evt, Action<object> cb)
        {
            if (m_Cbs.TryGetValue(evt, out var cbs))
            {
                cbs += cb;
            }
            else
            {
                m_Cbs.Add(evt, cb);
            }
        }

        public void Dispatch(string evt, object data)
        {
            if (m_Cbs.TryGetValue(evt, out var cbs))
            {
                cbs?.Invoke(data);
            }
        }

        public void RemoveListener(string evt, Action<object> cb)
        {
            if (m_Cbs.TryGetValue(evt, out var cbs))
            {
                cbs -= cb;
            }
        }

        public void SetLinkType(int linkType)
        {
            m_LinkType = linkType;
        }

        public void SetOwner(Ability owner)
        {
            m_Owner = owner;
        }
    }
}
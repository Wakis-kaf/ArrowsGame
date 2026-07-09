using CustomLitJson.Extensions;
using Framework.Runtime;
using Framework.Runtime.Archives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.Modules.GModuleManage
{
    [Serializable]
    public class ArchiveRole
    {
        [JsonSerializer]
        private int m_CurrentRoleId = -1;
        [JsonSerializer]
        private int m_CurrentRoleSkinId = -1;
        [JsonSerializer]
        private long m_GetAfkRewardTime = -1;
        [JsonSerializer]
        private int m_AfkHasCleanCount = 0;
        [JsonSerializer]
        private float m_MusicVolume = -1f;
        [JsonSerializer]
        private float m_EffectVolume = -1f;

        [JsonSerializer]
        private bool m_IsShakeOpen = true;
        [JsonSerializer]
        private Dictionary<int, int> m_RoleStatusMap;
        [JsonSerializer]
        private Dictionary<int, int> m_RoleBookMap;
        [JsonSerializer]
        private Dictionary<int, int> m_RoleLvMap;
        [JsonSerializer]
        private Dictionary<int, int> m_RoleStarMap;
        [JsonSerializer]
        private Dictionary<int, int> m_RoleEquipMap;
        [JsonSerializer]
        private Dictionary<int, int> m_RoleShopDayBuyMap;
        [JsonSerializer]
        private Dictionary<int, long> m_ItemRecoverMap;
        [JsonSerializer]
        private Dictionary<int, int> m_RoleItemStateMap;


        [JsonSerializer]
        private Dictionary<int, Dictionary<int, int>> m_RoleTalentActiveMap;

        [JsonIgnore]
        public Archive OwnArchive { get; set; }
        public ArchiveRole()
        {
            m_RoleShopDayBuyMap = new Dictionary<int, int>();
            m_RoleStatusMap = new Dictionary<int, int>();
            m_RoleBookMap = new Dictionary<int, int>();
            m_RoleLvMap = new Dictionary<int, int>();
            m_RoleStarMap = new Dictionary<int, int>();
            m_RoleEquipMap = new Dictionary<int, int>();
            m_ItemRecoverMap = new Dictionary<int, long>();
            m_RoleItemStateMap = new Dictionary<int, int>();
            m_RoleTalentActiveMap = new Dictionary<int, Dictionary<int, int>>();
        }
        public int GetRoleAfkHasCleanCount()
        {
            return m_AfkHasCleanCount;
        }
        public void SetRoleAfkHasCleanCount(int count)
        {
            m_AfkHasCleanCount = count;
            OwnArchive.MarkDirty();
        }
        public long GetRoleGetAfkRewardTime(long defaultTime)
        {
            if (m_GetAfkRewardTime <= 0)
            {
                return defaultTime;
            }
            return m_GetAfkRewardTime;
        }
        public long SetRoleGetAfkRewardTime(long time)
        {
            m_GetAfkRewardTime = time;
            OwnArchive.MarkDirty();
            return time;
        }
        public void SetEffectVolume(float volume)
        {
            m_EffectVolume = volume;
            OwnArchive.MarkDirty();
        }
        public float GetEffectVolume(float defaultVolume)
        {
            if (m_EffectVolume < 0)
            {
                return defaultVolume;
            }
            return m_EffectVolume;
        }
        public void ClearRoleItemState(int roleItemId)
        {
            m_RoleItemStateMap.Remove(roleItemId);
        }
        public int GetRoleItemState(int roleItemId, int defaultState)
        {
            if (m_RoleItemStateMap.ContainsKey(roleItemId)) { return m_RoleItemStateMap[roleItemId]; }
            return defaultState;
        }
        public void SetRoleItemState(int roleItemId, int state)
        {
            m_RoleItemStateMap[roleItemId] = state;
            OwnArchive.MarkDirty();
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_role_item_state_changed);
        }
        public void SetMusicVolume(float volume)
        {
            m_MusicVolume = volume;
            OwnArchive.MarkDirty();
        }
        public float GetMusicVolume(float defaultVolume)
        {
            if (m_MusicVolume < 0)
            {
                return defaultVolume;
            }
            return m_MusicVolume;
        }
        public int GetRoleShopBuyCount(int shopItemId, int defaultCount)
        {
            if (m_RoleShopDayBuyMap.ContainsKey(shopItemId)) { return m_RoleShopDayBuyMap[shopItemId]; }
            return defaultCount;
        }
        public void SetRoleShopBuyCount(int shopItemId, int buyCount)
        {
            if (m_RoleShopDayBuyMap.ContainsKey(shopItemId)) { m_RoleShopDayBuyMap[shopItemId] = buyCount; }
            else { m_RoleShopDayBuyMap.Add(shopItemId, buyCount); }
            OwnArchive.MarkDirty();
        }
        public int GetRoleBookStatus(int roleId, int defaultStauts)
        {
            if (m_RoleBookMap.ContainsKey(roleId)) { return m_RoleBookMap[roleId]; }
            return defaultStauts;
        }
        public void SetRoleBookStatus(int roleId, int status)
        {
            if (m_RoleBookMap.ContainsKey(roleId)) { m_RoleBookMap[roleId] = status; }
            else { m_RoleBookMap.Add(roleId, status); }
            OwnArchive.MarkDirty();
        }

        public void ClearRoleTalentStatus()
        {
            m_RoleTalentActiveMap.Clear();
            OwnArchive.MarkDirty();
        }
        public void SetRoleTalentStatus(int talentGroup, int talentIndex, int state)
        {
            if (!m_RoleTalentActiveMap.ContainsKey(talentGroup))
            {
                m_RoleTalentActiveMap.Add(talentGroup, new Dictionary<int, int>());
            }

            if (!m_RoleTalentActiveMap[talentGroup].ContainsKey(talentIndex))
            {
                m_RoleTalentActiveMap[talentGroup].Add(talentIndex, state);
            }
            m_RoleTalentActiveMap[talentGroup][talentIndex] = state;
            OwnArchive.MarkDirty();
        }
        public int GetRoleTalentStatus(int talentGroup, int talentIndex, int defaultStauts)
        {
            if (m_RoleTalentActiveMap.ContainsKey(talentGroup) &&
             m_RoleTalentActiveMap[talentGroup].ContainsKey(talentIndex))
            {
                return m_RoleTalentActiveMap[talentGroup][talentIndex];
            }
            return defaultStauts;
        }
        public int GetRoleId(int defaultRoleId)
        {
            if (m_CurrentRoleId == -1) return defaultRoleId;
            return m_CurrentRoleId;
        }
        public void SetRoleId(int roleId)
        {
            m_CurrentRoleId = roleId;
            OwnArchive.MarkDirty();
        }
        public int GetRoleSkinId(int defaultId)
        {
            if (m_CurrentRoleSkinId == -1) return defaultId;
            return m_CurrentRoleSkinId;
        }
        public void SetRoleSkinId(int roleSkinId)
        {
            m_CurrentRoleSkinId = roleSkinId;
            OwnArchive.MarkDirty();
        }
        public int GetRoleEquip(int euipTypeId, int defaultEquipId)
        {
            if (m_RoleEquipMap.ContainsKey(euipTypeId)) { return m_RoleEquipMap[euipTypeId]; }
            return defaultEquipId;
        }
        public void SetRoleEquip(int euipTypeId, int euipUId)
        {
            if (m_RoleEquipMap.ContainsKey(euipTypeId)) { m_RoleEquipMap[euipTypeId] = euipUId; }
            else { m_RoleEquipMap.Add(euipTypeId, euipUId); }
            OwnArchive.MarkDirty();
        }
        public int GetRoleStatus(int roleId, int defaultStauts)
        {
            if (m_RoleStatusMap.ContainsKey(roleId)) { return m_RoleStatusMap[roleId]; }
            return defaultStauts;
        }
        public void SetRoleStatus(int roleId, int status)
        {
            if (m_RoleStatusMap.ContainsKey(roleId)) { m_RoleStatusMap[roleId] = status; }
            else { m_RoleStatusMap.Add(roleId, status); }
            OwnArchive.MarkDirty();
        }
        public int GetRoleLv(int roleId, int defaultLv)
        {
            if (m_RoleLvMap.ContainsKey(roleId)) { return m_RoleLvMap[roleId]; }
            return defaultLv;
        }
        public void SetRoleLv(int roleId, int lv)
        {
            if (m_RoleLvMap.ContainsKey(roleId)) { m_RoleLvMap[roleId] = lv; }
            else { m_RoleLvMap.Add(roleId, lv); }
            OwnArchive.MarkDirty();
        }
        public int GetRoleStar(int roleId, int defaultStar)
        {
            if (m_RoleStarMap.ContainsKey(roleId)) { return m_RoleStarMap[roleId]; }
            return defaultStar;
        }
        public void SetRoleStar(int roleId, int star)
        {
            if (m_RoleStarMap.ContainsKey(roleId)) { m_RoleStarMap[roleId] = star; }
            else { m_RoleStarMap.Add(roleId, star); }
            OwnArchive.MarkDirty();
        }

        public void OnInitArchive()
        {
            CheckOrResetAfkTime();
        }
        private void CheckOrResetAfkTime()
        {
            if (m_GetAfkRewardTime <= 0)
            {
                SetRoleGetAfkRewardTime(DateTimeOffset.Now.ToUnixTimeSeconds());
            }
        }

        public void OnLoadFromArchive()
        {
            var dateTime = OwnArchive.UpdateTime;
            if (dateTime.Day != DateTime.Now.Day)
            {
                m_RoleShopDayBuyMap.Clear();
                m_AfkHasCleanCount = 0;
            }
            CheckOrResetAfkTime();
            OwnArchive.MarkDirty();
        }

        public long GetItemRecoveryTime(int itemId, long defaultSeconds)
        {
            if (m_ItemRecoverMap.TryGetValue(itemId, out var time))
            {
                return time;
            }
            return defaultSeconds;
        }
        public void SetItemRecoveryTime(int itemId, long time)
        {
            if (m_ItemRecoverMap.ContainsKey(itemId)) { m_ItemRecoverMap[itemId] = time; }
            else { m_ItemRecoverMap.Add(itemId, time); }
            OwnArchive.MarkDirty();
        }
        public void ClearItemRecoveryTime(int itemId)
        {
            if (m_ItemRecoverMap.ContainsKey(itemId))
            {
                m_ItemRecoverMap.Remove(itemId);
                OwnArchive.MarkDirty();
            }
        }
        public bool HasItemRecoveryTime(int itemId)
        {
            return m_ItemRecoverMap.ContainsKey(itemId);
        }

        public void SetShakeOpen(bool isOn)
        {
            m_IsShakeOpen = isOn;
            OwnArchive.MarkDirty();
        }
        public bool GetShakeOpen()
        {
            return m_IsShakeOpen;
        }
    }
}

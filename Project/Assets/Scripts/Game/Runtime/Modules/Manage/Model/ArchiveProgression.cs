using CustomLitJson.Extensions;
using System;
using System.Collections.Generic;

namespace Game.Modules.GModuleManage
{
    [Serializable]
    public class ArchiveProgression
    {
        [JsonIgnore] public GameArchive OwnArchive { get; internal set; }
        [JsonSerializer] private int m_Coins;
        [JsonSerializer] private Dictionary<int, int> m_PropCounts = new Dictionary<int, int>();
        [JsonSerializer] private Dictionary<string, int> m_AchievementProgress = new Dictionary<string, int>();
        [JsonSerializer] private Dictionary<string, int> m_AchievementClaimed = new Dictionary<string, int>();
        [JsonSerializer] private List<int> m_ChestClaimed = new List<int>();
        public int Coins => m_Coins;
        public void OnLoadFromArchive()
        {
            if (m_PropCounts == null) m_PropCounts = new Dictionary<int, int>();
            if (m_AchievementProgress == null) m_AchievementProgress = new Dictionary<string, int>();
            if (m_AchievementClaimed == null) m_AchievementClaimed = new Dictionary<string, int>();
            if (m_ChestClaimed == null) m_ChestClaimed = new List<int>();
        }
        public void AddCoins(int count) { m_Coins = Math.Max(0, m_Coins + count); Dirty(); }
        public bool SpendCoins(int count) { if (count < 0 || m_Coins < count) return false; m_Coins -= count; Dirty(); return true; }
        public int GetPropCount(int propId) => m_PropCounts.TryGetValue(propId, out var value) ? value : 0;
        public void AddProp(int propId, int count) { if (count <= 0) return; m_PropCounts[propId] = GetPropCount(propId) + count; Dirty(); }
        public bool ConsumeProp(int propId) { var count = GetPropCount(propId); if (count <= 0) return false; m_PropCounts[propId] = count - 1; Dirty(); return true; }
        public int GetAchievementProgress(string id) => m_AchievementProgress.TryGetValue(id, out var value) ? value : 0;
        public void AddAchievementProgress(string id, int amount) { if (string.IsNullOrEmpty(id) || amount <= 0) return; m_AchievementProgress[id] = GetAchievementProgress(id) + amount; Dirty(); }
        public bool IsAchievementClaimed(string id) => m_AchievementClaimed.TryGetValue(id, out var value) && value != 0;
        public void ClaimAchievement(string id) { if (!string.IsNullOrEmpty(id)) { m_AchievementClaimed[id] = 1; Dirty(); } }
        public bool IsChestClaimed(int levelId) => m_ChestClaimed.Contains(levelId);
        public bool ClaimChest(int levelId) { if (m_ChestClaimed.Contains(levelId)) return false; m_ChestClaimed.Add(levelId); Dirty(); return true; }
        private void Dirty() { if (OwnArchive != null) OwnArchive.MarkDirty(); }
    }
}

using CustomLitJson.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Modules.GModuleManage
{
    public class LevelArchiveStatus
    {
        [JsonIgnore]
        public const int Status_UnKnow = 0;
        [JsonIgnore]
        public const int Status_Passed = 1;
        [JsonIgnore]
        public const int Status_Gaming = 2;
    }


    [Serializable]
    public class ArchiveLevel
    {

        [JsonIgnore]
        public GameArchive OwnArchive { get; internal set; }
        [JsonSerializer]
        private Dictionary<int, int> m_LevelStatus;
        [JsonSerializer]
        private Dictionary<int, int> m_LevelRewardStatus;
        [JsonSerializer]
        private int m_HearNum;
        [JsonSerializer]
        private string m_LevelArrowsJson;
        //[JsonSerializer]
        //private Dictionary<int, int> m_MapTryCount;
        public ArchiveLevel()
        {

            m_LevelStatus = new Dictionary<int, int>();
            m_LevelRewardStatus = new Dictionary<int, int>();


        }
        public void OnLoadFromArchive()
        {

        }
        public void SetCurLevelArrowJson(string levelJson)
        {
            m_LevelArrowsJson = levelJson;
            OwnArchive.MarkDirty();
        }
        public string GetCurLevelArrowJson()
        {
            return m_LevelArrowsJson;
        }
        public void SetLevelStatus(int levelId, int status)
        {
            if (m_LevelStatus.ContainsKey(levelId))
            {
                m_LevelStatus[levelId] = status;
            }
            else { m_LevelStatus.Add(levelId, status); }
            OwnArchive.MarkDirty();
        }

        public int GetLevelStatus(int levelId, int defaultStatus)
        {
            if (m_LevelStatus.ContainsKey(levelId)) { return m_LevelStatus[levelId]; }
            return defaultStatus;
        }
        public int GetGamingLevel()
        {
            int lvId = -1;
            foreach (var item in m_LevelStatus)
            {
                if (item.Key > lvId && item.Value == LevelArchiveStatus.Status_Gaming)
                {
                    lvId = item.Key;
                }
            }
            return lvId;
        }
        public bool IsGamingLevel(int lvId)
        {
            return GetLevelStatus(lvId, LevelArchiveStatus.Status_UnKnow) == LevelArchiveStatus.Status_Gaming;
        }
        public int GetMaxPassedLevel()
        {
            int maxPassLvId = -1;
            foreach (var item in m_LevelStatus)
            {
                if (item.Key > maxPassLvId && item.Value == LevelArchiveStatus.Status_Passed)
                {
                    maxPassLvId = item.Key;
                }
            }
            return maxPassLvId;
        }
        public void PassCurLevel()
        {
            SetLevelPassed(GetCurLevelId());
        }
        public void SetLevelPassed(int levelId)
        {
            SetLevelStatus(levelId, LevelArchiveStatus.Status_Passed);
        }
        public int GetCurLevelId()
        {
            int gamingLvId = GetGamingLevel();
            if (gamingLvId != -1)
            {
                return gamingLvId;
            }
            int maxPassLvId = GetMaxPassedLevel();
            if (maxPassLvId == -1)
            {
                return 1;
            }
            return maxPassLvId + 1;
        }

        public int GetLevelRewardStatus(int levelId, int defaultStatus)
        {
            if (m_LevelRewardStatus.ContainsKey(levelId))
            {
                return m_LevelRewardStatus[levelId];
            }
            return defaultStatus;
        }
        public void SetLevelRewardStatus(int levelId, int status)
        {
            if (m_LevelRewardStatus.ContainsKey(levelId))
            {
                m_LevelRewardStatus[levelId] = status;
            }
            else { m_LevelRewardStatus.Add(levelId, status); }
            OwnArchive.MarkDirty();
        }


        public void SetCurLevelHeartNum(int heartNum)
        {
            m_HearNum = heartNum;
            OwnArchive.MarkDirty();
        }
        public int GetCurLevelHeartNum()
        {
            return m_HearNum;
        }
        public void ClearStatusType(int statusType)
        {
            var keys = m_LevelStatus.Keys.ToList();

            foreach (var item in keys)
            {
                if (m_LevelStatus[item] == statusType)
                {
                    m_LevelStatus.Remove(item);
                }
            }
            OwnArchive.MarkDirty();
        }
        public void ClearLevelStatusMap()
        {
            m_LevelStatus.Clear();
            m_LevelRewardStatus.Clear();
            OwnArchive.MarkDirty();
        }
        public void ClearLevelStatus(int levelId)
        {
            m_LevelStatus.Remove(levelId);
            m_LevelRewardStatus.Remove(levelId);
            OwnArchive.MarkDirty();
        }
    }
}

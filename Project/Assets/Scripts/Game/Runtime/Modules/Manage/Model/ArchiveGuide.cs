using CustomLitJson.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Modules.GModuleManage
{
    [Serializable]
    public class ArchiveGuide
    {

        [JsonIgnore]
        public GameArchive OwnArchive { get; internal set; }
        [JsonSerializer]
        private Dictionary<int, int> m_GuideStatus;
        [JsonSerializer]
        private Dictionary<int, List<int>> m_GuideCompleteTaskMap;
        public ArchiveGuide()
        {

            m_GuideStatus = new Dictionary<int, int>();
            m_GuideCompleteTaskMap = new Dictionary<int, List<int>>();
            //m_MapTryCount = new Dictionary<int, int>();

        }
        public void ClearAll()
        {
            m_GuideStatus.Clear();
            m_GuideCompleteTaskMap.Clear();
            OwnArchive.MarkDirty();
        }
        public void OnLoadFromArchive()
        {
            //m_GuideCompleteTaskMap.Clear();
            //m_GuideStatus.Clear();
           
            SaveArchive();
        }



        public void SaveCompleteGuideStatus(int guideId, int status)
        {
            if (m_GuideStatus.ContainsKey(guideId)) { m_GuideStatus[guideId] = status; }
            else { m_GuideStatus.Add(guideId, status); }
            SaveArchive();
        }
        public int GetCompleteGuideStatus(int guideId, int defaultStatus)
        {
            if (m_GuideStatus.ContainsKey(guideId)) { return m_GuideStatus[guideId]; }
            return defaultStatus;
        }

        public bool HasGuideSavedTaskId(int guideId, int taskId)
        {
            if (m_GuideCompleteTaskMap.TryGetValue(guideId,out var list))
            {
                return list.Contains(taskId);
            }
            return false;

        }

        public void SaveGuideCompleteTaskId(int guideId, int taskId)
        {
            if (m_GuideCompleteTaskMap.TryGetValue(guideId,out var list) ) {
                if (!list.Contains(taskId))
                {
                    list.Add(taskId);
                }
            }
            else { 
                m_GuideCompleteTaskMap.Add(guideId, new List<int>() { taskId});
            }
            SaveArchive();
        }
        private void SaveArchive()
        {
            OwnArchive.MarkDirty();
        }
    }
        
}


using Framework.Runtime.LogSystem;
using Framework.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Modules.GModuleGuid
{
    public interface IGuideMachineArchive
    {
        void SaveCompleteGuideId(int guideId);
        bool HasCompleteGuideId(int guideId);
        bool HasGuideSavedTaskId(int guideId, int taskId);
        void SaveGuideCompleteTaskId(int guideId, int taskId);
    }
    public class GuideMachine
    {
        public delegate int NodeTaskIdRecordGetter(int nodeId);
        private List<GuideNode> m_Nodes;
        private Dictionary<int, GuideNode> m_Id2NodeMap;
        private List<GuideTask> m_ActiveTasks;
       
        //private NodeTaskIdRecordGetter NodeTaskIdRecordGetterAgent;
        private CfgGuide m_CfgGuide;
        private IGuideMachineArchive m_GuideMachineArchive;

        public bool IsRunning()
        {
            return m_ActiveTasks.Count>0;
        }
        public void SetGuideMachineArchive(IGuideMachineArchive guideMachineArchive)
        {
            this.m_GuideMachineArchive = guideMachineArchive;
        }

        //public void SetNodeTaskIdRecordGetterAgent(NodeTaskIdRecordGetter agent)
        //{
        //    this.NodeTaskIdRecordGetterAgent = agent;
        //}
        public GuideMachine()
        {
            m_Id2NodeMap = new Dictionary<int, GuideNode>();
            m_Nodes = new List<GuideNode>();
            m_ActiveTasks = new List<GuideTask>();
        }
        public void UpdateMachine()
        {
            UpdateActiveTasks();
        }
        public void CompletTask(int guidId)
        {
            for (int i = m_ActiveTasks.Count - 1; i >= 0; i--)
            {
                var task = m_ActiveTasks[i];
                if(task.OwnerNode.guideId == guidId)
                {
                    task.DoneNow();
                    OnTaskDone(task);
                }
            }
            var node = FindNode(guidId);
            OnNodeComplete(node);
        }
        private void UpdateActiveTasks()
        {
            for (int i = 0; i < m_ActiveTasks.Count; i++)
            {
                var task = m_ActiveTasks[i];
                task.UpdateTask();
            }
            for (int i = m_ActiveTasks.Count-1; i>=0; i--)
            {
                var task = m_ActiveTasks[i];
                if (task.IsDone())
                {
                    OnTaskDone(task);
                    DoNextNodeTask(task);
                }
            }
        }
        private void OnTaskDone(GuideTask task)
        {
            m_ActiveTasks.Remove(task);
            int currentIndex = GetTaskIndex(task);
            if (task.OwnerNode.saveSubIndex && task.CfgSubTask.saveIndex)
            {
                m_GuideMachineArchive.SaveGuideCompleteTaskId(task.OwnerNode.guideId, currentIndex);
            }
           
        }
        public void Start()
        {
            // 获取最新的引导节点
            int lastGuideId = GetLastGuideId();
            if(lastGuideId == -1)
            {
                return;
            }
            StartNode(lastGuideId);
            // 获取guideId当前进度id
            //if (!m_GuideMachineArchive.TryGetGuideSavedTaskId(lastGuideId,out var taskId))
            //{
            //    var node = FindNode(lastGuideId);
            //    taskId = node.cfgGuideNode.restartSubIndex;
            //}
            

        }
        public int GetLastGuideId()
        {
            var cfgGuide = m_CfgGuide;
            for (int i = cfgGuide.guideNodes.Count - 1; i >= 0; i--)
            {
                var node = cfgGuide.guideNodes[i];
                if (m_GuideMachineArchive.HasCompleteGuideId(node.guideId)){
                    return node.postId;
                }
            }
            if (cfgGuide.guideNodes.Count > 0)
            {
                return cfgGuide.guideNodes[0].guideId;
            }
            return -1;
            
        }
        public bool StartNode(int nodeId)
        {
            if (!HasNode(nodeId))
            {
                if (nodeId > 0)
                {
                    Log.Error($"未找到id 为{nodeId}的节点");
                }
                return false;
            }
            var node = FindNode(nodeId);
            return StartNode(node);
        }
        private bool StartNode(GuideNode guideNode)
        {
            // 只有未执行过的新手引导才能执行
            if (!guideNode.IsSleepy())
            {
                return false;
            }
            // 获取首次进入的任务id
            int taskEntryId = 0;
            bool isStart = false;
            if(StartNodeTask(guideNode, taskEntryId))
            {
                guideNode.status = GuideStatus.Playing;
                isStart = true;

            }
            if (guideNode.cfgGuideNode.guideOne)
            {
                m_GuideMachineArchive.SaveCompleteGuideId(guideNode.guideId);
            }
            return isStart;
        }
        private bool OnNodeComplete(GuideNode node)
        {
            node.status = GuideStatus.Completed;
            // 保存执行过的ID
            int nodeId = node.guideId;
            m_GuideMachineArchive.SaveCompleteGuideId(node.guideId);
            // 执行下一个node
            return StartNode(node.postId);
        }

        private bool DoNextNodeTask(GuideTask currentTask)
        {
            int currentIndex = GetTaskIndex(currentTask);
            int nextIndex = currentIndex + 1;
            if(currentTask.OwnerNode.FindTask(nextIndex) != null)
            {                
                return StartNodeTask(currentTask.OwnerNode, nextIndex);
            }
            else
            {
                // 该Node已经执行完成
                // 需要跳到下一个Node
                return OnNodeComplete(currentTask.OwnerNode);
            }

        }
        private int GetTaskIndex(GuideTask task)
        {
            return task.OwnerNode.tasks.IndexOf(task);
        }
        private bool StartNodeTask(GuideNode guideNode,int guideTaskIndex)
        {
           
            GuideTask guideTask = guideNode.FindTask(guideTaskIndex);
            if (m_GuideMachineArchive.HasGuideSavedTaskId(guideNode.guideId, guideTaskIndex))
            {
                return DoNextNodeTask(guideTask);
            }
            if (guideTask == null)
            {
                Log.Error($"下标为{guideTaskIndex} 的task 不存在");
                return false;
            }
            if (!guideTask.IsSleepy())
            {
                Log.Error($"下标为{guideTaskIndex} 的task 已经在执行中");
                return false;
            }
            // 开启task
            // 启动的时候才生成，防止一开始注册有些业务逻辑还没有实现
            guideTask.entryTriggers = GenerateGuideTriggers(guideTask, m_CfgGuide.taskEntryTrigger);
            guideTask.exitTriggers = GenerateGuideTriggers(guideTask, m_CfgGuide.taskExitTrigger);
            guideTask.stayTriggers = GenerateGuideTriggers(guideTask, m_CfgGuide.taskStayTrigger);
            guideTask.doneTriggers = GenerateGuideTriggers(guideTask, m_CfgGuide.taskDoneTrigger);

            m_ActiveTasks.Add(guideTask);
            guideTask.taskStatus = GuideTaskStatus.Waiting;
            return true;

        }
        public void InitMachineByConfig(CfgGuide cfgGuide, IGuideMachineArchive guideMachineArchive)
        {
            SetGuideMachineArchive(guideMachineArchive);
            m_CfgGuide = cfgGuide;
            for (int i = 0; i < cfgGuide.guideNodes.Count; i++) {
                GenerateGuideNode(cfgGuide.guideNodes[i]);
            }
        }
        private GuideNode FindNode(int nodeId)
        {
            return m_Id2NodeMap[nodeId];
        }
        public bool HasNode(int nodeId)
        {
            return m_Id2NodeMap.ContainsKey(nodeId);
        }
        private GuideNode GetNodeByCfg(CfgGuideNode cfgGuideNode)
        {
            GuideNode node = new GuideNode();
            node.cfgGuideNode = cfgGuideNode;
            node.guideId = cfgGuideNode.guideId;
            node.priority = cfgGuideNode.priority;
            node.guideName = cfgGuideNode.guideName;
            node.guidType = cfgGuideNode.guidType;
            node.postId = cfgGuideNode.postId;
            node.restartSubIndex = cfgGuideNode.restartSubIndex;
            node.saveSubIndex = cfgGuideNode.saveSubIndex;
            node.tasks = GenerateGuidTasks(node,cfgGuideNode.subTaskList);
            return node; 
        }
        private List<GuideTask> GenerateGuidTasks(GuideNode guideNode,List<CfgSubTask> cfgGuideTaskNames)
        {
            List<GuideTask> tasks = new List<GuideTask>();
            for (int i = 0; i < cfgGuideTaskNames.Count; i++)
            {
                tasks.Add(GenerateGuidTask(guideNode,cfgGuideTaskNames[i]));
            }
            return tasks;
        }
        private GuideTask GenerateGuidTask(GuideNode guideNode, CfgSubTask subTaskCfg)
        {
            var cfgGuideTaskName = subTaskCfg.taskName;
            GuideTask guideTask = new GuideTask(guideNode);
            guideTask.CfgSubTask = subTaskCfg;
            guideTask.taskName = cfgGuideTaskName;
            //guideTask.triggers = GenerateGuideTriggers(guideTask,cfgGuideTask.triggers);
            //guideTask.doneTriggers = GenerateGuideTriggers(guideTask, cfgGuideTask.doneTriggers);
            //guideTask.triggerConditions = GenerateGuideConditions(guideTask, cfgGuideTask.triggerConditions);
            //guideTask.doneTriggerConditions = GenerateGuideConditions(guideTask, cfgGuideTask.doneTriggerConditions);
            return guideTask;
        }
        //private GuideTrigger GenerateGuideTrigger(CfgGuideTrigger trigger)
        //{
        //    List<GuideHandler> guideHandlers = new List<GuideHandler>();
        //    for (int i = 0; i < trigger.handlers.Length; i++)
        //    {
        //        var hdl = trigger.handlers[i];
        //    }
        //    List<GuideCondition> guideConditions = new List<GuideCondition>();
        //}
        private List<GuideTrigger> GenerateGuideTriggers(GuideTask guideTask, Dictionary<string,CfgGuideTask> triggersTasks)
        {
            string taskName = guideTask.taskName;
            if(!triggersTasks.TryGetValue(taskName, out var triggerTask))
            {
                return new List<GuideTrigger>();
            }
            List<GuideTrigger> triggerItems = new List<GuideTrigger>();
            for (int j = 0; j < triggerTask.triggers.Count; j++)
            {
                GuideTrigger trigger = new GuideTrigger();
                var cfgTrigger = triggerTask.triggers[j];
                List<GuideHandler> guideHandlers = GenerateGuideHandler(guideTask, cfgTrigger.handlers);
                List<GuideCondition> guideConditions = GenerateGuideConditions(guideTask, cfgTrigger.conditions);
                trigger.guideConditions = guideConditions;
                trigger.guideHandlers = guideHandlers;
                triggerItems.Add(trigger);
            }
            return triggerItems;
        }
        private List<GuideHandler> GenerateGuideHandler(GuideTask guideTask, CfgGuideData[] guideDatas)
        {
            string taskName = guideTask.taskName;
            List<GuideHandler> guideHandlers = new List<GuideHandler>();
            FunctionGuideTrigger functionGuideTrigger = new FunctionGuideTrigger();
            functionGuideTrigger.BindTask(guideTask);
            guideHandlers.Add(functionGuideTrigger);
            functionGuideTrigger.AddTriggerDatas(guideDatas);
            for (int i = 0; i < guideDatas.Length; i++) {
                var guideData = guideDatas[i];
                if (GuideFactoryTable.TriggerFactoryTable.TryGetValue(guideData.type, out var type))
                {
                    GuideHandler guideTrigger = Utility.ReflectionUtil.CreateInstance(type) as GuideHandler;
                    guideTrigger.BindTask(guideTask);
                    guideTrigger.SetParams(guideData.dataParams);
                    guideHandlers.Add(guideTrigger);
                }
            }
            return guideHandlers;

        }

        private List<GuideCondition> GenerateGuideConditions(GuideTask guideTask, CfgGuideData[] guideDatas)
        {
            string taskName = guideTask.taskName;
            List<GuideCondition> guideConditions = new List<GuideCondition>();
            FunctionGuideCondition functionGuideCondition = new FunctionGuideCondition();
            functionGuideCondition.BindTask(guideTask);
            guideConditions.Add(functionGuideCondition);
            functionGuideCondition.AddConditionDatas(guideDatas);
            for (int i   = 0; i < guideDatas.Length; i++)
            {
                var guideData = guideDatas[i];
                if (GuideFactoryTable.ConditionFactoryTable.TryGetValue(guideData.type, out var type))
                {
                    GuideCondition guideCondition = Utility.ReflectionUtil.CreateInstance(type) as GuideCondition;
                    guideCondition.BindTask(guideTask);
                    guideCondition.SetParams(guideData.dataParams);
                    guideConditions.Add(guideCondition);
                }
            }

            return guideConditions;
        }

        //private List<GuideCondition> GenerateGuideConditions(GuideTask guideTask, List<CfgGuideData> conditions)
        //{
        //    List<GuideCondition> guideConditions = new List<GuideCondition>();
        //    FunctionGuideCondition functionGuideCondition = new FunctionGuideCondition();
        //    functionGuideCondition.BindTask(guideTask);
        //    functionGuideCondition.AddConditionDatas(conditions);
        //    guideConditions.Add(functionGuideCondition);
        //    for (int i = 0; i < conditions.Count; i++)
        //    {
        //        if (GuideFactoryTable.ConditionFactoryTable.TryGetValue(conditions[i].type, out var type))
        //        {
        //            GuideCondition guideCondition = Utility.ReflectionUtil.CreateInstance(type) as GuideCondition;
        //            guideCondition.BindTask(guideTask);
        //            guideCondition.SetParams(conditions[i].dataParams);
        //            guideConditions.Add(guideCondition);
        //        }
        //    }
            
        //    return guideConditions;
        //}
        private void RegisterGuideNode(GuideNode node)
        {
            m_Id2NodeMap.Add(node.guideId, node);
        }
        private void GenerateGuideNode(CfgGuideNode cfgGuideNode)
        {
            if (HasNode(cfgGuideNode.guideId))
            {
                Log.Error($"当前已经注册了id为{cfgGuideNode.guideId}");
                return; 
            }
            GuideNode node  = GetNodeByCfg(cfgGuideNode);
            RegisterGuideNode(node);
            
        }

    }
    public enum GuideStatus
    {
        Sleepy, // 未开始
        Playing, // 执行中
        Completed,// 完成状态
    }
    public enum GuideTaskStatus
    {
        Sleepy,
        Waiting,
        Staying,
        Done

    }
    public class GuideNode
    {
        // 引导的id;
        public int guideId;
        // 引导的名称
        public string guideName;
        // 是否强制引导
        public string guidType;
        // 后置引导id;
        public int postId;
        // 引导优先级
        public int priority;
        // 中断后重新触发的子进度索引;
        public int restartSubIndex;
        // 保存进度的子进度索引;
        public bool saveSubIndex;
        public GuideStatus status = GuideStatus.Sleepy;
        public List<GuideTask> tasks = new List<GuideTask>();
        internal CfgGuideNode cfgGuideNode;

        public int GetSubTaskIndex(string taskName)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                if (tasks[i].CfgSubTask.taskName == taskName)
                {
                    return i;
                }
            }
            return -1;
        }
        public bool IsSleepy()
        {
            return status == GuideStatus.Sleepy;
        }

        public GuideTask FindTask(int guideTaskId)
        {
            if (guideTaskId < tasks.Count)
            {
                return tasks[guideTaskId];
            }
            return null;
        }
    }
    public class GuideTask
    {
        public string taskName;
        public GuideTaskStatus taskStatus = GuideTaskStatus.Sleepy;
        public List<GuideTrigger> entryTriggers = new List<GuideTrigger>();
        public List<GuideTrigger> exitTriggers = new List<GuideTrigger>();
        public List<GuideTrigger> stayTriggers = new List<GuideTrigger>();
        public List<GuideTrigger> doneTriggers = new List<GuideTrigger>();
        
        public GuideNode OwnerNode { get;private set; }
        public CfgSubTask CfgSubTask { get; internal set; }

        private bool IsInTriggers;

        public GuideTask(GuideNode ownNode)
        {
            OwnerNode = ownNode;
        }
        public bool IsSleepy()
        {
            return taskStatus == GuideTaskStatus.Sleepy;
        }
        public bool IsWaiting()
        {
            return taskStatus == GuideTaskStatus.Waiting;
        }
        public bool IsStaying()
        {
            return taskStatus == GuideTaskStatus.Staying;
        }
        public bool IsDone()
        {
            return taskStatus == GuideTaskStatus.Done;
        }
        public void UpdateTask()
        {
            // 根据状态 判断当前应该执行的操作
            // 判断能否执行triggers
            if (IsWaiting()&& IsEnableTrigger(out var trigger))
            {
                bool allSuc = PlayTiggers(trigger);
                if (allSuc)
                {
                    taskStatus = GuideTaskStatus.Staying;
                }
                
            }else if(IsStaying() && IsExitTrigger(out trigger))
            {
                bool allSuc = ExitTriggers(trigger);
                if (allSuc)
                {
                    taskStatus = GuideTaskStatus.Waiting;
                }
                
            }
            else if (IsStaying() && IsStayTrigger(out trigger))
            {
                StayTiggers(trigger);
            }
            else if (IsStaying() && IsDownTrigger(out trigger))
            {
                bool allSuc =  DoneTriggers(trigger);
                if (allSuc)
                {
                    taskStatus = GuideTaskStatus.Done;
                }
             
            }

        }
        public void DoneNow()
        {
            //if (IsStaying())
            //{
            //    DoneTriggers();
            //}
            //taskStatus = GuideTaskStatus.Done;
        }
        private bool IsEnableTrigger(out GuideTrigger sucTrigger)
        {
            bool defaultCondition = false;
            for (int i = 0; i < entryTriggers.Count; i++)
            {
                var trigger = entryTriggers[i];
                if (trigger.CheckCondition(false))
                {
                    sucTrigger = trigger;
                    return true;
                }
            }
            sucTrigger = null;
            return defaultCondition;

        }
        private bool IsDownTrigger(out GuideTrigger sucTrigger)
        {
            bool defaultCondition = doneTriggers.Count>0?false:true;
            for (int i = 0; i < doneTriggers.Count; i++)
            {
                var trigger = doneTriggers[i];
                if (trigger.CheckCondition(true))
                {
                    sucTrigger = trigger;
                    return true;
                }
            }
            sucTrigger = null;
            return defaultCondition;
        }
        private bool IsStayTrigger(out GuideTrigger sucTrigger)
        {
            bool defaultCondition = false;
            for (int i = 0; i < stayTriggers.Count; i++)
            {
                var trigger = stayTriggers[i];
                if (trigger.CheckCondition(defaultCondition))
                {
                    sucTrigger = trigger;
                    return true;
                }
            }
            sucTrigger = null;
            return defaultCondition;
        }
        private bool IsExitTrigger(out GuideTrigger sucTrigger)
        {
            bool defaultCondition = false;
            for (int i = 0; i < exitTriggers.Count; i++)
            {
                var trigger = exitTriggers[i];
                if (trigger.CheckCondition(false))
                {
                    sucTrigger = trigger;
                    return true;
                }
            }
            sucTrigger = null;
            return defaultCondition;
        }
        private bool PlayTiggers(GuideTrigger guideTrigger)
        {
          return  guideTrigger?.OnTrigger()??true;
            
        }
        private bool DoneTriggers(GuideTrigger guideTrigger)
        {
            return guideTrigger?.OnTrigger() ?? true;
        }
        private bool StayTiggers(GuideTrigger guideTrigger)
        {
            return guideTrigger?.OnTrigger() ?? true;
        }
        private bool ExitTriggers(GuideTrigger guideTrigger)
        {
            return guideTrigger?.OnTrigger() ?? true;
        }
    }
    public class GuideComponent
    {
        public string type;
        public GuideStatus status = GuideStatus.Sleepy;
        private Dictionary<string, CfgArgs> m_Name2ParamsMap = new Dictionary<string, CfgArgs>();
        public GuideTask OwnTask { get; set; }
        public void BindTask(GuideTask task)
        {
            OwnTask = task;
        }
        public void SetParams(List<CfgArgs> dataParams)
        {
            m_Name2ParamsMap = new Dictionary<string, CfgArgs>();
            for (int i = 0; i < dataParams.Count; i++)
            {
                var param = dataParams[i];
                if(!m_Name2ParamsMap.ContainsKey(param.argKey))
                    m_Name2ParamsMap.Add(param.argKey, param);
            }

        }
        public bool TryGetParams<T>(string paramName,out T res)
        {
            res = default;
            if (m_Name2ParamsMap.TryGetValue(paramName, out var paramData))
            {
                res = GetParamFromData<T>(paramData, default);
                return true;
            }
            return false;
        }
        public T GetParam<T>(string paramName,T defaultValue = default )
        {
            if(m_Name2ParamsMap.TryGetValue(paramName,out var paramData))
            {
                return GetParamFromData<T>(paramData, defaultValue);
            }
            return defaultValue;
        }
        private T GetParamFromData<T>(CfgArgs guideParams, T defaultValue)
        {
            return DataManager.ParseStringToType<T>(guideParams.argVal, defaultValue);
        }
    }
    public class FunctionGuideTrigger : GuideHandler
    {
        private List<CfgGuideData> triggerDatas = new List<CfgGuideData>();
        public void AddTriggerDatas(CfgGuideData[] triggers)
        {
            triggerDatas.AddRange(triggers);
        }
        public void AddTriggerDatas(List<CfgGuideData> triggers)
        {
            triggerDatas.AddRange(triggers);
        }
        public void AddTriggerData(CfgGuideData trigger)
        {
            triggerDatas.Add(trigger);
        }
        public override bool OnTrigger()
        {
            bool allSuc = true;
            for (int i = 0; i < triggerDatas.Count; i++) {
                if (GuideFactoryTable.TryGetGuideTriggerReference(triggerDatas[i].type,out var reference))
                {
                   var res= reference.Invoke(this, triggerDatas[i]);
                    if (!res)
                    {
                        allSuc = false;
                    }
                }
            }
            return allSuc;
        }
    }
    public class FunctionGuideCondition : GuideCondition
    {
        
        //private Dictionary<string, GuideConditionAgent> m_Type2AgentMap;
        private List<CfgGuideData> conditionDatas;
      
        public FunctionGuideCondition()
        {
            //this.m_Type2AgentMap = new Dictionary<string, GuideConditionAgent>();
            conditionDatas = new List<CfgGuideData>();
        }
        public override bool CheckCondition()
        {
            bool isInCondition = conditionDatas.Count>0?true: emptyDefaultCondition;
            for (int i = 0; i < conditionDatas.Count; i++) { 
                var conditionData = conditionDatas[i];
                if ( GuideFactoryTable.TryGetGuideConditionAgent(conditionData.type,out var agent))
                {
                    if (!agent.Invoke(this,conditionData))
                    {
                        isInCondition = false;
                    }
                }
                if (!isInCondition)
                {
                    return false;
                }
            }
            return isInCondition;
        }

        public void AddConditionDatas(CfgGuideData[] conditions)
        {
            conditionDatas.AddRange(conditions);
        }
        public void AddConditionDatas(List<CfgGuideData> conditions)
        {
            conditionDatas.AddRange(conditions);
        }
        public void AddConditionData(CfgGuideData condition)
        {
            conditionDatas.Add(condition);
        }
    }
    public class GuideTrigger
    {
        public List<GuideCondition> guideConditions;
        public List<GuideHandler> guideHandlers;

        public bool CheckCondition(bool emptyDefaultValue = true)
        {
            bool defaultValue =guideConditions.Count>0?true: emptyDefaultValue;
            for (int i = 0; i < guideConditions.Count; i++)
            {
                guideConditions[i].emptyDefaultCondition = emptyDefaultValue;
                if (!guideConditions[i].CheckCondition())
                {
                    return false;
                }
            }
            return defaultValue;

        }

        public bool OnTrigger()
        {
            bool allSuc = true;
            for (int i = 0; i < guideHandlers.Count; i++)
            {
               var res = guideHandlers[i].OnTrigger();
                if (!res)
                {
                    allSuc = false;
                }
            }
            return allSuc;
        }
    }
    /// <summary>
    /// 引导触发器，这里执行对应的触发效果
    /// </summary>
    public class GuideHandler : GuideComponent
    {
        public virtual bool OnTrigger()
        {
            return true;
        } 
    }
    /// <summary>
    /// 触发条件 当引导满足触发条件的时候，会执行触发器
    /// </summary>
    public class GuideCondition: GuideComponent
    {
        public bool emptyDefaultCondition = true;
        public virtual bool CheckCondition()
        {
            return emptyDefaultCondition;
        }
    }
}

using Framework.Runtime.LogSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Modules.GModuleTask
{
    public class TaskChain
    {
        
        public TaskChainStatus TaskChainStatus { get; private set; }
     
        public int TaskChainId { get; private set; }
        public string TaskChainName { get; private set; }
        private Dictionary<int, TaskNode> m_Id2NodeMap;
        private List<TaskNode> m_TaskNodes;
        public int UnLock()
        {
            if (!IsStatus(TaskChainStatus.Locked))
            {
                return TaskRet.retCode_hasUnlocked;
            }
            SwitchStatus(TaskChainStatus.DisActive);
            return TaskRet.retCode_None;
        }
        public void UpdateStatus()
        {
            if (IsStatus(TaskChainStatus.Locked)) { return; }
            if (IsStatus(TaskChainStatus.DisActive)) { return; }
            bool isInterruped = false;
            bool isActive = false;
            bool isAllComplete = true;
            for (int i = 0; i < m_TaskNodes.Count; i++)
            {
                TaskNode node = m_TaskNodes[i];
                if (node.IsFailed || node.IsGiveUp)
                {
                    isInterruped = true;
                }
                if (node.IsAccept || node.IsReached)
                {
                    isActive = true;
                }
                if (!node.IsSubmited)
                {
                    isAllComplete = false;
                }
            }
            
            if (isInterruped)
            {
                SwitchStatus(TaskChainStatus.Interrupted);

            }else if (isActive)
            {
                SwitchStatus(TaskChainStatus.InProgress);
            }else if (isAllComplete)
            {
                SwitchStatus(TaskChainStatus.Completed);
            }

        }
        private void SwitchStatus(TaskChainStatus status)
        {
            TaskChainStatus = status;
        }
        public TaskChain(CfgGameTaskChain initCfg)
        {
            TaskChainId = initCfg.taskChainId;
            TaskChainName = initCfg.taskChainName;
            m_TaskNodes = new List<TaskNode>();
            m_Id2NodeMap = new Dictionary<int, TaskNode>();
            this.InitTaskNodes(initCfg.taskNodes);
        }
        private void InitTaskNodes(List<CfgTaskNode> taskNodes)
        {
            for (int i = 0; i < taskNodes.Count; i++)
            {
                RegisterTaskNode(taskNodes[i]);
            }
        }
        public bool IsStatus(TaskChainStatus taskChainStatus)
        {
            return TaskChainStatus == taskChainStatus;
        }
        private void RegisterTaskNode(CfgTaskNode cfgTaskNode)
        {
            if (this.m_Id2NodeMap.ContainsKey(cfgTaskNode.taskId))
            {
                Log.Error($"该任务链已注册过相同id的任务节点{cfgTaskNode.taskId}");
                return;
            }

            TaskNode taskNode = new TaskNode(this, cfgTaskNode);
            m_Id2NodeMap.Add(cfgTaskNode.taskId, taskNode);
            m_TaskNodes.Add(taskNode);
        }

      
    }
    public enum TaskChainStatus
    {
        Locked,// 前置条件未达成，锁定状态
        DisActive,//任务链条未开启
        InProgress,//任务链中有任务正在活跃
        Completed, //整个任务链所有任务均已完成
        Interrupted, // 任务链中断，因为某个任务失败导致链中断
    }
    public enum TaskStatus
    {
        Locked, // 未解锁
        DisActive,// 满足接取条件，可以接受任务，但是未开启
        Accepted, // 已经接取任务，正在进行中
        Reached,// 任务目标已达成，但是未提交
        Submited, //任务已完成并且已经提交
        Failed, // 任务失败
        GiveUp, // 任务主动放弃
    }
    public class TaskNode
    {
        private TaskChain ownTaskChain;
        public TaskStatus TaskStatus { get; private set; }
        public int TaskId { get; private set; }
        public string TaskName { get; private set; }
        public string TaskSummary { get; private set; }
        public string TaskDescription { get; private set; }
        public string TaskTarget { get; private set; }

        public bool IsFailed => TaskStatus == TaskStatus.Failed;
        public bool IsGiveUp => TaskStatus == TaskStatus.GiveUp;
        public bool IsAccept => TaskStatus == TaskStatus.Accepted;
        public bool IsReached => TaskStatus == TaskStatus.Reached;
        public bool IsSubmited => TaskStatus == TaskStatus.Submited;
        
        public TaskNode(TaskChain taskChain, CfgTaskNode cfgTaskNode)
        {
            this.ownTaskChain = taskChain;
            this.TaskId = cfgTaskNode.taskId;
            this.TaskName = cfgTaskNode.taskName;
            this.TaskSummary = cfgTaskNode.taskSummary;
            this.TaskDescription = cfgTaskNode.taskDescription;
            this.TaskTarget = cfgTaskNode.taskTarget;
        }
        public void SetStatus(TaskStatus status)
        {
            TaskStatus = status;
            ownTaskChain.UpdateStatus();
        }
        
    }
    public class TaskRet
    {
        public const int retCode_None = 0;
        public const int retCode_hasUnlocked = 1;

        public const int retCode_chain_not_exist = 2;
        public const int retCode_task_not_exist = 3;
    }
    public class TaskManager
    {
       
        private Dictionary<int, TaskChain> m_ChainMap;
        public TaskManager()
        {
            this.m_ChainMap = new Dictionary<int, TaskChain>();
        }
        public int UnLockChain(int chainId)
        {
            if (!CheckChain(chainId)) return TaskRet.retCode_chain_not_exist;
            TaskChain chain = FindChain(chainId);
            int ret = chain.UnLock();
            if(ret == TaskRet.retCode_None)
            {
                chain.UpdateStatus();
            }
            return ret;

        }
        public void ActiveChain()
        {

        }
        public TaskChain FindChain(int chainId)
        {
            if (CheckChain(chainId))
            {
                return m_ChainMap[chainId];
            }
            return null;
        }
        private bool CheckChain(int chainId)
        {
            if (!m_ChainMap.ContainsKey(chainId))
            {
                Log.Error($"不存在id为{chainId}的任务链");
                return false;
            }
            return true;
        }
        public void InitTaskByCfg(CfgGameTaskTable cfgTaskTable)
        {
            for (int i = 0; i < cfgTaskTable.tasksCfg.Count; i++)
            {
                RegisterTaskChain(cfgTaskTable.tasksCfg[i]);
            }
        }
        private void RegisterTaskChain(CfgGameTaskChain cfgGameTaskChain)
        {
            
            int id = cfgGameTaskChain.taskChainId;
            if (this.m_ChainMap.ContainsKey(id))
            {
                Log.Error($"已注册过相同id的任务链{cfgGameTaskChain.taskChainId}");
                    return;
            }
            TaskChain taskChain = new TaskChain(cfgGameTaskChain);
            this.m_ChainMap.Add(id, taskChain);
        }
        
    }
}


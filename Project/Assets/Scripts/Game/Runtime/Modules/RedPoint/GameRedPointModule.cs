using Framework.Runtime.LogSystem;
using Framework.Runtime.UnitSystem.Base;
using Framework.Runtime.UnitSystem.BIInterfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Modules
{
    public interface IGameReadPointTable
    {
    }

    public class GameRedPointModule : GameModuleBaseInstance<GameRedPointModule>,IUnitUpdate
    {
        public delegate void RedPointStateChangeAction(RedPointVO redPointVO);
        private bool isTreeDirety = true;
        private bool isTreeUpdating = false;
        protected override void GenerateHandlers()
        {
            RegisterHandler<GameRedPointClientHandler>();
            RegisterHandler<GameRedPointDataHandler>();
            RegisterHandler<GameRedPointServerHandler>();
            RegisterHandler<GameRedPointViewHandler>();
        }
        public static List<RedPointType> RedPointTypes = Enum.GetValues(typeof(RedPointType)).Cast<RedPointType>().ToList();

        private Dictionary<string, List<string>> childRedPointDict = new Dictionary<string, List<string>>();

        private Dictionary<string, List<string>> parentRedPointDict = new Dictionary<string, List<string>>();

        private Dictionary<string, RedPointVO> redPointDict = new Dictionary<string, RedPointVO>();

        private List<string> rootRedPointList = new List<string>();
        private List<RedPointVO> dirtyVos = new List<RedPointVO>();

        protected override void OnModuleStart()
        {
            base.OnModuleStart();
            
        }
        public bool GetRedPointState(string key, RedPointType pointType = RedPointType.PurePoint)
        {
            RedPointVO vo = this.GetRedPointVO(key);
            return vo.GetPointNumTotal(pointType) > 0;
        }
        public void ChangeRedPointState(string key, bool state, RedPointType redPointType = RedPointType.PurePoint)
        {
            RedPointVO vo = this.GetRedPointVO(key);
            vo.SetPointNum(state ? 1 : 0,redPointType);
        }

        public RedPointVO FindRedPointVO(string key)
        {
            if (this.redPointDict.ContainsKey(key))
            {
                return this.redPointDict[key];
            }
            return null;
        }

        public void RegisterRedPoint(string key, string parentKey = "")
        {
            if (string.IsNullOrEmpty(key))
                return;
            RedPointVO redPointVO = this.GetRedPointVO(key);
            if (!string.IsNullOrEmpty(parentKey))
            {
                this.AddSubRedPoint(parentKey, key);
            }
            else
            {
                this.AddRootRedPoint(key);
            }
        }
        public void RegisterRedPoint(IGameRedPoint redPoint, string parentKey = "")
        {
            if (!string.IsNullOrEmpty(redPoint.Key))
            {
                RegisterRedPoint(redPoint.Key, parentKey);
            }
            if (redPoint.SubRedPoints != null)
            {
                for (int i = 0; i < redPoint.SubRedPoints.Count; i++)
                {
                    IGameRedPoint subRedPoint = redPoint.SubRedPoints[i];
                    RegisterRedPoint(subRedPoint, redPoint.Key);
                }
            }
           
            if (redPoint.SubRedPointKeys != null)
            {
                for (int i = 0; i < redPoint.SubRedPointKeys.Count; i++)
                {
                    RegisterRedPoint(redPoint.SubRedPointKeys[i], redPoint.Key);
                }
            }

        }

        public void SetRedPointNum(string key, int pointNum, RedPointType redPointType)
        {
            RedPointVO vo = this.GetRedPointVO(key);
            vo.SetPointNum(pointNum, redPointType);
        }
        public void AddRedPointNum(string key,int pointNum, RedPointType redPointType)
        {
            RedPointVO vo = this.GetRedPointVO(key);
            vo.AddRedPointNum(pointNum, redPointType);
        }

        public void UpdateParentRedPointState(RedPointVO dirtyVO)
        {
            if (isTreeUpdating) return;
            if (!dirtyVos.Contains(dirtyVO))
            {
                dirtyVos.Add(dirtyVO);
            }
            isTreeDirety = true;
        }
        private void UpdateTree()
        {
            if (!isTreeDirety) return;
            isTreeUpdating = true;
            for (int i = 0; i < dirtyVos.Count; i++) {
                var dirtyVo = dirtyVos[i];
                if (!dirtyVo.IsDirty) continue;
                UpdateRedPointVoTreeNode(dirtyVo.Key);
            }
            dirtyVos.Clear();
            isTreeDirety = false;
            isTreeUpdating = false;
            
        }
        private void DownUpdateRed(string key)
        {
            RedPointVO redPointVO = FindRedPointVO(key);
            redPointVO.ResetSubData();
            if (childRedPointDict.TryGetValue(key, out List<string> childs) && childs.Count > 0)
            {
                for (int i = 0; i < childs.Count; i++)
                {
                    RedPointVO childRedPointVo = FindRedPointVO(childs[i]);
                    DownUpdateRed(childs[i]);
                    for (int j = 0; j < RedPointTypes.Count; j++)
                    {
                        RedPointType redPointType = RedPointTypes[j];
                        int subTotalCount = childRedPointVo.GetPointNumTotal(redPointType);
                        int hasCount = redPointVO.GetPointNumChild(redPointType);
                        int newCount = hasCount + subTotalCount;
                        redPointVO.SetChildPointNum(newCount, redPointType);
                    }
                }   
            }
            redPointVO.CheckAndResetDirty();
            //Debug.Log($"红点key{redPointVO.Key} 子数量{redPointVO.GetPointNumChild(RedPointType.PurePoint)} 自身数量{redPointVO.GetPointNumSelf(RedPointType.PurePoint)} 总数量{redPointVO.GetPointNumTotal(RedPointType.PurePoint)}");

        }
        private void UpdateRedCount(string key)
        {
            RedPointVO redPointVO = FindRedPointVO(key);
            redPointVO.ResetSubData();
            if (childRedPointDict.TryGetValue(key, out List<string> childs) && childs.Count > 0)
            {
                for (int i = 0; i < childs.Count; i++)
                {
                    RedPointVO childRedPointVo = FindRedPointVO(childs[i]);
                    for (int j = 0; j < RedPointTypes.Count; j++)
                    {
                        RedPointType redPointType = RedPointTypes[j];
                        int subTotalCount = childRedPointVo.GetPointNumTotal(redPointType);
                        int hasCount = redPointVO.GetPointNumChild(redPointType);
                        int newCount = hasCount + subTotalCount;
                        redPointVO.SetChildPointNum(newCount, redPointType);
                    }
                }
            }
            redPointVO.CheckAndResetDirty();
        }
        private void UpUpdateRed(string key)
        {
            RedPointVO redPointVO = FindRedPointVO(key);
            if (parentRedPointDict.TryGetValue(key, out List<string> parents) && parents.Count>0)
            {
                for (int i = 0; i < parents.Count; i++)
                {
                    UpdateRedCount(parents[i]);
                    
                    //Debug.Log($"红点key{redPointVO.Key} 子数量{redPointVO.GetPointNumChild(RedPointType.PurePoint)} 自身数量{redPointVO.GetPointNumSelf(RedPointType.PurePoint)} 总数量{redPointVO.GetPointNumTotal(RedPointType.PurePoint)}");
                    UpUpdateRed(parents[i]);
                }
            }
            else
            {
                //Debug.Log($"红点key{redPointVO.Key} 子数量{redPointVO.GetPointNumChild(RedPointType.PurePoint)} 自身数量{redPointVO.GetPointNumSelf(RedPointType.PurePoint)} 总数量{redPointVO.GetPointNumTotal(RedPointType.PurePoint)}");
            }
        }
        private void UpdateRedPointVoTreeNode(string key)
        {
            DownUpdateRed(key);
            UpUpdateRed(key);
        }

        private void AddRootRedPoint(string key)
        {
            if (!this.rootRedPointList.Contains(key))
            {
                this.rootRedPointList.Add(key);
            }
        }
        private bool CyleCheck(string parentKey, string childKey)
        {
            if (childRedPointDict.TryGetValue(childKey, out List<string> childs)){
                for (int i = 0; i < childs.Count; i++)
                {
                    if (childs[i] == parentKey)
                    {
                        return false;
                    }
                    if(!CyleCheck(parentKey, childs[i]))
                    { 
                        return false;
                    }
                }
            }
            return true;
        }
        private void AddSubRedPoint(string parentKey, string childKey)
        {
            if (!CyleCheck(parentKey, childKey))
            {
                Log.Fatal($"请检查红点配置表! 当前红点树存在循环依赖的关系，请修改! parentKey==>{parentKey} childKey{childKey}");
                return;
            }

            if (!this.childRedPointDict.ContainsKey(parentKey))
            {
                this.childRedPointDict[parentKey] = new List<string> { childKey };
            }
            else if (!this.childRedPointDict[parentKey].Contains(childKey))
            {
                this.childRedPointDict[parentKey].Add(childKey);
            }

            if (!this.parentRedPointDict.ContainsKey(childKey))
            {
                this.parentRedPointDict[childKey] = new List<string> { parentKey };
            }
            else if (!this.parentRedPointDict[childKey].Contains(parentKey))
            {
                this.parentRedPointDict[childKey].Add(parentKey);
            }
        }

        public RedPointVO GetRedPointVO(string key)
        {
            RedPointVO vo = this.FindRedPointVO(key);
            if (vo == null)
            {
                vo = new RedPointVO(key);
                this.redPointDict[key] = vo;
            }
            return vo;
        }

        public void OnUnitUpdate()
        {
            UpdateTree();
        }

        public void AddRedChangeCb(string redKey, RedPointStateChangeAction changeCb)
        {
            GetRedPointVO(redKey).AddChangeCallBack(changeCb);
        }
        public void RemoveRedChangeCb(string redKey, RedPointStateChangeAction changeCb)
        {
            GetRedPointVO(redKey).RemoveChangeCallBack(changeCb);
        }
    }
}
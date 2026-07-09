using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Game.Modules.GameRedPointModule;

namespace Game.Modules
{
    public class RedPointVO
    {
        private class RedPointData
        {
            public int selfNum;
            public int subNum;
        }
        private string key = "";
        public string Key => key;
        private RedPointStateChangeAction onChangeCb;
        private Dictionary<RedPointType, RedPointData> redPointMap;
        private bool isDirty = true;
        public bool IsDirty => isDirty;
        
        public void ResetSubData()
        {
            for (int j = 0; j <GameRedPointModule.RedPointTypes.Count; j++)
            {
                RedPointType redPointType = RedPointTypes[j];
                if (redPointMap.TryGetValue(redPointType, out RedPointData data))
                {
                    //data.selfNum = 0;
                    data.subNum = 0;
                    redPointMap[redPointType] = data;
                }
            }
        }
        public RedPointVO(string key)
        {
            this.key = key;
            redPointMap = new Dictionary<RedPointType, RedPointData>();
            this.onChangeCb = null;
            SetRedTreeDirty();
        }
        public void RemoveChangeCallBack(RedPointStateChangeAction callBack)
        {
            if (callBack == null)
            {
                return;
            }
            this.onChangeCb -= callBack;
        }
        public void AddChangeCallBack(RedPointStateChangeAction callBack)
        {
            if (callBack == null)
            {
                return;
            }
            this.onChangeCb -= callBack;
            this.onChangeCb += callBack;
            callBack(this);
        }
        public int GetPointNumTotal(RedPointType redPointType)
        {
            int total = GetPointNumSelf(redPointType) + GetPointNumChild(redPointType);
            return total;
        }
        public int GetPointNumSelf(RedPointType redPointType)
        {
            if (redPointMap.TryGetValue(redPointType, out RedPointData data))
            {
                return data.selfNum;
            }
            return 0;
        }

        public int GetPointNumChild(RedPointType redPointType)
        {
            if (redPointMap.TryGetValue(redPointType, out RedPointData data))
            {
                return data.subNum;
            }
            return 0;
        }

        private void SetRedTreeDirty()
        {
            isDirty = true;
            GameRedPointModule.GetIns().UpdateParentRedPointState(this);
           
        }

        public void SetChildPointNum(int pointChildNum, RedPointType redPointType)
        {
            SetPointDataChildNum(pointChildNum, redPointType);
        }
        public void SetPointNum(int pointNum,RedPointType redPointType)
        {
            SetPointDataSelfNum(pointNum, redPointType);
        }
        public void AddRedPointNum(int pointNum,RedPointType redPointType)
        {
            if (redPointMap.TryGetValue(redPointType, out RedPointData data))
            {
                data.selfNum += pointNum;
                data.selfNum = Mathf.Max(0, data.selfNum);
            }
            else
            {
                redPointMap.Add(redPointType, new RedPointData()
                {
                    selfNum = pointNum>0?pointNum:0,
                });
            }
            SetRedTreeDirty();
        }
        private void SetPointDataSelfNum(int pointNumSelf,RedPointType redPointType)
        {
            if(redPointMap.TryGetValue(redPointType,out RedPointData data))
            {
                data.selfNum = pointNumSelf>0?pointNumSelf:0;
            }
            else
            {
                redPointMap.Add(redPointType, new RedPointData()
                {
                    selfNum = pointNumSelf > 0 ? pointNumSelf : 0,
                });
            }
            SetRedTreeDirty();
        }
        private void SetPointDataChildNum(int pointNumChild, RedPointType redPointType)
        {
            if (redPointMap.TryGetValue(redPointType, out RedPointData data))
            {
                data.subNum = pointNumChild;
            }
            else
            {
                redPointMap.Add(redPointType, new RedPointData()
                {
                    subNum = pointNumChild,
                });
            }
            SetRedTreeDirty();
        }

        private void InvokeCallBack()
        {
            this.onChangeCb?.Invoke(this);
        }
        public void CheckAndResetDirty()
        {
            if (isDirty)
            {
                InvokeCallBack();
                isDirty = false;
            }
        }

        public bool GetState(RedPointType redPointType = RedPointType.PurePoint)
        {
            return GetPointNumTotal(redPointType) > 0;
        }
    }
}
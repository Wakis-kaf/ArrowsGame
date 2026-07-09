using Framework.Runtime.LogSystem;
using Framework.Utils;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Framework.Runtime
{
    public class LoopManager
    {
        private List<UpdateVO> secondLoopCallback;

        private List<UpdateVO> secondLoopCallbackAddTemp;

        private List<UpdateVO> secondLoopCallbackRemoveTemp;

        private  List<UpdateVO> fixedLoopCallback;

        private  List<UpdateVO> fixedLoopCallbackAddTemp;

        private  List<UpdateVO> fixedLoopCallbackRemoveTemp;

        private  List<UpdateVO> lateLoopCallback;

        private  List<UpdateVO> lateLoopCallbackAddTemp;

        private  List<UpdateVO> lateLoopCallbackRemoveTemp;

        private  List<UpdateVO> loopCallBack;

        private  List<UpdateVO> loopCallBackAddTemp;

        private  List<UpdateVO> loopCallBackRemoveTemp;

        private  WaitForEndOfFrame m_WaitForEndOfFrameCache = new WaitForEndOfFrame();

        private  WaitForFixedUpdate m_WaitForFixedUpdateCache = new WaitForFixedUpdate();

        private List<UpdateVO> timeoutLoopCallBack;

        private List<UpdateVO> timeoutLoopCallBackAddTemp;

        private List<UpdateVO> timeoutLoopCallBackRemoveTemp;

        private  List<UpdateVO> timerLoopCallBack;

        private  List<UpdateVO> timerLoopCallBackAddTemp;

        private  List<UpdateVO> timerLoopCallBackRemoveTemp;

        public  void AddFixedLoop(Action updater, int priority = 1000)
        {
            AddToQueue(updater, priority, ref fixedLoopCallbackAddTemp, ref fixedLoopCallbackRemoveTemp,
                ref fixedLoopCallback);
        }

        public  void AddLateLoop(Action updater, int priority = 1000)
        {
            AddToQueue(updater, priority, ref lateLoopCallbackAddTemp, ref lateLoopCallbackRemoveTemp,
                ref lateLoopCallback);
        }


        public void AddSecond(Action updater, int timeSecond, int priority = 1000)
        {
            AddToQueue(updater, priority, ref timerLoopCallBackAddTemp, ref timerLoopCallBackRemoveTemp,
                ref timerLoopCallBack, timeSecond,true);
        }
        public  void AddTimer(Action updater, float timer, int priority = 1000)
        {
            AddToQueue(updater, priority, ref timerLoopCallBackAddTemp, ref timerLoopCallBackRemoveTemp,
                ref timerLoopCallBack, timer);
        }
        public void AddTimeout(Action updater, float timeout, int priority = 1000)
        {
            AddToQueue(updater, priority, ref timeoutLoopCallBackAddTemp, ref timeoutLoopCallBackRemoveTemp,
                ref timeoutLoopCallBack, timeout);
        }
        public void UpdateTimeout(Action updater, float timeout, int priority = 1000)
        {
            UpdateQueue(updater, priority, ref timeoutLoopCallBackAddTemp,ref timeoutLoopCallBack, timeout);
        }
        public  void AppUpdate(GameAppMessage appMessage)
        {
        }

        public  void FixedLoop()
        {
            UpdateQueue(ref fixedLoopCallbackAddTemp, ref fixedLoopCallbackRemoveTemp, ref fixedLoopCallback);
        }
        public const int InitCapacity = 16;
        public  void Init()
        {
            //secondLoopCallback = new List<UpdateVO>(InitCapacity);
            //secondLoopCallbackAddTemp = new List<UpdateVO>(InitCapacity);
            //secondLoopCallbackRemoveTemp = new List<UpdateVO>(InitCapacity);

            loopCallBack = new List<UpdateVO>(InitCapacity);
            loopCallBackAddTemp = new List<UpdateVO>(InitCapacity);
            loopCallBackRemoveTemp = new List<UpdateVO>(InitCapacity);

            timerLoopCallBack = new List<UpdateVO>(InitCapacity);
            timerLoopCallBackAddTemp = new List<UpdateVO>(InitCapacity);
            timerLoopCallBackRemoveTemp = new List<UpdateVO>(InitCapacity);

            fixedLoopCallback = new List<UpdateVO>(InitCapacity);
            fixedLoopCallbackAddTemp = new List<UpdateVO>(InitCapacity);
            fixedLoopCallbackRemoveTemp = new List<UpdateVO>(InitCapacity);

            lateLoopCallback = new List<UpdateVO>(InitCapacity);
            lateLoopCallbackAddTemp = new List<UpdateVO>(InitCapacity);
            lateLoopCallbackRemoveTemp = new List<UpdateVO>(InitCapacity);

            timeoutLoopCallBack = new List<UpdateVO>(32);
            timeoutLoopCallBackAddTemp = new List<UpdateVO>(32);
            timeoutLoopCallBackRemoveTemp = new List<UpdateVO>(32);

        }

        public  void LateLoop()
        {
            UpdateQueue(ref lateLoopCallbackAddTemp, ref lateLoopCallbackRemoveTemp, ref lateLoopCallback);
        }

        public  void Loop()
        {
            UpdateQueue(ref loopCallBackAddTemp, ref loopCallBackRemoveTemp, ref loopCallBack);
        }
        //public  void SecondLoop()
        //{
        //    UpdateQueue(ref loopCallBackAddTemp, ref loopCallBackRemoveTemp, ref loopCallBack);
        //}

        public  void RemoveFixedLoop(Action updater)
        {
            RemoveFromQueue(updater, ref fixedLoopCallbackAddTemp, ref fixedLoopCallbackRemoveTemp,
                ref fixedLoopCallback);
        }

        public  void RemoveLateLoop(Action updater)
        {
            RemoveFromQueue(updater, ref lateLoopCallbackAddTemp, ref lateLoopCallbackRemoveTemp, ref lateLoopCallback);
        }
        public void AddLoop(Action updater, int priority = 1000)
        {
            AddToQueue(updater, priority, ref loopCallBackAddTemp, ref loopCallBackRemoveTemp, ref loopCallBack);
        }
        public  void RemoveLoop(Action updater)
        {
            RemoveFromQueue(updater, ref loopCallBackAddTemp, ref loopCallBackRemoveTemp, ref loopCallBack);
        }

        public  void RemoveSecond(Action updater)
        {
            RemoveFromQueue(updater, ref timerLoopCallBackAddTemp, ref timerLoopCallBackRemoveTemp,
                ref timerLoopCallBack);
        }
        public  void RemoveTimer(Action updater)
        {
            RemoveFromQueue(updater, ref timerLoopCallBackAddTemp, ref timerLoopCallBackRemoveTemp,
                ref timerLoopCallBack);
        }
        public void RemoveTimeout(Action updater)
        {
            RemoveFromQueue(updater, ref timeoutLoopCallBackAddTemp, ref timeoutLoopCallBackRemoveTemp,
                ref timeoutLoopCallBack);
        }

        public void Start()
        {
            GameApp.Ins.GameAppShell.StartCoroutine(UpdateCoroutine());
            GameApp.Ins.GameAppShell.StartCoroutine(LateUpdateCoroutine());
            GameApp.Ins.GameAppShell.StartCoroutine(FixedUpdateCoroutine());
            //GameApp.Ins.GameAppShell.StartCoroutine(TimerUpdateCoroutine());
            //GameApp.Ins.GameAppShell.StartCoroutine(TimeoutUpdateCoroutine());
        }
        public void ClearAll()
        {
            loopCallBack.Clear();
            loopCallBackAddTemp.Clear();
            loopCallBackRemoveTemp.Clear();

            fixedLoopCallback.Clear();
            fixedLoopCallbackAddTemp.Clear();
            fixedLoopCallbackRemoveTemp.Clear();

            lateLoopCallback.Clear();
            lateLoopCallbackAddTemp.Clear();
            lateLoopCallbackRemoveTemp.Clear();


            timerLoopCallBack.Clear();
            timerLoopCallBackAddTemp.Clear();
            timerLoopCallBackRemoveTemp.Clear();

            timeoutLoopCallBack = new List<UpdateVO>(32);
            timeoutLoopCallBackAddTemp = new List<UpdateVO>(32);
            timeoutLoopCallBackRemoveTemp = new List<UpdateVO>(32);

        }
        public  void Stop()
        {
            Log.Debug("OnLoopManagerClose");
            ClearAll();
        }
        public void TimeoutLoop()
        {
            UpdateQueue(ref timeoutLoopCallBackAddTemp, ref timeoutLoopCallBackRemoveTemp, ref timeoutLoopCallBack,true);
        }

        public void TimerLoop()
        {
            UpdateQueue(ref timerLoopCallBackAddTemp, ref timerLoopCallBackRemoveTemp, ref timerLoopCallBack);
        }

        private  void AddToQueue(
            Action updater, int priority,
            ref List<UpdateVO> addTempList,
            ref List<UpdateVO> removeTempList,
            ref List<UpdateVO> list,
            float timer = 0,
            bool isSecond = false)
        {
            if(CheckRemoveFrom(removeTempList, updater))
            { 
                return;
            }
            if (IsContains(list, updater,out var updateVO))
            {
                updateVO.timer = timer;
                updateVO.isSecond = isSecond;
                return;
            }
            if (IsContains(addTempList, updater, out  updateVO))
            {
                updateVO.timer = timer;
                updateVO.isSecond = isSecond;
                return;
            }
            CheckAddTo(addTempList, updater, priority, timer, isSecond);
            //if(!CheckRemoveFrom(removeTempList, updater))
            //{
            //    CheckAddTo(addTempList, updater, priority, timer);
            //}

        }
        private  void UpdateQueue(
            Action updater, int priority,
            ref List<UpdateVO> addTempList,
            ref List<UpdateVO> list,
            float timer = 0)
        {
            if (IsContains(list, updater,out var updateVO))
            {
                updateVO.timer = timer;
                return;
            }
            if (IsContains(addTempList, updater, out  updateVO))
            {
                updateVO.timer = timer;
                return;
            }
        }

        private  bool CheckAddTo(List<UpdateVO> list, UpdateVO updateVo)
        {
            if (IsContains(list, updateVo)) return false;
            list.Add(updateVo);
            return true;
        }

        private  bool CheckAddTo(List<UpdateVO> list, Action updater, int priority, float timer = 0,bool isSecond = false)
        {
            if (IsContains(list, updater))
            {
                return false;
            }
            float addTime = Time.time;
            if (isSecond)
            {
                addTime = Mathf.RoundToInt(Time.time);
            }
            var vo = new UpdateVO(updater, priority, timer, Time.time);
            vo.isSecond = isSecond;
            list.Add(vo);
            return true;
        }

        private  bool CheckRemoveFrom(List<UpdateVO> list, Action updater)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].callBack == updater)
                {
                    list.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        private  IEnumerator FixedUpdateCoroutine()
        {
            while (GameApp.Ins.GameApplicationMainState == GameAppMainState.Playing)
            {
                yield return m_WaitForFixedUpdateCache;
                FunctionUtility.SafeCall(FixedLoop);

            }
        }

        private  UpdateVO GetFrom(List<UpdateVO> list, Action updater, int priority)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].callBack == updater)
                    return list[i];
            }

            return new UpdateVO(updater, priority, 0, -1);
        }

        private  void HandleRemove(ref List<UpdateVO> temp, ref List<UpdateVO> handleList)
        {
            for (int i = 0; i < temp.Count; i++)
            {
                handleList.Remove(temp[i]);
            }

            temp.Clear();
        }

        private  bool IsContains(List<UpdateVO> list, Action updater)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].callBack == updater)
                {
                    return true;
                }
            }

            return false;
        }

        private  bool IsContains(List<UpdateVO> list, Action updater, out UpdateVO updateVo)
        {
            updateVo = UpdateVO.Empty;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].callBack == updater)
                {
                    updateVo = list[i];
                    return true;
                }
            }

            return false;
        }

        private  bool IsContains(List<UpdateVO> list, UpdateVO updateVo)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Equals(updateVo))
                {
                    return true;
                }
            }

            return false;
        }

        private  IEnumerator LateUpdateCoroutine()
        {
            while (GameApp.Ins.GameApplicationMainState == GameAppMainState.Playing)
            {
                
                yield return m_WaitForEndOfFrameCache;
                FunctionUtility.SafeCall(LateLoop);

            }
        }

        private  void RemoveFromQueue(
            Action updater,
            ref List<UpdateVO> addTempList,
            ref List<UpdateVO> removeTempList,
            ref List<UpdateVO> list)
        {
            if (IsContains(list, updater, out var findVO))
            {
                CheckAddTo(removeTempList, findVO);
                return;
            }

            CheckRemoveFrom(addTempList, updater);
        }

        private  IEnumerator TimerUpdateCoroutine()
        {
            while (GameApp.Ins.GameApplicationMainState == GameAppMainState.Playing)
            {
                FunctionUtility.SafeCall(TimerLoop);
                yield return null;
            }
        }
        private  IEnumerator TimeoutUpdateCoroutine()
        {
            while (GameApp.Ins.GameApplicationMainState == GameAppMainState.Playing)
            {
                FunctionUtility.SafeCall(TimeoutLoop);
                yield return null;
            }
        }

        private  IEnumerator UpdateCoroutine()
        {
            while (GameApp.Ins.GameApplicationMainState == GameAppMainState.Playing)
            {
                FunctionUtility.SafeCall(Loop);
                FunctionUtility.SafeCall(TimerLoop);
                FunctionUtility.SafeCall(TimeoutLoop);
                //FunctionUtility.SafeCall(SecondLoop);
                yield return null;
                
            }
        }

        private void UpdateQueue(ref List<UpdateVO> addTemp,
            ref List<UpdateVO> removeTemp,
            ref List<UpdateVO> list, bool isRemove = false)
        {
            bool changed = false;
            for (int i = 0; i < removeTemp.Count; i++)
            {
                changed = true;
                list.Remove(removeTemp[i]);
            }

            removeTemp.Clear();
            for (int i = 0; i < addTemp.Count; i++)
            {
                changed = true;
                list.Add(addTemp[i]);
            }
            addTemp.Clear();
            if (changed) { list.Sort(UpdateVOCompare); }
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                var updater = list[i];
                float timeOffset = Time.time - updater.markTime;
                int timeCount = updater.timer<=0?1:(int)(timeOffset / updater.timer);
                float timeAdd = 0;
                for (int j = 0; j < timeCount; j++)
                {
                    timeAdd += updater.timer;                      
                    if (FunctionUtility.SafeCall(updater.callBack))
                    {
                        if (isRemove)
                        {
                            RemoveFromQueue(updater.callBack, ref addTemp, ref removeTemp, ref list);
                            break;
                        }
                    }
                }
                if (timeCount > 0)
                {
                    updater.markTime = updater.isSecond ? Mathf.RoundToInt(updater.markTime + timeAdd) : updater.markTime + timeAdd;
                }
               
            }
    
        }

        private  int UpdateVOCompare(UpdateVO v1, UpdateVO v2)
        {
            return v1.priority.CompareTo(v2.priority);
        }

        

        private class UpdateVO
        {
            public static  UpdateVO Empty = new UpdateVO(null, Int32.MaxValue, 0, -1);
            public Action callBack;
            public float markTime;
            public int priority;
            public float timer;
            public bool isSecond;

            public UpdateVO(Action cb, int priority, float timer, float createTime)
            {
                this.callBack = cb;
                this.priority = priority;
                this.timer = timer;
                this.markTime = createTime;
            }
        }
    }
}
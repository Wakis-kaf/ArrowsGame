using Framework.Runtime.LogSystem;
using Framework.Runtime.UI;

using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.MDebugger.UIView
{
    public class DebuggerLogTabNavView : View
    {
        //private UList2 m_ListLogInfo;
        private UContainer m_CtrLogTraceDetail;

        private int m_CurListLogContainerSizeIndex = 0;
        private LogLevel m_CurrentLevel;
        private int m_CurTraceDetailContainerSizeIndex = 0;
        private bool m_IsAllowScroll = true;
        private bool m_IsCollecting = true;

        private UList m_ListLogInfo;

        private List<Log.LogData> m_LogList;

        private int[] m_PcListLogContainerSize = new[]
        {
            440,
            -3,
            600,
        };

        private int[] m_PcTraceDetailContainerSize = new[]
        {
            220,
            663,
            60,
        };

        private int[] m_PhoneListLogContainerSize = new[]
                                        {
            730,
            290,
            890,
        };

        private int[] m_PhoneTraceDetailContainerSize = new[]
{
            220,
            660,
            60,
        };

        private string m_SearchInput;

        private UText m_TxtLogTraceDetail;
        private UButton m_UbtnClearAll;
        private USimpleTabBar m_UbtnError;
        private USimpleTabBar m_UbtnFatal;
        private USimpleTabBar m_UbtnInfo;
        private UButton m_UbtnTraceExpand;
        private USimpleTabBar m_UbtnWarn;
        private USimpleCheckBox m_UckbStopCollect;
        private USimpleCheckBox m_UckbStopScroll;
        private UInputField m_UifLogFilter;
        private USimpleTabBar m_UtbDebugInfo;

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            Log.RemoveLogReceive(OnLogDataReceived);
            Log.RemoveLogForgot(OnLogDataForgot);
        }

        protected override void OnInitUI()
        {
            base.OnInitUI();
            m_TxtLogTraceDetail = GetBindObject<UText>("utxtTranceDetail");
            //m_ListLogInfo = GetBindObject<UList>("ulistLog");
            m_ListLogInfo = GetBindObject<UList>("ulistLog");
            m_CtrLogTraceDetail = GetBindObject<UContainer>("uctrTraceDetail");
            m_UbtnTraceExpand = GetBindObject<UButton>("ubtnTraceExpand");
            m_UbtnTraceExpand.onClick.AddListener(TraceContainerResize);
            m_UbtnClearAll = GetBindObject<UButton>("ubtnClearAll");
            m_UckbStopScroll = GetBindObject<USimpleCheckBox>("uscbStopScroll");
            m_UckbStopCollect = GetBindObject<USimpleCheckBox>("uscbStopCollect");
            m_UtbDebugInfo = GetBindObject<USimpleTabBar>("ustbDebug");
            m_UbtnInfo = GetBindObject<USimpleTabBar>("ustbInfo");
            m_UbtnWarn = GetBindObject<USimpleTabBar>("ustbWarn");
            m_UbtnError = GetBindObject<USimpleTabBar>("ustbError");
            m_UbtnFatal = GetBindObject<USimpleTabBar>("ustbFatal");
            m_UifLogFilter = GetBindObject<UInputField>("uifLogFilter");
            m_UckbStopScroll.AddValueChanged(OnScrollChanged);
            m_UckbStopCollect.AddValueChanged(OnCollectChanged);
            m_UbtnClearAll.onClick.AddListener(OnClearAllClick);
            // 指定一个渲染器
            m_ListLogInfo.ListRenderType = typeof(DebuggerLogListRender);
            // 指定数据源
            m_LogList = Log.GetAllLogData();
            m_UtbDebugInfo.AddValueChanged((value) => { OnLogLevelChanged(LogLevel.DEBUG, value); });
            m_UbtnInfo.AddValueChanged((value) => { OnLogLevelChanged(LogLevel.INFO, value); });
            m_UbtnWarn.AddValueChanged((value) => { OnLogLevelChanged(LogLevel.WARN, value); });
            m_UbtnError.AddValueChanged((value) => { OnLogLevelChanged(LogLevel.ERROR, value); });
            m_UbtnFatal.AddValueChanged((value) => { OnLogLevelChanged(LogLevel.FATAL, value); });
            m_ListLogInfo.AddSelect((index) =>
            {
                Log.LogData data = (Log.LogData)m_ListLogInfo.GetDataAt(index);
                MessageDispatcher.Ins.Dispatch(MessageCode.msg_logdata_trance_show, data.GetMessage(500));
            });
            m_UifLogFilter.AddDebounceChanged(OnSearchChanged);
            Log.AddLogReceive(OnLogDataReceived);

            SubscribeEvent<string>(MessageCode.msg_logdata_trance_show, OnLogDataDetailShow);
            UpdateContainerBySizeIndex(m_CurListLogContainerSizeIndex);
        }

        private bool EnableLog(Log.LogData logData)
        {
            return (logData.logLevel & m_CurrentLevel) != 0 &&
                   (string.IsNullOrEmpty(m_SearchInput) || (logData.GetMessage().Contains(m_SearchInput)));
        }

        private bool IsNeedDispose()
        {
            return GameApp.Ins.GameApplicationMainState == GameAppMainState.Destroyed || DisplayGO == null || m_ListLogInfo == null ||
                   m_ListLogInfo.gameObject == null;
        }

        private void OnClearAllClick()
        {
            // 清除所有的历史记录
            m_ListLogInfo.ClearData();
        }

        private void OnCollectChanged(bool isStopCollect)
        {
            m_IsCollecting = !isStopCollect;
        }

        private void OnLogDataDetailShow(string stringTrace)
        {
            m_TxtLogTraceDetail.text = stringTrace;
        }

        private void OnLogDataForgot(Log.LogData log)
        {
            if (IsNeedDispose())
            {
                Dispose();
                return;
            }

            m_LogList.Remove(log);
            m_ListLogInfo?.RemoveData(log);
        }

        private void OnLogDataReceived(Log.LogData log)
        {
            if (IsNeedDispose())
            {
                Dispose();
                return;
            }

            if (!m_IsCollecting) return;
            m_LogList.Add(log);
            if (EnableLog(log))
            {
                m_ListLogInfo.AddData(log);
                if (m_IsAllowScroll)
                    m_ListLogInfo.MoveToEnd();
            }
        }

        private void OnLogLevelChanged(LogLevel logLevel, bool isActive)
        {
            if (isActive)
                m_CurrentLevel |= logLevel;
            else
            {
                m_CurrentLevel &= ~logLevel;
            }

            List<Log.LogData> NewlogList = new List<Log.LogData>();
            for (int i = 0; i < m_LogList.Count; i++)
            {
                var data = m_LogList[i];
                if (EnableLog(data))
                {
                    NewlogList.Add(data);
                }
            }

            m_ListLogInfo.SetDataSources(NewlogList);
        }

        private void OnScrollChanged(bool isStopScroll)
        {
            m_IsAllowScroll = !isStopScroll;
        }

        private void OnSearchChanged(string content)
        {
            List<Log.LogData> NewlogList = new List<Log.LogData>();
            m_SearchInput = content;
            for (int i = 0; i < m_LogList.Count; i++)
            {
                var data = m_LogList[i];
                if (EnableLog(data))
                {
                    NewlogList.Add(data);
                }
            }

            m_ListLogInfo.SetDataSources(NewlogList);
        }

        private void TraceContainerResize()
        {
            UpdateContainerBySizeIndex(m_CurTraceDetailContainerSizeIndex + 1);
        }

        private void UpdateContainerBySizeIndex(int index)
        {
            var siseArray = UIRoot.Instance.uiMode == UIMode.Phone ? m_PhoneTraceDetailContainerSize : m_PcTraceDetailContainerSize;
            m_CurTraceDetailContainerSizeIndex = index % siseArray.Length;
            //m_CurTraceDetailContainerSizeIndex =
            //    (m_CurTraceDetailContainerSizeIndex + 1) % siseArray.Length;
            m_CtrLogTraceDetail.RectTransform.anchoredPosition = Vector2.zero;
            m_CtrLogTraceDetail.Size = new Vector2(m_CtrLogTraceDetail.Size.x,
                siseArray[m_CurTraceDetailContainerSizeIndex]);
            m_CtrLogTraceDetail.RectTransform.SetLeft(0);
            m_CtrLogTraceDetail.RectTransform.SetRight(0);
            var logSizeArray = UIRoot.Instance.uiMode == UIMode.Phone ? m_PhoneListLogContainerSize : m_PcListLogContainerSize;
            if (UIRoot.Root.uiMode == UIMode.Pc)
            {
                m_ListLogInfo.RectTransform.anchoredPosition = new Vector2(0, -80);
            }
            else
            {
                m_ListLogInfo.RectTransform.anchoredPosition = new Vector2(0, 0);
            }

            m_ListLogInfo.Size = new Vector2(m_ListLogInfo.Size.x,
                logSizeArray[m_CurTraceDetailContainerSizeIndex]);
            if (UIRoot.Root.uiMode == UIMode.Pc)
            {
                m_ListLogInfo.RectTransform.SetLeft(0);
                m_ListLogInfo.RectTransform.SetRight(0);
            }
        }
    }
}
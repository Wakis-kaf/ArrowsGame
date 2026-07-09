using System;
using Framework.Runtime.LogSystem;
using Framework.Runtime.SystemEvent;
using Framework.Runtime.UI;
using UnityEngine;

namespace Framework.Runtime.MDebugger.UIView
{
    public class DebuggerLogListRender : UListDisplayUnit
    {
        private object m_LastData;
        private UButton m_UbtnDetail;
        private UText m_UptxtContent;

        protected override void OnShow()
        {
            //Debug.LogError("UListDisplayUnit OnShow");
        }
        protected override void OnHide()
        {
            //Debug.LogError("UListDisplayUnit OnHide");
        }
        protected override void OnGUI(object data)
        {
            base.OnGUI(data);
            if (m_LastData != data && data is Log.LogData logData)
            {
                m_UptxtContent.text = logData.GetShortMessage();
                m_UptxtContent.color = LogConfig.GetLevelColor(logData.logLevel);
                m_LastData = logData;
            }
        }

        protected override void OnInitUI()
        {
            base.OnInitUI();
            m_UptxtContent = GetBindObject<UText>("utxtContent");
            m_UbtnDetail = GetBindObject<UButton>("ubtnDetail");
            m_UbtnDetail.onClick.AddListener(OnBtnDetailClick);
        }

        private void OnBtnDetailClick()
        {
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_logdata_trance_show, ((Log.LogData)Data).GetMessage(500));
        }
    }
}
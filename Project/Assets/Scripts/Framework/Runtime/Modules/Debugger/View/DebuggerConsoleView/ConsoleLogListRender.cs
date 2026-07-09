using Framework.Runtime.LogSystem;
using Framework.Runtime.UI;

namespace Framework.Runtime.MDebugger.UIView
{
    public class ConsoleLogListRender : UListDisplayUnit
    {
        private UButton m_UbtnDetail;
        private UTMPText m_UptxtContent;

        protected override void OnGUI(object data)
        {
            base.OnGUI(data);
            if (data is GameConsole.ConsoleLogVO logVo)
            {
                m_UbtnDetail.gameObject.SetActive(false);
                m_UptxtContent.text = logVo.content;
                m_UptxtContent.color = LogConfig.GetLevelColor(logVo.logLevel);
            }
        }

        protected override void OnInitUI()
        {
            base.OnInitUI();
            m_UptxtContent = GetBindObject<UTMPText>("utmpTxtContent");
            m_UbtnDetail = GetBindObject<UButton>("ubtnDetail");
        }
    }
}
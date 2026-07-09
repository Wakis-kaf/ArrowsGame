using Framework.Runtime.MDebugger;
using Framework.Runtime.LogSystem;
using Framework.Runtime.UnitSystem.BIInterfaces;
using Framework.Runtime.UI;

namespace Framework.Runtime.MDebugger.UIView
{
    public class DebuggerConsoleTabNavView : View
    {
        private UButton m_BtnSubmit;
        private UInputField m_IFCmd;
        private UList m_ListConsoleItem;

        public override void OnDestroy()
        {
            base.OnDestroy();
            GameConsole.RemoveConsolePrintAgent();
            GameConsole.RemoveCMDAddAgent();
            GameConsole.RemoveCMDRemoveAgent();
        }

        protected override void OnInitUI()
        {
            base.OnInitUI();
            m_BtnSubmit = GetComponentInChildren<UButton>("BtnSubmit");
            m_ListConsoleItem = GetComponentInChildren<UList>("ListConsoleItem");
            m_IFCmd = GetComponentInChildren<UInputField>("IFCmd");
            GameConsole.Debug("Debug");
            GameConsole.Info("Info");
            GameConsole.Warning("Warning");
            GameConsole.Error("Error");
            GameConsole.Fatal("Fatal");
            m_ListConsoleItem.ListRenderType = typeof(ConsoleLogListRender);
            GameConsole.SetConsolePrintAgent(OnConsoleLogAdd);
            GameConsole.ClearPrintCache();

            GameConsole.SetCMDAddAgent(m_IFCmd.RegisterPrompt);
            GameConsole.SetCMDRemoveAgent(m_IFCmd.RegisterPrompt);
            GameConsole.ClearCMDCache();
            GameConsole.RegisterCMD("hello", R);
            GameConsole.RegisterCMD("hello msg2");
            GameConsole.RegisterCMD("hello wpd", R);
            GameConsole.RegisterCMD("hedao wpd", R);
            m_BtnSubmit.AddClick(() =>
            {
                if (!m_IFCmd.InvokePrompt())
                {
                    GameConsole.Error("无效输入指令 : " + m_IFCmd.text);
                }
                else
                {
                    GameConsole.Info("指令执行成功:" + m_IFCmd.text);
                }

                m_IFCmd.Clear();
            });
        }

        private void OnConsoleLogAdd(GameConsole.ConsoleLogVO logVo)
        {
            m_ListConsoleItem.AddData(logVo);
            m_ListConsoleItem.MoveToEnd();
        }

        private void R(string prefix, string[] agrs)
        {
            string msg = prefix;
            for (int i = 0; i < agrs.Length; i++)
            {
                msg += " " + agrs[i];
            }

            Log.Info(msg);
        }
    }
}
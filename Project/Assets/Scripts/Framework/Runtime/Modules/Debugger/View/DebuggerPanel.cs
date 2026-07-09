using Cysharp.Threading.Tasks;
using Framework.Runtime.Config;
using Framework.Runtime.UI;
using Framework.Runtime.UnitSystem.BIInterfaces;
using Framework.Utils;
using System;
using System.Collections;
using UnityEngine;

namespace Framework.Runtime.MDebugger.UIView
{
    /// <summary>
    /// 调试控制台窗口
    /// </summary>
    public class DebuggerPanel : Panel
    {
        private FpsCounter m_FpsCounter;
        private SimpleTabNavView m_TavTabNavView;
        public SimpleTabNavView TabNavView => m_TavTabNavView;
        private UnitFrameDebugger m_UnitFrameDebugger;
        private WaitForSeconds m_Waiter = new WaitForSeconds(0.1f);
        protected override void OnInit()
        {
            base.OnInit();
            //DisableUnit();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            m_TavTabNavView?.RemoveShowListener(OnTabNavShow);
        }

        public void OnUnitUpdate()
        {
            m_FpsCounter.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        protected override void OnInitUI()
        {
            base.OnInitUI();

            m_UnitFrameDebugger = DisplayGO.AddComponent<UnitFrameDebugger>();
            // 创建navigation
            var tabNavigation = m_UnitFrameDebugger.GetComponentInChild<UTabNavigation>("UTabNavigation");
            m_TavTabNavView = SimpleTabNavView.CreateNavTabView(tabNavigation);
            m_TavTabNavView.AddShowListener(OnTabNavShow);
            string logViewPath = UIRoot.Instance.uiMode == UIMode.Pc ? GlobalConstant.PcDebuggerLogLink : GlobalConstant.PhoneDebuggerLogLink;
            m_TavTabNavView.SetTabNavView<DebuggerLogTabNavView>(logViewPath, 1, "日志");
            string envViewPath = UIRoot.Instance.uiMode == UIMode.Pc ? GlobalConstant.PcDebuggerEnvViewLink : GlobalConstant.PhoneDebuggerEnvViewLink;
            m_TavTabNavView.SetTabNavView<EnvironmentView>(envViewPath, 2, "环境");
            m_TavTabNavView.AddSelect(0, OnFpsTabSelect);
            m_TavTabNavView.SwitchTab(1); // 切换到第一个tab
            DisableBgMask();
            m_FpsCounter = new FpsCounter(0.1f);
            GameApp.Ins.GameAppShell.StartCoroutine(FpsCounterCoroutine());
            GameApp.Ins.LoopManager.AddLoop(OnUnitUpdate);
            GameApp.Debugger.OnDebuggerPanelLoaded?.Invoke();


        }

        protected override void OnShow()
        {
            base.OnShow();
            HideBgMask();
        }

        private IEnumerator FpsCounterCoroutine()
        {
            while (true)
            {
                if (DisplayGO)
                {
                    if (m_UnitFrameDebugger.IsExpand)
                    {
                        m_TavTabNavView.SetTabLabel(0,
                            Utility.StringUtil.Concat("(x)FPS:", m_FpsCounter.GetFps(0).ToString())); // 设置帧率
                    }
                    else
                    {
                        m_UnitFrameDebugger.SetFoldFps(m_FpsCounter.GetFps(0));
                    }
                }
                yield return m_Waiter;
            }
        }

        private void OnFpsTabSelect()
        {
            m_UnitFrameDebugger.Fold();
        }

        private void OnTabNavShow()
        {
            m_TavTabNavView.SwitchTab(1); // 切换到第一个tab
        }

        public void Fold()
        {
            m_UnitFrameDebugger?.Fold();
        }
    }
}
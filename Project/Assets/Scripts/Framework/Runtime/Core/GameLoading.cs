using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Framework.Runtime.Config;
using Framework.Runtime.UI;
using System;

namespace Framework.Runtime
{
    public class GameLoading
    {
        private static GameLoading m_Instance;
        private bool enableCreate = false;
        private Panel m_LoadingPanel;
        private GameLoadingOption needLoadingOption;

        public GameLoading()
        {
        }

        public static GameLoading Ins
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new GameLoading();
                }
                return m_Instance;
            }
        }

        public void AppPopupUpdate(GameAppMessage appMessage)
        {
        }

        public void AppUpdate(GameAppMessage appMessage)
        {
            if (appMessage.MessageCode == GameAppMessage.code_gameSytem_start)
            {
                this.enableCreate = true;
                OpenLoadingCreate();
            }
        }

        public void CloseLoading()
        {
            if (m_LoadingPanel != null)
            {
                m_LoadingPanel.CloseWindow();
            }
        }


        public static void Close()
        {
            if (m_Instance == null) { return; }
            m_Instance.CloseLoading();
            m_Instance.m_LoadingPanel = null;
            m_Instance = null;
        }

        public void Init()
        {
            MessageDispatcher.Ins.Subscribe<float, float, string, Action>(MessageCode.msg_gameLoading_set, this.SetLoading);
        }
        public void Reset()
        {
            OpenLoading(new GameLoadingOption()
            {
                tipText = "",
                timer = 0,
                targetValue = 0,
                completeCb = null
            });
        }
        public void SetLoading(float timer, float targetValue, string tip, Action cb = null)
        {
            OpenLoading(new GameLoadingOption()
            {
                tipText = tip,
                timer = timer,
                targetValue = targetValue,
                completeCb = cb,
            });
        }
        public void SetLoading(float timer, float targetValue, string tip, Action cb = null, Type loadintPanelType = null)
        {
            OpenLoading(new GameLoadingOption()
            {
                tipText = tip,
                timer = timer,
                targetValue = targetValue,
                completeCb = cb,
                loadingPanelType = loadintPanelType ?? typeof(GameLoadingPanel)
            });
        }

        public void OpenLoading(GameLoadingOption option)
        {
            this.needLoadingOption = option;
            needLoadingOption.loadingPanelType = needLoadingOption.loadingPanelType ?? typeof(GameLoadingPanel);
            this.OpenLoadingCreate();
        }

        public void Start()
        {
        }

        private void OpenLoadingCreate()
        {
            if (!enableCreate) return;
            if (m_LoadingPanel != null && m_LoadingPanel.GetType() != needLoadingOption.loadingPanelType)
            {
                CloseLoading();
            }
            Type openLoadintType = needLoadingOption.loadingPanelType;
            m_LoadingPanel = PanelManager.Ins.OpenPanel(openLoadintType, GlobalConstant.PATH_RESOURCES_GAME_LOADING_PANEL,
                        GlobalConstant.LAYER_LOADING);
            if (needLoadingOption != null)
            {
                m_LoadingPanel.SetData(this.needLoadingOption);
            }
        }
    }

    public class GameLoadingOption
    {
        public Action completeCb;
        public bool isShowAntiAddition = false;
        public bool isShowCopyright = false;
        public int maxValue = 100;
        public int minValue = 0;
        public float targetValue;
        public float timer;
        public string tipText = "";
        public Action<float> updateCb;
        public Type loadingPanelType = typeof(GameLoadingPanel);

    }


}
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
namespace Framework.Runtime.MLanAndTheme
{

    public abstract class EnvAdapterComponent : MonoBehaviour, ILanAdapter, IThemeAdapter
    {

        [LabelText("是否开启多语言")]
        [SerializeField]
        private bool m_IsEnableLan = false;
        [SerializeField]
        [ShowIf("m_IsEnableLan", true)]
        [LabelText("多语言类型")]
        private LanguageType m_LanType = LanguageType.Zh_CN;
        [SerializeField]
        [ShowIf("m_IsEnableLan", true)]
        [LabelText("多语言ID")]
        private string m_LanId = "";
        [LabelText("是否开启主题")]
        [SerializeField]
        private bool m_IsEnableTheme = false;
        [SerializeField]
        [LabelText("主题类型")]
        [ShowIf("m_IsEnableTheme", true)]
        private ThemeType m_ThemeType = ThemeType.FollowEnv;
        [SerializeField]
        [LabelText("当前主题")]
        [ShowIf("m_IsEnableTheme", true)]
        [ReadOnly]
        private ThemeType m_CurrentThemeType = ThemeType.None;
        [SerializeField]
        [LabelText("主题项ID")]
        [ShowIf("m_IsEnableTheme", true)]
        private string m_ThemeItemId = "";

        public bool IsEnableLan { get => m_IsEnableLan; set => m_IsEnableLan = value; }
        public LanguageType LanType { get => m_LanType; set => m_LanType = value; }

        public string LanId => m_LanId;

        public bool IsEnableTheme { get => m_IsEnableTheme; set => m_IsEnableTheme = value; }
        public ThemeType UseThemeType { get => m_ThemeType; set => m_ThemeType = value; }
        public ThemeType CurrentThemeType { get => m_CurrentThemeType; set => m_CurrentThemeType = value; }
        public string ThemeItemId => m_ThemeItemId;

        private void Awake()
        {
            OnAdapterAwake();
            if (m_IsEnableLan)
            {
                GameApp.Lan2LocalManager?.RegisterLanAdapter(this);
            }
            if (m_IsEnableTheme)
            {
                GameApp.Theme2LocalManager?.RegisterThemeAdapter(this);

            }
        }
        private void OnDestroy()
        {
            OnAdapterDestroy();
            GameApp.Lan2LocalManager?.UnRegisterLanAdapter(this);
        }
        protected virtual void OnAdapterAwake()
        {

        }
        protected virtual void OnAdapterDestroy()
        {

        }

        public void SetCurrentLanType(LanguageType currentLanType)
        {
            m_LanType = currentLanType;
            OnRefreshLanVisual();
        }
        protected virtual void OnRefreshLanVisual()
        {

        }

        public void SetCurrentThemeType(ThemeType currentThemeType)
        {
            m_CurrentThemeType = currentThemeType;
            OnRefreshThemeVisual(GameApp.Theme2LocalManager.FindThemeItem(CurrentThemeType, ThemeItemId));
        }
        protected virtual void OnRefreshThemeVisual(CfgThemeItem themeItem)
        {

        }
    }
}

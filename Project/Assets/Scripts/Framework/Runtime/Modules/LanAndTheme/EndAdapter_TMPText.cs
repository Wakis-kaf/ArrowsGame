using System.Collections;
using System.Collections.Generic;
using Framework.Runtime.UI;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Framework.Runtime.MLanAndTheme
{
    public class EndAdapter_TMPText : EnvAdapterComponent
    {
        [SerializeField, ReadOnly]
        private TMP_Text m_TMPText;
        protected override void OnAdapterAwake()
        {
            base.OnAdapterAwake();
            m_TMPText = gameObject.GetComponent<TMP_Text>();
        }
        protected override void OnRefreshThemeVisual(CfgThemeItem themeItem)
        {
            if (themeItem == null) return;
            m_TMPText.color = UIUtil.Hex2Color(themeItem.hexColor);

        }
    }
}
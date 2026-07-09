using System.Collections;
using System.Collections.Generic;
using Framework.Runtime.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
namespace Framework.Runtime.MLanAndTheme
{
    public class EnvAdapter_Image : EnvAdapterComponent
    {
        [SerializeField, ReadOnly]
        private Image m_TargetImage;
        protected override void OnAdapterAwake()
        {
            base.OnAdapterAwake();
            m_TargetImage = gameObject.GetComponent<Image>();
        }
        protected override void OnRefreshThemeVisual(CfgThemeItem themeItem)
        {
            if (themeItem == null) return;
            m_TargetImage.color = UIUtil.Hex2Color(themeItem.hexColor);

        }
    }
}
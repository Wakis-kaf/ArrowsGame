using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
namespace Framework.Runtime.MLanAndTheme
{
    public enum LanguageType
    {
        // 默认/未知
        [LabelText("未知(Unknown)")]
        Unknown = 0,

        // 中文系列
        [LabelText("简体中文(Zh_CN)")]
        Zh_CN = 1,
        [LabelText("繁体中文(Zh_TW)")]
        Zh_TW = 2,
        [LabelText("繁体中文(Zh_HK)")]
        Zh_HK = 3,

        // 核心常用语言
        [LabelText("英语_美国(En_US)")]
        En_US = 10,
        [LabelText("英语_英国(En_GB)")]
        En_GB = 11,
        [LabelText("日语(Ja_JP)")]
        Ja_JP = 12,
        [LabelText("韩语(Ko_KR)")]
        Ko_KR = 13,

        // 欧洲主要语言
        [LabelText("法语(Fr_FR)")]
        Fr_FR = 20,
        [LabelText("德语(De_DE)")]
        De_DE = 21,
        [LabelText("西班牙语(Es_ES)")]
        Es_ES = 22,
        [LabelText("意大利语(It_IT)")]
        It_IT = 23,
        [LabelText("俄语(Ru_RU)")]
        Ru_RU = 24,
        [LabelText("葡萄牙语_葡萄牙(Pt_PT)")]
        Pt_PT = 25,
        [LabelText("葡萄牙语_巴西(Pt_BR)")]
        Pt_BR = 26,

        // 东南亚与南亚主要语言
        [LabelText("越南语(Vi_VN)")]
        Vi_VN = 30,
        [LabelText("泰语(Th_TH)")]
        Th_TH = 31,
        [LabelText("印尼语(Id_ID)")]
        Id_ID = 32,
        [LabelText("马来语(Ms_MY)")]
        Ms_MY = 33,
        [LabelText("印地语(Hi_IN)")]
        Hi_IN = 34,

        // 其他中东及高频语言
        [LabelText("阿拉伯语(Ar_SA)")]
        Ar_SA = 40,
        [LabelText("土耳其语(Tr_TR)")]
        Tr_TR = 41,
        [LabelText("波兰语(Pl_PL)")]
        Pl_PL = 42
    }
    public enum ThemeType
    {
        [LabelText("无主题")]
        None = -1,
        [LabelText("跟随环境主题")]
        FollowEnv = 0,
        [LabelText("简约")]
        Theme1 = 1,
        [LabelText("夜间")]
        Theme2 = 2,

    }
}
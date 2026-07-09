using System;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Framework.Runtime.UI.Editor
{
    public static class UnitUIDefaultStyle
    {
        public static string Style = "Cartoon/";

        public static class TMPFontStyle
        {
            private static TMP_FontAsset m_SIMKAI_SDF;

            public static TMP_FontAsset SIMKAI_SDF
            {
                get
                {
                    if (m_SIMKAI_SDF == null)
                        m_SIMKAI_SDF = Resources.Load<TMP_FontAsset>("font/SIMKAI_SDF");
                    if (m_SIMKAI_SDF == null)
                        throw new Exception("SIMKAI_SDF null error");
                    return m_SIMKAI_SDF;
                }
            }
        }

        public static class DefaultBtnStyle
        {
            public static readonly Vector2 NormalSize = new Vector2(160, 50);
        }

        public static class DefaultTextStyle
        {
            public static readonly Vector2 NormalSize = new Vector2(200, 50);
            public static readonly int NormalFontSize = 22;
            public static readonly Color DefaulColor = Color.black;
        }

        public static class DefaultImageStyle
        {
            public static readonly Vector2 NormalSize = new Vector2(100, 100);
            public static readonly Color DefaultColor = Color.white;

            public static readonly Sprite DefaultUISprite =
                AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            public static readonly Sprite DefaultUIBackground =
                AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        }

        public static class DefaultScrollBarStyle
        {
            public static readonly Vector2 Size = new Vector2(200, 30);
            public static Sprite ScrollBarSprite => DefaultImageStyle.DefaultUIBackground;
            public static Sprite HandleSprite => DefaultImageStyle.DefaultUIBackground;
            public static readonly Color HandleColor = Color.gray;
        }

        public static class DefaultPanelStyle
        {
            public static readonly Vector2 Size = new Vector2(800, 600);
            public static readonly Vector2 ScrollBarSize = new Vector2(30, 30);
            public static readonly Vector2 ContentSize = new Vector2(900, 900);
            public static readonly Color ContentColor = Color.grey;
        }

        public static class DefaultListStyle
        {
            public static readonly Vector2 Size = new Vector2(1200, 300);
            public static readonly Vector2 ScrollBarSize = new Vector2(30, 30);
            public static readonly Vector2 ContentSize = new Vector2(1200, 300);
            public static readonly Color ContentColor = Color.grey;
        }

        public static class DefaultCheckBoxStyle
        {
            public static readonly Vector2 Size = new Vector2(150, 40);
            public static readonly Vector2 BoxSize = new Vector2(40, 40);
            public static readonly Vector2 BoxMarkSize = new Vector2(40, 40);
            public static readonly float LabelSize = 22;

            public static readonly Sprite BoxSprite =
                Resources.Load<Sprite>(Style + "CheckBox/check_toggle_bg");

            public static readonly Sprite MarkSprite =
                Resources.Load<Sprite>(Style + "CheckBox/checkmark");
        }

        public static class DefaultTabBarStyle
        {
            public static readonly Vector2 Size = new Vector2(150, 80);
            public static readonly float LabelSize = 22;

            public static readonly Sprite BoxSprite =
                Resources.Load<Sprite>(Style + "CheckBox/check_toggle_bg");

            public static readonly Sprite MarkSprite =
                Resources.Load<Sprite>(Style + "CheckBox/checkmark");

            public static Sprite TabSprite => DefaultImageStyle.DefaultUIBackground;
        }
        // public static readonly Sprite DefaultUISprite =
        //     AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        //
        // public static readonly Sprite DefaultUIBackground =
        //     AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");

        public static readonly Vector2 UCheckBox_Size = new Vector2(150, 40);
        public static readonly Vector2 UCheckBox_Background_Size = new Vector2(40, 40);
        public static readonly Vector2 UCheckBox_CheckMark_Size = new Vector2(40, 40);
        public static readonly float UCheckBox_Label_Left = 40;
        public static readonly Vector2 UScrollBarSize = new Vector2(160, 20);
        public static readonly Vector2 UButtonNormalSize = new Vector2(160, 50);
        public static readonly Vector2 UPanelSize = new Vector2(800, 600);
        public static readonly Vector2 UPanelScrollBarSize = new Vector2(30, 30);

        public static Color UScrollBarHandleColor
        {
            get
            {
                //ColorUtility.TryParseHtmlString("", out Color color);
                return Color.gray;
            }
        }

        public static Color UPanelContentColor = new Color(Color.gray.r, Color.gray.b, Color.gray.a, 0.8f);
    }
}
using UnityEngine;
using UnityEditor;

namespace GridsSpaceEditor.UI.Shared
{
    public static class SectionHeader
    {
        public static void Draw(string text)
        {
            Draw(text, new[] { GUILayout.Height(22) });
        }

        public static void Draw(string text, GUILayoutOption[] options)
        {
            Rect r = GUILayoutUtility.GetRect(0, 22, options);
            EditorGUI.DrawRect(r, ColorPalette.SectionHeader);
            GUI.Label(new Rect(r.x + 5, r.y + 2, r.width, r.height), text, EditorStyles.boldLabel);
        }
    }
}

using UnityEngine;

namespace GridsSpaceEditor.UI.Shared
{
    public static class ColorPalette
    {
        public static readonly Color Background = new Color(0.12f, 0.12f, 0.12f);
        public static readonly Color GridLine = new Color(0.2f, 0.2f, 0.2f);
        public static readonly Color AxisLine = new Color(0.8f, 0.2f, 0.2f, 0.8f);
        public static readonly Color OriginMarker = Color.yellow;

        public static readonly Color CellFill = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        public static readonly Color CellSelected = new Color(0f, 0.7f, 1f, 0.8f);
        public static readonly Color CellBorder = new Color(1f, 1f, 1f, 0.2f);

        public static readonly Color BoxSelectAdd = new Color(0f, 1f, 0f, 0.15f);
        public static readonly Color BoxSelectRemove = new Color(1f, 0f, 0f, 0.15f);

        public static readonly Color PortInput = new Color(0f, 0.7f, 1f);
        public static readonly Color PortOutput = new Color(1f, 0.2f, 0.2f);
        public static readonly Color PortSelected = Color.yellow;

        public static readonly Color BrushCursorAdd = Color.cyan;
        public static readonly Color BrushCursorRemove = Color.red;

        public static readonly Color SectionHeader = new Color(0f, 0f, 0f, 0.2f);
        public static readonly Color DangerButton = new Color(1f, 0.4f, 0.4f);
    }
}

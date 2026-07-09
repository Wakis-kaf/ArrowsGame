using UnityEngine;
using UnityEditor;

public class SpriteEditorShortcut
{
    [MenuItem("Assets/打开雪碧图编辑器", false, 301)]
    static void OpenSpriteEditor()
    {
        // 直接使用Unity的内部方法打开Sprite Editor
        EditorApplication.ExecuteMenuItem("Window/2D/Sprite Editor");
    }
}
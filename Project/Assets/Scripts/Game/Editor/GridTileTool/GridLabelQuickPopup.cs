using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Game.Modules.GModuleScene;

public class GridLabelQuickPopup : EditorWindow
{
    private System.Action<List<int>> onConfirm;
    private int cellIndex;
    private GridCellMeta editingMeta;
    private List<LabelType> labelTypes;
    private List<int> choices = new List<int>();
    private Vector2 scroll;

    public static void Show(int idx, GridCellMeta meta, List<LabelType> types, System.Action<List<int>> confirm)
    {
        var win = CreateInstance<GridLabelQuickPopup>();
        win.titleContent = new GUIContent("快速标签");
        win.cellIndex = idx;
        win.editingMeta = meta;
        win.labelTypes = types ?? new List<LabelType>();
        win.onConfirm = confirm;

        if (meta != null && meta.labelTypeIds != null)
            win.choices = new List<int>(meta.labelTypeIds);
        else
            win.choices = new List<int>();

        var main = SceneView.lastActiveSceneView;
        if (main != null)
        {
            var center = main.position.center;
            win.position = new Rect(center.x - 120, center.y - 80, 240, 180);
        }
        else win.position = new Rect(Screen.width / 2, Screen.height / 2, 240, 180);
        win.ShowPopup();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField($"格子索引: {cellIndex}");
        EditorGUILayout.Space();

        if (labelTypes == null || labelTypes.Count == 0)
        {
            EditorGUILayout.HelpBox("未定义任何标签类型。请在主窗口上方的标签类型管理中添加。", MessageType.Warning);
            if (GUILayout.Button("关闭")) Close();
            return;
        }

        EditorGUILayout.LabelField("选择标签类型（多选）：", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        for (int i = 0; i < labelTypes.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            var rect = GUILayoutUtility.GetRect(18, 18, GUILayout.Width(20));
            EditorGUI.DrawRect(rect, labelTypes[i].color);
            bool toggled = choices.Contains(i);
            bool newToggled = EditorGUILayout.ToggleLeft(labelTypes[i].name, toggled);

            if (newToggled && !toggled)
                choices.Add(i);
            else if (!newToggled && toggled)
                choices.Remove(i);

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("确定")) { onConfirm?.Invoke(choices); Close(); }
        if (GUILayout.Button("清除")) { onConfirm?.Invoke(new List<int>()); Close(); }
        if (GUILayout.Button("取消")) Close();
        EditorGUILayout.EndHorizontal();

        if (Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter) { onConfirm?.Invoke(choices); Close(); Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.Escape) { Close(); Event.current.Use(); }
        }
    }
}
using System;
using GridsSpaceEditor.Core;
using GridsSpaceEditor.Data.Enums;
using GridsSpaceEditor.Data.Models;
using GridsSpaceEditor.UI.Shared;
using UnityEditor;
using UnityEngine;

namespace GridsSpaceEditor.UI.Tabs.PortTab
{
    public class PortLibraryPanel
    {
        private PortManager m_PortManager;
        private Action m_OnChanged;
        private int m_EditingPresetIndex = -1;

        public PortLibraryPanel(PortManager portManager, Action onChanged)
        {
            m_PortManager = portManager;
            m_OnChanged = onChanged;
        }

        public void Draw()
        {
            SectionHeader.Draw("预设库");

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+ 新建预设", GUILayout.Width(100)))
            {
                CreateNewPreset();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            if (m_PortManager.Templates.Count == 0)
            {
                EditorGUILayout.HelpBox("暂无预设。点击「新建预设」创建新端口预设。", MessageType.Info);
                return;
            }

            for (int i = 0; i < m_PortManager.Templates.Count; i++)
            {
                DrawPresetItem(i);
            }
        }

        private void CreateNewPreset()
        {
            var newPreset = new PortInstance
            {
                PortID = "NewPort",
                IOType = PortIOType.输入,
                Side = EdgeSide.顶部,
                PresetName = "新建预设"
            };
            m_PortManager.CreateNewPreset(newPreset);
            m_OnChanged?.Invoke();
        }

        private void DrawPresetItem(int index)
        {
            var template = m_PortManager.Templates[index];
            bool isEditing = m_EditingPresetIndex == index;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            string btnText = isEditing ? "▼ 收起" : $"▶ 编辑: {template.PortID}";
            if (GUILayout.Button(btnText))
            {
                m_EditingPresetIndex = isEditing ? -1 : index;
            }

            if (GUILayout.Button("应用", GUILayout.Width(50)))
            {
                m_PortManager.ApplyPreset(template);
                m_OnChanged?.Invoke();
            }

            if (GUILayout.Button("删除", GUILayout.Width(50)))
            {
                m_PortManager.DeletePreset(index);
                m_EditingPresetIndex = -1;
                m_OnChanged?.Invoke();
                GUIUtility.ExitGUI();
                return;
            }

            EditorGUILayout.EndHorizontal();

            if (isEditing)
            {
                DrawPresetFields(template);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPresetFields(PortInstance port)
        {
            EditorGUI.BeginChangeCheck();

            port.PortID = EditorGUILayout.TextField("端口 ID", port.PortID);
            port.IOType = (PortIOType)EditorGUILayout.EnumPopup("类型", port.IOType);

            EditorGUILayout.Space(5);

            if (port.IOType == PortIOType.输入)
            {
                port.InputFilter = EditorGUILayout.TextField("过滤器", port.InputFilter);
                port.InputDescription = EditorGUILayout.TextField("描述", port.InputDescription);
            }
            else
            {
                port.OutputType = EditorGUILayout.TextField("数据类型", port.OutputType);
                port.OutputDescription = EditorGUILayout.TextField("描述", port.OutputDescription);
            }

            port.PortDescription = EditorGUILayout.TextArea(port.PortDescription, GUILayout.Height(40));

            if (EditorGUI.EndChangeCheck())
            {
                m_PortManager.UpdatePreset(m_EditingPresetIndex, port);
            }
        }
    }
}

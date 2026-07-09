using System.Linq;
using GridsSpaceEditor.Data.Models;
using GridsSpaceEditor.UI.Shared;
using UnityEditor;
using UnityEngine;

namespace GridsSpaceEditor.UI.Tabs.GridTab
{
    public class PropertiesTab
    {
        private SystemData m_SystemData;

        public event System.Action OnSystemDataChanged;

        public PropertiesTab(SystemData systemData)
        {
            m_SystemData = systemData;
        }

        public void Draw()
        {
            DrawTypeConfig();
            GUILayout.Space(15);
            DrawTemplateConfig();
        }

        private void DrawTypeConfig()
        {
            SectionHeader.Draw("类型配置");

            while (m_SystemData.TypeLocks.Count < m_SystemData.GridTypes.Count)
                m_SystemData.TypeLocks.Add(true);

            for (int i = 0; i < m_SystemData.GridTypes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                m_SystemData.TypeLocks[i] = EditorGUILayout.Toggle(m_SystemData.TypeLocks[i], GUILayout.Width(20));

                if (m_SystemData.TypeLocks[i])
                    EditorGUILayout.LabelField(m_SystemData.GridTypes[i]);
                else
                {
                    m_SystemData.GridTypes[i] = EditorGUILayout.TextField(m_SystemData.GridTypes[i]);
                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        m_SystemData.GridTypes.RemoveAt(i);
                        m_SystemData.TypeLocks.RemoveAt(i);
                        OnSystemDataChanged?.Invoke();
                        GUIUtility.ExitGUI();
                        return;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("添加类型"))
            {
                m_SystemData.GridTypes.Add("NewType");
                m_SystemData.TypeLocks.Add(false);
                OnSystemDataChanged?.Invoke();
            }
        }

        private void DrawTemplateConfig()
        {
            SectionHeader.Draw("属性模版");

            while (m_SystemData.TemplateLocks.Count < m_SystemData.Templates.Count)
                m_SystemData.TemplateLocks.Add(true);

            for (int i = 0; i < m_SystemData.Templates.Count; i++)
            {
                var t = m_SystemData.Templates[i];
                EditorGUILayout.BeginVertical(GUI.skin.box);

                m_SystemData.TemplateLocks[i] = EditorGUILayout.ToggleLeft(
                    m_SystemData.TemplateLocks[i] ? "已锁定" : "已解锁",
                    m_SystemData.TemplateLocks[i]);

                if (!m_SystemData.TemplateLocks[i])
                {
                    t.Name = EditorGUILayout.TextField("模版名", t.Name);
                    t.Description = EditorGUILayout.TextField("描述", t.Description);

                    int idx = Mathf.Max(0, m_SystemData.GridTypes.IndexOf(t.Type));
                    idx = EditorGUILayout.Popup("默认类型", idx, m_SystemData.GridTypes.ToArray());

                    if (m_SystemData.GridTypes.Count > 0)
                        t.Type = m_SystemData.GridTypes[idx];

                    if (GUILayout.Button("删除模版"))
                    {
                        m_SystemData.Templates.RemoveAt(i);
                        m_SystemData.TemplateLocks.RemoveAt(i);
                        OnSystemDataChanged?.Invoke();
                        return;
                    }
                }
                else
                {
                    EditorGUILayout.LabelField($"{t.Name} ({t.Type})");
                }

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("添加模版"))
            {
                m_SystemData.Templates.Add(new GridCellData { Name = "NewTmpl" });
                m_SystemData.TemplateLocks.Add(false);
                OnSystemDataChanged?.Invoke();
            }

            if (GUI.changed)
                OnSystemDataChanged?.Invoke();
        }
    }
}

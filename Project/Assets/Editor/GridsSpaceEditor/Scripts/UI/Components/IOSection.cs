using System.IO;
using GridsSpaceEditor.Core;
using GridsSpaceEditor.Data.Models;
using UnityEditor;
using UnityEngine;

namespace GridsSpaceEditor.UI.Components
{
    public class IOSection
    {
        private SystemData m_SystemData;
        private string m_ExportFileName = "NewGridData";
        private string m_CurrentShapeCode = "";
        private string m_ImportShapeCode = "";
        private ShapeCodeEngine m_ShapeCodeEngine;

        public string ExportFileName
        {
            get => m_ExportFileName;
            set => m_ExportFileName = value;
        }

        public event System.Action OnExportRequested;
        public event System.Action OnImportRequested;
        public event System.Action OnSystemDataChanged;
        public event System.Action OnShapeCodeImported;

        public IOSection(SystemData systemData)
        {
            m_SystemData = systemData;
        }

        public void SetShapeCodeEngine(ShapeCodeEngine engine)
        {
            m_ShapeCodeEngine = engine;
        }

        public void UpdateCurrentShapeCode(string code)
        {
            m_CurrentShapeCode = code;
        }

        /// <param name="panelInnerWidth">右侧面板内容区可用宽度（与窗口分割一致，勿用 currentViewWidth）</param>
        public void Draw(float panelInnerWidth)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.BeginHorizontal();
            m_SystemData.ExportFolderPath = EditorGUILayout.TextField("导出目录", m_SystemData.ExportFolderPath);
            if (GUILayout.Button("选择", GUILayout.Width(45)))
            {
                string path = EditorUtility.OpenFolderPanel("选择导出目录", m_SystemData.ExportFolderPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                        path = "Assets" + path.Substring(Application.dataPath.Length);
                    m_SystemData.ExportFolderPath = path;
                    OnSystemDataChanged?.Invoke();
                }
            }
            EditorGUILayout.EndHorizontal();

            m_ExportFileName = EditorGUILayout.TextField("默认文件名", m_ExportFileName);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("导出 JSON"))
            {
                OnExportRequested?.Invoke();
            }
            if (GUILayout.Button("加载 JSON"))
            {
                OnImportRequested?.Invoke();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("当前形状码", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
            GUILayout.FlexibleSpace();
            bool copied = false;
            if (GUILayout.Button("复制", EditorStyles.miniButton, GUILayout.Width(45)))
            {
                if (!string.IsNullOrEmpty(m_CurrentShapeCode))
                {
                    EditorGUIUtility.systemCopyBuffer = m_CurrentShapeCode;
                    copied = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            GUIStyle textAreaStyle = new GUIStyle(GUI.skin.textArea) { wordWrap = true };
            float wrapWidth = Mathf.Max(50f, panelInnerWidth);

            float requiredHeight = string.IsNullOrEmpty(m_CurrentShapeCode)
                ? 50f
                : textAreaStyle.CalcHeight(new GUIContent(m_CurrentShapeCode), wrapWidth);
            Rect selectableRect = GUILayoutUtility.GetRect(0, Mathf.Max(50f, requiredHeight), GUILayout.ExpandWidth(true));
            EditorGUI.SelectableLabel(selectableRect, m_CurrentShapeCode, textAreaStyle);

            if (copied)
            {
                EditorGUILayout.LabelField("已复制到剪贴板", EditorStyles.miniLabel);
            }

            GUILayout.Space(6);
            EditorGUILayout.LabelField("导入形状码", EditorStyles.boldLabel);
            float importHeight = string.IsNullOrEmpty(m_ImportShapeCode)
                ? 50f
                : textAreaStyle.CalcHeight(new GUIContent(m_ImportShapeCode), wrapWidth);
            m_ImportShapeCode = EditorGUI.TextArea(
                GUILayoutUtility.GetRect(0, Mathf.Max(50f, importHeight), GUILayout.ExpandWidth(true)),
                m_ImportShapeCode, textAreaStyle);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("导入"))
            {
                ImportShapeCode();
            }
            if (GUILayout.Button("清空"))
            {
                m_ImportShapeCode = "";
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void ImportShapeCode()
        {
            if (m_ShapeCodeEngine == null)
            {
                EditorUtility.DisplayDialog("错误", "形状码引擎未初始化", "确定");
                return;
            }

            m_ShapeCodeEngine.Import(m_ImportShapeCode, () =>
            {
                OnShapeCodeImported?.Invoke();
            });
        }

        public void DrawSimple()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.BeginHorizontal();
            m_SystemData.ExportFolderPath = EditorGUILayout.TextField("导出目录", m_SystemData.ExportFolderPath);
            if (GUILayout.Button("浏览", GUILayout.Width(50)))
            {
                string path = EditorUtility.OpenFolderPanel("选择导出目录", m_SystemData.ExportFolderPath, "");
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                {
                    m_SystemData.ExportFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
                    OnSystemDataChanged?.Invoke();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        public void ExportData(GridSaveData saveData)
        {
            string initialPath = !string.IsNullOrEmpty(m_SystemData.LastExportPath)
                ? m_SystemData.LastExportPath
                : m_SystemData.ExportFolderPath;
            string path = EditorUtility.SaveFilePanel("导出 JSON", initialPath, m_ExportFileName, "json");
            if (!string.IsNullOrEmpty(path))
            {
                string directory = Path.GetDirectoryName(path);
                m_SystemData.LastExportPath = directory;
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(path, JsonUtility.ToJson(saveData, true));
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("成功", "导出成功", "确定");
                OnSystemDataChanged?.Invoke();
            }
        }

        public GridSaveData ImportData()
        {
            string initialPath = !string.IsNullOrEmpty(m_SystemData.LastImportPath)
                ? m_SystemData.LastImportPath
                : m_SystemData.ExportFolderPath;
            string path = EditorUtility.OpenFilePanel("加载", initialPath, "json");
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                m_SystemData.LastImportPath = Path.GetDirectoryName(path);
                OnSystemDataChanged?.Invoke();
                return JsonUtility.FromJson<GridSaveData>(File.ReadAllText(path));
            }
            return null;
        }
    }
}

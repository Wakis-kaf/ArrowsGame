using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.UI;
using Framework.Runtime.UI.Editor;
using Framework.Runtime.UI.Editor.PrefabBinderHelp;
using Framework.Runtime.UI.PrefabBinderHelp;
using Framework.Utils;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Windows;
using Object = UnityEngine.Object;

namespace Framework.Editor.Modules.UI.PrefabBind
{
    [CustomEditor(typeof(PrefabBinder), true)]
    public class PrefabBinderInspector : UnityEditor.Editor
    {
        
        private bool m_IsListExpanded = true;
        private bool m_IsSmartTypeHelpExpanded = false; // 默认折叠
        private Vector2 m_SmartTypeScroll;
        private string m_SmartTypeSearch = "";
        private PrefabBinder m_TargetPrefabBinder;
        private SerializedProperty m_BindAssetsProp;
        private ReorderableList m_ReorderableList;
        private Rect m_PathRect;
        private Object[] m_SelectedObjs;

        private void OnEnable()
        {
            m_TargetPrefabBinder = target as PrefabBinder;
            m_BindAssetsProp = serializedObject.FindProperty("m_BindAssets");
            m_ReorderableList = new ReorderableList(serializedObject, m_BindAssetsProp, true, true, false, false);
            DrawItemListArea();
            if (Application.isPlaying) return;
            AutoRemove();
            AutoGetAndAdd();
        }
        private GUIStyle _foldoutStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _dragLabelStyle;

        private void EnsureStyles()
        {
            if (_foldoutStyle == null)
            {
                _foldoutStyle = new GUIStyle(EditorStyles.foldout)
                {
                    fontStyle = FontStyle.Bold,
                    fontSize = 12
                };
            }
            if (_titleStyle == null)
            {
                _titleStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleLeft
                };
            }
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.toolbar)
                {
                    alignment = TextAnchor.MiddleLeft
                };
            }
            if (_dragLabelStyle == null)
            {
                _dragLabelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(0.15f, 0.3f, 0.6f) }
                };
            }
        }

        private void DrawItemListArea()
        {
            // 每项自适应高度（完整显示）
            m_ReorderableList.elementHeightCallback = (index) =>
            {
                var element = m_BindAssetsProp.GetArrayElementAtIndex(index);
                float height = EditorGUI.GetPropertyHeight(element, true);
                return Mathf.Max(20f, height + 6f);
            };

            //绘制单个元素
            m_ReorderableList.drawElementCallback =
                (rect, index, isActive, isFocused) =>
                {
                    var element = m_BindAssetsProp.GetArrayElementAtIndex(index);
                    float ph = EditorGUI.GetPropertyHeight(element, true);
                    var r = new Rect(rect.x + 2, rect.y + 2, rect.width - 4, ph);
                    EditorGUI.PropertyField(r, element, GUIContent.none, true);
                };

            //背景色（斑马纹）
            m_ReorderableList.drawElementBackgroundCallback = (rect, index, isActive, isFocused) =>
            {
                var bg = (index % 2 == 0) ? new Color(0, 0, 0, 0.03f) : new Color(0, 0, 0, 0.06f);
                EditorGUI.DrawRect(rect, bg);
            };

            //头部
            m_ReorderableList.drawHeaderCallback = (rect) =>
            {
                if (m_IsListExpanded)
                {
                    string title = string.Format("{0}  (共 {1} 项)", m_BindAssetsProp.displayName, m_BindAssetsProp.arraySize);
                    GUI.Label(rect, title, _titleStyle);
                }
            };
        }
        private void AutoRemove()
        {
            var list = m_TargetPrefabBinder.NameList;
            for (int i = list.Count-1; i >=0; i--)
            {
                var name = list[i];
                var item = m_TargetPrefabBinder.GetObj(name);
                if (item == null)
                {
                    m_TargetPrefabBinder.RemoveBind(name);
                }
            }
        }
        private void AutoGetAndAdd()
        {
            var childs = GameObjectUtil.GetAllChildObjects(m_TargetPrefabBinder.transform);
            for (int i = 0; i < childs.Count; i++)
            {
                var child = childs[i];
                if (CanSmartGet(child.gameObject))
                {
                    AddObject(child.gameObject);
                }
            }
            // 批量添加后再次标记脏
            EditorUtility.SetDirty(m_TargetPrefabBinder);
            serializedObject.Update();
        }

        private void DrawFunArea()
        {
            EnsureStyles();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("操作", _titleStyle);
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_AutoGetInChildPrefabBinder","获取子PrefabBinder下物体");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_AutoGetChildPrefabBinderSelf", "获取子PrefabBinder本身");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("自动获取", GUILayout.Height(22)))
            {
                AutoGetAndAdd();
            }
            if (GUILayout.Button("清空全部(不包含手动添加)", GUILayout.Height(22)))
            {
                m_TargetPrefabBinder.ClearAllBind();
            }
            if (GUILayout.Button("清空全部", GUILayout.Height(22)))
            {
                m_TargetPrefabBinder.ClearAllBind();
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawDragListenArea()
        {
            EnsureStyles();
            //获得一个长500的框
            m_PathRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(48));
            var areaRect = new Rect(m_PathRect.x, m_PathRect.y, m_PathRect.width, m_PathRect.height);
            EditorGUI.DrawRect(areaRect, new Color(0.92f, 0.95f, 1f, 0.6f));
            var border = new Rect(areaRect.x + 1, areaRect.y + 1, areaRect.width - 2, areaRect.height - 2);
            EditorGUI.DrawRect(new Rect(border.x, border.y, border.width, 1), new Color(0.6f,0.75f,1f,0.9f));
            EditorGUI.DrawRect(new Rect(border.x, border.yMax-1, border.width, 1), new Color(0.6f,0.75f,1f,0.9f));
            EditorGUI.DrawRect(new Rect(border.x, border.y, 1, border.height), new Color(0.6f,0.75f,1f,0.9f));
            EditorGUI.DrawRect(new Rect(border.xMax-1, border.y, 1, border.height), new Color(0.6f,0.75f,1f,0.9f));

            var icon = EditorGUIUtility.IconContent("Prefab Icon");
            var content = new GUIContent("  拖拽 GameObject 或组件到此区域自动绑定（支持批量）", icon.image);
            GUI.Label(areaRect, content, _dragLabelStyle);
            //如果鼠标正在拖拽中或拖拽结束时，并且鼠标所在位在文本输入框内
            if (m_PathRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    //改变鼠标的外表
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                    m_SelectedObjs = DragAndDrop.objectReferences;
                }

                if (Event.current.type == EventType.DragExited && m_SelectedObjs != null)
                {
                    OnObjsDragEnd();
                }
            }
        }

        private void OnObjsDragEnd()
        {
            for (int i = 0; i < m_SelectedObjs.Length; i++)
            {
                var obj = m_SelectedObjs[i];
                AddObject(obj,true);
            }
        }

        private bool CanSmartGet(Object obj)
        {
            bool canGet = obj is GameObject ;
            if (!canGet) return false;
            GameObject objGo = obj as GameObject;
            if (!m_TargetPrefabBinder.AutoGetInChildPrefabBinder )
            {
                var parentPrefabBinbder = objGo.transform.GetComponentInParent<PrefabBinder>();
                bool isSelfCmp = parentPrefabBinbder!=null&&parentPrefabBinbder.gameObject == objGo;
                if (parentPrefabBinbder!= m_TargetPrefabBinder &&(!isSelfCmp||!m_TargetPrefabBinder.AutoGetChildPrefabBinderSelf))
                {
                    return false;
                }
            }
            foreach (var kvp in PrefabBinder.SmartTypePrefix)
            {
                if (obj.name.StartsWith(kvp.Key))
                {
                    if(!SmartAdd(obj as GameObject, out Object addObj, out string type))
                    {
                        return false;
                    }
                    if (m_TargetPrefabBinder.HasAsset(addObj))
                    {
                        return false;
                    }
                    Debug.Log("SmartAdd" + obj.name);
                    return true;
                }
            }

            return false;
        }

        private bool SmartAdd(GameObject go, out Object obj, out string type)
        {
            string name = go.name;
            obj = go;
            type = "";
            foreach (var kvp in PrefabBinder.SmartTypePrefix)
            {
                if (name.StartsWith(kvp.Key))
                {
                    if (kvp.Value == typeof(GameObject))
                    {
                        obj = go;
                    }
                    else
                    {
                        obj = go.GetComponent(kvp.Value);
                    }
                    if (obj == null)
                    {
                        Debug.LogError($"命名不规范 名称为{name}的对象 不符合组件或者物体{kvp.Value}");
                        return false;
                    }
                    break;
                }
            }
         
            try
            {
                type = obj.GetType().FullName;
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message + "  ");
                return false;
            }
        }

        private void AddObject(Object obj,bool isCustomAdd = false)
        {
            if (obj == null) return;
            string objName = "";
            string type = "";
            bool isGO = false;
            Object needAddObj = obj;
            if (obj is GameObject go)
            {
                objName = go.name;
                isGO = true;
                // 智能识别类型
                SmartAdd(go, out Object newObj, out string objType);
                type = objType;
                needAddObj = newObj;
            }

            if (obj is Component cmp)
            {
                objName = cmp.gameObject.name;
                type = cmp.GetType().FullName;
            }
            else if (!isGO)
            {
                objName = obj.name;
                type = obj.GetType().FullName;
            }
            objName = char.ToLower(objName[0]) + objName.Substring(1);

            objName = RenameUtil.GetNoRepeatName(m_TargetPrefabBinder.HasAsset, objName);
            m_TargetPrefabBinder.AddBind(objName, needAddObj, type, isCustomAdd);

            EditorUtility.SetDirty(m_TargetPrefabBinder);
            serializedObject.Update();
        }

        public override void OnInspectorGUI()
        {
            //绘制功能区域 自动获取按钮、清空按钮
            // 绘制已有节点
            serializedObject.Update();
            DrawFunArea();
            EnsureStyles();
            DrawSmartTypeHelp();
            m_IsListExpanded = EditorGUILayout.Foldout(m_IsListExpanded, "绑定列表", _foldoutStyle);
            if (m_IsListExpanded)
            {
                m_ReorderableList.DoLayoutList();
            }
            // 绘制拖拽区域
            DrawDragListenArea();
            serializedObject.ApplyModifiedProperties();
        }
        private void DrawSmartTypeHelp()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            m_IsSmartTypeHelpExpanded = EditorGUILayout.Foldout(m_IsSmartTypeHelpExpanded, "命名规则 (SmartTypePrefix)", true, _foldoutStyle);

            if (m_IsSmartTypeHelpExpanded)
            {
                GUILayout.Space(2);

                using (new EditorGUILayout.HorizontalScope())
                {
                    m_SmartTypeSearch = EditorGUILayout.TextField(m_SmartTypeSearch, EditorStyles.toolbarSearchField);
                    if (GUILayout.Button("", GUI.skin.FindStyle("SearchCancelButton")))
                    {
                        m_SmartTypeSearch = "";
                        GUI.FocusControl(null);
                    }
                }

                GUILayout.Space(2);

                m_SmartTypeScroll = EditorGUILayout.BeginScrollView(m_SmartTypeScroll, GUILayout.Height(150));

                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    EditorGUILayout.LabelField("前缀", GUILayout.Width(80));
                    EditorGUILayout.LabelField("组件类型");
                }

                foreach (var kvp in PrefabBinder.SmartTypePrefix)
                {
                    if (!string.IsNullOrEmpty(m_SmartTypeSearch))
                    {
                        bool matchKey = kvp.Key.IndexOf(m_SmartTypeSearch, StringComparison.OrdinalIgnoreCase) >= 0;
                        bool matchType = kvp.Value.Name.IndexOf(m_SmartTypeSearch, StringComparison.OrdinalIgnoreCase) >= 0;
                        if (!matchKey && !matchType) continue;
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(kvp.Key, EditorStyles.miniLabel, GUILayout.Width(80));
                        EditorGUILayout.LabelField(kvp.Value.Name, EditorStyles.miniLabel);
                    }
                }

                EditorGUILayout.EndScrollView();
                GUILayout.Space(2);
            }
            EditorGUILayout.EndVertical();
        }

    }
}
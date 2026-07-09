using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.Runtime.UI.Editor
{
    public class ComponentSelector : ScriptableWizard
    {
        public delegate void OnSelectionCallback(Object obj, bool dirty);

        private Type mType;
        private string mTitle;
        private OnSelectionCallback mCallback;
        private Object[] mObjects;
        private Object[] mFilteredObjects;
        private bool mSearched = false;
        private Vector2 mScroll = Vector2.zero;
        private string[] mExtensions = null;
        private string mSearchFilter = "";

        // 样式常量
        private const float LABEL_WIDTH = 120f;
        private const float BUTTON_WIDTH = 80f;
        private const float SMALL_BUTTON_WIDTH = 60f;
        private const float ROW_HEIGHT = 22f;
        private const float SPACING = 8f;

        private static string GetName(Type t)
        {
            string s = t.ToString();
            s = s.Replace("UnityEngine.", "");
            if (s.StartsWith("UI")) s = s.Substring(2);
            return s;
        }

        /// <summary>
        /// Draw a button + object selection combo filtering specified types.
        /// </summary>
        static public void Draw<T>(string buttonName, T obj, OnSelectionCallback cb, bool editButton,
            params GUILayoutOption[] options) where T : Object
        {
            T currentObj = obj;
            bool showButton = false;

            GUILayout.BeginHorizontal();
            {
                showButton = GUILayout.Button(buttonName, EditorStyles.miniButton, GUILayout.Width(LABEL_WIDTH));
                currentObj = EditorGUILayout.ObjectField(currentObj, typeof(T), false, options) as T;

                if (editButton && currentObj != null && currentObj is MonoBehaviour)
                {
                    Component mb = currentObj as Component;
                    if (Selection.activeObject != mb.gameObject && GUILayout.Button("Edit", EditorStyles.miniButton, GUILayout.Width(50f)))
                        Selection.activeObject = mb.gameObject;
                }
                else if (currentObj != null && GUILayout.Button("×", EditorStyles.miniButton, GUILayout.Width(25f)))
                {
                    currentObj = null;
                }
            }
            GUILayout.EndHorizontal();

            if (showButton)
                Show<T>(cb);
            else
                cb(currentObj, true);
        }

        /// <summary>
        /// Draw a button + object selection combo filtering specified types.
        /// </summary>
        static public void Draw<T>(T obj, OnSelectionCallback cb, bool editButton, params GUILayoutOption[] options)
            where T : Object
        {
            Draw<T>(GetTypeName<T>(), obj, cb, editButton, options);
        }

        static public string GetTypeName<T>()
        {
            string s = typeof(T).ToString();
            if (s.StartsWith("UI")) s = s.Substring(2);
            else if (s.StartsWith("UnityEngine.")) s = s.Substring(12);
            return s;
        }

        /// <summary>
        /// Show the selection wizard.
        /// </summary>
        static public void Show<T>(OnSelectionCallback cb) where T : Object
        {
            Show<T>(cb, new string[] { ".prefab" });
        }

        /// <summary>
        /// Show the selection wizard.
        /// </summary>
        static public void Show<T>(OnSelectionCallback cb, string[] extensions) where T : Object
        {
            Type type = typeof(T);
            string title = "选择 " + GetName(type);
            ComponentSelector comp = DisplayWizard<ComponentSelector>(title);
            comp.mTitle = title;
            comp.mType = type;
            comp.mCallback = cb;
            comp.mExtensions = extensions;
            comp.mObjects = Resources.FindObjectsOfTypeAll(typeof(T));
            comp.mFilteredObjects = comp.mObjects;

            if (comp.mObjects == null || comp.mObjects.Length == 0)
            {
                comp.Search();
            }
            else
            {
                // Remove invalid fonts (Lucida Grande etc)
                if (typeof(T) == typeof(Font))
                {
                    for (int i = 0; i < comp.mObjects.Length; ++i)
                    {
                        Object obj = comp.mObjects[i];
                        if (obj.name == "Arial") continue;
                        string path = AssetDatabase.GetAssetPath(obj);
                        if (string.IsNullOrEmpty(path)) comp.mObjects[i] = null;
                    }
                }

                Array.Sort(comp.mObjects,
                    delegate (Object a, Object b)
                    {
                        if (a == null) return b == null ? 0 : 1;
                        if (b == null) return -1;
                        return a.name.CompareTo(b.name);
                    });

                comp.mFilteredObjects = comp.mObjects;
            }
        }

        /// <summary>
        /// Search the entire project for required assets.
        /// </summary>
        private void Search()
        {
            mSearched = true;

            if (mExtensions != null)
            {
                string[] paths = AssetDatabase.GetAllAssetPaths();
                bool isComponent = mType.IsSubclassOf(typeof(Component));
                List<Object> list = new List<Object>();

                for (int i = 0; i < mObjects.Length; ++i)
                    if (mObjects[i] != null)
                        list.Add(mObjects[i]);

                for (int i = 0; i < paths.Length; ++i)
                {
                    string path = paths[i];

                    bool valid = false;

                    for (int b = 0; b < mExtensions.Length; ++b)
                    {
                        if (path.EndsWith(mExtensions[b], StringComparison.OrdinalIgnoreCase))
                        {
                            valid = true;
                            break;
                        }
                    }

                    if (!valid) continue;

                    EditorUtility.DisplayProgressBar("Loading", "Searching assets, please wait...",
                        (float)i / paths.Length);
                    Object obj = AssetDatabase.LoadMainAssetAtPath(path);
                    if (obj == null || list.Contains(obj)) continue;

                    if (!isComponent)
                    {
                        Type t = obj.GetType();
                        if (t == mType || t.IsSubclassOf(mType) && !list.Contains(obj))
                            list.Add(obj);
                    }
                    else if (PrefabUtility.GetPrefabType(obj) == PrefabType.Prefab)
                    {
                        Object t = (obj as GameObject).GetComponent(mType);
                        if (t != null && !list.Contains(t)) list.Add(t);
                    }
                }

                list.Sort(delegate (Object a, Object b) { return a.name.CompareTo(b.name); });
                mObjects = list.ToArray();
                mFilteredObjects = mObjects;
            }

            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// Filter objects based on search text
        /// </summary>
        private void FilterObjects()
        {
            if (string.IsNullOrEmpty(mSearchFilter))
            {
                mFilteredObjects = mObjects;
                return;
            }

            List<Object> filtered = new List<Object>();
            string searchLower = mSearchFilter.ToLower();

            foreach (Object obj in mObjects)
            {
                if (obj == null) continue;

                if (obj.name.ToLower().Contains(searchLower))
                {
                    filtered.Add(obj);
                }
                else
                {
                    // Also check path
                    string path = AssetDatabase.GetAssetPath(obj);
                    if (!string.IsNullOrEmpty(path) && path.ToLower().Contains(searchLower))
                    {
                        filtered.Add(obj);
                    }
                }
            }

            mFilteredObjects = filtered.ToArray();
        }

        /// <summary>
        /// Draw the custom wizard.
        /// </summary>
        private void OnGUI()
        {
            // 设置统一的标签宽度
            EditorGUIUtility.labelWidth = LABEL_WIDTH;

            // 标题区域
            GUILayout.Space(SPACING);
            EditorGUILayout.LabelField(mTitle, EditorStyles.boldLabel);
            GUILayout.Space(SPACING * 0.5f);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(SPACING);

            // 搜索框
            DrawSearchBox();

            if (mFilteredObjects == null || mFilteredObjects.Length == 0)
            {
                DrawEmptyState();
            }
            else
            {
                DrawObjectList();
            }

            DrawSearchButton();
        }

        /// <summary>
        /// Draw search box
        /// </summary>
        private void DrawSearchBox()
        {
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Search:", GUILayout.Width(50f));
                string newSearchFilter = EditorGUILayout.TextField(mSearchFilter);

                if (newSearchFilter != mSearchFilter)
                {
                    mSearchFilter = newSearchFilter;
                    FilterObjects();
                }

                if (GUILayout.Button("Clear", GUILayout.Width(50f)) && !string.IsNullOrEmpty(mSearchFilter))
                {
                    mSearchFilter = "";
                    FilterObjects();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(SPACING);
        }

        /// <summary>
        /// Draw empty state message
        /// </summary>
        private void DrawEmptyState()
        {
            if (string.IsNullOrEmpty(mSearchFilter))
            {
                EditorGUILayout.HelpBox($"No {GetName(mType)} components found.\nTry creating a new one.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"No {GetName(mType)} components found matching '{mSearchFilter}'.\nTry a different search term.", MessageType.Info);
            }

            GUILayout.Space(SPACING);
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Close", GUILayout.Width(BUTTON_WIDTH)))
                {
                    Close();
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw the list of available objects
        /// </summary>
        private void DrawObjectList()
        {
            Object sel = null;

            // 显示结果计数
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label($"Found {mFilteredObjects.Length} items", EditorStyles.miniLabel);
                if (!string.IsNullOrEmpty(mSearchFilter))
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"Filtered by: '{mSearchFilter}'", EditorStyles.miniLabel);
                }
            }
            GUILayout.EndHorizontal();

            // 列表标题
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Name", EditorStyles.boldLabel, GUILayout.Width(LABEL_WIDTH));
                GUILayout.Label("Path", EditorStyles.boldLabel);
                GUILayout.Label("Action", EditorStyles.boldLabel, GUILayout.Width(SMALL_BUTTON_WIDTH));
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4f);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(4f);

            // 对象列表
            mScroll = GUILayout.BeginScrollView(mScroll);
            {
                foreach (Object o in mFilteredObjects)
                {
                    if (DrawObject(o))
                        sel = o;
                }
            }
            GUILayout.EndScrollView();

            if (sel != null)
            {
                mCallback(sel, false);
                Close();
            }
        }

        /// <summary>
        /// Draw search button
        /// </summary>
        private void DrawSearchButton()
        {
            if (!mSearched)
            {
                GUILayout.Space(SPACING);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                GUILayout.Space(SPACING * 0.5f);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Show All Assets", GUILayout.Width(BUTTON_WIDTH + 40f)))
                    {
                        Search();
                    }
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Draw details about the specified object in column format.
        /// </summary>
        private bool DrawObject(Object obj)
        {
            if (obj == null) return false;

            bool retVal = false;
            Component comp = obj as Component;

            GUILayout.BeginHorizontal();
            {
                string path = AssetDatabase.GetAssetPath(obj);
                Color originalColor = GUI.color;

                // 设置颜色提示
                if (string.IsNullOrEmpty(path))
                {
                    path = "[Embedded]";
                    GUI.contentColor = new Color(0.6f, 0.6f, 0.6f);
                }
                else if (comp != null && EditorUtility.IsPersistent(comp.gameObject))
                {
                    GUI.contentColor = new Color(0.4f, 0.6f, 1f);
                }

                // 对象名称
                GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
                labelStyle.alignment = TextAnchor.MiddleLeft;

                retVal |= GUILayout.Button(obj.name, labelStyle, GUILayout.Width(LABEL_WIDTH), GUILayout.Height(ROW_HEIGHT));

                // 路径信息
                GUIStyle pathStyle = new GUIStyle(EditorStyles.label);
                pathStyle.alignment = TextAnchor.MiddleLeft;
                pathStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);

                string displayPath = string.IsNullOrEmpty(path) ? "[Embedded]" : path.Replace("Assets/", "");
                retVal |= GUILayout.Button(displayPath, pathStyle, GUILayout.Height(ROW_HEIGHT));

                // 选择按钮
                GUI.contentColor = originalColor;
                retVal |= GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(SMALL_BUTTON_WIDTH), GUILayout.Height(ROW_HEIGHT - 4f));

                GUI.contentColor = Color.white;
            }
            GUILayout.EndHorizontal();

            // 添加分隔线
            GUILayout.Space(2f);
            return retVal;
        }
    }
}
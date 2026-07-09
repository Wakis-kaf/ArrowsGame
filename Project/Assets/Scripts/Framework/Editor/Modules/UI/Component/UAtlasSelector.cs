using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.Runtime.UI.Editor
{
    public class UAtlasSelector : ScriptableWizard
    {
        public delegate void OnSelectionCallback(Object obj, bool dirty);

        private Type mType;
        private string mTitle;
        private OnSelectionCallback mCallback;
        private List<UAtlas> mAtlases = new List<UAtlas>();
        private List<UAtlas> mFilteredAtlases = new List<UAtlas>();
        private bool mSearched = false;
        private Vector2 mScroll = Vector2.zero;
        private string[] mExtensions = null;
        private string mSearchFilter = "";

        private const float MIN_CELL = 60f;
        private const float MAX_CELL = 300f;
        private float mCellSize = 150f;
        private int mColumns = 4;
        private UAtlas mHoverAtlas = null;
        private Vector2 mHoverPos = Vector2.zero;
        private UAtlas mSelectedAtlas = null;
        private bool mShowTooltip = false;
        private Rect mLastHoverRect;

        private Dictionary<UAtlas, Texture2D> mThumbCache = new Dictionary<UAtlas, Texture2D>();
        private Dictionary<UAtlas, bool> mLoading = new Dictionary<UAtlas, bool>();

        private GUIStyle mNameStyle;
        private GUIStyle mSizeStyle;
        private GUIStyle mTooltipStyle;
        private Texture2D mTooltipBg;

        private static string GetName(Type t)
        {
            string s = t.ToString();
            s = s.Replace("UnityEngine.", "");
            if (s.StartsWith("UI")) s = s.Substring(2);
            return s;
        }

        static public void Draw<T>(string buttonName, T obj, OnSelectionCallback cb, bool editButton,
            params GUILayoutOption[] options) where T : Object
        {
            T currentObj = obj;
            bool showButton = false;

            GUILayout.BeginHorizontal();
            {
                showButton = GUILayout.Button(buttonName, EditorStyles.miniButton, GUILayout.Width(120f));
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

        static public void Show<T>(OnSelectionCallback cb) where T : Object
        {
            Show<T>(cb, new string[] { ".prefab" });
        }

        static public void Show<T>(OnSelectionCallback cb, string[] extensions) where T : Object
        {
            Type type = typeof(T);
            string title = "选择 " + GetName(type);
            UAtlasSelector comp = DisplayWizard<UAtlasSelector>(title);
            comp.mTitle = title;
            comp.mType = type;
            comp.mCallback = cb;
            comp.mExtensions = extensions;
            comp.Initialize();
        }

        private void Initialize()
        {
            CollectAllAtlases();
            FilterAtlases();
            PreloadThumbs();
            InitializeStyles();
        }

        private void CollectAllAtlases()
        {
            mAtlases.Clear();

            string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in allPrefabs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    UAtlas atlas = prefab.GetComponent<UAtlas>();
                    if (atlas != null && !mAtlases.Contains(atlas))
                    {
                        mAtlases.Add(atlas);
                    }
                }
            }

            UAtlas[] sceneAtlases = Resources.FindObjectsOfTypeAll<UAtlas>();
            foreach (UAtlas atlas in sceneAtlases)
            {
                if (atlas != null && !mAtlases.Contains(atlas))
                {
                    mAtlases.Add(atlas);
                }
            }

            mAtlases.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
        }

        private void PreloadThumbs()
        {
            foreach (UAtlas atlas in mAtlases)
            {
                if (atlas != null && atlas.mainTexture != null && !mThumbCache.ContainsKey(atlas))
                {
                    mLoading[atlas] = true;
                    EditorApplication.delayCall += () => LoadThumb(atlas);
                }
            }
        }

        private void LoadThumb(UAtlas atlas)
        {
            if (atlas == null || this == null) return;

            try
            {
                if (atlas.mainTexture != null)
                {
                    Texture2D thumb = AssetPreview.GetAssetPreview(atlas.mainTexture);
                    if (thumb == null)
                    {
                        thumb = AssetPreview.GetMiniThumbnail(atlas.mainTexture);
                    }

                    if (thumb != null && !mThumbCache.ContainsKey(atlas))
                    {
                        mThumbCache[atlas] = thumb;
                        Repaint();
                    }
                }
                mLoading[atlas] = false;
            }
            catch
            {
                mLoading[atlas] = false;
            }
        }

        private void FilterAtlases()
        {
            if (string.IsNullOrEmpty(mSearchFilter))
            {
                mFilteredAtlases = new List<UAtlas>(mAtlases);
                return;
            }

            mFilteredAtlases.Clear();
            string searchLower = mSearchFilter.ToLower();

            foreach (UAtlas atlas in mAtlases)
            {
                if (atlas == null) continue;

                if (atlas.name.ToLower().Contains(searchLower))
                {
                    mFilteredAtlases.Add(atlas);
                }
                else if (atlas.mainTexture != null && atlas.mainTexture.name.ToLower().Contains(searchLower))
                {
                    mFilteredAtlases.Add(atlas);
                }
                else
                {
                    string path = AssetDatabase.GetAssetPath(atlas);
                    if (!string.IsNullOrEmpty(path) && path.ToLower().Contains(searchLower))
                    {
                        mFilteredAtlases.Add(atlas);
                    }
                }
            }
        }

        private void InitializeStyles()
        {
            mNameStyle = new GUIStyle(EditorStyles.label);
            mNameStyle.alignment = TextAnchor.MiddleCenter;
            mNameStyle.normal.textColor = new Color(0.1f, 0.1f, 0.1f);

            mSizeStyle = new GUIStyle(EditorStyles.miniLabel);
            mSizeStyle.alignment = TextAnchor.MiddleCenter;
            // 使用深蓝色而不是灰色
            mSizeStyle.normal.textColor = new Color(0.1f, 0.3f, 0.6f);
            mSizeStyle.fontStyle = FontStyle.Bold; // 可以加粗一下

            mTooltipStyle = new GUIStyle(GUI.skin.box);
            mTooltipStyle.normal.textColor = Color.white;
            mTooltipStyle.padding = new RectOffset(6, 6, 6, 6);
            mTooltipStyle.wordWrap = true;

            mTooltipBg = new Texture2D(1, 1);
            mTooltipBg.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f, 0.95f));
            mTooltipBg.Apply();
            mTooltipStyle.normal.background = mTooltipBg;
        }

        private void OnGUI()
        {
            if (mNameStyle == null) InitializeStyles();

            EditorGUIUtility.labelWidth = 120f;

            GUILayout.Space(8);
            EditorGUILayout.LabelField(mTitle, EditorStyles.boldLabel);
            GUILayout.Space(4);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(8);

            DrawToolbar();

            if (mFilteredAtlases.Count == 0)
            {
                DrawEmpty();
            }
            else
            {
                DrawGrid();
            }

            if (Event.current.type == EventType.Repaint)
            {
                mShowTooltip = false;
            }

            if (mShowTooltip && mHoverAtlas != null)
            {
                DrawTooltip();
            }
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("搜索:", GUILayout.Width(40));
                string newFilter = EditorGUILayout.TextField(mSearchFilter);
                if (newFilter != mSearchFilter)
                {
                    mSearchFilter = newFilter;
                    FilterAtlases();
                }

                if (GUILayout.Button("清空", GUILayout.Width(50)) && !string.IsNullOrEmpty(mSearchFilter))
                {
                    mSearchFilter = "";
                    FilterAtlases();
                }

                GUILayout.FlexibleSpace();

                GUILayout.Label("大小:", GUILayout.Width(40));
                mCellSize = GUILayout.HorizontalSlider(mCellSize, MIN_CELL, MAX_CELL, GUILayout.Width(100));

                if (GUILayout.Button("小", GUILayout.Width(30))) mCellSize = MIN_CELL;
                if (GUILayout.Button("中", GUILayout.Width(30))) mCellSize = (MIN_CELL+MAX_CELL)/2;
                if (GUILayout.Button("大", GUILayout.Width(30))) mCellSize = MAX_CELL;

                GUILayout.Label($"{mCellSize:F0}", GUILayout.Width(40));
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        private void DrawEmpty()
        {
            if (string.IsNullOrEmpty(mSearchFilter))
            {
                EditorGUILayout.HelpBox("未找到UAtlas组件。", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"未找到匹配 '{mSearchFilter}' 的UAtlas。", MessageType.Info);
            }

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("关闭", GUILayout.Width(80)))
                {
                    Close();
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawGrid()
        {
            mColumns = Mathf.Max(1, Mathf.FloorToInt(position.width / (mCellSize + 20)));
            float cellWidth = (position.width - 20 * (mColumns + 1)) / mColumns;

            mScroll = GUILayout.BeginScrollView(mScroll, false, true, GUILayout.ExpandHeight(true));
            {
                float y = 10;
                float x = 10;
                bool needRepaint = false;

                for (int i = 0; i < mFilteredAtlases.Count; i++)
                {
                    if (i % mColumns == 0 && i > 0)
                    {
                        y += mCellSize + 50;
                        x = 10;
                    }

                    UAtlas atlas = mFilteredAtlases[i];
                    if (atlas == null) continue;

                    Rect cellRect = new Rect(x, y, cellWidth, mCellSize + 40);

                    bool isHover = cellRect.Contains(Event.current.mousePosition);
                    if (isHover && Event.current.type == EventType.Repaint)
                    {
                        mHoverAtlas = atlas;
                        mHoverPos = Event.current.mousePosition;
                        mLastHoverRect = cellRect;
                        mShowTooltip = true;
                        needRepaint = true;
                    }

                    bool isSelected = atlas == mSelectedAtlas;
                    DrawCell(cellRect, atlas, isSelected);

                    if (Event.current.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition))
                    {
                        mSelectedAtlas = atlas;
                        if (Event.current.clickCount == 2)
                        {
                            mCallback(atlas, false);
                            Close();
                        }
                        Event.current.Use();
                    }

                    x += cellWidth + 20;
                }

                GUILayout.Space(y + mCellSize + 50);

                if (needRepaint && Event.current.type == EventType.Repaint)
                {
                    Repaint();
                }
            }
            GUILayout.EndScrollView();

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label($"共 {mFilteredAtlases.Count} 项", EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
                if (mSelectedAtlas != null && GUILayout.Button("选择", GUILayout.Width(60)))
                {
                    mCallback(mSelectedAtlas, false);
                    Close();
                }
                if (GUILayout.Button("取消", GUILayout.Width(60)))
                {
                    Close();
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawCell(Rect rect, UAtlas atlas, bool selected)
        {
            GUI.BeginGroup(rect);

            // 使用Unity默认的box样式作为背景
            Rect bgRect = new Rect(0, 0, rect.width, rect.height);
            GUI.Box(bgRect, GUIContent.none);

            if (selected)
            {
                // 选中时的高亮效果
                EditorGUI.DrawRect(bgRect, new Color(0.2f, 0.4f, 0.8f, 0.15f));
                Handles.color = new Color(0.2f, 0.5f, 1f, 0.8f);
                Handles.DrawAAPolyLine(2,
                    new Vector3(0, 0),
                    new Vector3(rect.width, 0),
                    new Vector3(rect.width, rect.height),
                    new Vector3(0, rect.height),
                    new Vector3(0, 0)
                );
            }

            // 使用透明背景
            Rect thumbRect = new Rect(5, 5, rect.width - 10, rect.height - 45);

            // 创建棋盘格透明背景
            DrawCheckerboard(thumbRect);

            Texture2D thumb = null;
            mThumbCache.TryGetValue(atlas, out thumb);

            if (thumb != null)
            {
                float aspect = (float)thumb.width / thumb.height;
                float thumbHeight = thumbRect.height - 10;
                float thumbWidth = thumbHeight * aspect;

                if (thumbWidth > thumbRect.width - 10)
                {
                    thumbWidth = thumbRect.width - 10;
                    thumbHeight = thumbWidth / aspect;
                }

                Rect drawRect = new Rect(
                    thumbRect.x + (thumbRect.width - thumbWidth) * 0.5f,
                    thumbRect.y + (thumbRect.height - thumbHeight) * 0.5f,
                    thumbWidth,
                    thumbHeight
                );

                GUI.DrawTexture(drawRect, thumb, ScaleMode.ScaleToFit);
            }
            else
            {
                GUIStyle style = new GUIStyle(EditorStyles.label);
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = new Color(0.3f, 0.3f, 0.3f);

                bool isLoading = false;
                mLoading.TryGetValue(atlas, out isLoading);

                if (isLoading)
                {
                    GUI.Label(thumbRect, "加载中...", style);
                }
                else if (atlas.mainTexture == null)
                {
                    GUI.Label(thumbRect, "无纹理", style);
                }
            }

            string displayName = atlas.name;
            if (displayName.Length > 20)
            {
                displayName = displayName.Substring(0, 18) + "...";
            }

            Rect nameRect = new Rect(0, rect.height - 35, rect.width, 20);
            mNameStyle.normal.textColor = selected ? new Color(0.1f, 0.3f, 0.8f) : new Color(0.1f, 0.1f, 0.1f);
            GUI.Label(nameRect, displayName, mNameStyle);

            if (atlas.mainTexture != null)
            {
                Rect sizeRect = new Rect(0, rect.height - 20, rect.width, 15);
                mSizeStyle.normal.textColor = selected ? new Color(0.1f, 0.4f, 0.8f) : new Color(0.1f, 0.3f, 0.6f);
                GUI.Label(sizeRect, $"{atlas.mainTexture.width}×{atlas.mainTexture.height}", mSizeStyle);
            }

            GUI.EndGroup();
        }

        // 绘制棋盘格透明背景
        private void DrawCheckerboard(Rect rect)
        {
            int checkerSize = 8;
            for (int x = 0; x < rect.width; x += checkerSize)
            {
                for (int y = 0; y < rect.height; y += checkerSize)
                {
                    bool isDark = ((x / checkerSize) + (y / checkerSize)) % 2 == 1;
                    Rect cellRect = new Rect(rect.x + x, rect.y + y,
                                           Mathf.Min(checkerSize, rect.width - x),
                                           Mathf.Min(checkerSize, rect.height - y));

                    EditorGUI.DrawRect(cellRect, isDark ? new Color(0.8f, 0.8f, 0.8f, 0.5f) : new Color(1f, 1f, 1f, 0.5f));
                }
            }

            // 添加边框
            Handles.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
            Handles.DrawAAPolyLine(1,
                new Vector3(rect.x, rect.y),
                new Vector3(rect.x + rect.width, rect.y),
                new Vector3(rect.x + rect.width, rect.y + rect.height),
                new Vector3(rect.x, rect.y + rect.height),
                new Vector3(rect.x, rect.y)
            );
        }
        private void DrawTooltip()
        {
            if (mHoverAtlas == null) return;

            string info = $"名称: {mHoverAtlas.name}\n";

            if (mHoverAtlas.mainTexture != null)
            {
                info += $"纹理: {mHoverAtlas.mainTexture.name}\n";
                info += $"尺寸: {mHoverAtlas.mainTexture.width}×{mHoverAtlas.mainTexture.height}\n";
            }

            if (mHoverAtlas.spriteList != null)
            {
                info += $"精灵数: {mHoverAtlas.spriteList.Count}";
            }

            GUIContent content = new GUIContent(info);
            Vector2 size = mTooltipStyle.CalcSize(content);
            size.x = Mathf.Min(size.x, 250);
            size.y = mTooltipStyle.CalcHeight(content, size.x);

            float x = Mathf.Min(mHoverPos.x + 15, position.width - size.x - 10);
            float y = Mathf.Min(mHoverPos.y + 15, position.height - size.y - 10);

            Rect rect = new Rect(x, y, size.x, size.y);
            GUI.Box(rect, info, mTooltipStyle);
        }

        private void OnDestroy()
        {
            if (mTooltipBg != null)
            {
                UnityEngine.Object.DestroyImmediate(mTooltipBg);
            }
            mThumbCache.Clear();
            mLoading.Clear();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Framework.Runtime.UI.Editor
{
    public class Sprite2Selector : ScriptableWizard
    {
        public delegate void Callback(string spriteName, Sprite sprite);

        public static Sprite2Selector instance;

        private SpriteAtlas _atlas; //当前选择的图集
        private Texture2D _mainSprite; //当前选择的图集
        private List<Sprite> _sprites; //当前选择的图集
        private Texture2D _mainTex; //当前选择的图集
        private Texture2D[] _texs; //当前选择的图集
        private Dictionary<string, Sprite> m_Name2Sprite;
        private string _selectSprite; //当前选择的图元
        private Callback _callback; //选择回调
        private Vector2 _pos = Vector2.zero;
        private float _clickTime;

        //void OnEnable() { instance = this; }
        //void OnDisable() { instance = null; }

        private static string _searchSprite = ""; //搜索的

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = 80;

            if (_atlas == null)
            {
                GUILayout.Label("No Atlas selected.", "LODLevelNotifyText");
                return;
            }

            GUILayout.Label(_atlas.name + " Sprites", "LODLevelNotifyText");
            UnitUIEditorTool.DrawSeparator();
            GUILayout.BeginHorizontal();
            GUILayout.Space(84f);

            bool close = false;
            string before = _searchSprite;
            string after = EditorGUILayout.TextField("", before, "SearchTextField");
            _searchSprite = after;

            if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18f)))
            {
                _searchSprite = "";
                GUIUtility.keyboardControl = 0;
            }

            GUILayout.Space(84f);
            GUILayout.EndHorizontal();

            if (_mainTex == null)
            {
                GUILayout.Label("该图集无关联贴图");
                return;
            }

            List<string> result = new List<string>();
            result = m_Name2Sprite.Keys.ToList();
            for (int i = result.Count - 1; i >= 0; i--)
            {
                if (!string.IsNullOrEmpty(_searchSprite) &&
                    result[i].IndexOf(_searchSprite, StringComparison.Ordinal) == -1)
                {
                    result.RemoveAt(i);
                }
            }

            float size = 80f;
            float padded = size + 10f;
            int columns = Mathf.FloorToInt(Screen.width / padded);
            if (columns < 1) columns = 1;

            int offset = 0;
            Rect rect = new Rect(10f, 0, size, size); 

            GUILayout.Space(10f);
            _pos = GUILayout.BeginScrollView(_pos);
            int rows = 1;
            while (offset < result.Count)
            {
                GUILayout.BeginHorizontal();
                {
                    int col = 0;
                    rect.x = 10f;
                    for (; offset < result.Count; ++offset)
                    {
                        Sprite sprite = GetSprite(result[offset]);
                        if (sprite == null) continue;
                        if (GUI.Button(rect, ""))
                        {
                            if (Event.current.button == 0) //左键点击
                            {
                                float delta = Time.realtimeSinceStartup - _clickTime;
                                _clickTime = Time.realtimeSinceStartup;
                                if (_selectSprite != sprite.name) //点击了其他的sprite
                                {
                                    _selectSprite = sprite.name;
                                    if (_callback != null) _callback(sprite.name, sprite);
                                }
                                else if (delta < 0.5f) close = true;
                            }
                        }

                        if (Event.current.type == EventType.Repaint)
                        {
                            UnitUIEditorTool.DrawTiledTexture(rect, UnitUIEditorTool.BackdropTexture);
                            Rect uv = sprite.rect;
                            uv = ConvertToTexCoords(uv, sprite.texture.width, sprite.texture.height);

                            float scaleX = rect.width / uv.width;
                            float scaleY = rect.height / uv.height;
                            float aspect = scaleY / scaleX / ((float) sprite.texture.height / sprite.texture.width);
                            Rect clipRect = rect;
                            if (Math.Abs(aspect - 1f) > 0)
                            {
                                if (aspect < 1f)
                                {
                                    float padding = size * (1f - aspect) * 0.5f;
                                    clipRect.xMin += padding;
                                    clipRect.xMax -= padding;
                                }
                                else
                                {
                                    float padding = size * (1f - 1f / aspect) * 0.5f;
                                    clipRect.yMin += padding;
                                    clipRect.yMax -= padding;
                                }
                            }

                            GUI.DrawTextureWithTexCoords(clipRect, sprite.texture, uv);
                            if (_selectSprite == sprite.name)
                            {
                                UnitUIEditorTool.DrawOutline(rect, new Color(0.4f, 1f, 0f, 1f));
                            }
                        }

                        GUI.backgroundColor = new Color(1f, 1f, 1f, 0.5f);
                        GUI.contentColor = new Color(1f, 1f, 1f, 0.7f);
                        GUI.Label(new Rect(rect.x, rect.y + rect.height, rect.width, 32f), sprite.name,
                            "ProgressBarBack");
                        GUI.contentColor = Color.white;
                        GUI.backgroundColor = Color.white;

                        if (++col >= columns)
                        {
                            ++offset;
                            break;
                        }

                        rect.x += padded;
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(padded);
                rect.y += padded + 26;
                ++rows;
            }

            GUILayout.Space(rows * 26);
            GUILayout.EndScrollView();
            if (close) Close();
        }

        private Sprite GetSprite(string name)
        {
            if (m_Name2Sprite == null) return null;
            m_Name2Sprite.TryGetValue(name, out var res);
            return res;
        }

        public static Rect ConvertToTexCoords(Rect rect, int width, int height)
        {
            Rect final = rect;

            if (width > 0f && height > 0f)
            {
                final.xMin = rect.xMin / width;
                final.xMax = rect.xMax / width;
                final.yMin = rect.yMin / height;
                final.yMax = rect.yMax / height;
            }

            return final;
        }

        public static void Show(SpriteAtlas atlas, string spriteName, Callback callback)
        {
            if (instance != null)
            {
                instance.Close();
                instance = null;
            }

            Sprite2Selector comp = DisplayWizard<Sprite2Selector>("Select a Sprite");
            instance = comp;
            var packables = atlas.GetPackables();
            comp._texs = new Texture2D[packables.Length];
            List<Texture2D> texture2Ds = new List<Texture2D>();
            List<Sprite> sprites = new List<Sprite>();
            Dictionary<string, Sprite> m_name2Sprite = new Dictionary<string, Sprite>();
            for (int i = 0; i < packables.Length; i++)
            {
                string name = "";
                Debug.Log(packables[i].GetType());
                if (packables[i] is DefaultAsset defaultAsset)
                {
                    List<string> subFolders = AssetDatabase.GetSubFolders(AssetDatabase.GetAssetPath(defaultAsset)).ToList();  // 获取子文件夹
                    subFolders.Add(AssetDatabase.GetAssetPath(defaultAsset));
                    foreach (string folder in subFolders)
                    {
                        string[] guids = AssetDatabase.FindAssets("t:texture2D", new[] { folder });  // 获取子文件夹下所有 Texture2D 的 GUID
                        foreach (string guid in guids)
                        {
                            string path = AssetDatabase.GUIDToAssetPath(guid);  
                            Sprite tmpSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);  // 尝试加载 Sprite
                            if (tmpSprite != null)
                            {
                                texture2Ds.Add(tmpSprite.texture);
                                sprites.Add(tmpSprite);  // 将 Sprite 添加到列表中
                                m_name2Sprite.Add(tmpSprite.name, tmpSprite);
                            }
                            else
                            {
                                // 将 GUID 转化为文件路径
                                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);  // 加载 Texture2D
                                if (texture != null)
                                {
                                    texture2Ds.Add(texture);  // 将 Texture2D 添加到列表中
                                    var txtSprite = UIUtil.Texture2DToSprite(texture);
                                    sprites.Add(txtSprite);
                                    m_name2Sprite.Add(txtSprite.name, txtSprite);
                                }
                            }
                           
                           
                        }
                    }
                    continue;
                }else if (packables[i] is Sprite sprite)
                {
                    texture2Ds.Add(sprite.texture);
                    sprites.Add(sprite);
                    name = sprite.name;
                }else if (packables[i] is Texture2D texture2D)
                {
                    sprites.Add(UIUtil.Texture2DToSprite(texture2D));
                    texture2Ds.Add(texture2D);
                    name = texture2D.name;
                }
                m_name2Sprite.Add(name, sprites[i]);
            }

            comp._texs = texture2Ds.ToArray();
            if (string.IsNullOrEmpty(spriteName) && comp._texs.Length > 0)
            {
                spriteName = comp._texs[0].name;
            }

            comp._selectSprite = spriteName;
            comp._callback = callback;
            comp._atlas = atlas;
            comp._sprites = sprites;
            comp.m_Name2Sprite = m_name2Sprite;
            comp._mainTex = m_name2Sprite.ContainsKey(spriteName) ? m_name2Sprite[spriteName].texture : comp._texs.Length>0?comp._texs[1]:null;
        }
    }
}
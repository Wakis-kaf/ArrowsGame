using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace Framework.Runtime.UI.Editor
{
    [CustomEditor(typeof(USprite2))]
    public class USprite2Editor : UImageEditor
    {
        private string[] m_SpriteNames;
        private USprite2 _sprite;

        protected override void OnEnable()
        {
            base.OnEnable();
            _sprite = target as USprite2;
        }

        protected override void OnPreInspectorGUI()
        {
            _sprite = target as USprite2;
            if (_sprite == null) return;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Atlas", "DropDown", GUILayout.Width(55f)))
            {
                ComponentSelector.Show<SpriteAtlas>(OnSelectAtlas);
            }

            
            UnitUIEditorTool.DrawProperty("", serializedObject, "m_SpriteAtlas", GUILayout.MinWidth(20f));
            SerializedProperty sp = serializedObject.FindProperty("m_SpriteAtlas");
            SpriteAtlas nowAtlas = sp.objectReferenceValue as SpriteAtlas;
            _sprite.SpriteAtlas = nowAtlas;

            if (GUILayout.Button("Edit", GUILayout.Width(40f)))
            {
                if (nowAtlas != null)
                {
                    //SpriteEditor.Open(nowAtlas.mainTexture);
                }
            }

            GUILayout.EndHorizontal();
            ////////////////////////////////////////////////////////////////////////////////
            if (nowAtlas == null) //无图集
            {
                OnSelectAtlas(null);
                SelectSprite("", null);
            }
            else
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Sprite", "DropDown", GUILayout.Width(55f)))
                {
                    Sprite2Selector.Show(nowAtlas, _sprite.SpriteName, SelectSprite);
                }

                GUILayout.Label(_sprite.SpriteName, "HelpBox", GUILayout.Height(18f), GUILayout.MinWidth(20f));

                //优化后的 Tiled 模式
                if (_sprite.sprite != null)
                {
                    if (GUILayout.Button("OpTiled", GUILayout.Width(60f)))
                    {
                        // var eff = _sprite.gameObject.GetComponent<CTiledEffect>();
                        // if (eff == null)
                        // {
                        //     eff = _sprite.gameObject.AddComponent<CTiledEffect>();
                        // }
                    }

                    //编辑器下没法监听组件移除，所以只能出此下策了
                    if (_sprite.material != null)
                    {
                        // var eff = _sprite.gameObject.GetComponent<CTiledEffect>();
                        // if (eff == null && _sprite.material.shader == Shader.Find("UI/UITiledEffect"))
                        // {
                        //     _sprite.material = null;
                        // }
                    }
                }

                if (GUILayout.Button("Edit", GUILayout.Width(40f)))
                {
                    if (_sprite.sprite != null)
                    {
                        SpriteEmitEditor.Open(_sprite.sprite);
                    }
                }

                GUILayout.EndHorizontal();
            }

            UnitUIEditorTool.DrawProperty("AutoSnap", serializedObject, "autoSnap");
            UnitUIEditorTool.DrawProperty("ShowWhiteSource", serializedObject, "showWhiteSource");
        }

        private void OnSelectAtlas(Object obj,bool dirty = true)
        {
            if (_sprite.SpriteAtlas != obj)
            {
                Undo.RecordObject(_sprite, "On Select Atlas");
                _sprite.SpriteAtlas = obj as SpriteAtlas;
                if(dirty)
                    EditorUtility.SetDirty(_sprite);
            }
        }

        private void SelectSprite(string spriteName, Sprite sprite)
        {
            if (_sprite.SpriteName != spriteName)
            {
                Undo.RecordObject(_sprite, "On Select Sprite");
                _sprite.SpriteName = spriteName;
                _sprite.sprite = sprite;
                EditorUtility.SetDirty(_sprite);
            }
        }

        /*protected override void OnPreInspectorGUI()
        {
            serializedObject.Update();  
            var spriteAtlasSP = UnitUIEditorTool.SerializedProperty(serializedObject, "m_SpriteAtlas");
            var spriteAtlas = spriteAtlasSP.objectReferenceValue as SpriteAtlas;
            if (spriteAtlas != null)
            {
                EditorGUI.BeginChangeCheck();
                UnitUIEditorTool.SerializedProperty(serializedObject, "m_SpriteName");
                if (EditorGUI.EndChangeCheck())
                {
                    Debug.Log(_sprite.SpriteName);
                    _sprite.SpriteName = _sprite.SpriteName;
                } 
            }
        
            base.OnPreInspectorGUI();
            serializedObject.ApplyModifiedProperties();
        }*/
    }
}
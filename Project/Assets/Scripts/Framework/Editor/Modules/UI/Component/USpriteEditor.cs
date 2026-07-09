using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Framework.Runtime.UI.Editor
{
    [CustomEditor(typeof(USprite), true)]
    [CanEditMultipleObjects]
    public class USpriteEditor : ImageEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            OnPreInspectorGUI();
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();

            serializedObject.Update();
            OnAfterInspectorGUI();
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnAfterInspectorGUI()
        {
            UnitUIEditorTool.DrawProperty("RaycastConsiderAlpha", serializedObject, "raycastConsiderAlpha");
            
            SerializedProperty grayProp = serializedObject.FindProperty("m_Gray");
            if (grayProp != null)
            {
                EditorGUILayout.PropertyField(grayProp, new GUIContent("Gray"));
            }
            
            foreach (Object obj in targets)
            {
                USprite sprite = obj as USprite;
                if (sprite != null)
                {
                    sprite.CheckAtlasSprite();
                }
            }
        }

        protected virtual void OnPreInspectorGUI()
        {
            _sprite = target as USprite;
            SerializedProperty atlasProp = serializedObject.FindProperty("m_Atlas");
            
            if (GUILayout.Button("清空图集"))
            {
                foreach (Object obj in targets)
                {
                    USprite sprite = obj as USprite;
                    if (sprite != null)
                    {
                        sprite.ClearSprite();
                        EditorUtility.SetDirty(sprite);
                    }
                }
                atlasProp.objectReferenceValue = null;
                serializedObject.ApplyModifiedProperties();
            }
            
            GUILayout.Space(15);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Atlas", "DropDown", GUILayout.Width(55f)))
            {
                UAtlasSelector.Show<UAtlas>(OnSelectAtlas);
            }

            UnitUIEditorTool.DrawProperty("", serializedObject, "m_Atlas", GUILayout.MinWidth(20f));
            
            UAtlas nowAtlas = atlasProp.objectReferenceValue as UAtlas;
            
            foreach (Object obj in targets)
            {
                USprite sprite = obj as USprite;
                if (sprite != null)
                {
                    sprite.Atlas = nowAtlas;
                }
            }

            if (GUILayout.Button("Edit", GUILayout.Width(40f)))
            {
                if (nowAtlas != null)
                {
                    SpriteEditor.Open(nowAtlas.mainTexture);
                }
            }
            GUILayout.EndHorizontal();
            
            if (nowAtlas == null)
            {
                OnSelectAtlas(null, false);
                SelectSprite(null, false);
            }
            else
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Sprite", "DropDown", GUILayout.Width(55f)))
                {
                    string spriteName = _sprite != null ? _sprite.SpriteName : "";
                    SpritesSelector.Show(nowAtlas, spriteName, SelectSprite);
                }

                string displaySpriteName = GetSpriteNameForDisplay();
                GUILayout.Label(displaySpriteName, "HelpBox", GUILayout.Height(18f), GUILayout.MinWidth(20f));

                if (GUILayout.Button("Edit", GUILayout.Width(40f)))
                {
                    if (_sprite != null && _sprite.sprite != null)
                    {
                        SpriteEditor.Open(_sprite.sprite);
                    }
                }
                GUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledScope(false))
                {
                    Sprite displaySprite = _sprite != null ? _sprite.sprite : null;
                    EditorGUILayout.ObjectField("雪碧图", displaySprite, typeof(Sprite), false);
                }
                EditorGUILayout.EndHorizontal();
            }

            if (UnitUIEditorTool.DrawProperty("AutoSnap", serializedObject, "m_IsAutoSnap"))
            {
                var snapSp = serializedObject.FindProperty("m_IsAutoSnap");
                if (snapSp.boolValue == true)
                {
                    foreach (Object obj in targets)
                    {
                        USprite sprite = obj as USprite;
                        if (sprite != null)
                        {
                            sprite.SetNativeSize();
                        }
                    }
                }
            }
            UnitUIEditorTool.DrawProperty("ShowWhiteSource", serializedObject, "isShowWhiteSource");
        }
        
        private string GetSpriteNameForDisplay()
        {
            if (targets.Length == 1)
            {
                return _sprite != null ? _sprite.SpriteName : "";
            }
            
            string firstSpriteName = "";
            bool allSame = true;
            foreach (Object obj in targets)
            {
                USprite sprite = obj as USprite;
                if (sprite != null)
                {
                    if (string.IsNullOrEmpty(firstSpriteName))
                    {
                        firstSpriteName = sprite.SpriteName;
                    }
                    else if (sprite.SpriteName != firstSpriteName)
                    {
                        allSame = false;
                        break;
                    }
                }
            }
            return allSame ? firstSpriteName : "Multiple values";
        }

        private USprite _sprite;

        private void OnSelectAtlas(Object obj, bool dirty = true)
        {
            UAtlas atlas = obj as UAtlas;
            foreach (Object targetObj in targets)
            {
                USprite sprite = targetObj as USprite;
                if (sprite != null)
                {
                    sprite.Atlas = atlas;
                    if (dirty)
                        EditorUtility.SetDirty(sprite);
                }
            }
        }

        private void SelectSprite(string spriteName, bool dirty = true)
        {
            foreach (Object targetObj in targets)
            {
                USprite sprite = targetObj as USprite;
                if (sprite != null)
                {
                    sprite.SpriteName = spriteName;
                    if (dirty)
                        EditorUtility.SetDirty(sprite);
                }
            }
        }
    }
}
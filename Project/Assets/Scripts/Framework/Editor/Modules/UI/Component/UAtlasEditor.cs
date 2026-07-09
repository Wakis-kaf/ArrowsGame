using UnityEditor;
using UnityEngine;

namespace Framework.Runtime.UI.Editor
{
    [CustomEditor(typeof (UAtlas))]
    public class UAtlasEditor: UnityEditor.Editor
    {
        private UAtlas _atlas;
        private Sprite _sprite;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _atlas = target as UAtlas;
            UnitUIEditorTool.DrawProperty("MainTexture", serializedObject, "mainTexture");
            //UnitUIEditorTool.DrawProperty("_spriteList", serializedObject, "_spriteList");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Sprite", "DropDown"))
            {
                SpritesSelector.Show(_atlas, _sprite != null ? _sprite.name : "", OnSelect);
            }
            if (GUILayout.Button("Edit", GUILayout.Width(40f)))
            {
                if (_atlas != null && _atlas.mainTexture != null)
                {
                    SpriteEditor.Open(_atlas.mainTexture);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6f);
            if (!UnitUIEditorTool.DrawHeader("Sprite Details")) return;
            UnitUIEditorTool.BeginContents();

            EditorGUILayout.TextField("Name", _sprite != null ? _sprite.name : "");
            EditorGUILayout.TextField("Width", _sprite != null ? "" + _sprite.rect.width : "");
            EditorGUILayout.TextField("Height", _sprite != null ? "" + _sprite.rect.height : "");

            UnitUIEditorTool.EndContents();

            serializedObject.ApplyModifiedProperties();
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            if (_sprite == null) return;
            UnitUIEditorTool.DrawSprite(_sprite, rect, Color.white);
        }

        private void OnSelect(string spriteName,bool dirty)
        {
            _sprite = _atlas.GetSprite(spriteName);
            Repaint();
        }
    }
}
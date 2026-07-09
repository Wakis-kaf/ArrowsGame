using Framework.Runtime.UI.UIAnimae.Tweeners;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;


namespace Framework.Runtime.UI.Editor
{
    public class AtlasSpriteChangeTweenerEditor : OdinValueDrawer<AtlasSpriteChangeTweener>
    {
        private AtlasSpriteChangeTweener _tweener;
        protected override void DrawPropertyLayout(GUIContent label)
        {
            
            _tweener = this.ValueEntry.SmartValue;
            Rect rect = EditorGUILayout.GetControlRect();
            GUILayout.BeginHorizontal();
            SerializedObject serializedObject = this.ValueEntry.Property.Tree.UnitySerializedObject;
            if (GUILayout.Button("Atlas", "DropDown", GUILayout.Width(55f)))
            {
                UAtlasSelector.Show<UAtlas>(OnSelectAtlas);
            }
            EditorGUI.BeginChangeCheck();
            UAtlas currentAtlas = (UAtlas)EditorGUILayout.ObjectField(_tweener.Atlas, typeof(UAtlas), false);
            if (EditorGUI.EndChangeCheck())
            {
                _tweener.Atlas = currentAtlas;     
                serializedObject.ApplyModifiedProperties();
            }
            UAtlas nowAtlas = _tweener.Atlas;
            if (GUILayout.Button("Edit", GUILayout.Width(40f)) && _tweener.Atlas != null)
            {
                // SpriteEditor.Open(_tweener.Atlas.mainTexture);
            }
            GUILayout.EndHorizontal();


            if (nowAtlas == null) //无图集
            {
                OnSelectAtlas(null, false);
                SelectSprite(null, false);
            }
            else
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Sprite", "DropDown", GUILayout.Width(55f)))
                {
                    SpritesSelector.Show(nowAtlas, _tweener.SpriteName, SelectSprite);
                }

                GUILayout.Label(_tweener.SpriteName, "HelpBox", GUILayout.Height(18f), GUILayout.MinWidth(20f));
                GUILayout.EndHorizontal();
                
            }
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("雪碧图", _tweener.Sprite, typeof(Sprite), false);
            }
            
            _tweener.targetSprite = (USprite)EditorGUILayout.ObjectField("目标Sprite",_tweener.targetSprite, typeof(USprite), true);
        }
        private void SelectSprite(string spriteName, bool dirty = true)
        {
            _tweener.SpriteName = spriteName;
            //if (dirty)
                //EditorUtility.SetDirty(_tweener);
        }


        //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        //{
        //    GUILayout.BeginHorizontal();
        //    if (GUILayout.Button("Atlas", "DropDown", GUILayout.Width(55f)))
        //    {
        //        ComponentSelector.Show<UAtlas>(OnSelectAtlas);
        //    }

        //    //SerializedProperty sp = UnitUIEditorTool.DrawProperty("", serializedObject, "m_Atlas", GUILayout.MinWidth(20f));
        //    //UAtlas nowAtlas = sp.objectReferenceValue as UAtlas;
        //    //_tweener.atlas = nowAtlas;

        //    //if (GUILayout.Button("Edit", GUILayout.Width(40f)))
        //    //{
        //    //    if (nowAtlas != null)
        //    //    {
        //    //        SpriteEditor.Open(nowAtlas.mainTexture);
        //    //    }
        //    //}
        //    //GUILayout.EndHorizontal();
        //    //_tweener = property.serializedObject;
        //}
        ////public override void OnInspectorGUI()
        //{
        //    serializedObject.Update();
        //    OnPreInspectorGUI();
        //    serializedObject.ApplyModifiedProperties();

        //    base.OnInspectorGUI();

        //    serializedObject.Update();
        //    OnAfterInspectorGUI();
        //    serializedObject.ApplyModifiedProperties();
        //}

        //protected virtual void OnAfterInspectorGUI()
        //{
        //    //UnitUIEditorTool.DrawProperty("RaycastConsiderAlpha", serializedObject, "raycastConsiderAlpha");

        //}

        //protected virtual void OnPreInspectorGUI()
        //{
        //    _tweener = target as AtlasSpriteChangeTweener;
        //    if (_tweener == null) return;
        //    GUILayout.BeginHorizontal();
        //    if (GUILayout.Button("Atlas", "DropDown", GUILayout.Width(55f)))
        //    {
        //        ComponentSelector.Show<UAtlas>(OnSelectAtlas);
        //    }

        //    SerializedProperty sp = UnitUIEditorTool.DrawProperty("", serializedObject, "m_Atlas", GUILayout.MinWidth(20f));
        //    UAtlas nowAtlas = sp.objectReferenceValue as UAtlas;
        //    _tweener.atlas = nowAtlas;

        //    if (GUILayout.Button("Edit", GUILayout.Width(40f)))
        //    {
        //        if (nowAtlas != null)
        //        {
        //            SpriteEditor.Open(nowAtlas.mainTexture);
        //        }
        //    }
        //    GUILayout.EndHorizontal();
        //    ////////////////////////////////////////////////////////////////////////////////
        //    if (nowAtlas == null) //无图集
        //    {
        //        OnSelectAtlas(null, false);
        //        SelectSprite(null, false);
        //    }
        //    else
        //    {
        //        GUILayout.BeginHorizontal();
        //        if (GUILayout.Button("Sprite", "DropDown", GUILayout.Width(55f)))
        //        {
        //            SpritesSelector.Show(nowAtlas, _tweener.SpriteName, SelectSprite);
        //        }

        //        GUILayout.Label(_tweener.SpriteName, "HelpBox", GUILayout.Height(18f), GUILayout.MinWidth(20f));

        //        if (GUILayout.Button("Edit", GUILayout.Width(40f)))
        //        {
        //            if (_tweener.sprite != null)
        //            {
        //                SpriteEditor.Open(_tweener.sprite);
        //            }
        //        }
        //        GUILayout.EndHorizontal();
        //    }

        //    UnitUIEditorTool.DrawProperty("AutoSnap", serializedObject, "m_IsAutoSnap");
        //    UnitUIEditorTool.DrawProperty("ShowWhiteSource", serializedObject, "isShowWhiteSource");
        //}

        private void OnSelectAtlas(UnityEngine.Object obj, bool dirty = true)
        {
            _tweener.Atlas = obj as UAtlas;
            if (dirty)
                EditorUtility.SetDirty(obj);
        }

        //private void SelectSprite(string spriteName, bool dirty = true)
        //{
        //    //_tweener.SpriteName = spriteName;
        //    //if (dirty)
        //    //    EditorUtility.SetDirty(_tweener);
        //}
    }
}

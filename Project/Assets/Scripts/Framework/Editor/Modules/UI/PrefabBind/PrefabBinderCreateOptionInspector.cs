using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.UI.Editor.PrefabBinderHelp;
using Framework.Runtime.UI.PrefabBinderHelp;
using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Framework.Editor.Modules.UI.PrefabBind
{
    [CustomEditor(typeof(PrefabCreateOptions), true)]
    public class PrefabBinderCreateOptionInspector : OdinEditor
    {
        private PrefabCreateOptions prefabCreateOptions;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            prefabCreateOptions = target as PrefabCreateOptions;
            if (prefabCreateOptions.createOptionType == CreateOptionType.UIClass)
            {
                try
                {
                    DrawUICreateArea();
                }
                catch (System.Exception)
                {
                }
                
            }else if (prefabCreateOptions.createOptionType == CreateOptionType.Reference)
            {
                DrawReferenceCreateArea();
            }
            
        }
        private void DrawReferenceCreateArea()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            if (GUILayout.Button("更新变量声明", GUILayout.Height(26)))
            {
                PrefabBinderUtil.UpdatePrefabBinderDeclare(prefabCreateOptions.ReferenceCreateOption.isCustomFileName, prefabCreateOptions.ReferenceCreateOption.customFileName);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            if (GUILayout.Button("打开脚本", GUILayout.Height(26)))
            {
                PrefabBinderUtil.OpenScript(prefabCreateOptions.ReferenceCreateOption.isCustomFileName, prefabCreateOptions.ReferenceCreateOption.customFileName);
            }
            EditorGUILayout.EndHorizontal();
        }
        private void DrawUICreateArea()
        {
            // 使用 GUILayout 而不是 EditorGUILayout
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            if (GUILayout.Button("更新或创建视图并更新变量", GUILayout.Height(26)))
            {
                PrefabBinderUtil.CreateOrUpdateUIDeclare();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            if (GUILayout.Button("更新变量声明", GUILayout.Height(26)))
            {
                PrefabBinderUtil.UpdatePrefabBinderDeclare(prefabCreateOptions.UICreateOption.isCustomFileName, prefabCreateOptions.UICreateOption.customFileName);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            if (GUILayout.Button("打开脚本", GUILayout.Height(26)))
            {
                PrefabBinderUtil.OpenScript(prefabCreateOptions.UICreateOption.isCustomFileName, prefabCreateOptions.UICreateOption.customFileName);
            }
            EditorGUILayout.EndHorizontal();
        }
    
    }
}
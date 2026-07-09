using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Framework.Runtime.UI.Editor
{
    [CustomEditor(typeof(UCheckBoxGroup), true)]
    public class UCheckBoxGroupEditor : UContainerEditor
    {
        private string newTabName = "";
        private SerializedProperty curTabsSP;
        private SerializedProperty checkBoxListSP;
        private SerializedProperty selectedIndexListSP;
        private Queue<int> m_DeleteIndexQueue;
        private UCheckBoxGroup m_CheckBoxGroup2;
        protected override void OnEnable()
        {
            base.OnEnable();
            curTabsSP = serializedObject.FindProperty("m_Tabs");
            checkBoxListSP = serializedObject.FindProperty("m_CheckboxList");
            selectedIndexListSP = serializedObject.FindProperty("m_SelectedIndexList");
            m_DeleteIndexQueue = new Queue<int>();
            m_CheckBoxGroup2 = target as UCheckBoxGroup;
        }

        protected override void PreDrawInspectorGUI()
        {
            serializedObject.Update();
            bool getItemFromPool = UnitUIEditorTool.SerializedProperty(serializedObject, "m_GetItemFromPool").boolValue;
            //UnitUIEditorTool.SerializedProperty(serializedObject, "m_SelectedIndexList");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_Padding");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_Spacing");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_StartCorner");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_Axis");
            //if(!isOptimizeDisplay)
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_Alignment");
            if (UnitUIEditorTool.SerializedProperty(serializedObject, "m_Constraint").enumValueIndex == 2)
            {
                UnitUIEditorTool.SerializedProperty(serializedObject, "m_FixedCount");
            }
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_LoadingInterval");
            bool isCustomItemSize = UnitUIEditorTool.SerializedProperty(serializedObject, "m_IsCustomItemSize").boolValue;
            if (isCustomItemSize)
            {
                UnitUIEditorTool.SerializedProperty(serializedObject, "m_ItemSize");
            }
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_ChildPivot");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_AllowMultiSelect");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_AllowSwitchOff");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_XFlexSizeType");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_YFlexSizeType");
            //UnitUIEditorTool.SerializedProperty(serializedObject, "m_FlexContentSize");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_SyncRenderName");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_CheckBoxPrefab");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_CheckboxList");
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();

            base.PreDrawInspectorGUI();

        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("刷新视图"))
            {
                (target as UCheckBoxGroup)?.RefreshLayout(true);
            }
            base.OnInspectorGUI();

            if (GUILayout.Button("Clear"))
            {
                curTabsSP.ClearArray();
                checkBoxListSP.ClearArray();
                selectedIndexListSP.ClearArray();
                curTabsSP.arraySize = 0;
                checkBoxListSP.arraySize = 0;
                selectedIndexListSP.arraySize = 0;
                m_CheckBoxGroup2.ClearAllCheckBox();
            }
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            UnitUIEditorTool.DrawSeparator();
            /*
            * 绘制tabs
            */
            EditorGUILayout.BeginHorizontal();
            bool isRefresh = false;
            EditorGUIUtility.labelWidth = 60;
            newTabName = EditorGUILayout.TextField("新标签: ", newTabName);
            if (GUILayout.Button("Add"))
            {
                if (!string.IsNullOrEmpty(newTabName))
                {
                    // TODO : 添加tabs
                    //myScript.tabs.Add(newTabName);
                    curTabsSP.InsertArrayElementAtIndex(curTabsSP.arraySize);
                    curTabsSP.GetArrayElementAtIndex(curTabsSP.arraySize - 1).stringValue = newTabName;
                    newTabName = "";
                    isRefresh = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < curTabsSP.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var sp = curTabsSP.GetArrayElementAtIndex(i);
                string newValue = EditorGUILayout.TextField("标签名: ", sp.stringValue);
                if (newValue != sp.stringValue)
                {
                    isRefresh = true;
                    sp.stringValue = newValue;
                }
                if (GUILayout.Button("Remove"))
                {
                    m_DeleteIndexQueue.Enqueue(i);
                }
                EditorGUILayout.EndHorizontal();
            }


            for (int i = 0; i < m_DeleteIndexQueue.Count; i++)
            {
                var index = m_DeleteIndexQueue.Dequeue();
                m_CheckBoxGroup2.DeleteCheckBox(index);
                curTabsSP.DeleteArrayElementAtIndex(index);
                isRefresh = true;
            }
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            if (isRefresh)
                m_CheckBoxGroup2.RefreshLayout(true);


        }
    }
}
using UnityEditor;
using UnityEngine;

namespace Framework.Runtime.UI.Editor
{
    [CustomEditor(typeof(UList), true)]
    public class UListEditor : UContainerEditor
    {
      
        protected override void PreDrawInspectorGUI()
        {
            serializedObject.Update();
            if (GUILayout.Button("刷新视图"))
            {
                (target as UList)?.RefreshLayout();
            }
            bool getItemFromPool =
                UnitUIEditorTool.SerializedProperty(serializedObject, "m_GetItemFromPool").boolValue;
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_LayoutEfficiency");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_Padding");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_Spacing");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_StartCorner");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_XFlexSizeType");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_YFlexSizeType");
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
            //UnitUIEditorTool.SerializedProperty(serializedObject, "m_FlexContentSize");
            //UnitUIEditorTool.SerializedProperty(serializedObject, "m_AutoContentSize");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_SyncRenderNameSync");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_ListRenderPrefab");
            UnitUIEditorTool.SerializedProperty(serializedObject, "m_DataRefreshTimer");
            if(UnitUIEditorTool.SerializedProperty(serializedObject, "m_EnableVisibleAnimation").boolValue)
            {
                UnitUIEditorTool.SerializedProperty(serializedObject, "m_VisibleAnimationDuration");
                UnitUIEditorTool.SerializedProperty(serializedObject, "m_CanvasGroupFadeDuration");
            }
            serializedObject.Update();
            if (serializedObject.ApplyModifiedProperties())
            {
                (target as UList)?.RefreshLayout();
            }
            base.PreDrawInspectorGUI();
        }

     
      
    }
}
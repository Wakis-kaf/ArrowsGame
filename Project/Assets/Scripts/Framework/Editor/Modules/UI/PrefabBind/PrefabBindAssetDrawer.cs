using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Runtime.Modules.UI.PrefabBind;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.Editor.Modules.UI.PrefabBind
{
    [CustomPropertyDrawer(typeof(PrefabBindAsset))]
    public class PrefabBindAssetDrawer : PropertyDrawer
    {
        private const string KeyTitleName = "name";
        private const float LineHeight = 20f;
        private const float Spacing = 2f;
        private const float ButtonWidth = 50f;
        private const float DeleteButtonWidth = 40f;

        private Dictionary<Object, string[]> m_ObjCmpNames = new Dictionary<Object, string[]>();
        private Dictionary<Object, Object[]> m_ObjCmps = new Dictionary<Object, Object[]>();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return LineHeight + Spacing;
        }

        private void ClearBindAssetCurType(SerializedProperty property)
        {
            var prop = property.FindPropertyRelative("m_CurCmpType");
            prop.stringValue = "";
        }

        private void SetBindAssetCurType(SerializedProperty property, string type)
        {
            var prop = property.FindPropertyRelative("m_CurCmpType");
            prop.stringValue = type;
        }

        private string GetBindAssetCurType(SerializedProperty property)
        {
            var prop = property.FindPropertyRelative("m_CurCmpType");
            return prop.stringValue;
        }

        private void ClearBindAssetCmp(Object obj)
        {
            if (obj != null && m_ObjCmpNames.ContainsKey(obj))
            {
                m_ObjCmpNames.Remove(obj);
                m_ObjCmps.Remove(obj);
            }
        }

        private List<string> CheckBindAssetCmp(SerializedProperty property, Object obj)
        {
            if (obj == null)
            {
                return new List<string>();
            }
            List<string> cmpNames = new List<string>();
            List<Object> allCmps = new List<Object>();

            GameObject go = null;
            if (obj is GameObject goObj)
            {
                go = goObj;
            }
            if (obj is Component cmp)
            {
                go = cmp.gameObject;
            }

            if (go != null)
            {
                cmpNames.Add(go.GetType().FullName);
                allCmps.Add(go);

                var cmps = go.GetComponents<Component>();
                cmpNames.AddRange(cmps.Select((cmp) =>
                {
                    return cmp.GetType().FullName;
                }));
                allCmps.AddRange(cmps);
            }
            else
            {
                cmpNames.Add(obj.GetType().FullName);
                allCmps.Add(obj);
            }

            ClearBindAssetCmp(obj);
            m_ObjCmpNames.Add(obj, cmpNames.ToArray());
            m_ObjCmps.Add(obj, allCmps.ToArray());
            return cmpNames;
        }

        private void InitBindAssetCmp(SerializedProperty property, Object obj)
        {
            if (obj == null)
            {
                return;
            }
            var cmpNames = CheckBindAssetCmp(property, obj);
            SetBindAssetCurType(property, cmpNames[0]);
        }

        private void UpdateBindAssetCurType(SerializedProperty property, Object obj, string curType, Object newAsset)
        {
            SetBindAssetCurType(property, curType);
            var objProp = property.FindPropertyRelative("m_Asset");
            objProp.objectReferenceValue = newAsset;
        }

        private bool TryGetBindAssetCmps(Object obj, out string[] cmpNames, out Object[] cmps)
        {
            cmpNames = default;
            cmps = default;
            if (obj == null) return false;
            if (m_ObjCmpNames.TryGetValue(obj, out cmpNames))
            {
                m_ObjCmps.TryGetValue(obj, out cmps);
                return true;
            }
            return false;
        }

        private string GetNoRepeatName(SerializedProperty property, string name)
        {
            SerializedProperty listProperty = property.serializedObject.FindProperty("m_BindAssets");
            PrefabBinder prefabBinder = property.serializedObject.targetObject as PrefabBinder;
            return RenameUtil.GetNoRepeatName(prefabBinder.HasAsset, name);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 保存原始GUI状态
            Color originalColor = GUI.color;
            Color originalBackgroundColor = GUI.backgroundColor;
            float originalLabelWidth = EditorGUIUtility.labelWidth;

            try
            {
                EditorGUI.BeginProperty(position, label, property);

                // 设置背景色，让每个条目有轻微区分
                var backgroundRect = position;
                backgroundRect.height = LineHeight;
                if (Event.current.type == EventType.Repaint)
                {
                    var bgColor = EditorGUIUtility.isProSkin ?
                        new Color(0.2f, 0.2f, 0.2f, 0.3f) :
                        new Color(0.9f, 0.9f, 0.9f, 0.3f);
                    EditorGUI.DrawRect(backgroundRect, bgColor);
                }

                // 计算各个字段的矩形区域
                float totalWidth = position.width - DeleteButtonWidth - Spacing * 3;
                float nameWidth = totalWidth * 0.25f;
                float objectWidth = totalWidth * 0.35f;
                float popupWidth = totalWidth * 0.4f;

                Rect nameRect = new Rect(position.x, position.y, nameWidth, LineHeight);
                Rect objRect = new Rect(nameRect.xMax + Spacing, position.y, objectWidth, LineHeight);
                Rect popRect = new Rect(objRect.xMax + Spacing, position.y, popupWidth, LineHeight);
                Rect btnDelRect = new Rect(popRect.xMax + Spacing, position.y, DeleteButtonWidth, LineHeight);

                bool isDirty = false;

                // 名称字段
                EditorGUIUtility.labelWidth = 40f;
                var nameProp = property.FindPropertyRelative("m_Name");
                string newValue = EditorGUI.TextField(nameRect, "名称", nameProp.stringValue);
                if (newValue != nameProp.stringValue)
                {
                    nameProp.stringValue = GetNoRepeatName(property, newValue);
                    isDirty = true;
                }

                var isCustomProp = property.FindPropertyRelative("m_IsCustomAdd");
                if (isCustomProp != null)
                {
                    Rect customLabelRect = new Rect(nameRect.xMax - 45, nameRect.y, 40, nameRect.height);
                    GUIStyle miniStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleRight,
                        normal = { textColor = new Color(1f, 0.7f, 0.2f, 0.8f) },
                        fontSize = 9
                    };
                    string tip = isCustomProp.boolValue ? "手动" : "自动";
                    GUI.color = isCustomProp.boolValue ? Color.green : Color.red;
                    GUI.Label(customLabelRect, tip, miniStyle);
                    GUI.color = Color.white;
                }

                // 对象字段 - 关键修复：确保正确获取和设置对象引用
                EditorGUIUtility.labelWidth = 45f;
                var objProp = property.FindPropertyRelative("m_Asset");
                Object lastObj = objProp.objectReferenceValue;

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    Object newObj = EditorGUI.ObjectField(objRect, "引用", objProp.objectReferenceValue, typeof(Object), true);
                    if (check.changed)
                    {
                        objProp.objectReferenceValue = newObj;
                        isDirty = true;

                        // 重新获取组件列表并选取组件类型
                        ClearBindAssetCurType(property);
                        ClearBindAssetCmp(lastObj);

                        if (newObj != null)
                        {
                            InitBindAssetCmp(property, newObj);
                        }
                    }
                }

                // 确保当前对象在字典中有组件信息
                Object currentObj = objProp.objectReferenceValue;
                if (currentObj != null && !m_ObjCmpNames.ContainsKey(currentObj))
                {
                    CheckBindAssetCmp(property, currentObj);
                }

                // 组件类型下拉菜单 - 关键修复：确保组件信息正确显示
                if (TryGetBindAssetCmps(currentObj, out string[] cmps, out Object[] assets))
                {
                    string curTypeName = GetBindAssetCurType(property);
                    int index = Array.IndexOf(cmps, curTypeName);
                    if (index < 0) index = 0; // 如果当前类型不在列表中，选择第一个

                    EditorGUIUtility.labelWidth = 50f;
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        int select = EditorGUI.Popup(popRect, "组件", index, cmps);
                        if (check.changed && index != select)
                        {
                            isDirty = true;
                            UpdateBindAssetCurType(property, currentObj, cmps[select], assets[select]);
                        }
                    }
                }
                else
                {
                    // 没有组件时显示提示
                    EditorGUI.LabelField(popRect, "组件", currentObj == null ? "No Object" : "No Components");
                }

                // 删除按钮
                GUI.backgroundColor = Color.red;
                var deleteButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    normal = { textColor = Color.white },
                    hover = { textColor = Color.white },
                    active = { textColor = Color.white },
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                };

                if (GUI.Button(btnDelRect, "×", deleteButtonStyle))
                {
                    // 删除元素
                    PrefabBinder prefabBinder = property.serializedObject.targetObject as PrefabBinder;
                    if (prefabBinder != null)
                    {
                        prefabBinder.RemoveBind(nameProp.stringValue);
                    }
                }

                // 如果有修改，应用变化
                if (isDirty)
                {
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            finally
            {
                // 恢复原始GUI状态
                GUI.color = originalColor;
                GUI.backgroundColor = originalBackgroundColor;
                EditorGUIUtility.labelWidth = originalLabelWidth;
                EditorGUI.EndProperty();
            }
        }
    }
}
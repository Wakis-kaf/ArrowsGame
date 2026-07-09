using System.Collections.Generic;
using UnityEngine;

namespace Framework.Editor.Utils
{
    public class GUIToggleGroup
    {
        private int m_CurSelect = -1;
        private int m_LastSelect = -1;
        private Dictionary<int, bool> m_SelectDict;
        private List<string> m_Items;
        public Color normalColor = Color.white;
        public Color selectColor = Color.gray;
        public bool isDrawBtn = true;
        public GUIToggleGroup(List<string> items)
        {
            m_SelectDict = new Dictionary<int, bool>();
            m_Items = items;
            for (int i = 0; i <items.Count; i++)
            {
                m_SelectDict.Add(i,false);
            }
        }

        public void  Select(int index)
        {
            if (m_LastSelect!=-1)
            {
                m_SelectDict[m_LastSelect] = false;
            }
            m_LastSelect = m_CurSelect;
            m_CurSelect = index;
            m_SelectDict[m_CurSelect] = true;
        }
        public void OnGUI()
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < m_Items.Count; i++)
            {
                if (isDrawBtn)
                {
                    GUI.color = normalColor;
                    if (m_SelectDict[i])
                    {
                        GUI.color = selectColor;
                    }
                    if (GUILayout.Button(m_Items[i]))
                    {
                        Select(i);
                    }
                }
                else
                {
                    if (GUILayout.Toggle(m_SelectDict[i], m_Items[i]))
                    {
                        Select(i);
                    }    
                }
            }
            GUI.color = normalColor;
            GUILayout.EndHorizontal();
        }
    }
    public static class WindowGUILayoutUtility
    {
       
    }
}
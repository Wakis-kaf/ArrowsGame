using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Runtime.Module.Core;
using Sirenix.OdinInspector;
using UnityEngine;
namespace Framework.Runtime.MLanAndTheme
{
    public interface ILanAdapter
    {

        public bool IsEnableLan { get; set; }

        public LanguageType LanType { get; set; }

        public string LanId { get; }

        void SetCurrentLanType(LanguageType currentLanType);
    }
    public class Lan2LocalManager
    {
        private List<ILanAdapter> m_Adapters;
        private LanguageType m_CurrentLanType = LanguageType.Zh_CN;
        public LanguageType CurrentLanType
        {
            get { return m_CurrentLanType; }

        }
        public void SetCurrentLanType(LanguageType lanType)
        {
            if (m_CurrentLanType == lanType) return;
            m_CurrentLanType = lanType;
            foreach (var adapter in m_Adapters)
            {
                adapter.SetCurrentLanType(CurrentLanType);
            }
        }

        public Lan2LocalManager()
        {
            m_Adapters = new List<ILanAdapter>(20);
        }
        public void RegisterLanAdapter(ILanAdapter adapter)
        {
            if (!adapter.IsEnableLan) return;
            if (!m_Adapters.Contains(adapter))
            {
                m_Adapters.Add(adapter);
            }
            adapter.SetCurrentLanType(CurrentLanType);
        }

        public void UnRegisterLanAdapter(EnvAdapterComponent envAdapterComponent)
        {
            if (!m_Adapters.Contains(envAdapterComponent)) return;
            m_Adapters.Remove(envAdapterComponent);
        }
    }
}
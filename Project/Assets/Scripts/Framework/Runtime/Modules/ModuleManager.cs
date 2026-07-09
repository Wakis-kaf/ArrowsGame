using Framework.Runtime.LogSystem;
using Framework.Runtime.Module.Core;
using Framework.Utils;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Runtime.Module
{
    public  class ModuleManager
    {
        private  List<Type> m_CurDomainTypes;
        private  List<ModuleUnit> m_Modules;
        private  List<Type> m_ModuleTypes;

        public  void AppPopupUpdate(GameAppMessage appMessage)
        {
            for (int i = 0; i < m_Modules.Count; i++)
            {
                var module = m_Modules[i];
                module.OnAppPopupUpdate(appMessage);
            }
        }

        public  void AppUpdate(GameAppMessage appMessage)
        {
            for (int i = 0; i < m_Modules.Count; i++)
            {
                var module = m_Modules[i];
                module.OnAppUpdate(appMessage);
            }
        }

        public  T GetModuleUnit<T>() where T : ModuleUnit
        {
            return GetModuleUnit(typeof(T)) as T;
        }

        public  ModuleUnit GetModuleUnit(Type moduleUnitType)
        {
            for (int i = 0; i < m_Modules.Count; i++)
            {
                var module = m_Modules[i];
                if (module.Type == moduleUnitType)
                    return module;
            }

            return null;
        }

        public  void Init()
        {
            m_CurDomainTypes = new List<Type>();
            m_ModuleTypes = new List<Type>();
            m_Modules = new List<ModuleUnit>();
        }

        public  void ModuleManagerStop()
        {
            // 释放所有模块
            int count = m_Modules.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                m_Modules[i].Dispose(); // 回收模块的内存
            }

            // 清空数据
            m_CurDomainTypes.Clear();
            m_ModuleTypes.Clear();
            m_Modules.Clear();
        }

        public  void Start()
        {
            LoadBuiltInModule();
            LoadPluginModule();
            LoadGameThemeModule();
            GenerateModule();
            UpdateModule();
        }

        public  bool TryGetModuleUnit<T>(out T module) where T : ModuleUnit
        {
            module = GetModuleUnit<T>();
            return module != null;
        }

        private  int BuiltInModuleSort(Type type, Type type2)
        {
            var dict = ModuleGenerateTable.BuiltInModule;
            return dict[type] > dict[type2] ? 1 : -1;
        }

        private  int GameThemeModuleSort(Type type, Type type2)
        {
            var dict = ModuleGenerateTable.BuiltInGameThemeModule;
            return dict[type] > dict[type2] ? 1 : -1;
        }

        private  void GenerateModule()
        {
            for (int i = 0; i < m_ModuleTypes.Count; i++)
            {
                var type = m_ModuleTypes[i];
                Log.Debug($"注册模块 {type}");
                var module = Utility.ReflectionUtil.CreateInstance(type) as ModuleUnit;
                if (module == null)
                {
                    Log.Error($"注册模块 {type} fail");
                    continue;
                }

                m_Modules.Add(module);
                //module.DoConstruct();
            }
        }

        private  void LoadBuiltInModule()
        {
            var moduleTypes = ModuleGenerateTable.BuiltInModule.Keys.ToList();
            moduleTypes.Sort(BuiltInModuleSort);
            m_ModuleTypes.AddRange(moduleTypes);
        }

        private  void LoadGameThemeModule()
        {
            var moduleTypes = ModuleGenerateTable.BuiltInGameThemeModule.Keys.ToList();
            moduleTypes.Sort(GameThemeModuleSort);
            m_ModuleTypes.AddRange(moduleTypes);
        }

        private  void LoadPluginModule()
        {
            var moduleTypes = ModuleGenerateTable.BuiltInPluginModule.Keys.ToList();
            moduleTypes.Sort(PluginModuleSort);
            m_ModuleTypes.AddRange(moduleTypes);
        }

        private  int PluginModuleSort(Type type, Type type2)
        {
            var dict = ModuleGenerateTable.BuiltInPluginModule;
            return dict[type] > dict[type2] ? 1 : -1;
        }

        private  void UpdateModule()
        {
            for (int i = 0; i < m_Modules.Count; i++)
            {
                m_Modules[i].DoConstruct();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Framework.Utils
{
    public static partial class Utility
    {
        /// <summary>
        /// 程序集相关的实用函数。
        /// </summary>
        public static class AssemblyUtil
        {
            private static readonly List<System.Reflection.Assembly>s_Assemblies = null;

            private static readonly Dictionary<string, Type> s_CachedTypes =
                new Dictionary<string, Type>((IEqualityComparer<string>)StringComparer.Ordinal);
            private static readonly Dictionary<string, Assembly> s_AssembCachedMap =
                new Dictionary<string, Assembly>();

            static AssemblyUtil()
            {
                Utility.AssemblyUtil.s_Assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
                s_CachedTypes.Clear();
                s_AssembCachedMap.Clear();
            }
            public static void AddAssembly(Assembly assembly)
            {
                if(s_Assemblies.Contains(assembly))return;
                s_Assemblies.Add(assembly);

            }
            /// <summary>
            /// 获取已加载的程序集。
            /// </summary>
            /// <returns>已加载的程序集。</returns>
            public static List< System.Reflection.Assembly> GetAssemblies()
            {
                return Utility.AssemblyUtil.s_Assemblies;
            }
            /// <summary>
            /// 获取已加载的程序集中的指定类型。
            /// </summary>
            /// <param name="typeName">要获取的类型名。</param>
            /// <returns>已加载的程序集中的指定类型。</returns>
            public static Type GetType(Type type)
            {
                string typeName = type.FullName;
                if (string.IsNullOrEmpty(typeName))
                {
                    Debug.LogError("Type name is invalid.");
                    return null;
                }

                Type type1 = (Type)null;
                if (Utility.AssemblyUtil.s_CachedTypes.TryGetValue(typeName, out type1))
                    return type1;
                Type type2 = Type.GetType(typeName);
                if (type2 != null)
                {
                    Utility.AssemblyUtil.s_CachedTypes.Add(typeName, type2);
                    return type2;
                }
                string typeAssemblyName = type.Assembly.GetName().Name;
                var typeAssembly = GetAssemblyByName(typeAssemblyName);
                if (typeAssembly == null) return null;
                Assembly.Load(typeAssemblyName);
                string typeFullName = $"{typeName}, {typeAssembly.FullName}";
                Type type3 = Type.GetType(typeFullName);
                if (type3 != null)
                {
                    Utility.AssemblyUtil.s_CachedTypes.Add(typeName, type3);
                    return type3;
                }
                else
                {
                    Debug.LogError($"类型为空{typeName}");
                    return null;
                }

            }
            
            public static Assembly GetAssemblyByName(string assemblyName)
            {
                if (s_AssembCachedMap.ContainsKey(assemblyName))
                {
                    return s_AssembCachedMap[assemblyName];
                }
                for (int i = 0; i < s_Assemblies.Count; i++)
                {
                    System.Reflection.Assembly assembly = Utility.AssemblyUtil.s_Assemblies[i];
                    if (assembly.GetName().Name == assemblyName)
                    {
                        Assembly.Load(assemblyName);
                        s_AssembCachedMap.Add(assemblyName, assembly);
                        return assembly;
                    }
                }
                return null;    

            }
            public static Type GetType(string assemblyName,string className)
            {
                for (int i = 0; i < s_Assemblies.Count; i++)
                {
                    string curAssemblyName = s_Assemblies[i].GetName().Name;
                    if(curAssemblyName == assemblyName)
                    {
                        Assembly assembly = s_Assemblies[i];
                        Type type = assembly.GetType(className);
                        return type;
                    }
                }
                Debug.Log($"未找到{assemblyName}程序集");
                return null;
            }
            /// <summary>
            /// 获取已加载的程序集中的所有类型。
            /// </summary>
            /// <returns>已加载的程序集中的所有类型。</returns>
            public static Type[] GetTypes()
            {
                List<Type> typeList = new List<Type>();
                foreach (System.Reflection.Assembly assembly in Utility.AssemblyUtil.s_Assemblies)
                    typeList.AddRange((IEnumerable<Type>)assembly.GetTypes());
                return typeList.ToArray();
            }

            /// <summary>
            /// 获取已加载的程序集中的所有类型。
            /// </summary>
            /// <param name="results">已加载的程序集中的所有类型。</param>
            public static void GetTypes(List<Type> results)
            {
                if (results == null)
                    throw new Exception("Results is invalid.");
                results.Clear();
                foreach (System.Reflection.Assembly assembly in Utility.AssemblyUtil.s_Assemblies)
                    results.AddRange((IEnumerable<Type>)assembly.GetTypes());
            }

            public static void LoadAssemblyByBytes(byte[] bytes)
            {
                Assembly assembly = Assembly.Load(bytes);
                if (assembly != null)
                {
                    Debug.Log("加载程序集成功!");
                    AddAssembly(assembly);
                }
                
            }
        }
    }
}
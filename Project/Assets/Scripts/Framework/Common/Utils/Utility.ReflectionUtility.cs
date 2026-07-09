using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Framework.Utils
{
    public static partial class Utility
    {
        public static class ReflectionUtil
        {
            private static HashSet<string> LoadedAssembly = new HashSet<string>();

            /// <summary>
            /// 创建当前程序集中某个类的派生类实例
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public static List<T> CreateDerivedClassInstance<T>(bool containSelf = false) where T : class
            {
                List<Type> types = GetDerivedClassType<T>(containSelf);
                List<T> instances = new List<T>();
                foreach (var type in types)
                {
                    T instance = CreateInstance(type) as T;
                    instances.Add(instance);
                }

                return instances;
            }

            public static T CreateInstance<T>() where T : class
            {
                Type type = Utility.AssemblyUtil.GetType(typeof(T));
                if (type != null)
                {
                    return (T)System.Activator.CreateInstance(type);
                }
                return null;
            }

            public static T CreateInstance<T>(params object[] objects)
            {
                try
                {
                    Type type = Utility.AssemblyUtil.GetType(typeof(T));
                    return (T)System.Activator.CreateInstance(type, objects);
                }
                catch (Exception e)
                {
                    Debug.LogError($"CreateInstance 错误 {e}");
                    return default;
                }
            }

            public static T CreateInstance<T>(Type type)
            {
                type = Utility.AssemblyUtil.GetType(type);
                return (T)System.Activator.CreateInstance(type);
            }

            public static T CreateInstance<T>(Type type, params object[] objects)
            {
                type = Utility.AssemblyUtil.GetType(type);
                return (T)System.Activator.CreateInstance(type, objects);
            }

            public static object CreateInstance(Type type, params object[] objects)
            {
                type = Utility.AssemblyUtil.GetType(type);
                if(type == null)
                {
                    Debug.LogError($"create Instance null error{type}");
                    return null;
                }
                try
                {
                    return System.Activator.CreateInstance(type, objects);

                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    return null;
                }

            }

            public static T DeepCopyByReflection<T>(T obj)
            {
                if (obj is string || obj.GetType().IsValueType)
                    return obj;

                object retval = Activator.CreateInstance(obj.GetType());
                FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    try
                    {
                        field.SetValue(retval, DeepCopyByReflection(field.GetValue(obj)));
                    }
                    catch { }
                }

                return (T)retval;
            }

            public static Type[] GetAssignedClassOf(Type interfaceType, Type attributeType = null)
            {
                if (!interfaceType.IsInterface) return null;
                var assemblyAllTypes = Utility.AssemblyUtil.GetTypes();
                var res = new List<Type>();
                for (int i = 0; i < assemblyAllTypes.Length; i++)
                {
                    Type item = assemblyAllTypes[i];
                    if (!item.IsClass) continue;
                    if (interfaceType.IsAssignableFrom(item))
                    {
                        if (attributeType != null && !Attribute.IsDefined(item, attributeType))
                            continue;
                        res.Add(item);
                    }
                }

                return res.ToArray();
            }

            public static Type[] GetAssignedClassOf<T>()
            {
                var interfaceType = typeof(T);
                return GetAssignedClassOf(interfaceType);
            }

            public static Type[] GetAssignedClassOf<T>(Type attributeType)
            {
                var interfaceType = typeof(T);
                return GetAssignedClassOf(interfaceType, attributeType);
            }

            /// <summary>
            /// 获取当前程序集中某个类的派生类类型
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public static List<Type> GetDerivedClassType<T>(bool containSelf = false) where T : class
            {
                // 获取所有的类型
                var types = Utility.AssemblyUtil.GetTypes();
                var baseType = typeof(T);
                List<Type> derivedClassType = new List<Type>();
                foreach (var type in types)
                {
                    // 判断 type 是否继承于基类 baseType
                    if (type.IsSubclassOf(baseType) && (!containSelf && type != baseType))
                    {
                        derivedClassType.Add(type);
                    }
                }

                return derivedClassType;
            }

            /// <summary>
            /// 获取当前程序集中某个类的派生类类型名字
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public static List<string> GetDerivedClassTypeName<T>(bool containSelf = false) where T : class
            {
                // 获取所有的类型
                var types = Utility.AssemblyUtil.GetTypes();
                var baseType = typeof(T);
                List<string> derivedClassType = new List<string>();
                foreach (var type in types)
                {
                    // 判断 type 是否继承于基类 baseType
                    if (type.IsSubclassOf(baseType) && (!containSelf && type != baseType))
                    {
                        derivedClassType.Add(type.ToString());
                    }
                }

                return derivedClassType;
            }

            public static Type[] GetSubClassOf<T>(Type attrType = null)
            {
                return GetSubClassOf(typeof(T), attrType);
            }

            public static Type[] GetSubClassOf(Type type, Type attrType = null)
            {
                var assemblyAllTypes = Utility.AssemblyUtil.GetTypes();
                var names = new List<Type>();
                for (int i = 0; i < assemblyAllTypes.Length; i++)
                {
                    Type item = assemblyAllTypes[i];
                    if (item.IsSubclassOf(type))
                    {
                        if (attrType != null && !Attribute.IsDefined(item, attrType)) continue;
                        names.Add(item);
                    }
                }

                return names.ToArray();
            }

            public static Type[] GetSubClassOfRawGeneric(Type type, bool ignoreGenericType = false)
            {
                var assemblyAllTypes = Utility.AssemblyUtil.GetTypes();
                var names = new List<Type>();
                for (int i = 0; i < assemblyAllTypes.Length; i++)
                {
                    if (assemblyAllTypes[i].IsSubClassOfRawGeneric(type))
                    {
                        if (ignoreGenericType && assemblyAllTypes[i].IsGenericType) continue;
                        names.Add(assemblyAllTypes[i]);
                    }
                }

                return names.ToArray();
            }

            public static bool IsImplementedGenericInterface(Type interfaceType, Type targetType,
                out Type genericInterfaceType)
            {
                var interfaces = targetType.GetInterfaces();
                genericInterfaceType = null;
                bool hasImplemented = false;
                if (interfaces.Length > 0)
                {
                    foreach (var inter in interfaces)
                    {
                        if (!inter.IsGenericType) continue;

                        var t = inter.GetGenericTypeDefinition();

                        if (t == interfaceType)
                        {
                            genericInterfaceType = inter;
                            hasImplemented = true;
                            break;
                        }
                    }
                }

                return hasImplemented;
            }

            public static bool TryGetGenericTypesDefinition(Type type, out Type[] types)
            {
                types = null;
                if (!type.IsGenericType) return false; // 判断是否是泛型
                types = type.GetGenericArguments();
                return true;
            }

            private static void EnsureAssemblyLoaded(Type targetType)
            {
                var assembly = targetType.Assembly;
                var assemblyName = assembly.GetName().Name;
                if (LoadedAssembly.Contains(assemblyName))
                {
                    return;
                }
                var assemblies = AssemblyUtil.GetAssemblies();
                for (int i = 0; i < assemblies.Count; i++)
                {
                    if (assemblies[i].GetName().Name == assemblyName)
                    {
                        Assembly.Load(assemblyName);
                        LoadedAssembly.Add(assemblyName);
                    }
                }
            }
        }
    }
}
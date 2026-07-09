using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class TypeExtension
{
    public static bool HasBaseType(this Type type, Type baseType, bool isJudge = false)
    {
        if (type == baseType)
        {
            if (isJudge) return true;
            else return false;
        }
        Type objType = typeof(object);
        while (type != baseType && type != objType)
        {
            type = type.BaseType;
        }

        return type == baseType;
    }

    public static bool IsNewAble(this Type type, Type[] constructorArgsType = null)
    {
        if (type.IsAbstract || type.IsStatic() || type.IsSubOrEqualOf(typeof(MonoBehaviour))) return false;
        if (constructorArgsType == null)
        {
            constructorArgsType = Array.Empty<Type>();
        }
        return type.GetConstructor(constructorArgsType) != null;
    }

    public static bool IsNewAbleWithArgs(this Type type, params object[] args)
    {
        List<Type> types = new List<Type>();
        if (args != null)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == null)
                {
                    types.Add(typeof(object));
                }
                else
                {
                    types.Add(args[i].GetType());
                }
            }
        }

        return type.IsNewAble(types.ToArray());
    }

    public static bool IsStatic(this MemberInfo member)
    {
        FieldInfo fieldInfo = member as FieldInfo;
        if (fieldInfo != null)
        {
            return fieldInfo.IsStatic;
        }

        PropertyInfo propertyInfo = member as PropertyInfo;
        if (propertyInfo != null)
        {
            if (!propertyInfo.CanRead)
            {
                return propertyInfo.GetSetMethod(nonPublic: true).IsStatic;
            }

            return propertyInfo.GetGetMethod(nonPublic: true).IsStatic;
        }

        MethodBase methodBase = member as MethodBase;
        if (methodBase != null)
        {
            return methodBase.IsStatic;
        }

        EventInfo eventInfo = member as EventInfo;
        if (eventInfo != null)
        {
            return eventInfo.GetRaiseMethod(nonPublic: true).IsStatic;
        }

        Type type = member as Type;
        if (type != null)
        {
            if (type.IsSealed)
            {
                return type.IsAbstract;
            }

            return false;
        }
        return false;
    }

    public static bool IsSubClassOfRawGeneric(this Type type, Type generic)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (generic == null) throw new ArgumentNullException(nameof(generic));

        while (type != null && type != typeof(object))
        {
            bool isTheRawGenericType = IsTheRawGenericType(type);
            if (isTheRawGenericType) return true;
            type = type.BaseType;
        }

        return false;

        bool IsTheRawGenericType(Type test)
            => generic == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
    }

    public static bool IsSubOrEqualOf(this Type type, Type target)
    {
        return type == target || type.IsSubclassOf(target);
    }
}
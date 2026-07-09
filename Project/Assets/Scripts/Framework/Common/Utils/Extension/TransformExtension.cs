using Framework.Utils;
using System;
using System.Linq;
using UnityEngine;

public static class TransformExtension
{
    public static Transform FindDeep(this Transform transform, string target)
    {
        Transform tarTransform = transform.Find(target);
        if (tarTransform == null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                tarTransform = transform.GetChild(i).FindDeep(target);
                if (tarTransform != null) return tarTransform;
            }
        }
        return tarTransform;
    }

    public static Component GetChildComponent(this Transform transform, string componentType)
    {
        return transform.GetComponentInChildren(Type.GetType(componentType));
    }

    public static Component GetComponentInChild(this Transform transform, string componentType, string childName)
    {
        return transform.GetComponentInChild(Type.GetType(componentType), childName);
    }

    public static Component GetComponentInChild(this Transform transform, Type componentType, string childName)
    {
        var cmps = transform.GetComponentsInChildren(componentType, true).Where(cmp =>
        {
            return (cmp as Component).gameObject.name.Equals(childName);
        });
        return cmps.Any() ? cmps.ElementAt(0) : default;
    }

    public static T GetComponentInChild<T>(this Transform transform, string childName)
    {
        var cmps = transform.GetComponentsInChildren<T>(true).Where(item =>
        {
            Component cmp = item as Component;
            return cmp.gameObject.name.Equals(childName);
        });
        var enumerable = cmps as T[] ?? cmps.ToArray();
        return enumerable.Any() ? enumerable.ElementAt(0) : default;
    }

    public static T GetComponentInChild<T>(this MonoBehaviour mono, string childName)
    {
        var cmps = mono.GetComponentsInChildren<T>(true).Where(cmp =>
        {
            return (cmp as Component).gameObject.name.Equals(childName);
        });
        return cmps.Any() ? cmps.ElementAt(0) : default;
    }

    public static T GetComponentInChild<T>(this RectTransform rectTransform, string childName)
    {
        var cmps = rectTransform.GetComponentsInChildren<T>(true).Where(cmp =>
        {
            return (cmp as Component).gameObject.name.Equals(childName);
        });
        return cmps.Any() ? cmps.ElementAt(0) : default;
    }

    public static T GetComponentInFirstChild<T>(this Transform transform)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.TryGetComponent<T>(out T res))
            {
                return res;
            }
        }

        return default;
    }

    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        Type type = Utility.AssemblyUtil.GetType(typeof(T));
        if (go.TryGetComponent(type, out Component res)) return res as T;
        res = go.AddComponent(type);
        return res as T;
    }

    public static Component GetOrAddComponent(this GameObject go, Type type)
    {
        Component res;
        type = Utility.AssemblyUtil.GetType(type);
        if (go.TryGetComponent(type, out res)) return res;
        res = go.AddComponent(type);
        return res;
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Utils
{
    public class GameObjectUtil
    {
        public static Component AddComponent(GameObject gameObject, string type)
        {
            Type getType = Utility.AssemblyUtil.GetType(Type.GetType(type));
            if (getType == null) return null;
            return gameObject.AddComponent(getType);
        }

        public static Component AddComponent(GameObject gameObject, Type type)
        {
            return gameObject.AddComponent(type);
        }

        public static void DestroyChilds(Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                GameObject.Destroy(child.gameObject);
            }
        }

        public static void DestroyChildsImmediate(Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                GameObject.DestroyImmediate(child.gameObject);
            }
        }

        public static Transform FindDeep(Transform transform, string target)
        {
            return transform.FindDeep(target);
        }

        public static List<Transform> GetAllChildObjects(Transform parent)
        {
            List<Transform> list = new List<Transform>();

            GetAllChildObjects(parent, list);
            return list;
        }

        public static List<Transform> GetAllChildObjects(Transform parent, List<Transform> list)
        {
            int childCount = parent.childCount;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = parent.GetChild(i);
                // 添加子对象到列表
                list.Add(child);

                // 递归调用，获取深层子对象
                GetAllChildObjects(child, list);
            }

            return list;
        }

        public static Component GetChildComponentByName(Transform transform, string childName, string cmpTypeName,
            bool includeSelf = true)
        {
            Transform curTransform = transform;
            Component cmp = null;
            if (includeSelf && curTransform.name == childName)
            {
                cmp = GetComponent(curTransform, cmpTypeName);
                if (cmp != null) return cmp;
            }

            // 寻找子组件
            for (int i = 0; i < curTransform.childCount; i++)
            {
                curTransform = curTransform.GetChild(i);
                if (curTransform.name == childName)
                {
                    cmp = GetComponent(curTransform, cmpTypeName);
                    if (cmp == null)
                    {
                        // 递归寻找
                        cmp = GetChildComponentByName(curTransform, childName, cmpTypeName, false);
                    }

                    if (cmp != null)
                    {
                        return cmp;
                    }
                }
            }

            return null;
        }

        public static Component GetComponent(GameObject gameObject, Type type)
        {
            return gameObject.GetComponent(type);
        }

        public static Component GetComponent(Transform transform, string cmpTypeName)
        {
            Type type = Utility.AssemblyUtil.GetType(Type.GetType(cmpTypeName));
            if (type == null)
            {
                Component[] cmps = transform.GetComponents<Component>();
                for (int i = 0; i < cmps.Length; i++)
                {
                    if (cmps.GetType().Name == cmpTypeName) return cmps[i];
                }
            }

            return transform.GetComponent(cmpTypeName);
        }

        public static Component GetComponentByType(Transform transform, Type type)
        {
            type = Utility.AssemblyUtil.GetType(type);
            return transform.GetComponent(type);
        }

        public static Component GetComponentInChildrenByType(Transform transform, Type type)
        {
            type = Utility.AssemblyUtil.GetType(type);
            return transform.gameObject.GetComponentInChildren(type);
        }

        public static T[] GetComponentsInChild<T>(Transform transform) where T : MonoBehaviour
        {
            int childCount = transform.childCount;
            List<T> res = new List<T>();
            for (int i = 0; i < childCount; i++)
            {
                foreach (var component in transform.GetChild(i).GetComponents<T>())
                {
                    res.Add(component);
                }
            }
            return res.ToArray();
        }

        public static Component[] GetComponentsInChildren(Transform transform, Type type, bool includeInactive = true)
        {
            type = Utility.AssemblyUtil.GetType(type);
            return transform.GetComponentsInChildren(type, includeInactive);
        }

        public static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            T res;
            if (go.TryGetComponent<T>(out res)) return res;
            res = go.AddComponent<T>();
            return res;
        }

        public static void SetActive(Component component, bool active)
        {
            if (component == null) return;
            SetActive(component.gameObject, active);
        }

        public static void SetActive(GameObject gameObject, bool active)
        {
            if (gameObject == null) return;
            if(gameObject.activeSelf != active)
            {
                gameObject.SetActive(active);
            }
           
        }

        public static void SetAnchorPosition(RectTransform transform, float x, float y)
        {
            transform.anchoredPosition = new Vector2(x, y);
        }

        public static void SetAnchorPosition3D(RectTransform transform, float x, float y, float z)
        {
            transform.anchoredPosition3D = new Vector3(x, y, z);
        }

        public static void SetEulerAngles(GameObject gameObject, float x, float y, float z)
        {
            SetSetEulerAngles(gameObject.transform, x, y, z);
        }

        public static void SetLayer(GameObject gameObject, int layer, bool isDeep = true)
        {
            gameObject.layer = layer;
            if (isDeep)
            {
                var gos = gameObject.GetComponentsInChildren<Transform>();
                foreach (var tran in gos)
                {
                    tran.gameObject.layer = layer;
                }
            }
        }

        public static void SetLayer(GameObject gameObject, string layerName, bool isDeep = true)
        {
            SetLayer(gameObject, LayerMask.NameToLayer(layerName), isDeep);
        }

        public static void SetParent(GameObject gameObject, Transform parent)
        {
            SetParent(gameObject.transform, parent);
        }

        public static void SetParent(Component component, Transform parent)
        {
            SetParent(component.transform, parent);
        }

        public static void SetParent(Transform transform, Transform parent)
        {
            transform.SetParent(parent);
        }

        public static void SetPosition(GameObject gameObject, float x, float y, float z)
        {
            SetPosition(gameObject.transform, x, y, z);
        }

        public static void SetPosition(Component component, float x, float y, float z)
        {
            SetPosition(component.transform, x, y, z);
        }

        public static void SetPosition(Transform transform, float x, float y, float z)
        {
            transform.position = new Vector3(x, y, z);
        }

        public static void SetScale(Transform transform, Vector3 scale)
        {
            transform.localScale = scale;
        }

        public static void SetScale(Transform transform, float x, float y, float z)
        {
            transform.localScale = new Vector3(x, y, z);
        }

        public static void SetSetEulerAngles(Component component, float x, float y, float z)
        {
            SetSetEulerAngles(component.transform, x, y, z);
        }

        public static void SetSetEulerAngles(Transform transform, float x, float y, float z)
        {
            transform.eulerAngles = new Vector3(x, y, z);
        }
    }
}
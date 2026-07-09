using Framework.Runtime.UI;
using Framework.Runtime.UI.PrefabBinderHelp;
using Spine.Unity;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Framework.Runtime.Modules.UI.PrefabBind
{
    [Serializable]
    public class PrefabBindAsset
    {
        [SerializeField]
        private Object m_Asset;

        [SerializeField]
        private string m_CurCmpType;

        [SerializeField]
        private string m_Name;
        [SerializeField]
        private bool m_IsCustomAdd;


        public PrefabBindAsset()
        {
        }
        public bool IsCustomAdd => m_IsCustomAdd;
        public Object Asset => m_Asset;
        public string Name => m_Name;

        public void Set(string name, Object asset, string type, bool isCustomAdd = false)
        {
            m_Name = name;
            m_Asset = asset;
            m_CurCmpType = type;
            m_IsCustomAdd = isCustomAdd;
        }
    }

    public class PrefabBinder : MonoBehaviour
    {
#if UNITY_EDITOR
        #region
        public static Dictionary<string, System.Type> SmartTypePrefix = new Dictionary<string, Type>()
        {
            { "go", typeof(GameObject)},
            { "rt", typeof(RectTransform) },
            { "rect", typeof(RectTransform) },
            {"trans",typeof(Transform)},
            {"pb",typeof(PrefabBinder)},
            {"uckbGroup",typeof(UCheckBoxGroup)},
            {"usp",typeof(USprite)},
            {"utxt",typeof(UText)},
            {"utmpTxt",typeof(UTMPText)},
            {"ulist",typeof(UList)},
            {"ubtn",typeof(UButton)},
            {"utex",typeof(UTexture)},
            {"uctr",typeof(UContainer)},
            {"uscb",typeof(USimpleCheckBox)},
            {"ustb",typeof(USimpleTabBar)} ,
            {"uif",typeof(UInputField)},
            {"uds",typeof(UDropSelect)},
            {"uckbtabNav",typeof(UCkbTabNavigation)},
            {"uckb",typeof(UCheckBox)},
            {"upb",typeof(UProgressBar)},
            {"uvpb",typeof(UValueProgress)},
            {"uimg",typeof(UImage)},
            {"img",typeof(Image)},
            {"sr",typeof(SpriteRenderer)},
            {"bc2",typeof(BoxCollider2D)},
            {"skelAnim",typeof(SkeletonAnimation)},
            {"uguidMask",typeof(UGuideMask)},
            {"tmp",typeof(TextMeshPro)},
            {"gridLayoutGroup",typeof(GridLayoutGroup)},
            {"uBaseRender",typeof(UIBaseRender)},
            {"camera",typeof(Camera)},

            {"lineRender",typeof(LineRenderer)},

        };
        #endregion
#endif
        [SerializeField]
        private List<PrefabBindAsset> m_BindAssets = new List<PrefabBindAsset>();
        [SerializeField]
        private bool m_AutoGetInChildPrefabBinder = false;
        [SerializeField]
        private bool m_AutoGetChildPrefabBinderSelf = true;
        public bool AutoGetInChildPrefabBinder => m_AutoGetInChildPrefabBinder;
        public bool AutoGetChildPrefabBinderSelf => m_AutoGetChildPrefabBinderSelf;
        public List<string> NameList
        {
            get
            {
                List<string> list = new List<string>();
                for (int i = m_BindAssets.Count - 1; i >= 0; i--)
                {
                    list.Add(m_BindAssets[i].Name);
                }
                return list;
            }
        }

        public void AddBind(string name, Object asset, string type, bool isCustomAdd)
        {
            var passet = new PrefabBindAsset();
            passet.Set(name, asset, type, isCustomAdd);
            m_BindAssets.Add(passet);
        }

        public void Clear()
        {

            //m_BindAssets.Clear();
        }

        public void ClearAllBind(bool includeCustomAdd = false)
        {
            if (includeCustomAdd)
            {
                m_BindAssets.Clear();
            }
            else
            {
                for (int i = m_BindAssets.Count - 1; i >= 0; i--)
                {
                    if (!m_BindAssets[i].IsCustomAdd)
                    {
                        m_BindAssets.RemoveAt(i);

                    }
                }
            }

        }
        public bool TryGetObj<T>(out T obj) where T : Object
        {
            for (int i = m_BindAssets.Count - 1; i >= 0; i--)
            {
                if (m_BindAssets[i].Asset is T tAsset)
                {
                    obj = tAsset;
                    return true;
                }
            }
            obj = null;
            return false;
        }
        public T GetObj<T>() where T : Object
        {
            for (int i = m_BindAssets.Count - 1; i >= 0; i--)
            {
                if (m_BindAssets[i].Asset is T tAsset)
                {
                    return tAsset;
                }
            }
            return null;
        }
        public T GetObj<T>(string name) where T : Object
        {
            for (int i = m_BindAssets.Count - 1; i >= 0; i--)
            {
                if (m_BindAssets[i].Name == name)
                {
                    return m_BindAssets[i].Asset as T;
                }
            }
            return TryFindObject<T>(name);
        }

        public Object GetObj(string name)
        {
            return GetObj<Object>(name);
        }

        public Object GetObjectByType(string typeName)
        {
            for (int i = m_BindAssets.Count - 1; i >= 0; i--)
            {
                var type = m_BindAssets[i].Asset.GetType();
                if (type.FullName == typeName || type.Name == typeName)
                {
                    return m_BindAssets[i].Asset;
                }
            }

            return null;
        }

        public bool HasAsset(string name)
        {
            for (int i = m_BindAssets.Count - 1; i >= 0; i--)
            {
                if (m_BindAssets[i].Name == name)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasAsset(Object asset)
        {
            for (int i = m_BindAssets.Count - 1; i >= 0; i--)
            {
                if (m_BindAssets[i].Asset == asset)
                {
                    return true;
                }
            }

            return false;
        }

        public void RemoveBind(string name)
        {
            for (int i = m_BindAssets.Count - 1; i >= 0; i--)
            {
                if (m_BindAssets[i].Name == name)
                {
                    m_BindAssets.RemoveAt(i);
                }
            }
        }

        public T TryFindObject<T>(string name) where T : Object
        {
            var child = transform.Find(name);
            if (child == null) return default;
            return child.GetComponent<T>();
        }

        // private void OnTransformChildrenChanged() { // 重新识别 Debug.Log("触发重新识别"); }
    }
}
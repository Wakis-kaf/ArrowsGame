using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace Framework.Runtime.MSceneUnit
{
    public class UnitSkinController : SceneUnitComponent
    {
        private Dictionary<string, Transform> m_AvatarPartMap = new Dictionary<string, Transform>();
        private SceneUnitBoneAgent m_BoneAgent;
        [SerializeField] private GameObject m_SkinGO;
        [FormerlySerializedAs("avatarRoot")][SerializeField] private UnitRoot unitRoot;

        public SceneUnitBoneAgent BoneAgent => m_BoneAgent;
        public GameObject SkinGo => m_SkinGO;

        public virtual void BindSkin(GameObject skinGO)
        {
            m_SkinGO = skinGO;
            unitRoot = OwnSceneUnit.UnitRoot;
            m_BoneAgent = SkinGo.GetComponentInChildren<SceneUnitBoneAgent>();
            OwnSceneUnit.UnitRoot.AddChildUnit(skinGO.GetOrAddComponent<Skin>());
            skinGO.transform.SetParent(OwnSceneUnit.UnitRoot.transform, false);
            // 查找角色下所有的SkinMeshRender组件
            var skinParts = skinGO.transform.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < skinParts.Length; i++)
            {
                var skinPart = skinParts[i];
                var skinPartTransform = skinPart.transform;
                m_AvatarPartMap.Add(skinPartTransform.name, skinPartTransform);
            }
        }

        public void DecodeAvatarUrl(string url)
        {
            string[] items = url.Split("#");
            for (int i = 0; i < items.Length; i++)
            {
                if (!string.IsNullOrEmpty(items[i]))
                {
                    var kvp = items[i].Split(":");
                    string name = kvp[0];
                    bool status = kvp[1] == "1";
                    SetAvatarPartState(name, status);
                }
            }
        }

        public string GetAvatarUrl()
        {
            StringBuilder url = new StringBuilder();
            for (int i = 0; i < m_AvatarPartMap.Keys.Count; i++)
            {
                var key = m_AvatarPartMap.Keys.ElementAt(i);
                string status = m_AvatarPartMap[key].gameObject.activeSelf ? "1" : "0";
                url.Append(key + ":" + status);
                url.Append("#");
            }

            return url.ToString();
        }

        public void SetAllPartStatus(bool status)
        {
            for (int i = 0; i < m_AvatarPartMap.Keys.Count; i++)
            {
                m_AvatarPartMap[m_AvatarPartMap.Keys.ElementAt(i)].gameObject.SetActive(status);
            }
        }

        public void SetAvatarPartState(string partName, bool visible)
        {
            if (m_AvatarPartMap.TryGetValue(partName, out var part))
            {
                part.gameObject.SetActive(visible);
            }
            else
            {
                part = unitRoot.transform.FindDeep(partName);
                if (part != null)
                {
                    part.gameObject.SetActive(visible);
                    m_AvatarPartMap.Add(partName, part.transform);
                }
            }
        }

        protected override void OnSceneUnitModelLoaded()
        {
            base.OnSceneUnitModelLoaded();
            BindSkin(OwnSceneUnit.UnitModelGo);
        }
    }
}
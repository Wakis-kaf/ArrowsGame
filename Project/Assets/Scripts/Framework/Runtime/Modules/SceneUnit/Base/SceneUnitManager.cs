using Framework.Runtime.Module.Core;
using Framework.Runtime.UnitSystem.Base;
using UnityEngine;

namespace Framework.Runtime.MSceneUnit
{
    public class SceneUnitManager : ModuleUnit 
    {
        private Transform m_SceneAnimalUnitRoot;
        private Transform m_SceneRoleUnitRoot;
        private Transform m_SceneUnitRoot;
        public SceneUnitEventDispatcher SceneUnitEventDispatcher { get; private set; }
        protected override void OnModuleConstructed()
        {
            base.OnModuleConstructed();
            SceneUnitEventDispatcher = new SceneUnitEventDispatcher();
        }

        public Transform SceneAnimalUnitRoot
        {
            get
            {
                if (m_SceneAnimalUnitRoot == null)
                {
                    m_SceneAnimalUnitRoot = new GameObject("SceneAnimalUnitsRoot").transform;
                }

                return m_SceneAnimalUnitRoot;
            }
        }

        public Transform SceneRoleUnitRoot
        {
            get
            {
                if (m_SceneRoleUnitRoot == null)
                {
                    m_SceneRoleUnitRoot = new GameObject("SceneRoleUnitsRoot").transform;
                }

                return m_SceneRoleUnitRoot;
            }
        }

        public Transform SceneUnitRoot
        {
            get
            {
                if (m_SceneUnitRoot == null)
                {
                    m_SceneUnitRoot = new GameObject("SceneUnitsRoot").transform;
                    m_SceneUnitRoot.SetParent(GameApp.Ins.GameAppShell.transform);
                }

                return m_SceneUnitRoot;
            }
        }
    }
}
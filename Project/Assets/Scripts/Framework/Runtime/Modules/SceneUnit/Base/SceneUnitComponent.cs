using Framework.Runtime.UnitSystem.BIInterfaces;
using Framework.Runtime.UnitSystem.MonoBase;
using UnityEngine;

namespace Framework.Runtime.MSceneUnit
{
    public interface ISceneUnitComponent
    {
        public bool IsLoaded();
        public void BindOwnerUnit(SceneUnit ownerSceneUnit);
    }

    public abstract class SceneUnitComponent : MonoBehaviourUnit, IUnitDestroy, ISceneUnitComponent
    {
        private SceneUnit m_OwnSceneUnit;
        public SceneUnit OwnSceneUnit => m_OwnSceneUnit;
        public virtual Transform UnitEntityTransform => OwnSceneUnit.EntityRoot;

        public virtual bool IsLoaded()
        {
            return true;
        }
        public void BindOwnerUnit(SceneUnit ownerSceneUnit)
        {
            if (m_OwnSceneUnit == ownerSceneUnit) return;
            m_OwnSceneUnit = ownerSceneUnit;
            if (m_OwnSceneUnit == null)
            {
                OwnSceneUnit?.RemoveEnableListener(OnSceneUnitEnable);
                OwnSceneUnit?.RemoveDisableListener(OnSceneUnitDisable);
            }
            else
            {
                OwnSceneUnit?.AddEnableListener(OnSceneUnitEnable);
                OwnSceneUnit?.AddDisableListener(OnSceneUnitDisable);
            }
            ownerSceneUnit.AddModelLoadedListener(OnSceneUnitModelLoaded);
            OnComponentInit();
        }

        public void OnUnitDestroy()
        {
            OwnSceneUnit?.RemoveEnableListener(OnSceneUnitEnable);
            OwnSceneUnit?.RemoveDisableListener(OnSceneUnitDisable);
        }

        protected virtual void OnSceneUnitDisable()
        {
        }
        protected virtual void OnComponentInit()
        {
        }
        protected virtual void OnSceneUnitEnable()
        {
        }

        protected virtual void OnSceneUnitModelLoaded()
        {
        }
    }
}
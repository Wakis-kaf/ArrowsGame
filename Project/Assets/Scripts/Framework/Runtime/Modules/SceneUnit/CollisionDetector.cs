using UnityEngine;

namespace Framework.Runtime.MSceneUnit
{
    public class CollisionDetector : MonoBehaviour
    {
        private SceneUnitEvent m_SceneUnitEvtCache;
        public SceneUnit OwnSceneUnit { get;private set; }
        public void BindOwnSceneUnit(SceneUnit ownSceneUnit)
        {
            OwnSceneUnit = ownSceneUnit;
            m_SceneUnitEvtCache = OwnSceneUnit.CreateSceneUnitEvent(SceneUnitEventCode.Code_OnSceneUnitTriggerEnter, null);
        }

        public void OnTriggerEnter(UnityEngine.Collider other)
        {
            if (OwnSceneUnit == null) return;
            if (other.TryGetComponent<SceneUnitGetter>(out SceneUnitGetter sceneUnitGetter) &&
                OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnSceneUnitTriggerEnter))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnSceneUnitTriggerEnter;
                m_SceneUnitEvtCache.arg = sceneUnitGetter.OwnerSceneUnit;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
            else if (OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnTriggerEnter))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnTriggerEnter;
                m_SceneUnitEvtCache.arg = other;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
        }

        public void OnTriggerExit(UnityEngine.Collider other)
        {
            if (OwnSceneUnit == null) return;
            if (other.TryGetComponent<SceneUnitGetter>(out SceneUnitGetter sceneUnitGetter) &&
                OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnSceneUnitTriggerExit))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnSceneUnitTriggerExit;
                m_SceneUnitEvtCache.arg = sceneUnitGetter.OwnerSceneUnit;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
            else if (OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnTriggerExit))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnTriggerExit;
                m_SceneUnitEvtCache.arg = other;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
        }

        private void OnCollisionEnter(UnityEngine.Collision collision)
        {
            if (OwnSceneUnit == null) return;
            if (collision.gameObject.TryGetComponent<SceneUnitGetter>(out SceneUnitGetter sceneUnitGetter) &&
                OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnSceneUnitCollisionEnter))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnSceneUnitCollisionEnter;
                m_SceneUnitEvtCache.arg = sceneUnitGetter.OwnerSceneUnit;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
            else if (OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnCollisionEnter))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnCollisionEnter;
                m_SceneUnitEvtCache.arg = collision;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
        }

        private void OnCollisionExit(UnityEngine.Collision collision)
        {
            if (OwnSceneUnit == null) return;
            if (collision.gameObject.TryGetComponent<SceneUnitGetter>(out SceneUnitGetter sceneUnitGetter) &&
                OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnSceneUnitCollisionExit))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnSceneUnitCollisionExit;
                m_SceneUnitEvtCache.arg = sceneUnitGetter.OwnerSceneUnit;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
            else if (OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnCollisionExit))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnCollisionExit;
                m_SceneUnitEvtCache.arg = collision;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
        }

        private void OnCollisionStay(UnityEngine.Collision collision)
        {
            if (OwnSceneUnit == null) return;
            if (collision.gameObject.TryGetComponent<SceneUnitGetter>(out SceneUnitGetter sceneUnitGetter) &&
                OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnSceneUnitCollisionStay))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnSceneUnitCollisionStay;
                m_SceneUnitEvtCache.arg = sceneUnitGetter.OwnerSceneUnit;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
            else if (OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnCollisionStay))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnCollisionStay;
                m_SceneUnitEvtCache.arg = collision;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
        }

        private void OnTriggerStay(UnityEngine.Collider other)
        {
            if (OwnSceneUnit == null) return;
            if (other.TryGetComponent<SceneUnitGetter>(out SceneUnitGetter sceneUnitGetter) &&
                OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnSceneUnitTriggerStay))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnSceneUnitTriggerStay;
                m_SceneUnitEvtCache.arg = sceneUnitGetter.OwnerSceneUnit;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
            else if (OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnTriggerStay))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnTriggerStay;
                m_SceneUnitEvtCache.arg = other;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
        }

        public void OnTriggerEnter2D(UnityEngine.Collider2D other)
        {
            if (OwnSceneUnit == null) return;
            if (other.TryGetComponent<SceneUnitGetter>(out SceneUnitGetter sceneUnitGetter) &&
                OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnSceneUnitTriggerEnter2D))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnSceneUnitTriggerEnter2D;
                m_SceneUnitEvtCache.arg = sceneUnitGetter.OwnerSceneUnit;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
            else if (OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnTriggerEnter2D))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnTriggerEnter2D;
                m_SceneUnitEvtCache.arg = other;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
        }

        public void OnTriggerExit2D(UnityEngine.Collider2D other)
        {
            if (OwnSceneUnit == null) return;
            if (other.TryGetComponent<SceneUnitGetter>(out SceneUnitGetter sceneUnitGetter) &&
                OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnSceneUnitTriggerExit2D))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnSceneUnitTriggerExit2D;
                m_SceneUnitEvtCache.arg = sceneUnitGetter.OwnerSceneUnit;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
            else if (OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnTriggerExit2D))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnTriggerExit2D;
                m_SceneUnitEvtCache.arg = other;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
        }

        private void OnCollisionEnter2D(UnityEngine.Collision2D collision)
        {
            if (OwnSceneUnit == null) return;
            if (collision.gameObject.TryGetComponent<SceneUnitGetter>(out SceneUnitGetter sceneUnitGetter) &&
                OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnSceneUnitCollisionEnter2D))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnSceneUnitCollisionEnter2D;
                m_SceneUnitEvtCache.arg = sceneUnitGetter.OwnerSceneUnit;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
            else if (OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnCollisionEnter2D))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnCollisionEnter2D;
                m_SceneUnitEvtCache.arg = collision;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
        }

        private void OnCollisionExit2D(UnityEngine.Collision2D collision)
        {
            if (OwnSceneUnit == null) return;
            if (collision.gameObject.TryGetComponent<SceneUnitGetter>(out SceneUnitGetter sceneUnitGetter) &&
                OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnSceneUnitCollisionExit2D))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnSceneUnitCollisionExit2D;
                m_SceneUnitEvtCache.arg = sceneUnitGetter.OwnerSceneUnit;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
            else if (OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnCollisionExit2D))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnCollisionExit2D;
                m_SceneUnitEvtCache.arg = collision;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
        }

        private void OnCollisionStay2D(UnityEngine.Collision2D collision)
        {
            if (OwnSceneUnit == null) return;
            if (collision.gameObject.TryGetComponent<SceneUnitGetter>(out SceneUnitGetter sceneUnitGetter) &&
                OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnSceneUnitCollisionStay2D))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnSceneUnitCollisionStay2D;
                m_SceneUnitEvtCache.arg = sceneUnitGetter.OwnerSceneUnit;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
            else if (OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnCollisionStay2D))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnCollisionStay2D;
                m_SceneUnitEvtCache.arg = collision;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
        }

        private void OnTriggerStay2D(UnityEngine.Collider2D other)
        {
            if (OwnSceneUnit == null) return;
            if (other.TryGetComponent<SceneUnitGetter>(out SceneUnitGetter sceneUnitGetter) &&
                OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnSceneUnitTriggerStay2D))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnSceneUnitTriggerStay2D;
                m_SceneUnitEvtCache.arg = sceneUnitGetter.OwnerSceneUnit;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
            else if (OwnSceneUnit.HasRegisterSceneUnitEvent(SceneUnitEventCode.Code_OnTriggerStay2D))
            {
                m_SceneUnitEvtCache.eventName = SceneUnitEventCode.Code_OnTriggerStay2D;
                m_SceneUnitEvtCache.arg = other;
                OwnSceneUnit.DispatchSceneUnitEvent(m_SceneUnitEvtCache);
            }
        }
    }
}
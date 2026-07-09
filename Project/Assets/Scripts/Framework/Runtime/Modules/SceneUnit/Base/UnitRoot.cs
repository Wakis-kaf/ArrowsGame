using Framework.Runtime.UnitSystem.MonoBase;
using UnityEngine;

namespace Framework.Runtime.MSceneUnit
{
    public class UnitRoot : MonoBehaviourUnit
    {
        private GameObject m_Go;

        public GameObject GO
        {
            get
            {
                if (m_Go == null)
                    m_Go = gameObject;
                return m_Go;
            }
        }
    }
}
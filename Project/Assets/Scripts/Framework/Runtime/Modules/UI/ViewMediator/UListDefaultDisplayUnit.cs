using UnityEngine;

namespace Framework.Runtime.UI
{
    public class UListDefaultDisplayUnit : UListDisplayUnit
    {
        private UButton m_UButton;

        public UListDefaultDisplayUnit()
        {
        }

        public UListDefaultDisplayUnit(object data) : base(data)
        {
        }

        public UButton UButton
        {
            get
            {
                if (m_UButton != null)
                    return m_UButton;
                m_UButton = DisplayGO?.GetComponent<UButton>();
                return m_UButton;
            }
        }

        protected Vector2 GetRandomSize()
        {
            return new Vector2(Random.Range(30, 60), Random.Range(30, 60));
        }
    }
}
using System;

namespace Framework.Runtime.UI
{
    public static class DisplayUnitPoolAgent
    {
        private static Func<Type, IDisplayUnit> m_DisplayUnitCreateAgent;
        private static Action<Type, IDisplayUnit> m_PoolCacheAgent;

        public static void SaveToPool(Type displayObjType, IDisplayUnit displayUnit)
        {
            m_PoolCacheAgent?.Invoke(displayObjType, displayUnit);
        }

        public static bool TryGetFromPool(Type displayObjType, out IDisplayUnit displayObj)
        {
            displayObj = default;
            var obj = m_DisplayUnitCreateAgent?.Invoke(displayObjType);
            if (obj == null) return false;
            displayObj = obj;
            return true;
        }
    }
}
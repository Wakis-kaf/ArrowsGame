using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Framework.Runtime.MCombat
{
    public static class CombatDynamicNumeric
    {
        public delegate double DynamicNumericGetter(string numCode, IEnumerable<KV> kvs);
        private static Dictionary<string, DynamicNumericGetter> GetterMap;
        static CombatDynamicNumeric()
        {
            GetterMap = new Dictionary<string, DynamicNumericGetter>();
        }
        public static void SetGetter(string numCode, DynamicNumericGetter getter)
        {
            GetterMap[numCode] = getter;
        }
        public static double GetValue(string numCode,double defaultValue = default, IEnumerable<KV> kvs = null)
        {
            if(GetterMap.TryGetValue(numCode,out var getter))
            {
                return getter(numCode,kvs);
            }
            return defaultValue;
        }
    }
}

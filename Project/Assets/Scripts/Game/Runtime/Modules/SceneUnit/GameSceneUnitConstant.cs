using Framework.Runtime.MCombat;

using System;
using System.Collections.Generic;

namespace Game.Modules.GModuleSceneUnit
{
    public static class GameSceneUnitConstant
    {
        private static Dictionary<string, Type> m_SceneUnitTypeMap = new Dictionary<string, Type>
        {

        };

        public static Type FindSceneUnitType(string sceneItemClassType, Type defaultType = null)
        {
            if (m_SceneUnitTypeMap.ContainsKey(sceneItemClassType))
            {
                return m_SceneUnitTypeMap[sceneItemClassType];
            }
            return defaultType;
        }
    }
}
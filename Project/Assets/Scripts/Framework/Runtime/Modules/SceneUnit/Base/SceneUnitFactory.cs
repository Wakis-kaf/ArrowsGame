
using System;
using UnityEngine;

namespace Framework.Runtime.MSceneUnit
{
    public class SceneUnitFactory
    {
        public static T CreateSceneUnit<T>(string name, bool changeParent = true) where T :SceneUnit
        {
            return CreateSceneUnit(typeof(T), name, changeParent) as T;
        }
        public static SceneUnit CreateSceneUnit(Type type,string name, bool changeParent = true)
        {
            GameObject unit = new GameObject(name);
            if (changeParent)
            {
                unit.transform.SetParent(GameApp.SceneUnitManager.SceneUnitRoot);
            }
            var sceneUnit = (SceneUnit)unit.AddComponent(type);
            return sceneUnit;
        }
        public static SceneUnit CreateSceneUnit(string name, bool changeParent = true)
        {
            GameObject unit = new GameObject(name);
            if (changeParent)
            {
                unit.transform.SetParent(GameApp.SceneUnitManager.SceneUnitRoot);
            }
            var sceneUnit = unit.AddComponent<SceneUnit>();
            return sceneUnit;
        }
    }
}
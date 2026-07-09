using Framework.Runtime;
using Framework.Runtime.MAsset;
using Framework.Runtime.MGameModule;
using Framework.Runtime.MSceneUnit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleStage
{
    public class GameStageClientHandler : GameModuleLogicHandler
    {

        public static GameStageClientHandler Ins => GetModuleHandlerIns<GameStageClientHandler>();
        private Dictionary<Type, GameStage> m_Type2Stage;
        private GameObject m_StagetRoot;
        public GameObject StageRoot
        {
            get
            {
                if(m_StagetRoot == null)
                {
                    m_StagetRoot = new GameObject("StageRoot");
                    m_StagetRoot.transform.SetParent(GameApp.Ins.GameAppShell.transform);
                }
                return m_StagetRoot;
            }
        }
        protected override void OnHandlerAwake()
        {
            m_Type2Stage = new Dictionary<Type, GameStage>();
        }
        protected override void OnHandlerEnable()
        {
            
        }
        protected override void OnHandlerStart()
        {
            
        }
        protected override void OnHandlerDestroy()
        {
            
        }
        public T FindStage<T>() where T : GameStage
        {
            Type type = typeof(T);
            if (m_Type2Stage.TryGetValue(type, out var stage))
            {
                return stage as T;
            }
            return null;
        }
        public void HideStage<T>() where T : GameStage
        {
            var stage = FindStage<T>();
            stage?.SetActive(false);
            //stage?.OnStageHide();
        }
        public void ShowStage<T>() where T : GameStage
        {
            var stage = FindStage<T>();
            stage?.SetActive(true);
            //stage?.OnStageShow();
            
        }
        public bool IsStageLoaded<T>() where T : GameStage
        {
            var stage = FindStage<T>();
            if(stage == null) return false;
            return stage.IsLoaded();
        }
        public T TryLoadStage<T>(string stageDir, string stageName, Action<T> onStageLoaded = null) where T : GameStage 
        {
            Type type = typeof(T);
            if (m_Type2Stage.TryGetValue(type, out var stage))
            {
                stage.AddModelLoadedListener(() =>
                {
                    onStageLoaded?.Invoke(stage as T);
                });
                return stage as T;
            }
            stage = LoadStage<T>(stageDir, stageName);
            m_Type2Stage.Add(type, stage);
            stage.SetActive(false);
            stage.AddModelLoadedListener(() =>
            {
                onStageLoaded?.Invoke(stage as T);
            });
            return stage as T;
        }
        public T OpenStage<T>(string stageDir,string stageName) where T : GameStage
        {
            Type type = typeof(T);
            if(m_Type2Stage.TryGetValue(type,out var stage))
            {
                stage.SetActive(true);
                return stage as T;
            }
            var sceneUnit = LoadStage<T>(stageDir, stageName);
            m_Type2Stage.Add(type, sceneUnit);
            sceneUnit.SetActive(true);
            return sceneUnit as T;
        }
        private T LoadStage<T>(string stageDir, string stageName) where T : GameStage
        {
            string path = $"Assets/AddressableResources/Stage/{stageDir}/Prefabs/{stageName}.prefab";
            string link = AssetPathEncoder.EncodeEnvAssetLink(path);
            var sceneUnit = SceneUnitFactory.CreateSceneUnit<T>(stageName, false);
            sceneUnit.SetRootParent(StageRoot.transform);
            sceneUnit.LoadModel(link);
            return sceneUnit;
        }
    }

}

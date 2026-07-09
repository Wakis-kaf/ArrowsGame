using Framework.Runtime;
using Framework.Runtime.LogSystem;
using Framework.Runtime.MAsset;
using Framework.Runtime.MObjectPool.Core;
using Framework.Runtime.MSceneUnit;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Modules.GModuleSceneUnit
{
    public struct SceneUnitGenerateData : IModifierData
    {
        public bool isEntity;
        public Transform parent;
        public Vector3 position;
        public GameObject prefab;
        public Quaternion rotation;
        public int sceneItemId;
        public Type sceneUnitType;

        public IAssetVO scenUnitModelAssetVO;
        public GameObject sceneUnitRootPrefab;
        public GameObject sceneUnitModelPrefab;
        public string scenUnitModelPath;
        public string tag;
        public int prewarmCount;
    }

    /// <summary>
    /// 管理和加载所需要用到的SceneUnits
    /// </summary>
    public class GameSceneUnitPool : ObjectPool<SceneUnit>
    {
        public const int load_ret_all_loaded = 1;

        private Dictionary<int, IAssetVO> m_Id2AssetVOMap;
        private Dictionary<int, GameObject> m_Id2SceneUnitRootPrefab;
        private Dictionary<int, GameObject> m_Id2SceneUnitModelPrefab;

        private SceneUnitGenerateData m_SceneUnitData;

        private LinkedList<SceneUnitLoadTask> m_Tasks;

        public GameSceneUnitPool()
        {
            m_SceneUnitData = new SceneUnitGenerateData();
            m_Id2AssetVOMap = new Dictionary<int, IAssetVO>();
            m_Id2SceneUnitRootPrefab = new Dictionary<int, GameObject>();
            m_Id2SceneUnitModelPrefab = new Dictionary<int, GameObject>();
            m_Tasks = new LinkedList<SceneUnitLoadTask>();
            CreateAndAddObjectModifier<GameSceneUnitPoolModifier>("GameSceneUnitPoolModifier");
        }

        public delegate void SceneUnitLoadCallbak(SceneUnitLoadTask task);

        public void BindSceneUnitRootPrefab(int sceneItemId, GameObject prefab)
        {
            m_Id2SceneUnitRootPrefab[sceneItemId] = prefab;
        }
        public void BindSceneUnitModelPrefab(int sceneItemId, GameObject prefab)
        {
            m_Id2SceneUnitModelPrefab[sceneItemId] = prefab;
        }
        public GameObject FindSceneUnitRootPrefab(int sceneItemId)
        {
            return m_Id2SceneUnitRootPrefab.ContainsKey(sceneItemId) ? m_Id2SceneUnitRootPrefab[sceneItemId] : null;
        }
        public GameObject FindSceneUnitModelPrefab(int sceneItemId)
        {
            return m_Id2SceneUnitModelPrefab.ContainsKey(sceneItemId) ? m_Id2SceneUnitModelPrefab[sceneItemId] : null;
        }

        public SceneUnitLoadTask CheckSceneUnitLoad(List<int> sceneUnitIds, SceneUnitLoadCallbak loadCb)
        {
            SceneUnitLoadTask loadTask = new SceneUnitLoadTask();
            loadTask.loadSceneUnitIds = sceneUnitIds;
            loadTask.loadCb = loadCb;
            m_Tasks.AddLast(loadTask);
            if (sceneUnitIds.Count <= 0)
            {
                loadTask.isAllLoaded = true;
                return loadTask;
            }
            for (int i = 0; i < sceneUnitIds.Count; i++)
            {
                var id = sceneUnitIds[i];
                IAssetVO assetVO = FindOrGetAssetVO(id);
                if (assetVO != null)
                {
                    assetVO.AddAssetLoadCallback((assetVo) =>
                    {
                        RecordAssetVO(id, assetVO);
                        OnAssetLoaded(loadTask, assetVo);
                    });
                }
                RecordAssetVO(id, assetVO);
            }
            return loadTask;
        }
        public TSceneUnit GetSceneUnit<TSceneUnit>(int sceneItemId, bool isEntity = false) where TSceneUnit : SceneUnit
        {
            return GetSceneUnit(typeof(TSceneUnit), sceneItemId, isEntity) as TSceneUnit;
        }
        public SceneUnit GetSceneUnit(int sceneItemId, bool isEntity = false)
        {
            var className = GetSceneUnitClassName(sceneItemId);
            var type = GameSceneUnitConstant.FindSceneUnitType(className);
            if (type == null)
            {
                Log.Error($"未在GameSceneUnitConstant 中定义名称为{className} 的SceneUnit类型");
                return null;
            }
            return GetSceneUnit(type, sceneItemId, isEntity);
        }

        public SceneUnit GetSceneUnit(Type type, int sceneItemId, bool isEntity = false)
        {
            string tag = $"{type.Name}_{sceneItemId}";
            m_SceneUnitData.tag = tag;
            m_SceneUnitData.sceneItemId = sceneItemId;


            m_SceneUnitData.sceneUnitRootPrefab = FindSceneUnitRootPrefab(sceneItemId);
            m_SceneUnitData.sceneUnitModelPrefab = FindSceneUnitModelPrefab(sceneItemId);
            bool loadAsset = (m_SceneUnitData.sceneUnitRootPrefab == null && m_SceneUnitData.sceneUnitModelPrefab == null);
            m_SceneUnitData.scenUnitModelPath = loadAsset ? GetSceneUnitModelPath(sceneItemId) : null;
            m_SceneUnitData.scenUnitModelAssetVO = loadAsset ? FindOrGetAssetVO(sceneItemId) : null;

            m_SceneUnitData.isEntity = isEntity;
            m_SceneUnitData.sceneUnitType = type;
            SceneUnit sceneUnit = GetOrCreateItem(tag, m_SceneUnitData, m_SceneUnitData);
            sceneUnit.Id = sceneItemId;
            return sceneUnit;
        }



        public void PrewarmSceneUnit<TSceneUnit>(int sceneItemId, int count, bool isEntity = false) where TSceneUnit : SceneUnit
        {
            string tag = $"{typeof(TSceneUnit).Name}_{sceneItemId}";
            m_SceneUnitData.tag = tag;
            m_SceneUnitData.prewarmCount = count;
            m_SceneUnitData.sceneItemId = sceneItemId;
            m_SceneUnitData.sceneUnitRootPrefab = FindSceneUnitRootPrefab(sceneItemId);
            m_SceneUnitData.sceneUnitModelPrefab = FindSceneUnitModelPrefab(sceneItemId);
            bool loadAsset = (m_SceneUnitData.sceneUnitRootPrefab == null && m_SceneUnitData.sceneUnitModelPrefab == null);
            m_SceneUnitData.scenUnitModelPath = loadAsset ? GetSceneUnitModelPath(sceneItemId) : null;
            m_SceneUnitData.scenUnitModelAssetVO = loadAsset ? FindOrGetAssetVO(sceneItemId) : null;
            m_SceneUnitData.sceneUnitType = typeof(TSceneUnit);
            m_SceneUnitData.isEntity = isEntity;
            PrewarmItem(tag,
                count,
                m_SceneUnitData,
                m_SceneUnitData,
                m_SceneUnitData,
                m_SceneUnitData);
        }

        public void PutSceneUnit<TSceneUnit>(int sceneItemId, TSceneUnit sceneUnit) where TSceneUnit : SceneUnit
        {
            if (sceneUnit == null) return;
            if (sceneUnit.IsInPool) return;
            var typeName = sceneUnit.Type.Name;
            string tag = $"{typeName}_{sceneItemId}";
            m_SceneUnitData.tag = tag;
            m_SceneUnitData.sceneItemId = sceneItemId;
            m_SceneUnitData.sceneUnitRootPrefab = FindSceneUnitRootPrefab(sceneItemId);
            m_SceneUnitData.sceneUnitModelPrefab = FindSceneUnitModelPrefab(sceneItemId);
            bool loadAsset = (m_SceneUnitData.sceneUnitRootPrefab == null && m_SceneUnitData.sceneUnitModelPrefab == null);
            m_SceneUnitData.scenUnitModelPath = loadAsset ? GetSceneUnitModelPath(sceneItemId) : null;
            m_SceneUnitData.scenUnitModelAssetVO = loadAsset ? FindOrGetAssetVO(sceneItemId) : null;
            m_SceneUnitData.sceneUnitType = sceneUnit.Type;
            PutOrDestroyItem(tag, sceneUnit, m_SceneUnitData);
        }

        private IAssetVO FindAssetVO(int id)
        {
            if (m_Id2AssetVOMap.TryGetValue(id, out var assetVO))
            {
                return assetVO;
            }
            return null;
        }

        private IAssetVO FindOrGetAssetVO(int sceneUnitId)
        {
            IAssetVO assetVO = FindAssetVO(sceneUnitId);
            if (assetVO == null)
            {
                // 获取场景物体配置
                string assetLink = AssetPathEncoder.EncodeEnvAssetLink(GetSceneUnitModelPath(sceneUnitId));
                assetVO = GameApp.AssetManager.LoadAssetAsync(assetLink, null);
                RecordAssetVO(sceneUnitId, assetVO);
            }
            return assetVO;
        }

        private string GetSceneUnitClassName(int sceneUnitId)
        {
            var sceneUnitCfg = GameModuleHandler.GetModuleHandlerIns<GameSceneUnitDataHandler>().
              GetCfgSceneItemInfo(sceneUnitId);
            if (sceneUnitCfg == null)
            {
                Log.Error($"未找到id为{sceneUnitId}的场景物体");
                return "";
            }
            if (string.IsNullOrEmpty(sceneUnitCfg.clsType))
            {
                Log.Error($"id为{sceneUnitId}的场景物体 类型定义为空字符串 sceneItemClassType");
            }
            return sceneUnitCfg.clsType;
        }

        private string GetSceneUnitModelPath(int sceneUnitId)
        {
            var sceneUnitCfg = GameModuleHandler.GetModuleHandlerIns<GameSceneUnitDataHandler>().
                GetCfgSceneItemInfo(sceneUnitId);
            if (sceneUnitCfg == null)
            {
                Log.Error($"未找到id为{sceneUnitId}的场景物体");
                return "";
            }
            if (string.IsNullOrEmpty(sceneUnitCfg.pbPath))
            {
                Log.Error($"id为{sceneUnitId}的场景物体 预制体路径为空prefabPath");
            }
            return sceneUnitCfg.pbPath;
        }

        private bool IsAssetLoaded(int assetId)
        {
            IAssetVO assetVO = FindAssetVO(assetId);
            if (assetVO == null) return false;
            return assetVO.IsLoaded;
        }

        private bool IsAssetLoadSuccess(int assetId)
        {
            IAssetVO assetVO = FindAssetVO(assetId);
            if (assetVO == null) return false;
            return assetVO.IsLoadSuccess;
        }

        private void OnAssetLoaded(SceneUnitLoadTask task, IAssetVO assetVO)
        {
            bool isAllLoadSuc = true;
            bool isAllLoaded = true;
            for (int i = 0; i < task.loadSceneUnitIds.Count; i++)
            {
                int loadUnitId = task.loadSceneUnitIds[i];
                if (!IsAssetLoadSuccess(loadUnitId))
                {
                    isAllLoadSuc = false;
                }
                if (!IsAssetLoaded(loadUnitId))
                {
                    isAllLoaded = false;
                }
            }
            if (isAllLoaded)
            {
                task.isAllLoaded = isAllLoaded;
                task.isAllLoadSuccess = isAllLoadSuc;
                task.loadCb?.Invoke(task);
                m_Tasks.Remove(task);
            }
        }

        private void RecordAssetVO(int id, IAssetVO assetVO)
        {
            if (!m_Id2AssetVOMap.ContainsKey(id))
            {
                m_Id2AssetVOMap.Add(id, assetVO);
            }
            else
            {
                m_Id2AssetVOMap[id] = assetVO;
            }
        }

        public class SceneUnitLoadTask
        {
            public bool isAllLoaded;
            public bool isAllLoadSuccess;
            public SceneUnitLoadCallbak loadCb;
            public List<int> loadSceneUnitIds;
        }
    }
}
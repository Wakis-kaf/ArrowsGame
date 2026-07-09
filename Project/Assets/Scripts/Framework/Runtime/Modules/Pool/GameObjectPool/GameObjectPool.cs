using Framework.Library;
using Framework.Runtime.MObjectPool.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.MObjectPool.GOPool
{
    public class GameObjectGenerateData : IModifierData
    {
        public Transform parent;
        public Vector3 position;
        public GameObject prefab;
        public Quaternion rotation;
    }

    public class GameObjectPool : ObjectPool<GameObject>
    {
        private GameObjectGenerateData _gameObjectGenerateData = new GameObjectGenerateData();

        private Dictionary<int[], PutObjectData> _taskId2PutObjectDatas =
            new Dictionary<int[], PutObjectData>();

        private LightCoroutine<PutObjectData> m_LightCoroutine;
        private Action<PutObjectData> m_OnLightCoroutineDestroyCb;
        private Transform m_Root;

        public GameObjectPool(Transform root, string name = "GameObjectPool")
        {
            poolName = name;
            m_Root = root;
            // 添加修改器
            CreateAndAddObjectModifier<GameObjectModifier>("GameObjectModifier");
            CreateAndAddObjectModifier<RigidbodyModifier>("RigidbodyModifier");
            CreateAndAddObjectModifier<TrailRenderModifier>("TrailRenderModifier");
            m_OnLightCoroutineDestroyCb = OnLightCoroutineDestroy;
        }

        public Transform poolModelObjectTransform { get; private set; }

        public GameObject GetObject(string tag, GameObject prefab, Vector3 position = default,
            Quaternion rotation = default, Transform parent = null)
        {
            _gameObjectGenerateData.prefab = prefab;
            _gameObjectGenerateData.position = position;
            _gameObjectGenerateData.rotation = rotation;
            _gameObjectGenerateData.parent = parent ?? poolModelObjectTransform;

            return GetOrCreateItem(tag, _gameObjectGenerateData, _gameObjectGenerateData);
        }

        public GameObject GetObject(string tag, Transform parent)
        {
            return GetObject(tag, default, default, parent);
        }

        public GameObject GetObject(string tag, Vector3 position = default,
            Quaternion rotation = default, Transform parent = null, bool generateInRoot = false)
        {
            _gameObjectGenerateData.position = position;
            _gameObjectGenerateData.rotation = rotation;

            _gameObjectGenerateData.parent = generateInRoot ? poolModelObjectTransform : parent;
            return GetOrCreateItem(tag, _gameObjectGenerateData, _gameObjectGenerateData);
        }

        public void Init()
        {
            poolModelObjectTransform = GeneratePoolRoot();
            poolModelObjectTransform.SetParent(m_Root);
            m_LightCoroutine = new LightCoroutine<PutObjectData>(poolModelObjectTransform.gameObject);
        }

        // 预热对象池
        public void PrewarmObject(string tag, GameObject prefab, int count,
            IModifierData createData = default,
            IModifierData prewarmData = default,
            IModifierData putData = default,
            IModifierData destroyData = default)
        {
            if (ReferenceEquals(createData, default))
            {
                createData = new GameObjectGenerateData()
                {
                    prefab = prefab,
                    parent = poolModelObjectTransform
                };
            }

            PrewarmItem(tag, count, createData, prewarmData, putData, destroyData);
        }

        public GameObject PutObject(string tag, GameObject gameObject, float destroyTime = 0f)
        {
            if (Math.Abs(destroyTime) < 1e-3)
            {
                _gameObjectGenerateData.parent = poolModelObjectTransform;
                return PutOrDestroyItem(tag, gameObject, _gameObjectGenerateData);
            }
            else
            {
                PutObjectData data =
                    new PutObjectData(tag, gameObject, Time.time, destroyTime);
                // 使用协程销毁
                m_LightCoroutine.StartCoroutineTasks("DestroyGameObject", data, m_OnLightCoroutineDestroyCb);
                return null;
            }
        }

        private Transform GeneratePoolRoot()
        {
            return new GameObject($"[ ObjectPool ] : {poolName}").transform;
        }

        private void OnLightCoroutineDestroy(PutObjectData data)
        {
            _gameObjectGenerateData.parent = poolModelObjectTransform;
            PutOrDestroyItem(data.tag, data.gameObject, _gameObjectGenerateData);
        }

        public readonly struct PutObjectData : ILightCoroutineTaskData
        {
            public readonly GameObject gameObject;
            public readonly string tag;

            public PutObjectData(string tag, GameObject gameObject, float createTime, float waitSeconds)
            {
                this.tag = tag;
                this.gameObject = gameObject;
                this.createTime = createTime;
                this.waitSeconds = waitSeconds;
            }

            public readonly float createTime { get; }

            public readonly float waitSeconds { get; }
        }
    }
}
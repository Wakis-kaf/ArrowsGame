using Framework.Runtime.LogSystem;
using Framework.Runtime.MAsset;
using Framework.Runtime.MObjectPool.Core;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.UnitSystem.BIInterfaces;
using Framework.Runtime.UnitSystem.MonoBase;
using Framework.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Framework.Runtime.MSceneUnit
{
    public interface ISceneUnitGetter
    {
        public SceneUnit OwnerSceneUnit { get; set; }
    }

    public class SceneUnit : MonoBehaviourUnit, 
        ISceneUnitGetter, 
        IUnitAwake, 
        IUnitEnable, 
        IUnitDisable, 
        IUnitUpdate,
        IPoolElement
    {
        private Transform m_CameraLookAt;

        private Dictionary<Type, ISceneUnitComponent> m_Components = new Dictionary<Type, ISceneUnitComponent>();

        private object m_Data;

        private int m_Id;

        private bool m_IsDirtyData = true;

        private bool m_IsPlayer;

        private bool m_IsSceneUnitEnable;

        private IAssetVO m_ModelLoadAssetVo;

        private Action m_ModelLoadedCb;

        private string m_Oid = Utility.IDGenerator.GetStrGuidID();

        private Action m_OnSceneUnitDisable;

        private Action m_OnSceneUnitEnable;
        [SerializeField]

        private GameObject m_UnitModelGO;

        private UnitRoot m_UnitRoot;
        private PrefabBinder m_PrefabBinder;
        public PrefabBinder EntityPrefabBinder { get => m_PrefabBinder; }
        public object Data => m_Data;
        public Transform CameraLookAt
        {
            get
            {
                if (m_CameraLookAt == null)
                {
                    m_CameraLookAt = new GameObject("CameraLookAt").transform;
                    m_CameraLookAt.SetParent(UnitRoot.transform);
                    m_CameraLookAt.localPosition = Vector3.up * 2f;
                }

                return m_CameraLookAt;
            }
            set { m_CameraLookAt = value; }
        }

        public virtual int Id
        {
            get => m_Id; set
            {
                m_Id = value;
                OnIdSet(value);
            }
        }

        public bool IsPlayer
        {
            get => m_IsPlayer;
            set
            {
                m_IsPlayer = value;
                if (value)
                {
                    UnitRoot.gameObject.tag = "Player";
                }
            }
        }

        public virtual string Oid { get => m_Oid; }

        public SceneUnit OwnerSceneUnit
        { get => this; set { } }
        private Transform m_RootTransform;
        public Transform RootTransform
        {
            get
            {
                if(m_RootTransform == null)
                {
                    m_RootTransform = transform;
                    BindRootMonoEvents(m_RootTransform);
                }
                return m_RootTransform;
            }
            set
            {
                m_RootTransform = value;
            }
        }
        private void BindRootMonoEvents(Transform transform)
        {
            MonoEvents monoEvents = transform.gameObject.GetOrAddComponent<MonoEvents>();
            monoEvents.SetOnShow(OnRootShow);
            monoEvents.SetOnHide(OnRootHide);
        }
        private void BindEntityRootMonoEvents(Transform transform)
        {
            MonoEvents monoEvents = transform.gameObject.GetOrAddComponent<MonoEvents>();
            monoEvents.SetOnShow(OnEntityRootShow);
            monoEvents.SetOnHide(OnEntityRootHide);
        }
        public GameObject UnitModelGo => m_UnitModelGO;

        public UnitRoot UnitRoot
        {
            get
            {
                if (m_UnitRoot == null)
                {
                    m_UnitRoot = RootTransform.gameObject.AddComponent<UnitRoot>();
                }

                return m_UnitRoot;
            }
        }
        // [SerializeField]
        private Transform m_EntityRoot;
        public Transform EntityRoot
        {
            get
            {
                if(m_EntityRoot == null)
                {
                    m_EntityRoot = new GameObject("EntityRoot").transform;
                    m_EntityRoot.transform.SetParent(UnitRoot.transform);
                    m_EntityRoot.localPosition = Vector3.zero;
                    m_EntityRoot.localRotation = Quaternion.identity;
                    BindEntityRootMonoEvents(m_EntityRoot);
                }
                return m_EntityRoot;
            }
        }

        public bool IsInPool { get; set; }
        public Pool Pool { get; set; }


        protected virtual void OnRootShow()
        {

        }
        protected virtual void OnRootHide()
        {

        }
        protected virtual void OnEntityRootShow()
        {

        }
        protected virtual void OnEntityRootHide()
        {

        }
        protected virtual void OnIdSet(int id)
        {

        }
     
        public void AddDisableListener(Action listener)
        {
            m_OnSceneUnitDisable += listener;
        }

        public void AddEnableListener(Action listener)
        {
            m_OnSceneUnitEnable += listener;
        }

        public void AddModelLoadedListener(Action sceneUnitModelLoadedCb)
        {
            if (IsModelLoaded())
            {
                sceneUnitModelLoadedCb?.Invoke();
                return;
            }
            m_ModelLoadedCb -= sceneUnitModelLoadedCb;
            m_ModelLoadedCb += sceneUnitModelLoadedCb;
        }

        public ISceneUnitComponent AddSceneUnitComponent(string typeName)
        {
            Type type = Type.GetType(typeName);
            return GetOrAddSceneUnitComponent(type);
        }
        public T GetOrAddSceneUnitComponent<T>() where T : class ,ISceneUnitComponent
        {
            return GetOrAddSceneUnitComponent(typeof(T)) as T;
        }
        public ISceneUnitComponent GetOrAddSceneUnitComponent(Type type)
        {
            var cmp = FindSceneUnitComponent(type);
            if (cmp != null) return cmp;
            if (type.IsSubclassOf(typeof(UnityEngine.Component)))
            {
                cmp = UnitRoot.GO.GetOrAddComponent(type) as ISceneUnitComponent;
            }
            else
            {
                cmp = System.Activator.CreateInstance(Utility.AssemblyUtil.GetType(type)) as ISceneUnitComponent;
            }
            RegisterSceneUnitComponent(cmp);
            return cmp;
        }

        public void AddSceneUnitEvent(string eventName, Action<SceneUnitEvent> listener)
        {
           GameApp.SceneUnitManager.SceneUnitEventDispatcher.AddEventListener(this, eventName, listener);
        }

        public void BindModel(GameObject gameObject)
        {
            bool isValueChanged = m_UnitModelGO != gameObject;
            m_UnitModelGO = gameObject;
            if (m_UnitModelGO != null && isValueChanged)
            {
               
                if (m_UnitModelGO.transform != EntityRoot)
                {
                    m_UnitModelGO.transform.SetParent(EntityRoot);
                    m_UnitModelGO.transform.localPosition = Vector3.zero;
                    m_UnitModelGO.transform.localRotation = Quaternion.identity;
                    //GameObjectUtil.SetLayer(m_UnitModelGO, RootTransform.gameObject.layer);
                }
                m_PrefabBinder = EntityRoot.GetComponentInChildren<PrefabBinder>();
                this.RegisterSceneUnitComponents();
                this.RegisterColliders();
            }
            OnModelLoaded(m_UnitModelGO);

            if (m_IsDirtyData)
            {
                m_IsDirtyData = false;
                UpdateGUI();
            }

            m_ModelLoadedCb?.Invoke();
            m_ModelLoadedCb = null;
        }
        public virtual SceneUnitEvent CreateSceneUnitEvent(string eventName, object obj)
        {
            return new SceneUnitEvent(this, eventName, obj);
        }
        //public virtual SceneUnitEvent CreateSceneUnitEvent(string eventName, object[] args)
        //{
        //    return new SceneUnitEvent(this, eventName, args);
        //}

        private void DisableSceneUnit()
        {
            m_IsSceneUnitEnable = false;
            m_OnSceneUnitDisable?.Invoke();
        }
        public void DispatchSceneUnitEvent(string eventName,  object obj)
        {
            GameApp.SceneUnitManager.SceneUnitEventDispatcher.DispatchEvent(this, CreateSceneUnitEvent(eventName, obj));
        }
        public  void DispatchSceneUnitEvent(SceneUnitEvent evt)
        {
            GameApp.SceneUnitManager.SceneUnitEventDispatcher.DispatchEvent(this, evt);
        }
        public void DispatchSceneUnitEvent(string eventName, params object[] args)
        {
            GameApp.SceneUnitManager.SceneUnitEventDispatcher.DispatchEvent(this, CreateSceneUnitEvent(eventName, args));
        }

        private void EnableSceneUnit()
        {
            m_IsSceneUnitEnable = true;
            m_OnSceneUnitEnable?.Invoke();
        }

        public ISceneUnitComponent FindSceneUnitComponent(string typeName)
        {
            Type type = Type.GetType(typeName);
            return FindSceneUnitComponent(type);
        }
        public T FindSceneUnitComponent<T>() where T : class, ISceneUnitComponent
        {
            return FindSceneUnitComponent(typeof(T)) as T;
        }
        public ISceneUnitComponent FindSceneUnitComponent(Type type)
        {
            if (m_Components.TryGetValue(type, out var cmp)) return cmp;
            foreach (var kvp in m_Components)
            {
                if (kvp.Key.IsSubclassOf(type))
                {
                    m_Components.Add(type, kvp.Value);
                    return kvp.Value;
                }
            }
            return cmp;
        }

        public Vector3 GetEulerAngles()
        {
            return EntityRoot.transform.eulerAngles;
        }

        public Vector3 GetPosition()
        {
            return EntityRoot.transform.position;
        }
        public virtual Vector3 GetCenterPosition()
        {
            return GetPosition();
        }

        public ISceneUnitComponent GetSceneUnitComponent(string typeName)
        {
            Type type = Type.GetType(typeName);
            return GetSceneUnitComponent(type);
        }
        public T GetSceneUnitComponent<T>() where T :class , ISceneUnitComponent
        {
            return GetSceneUnitComponent(typeof(T)) as T;
        }

        public ISceneUnitComponent GetSceneUnitComponent(Type type)
        {
            return GetOrAddSceneUnitComponent(type);
        }

        public bool HasRegisterSceneUnitEvent(string eventName)
        {
            return GameApp.SceneUnitManager.SceneUnitEventDispatcher.HasSceneUnitRegister(this, eventName);
        }

        public bool IsLoaded()
        {
            return IsModelLoaded() && IsAllComponentLoaded();
        }
        private bool IsAllComponentLoaded()
        {
            foreach (var kvp in m_Components)
            {
                if (!kvp.Value.IsLoaded())
                {
                    return false;
                }
            }
            return true;
        }
        public bool IsModelLoaded()
        {
            return UnitModelGo != null;
        }
        public void LoadModel(IAssetVO assetVO)
        {
            if (m_AssetPath == assetVO.assetPath) return;
            if (IsModelLoaded())
            {
                ClearLoadedModelAsset();
            }
            m_ModelLoadAssetVo = assetVO;
            m_AssetPath = assetVO.assetPath;
            m_ModelLoadAssetVo.AddAssetLoadCallback(OnModelLoadComplete);

        }
        private void ClearLoadedModelAsset()
        {
            m_ModelLoadAssetVo?.UnLoadAsync();
            m_ModelLoadAssetVo = null;
            m_UnitModelGO?.transform?.SetParent(null);
            if (m_UnitModelGO != null)
            {
                Destroy(m_UnitModelGO);
            }
            m_AssetPath = "";
            m_UnitModelGO = null;
        }
        private string m_AssetPath;
        public void LoadModel(string modelLink)
        {
            if (m_AssetPath == modelLink) return;
            if (IsModelLoaded())
            {
                ClearLoadedModelAsset();
            }
            m_AssetPath = modelLink;
            m_ModelLoadAssetVo = GameApp.AssetManager.LoadAssetAsync(modelLink, OnModelLoadComplete);
        }

        public void OnUnitDisable()
        {
            DisableSceneUnit();
        }

        public void OnUnitEnable()
        {
            EnableSceneUnit();
        }

        public virtual void OnUnitUpdate()
        {
        }

        public void RemoveDisableListener(Action listener)
        {
            m_OnSceneUnitDisable -= listener;
        }

        public void RemoveEnableListener(Action listener)
        {
            m_OnSceneUnitEnable -= listener;
        }

        public void RemoveSceneUnitEevent(string eventName, Action<SceneUnitEvent> listener)
        {
            GameApp.SceneUnitManager.SceneUnitEventDispatcher.RemoveEventListener(this, eventName, listener);
        }

        public Vector2 WorldPosToScreenPos(Camera camera, float offsetX = 0, float offsetY = 0, float offsetZ = 0)
        {
            Vector3 pos = EntityRoot.transform.position;
            pos.x += offsetX;
            pos.y += offsetY;
            pos.z += offsetZ;
            return camera.WorldToScreenPoint(pos);
        }

        public void SetActive(bool avtive)
        {
            GameObjectUtil.SetActive(RootTransform, avtive);
        }
        public void SetModelActive(bool avtive)
        {
            GameObjectUtil.SetActive(EntityRoot, avtive);
        }
        public void UpdateGUI()
        {
            this.SetData(this.Data);
        }
        public void SetData(object data = null)
        {
            m_Data = data;
            if (UnitModelGo != null)
            {
                OnSceneUnitGUI(m_Data);
            }
            else
            {
                m_IsDirtyData = true;
            }
        }

        public virtual void SetEulerAngles(Vector3 eulerAngles)
        {
            EntityRoot.transform.eulerAngles = eulerAngles;
        }

        public virtual void SetLocalEulerAngles(Vector3 eulerAngles)
        {
            EntityRoot.transform.localEulerAngles = eulerAngles;
        }

        public void SetLocalPosition(Vector3 position)
        {
            EntityRoot.transform.localPosition = position;
        }

        public void SetModelLoadedListener(Action sceneUnitModelLoadedCb)
        {
            if (IsModelLoaded())
            {
                sceneUnitModelLoadedCb?.Invoke();
                return;
            }
            m_ModelLoadedCb = sceneUnitModelLoadedCb;
        }

        public virtual void SetPosition(Vector3 position)
        {
            EntityRoot.transform.position = position;
        }

        public void SetRootParent(Transform parent = null)
        {
            RootTransform.SetParent(parent);
        }

        public virtual void SetScale(Vector3 scale)
        {
            EntityRoot.transform.localScale = scale;
        }

        public virtual void SetScale(float x, float y, float z)
        {
            EntityRoot.transform.localScale = new Vector3(x, y, z);
        }

        protected override void Awake()
        {
            base.Awake();
            if (this.m_UnitModelGO != null)
            {
                var go = this.m_UnitModelGO;
                this.m_UnitModelGO = null;
                this.BindModel(go);
            }
            this.RegisterSceneUnitComponents();
        }

        protected override void DisposeManagedResources()
        {
        }

        protected virtual void OnModelLoaded(GameObject modelGamObject)
        {
        }

        protected virtual void OnSceneUnitGUI(object data)
        {
        }
      
        private void OnModelLoadComplete(IAssetVO assetVo)
        {
            m_ModelLoadAssetVo = assetVo;
            if(assetVo == null)
            {
                Log.Error("模型加载错误!");
                return;
            }
            BindModel(assetVo.GetInstance(EntityRoot.transform));
        }

        private void RegisterColliders()
        {
            var colliders = m_UnitModelGO.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject.TryGetComponent<ISceneUnitGetter>(out ISceneUnitGetter sceneUnitGetter))
                {
                    sceneUnitGetter.OwnerSceneUnit = this;
                }
                else
                {
                    colliders[i].gameObject.AddComponent<SceneUnitGetter>().OwnerSceneUnit = this;
                }
            }
            var collider2ds = m_UnitModelGO.GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < collider2ds.Length; i++)
            {
                if (collider2ds[i].gameObject.TryGetComponent<ISceneUnitGetter>(out ISceneUnitGetter sceneUnitGetter))
                {
                    sceneUnitGetter.OwnerSceneUnit = this;
                }
                else
                {
                    collider2ds[i].gameObject.AddComponent<SceneUnitGetter>().OwnerSceneUnit = this;
                }
            }
        }

        private void RegisterSceneUnitComponent(ISceneUnitComponent cmp)
        {
            RegisterSceneUnitComponent(cmp, cmp.GetType());
        }

        private void RegisterSceneUnitComponent(ISceneUnitComponent cmp, Type type)
        {
            if (m_Components.ContainsKey(type)) return;
            cmp.BindOwnerUnit(this);
            m_Components.Add(type, cmp);
        }

        private void RegisterSceneUnitComponents()
        {
            var sceneComponents = RootTransform.GetComponentsInChildren<ISceneUnitComponent>();
            for (int i = 0; i < sceneComponents.Length; i++)
            {
                RegisterSceneUnitComponent(sceneComponents[i]);
            }
        }

        public virtual void OnCreateInPool()
        {
            
        }

        public virtual void OnDestroyByPool()
        {
            
        }

        public virtual void OnGetFromPool()
        {
            EnableUnit();
        }

        public virtual void OnPrewarmInPool()
        {
            
        }

        public virtual void OnPutToPool()
        {
            DisableUnit();
        }

        public virtual void OnUnitAwake()
        {
            
        }

        public virtual int GetLayer()
        { 
            return LayerMask.NameToLayer("Default");
        }
    }

    public class SceneUnitGetter : MonoBehaviour, ISceneUnitGetter
    {
        public SceneUnit OwnerSceneUnit { get; set; }
    }
}
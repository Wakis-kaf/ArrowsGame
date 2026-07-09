using Framework.Runtime;
using Framework.Runtime.MObjectPool.Core;
using Framework.Runtime.MSceneUnit;
using UnityEngine;

namespace Game.Modules.GModuleSceneUnit
{
    public class GameSceneUnitPoolModifier : ObjectModifier
    {
        public override string modifierName
        {
            get => "GameSceneUnitPoolModifier";
            set { }
        }

        public override T OnCreate<T>(T item, IModifierData data) where T : class
        {
            if (data is SceneUnitGenerateData adata)
            {
                if (!adata.isEntity)
                {
                    SceneUnit sceneUnit = SceneUnitFactory.CreateSceneUnit(adata.sceneUnitType, adata.tag) as SceneUnit;
                    sceneUnit.Id = adata.sceneItemId;
                    if(adata.sceneUnitModelPrefab != null)
                    {
                        var ins =   GameObject.Instantiate(adata.sceneUnitModelPrefab, null);
                        sceneUnit.BindModel(ins);
                    }
                    else if (adata.scenUnitModelAssetVO != null)
                    {
                        sceneUnit.LoadModel(adata.scenUnitModelAssetVO);
                    }
                    else{
                        sceneUnit.LoadModel(adata.scenUnitModelPath);
                    }
                    return sceneUnit as T;
                }
                else
                {
                    GameObject entityIns = null;
                    if(adata.sceneUnitRootPrefab!=null){
                        entityIns =  GameObject.Instantiate(adata.sceneUnitRootPrefab, null);
                    }
                    else if (adata.scenUnitModelAssetVO.IsLoadSuccess)
                    {
                        entityIns = adata.scenUnitModelAssetVO.GetInstance();
                       
                    }

                    if(entityIns!=null){
                        SceneUnit sceneUnit = entityIns.GetComponent<SceneUnit>();
                        sceneUnit.Id = adata.sceneItemId;
                        sceneUnit.transform.SetParent(GameApp.SceneUnitManager.SceneUnitRoot);
                        sceneUnit.transform.localPosition = Vector3.zero;
                        return sceneUnit as T;
                    }
                    return null;
                }
            }

            return base.OnCreate(item, data);
        }

        public override T OnDestroy<T>(T item, IModifierData data)
        {
            if (item is SceneUnit sceneUnit)
            {
                sceneUnit.Dispose();
                return null;
            }

            return base.OnDestroy(item, data);
        }

        public override T OnGet<T>(T item, IModifierData data)
        {
            if (item is SceneUnit sceneUnit)
            {
                sceneUnit.transform.position = Vector3.zero;
                sceneUnit.transform.rotation = Quaternion.identity;
                if (data is SceneUnitGenerateData adata)
                {
                    sceneUnit.EntityRoot.position = adata.position;
                    sceneUnit.EntityRoot.rotation = adata.rotation;
                    //gameObject.transform.parent = adata.parent;
                }
                sceneUnit.EnableUnit();
                sceneUnit.SetActive(true);
                if (sceneUnit.IsModelLoaded())
                {
                    sceneUnit.BindModel(sceneUnit.UnitModelGo);
                }
            }

            return base.OnGet(item, data);
        }

        public override T OnPut<T>(T item, IModifierData data)
        {
            if (item is SceneUnit sceneUnit)
            {
                if (data is SceneUnitGenerateData adata)
                {
                    //sceneUnit.transform.SetParent(adata.parent);
                }
                sceneUnit.DisableUnit();
                sceneUnit.SetActive(false);
                if (sceneUnit.transform.parent != GameApp.SceneUnitManager.SceneUnitRoot)
                {
                    sceneUnit.transform.SetParent(GameApp.SceneUnitManager.SceneUnitRoot);
                }

                sceneUnit.transform.position = Vector3.zero;
                sceneUnit.transform.rotation = Quaternion.identity;
                sceneUnit.EntityRoot.position = Vector3.zero;
                sceneUnit.EntityRoot.rotation = Quaternion.identity;
            }

            return base.OnPut(item, data);
        }
    }
}
using Framework.Runtime.LogSystem;
using Framework.Runtime.MObjectPool.Core;
using UnityEngine;

namespace Framework.Runtime.MObjectPool.GOPool
{
    public class GameObjectModifier : ObjectModifier
    {
        public override string modifierName
        {
            get => "GameObjectModifier";
            set { }
        }

        public override T OnCreate<T>(T item, IModifierData data) where T : class
        {
            Log.Warning("GameObjectModifier.cs" + typeof(T));
            if (data is GameObjectGenerateData adata)
            {
                if (!ReferenceEquals(adata.prefab, null))
                {
                    // 创建一个新的go
                    var go = GameObject.Instantiate(adata.prefab, adata.position, adata.rotation, adata.parent);
                    go.name = adata.prefab.name;
                    return go as T;
                }

                //return new GameObject() as T;
            }

            return base.OnCreate(item, data);
        }

        public override T OnDestroy<T>(T item, IModifierData data)
        {
            if (item is GameObject gameObject)
            {
                GameObject.Destroy(gameObject);
                return null;
            }

            return base.OnDestroy(item, data);
        }

        public override T OnGet<T>(T item, IModifierData data)
        {
            if (item is GameObject gameObject)
            {
                if (data is GameObjectGenerateData adata)
                {
                    gameObject.transform.position = adata.position;
                    gameObject.transform.rotation = adata.rotation;
                    gameObject.transform.parent = adata.parent;
                }

                gameObject.SetActive(true);
            }

            return base.OnGet(item, data);
        }

        public override T OnPut<T>(T item, IModifierData data)
        {
            if (item is GameObject gameObject)
            {
                if (data is GameObjectGenerateData adata)
                {
                    gameObject.transform.SetParent(adata.parent);
                }

                gameObject.SetActive(false);
            }

            return base.OnPut(item, data);
        }
    }
}
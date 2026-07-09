using Framework.Runtime.MObjectPool.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.MObjectPool.GOPool
{
    public class RigidbodyModifier : ObjectModifier
    {
        private Dictionary<GameObject, Rigidbody> _rigidbodiesCache = new Dictionary<GameObject, Rigidbody>();

        public override string modifierName
        {
            get => "RigidbodyModifier";
            set { }
        }

        public override T OnDestroy<T>(T item, IModifierData data)
        {
            if (item is GameObject gameObject)
            {
                if (_rigidbodiesCache.ContainsKey(gameObject))
                {
                    _rigidbodiesCache.Remove(gameObject);
                }
            }

            return base.OnDestroy(item, data);
        }

        public override T OnGet<T>(T item, IModifierData data)
        {
            if (item is GameObject gameObject)
            {
                if (!_rigidbodiesCache.ContainsKey(gameObject))
                {
                    if (gameObject.TryGetComponent(out Rigidbody rigidbody))
                    {
                        _rigidbodiesCache.Add(gameObject, rigidbody);
                    }
                }

                if (_rigidbodiesCache.ContainsKey(gameObject))
                {
                    _rigidbodiesCache[gameObject].velocity = Vector3.zero;
                    // _rigidbodiesCache[gameObject].isKinematic = false;
                    // _rigidbodiesCache[gameObject].useGravity = true;
                    // _rigidbodiesCache[gameObject].constraints = RigidbodyConstraints.None;
                }
            }

            return base.OnGet(item, data);
        }

        public override T OnPut<T>(T item, IModifierData data)
        {
            if (item is GameObject gameObject)
            {
                if (!_rigidbodiesCache.ContainsKey(gameObject))
                {
                    if (gameObject.TryGetComponent(out Rigidbody rigidbody))
                    {
                        _rigidbodiesCache.Add(gameObject, rigidbody);
                    }
                }

                if (_rigidbodiesCache.ContainsKey(gameObject))
                {
                    _rigidbodiesCache[gameObject].velocity = Vector3.zero;
                    // _rigidbodiesCache[gameObject].isKinematic = true;
                    // _rigidbodiesCache[gameObject].useGravity = false;
                    // _rigidbodiesCache[gameObject].constraints = RigidbodyConstraints.FreezeAll;
                }
            }

            return base.OnPut(item, data);
        }
    }
}
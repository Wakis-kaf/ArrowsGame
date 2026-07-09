using Framework.Runtime.MObjectPool.Core;

using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.MObjectPool.GOPool
{
    public class TrailRenderModifier : ObjectModifier
    {
        private Dictionary<GameObject, TrailRenderer>
            _trailRenderersCache = new Dictionary<GameObject, TrailRenderer>();

        public override string modifierName
        {
            get => "TrailRenderModifier";
            set { }
        }

        public override T OnGet<T>(T item, IModifierData data)
        {
            if (item is GameObject gameObject)
            {
                if (!_trailRenderersCache.ContainsKey(gameObject))
                {
                    if (gameObject.TryGetComponent(out TrailRenderer trail))
                    {
                        _trailRenderersCache.Add(gameObject, trail);
                    }
                }

                if (_trailRenderersCache.ContainsKey(gameObject))
                {
                    _trailRenderersCache[gameObject].Clear();
                }
            }

            return base.OnGet(item, data);
        }
    }
}
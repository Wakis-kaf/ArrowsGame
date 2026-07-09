using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework.Runtime.UI
{
    public class UAtlas : MonoBehaviour
    {
        public Texture mainTexture;

        [SerializeField] private List<Sprite> _spriteList = new List<Sprite>();

        // 使用属性来确保正确的序列化
        public List<Sprite> spriteList
        {
            get
            {
#if UNITY_EDITOR
                // 在编辑器模式下清理空引用并重新建立引用
                if (!Application.isPlaying)
                {
                    CleanNullSprites();
                    RebuildSpriteReferences();
                }
#endif
                return _spriteList;
            }
            set { _spriteList = value; }
        }

        private bool _release;

        public bool IsRelease
        {
            get { return _release; }
            set { _release = value; }
        }

        public void AddSprite(Sprite sprite)
        {
            if (sprite != null && !_spriteList.Contains(sprite))
            {
                _spriteList.Add(sprite);
                MarkAsChanged();
            }
        }

        public Sprite GetSprite(string theName)
        {
            if (string.IsNullOrEmpty(theName))
            {
                return null;
            }

            // 确保列表没有空引用
            CleanNullSprites();

            return spriteList.FirstOrDefault(sprite => sprite != null && sprite.name == theName);
        }

        // 清理空引用
        public void CleanNullSprites()
        {
#if UNITY_EDITOR
            int removedCount = _spriteList.RemoveAll(sprite => sprite == null);
            if (removedCount > 0)
            {
                MarkAsChanged();
            }
#endif
        }

        // 重新建立 Sprite 引用
        private void RebuildSpriteReferences()
        {
#if UNITY_EDITOR
            if (mainTexture == null) return;

            string texturePath = UnityEditor.AssetDatabase.GetAssetPath(mainTexture);
            if (string.IsNullOrEmpty(texturePath)) return;

            // 获取当前有效的 Sprite
            Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(texturePath);
            List<Sprite> validSprites = new List<Sprite>();
            foreach (Object asset in assets)
            {
                Sprite sprite = asset as Sprite;
                if (sprite != null)
                {
                    validSprites.Add(sprite);
                }
            }

            // 如果数量不一致，重新建立引用
            if (validSprites.Count != _spriteList.Count ||
                _spriteList.Any(sprite => sprite == null))
            {
                _spriteList.Clear();
                _spriteList.AddRange(validSprites);
                _spriteList.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
                MarkAsChanged();
            }
#endif
        }

        public void MarkAsChanged()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.EditorUtility.SetDirty(gameObject);
            }
#endif

            // 原有的 USprite 刷新逻辑...
            USprite[] array = Object.FindObjectsOfType(typeof(USprite)) as USprite[];
            if (array == null)
            {
                return;
            }

            foreach (USprite cSprite in array)
            {
                if (cSprite.Atlas == this)
                {
                    cSprite.RefreshSprite();
#if UNITY_EDITOR
                    if ((Application.isEditor))
                    {
                        UnityEditor.EditorUtility.SetDirty(cSprite);
                    }
#endif
                }
            }
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            // 在 Inspector 修改时自动清理和重建引用
            CleanNullSprites();
            RebuildSpriteReferences();
        }

#endif

        public void OnDestroy()
        {
            Resources.UnloadAsset(mainTexture);
        }
    }
}
using UnityEngine;
using Sirenix.OdinInspector;

namespace Framework.Library.C2D
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteOutline : MonoBehaviour
    {
        [SerializeField] public float outlineWidth = 0f;
        [SerializeField] public Color outlineColor = Color.white;

        private SpriteRenderer spriteRenderer;
        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            UpdateOutline();
        }

        private void OnDisable()
        {
            ClearOutline();
        }

        [Button("刷新视图")]
        public void UpdateOutline()
        {
            if (!Application.isPlaying) return;

            Initialize();

            if (Mathf.Abs(outlineWidth) < 0.001f)
            {
                ClearOutline();
                return;
            }

            if (propertyBlock == null)
                propertyBlock = new MaterialPropertyBlock();

            spriteRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat("_OutlineWidth", outlineWidth);
            propertyBlock.SetColor("_OutlineColor", outlineColor);
            spriteRenderer.SetPropertyBlock(propertyBlock);
        }

        private void ClearOutline()
        {
            if (propertyBlock == null || spriteRenderer == null) return;

            spriteRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat("_OutlineWidth", 0);
            spriteRenderer.SetPropertyBlock(propertyBlock);
        }

        private void Initialize()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnValidate()
        {
            if (Application.isPlaying && spriteRenderer != null)
            {
                UpdateOutline();
            }
        }

        public void SetOutline(float width, Color color)
        {
            outlineWidth = width;
            outlineColor = color;
            UpdateOutline();
        }
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Runtime.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Image))]
    public class CircularTransitionController : MonoBehaviour
    {
        [Header("Settings")]
        public Color maskColor = Color.black; // 新增：默认黑色
        [Range(0f, 1f)]
        public float progress = 0f;
        [Range(0.01f, 2.0f)]
        public float smoothness = 2f;

        private Image _targetImage;
        private Material _materialInstance;
        private Coroutine _transitionCoroutine;

        private static readonly int ProgressID = Shader.PropertyToID("_Progress");
        private static readonly int SmoothnessID = Shader.PropertyToID("_Smoothness");
        private static readonly int AspectID = Shader.PropertyToID("_Aspect");
        private static readonly int ColorID = Shader.PropertyToID("_Color"); // 新增：颜色ID

        void OnEnable() { Init(); }

        void Init()
        {
            if (_targetImage == null) _targetImage = GetComponent<Image>();
            if (_materialInstance == null && _targetImage.material != null)
            {
                CreateMaterialInstance(_targetImage.material);
            }
        }

        private void CreateMaterialInstance(Material baseMaterial)
        {
            if (_materialInstance != null)
            {
                if (Application.isPlaying) Destroy(_materialInstance);
                else DestroyImmediate(_materialInstance);
            }

            if (baseMaterial != null)
            {
                _materialInstance = new Material(baseMaterial);
                _targetImage.material = _materialInstance;
            }
        }

        /// <summary>
        /// 内部核心方法：设置颜色
        /// </summary>
        public void SetColor(Color color)
        {
            maskColor = color;
            UpdateMaterial();
        }

        public void SetMaterial(Material newBaseMaterial)
        {
            if (newBaseMaterial == null) return;
            if (_targetImage == null) _targetImage = GetComponent<Image>();
            CreateMaterialInstance(newBaseMaterial);
            UpdateMaterial();
        }

        void Update() { UpdateMaterial(); }

        public void SetProgressImmediate(float value)
        {
            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
                _transitionCoroutine = null;
            }
            progress = Mathf.Clamp01(value);
            UpdateMaterial();
        }

        public void PlayTransition(float target, float duration)
        {
            if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = StartCoroutine(TransitionRoutine(target, duration));
        }

        private IEnumerator TransitionRoutine(float target, float duration)
        {
            float startValue = progress;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                progress = Mathf.SmoothStep(startValue, target, elapsed / duration);
                UpdateMaterial();
                yield return null;
            }
            progress = target;
            UpdateMaterial();
            _transitionCoroutine = null;
        }

        private void UpdateMaterial()
        {
            if (_materialInstance == null) return;

            // 同步颜色到 Shader
            _materialInstance.SetColor(ColorID, maskColor);
            _materialInstance.SetFloat(ProgressID, progress);

            if (_materialInstance.HasProperty("_Blurriness"))
                _materialInstance.SetFloat("_Blurriness", smoothness);
            else if (_materialInstance.HasProperty("_Smoothness"))
                _materialInstance.SetFloat(SmoothnessID, smoothness);

            Rect rect = _targetImage.rectTransform.rect;
            float aspect = rect.height > 0 ? rect.width / rect.height : 1f;
            _materialInstance.SetFloat(AspectID, aspect);
        }

        private void OnDestroy()
        {
            if (_materialInstance != null)
            {
                if (Application.isPlaying) Destroy(_materialInstance);
                else DestroyImmediate(_materialInstance);
            }
        }
    }
}
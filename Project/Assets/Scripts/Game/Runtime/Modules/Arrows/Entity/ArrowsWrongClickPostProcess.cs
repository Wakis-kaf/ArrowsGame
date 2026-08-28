using UnityEngine;

namespace Game.Modules.GModuleArrows
{
    [RequireComponent(typeof(Camera))]
    public class ArrowsWrongClickPostProcess : MonoBehaviour
    {
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        private Material m_Material;
        private float m_Intensity;

        private void Awake()
        {
            enabled = false;
        }

        public void Flash(float intensity = 0.72f)
        {
            if (!EnsureMaterial()) return;
            m_Intensity = Mathf.Max(m_Intensity, intensity);
            enabled = true;
        }

        private void Update()
        {
            m_Intensity = Mathf.MoveTowards(m_Intensity, 0, Time.unscaledDeltaTime * 3.6f);
            if (m_Intensity <= 0) enabled = false;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!EnsureMaterial())
            {
                Graphics.Blit(source, destination);
                return;
            }

            m_Material.SetFloat(IntensityId, m_Intensity);
            Graphics.Blit(source, destination, m_Material);
        }

        private bool EnsureMaterial()
        {
            if (m_Material != null) return true;
            var shader = Resources.Load<Shader>("Shaders/ArrowsWrongClickPostProcess");
            if (shader == null) shader = Shader.Find("ArrowsGame/WrongClickPostProcess");
            if (shader == null) return false;
            m_Material = new Material(shader) { hideFlags = HideFlags.DontSave };
            return true;
        }

        private void OnDestroy()
        {
            if (m_Material != null) Destroy(m_Material);
        }
    }
}

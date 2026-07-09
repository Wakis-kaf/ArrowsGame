using UnityEngine;
using DG.Tweening;
using System;
using Random = UnityEngine.Random;
public class DissolveController : MonoBehaviour
{
    private Material _mat;
    private static Texture2D _sharedNoise;

    void Awake()
    {
        EnsureInit();
    }

    public void EnsureInit()
    {
        if (_mat != null) return;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            _mat = sr.material;
        }

        if (_sharedNoise == null)
        {
            _sharedNoise = GeneratePerlinNoise(256, 256, 15f);
        }

        if (_mat != null)
        {
            _mat.SetTexture("_NoiseTex", _sharedNoise);
        }
    }

    // 增加此方法，用于在 BuildingSceneUnit 中停止动画
    public void DOKill()
    {
        if (_mat != null)
        {
            DOTween.Kill(_mat);
        }
    }

    public Tweener DoDissolve(float targetValue, float duration,Action onComplete = null)
    {
        EnsureInit();
        if (_mat == null) return null;

        // 使用 _mat 作为 target 方便管理和 Kill
        return DOTween.To(() => _mat.GetFloat("_DissolveAmount"),
                          x => _mat.SetFloat("_DissolveAmount", x),
                          targetValue, duration)
                      .SetTarget(_mat)
                      .OnComplete(() =>
                      {
                          if (targetValue <= 0.001f) SetDissolveValue(0f);
                          onComplete?.Invoke();
                      }).SetUpdate(true);
    }

    public void SetDissolveValue(float val)
    {
        EnsureInit();
        if (_mat != null)
        {
            _mat.SetFloat("_DissolveAmount", val);
        }
    }

    private Texture2D GeneratePerlinNoise(int width, int height, float scale)
    {
        Texture2D tex = new Texture2D(width, height);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[width * height];
        float offsetX = Random.Range(0f, 100f);
        float offsetY = Random.Range(0f, 100f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float xCoord = (float)x / width * scale + offsetX;
                float yCoord = (float)y / height * scale + offsetY;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                pixels[y * width + x] = new Color(sample, sample, sample);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}
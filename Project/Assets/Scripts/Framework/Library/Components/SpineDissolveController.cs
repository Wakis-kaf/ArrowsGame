using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class SpineDissolveController : MonoBehaviour
{
    public enum DissolveType { Default, UpToDown, DownToUp, LeftToRight, RightToLeft, InToOut, OutToIn }
    public enum CoordMode { UV, Local }

    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;
    private static Texture2D _sharedNoise;

    [SerializeField, Range(0, 1), OnValueChanged("OnDissolveProgressChanged")]
    private float _currentDissolveProgress;
    private System.Action _onDissolveCompleteCallback;

    private static readonly int DissolveAmountID = Shader.PropertyToID("_DissolveAmount");
    private static readonly int NoiseTexID = Shader.PropertyToID("_NoiseTex");
    private static readonly int ModelRectID = Shader.PropertyToID("_ModelRect");
    private static readonly int CoordModeID = Shader.PropertyToID("_CoordMode");
    private static readonly int DissolveTypeID = Shader.PropertyToID("_DissolveType");

    void Awake() => EnsureInit();

    public void EnsureInit()
    {
        if (_renderer == null) _renderer = GetComponent<Renderer>();
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        if (_sharedNoise == null) _sharedNoise = GeneratePerlinNoise(256, 256, 12f);

        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetTexture(NoiseTexID, _sharedNoise);
        _renderer.SetPropertyBlock(_propBlock);
    }

    public Tweener DoDissolve(float targetValue, float duration,
        System.Action onComplete = null, DissolveType type = DissolveType.Default,
        CoordMode mode = CoordMode.Local)
    {
        EnsureInit();

        UpdateDissolveSettings(mode, type);

        _onDissolveCompleteCallback = onComplete;

        _renderer.GetPropertyBlock(_propBlock);
        _currentDissolveProgress = _propBlock.GetFloat(DissolveAmountID);

        return DOTween.To(GetProgress, SetProgress, targetValue, duration)
            .OnUpdate(ApplyPropertyBlock)
            .OnComplete(HandleComplete)
            .SetTarget(this)
            .SetUpdate(true);
    }

    private void UpdateDissolveSettings(CoordMode mode, DissolveType type)
    {
        _renderer.GetPropertyBlock(_propBlock);

        if (mode == CoordMode.Local)
        {
            Bounds b = _renderer.localBounds;
            Vector4 rect = new Vector4(b.min.x, b.min.y, b.max.x, b.max.y);
            _propBlock.SetVector(ModelRectID, rect);
        }

        _propBlock.SetFloat(CoordModeID, (float)mode);
        _propBlock.SetFloat(DissolveTypeID, (float)type);

        _renderer.SetPropertyBlock(_propBlock);
    }

    private float GetProgress() => _currentDissolveProgress;
    private void SetProgress(float val) => _currentDissolveProgress = val;

    private void ApplyPropertyBlock()
    {
        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat(DissolveAmountID, _currentDissolveProgress);
        _renderer.SetPropertyBlock(_propBlock);
    }

    private void HandleComplete()
    {
        _onDissolveCompleteCallback?.Invoke();
        _onDissolveCompleteCallback = null;
    }

    private void OnDissolveProgressChanged()
    {
        SetDissolveValue(_currentDissolveProgress);
    }

    public void SetDissolveValue(float val)
    {
        EnsureInit();
        _currentDissolveProgress = val;
        ApplyPropertyBlock();
    }

    private Texture2D GeneratePerlinNoise(int width, int height, float scale)
    {
        Texture2D tex = new Texture2D(width, height);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float sample = Mathf.PerlinNoise((float)x / width * scale, (float)y / height * scale);
                pixels[y * width + x] = new Color(sample, sample, sample);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}
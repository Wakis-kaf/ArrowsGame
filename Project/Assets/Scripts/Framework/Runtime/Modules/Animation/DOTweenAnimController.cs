using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Object = UnityEngine.Object;

public enum AnimProp
{
    Move, LocalMove, Rotate, LocalRotate, Scale,
    Fade, Color, RectSize, AnchorPos,
    Visible, Hidden, FontSize, Text
}

public class DOTweenAnimController<TBranch> where TBranch : Enum
{
    private Dictionary<TBranch, List<Action<Sequence>>> m_BranchBuilders = new Dictionary<TBranch, List<Action<Sequence>>>();
    private Dictionary<TBranch, Sequence> m_ActiveSequences = new Dictionary<TBranch, Sequence>();
    private TBranch m_BuildingBranch;

    public bool IsPlaying(TBranch branch) => m_ActiveSequences.TryGetValue(branch, out var s) && s != null && s.IsActive() && s.IsPlaying();

    public void KillBranch(TBranch branch)
    {
        if (m_ActiveSequences.TryGetValue(branch, out var s))
        {
            if (s != null && s.IsActive()) s.Kill();
            m_ActiveSequences.Remove(branch);
        }
    }

    public void KillAndClearBranch(TBranch branch)
    {
        KillBranch(branch);
        if (m_BranchBuilders.ContainsKey(branch)) m_BranchBuilders[branch].Clear();
    }

    public void PauseBranch(TBranch branch)
    {
        if (m_ActiveSequences.TryGetValue(branch, out var s) && s != null && s.IsActive()) s.Pause();
    }

    public void ResumeBranch(TBranch branch)
    {
        if (m_ActiveSequences.TryGetValue(branch, out var s) && s != null && s.IsActive()) s.Play();
    }

    public void CompleteBranch(TBranch branch, bool withCallbacks = true)
    {
        if (m_ActiveSequences.TryGetValue(branch, out var s) && s != null && s.IsActive()) s.Complete(withCallbacks);
    }

    public void KillAll()
    {
        foreach (var s in m_ActiveSequences.Values) s?.Kill();
        m_ActiveSequences.Clear();
    }

    public DOTweenAnimController<TBranch> Branch(TBranch branch)
    {
        m_BuildingBranch = branch;
        if (!m_BranchBuilders.ContainsKey(branch)) m_BranchBuilders[branch] = new List<Action<Sequence>>();
        return this;
    }

    public DOTweenAnimController<TBranch> Append(Tween tween)
    {
        m_BranchBuilders[m_BuildingBranch].Add((seq) => seq.Append(tween));
        return this;
    }

    public DOTweenAnimController<TBranch> Join(Tween tween)
    {
        m_BranchBuilders[m_BuildingBranch].Add((seq) => seq.Join(tween));
        return this;
    }

    public DOTweenAnimController<TBranch> Append(Object target, AnimProp prop, object val, float dur, Ease ease = Ease.Linear, float delay = 0)
    {
        m_BranchBuilders[m_BuildingBranch].Add((seq) =>
        {
            if (dur <= 0) seq.AppendInterval(delay).AppendCallback(() => ApplyInstant(target, prop, val));
            else seq.AppendInterval(delay).Append(CreateTween(target, prop, val, dur, ease));
        });
        return this;
    }

    public DOTweenAnimController<TBranch> Join(Object target, AnimProp prop, object val, float dur, Ease ease = Ease.Linear, float delay = 0)
    {
        m_BranchBuilders[m_BuildingBranch].Add((seq) =>
        {
            if (dur <= 0) seq.InsertCallback(seq.Duration() + delay, () => ApplyInstant(target, prop, val));
            else seq.Join(CreateTween(target, prop, val, dur, ease).SetDelay(delay));
        });
        return this;
    }

    public DOTweenAnimController<TBranch> OnComplete(Action action)
    {
        var builders = m_BranchBuilders[m_BuildingBranch];
        if (builders.Count == 0) return this;
        var lastBuilder = builders[builders.Count - 1];
        builders[builders.Count - 1] = (seq) =>
        {
            lastBuilder(seq);
            seq.InsertCallback(seq.Duration(), () => action?.Invoke());
        };
        return this;
    }

    public DOTweenAnimController<TBranch> OnCompleteUp(Action action)
    {
        m_BranchBuilders[m_BuildingBranch].Add((seq) => seq.AppendCallback(() => action?.Invoke()));
        return this;
    }

    public DOTweenAnimController<TBranch> Wait(float dur)
    {
        m_BranchBuilders[m_BuildingBranch].Add((seq) => seq.AppendInterval(dur));
        return this;
    }

    public void Play(TBranch branch)
    {
        KillBranch(branch);
        if (m_BranchBuilders.TryGetValue(branch, out var builders))
        {
            Sequence s = DOTween.Sequence();
            foreach (var b in builders) b(s);
            m_ActiveSequences[branch] = s;
            s.OnComplete(() => m_ActiveSequences.Remove(branch));
            s.Play();
        }
    }

    public async Task PlayAsync(TBranch branch)
    {
        Play(branch);
        if (m_ActiveSequences.TryGetValue(branch, out var s)) await s.AsyncWaitForCompletion();
    }

    private Tween CreateTween(Object target, AnimProp prop, object val, float dur, Ease ease)
    {
        if (target == null) return null;
        var go = target as GameObject ?? (target as Component)?.gameObject;
        if (go == null) return null;

        Tween t = null;
        switch (prop)
        {
            case AnimProp.Move: t = go.transform.DOMove(AsV3(val), dur); break;
            case AnimProp.LocalMove: t = go.transform.DOLocalMove(AsV3(val), dur); break;
            case AnimProp.Rotate: t = go.transform.DORotate(AsV3(val), dur); break;
            case AnimProp.LocalRotate: t = go.transform.DOLocalRotate(AsV3(val), dur); break;
            case AnimProp.Scale: t = go.transform.DOScale(AsV3(val), dur); break;
            case AnimProp.RectSize: t = go.GetComponent<RectTransform>()?.DOSizeDelta(AsV2(val), dur); break;
            case AnimProp.AnchorPos: t = go.GetComponent<RectTransform>()?.DOAnchorPos(AsV2(val), dur); break;
            case AnimProp.FontSize:
                var tmp = go.GetComponent<TMP_Text>();
                if (tmp) t = DOTween.To(() => tmp.fontSize, x => tmp.fontSize = x, AsFloat(val), dur);
                break;
            case AnimProp.Fade:
                float f = AsFloat(val);
                var cg = go.GetComponent<CanvasGroup>();
                if (cg) t = cg.DOFade(f, dur);
                else
                {
                    var img = go.GetComponent<Graphic>();
                    if (img) t = img.DOFade(f, dur);
                    else
                    {
                        var spr = go.GetComponent<SpriteRenderer>();
                        if (spr) t = spr.DOFade(f, dur);
                        else
                        {
                            var txt = go.GetComponent<TMP_Text>();
                            if (txt) t = txt.DOFade(f, dur);
                        }
                    }
                }
                break;
            case AnimProp.Color:
                Color c = AsColor(val);
                var g = go.GetComponent<Graphic>();
                if (g) t = g.DOColor(c, dur);
                else
                {
                    var sr = go.GetComponent<SpriteRenderer>();
                    if (sr) t = sr.DOColor(c, dur);
                    else
                    {
                        var tx = go.GetComponent<TMP_Text>();
                        if (tx) t = tx.DOColor(c, dur);
                    }
                }
                break;
            case AnimProp.Text:
                var tmpText = go.GetComponent<TMP_Text>();
                if (tmpText) t = DOTween.To(() => tmpText.text, x => tmpText.text = x, val.ToString(), dur).SetTarget(tmpText);
                else
                {
                    var uText = go.GetComponent<Text>();
                    if (uText) t = uText.DOText(val.ToString(), dur);
                }
                break;
        }
        return t?.SetEase(ease);
    }

    private void ApplyInstant(Object target, AnimProp prop, object val)
    {
        if (target == null) return;
        var go = target as GameObject ?? (target as Component)?.gameObject;
        if (go == null) return;
        if (prop == AnimProp.Visible) { go.SetActive(true); return; }
        if (prop == AnimProp.Hidden) { go.SetActive(false); return; }
        CreateTween(target, prop, val, 0.001f, Ease.Linear)?.Complete();
    }

    private Vector3 AsV3(object val) => val is Vector3 v3 ? v3 : (val is Vector2 v2 ? (Vector3)v2 : Vector3.zero);
    private Vector2 AsV2(object val) => val is Vector2 v2 ? v2 : (val is Vector3 v3 ? (Vector2)v3 : Vector2.zero);
    private float AsFloat(object val) => val is float f ? f : (val is Vector3 v3 ? v3.x : (val is int i ? (float)i : 0f));
    private Color AsColor(object val) => val is Color color ? color : Color.white;
}
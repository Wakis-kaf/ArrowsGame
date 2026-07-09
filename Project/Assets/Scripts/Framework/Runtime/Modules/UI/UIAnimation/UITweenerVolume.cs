using Framework.Runtime.UI.UIAnimae.Tweeners;

using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.UI.UIAnimae
{
    [Serializable]
    public class UITweenerVolume
    {
        [Title("$m_TweenerType")]
        //[SerializeField ,LabelText("动效类型"), EnumPaging, OnValueChanged("OnTweenerTypeChanged")]
        [EnumPaging,SerializeField, OnValueChanged("OnTweenerTypeChanged"), LabelText("动效类型")]
        private UITweenerType m_TweenerType;

        [SerializeField] private string m_Id = "";
        [SerializeField,FoldoutGroup("AnimationControlTweener"), HideLabel, ShowIf("@this.m_TweenerType ", UITweenerType.AnimationControl)]
        private AnimationControlTweener m_AnimationControlTweener = new AnimationControlTweener();

        [SerializeField, FoldoutGroup("AnimatorCallTweener"), HideLabel,ShowIf("@this.m_TweenerType ", UITweenerType.CallAnimatorFunc)]
        private AnimatorCallTweener m_AnimatorCallTweener = new AnimatorCallTweener();

        private Dictionary<UITweenerType, UITweener> m_AnimType2AnimationDict =
            new Dictionary<UITweenerType, UITweener>();

        private UITweenSequence m_BindSequence;

        [SerializeField, FoldoutGroup("CanvasGroupAlphaTweener"), HideLabel, ShowIf("@this.m_TweenerType ", UITweenerType.CanvasGroupAlpha)]
        private CanvasGroupAlphaTweener m_CanvasGroupAlphaTweener = new CanvasGroupAlphaTweener();

        [NonSerialized] private UITweener m_CurrentTweener;

        [SerializeField, ShowIf("@this.m_TweenerType", UITweenerType.Empty)]
        private EmptyTweener m_EmptyTweener = new EmptyTweener();


        [SerializeField, FoldoutGroup("SpriteChangeTweener"), HideLabel, ShowIf("@this.m_TweenerType ", UITweenerType.ImageChange)]
        private SpriteChangeTweener m_SpriteChangeTweener = new SpriteChangeTweener();
        [SerializeField, FoldoutGroup("AtlasSpriteChangeTweener"), HideLabel, ShowIf("@this.m_TweenerType ", UITweenerType.AtlasSpriteChange)]
        private AtlasSpriteChangeTweener m_AtlasSpriteChangeTweener = new AtlasSpriteChangeTweener();

        [SerializeField, FoldoutGroup("SpriteColorTweener"), HideLabel, ShowIf("@this.m_TweenerType ", UITweenerType.SpriteColor)]
        private SpriteColorTweener m_SpriteColorTweener = new SpriteColorTweener();

        [SerializeField, FoldoutGroup("TextChangeTweener"), HideLabel, ShowIf("@this.m_TweenerType ", UITweenerType.TextChange)]
        private TextChangeTweener m_TextChangeTweener = new TextChangeTweener();

        [SerializeField, FoldoutGroup("TextColorTweener"), HideLabel, ShowIf("@this.m_TweenerType ", UITweenerType.TextColor)]
        private TextColorTweener m_TextColorTweener = new TextColorTweener();

        [SerializeField, FoldoutGroup("TransformMoveTweener"), HideLabel, ShowIf("@this.m_TweenerType ", UITweenerType.TransformMove)]
        private TransformPositionTweener m_TransformMoveTweener = new TransformPositionTweener();

        [SerializeField, FoldoutGroup("TransformRotateTweener"), HideLabel, ShowIf("@this.m_TweenerType ", UITweenerType.TransformRotate)]
        private TransformRotateTweener m_TransformRotateTweener = new TransformRotateTweener();

        [SerializeField, FoldoutGroup("TransformScaleTweener"), HideLabel, ShowIf("@this.m_TweenerType ", UITweenerType.TransformScale)]
        private TransformScaleTweener m_TransformScaleTweener = new TransformScaleTweener();
        [SerializeField, FoldoutGroup("GameActiveTweener"), HideLabel, ShowIf("@this.m_TweenerType ", UITweenerType.GameActiveChange)]
        private GameActiveChangeTweener m_GameActiveChangeTweener = new GameActiveChangeTweener();




        private List<UITweener> m_UIAnimationList = new List<UITweener>();
        public UITweenSequence BindSequence => m_BindSequence;

        public UITweener CurrentTweene
        {
            get
            {
                if (m_CurrentTweener == null)
                {
                    m_CurrentTweener = m_AnimType2AnimationDict[m_TweenerType];
                }

                return m_CurrentTweener;
            }
        }

        public UITweenerType CurTweenerType => m_TweenerType;
        public string Id => m_Id;

        public void Init(UITweenSequence sequence)
        {
            m_BindSequence = sequence;
            InitEffects();
            CurrentTweene.BindSequence(sequence);
            CurrentTweene.Init();
        }
        private void ClearEffects()
        {
            m_EmptyTweener = new EmptyTweener();
            m_TransformScaleTweener = new TransformScaleTweener();
            m_TransformRotateTweener = new TransformRotateTweener();
            m_TransformMoveTweener = new TransformPositionTweener();
            m_SpriteChangeTweener = new SpriteChangeTweener();
            m_AtlasSpriteChangeTweener = new AtlasSpriteChangeTweener();
            m_SpriteColorTweener = new SpriteColorTweener();
            m_TextChangeTweener = new TextChangeTweener();
            m_TextColorTweener = new TextColorTweener();
            m_AnimatorCallTweener = new AnimatorCallTweener();
            m_CanvasGroupAlphaTweener = new CanvasGroupAlphaTweener();
            m_AnimationControlTweener = new AnimationControlTweener();
            m_GameActiveChangeTweener = new GameActiveChangeTweener();
        }
     
        private void InitEffects()
        {
            m_UIAnimationList.Clear();
            m_AnimType2AnimationDict.Clear();
            RegisterEffect(ref m_EmptyTweener, UITweenerType.Empty);
            RegisterEffect(ref m_TransformScaleTweener, UITweenerType.TransformScale);
            RegisterEffect(ref m_TransformRotateTweener, UITweenerType.TransformRotate);
            RegisterEffect(ref m_TransformMoveTweener, UITweenerType.TransformMove);
            RegisterEffect(ref m_SpriteChangeTweener, UITweenerType.ImageChange);
            RegisterEffect(ref m_AtlasSpriteChangeTweener, UITweenerType.AtlasSpriteChange);
            RegisterEffect(ref m_SpriteColorTweener, UITweenerType.SpriteColor);
            RegisterEffect(ref m_TextChangeTweener, UITweenerType.TextChange);
            RegisterEffect(ref m_TextColorTweener, UITweenerType.TextColor);
            RegisterEffect(ref m_AnimatorCallTweener, UITweenerType.CallAnimatorFunc);
            RegisterEffect(ref m_CanvasGroupAlphaTweener, UITweenerType.CanvasGroupAlpha);
            RegisterEffect(ref m_AnimationControlTweener, UITweenerType.AnimationControl);
            RegisterEffect(ref m_GameActiveChangeTweener, UITweenerType.GameActiveChange);
             //RegisterEffect(ref m_SuperAnimation, UITweenerType.SuperAnimation);
        }

        private void OnTweenerTypeChanged()
        {
            ClearEffects();
            InitEffects();
            SwitchTweener(m_TweenerType);
        }

        private void RegisterEffect<T>(ref T save, UITweenerType type) where T : UITweener
        {
            if (!m_AnimType2AnimationDict.ContainsKey(type))
            {
                m_AnimType2AnimationDict.Add(type, save);
            }

            m_UIAnimationList.Add(save);
        }

        private void SwitchTweener(UITweenerType tweenerType)
        {
            m_CurrentTweener = m_AnimType2AnimationDict[tweenerType];
        }
    }
}
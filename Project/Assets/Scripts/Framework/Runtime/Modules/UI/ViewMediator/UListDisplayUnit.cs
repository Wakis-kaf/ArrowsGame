using Cysharp.Threading.Tasks;
using DG.Tweening;
using Framework.Utils;

using System;
using System.Threading;
using UnityEngine;

namespace Framework.Runtime.UI
{
    public class ListOption
    {
        public Vector3 itemScale = Vector3.one;
    }
    public class UListDisplayUnit : DisplayUnit, IAutoLayoutItem
    {
        private UIBaseRender m_BaseRender;
        private int m_Index;
        private bool m_IsCanvasAlphaZero = false;
        private bool m_IsSelected;
        private ListOption m_ListOption;
        private RectTransform m_RectTransform;
        private Vector2 m_SlotSize = Vector2.one * 100;
        private Type m_Type;
        public UListDisplayUnit()
        {
        }

        public UListDisplayUnit(object data)
        {
            SetData(data);
        }

        public virtual UList BindList { get; set; }

        public int Index
        {
            get { return m_Index; }
            set { m_Index = value; }
        }

        public bool IsSelected
        {
            get
            {
                return m_IsSelected;
            }
            set
            {
                m_IsSelected = value;
            }
        }

        public RectTransform rectTransform
        {
            get
            {
                if (m_RectTransform == null && DisplayGO != null)
                {
                    m_RectTransform = GameObjectUtil.GetOrAddComponent<RectTransform>(DisplayGO);
                }

                return m_RectTransform;
            }
        }

        //public override bool IsShow
        //{
        //    get => m_IsShow;
        //}
        public Type selfType
        {
            get
            {
                if (m_Type == null)
                {
                    m_Type = GetType();
                }

                return m_Type;
            }
        }

        public Vector2 size
        {
            get
            {
                if (DisplayGO != null)
                {
                    return rectTransform.sizeDelta;
                }

                return slotSize;
            }
        }

        public virtual Vector2 slotSize
        {
            get { return m_SlotSize; }
            set { m_SlotSize = value; }
        }
        
        public UIBaseRender UIBaseRender
        {
            get
            {
                if(m_BaseRender == null)
                {
                    m_BaseRender = DisplayGO.GetComponent<UIBaseRender>();
                }
                return m_BaseRender;
            }
        }
        CancellationTokenSource m_StartCancelToken;
        public int GetInVisibleSetIndex()
        {
            return BindList!=null?BindList.GetInVisibleSetIndex(this.Index):-1;
        }
        public virtual void OnOptionSet(ListOption option)
        {

        }

        public void SetListOption(ListOption option)
        {
            if (IsUIInited)
            {
                m_ListOption = option;
                OnOptionSet(option);
            }
            else
            {
                m_ListOption = option;
            }
        }

        protected override void CheckInitUI()
        {
            base.CheckInitUI();
            OnOptionSet(m_ListOption);
        }

        protected override void OnStartHideEffect(Action hideCompleted)
        {
            if (CanvasGroup != null )
            {
                StopInListVisibleAnimation();
                DoCanvasGroupHide();
                hideCompleted();
            }
            else
            {
                DisplayGO.SetActive(false);
                hideCompleted();
            }


        }
        protected override void OnClearHideEffect()
        {
            base.OnClearHideEffect();
        }
        protected override void OnClearShowEffect()
        {
            base.OnClearShowEffect();
            StopInListVisibleAnimation();
        }
        private void StopInListVisibleAnimation()
        {
            m_StartCancelToken?.Cancel();
            m_StartCancelToken?.Dispose();
            m_StartCancelToken = null;
        }
        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            StopInListVisibleAnimation();
        }
        protected override async void OnStartShowEffect(Action ShowCompleteCb)
        {
            if (CanvasGroup != null )
            {
                try
                {
                    await StartInListVisibleAnimation();
                    if (m_StartCancelToken != null && m_StartCancelToken.IsCancellationRequested)
                    {
                        return;
                    }
                    DoCanvasGroupShow(BindList!=null && BindList.EnableVisibleAnimation ?BindList.CanvasGroupFadeDuration:0);
                    ShowCompleteCb();
                }
                catch (OperationCanceledException)
                {
                   
                }
               
            }
            else
            {
                DisplayGO.SetActive(true);
                ShowCompleteCb();
            }
        }
        private void DoCanvasGroupShow(float duration =0f)
        {
            if (CanvasGroup == null || CanvasGroup.alpha>=1) return;
            //CanvasGroup.alpha = 1;
            if (duration <= 0)
            {
                CanvasGroup.alpha = 1;
            }
            else
            {
                CanvasGroup.DOFade(1, duration);
            }
            CanvasGroup.blocksRaycasts = true;
        }
        private void DoCanvasGroupHide()
        {
            if (CanvasGroup == null) return;
            CanvasGroup.DOKill();
            CanvasGroup.alpha = 0;
            CanvasGroup.blocksRaycasts = false;
        }
        private async UniTask StartInListVisibleAnimation()
        {
            if (CanvasGroup == null || BindList == null ||!BindList.EnableVisibleAnimation) return ;
            m_StartCancelToken = new CancellationTokenSource();
            int  inVisibleIndex = GetInVisibleSetIndex();
            if (inVisibleIndex == -1) return;
            int count = 0;
            for (int i = 0; i < inVisibleIndex; i++)
            {
                int visibleIndex = BindList.GetVisibleIndexAt(i);
                var item = BindList.GetVisibleItem(visibleIndex);
                if(item==null || item.IsShowEffectIng)
                {
                    count++;
                }
            }
            float timer = BindList.VisibleAnimationDuration * count;
            await UniTask.Delay(TimeSpan.FromSeconds(timer),cancellationToken: m_StartCancelToken.Token);
        }
    }
}
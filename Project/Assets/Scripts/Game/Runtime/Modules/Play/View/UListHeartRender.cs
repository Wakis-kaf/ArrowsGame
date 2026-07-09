using Framework.Runtime.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.MAsset;
using Framework.Runtime.Config;
using System.Data.Common;
using Game.Modules.GModuleArrows;
using System;
using DG.Tweening;
namespace Game.Modules
{
    public class UListHeartRender : UListDisplayUnit
    {
        #region PrefabBinder 自动引用区域 开始
        private Framework.Runtime.UI.USprite uspHeartGray;
        private Framework.Runtime.UI.USprite uspHeart;

        #endregion PrefabBinder 自动引用区域 结束
        private readonly Color m_AliveColor = UIUtil.Hex2Color("#FF0000");
        private readonly Color m_DeathColor = UIUtil.Hex2Color("#808080");
        protected override void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
        {
            this.uspHeartGray = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspHeartGray");
            this.uspHeart = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspHeart");

        }
        public override int GetOpenLayer(int externalLayer)
        {
            return externalLayer;

        }
        public override string GetAssetLink(string outAssetLink)
        {
            string assetPath = "Assets/AddressableResources/UI/Play/Prefabs/Renders/UListHeartRender.prefab";
            return AssetPathEncoder.EncodeEnvAssetLink(assetPath, AssetType.PrefabAsset);

        }

        /// <summary>
        /// 子类重写，构造函数中调用
        /// </summary>
        protected override void OnInit()
        {

        }
        /// <summary>
        /// 显示对象初始化UI,当绑定的预制体加载完成后回调(子类重写)
        /// </summary>
        protected override void OnInitUI()
        {

        }
        /// <summary>
        /// 注册页面消息，次于 OnInitUI 之后执行
        /// </summary>
        protected override void OnSubscribeMessages()
        {

        }
        /// <summary>
        /// 当绑定UI加载完成且UI显示回调(子类重写)
        /// </summary>
        protected override void OnShow()
        {

        }
        /// <summary>
        /// 当绑定UI加载完成且UI隐藏回调(子类重写)    
        /// </summary>
        protected override void OnHide()
        {

        }
        /// <summary>
        /// 当绑定UI加载完成且数据更新的时候调用(子类重写)
        /// </summary>
        /// <param name="data"></param>

        private LevelHeartVO m_BindHeartVO;
        // private LevelHeartVOStatus m_Status;
        protected override void OnGUI(object data)
        {
            if (data is LevelHeartVO heartVO)
            {
                m_BindHeartVO = heartVO;
                heartVO.BindStatusChanged(OnBindHeartStatusChanged);
            }
        }

        private void OnBindHeartStatusChanged(LevelHeartVOStatus status)
        {
            // if (m_Status == status) return;
            // m_Status = status;
            if (status == LevelHeartVOStatus.Alive)
            {
                PlayHeartAliveAnim();
            }
            else if (status == LevelHeartVOStatus.Death)
            {
                PlayHeartDeathAnim();
            }
        }
        private void PlayHeartAliveAnim()
        {
            if (!m_BindHeartVO.IsAnim)
            {
                SetActive(uspHeartGray, false);
                SetActive(uspHeart, true);
                uspHeart.rectTransform.localScale = Vector3.one;
                uspHeart.color = m_AliveColor;
                return;
            }
            SetActive(uspHeartGray, false);
            SetActive(uspHeart, true);
            int order = m_BindHeartVO.order;
            float delay = 0.3f + order * 0.4f;
            uspHeart.rectTransform.DOKill();
            uspHeart.DOKill();

            uspHeart.rectTransform.localScale = Vector3.one * 1.5f;
            uspHeart.color = new Color(m_AliveColor.r, m_AliveColor.g, m_AliveColor.b, 0f);
            uspHeart.gameObject.SetActive(true);

            uspHeart.rectTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutQuad).SetDelay(delay);
            uspHeart.DOFade(1f, 0.5f).SetEase(Ease.Linear).SetDelay(delay);
        }
        private void PlayHeartDeathAnim(int jumpCount = 3)
        {
            if (!m_BindHeartVO.IsAnim)
            {
                SetActive(uspHeartGray, false);
                SetActive(uspHeart, true);
                int order = m_BindHeartVO.order;
                float delay = 0.3f + order * 0.4f;
                uspHeart.rectTransform.DOKill();
                uspHeart.DOKill();

                uspHeart.rectTransform.localScale = Vector3.one * 1.5f;
                uspHeart.color = new Color(m_DeathColor.r, m_DeathColor.g, m_DeathColor.b, 0f);
                uspHeart.gameObject.SetActive(true);
                uspHeart.rectTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutQuad).SetDelay(delay);
                uspHeart.DOFade(1f, 0.5f).SetEase(Ease.Linear).SetDelay(delay);
                return;
            }
            uspHeartGray.rectTransform.DOKill();
            uspHeartGray.DOKill();
            SetActive(uspHeartGray, false);
            SetActive(uspHeart, true);
            uspHeartGray.rectTransform.localScale = Vector3.one;
            uspHeartGray.color = m_DeathColor;
            int vibrato = jumpCount * 2;
            uspHeart.rectTransform.DOPunchScale(new Vector3(0.4f, 0.4f, 0f), 0.6f, vibrato, 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    SetActive(uspHeartGray, true);
                    SetActive(uspHeart, false);
                });
        }

        /// <summary>
        /// 当绑定UI被删除时回调(子类重写)
        /// </summary>
        public override void OnDestroy()
        {

        }
    }

}







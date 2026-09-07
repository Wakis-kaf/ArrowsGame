using System;
using Game.Modules.GModuleInventory;
using Game.Modules.GModuleProgression;
using Framework.Runtime;
using Framework.Runtime.Modules.UI.PrefabBind;
using UnityEngine;
namespace Game.Modules
{
    public class PropMonitor : MonoBehaviour
    {
        #region PrefabBinder 自动引用区域 开始
        private PrefabBinder m_EntityPrefabBinder;
        public PrefabBinder EntityPrefabBinder
        {
            get
            {
                if (m_EntityPrefabBinder == null)
                {
                    m_EntityPrefabBinder = gameObject.GetComponent<PrefabBinder>() ?? gameObject.AddComponent<PrefabBinder>();
                }
                return m_EntityPrefabBinder;
            }
        }
        private Framework.Runtime.UI.UTMPText utmpTxtRecovTime => EntityPrefabBinder?.GetObj<Framework.Runtime.UI.UTMPText>("utmpTxtRecovTime");
        private Framework.Runtime.UI.UButton ubtnAdd => EntityPrefabBinder?.GetObj<Framework.Runtime.UI.UButton>("ubtnAdd");
        private Framework.Runtime.UI.UTMPText utmpTxtNum => EntityPrefabBinder?.GetObj<Framework.Runtime.UI.UTMPText>("utmpTxtNum");
        private Framework.Runtime.UI.USprite uspIcon => EntityPrefabBinder?.GetObj<Framework.Runtime.UI.USprite>("uspIcon");
        private Framework.Runtime.UI.USprite uspBg => EntityPrefabBinder?.GetObj<Framework.Runtime.UI.USprite>("uspBg");

        #endregion PrefabBinder 自动引用区域 结束

        public int propId;
        public bool showUpLimit; // 如果开启后显示 为 xx/xx 比如体力 3/5
        public bool showAddBtn;

        private CfgItemInfo m_Config;
        private float m_NextRefresh;

        private void OnEnable()
        {
            MessageDispatcher.Ins.Subscribe(MessageCode.msg_on_inventory_changed, Refresh);
            MessageDispatcher.Ins.Subscribe(MessageCode.msg_on_mainArchiveLoaded, Refresh);
            Refresh();
        }
        private void OnDisable()
        {
            MessageDispatcher.Ins.Unsubscribe(MessageCode.msg_on_inventory_changed, Refresh);
            MessageDispatcher.Ins.Unsubscribe(MessageCode.msg_on_mainArchiveLoaded, Refresh);
        }
        private void Update()
        {
            if (Time.unscaledTime >= m_NextRefresh) { m_NextRefresh = Time.unscaledTime + 1f; Refresh(); }
        }
        private void Refresh()
        {
            if (propId <= 0) return;
            m_Config = GameInventoryDataHandler.Ins.GetItemInfoCfg(propId);
            if (m_Config == null) return;
            int count = GameInventoryDataHandler.Ins.GetItemHasCount(propId);
            int limit = GameInventoryDataHandler.Ins.GetItemHoldMaxCount(m_Config);
            if (utmpTxtNum != null) utmpTxtNum.text = showUpLimit ? string.Format("{0}/{1}", count, limit) : count.ToString();
            if (uspIcon != null)
            {
                uspIcon.Path = m_Config.iconSpritePath;
            }
            RefreshRecovery();
        }
        private void RefreshRecovery()
        {
            if (utmpTxtRecovTime == null) return;
            long recoverSeconds = m_Config == null ? 0 : m_Config.recoverySeconds;
            if (recoverSeconds <= 0) { utmpTxtRecovTime.text = string.Empty; return; }
            long next = GameProgressionService.GetItemRecoveryTime(propId);
            long remain = Math.Max(0, next - DateTimeOffset.Now.ToUnixTimeSeconds());
            utmpTxtRecovTime.text = remain <= 0 ? string.Empty : string.Format("{0:D2}:{1:D2}", remain / 60, remain % 60);
        }

    }
}




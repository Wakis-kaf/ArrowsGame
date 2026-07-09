using CustomLitJson.Extensions;
using Framework.Runtime;
using Framework.Runtime.Archives;
using Game.Modules.GModuleInventory;
using Game.Modules.GModulePlayer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleManage
{
    public class GameArchive : Archive
    {
        [JsonIgnore]
        public static GameArchive Main => GameManageClientHandler.Ins?.GameMainArchive;

        [JsonSerializer]
        private ArchiveInventory m_InvArc;
        [JsonSerializer]
        private ArchiveRole m_RoleArc;
        [JsonSerializer]
        private ArchiveLevel m_LevelArc;
        [JsonSerializer]
        private ArchiveGuide m_GuideArc;
        [JsonIgnore]
        public ArchiveInventory InventoryArchive => m_InvArc;
        [JsonIgnore]
        public ArchiveRole RoleArchive => m_RoleArc;
        [JsonIgnore]
        public ArchiveLevel LevelArchive => m_LevelArc;
        [JsonIgnore]
        public ArchiveGuide GuideArchive => m_GuideArc;
        [JsonIgnore]
        public bool IsNewCreateArchive { get; internal set; }

        public void InitArchive()
        {
            m_RoleArc = new ArchiveRole();
            m_RoleArc.OwnArchive = this;
            m_RoleArc.OnInitArchive();

            m_InvArc = new ArchiveInventory();
            m_InvArc.OwnArchive = this;


            m_LevelArc = new ArchiveLevel();
            m_LevelArc.OwnArchive = this;

            m_GuideArc = new ArchiveGuide();
            m_GuideArc.OwnArchive = this;

            MarkDirty();
        }
        public override void OnDrityUpdate()
        {
            base.OnDrityUpdate();
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_mainArchive_dirty);
        }

        public void LoadFromArchive()
        {
            m_RoleArc.OwnArchive = this;
            m_RoleArc.OnLoadFromArchive();

            m_InvArc.OwnArchive = this;
            m_InvArc.OnLoadFromArchive();

            m_LevelArc.OwnArchive = this;
            m_LevelArc.OnLoadFromArchive();

            m_GuideArc.OwnArchive = this;
            m_GuideArc.OnLoadFromArchive();
        }
    }
}
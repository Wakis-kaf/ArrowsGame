using CustomLitJson.Extensions;
using Framework.Runtime.Module;
using Framework.Utils;

using System;
using UnityEngine;

namespace Framework.Runtime.Archives
{
    /// <summary>
    /// 存档文件 , 每一份存档都会单独序列化为一个文件夹保存到目标路径下
    /// </summary>
    [Serializable]
    public abstract class Archive
    {
        [SerializeField, JsonSerializer] protected DateTime m_CretTime;
        [SerializeField, JsonSerializer] protected string m_Id;
        [SerializeField, JsonSerializer] protected string m_Name;

        // 存档创建时间
        [SerializeField, JsonSerializer] protected DateTime m_UpdTime;

        [SerializeField, JsonSerializer] protected string m_Ver;
        // 存档上次更新时间

        private Type m_Type;
        internal bool isWriting;

        public Archive()
        {
            m_Id = Utility.IDGenerator.GetStrGuidID(); // UUID  生成独一无二的UU ID
            m_UpdTime = m_CretTime = DateTime.Now;
            this.IsDirty = true;
        }
        public void InitInfo(string version, string name)
        {
            this.m_Ver = version;
            this.m_Name = name;
        }


        [JsonIgnore] public virtual int ArchiveType => ArchiveTypeCode.GameArchive;
        [JsonIgnore] public DateTime CreateTime => m_CretTime;
        [JsonIgnore] public string Id => m_Id;

        [JsonIgnore]
        public bool IsDirty { get; set; } = true;

        [JsonIgnore] public string Name => m_Name;

        [JsonIgnore]
        public bool SaveWaiting { get; set; } = false;

        [JsonIgnore]
        public Type Type
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

        // 存档名字 存档创建时间
        [JsonIgnore] public DateTime UpdateTime => m_UpdTime;

        [JsonIgnore] public string Version => m_Ver; // 存档版本
        public virtual string GetArchiveFileName()
        {
            return string.IsNullOrEmpty(m_Name) ? "Archive" : m_Name;
        }

        /// <summary>
        /// 存档被反序列化之后调用
        /// </summary>
        public virtual void OnAfterDeSerialize()
        {
        }

        /// <summary>
        /// 存档被序列化之后调用
        /// </summary>
        /// <param name="res">是否序列化成功</param>
        public virtual void OnAfterSerialize(bool res)
        {
        }

        /// <summary>
        /// 存档被序列化之前调用
        /// </summary>
        public virtual void OnBeforeSerialize()
        {
            // 更新保存时间
            m_UpdTime = DateTime.Now;
        }

        // 存档上次更新时间
        public virtual void Save(bool isOverride = true, Action sucCb = null, Action failCb = null, Action beforeSaveCb = null)
        {
            IsDirty = true;
            GameApp.ArchiveModule.SaveArchive(this, true, sucCb, failCb, beforeSaveCb);
        }
        public void MarkDirty()
        {
            IsDirty = true;
            OnDrityUpdate();
        }
        public virtual void OnDrityUpdate()
        {

        }
        public void SetName(string name)
        {
            m_Name = name;
        }
    }
}
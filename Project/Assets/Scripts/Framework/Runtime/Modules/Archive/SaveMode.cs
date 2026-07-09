namespace Framework.Runtime.Archives
{
    public enum SaveDirPath
    {
        PersistencePath,
        DataPath,
        StreamingPath,
        TemporaryCachePath
    }

    public sealed partial class ArchiveModule
    {
        public enum SaveMode
        {
            Json, // Json  化保存
            Binary, // 二进制保存
            Xml, // XML 化保存
        }
    }
}
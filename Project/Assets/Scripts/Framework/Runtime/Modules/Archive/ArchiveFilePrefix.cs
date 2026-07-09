namespace Framework.Runtime.Archives
{
    public sealed partial class ArchiveModule
    {
        private static class ArchiveFilePrefix
        {
            public static string GetPrefix(SaveMode saveMode)
            {
                switch (saveMode)
                {
                    default:
                    case SaveMode.Binary:
                        return ".bytes";

                    case SaveMode.Json:
                        return ".json";

                    case SaveMode.Xml:
                        return ".xml";
                }
            }
        }
    }
}
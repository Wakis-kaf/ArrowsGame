namespace Framework.Runtime.Archives
{
    public sealed partial class ArchiveModule
    {
        private static class ArchiverSerializerFactory
        {
            public static IArchiveSerializer CreatSerializer(SaveMode saveMode)
            {
                switch (saveMode)
                {
                    case SaveMode.Binary:
                        return new BinaryArchiveSerializer();

                    case SaveMode.Json:
                        return new JsonArchiveSerializer();

                    case SaveMode.Xml:
                        break;
                }

                return default;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Framework.Runtime.Archives.ArchiveModule;

namespace Framework.Runtime.Archives
{
    public interface ArchiverEntryprorFactory
    {
        public static IArchiveEncryptor CreatEncryptor(SaveMode saveMode)
        {
            switch (saveMode)
            {
                case SaveMode.Binary:
                    return new BinaryArchiveEncryptor();

                case SaveMode.Json:
                    return new JsonArchiveEncryptor();

                case SaveMode.Xml:
                    break;
            }

            return default;
        }
    }
}

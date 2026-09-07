using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFXlsx.ConfigJsonTemplate;
using UFXlsx.Main;

namespace UFXlsx.Decoder
{
    public interface IDecoder
    {
        WriteData[] DecodeExcel(ExportChannelConfigJT chanelExportData);
    }

    public class WriteData
    {
        public byte[] buffer;
        public string exportFullPath;
        public string content;
        public List<WriteData> extendsWrites = new List<WriteData>();
    }
}
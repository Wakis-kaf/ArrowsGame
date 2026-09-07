using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFXlsx.Decoder.CSharp;
using UFXlsx.Decoder.Json;
using UFXlsx.Decoder.Lua;
using UFXlsx.Decoder.TypeScript;
using UFXlsx.ExcelReader;
using UFXlsx.Main;

namespace UFXlsx.Decoder
{
    public class DecoderFactory : Single<DecoderFactory>
    {
        public IDecoder GetDecoder(string outputType)
        {
            Enum.TryParse(typeof(ExportType), outputType, true, out object exportType);
            if (exportType == null)
            {
                ExporterEnvironment.LogError($"未找到类型为 {outputType} 的decoder");
            }
            ExportType export = exportType != null ? (ExportType)(exportType) : ExportType.Json;
            return GetDecoder(export);
        }

        public IDecoder GetDecoder(ExportType outputType)
        {
            if (m_DecoderMap.TryGetValue(outputType, out var decoder))
            {
                return decoder;
            }
            return null;
        }

        public void Init()
        {
            m_DecoderMap = new Dictionary<ExportType, IDecoder>();
            m_DecoderMap.Add(ExportType.Json, new JsonDecoder());
            m_DecoderMap.Add(ExportType.CSharpJson, new CSharpJsonDecoder());
            m_DecoderMap.Add(ExportType.Lua, new LuaDecoder());
            m_DecoderMap.Add(ExportType.TypeScript, new TypeScriptDecoder());
        }

        private Dictionary<ExportType, IDecoder> m_DecoderMap;
    }
}
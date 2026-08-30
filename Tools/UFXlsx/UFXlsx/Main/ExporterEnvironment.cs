using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UFXlsx.FileHandle;
using System.Xml;
using UFXlsx.ConfigJsonTemplate;
using LitJson;

namespace UFXlsx.Main
{
    public enum ExportType
    {
        Json = 1,
        Lua = 2,
        CSharp = 3,
        Xml = 4,
        TypeScript = 5,
        CSharpJson = 6,
    }

    public class ChanelExportData
    {
        public ExcelExportData excelExportData;
        public string exportConfigPath;
        public ExportType exportType;

        public SheetExportData[] sheetExportDatas;

        //public int dataRowBegin;
        //public int keyRow;
        public IWorkbook workBook;
    }

    public class ExcelExportData
    {
        public string[] channelList;
        public int exportType;
        public string filePath;

        public string ToString()
        {
            string content = $"导出相对文件 【{filePath}】\n 导出类型【{exportType}】";
            if (channelList != null)
            {
                for (int i = 0; i < channelList.Length; i++)
                {
                    content += $"输出通道配置文件: 【{channelList[i]}】\n";
                }
            }
            return content;
        }
    }

    public class ExporterEnvironment
    {
        public static Regex pureNumRegex = new Regex("^(-?[0-9]*[.]*[0-9]{0,3})$");

        public enum ExportType
        {
            Global = 0,
            Single = 1
        }

        public static string GetConfigPath(string path)
        {
            return Path.Combine(configDirPath, path);
        }

        public static string GetExcelPath(string path)
        {
            return Path.Combine(excelDir, path);
        }

        public static string GetGlobalConfigPath()
        {
            return Path.Combine(configDirPath, Define.GLOBAL_CONFIG_NAME);
        }

        public static string GetGlobalExportConfigPath()
        {
            return Path.Combine(configDirPath, Define.GLOBAL_EXPORT_CONFIG_NAME);
        }

        public static ExcelExportJT[] GetGlobalExportList()
        {
            return globalExportConfigJT.globalExportList;
        }

        public static string GetOutputPath(string path)
        {
            return Path.Combine(configOutputDir, path);
        }

        public static ExcelExportJT[] GetSingleExportList()
        {
            return globalExportConfigJT.singleExportList;
        }

        public static void Log(string msg)
        {
            m_LogAgent?.Invoke(msg);
        }

        public static void LogError(string msg)
        {
            m_LogErrorAgent?.Invoke(msg);
        }

        public static ExportChannelConfigJT ReadChannelExportConfig(ExportChannelJT chanelExportData)
        {
            Log($"读取通道导出配置文件: 【{chanelExportData.exportConfigPath}】\n");

            ExportChannelConfigJT channelConfigJT = JsonMapper.ToObject<ExportChannelConfigJT>(FileUtil.ReadFileContent(chanelExportData.exportConfigPath));
            channelConfigJT.Init();
            Log($"通道导出配置文件读取结束!\n");
            // 读取配置文件
            return channelConfigJT;
        }

        public static void ReadConfig(string targetDir, JsonData exportItems = null)
        {
            SetDirPath(targetDir);
            ReadGlobalConfig();
            ReadExportConfig(exportItems);
        }

        public static void RemoveLogAgent()
        {
            m_LogAgent = null;
        }

        public static void RemoveLogErrorAgent()
        {
            m_LogErrorAgent = null;
        }

        public static void SetDirPath(string path)
        {
            configDirPath = path;
            Log($"设置配置根路径:【{path}】");
            globalExportList = new List<ExcelExportData>();
            singleExportList = new List<ExcelExportData>();
        }

        public static void SetLogAgent(Action<string> agent)
        {
            m_LogAgent = agent;
        }

        public static void SetLogErrorAgent(Action<string> agent)
        {
            m_LogErrorAgent = agent;
        }

        private static string configDirPath = "";
        private static string configOutputDir = "";
        private static string excelDir = "";
        private static GlobalConfigJT globalConfigJT;
        private static GlobalExportConfigJT globalExportConfigJT;
        private static List<ExcelExportData> globalExportList;
        private static Action<string> m_LogAgent;
        private static Action<string> m_LogErrorAgent;
        private static List<ExcelExportData> singleExportList;
        /*  private static OutputType outputType;
          public static OutputType OutputType => outputType;*/

        private static void ReadExportConfig(JsonData exportItems = null)
        {
            var globalExportConfigPath = GetGlobalExportConfigPath();
            Log($"开始解析导出配置json【{globalExportConfigPath}】");
            if (exportItems != null)
            {
                globalExportConfigJT = JsonMapper.ToObject<GlobalExportConfigJT>(exportItems.ToJson());
            }
            else
            {
                globalExportConfigJT = JsonMapper.ToObject<GlobalExportConfigJT>(FileUtil.ReadFileContent(globalExportConfigPath));
            }
            Log($"解析导出配置json结束");
            // 单独导文件
        }

        private static void ReadGlobalConfig()
        {
            var globalConfigPath = GetGlobalConfigPath();
            Log($"开始读取全局配置:【{globalConfigPath}】");
            globalConfigJT = JsonMapper.ToObject<GlobalConfigJT>(FileUtil.ReadFileContent(globalConfigPath));
            excelDir = globalConfigJT.excelDir;
            configOutputDir = globalConfigJT.outputDir;
            Log($"解析全局配置结束");
        }
    }

    public class LuaChanelExportData : ChanelExportData
    {
        public LuaChanelExportData(ChanelExportData excelChanelExportData)
        {
            excelExportData = excelChanelExportData.excelExportData;
            exportConfigPath = excelChanelExportData.exportConfigPath;
            exportType = excelChanelExportData.exportType;
            //this.dataRowBegin = excelChanelExportData.dataRowBegin;
            //this.keyRow = excelChanelExportData.keyRow;
            workBook = excelChanelExportData.workBook;
            sheetExportDatas = excelChanelExportData.sheetExportDatas;
            if (sheetExportDatas != null)
            {
                for (int i = 0; i < sheetExportDatas.Length; i++)
                {
                    sheetExportDatas[i].ownerChanelExportData = this;
                }
            }
        }
    }

    public class SheetExportData
    {
        public int dataRowBegin;
        public string exportField;
        public string exportFullPath;

        //public bool isArrayExport = true;
        public string exportMapKey = "";

        public string exportPath;
        public int keyRow;
        public ChanelExportData ownerChanelExportData;
        public string sheetName;
        public int typeRow;

        public SheetExportData(ChanelExportData chanelExportData)
        {
            ownerChanelExportData = chanelExportData;
        }
    }
}
//using NPOI.HSSF.UserModel;
//using NPOI.XSSF.UserModel;
using LitJson;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using UFXlsx.ConfigJsonTemplate;
using UFXlsx.Decoder;

namespace UFXlsx.Main
{
    public class ExcelConfigExporter : Single<ExcelConfigExporter>
    {
        public ExportChannelConfigJT currentChannelConfig;

        public void StartOuputAllByConfig(string filePath,JsonData exportItems = null)
        {
            string dir = filePath.Substring(0, filePath.LastIndexOf("\\"));
            StartOutputAll(dir, exportItems);
        }

        public void StartOutputAll(string targetDir,JsonData exportItems = null)
        {
            // 读取配置
            ExporterEnvironment.Log($"开始导出所有单配置: 导出路径【{targetDir}】\n");
            ExporterEnvironment.ReadConfig(targetDir, exportItems);
            DecoderFactory.Instance.Init();
            ExporterDatabase.Instance.Clear();
            // 开始输出
            // 先输出单个,再输出全局
            var singleExportList = ExporterEnvironment.GetSingleExportList();
            for (int i = 0; i < singleExportList.Length; i++)
            {
                var singleExport = singleExportList[i];
                singleExport.excelPath = ExporterEnvironment.GetExcelPath(singleExport.excelPath);
                OutputSingle(singleExport);
            }
        }

        private void OutputSingle(ExcelExportJT excelExportData)
        {
            for (int i = 0; i < excelExportData.exportChannelList.Length; i++)
            {
                var channelConfigPath = excelExportData.exportChannelList[i];
                // 不需要解析,直接输出就行
                OutputSingle(excelExportData, channelConfigPath);
            }
        }

        private void OutputSingle(ExcelExportJT excelExportData, ExportChannelJT channelConfig)
        {
            ExporterEnvironment.Log($"开始导出生成配置: 导出【{excelExportData.excelPath}】导出通道【{channelConfig.exportConfigPath}】 \n");
            channelConfig.exportConfigPath = ExporterEnvironment.GetConfigPath(channelConfig.exportConfigPath);
            var channelExportConfig = ExporterEnvironment.ReadChannelExportConfig(channelConfig);
            TemplateExporter.Instance.SetTemplatePath(ExporterEnvironment.GetConfigPath(channelExportConfig.exportTemplatePath));
            channelExportConfig.fromChannelConfig = channelConfig;
            channelExportConfig.ownerExcelConfig = excelExportData;
            ExporterEnvironment.Log($"开始导出生成配置: 导出:【{excelExportData.excelPath}】 输出目录【{channelConfig.exportConfigPath}】\n");
            OuputChannel(channelExportConfig);
        }

        private void OuputChannel(ExportChannelConfigJT channelConfig)
        {
            currentChannelConfig = channelConfig;
            // 读取 excel 文件
            ExporterEnvironment.Log($"读取Excel: 【{channelConfig.ownerExcelConfig.excelPath}】\n  读取类型 {channelConfig.exportType}");
            //FileInfo info = new FileInfo(channelConfig.ownerExcelConfig.excelPath);
            try
            {
                //using (FileStream fs = info.OpenRead())
                using (FileStream fs = new FileStream(channelConfig.ownerExcelConfig.excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    string extension = channelConfig.ownerExcelConfig.excelPath.Split(".")[1];
                    if (extension.Equals("xls"))
                    {
                        channelConfig.workBook = new HSSFWorkbook(fs);
                    }
                    else if (extension.Equals("xlsx"))
                    {
                        channelConfig.workBook = new XSSFWorkbook(fs);
                    }
                    else
                    {
                        return;
                    }
                    // 解析配置
                    IDecoder decoder = DecoderFactory.Instance.GetDecoder(channelConfig.exportType);
                    // 获取输出路径和输出名字
                    var writesDatas = decoder.DecodeExcel(channelConfig);
                    for (int i = 0; i < writesDatas.Length; i++)
                    {
                        // 写入文件
                        File.WriteAllBytes(writesDatas[i].exportFullPath, writesDatas[i].buffer);
                    }
                    currentChannelConfig = null;
                }
            }
            catch (Exception ex)
            {
                ExporterEnvironment.LogError($"读取Excel失败: 【{ex} 】\n");
                currentChannelConfig = null;
                throw;
            }
        }
    }
}
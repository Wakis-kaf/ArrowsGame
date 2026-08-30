using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFXlsx.Main;

namespace UFXlsx.ConfigJsonTemplate
{
    public class ExportChannelConfigJT
    {
        public string exportType;
        public int exportFormatType = 1;
        public string exportTemplatePath ="";
        public ExportSheetJT[] exportSheetList = Array.Empty<ExportSheetJT>();

        public ExportChannelJT fromChannelConfig { get;  set; }
        public ExcelExportJT ownerExcelConfig { get; internal set; }
        public IWorkbook workBook { get; internal set; }
        public void Init()
        {
            for (int i = 0; i < exportSheetList.Length; i++)
            {
                exportSheetList[i].exportFullPath = ExporterEnvironment.GetOutputPath(exportSheetList[i].exportPath);
            }
        }
    }
}

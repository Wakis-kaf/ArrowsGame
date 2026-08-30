using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFXlsx.ConfigJsonTemplate
{
    public class ExportSheetJT
    {
        public int keyRow = -1;
        public int typeRow = -1;
        public int dataRowBegin = -1;
        public int[] commentRowList = Array.Empty<int>();
        //public bool isArrayExport;
        //public bool isIgnoreMapExportKey = false;
        //public string mapExportKey = "";
        public string sheetName;
        public string exportField;
        public string exportPath;
        public string exportFullPath;

        //public bool isMultiRow = false;
        //public bool isMultiRowGroup = false;
        //public int multiRowMission = 1;
        //public string multiRowOutField = "multiRowData";
        public ExportSheetGroupJT[] exportGroupList = Array.Empty<ExportSheetGroupJT>();
        public LuaExportConfigJT luaExportConfig = null;
        public TypeScriptExportConfigJT typeScriptExportConfig = null;
        public ExportChannelConfigJT ownerChannelConfig { get; internal set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFXlsx.Main
{
    public enum ExcelValueType
    {
        Float,
        Double,
        Boolean,
        Int,
        LongInt,
        String,
        Object,
        DoubleArray,
        DoubleArray2D,
        StringArray,
        StringArray2D,
        IntArray,
        IntArray2D,
        MultiColMap, // 多列多维Map
        MultiColArray, // 多列多维数组
        MultiRowArray, // 多行多维数组
        MultiRowMap, // 多行多维数组
        ForeignKey // foreignKey:: sheet.[sheetName].[key] or global.[excelName].[sheetName].[key]
    }

    public static class SpecicalTypePrefix
    {
        public const string objectPrefix = "<__SPT_OBJECT>";
        public const string stringArrayPrefix = "<__SPT_STRINGARRAY>";
        public const string stringArray2DPrefix = "<__SPT_STRINGARRAY2D>";
        public const string doubleArrayPrefix = "<__SPT_DOUBLEARRAY>";
        public const string doubleArray2DPrefix = "<__SPT_DOUBLEARRAY2D>";
        public const string intArrayPrefix = "<__SPT_INTARRAY>";
        public const string intArray2DPrefix = "<__SPT_INTARRAY2D>";
    }

    // 定义一些关键字和特殊字符
    public static class Define
    {
        public const string GLOBAL_CONFIG_NAME = "GlobalConfig.json"; // 全局配置菜单xml
        public const string GLOBAL_EXPORT_CONFIG_NAME = "GlobalExportConfig.json"; // 文件导出配置菜单
    }

    public static class ExportTemplateKeyWord
    {
        public const string CONTENT = "CONTENT"; // 内容
        public const string COMMENT = "COMMENT"; // 注释
        public const string EXPORTFILENAME = "EXPORTFILENAME"; // 导出的文件名
        public const string TABLENAME = "TABLENAME"; // 导出的文件名
        public const string DECLARE = "DECLARE"; // 导出的文件名
        public const string DECLARENAME = "DECLARENAME"; // 导出的文件名
    }

    public static class SpecailStr
    {
        public const string SPLIT = "#";
        public const string MULTI_TYPE_ROW_MISSION = "MULTI_TYPE_ROW_MISSION";
        public const string MULTI_TYPE_DIMENSION = "MULTI_TYPE_DIMENSION";
        public const string MULTI_TYPE_RANGELENGTH = "MULTI_TYPE_RANGELENGTH";

        public const string MULTI_TYPE_FIELD = "MULTI_TYPE_FIELD";
        public const string MULTI_TYPE_MAPKEY = "MULTI_TYPE_MAPKEY";

        public const string MULTI_TYPE_COLUMN_START = "MULTI_TYPE_COLUMN_START";
        public const string MULTI_TYPE_ITEM_ARRAY_OUT = "MULTI_TYPE_ITEM_ARRAY_OUT";
        public const string MULTI_TYPE_ITEM_ARRAY_Merge = "MULTI_TYPE_ITEM_ARRAY_Merge";
    }
}
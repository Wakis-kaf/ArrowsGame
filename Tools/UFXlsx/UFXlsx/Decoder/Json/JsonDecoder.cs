using LitJson;
using Microsoft.VisualBasic;
using NPOI.SS.UserModel;
//using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using UFXlsx.ConfigJsonTemplate;
using UFXlsx.Decoder;
using UFXlsx.Main;
using ExporterEnvironment = UFXlsx.Main.ExporterEnvironment;

namespace UFXlsx.Decoder.Json
{
    public class CellExtendArgs
    {
        public string[] args;
        public CellExtendArgs childExtend;
        public ColumnData ColumnData;
        public ExcelValueType excelValueType;
        public CellExtendArgs parentExtend;

        public void AddSubExtend(CellExtendArgs groupArg)
        {
            if (childExtend == null)
            {
                childExtend = groupArg;
                groupArg.SetParentGroup(this);
            }
            else
            {
                childExtend.AddSubExtend(groupArg);
            }
        }

        public T GetData<T>(string key)
        {
            if (m_Datas.TryGetValue(key, out var data))
                return (T)data;
            return default;
        }

        public void SetData(string key, object data)
        {
            if (m_Datas.ContainsKey(key)) m_Datas[key] = data;
            else m_Datas.Add(key, data);
        }

        public void SetParentGroup(CellExtendArgs groupArg)
        {
            if (groupArg != null)
            {
                this.parentExtend = groupArg;
            }
        }

        private Dictionary<string, object> m_Datas = new Dictionary<string, object>();
    }

    public class ColumnData
    {
        public ValueGroup BelongGroup { get; private set; }
        public int ColumnIndex { get; private set; }
        public ExcelValueType ExcelValueType { get; private set; }
        public CellExtendArgs ExtendArgs { get; private set; }
        public bool HasExtend { get => ExtendArgs != null; }
        public string Key { get; private set; }

        public void AddExtendArgs(CellExtendArgs extendArgs)
        {
            if (this.HasExtend)
            {
                this.ExtendArgs.AddSubExtend(extendArgs);
            }
            else
            {
                this.ExtendArgs = extendArgs;
            }
        }

        public int GetGroupWithNameSubCount(string groupName)
        {
            if (IsCurrentGroupSubFromGroupWithName(groupName, out ValueGroup findGroup))
            {
                return findGroup.GetSubGroupCount();
            }
            return 0;
        }

        public bool IsCurrentGroupSubFromGroupWithName(string groupName, out ValueGroup findGroup)
        {
            findGroup = default;
            if (BelongGroup == null) return false;
            ValueGroup group = BelongGroup;
            while (group != null)
            {
                findGroup = group;
                if (group.FieldName == groupName) return true;
                group = group.ParenValueGroup;
            }
            return false;
        }

        public void SetBelongGroup(ValueGroup group, string parentName = "")
        {
            if (BelongGroup == null)
            {
                BelongGroup = group;
            }
            else
            {
                if (IsCurrentGroupSubFromGroupWithName(parentName, out var parent))
                {
                    parent.AddSubValueGroup(group);
                    BelongGroup = group;
                }
                else
                {
                    BelongGroup.AddSubValueGroup(group);
                    BelongGroup = group;
                }
            }
        }

        public void SetCellType(ExcelValueType cellType)
        {
            this.ExcelValueType = cellType;
        }

        public void SetColumnIndex(int columnIndex)
        {
            ColumnIndex = columnIndex;
        }

        public void SetKey(string key)
        {
            Key = key;
        }
    }

    public class CommentData
    {
        public Dictionary<string, Dictionary<string, List<string>>> m_Sheet2KeyWordComment;

        public CommentData()
        {
            m_Sheet2KeyWordComment = new Dictionary<string, Dictionary<string, List<string>>>();
        }

        public void AddComment(string sheetName, string commentTitle, string comment)
        {
            if (!m_Sheet2KeyWordComment.TryGetValue(sheetName, out var keyWordComments))
            {
                m_Sheet2KeyWordComment.Add(sheetName, new Dictionary<string, List<string>> { { commentTitle, new List<string> { comment } } });
            }
            else
            {
                if (!keyWordComments.TryGetValue(commentTitle, out var comments))
                {
                    keyWordComments.Add(commentTitle, new List<string> { comment });
                }
                else
                {
                    comments.Add(comment);
                }
            }
        }

        public string GetOneLineStr(string content)
        {
            content = content.Replace("\r\n", "; ").Replace("\n", "; ");
            return content;
        }

        public string GetString(bool outCommentInOneLine = true)
        {
            StringBuilder commentSB = new StringBuilder();
            var commentSheets = m_Sheet2KeyWordComment.Keys;
            for (int i = 0; i < commentSheets.Count; i++)
            {
                var commentSheetTitle = commentSheets.ElementAt(i);
                commentSB.AppendLine($"*****************{commentSheetTitle}: *********** 开始 ***********");
                var keyWordDict = m_Sheet2KeyWordComment[commentSheetTitle];
                var keyWordsKeys = keyWordDict.Keys;
                for (int j = 0; j < keyWordsKeys.Count; j++)
                {
                    var keyWordKey = keyWordsKeys.ElementAt(j);
                    var commentList = keyWordDict[keyWordKey];
                    for (int k = 0; k < commentList.Count; k++)
                    {
                        var comment = outCommentInOneLine ? GetOneLineStr(commentList[k]) : commentList[k];
                        if (!string.IsNullOrEmpty(comment))
                            commentSB.AppendLine($" //注释: {comment}");
                    }
                    commentSB.AppendLine($" {keyWordKey},\n");
                }
                commentSB.AppendLine($"*****************{commentSheetTitle}: *********** 结束 ***********");
            }
            return commentSB.ToString();
        }
    }
    public class CellData
    {
        public ICell cell;
        public IRow Row;
        public string StringCellValue;
        public ISheet sheet;
        public ExportSheetJT sheetExportData;
        public bool isEmptyRow;
    }
    public class JsonDecoder : IDecoder
    {
       
        public JsonDecoder()
        {
            m_File2Content = new Dictionary<string, JsonData>();
            m_KeyWordComment = new Dictionary<string, CommentData>();
        }

        public ValueGroup GlobalValueGroup => m_GlobalValueGroup;

        public static string JsonUTF8toUnicode(string jsonStr)
        {
            Regex reg = new Regex(@"(?i)\\[uU]([0-9a-f]{4})");
            var str = reg.Replace(jsonStr, delegate (Match m) { return ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString(); });
            return str;
        }

        public void BeginDecode(ExportSheetJT sheetExportData)
        {
            m_GroupedSet.Clear();
            columnIndex2Data.Clear();
        }

        public virtual WriteData[] DecodeExcel(ExportChannelConfigJT chanelExportData)
        {
            DecodeInit();
            DecodeExcelToJson(chanelExportData);
            return GetWriteData();
        }

        public void DecodeInit()
        {
            m_File2Content.Clear();
            m_KeyWordComment.Clear();
        }

        public JsonData DecodeSheet(ExportSheetJT sheetExportData, ISheet sheet)
        {
            BeginDecode(sheetExportData);

            // 读取key row
            ReadTableKey(sheetExportData, sheet);

            // 读取 type row
            ReadTableDataType(sheetExportData, sheet);

            // 读取注释
            ReadTableComment(sheetExportData, sheet);

            // 读取数据
            ReadTableData(sheetExportData, sheet);

            return EndDecode(sheetExportData, sheet);
        }

        public JsonData EndDecode(ExportSheetJT sheetExportData, ISheet sheet)
        {
            string sheetField = sheetExportData.exportField;
            if (GlobalValueGroup.TryFindChildGroupByName(sheetField, out var findGroup))
            {
                return findGroup.ConcatJsonData();
            }
            return null;
        }

        public ColumnData GetColumnData(int columnIndex)
        {
            if (columnIndex2Data.TryGetValue(columnIndex, out var columnData))
            {
                return columnData;
            }
            columnIndex2Data.Add(columnIndex, new ColumnData());
            return columnIndex2Data[columnIndex];
        }

        public string GetColumnKey(int column)
        {
            ColumnData columnData = GetColumnData(column);
            return columnData.Key;
        }

        public bool IsNormalType(ExcelValueType excelValueType)
        {
            if (excelValueType != ExcelValueType.MultiColMap &&
                excelValueType != ExcelValueType.MultiColArray &&
                excelValueType != ExcelValueType.MultiRowArray &&
                excelValueType != ExcelValueType.MultiRowMap &&
                excelValueType != ExcelValueType.ForeignKey
                )
            {
                return true;
            }
            return false;
        }

        public bool ReadTableKey(ExportSheetJT sheetExportData, ISheet sheet)
        {
            var keyRow = sheet.GetRow(sheetExportData.keyRow - 1);
            if (keyRow == null)
            {
                ExporterEnvironment.LogError("缺少字段定义行 keyRow 关键行! 请检查Excel 表格 或者 导出_config.xml 是否配置keyRow?");
                return false;
            }
            for (int j = 0; j < keyRow.LastCellNum; j++)
            {
                var cell = keyRow.GetCell(j);
                string key = string.Empty;
                if (cell != null)
                {
                    cell.SetCellType(CellType.String);
                    key = cell.StringCellValue;
                }
                else
                {
                    key = j.ToString();
                }
                ColumnData columnData = GetColumnData(j);
                columnData.SetKey(key);
                columnData.SetColumnIndex(j);
            }
            return true;
        }

        public void RecordData(JsonData target, JsonData value, string mapField)
        {
            target[mapField] = value;
        }

        public string UpdateColumnType(int columnIndex, string originTypeStr)
        {
            string[] args = originTypeStr.Split(SpecailStr.SPLIT);
            string key = args[0];
            ExcelValueType rootType = DecodeExcelValueType(key);
            if (IsNormalType(rootType))
            {
                return key;
            }
            else
            {
                return UpdateSpecialColumnType(columnIndex, originTypeStr);
            }
        }

        protected Dictionary<string, JsonData> m_File2Content;
        protected Dictionary<string, CommentData> m_KeyWordComment;

        protected void DecodeExcelToJson(ExportChannelConfigJT chanelExportData)
        {
            // 将配置文件转为json
            m_File2Content.Clear();
            m_GlobalValueGroup = new ValueGroup();
            m_GlobalValueGroup.IsArray = false;
            for (int i = 0; i < chanelExportData.exportSheetList.Length; i++)
            {
                var sheetExportData = chanelExportData.exportSheetList[i];
                sheetExportData.ownerChannelConfig = chanelExportData;
                var sheet = chanelExportData.workBook.GetSheet(sheetExportData.sheetName);
                ExporterEnvironment.Log($"读取sheet【{sheetExportData.sheetName}】读取结果为空 ？{sheet == null}");
                AppentData(sheetExportData.exportFullPath, sheetExportData.exportField, DecodeSheet(sheetExportData, sheet));
            }
        }

        protected virtual string OutComment(string exportFullPath)
        {
            string content = "";
            if (m_KeyWordComment.TryGetValue(exportFullPath, out var commentData))
            {
                content = commentData.GetString();
            }
            return $"--[[\n {content} \n --]]\n\n";
        }

        private Dictionary<int, ColumnData> columnIndex2Data = new Dictionary<int, ColumnData>();
        private ValueGroup m_GlobalValueGroup;
        private HashSet<int> m_GroupedSet = new HashSet<int>();

        private void AppentData(string fileName, string fieldName, JsonData content)
        {
            if (!m_File2Content.ContainsKey(fileName))
            {
                m_File2Content.Add(fileName, new JsonData());
            }
            m_File2Content[fileName][fieldName] = content;
        }

        private ExcelValueType DecodeExcelValueType(string valueStr)
        {
            if (!Enum.TryParse(typeof(ExcelValueType), valueStr, true, out object cellTypeObj))
            {
                return ExcelValueType.String;
            }
            else
            {
                return (ExcelValueType)cellTypeObj;
            }
        }

        private ExcelValueType GetCellTypeAt(int column)
        {
            ColumnData columnData = GetColumnData(column);
            return columnData.ExcelValueType;
        }

        private int[] GetColumnIndexs()
        {
            return this.columnIndex2Data.Keys.ToArray();
        }

        private int GetKeyCount()
        {
            return GetColumnIndexs().Length;
        }

        private WriteData[] GetWriteData()
        {
            List<WriteData> writeDatas = new List<WriteData>();
            foreach (var path in m_File2Content.Keys)
            {
                if (!m_File2Content.ContainsKey(path) || m_File2Content[path] == null)
                {
                    ExporterEnvironment.Log($"读取sheet{path} 失败，请检查表 ？");
                    continue;
                }
                string content = m_File2Content[path].ToJson();
                // 解码 Unicode 转义序列
                content = System.Text.RegularExpressions.Regex.Unescape(content);
                writeDatas.Add(new WriteData()
                {
                    exportFullPath = path,
                    buffer = Encoding.UTF8.GetBytes(content),
                    content = content
                });
            }
            ;
            return writeDatas.ToArray();
        }

        private void HandleMultiColArray(CellExtendArgs cellExtendArgs, CellData cell)
        {
            ColumnData columnData = cellExtendArgs.ColumnData;
            string fieldName = cellExtendArgs.GetData<string>(SpecailStr.MULTI_TYPE_FIELD);
            int startColumnIndex = cellExtendArgs.GetData<int>(SpecailStr.MULTI_TYPE_COLUMN_START);
            int rangeLength = cellExtendArgs.GetData<int>(SpecailStr.MULTI_TYPE_RANGELENGTH);
            int dimension = cellExtendArgs.GetData<int>(SpecailStr.MULTI_TYPE_DIMENSION);
            if (rangeLength == -1)
            {
                rangeLength = GetKeyCount() - startColumnIndex;
            }
            int endColumnIndex = startColumnIndex + rangeLength;
            bool itemArray = cellExtendArgs.GetData<bool>(SpecailStr.MULTI_TYPE_ITEM_ARRAY_OUT);
            bool isMerge = cellExtendArgs.GetData<bool>(SpecailStr.MULTI_TYPE_ITEM_ARRAY_Merge);
            if (!columnData.IsCurrentGroupSubFromGroupWithName(fieldName, out var findGroup))
            {
                ValueGroup valueGroup = new ValueGroup();
                valueGroup.IsArray = true;
                valueGroup.FromCell = cell;
                valueGroup.IsSheetGroup = fieldName == cell.sheetExportData.exportField;
                valueGroup.SetFieldName(fieldName);
                if (isMerge)
                {
                    valueGroup.enableCheckWrite = false;
                }
                for (int inAreaColumnIndex = startColumnIndex; inAreaColumnIndex < endColumnIndex; inAreaColumnIndex++)
                {
                    ColumnData inAreaColumnData = GetColumnData(inAreaColumnIndex);
                    inAreaColumnData.SetBelongGroup(valueGroup);
                }
            }
            if (isMerge)
            {
                return;
            }
            for (int currentColumnIndex = startColumnIndex; currentColumnIndex < startColumnIndex + rangeLength; currentColumnIndex += dimension)
            {
                int currentGroupIndex = (int)(currentColumnIndex - startColumnIndex) / dimension + 1;
                int currentHasGroupCount = columnData.GetGroupWithNameSubCount(fieldName);
                if (currentHasGroupCount < currentGroupIndex)
                {
                    ValueGroup newRowGroup = new ValueGroup();
                    newRowGroup.IsArray = itemArray;
                    newRowGroup.FromCell = cell;
                    newRowGroup.SetFieldName((currentGroupIndex - 1) + "");
                    for (int inAreaColumnIndex = currentColumnIndex; inAreaColumnIndex < currentColumnIndex + dimension; inAreaColumnIndex++)
                    {
                        ColumnData inAreaColumnData = GetColumnData(inAreaColumnIndex);
                        inAreaColumnData.SetBelongGroup(newRowGroup);
                    }
                }
         
            }
          
        }

        private void HandleMultiColMap(CellExtendArgs cellExtendArgs, CellData cell)
        {
            ColumnData columnData = cellExtendArgs.ColumnData;
            string fieldName = cellExtendArgs.GetData<string>(SpecailStr.MULTI_TYPE_FIELD);
            string mapKey = cellExtendArgs.GetData<string>(SpecailStr.MULTI_TYPE_MAPKEY);
            int startColumnIndex = cellExtendArgs.GetData<int>(SpecailStr.MULTI_TYPE_COLUMN_START);
            int rangeLength = cellExtendArgs.GetData<int>(SpecailStr.MULTI_TYPE_RANGELENGTH);
            int dimension = cellExtendArgs.GetData<int>(SpecailStr.MULTI_TYPE_DIMENSION);
            if (rangeLength == -1)
            {
                rangeLength = GetKeyCount() - startColumnIndex;
            }

            int endColumnIndex = startColumnIndex + rangeLength;
            if (!columnData.IsCurrentGroupSubFromGroupWithName(fieldName, out var findGroup))
            {
                ValueGroup valueGroup = new ValueGroup();
                valueGroup.FromCell = cell;
                valueGroup.IsArray = false;
                valueGroup.SetFieldName(fieldName);
                valueGroup.IsSheetGroup = fieldName == cell.sheetExportData.exportField;
                for (int inAreaColumnIndex = startColumnIndex; inAreaColumnIndex < endColumnIndex; inAreaColumnIndex++)
                {
                    ColumnData inAreaColumnData = GetColumnData(inAreaColumnIndex);
                    inAreaColumnData.SetBelongGroup(valueGroup);
                }
            }

            // 检查是否单开组
            for (int currentColumnIndex = startColumnIndex; currentColumnIndex < startColumnIndex + rangeLength; currentColumnIndex += dimension)
            {
                int currentGroupIndex = (int)(currentColumnIndex - startColumnIndex) / dimension + 1;
                int currentHasGroupCount = columnData.GetGroupWithNameSubCount(fieldName);
                if (currentHasGroupCount < currentGroupIndex)
                {
                    ValueGroup newRowGroup = new ValueGroup();
                    newRowGroup.IsArray = false;
                    newRowGroup.FromCell = cell;
                    newRowGroup.SetFieldName(mapKey);
                    for (int inAreaColumnIndex = currentColumnIndex; inAreaColumnIndex < currentColumnIndex + dimension; inAreaColumnIndex++)
                    {
                        ColumnData inAreaColumnData = GetColumnData(inAreaColumnIndex);
                        inAreaColumnData.SetBelongGroup(newRowGroup);
                    }
                }
                //currentColumnIndex += dimension;
            }
        }
  
        private void HandleMultiRowArray(CellExtendArgs cellExtendArgs, CellData cell)
        {
            ColumnData columnData = cellExtendArgs.ColumnData;
            string fieldName = cellExtendArgs.GetData<string>(SpecailStr.MULTI_TYPE_FIELD);
            int startColumnIndex = cellExtendArgs.GetData<int>(SpecailStr.MULTI_TYPE_COLUMN_START);
            int rowMission = cellExtendArgs.GetData<int>(SpecailStr.MULTI_TYPE_ROW_MISSION);
            int rangeLength = cellExtendArgs.GetData<int>(SpecailStr.MULTI_TYPE_RANGELENGTH);
            bool itemArray = cellExtendArgs.GetData<bool>(SpecailStr.MULTI_TYPE_ITEM_ARRAY_OUT);
            bool isMerge = cellExtendArgs.GetData<bool>(SpecailStr.MULTI_TYPE_ITEM_ARRAY_Merge);
            if (rangeLength == -1)
            {
                rangeLength = GetKeyCount() - startColumnIndex;
            }
          


            int endColumnIndex = startColumnIndex + rangeLength;
            bool isEmptyRow = true;
            if (!cell.isEmptyRow)
            {
                for (int inAreaColumnIndex = startColumnIndex; inAreaColumnIndex < endColumnIndex; inAreaColumnIndex++)
                {
                    ICell curCell = cell.Row.GetCell(inAreaColumnIndex);
                    if (curCell != null)
                    {
                        curCell.SetCellType(CellType.String);
                        if (!string.IsNullOrEmpty(curCell.StringCellValue))
                        {
                            isEmptyRow = false;
                        }
                    }
                }
            }
            
            if (!columnData.IsCurrentGroupSubFromGroupWithName(fieldName, out var findGroup))
            {
                // 创建RootGroup
                ValueGroup valueGroup = new ValueGroup();
                valueGroup.IsArray = true;
                valueGroup.FromCell = cell;
                valueGroup.IsSheetGroup = fieldName == cell.sheetExportData.exportField;
                //valueGroup.enableWrite = false;
                valueGroup.SetFieldName(fieldName);
                if (isMerge)
                {
                    valueGroup.enableCheckWrite = false;
                }

                for (int inAreaColumnIndex = startColumnIndex; inAreaColumnIndex < endColumnIndex; inAreaColumnIndex++)
                {
                    ColumnData inAreaColumnData = GetColumnData(inAreaColumnIndex);
                    inAreaColumnData.SetBelongGroup(valueGroup);
                }
            }

            // 检查是否单开组
            bool isEmptyInMission = true;
            if (!cell.isEmptyRow)
            {
                for (int i = startColumnIndex; i < startColumnIndex + rowMission; i++)
                {
                    ICell curCell = cell.Row.GetCell(i);
                    if (curCell != null)
                    {
                        curCell.SetCellType(CellType.String);
                        if (!string.IsNullOrEmpty(curCell.StringCellValue))
                        {
                            isEmptyInMission = false;
                        }
                    }
                }
            }
            
           
            if (!isEmptyInMission && !isMerge)
            {
                ValueGroup newRowGroup = new ValueGroup();
                newRowGroup.IsArray = itemArray;
                newRowGroup.FromCell = cell;
                //newRowGroup.enableWrite = true;

                int currentGroupCount = columnData.GetGroupWithNameSubCount(fieldName);
                newRowGroup.SetFieldName(currentGroupCount + "");
                for (int inAreaColumnIndex = startColumnIndex; inAreaColumnIndex < endColumnIndex; inAreaColumnIndex++)
                {
                    ColumnData inAreaColumnData = GetColumnData(inAreaColumnIndex);
                    //inAreaColumnData.BelongGroup.enableWrite = true;
                    inAreaColumnData.SetBelongGroup(newRowGroup, fieldName);  
                }
            }
        }

        private void HandleMultiRowMap(CellExtendArgs cellExtendArgs, CellData cell)
        {
            ColumnData columnData = cellExtendArgs.ColumnData;
            string fieldName = cellExtendArgs.GetData<string>(SpecailStr.MULTI_TYPE_FIELD);
            string mapKey = cellExtendArgs.GetData<string>(SpecailStr.MULTI_TYPE_MAPKEY);
            int startColumnIndex = cellExtendArgs.GetData<int>(SpecailStr.MULTI_TYPE_COLUMN_START);
            int rowMission = cellExtendArgs.GetData<int>(SpecailStr.MULTI_TYPE_ROW_MISSION);
            int rangeLength = cellExtendArgs.GetData<int>(SpecailStr.MULTI_TYPE_RANGELENGTH);
            bool isItemArray = cellExtendArgs.GetData<bool>(SpecailStr.MULTI_TYPE_ITEM_ARRAY_OUT);
            if (rangeLength == -1)
            {
                rangeLength = GetKeyCount() - startColumnIndex;
            }
            int endColumnIndex = startColumnIndex + rangeLength;
            if (!columnData.IsCurrentGroupSubFromGroupWithName(fieldName, out var findGroup))
            {
                // 创建RootGroup
                ValueGroup valueGroup = new ValueGroup();
                valueGroup.IsArray = false;
                valueGroup.FromCell = cell;
                valueGroup.IsSheetGroup = fieldName == cell.sheetExportData.exportField;
                valueGroup.SetFieldName(fieldName);

                for (int inAreaColumnIndex = startColumnIndex; inAreaColumnIndex < endColumnIndex; inAreaColumnIndex++)
                {
                    ColumnData inAreaColumnData = GetColumnData(inAreaColumnIndex);
                    inAreaColumnData.SetBelongGroup(valueGroup);
                }
            }

            // 检查是否单开组
            bool isEmptyInMission = true;
            if (!cell.isEmptyRow)
            {
                for (int i = startColumnIndex; i < startColumnIndex + rowMission; i++)
                {
                    ICell curCell = cell.Row.GetCell(i);
                    if (curCell != null)
                    {
                        curCell.SetCellType(CellType.String);
                        if (!string.IsNullOrEmpty(curCell.StringCellValue))
                        {
                            isEmptyInMission = false;
                        }
                    }
                }
            }
            
            if (!isEmptyInMission)
            {
                // 单开组
                ValueGroup newRowGroup = new ValueGroup();
                newRowGroup.IsArray = false;
                newRowGroup.FromCell = cell;
                // 放到读取数据后动态解析名称
                newRowGroup.SetFieldName(mapKey);
                for (int inAreaColumnIndex = startColumnIndex; inAreaColumnIndex < endColumnIndex; inAreaColumnIndex++)
                {
                    ColumnData inAreaColumnData = GetColumnData(inAreaColumnIndex);
                    inAreaColumnData.SetBelongGroup(newRowGroup, fieldName);
                }
            }
        }

        private bool IsNormalType(string typeStr)
        {
            ExcelValueType cellType = DecodeExcelValueType(typeStr);
            return IsNormalType(cellType);
        }

        private void OnTableReadBegin(ExportSheetJT sheetExportData, ISheet sheet)
        {
            //ValueGroup sheetGroup = new ValueGroup();
            //sheetGroup.SetFieldName(sheetExportData.exportField);
            //sheetGroup.IsArray = sheetExportData.isArrayExport;
            // //TODO:分配一个SheetGroup
            //GlobalValueGroup.AddSubValueGroup(sheetGroup);
            int[] columnIndexs = GetColumnIndexs();
            for (int i = 0; i < columnIndexs.Length; i++)
            {
                int columnIndex = columnIndexs[i];
                ColumnData columnData = GetColumnData(columnIndex);
                columnData.SetBelongGroup(GlobalValueGroup);
            }
        }

        private void OnTableReadEnd()
        {
        }

        private bool ReadTableComment(ExportSheetJT sheetExportData, ISheet sheet)
        {
            var rowIndexList = sheetExportData.commentRowList;
            for (int rowIndex = 0; rowIndex < rowIndexList.Length; rowIndex++)
            {
                var commentRow = sheet.GetRow(rowIndexList[rowIndex] - 1);
                if (commentRow == null) continue;
                for (int columnIndex = 0; columnIndex < MathF.Max(GetKeyCount(), commentRow.LastCellNum); columnIndex++)
                {
                    var cell = commentRow.GetCell(columnIndex);
                    cell?.SetCellType(CellType.String);
                    var comment = string.Empty;
                    if (cell != null)
                    {
                        comment = cell.StringCellValue;
                    }
                    string key = GetColumnKey(columnIndex);
                    if (!m_KeyWordComment.ContainsKey(sheetExportData.exportFullPath))
                    {
                        m_KeyWordComment.Add(sheetExportData.exportFullPath, new CommentData());
                    }
                    string commentTitle = new FileInfo(sheetExportData.ownerChannelConfig.ownerExcelConfig.excelPath).Name;
                    m_KeyWordComment[sheetExportData.exportFullPath].AddComment($"导出 Excel: {commentTitle},导出 sheet: {sheetExportData.sheetName}", key, comment);
                }
            }
            return true;
        }

        private void ReadTableData(ExportSheetJT sheetExportData, ISheet sheet)
        {
            OnTableReadBegin(sheetExportData, sheet);
            // 
            if(sheetExportData.dataRowBegin-1> sheet.LastRowNum)
            {
                ReadTableEmptyRow(sheetExportData, sheet);
            }
            else
            {
                for (int rowIndex = sheetExportData.dataRowBegin - 1; rowIndex <= sheet.LastRowNum; rowIndex++)
                {
                    var row = sheet.GetRow(rowIndex);
                    if (row == null) continue;
                    ReadTableRow(sheetExportData, sheet, row);
                }
            }
                
            OnTableReadEnd();
        }

        private bool ReadTableDataType(ExportSheetJT sheetExportData, ISheet sheet)
        {
            var typeRow = sheet.GetRow(sheetExportData.typeRow - 1);
            if (typeRow == null)
            {
                ExporterEnvironment.LogError("缺少字段类型行 typeRow 关键行! 请检查Excel 表格 或者 导出_config.xml 是否配置 typeRow?");
                return false;
            }

            for (int columnIndex = 0; columnIndex < typeRow.LastCellNum; columnIndex++)
            {
                var cell = typeRow.GetCell(columnIndex);
                var typeStr = string.Empty;
                if (cell != null)
                {
                    typeStr = cell.StringCellValue;
                }
                if (!string.IsNullOrEmpty(typeStr))
                {
                    typeStr = UpdateColumnType(columnIndex, typeStr);
                }
                ExcelValueType cellType = DecodeExcelValueType(typeStr);
                ColumnData columnData = GetColumnData(columnIndex);
                columnData.SetCellType(cellType);
            }

            return true;
        }
        private void ReadTableEmptyRow(ExportSheetJT sheetExportData, ISheet sheet)
        {
            int[] columnIndexs = GetColumnIndexs();
            for (int j = 0; j < columnIndexs.Length; j++)
            {
                int columnIndex = columnIndexs[j];
                UpdateCell(sheetExportData, sheet, null, columnIndex);
            }
        }

        private void ReadTableRow(ExportSheetJT sheetExportData, ISheet sheet, IRow row)
        {
            if (row == null) return;
            int[] columnIndexs = GetColumnIndexs();
            for (int j = 0; j < columnIndexs.Length; j++)
            {
                int columnIndex = columnIndexs[j];

                //ColumnData columnData = GetColumnData(columnIndex);
                UpdateCell(sheetExportData, sheet, row, columnIndex);
            }
        }

        private void RenameGroupCheck(ColumnData columnData, string cellValue)
        {
            if (columnData.IsCurrentGroupSubFromGroupWithName("@" + columnData.Key, out var findGroup))
            {
                findGroup.SetFieldName(cellValue);
            }
        }
        public virtual bool HandleSpecialNormaValue(ExcelValueType excelValueType, JsonData jsonData, string key, string stringValue,bool isArray)
        {
            return false;
        }
        private void UpdateCell(ExportSheetJT sheetExportData, ISheet sheet, IRow row, int columnIndex)
        {
            CellData cellData = new CellData();
            cellData.isEmptyRow = false;
            cellData.Row = row;
            var columnData = GetColumnData(columnIndex);
            cellData.sheet = sheet;
            cellData.sheetExportData = sheetExportData;
            cellData.StringCellValue = string.Empty;
            if (row == null)
            {
                cellData.isEmptyRow = true;
                if (columnData.HasExtend)
                {
                    UpdateGroupByCell(columnData.ExtendArgs, cellData);
                }

                //columnData.BelongGroup.SaveData(columnData.ExcelValueType, columnData.Key, string.Empty, HandleSpecialNormaValue);
                return;
            }
            else
            {
                var cell = row.GetCell(columnIndex);
                cellData.cell = cell;
                if (cell == null)
                {
                    if (columnData.HasExtend)
                    {
                        UpdateGroupByCell(columnData.ExtendArgs, cellData);
                    }
                    columnData.BelongGroup.SaveData(columnData.ExcelValueType, columnData.Key, string.Empty, HandleSpecialNormaValue);
                    return;
                }
                cellData.cell.SetCellType(CellType.String);
                string cellValue = cellData.cell.StringCellValue;
                ExcelValueType cellType = columnData.ExcelValueType;
                cellData.StringCellValue = cellValue;
                if (columnData.HasExtend)
                {
                    UpdateGroupByCell(columnData.ExtendArgs, cellData);
                }
                cellValue = cellData.cell.StringCellValue;
                columnData.BelongGroup.SaveData(cellType, columnData.Key, cellValue, HandleSpecialNormaValue);
                RenameGroupCheck(columnData, cellValue);
            }
            
        }

        private void UpdateGroupByCell(CellExtendArgs cellExtendArgs, CellData cell)
        {
            if (cellExtendArgs.excelValueType == ExcelValueType.MultiRowMap)
            {
                HandleMultiRowMap(cellExtendArgs, cell);
            }
            else if (cellExtendArgs.excelValueType == ExcelValueType.MultiRowArray)
            {
                HandleMultiRowArray(cellExtendArgs, cell);
            }
            else if (cellExtendArgs.excelValueType == ExcelValueType.MultiColMap)
            {
                HandleMultiColMap(cellExtendArgs, cell);
            }
            else if (cellExtendArgs.excelValueType == ExcelValueType.MultiColArray)
            {
                HandleMultiColArray(cellExtendArgs, cell);
            }
            if (cellExtendArgs.childExtend != null)
            {
                UpdateGroupByCell(cellExtendArgs.childExtend, cell);
            }
        }

        private string UpdateSpecialColumnType(int columnIndex, string originTypeStr)
        {
            CellExtendArgs keyTypeExtendArgs = new CellExtendArgs();
            ColumnData columnData = GetColumnData(columnIndex);
            columnData.AddExtendArgs(keyTypeExtendArgs);
            keyTypeExtendArgs.ColumnData = columnData;
            string[] args = keyTypeExtendArgs.args = originTypeStr.Split(SpecailStr.SPLIT);
            ExcelValueType excelValueType = DecodeExcelValueType(args[0]);
            if (excelValueType == ExcelValueType.MultiRowMap)// MultiRowMap#ROW_MISSION#DIMENSION#FIELD#MAPKEY#TYPE
            {
                keyTypeExtendArgs.excelValueType = ExcelValueType.MultiRowMap;
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_ROW_MISSION, int.Parse(args[1]));
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_RANGELENGTH, int.Parse(args[2]));
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_FIELD, args[3]);
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_MAPKEY, args[4]);
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_COLUMN_START, columnIndex);
                ExcelValueType columnType = DecodeExcelValueType(args[5]);
                if (IsNormalType(columnType))
                {
                    return args[5];
                }
                return UpdateColumnType(columnIndex, string.Join("#", args.Skip(5)));
            }
            else if (excelValueType == ExcelValueType.MultiRowArray)
            {
                keyTypeExtendArgs.excelValueType = ExcelValueType.MultiRowArray;
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_ROW_MISSION, int.Parse(args[1]));
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_RANGELENGTH, int.Parse(args[2]));
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_FIELD, args[3]);
                bool.TryParse(args[4], out bool itemArray);
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_ITEM_ARRAY_OUT, itemArray);
                bool.TryParse(args[5], out bool itemMerge);
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_ITEM_ARRAY_Merge, itemMerge);
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_COLUMN_START, columnIndex);
                ExcelValueType columnType = DecodeExcelValueType(args[6]);
                if (IsNormalType(columnType))
                {
                    return args[6];
                }
                return UpdateColumnType(columnIndex, string.Join("#", args.Skip(6)));
            }
            else if (excelValueType == ExcelValueType.MultiColMap)
            {
                keyTypeExtendArgs.excelValueType = ExcelValueType.MultiColMap;
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_DIMENSION, int.Parse(args[1]));
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_RANGELENGTH, int.Parse(args[2]));
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_FIELD, args[3]);
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_MAPKEY, args[4]);
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_COLUMN_START, columnIndex);
                ExcelValueType columnType = DecodeExcelValueType(args[5]);
                if (IsNormalType(columnType))
                {
                    return args[5];
                }
                return UpdateColumnType(columnIndex, string.Join("#", args.Skip(5)));
            }
            else if (excelValueType == ExcelValueType.MultiColArray)
            {
                keyTypeExtendArgs.excelValueType = ExcelValueType.MultiColArray;
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_DIMENSION, int.Parse(args[1]));
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_RANGELENGTH, int.Parse(args[2]));
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_FIELD, args[3]);
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_COLUMN_START, columnIndex);
                bool.TryParse(args[4], out bool itemArray);
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_ITEM_ARRAY_OUT, itemArray);
                bool.TryParse(args[5], out bool itemMerge);
                keyTypeExtendArgs.SetData(SpecailStr.MULTI_TYPE_ITEM_ARRAY_Merge, itemMerge);

                ExcelValueType columnType = DecodeExcelValueType(args[6]);
                if (IsNormalType(columnType))
                {
                    return args[6];
                }
                return UpdateColumnType(columnIndex, string.Join("#", args.Skip(6)));
            }
            else if (excelValueType == ExcelValueType.ForeignKey)
            {
                keyTypeExtendArgs.excelValueType = ExcelValueType.ForeignKey;
                return args[0];
            }
            else
            {
                return args[0];
            }
        }
    }

    public class ValueGroup
    {
        public ValueGroup()
        {
            m_SubValueGroups = new List<ValueGroup>();
            IsArray = false;
        }
        //public bool enableWrite = false;
        public bool IsEmptyGroup { get; private set; } = true;
        public string FieldName { get; private set; }
        public string FinalFieldName { get; private set; }
        public bool IsArray
        {
            get => m_IsAarray;
            set
            {
                m_IsAarray = value;
                m_Json = new JsonData();
                m_Json.SetJsonType(m_IsAarray ? JsonType.Array : JsonType.Object);
            }
        }

        public ValueGroup ParenValueGroup { get; private set; }
        public bool IsSheetGroup { get; internal set; }
        public CellData FromCell { get; internal set; }

        public void AddSubValueGroup(ValueGroup subValueGroup)
        {
            //IsEmptyGroup = false;
            subValueGroup.SetParentGroup(this);
            if (!m_SubValueGroups.Contains(subValueGroup))
            {
                m_SubValueGroups.Add(subValueGroup);
            }
        }
        public bool enableCheckWrite = true;
        public bool CheckWrite(string key)
        {
            if (!enableCheckWrite)
            {
                return true;
            }
            if (!m_WriteCount.ContainsKey(key))
            {
                m_WriteCount.Add(key, 0);
                return true;
            }
            if (m_WriteCount.TryGetValue(key, out int count) && count > 0)
            {
                m_WriteCount[key] = count - 1;
                return true;
            }
            return false;
        }
        public bool IsEmptySubAndDataGroup()
        {
            if (!IsEmptyGroup) return false;
            for (int i = 0; i < m_SubValueGroups.Count; i++)
            {
                if (!m_SubValueGroups[i].IsEmptySubAndDataGroup())
                {
                    return false;
                }
            }
            return true;
        }
        public JsonData ConcatJsonData()
        {
            //if (!enableWrite) return m_Json;
            if(IsArray && IsEmptyGroup)
            {
                m_Json.Clear();
                m_Json.SetJsonType(JsonType.Array);
            }
            for (int i = 0; i < m_SubValueGroups.Count; i++)
            {
                if (IsArray)
                {
                    //if(!m_SubValueGroups[i].IsEmptyGroup )
                    if(!m_SubValueGroups[i].IsEmptySubAndDataGroup())
                        m_Json.Add(m_SubValueGroups[i].ConcatJsonData());
                }
                else
                {
                    var subGroup = m_SubValueGroups[i];
                    bool isAdd = true;
                    if(IsCreateInNullData() && subGroup.IsCreateInNullData())
                    {
                        // 空开行
                        isAdd = false;
                    }
                    if (isAdd)
                        m_Json[subGroup.FinalFieldName] = subGroup.ConcatJsonData();
                }
            }
            return m_Json;
        }
        public bool IsCreateInNullData()
        {
            return FromCell != null && FromCell.cell == null;
        }

        public int GetSubGroupCount()
        {
            return m_SubValueGroups.Count;
        }

        public void SaveData(ExcelValueType cellType, 
            string key, 
            string stringValue, Func<ExcelValueType,JsonData,string,string,bool,bool> SpecialNormaValueHandler)
        {
            //if(SpecialNormaValueHandler == null)
            //{
            //    SpecialNormaValueHandler = HandleSpecialNormaValue;
            //}
            //if (!enableWrite) return;
            if(this.FieldName == $"${key}")
            {
                this.FinalFieldName = stringValue;
            }
            if (!CheckWrite(key))
            {
                return;
            }
            JsonData jsonData = m_Json;
            if (!string.IsNullOrEmpty(stringValue))
            {
                IsEmptyGroup = false;
            }
            switch (cellType)
            {
                case ExcelValueType.Float:

                    float.TryParse(stringValue, out float fValue);
                    if (IsArray)
                    {
                        //if (!string.IsNullOrEmpty(stringValue))
                            jsonData.Add(fValue);
                        //jsonData.Add(JsonMapper.ToObject(JsonMapper.ToJson(fValue)));
                    }
                    else
                    {
                        jsonData[key] = fValue;
                    }
                    break;

                case ExcelValueType.Double:

                    double.TryParse(stringValue, out double dValue);
                    if (IsArray)
                    {
                        jsonData.Add(dValue);
                    }
                    else
                    {
                        jsonData[key] = dValue;
                    }
                    break;

                case ExcelValueType.Boolean:

                    if (ExporterEnvironment.pureNumRegex.IsMatch(stringValue) &&
                        double.TryParse(stringValue, out double dValue2))
                    {
                        if (IsArray)
                        {
                            jsonData.Add(dValue2);
                        }
                        else
                        {
                            jsonData[key] = dValue2;
                        }
                        break;
                    }
                    bool.TryParse(stringValue, out bool bValue);
                    if (IsArray)
                    {
                        jsonData.Add(bValue);
                    }
                    else
                    {
                        jsonData[key] = bValue;
                    }
                    break;

                case ExcelValueType.LongInt:

                    long.TryParse(stringValue, out long lValue);
                    if (IsArray)
                    {
                        jsonData.Add(lValue);
                    }
                    else
                    {
                        jsonData[key] = lValue;
                    }
                    break;

                case ExcelValueType.Int:

                    int.TryParse(stringValue, out int iValue);
                    if (IsArray)
                    {
                        jsonData.Add(iValue);
                    }
                    else
                    {
                        jsonData[key] = iValue;
                    }
                    break;

                case ExcelValueType.String:
                    if (IsArray)
                    {
                         if(!string.IsNullOrEmpty(stringValue))
                            jsonData.Add(stringValue);
                    }
                    else
                    {
                        jsonData[key] = stringValue;
                    }
                    break;

                case ExcelValueType.Object:
                    if (!SpecialNormaValueHandler(ExcelValueType.Object,jsonData,key,stringValue,IsArray))
                    {
                        string objValue = SpecicalTypePrefix.objectPrefix + stringValue;

                        if (IsArray)
                        {
                            jsonData.Add(objValue);
                        }
                        else
                        {
                            jsonData[key] = objValue;
                        }
                    }
                  
                    break;

                case ExcelValueType.StringArray:
                    if (!SpecialNormaValueHandler(ExcelValueType.StringArray, jsonData, key, stringValue, IsArray))
                    {
                        string stringArrayValue = SpecicalTypePrefix.stringArrayPrefix + stringValue;

                        if (IsArray)
                        {
                            jsonData.Add(stringArrayValue);
                        }
                        else
                        {
                            jsonData[key] = stringArrayValue;
                        }

                    }

                    break;
                case ExcelValueType.StringArray2D:
                    if (!SpecialNormaValueHandler(ExcelValueType.StringArray2D, jsonData, key, stringValue, IsArray))
                    {
                        string stringArrayValue = SpecicalTypePrefix.stringArray2DPrefix + stringValue;

                        if (IsArray)
                        {
                            jsonData.Add(stringArrayValue);
                        }
                        else
                        {
                            jsonData[key] = stringArrayValue;
                        }

                    }

                    break;
                case ExcelValueType.DoubleArray:
                    if (!SpecialNormaValueHandler(ExcelValueType.DoubleArray, jsonData, key, stringValue, IsArray))
                    {
                        string intArrayValue = SpecicalTypePrefix.doubleArrayPrefix + stringValue;

                        if (IsArray)
                        {
                            jsonData.Add(intArrayValue);
                        }
                        else
                        {
                            jsonData[key] = intArrayValue;
                        }
                    }
                    break;
                case ExcelValueType.DoubleArray2D:
                    if (!SpecialNormaValueHandler(ExcelValueType.DoubleArray2D, jsonData, key, stringValue, IsArray))
                    {
                        string intArrayValue = SpecicalTypePrefix.doubleArray2DPrefix + stringValue;

                        if (IsArray)
                        {
                            jsonData.Add(intArrayValue);
                        }
                        else
                        {
                            jsonData[key] = intArrayValue;
                        }
                    }
                    break;
                case ExcelValueType.IntArray:
                    if (!SpecialNormaValueHandler(ExcelValueType.IntArray, jsonData, key, stringValue, IsArray))
                    {
                        string intArrayValue = SpecicalTypePrefix.intArrayPrefix + stringValue;

                        if (IsArray)
                        {
                            jsonData.Add(intArrayValue);
                        }
                        else
                        {
                            jsonData[key] = intArrayValue;
                        }
                    }
                    

                    break;

                case ExcelValueType.IntArray2D:
                    if (!SpecialNormaValueHandler(ExcelValueType.IntArray2D, jsonData, key, stringValue, IsArray))
                    {
                        string intArray2DValue = SpecicalTypePrefix.intArray2DPrefix + stringValue;
                        if (IsArray)
                        {
                            jsonData.Add(intArray2DValue);
                        }
                        else
                        {
                            jsonData[key] = intArray2DValue;
                        }
                    }
                    break;

                default:
                    if (IsArray)
                    {
                        jsonData.Add(stringValue);
                    }
                    else
                    {
                        jsonData[key] = stringValue;
                    }
                    break;
            }
        }
   
        public void SetFieldName(string fieldName)
        {
            this.FieldName = fieldName;
            this.FinalFieldName = this.FieldName;
        }

        public void SetParentGroup(ValueGroup parenValueGroup)
        {
            this.ParenValueGroup = parenValueGroup;
        }

        public void SetWriteCount(string key, int writeCount)
        {
            if (m_WriteCount.ContainsKey(key))
            {
                m_WriteCount[key] = writeCount;
            }
            else
            {
                m_WriteCount.Add(key, writeCount);
            }
        }

        public bool TryFindChildGroupByName(string name, out ValueGroup findGroup)
        {
            if (FieldName == name)
            {
                findGroup = this;
                return true;
            }
            for (int i = 0; i < m_SubValueGroups.Count; i++)
            {
                if (m_SubValueGroups[i].TryFindChildGroupByName(name, out findGroup))
                {
                    return true;
                }
            }
            findGroup = null;
            return false;
        }

        private bool m_IsAarray;
        private JsonData m_Json;
        private List<ValueGroup> m_SubValueGroups;
        private Dictionary<string, int> m_WriteCount = new Dictionary<string, int>();
    }
}
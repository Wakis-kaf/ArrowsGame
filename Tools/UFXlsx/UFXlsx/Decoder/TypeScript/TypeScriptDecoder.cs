using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UFXlsx.ConfigJsonTemplate;
using UFXlsx.Decoder.Json;
using UFXlsx.Main;

namespace UFXlsx.Decoder.TypeScript
{
    public class TypeScriptDecoder : JsonDecoder
    {
        private TypeScriptTable m_CurTable;

        public override WriteData[] DecodeExcel(ExportChannelConfigJT chanelExportData)
        {
            DecodeExcelToJson(chanelExportData);
            var keys = m_File2Content.Keys;
            WriteData[] writeDatas = new WriteData[keys.Count];
            TemplateExporter.Instance.Clear();
            for (int i = 0; i < keys.Count; i++)
            {
                writeDatas[i] = new WriteData();
                writeDatas[i].exportFullPath = keys.ElementAt(i);
                FileInfo fileInfo = new FileInfo(writeDatas[i].exportFullPath);
                string name = fileInfo.Name;
                string nameWithoutExtension = name.Replace(fileInfo.Extension, "");
                TemplateExporter.Instance.SetValue(ExportTemplateKeyWord.CONTENT, JsonToTypeScript(nameWithoutExtension, m_File2Content[keys.ElementAt(i)], chanelExportData.exportFormatType));
                TemplateExporter.Instance.SetValue(ExportTemplateKeyWord.EXPORTFILENAME, name);
                TemplateExporter.Instance.SetValue(ExportTemplateKeyWord.COMMENT, OutComment(writeDatas[i].exportFullPath));
                TemplateExporter.Instance.SetValue(ExportTemplateKeyWord.DECLARENAME, nameWithoutExtension);
                TemplateExporter.Instance.SetValue(ExportTemplateKeyWord.TABLENAME, name.Split(".")[0]);
                TemplateExporter.Instance.SetValue(ExportTemplateKeyWord.DECLARE,TsDeclareGenerator.Instance.GenerateFromJsonData(m_File2Content[keys.ElementAt(i)], nameWithoutExtension));
                string exportTxt = TemplateExporter.Instance.GetExportTxt();
                writeDatas[i].buffer = Encoding.UTF8.GetBytes(exportTxt);
            }
            return writeDatas;
        }
   
        protected override string OutComment(string exportFullPath)
        {
            string content = "";
            if (m_KeyWordComment.TryGetValue(exportFullPath, out var commentData))
            {
                content = commentData.GetString();
            }
            return $" \n {content} \n  \n\n";
        }

        public string JsonToTypeScript(string name, JsonData jsonData, int formatType = 1)
        {
            m_CurTable = ParseObject(name, jsonData);
            var str = "";
            switch (formatType)
            {
                case 1:
                    str = m_CurTable.GetStringExpaneArray().Trim();
                    break;

                case 2:
                    str = m_CurTable.GetStringAllExpand().Trim();
                    break;

                default:
                    str = m_CurTable.GetString().Trim();
                    break;
            }
            return str;
        }

        // 类型名生成规则
        private static string GetTypeName(string fieldName) => fieldName.ToLower();

        private static string GetArrayItemTypeName(string fieldName) => fieldName.ToLower() + "_item";

        // 递归对象
        private TypeScriptTable ParseObject(string fieldName, JsonData jsonObj)
        {
            var table = new TypeScriptTable(fieldName) { };
            foreach (KeyValuePair<string, JsonData> kv in jsonObj)
            {
                var key = kv.Key;
                var value = kv.Value;
                switch (value.GetJsonType())
                {
                    case JsonType.Boolean:
                        table.AddItem(key, (bool)value);
                        break;

                    case JsonType.String:
                        table.AddItem(key, (string)value);
                        break;

                    case JsonType.Int:
                        table.AddItem(key, (int)value);
                        break;

                    case JsonType.Double:
                        table.AddItem(key, float.Parse(value.ToString()));
                        break;

                    case JsonType.Long:
                        table.AddItem(key, (float)value);
                        break;

                    case JsonType.None:
                        table.AddItemNil(key);
                        break;

                    case JsonType.Object:
                        var childObjType = GetTypeName(key);
                        var childObj = ParseObject(key, value);
                        table.AddItem(key, childObj);
                        break;

                    case JsonType.Array:
                        var arrType = GetArrayItemTypeName(key);
                        var arrTable = ParseArray(key, value);
                        arrTable.isArray = true;
                        table.AddItem(key, arrTable);
                        break;
                }
            }
            return table;
        }

        // 递归数组
        private TypeScriptTable ParseArray(string parentField, JsonData jsonArr)
        {
            var table = new TypeScriptTable(parentField) { };
            int idx = 0;
            foreach (JsonData item in jsonArr)
            {
                string itemName = idx.ToString();
                switch (item.GetJsonType())
                {
                    case JsonType.Boolean:
                        table.AddArrayItem(itemName, (bool)item);
                        break;

                    case JsonType.String:
                        table.AddArrayItem(itemName, (string)item);
                        break;

                    case JsonType.Int:
                        table.AddArrayItem(itemName, (int)item);
                        break;

                    case JsonType.Double:
                        table.AddArrayItem(itemName, (float)item);
                        break;

                    case JsonType.Long:
                        table.AddArrayItem(itemName, (int)item);
                        break;

                    case JsonType.None:
                        table.AddArrayItemNil(itemName);
                        break;

                    case JsonType.Object:
                        var childObj = ParseObject(parentField, item);
                        table.AddArrayItem(itemName, childObj);
                        break;

                    case JsonType.Array:
                        // 嵌套数组，递归
                        var nestedArrType = GetArrayItemTypeName(parentField);
                        var nestedArr = ParseArray(parentField, item);
                        table.AddArrayItem(itemName, nestedArr);
                        break;
                }
                idx++;
            }
            return table;
        }
    }
}
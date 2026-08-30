using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using UFXlsx.ConfigJsonTemplate;
using UFXlsx.Decoder.Json;
using UFXlsx.Main;
using ExporterEnvironment = UFXlsx.Main.ExporterEnvironment;

namespace UFXlsx.Decoder.Lua
{
    public enum ELuaItemType
    {
        Table,
        String,
        Num,
        Boolean,
        nil
    }

    public class LuaDecoder : JsonDecoder
    {
        //protected override string OutComment(string exportFullPath)
        //{
        //    string content = "";
        //    if (m_KeyWordComment.TryGetValue(exportFullPath, out var commentData))
        //    {
        //        content = commentData.GetString();
        //    }
        //    return $"--[[\n {content} \n --]]\n\n";
        //}

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
                TemplateExporter.Instance.SetValue(ExportTemplateKeyWord.CONTENT, JsonToLua(m_File2Content[keys.ElementAt(i)], chanelExportData.exportFormatType));
                TemplateExporter.Instance.SetValue(ExportTemplateKeyWord.COMMENT, OutComment(writeDatas[i].exportFullPath));
                TemplateExporter.Instance.SetValue(ExportTemplateKeyWord.EXPORTFILENAME, new FileInfo(writeDatas[i].exportFullPath).Name);
                TemplateExporter.Instance.SetValue(ExportTemplateKeyWord.TABLENAME, new FileInfo(writeDatas[i].exportFullPath).Name.Split(".")[0]);
                string exportTxt = TemplateExporter.Instance.GetExportTxt();
                //ExporterEnvironment.Log($"最终导出\n {exportTxt}");
                //writeDatas[i].buffer = Encoding.UTF8.GetBytes(OutComment(writeDatas[i].exportFullPath) + JsonToLua(m_File2Content[keys.ElementAt(i)], chanelExportData.exportFormatType));
                writeDatas[i].buffer = Encoding.UTF8.GetBytes(exportTxt);
            }
            return writeDatas;
        }

        public static string JsonToLua(JsonData jsonData, int formatType = 1)
        {
            var luaBaseTable = JsonObject2LuaTable(jsonData);
            var str = "";
            switch (formatType)
            {
                case 1:
                    str = luaBaseTable.GetStringExpaneArray().Trim();
                    break;

                case 2:
                    str = luaBaseTable.GetStringAllExpand().Trim();
                    break;

                default:
                    str = luaBaseTable.GetString().Trim();
                    break;
            }
            return str;
        }

        public static string JsonToLua(string jsonStr)
        {
            var jsonBase = JsonMapper.ToObject(jsonStr);
            return JsonToLua(jsonBase);
        }

        private static LuaTable JsonObject2LuaTable(JsonData jsonObj)
        {
            var curLuaTable = new LuaTable();

            if (jsonObj.GetJsonType() == JsonType.Array)
            {
                return JsonArray2LuaTable(jsonObj);
            }
            else if (jsonObj.GetJsonType() == JsonType.Object)
            {
                //无序
                foreach (KeyValuePair<string, JsonData> kvp in jsonObj)
                {
                    switch (kvp.Value.GetJsonType())
                    {
                        case JsonType.Boolean:
                            curLuaTable.AddItem(kvp.Key, (bool)kvp.Value);
                            break;

                        case JsonType.Array:
                            curLuaTable.AddItem(kvp.Key, JsonArray2LuaTable(kvp.Value));
                            break;

                        case JsonType.String:
                            curLuaTable.AddItem(kvp.Key, (string)kvp.Value);
                            break;

                        case JsonType.Int: //转成string
                            curLuaTable.AddItem(kvp.Key, (int)kvp.Value);
                            break;

                        case JsonType.Double:
                            curLuaTable.AddItem(kvp.Key, float.Parse(kvp.Value.ToString()));
                            break;

                        case JsonType.None:
                            curLuaTable.AddItemNil(kvp.Key);
                            break;

                        case JsonType.Object:
                            curLuaTable.AddItem(kvp.Key, JsonObject2LuaTable(kvp.Value));
                            break;

                        case JsonType.Long:
                            curLuaTable.AddItem(kvp.Key, (float)kvp.Value);
                            break;
                    }
                }
            }

            return curLuaTable;
        }

        private static LuaTable JsonArray2LuaTable(JsonData json_arr)
        {
            var curLuaTable = new LuaTable();
            //往luaTable里面扔有序数组
            foreach (JsonData item in json_arr)
            {
                //检查子项类型

                switch (item.GetJsonType())
                {
                    case JsonType.Boolean:
                        curLuaTable.AddItem((bool)item);
                        break;

                    case JsonType.Array:
                        curLuaTable.AddItem(JsonArray2LuaTable(item));
                        break;

                    case JsonType.String:
                        curLuaTable.AddItem((string)item);
                        break;

                    case JsonType.Object:
                        curLuaTable.AddItem(JsonObject2LuaTable(item));
                        break;

                    case JsonType.Double:
                        curLuaTable.AddItem((float)item);
                        break;

                    case JsonType.Int:
                        curLuaTable.AddItem((int)item);
                        break;

                    case JsonType.None:
                        curLuaTable.AddItemNil();
                        break;

                    case JsonType.Long:
                        curLuaTable.AddItem((int)item);
                        break;
                }
            }

            return curLuaTable;
        }
    }
}
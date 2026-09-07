using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using UFXlsx.Main;

namespace UFXlsx.Decoder.Lua
{
    public class LuaObject
    {
        public ELuaItemType type;

        public decimal value_num;
        public string value_str;
        public bool value_boolean;
        public LuaTable value_table;

        public LuaObject(string value)
        {
            type = ELuaItemType.String;
            value_str = value;
        }

        public LuaObject(float value)
        {
            type = ELuaItemType.Num;
            value_num = Convert.ToDecimal(value);
        }

        public LuaObject(int value)
        {
            type = ELuaItemType.Num;
            value_num = value;
        }

        public LuaObject(bool value)
        {
            type = ELuaItemType.Boolean;
            value_boolean = value;
        }

        public LuaObject(LuaTable value)
        {
            type = ELuaItemType.Table;
            value_table = value;
        }

        public LuaObject()
        {
            type = ELuaItemType.nil;
        }

        public string GetStringAllExpand(int layer = 0)
        {
            switch (type)
            {
                case ELuaItemType.Table:
                    return value_table.GetStringAllExpand(layer);

                default:
                    return GetNormalTypeString();
            }
        }

        public string GetStringExpaneArray(out bool containsObject, int layer = 0)
        {
            containsObject = false;
            switch (type)
            {
                case ELuaItemType.Table:
                    string value = value_table.GetStringExpaneArray(out containsObject, layer);
                    containsObject = true;
                    return value;

                default:
                    return GetNormalTypeString();
            }
        }

        public string GetString()
        {
            switch (type)
            {
                case ELuaItemType.Table:
                    return value_table.GetString();

                default:
                    return GetNormalTypeString(); ;
            }
        }

        private string GetNormalTypeString()
        {
            switch (type)
            {
                case ELuaItemType.Boolean:
                    if (value_boolean)
                    {
                        return "true";
                    }
                    else
                    {
                        return "false";
                    }
                case ELuaItemType.Num:
                    // 默认导出会有科学计数法显示
                    //return value_num.ToString();
                    return Convert.ToDecimal(Decimal.Parse(value_num.ToString(), System.Globalization.NumberStyles.Number)).ToString();

                case ELuaItemType.String:
                    if (value_str.StartsWith(SpecicalTypePrefix.objectPrefix))
                    {
                        return value_str.Replace(SpecicalTypePrefix.objectPrefix, "");
                    }
                    else if (value_str.StartsWith(SpecicalTypePrefix.intArrayPrefix))
                    {
                        string res = value_str.Replace(SpecicalTypePrefix.intArrayPrefix, "");
                        int startIndex = 0;
                        int endIndex = res.Length;
                        if (res.StartsWith("{"))
                        {
                            startIndex = 1;
                        }
                        if (res.EndsWith("}"))
                        {
                            endIndex = endIndex - 1;
                        }
                        res = res.Substring(startIndex, endIndex - startIndex);
                        string[] items = res.Split(",");
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < items.Length; i++)
                        {
                            if (i != 0)
                            {
                                sb.Append(",");
                            }
                            sb.Append($"{items[i]}");
                        }
                        string content = sb.ToString();

                        return $"{{{content}}}";
                    }
                    else if (value_str.StartsWith(SpecicalTypePrefix.doubleArrayPrefix))
                    {
                        string res = value_str.Replace(SpecicalTypePrefix.doubleArrayPrefix, "");
                        int startIndex = 0;
                        int endIndex = res.Length;
                        if (res.StartsWith("{"))
                        {
                            startIndex = 1;
                        }
                        if (res.EndsWith("}"))
                        {
                            endIndex = endIndex - 1;
                        }
                        res = res.Substring(startIndex, endIndex - startIndex);
                        string[] items = res.Split(",");
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < items.Length; i++)
                        {
                            if (i != 0)
                            {
                                sb.Append(",");
                            }
                            sb.Append($"{items[i]}");
                        }
                        string content = sb.ToString();

                        return $"{{{content}}}";
                    }
                    else if (value_str.StartsWith(SpecicalTypePrefix.doubleArray2DPrefix))
                    {
                        string res = value_str.Replace(SpecicalTypePrefix.doubleArray2DPrefix, "");
                        res = res.Replace("[", "{");
                        res = res.Replace("]", "}");
                        return $"{{{res}}}";
                    }
                    else if (value_str.StartsWith(SpecicalTypePrefix.intArray2DPrefix))
                    {
                        string res = value_str.Replace(SpecicalTypePrefix.intArray2DPrefix, "");
                        res = res.Replace("[", "{");
                        res = res.Replace("]", "}");
                        return $"{{{res}}}";
                    }
                    else if (value_str.StartsWith(SpecicalTypePrefix.stringArray2DPrefix))
                    {
                        string res = value_str.Replace(SpecicalTypePrefix.stringArray2DPrefix, "");
                        res = res.Replace("[", "{");
                        res = res.Replace("]", "}");
                        return $"{{{res}}}";
                    }
                    else if (value_str.StartsWith(SpecicalTypePrefix.stringArrayPrefix))
                    {
                        string res = value_str.Replace(SpecicalTypePrefix.stringArrayPrefix, "");
                        int startIndex = 0;
                        int endIndex = res.Length;
                        if (res.StartsWith("{"))
                        {
                            startIndex = 1;
                        }
                        if (res.EndsWith("}"))
                        {
                            endIndex = endIndex - 1;
                        }
                        res = res.Substring(startIndex, endIndex - startIndex);
                        string[] items = res.Split(",");
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < items.Length; i++)
                        {
                            if (i != 0)
                            {
                                sb.Append(",");
                            }
                            sb.Append($"\"{items[i]}\"");
                        }
                        string content = sb.ToString();

                        return $"{{{content}}}";
                    }
                    return "\"" + value_str + "\"";

                case ELuaItemType.nil:
                    return "nil";

                default:
                    return "";
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UFXlsx.Decoder.Lua;
using UFXlsx.Main;
using static UFXlsx.Decoder.TypeScript.TypeScriptDecoder;

namespace UFXlsx.Decoder.TypeScript
{
    public class TypeScriptTable
    {
        public string tableName;
        private List<TypeScriptObject> mItems_order = new List<TypeScriptObject>(); //存放table中的有序部分
        private Dictionary<string, TypeScriptObject> mItems_kv = new Dictionary<string, TypeScriptObject>(); //存放table中的无序部分
        public bool isArray = false;

        public TypeScriptTable(string name)
        {
            this.tableName = name;
        }

        #region 有序数组添加

        /// <summary>
        /// [有序]添加string value
        /// </summary>
        /// <param name="value"></param>
        public void AddArrayItem(string name, string value)
        {
            var obj = new TypeScriptObject(value);
            obj.key_name = name;
            mItems_order.Add(obj);
        }

        /// <summary>
        /// [有序]num value
        /// </summary>
        /// <param name="value"></param>
        public void AddArrayItem(string name, int value)
        {
            var obj = new TypeScriptObject(value);
            obj.key_name = name;
            mItems_order.Add(obj);
        }

        /// <summary>
        /// [有序]num value
        /// </summary>
        /// <param name="value"></param>
        public void AddArrayItem(string name, float value)
        {
            var obj = new TypeScriptObject(value);
            obj.key_name = name;
            mItems_order.Add(obj);
        }

        /// <summary>
        /// [有序]bool value
        /// </summary>
        /// <param name="value"></param>
        public void AddArrayItem(string name, bool value)
        {
            var obj = new TypeScriptObject(value);
            obj.key_name = name;
            mItems_order.Add(obj);
        }

        public void AddArrayItem(string name, TypeScriptTable value)
        {
            var obj = new TypeScriptObject(value);
            obj.key_name = name;
            mItems_order.Add(obj);
        }

        /// <summary>
        /// 加入nil
        /// </summary>
        public void AddArrayItemNil(string name)
        {
            var obj = new TypeScriptObject();
            obj.key_name = name;
            mItems_order.Add(obj);
        }

        #endregion 有序数组添加

        #region Key_Value添加

        public void AddItem(string key, string value)
        {
            if (!mItems_kv.ContainsKey(key))
            {
                var obj = new TypeScriptObject(value);
                obj.key_name = key;
                mItems_kv.Add(key, obj);
            }
        }

        public void AddItem(string key, int value)
        {
            if (!mItems_kv.ContainsKey(key))
            {
                var obj = new TypeScriptObject(value);
                obj.key_name = key;
                mItems_kv.Add(key, obj);
            }
        }

        public void AddItem(string key, float value)
        {
            if (!mItems_kv.ContainsKey(key))
            {
                var obj = new TypeScriptObject(value);
                obj.key_name = key;
                mItems_kv.Add(key, obj);
            }
        }

        public void AddItem(string key, bool value)
        {
            if (!mItems_kv.ContainsKey(key))
            {
                var obj = new TypeScriptObject(value);
                obj.key_name = key;
                mItems_kv.Add(key, obj);
            }
        }

        public void AddItem(string key, TypeScriptTable value)
        {
            if (!mItems_kv.ContainsKey(key))
            {
                var obj = new TypeScriptObject(value);
                obj.key_name = key;
                //obj.declareTypeName = string.IsNullOrEmpty(declareTypeName) ? TypeScriptDecoder.GetDeclareTypeName(key) : declareTypeName;
                mItems_kv.Add(key, obj);
            }
        }

        public void AddItemNil(string key)
        {
            if (!mItems_kv.ContainsKey(key))
            {
                var obj = new TypeScriptObject();
                obj.key_name = key;
                mItems_kv.Add(key, obj);
            }
        }

        #endregion Key_Value添加

        /// <summary>
        /// 全部展开显示
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public string GetStringAllExpand(int layer = 1)
        {
            StringBuilder str;
            if (mItems_order.Count > 0 || isArray)
            {
                str = new StringBuilder(GetTabAfter("[\n", layer));
                for (int i = 0; i < mItems_order.Count; i++)
                {
                    var item = mItems_order[i];
                    str.Append(item.GetStringAllExpand(layer + 1));
                    str.Append(",\n");
                    str = GetTabAfter(str, i == mItems_order.Count - 1 ? layer - 1 : layer);
                }
                str.Append("]");
                return str.ToString();
            }
            if (mItems_kv.Count > 0)
            {
                str = new StringBuilder(GetTabAfter("{\n", layer));
                for (int i = 0; i < mItems_kv.Keys.Count; i++)
                {
                    string key = mItems_kv.Keys.ElementAt(i);
                    TypeScriptObject value = mItems_kv[key];
                    if (ExporterEnvironment.pureNumRegex.IsMatch(key))
                    {
                        key = $"[{key}]";
                    }
                    str.Append(key + " : " + value.GetStringAllExpand(layer + 1));
                    str.Append(",\n");
                    str = GetTabAfter(str, i == mItems_kv.Keys.Count - 1 ? layer - 1 : layer);
                }
                str.Append("}");
                return str.ToString();
            }

            return GetTabAfter("{\n", layer) + "}";
        }

        public string GetStringExpaneArray()
        {
            return GetStringExpaneArray(out bool hasObject, 1);
        }

        /// <summary>
        /// 数组展开显示, 数据行显示为一行
        /// </summary>
        /// <returns></returns>
        public string GetStringExpaneArray(out bool containsObject, int layer = 1)
        {
            StringBuilder strStart = new StringBuilder("{");
            StringBuilder strContent = new StringBuilder("");
            StringBuilder strEnd = new StringBuilder("}");
            bool hasObject = false;
            containsObject = false;
            if (mItems_order.Count > 0 || isArray)
            {
                strStart = new StringBuilder("[");
                for (int i = 0; i < mItems_order.Count; i++)
                {
                    var item = mItems_order[i];
                    strContent.Append(item.GetStringExpaneArray(out hasObject, layer + 1));
                    strContent.Append(",");
                    if (hasObject)
                    {
                        containsObject = true;
                        strContent.Append("\n");
                        strContent = GetTabAfter(strContent, i == mItems_order.Count - 1 ? layer - 1 : layer);
                    }
                }
                strEnd = new StringBuilder("]");
            }
            if (mItems_kv.Count > 0)
            {
                for (int i = 0; i < mItems_kv.Keys.Count; i++)
                {
                    string key = mItems_kv.Keys.ElementAt(i);
                    TypeScriptObject value = mItems_kv[key];
                    if (ExporterEnvironment.pureNumRegex.IsMatch(key))
                    {
                        key = $"[{key}]";
                    }
                    strContent.Append(key + " : " + value.GetStringExpaneArray(out hasObject, layer + 1));
                    strContent.Append(",");
                    if (hasObject)
                    {
                        containsObject = true;
                        strContent.Append("\n");
                        strContent = GetTabAfter(strContent, i == mItems_kv.Keys.Count - 1 ? layer - 1 : layer);
                    }
                }
            }
            if (containsObject)
            {
                GetTabAfter(strStart.Append("\n"), layer);
                GetTabBefore(strEnd, layer - 1).Insert(0, "\n");
            }
            return strStart.Append(strContent.Append(strEnd)).ToString();
        }

        public StringBuilder GetTabAfter(StringBuilder content, int layer)
        {
            for (int i = 0; i < layer; i++)
            {
                content.Append("\t");
            }
            return content;
        }

        public string GetTabAfter(string content, int layer)
        {
            for (int i = 0; i < layer; i++)
            {
                content = content + "\t";
            }
            return content;
        }

        public StringBuilder GetTabBefore(StringBuilder content, int layer)
        {
            for (int i = 0; i < layer; i++)
            {
                content.Insert(0, "\t");
            }
            return content;
        }

        public string GetTabBefore(string content, int layer)
        {
            for (int i = 0; i < layer; i++)
            {
                content = "\t" + content;
            }
            return content;
        }

        /// <summary>
        /// 无格式全压缩显示
        /// </summary>
        /// <returns></returns>
        public string GetString()
        {
            StringBuilder str = new StringBuilder();
            if (mItems_order.Count > 0 || isArray)
            {
                str = new StringBuilder("[");
                foreach (var item in mItems_order)
                {
                    str.Append(item.GetString());
                    str.Append(",");
                }
                str.Append("]");
                return str.ToString();
            }

            if (mItems_kv.Count > 0)
            {
                str = new StringBuilder("{");
                foreach (var item in mItems_kv)
                {
                    string key = item.Key;
                    if (ExporterEnvironment.pureNumRegex.IsMatch(key))
                    {
                        key = $"[{key}]";
                    }
                    str.Append(key + ":" + item.Value.GetString());
                    str.Append(",");
                }
                str.Append("}");
                return str.ToString();
            }

            return "{}";
        }
    }
}
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UFXlsx.Decoder.CSharp
{
    public static class JsonToCSharpConverter
    {
        public static string ConvertJsonToCSharpEntity(string json, string className = "Root")
        {
            JsonData data = JsonMapper.ToObject(json);
            var sb = new StringBuilder();
            var generatedClasses = new HashSet<string>();
            GenerateClass(data, className, sb, generatedClasses);
            return sb.ToString();
        }

        private static void GenerateClass(JsonData data, string className, StringBuilder sb, HashSet<string> generatedClasses)
        {
            if (generatedClasses.Contains(className)) return;
            generatedClasses.Add(className);

            sb.AppendLine($"public class {className}");
            sb.AppendLine("{");

            if (data.IsObject)
            {
                foreach (var entry in data)
                {
                    var keyValue = (DictionaryEntry)entry;
                    string key = keyValue.Key.ToString();
                    var value = (JsonData)keyValue.Value;
                    string propType = GetCSharpType(value, key, sb, generatedClasses);
                    sb.AppendLine($"    public {propType} {key} {{ get; set; }}");
                }
            }
            else if (data.IsArray)
            {
                if (data.Count > 0)
                {
                    GenerateClass(data[0], className + "Item", sb, generatedClasses);
                    sb.AppendLine($"    public List<{className}Item> Items {{ get; set; }}");
                }
            }

            sb.AppendLine("}");
            sb.AppendLine();
        }
        private static string GetCSharpType(JsonData value, string propName, StringBuilder sb, HashSet<string> generatedClasses)
        {
            if (value.IsInt) return "int";
            if (value.IsLong) return "long";
            if (value.IsDouble) return "double";
            if (value.IsBoolean) return "bool";
            if (value.IsString) return "string";
            if (value.IsArray)
            {
                if (value.Count > 0)
                {
                    string itemType = GetCSharpType(value[0], propName + "Item", sb, generatedClasses);
                    return $"List<{itemType}>";
                }
                return "List<object>";
            }
            if (value.IsObject)
            {
                string className = propName + "Class";
                GenerateClass(value, className, sb, generatedClasses);
                return className;
            }
            return "object";
        }
    }
}
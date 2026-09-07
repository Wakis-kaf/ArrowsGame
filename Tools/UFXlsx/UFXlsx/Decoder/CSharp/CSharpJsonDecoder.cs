using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using UFXlsx.ConfigJsonTemplate;
using UFXlsx.Decoder.Json;
using UFXlsx.Main;

namespace UFXlsx.Decoder.CSharp
{
    public class CSharpJsonDecoder : JsonDecoder
    {
        public override WriteData[] DecodeExcel(ExportChannelConfigJT chanelExportData)
        {
            WriteData[] writeDatas = base.DecodeExcel(chanelExportData);
            return writeDatas;
        }
        public override bool HandleSpecialNormaValue(ExcelValueType excelValueType,
             JsonData jsonData,
             string key,
             string stringValue,
             bool isArray)
        {
            switch (excelValueType)
            {
                case ExcelValueType.Object:
                    if (isArray)
                    {
                        //if(!string.IsNullOrEmpty(stringValue))
                            jsonData.Add(stringValue);
                    }
                    else
                    {
                        jsonData[key] = stringValue;
                    }
                    return true;
                case ExcelValueType.StringArray:
                    JsonData stringArrayValue = ParseStringToJsonArray(stringValue);
                    if (isArray)
                    {
                        jsonData.Add(stringArrayValue);
                    }
                    else
                    {
                        jsonData[key] = stringArrayValue;
                    }
                    return true;
                case ExcelValueType.StringArray2D:
                    JsonData stringArray2DValue = ParseStringToStringArray2D(stringValue);
                    if (isArray)
                    {
                        jsonData.Add(stringArray2DValue);
                    }
                    else
                    {
                        jsonData[key] = stringArray2DValue;
                    }
                    return true;
                case ExcelValueType.IntArray:
                    JsonData intArrayValue = ParseStringToIntArray(stringValue);
                    if (isArray)
                    {
                        jsonData.Add(intArrayValue);
                    }
                    else
                    {
                        jsonData[key] = intArrayValue;
                    }
                    return true;

                case ExcelValueType.IntArray2D:
                    JsonData intArray2DValue = ParseStringToIntArray2D(stringValue);
                    if (isArray)
                    {
                        jsonData.Add(intArray2DValue);
                    }
                    else
                    {
                        jsonData[key] = intArray2DValue;
                    }
                    return true;
                case ExcelValueType.DoubleArray:
                    JsonData doubleArrayValue = ParseStringToDoubleArray(stringValue);
                    if (isArray)
                    {
                        jsonData.Add(doubleArrayValue);
                    }
                    else
                    {
                        jsonData[key] = doubleArrayValue;
                    }
                    return true;
                case ExcelValueType.DoubleArray2D:
                    JsonData doubleArray2dValue = ParseStringToDoubleArray2D(stringValue);
                    if (isArray)
                    {
                        jsonData.Add(doubleArray2dValue);
                    }
                    else
                    {
                        jsonData[key] = doubleArray2dValue;
                    }
                    return true;
            }
            return false;
        }

        // 解析字符串到字符串数组
        private JsonData ParseStringToJsonArray(string stringValue)
        {
            JsonData jsonArray = new JsonData();

            if (string.IsNullOrEmpty(stringValue))
            {
                jsonArray.SetJsonType(JsonType.Array);
                return jsonArray;
            }

            // 处理 "[xxx,xxxxx]" 格式
            if (stringValue.StartsWith("[") && stringValue.EndsWith("]"))
            {
                string trimmedValue = stringValue.Substring(1, stringValue.Length - 2);
                string[] elements = trimmedValue.Split(',');

                foreach (string element in elements)
                {
                    string trimmedElement = element.Trim();
                    // 移除可能的引号
                    if (trimmedElement.StartsWith("\"") && trimmedElement.EndsWith("\""))
                    {
                        trimmedElement = trimmedElement.Substring(1, trimmedElement.Length - 2);
                    }
                    jsonArray.Add(trimmedElement);
                }
            }
            // 处理 "a,b,c" 格式
            else if (stringValue.Contains(","))
            {
                string[] elements = stringValue.Split(',');
                foreach (string element in elements)
                {
                    jsonArray.Add(element.Trim());
                }
            }
            // 处理单个值 "xx" 格式
            else
            {
                jsonArray.Add(stringValue.Trim());
            }

            return jsonArray;
        }

        // 解析字符串到整数数组
        private JsonData ParseStringToIntArray(string stringValue)
        {
            JsonData jsonArray = new JsonData();

            if (string.IsNullOrEmpty(stringValue))
            {
                jsonArray.SetJsonType(JsonType.Array);
                return jsonArray;
            }

            string[] elements;

            // 处理 "[1,2,3]" 格式
            if (stringValue.StartsWith("[") && stringValue.EndsWith("]"))
            {
                string trimmedValue = stringValue.Substring(1, stringValue.Length - 2);
                elements = trimmedValue.Split(',');
            }
            // 处理 "1,2,3" 格式
            else if (stringValue.Contains(","))
            {
                elements = stringValue.Split(',');
            }
            // 处理单个值 "123" 格式
            else
            {
                elements = new string[] { stringValue };
            }

            foreach (string element in elements)
            {
                string trimmedElement = element.Trim();
                if (int.TryParse(trimmedElement, out int intValue))
                {
                    jsonArray.Add(intValue);
                }
                else
                {
                    // 如果解析失败，可以记录日志或使用默认值
                    jsonArray.Add(0);
                }
            }

            return jsonArray;
        }
        private JsonData ParseStringToDoubleArray(string stringValue)
        {
            JsonData jsonArray = new JsonData();

            if (string.IsNullOrEmpty(stringValue))
            {
                jsonArray.SetJsonType(JsonType.Array);
                return jsonArray;
            }

            string[] elements;

            // 处理 "[1,2,3]" 格式
            if (stringValue.StartsWith("[") && stringValue.EndsWith("]"))
            {
                string trimmedValue = stringValue.Substring(1, stringValue.Length - 2);
                elements = trimmedValue.Split(',');
            }
            // 处理 "1,2,3" 格式
            else if (stringValue.Contains(","))
            {
                elements = stringValue.Split(',');
            }
            // 处理单个值 "123" 格式
            else
            {
                elements = new string[] { stringValue };
            }

            foreach (string element in elements)
            {
                string trimmedElement = element.Trim();
                if (Double.TryParse(trimmedElement, out Double doubleValue))
                {
                    jsonArray.Add(doubleValue);
                }
                else
                {
                    // 如果解析失败，可以记录日志或使用默认值
                    jsonArray.Add(0);
                }
            }

            return jsonArray;
        }
        private JsonData ParseStringToDoubleArray2D(string stringValue)
        {
            JsonData jsonArray2D = new JsonData();
            jsonArray2D.SetJsonType(JsonType.Array);

            if (string.IsNullOrEmpty(stringValue))
            {
                return jsonArray2D;
            }

            string processedValue = stringValue.Trim();

            // 处理 "[[1.1,2.2],[3.3,4.4]]" 格式
            if (processedValue.StartsWith("[[") && processedValue.EndsWith("]]"))
            {
                string trimmedValue = processedValue.Substring(2, processedValue.Length - 4);
                string[] rows = trimmedValue.Split(new[] { "],[" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string row in rows)
                {
                    JsonData jsonRow = ParseDoubleRow(row);
                    jsonArray2D.Add(jsonRow);
                }
            }
            // 处理 "[1.1,2.2],[3.3,4.4]" 格式（不带外层方括号）
            else if (processedValue.StartsWith("[") && processedValue.Contains("],["))
            {
                // 移除可能的首尾空格，但保留内容
                string content = processedValue;
                string[] rows = SplitMultipleArrays(content);

                foreach (string row in rows)
                {
                    JsonData jsonRow = ParseDoubleRow(row);
                    jsonArray2D.Add(jsonRow);
                }
            }
            // 处理 "1.1,2.2;3.3,4.4" 格式（使用分号分隔行）
            else if (processedValue.Contains(";"))
            {
                string[] rows = processedValue.Split(';');

                foreach (string row in rows)
                {
                    JsonData jsonRow = ParseDoubleRow(row);
                    jsonArray2D.Add(jsonRow);
                }
            }
            // 处理单行数组
            else
            {
                JsonData jsonRow = ParseDoubleRow(processedValue);
                jsonArray2D.Add(jsonRow);
            }

            return jsonArray2D;
        }

        // 解析字符串到二维整数数组
        private JsonData ParseStringToIntArray2D(string stringValue)
        {
            JsonData jsonArray2D = new JsonData();
            jsonArray2D.SetJsonType(JsonType.Array);

            if (string.IsNullOrEmpty(stringValue))
            {
                return jsonArray2D;
            }

            string processedValue = stringValue.Trim();

            // 处理 "[[1,2],[3,4]]" 格式
            if (processedValue.StartsWith("[[") && processedValue.EndsWith("]]"))
            {
                string trimmedValue = processedValue.Substring(2, processedValue.Length - 4);
                string[] rows = trimmedValue.Split(new[] { "],[" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string row in rows)
                {
                    JsonData jsonRow = ParseIntRow(row);
                    jsonArray2D.Add(jsonRow);
                }
            }
            // 处理 "[1,2],[3,4]" 格式（不带外层方括号）
            else if (processedValue.StartsWith("[") && processedValue.Contains("],["))
            {
                string[] rows = SplitMultipleArrays(processedValue);

                foreach (string row in rows)
                {
                    JsonData jsonRow = ParseIntRow(row);
                    jsonArray2D.Add(jsonRow);
                }
            }
            // 处理 "1,2;3,4" 格式（使用分号分隔行）
            else if (processedValue.Contains(";"))
            {
                string[] rows = processedValue.Split(';');

                foreach (string row in rows)
                {
                    JsonData jsonRow = ParseIntRow(row);
                    jsonArray2D.Add(jsonRow);
                }
            }
            // 处理单行数组
            else
            {
                JsonData jsonRow = ParseIntRow(processedValue);
                jsonArray2D.Add(jsonRow);
            }

            return jsonArray2D;
        }

        // 分割多个数组格式，如 "[1,2],[3,4]" -> ["[1,2]", "[3,4]"]
        private string[] SplitMultipleArrays(string arrayString)
        {
            List<string> arrays = new List<string>();
            int bracketCount = 0;
            int startIndex = 0;

            for (int i = 0; i < arrayString.Length; i++)
            {
                if (arrayString[i] == '[')
                {
                    bracketCount++;
                }
                else if (arrayString[i] == ']')
                {
                    bracketCount--;
                    // 当括号匹配完成，且后面有逗号时，分割数组
                    if (bracketCount == 0)
                    {
                        // 获取当前数组
                        string currentArray = arrayString.Substring(startIndex, i - startIndex + 1).Trim();
                        arrays.Add(currentArray);

                        // 寻找下一个数组的起始位置
                        if (i + 1 < arrayString.Length && arrayString[i + 1] == ',')
                        {
                            startIndex = i + 2; // 跳过逗号
                            i++; // 跳过逗号
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            return arrays.ToArray();
        }

        // 解析double行数据
        private JsonData ParseDoubleRow(string rowString)
        {
            JsonData jsonRow = new JsonData();
            string processedRow = rowString.Trim();

            // 移除行首尾的方括号（如果存在）
            if (processedRow.StartsWith("[") && processedRow.EndsWith("]"))
            {
                processedRow = processedRow.Substring(1, processedRow.Length - 2);
            }

            string[] elements = processedRow.Split(',');

            foreach (string element in elements)
            {
                string trimmedElement = element.Trim();
                if (double.TryParse(trimmedElement, out double doubleValue))
                {
                    jsonRow.Add(doubleValue);
                }
                else
                {
                    jsonRow.Add(0.0);
                }
            }

            return jsonRow;
        }

        // 解析int行数据
        private JsonData ParseIntRow(string rowString)
        {
            JsonData jsonRow = new JsonData();
            string processedRow = rowString.Trim();

            // 移除行首尾的方括号（如果存在）
            if (processedRow.StartsWith("[") && processedRow.EndsWith("]"))
            {
                processedRow = processedRow.Substring(1, processedRow.Length - 2);
            }

            string[] elements = processedRow.Split(',');

            foreach (string element in elements)
            {
                string trimmedElement = element.Trim();
                if (int.TryParse(trimmedElement, out int intValue))
                {
                    jsonRow.Add(intValue);
                }
                else
                {
                    jsonRow.Add(0);
                }
            }

            return jsonRow;
        }
        // 解析字符串到二维字符串数组（修复版）
        private JsonData ParseStringToStringArray2D(string stringValue)
        {
            JsonData jsonArray2D = new JsonData();
            if (string.IsNullOrEmpty(stringValue))
            {
                jsonArray2D.SetJsonType(JsonType.Array);
                return jsonArray2D;
            }

            string processedValue = stringValue.Trim();

            // 处理完整的二维数组格式 [[...],[...]]
            if (processedValue.StartsWith("[[") && processedValue.EndsWith("]]"))
            {
                processedValue = processedValue.Substring(2, processedValue.Length - 4);
                return ParseRows(processedValue, true);
            }
            // 处理不完整但包含多个数组的格式 [...],[...]
            else if (processedValue.Contains("],["))
            {
                return ParseRows(processedValue, false);
            }
            // 处理单行数组或简单分隔格式
            else
            {
                return ParseSimple2DArray(processedValue);
            }
        }

        // 解析行数据
        private JsonData ParseRows(string rowData, bool isFullFormat)
        {
            JsonData jsonArray2D = new JsonData();

            string[] rows;
            if (isFullFormat)
            {
                rows = rowData.Split(new[] { "],[" }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                // 对于 "[456,789],[111,你好]" 这种情况，需要更精确的分割
                List<string> rowList = new List<string>();
                int bracketCount = 0;
                int startIndex = 0;

                for (int i = 0; i < rowData.Length; i++)
                {
                    if (rowData[i] == '[')
                    {
                        bracketCount++;
                    }
                    else if (rowData[i] == ']')
                    {
                        bracketCount--;
                        // 当括号匹配完成时，分割行
                        if (bracketCount == 0 && i + 1 < rowData.Length && rowData[i + 1] == ',')
                        {
                            string row = rowData.Substring(startIndex, i - startIndex + 1);
                            rowList.Add(row);
                            startIndex = i + 2; // 跳过 "],"
                            i++; // 跳过逗号
                        }
                    }
                }

                // 添加最后一行
                if (startIndex < rowData.Length)
                {
                    string lastRow = rowData.Substring(startIndex);
                    rowList.Add(lastRow);
                }

                rows = rowList.ToArray();
            }

            foreach (string row in rows)
            {
                JsonData jsonRow = ParseSingleRow(row.Trim());
                if (jsonRow.Count > 0)
                {
                    jsonArray2D.Add(jsonRow);
                }
            }

            return jsonArray2D;
        }

        // 解析单行数据
        private JsonData ParseSingleRow(string rowString)
        {
            JsonData jsonRow = new JsonData();

            string processedRow = rowString.Trim();

            // 移除行首尾的方括号
            if (processedRow.StartsWith("[") && processedRow.EndsWith("]"))
            {
                processedRow = processedRow.Substring(1, processedRow.Length - 2);
            }

            string[] elements = SplitArrayElements(processedRow);

            foreach (string element in elements)
            {
                string cleanedElement = CleanStringElement(element.Trim());
                jsonRow.Add(cleanedElement);
            }

            return jsonRow;
        }

        // 解析简单的二维数组（分号分隔等）
        private JsonData ParseSimple2DArray(string stringValue)
        {
            JsonData jsonArray2D = new JsonData();

            string[] rows = stringValue.Split(';');

            foreach (string row in rows)
            {
                JsonData jsonRow = ParseSingleRow(row.Trim());
                if (jsonRow.Count > 0)
                {
                    jsonArray2D.Add(jsonRow);
                }
            }

            return jsonArray2D;
        }

        // 分割数组元素（保持原来的逻辑）
        private string[] SplitArrayElements(string rowString)
        {
            if (string.IsNullOrEmpty(rowString))
                return new string[0];

            List<string> elements = new List<string>();
            int startIndex = 0;
            bool inQuotes = false;
            char quoteChar = '"';

            for (int i = 0; i < rowString.Length; i++)
            {
                char currentChar = rowString[i];

                if (currentChar == '"' || currentChar == '\'')
                {
                    if (!inQuotes)
                    {
                        inQuotes = true;
                        quoteChar = currentChar;
                    }
                    else if (currentChar == quoteChar)
                    {
                        inQuotes = false;
                    }
                }
                else if (currentChar == ',' && !inQuotes)
                {
                    string element = rowString.Substring(startIndex, i - startIndex).Trim();
                    if (!string.IsNullOrEmpty(element))
                    {
                        elements.Add(element);
                    }
                    startIndex = i + 1;
                }
            }

            // 添加最后一个元素
            if (startIndex < rowString.Length)
            {
                string lastElement = rowString.Substring(startIndex).Trim();
                if (!string.IsNullOrEmpty(lastElement))
                {
                    elements.Add(lastElement);
                }
            }

            return elements.ToArray();
        }

        // 清理字符串元素
        private string CleanStringElement(string element)
        {
            if (string.IsNullOrEmpty(element))
                return element;

            // 只有当元素完全被匹配的引号包围时才移除引号
            if ((element.StartsWith("\"") && element.EndsWith("\"")) ||
                (element.StartsWith("'") && element.EndsWith("'")))
            {
                return element.Substring(1, element.Length - 2);
            }

            return element;
        }
    }
}
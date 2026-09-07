using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using UFXlsx.Main;

public class TsDeclareGenerator
{
    private static TsDeclareGenerator _instance;
    public static TsDeclareGenerator Instance => _instance ??= new TsDeclareGenerator();

    private readonly List<string> _interfaceDefinitions;
    private readonly Dictionary<string, string> _structureToInterfaceMap;

    private TsDeclareGenerator()
    {
        _interfaceDefinitions = new List<string>();
        _structureToInterfaceMap = new Dictionary<string, string>();
    }

    public string GenerateFromJsonData(JsonData data, string rootInterfaceName = "RootObject")
    {
        _interfaceDefinitions.Clear();
        _structureToInterfaceMap.Clear();

        var rootTypeDescriptor = ProcessNode(data, rootInterfaceName, isRoot: true);

        var sb = new StringBuilder();

        // 先添加所有接口定义
        foreach (var interfaceDef in _interfaceDefinitions)
        {
            sb.AppendLine(interfaceDef);
            sb.AppendLine();
        }

        // 添加根接口
        sb.AppendLine($"export interface {rootInterfaceName} {rootTypeDescriptor}");
        return sb.ToString();
    }

    private string ProcessNode(JsonData node, string parentName, bool isRoot = false, bool isArrayElement = false)
    {
        if (node == null) return "any";

        switch (node.GetJsonType())
        {
            case JsonType.Object:
                return ProcessObject(node, parentName, isRoot, isArrayElement);

            case JsonType.Array:
                return ProcessArray(node, parentName);

            case JsonType.String:
                string value_str = node.ToString();
                if (value_str.StartsWith(SpecicalTypePrefix.objectPrefix))
                {
                    return "any";
                }
                else if (value_str.StartsWith(SpecicalTypePrefix.intArrayPrefix))
                {
                    return "number[]";
                }
                else if (value_str.StartsWith(SpecicalTypePrefix.doubleArrayPrefix)){
                    return "number[]";
                }
                else if (value_str.StartsWith(SpecicalTypePrefix.doubleArray2DPrefix)){
                    return "number[][]";
                }
                else if (value_str.StartsWith(SpecicalTypePrefix.intArray2DPrefix))
                {
                    return "number[][]";
                }
                else if (value_str.StartsWith(SpecicalTypePrefix.stringArrayPrefix))
                {
                    return "string[]";
                }
                else if (value_str.StartsWith(SpecicalTypePrefix.stringArray2DPrefix))
                {
                    return "string[][]";
                }
                return "string";

            case JsonType.Int:
            case JsonType.Long:
            case JsonType.Double:
                return "number";

            case JsonType.Boolean:
                return "boolean";

            default:
                return "any";
        }
    }

    private string ProcessObject(JsonData objectNode, string parentName, bool isRoot, bool isArrayElement)
    {
        if (objectNode.GetJsonType() != JsonType.Object) return "any";

        string structureHash = GenerateStructureHash(objectNode);

        // 确定接口命名策略
        string interfaceName;
        bool shouldCreateInterface = false;

        if (isRoot)
        {
            // 根对象直接内联
            return BuildObjectDefinition(objectNode, parentName);
        }
        else if (_structureToInterfaceMap.TryGetValue(structureHash, out var existingName))
        {
            // 已有相同结构，使用现有名称
            return existingName;
        }
        else if (isArrayElement)
        {
            // 数组中的对象：parentName + "Item"
            interfaceName = $"{parentName}";
            shouldCreateInterface = true;
        }
        else
        {
            // 普通对象字段：使用字段名
            interfaceName = parentName;
            shouldCreateInterface = true;
        }

        if (shouldCreateInterface)
        {
            _structureToInterfaceMap[structureHash] = interfaceName;
            string objectDefinition = BuildObjectDefinition(objectNode, interfaceName);
            _interfaceDefinitions.Add($"export interface {interfaceName} {objectDefinition}");
            return interfaceName;
        }

        return BuildObjectDefinition(objectNode, parentName);
    }

    private bool IsStrIsNumber(string str)
    {
        if (int.TryParse(str, out int numberValue))
        {
            return true;
        }
        return false;
    }

    private string BuildObjectDefinition(JsonData objectNode, string interfaceName)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");

        // 统计所有对象和数组子元素的结构hash
        var childDeclareHashes = new Dictionary<string, int>();
        bool isAllKeyNumber = true;
        foreach (KeyValuePair<string, JsonData> kvp in objectNode)
        {
            var value = kvp.Value;
            if (value != null)
            {
                string hash = GenerateStructureHash(value);
                if (!childDeclareHashes.ContainsKey(hash))
                    childDeclareHashes[hash] = 0;
                childDeclareHashes[hash]++;
            }
            if (!IsStrIsNumber(kvp.Key))
            {
                isAllKeyNumber = false;
            }
            //else if (value != null && value.GetJsonType() == JsonType.Array)
            //{
            //    // 统计数组元素结构hash
            //    string arrHash = GenerateStructureHash(value[0]);
            //    if (!childDeclareHashes.ContainsKey(arrHash))
            //        childDeclareHashes[arrHash] = 0;
            //    childDeclareHashes[arrHash]++;
            //}
        }

        // 判断是否所有对象子元素结构一致
        bool allObjectChildrenSame;
        if (objectNode.IsArray)
        {
            allObjectChildrenSame = true;
        }
        else
        {
            allObjectChildrenSame = isAllKeyNumber || childDeclareHashes.Count == 1 && objectNode.Count >= 2;
        }
        foreach (KeyValuePair<string, JsonData> kvp in objectNode)
        {
            string key = kvp.Key;
            JsonData value = kvp.Value;

            string fieldType;
            if (value != null && value.GetJsonType() == JsonType.Object)
            {
                fieldType = ProcessNode(value, allObjectChildrenSame ? interfaceName + "_item" : key, isArrayElement: false);
            }
            else if (value != null && value.GetJsonType() == JsonType.Array && value.Count > 0 && value[0].GetJsonType() == JsonType.Object)
            {
                // 判断该数组元素结构hash是否和所有数组子元素结构hash一致
                string arrHash = GenerateStructureHash(value[0]);
                bool arrAllSame = true;
                for (int i = 1; i < value.Count; i++)
                {
                    if (GenerateStructureHash(value[i]) != arrHash)
                    {
                        arrAllSame = false;
                        break;
                    }
                }
                fieldType = ProcessNode(value, allObjectChildrenSame ? interfaceName + "_item" : key, isArrayElement: false);
            }
            else
            {
                fieldType = ProcessNode(value, key, isArrayElement: false);
            }

            sb.AppendLine($"  {EscapeKeyIfNeeded(key)}: {fieldType};");
        }
        sb.Append("}");

        return sb.ToString();
    }

    private string ProcessArray(JsonData arrayNode, string parentName)
    {
        if (arrayNode.GetJsonType() != JsonType.Array || arrayNode.Count == 0)
        {
            return "any[]";
        }

        // 检查数组元素是否都是相同结构的对象
        JsonData firstElement = arrayNode[0];
        if (firstElement.GetJsonType() == JsonType.Object)
        {
            string structureHash = GenerateStructureHash(firstElement);
            bool allSameStructure = true;

            for (int i = 1; i < arrayNode.Count; i++)
            {
                if (arrayNode[i].GetJsonType() != JsonType.Object ||
                    GenerateStructureHash(arrayNode[i]) != structureHash)
                {
                    allSameStructure = false;
                    break;
                }
            }

            if (allSameStructure)
            {
                // 处理数组中的对象元素
                string elementType = ProcessNode(firstElement, parentName, isArrayElement: true);
                return $"{elementType}[]";
            }
        }

        // 混合类型数组
        HashSet<string> elementTypes = new HashSet<string>();
        for (int i = 0; i < arrayNode.Count; i++)
        {
            string itemType = ProcessNode(arrayNode[i], parentName);
            elementTypes.Add(itemType);
        }

        return elementTypes.Count == 1
            ? $"{GetFirstElement(elementTypes)}[]"
            : $"({string.Join(" | ", elementTypes)})[]";
    }

    private string GenerateStructureHash(JsonData node)
    {
        if (node == null) return "null";

        switch (node.GetJsonType())
        {
            case JsonType.Object:
                var keyTypeList = new List<string>();
                foreach (KeyValuePair<string, JsonData> kvp in node)
                {
                    string key = kvp.Key;
                    string typeDesc = GenerateStructureHash(kvp.Value);
                    keyTypeList.Add($"{key}:{typeDesc}");
                }
                keyTypeList.Sort();
                return $"{{{string.Join(",", keyTypeList)}}}";

            case JsonType.Array:
                if (node.Count == 0)
                    return "Array<any>";
                // 合并所有元素的结构hash，去重后排序
                var elementHashes = new HashSet<string>();
                for (int i = 0; i < node.Count; i++)
                {
                    elementHashes.Add(GenerateStructureHash(node[i]));
                }
                var sorted = new List<string>(elementHashes);
                sorted.Sort();
                return $"Array<{string.Join("|", sorted)}>";

            case JsonType.String:
                return "string";

            case JsonType.Int:
            case JsonType.Long:
            case JsonType.Double:
                return "number";

            case JsonType.Boolean:
                return "boolean";

            default:
                return "any";
        }
    }

    private string GetFirstElement<T>(HashSet<T> set)
    {
        foreach (var item in set)
        {
            return item.ToString();
        }
        return "any";
    }

    private string EscapeKeyIfNeeded(string key)
    {
        if (string.IsNullOrEmpty(key)) return "\"\"";

        // 检查是否是有效的标识符
        if (!char.IsLetter(key[0]) && key[0] != '_')
        {
            return $"\"{key}\"";
        }

        for (int i = 1; i < key.Length; i++)
        {
            if (!char.IsLetterOrDigit(key[i]) && key[i] != '_')
            {
                return $"\"{key}\"";
            }
        }

        return key;
    }

    private string CapitalizeFirstLetter(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return char.ToUpper(str[0]) + str.Substring(1);
    }
}
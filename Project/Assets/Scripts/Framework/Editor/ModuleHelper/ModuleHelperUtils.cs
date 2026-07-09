using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Editor.ModuleHelpes
{
    public class ModuleHelperUtils
    {
        // 检测模块命名合法性
        public static string CheckModuleName(string name)
        {
            if (char.IsLower(name[0]))
            {
                return "模块名首字母应当大写";
            }
            if (char.IsNumber(name[0]))
            {
                return "模块名不能以数字开头";
            }
            var idx = name.IndexOfAny(Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()).ToArray());
            if (idx >= 0)
            {
                return string.Format("文件名包含非法字符{0}", name[idx]);
            }
            return null;
        }

        // 读取模板文件，字符串替换化后写入新文件
        public static void CreateAndWriteFileByTemplate(string oldFilePath, string newFilePath, string oldStr, string newStr)
        {
            string content = File.ReadAllText(oldFilePath);
            content = content.Replace(oldStr, newStr);
            if (!File.Exists(newFilePath))
            {
                File.WriteAllText(newFilePath, content, Encoding.GetEncoding("utf-8"));
            }
        }
    }
}
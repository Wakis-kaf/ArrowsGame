using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UFXlsx.Main
{
    public class TemplateExporter : Single<TemplateExporter>
    {
        private Dictionary<string, string> m_Key2Value;
        private String m_ExportTemplate;
        private Regex m_KeyWordRex = new Regex(@"\*\*([a-zA-Z0-9]+)\*\*");

        public TemplateExporter()
        {
            m_Key2Value = new Dictionary<string, string>();
            // 先读取本地txt
            m_ExportTemplate = System.IO.File.ReadAllText(@".\ExportTemplate.txt");
        }

        public void Clear()
        {
            m_Key2Value.Clear();
        }

        public void SetValue(string name, string value)
        {
            if (!m_Key2Value.ContainsKey(name))
                m_Key2Value.Add(name, value);
            m_Key2Value[name] = value;
        }

        public string GetValue(string name)
        {
            m_Key2Value.TryGetValue(name, out string value);
            return value;
        }

        public string GetExportTxt()
        {
            StringBuilder res = new StringBuilder(m_ExportTemplate);
            foreach (Match item in m_KeyWordRex.Matches(m_ExportTemplate))
            {
                string value = GetValue(item.Value.Substring(2, item.Length - 4));
                res.Replace(item.Value, value);
            }
            return res.ToString();
        }

        public void SetTemplatePath(string path)
        {
            if (File.Exists(path))
            {
                ExporterEnvironment.Log($"开始读取导出模板配置: 导出:【{path}】\n");
                m_ExportTemplate = System.IO.File.ReadAllText(path);
            }
        }
    }
}
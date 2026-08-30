using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFXlsx.Main
{
    public class ExporterDatabase : Single<ExporterDatabase>
    {
        private Dictionary<string, string> content2NameDeclareMap = new Dictionary<string, string>();

        public void Clear()
        {
            content2NameDeclareMap.Clear();
        }

        public string MergeTSDeclareContent(string content, string declareName)
        {
            if (content2NameDeclareMap.ContainsKey(content))
            {
                return content2NameDeclareMap[content];
            }
            content2NameDeclareMap.Add(content, declareName);
            return declareName;
        }
    }
}
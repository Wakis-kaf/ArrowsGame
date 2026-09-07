using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFXlsx.ConfigJsonTemplate
{
    public class ExportSheetGroupJT
    {
        public string groupField; // 分组字段
        public bool isRemoveColumnFromPublic = true;
        public int[] includeColumnList = Array.Empty<int>(); // 需要分组的列标
        public int[] notIncludeColumnList = Array.Empty<int>(); // 需要分组的列标
        public ExportSheetGroupJT[] exportGroupList = Array.Empty<ExportSheetGroupJT>(); // 子分组

    }
}

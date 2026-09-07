using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFXlsx.ConfigJsonTemplate
{
    public class GlobalExportConfigJT
    {
        public ExcelExportJT[] globalExportList = Array.Empty<ExcelExportJT>();
        public ExcelExportJT[] singleExportList = Array.Empty<ExcelExportJT>();
    }
}

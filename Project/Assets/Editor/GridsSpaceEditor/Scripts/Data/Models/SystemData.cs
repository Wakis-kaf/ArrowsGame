using System;
using System.Collections.Generic;

namespace GridsSpaceEditor.Data.Models
{
    [Serializable]
    public class SystemData
    {
        public List<string> GridTypes = new List<string>();
        public List<GridCellData> Templates = new List<GridCellData>();
        public List<bool> TypeLocks = new List<bool>();
        public List<bool> TemplateLocks = new List<bool>();
        public string ExportFolderPath = "Assets/Editor/GridsSpaceEditor/Export/";
        /// <summary>上次导出时的文件夹路径</summary>
        public string LastExportPath = "";
        /// <summary>上次导入时的文件夹路径</summary>
        public string LastImportPath = "";
        public bool CenterAlignment = true;
        /// <summary>网格编辑模式下：为 true 时显示所有格子上已开启的端口；为 false 时仅显示当前选中格子的端口。</summary>
        public bool ShowAllPortsInGridEdit = true;
        /// <summary>在网格视图中显示端口信息文本标签</summary>
        public bool ShowPortLabels = false;
        /// <summary>端口标签字号（默认9）</summary>
        public int PortLabelFontSize = 6;
        public PortLibraryData PortLibrary = new PortLibraryData();
        public event System.Action OnSystemDataChanged;
    }
}

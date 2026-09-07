using Microsoft.Win32;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ExporterEnvironment = UFXlsx.Main.ExporterEnvironment;
namespace UFXlsx
{
    public class FileUtil
    {
        public enum FileType
        {
            None,
            Lua,
            Xml,
            Cs
        }

        private static SaveFileDialog _saveDialog;
        private static OpenFileDialog _openDialog;
        private readonly static string _fileLua = ".lua";
        private readonly static string _fileXML = ".xml";
        private readonly static string _fileCs = ".cs";
        /// <summary>
        /// 根据后缀名得到文件类型
        /// </summary>
        /// <param name="fileExtension"></param>
        /// <returns></returns>
        public static FileType GetFileTypeByExtension(string fileExtension)
        {
            FileType type = FileType.None;
            if (fileExtension.Equals(_fileLua))
            {
                type = FileType.Lua;
            }
            else if (fileExtension.Equals(_fileXML))
            {
                type = FileType.Xml;
            }
            else if (fileExtension.Equals(_fileCs))
            {
                type = FileType.Cs;
            }
            return type;
        }

        /// <summary>
        /// 判断当前操作文件是否是Excel文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="needLog"></param>
        /// <returns></returns>
        public static bool CheckOperatingFileIsExcel(string path, bool needLog = true)
        {
            if (File.Exists(path))
            {
                FileInfo info = new FileInfo(path);
                if (info.Extension.Equals(".xls") || info.Extension.Equals(".xlsx"))
                {
                    if (needLog)
                    {
                        ExporterEnvironment.Log($"当前选择Excel文件：{path}");
                    }
                    return true;
                }
            }
            if (needLog)
            {
                ExporterEnvironment.Log($"当前选中文件非Excel文件:{path}\n请重新选择!!!!");
            }
            return false;
        }

        /// <summary>
        /// 判断当前操作路径是否合法
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool CheckChoosePathLegal(string path)
        {
            bool isLegalPath = false;
            if (Directory.Exists(path))
            {
                ExporterEnvironment.Log($"当前选择文件夹：{path}");
                isLegalPath = true;
            }
            else if (File.Exists(path))
            {
                ExporterEnvironment.Log($"当前选择文件：{path}");
                isLegalPath = true;
            }
            else
            {
                ExporterEnvironment.Log($"无法解析当前选中内容：{path}\n请重新选择!!!!");
            }
            return isLegalPath;
        }

        public static bool CheckChooseFolderLegal(string path)
        {
            bool isLegalPath = false;
            if (Directory.Exists(path))
            {
                ExporterEnvironment.Log($"当前选择文件夹：{path}");
                isLegalPath = true;
            }
            return isLegalPath;
        }

        public static string ReadFileContent(string filePath)
        {
            return ReadFileContent(new FileInfo(filePath));
        }
        public static string ReadFileContent(FileInfo info)
        {
            TextReader reader = info.OpenText();
            string fileText = reader.ReadToEnd();
            reader.Close();
            return fileText;
        }

        /// <summary>
        /// 检查拖拽进来的是否是文件或文件夹
        /// </summary>
        /// <param name="e"></param>
        public static void CheckTextBoxDrag(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) //判断拖来的是否是文件
                e.Effects = DragDropEffects.Link; //是则将拖动源中的数据连接到控件
            else
                e.Effects = DragDropEffects.None;
        }

        public static void SaveDataByExcel(IWorkbook workbook)
        {
            if (_saveDialog == null)
            {
                _saveDialog = new SaveFileDialog();
                _saveDialog.AddExtension = true;
                _saveDialog.DefaultExt = ".xlsx";
                _saveDialog.OverwritePrompt = true;
                _saveDialog.Filter = @"Excel(*.xlsx,*.xls)|*.xlsx;*.xls";
                _saveDialog.RestoreDirectory = false;
            }

            if (_saveDialog.ShowDialog() == true)
            {
                using (FileStream fs = File.OpenWrite(_saveDialog.FileName))
                {
                    workbook.Write(fs,false);
                    fs.Dispose();
                    fs.Close();
                }
            }
        }
        public static string GetXMLPathByDialog()
        {
            if (_openDialog == null)
            {
                _openDialog = new OpenFileDialog();
                _openDialog.Multiselect = true;
                //文件格式
                _openDialog.Filter = "XML Files|*.xml";
                //还原当前目录
                _openDialog.RestoreDirectory = true;
            }
            if (_openDialog.ShowDialog() == true)
            {
                return _openDialog.FileName;
            }
            return string.Empty;
        }
        public static string GetJSONPathByDialog()
        {
            if (_openDialog == null)
            {
                _openDialog = new OpenFileDialog();
                _openDialog.Multiselect = true;
                //文件格式
                _openDialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt";
                //还原当前目录
                _openDialog.RestoreDirectory = true;
            }
            if (_openDialog.ShowDialog() == true)
            {
                return _openDialog.FileName;
            }
            return string.Empty;
        }
        public static string GetExcelPathByDialog()
        {
            if (_openDialog == null)
            {
                _openDialog = new OpenFileDialog();
                _openDialog.Multiselect = true;
                //文件格式
                _openDialog.Filter = @"Excel(*.xlsx,*.xls)|*.xlsx;*.xls";
                //还原当前目录
                _openDialog.RestoreDirectory = true;
            }
            if (_openDialog.ShowDialog() == true)
            {
                return _openDialog.FileName;
            }
            return string.Empty;
        }
    }
}

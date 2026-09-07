using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using UFXlsx.Main;
using ExporterEnvironment = UFXlsx.Main.ExporterEnvironment;

namespace UFXlsx
{
    /// <summary>
    /// 配置表项数据模型
    /// </summary>
    public class ConfigTableItem : ObservableObject
    {
        private string _name;
        private bool _isSelected;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value))
                {
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler SelectionChanged;

        public ConfigTableItem(string name, bool isSelected = true)
        {
            Name = name;
            IsSelected = isSelected;
        }
    }

    /// <summary>
    /// 简单的可观察对象基类
    /// </summary>
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<ConfigTableItem> _configTables = new ObservableCollection<ConfigTableItem>();
        private string _currentConfigPath;
        private string _excelDir;
        private DispatcherTimer _selectionTimer;

        public MainWindow()
        {
            InitializeComponent();
            ExporterEnvironment.SetLogAgent(Log);
            ExporterEnvironment.SetLogErrorAgent(LogError);

            configTableList.ItemsSource = _configTables;
            _configTables.CollectionChanged += ConfigTables_CollectionChanged;

            // 初始化选择变化监听器
            _selectionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _selectionTimer.Tick += SelectionTimer_Tick;
        }

        private void ConfigTables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateSelectedCount();

            // 当添加或移除项目时，添加或移除事件监听
            if (e.NewItems != null)
            {
                foreach (ConfigTableItem item in e.NewItems)
                {
                    item.SelectionChanged += ConfigTableItem_SelectionChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ConfigTableItem item in e.OldItems)
                {
                    item.SelectionChanged -= ConfigTableItem_SelectionChanged;
                }
            }
        }

        private void ConfigTableItem_SelectionChanged(object sender, EventArgs e)
        {
            // 当单个项目选择状态变化时，重启计时器
            _selectionTimer.Stop();
            _selectionTimer.Start();
        }

        private void SelectionTimer_Tick(object sender, EventArgs e)
        {
            UpdateSelectedCount();
            _selectionTimer.Stop();
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        private async Task LoadConfigFile(string configPath)
        {
            try
            {
                if (!File.Exists(configPath))
                {
                    LogError($"配置文件不存在: {configPath}");
                    return;
                }

                _currentConfigPath = configPath;
                Dispatcher.Invoke(() =>
                {
                    globalConfigPath.Text = configPath;
                    configStatus.Text = "正在解析配置文件...";
                    configStatus.Foreground = new SolidColorBrush(Colors.Orange);
                });

                // 清空现有配置表
                Dispatcher.Invoke(() =>
                {
                    // 先移除所有事件监听
                    foreach (var item in _configTables)
                    {
                        item.SelectionChanged -= ConfigTableItem_SelectionChanged;
                    }
                    _configTables.Clear();
                });

                // 异步读取并解析JSON文件
                await Task.Run(() =>
                {
                    try
                    {
                        // 读取GlobalConfig.json
                        string jsonContent = File.ReadAllText(configPath);
                        JsonData configData = JsonMapper.ToObject(jsonContent);

                        // 获取excelDir目录
                        if (!configData.IsObject)
                        {
                            throw new Exception("GlobalConfig.json格式不正确");
                        }

                        IDictionary configDict = (IDictionary)configData;
                        if (!configDict.Contains("excelDir"))
                        {
                            throw new Exception("GlobalConfig.json中缺少excelDir字段");
                        }

                        _excelDir = ((JsonData)configDict["excelDir"]).ToString();

                        // 构建GlobalExportConfig.json路径
                        string exportConfigPath = Path.Combine(_excelDir, "GlobalExportConfig.json");
                        if (!File.Exists(exportConfigPath))
                        {
                            throw new Exception($"GlobalExportConfig.json不存在于: {exportConfigPath}");
                        }

                        // 读取GlobalExportConfig.json
                        string exportConfigContent = File.ReadAllText(exportConfigPath);
                        JsonData exportConfigData = JsonMapper.ToObject(exportConfigContent);

                        // 解析singleExportList
                        List<string> tableNames = ParseSingleExportList(exportConfigData);

                        // 在UI线程上更新
                        Dispatcher.Invoke(() =>
                        {
                            if (tableNames.Count == 0)
                            {
                                configStatus.Text = "未找到配置表信息";
                                configStatus.Foreground = new SolidColorBrush(Colors.Red);
                                configTableCard.Visibility = Visibility.Collapsed;
                                LogError("GlobalExportConfig.json中未找到有效的配置表信息");
                            }
                            else
                            {
                                // 添加找到的表名
                                foreach (var tableName in tableNames.Distinct().OrderBy(name => name))
                                {
                                    var configItem = new ConfigTableItem(tableName);
                                    configItem.SelectionChanged += ConfigTableItem_SelectionChanged;
                                    _configTables.Add(configItem);
                                }

                                configStatus.Text = $"已加载 {_configTables.Count} 个配置表";
                                configStatus.Foreground = new SolidColorBrush(Colors.Green);
                                configTableCard.Visibility = Visibility.Visible;

                                // 默认全选
                                BtnSelectAll_Click(null, null);

                                Log($"配置文件加载成功，找到 {_configTables.Count} 个配置表");
                                Log($"Excel目录: {_excelDir}");
                                Log($"导出配置: {exportConfigPath}");
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            LogError($"加载配置文件失败: {ex.Message}");
                            configStatus.Text = "配置文件加载失败";
                            configStatus.Foreground = new SolidColorBrush(Colors.Red);
                            configTableCard.Visibility = Visibility.Collapsed;
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                LogError($"加载配置文件失败: {ex.Message}");
                Dispatcher.Invoke(() =>
                {
                    configStatus.Text = "配置文件加载失败";
                    configStatus.Foreground = new SolidColorBrush(Colors.Red);
                    configTableCard.Visibility = Visibility.Collapsed;
                });
            }
        }

        /// <summary>
        /// 从GlobalExportConfig.json中解析singleExportList
        /// </summary>
        private List<string> ParseSingleExportList(JsonData exportConfigData)
        {
            List<string> tableNames = new List<string>();

            if (exportConfigData == null || !exportConfigData.IsObject)
            {
                return tableNames;
            }

            try
            {
                IDictionary dict = (IDictionary)exportConfigData;

                // 检查是否有singleExportList字段
                if (!dict.Contains("singleExportList"))
                {
                    LogError("GlobalExportConfig.json中缺少singleExportList字段");
                    return tableNames;
                }

                JsonData singleExportList = (JsonData)dict["singleExportList"];
                if (!singleExportList.IsArray)
                {
                    LogError("singleExportList不是数组类型");
                    return tableNames;
                }

                // 遍历singleExportList数组
                for (int i = 0; i < singleExportList.Count; i++)
                {
                    JsonData item = singleExportList[i];
                    if (item.IsObject)
                    {
                        IDictionary itemDict = (IDictionary)item;
                        if (itemDict.Contains("excelPath") && ((JsonData)itemDict["excelPath"]).IsString)
                        {
                            string excelPath = ((JsonData)itemDict["excelPath"]).ToString();
                            if (!string.IsNullOrEmpty(excelPath))
                            {
                                tableNames.Add(excelPath);
                            }
                        }
                    }
                }

                Log($"从singleExportList中解析到 {tableNames.Count} 个配置表");
            }
            catch (Exception ex)
            {
                LogError($"解析singleExportList失败: {ex.Message}");
            }

            return tableNames;
        }

        /// <summary>
        /// 更新选中计数
        /// </summary>
        private void UpdateSelectedCount()
        {
            if (_configTables == null) return;

            int selectedCount = _configTables.Count(item => item.IsSelected);
            int totalCount = _configTables.Count;

            // 更新选中数量文本
            selectedCountText.Text = $"已选中 {selectedCount} / {totalCount} 个配置表";

            // 更新按钮文本
            if (selectedCount == 0)
            {
                selectedCountText.Foreground = new SolidColorBrush(Colors.Orange);
                btnOutputSelected.Content = "导出配置表";
            }
            else if (selectedCount == totalCount)
            {
                selectedCountText.Foreground = new SolidColorBrush(Colors.Green);
                btnOutputSelected.Content = "导出全部配置表";
            }
            else
            {
                selectedCountText.Foreground = new SolidColorBrush(Colors.Green);
                btnOutputSelected.Content = $"导出选中配置表 ({selectedCount})";
            }
        }

        /// <summary>
        /// 配置文件拖放
        /// </summary>
        private void GlobalConfig_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files != null && files.Length > 0)
                    {
                        string fileName = files[0];
                        if (Path.GetExtension(fileName).Equals(".json", StringComparison.OrdinalIgnoreCase))
                        {
                            _ = LoadConfigFile(fileName);
                        }
                        else
                        {
                            LogError("请选择JSON格式的配置文件");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"文件拖放失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GlobalConfig_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length == 1 &&
                    Path.GetExtension(files[0]).Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    e.Effects = DragDropEffects.Link;
                    globalConfigPath.Background = new SolidColorBrush(Color.FromArgb(30, 33, 150, 243));
                    return;
                }
            }
            e.Effects = DragDropEffects.None;
        }

        private void GlobalConfig_DragLeave(object sender, DragEventArgs e)
        {
            globalConfigPath.Background = new SolidColorBrush(Color.FromArgb(255, 250, 250, 250));
        }

        /// <summary>
        /// 点击配置文件区域打开文件选择
        /// </summary>
        private void GlobalConfigPath_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BtnBrowseClick(sender, e);
        }

        /// <summary>
        /// 浏览配置文件
        /// </summary>
        private void BtnBrowseClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "JSON配置文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                    Title = "选择 GlobalConfig.json 文件",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    _ = LoadConfigFile(openFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                LogError($"打开文件对话框失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 全选
        /// </summary>
        private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _configTables)
            {
                item.IsSelected = true;
            }
            UpdateSelectedCount();
        }

        /// <summary>
        /// 取消全选
        /// </summary>
        private void BtnUnselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _configTables)
            {
                item.IsSelected = false;
            }
            UpdateSelectedCount();
        }

        /// <summary>
        /// 导出选中配置表
        /// </summary>
        private void BtnOutputSelected_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentConfigPath))
            {
                LogError("请先选择配置文件");
                return;
            }

            if (string.IsNullOrEmpty(_excelDir))
            {
                LogError("未找到Excel目录配置");
                return;
            }

            var selectedTables = _configTables.Where(t => t.IsSelected).Select(t => t.Name).ToList();
            if (selectedTables.Count == 0)
            {
                LogError("请至少选择一个配置表");
                return;
            }

            Thread thread = new Thread(() => OutputSelectedTablesThread(selectedTables));
            thread.Start();
        }

        /// <summary>
        /// 导出选中配置表的线程方法
        /// </summary>
        private void OutputSelectedTablesThread(List<string> selectedTables)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    // 禁用按钮，防止重复点击
                    btnOutputSelected.IsEnabled = false;
                    btnOutputAllSingle.IsEnabled = false;
                    btnOutputSelected.Content = "导出中...";

                    Log($"开始导出 {selectedTables.Count} 个配置表...");

                    // 构建GlobalExportConfig.json路径
                    string exportConfigPath = Path.Combine(_excelDir, "GlobalExportConfig.json");
                    if (!File.Exists(exportConfigPath))
                    {
                        throw new Exception($"GlobalExportConfig.json不存在: {exportConfigPath}");
                    }

                    // 创建过滤后的配置
                    CreateFilteredExportConfig(exportConfigPath, selectedTables);

                    Log($"成功导出 {selectedTables.Count} 个配置表");
                }
                catch (Exception ex)
                {
                    LogError($"导出配置表失败: {ex.Message}");
                }
                finally
                {
                    btnOutputSelected.IsEnabled = true;
                    btnOutputAllSingle.IsEnabled = true;
                    UpdateSelectedCount();
                }
            });
        }

        /// <summary>
        /// 创建过滤后的导出配置并执行导出
        /// </summary>
        private void CreateFilteredExportConfig(string exportConfigPath, List<string> selectedTables)
        {
            try
            {
                // 读取原始GlobalExportConfig.json
                string exportConfigContent = File.ReadAllText(exportConfigPath);
                JsonData exportConfigData = JsonMapper.ToObject(exportConfigContent);

                // 创建过滤后的配置
                JsonData filteredConfig = FilterExportConfig(exportConfigData, selectedTables);

                // 将过滤后的配置保存到临时文件
                //string tempConfigPath = Path.GetTempFileName();
                //File.WriteAllText(tempConfigPath, JsonMapper.ToJson(filteredConfig));

                try
                {
                    ClearAllLog();
                    // 使用原始GlobalConfig.json和临时导出配置文件导出
                    ExcelConfigExporter.Instance.StartOuputAllByConfig(_currentConfigPath, filteredConfig);

                    // 注意：这里可能需要修改ExcelConfigExporter以支持传入过滤后的导出配置
                    // 如果无法直接修改，可以尝试其他方法
                }
                finally
                {
                    // 删除临时文件
                    //try { File.Delete(tempConfigPath); } catch { }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"创建过滤配置失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 过滤导出配置，只保留选中的表
        /// </summary>
        private JsonData FilterExportConfig(JsonData originalConfig, List<string> selectedTables)
        {
            if (originalConfig == null || !originalConfig.IsObject)
            {
                return originalConfig;
            }

            // 深拷贝原始配置
            JsonData filteredConfig = JsonMapper.ToObject(JsonMapper.ToJson(originalConfig));

            IDictionary dict = (IDictionary)filteredConfig;

            // 过滤singleExportList
            if (dict.Contains("singleExportList"))
            {
                JsonData singleExportList = (JsonData)dict["singleExportList"];
                if (singleExportList.IsArray)
                {
                    JsonData filteredList = new JsonData();

                    for (int i = 0; i < singleExportList.Count; i++)
                    {
                        JsonData item = singleExportList[i];
                        if (item.IsObject)
                        {
                            IDictionary itemDict = (IDictionary)item;
                            if (itemDict.Contains("excelPath") && ((JsonData)itemDict["excelPath"]).IsString)
                            {
                                string excelPath = ((JsonData)itemDict["excelPath"]).ToString();
                                if (selectedTables.Contains(excelPath))
                                {
                                    filteredList.Add(item);
                                }
                            }
                        }
                    }

                    dict["singleExportList"] = filteredList;
                }
            }

            return filteredConfig;
        }

        /// <summary>
        /// 直接导出选中配置表的替代方案
        /// </summary>
        private void ExportSelectedTablesDirectly(List<string> selectedTables)
        {
            try
            {
                // 构建GlobalExportConfig.json路径
                string exportConfigPath = Path.Combine(_excelDir, "GlobalExportConfig.json");
                if (!File.Exists(exportConfigPath))
                {
                    throw new Exception($"GlobalExportConfig.json不存在: {exportConfigPath}");
                }

                // 读取原始GlobalExportConfig.json
                string exportConfigContent = File.ReadAllText(exportConfigPath);
                JsonData exportConfigData = JsonMapper.ToObject(exportConfigContent);

                IDictionary dict = (IDictionary)exportConfigData;
                if (!dict.Contains("singleExportList"))
                {
                    throw new Exception("GlobalExportConfig.json中缺少singleExportList字段");
                }

                JsonData singleExportList = (JsonData)dict["singleExportList"];
                if (!singleExportList.IsArray)
                {
                    throw new Exception("singleExportList不是数组类型");
                }

                // 遍历并导出选中的表
                int exportedCount = 0;
                for (int i = 0; i < singleExportList.Count; i++)
                {
                    JsonData item = singleExportList[i];
                    if (item.IsObject)
                    {
                        IDictionary itemDict = (IDictionary)item;
                        if (itemDict.Contains("excelPath") && ((JsonData)itemDict["excelPath"]).IsString)
                        {
                            string excelPath = ((JsonData)itemDict["excelPath"]).ToString();
                            if (selectedTables.Contains(excelPath))
                            {
                                // 这里需要调用你的实际导出方法
                                // 可能需要根据你的导出器调整
                                Log($"正在导出: {excelPath}");
                                exportedCount++;
                            }
                        }
                    }
                }

                Log($"成功导出 {exportedCount} 个配置表");
            }
            catch (Exception ex)
            {
                throw new Exception($"导出失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 调试：显示JSON结构
        /// </summary>
        private void DebugJsonStructure(string filePath)
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                JsonData jsonData = JsonMapper.ToObject(jsonContent);

                Log("=== JSON结构分析 ===");
                Log($"文件: {Path.GetFileName(filePath)}");
                DebugJsonStructureRecursive(jsonData, "", "root");
                Log("=== 分析结束 ===");
            }
            catch (Exception ex)
            {
                LogError($"分析JSON结构失败: {ex.Message}");
            }
        }

        private void DebugJsonStructureRecursive(JsonData data, string indent = "", string propertyName = "")
        {
            try
            {
                if (data == null)
                {
                    Log($"{indent}{propertyName}: null");
                    return;
                }

                if (data.IsArray)
                {
                    Log($"{indent}{propertyName}: [Array, Count={data.Count}]");
                    for (int i = 0; i < Math.Min(data.Count, 5); i++) // 只显示前5个元素
                    {
                        DebugJsonStructureRecursive(data[i], indent + "  ", $"[{i}]");
                    }
                    if (data.Count > 5) Log($"{indent}  ... and {data.Count - 5} more");
                }
                else if (data.IsObject)
                {
                    Log($"{indent}{propertyName}: {{Object}}");
                    IDictionary dict = (IDictionary)data;
                    foreach (DictionaryEntry entry in dict)
                    {
                        DebugJsonStructureRecursive((JsonData)entry.Value, indent + "  ", entry.Key.ToString());
                    }
                }
                else if (data.IsString)
                {
                    string value = data.ToString();
                    if (value.Length > 50) value = value.Substring(0, 50) + "...";
                    Log($"{indent}{propertyName}: \"{value}\"");
                }
                else if (data.IsBoolean)
                {
                    Log($"{indent}{propertyName}: {data} (bool)");
                }
                else if (data.IsDouble || data.IsInt || data.IsLong)
                {
                    Log($"{indent}{propertyName}: {data} (number)");
                }
                else
                {
                    Log($"{indent}{propertyName}: Unknown type");
                }
            }
            catch (Exception ex)
            {
                Log($"{indent}{propertyName}: Error: {ex.Message}");
            }
        }

        /// <summary>
        /// 取消操作
        /// </summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // 如果需要实现取消功能，可以在这里添加
        }

        // 以下是原有的日志相关方法（保持不变）
        private delegate void PrimeDelegate(string arg, string timeStamp);

        private void Log(string msg)
        {
            Dispatcher.Invoke(DispatcherPriority.SystemIdle, new PrimeDelegate(AddExtractLog), msg, GetTimeStamp());
        }

        private void AddExtractLog(string log, string timeStamp)
        {
            TxtLogOutput.AppendText($"【INFO( {timeStamp} )】：{log}\n");
            TxtLogOutput.ScrollToEnd();
            TxtLogOutput.CaretIndex = TxtLogOutput.Text.Length; // 设置光标位置到最后
        }

        private void AddExtractLogError(string log, string timeStamp)
        {
            TxtLogOutput.AppendText($"【ERROR! ({timeStamp})】：{log}\n");
            TxtLogOutput.ScrollToEnd();
            TxtLogOutput.CaretIndex = TxtLogOutput.Text.Length; // 设置光标位置到最后
        }

        public string GetTimeStamp()
        {
            return DateTime.Now.ToString("hh:mm:ss.fff");
        }

        private void LogError(string msg)
        {
            Dispatcher.Invoke(DispatcherPriority.SystemIdle, new PrimeDelegate(AddExtractLogError), msg, GetTimeStamp());
        }

        private void BtnOutputAllSingleClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentConfigPath))
            {
                LogError("请先选择配置文件");
                return;
            }

            Thread thread = new Thread(OutputAllSingleThread);
            thread.Start();
        }

        private void OutputAllSingleThread()
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    btnOutputAllSingle.IsEnabled = false;
                    btnOutputSelected.IsEnabled = false;
                    ClearAllLog();
                    btnOutputAllSingle.Content = "导出中...";

                    ExcelConfigExporter.Instance.StartOuputAllByConfig(_currentConfigPath);
                    ExporterEnvironment.Log($"导出所有配置表成功！");
                }
                catch (Exception ex)
                {
                    ExporterEnvironment.LogError($"导出所有配置表失败: {ex}");
                }
                finally
                {
                    btnOutputAllSingle.IsEnabled = true;
                    btnOutputSelected.IsEnabled = true;
                    btnOutputAllSingle.Content = "导出全部配置表";
                }
            });
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 保持原有逻辑
        }

        private void BtnClearLogClick(object sender, RoutedEventArgs e)
        {
            ClearAllLog();
        }
        private void ClearAllLog()
        {
            TxtLogOutput.Text = "";
        }
    }
}
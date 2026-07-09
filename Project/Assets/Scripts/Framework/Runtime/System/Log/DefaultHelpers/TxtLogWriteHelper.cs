using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Framework.Runtime.LogSystem.Helper
{
    public class TxtLogWriteHelper : ILogWriteHelper
    {
        private string m_LogFileName;
        private string m_CurrentWrightPath;
        private bool m_SystemInfoHasLoged = false;
        private StreamWriter m_SW;
        private WriteOption m_WriteOption;
        private bool m_Inited = false;
        private List<FileInfo> m_OldLogFileInfos;
        private StringBuilder contentSb;

        public TxtLogWriteHelper()
        {
            contentSb = new StringBuilder();
            m_LogFileName = DateTime.Now.GetDateTimeFormats('s')[0].ToString();
            m_LogFileName = m_LogFileName.Replace("-", "_");
            m_LogFileName = m_LogFileName.Replace(":", "_");
            m_LogFileName = m_LogFileName.Replace(" ", "");
            m_LogFileName = m_LogFileName.Replace("T", "_");
            m_LogFileName = m_LogFileName + ".log";
        }

        private void LogClear(WriteOption writerOption)
        {
            if (m_OldLogFileInfos == null) return;
            if (!writerOption.clearOldLog) return;
            string fileDir = GetFullDirPath(writerOption);
            if (!Directory.Exists(fileDir)) // 如果根目录不存在就退出
            {
                Debug.LogError($"Old Log Check Error ! DirPath Not Exist {fileDir}");
                return;
            }

            var files = m_OldLogFileInfos; // 获取目录下的所有子文件
            if (writerOption.saveLogOnlyCurrent) // 删除所有的子文件
            {
                for (int i = files.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        if (files[i].FullName != m_CurrentWrightPath)
                        {
                            files[i].Delete();
                            files.RemoveAt(i);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Delete File Fail " + e);
                        //throw;
                    }
                }
            }

            if (writerOption.clearOldLog)
            {
                if (files.Count > writerOption.logFileMaxCount)
                {
                    var oldFile = files[0];
                    var lastTime = files[0].CreationTime;
                    foreach (var file in files)
                    {
                        if (lastTime > file.CreationTime)
                        {
                            oldFile = file;
                            lastTime = file.CreationTime;
                        }
                    }

                    try
                    {
                        oldFile.Delete();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("旧日志删除失败"+e);
                    }
                }
            }
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        private string GetFullDirPath(WriteOption? writeOption)
        {
            WriteOption option = (WriteOption)writeOption;
            string dirPath = option.saveDirPath;
            string writerPath = option.logWriteDirPath;
            if (writerPath.StartsWith("/"))
            {
                writerPath = writerPath.Remove(0, 1);
            }

            string fullPath = Path.Combine(dirPath, writerPath);
            return fullPath;
        }

        public void SetWriteOption(WriteOption writeOption)
        {
            m_WriteOption = writeOption;

            Init();
            LogClear(writeOption);
        }

        private void Init()
        {
            string fileDir = GetFullDirPath(m_WriteOption);
            // 如果根目录不存在就退出
            if (!Directory.Exists(fileDir))
            {
                Debug.LogError($"Old Log Check Error ! DirPath Not Exist {fileDir} ,已创建新目录{fileDir}");
                Directory.CreateDirectory(fileDir);
            }
            Debug.Log($"日志文件保存路径{Path.Combine(GetFullDirPath(m_WriteOption), m_LogFileName)}");
            DirectoryInfo directory = new DirectoryInfo(fileDir);
            m_OldLogFileInfos = directory.GetFiles("*").ToList();
            m_Inited = true;
            UpdateStreamWriter();
        }

        private void UpdateStreamWriter()
        {
            if (!m_WriteOption.enableWrite) return;
            string fileDir = GetFullDirPath(m_WriteOption);
            string filePath = Path.Combine(fileDir, m_LogFileName);
            m_CurrentWrightPath = Path.GetFullPath(filePath);
            try
            {
                if (m_SW != null)
                {
                    m_SW.Flush();
                    m_SW.Close();
                    m_SW.Dispose();
                    m_SW = null;
                }
                StreamWriter sw = File.AppendText(filePath);
                sw.AutoFlush = true;
                sw.Write(contentSb);
                m_SW = sw;
            }
            catch (Exception e)
            {
                Debug.LogError($"日志写入失败!{e.Message}");
            }
        }

        public void WriteLog(Log.LogData logData)
        {
            var writerOption = m_WriteOption;
            if (!m_SystemInfoHasLoged)
            {
                WriteSystemInfo();
                m_SystemInfoHasLoged = true;
            }
            WriteLine(logData.GetMessage());
        }

        private void WriteLine(string line)
        {
            contentSb.AppendLine(line);
            if (!m_WriteOption.enableWrite || m_SW == null) return;
            m_SW.WriteLine(line);
        }

        private void WriteSystemInfo()
        {
            WriteLine(
                "*********************************************************************************************************start");
            WriteLine("By " + SystemInfo.deviceName);
            DateTime now = DateTime.Now;
            WriteLine(string.Concat(new object[]
            {
                now.Year.ToString(), "年", now.Month.ToString(), "月", now.Day, "日  ", now.Hour.ToString(), ":",
                now.Minute.ToString(), ":", now.Second.ToString()
            }));
            WriteLine("");
            WriteLine("操作系统:  " + SystemInfo.operatingSystem);
            WriteLine("系统内存大小:  " + SystemInfo.systemMemorySize);
            WriteLine("设备模型:  " + SystemInfo.deviceModel);
            WriteLine("设备唯一标识符:  " + SystemInfo.deviceUniqueIdentifier);
            WriteLine("处理器数量:  " + SystemInfo.processorCount);
            WriteLine("处理器类型:  " + SystemInfo.processorType);
            WriteLine("显卡标识符:  " + SystemInfo.graphicsDeviceID);
            WriteLine("显卡名称:  " + SystemInfo.graphicsDeviceName);
            WriteLine("显卡标识符:  " + SystemInfo.graphicsDeviceVendorID);
            WriteLine("显卡厂商:  " + SystemInfo.graphicsDeviceVendor);
            WriteLine("显卡版本:  " + SystemInfo.graphicsDeviceVersion);
            WriteLine("显存大小:  " + SystemInfo.graphicsMemorySize);
            WriteLine("显卡着色器级别:  " + SystemInfo.graphicsShaderLevel);
            WriteLine("是否支持内置阴影:  " + SystemInfo.supportsShadows);
            WriteLine(
                "*********************************************************************************************************end");
            WriteLine("LogInfo:");
            WriteLine("");
        }
    }
}
using Framework.Runtime.Archives;

namespace Framework.Runtime.LogSystem
{
    public interface ILogWriter
    {
        public bool LogClearOld { get; set; }
        public int LogFileMaxCount { get; set; }
        public string LogWritePath { get; set; }
        public SaveDirPath SaveDirPath { get; }
        public bool SaveLogOnlyCurrent { get; set; }
    }
}
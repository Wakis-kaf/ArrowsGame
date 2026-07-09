namespace Framework.Runtime.LogSystem
{
    public interface ILogWriteHelper
    {
        void SetWriteOption(WriteOption writeOption);

        void WriteLog(Log.LogData logData);
    }

    public struct WriteOption
    {
        public bool clearOldLog;
        public bool enableWrite;
        public int logFileMaxCount;

        public string logWriteDirPath; // 相对路径
        public string saveDirPath;
        public bool saveLogOnlyCurrent;

        public WriteOption(string saveDirPath, bool clearOldLog, string logWriteDirPath, bool saveLogOnlyCurrent,
            int logFileMaxCount, bool enableWrite)
        {
            this.saveDirPath = saveDirPath;
            this.logWriteDirPath = logWriteDirPath;
            this.saveLogOnlyCurrent = saveLogOnlyCurrent;
            this.clearOldLog = clearOldLog;
            this.logFileMaxCount = logFileMaxCount;
            this.enableWrite = enableWrite;
        }
    }
}
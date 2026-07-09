namespace Framework.Runtime.Archives
{
    public interface IArchiveWriter
    {
        /// <summary>
        /// 当存档保存的时候调用
        /// </summary>
        /// <param name="archive"></param>
        void OnArchieSave<T>(T archive) where T : Archive;

        /// <summary>
        /// 当存档被读取的时候调用
        /// </summary>
        /// <param name="archive"></param>
        void OnArchiveLoad<T>(T archive) where T : Archive;
    }
}
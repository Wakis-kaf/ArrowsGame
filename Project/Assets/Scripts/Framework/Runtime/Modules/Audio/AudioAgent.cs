using Framework.Runtime.MAsset;

using System;

namespace Framework.Runtime.MAudio
{
    public static class AudioAgent
    {
        private static Func<string, Action<IAssetVO>, IAssetVO> m_AssetLoadAsyncAgent;
        private static Func<string, IAssetVO> m_AssetLoadSyncAgent;

        public static IAssetVO LoadAssetAsync(string assetPath, Action<IAssetVO> cb = null)
        {
            if (m_AssetLoadAsyncAgent != null)
                return m_AssetLoadAsyncAgent?.Invoke(assetPath, cb);
            return null;
        }

        public static IAssetVO LoadAssetSync(string assetPath)
        {
            if (m_AssetLoadSyncAgent != null)
                return m_AssetLoadSyncAgent?.Invoke(assetPath);
            return null;
        }

        public static void SetAssetLoadAsyncAgent(Func<string, Action<IAssetVO>, IAssetVO> agent)
        {
            m_AssetLoadAsyncAgent = agent;
        }

        public static void SetAssetLoadSyncAgent(Func<string, IAssetVO> agent)
        {
            m_AssetLoadSyncAgent = agent;
        }
    }
}
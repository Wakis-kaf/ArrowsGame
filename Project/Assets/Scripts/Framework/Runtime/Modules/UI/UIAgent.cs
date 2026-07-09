using Framework.Runtime.MAsset;

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.Runtime.UI
{

    public static class UIAudioType
    {
        public const int None = -1;
        public const int NormalButtonClick = 1001;
        public const int MainTabClick = 2001;
    }
    /// <summary>
    /// UI 资源加载代理
    /// </summary>
    public static class UIAgent
    {
        public static readonly string DEFAULT_ULIST_RENDER_PATH = "UnitUI/UList/Prefabs/UListDefaultRender";
        private static Func<string, int, string> assetPathPreDecodeAgent;
        private static Func<string, int, bool> isAssetExistAgent;
        private static Func<string, int, bool> isPrefabExistAgent;
        private static Action<string, int, Action<GameObject>> loadPrefabAsyncAgent;
        private static Func<string, int, GameObject> loadPrefabSyncAgent;
        private static Action<Type, string, Action<Object>> loadUIAssetAsyncAgent;
        private static Action<string, Action<GameObject>> loadUIPrefabAsyncAgent;
        private static Action<string> logErrorAgent;
        private static Action<string> logInfoAgent;
        private static Func<GameObject, Transform, GameObject> prefabInstantiateAgent;
        private static Func<string, int, string> prefabPathPreDecodeAgent;
        private static Action<object, int> audioEffectPlayAgent;
        static UIAgent()
        {
        }

        private static Func<string, Action<IAssetVO>, IAssetVO> m_AssetLoadAsyncAgent;

        // 新接口
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
        public static void SetAudioPlayAgent(Action<object, int> agent)
        {
            audioEffectPlayAgent = agent;
        }

       
        public static void Error(string msg)
        {
            logErrorAgent?.Invoke(msg);
        }

        public static void Info(string msg)
        {
            logInfoAgent?.Invoke(msg);
        }

        public static void RemoveErrorLogAgent()
        {
            logErrorAgent = null;
        }

        public static void RemoveInfoLogAgent()
        {
            logInfoAgent = null;
        }

        public static void SetErrorLogAgent(Action<string> agent)
        {
            logErrorAgent = agent;
        }

        public static void SetInfoLogAgent(Action<string> agent)
        {
            logInfoAgent = agent;
        }

        public static void PlayAudioEffect(object fromer, int audioType)
        {
            audioEffectPlayAgent?.Invoke(fromer, audioType);
        }
    }
}
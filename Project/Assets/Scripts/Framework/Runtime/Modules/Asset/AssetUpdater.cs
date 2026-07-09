using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.MAsset
{
    public class AssetUpdater
    {
        private int m_NeedStorageFileCount = 0;
        private int m_PerTickHandleCountMax = 20;
        private Action m_ResNewsestCheckOver;
        private List<string> storagePathSet;

        public void CheckResNewest()
        {
            StorageFirstResFilesAllCompleted();
            if (!GameEnv.ResConfig.IsUpdateResNewset)
            {
                this.m_ResNewsestCheckOver?.Invoke();
                return;
            }

            if (!GameEnv.ResConfig.IsUseGameFirstRes)
            {
                CheckStorageResNewset();
            }
        }

        public void SetResNewsetCallback(Action onResNewestCheckOver)
        {
            m_ResNewsestCheckOver = onResNewestCheckOver;
        }

        private void CheckStorageResNewset()
        {
            // TODO:实现资源最新化
            m_ResNewsestCheckOver?.Invoke();
        }

        private void OnFileStorageFail(string fileName)
        {
            storagePathSet.Remove(fileName);
        }

        private void OnResDirClearFail(object obj)
        {
            Debug.Log("删除文件失败");
        }

        private void OnResDirCreateFail()
        {
        }

        private void StorageFirstResFilesAllCompleted()
        {
            GameConfig.TrySetResGameCfg("isUseGameFirstRes", false);

            CheckStorageResNewset();
        }
    }
}
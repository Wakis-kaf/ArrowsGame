using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Framework.Runtime.Base;
using Framework.Runtime.LogSystem;
using Framework.Runtime.Storage;
using Framework.Utils;
using UnityEngine;

namespace Framework.Runtime.MAsset
{
    public class AssetFileWebLoader : UnitObject
    {
        private Dictionary<string, AssetFileVO> path2FileVO = new Dictionary<string, AssetFileVO>();

        public IAssetFileVO LoadAssetFileAsync(Type fileType, string fullPath, Action<IAssetFileVO> cb, int pirority = 0)
        {
            AssetFileVO assetFileVO = GetOrCreaAssetFileVO(fullPath);
            if (assetFileVO.IsDown)
            {
                cb?.Invoke(assetFileVO);
                return assetFileVO;
            }
            assetFileVO.isDown = false;
            GameApp.WebRequestModule.UnityWebRequestMgr.GetFile(fullPath, null, (err, datas) =>
            {
                if (datas != null && datas is byte[] bytesData)
                {
                    assetFileVO.isDown = true;
                    assetFileVO.isLoadingSuccess = true;
                    assetFileVO.errCode = err;
                    assetFileVO.bytes = bytesData;
                    cb.Invoke(assetFileVO);
                }
                else
                {
                    assetFileVO.isDown = true;
                    assetFileVO.isLoadingSuccess = false;
                    assetFileVO.errCode = err;
                    cb.Invoke(assetFileVO);
                }
            }, 30);
            return assetFileVO;
        }

        public IAssetFileVO LoadAssetFileSync(Type fileType, string fullPath)
        {
            AssetFileVO assetFileVO = GetOrCreaAssetFileVO(fullPath);
            if (assetFileVO.IsDown)
            {
                return assetFileVO;
            }
            assetFileVO.isDown = true;
            assetFileVO.isLoadingSuccess = false;
            assetFileVO.errCode = AssetLoader.ErrCode_PlatformInterrupt;
            Log.Warning($"WEB端 不允许同步加载文件!{fullPath} 请确认!");
            return assetFileVO;
        }

        private AssetFileVO GetOrCreaAssetFileVO(string fullPath)
        {
            if (path2FileVO.ContainsKey(fullPath))
            {
                return path2FileVO[fullPath];
            }
            path2FileVO.Add(fullPath, new AssetFileVO(fullPath));
            return path2FileVO[fullPath];
        }
    }
}
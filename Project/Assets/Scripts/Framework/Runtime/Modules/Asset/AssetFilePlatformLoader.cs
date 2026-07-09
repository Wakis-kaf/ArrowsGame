using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Framework.Runtime.Base;
using Framework.Runtime.Storage;
using Framework.Utils;
using UnityEngine;

namespace Framework.Runtime.MAsset
{
    public interface IAssetFileVO
    {
        public IAssetVO AssetVO { get; }
        public byte[] Bytes { get; }
        public string Errcode { get; }
        public bool IsDown { get; }
        public bool IsLoadingSuccess { get; }
        public PathUrlOption Option { get; }

        public void Clear();
    }

    public class AssetFilePlatformLoader : UnitObject
    {
        public IAssetFileVO LoadAssetFileAsync(Type fileType, string fullPath, Action<IAssetFileVO> cb, int pirority = 0)
        {
            AssetFileVO assetFileVO = GameApp.AssetManager.AssetLoader.GetOrCreaAssetFileVO(fullPath);
            if (assetFileVO.IsDown)
            {
                cb?.Invoke(assetFileVO);
                return assetFileVO;
            }
            assetFileVO.isDown = false;
            if (!GameApp.Ins.PlatformStorage.IsStorageFileExistSync(fullPath))
            {
                assetFileVO.isDown = true;
                assetFileVO.isLoadingSuccess = false;
                assetFileVO.errCode = AssetLoader.ErrCode_FileNotExist;
                return assetFileVO;
            }
            PlatformStorage.Instance.TryGetStorage(fullPath, (sucData) =>
            {
                if (sucData != null && sucData is byte[] bytesData)
                {
                    assetFileVO.isDown = true;
                    assetFileVO.errCode = AssetLoader.ErrCode_None;
                    assetFileVO.bytes = bytesData;
                    object data = Utility.Convert.ConvertByteToType(bytesData, fileType);
                    if (data == null)
                    {
                        assetFileVO.isDown = true;
                        assetFileVO.isLoadingSuccess = true;
                        assetFileVO.errCode = AssetLoader.ErrCode_FileNotMatchType;
                    }
                    else
                    {
                        assetFileVO.isLoadingSuccess = true;
                        assetFileVO.data = data;
                        assetFileVO.errCode = AssetLoader.ErrCode_None;
                    }
                    cb.Invoke(assetFileVO);
                }
                else
                {
                    assetFileVO.isDown = true;
                    assetFileVO.isLoadingSuccess = false;
                    assetFileVO.errCode = AssetLoader.ErrCode_FileNotMatchType;
                    cb.Invoke(assetFileVO);
                }
            }, (failData) =>
            {
                assetFileVO.isDown = true;
                assetFileVO.isLoadingSuccess = false;
                assetFileVO.errCode = AssetLoader.ErrCode_FileLoadFail;
                cb.Invoke(assetFileVO);
            });

            return assetFileVO;
        }

        public IAssetFileVO LoadAssetFileSync(Type fileType, string fullPath)
        {
            AssetFileVO assetFileVO = GameApp.AssetManager.AssetLoader.GetOrCreaAssetFileVO(fullPath);
            if (assetFileVO.IsDown)
            {
                return assetFileVO;
            }
            if (!PlatformStorage.Instance.IsStorageFileExistSync(fullPath))
            {
                assetFileVO.isDown = true;
                assetFileVO.isLoadingSuccess = false;
                assetFileVO.errCode = AssetLoader.ErrCode_FileNotExist;
                return assetFileVO;
            }
            if (PlatformStorage.Instance.TryGetStorageSync(fullPath, out byte[] bytesData))
            {
                if (bytesData != null)
                {
                    assetFileVO.isDown = true;
                    assetFileVO.errCode = AssetLoader.ErrCode_None;
                    assetFileVO.bytes = bytesData;
                    assetFileVO.isLoadingSuccess = true;
                    object data = Utility.Convert.ConvertByteToType(bytesData, fileType);
                    if (data == null)
                    {
                        assetFileVO.isDown = true;
                        assetFileVO.errCode = AssetLoader.ErrCode_FileNotMatchType;
                    }
                    else
                    {
                        assetFileVO.data = data;
                        assetFileVO.errCode = AssetLoader.ErrCode_None;
                    }
                }
                else
                {
                    assetFileVO.isDown = true;
                    assetFileVO.isLoadingSuccess = false;
                    assetFileVO.errCode = AssetLoader.ErrCode_FileNotMatchType;
                }
                return assetFileVO;
            }
            assetFileVO.isDown = true;
            assetFileVO.isLoadingSuccess = false;
            assetFileVO.errCode = AssetLoader.ErrCode_FileLoadFail;
            return assetFileVO;
        }
    }

    public class AssetFileVO : IAssetFileVO
    {
        public IAssetVO assetVO;
        public byte[] bytes;
        public object data;
        public string errCode;
        public bool isDown;
        public bool isLoadingSuccess;
        public PathUrlOption option;
        private string fullPath;

        public AssetFileVO(string fullPath)
        {
            this.fullPath = fullPath;
        }

        public IAssetVO AssetVO => assetVO;

        //public object Data => data;
        public byte[] Bytes => bytes;

        public string Errcode => errCode;
        public bool IsDown => isDown;
        public bool IsLoadingSuccess => isLoadingSuccess;
        public PathUrlOption Option => option;

        public void Clear()
        {
            this.bytes = null;
            this.isLoadingSuccess = false;
            this.isDown = false;
            this.errCode = "";
        }
    }
}
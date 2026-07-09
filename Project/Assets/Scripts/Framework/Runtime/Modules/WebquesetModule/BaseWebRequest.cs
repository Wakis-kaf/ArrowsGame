using Framework.Runtime.LogSystem;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Framework.Runtime
{
    public delegate void DelGetAbCallback(string error, AssetBundle assetBundle);

    public delegate void DelGetAudioClipCallback(string error, AudioClip audioClip);

    public delegate void DelGetFileCallback(string error, byte[] data);

    public delegate void DelGetTextCallback(string error, string text);

    public delegate void DelGetTextureCallback(string error, Texture2D texture2D);

    public delegate void DelWebRequestCallback(string error, float progress, UnityWebRequest unityWeb);

    /// <summary>
    /// 基于Unity Web Request 的下载接口封装 为避免过度使用协程导致卡顿，使用同步Update 进行 下载任务的更新 功能： 1.封装Get请求
    /// 2. 封装Post请求
    /// 3. 封装 下载资源
    /// 4. 封装上传资源
    /// </summary>
    public abstract class BaseWebRequest
    {
        public const int defaultTimout = 30;

        public void Download(string url, string savePath, DelWebRequestCallback callback = null, int timeout = defaultTimout)
        {
            using (var uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET))
            {
                uwr.timeout = timeout;

                GameApp.Ins.GameAppShell.StartCoroutine(Download(uwr, savePath, callback));
            }
        }

        public virtual IEnumerator Download(UnityWebRequest uwr, string savePath, DelWebRequestCallback callback = null)
        {
            uwr.downloadHandler = new DownloadHandlerFile(savePath);
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                callback(uwr.error, 0, null);
                yield break;
            }
            while (!uwr.isDone)
            {
                if (callback != null && uwr.downloadProgress < 1) callback(string.Empty, uwr.downloadProgress, uwr);
                yield return null;
            }

            if (callback != null)
            {
                callback(uwr.error, uwr.downloadProgress, uwr);
            }
        }

        public void Get(string url, Action<UnityWebRequest> callback = null, int timeout = defaultTimout)
        {
            using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                uwr.timeout = timeout;
                GameApp.Ins.GameAppShell.StartCoroutine(Get(uwr, callback));
            }
        }

        public virtual IEnumerator Get(UnityWebRequest uwr, Action<UnityWebRequest> callback = null)
        {
            using (uwr)
            {
                yield return uwr.SendWebRequest();
                if (callback != null)
                {
                    callback(uwr);
                }
            }
        }

        public void GetAssetBundle(string url, Action<float> progress, DelGetAbCallback callback, bool cache = true, int timeout = defaultTimout)
        {
            GameApp.Ins.GameAppShell.StartCoroutine(getAssetBundle(url, progress, callback, timeout, cache));
        }

        public void GetAudioClip(string url, AudioType audioType, Action<float> progress,
            DelGetAudioClipCallback callback, int timeout = defaultTimout)
        {
            GameApp.Ins.GameAppShell.StartCoroutine(getAudioClip(url, audioType, progress, callback, timeout));
        }

        public void GetFile(string url, Action<float> progress, DelGetFileCallback callback, int timeout = defaultTimout)
        {
            GameApp.Ins.GameAppShell.StartCoroutine(getFile(url, progress, callback, timeout));
        }

        public void GetHeadFile(string url, Action<UnityWebRequest> callback, int timeout = defaultTimout)
        {
            GameApp.Ins.GameAppShell.StartCoroutine(getHeadFile(url, callback, timeout));
        }

        public void GetText(string url, Action<float> progress, DelGetTextCallback callback, int timeout = defaultTimout)
        {
            GameApp.Ins.GameAppShell.StartCoroutine(getText(url, progress, callback, timeout));
        }

        public void GetTexture(string url, Action<float> progress, DelGetTextureCallback callback, int timeout = defaultTimout)
        {
            GameApp.Ins.GameAppShell.StartCoroutine(getTexture(url, progress, callback, timeout));
        }

        public void Post(string url, WWWForm form, Action<UnityWebRequest> callback = null, int timeout = defaultTimout)
        {
            using (UnityWebRequest uwr = UnityWebRequest.Post(url, form))
            {
                uwr.timeout = timeout;

                GameApp.Ins.GameAppShell.StartCoroutine(Post(uwr, callback));
            }
        }

        public virtual IEnumerator Post(UnityWebRequest uwr, Action<UnityWebRequest> callback = null)
        {
            yield return uwr.SendWebRequest();
            if (callback != null)
            {
                callback(uwr);
            }
        }

        public void Upload(byte[] bytes, DelWebRequestCallback callback = null, int timeout = defaultTimout)
        {
            using (UnityWebRequest uwr = new UnityWebRequest())
            {
                UploadHandler uploader = new UploadHandlerRaw(bytes);
                uploader.contentType = "application/octet-stream";

                GameApp.Ins.GameAppShell.StartCoroutine(Upload(uwr, uploader, callback));
            }
        }

        public virtual IEnumerator Upload(UnityWebRequest uwr, UploadHandler uploader,
            DelWebRequestCallback callback = null)
        {
            uwr.uploadHandler = uploader;
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                callback(uwr.error, 0, null);
                yield break;
            }
            while (!uwr.isDone)
            {
                if (callback != null && uwr.downloadProgress < 1) callback(uwr.error, uwr.uploadProgress, uwr);
                yield return null;
            }

            if (callback != null) callback(uwr.error, uwr.uploadProgress, uwr);
        }

        private IEnumerator getAssetBundle(string url, Action<float> progress, DelGetAbCallback callback, int timeout, bool cache)
        {
            //url =  Application.streamingAssetsPath + "/FirstRes/res/res";
            Log.Debug("加载" + url + " Application.streamingAssetsPath:" + Application.streamingAssetsPath);
            using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                if (!cache)
                {
                    //uwr.SetRequestHeader("Cache-Control", "no-cache, no-store, must-revalidate");
                    //uwr.SetRequestHeader("Pragma", "no-cache");
                    //uwr.SetRequestHeader("Expires", "0");
                }

                uwr.timeout = timeout;
                //DownloadHandlerAssetBundle handler = new DownloadHandlerAssetBundle(uwr.url, uint.MaxValue);

                //uwr.downloadHandler = handler;
                if (uwr.downloadHandler == null)
                {
                    Debug.LogError("DownloadHandler 为空！");
                    yield break;
                }
                yield return uwr.SendWebRequest();
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    callback(uwr.error, null);
                    yield break;
                }
                while (!uwr.isDone)
                {
                    if (callback != null && uwr.downloadProgress < 1) progress?.Invoke(uwr.downloadProgress);
                    yield return null;
                }
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"加载失败: {uwr.error}");
                    yield break;
                }
                AssetBundle ab = null;
                if (string.IsNullOrEmpty(uwr.error))
                {
                    byte[] results = uwr.downloadHandler.data;
                    ab = AssetBundle.LoadFromMemory(results);
                }

                if (callback != null)
                {
                    callback(uwr.error, ab);
                }
            }
        }

        private IEnumerator getAudioClip(string url, AudioType audioType, Action<float> progress,
            DelGetAudioClipCallback callback, int timeout)
        {
            using (var uwr = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
            {
                uwr.timeout = timeout;

                yield return uwr.SendWebRequest();
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    callback(uwr.error, null);
                    yield break;
                }
                while (!uwr.isDone)
                {
                    if (progress != null) progress(uwr.downloadProgress);
                    yield return null;
                }

                AudioClip clip = (string.IsNullOrEmpty(uwr.error)) ? DownloadHandlerAudioClip.GetContent(uwr) : null;
                if (callback != null) callback(uwr.error, clip);
            }
        }

        private IEnumerator getFile(string url, Action<float> progress, DelGetFileCallback callback, int timeout)
        {
            using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                uwr.timeout = timeout;
                if (uwr.downloadHandler == null)
                {
                    Debug.LogError("DownloadHandler 为空！");
                    yield break;
                }
                yield return uwr.SendWebRequest();
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    callback(uwr.error, null);
                    yield break;
                }
                while (!uwr.isDone)
                {
                    if (callback != null && uwr.downloadProgress < 1) progress?.Invoke(uwr.downloadProgress);
                    yield return null;
                }
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"加载失败: {uwr.error}");
                    yield break;
                }
                if (string.IsNullOrEmpty(uwr.error))
                {
                    byte[] results = uwr.downloadHandler.data;
                    callback?.Invoke(uwr.error, results);
                }
            }
        }

        private IEnumerator getHeadFile(string url, Action<UnityWebRequest> callback, int timeout)
        {
            using (UnityWebRequest uwr = UnityWebRequest.Head(url))
            {
                uwr.timeout = timeout;
                yield return uwr.SendWebRequest();
                if (callback != null)
                {
                    callback(uwr);
                }
            }
        }

        private IEnumerator getText(string url, Action<float> progress, DelGetTextCallback callback, int timeout)
        {
            using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                uwr.timeout = timeout;

                yield return uwr.SendWebRequest();
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    callback(uwr.error, string.Empty);
                    yield break;
                }
                while (!uwr.isDone)
                {
                    if (progress != null) progress(uwr.downloadProgress);
                    yield return null;
                }

                string text = (string.IsNullOrEmpty(uwr.error)) ? uwr.downloadHandler.text : string.Empty;
                if (callback != null)
                {
                    callback(uwr.error, text);
                }
            }
        }

        private IEnumerator getTexture(string url, Action<float> progress, DelGetTextureCallback callback, int timeout)
        {
            using (UnityWebRequest uwr = new UnityWebRequest(url))
            {
                uwr.timeout = timeout;
                DownloadHandlerTexture downloadTexture = new DownloadHandlerTexture(true);
                uwr.downloadHandler = downloadTexture;

                yield return uwr.SendWebRequest();
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    callback(uwr.error, null);
                    yield break;
                }
                while (!uwr.isDone)
                {
                    if (callback != null && uwr.downloadProgress < 1) progress(uwr.downloadProgress);
                    yield return null;
                }

                Texture2D texture2D = (string.IsNullOrEmpty(uwr.error)) ? downloadTexture.texture : null;
                if (callback != null)
                {
                    callback(uwr.error, texture2D);
                }
            }
        }
    }
}
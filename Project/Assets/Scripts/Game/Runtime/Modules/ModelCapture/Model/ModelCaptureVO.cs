// ModelCaptureVO.cs
using Framework.Runtime;
using Framework.Runtime.LogSystem;
using Framework.Runtime.MAsset;
using Framework.Utils;
using Game.Modules.GModuleModelCapture;
using UnityEditor;
using UnityEngine;
namespace Game.Modules.GModuleModelCapture
{
    public class ModelCaptureVO
    {
        private IAssetVO assetVO;
        private ModelCaptureOption option;
        
        private GameObject goRoot;
        private GameObject modelRoot;
        private GameObject model;
        private Camera camera;
        private bool isRendering = false;
        private bool isLoaded = false;
        private bool isLoading = false;
        private bool isChecked = false;
        public RenderTexture RenderTexture { get; private set; }
        public int Index { get; internal set; }

        public void SetOption(ModelCaptureOption option)
        {
            this.option = option;
            this.model = option.model;
            isLoaded = false;
            isRendering = false;
            isLoading = false;
            isChecked = false;
            CheckComponents();
        }

        public void Show()
        {
            isRendering = true;
            this.CheckRendering();
        }
        public void Hide()
        {
            isRendering = false;
            DisActiveModel();
        }

        private void CheckRendering()
        {
            if (!isRendering)
            {
                DisActiveModel();
                return;
            }
            if (isLoaded)
            {
                if (!isChecked)
                {
                    CheckComponents();
                    CheckModel();
                    isChecked = true;
                }
                ActiveModel();
                
            }
            else
            {
                isLoading = true;
                if (this.option.model != null)
                {
                    this.model = this.option.model;
                    this.OnModelLoaded();
                }
                else if(this.assetVO==null)
                {
                    var path = AssetPathEncoder.EncodeEnvAssetLink(option.modelPathLink);
                    this.assetVO = GameApp.AssetManager.LoadAssetAsync(path, (assetVO) =>
                    {
                        this.assetVO = assetVO;
                        this.OnModelLoaded();
                    });
                }
            }
        }
        public void Dispose()
        {
            GameModelCapture.Ins.PutModelCapture(this);
        }
        public void Close()
        {
            isLoaded = false;
            isLoading = false;
            if (RenderTexture != null)
            {
                camera.targetTexture = null;
                RenderTexture.ReleaseTemporary(RenderTexture);
                RenderTexture = null;
            }
            
            assetVO?.UnLoadAsync();
            assetVO = null;
            model = null;
            goRoot?.gameObject.SetActive(false);
        }

        private void OnModelLoaded()
        {
            if (!isLoading) return;
            isLoaded = true;
            isLoading = false;
            this.CheckRendering();
        }
        private void ActiveModel()
        {
            goRoot?.SetActive(true);
        }
        private void DisActiveModel()
        {
            goRoot?.SetActive(false);
        }
        private void CheckComponents()
        {
            if (goRoot == null)
            {
                goRoot = new GameObject("ModelCapture");
                goRoot.transform.SetParent(GameModelCapture.Ins.ModelCaptureRoot);
                modelRoot = new GameObject("ModelRoot");
                GameObject cameraGO = new GameObject("ModelCaptureCamera");
                camera = cameraGO.AddComponent<Camera>();
                camera.transform.SetParent(goRoot.transform);
                camera.transform.localPosition = Vector3.zero;
                camera.clearFlags = CameraClearFlags.SolidColor;
                // 设置相机层级 - 需要你根据项目实际情况调整
                int modelCaptureLayer = LayerMask.NameToLayer("ModelCapture");
                if (modelCaptureLayer != -1)
                {
                    camera.cullingMask = 1 << modelCaptureLayer;
                }
                modelRoot.transform.SetParent(goRoot.transform);
            }
            bool isCreate = true;
            if (RenderTexture != null)
            {
                if((RenderTexture.width != option.rtWidth
                || RenderTexture.height != option.rtHeight))
                {
                    camera.targetTexture = null;
                    RenderTexture.ReleaseTemporary(RenderTexture);
                }
                else
                {
                    isCreate = false;
                }
            }
            if (isCreate)
            {
                RenderTexture = RenderTexture.GetTemporary(option.rtWidth, option.rtHeight, 100);
            }
            camera.targetTexture = RenderTexture;
            goRoot.transform.position = option.birthPostion;
            modelRoot.transform.localPosition = option.modelLocalPos;
        }
        private void CheckModel()
        {
            model.transform.SetParent(modelRoot.transform);
            model.transform.localPosition = Vector3.zero;
            model.SetActive(true);
            // 设置层级 - 需要你根据项目实际情况调整
            int modelCaptureLayer = LayerMask.NameToLayer("ModelCapture");
            if (modelCaptureLayer != -1)
            {
                GameObjectUtil.SetLayer(goRoot, modelCaptureLayer, true);

            }
            if (option.ModelCaptureType == ModelCaptureType.CameraCanvas)
            {
                Canvas canvas = model.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = camera;
                }
            }
        }
    }
}
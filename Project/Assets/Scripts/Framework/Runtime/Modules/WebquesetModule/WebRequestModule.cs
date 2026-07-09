using Framework.Runtime.Module.Core;

namespace Framework.Runtime
{
    public class WebRequestModule : ModuleUnit
    {
        private UnityWebRequestMgr unityWebRequestMgr;
        private WebDownloader webDownloader;
        public UnityWebRequestMgr UnityWebRequestMgr => unityWebRequestMgr;
        public WebDownloader WebDownloader => webDownloader;

        protected override void OnModuleConstructed()
        {
            base.OnModuleConstructed();
            unityWebRequestMgr = new UnityWebRequestMgr();
            //webDownloader = new WebDownloader();
        }

        //protected override void OnRegisterModulePart()
        //{
        //    RegisterModulePart<UnityWebRequestMgr>();
        //    RegisterModulePart<WebDownloader>();
        //}
    }
}
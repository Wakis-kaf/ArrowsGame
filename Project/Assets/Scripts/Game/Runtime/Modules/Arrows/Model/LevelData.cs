namespace Game.Modules.GModuleArrows
{

    public class LevelArgs
    {
        public float minZoomScale = 0.2f;
        public float startZoomScale = 0.6f;
        public float maxZoomScale = 1.2f;
        public float scrollSpeed = 5f;
        public float gameZoomSpeed = 12f;
        public float entryAnimZoomSpeed = 6f;
    }


    public class LevelInfo
    {
        public int levelId;
        public int heartNum = 3;
        public string arrowLayoutId;
        public CfgLevel levelCfg;
        public LevelArgs levelArgs;
        public CfgLevelAnimArgs LevelAnimArgs;
        public LevelPointPresets pointPresets;
        public LevelArrowsPresure arrowsPresure;
        public bool errorLineSubHeartRepeat = false;// 错误点击重复扣血
        public bool isInfHeart = false; // 不扣血

        public LevelInfo()
        {
            levelArgs = new LevelArgs();
        }
    }
}

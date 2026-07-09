using Sirenix.OdinInspector;

namespace Game.Modules.GModuleArrows
{
    public class LevelArrowsPresure
    {
        public int presureArgId;
        public float maxAllowGenerateTime = 5f; // 最大允许生成时间，单位秒
        public int minLineLength = 3;
        public int normalLineLength = 20;
        public int maxLineLength = 50;
        public float turnTendency = 0.2f; // 转弯倾向，0=直行，1=转弯
        public int maxTurnsPerLine = 3;    // 每条线最大转弯次数
        public int mainLineGenerateMaxRetractNum = 5; // 允许回溯最大次数
        public int emptyReGenerateMaxRetractNum = 10; // 允许回溯最大次数
        public bool retractFromTurn = true; //  从拐角处开始回溯
        public bool tryMaxLength; // 允许自动尝试搜索其他线条
        public bool isEnableRootTraceHead = false; // 是否允许根节点回溯方向生成
        public int customSeed = 0;
        public int runtimeSeed = 0;
        public bool isUsingCustomSeed = false;
    }
}
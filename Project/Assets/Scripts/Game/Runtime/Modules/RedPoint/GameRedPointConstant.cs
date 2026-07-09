using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Modules
{
    public enum RedPointType
    {
        Hidden = -1, //隐藏不显示
        PurePoint = 1, // 纯红点
        PointWithNum = 2, // 红点带数量
        Warning = 3, // 警告红点
        WarningWithNum = 4, // 警告红点
        Error = 5, //错误红点
        ErrorWithNum = 6, // 错误带数字
    }

    public interface IGameRedPoint
    {
        public string Key { get; set; }
        public List<string> SubRedPointKeys { get; set; }
        public List<IGameRedPoint> SubRedPoints { get; set; }
    }

    public static class GameRedPointConstant
    {
        public const string Red_RoleTabView = "Red_RoleTabView";
        
        public const string Red_RoleTalent = "Red_RoleTalent";
        public const string Red_ChapterWaveReward = "Red_ChapterWaveReward";
        public const string Red_BookTabView = "Red_BookTabView";
        public const string Red_BookReward = "Red_BookReward";

        // 主页红点
        public const string Red_AfkReward = "Red_AfkReward";
        public const string Red_FightTabView = "Red_FightTabView";
        // 商城红点
        public const string Red_ShopTabView = "Red_ShopTabView";
        public const string Red_ShopHasFree = "Red_ShopHasFree";
        //public static GameRedRootPoint GameRedRootPoint = new GameRedRootPoint();
    }
    public class CfgGameRedPoint
    {
        public List<GameRedPoint> cfgRedPoints { get; set; }
    }

    public class GameRedPoint : IGameRedPoint
    {
        public virtual string Key { get; set; }
        public virtual List<string> SubRedPointKeys { get; set; }
        public virtual List<IGameRedPoint> SubRedPoints { get; set; }
    }
}
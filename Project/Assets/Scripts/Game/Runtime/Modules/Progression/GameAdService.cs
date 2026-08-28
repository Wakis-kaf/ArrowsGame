using System;

namespace Game.Modules.GModuleProgression
{
    public enum AdRewardType { Prop, Coins, Revive, ChestDouble }
    public interface IGameAdService { bool IsAvailable { get; } void ShowRewarded(AdRewardType rewardType, Action<bool> completed); }
    public static class GameAdService
    {
        public static bool AdsEnabled { get; set; }
        public static IGameAdService Provider { get; set; }
        public static bool IsAvailable { get { return AdsEnabled && Provider != null && Provider.IsAvailable; } }
        public static void ShowRewarded(AdRewardType rewardType, Action<bool> completed)
        {
            if (!IsAvailable) { if (completed != null) completed(false); return; }
            Provider.ShowRewarded(rewardType, completed);
        }
    }
}

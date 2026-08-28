using Framework.Runtime;
using Game.Modules.GModuleManage;
using System;

namespace Game.Modules.GModuleProgression
{
    public static class GameProgressionService
    {
        public const int PropCoinCost = 30;
        public static bool IsChestLevel(int levelId) { return levelId > 0 && levelId % GameProgressionConstant.ChestEveryLevels == 0; }
        public static int GetLevelCoins(int levelId) { return 10 + (levelId > 0 ? levelId * 2 : 0); }
        public static void GrantLevelReward(int levelId)
        {
            var archive = GameArchive.Main == null ? null : GameArchive.Main.ProgressionArchive;
            if (archive == null) return;
            archive.AddCoins(GetLevelCoins(levelId));
            archive.AddAchievementProgress("levels_passed", 1);
            if (IsChestLevel(levelId)) MessageDispatcher.Ins.Dispatch(MessageCode.msg_open_progression_chest, levelId);
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_progression_rewarded, levelId);
        }
        public static bool ClaimChestReward(int levelId, out int coins, out int propId)
        {
            coins = 0; propId = 0;
            var archive = GameArchive.Main == null ? null : GameArchive.Main.ProgressionArchive;
            if (archive == null || !IsChestLevel(levelId) || !archive.ClaimChest(levelId)) return false;
            if (levelId % 20 == 0) { coins = 100 + levelId * 2; archive.AddCoins(coins); }
            else { propId = (levelId / 10) % 3 == 1 ? GameProgressionConstant.PropUndo : (levelId / 10) % 3 == 2 ? GameProgressionConstant.PropClear : GameProgressionConstant.PropHint; archive.AddProp(propId, 1); }
            return true;
        }
        public static bool TryConsumeProp(int propId)
        {
            var consumed = GameArchive.Main != null && GameArchive.Main.ProgressionArchive != null && GameArchive.Main.ProgressionArchive.ConsumeProp(propId);
            if (consumed) MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_progression_changed);
            return consumed;
        }
        public static bool TrySpendCoins(int count) { return GameArchive.Main != null && GameArchive.Main.ProgressionArchive != null && GameArchive.Main.ProgressionArchive.SpendCoins(count); }
        public static int GetPropCount(int propId) { return GameArchive.Main == null || GameArchive.Main.ProgressionArchive == null ? 0 : GameArchive.Main.ProgressionArchive.GetPropCount(propId); }
        public static void AddProp(int propId, int count)
        {
            if (GameArchive.Main == null || GameArchive.Main.ProgressionArchive == null) return;
            GameArchive.Main.ProgressionArchive.AddProp(propId, count);
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_progression_changed);
        }
        public static void AcquireProp(int propId, Action<bool> completed)
        {
            if (GameAdService.IsAvailable)
            {
                GameAdService.ShowRewarded(AdRewardType.Prop, rewarded =>
                {
                    if (rewarded) AddProp(propId, 1);
                    completed?.Invoke(rewarded);
                });
                return;
            }
            var acquired = TrySpendCoins(PropCoinCost);
            if (acquired) AddProp(propId, 1);
            completed?.Invoke(acquired);
        }
        public static bool IsPropBarVisible(int levelId) { return levelId >= GameProgressionConstant.NewPlayerHiddenUntilLevel; }
    }
}

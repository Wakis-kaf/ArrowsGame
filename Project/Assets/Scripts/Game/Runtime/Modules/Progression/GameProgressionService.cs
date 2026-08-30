using Framework.Runtime;
using Game.Modules.GModuleManage;
using Game.Modules.GModuleInventory;
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
            AddItem(GameProgressionConstant.PropCoin, GetLevelCoins(levelId));
            archive.AddAchievementProgress("levels_passed", 1);
            if (IsChestLevel(levelId)) MessageDispatcher.Ins.Dispatch(MessageCode.msg_open_progression_chest, levelId);
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_progression_rewarded, levelId);
        }
        public static bool ClaimChestReward(int levelId, out int coins, out int propId)
        {
            coins = 0; propId = 0;
            var archive = GameArchive.Main == null ? null : GameArchive.Main.ProgressionArchive;
            if (archive == null || !IsChestLevel(levelId) || !archive.ClaimChest(levelId)) return false;
            if (levelId % 20 == 0) { coins = 100 + levelId * 2; AddItem(GameProgressionConstant.PropCoin, coins); }
            else { propId = (levelId / 10) % 3 == 1 ? GameProgressionConstant.PropTime : (levelId / 10) % 3 == 2 ? GameProgressionConstant.PropClear : GameProgressionConstant.PropTip; AddItem(propId, 1); }
            return true;
        }
        public static bool TryConsumeProp(int propId)
        {
            var consumed = TryConsumeItem(propId);
            if (consumed) MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_progression_changed);
            return consumed;
        }
        public static int GetPropCount(int propId) => GameInventoryDataHandler.Ins.GetItemHasCount(propId);
        public static bool TryConsumeItem(int itemId, int count = 1)
        {
            var result = GameInventoryDataHandler.Ins.TakeOutItem(itemId, count);
            if (result.operateCount < count) return false;
            var cfg = GameInventoryDataHandler.Ins.GetItemInfoCfg(itemId);
            if (cfg != null && cfg.recoverySeconds > 0 && GameArchive.Main != null)
                GameArchive.Main.RoleArchive.SetItemRecoveryTime(itemId, DateTimeOffset.Now.ToUnixTimeSeconds() + cfg.recoverySeconds);
            return true;
        }
        public static void AddItem(int itemId, int count) => GameInventoryDataHandler.Ins.StoreItem(itemId, count, null);
        public static long GetItemRecoveryTime(int itemId) => GameArchive.Main?.RoleArchive?.GetItemRecoveryTime(itemId, 0) ?? 0;
        public static void AddProp(int propId, int count)
        {
            AddItem(propId, count);
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
            var acquired = TryConsumeItem(GameProgressionConstant.PropCoin, PropCoinCost);
            if (acquired) AddProp(propId, 1);
            completed?.Invoke(acquired);
        }
        public static bool IsPropBarVisible(int levelId)
        {
            return levelId >= GameManageDataHandler.Ins.GetGameMainCfg().propUnLockLv;
        }
    }
}

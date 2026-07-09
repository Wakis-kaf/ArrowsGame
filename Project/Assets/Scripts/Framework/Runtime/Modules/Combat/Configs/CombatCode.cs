namespace Framework.Runtime.MCombat
{
    public enum CombateEventRetCode
    {
        Success = 1 << 1,
    }

    public class CombatCode
    {
        
        public static string OnAbilityAwake = "OnAbilityAwake";
        public static string OnAbilityBirth = "OnAbilityBirth";
        public static string OnAbilityDestroyFail = "OnAbilityDestroyFail";
        public static string OnAbilityDestroySuccess = "OnAbilityDestroySuccess";
        public static string OnAbilityDisableFail = "OnAbilityDisableFail";
        public static string OnAbilityDisableSuccess = "OnAbilityDisableSuccess";
        public static string OnAbilityEnableFail = "OnAbilityEnableFail";
        public static string OnAbilityEnableSuccess = "OnAbilityEnableSuccess";
        public static string OnAbilityUpdate = "OnAbilityUpdate";
        public static string OnAfterUnitAttack = "OnAfterUnitAttack";
        public static string OnAfterUnitDeath = "OnAfterUnitDeath";
        public static string OnAfterUnitHurt = "OnAfterUnitHurt";
        public static string OnAfterUnitKill = "OnAfterUnitKill";

        // 攻击钩子事件
        public static string OnBeforeUnitAttack = "OnBeforeUnitAttack";

        public static string OnBeforeUnitDeath = "OnBeforeUnitDeath";
        public static string OnBeforeUnitHurt = "OnBeforeUnitHurt";
        public static string OnUnitAttack = "OnUnitAttack";
        public static string OnUnitAttackFail = "OnUnitAttackFail";
        public static string OnUnitAttackSuccess = "OnUnitAttackSuccess";
        public static string OnUnitDeath = "OnUnitDeath";
        public static string OnUnitEmpty = "OnUnitEmpty";
        public static string OnUnitHurt = "OnUnitHurt";
        public static string OnUnitKill = "OnUnitKill";
        public static string OnUnitKillSuccess = "OnUnitKillSuccess";
        public static string OnUnitValueChange = "OnUnitValueChange";
        public static string TryDestroyAbility = "TryDestroyAbility";
        public static string TryDisableAbility = "TryDisableAbility";
        public static string TryEnableAbility = "TryEnableAbility";



        public const string OnAbilityParamsUpdate = "OnAbilityParamsUpdate";
        public const string OnBuffMerge = "OnBuffMerge";
        // 大门血量更新事件
        public const string SetDoorEnhance = "SetDoorEnhance";
        
        public const string SetDoorEnahnce = "SetDoorEnahnce";
        // 大门血量恢复事件
        public const string SetDoorRecovery = "SetDoorRecovery";
        // 元宝收益产生事件
        public const string SetBedProfit = "SetBedProfit";
        // 祥云收益产生事件
        public const string SetShrineProfit = "SetShrineProfit";
        
        public const string OnDoorDamage = "OnDoorDamage";
        public const string OnMonsterHit= "OnMonsterHit";
        public const string OnTowerAttack= "OnTowerAttack";
    }
}
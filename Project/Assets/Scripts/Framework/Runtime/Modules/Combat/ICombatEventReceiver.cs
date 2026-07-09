using System;

namespace Framework.Runtime.MCombat
{
    public interface ICombatEventReceiver
    {
        CombatEvent SendEvent(CombatEvent combatEvent);

        /// <summary>
        /// 开启的时候触发回调，处理事件
        /// 最终传回给事件的创造者的时候，不管是不是激活状态，都会触发回调
        /// </summary>
        /// <param name="combatEvent"></param>
        /// <returns></returns>
        CombatEvent HandleEvent(CombatEvent combatEvent);

        /// <summary>
        /// 开启的时候触发回调，接收事件
        /// </summary>
        /// <param name="combatEvent"></param>
        /// <returns></returns>
        CombatEvent ReceiveEvent(CombatEvent combatEvent);
        public bool IsActive();
        public bool IsEnabled();
    }
}
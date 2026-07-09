using Framework.Runtime.UnitSystem.BIInterfaces;
using Game.Modules.GModuleTip;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Modules.GModuleTip
{
    public  class GameTip :GameModuleBaseInstance<GameTip>,IUnitUpdate
    {
        private TipPool TipPool { get;  set; }
        public TipPositionManager TipPositionManager { get; private set; }
        protected override void OnConstructed()
        {
            base.OnConstructed();
            TipFactory.Clear();
            TipPool = new TipPool();
            TipPositionManager = new TipPositionManager();
        }
        public void PutTip(Tip tip)
        {
            TipPool.PutTip(tip);
            TipPositionManager.UnregisterTip(tip);
        }
        public  Tip TipBounceMsg(string msg, TipOption option = null)
        {
            if (option == null)
                option = TipFactory.GetDefaultBounceMsgTipOption();

            option.data = msg;
            return PopTip(option);
        }
        public  Tip TipCommonMsg(string msg, TipOption option = null)
        {
            if (option == null)
                option = TipFactory.GetDefaultCommonMsgTipOption();

            option.data = msg;
            return PopTip(option);
        }
        public void DisposeAllFightTip()
        {
            var option = TipFactory.GetDefaultFightTextTipOption();
            TipPool.DisposeAllTip(option.tipName);
        }
        public  Tip TipFightText(string msg, FightTextTipType fightTextTip = FightTextTipType.NormalDamageTip, TipOption option = null)
        {
            if (option == null)
                option = TipFactory.GetDefaultFightTextTipOption();
            FightTextTipData data = new FightTextTipData()
            {
                tipValue = msg,
                fightTextTipType = fightTextTip
            };
            option.data = data;
            return PopTip(option);
        }
        public  Tip Tip(TipOption option )
        {
            return PopTip(option);
        }
        public  Tip PopTip(TipOption option)
        {
            Tip tip = TipPool.GetOrCreateTip(option, (tip) =>
            {
                tip.SetOption(option);
                tip.SetData(option.data??tip.Data);
                if (option.isCheckPosOverlap)
                {
                    TipPositionManager.RegisterTip(tip, option);
                }
                DoTipStartAnimation(tip, () =>
                {
                    DoTipEndAnimation(tip, () =>
                    {
                        if (option.isCheckPosOverlap)
                        {
                            TipPositionManager.UnregisterTip(tip);
                        }
                        if (option.autoPut)
                        {
                            PutTip(tip);
                        }
                    });
                });

            });

            return tip;
        }

        private  void DoTipStartAnimation(Tip tip, System.Action cb)
        {
            tip.OnPlayStartAnimation(cb);
        }

        private  void DoTipEndAnimation(Tip tip, System.Action cb)
        {
            tip.OnPlayEndAnimation(cb);
        }

        public void OnUnitUpdate()
        {
            TipPositionManager.OnUpdate();
        }
    }
}
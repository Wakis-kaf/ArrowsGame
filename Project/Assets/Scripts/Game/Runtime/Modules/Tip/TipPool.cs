// TipPool.cs
using Framework.Runtime.UI;
using Game.Modules.GModuleTip;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
namespace Game.Modules.GModuleTip
{
    public class TipPool
    {
        private  Dictionary<string, Queue<Tip>> tipPool = new Dictionary<string, Queue<Tip>>();
        private  Dictionary<string, List<Tip>> createdPool = new Dictionary<string, List<Tip>>();

        public  Tip GetOrCreateTip(TipOption option, System.Action<Tip> cb = null)
        {
            if (option.usePoolTip)
                return GetOrCreateTipFromPool(option, cb);

            return CreateTip(option, cb);
        }

        private  Tip GetOrCreateTipFromPool(TipOption option, System.Action<Tip> cb = null)
        {
            if (string.IsNullOrEmpty(option.tipName)){
                option.tipName = option.tipType.Name;
            }
            string tipTypeName = option.tipName;

            if (!tipPool.ContainsKey(tipTypeName) || tipPool[tipTypeName].Count == 0)
            {
                var newTip = CreateTip(option, cb);
                newTip.OnGetFromPool();
                return newTip;
            }
                

            Tip tip = tipPool[tipTypeName].Dequeue();

            ResetTip(tip, option);
            tip.OnGetFromPool();

            tip.OpenWindow();
            cb?.Invoke(tip);

            return tip;
        }

        private  void ResetTip(Tip tip, TipOption option)
        {
            tip.ResetByOption (option);
        }
        public void DisposeAllTip(string tipName)
        {
            if (!createdPool.TryGetValue(tipName, out var list)) return;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].IsInPool) continue;
                PutTip(list[i]);
            }
        }
        private  Tip CreateTip(TipOption option, System.Action<Tip> cb = null)
        {
            if (string.IsNullOrEmpty(option.tipName))
            {
                option.tipName = option.tipType.Name;
            }
            var tip = UIWindow.Ins.OpenWindow(option.tipType, option.prefabLink, option.layer,
                (displayUnit) =>
                {
                    ResetTip(displayUnit as Tip, option);
                    displayUnit.OpenWindow();
                    cb?.Invoke(displayUnit as Tip);
                }) as Tip; 
            tip.IsAutoDispose = false;
            if(createdPool.TryGetValue(option.tipName,out var list))
            {
                list.Add(tip);
            }
            else
            {
                createdPool.Add(option.tipName, new List<Tip>() { tip });
            }
             return tip as Tip;
        }

        public  void PutTip(Tip tip)
        {
            string name = tip.option.tipName;

            if (tipPool.ContainsKey(name) && tipPool[name].Contains(tip))
                return;

            if (!tipPool.ContainsKey(name))
                tipPool[name] = new Queue<Tip>();
            tipPool[name].Enqueue(tip);
            tip.OnPutToPool();
            tip.CloseWindow();
           


        }
    }
}
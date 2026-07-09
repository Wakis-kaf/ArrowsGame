using Framework.Runtime.UI;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleBar
{

public class BarPool
{
    private  Dictionary<string, List<Bar>> barPool = new Dictionary<string, List<Bar>>();

    public  Bar GetOrCreateBar(BarOption option, System.Action<Bar> cb = null)
    {
        if (option.usePoolBar)
        {
            return GetOrCreateBarFromPool(option, cb);
        }
        return CreateBar(option, cb);
    }

    private  Bar GetOrCreateBarFromPool(BarOption option, System.Action<Bar> cb = null)
    {
        string barTypeName = option.barType.Name;

        if (!barPool.ContainsKey(barTypeName) || barPool[barTypeName].Count == 0)
        {
            return CreateBar(option, cb);
        }

        var poolList = barPool[barTypeName];
        Bar bar = poolList[poolList.Count - 1];
        poolList.RemoveAt(poolList.Count - 1);

        ResetBar(bar, option);
        bar.OpenWindow();
        cb?.Invoke(bar);
        return bar;
    }

    private  void ResetBar(Bar bar, BarOption option)
    {
           bar.ResetByOption(option);
        
    }

    private  Bar CreateBar(BarOption option, System.Action<Bar> cb = null)
    {
        Bar bar = UIWindow.Ins.OpenWindow(option.barType, option.prefabLink, option.layer,
            (displayUnit) => {
                ResetBar(displayUnit as Bar, option);
                displayUnit.OpenWindow();
                cb?.Invoke(displayUnit as Bar);
            }) as Bar;
            bar.IsAutoDispose = false;
        return bar;
    }

    public  void PutBar(Bar bar)
    {
        string name = bar.option.barTypeName;

        if (barPool.ContainsKey(name) && barPool[name].Contains(bar))
        {
            return;
        }

        if (!barPool.ContainsKey(name))
        {
            barPool[name] = new List<Bar>();
        }

        bar.CloseWindow();
        barPool[name].Add(bar);
    }
}
}

using Framework.Runtime.MCombat;
using Framework.Utils;
using Game.Modules.GModuleInventory;
using Game.Modules.GModuleSceneUnit;
using Game.Runtime.Utils;
using System.Collections.Generic;

public class CfgArgs
{
    public int argType;
    public string argKey;
    public string argVal;
    public T GetData<T>(T defaultValue = default)
    {
        if( Utility.Convert.TryConvertToObject<T>(argVal, out T res, defaultValue))
        {
            return res;
        }
        return defaultValue;
    }
    public static T GetDataFromArgs<T>(CfgArgs[] args,string key, T defaultValue = default)
    {
        foreach (var item in args)
        {
            if (item.argKey == key)
            {
                return item.GetData<T>();
            }
        }
        return defaultValue;
    }
    public void SetArgs(DataManager targetContext, 
        AttributeBox targetAttrBox,
        int argType,
        string argKey,
        string argVal, 
        IEnumerable<KV> kvs = null
        )
    {
        argVal = StringFormatUtil.FormatStr(argVal, kvs);
        if (argType == ArgParamType.Context)
        {
            targetContext.SetDataAsString(argKey, argVal);
        }
        else if (argType == ArgParamType.NumAttr_BaseRoot)
        {
            targetAttrBox.GetNumberAttribute(argKey).
                SetValue(AttrLayerCode.Base,
                AttrNumCode.Root,
                DataManager.ParseStringToType<double>(argVal));
        }
        else if (argType == ArgParamType.NumAttr_RoleAdd)
        {
            targetAttrBox.GetNumberAttribute(argKey).
              SetValue(AttrLayerCode.Role,
              AttrNumCode.Add,
              DataManager.ParseStringToType<double>(argVal));
        }
        else if (argType == ArgParamType.NumAttr_BuffAdd)
        {
            targetAttrBox.GetNumberAttribute(argKey).
              SetValue(AttrLayerCode.Buff,
              AttrNumCode.Add,
              DataManager.ParseStringToType<double>(argVal));
        }
        else if (argType == ArgParamType.NumAttr_BuffPercent)
        {
            targetAttrBox.GetNumberAttribute(argKey).
              SetValue(AttrLayerCode.Buff,
              AttrNumCode.Percent,
              DataManager.ParseStringToType<double>(argVal));
        }else if(argType == ArgParamType.Dynamic_NumBaseRoot)
        {
            double dynamicValue = CombatDynamicNumeric.GetValue(argVal, 0, kvs);
            targetAttrBox.GetNumberAttribute(argKey).
                SetValue(AttrLayerCode.Base,
                AttrNumCode.Root,
                dynamicValue);
        }
    }
    public void ModifyArgs(DataManager targetContext,
       AttributeBox targetAttrBox,
       int argType,
       string argKey,
       string argVal, IEnumerable<KV> kvs = null
       )
    {
        argVal = StringFormatUtil.FormatStr(argVal, kvs);
        if (argType == ArgParamType.Context)
        {
            targetContext.SetDataAsString(argKey, argVal);
        }
        else if (argType == ArgParamType.NumAttr_BaseRoot)
        {
            targetAttrBox.GetNumberAttribute(argKey).
                ModifyValue(AttrLayerCode.Base,
                AttrNumCode.Root,
                DataManager.ParseStringToType<double>(argVal));
        }
        else if (argType == ArgParamType.NumAttr_RoleAdd)
        {
            targetAttrBox.GetNumberAttribute(argKey).
              ModifyValue(AttrLayerCode.Role,
              AttrNumCode.Add,
              DataManager.ParseStringToType<double>(argVal));
        }
        else if (argType == ArgParamType.NumAttr_BuffAdd)
        {
            targetAttrBox.GetNumberAttribute(argKey).
              ModifyValue(AttrLayerCode.Buff,
              AttrNumCode.Add,
              DataManager.ParseStringToType<double>(argVal));
        }
        else if (argType == ArgParamType.NumAttr_BuffPercent)
        {
            targetAttrBox.GetNumberAttribute(argKey).
              ModifyValue(AttrLayerCode.Buff,
              AttrNumCode.Percent,
              DataManager.ParseStringToType<double>(argVal));
        }
    }
    public void SetArgs(DataManager targetContext, AttributeBox targetAttrBox, IEnumerable<KV> kvs = null)
    {
        var item = this;
        SetArgs(targetContext,targetAttrBox,item.argType,item.argKey,item.argVal, kvs);
        
    }
    public void ModifyArgs(DataManager targetContext, AttributeBox targetAttrBox,IEnumerable<KV> kvs = null)
    {
        var item = this;
        ModifyArgs(targetContext, targetAttrBox, item.argType, item.argKey, item.argVal);
    }
}

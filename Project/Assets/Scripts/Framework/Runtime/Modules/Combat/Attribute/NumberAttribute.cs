using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Runtime.MCombat
{
    public class NumberAttribute
    {
        private Dictionary<int, NumberNumeric> m_Numerics = new Dictionary<int, NumberNumeric>(64)
        {
            //{(int)AttributeLayerCode.Base, new NumberNumeric()},
            //{(int)AttributeLayerCode.Equip,new NumberNumeric()},
            //{(int)AttributeLayerCode.Buff, new NumberNumeric()},
        };
        public NumberAttribute()
        {
            var baseNumeric = GetLayer((int)AttrLayerCode.Base);
            var roleNumeric = GetLayer((int)AttrLayerCode.Role);
            var equipNumeric = GetLayer((int)AttrLayerCode.Equip);
            var buffNumeric = GetLayer((int)AttrLayerCode.Buff);
        }

        public string ToDebugString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in m_Numerics)
            {
                sb.AppendLine($"** 数值层 {kvp.Key} ** ");
                sb.AppendLine(kvp.Value.ToDebugString());
                sb.AppendLine($"********************* ");
            }
            return sb.ToString();
        }

        private Action<NumberAttribute, NumberNumeric> m_Cb;
        private double m_Value;
        public double FinalValue
        {
            get
            {
                m_IsUpdateCbMute = true;
                UpdateFinalValue();
                m_IsUpdateCbMute = false;
                return m_Value;
            }
        }
        public float FloatFinalValue=>(float)FinalValue;
        public int IntFinalValue=>(int)FinalValue;
        private double m_RealtimeValue = 0;

        public double RealtimeValue
        {
            get
            {
                return m_RealtimeValue;
            }
        }

        private bool m_EnableRealtimeValueOverflow;

        public bool EnableRealtimeValueOverflow
        {
            get => m_EnableRealtimeValueOverflow;
            set => m_EnableRealtimeValueOverflow=value ;
        }

        public void ModifyValue(int layerCode, int numericCode, double value)
        {
            GetLayer(layerCode).ModifyValue(numericCode,value);
            // 更新数值
            UpdateFinalValue();
        }
        public void SetValue(int layerCode,int numericCode, double value)
        {
            GetLayer(layerCode).SetValue(numericCode,value);
            // 更新数值
            UpdateFinalValue();
        }

        private bool m_IsUpdateCbMute = false;
        public void SetUpdateCbMute(bool isMute =false)
        {
            m_IsUpdateCbMute = isMute;
        }
        public void UpdateFinalValue()
        {
            var baseNumeric = GetLayer((int)AttrLayerCode.Base);
            var roleNumeric = GetLayer((int)AttrLayerCode.Role);
            var equipNumeric = GetLayer((int)AttrLayerCode.Equip);
            var buffNumeric = GetLayer((int)AttrLayerCode.Buff);
            roleNumeric.SetValue((int)AttrNumCode.Root, baseNumeric.FinalValue);
            equipNumeric.SetValue((int)AttrNumCode.Root, roleNumeric.FinalValue);
            buffNumeric.SetValue((int)AttrNumCode.Root,equipNumeric.FinalValue);
            m_Value = buffNumeric.FinalValue;
            if (!EnableRealtimeValueOverflow)
            {
                ClampRealtimeValue();
            }
            CallUpdateCb();
        }

        private void ClampRealtimeValue()
        {
            m_RealtimeValue = Math.Max(m_RealtimeValue, m_Value);
        }

        private void SetRealtimeValue(double value)
        {
            value = Math.Max(m_RealtimeValue, m_Value);
            m_RealtimeValue = value;
        }

        private void CallUpdateCb(NumberNumeric numeric = null)
        {
            if(!m_IsUpdateCbMute)
                m_Cb?.Invoke(this,numeric);
        }

        public void SyncRealtimeValueToMax()
        {
            m_RealtimeValue = FinalValue;
        }
        public NumberNumeric GetLayer(int layerCode)
        {
            if (m_Numerics.TryGetValue(layerCode, out var attribute))
            {
                return attribute;
            }
            attribute = new NumberNumeric();
            m_Numerics.Add(layerCode,attribute);
            attribute.SetAttrUpdateCb((numeric) =>
            {
                CallUpdateCb(numeric);
            });
            return attribute;
        }

        public void AddAttrUpdateCb(Action<NumberAttribute, NumberNumeric> cb)
        {
            m_Cb -= cb;
            m_Cb += cb;
            m_Cb?.Invoke(this, null);
        }
        public void ModifyBaseRootValue(double value)
        {
            ModifyValue(AttrLayerCode.Base, AttrNumCode.Root, value);
        }
        public void SetBaseRootValue(double value){
            SetValue(AttrLayerCode.Base, AttrNumCode.Root, value);
        }
        
        public double GetBaseRootValue(){
            return this.GetLayer((int)AttrLayerCode.Base).GetValue((int)AttrNumCode.Root);
        }
        public double GetBaseLayerBaseValue()
        {
            return this.GetLayer((int)AttrLayerCode.Base).GetValue((int)AttrNumCode.Base);
        }
        
        public double GetBaseFinalValue()
        {
            return this.GetLayer((int)AttrLayerCode.Base).FinalValue;
        }

    }
}
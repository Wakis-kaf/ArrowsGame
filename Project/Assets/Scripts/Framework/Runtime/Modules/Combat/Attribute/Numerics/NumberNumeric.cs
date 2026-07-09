using System;
using System.Collections.Generic;
using System.Text;
namespace Framework.Runtime.MCombat
{
    public class AttrNumCode
    {
        /// <summary>
        /// 注意！除了底层能使用这个，其他层不能使用该层数码，否则会被覆盖
        /// </summary>
        public const int Root = 1;
        public const int Base = 2;
        public const int Percent = 3;
        public const int Add = 4;

    }
    public class AttrLayerCode
    {
        public const int Base = 1;
        public const int Role = 2;
        public const int Equip = 3;
        public const int Buff = 4; 
    }

    public class NumberNumeric
    {
        public NumberNumeric() {
            SetValue((int)AttrNumCode.Root,0);
            SetValue((int)AttrNumCode.Base, 0);
            SetValue((int)AttrNumCode.Percent, 0);
            SetValue((int)AttrNumCode.Add, 0);
        }  
        private Dictionary<int, double> m_Values = new Dictionary<int, double>()
        {
            //{(int)NumericCode.Root, 0},
            //{(int)NumericCode.Base, 0},
            //{(int)NumericCode.Percent, 0},
            //{(int)NumericCode.Add, 0},
        };
        public string ToDebugString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in m_Values)
            {
                sb.AppendLine($"数值码: {kvp.Key} 数值:{kvp.Value}");
            }

            return sb.ToString();
        }
        private HashSet<int> m_Codes = new HashSet<int>();
        public HashSet<int> Codes => m_Codes;
        private Action<NumberNumeric> m_Cb;
        private double m_Value;
        // 这个值只是作为当前的上限值，
        public double FinalValue
        {
            get
            {
                UpdateFinalValue();
                return m_Value;
            }
        }
        public void ModifyValue(int layerCode, double value)
        {
            SetValue(layerCode, GetValue(layerCode) + value);
        }
        public void SetValue(int layerCode, double value)
        {
            if (m_Values.ContainsKey(layerCode))
            {
                m_Values[layerCode] = value;
            }
            else
            {
                m_Codes.Add(layerCode);
                m_Values.Add(layerCode,value);
            }

            UpdateFinalValue();
        }

        private void UpdateFinalValue()
        {
            m_Value = (GetValue((int)AttrNumCode.Root) + GetValue((int)AttrNumCode.Base)) * ((100 + GetValue((int)AttrNumCode.Percent))/100) + GetValue((int)AttrNumCode.Add);
            m_Cb?.Invoke(this);
        }
        public void SetAttrUpdateCb(Action<NumberNumeric> cb = null)
        {
            m_Cb = cb;
        }
        public double GetValue(int layerCode)
        {
            m_Values.TryGetValue(layerCode, out var res);
            return res;
        }

       
    }
}
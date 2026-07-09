using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

namespace Framework.Runtime.MCombat
{
    public enum AttributeType
    {
        [LabelText("小数类型")]
        Float,
        [LabelText("布尔数类型")]
        Bool,
        [LabelText("整数类型")]
        Int,
        [LabelText("字符串类型")]
        String,
        [LabelText("多层数值类型")]
        NumberAttribute,
        [LabelText("字典类型")]
        Dictionary
    }
    public class Attribute
    {
        private int m_IntValue;
        private bool m_BoolValue;
        private float m_FloatValue;
        private string m_StringValue;
        private object m_ObjValue;
        private NumberAttribute m_NumberAttributeValue;
        private Action<Attribute> m_Cb;
        private Func<object, object, bool> equalAgent;
        public NumberAttribute NumberAttributeValue
        {
            get
            {
                if (m_NumberAttributeValue == null)
                {
                    m_NumberAttributeValue = new NumberAttribute();
                    m_NumberAttributeValue.AddAttrUpdateCb(OnNumberAttrUpdateCb);
                }
                    
                return m_NumberAttributeValue;
            }
        }

        private void OnNumberAttrUpdateCb(NumberAttribute attribute, NumberNumeric numeric)
        {
            m_Cb?.Invoke(this);
        }

        public int IntValue { 
            get => m_IntValue;
            set 
            {
                if (value != m_IntValue)
                {
                    m_IntValue = value;
                    m_Cb?.Invoke(this);
                }
                
            } 
        }
        public int AddIntValue(int value)
        {
            IntValue += value;
            return IntValue;
        }
        public float AddFloatValue(int value)
        {
            FloatValue += value;
            return FloatValue;
        }
        public bool BoolValue
        {
            get => m_BoolValue;
            set
            {
                if (value != m_BoolValue)
                {
                    m_BoolValue = value;
                    m_Cb?.Invoke(this);
                }
            }
        }
        public string StringValue
        {
            get => m_StringValue;
            set
            {
                if (value != m_StringValue)
                {
                    m_StringValue = value;
                    m_Cb?.Invoke(this);
                } 
            }
        }
        public float FloatValue
        {
            get => m_FloatValue;
            set
            {
                if (value != m_FloatValue)
                {
                    m_FloatValue = value;
                    m_Cb?.Invoke(this);
                }
            }
        }
        public object ObjValue
        {
            get => m_ObjValue;
            set
            {
                if (equalAgent != null )
                {
                    if(!equalAgent(m_ObjValue, value))
                    {
                        m_ObjValue = value;
                        m_Cb?.Invoke(this);
                    }
                }else if (value != m_ObjValue)
                {
                    m_ObjValue = value;
                    m_Cb?.Invoke(this);
                }
                
            }
        }
        public T GetObjValue<T>() {
            if (m_ObjValue == null) return default;
            return (T)ObjValue;
        }
        public bool TryGetObjValue<T>(out T res)
        {
            if (ObjValue is T tmp)
            {
                res = tmp;
                return true;
            }
            res = default;
            return false;
        }
        public void AddValueChangeCb(Action<Attribute> cb)
        {
            m_Cb += cb;
        }

        public void RemoveValueChangeCb(Action<Attribute> cb)
        {
            m_Cb -= cb;
        }
        public void SetValueChangeCb(Action<Attribute> cb = null)
        {
            m_Cb = cb;
        }
        public void SetObjValueEqual(Func<object,object,bool> equal=null)
        {
            equalAgent = equal;
        }
    }
    public class AttributeBox
    {
        private Dictionary<string, NumberAttribute> m_Name2AttributeMap = new Dictionary<string, NumberAttribute>(12);

        public bool TryFindNumberAttribute(string attrCode,out NumberAttribute numberAttribute)
        {
            if (m_Name2AttributeMap.TryGetValue(attrCode,out numberAttribute))
            {
                return true;
            }
            return false;
        }

        public NumberAttribute GetNumberAttribute(string attrCode){
            if(m_Name2AttributeMap.ContainsKey(attrCode)){
                return this.m_Name2AttributeMap[attrCode];
            }
            m_Name2AttributeMap[attrCode] = new NumberAttribute();
            return this.m_Name2AttributeMap[attrCode];
        }

        public void ClearAll()
        {
            m_Name2AttributeMap.Clear();
        }
    }
}
////using Assets.Plugins.Scripts.Comlib.Tools;
////using System;
////using System.Collections.Generic;
////using System.Linq;
////using System.Linq.Expressions;
////using System.Text;
////using System.Threading.Tasks;
//using org.mariuszgromada.math.mxparser;
//using System;
//using System.Collections.Generic;
//using UnityEngine;

//namespace UnitFramework.GameExtension.CombatSystemExtension
//{
//    /*
//     公式
//     */
//    public class NumericFormula: ScriptableObject
//    {
//        [SerializeField]
//        private string m_FormulaExpression; // 公式表达式
//        public string FormulaExpression=> m_FormulaExpression; // 公式表达式
//        public string formulaDescription;
        
//        //private static List<Constant> m_CommonAttrsConstant = new List<Constant>();
//        private Dictionary<int, Constant> m_AttrCodeToConstantMap = new Dictionary<int, Constant>();
//        private Dictionary<int, Constant> m_AttrCodeToLayerConstantMap = new Dictionary<int, Constant>();
//        //static NumericFormula()
//        //{
//        //    var attrs = Enum.GetValues(typeof(AttributeCodeType));
//        //    foreach (var item in attrs)
//        //    {
//        //        AttributeCodeType code = (AttributeCodeType)item;
//        //        if (code == AttributeCodeType.CustomAttribute) continue;
//        //        m_CommonAttrsConstant.Add(new Constant(code.ToString().ToLower()));
//        //    }
//        //}
//        public double GetCalculateValue(LevelAttrModel attrModel,string formulaExpression = "")
//        {
//            formulaExpression = string.IsNullOrEmpty(formulaExpression) ? this.FormulaExpression : formulaExpression;
//            Expression expression = new Expression(formulaExpression);
//            foreach (var code in attrModel.IntCodeSet)
//            {
//                expression.addConstants(FindAttrConstant(code, attrModel.GetIntValue(code)));
//            }
//            foreach (var code in attrModel.FloatCodeSet)
//            {
//                expression.addConstants(FindAttrConstant(code, attrModel.GetFloatValue(code)));
//            }
//            foreach (var code in attrModel.AttributeCodeSet)
//            {
//                NumberAttribute numberAttribute = attrModel.GetNumberAttribute(code);
//                expression.addConstants(FindAttrConstant(code, numberAttribute.RealtimeValue));
//                foreach (var layerCode in numberAttribute.LayerCodeSet)
//                {
//                    NumberNumeric numberNumeric = numberAttribute.GetNumeric(layerCode);
//                    expression.addConstants(FindAttrLayerConstant(code, layerCode, numberNumeric.FinalValue));

//                    foreach (var numericCode in numberNumeric.Codes)
//                    {
//                        expression.addConstants(FindAttrNumericConstant(code, layerCode, numericCode, numberNumeric.GetValue(numericCode)));
//                    }
//                }
//            }
//            CombatSystemLogger.Info($"公式表达式 : {expression.getExpressionString()}");
//            return expression.calculate();
//            //Constant x = new Constant("x", 10);
//            //Constant y = new Constant("y", 1);
//            //Expression expression = new Expression("x+y", x, y);
//        }
       
//        private string GetAttrCodeName(int attrCode)
//        {
//            var attrs = Enum.GetValues(typeof(AttributeCodeType));
//            foreach (var item in attrs)
//            {
//                AttributeCodeType code = (AttributeCodeType)item;
//                if (code == AttributeCodeType.CustomAttribute) continue;
//                if ((int)code == attrCode) return code.ToString();
//            }
//            return $"CustomAttrCode[{attrCode}]";
//        }
//        private string GetAttrCodeLayerName(int attrCode,int layerCode)
//        {
//            var attrs = Enum.GetValues(typeof(AttributeLayerCode));
//            string layerName = $"CustomLayer[{layerCode}]";
//            foreach (var item in attrs)
//            {
//                AttributeLayerCode code = (AttributeLayerCode)item;
//                if ((int)code == layerCode)
//                {
//                    layerName = code.ToString();
//                    break;
//                }
//            }
//            return $"{GetAttrCodeName(attrCode)}_{layerName}";
//        }
//        private string GetAttrCodeLayerNumericName(int attrCode, int layerCode,int numericCode)
//        {
//            var attrs = Enum.GetValues(typeof(NumericCode));
//            string layerName = $"CustomLayerNumeric[{numericCode}]";
//            foreach (var item in attrs)
//            {
//                NumericCode code = (NumericCode)item;
//                if ((int)code == layerCode)
//                {
//                    layerName = code.ToString();
//                    break;
//                }
//            }
//            return $"{GetAttrCodeName(attrCode)}_{GetAttrCodeLayerName(attrCode,layerCode)}_{layerName}";
//        }
//        private Constant FindAttrConstant(int attrCode, double value)
//        {
//            if (m_AttrCodeToConstantMap.TryGetValue(attrCode, out var res))
//            {
//                res.setConstantValue(value);
//                return res;
//            }
//            Constant constant = new Constant(GetAttrCodeName(attrCode), value);
//            m_AttrCodeToConstantMap.Add(attrCode, constant);
//            return constant;
//        }
//        private Constant FindAttrLayerConstant(int attrCode,int layerCode, double value)
//        {
//            if (m_AttrCodeToLayerConstantMap.TryGetValue(attrCode, out var res))
//            {
//                res.setConstantValue(value);
//                return res;
//            }
//            Constant constant = new Constant(GetAttrCodeLayerName(attrCode, layerCode), value);
//            m_AttrCodeToLayerConstantMap.Add(attrCode, constant);
//            return constant;
//        }
//        private Constant FindAttrNumericConstant(int attrCode, int layerCode, int numericCode, double value)
//        {
//            if (m_AttrCodeToLayerConstantMap.TryGetValue(attrCode, out var res))
//            {
//                res.setConstantValue(value);
//                return res;
//            }
//            Constant constant = new Constant(GetAttrCodeLayerNumericName(attrCode, layerCode, numericCode), value);
//            m_AttrCodeToLayerConstantMap.Add(attrCode, constant);
//            return constant;
//        }
//        void test()
//        {
//            bool isCallSuccessful = License.iConfirmNonCommercialUse("Johnny");
//            Constant x = new Constant("x", 10);
//            Constant y = new Constant("y", 1);
//            Expression expression = new Expression("x+y", x, y);
//            //Console.WriteLine(x.getConstantName() + " = " + x.getConstantValue());
//            //Console.WriteLine(y.getConstantName() + " = " + y.getConstantValue());
//            //Console.WriteLine("Res: " + expression.getExpressionString() + " = " + expression.calculate());
//            //Console.ReadKey();
//        }
      
//    }
//}

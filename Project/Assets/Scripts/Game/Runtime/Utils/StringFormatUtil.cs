using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
namespace Game.Runtime.Utils
{
    public static class StringFormatUtil 
    {
        private static readonly string[] Numbers = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
        private static readonly string[] Units = { "", "十", "百", "千" };
        private static readonly string[] BigUnits = { "", "万", "亿", "兆" };
        public static string FormatCurrency(double number, int decimals = 1)
        {
            bool isNegative = number < 0;
            number = Math.Abs(number);

            string formattedNumber;

            if (number >= 1000000000000)
            {
                formattedNumber = FormatWithUnit(number, 1000000000000, "G", decimals);
            }
            else if (number >= 1000000000)
            {
                formattedNumber = FormatWithUnit(number, 1000000000, "B", decimals);
            }
            else if (number >= 1000000)
            {
                formattedNumber = FormatWithUnit(number, 1000000, "M", decimals);
            }
            else if (number >= 1000)
            {
                formattedNumber = FormatWithUnit(number, 1000, "K", decimals);
            }
            else
            {
                formattedNumber = ((long)number).ToString();
            }

            return isNegative ? "-" + formattedNumber : formattedNumber;
        }

        private static string FormatWithUnit(double number, double divisor, string unit, int decimals)
        {
            double value = number / divisor;
            string formatString = decimals > 0 ? $"F{decimals}" : "F0";
            string formattedValue = value.ToString(formatString);

            if (decimals > 0 && formattedValue.EndsWith(new string('0', decimals)))
            {
                formattedValue = value.ToString("F0");
            }

            return formattedValue + unit;
        }
        public static string NumToChinese(this int num)
        {
            return ToChineseNumber(num);
        }
        public static string NumsToChinese(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return Regex.Replace(text, @"\d+", match =>
            {
                string numStr = match.Value;

                if (numStr.Length == 1)
                {
                    int singleDigit = int.Parse(numStr);
                    return Numbers[singleDigit];
                }

                return ToChineseNumber(int.Parse(numStr));
            });
        }
        public static string ToChineseNumber(int num)
        {
            if (num == 0) return "零";
            if (num < 0) return "负" + ToChineseNumber(-num);

            string numStr = num.ToString();
            StringBuilder result = new StringBuilder();

            int groupCount = (numStr.Length + 3) / 4;
            int start = 0;

            for (int group = groupCount - 1; group >= 0; group--)
            {
                int end = numStr.Length - group * 4;
                end = Math.Min(end, start + 4);

                string groupStr = numStr.Substring(start, end - start);
                string groupChinese = ConvertGroup(groupStr);

                if (!string.IsNullOrEmpty(groupChinese))
                {
                    result.Append(groupChinese);
                    result.Append(BigUnits[group]);
                }

                start = end;
            }

            string finalResult = result.ToString();

            if (finalResult.StartsWith("一十"))
            {
                finalResult = finalResult.Substring(1);
            }

            while (finalResult.Contains("零零"))
            {
                finalResult = finalResult.Replace("零零", "零");
            }

            if (finalResult.EndsWith("零"))
            {
                finalResult = finalResult.Substring(0, finalResult.Length - 1);
            }

            return finalResult;
        }

        private static string ConvertGroup(string groupStr)
        {
            StringBuilder result = new StringBuilder();
            bool lastWasZero = false;

            for (int i = 0; i < groupStr.Length; i++)
            {
                int digit = groupStr[i] - '0';

                if (digit == 0)
                {
                    lastWasZero = true;
                    continue;
                }

                if (lastWasZero)
                {
                    result.Append("零");
                    lastWasZero = false;
                }

                int unitIndex = groupStr.Length - i - 1;
                result.Append(Numbers[digit]);

                if (unitIndex > 0)
                {
                    result.Append(Units[unitIndex]);
                }
            }

            return result.ToString();
        }
        public static string FormatStr(string str, IEnumerable<KV> kvs)
        {
            if (kvs == null) return str;
            StringBuilder sb = new StringBuilder(str);
            foreach (var item in kvs)
            {
                FormatStr(sb, item.key, item.val);
                //sb.Replace($"{{{item.key}}}", item.val);
            }
            return sb.ToString();
        }
        public static string FormatStr(string str,string key,string strVal)
        {
            return str.Replace($"{{{key}}}", strVal);
        }
        public static void FormatStr(StringBuilder sb, string key, string strVal)
        {
            sb.Replace($"{{{key}}}", strVal);
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Runtime.Utils
{
    public class TimeFormatUtil
    {
        public static string FormatSeconds(int seconds, int formatCount = 4)
        {
            if (seconds < 0) return "00:00";
            if (formatCount < 1 || formatCount > 4) formatCount = 4;

            // 计算时间单位
            int days = seconds / (24 * 3600);
            int hours = (seconds % (24 * 3600)) / 3600;
            int minutes = (seconds % 3600) / 60;
            int secs = seconds % 60;

            // 存储时间单位数组
            int[] timeValues = { days, hours, minutes, secs };

            // 确定起始索引：优先显示高级别单位
            int startIndex = 0;

            // 从最高级别开始，尝试找到足够数量的非零单位
            for (int i = 0; i < 4; i++)
            {
                // 检查从这个位置开始，是否有足够单位显示
                if (4 - i >= formatCount)
                {
                    startIndex = i;
                    // 如果这个位置是0，但后面有非零值，继续向后找
                    if (timeValues[i] == 0)
                    {
                        // 检查后面是否还有非零值
                        bool hasNonZeroAhead = false;
                        for (int j = i; j < 4; j++)
                        {
                            if (timeValues[j] > 0)
                            {
                                hasNonZeroAhead = true;
                                break;
                            }
                        }
                        if (hasNonZeroAhead && i < 3)
                            continue;
                    }
                    break;
                }
            }

            // 构建结果字符串
            string result = "";
            for (int i = startIndex; i < startIndex + formatCount && i < 4; i++)
            {
                if (!string.IsNullOrEmpty(result))
                    result += ":";
                result += timeValues[i].ToString("D2");
            }

            return result;
        }
    }
}

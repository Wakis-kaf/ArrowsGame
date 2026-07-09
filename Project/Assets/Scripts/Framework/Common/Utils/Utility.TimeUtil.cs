using System;

namespace Framework.Utils
{
    public static partial class Utility
    {
        public static class TimeUtil
        {
            public static string GetNowTime(string format)
            {
                //return  DateTime.Now.ToString(" yyyyMMddHHmmssfff");
                return DateTime.Now.ToString(format);
            }

            public static string GetNowTime()
            {
                return DateTime.Now.ToString();
            }
        }
    }
}
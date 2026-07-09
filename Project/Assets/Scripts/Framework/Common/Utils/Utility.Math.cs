using System;

namespace Framework.Utils
{
    public static partial class Utility
    {
        public static class MathUtil
        {
            public static double Round(double value, int digits)
            {
                return Math.Round(value, digits);
            }
        }
    }
}
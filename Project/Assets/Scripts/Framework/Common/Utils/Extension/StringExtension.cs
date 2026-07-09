using Framework.Utils;
using System.Text.RegularExpressions;

public static class StringExtensions
{
    /// <summary>
    /// 忽略大小写替换字符串
    /// </summary>
    /// <param name="source">源字符串</param>
    /// <param name="oldValue">要替换的旧值</param>
    /// <param name="newValue">替换的新值</param>
    /// <returns>替换后的字符串</returns>
    public static string ReplaceIgnoreCase(this string source, string oldValue, string newValue)
    {
        if (source == null)
            return null;

        if (string.IsNullOrEmpty(oldValue))
            return source;

        // 使用正则表达式进行忽略大小写的替换
        return Regex.Replace(
            source,
            Regex.Escape(oldValue),
            Regex.Escape(newValue),
            RegexOptions.IgnoreCase
        );
    }

    public static T ConvetToObject<T>(this string str,T defaultValue = default)
    {
        bool res = Utility.Convert.TryConvertToObject<T>(str, out var  obj, defaultValue);
        return obj;

    }
}
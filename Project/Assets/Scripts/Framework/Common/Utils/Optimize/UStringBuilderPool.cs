using Framework.Misc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Misc
{
    public class UStringBuilderPool
    {
        private static Queue<StringBuilder> m_FreeStringBuilders = new Queue<StringBuilder>();

        public static string Concat(StringBuilder stringBuilder, string s1, string s2)
        {
            stringBuilder.Remove(0, stringBuilder.Length);
            stringBuilder.Append(s1);
            stringBuilder.Append(s2);
            return stringBuilder.ToString();
        }

        public static string Concat(StringBuilder stringBuilder, string s1, string s2, string s3)
        {
            stringBuilder.Remove(0, stringBuilder.Length);
            stringBuilder.Append(s1);
            stringBuilder.Append(s2);
            stringBuilder.Append(s3);
            return stringBuilder.ToString();
        }

        public static string Format(StringBuilder stringBuilder, string src, params object[] args)
        {
            stringBuilder.Remove(0, stringBuilder.Length);
            stringBuilder.AppendFormat(src, args);
            return stringBuilder.ToString();
        }

        public static StringBuilder GetSharedStringBuilder()
        {
            if (m_FreeStringBuilders.Count == 0) return new StringBuilder(32);
            var sb = m_FreeStringBuilders.Dequeue();
            sb.Remove(0, sb.Length);
            return sb;
        }

        public static StringBuilder GetSharedStringBuilder(string value)
        {
            if (m_FreeStringBuilders.Count == 0) return new StringBuilder(value);
            var sb = m_FreeStringBuilders.Dequeue();
            sb.Remove(0, sb.Length);
            sb.Append(value);
            return sb;
        }

        public static void Init()
        {
            Release();
        }

        public static void Release()
        {
            m_FreeStringBuilders.Clear();
        }

        public static void Release(StringBuilder stringBuilder)
        {
            m_FreeStringBuilders.Enqueue(stringBuilder);
        }
    }
}

public static class StringBuilderExtension
{
    public static StringBuilder ToLower(this StringBuilder sb)
    {
        for (int i = 0; i < sb.Length; i++)
        {
            sb[i] = Char.ToLower(sb[i]);
        }

        return sb;
    }

    public static string ToStringAndRelease(this StringBuilder sb)
    {
        string value = sb.ToString();
        UStringBuilderPool.Release(sb);
        return value;
    }

    public static StringBuilder ToUpper(this StringBuilder sb)
    {
        for (int i = 0; i < sb.Length; i++)
        {
            sb[i] = Char.ToUpper(sb[i]);
        }

        return sb;
    }
}
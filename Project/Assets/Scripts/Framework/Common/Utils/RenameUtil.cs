using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenameUtil
{
    public static string GetNoRepeatName(Func<string, bool> contains, string name)
    {
        var stack = new Stack<char>();
        while (contains.Invoke(name))
        {
            stack.Clear();
            int end = 0;
            for (var i = name.Length - 1; i >= 0; i--)
            {
                if (!char.IsNumber(name[i]))
                {
                    end = i + 1;
                    break;
                }

                stack.Push(name[i]);
            }

            var result = new string(stack.ToArray());
            //var numberWords = Regex.Match(name, @"\d+$").Value;
            int.TryParse(result, out int number);
            number++;
            // 数字加一，并替换
            name = name.Substring(0, end) + number;
        }

        return name;
    }
}
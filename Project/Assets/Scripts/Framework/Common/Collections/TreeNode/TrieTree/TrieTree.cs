using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Framework.NodeTree.TrieTreeStructure
{
    public class TrieTree
    {
        private Dictionary<string, Action<string, string[]>> _prefix2Handler;

        private Dictionary<string, int> _prefixTrieRegisterCount;

        private Action<string[]> _promptListeners;

        /// <summary>
        /// 前缀树根节点
        /// </summary>
        private TrieNode _trieRootNode;

        public TrieTree()
        {
            _trieRootNode = new TrieNode();
            _trieRootNode.data = new TrieNodeData();
            _prefixTrieRegisterCount = new Dictionary<string, int>();
            _prefix2Handler = new Dictionary<string, Action<string, string[]>>();
        }

        /// <summary>
        /// 添加联想监听
        /// </summary>
        /// <param name="func"></param>
        public void AddPromptListener(Action<string[]> func)
        {
            if (func == null) return;
            _promptListeners += func;
        }

        /// <summary>
        /// 输入前缀获取联想词缀
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public string[] GetPromptList(string prefix)
        {
            GetTrie(prefix, out var keyStringList, out var prefixKeyWords, out var suffixKeyWords);
            return keyStringList;
        }

        /// <summary>
        /// 根据前缀获取关键词
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns>返回一个元组，第一个为当前关键词条，第二个为前缀的前缀单词列表，第三个为后缀单词列表</returns>
        public void GetTrie(string prefix, out string[] curKeyWords, out string[] prefixKeyWords, out string[] suffixKeyWords)
        {
            curKeyWords = prefixKeyWords = suffixKeyWords = Array.Empty<string>();
            TrieNode longLengthNode = _trieRootNode;
            bool isWhileEnd = false;
            isWhileEnd = prefix.EndsWith(" ");
            prefix = PrefixPreHandle(prefix);
            if (!isWhileEnd)
                prefix = prefix.Trim();
            for (int i = 0; i < prefix.Length; i++)
            {
                char chr = prefix[i];
                TrieNode find = longLengthNode.FindTrieNode(chr);
                if (find == null)
                {
                    Debug.Log($"Not Find{chr}");
                    break;
                }
                longLengthNode = find;
            }

            // 空树就返回
            if (longLengthNode == _trieRootNode) return;
            // 否则就返回单词树
            string[] before = longLengthNode.GetBeforeWords();
            string[] after = longLengthNode.GetAfterWords();
            // string logInfo = $"最长匹配字符串为{longLengthNode.data.str}" +
            // $"当前词{longLengthNode.data.chr}" + $",是否为空字符{longLengthNode.data.chr == ' '}" + $"前缀单词数{before.Length}";

            if (after != null)
            {
                string[] res = (string[])after.Clone();
                if (before.Length != 0)
                {
                    string beforeWord = "";
                    for (int j = 0; j < before.Length; j++)
                    {
                        beforeWord += " " + before[j];
                    }
                    beforeWord = beforeWord.Trim();
                    for (int j = 0; j < after.Length; j++)
                    {
                        res[j] = beforeWord + " " + after[j];
                    }
                }
                // logInfo += $"后缀单词数{after.Length}"; Debug.Log(logInfo);
                curKeyWords = res;
                prefixKeyWords = before;
                suffixKeyWords = after;
                return;
            }
            curKeyWords = before;
            prefixKeyWords = before;
            return;
        }

        /// <summary>
        /// 当前前缀是否包含当前前缀
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public bool IsContains(string prefix)
        {
            prefix = PrefixPreHandle(prefix);
            string[] keyWords = GetPrompts(prefix);
            if (keyWords == null) return false;
            string targetPrefix = "";
            string[] suffixWords = null;
            bool findCmd = false;
            for (int i = 0; i < keyWords.Length; i++)
            {
                var word = keyWords[i];
                targetPrefix = targetPrefix + " " + word;
                targetPrefix = targetPrefix.Trim();
                if (_prefix2Handler.ContainsKey(targetPrefix))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 注册命令
        /// </summary>
        /// <param name="prefix"></param>
        public void Register(string prefix, Action<string, string[]> func)
        {
            if (!_prefixTrieRegisterCount.ContainsKey(prefix))
            {
                _prefixTrieRegisterCount.Add(prefix, 0);
            }
            AddTrie(prefix);
            _prefixTrieRegisterCount[prefix]++;
            AddPromptListener(prefix, func);
        }

        /// <summary>
        /// 移除命令
        /// </summary>
        /// <param name="prefix"></param>
        public void Remove(string prefix, Action<string, string[]> func)
        {
            if (_prefixTrieRegisterCount.ContainsKey(prefix))
            {
                RemoveTrie(prefix);
                _prefixTrieRegisterCount[prefix]--;
                if (_prefixTrieRegisterCount[prefix] == 0)
                {
                    _prefixTrieRegisterCount.Remove(prefix);
                }
            }
            RemovePromptListener(prefix, func);
        }

        /// <summary>
        /// 移除命令
        /// </summary>
        /// <param name="func"></param>
        public void RemovePromptListener(Action<string[]> func)
        {
            if (func == null) return;

            _promptListeners -= func;
        }

        /// <summary>
        /// 提交字符传，并触发该字符前缀有关的订阅事件
        /// </summary>
        public bool Submit(string prefix)
        {
            prefix = PrefixPreHandle(prefix);
            string[] keyWords = GetPrompts(prefix);
            bool hasFoundCmd = false;
            if (keyWords == null) return false;
            _promptListeners?.Invoke(keyWords);
            string targetPrefix = "";
            string[] suffixWords = null;
            for (int i = 0; i < keyWords.Length; i++)
            {
                var word = keyWords[i];
                targetPrefix = targetPrefix + " " + word;
                targetPrefix = targetPrefix.Trim();
                if (_prefix2Handler.ContainsKey(targetPrefix))
                {
                    int length = keyWords.Length - i - 1;
                    suffixWords = new string[length];
                    for (int j = 0; j < length; j++)
                    {
                        suffixWords[j] = keyWords[i + j + 1];
                    }
                    _prefix2Handler[targetPrefix]?.Invoke(targetPrefix, suffixWords);
                    hasFoundCmd = true;
                }
            }

            return hasFoundCmd;
        }

        private void AddPromptListener(string prefix, Action<string, string[]> func)
        {
            if (func == null) return;
            if (!_prefix2Handler.ContainsKey(prefix))
            {
                _prefix2Handler.Add(prefix, func);
            }
            else
            {
                _prefix2Handler[prefix] += func;
            }
        }

        /// <summary>
        /// 添加字符串到前缀数中
        /// </summary>
        /// <param name="prefix"></param>
        private void AddTrie(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return;
            }
            prefix = PrefixPreHandle(prefix);
            TrieNode start = _trieRootNode;
            for (int i = 0; i < prefix.Length; i++)
            {
                // 获取字符
                char chr = prefix[i];
                start = start.CreateTrieNode(chr);
            }
        }

        private string[] GetPrompts(string prefix)
        {
            prefix = prefix.Trim();
            if (String.IsNullOrEmpty(prefix)) return null;
            return prefix.Split(' ');
        }

        private string PrefixPreHandle(string prefix)
        {
            prefix = prefix.Trim();
            prefix = Regex.Replace(prefix, @"\s+", " "); // 替换多个空格为一个空格
            prefix = prefix + " ";
            return prefix;
        }

        private void RemovePromptListener(string prefix, Action<string, string[]> func)
        {
            if (func == null) return;
            if (_prefix2Handler.ContainsKey(prefix))
            {
                _prefix2Handler[prefix] -= func;
            }
        }

        /// <summary>
        /// 从前缀树中移除该字符串
        /// </summary>
        /// <param name="prefix"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void RemoveTrie(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return;
            }
            prefix = PrefixPreHandle(prefix);
            TrieNode start = _trieRootNode;
            List<TrieNode> nodes = new List<TrieNode>();
            for (int i = 0; i < prefix.Length; i++)
            {
                // 获取字符
                char chr = prefix.ElementAt(i);
                start = start.FindTrieNode(chr);
                if (start == null) break;
                nodes.Add(start);
            }
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                var node = nodes[i];
                node.data.count--;
                if (node.data.count == 0)
                {
                    node.Parent.RemoveNode(node);
                }
            }
        }
    }
}
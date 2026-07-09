using System.Collections.Generic;

namespace Framework.NodeTree.TrieTreeStructure
{
    public class TrieNode : TreeNode<TrieNodeData>
    {
        public TrieNode CreateTrieNode(char chr)
        {
            TrieNode node = FindTrieNode(chr);
            if (node != null)
            {
                node.data.count++;
                return node;
            }

            node = CreateAndAddNode<TrieNode>();
            node.data = new TrieNodeData();
            node.data.chr = chr;
            node.Init();
            return node;
        }

        public TrieNode FindTrieNode(char chr)
        {
            for (int i = 0; i < ChildCount; i++)
            {
                var item = GetChildNodeAt(i);
                if (item.data.chr == chr)
                {
                    return item as TrieNode;
                }
            }

            return null;
        }

        public string[] GetAfterWords(bool whiteSpaceStop = false)
        {
            if (data.chr == ' ' && whiteSpaceStop)
                return null;
            List<string> afterWords = new List<string>();
            if ((ChildCount == 0 || (ChildCount == 0 && GetChildNodeAt(0).data.chr == ' '))
                && !string.IsNullOrEmpty(data.wordStr))
            {
                afterWords.Add(data.wordStr);
                return afterWords.ToArray();
            }
            else
            {
                for (int i = 0; i < ChildCount; i++)
                {
                    var item = GetChildNodeAt(i) as TrieNode;
                    if (item.data.chr == ' ')
                    {
                        afterWords.Add(data.wordStr);
                    }

                    string[] child = item.GetAfterWords(true);
                    if (child != null)
                    {
                        afterWords.AddRange(child);
                    }
                }
            }

            return afterWords.ToArray();
        }

        public string[] GetBeforeWords()
        {
            TrieNode current = this;
            List<string> beforeWords = data.words;
            return beforeWords.ToArray();
        }

        private void Init()
        {
            TrieNode parent = GetParent<TrieNode>();
            data.str = parent.data.str + data.chr;
            data.count = 1;
            data.words = new List<string>(parent.data.words);
            if (data.chr != ' ')
            {
                data.wordStr = parent.data.wordStr + data.chr;
            }
            else
            {
                if (!string.IsNullOrEmpty(parent.data.wordStr))
                {
                    data.words.Add(parent.data.wordStr);
                }
            }
        }
    }
}
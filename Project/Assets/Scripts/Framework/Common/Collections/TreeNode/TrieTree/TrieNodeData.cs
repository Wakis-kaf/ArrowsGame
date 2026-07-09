using System.Collections.Generic;

namespace Framework.NodeTree.TrieTreeStructure
{
    public class TrieNodeData : INodeData
    {
        public char chr = char.MinValue;
        public int count = 0;
        public string str = "";
        public List<string> words = new List<string>();
        public string wordStr = "";
        public object nodeObject { get; set; }
    }
}
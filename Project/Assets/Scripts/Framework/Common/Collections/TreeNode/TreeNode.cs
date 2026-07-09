using Framework.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.NodeTree
{
    [System.Serializable]
    public class TreeNode<T> where T : INodeData
    {
        [SerializeField] private List<TreeNode<T>> _childNodes = new List<TreeNode<T>>();
        private T _data;
        private string _id;
        private Dictionary<string, TreeNode<T>> _id2Node = new Dictionary<string, TreeNode<T>>();
        private TreeNode<T> _parent;
        private TreeNode<T> _root;

        public TreeNode()
        {
            _root = this;
        }

        public int ChildCount => _childNodes.Count;

        public T data
        {
            get { return _data; }
            set
            {
                if (_data != null && !_data.Equals(value))
                {
                    _data.nodeObject = null;
                }

                OnDataSet(value);
                _data = value;
                _data.nodeObject = this;
            }
        }

        public bool HasData => data != null;
        public bool HasParent => !ReferenceEquals(Parent, null);
        public string Id => _id;
        public TreeNode<T> Parent => _parent;
        public TreeNode<T> Root => _root;

        public TreeNode<T> AddNode(TreeNode<T> node)
        {
            if (TryGetNode(node, out TreeNode<T> res))
            {
                // Debug
                Debug.Log("重复添加节点!");
                return default;
            }

            if (node.HasParent)
            {
                node.Parent.RemoveNode(node);
            }
            _childNodes.Add(node);
            _id2Node.Add(node.Id, node);
            node.SetRoot(this.Root);
            node.SetParent(this);
            node.OnAdd(this);
            OnAddChildNode(node);
            return node;
        }

        /// <summary>
        /// ��������ӽڵ�
        /// </summary>
        /// <returns></returns>
        public TreeNode<T> CreateAndAddNode()
        {
            return AddNode(CreateNode());
        }

        public TreeSub CreateAndAddNode<TreeSub>() where TreeSub : TreeNode<T>
        {
            return AddNode(CreateNode<TreeSub>()) as TreeSub;
        }

        public TreeNode<T> CreateNode()
        {
            TreeNode<T> node = new TreeNode<T>();
            node.SetId(Utility.IDGenerator.GetRandomIDStrByDoubleArray(16));
            return node;
        }

        public Tf CreateNode<Tf>() where Tf : TreeNode<T>
        {
            Tf node = System.Activator.CreateInstance<Tf>();
            node.data = System.Activator.CreateInstance<T>();
            node.SetId(Utility.IDGenerator.GetRandomIDStrByDoubleArray(16));
            return node;
        }

        public Tf Find<Tf>(Func<Tf, bool> findCondition) where Tf : TreeNode<T>
        {
            foreach (var node in _childNodes)
            {
                if (node is Tf findTF && findCondition(findTF))
                {
                    return findTF;
                }
            }

            return null;
        }

        public void GetAllChildNodes(List<TreeNode<T>> res)
        {
            res.Add(this);
            foreach (var item in _childNodes)
            {
                item.GetAllChildNodes(res);
            }
        }

        public List<TreeNode<T>> GetAllNodesDFS()
        {
            List<TreeNode<T>> res = new List<TreeNode<T>>();
            GetAllChildNodes(res);
            return res;
        }

        public TreeNode<T> GetChildNodeAt(int i)
        {
            return _childNodes[i];
        }

        public TParent GetParent<TParent>() where TParent : TreeNode<T>
        {
            return _parent as TParent;
        }

        public virtual void OnAdd(TreeNode<T> parent)
        {
        }

        public virtual void OnAddChildNode(TreeNode<T> childNode)
        {
        }

        public virtual void OnDataSet(T data)
        {
        }

        public virtual void OnRemove(TreeNode<T> parent)
        {
        }

        public void RemoveNode(TreeNode<T> node)
        {
            if (TryGetNode(node, out TreeNode<T> res))
            {
                node.OnRemove(this);
                node.SetParent(null);
                _childNodes.Remove(node);
                _id2Node.Remove(node.Id);
            }
        }

        public string SetId(string id)
        {
            _id = id;
            return Id;
        }

        public TreeNode<T> SetParent(TreeNode<T> parent)
        {
            _parent = parent;
            return parent;
        }

        public TreeNode<T> SetRoot(TreeNode<T> root)
        {
            _root = root;
            foreach (var item in _childNodes)
            {
                item.SetRoot(this.Root);
            }

            return _root;
        }

        public bool TryGetNode(TreeNode<T> find, out TreeNode<T> node)
        {
            node = default;
            if (_childNodes.Contains(find))
            {
                node = find;
                return true;
            }

            if (_id2Node.ContainsKey(find.Id))
            {
                Debug.LogWarning($"未找到node ID {find.Id}");
                node = _id2Node[find.Id];
                return true;
            }

            return false;
        }

        public bool TryGetNode(string nodeId, out TreeNode<T> node)
        {
            if (_id2Node.ContainsKey(nodeId))
            {
                node = _id2Node[nodeId];
                return true;
            }

            node = default;
            return false;
        }
    }
}
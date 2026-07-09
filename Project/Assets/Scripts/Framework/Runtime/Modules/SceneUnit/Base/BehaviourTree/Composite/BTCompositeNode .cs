using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Runtime.MSceneUnit.BT
{
    public class BTCompositeNode : BTNode
    {
        // 添加一个属性来访问子节点列表（用于RootNode）
        public List<BTNode> ChildNodeList => _childNodeList;
        // 组合节点维护子节点列表
        protected List<BTNode> _childNodeList = new List<BTNode>();

        protected override void OnConstructor()
        {
            base.OnConstructor();
            _childNodeList = new List<BTNode>();
        }
        //protected override void OnInit()
        //{
            
        //    base.OnInit();
        //}

        public void AddNode(BTNode node)
        {
            if (!HasNode(node) && CanAddNode(node))
            {
                node.SetParent(this);
                _childNodeList.Add(node);
            }
        }

        public void RemoveNode(BTNode node)
        {
            for (int i = 0; i < _childNodeList.Count; i++)
            {
                if (_childNodeList[i] == node)
                {
                    _childNodeList.RemoveAt(i);
                    break;
                }
            }
        }

        public bool HasNode(BTNode node)
        {
            foreach (BTNode v in _childNodeList)
            {
                if (v == node) return true;
            }
            return false;
        }

        public BTNode GetNodeAt(int index)
        {
            return _childNodeList[index];
        }

        public virtual bool CanAddNode(BTNode node)
        {
            return true;
        }

    }
}

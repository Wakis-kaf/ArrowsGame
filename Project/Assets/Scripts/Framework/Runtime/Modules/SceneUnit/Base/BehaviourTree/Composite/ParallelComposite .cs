using Framework.Runtime.MSceneUnit.BT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Runtime.MSceneUnit.BT
{
    public class ParallelComposite : BTCompositeNode
    {
        //private int _lastRunningIndex = 0;
        private HashSet<int> _sucSet = new HashSet<int>();
        protected override void OnConstructor()
        {
            _sucSet = new HashSet<int>();
        }
        public override void OnEnter()
        {
            //_lastRunningIndex = 0;
            _sucSet.Clear();
        }

        public override BTState OnRunning()
        {
            bool allSuccess = true;
            for (int i = 0; i < _childNodeList.Count; i++)
            {
                BTNode node = _childNodeList[i];
                if (_sucSet.Contains(i))
                {
                    continue;
                }
                BTState state = node.Execute();
                if (state == BTState.Fail)
                {
                    return BTState.Fail;
                }
                else if(state == BTState.Running)
                {
                    allSuccess = false;
                }
                else if (state == BTState.Success)
                {
                    _sucSet.Add(i);
                }
            }

            return allSuccess ? BTState.Success : BTState.Running;
        }

       
    }
}

// ParallelComposite.cs
/*
并行其下所有子节点
所有节点成功则返回成功（有任意子节点失败则失败）
若有任意子节点处于running，其必定处于running
*/
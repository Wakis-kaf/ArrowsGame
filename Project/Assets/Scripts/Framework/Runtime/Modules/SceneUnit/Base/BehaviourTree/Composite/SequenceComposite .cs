using Framework.Runtime.MSceneUnit.BT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Runtime.MSceneUnit.BT
{
    // SequenceComposite.cs
    /*
    顺序节点：依次执行子节点
    当前执行节点返回 Success，就继续执行后续节点

    当前执行节点返回 Fail，退出停止，向父节点
    返回 Fail，下次执行直接从第一个节点开始

    当前执行节点返回 Running, 记录当前节点，向父节
    点返回 Running，下次执行直接从该节点开始

    如果所有节点都返回 Success，向父节点返回 Success
    */
    public class SequenceComposite : BTCompositeNode
    {
        private int _lastRunningIndex = 0;


        public override void OnEnter()
        {
            _lastRunningIndex = 0;
        }

        public override BTState OnRunning()
        {
            for (int i = _lastRunningIndex; i < _childNodeList.Count; i++)
            {
                _lastRunningIndex = i;
                BTNode node = GetNodeAt(i);
                BTState state = node.Execute();
                if (state == BTState.Fail)
                {
                    return BTState.Fail;
                }
                else if (state == BTState.Running)
                {
                    return BTState.Running;
                }
            }
            return BTState.Success; // 注意：这里应该是 Success，因为所有节点都成功了
        }
    }
}

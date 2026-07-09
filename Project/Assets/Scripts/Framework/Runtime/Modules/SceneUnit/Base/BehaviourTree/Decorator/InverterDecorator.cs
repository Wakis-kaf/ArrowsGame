using Framework.Runtime.MSceneUnit.BT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Runtime.MSceneUnit.BT
{
    // InverterDecorator.cs
    /*
    修饰节点取反 - 对子节点执行结果取反
    */
    public class InverterDecorator : BTDecoratorNode
    {
        

        public override bool CanAddNode(BTNode node)
        {
            return _childNodeList.Count < 1; // 只能有一个子节点
        }

        public override BTState OnRunning()
        {
            BTNode firstNode = GetNodeAt(1);
            if (firstNode != null)
            {
                return InvertRes(firstNode.Execute());
            }
            return BTState.Fail;
        }

        private BTState InvertRes(BTState res)
        {
            if (res == BTState.Success)
            {
                return BTState.Fail;
            }
            else if (res == BTState.Fail)
            {
                return BTState.Success;
            }
            return BTState.Running;
        }
    }
}

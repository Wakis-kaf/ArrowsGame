using Framework.Runtime.MSceneUnit.BT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Runtime.MSceneUnit.BT
{
    // RepeatDecorator.cs
    /*
    修饰节点_重复:
    开始执行该节点时，将记录次数清零
    顺序执行所有子节点(记为 1 次)，不关心节点返回结果
    如果 执行次数 < 配置执行次数 向父节点返回 Running
    如果 执行次数 >= 配置执行次数 向父节点返回 Success
    */
    public class RepeatDecorator : BTDecoratorNode
    {
        public int RepeatTimes { get; set; } = 1;
        private int _curRepeatTimes = 0;

        
        public override void OnEnter()
        {
            RepeatTimes = 1;
            _curRepeatTimes = 0;
        }

        public override BTState OnRunning()
        {
            bool isRunning = false;

            for (int i = _curRepeatTimes; i < RepeatTimes; i++)
            {
                _curRepeatTimes = i;

                for (int j = 0; j < _childNodeList.Count; j++)
                {
                    BTNode node = GetNodeAt(j);
                    BTState state = node.Execute();

                    if (state == BTState.Fail)
                    {
                        return BTState.Fail;
                    }
                    else if (state == BTState.Running)
                    {
                        isRunning = true;
                    }
                }
            }

            if (isRunning)
            {
                return BTState.Running;
            }

            return BTState.Success;
        }
    }
}

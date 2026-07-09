using Framework.Runtime.MSceneUnit.BT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Runtime.MSceneUnit.BT
{
    public class SelectComposite : BTCompositeNode
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
                if (state == BTState.Success)
                {
                    return BTState.Success;
                }
                else if (state == BTState.Running)
                {
                    return BTState.Running;
                }
            }
            return BTState.Fail;
        }
    }
}

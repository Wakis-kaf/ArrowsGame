using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Runtime.MSceneUnit.BT
{
    public class RootNode : BTCompositeNode
    {
        protected override void OnInit()
        {
            RootNode = this;
        }

        public override BTState OnRunning()
        {
            foreach (BTNode node in ChildNodeList)
            {
                node.Execute();
            }
            return BTState.Running;
        }
    }
}

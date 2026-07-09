// BTNode.cs
using Framework.Runtime.MSceneUnit.BT;
namespace Framework.Runtime.MSceneUnit.BT
{
    public class BTNode
    {
        public BTNode ParentNode { get; protected set; } = null;
        public BTNode RootNode { get; protected set; } = null;
        public BTState State { get; protected set; } = BTState.Idle;

        public BTNode()
        {
            ParentNode = null;
            RootNode = null;
            State = BTState.Idle;
            OnConstructor();
            OnInit();
        }
        
        protected virtual void OnConstructor() { 
        
        }

        protected virtual void OnInit()
        {
            // 子类可重写
        }

        public void SetParent(BTNode parentNode)
        {
            ParentNode = parentNode;
            if (parentNode != null)
            {
                SetRoot(parentNode.RootNode);
            }
        }

        public void SetRoot(BTNode rootNode)
        {
            RootNode = rootNode;
        }

        public virtual void OnEnter()
        {
            // 子类可重写
        }

        public virtual void OnExit()
        {
            // 子类可重写
        }

        public virtual BTState OnRunning()
        {
            return BTState.Success;
        }

        public bool IsRunningSuccess()
        {
            return Execute() == BTState.Success;
        }

        public bool IsRunningFail()
        {
            return Execute() == BTState.Fail;
        }

        public bool IsRunning()
        {
            return Execute() == BTState.Running;
        }

        public BTState Execute()
        {
            if (State == BTState.Idle)
            {
                OnEnter();
            }

            BTState state = OnRunning();

            if (state != BTState.Running)
            {
                OnExit();
                State = BTState.Idle;
            }
            else
            {
                State = state;
            }

            return state;
        }
    }
}
using Framework.Runtime.UnitSystem.Base;
using Framework.Utils;
using System.Collections.Generic;

namespace Framework.Runtime.UnitSystem.BIInterfaces
{
    public class UnitBehaviourAwakeHandler : UnitBehaviourHandler<IUnitAwake>
    {
        private List<IUnitAwake> m_AwakeWaitingUnits = new List<IUnitAwake>(1000);

        /// <summary>
        /// Awake 优先级
        /// </summary>
        public override int Priority => 1000;

        public override void OnAssignUnitDeRegister(IUnitAwake assignUnit)
        {
            base.OnAssignUnitDeRegister(assignUnit);
            m_AwakeWaitingUnits.Remove(assignUnit);
        }

        public override void OnAssignUnitRegister(IUnitAwake assignUnit)
        {
            base.OnAssignUnitRegister(assignUnit);
            // 如果已经注册
            if (ReferenceEquals(assignUnit.OwnerBehaviourUnit, null) || assignUnit.OwnerBehaviourUnit.IsUnitEnable)
            {
                FunctionUtility.SafeCall(assignUnit.OnUnitAwake);
            }
            else
            {
                // 加入等待队列
                m_AwakeWaitingUnits.Add(assignUnit);
            }
        }

        public override void OnUnitModuleUpdate()
        {
            base.OnUnitModuleUpdate();
            int count = m_AwakeWaitingUnits.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var item = m_AwakeWaitingUnits[i];
                if (item.OwnerBehaviourUnit.IsUnitEnable)
                {
                    FunctionUtility.SafeCall(item.OnUnitAwake);
                    m_AwakeWaitingUnits.RemoveAt(i);
                    // 移出队列
                    //m_AwakeWaitingUnits[i] = m_AwakeWaitingUnits[count - 1];
                    //m_AwakeWaitingUnits.RemoveAt(count - 1);
                    //count -= 1;
                }
            }
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            m_AwakeWaitingUnits.Clear();
            m_AwakeWaitingUnits = null;
        }
    }
}
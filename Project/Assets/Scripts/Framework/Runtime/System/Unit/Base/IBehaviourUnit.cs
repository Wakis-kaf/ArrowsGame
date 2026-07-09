using Framework.Runtime.Base;

namespace Framework.Runtime.UnitSystem.Base
{
    public interface IBehaviourUnit : IUnitObject
    {
        public int ChildCount { get; }
        public bool IsUnitEnable { get; }
        public IBehaviourUnit OwnerBehaviourUnit { get; }
        public IBehaviourUnit ParentUnit { get; }
        public string UnitName { get; }

        /// <summary>
        /// 单位优先级，注入到父单位中时会根据优先级排序 排序按照优先级从高到低降序排序 优先级越高，Behaviour 方法越先执行，被回收时越后回收资源
        /// </summary>
        public int UnitPriority { get; } // 单元优先级

        public T AddChildUnit<T>(T unit) where T : IBehaviourUnit;

        public int ChildIndexOf(IBehaviourUnit behaviourUnit);

        public T GetUnit<T>() where T : IBehaviourUnit;

        public IBehaviourUnit GetUnitAt(int index);

        public bool HasChild(IBehaviourUnit child);

        public void RemoveChildUnit<T>() where T : IBehaviourUnit;

        public void RemoveChildUnit(IBehaviourUnit behaviourUnit);

        public void RemoveChildUnitAt(int index);

        public IBehaviourUnit SetParentUnit(IBehaviourUnit parent);

        public bool TryGetUnit<T>(out T unit) where T : IBehaviourUnit;

        public bool TryGetUnits<T>(out T[] units) where T : IBehaviourUnit;

        public void DisableUnit();

        public void EnableUnit();
    }
}
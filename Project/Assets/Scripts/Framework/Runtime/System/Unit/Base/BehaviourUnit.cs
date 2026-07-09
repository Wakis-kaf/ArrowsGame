using Framework.Runtime.Base;
using Framework.Runtime.LogSystem;
using System;
using System.Collections.Generic;

namespace Framework.Runtime.UnitSystem.Base
{
    public partial class BehaviourUnit : UnitObject, IBehaviourUnit
    {
        public BehaviourUnit()
        {
            m_ChildUnits = new List<IBehaviourUnit>();
            UnitCommonFunction.RegisterUnitToFrame(this);
            EnableUnit();
        }

        public void DisableUnit()
        {
            UnitCommonFunction.HandleUnitDisable(ref m_IsUnitEnable, this);
        }

        public void EnableUnit()
        {
            UnitCommonFunction.HandleUnitEnable(ref m_IsUnitEnable, ref m_IsFirstEnabled, this);
        }

        protected override void Dispose(bool isDisposeManagedResources)
        {
            if (IsDisposed) return;
            // 如果当前为启用 禁用
            if (m_IsUnitEnable)
            {
                DisableUnit();
            }
            try
            {
                UnitCommonFunction.DeRegisterUnitFromFrame(this);
            }
            catch (Exception e)
            {
                Log.Fatal(e);
            }
            // 注销单元
            base.Dispose(isDisposeManagedResources);
        }
    }

    /// <summary>
    /// Unit and unit public field
    /// </summary>
    public partial class BehaviourUnit
    {
        private List<IBehaviourUnit> m_ChildUnits;
        private bool m_IsFirstEnabled = false;
        private bool m_IsUnitEnable = true;
        private IBehaviourUnit m_Parent;
        public int ChildCount => m_ChildUnits.Count;
        public virtual bool IsUnitEnable => m_IsUnitEnable;
        public IBehaviourUnit OwnerBehaviourUnit => this;
        public IBehaviourUnit ParentUnit => m_Parent;

        public virtual string UnitName
        {
            get { return Type.Name; }
        }

        public virtual int UnitPriority => 0;
    }

    /// <summary>
    /// Mono Unit and unit public function
    /// </summary>
    public partial class BehaviourUnit
    {
        public T AddChildUnit<T>() where T : IBehaviourUnit
        {
            return UnitCommonFunction.AddChildUnit<T>(this);
        }

        public T AddChildUnit<T>(T unit) where T : IBehaviourUnit
        {
            return UnitCommonFunction.AddChildUnit(this, unit, ref m_ChildUnits);
        }

        public int ChildIndexOf(IBehaviourUnit behaviourUnit)
        {
            return UnitCommonFunction.ChildIndexOf(ref m_ChildUnits, behaviourUnit);
        }

        public T GetUnit<T>() where T : IBehaviourUnit
        {
            return UnitCommonFunction.GetUnit<T>(this);
        }

        public IBehaviourUnit GetUnitAt(int index)
        {
            return UnitCommonFunction.GetUnitAt(index, ref m_ChildUnits);
        }

        public bool HasChild(IBehaviourUnit child)
        {
            return UnitCommonFunction.HasChild(ref m_ChildUnits, child);
        }

        public void RemoveChildUnit(IBehaviourUnit behaviourUnit)
        {
            UnitCommonFunction.RemoveChildUnit(this, behaviourUnit);
        }

        public void RemoveChildUnit<T>() where T : IBehaviourUnit
        {
            UnitCommonFunction.RemoveChildUnit<T>(this, ref m_ChildUnits);
        }

        public void RemoveChildUnitAt(int index)
        {
            UnitCommonFunction.RemoveChildUnitAt(ref m_ChildUnits, index);
        }

        public IBehaviourUnit SetParentUnit(IBehaviourUnit parent)
        {
            return UnitCommonFunction.SetParent(this, parent, ref m_Parent);
        }

        public bool TryGetUnit<T>(out T unit) where T : IBehaviourUnit
        {
            return UnitCommonFunction.TryGetUnit<T>(ref m_ChildUnits, out unit);
        }

        public bool TryGetUnits<T>(out T[] units) where T : IBehaviourUnit
        {
            return UnitCommonFunction.TryGetUnits<T>(ref m_ChildUnits, out units);
        }

        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();

            UnitCommonFunction.DisposeManagedResources(ref m_ChildUnits, this);
        }
    }
}
using Framework.Runtime.LogSystem;
using Framework.Runtime.UnitSystem.Base;
using System;
using System.Collections.Generic;

namespace Framework.Runtime.UnitSystem.MonoBase
{
    public partial class MonoBehaviourUnit : MonoUnitObject, IBehaviourUnit
    {
        /// <summary>
        /// 关闭Unit在Unit管理系统中的功能,IUpdate等会失效
        /// </summary>
        public void DisableUnit()
        {
            try
            {
                UnitCommonFunction.HandleUnitDisable(ref m_IsUnitEnable, this);
            }
            catch (Exception e)
            {
                Log.Fatal(e);
            }
        }
        /// <summary>
        /// 开启Unit在Unit管理系统中的功能,IUpdate等会生效
        /// </summary>
        public void EnableUnit()
        {
            try
            {
                UnitCommonFunction.HandleUnitEnable(ref m_IsUnitEnable, ref m_IsFirstEnabled, this);
            }
            catch (Exception e)
            {
                Log.Fatal(e);
            }
        }

        protected virtual void Awake()
        {
            try
            {
                UnitCommonFunction.RegisterUnitToFrame(this);
            }
            catch (Exception e)
            {
                Log.Fatal(e);
            }
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
            base.Dispose(isDisposeManagedResources);
        }

        private void OnDisable()
        {
            DisableUnit();
        }

        private void OnEnable()
        {
            EnableUnit();
        }
    }

    public partial class MonoBehaviourUnit
    {
        private List<IBehaviourUnit> m_ChildUnits = new List<IBehaviourUnit>();
        private bool m_IsFirstEnabled = false;
        private bool m_IsUnitEnable = true;
        private IBehaviourUnit m_Parent;
        private UnitEnabledEventArgs m_UnitEnabledEventArgs;
        public int ChildCount => m_ChildUnits.Count;
        public bool IsUnitEnable => m_IsUnitEnable;
        public IBehaviourUnit OwnerBehaviourUnit => this;
        public IBehaviourUnit ParentUnit => m_Parent;

        public virtual string UnitName
        {
            get { return name; }
        }

        public virtual int UnitPriority => 0;
    }

    /// <summary>
    /// Mono Unit and unit public function
    /// </summary>
    public partial class MonoBehaviourUnit
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

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            UnitCommonFunction.DisposeManagedResources(ref m_ChildUnits, this);
        }

        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            UnitCommonFunction.DisposeUnManagedResources(ref m_ChildUnits, ref m_UnitEnabledEventArgs);
        }
    }
}
using System;
using System.Collections.Generic;

namespace Framework.Runtime.UnitSystem.Base
{
    public static class UnitCommonFunction
    {
        public static T AddChildUnit<T>(IBehaviourUnit owner) where T : IBehaviourUnit
        {
            T child = (T)Activator.CreateInstance(typeof(T));
            return AddChildUnit<T>(owner, child);
        }

        public static T AddChildUnit<T>(IBehaviourUnit parent, T child) where T : IBehaviourUnit
        {
            return parent.AddChildUnit(child);
        }

        public static T AddChildUnit<T>(IBehaviourUnit parent, T child, ref List<IBehaviourUnit> childs)
            where T : IBehaviourUnit
        {
            if (parent.HasChild(child)) return child;
            childs.Add(child);
            if (child.UnitPriority != 0)
            {
                childs.Sort(UnitSortCmp);
            }

            child.SetParentUnit(parent);
            return child;
        }

        public static int ChildIndexOf(IBehaviourUnit parent, IBehaviourUnit child)
        {
            return parent.ChildIndexOf(child);
        }

        public static int ChildIndexOf(ref List<IBehaviourUnit> childs, IBehaviourUnit child)
        {
            return childs.IndexOf(child);
        }

        public static void DeRegisterUnitFromFrame(IBehaviourUnit behaviourUnit)
        {
            UnitManager.Instance?.OnUnitDeRegister(behaviourUnit);
        }

        public static void DisposeManagedResources(ref List<IBehaviourUnit> mChildUnits, IBehaviourUnit owner)
        {
            DisposeChilds(mChildUnits);
            SetParent(owner, null);
        }

        public static void DisposeUnManagedResources(ref List<IBehaviourUnit> childs,
            ref UnitEnabledEventArgs unitEnabledEventArgs)
        {
            childs.Clear();
            childs = null;
            unitEnabledEventArgs = null;
        }

        public static T GetUnit<T>(IBehaviourUnit behaviourUnit) where T : IBehaviourUnit
        {
            TryGetUnit(behaviourUnit, out T find);
            return find;
        }

        public static IBehaviourUnit GetUnitAt(int index, ref List<IBehaviourUnit> childs)
        {
            if (childs.Count > index) return childs[index];
            return null;
        }

        public static void HandleUnitDisable(ref bool isUnitEnable, IBehaviourUnit behaviourUnit)
        {
            if (!isUnitEnable) return;
            isUnitEnable = false;
            UnitManager.Instance?.UnitDisable(behaviourUnit);
        }

        public static void HandleUnitEnable(ref bool isUnitEnable, ref bool isUnitFirstEnabled,
            IBehaviourUnit behaviourUnit)
        {
            if (isUnitEnable && isUnitFirstEnabled) return;
            isUnitEnable = true;
            isUnitFirstEnabled = true;
            UnitManager.Instance?.UnitEnable(behaviourUnit);
        }

        public static bool HasChild(ref List<IBehaviourUnit> childs, IBehaviourUnit child)
        {
            return childs.Contains(child);
        }

        public static void RegisterUnitToFrame(IBehaviourUnit behaviourUnit)
        {
            UnitManager.Instance?.OnUnitRegister(behaviourUnit);
        }

        public static void RemoveChildUnit(IBehaviourUnit owner, IBehaviourUnit child)
        {
            int index = ChildIndexOf(owner, child);
            if (index != -1)
            {
                RemoveChildUnitAt(owner, index);
                SetParent(child, null);
            }
        }

        public static void RemoveChildUnit<T>(IBehaviourUnit owner) where T : IBehaviourUnit
        {
            owner.RemoveChildUnit<T>();
        }

        public static void RemoveChildUnit<T>(IBehaviourUnit owner, ref List<IBehaviourUnit> childs)
                    where T : IBehaviourUnit
        {
            int count = childs.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var child = childs[i];
                if (child is T res)
                {
                    childs.RemoveAt(i);
                    SetParent(child, null);
                }
            }
        }

        public static void RemoveChildUnitAt(IBehaviourUnit parent, int index)
        {
            parent.RemoveChildUnitAt(index);
        }

        public static void RemoveChildUnitAt(ref List<IBehaviourUnit> mChildUnits, int index)
        {
            if (index >= mChildUnits.Count) return;
            mChildUnits.RemoveAt(index);
        }

        public static IBehaviourUnit SetParent(IBehaviourUnit child, IBehaviourUnit newParent)
        {
            return child.SetParentUnit(newParent);
        }

        public static IBehaviourUnit SetParent(IBehaviourUnit child, IBehaviourUnit newParent,
                    ref IBehaviourUnit oldParent)
        {
            if (ReferenceEquals(newParent, oldParent)) return newParent;
            if (!ReferenceEquals(oldParent, null)) RemoveChildUnit(oldParent, child);
            if (!ReferenceEquals(newParent, null)) AddChildUnit(newParent, child);
            oldParent = newParent;
            return oldParent;
        }

        public static bool TryGetUnit<T>(IBehaviourUnit owner, out T find) where T : IBehaviourUnit
        {
            return owner.TryGetUnit(out find);
        }

        public static bool TryGetUnit<T>(ref List<IBehaviourUnit> childs, out T unit) where T : IBehaviourUnit
        {
            int count = childs.Count;
            unit = default;
            for (int i = 0; i < count; i++)
            {
                if (childs[i] is T res)
                {
                    unit = res;
                    return true;
                }
            }

            return false;
        }

        public static bool TryGetUnits<T>(IBehaviourUnit owner, out T[] findUnits) where T : IBehaviourUnit
        {
            return owner.TryGetUnits<T>(out findUnits);
        }

        public static bool TryGetUnits<T>(ref List<IBehaviourUnit> units, out T[] findUnits) where T : IBehaviourUnit
        {
            List<T> result = new List<T>();
            int count = units.Count;
            for (int i = 0; i < count; i++)
            {
                if (units[i] is T res)
                {
                    result.Add(res);
                }
            }

            findUnits = result.ToArray();
            return true;
        }

        /// <summary>
        /// 按照优先级进行降序排序///
        /// </summary>
        public static int UnitSortCmp(IBehaviourUnit unit1, IBehaviourUnit unit2)
        {
            return unit1.UnitPriority < unit2.UnitPriority ? 1 : -1;
        }

        private static void DisposeChilds(List<IBehaviourUnit> childs)
        {
            int count = childs.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                childs[i].Dispose();
            }
        }
    }
}
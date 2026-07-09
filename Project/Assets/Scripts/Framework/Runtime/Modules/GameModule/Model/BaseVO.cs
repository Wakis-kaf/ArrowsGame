using Framework.Runtime.Base;
using Framework.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Modules
{
    public class BaseVO : UnitObject
    {
        public BaseVO()
        {
            OnInit();
        }
        protected virtual void OnInit()
        {

        }
        public virtual void UpdateVO()
        {

        }
        public static T DeepCopyByBinary<T>(T obj)
        {
            return Utility.Binary.DeepCopyByBinary(obj);
        }
    }
}
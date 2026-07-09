using Framework.Runtime.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Runtime
{
    public interface IMessageSubscriber:IUnitObject
    {
        public bool IsActiveSubscriber { get;}
    }
}

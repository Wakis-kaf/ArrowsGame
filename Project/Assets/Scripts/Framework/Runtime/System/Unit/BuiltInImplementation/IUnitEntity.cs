using Framework.Runtime.UnitSystem.BIInterfaces;

namespace Framework.Runtime.UnitSystem
{
    public interface IUnitEntity :
        IUnitAwake,
        IUnitEnable,
        IUnitStart,
        IUnitDisable,
        IUnitDestroy
    {
    }
}
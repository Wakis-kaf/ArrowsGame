using Framework.Runtime.MGameModule;

namespace Game.Modules.GModuleSceneUnit
{
    public class GameSceneUnitClientHandler : GameModuleLogicHandler
    {
        public static GameSceneUnitClientHandler Ins => GameModuleHandler.GetModuleHandlerIns<GameSceneUnitClientHandler>();

        public GameSceneUnitPool GameSceneUnitPool { get; private set; }

        protected override void OnHandlerAwake()
        {
            GameSceneUnitPool = new GameSceneUnitPool();
        }

        protected override void OnHandlerDestroy()
        {
        }

        protected override void OnHandlerEnable()
        {
        }

        protected override void OnHandlerStart()
        {
        }
    }
}
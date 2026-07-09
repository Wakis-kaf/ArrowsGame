using Framework.Runtime.MGameModule;
using Framework.Runtime.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleArrowGenerateEdit
{
    public class GameArrowGenerateEditViewHandler : GameModuleViewHandler
    {
        public static GameArrowGenerateEditViewHandler Ins => GetModuleHandlerIns<GameArrowGenerateEditViewHandler>();

        protected override void OnHandlerAwake()
        {

        }
        protected override void OnHandlerEnable()
        {

        }
        protected override void OnHandlerStart()
        {

        }
        protected override void OnHandlerDestroy()
        {

        }

        public void OpenArrowsGenerateEditPanel()
        {
            Panel.OpenPanel<ArrowsGenerateEditPanel>();
        }
        public void OpenArrowsGenerateOptionPanel()
        {
            Panel.OpenPanel<ArrowsGenerateOptionPanel>();
        }
    }

}

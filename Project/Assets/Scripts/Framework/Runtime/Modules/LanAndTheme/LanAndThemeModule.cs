using System.Collections;
using System.Collections.Generic;
using Framework.Runtime.Module.Core;
using Sirenix.OdinInspector;
using UnityEngine;
namespace Framework.Runtime.MLanAndTheme
{
    public class LanAndThemeModule : ModuleUnit
    {

        public Lan2LocalManager Lan2LocalManager { get; private set; }
        public Theme2LocalManager Theme2LocalManager { get; private set; }
        protected override void OnInit()
        {
            base.OnInit();
            Lan2LocalManager = new Lan2LocalManager();
            Theme2LocalManager = new Theme2LocalManager();
        }
    }
}
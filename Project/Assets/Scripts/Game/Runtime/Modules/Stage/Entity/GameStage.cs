using Framework.Runtime.MSceneUnit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.Modules.GModuleStage
{
    public class GameStage : SceneUnit
    {
        protected override void OnRootShow()
        {
            base.OnRootShow();
            OnStageShow();
        }
        protected override void OnRootHide()
        {
            base.OnRootHide();
            OnStageHide();
        }
        public virtual void OnStageHide()
        {
          
        }

        public virtual void OnStageShow()
        {
           
        }
        public virtual void OnStageLoaded()
        {

        }
        protected override void OnModelLoaded(GameObject modelGamObject)
        {
            base.OnModelLoaded(modelGamObject);
            OnStageLoaded();

        }
    }
}

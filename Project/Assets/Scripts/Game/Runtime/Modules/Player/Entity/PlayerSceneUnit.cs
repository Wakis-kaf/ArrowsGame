using Framework.Runtime;
using Framework.Runtime.MSceneUnit;
using Framework.Runtime.UnitSystem.BIInterfaces;
using Game.Modules.GModuleInput;
using Game.Modules.GModuleManage;
using Game.Modules.GModuleSceneUnit;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace Game.Modules.GModulePlayer {
    public class PlayerSceneUnit : RoleSceneUnit,IUnitUpdate
    {
        protected override void OnModelLoaded(GameObject gameObject)
        {
            base.OnModelLoaded(gameObject);
            TPSCameraController cameraController = GetOrAddSceneUnitComponent<TPSCameraController>();
            cameraController.SetFollowTransform(EntityRoot);
            cameraController.SetLookAtTransform(EntityRoot);
            cameraController.PitchAngle = 50;
            cameraController.Distance = 10;
            cameraController.SmoothSpeed = 5;

            RbMoveController rd2MoveController = GetOrAddSceneUnitComponent<RbMoveController>();
            rd2MoveController.Rigidbody.freezeRotation = true;
            rd2MoveController.MoveSpeed = 5;
            EnabelJoyMove();

        }
        //public override void OnUnitUpdate()
        //{
        //    base.OnUnitUpdate();
           
        //}
        private Vector2 GetPlayerInput()
        {

            var layer = GameInputClientHandler.Ins.GetPlayerInputLayer();
            var horizontal = layer.ValueRaw(GameInputConstant.Play_Input_Horzontal_Name);
            var vertical = layer.ValueRaw(GameInputConstant.Play_Input_Vertical_Name);
            return new Vector3(horizontal, vertical);
        }
        public void EnabelJoyMove()
        {
            RbMoveController rd2MoveController = GetOrAddSceneUnitComponent<RbMoveController>();
            rd2MoveController.EnableMove();
            GameApp.Ins.LoopManager.AddLoop(TickInput);
            //rd2MoveController.AddMoveChangeListener(OnMoveChange);
        }

        private void TickInput()
        {
            var rb2MoveController = FindSceneUnitComponent<RbMoveController>();
            if (!rb2MoveController.IsMoveEnable) return;
            rb2MoveController.SetMoveInput(GetPlayerInput());
        }

        public void DisableJoyMove()
        {
            RbMoveController rd2MoveController = GetOrAddSceneUnitComponent<RbMoveController>();
            rd2MoveController.DisableMove();
            GameApp.Ins.LoopManager.RemoveLoop(TickInput);
            //rd2MoveController.RemoveMoveChangeListener(OnMoveChange);
            //OnMoveChange(Vector3.zero);
        }
        protected override void OnRootShow()
        {
            base.OnRootShow();
            TPSCameraController cameraController = GetOrAddSceneUnitComponent<TPSCameraController>();
            GameApp.Ins.CameraStackManager.RegisterCamera(cameraController.Camera);
            GameApp.Ins.CameraStackManager.SetBaseCamera(cameraController.Camera);
        }
        protected override void OnRootHide()
        {
            base.OnRootHide();
            TPSCameraController cameraController = GetOrAddSceneUnitComponent<TPSCameraController>();
            GameApp.Ins.CameraStackManager.UnregisterCamera(cameraController.Camera);
        }

    }

}


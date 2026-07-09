using System;
using System.Collections.Generic;
using Framework.Runtime;
using Framework.Runtime.CameraManage;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.UI;
using Game.Modules.GModuleAudio;
using Game.Modules.GModuleStage;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Modules.GModuleArrows
{
	public class ArrowsGameStage : GameStage
	{
		#region PrefabBinder 自动引用区域 开始
		public UnityEngine.Transform transArrowsRoot => EntityPrefabBinder?.GetObj<UnityEngine.Transform>("transArrowsRoot");
		public UnityEngine.Transform transPointsRoot => EntityPrefabBinder?.GetObj<UnityEngine.Transform>("transPointsRoot");
		public UnityEngine.Camera gameCamera => EntityPrefabBinder?.GetObj<UnityEngine.Camera>("gameCamera");
		public Game.Modules.GModuleArrows.ArrowsGameCameraController arrowsGameCameraController => EntityPrefabBinder?.GetObj<Game.Modules.GModuleArrows.ArrowsGameCameraController>("arrowsGameCameraController");
		public Game.Modules.GModuleArrows.ArrowPointSceneUnit arrowPointSceneUnitPrefab => EntityPrefabBinder?.GetObj<Game.Modules.GModuleArrows.ArrowPointSceneUnit>("arrowPointSceneUnitPrefab");
		public Game.Modules.GModuleArrows.ArrowLineSceneUnit arrowLineSceneUnitPrefab => EntityPrefabBinder?.GetObj<Game.Modules.GModuleArrows.ArrowLineSceneUnit>("arrowLineSceneUnitPrefab");

		#endregion PrefabBinder 自动引用区域 结束
		public override void OnStageShow()
		{
			base.OnStageShow();

			CameraStackManager.Instance.RegisterCamera(gameCamera);
			CameraStackManager.Instance.SetBaseCamera(gameCamera);
			// MessageDispatcher.Ins.Dispatch(MessageCode.msg_open_gameplay_panel);
			MessageDispatcher.Ins.Subscribe<LevelStatus>(MessageCode.msg_on_level_status_change, OnLevelStatusChange);
			MessageDispatcher.Ins.Subscribe<float, bool>(MessageCode.msg_set_camera_zoom, OnSetCameraZoom);
			MessageDispatcher.Ins.Subscribe<float, bool>(MessageCode.msg_add_camera_zoom, OnAddCameraZoom);
			arrowsGameCameraController.BindCamera(gameCamera);
			GameApp.Ins.LoopManager.AddLoop(CheckPointerClick);
		}
		private void OnAddCameraZoom(float zoomStep, bool isQuick)
		{
			var targetZoom = arrowsGameCameraController.TargetZoomScale;
			arrowsGameCameraController.SetTargetZoom(targetZoom + zoomStep, isQuick);
		}
		private void OnSetCameraZoom(float zoom, bool isQuick)
		{
			arrowsGameCameraController.SetTargetZoom(zoom, isQuick);
		}

		private void OnLevelStatusChange(LevelStatus status)
		{
			if (status == LevelStatus.Playing)
			{
				ArrowsGameCameraInput.Ins.EnbaleInput();
			}
			else
			{
				ArrowsGameCameraInput.Ins.DisableInput();
			}


		}

		public override void OnStageHide()
		{
			base.OnStageHide();
			CameraStackManager.Instance.UnregisterCamera(gameCamera);
			// MessageDispatcher.Ins.Dispatch(MessageCode.msg_close_gameplay_panel);
			MessageDispatcher.Ins.Unsubscribe<LevelStatus>(MessageCode.msg_on_level_status_change, OnLevelStatusChange);
			MessageDispatcher.Ins.Unsubscribe<float, bool>(MessageCode.msg_set_camera_zoom, OnSetCameraZoom);
			GameApp.Ins.LoopManager.RemoveLoop(CheckPointerClick);
		}
		private void CheckPointerClick()
		{
			if (!Input.GetMouseButtonUp(0)) return;
			if (arrowsGameCameraController.IsZoomingStopStabled &&
			ArrowsGameCameraInput.Ins.ShouldCheckNullPoint()
			&& !ArrowsGameCameraInput.Ins.IsDragging
			&& !ArrowsGameCameraInput.Ins.IsZooming
			&& ArrowsGameCameraInput.Ins.IsInputEnabled)
			{
				GameAudioClientHandler.Ins.PlayEffect(GameAudioConstant.Eff_ScreenClick);
				LevelVO.Current.CheckPointTriggerByMouse(LevelVO.Current.LevelInfo.LevelAnimArgs.worldPointDetectRadius);
				PlayArrowClickPointTip();
			}

		}
		private void PlayArrowClickPointTip()
		{
			ArrowClickPointTipWindowData data = new ArrowClickPointTipWindowData();
			data.innerScreeRadius = UIUtil.WorldToScreenSpace(gameCamera, LevelVO.Current.LevelInfo.LevelAnimArgs.worldPointDetectInnerRadius);
			data.outerScreeRadius = UIUtil.WorldToScreenSpace(gameCamera, LevelVO.Current.LevelInfo.LevelAnimArgs.worldPointDetectRadius);
			data.showTipScreenPos = Input.mousePosition;
			MessageDispatcher.Ins.Dispatch(MessageCode.msg_play_arrow_click_point_tip, data);
		}


		public void ResetStage()
		{
			var layout = LevelVO.Current.LevelPointLayout;
			var levelArgs = LevelVO.Current.LevelInfo.levelArgs;
			arrowsGameCameraController.SetWorldArea(layout.MinAreaX, layout.MaxAreaX, layout.MinAreaY, layout.MaxAreaY);
			arrowsGameCameraController.SetArgs(levelArgs.minZoomScale, levelArgs.startZoomScale, levelArgs.maxZoomScale, levelArgs.scrollSpeed);
			arrowsGameCameraController.SetZoomSpeed(levelArgs.gameZoomSpeed);
			arrowsGameCameraController.SetTargetZoom(levelArgs.minZoomScale, true);
			// arrowsGameCameraController.SetTargetZoom(levelArgs.startZoomScale);

		}

		public void PlayStage()
		{
			var levelArgs = LevelVO.Current.LevelInfo.levelArgs;
			// arrowsGameCameraController.SetZoomSpeed(levelArgs.gameZoomSpeed);
			if (GameArrowsClientHandler.Ins.IsQuickAnimModel())
			{
				arrowsGameCameraController.SetTargetZoom(levelArgs.minZoomScale);
			}
			else
			{
				arrowsGameCameraController.SetTargetZoom(levelArgs.startZoomScale);
			}

		}

		public void DoGameSuccessAnim()
		{
			var levelArgs = LevelVO.Current.LevelInfo.levelArgs;
			arrowsGameCameraController.SetTargetZoom(levelArgs.minZoomScale);
			arrowsGameCameraController.SnapToCenter();
		}
	}
}










using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Framework.Runtime;
using Framework.Runtime.Archives;
using Framework.Runtime.LogSystem;
using Framework.Runtime.MAsset;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.UI;
using Game.Modules.GModuleArrows;
using Game.Modules.GModuleManage;
using Game.Modules.GModuleInventory;
namespace Game.Modules
{
	public class DebugGMView : View
	{
		#region PrefabBinder 自动引用区域 开始
		private Framework.Runtime.UI.UButton ubtnJumpLv;
		private Framework.Runtime.UI.UInputField uifJumpLevelId;
		private Framework.Runtime.UI.UButton ubtnLvUp;
		private Framework.Runtime.UI.UDropSelect udsLvUpItemType;
		private Framework.Runtime.UI.UButton ubtnGameSuccess;
		private Framework.Runtime.UI.UButton ubtnPringIsGameHasAllSolve;
		private UnityEngine.GameObject goGroupGame;
		private Framework.Runtime.UI.UButton ubtnClearEquipInv;
		private Framework.Runtime.UI.UButton ubtnAddAfkOneHour;
		private Framework.Runtime.UI.UButton ubtnClearAllArchives;
		private Framework.Runtime.UI.UButton ubtnClearGuideArchives;
		private UnityEngine.GameObject goGroupArchive;
		private Framework.Runtime.UI.UButton ubtnSetFbPassLv;
		private Framework.Runtime.UI.UInputField uifFbLevelId;
		private Framework.Runtime.UI.UInputField uifFbId;
		private UnityEngine.RectTransform transPassTargetFb;
		private Framework.Runtime.UI.UButton ubtnClearFbArchives;
		private Framework.Runtime.UI.UButton ubtnClearFbTryCountArchives;
		private UnityEngine.GameObject goGroupFb;
		private Framework.Runtime.UI.UButton ubtnClearCurrencyInv;
		private Framework.Runtime.UI.UButton ubtnGetRdmEquip;
		private Framework.Runtime.UI.UButton ubtnAddPower;
		private Framework.Runtime.UI.UButton ubtnAddEquipUpItem;
		private Framework.Runtime.UI.UButton ubtnAddSpecTalentUpItem;
		private Framework.Runtime.UI.UButton ubtnAddTalentUpItem;
		private Framework.Runtime.UI.UButton ubtnAddRoleStarUpItem;
		private Framework.Runtime.UI.UButton ubtnAddRoleLvUpItem;
		private Framework.Runtime.UI.UButton ubtnAddMoney;
		private UnityEngine.GameObject goGroupCurrency;
		private UnityEngine.GameObject goGroupTemplate;
		private UnityEngine.RectTransform rtContent;

		#endregion PrefabBinder 自动引用区域 结束

		protected override void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
		{
			this.ubtnJumpLv = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnJumpLv");
			this.uifJumpLevelId = prefabBinder.GetObj<Framework.Runtime.UI.UInputField>("uifJumpLevelId");
			this.ubtnLvUp = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnLvUp");
			this.udsLvUpItemType = prefabBinder.GetObj<Framework.Runtime.UI.UDropSelect>("udsLvUpItemType");
			this.ubtnGameSuccess = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnGameSuccess");
			this.ubtnPringIsGameHasAllSolve = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnPringIsGameHasAllSolve");
			this.goGroupGame = prefabBinder.GetObj<UnityEngine.GameObject>("goGroupGame");
			this.ubtnClearEquipInv = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnClearEquipInv");
			this.ubtnAddAfkOneHour = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnAddAfkOneHour");
			this.ubtnClearAllArchives = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnClearAllArchives");
			this.ubtnClearGuideArchives = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnClearGuideArchives");
			this.goGroupArchive = prefabBinder.GetObj<UnityEngine.GameObject>("goGroupArchive");
			this.ubtnSetFbPassLv = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnSetFbPassLv");
			this.uifFbLevelId = prefabBinder.GetObj<Framework.Runtime.UI.UInputField>("uifFbLevelId");
			this.uifFbId = prefabBinder.GetObj<Framework.Runtime.UI.UInputField>("uifFbId");
			this.transPassTargetFb = prefabBinder.GetObj<UnityEngine.RectTransform>("transPassTargetFb");
			this.ubtnClearFbArchives = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnClearFbArchives");
			this.ubtnClearFbTryCountArchives = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnClearFbTryCountArchives");
			this.goGroupFb = prefabBinder.GetObj<UnityEngine.GameObject>("goGroupFb");
			this.ubtnClearCurrencyInv = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnClearCurrencyInv");
			this.ubtnGetRdmEquip = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnGetRdmEquip");
			this.ubtnAddPower = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnAddPower");
			this.ubtnAddEquipUpItem = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnAddEquipUpItem");
			this.ubtnAddSpecTalentUpItem = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnAddSpecTalentUpItem");
			this.ubtnAddTalentUpItem = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnAddTalentUpItem");
			this.ubtnAddRoleStarUpItem = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnAddRoleStarUpItem");
			this.ubtnAddRoleLvUpItem = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnAddRoleLvUpItem");
			this.ubtnAddMoney = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnAddMoney");
			this.goGroupCurrency = prefabBinder.GetObj<UnityEngine.GameObject>("goGroupCurrency");
			this.goGroupTemplate = prefabBinder.GetObj<UnityEngine.GameObject>("goGroupTemplate");
			this.rtContent = prefabBinder.GetObj<UnityEngine.RectTransform>("rtContent");

		}
		public override int GetOpenLayer(int externalLayer)
		{
			return externalLayer;

		}
		public override string GetAssetLink(string outAssetLink)
		{
			if (UIRoot.IsPhoneUI())
			{
				string assetPath = "Assets/AddressableResources/UI/Manage/Prefabs/PhoneDebugGMView.prefab";
				return AssetPathEncoder.EncodeEnvAssetLink(assetPath, AssetType.PrefabAsset);
			}
			else
			{
				string assetPath = "Assets/AddressableResources/UI/Manage/Prefabs/PcDebugGMView.prefab";
				return AssetPathEncoder.EncodeEnvAssetLink(assetPath, AssetType.PrefabAsset);
			}


		}

		protected override void OnInitUI()
		{
			base.OnInitUI();
			ubtnPringIsGameHasAllSolve.AddClick(PrintIsGameHasAllSolveClick);
			ubtnGameSuccess.AddClick(GameSuccessClick);
			ubtnJumpLv.AddClick(OnJumpLevelClick);
			// Phone GM 复用现有输入控件：uifFbId 支持“道具id:数量”，数量输入为空时默认 1。
			ubtnAddMoney.AddClick(OnAddItemClick);
			if (UIRoot.IsPhoneUI()) ubtnAddMoney.Text = "增加道具 (ID:数量)";

			UIUtil.RefreshLayoutDelay(rtContent);
		}

		private void OnAddItemClick()
		{
			var raw = uifFbId == null ? string.Empty : uifFbId.text.Trim();
			var parts = raw.Split(':');
			int itemId, count;
			if (parts.Length == 2 && int.TryParse(parts[0], out itemId) && int.TryParse(parts[1], out count)) { }
			else if (!int.TryParse(raw, out itemId)) return;
			else if (uifFbLevelId == null || !int.TryParse(uifFbLevelId.text, out count)) count = 1;
			if (itemId <= 0 || count <= 0) return;
			var operation = GameInventoryDataHandler.Ins.StoreItem(itemId, count);
			Log.Info(operation.operateCount > 0 ? $"增加道具成功: {itemId} x{operation.operateCount}" : $"增加道具失败: {operation.errMessage}");
		}

		private void OnJumpLevelClick()
		{
			var levelId = int.Parse(uifJumpLevelId.text);
			LevelVO.Current?.LevelQuit();
			MessageDispatcher.Ins.Dispatch(MessageCode.msg_return_home);
			GameArchive.Main.LevelArchive.ClearLevelStatusMap();
			GameArrowsClientHandler.Ins.ReStartLevel(levelId);
			FoldDebuggerPanel();
		}

		private void GameSuccessClick()
		{
			MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_gameSuccess_direcly);
			FoldDebuggerPanel();
		}

		private void PrintIsGameHasAllSolveClick()
		{
			var hasSolveAll = LevelVO.Current.IsAllArrowCanRemove();
			Log.Info($"当前棋盘是否所有线条都可以移除【棋盘有解：？】 {hasSolveAll}");
			FoldDebuggerPanel();

		}
		private void FoldDebuggerPanel()
		{
			GameApp.Debugger.FoldDebuggerPanel();

		}

		private void AddInGameBgItem()
		{

		}
	}

}



































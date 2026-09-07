using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Framework.Runtime;
using Framework.Runtime.LogSystem;
using Framework.Runtime.UI;
using Framework.Utils;
using Game.Modules.GModuleAudio;
using Game.Modules.GModuleManage;
using Game.Modules.GModuleProgression;
using Game.Modules.GModuleSceneUnit;
using Game.Modules.GModuleStage;
using Game.Modules.GModuleTip;
using UnityEngine;
using static Game.Modules.GModuleArrows.ArrowLineSceneUnit;

namespace Game.Modules.GModuleArrows
{
    public enum LevelStatus
    {
        NoInited,
        Loading,
        Loaded,
        PrePlaying,
        Playing,
        Failed,
        SuccessAniming,
        Success,
    }
    public class LevelVO : BaseVO
    {
        public LevelInfo LevelInfo { get; private set; }

        private ArrowsGameStage m_GameStage;
        private Action<LevelStatus> m_OnStatusChange;
        private LevelStatus m_Status = LevelStatus.NoInited;
        public LevelPointLayout LevelPointLayout { get; private set; }
        public LevelArrowsBoard LevelArrowsBoard { get; private set; }
        public ArrowsGameStage GameStage => m_GameStage;
        private List<ArrowPointSceneUnit> m_AllPoints;
        private List<ArrowLineSceneUnit> m_AllArrows;
        // private List<LevelArrowNode> m_AllArrowNodes;
        private List<ArrowLineSceneUnit> m_NeedRePutArrows;
        private LevelArrowNode m_LastRemovedArrow;
        private List<LevelHeartVO> m_Hearts;
        private HashSet<ArrowLineSceneUnit> m_HeartSubArrows;
        public static LevelVO Current { get; private set; }
        // private int m_CurHeartNum;
        protected override void OnInit()
        {
            base.OnInit();
            m_AllPoints = new List<ArrowPointSceneUnit>();
            m_AllArrows = new List<ArrowLineSceneUnit>();
            // m_AllArrowNodes = new List<LevelArrowNode>();
            m_NeedRePutArrows = new List<ArrowLineSceneUnit>();
            m_HeartSubArrows = new HashSet<ArrowLineSceneUnit>();
        }
        public void SetStatusChange(Action<LevelStatus> onStatusChange)
        {
            m_OnStatusChange = onStatusChange;
            onStatusChange?.Invoke(m_Status);
        }
        public void RemoveStatusChange(Action<LevelStatus> onStatusChange)
        {
            m_OnStatusChange -= onStatusChange;
        }
        public bool IsStatus(LevelStatus status)
        {
            return m_Status == status;
        }
        public bool IsLoading()
        {
            return IsStatus(LevelStatus.Loading);
        }
        protected void SwitchStatus(LevelStatus status)
        {
            m_Status = status;
            m_OnStatusChange?.Invoke(m_Status);
        }
        private bool IsPointLayoutLoaded()
        {
            return LevelInfo.pointPresets != null && LevelInfo.pointPresets.pointLayoutName == LevelInfo.levelCfg.pointLayoutName;
        }
        private bool IsStageLoaded()
        {
            return m_GameStage != null && m_GameStage.IsLoaded(); ;
        }
        private bool IsArrowLayoutLoaded()
        {
            return LevelArrowsBoard != null && LevelInfo.arrowLayoutId == LevelArrowsBoard.ArrowLayoutId && LevelArrowsBoard.GetActivedArrows().Count > 0;
        }
        public bool HasAllLoaded()
        {
            if (!IsStageLoaded())
            {
                return false;
            }
            if (!IsPointLayoutLoaded())
            {
                return false;
            }
            if (!IsArrowLayoutLoaded())
            {
                return false;
            }
            return true; ;
        }
        private void CheckLeveLoad()
        {
            if (HasAllLoaded())
            {
                OnAllLoaded();
                return;
            }
            SwitchStatus(LevelStatus.Loading);
            m_OnPointLoadedCb = null;
            m_OnAllLoadedCb = null;
            LoadStage();
            LoadPointLayout();
            LoadArrowsLayout();
        }
        // private void OnAllLoaded()
        // {
        //     SwitchStatus(LevelStatus.Loaded);
        //     StartGame();
        // }
        public void OnLevelLoaded(Action onAllLoadedCb)
        {
            if (HasAllLoaded())
            {
                onAllLoadedCb?.Invoke();
                return;
            }
            m_OnAllLoadedCb += onAllLoadedCb;
        }
        private void CheckAllLoaded()
        {
            if (!IsLoading())
            {
                return;
            }
            if (!HasAllLoaded())
            {
                return;
            }
            OnAllLoaded();
        }
        public ArrowPointSceneUnit GetPointNear(Vector3 mouseWorldPos, float checkRadius)
        {
            float minDis = float.MaxValue;
            ArrowPointSceneUnit nearPoint = null;
            foreach (var nodeEntity in m_AllPoints)
            {
                float dis = Vector3.Distance(nodeEntity.PointNode.worldPosition, mouseWorldPos);
                if (dis <= minDis)
                {
                    nearPoint = nodeEntity;
                    minDis = dis;
                }
            }
            if (minDis <= checkRadius)
            {
                return nearPoint;
            }
            return null;
        }
        // public bool GeneratePointLayout(LevelPointPresets pointPresets, LevelArrowsPresure arrowsPresure)
        // {
        //     LevelPointLayout = new LevelPointLayout();
        //     LevelPointLayout.InitializeLayout(pointPresets, Vector3.zero);
        //     LevelArrowsBoard = new LevelArrowsBoard();
        //     bool isLayoutGenerateSuc = LevelArrowsBoard.SetupBoard(LevelPointLayout, arrowsPresure);
        //     return isLayoutGenerateSuc;
        // }
        private void OnAllLoaded()
        {
            SwitchStatus(LevelStatus.Loaded);
            m_OnAllLoadedCb?.Invoke();
            m_OnAllLoadedCb = null;
            StartGame();
        }
        private void LoadStage()
        {
            if (IsStageLoaded())
            {
                return;
            }
            if (!GameStageClientHandler.Ins.IsStageLoaded<ArrowsGameStage>())
            {
                m_GameStage = GameStageClientHandler.Ins.TryLoadStage<ArrowsGameStage>("ArrowsGame", "ArrowsGameStage", OnArrowGameStageLoaded);
            }
        }
        private void LoadPointLayout()
        {
            if (IsPointLayoutLoaded())
            {
                return;
            }
            var spaceVec = new Vector2(LevelInfo.levelCfg.pointSpaceX, LevelInfo.levelCfg.pointSpaceY);
            GameArrowsDataHandler.Ins.GetOrLoadPointsLayout(LevelInfo.levelCfg.pointLayoutName, spaceVec, OnLevelPointLayoutLoaded);
        }
        private void AwaitPointLayoutLoaded(Action onLoaded)
        {
            if (IsPointLayoutLoaded())
            {
                onLoaded?.Invoke();
                return;
            }
            m_OnPointLoadedCb += onLoaded;
        }
        private void LoadArrowsLayout()
        {
            LevelInfo.arrowsPresure = GameArrowsDataHandler.Ins.LoadLevelArrowsPresure(LevelInfo.levelCfg.arrowsGenerateArgId);
            if (IsArrowLayoutLoaded())
            {
                return;
            }
            LevelArrowsBoard = new LevelArrowsBoard();
            LevelArrowsBoard.SetArrowLayoutId(LevelInfo.arrowLayoutId);
            if (IsRecoverLevel())
            {
                string levelJson = GameArchive.Main.LevelArchive.GetCurLevelArrowJson();
                CfgArrowLayout arrowLayout = Utility.Json.ToObject<CfgArrowLayout>(levelJson);
                if (arrowLayout != null)
                {
                    Log.Info($"从存档中恢复数据! ");
                    OnLevelArrowLayoutLoaded(LevelInfo.levelCfg.arrowsLayoutName, arrowLayout);

                    return;
                }
            }
            if (LevelInfo.levelCfg.arrowsLayoutGenerateType == ARROW_GENERATE_BY_CONFIG)
            {
                // 加载线条配置
                GameArrowsDataHandler.Ins.GerOrLoadArrowsLayout(LevelInfo.levelCfg.arrowsLayoutName, OnLevelArrowLayoutLoaded);
            }
            else if (LevelInfo.levelCfg.arrowsLayoutGenerateType == ARROW_GENERATE_BY_SEED) // 不使用种子
            {
                LevelInfo.arrowsPresure.isUsingCustomSeed = false;
                AwaitPointLayoutLoaded(GenerateArrowsLayout);
            }
            else if (LevelInfo.levelCfg.arrowsLayoutGenerateType == ARROW_GENERATE_BY_FIXED_SEED) // 使用种子
            {
                LevelInfo.arrowsPresure.isUsingCustomSeed = true;
                LevelInfo.arrowsPresure.customSeed = LevelInfo.levelCfg.customSeed;
                // 使用种子动态生成
                AwaitPointLayoutLoaded(GenerateArrowsLayout);
            }

        }

        private void GenerateArrowsLayout()
        {
            bool isLayoutGenerateSuc = LevelArrowsBoard.SetupBoard(LevelPointLayout, LevelInfo.arrowsPresure);
            CheckAllLoaded();
        }
        private void OnLevelArrowLayoutLoaded(string arrowLayoutName, CfgArrowLayout arrowLayout)
        {
            if (LevelInfo.levelCfg.arrowsLayoutName != arrowLayoutName)
            {
                Log.Error($"当前加载的线条布局与配置不匹配! 当前配置的线条布局：{LevelInfo.levelCfg.arrowsLayoutName} 当前加载的线条布局：{arrowLayoutName}");
                return;
            }
            if (arrowLayout == null)
            {
                Log.Warning("当前关卡配置文件不存在，尝试根据参数动态生成");
                LevelInfo.levelCfg.arrowsLayoutGenerateType = ARROW_GENERATE_BY_SEED;
                LoadArrowsLayout();
                return;
            }


            LevelInfo.arrowsPresure = arrowLayout.presureArg;
            LevelArrowsBoard.SetupBoardArrows(arrowLayout.arrowNodes);
            CheckAllLoaded();
        }
        private void OnLevelPointLayoutLoaded(LevelPointPresets presets)
        {
            if (presets.pointLayoutName != LevelInfo.levelCfg.pointLayoutName)
            {
                Log.Error($"当前加载的点布局与配置不匹配! 当前配置的点布局：{LevelInfo.levelCfg.pointLayoutName} 当前加载的点布局：{presets.pointLayoutName}");
                return;
            }
            LevelInfo.pointPresets = presets;
            LevelPointLayout = new LevelPointLayout();
            LevelPointLayout.InitializeLayout(LevelInfo.pointPresets, Vector3.zero);
            m_OnPointLoadedCb?.Invoke();
            CheckAllLoaded();
        }

        private void OnArrowGameStageLoaded(ArrowsGameStage stage)
        {
            // 初始化对象池
            GameSceneUnitClientHandler.Ins.GameSceneUnitPool.BindSceneUnitRootPrefab(GameArrowsConstant.ArrowPointSceneUnitId, stage.arrowPointSceneUnitPrefab.gameObject);
            GameSceneUnitClientHandler.Ins.GameSceneUnitPool.BindSceneUnitRootPrefab(GameArrowsConstant.ArrowLineSceneUnitId, stage.arrowLineSceneUnitPrefab.gameObject);
            GameSceneUnitClientHandler.Ins.GameSceneUnitPool.PrewarmSceneUnit<ArrowPointSceneUnit>(GameArrowsConstant.ArrowPointSceneUnitId, 100, true);
            GameSceneUnitClientHandler.Ins.GameSceneUnitPool.PrewarmSceneUnit<ArrowLineSceneUnit>(GameArrowsConstant.ArrowLineSceneUnitId, 100, true);
            CheckAllLoaded();
        }

        public async UniTask ShowEnityAndPlayAnimAsync()
        {
            SyncPointOccupied();
            GameStage.ResetStage();

            ReleaseAllShowedPoints();
            RelaseAllShowedLines();

            await ShowAllPoints();
            await ShowAllLines();


        }
        protected virtual void OnGamePlayPreparedOver()
        {
            SwitchStatus(LevelStatus.Playing);
            GameArchive.Main.LevelArchive.ClearStatusType(LevelArchiveStatus.Status_Gaming);
            GameArchive.Main.LevelArchive.SetLevelStatus(LevelInfo.levelId, LevelArchiveStatus.Status_Gaming);

            MessageDispatcher.Ins.Dispatch(MessageCode.msg_open_gameplay_panel);
            GameStage.PlayStage();
        }
        private void SyncPointOccupied()
        {

        }
        private void ReleaseAllShowedPoints()
        {
            foreach (var point in m_AllPoints)
            {
                GameSceneUnitClientHandler.Ins.GameSceneUnitPool.PutSceneUnit(GameArrowsConstant.ArrowPointSceneUnitId, point);
            }
            m_AllPoints.Clear();
        }

        private int m_PointExitAnimOverNum;
        private async UniTask PlayAllPointExitAnim()
        {
            m_PointExitAnimOverNum = 0;
            foreach (var point in m_AllPoints)
            {
                if (point == null || point.IsInPool) continue;
                point.PlayExitAnim(OnPointExitAnimOver);

            }
            await UniTask.WaitUntil(IsAllPointExitOver);
        }
        private void OnPointExitAnimOver(ArrowPointSceneUnit unit)
        {
            m_PointExitAnimOverNum++;
        }
        private bool IsAllPointExitOver()
        {
            return m_PointExitAnimOverNum == m_AllPoints.Count;
        }
        private int m_PointEntryAnimOverNum;
        private async UniTask ShowAllPoints()
        {
            m_PointEntryAnimOverNum = 0;

            foreach (var node in LevelPointLayout.GetAllNodes())
            {
                if (LevelInfo.levelCfg.isHideUnOccupiedPoint && !node.isOccupied) continue;
                ArrowPointSceneUnit pointSceneUnit = GameSceneUnitClientHandler.Ins.GameSceneUnitPool.GetSceneUnit<ArrowPointSceneUnit>(GameArrowsConstant.ArrowPointSceneUnitId, true);
                pointSceneUnit.RootTransform.gameObject.name = "Point_" + node.id;
                pointSceneUnit.SetRootParent(GameStage.transPointsRoot);
                pointSceneUnit.SetPosition(node.worldPosition);
                // pointSceneUnit.SetPointClick(CheckPointTrigger);
                pointSceneUnit.SetPointNodeData(node);
                pointSceneUnit.PlayEntryAnim(OnPointEntryAnimOver);
                m_AllPoints.Add(pointSceneUnit);
            }
            await UniTask.WaitUntil(IsAllPointEntryOver);

        }
        private bool IsAllPointEntryOver()
        {
            return m_PointEntryAnimOverNum == m_AllPoints.Count;
        }

        private void OnPointEntryAnimOver(ArrowPointSceneUnit unit)
        {
            m_PointEntryAnimOverNum++;
        }

        private Vector3 GetMousePosToWorldPosDir(Vector3 worldPosition) //  这里返回 鼠标点击位置的世界坐标 -  当前世界坐标的向量方向
        {
            Vector3 mouseWorldPos = GameStage.gameCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
            worldPosition.z = 0f;
            return (mouseWorldPos - worldPosition).normalized;
        }
        private ArrowLineSceneUnit GetNearExistArrow(Vector3 worldPosition)
        {
            Vector3 clickDirection = GetMousePosToWorldPosDir(worldPosition);
            if (clickDirection.sqrMagnitude < 0.0001f) return null;

            // 1. 根据鼠标相对点击点的偏移，确定四个主方向的检查优先级
            List<Vector2Int> searchDirections = new List<Vector2Int>();

            float absX = Mathf.Abs(clickDirection.x);
            float absY = Mathf.Abs(clickDirection.y);

            if (absX > absY)
            {
                // 横向偏移更大，优先检查左右
                searchDirections.Add(clickDirection.x > 0 ? Vector2Int.right : Vector2Int.left);
                searchDirections.Add(clickDirection.y > 0 ? Vector2Int.up : Vector2Int.down);
                searchDirections.Add(clickDirection.y > 0 ? Vector2Int.down : Vector2Int.up);
                searchDirections.Add(clickDirection.x > 0 ? Vector2Int.left : Vector2Int.right);
            }
            else
            {
                // 纵向偏移更大，优先检查上下
                searchDirections.Add(clickDirection.y > 0 ? Vector2Int.up : Vector2Int.down);
                searchDirections.Add(clickDirection.x > 0 ? Vector2Int.right : Vector2Int.left);
                searchDirections.Add(clickDirection.x > 0 ? Vector2Int.left : Vector2Int.right);
                searchDirections.Add(clickDirection.y > 0 ? Vector2Int.down : Vector2Int.up);
            }

            // 2. 找到当前点击点的网格索引
            Vector3Int centerGridIndex = Vector3Int.zero;
            bool foundCenterNode = false;
            foreach (var node in LevelPointLayout.GetAllNodes())
            {
                if (Vector3.Distance(node.worldPosition, worldPosition) < 0.01f)
                {
                    centerGridIndex = node.index;
                    foundCenterNode = true;
                    break;
                }
            }

            if (!foundCenterNode) return null;

            // 3. 按照优先级方向依次探测邻近网格点，并检查是否有线段占用
            foreach (var dir in searchDirections)
            {
                Vector3Int neighborIndex = centerGridIndex + new Vector3Int(dir.x, dir.y, 0);

                // 确保邻近点在布局边界内
                if (neighborIndex.x >= LevelPointLayout.MinX && neighborIndex.x <= LevelPointLayout.MaxX &&
                    neighborIndex.y >= LevelPointLayout.MinY && neighborIndex.y <= LevelPointLayout.MaxY)
                {
                    // 检查该邻近网格坐标是否生成了点
                    var neighborNode = LevelPointLayout.GetNodeByIndex(neighborIndex);
                    // 注意：如果您的 LevelPointLayout 没有直接根据 index 拿 node 的方法，可以通过遍历或者上面的 TryDetectPointOccupied 逻辑定位

                    if (TryDetectPointOccupied(neighborIndex, out ArrowLineSceneUnit occArrow, out ArrowPointSceneUnit occPoint))
                    {
                        if (occArrow != null)
                        {
                            return occArrow; // 找到最近的被占用线段，直接返回
                        }
                    }
                }
            }

            return null;
        }
        public LevelPointNode GetTipPoint()
        {
            foreach (var node in LevelPointLayout.GetAllNodes())
            {
                var targetArrow = GetPointOccupiedLine(node.index);
                if (targetArrow != null && !GetArrowOccMoveInDirection(targetArrow.ArrowNode, out ArrowLineSceneUnit occArrow, out ArrowPointSceneUnit occPoint))
                {
                    return node;
                }
            }
            return null;
        }
        public bool TryUseHintProp()
        {
            var point = GetTipPoint();
            var scenePoint = point == null ? null : GetPointSceneUnitById(point.id);
            if (scenePoint == null) return false;
            var screenPosition = GameStage.gameCamera.WorldToScreenPoint(scenePoint.RootTransform.position);
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_play_arrow_click_point_tip, new Game.Modules.ArrowClickPointTipWindowData
            {
                showTipScreenPos = screenPosition,
                innerScreeRadius = 28f,
                outerScreeRadius = 80f
            });
            return true;
        }
        public bool TryUseClearProp()
        {
            var point = GetTipPoint();
            if (point == null) return false;
            CheckPointTrigger(GetPointSceneUnitById(point.id));
            return true;
        }
        public bool TryUseUndoProp()
        {
            if (m_LastRemovedArrow == null || m_LastRemovedArrow.IsEnable()) return false;
            foreach (var pending in m_NeedRePutArrows.ToArray())
            {
                if (pending.ArrowNode != m_LastRemovedArrow) continue;
                m_NeedRePutArrows.Remove(pending);
                GameSceneUnitClientHandler.Ins.GameSceneUnitPool.PutSceneUnit(GameArrowsConstant.ArrowLineSceneUnitId, pending);
            }
            m_LastRemovedArrow.SetStatus(LevelArrowStatus.Status_Enable);
            foreach (var pointIndex in m_LastRemovedArrow.occupiedPointIndexs) LevelPointLayout.SetNodesOccupyRemoved(pointIndex, false);
            var arrow = GameSceneUnitClientHandler.Ins.GameSceneUnitPool.GetSceneUnit<ArrowLineSceneUnit>(GameArrowsConstant.ArrowLineSceneUnitId, true);
            arrow.RootTransform.gameObject.name = "Arrow" + m_LastRemovedArrow.Id;
            arrow.SetRootParent(GameStage.transArrowsRoot);
            arrow.SetArrowData(m_LastRemovedArrow);
            m_AllArrows.Add(arrow);
            m_LastRemovedArrow = null;
            SaveArrowsArchive();
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_arrowLineChanged);
            return true;
        }
        public bool IsAllArrowCanRemove()
        {
            foreach (var arrow in m_AllArrows)
            {
                if (!arrow.ArrowNode.isHasSolveDeep)
                {
                    return false;
                }
            }
            return true;
        }
        public void CheckPointTriggerByMouse(float checkRadius)
        {
            Vector3 mouseWorldPos = GameStage.gameCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
            var point = GetPointNear(mouseWorldPos, checkRadius);
            CheckPointTrigger(point);
        }
        public bool IsPointCanTriggerArrow(ArrowPointSceneUnit hitPoint)
        {
            if (hitPoint == null)
            {
                return false;
            }
            var nodeIndex = hitPoint.PointNode.index;
            var targetArrow = GetPointOccupiedLine(nodeIndex);
            if (targetArrow == null)
            {
                var nearArrow = GetNearExistArrow(hitPoint.PointNode.worldPosition);
                if (nearArrow == null)
                {
                    return false;
                }
                else
                {
                    targetArrow = nearArrow;
                }
            }
            return IsArrowHasOccInMoveDir(targetArrow.ArrowNode);
        }
        public void CheckPointTrigger(ArrowPointSceneUnit hitPoint)
        {
            if (hitPoint == null)
            {
                return;
            }
            var nodeIndex = hitPoint.PointNode.index;
            var targetArrow = GetPointOccupiedLine(nodeIndex);
            if (targetArrow == null)
            {
                Log.Info("点未被占用");
                var nearArrow = GetNearExistArrow(hitPoint.PointNode.worldPosition);
                if (nearArrow == null)
                {
                    Log.Info("周围也没有线段占用");
                    return;
                }
                else
                {
                    targetArrow = nearArrow;
                }
            }
            TriggerArrow(targetArrow);
        }
        public bool IsGameAlive()
        {
            return GetHearNum() > 0;
        }
        private bool CheckIsGameSuccess()
        {
            return m_AllArrows.Count <= 0;
        }
        public bool IsNotHasHeart()
        {
            return GetHearNum() <= 0;
        }
        public int GetHearNum()
        {
            return GameArchive.Main.LevelArchive.GetCurLevelHeartNum();
        }

        private void SubGameHeart(ArrowLineSceneUnit targetArrow)
        {
            if (LevelInfo.isInfHeart) return;
            if (GetHearNum() <= 0) return;
            if (m_HeartSubArrows.Contains(targetArrow)) return;
            int curHeartNum = GetHearNum();
            var heartInfo = m_Hearts[curHeartNum - 1];
            curHeartNum--;
            curHeartNum = Mathf.Max(curHeartNum, 0);
            GameArchive.Main.LevelArchive.SetCurLevelHeartNum(curHeartNum);
            // 设置状态
            heartInfo.SetDead(true);
            m_HeartSubArrows.Add(targetArrow);
        }
        private bool ReVivalOnHeart()
        {
            int curHeartNum = GetHearNum();
            if (curHeartNum >= LevelInfo.heartNum)
            {
                Log.Info("当前生命值已满，无需复活！");
                return false;
            }
            var heartInfo = m_Hearts[curHeartNum];
            heartInfo.SetAlive();
            curHeartNum++;
            GameArchive.Main.LevelArchive.SetCurLevelHeartNum(curHeartNum);
            return true;

        }
        private void GameSuccessDirectly()
        {
            RelaseAllShowedLines();
            GameSuccess().Forget();
        }
        private async UniTask GameSuccess()
        {
            SwitchStatus(LevelStatus.SuccessAniming);
            GameSuccessRecord();
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_close_gameplay_panel);
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_gameSuccess);
            GameStage.DoGameSuccessAnim();
            await PlayGameSuccessAnim();
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_open_gameSuccess_panel);
            SwitchStatus(LevelStatus.Success);
            ClearScene();
        }
        private void GameSuccessRecord()
        {
            GameArchive.Main.LevelArchive.PassCurLevel();
            GameProgressionService.GrantLevelReward(LevelInfo.levelId);
        }
        private async UniTask PlayGameSuccessAnim()
        {

            await PlayAllPointExitAnim();
        }



        private void GameOver()
        {
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_open_gameOver_panel);
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_gameOver);
            SwitchStatus(LevelStatus.Failed);
        }

        private void TriggerArrow(ArrowLineSceneUnit targetArrow)
        {
            if (targetArrow == null) return;
            // 判断该线移动方向上是否有其他点占用
            if (GetArrowOccMoveInDirection(targetArrow.ArrowNode, out ArrowLineSceneUnit occArrow, out ArrowPointSceneUnit occPoint))
            {
                Log.Info($"改{targetArrow.ArrowNode.Id}线段无法移除，前面有线条占用{occArrow.ArrowNode.Id}");
                if (IsGameAlive())
                {
                    GameFeedbackService.OnWrongAction();
                    SubGameHeart(targetArrow);
                    Vector3 offset = -targetArrow.ArrowNode.MoveDirection * 0f;
                    GameAudioClientHandler.Ins.PlayEffect(GameAudioConstant.Eff_ErrorAns);
                    targetArrow.PlayMoveCollisionAnim(occPoint.PointNode.worldPosition + offset, () =>
                    {
                        occArrow.PlayFade(ArrowFadeType.BodySync, Color.red, 0.1f, 0.1f);
                        targetArrow.PlayFade(ArrowFadeType.FromHead, Color.red, 0.1f, -1f);
                    });
                    if (IsNotHasHeart())
                    {
                        //  游戏结束
                        GameOver();

                    }
                }
            }
            else
            {
                Log.Info($"移除线段{targetArrow.ArrowNode.Id}");
                RemoveAndPlayArrowLine(targetArrow);
                targetArrow.PlayFade(ArrowFadeType.FromHead, UIUtil.Hex2Color("11659a"), 0.15f, -1);
                GameAudioClientHandler.Ins.PlayEffect(GameAudioConstant.Eff_ArrowMove);
                if (CheckIsGameSuccess())
                {
                    // 游戏成功
                    GameSuccess().Forget();
                }
            }
        }
        private void RemoveAndPlayArrowLine(ArrowLineSceneUnit arrowLineSceneUnit)
        {
            m_LastRemovedArrow = arrowLineSceneUnit.ArrowNode;
            m_AllArrows.Remove(arrowLineSceneUnit);
            arrowLineSceneUnit.ArrowNode.SetStatus(LevelArrowStatus.Status_Disable);
            // m_AllArrowNodes.Remove(arrowLineSceneUnit.ArrowNode);
            m_NeedRePutArrows.Add(arrowLineSceneUnit);
            LevelPointLayout.SetArrowOccupyRemoved(arrowLineSceneUnit.ArrowNode.occupiedPointIndexs);
            arrowLineSceneUnit.PlayLevelAnim(() =>
            {
                CheckArrowPut(arrowLineSceneUnit);
            });
            SaveArrowsArchive();
            PlayArrowExitPathPointsAnim(arrowLineSceneUnit);
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_arrowLineChanged);

        }
        public void SaveToArchive()
        {

        }
        public void SaveArrowsArchive()
        {
            var cfg = ToRecordArrowLayout();
            string levelJson = Utility.Json.ToJson(cfg);
            GameArchive.Main.LevelArchive.SetCurLevelArrowJson(levelJson);
        }



        private List<Vector3Int> GetArrowMovePathPoints(ArrowLineSceneUnit arrowLineSceneUnit)
        {

            List<Vector3Int> pointIds = new List<Vector3Int>(arrowLineSceneUnit.ArrowNode.occupiedPointIndexs);
            var pathPoints = arrowLineSceneUnit.ArrowNode.PathPoints;
            var endPointIndex = pointIds[pointIds.Count - 1];
            var endPoint = LevelPointLayout.GetNodeByIndex(endPointIndex);
            var headIndex = endPoint.index;
            Vector3Int nextIndex = headIndex + Vector3Int.RoundToInt(arrowLineSceneUnit.ArrowNode.MoveDirection);
            int minX = LevelPointLayout.MinX;
            int maxX = LevelPointLayout.MaxX;
            int minY = LevelPointLayout.MinY;
            int maxY = LevelPointLayout.MaxY;
            while (nextIndex.x >= minX && nextIndex.x <= maxX && nextIndex.y >= minY && nextIndex.y <= maxY)
            {
                var pointNode = LevelPointLayout.GetNodeByIndex(nextIndex);
                if (pointNode != null)
                {
                    pointIds.Add(pointNode.index);
                }
                nextIndex = nextIndex + Vector3Int.RoundToInt(arrowLineSceneUnit.ArrowNode.MoveDirection);
            }
            return pointIds;
        }
        private void PlayArrowExitPathPointsAnim(ArrowLineSceneUnit arrowLineSceneUnit)
        {
            var pointIndexs = GetArrowMovePathPoints(arrowLineSceneUnit);
            for (int i = 0; i < pointIndexs.Count; i++)
            {
                var pointIndex = pointIndexs[i];
                var point = GetPointSceneUnitByIndex(pointIndex);
                point?.UpdateSize();
                point?.PlayArrowExitPathThroughAnim(i);
            }

        }
        private void CheckArrowPut(ArrowLineSceneUnit arrowLineSceneUnit)
        {
            if (m_NeedRePutArrows.Contains(arrowLineSceneUnit))
            {
                m_NeedRePutArrows.Remove(arrowLineSceneUnit);
                GameSceneUnitClientHandler.Ins.GameSceneUnitPool.PutSceneUnit(GameArrowsConstant.ArrowLineSceneUnitId, arrowLineSceneUnit);
            }
        }
        public ArrowPointSceneUnit GetPointSceneUnitById(int pointId)
        {
            foreach (var point in m_AllPoints)
            {
                if (point.PointNode.id == pointId)
                {
                    return point;
                }
            }
            return null;
        }
        public ArrowPointSceneUnit GetPointSceneUnitByIndex(Vector3Int index)
        {
            foreach (var point in m_AllPoints)
            {
                if (point.PointNode.index == index)
                {
                    return point;
                }
            }
            return null;
        }
        public bool IsArrowHasOccInMoveDir(LevelArrowNode arrowNode)
        {
            return GetArrowOccMoveInDirection(arrowNode, out ArrowLineSceneUnit occArrow, out ArrowPointSceneUnit occPoint);
        }
        private bool GetArrowOccMoveInDirection(LevelArrowNode arrowNode, out ArrowLineSceneUnit occArrow, out ArrowPointSceneUnit occPoint)
        {
            var endPointIndex = arrowNode.occupiedPointIndexs[arrowNode.occupiedPointIndexs.Count - 1];
            var endPointNode = LevelPointLayout.GetNodeByIndex(endPointIndex);
            occArrow = null;
            occPoint = null;
            if (endPointNode == null)
            {
                Log.Error($"点{endPointIndex}不存在");
                return false;
            }
            Vector3Int step = Vector3Int.RoundToInt(arrowNode.MoveDirection);

            Vector3Int currentIdx = endPointNode.index + step;
            while (currentIdx.x >= LevelPointLayout.MinX && currentIdx.x <= LevelPointLayout.MaxX &&
                   currentIdx.y >= LevelPointLayout.MinY && currentIdx.y <= LevelPointLayout.MaxY)
            {
                if (TryDetectPointOccupied(currentIdx, out occArrow, out occPoint))
                {
                    return true;
                }

                currentIdx += step;
            }

            return false;

        }

        private void RelaseAllShowedLines()
        {
            foreach (var arrow in m_AllArrows)
            {
                GameSceneUnitClientHandler.Ins.GameSceneUnitPool.PutSceneUnit(GameArrowsConstant.ArrowLineSceneUnitId, arrow);
            }
            m_AllArrows.Clear();
            // m_AllArrowNodes.Clear();
            ClearAllUnPutLines();
        }
        public bool IsPointOccupiedLine(Vector3Int nodeIndex)
        {
            return GetPointOccupiedLine(nodeIndex) != null;
        }

        public bool TryDetectPointOccupied(Vector3Int nodeIndex, out ArrowLineSceneUnit occArrow, out ArrowPointSceneUnit occPoint)
        {
            occArrow = null;
            occPoint = null;
            foreach (var arrow in m_AllArrows)
            {
                foreach (var pointIndex in arrow.ArrowNode.occupiedPointIndexs)
                {
                    var pointNode = LevelPointLayout.GetNodeByIndex(pointIndex);
                    if (pointNode.index == nodeIndex)
                    {
                        occArrow = arrow;
                        occPoint = GetPointSceneUnitById(pointNode.id);
                        return true;
                    }
                }
            }
            return false;
        }

        public ArrowLineSceneUnit GetPointOccupiedLine(Vector3Int nodeIndex)
        {
            foreach (var arrow in m_AllArrows)
            {
                if (arrow.ArrowNode.occupiedPointIndexs.Contains(nodeIndex))
                {
                    return arrow;
                }
            }
            return null;
        }
        private int m_LineEntryAnimOverNum;
        private Action m_OnPointLoadedCb = null;
        private Action m_OnAllLoadedCb = null;

        private async UniTask ShowAllLines()
        {
            m_LineEntryAnimOverNum = 0;
            var lines = LevelArrowsBoard.GetActivedArrows();
            foreach (var arrow in lines)
            {
                ArrowLineSceneUnit arrowSceneUnit = GameSceneUnitClientHandler.Ins.GameSceneUnitPool.GetSceneUnit<ArrowLineSceneUnit>(GameArrowsConstant.ArrowLineSceneUnitId, true);
                arrowSceneUnit.RootTransform.gameObject.name = "Arrow" + arrow.Id;
                arrowSceneUnit.SetRootParent(GameStage.transArrowsRoot);
                arrowSceneUnit.SetArrowData(arrow);
                arrowSceneUnit.PlayEntryAnim(OnLineAnimOver);
                m_AllArrows.Add(arrowSceneUnit);
                // m_AllArrowNodes.Add(arrow);
            }
            SaveArrowsArchive();
            await UniTask.WaitUntil(IsAllLineAnimOver);
        }
        private bool IsAllLineAnimOver()
        {
            return m_LineEntryAnimOverNum == m_AllArrows.Count;
        }

        private void OnLineAnimOver(ArrowLineSceneUnit unit)
        {
            m_LineEntryAnimOverNum++;
        }

        public void SetAsCurrent()
        {
            Current = this;
        }
        private void ClearScene()
        {
            LevelArrowsBoard?.ClearBoard();
            ReleaseAllShowedPoints();
            RelaseAllShowedLines();
            GameStageClientHandler.Ins.HideStage<ArrowsGameStage>();

        }
        private void ClearAllUnPutLines()
        {
            foreach (var arrow in m_NeedRePutArrows)
            {
                GameSceneUnitClientHandler.Ins.GameSceneUnitPool.PutSceneUnit(GameArrowsConstant.ArrowLineSceneUnitId, arrow);
            }
            m_NeedRePutArrows.Clear();
        }
        private void StartGame()
        {
            GameStageClientHandler.Ins.ShowStage<ArrowsGameStage>();
            // GameAudioClientHandler.Ins.EnablePlayTouchSound(GameAudioConstant.Eff_ScreenClick);
            // GenerateArrowsBoard();
            ArrowsBoardSetup();
            GameLevelPlayPrepare().Forget();
        }

        private void ArrowsBoardSetup()
        {
            LevelArrowsBoard.DoBoardValidate(LevelPointLayout);
        }
        public void ReStartLevel()
        {
            ReStartLevel(LevelInfo.levelId);
        }
        public void ReStartLevel(int levelId)
        {
            GameArchive.Main.LevelArchive.ClearLevelStatus(levelId);
            StartLevel(levelId);
        }
        public void StartLevel(int levelId)
        {
            if (IsLoading())
            {
                GameTip.Ins.TipCommonMsg("场景加载中，请稍后再试");
                return;
            }
            GameLoading.Ins.OpenLoading(new GameArrowsLoadingOption()
            {
                loadingPanelType = typeof(GameArrowsLoadingPanel),
                tipText = "正在全力加载中...",
                minValue = 0,
                maxValue = 100,
                targetValue = 100,
                timer = 1f,
                minDisplayTime = 0.2f
            });
            PanelManager.Ins.CloseAllUIPanel();
            PanelManager.Ins.CloseAllHighUIPanel();
            ClearScene();
            if (LevelInfo == null || LevelInfo.levelId != levelId)
            {
                LevelInfo = new LevelInfo();
                LevelInfo.levelId = levelId;
                LevelInfo.levelCfg = GameArrowsDataHandler.Ins.GetLevelConfig(LevelInfo.levelId);
                LevelInfo.LevelAnimArgs = GameArrowsDataHandler.Ins.GetLevelAnimArgs(LevelInfo.levelId, true, !GameArrowsClientHandler.Ins.IsQuickAnimModel());
            }
            MessageDispatcher.Ins.Subscribe(MessageCode.msg_on_gameSuccess_direcly, GameSuccessDirectly);
            InitLevelInfoArrowLayoutId();
            CheckLeveLoad();

        }
        private void InitLevelInfoArrowLayoutId()
        {
            int arrowLayoutGenerateType = LevelInfo.levelCfg.arrowsLayoutGenerateType;
            if (arrowLayoutGenerateType == ARROW_GENERATE_BY_CONFIG) // 使用布局生成
            {
                LevelInfo.arrowLayoutId = LevelInfo.levelCfg.arrowsLayoutName;
            }
            else if (arrowLayoutGenerateType == ARROW_GENERATE_BY_SEED) //不使用种子并配合参数生成
            {
                LevelInfo.arrowLayoutId = $"Level_{LevelInfo.levelId}_{Utility.IDGenerator.GetStrGuidID()}";
            }
            else if (arrowLayoutGenerateType == ARROW_GENERATE_BY_FIXED_SEED) // 使用种子并配合参数生成
            {
                LevelInfo.arrowLayoutId = $"Level_{LevelInfo.levelId}_{LevelInfo.levelCfg.customSeed}";
            }
        }
        public void LevelQuit()
        {
            MessageDispatcher.Ins.Unsubscribe(MessageCode.msg_on_gameSuccess_direcly, GameSuccessDirectly);
            ClearScene();
        }
        public bool IsRecoverLevel()
        {
            return GameArchive.Main.LevelArchive.IsGamingLevel(LevelInfo.levelId);
        }
        private void InitHearts()
        {
            m_Hearts = new List<LevelHeartVO>();
            bool isRecoverLevel = IsRecoverLevel();
            if (!isRecoverLevel)
            {
                GameArchive.Main.LevelArchive.SetCurLevelHeartNum(LevelInfo.heartNum);
            }
            int heartNum = GameArchive.Main.LevelArchive.GetCurLevelHeartNum();

            for (int i = 0; i < LevelInfo.heartNum; i++)
            {
                var heartVO = new LevelHeartVO();
                m_Hearts.Add(heartVO);
                heartVO.order = i;
                if (i < heartNum)
                {
                    heartVO.SetAlive(true);
                }
                else
                {
                    heartVO.SetDead(false);
                }
            }
            // m_CurHeartNum = LevelInfo.heartNum;
            // GameArchive.Main.LevelArchive.SetCurLevelHeartNum(LevelInfo.heartNum);
        }
        public void ReStartGame()
        {
            if (!IsPlaying() && !IsFailed()) return;
            // if (LevelInfo.levelCfg.arrowsLayoutGenerateType != ARROW_GENERATE_BY_CONFIG)
            // {
            //     LevelInfo.arrowsPresure.isUsingCustomSeed = true;
            //     LevelInfo.arrowsPresure.customSeed = LevelInfo.arrowsPresure.runtimeSeed;
            // }
            GameArchive.Main.LevelArchive.SetCurLevelHeartNum(LevelInfo.heartNum);
            LevelArrowsBoard.ReEnableAllArrows();


            // GenerateArrowsBoard();
            ArrowsBoardSetup();
            GameLevelPlayPrepare().Forget();
        }
        public const int ARROW_GENERATE_BY_CONFIG = 0;
        public const int ARROW_GENERATE_BY_SEED = 1;
        public const int ARROW_GENERATE_BY_FIXED_SEED = 2;
        private void GameLevelDataReset()
        {
            m_HeartSubArrows.Clear();

        }
        private async UniTask GameLevelPlayPrepare()
        {
            SwitchStatus(LevelStatus.PrePlaying);

            MessageDispatcher.Ins.Dispatch(MessageCode.msg_close_gameplay_panel);
            GameLoading.Ins.CloseLoading();
            GameLevelDataReset();
            InitHearts();
            if (!GameAudioClientHandler.Ins.IsBgmPlay(GameAudioConstant.Bgm_GamePlay_1))
            {
                GameAudioClientHandler.Ins.PlayBgm(GameAudioConstant.Bgm_GamePlay_1);
            }
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_game_start);
            await ShowEnityAndPlayAnimAsync();


            OnGamePlayPreparedOver();

        }
        // private bool GenerateArrowsBoard()
        // {
        //     bool isLayoutSuc = GeneratePointLayout(LevelInfo.pointPresets, LevelInfo.arrowsPresure);
        //     if (isLayoutSuc)
        //     {
        //         Log.Info("生成布局成功");
        //     }
        //     else
        //     {
        //         Log.Error("生成布局失败");
        //     }

        //     return isLayoutSuc;
        // }

        public List<LevelHeartVO> GetLevelHeartInfoList()
        {
            return m_Hearts;
        }
        public bool IsFailed()
        {
            return IsStatus(LevelStatus.Failed);
        }
        public bool IsPlaying()
        {
            return IsStatus(LevelStatus.Playing);
        }
        public void OnTimeExpired()
        {
            if (IsPlaying()) GameOver();
        }

        public int GetTotalArrowLineNum()
        {
            return LevelArrowsBoard.GetActivedArrows().Count;
        }
        public int GetRemainArrowLineNum()
        {
            return m_AllArrows.Count;
        }

        public void TryRevivalGame(Action onRevivalSuccess = null, Action onRevivalFail = null)
        {
            if (GameEnv.IsInDevlopMode())
            {
                if (ReVivalOnHeart())
                {
                    OnGameRevival(onRevivalSuccess);
                }
                else
                {
                    OnGameRevivalFail(onRevivalFail);
                }
                return;
            }
            // TODO : 处理正式版的复活逻辑
        }
        private void OnGameRevival(Action onRevivalSuccess = null)
        {
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_gameRevival);
            SwitchStatus(LevelStatus.Playing);
            onRevivalSuccess?.Invoke();
        }
        private void OnGameRevivalFail(Action onRevivalFail = null)
        {
            onRevivalFail?.Invoke();
        }
        public CfgArrowLayout ToCfgArrowLayout()
        {
            CfgArrowLayout cfgArrowLayout = new CfgArrowLayout();
            cfgArrowLayout.presureArg = LevelInfo.arrowsPresure;
            cfgArrowLayout.arrowNodes = LevelArrowsBoard.GetActivedArrows();
            return cfgArrowLayout;
        }
        public CfgArrowLayout ToRecordArrowLayout()
        {
            CfgArrowLayout cfgArrowLayout = new CfgArrowLayout();
            cfgArrowLayout.presureArg = LevelInfo.arrowsPresure;
            cfgArrowLayout.arrowNodes = LevelArrowsBoard.GetAllArrows();
            return cfgArrowLayout;
        }
    }
}

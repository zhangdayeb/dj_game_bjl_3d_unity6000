// Assets/Game/Controllers/GameController.cs
// 主游戏控制器 - 完全修复版，移除无意义转换方法，统一使用标准数据类型
// 负责游戏状态管理、事件分发、业务逻辑协调
// 修复时间: 2025/6/23

using System;
using System.Collections;
using UnityEngine;
using BaccaratGame.Core.Events;
using BaccaratGame.Core.Architecture;
using BaccaratGame.Game.Managers;
using BaccaratGame.Data;  // 统一引用标准数据类型

namespace BaccaratGame.Game.Controllers
{
    /// <summary>
    /// 主游戏控制器
    /// 作为事件协调中心，管理游戏流程和各模块间的协调
    /// </summary>
    public class GameController : MonoBehaviour
    {
        #region 配置

        [Header("游戏配置")]
        [SerializeField] private float _bettingPhaseMinDuration = 15f;
        [SerializeField] private float _bettingPhaseMaxDuration = 30f;
        [SerializeField] private float _dealingPhaseDuration = 10f;
        [SerializeField] private float _resultDisplayDuration = 5f;
        
        [Header("调试设置")]
        [SerializeField] private bool _enableDebugLogs = true;

        #endregion

        #region 私有字段

        // 核心组件引用 - 移除了BaccaratGameStateManager依赖
        private BaccaratDataStore _dataStore;
        private BettingManager _bettingManager;
        private ChipManager _chipManager;

        // 当前游戏状态
        private GameState _currentGameState = GameState.Initializing;
        private RoundState _currentRoundState = RoundState.Idle;
        private string _currentRoundId;
        private int _currentRoundNumber = 0;

        // 计时器
        private Coroutine _bettingPhaseCoroutine;
        private Coroutine _dealingPhaseCoroutine;
        private Coroutine _resultPhaseCoroutine;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            SubscribeToEvents();
            InitializeGame();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region 初始化

        private void InitializeComponents()
        {
            // 获取核心组件引用 - 使用新的API，移除BaccaratGameStateManager
            _dataStore = FindFirstObjectByType<BaccaratDataStore>();
            _bettingManager = FindFirstObjectByType<BettingManager>();
            _chipManager = FindFirstObjectByType<ChipManager>();

            if (_dataStore == null)
                Debug.LogError("GameController: BaccaratDataStore not found!");
            
            if (_bettingManager == null)
                Debug.LogError("GameController: BettingManager not found!");
                
            if (_chipManager == null)
                Debug.LogError("GameController: ChipManager not found!");
        }

        private void InitializeGame()
        {
            DebugLog("Initializing game...");
            
            ChangeGameState(GameState.Ready);
            ChangeRoundState(RoundState.Idle);
            
            GameEvents.TriggerGameInitialized();
        }

        #endregion

        #region 事件订阅

        private void SubscribeToEvents()
        {
            // 游戏状态事件
            GameEvents.OnGameStateChanged += OnGameStateChanged;
            GameEvents.OnRoundStateChanged += OnRoundStateChanged;
            
            // 投注事件 - 直接使用标准BetInfo类型
            GameEvents.OnBetPlaced += OnBetPlaced;
            GameEvents.OnBetCancelled += OnBetCancelled;
            GameEvents.OnBetConfirmed += OnBetConfirmed;
            
            // 回合事件
            GameEvents.OnRoundStarted += OnRoundStarted;
            GameEvents.OnBettingPhaseEnded += OnBettingPhaseEnded;
            GameEvents.OnDealingCompleted += OnDealingCompleted;
            
            // 网络事件（假设存在）
            // NetworkEvents.OnRoundStartMessage += OnNetworkRoundStart;
            // NetworkEvents.OnRoundResultMessage += OnNetworkRoundResult;
        }

        private void UnsubscribeFromEvents()
        {
            // 游戏状态事件
            GameEvents.OnGameStateChanged -= OnGameStateChanged;
            GameEvents.OnRoundStateChanged -= OnRoundStateChanged;
            
            // 投注事件
            GameEvents.OnBetPlaced -= OnBetPlaced;
            GameEvents.OnBetCancelled -= OnBetCancelled;
            GameEvents.OnBetConfirmed -= OnBetConfirmed;
            
            // 回合事件
            GameEvents.OnRoundStarted -= OnRoundStarted;
            GameEvents.OnBettingPhaseEnded -= OnBettingPhaseEnded;
            GameEvents.OnDealingCompleted -= OnDealingCompleted;
        }

        #endregion

        #region 状态管理

        private void ChangeGameState(GameState newState)
        {
            var oldState = _currentGameState;
            _currentGameState = newState;
            
            DebugLog($"Game state changed: {oldState} -> {newState}");
            GameEvents.TriggerGameStateChanged(oldState, newState);
        }

        private void ChangeRoundState(RoundState newState)
        {
            var oldState = _currentRoundState;
            _currentRoundState = newState;
            
            DebugLog($"Round state changed: {oldState} -> {newState}");
            GameEvents.TriggerRoundStateChanged(oldState, newState);
        }

        #endregion

        #region 投注事件处理 - 直接使用标准BetInfo类型

        private void OnBetPlaced(BetInfo betInfo)
        {
            DebugLog($"Bet placed: {betInfo.betType} - {betInfo.amount}");
            
            // 直接传递给数据存储，无需转换
            if (_dataStore != null)
            {
                _dataStore.AddBet(betInfo);
            }
        }

        private void OnBetCancelled(BetInfo betInfo)
        {
            DebugLog($"Bet cancelled: {betInfo.betId}");
            
            // 直接从数据存储移除，无需转换
            if (_dataStore != null)
            {
                _dataStore.RemoveBet(betInfo.betId);
            }
        }

        private void OnBetConfirmed(BetInfo betInfo)
        {
            DebugLog($"Bet confirmed: {betInfo.betType} - {betInfo.amount}");
        }

        #endregion

        #region 回合管理

        private void StartNewRound(RoundStartMessageInfo messageInfo)
        {
            _currentRoundId = messageInfo.roundId;
            _currentRoundNumber = messageInfo.roundNumber;
            
            ChangeGameState(GameState.Playing);
            // 修复：使用正确的回合状态，直接进入Betting阶段
            ChangeRoundState(RoundState.Betting);

            // 创建回合信息
            var roundInfo = new RoundInfo
            {
                roundId = messageInfo.roundId,
                roundNumber = messageInfo.roundNumber,
                startTime = messageInfo.startTime,
                bettingDuration = messageInfo.bettingDuration,
                tableId = messageInfo.tableId,
                dealerId = messageInfo.dealerId
            };

            GameEvents.TriggerRoundStarted(roundInfo);
            
            // 开始投注阶段
            StartBettingPhase(messageInfo.bettingDuration);
        }

        private void StartBettingPhase(float duration)
        {
            DebugLog($"Starting betting phase for {duration} seconds");
            
            ChangeRoundState(RoundState.Betting);
            
            var bettingPhaseInfo = new BettingPhaseInfo
            {
                roundId = _currentRoundId,
                duration = duration,
                startTime = DateTime.Now,
                allowBetting = true
            };
            
            GameEvents.TriggerBettingPhaseStarted(bettingPhaseInfo);
            
            // 启动投注倒计时
            if (_bettingPhaseCoroutine != null)
                StopCoroutine(_bettingPhaseCoroutine);
            
            _bettingPhaseCoroutine = StartCoroutine(BettingPhaseCountdown(duration));
        }

        private IEnumerator BettingPhaseCountdown(float duration)
        {
            float timeRemaining = duration;
            
            while (timeRemaining > 0)
            {
                // 发送倒计时警告（最后5秒）
                if (timeRemaining <= 5f)
                {
                    GameEvents.TriggerBettingPhaseEnding(timeRemaining);
                }
                
                yield return new WaitForSeconds(1f);
                timeRemaining -= 1f;
            }
            
            // 投注阶段结束
            EndBettingPhase();
        }

        private void EndBettingPhase()
        {
            DebugLog("Betting phase ended");
            
            ChangeRoundState(RoundState.Dealing);
            GameEvents.TriggerBettingPhaseEnded();
            
            // 开始发牌阶段
            StartDealingPhase();
        }

        private void StartDealingPhase()
        {
            DebugLog("Starting dealing phase");
            
            var dealingPhaseInfo = new DealingPhaseInfo
            {
                roundId = _currentRoundId,
                estimatedDuration = _dealingPhaseDuration,
                expectedCardCount = 4, // 初始4张牌
                startTime = DateTime.Now
            };
            
            GameEvents.TriggerDealingPhaseStarted(dealingPhaseInfo);
            
            // 模拟发牌过程
            if (_dealingPhaseCoroutine != null)
                StopCoroutine(_dealingPhaseCoroutine);
            
            _dealingPhaseCoroutine = StartCoroutine(SimulateDealingPhase());
        }

        private IEnumerator SimulateDealingPhase()
        {
            yield return new WaitForSeconds(_dealingPhaseDuration);
            
            // 发牌完成，触发结果
            var dealingCompleteInfo = new DealingCompleteInfo
            {
                roundId = _currentRoundId,
                isComplete = true,
                dealingDuration = _dealingPhaseDuration
            };
            
            GameEvents.TriggerDealingCompleted(dealingCompleteInfo);
            
            // 进入结果阶段
            StartResultPhase();
        }

        private void StartResultPhase()
        {
            DebugLog("Starting result phase");
            ChangeRoundState(RoundState.Result);
            
            // 这里可以处理游戏结果计算
            // 示例：创建模拟结果
            var mockResult = CreateMockRoundResult();
            GameEvents.TriggerRoundResult(mockResult);
            
            // 显示结果一段时间后准备下一轮
            if (_resultPhaseCoroutine != null)
                StopCoroutine(_resultPhaseCoroutine);
            
            _resultPhaseCoroutine = StartCoroutine(ResultDisplayPhase(mockResult));
        }

        private IEnumerator ResultDisplayPhase(RoundResult result)
        {
            yield return new WaitForSeconds(_resultDisplayDuration);
            
            // 结果显示结束，准备下一轮
            PrepareNextRound();
        }

        private void PrepareNextRound()
        {
            DebugLog("Preparing next round");
            ChangeRoundState(RoundState.Idle);
            
            // 清理当前回合数据
            // 等待下一轮开始
        }

        #endregion

        #region 事件响应处理 - 直接使用标准类型

        private void OnRoundStarted(RoundInfo roundInfo)
        {
            DebugLog($"Round started: {roundInfo.roundId}");
        }

        private void OnBettingPhaseEnded()
        {
            DebugLog("Betting phase ended");
        }

        private void OnDealingCompleted(DealingCompleteInfo dealingInfo)
        {
            DebugLog($"Dealing completed for round: {dealingInfo.roundId}");
        }

        private void OnRoundResult(RoundResult result)
        {
            DebugLog($"Round result: {result.mainResult}");
            
            // 修复：移除UpdateRoundResult调用，前端只获取数据不更新
            // 如果需要可以触发其他事件或UI更新
        }

        #endregion

        #region 游戏状态事件处理

        private void OnGameStateChanged(GameState oldState, GameState newState)
        {
            DebugLog($"Game state changed from {oldState} to {newState}");
        }

        private void OnRoundStateChanged(RoundState oldState, RoundState newState)
        {
            DebugLog($"Round state changed from {oldState} to {newState}");
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 创建模拟回合结果 - 使用标准RoundResult类型
        /// </summary>
        private RoundResult CreateMockRoundResult()
        {
            var result = new RoundResult
            {
                gameNumber = _currentRoundId,
                playerTotal = UnityEngine.Random.Range(0, 10),
                bankerTotal = UnityEngine.Random.Range(0, 10),
                mainResult = (BaccaratResult)UnityEngine.Random.Range(1, 4), // 庄、闲、和
                hasPlayerPair = UnityEngine.Random.value < 0.1f,
                hasBankerPair = UnityEngine.Random.value < 0.1f,
                isLucky6 = false,
                isDragon7 = false,
                isPanda8 = false
            };

            // 分析结果
            result.AnalyzeResult();
            
            return result;
        }

        #endregion

        #region 公共方法 - 供外部调用

        /// <summary>
        /// 手动开始新回合（用于测试或外部触发）
        /// </summary>
        public void StartNewRoundManually(string roundId, int roundNumber, float bettingDuration = 30f)
        {
            var messageInfo = new RoundStartMessageInfo
            {
                roundId = roundId,
                roundNumber = roundNumber,
                tableId = "default_table",
                dealerId = "default_dealer",
                bettingDuration = bettingDuration,
                startTime = DateTime.Now
            };
            
            StartNewRound(messageInfo);
        }

        /// <summary>
        /// 获取当前游戏状态
        /// </summary>
        public GameState GetCurrentGameState()
        {
            return _currentGameState;
        }

        /// <summary>
        /// 获取当前回合状态
        /// </summary>
        public RoundState GetCurrentRoundState()
        {
            return _currentRoundState;
        }

        #endregion

        #region 调试方法

        private void DebugLog(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[GameController] {message}");
            }
        }

        #endregion
    }

    #region 消息信息类 - 补充缺失的数据结构

    /// <summary>
    /// 回合开始消息信息
    /// </summary>
    [Serializable]
    public class RoundStartMessageInfo
    {
        public string roundId;
        public int roundNumber;
        public string tableId;
        public string dealerId;
        public float bettingDuration;
        public DateTime startTime;

        public RoundStartMessageInfo()
        {
            startTime = DateTime.Now;
        }
    }

    #endregion
}
// Assets/Core/Architecture/BaccaratDataStore.cs
// 百家乐数据存储 - 修复版，移除无意义转换方法，统一使用标准数据类型
// 负责游戏数据的统一存储和管理，通过事件与其他模块通信

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BaccaratGame.Core.Events;
using BaccaratGame.Data;  // 统一引用标准数据类型

namespace BaccaratGame.Core.Architecture
{
    /// <summary>
    /// 百家乐数据存储
    /// 管理游戏中的所有数据，通过事件系统通知数据变化
    /// </summary>
    public class BaccaratDataStore : MonoBehaviour
    {
        #region 配置

        [Header("玩家配置")]
        [SerializeField] private string _playerId = "player_001";
        [SerializeField] private decimal _initialBalance = 1000m;
        
        [Header("桌台配置")]
        [SerializeField] private string _tableId = "table_001";
        [SerializeField] private string _dealerId = "dealer_001";
        
        [Header("调试设置")]
        [SerializeField] private bool _enableDebugLogs = true;

        #endregion

        #region 核心数据 - 直接使用标准数据类型

        // 玩家数据
        private decimal _playerBalance;
        private string _playerName;
        
        // 桌台数据
        private string _currentTableId;
        private string _currentDealerId;
        private string _currentRoundId;
        private int _currentRoundNumber;
        
        // 游戏状态数据
        private GameState _currentGameState = GameState.Initializing;
        private RoundState _currentRoundState = RoundState.Idle;
        private ConnectionState _connectionState = ConnectionState.Disconnected;
        
        // 投注数据 - 直接使用标准BetInfo类型
        private Dictionary<BaccaratBetType, BetInfo> _currentBets = new Dictionary<BaccaratBetType, BetInfo>();
        private List<BetInfo> _betHistory = new List<BetInfo>();
        private decimal _totalBetAmount = 0m;
        
        // 手牌数据
        private List<Card> _playerCards = new List<Card>();
        private List<Card> _bankerCards = new List<Card>();
        private int _playerTotal = 0;
        private int _bankerTotal = 0;
        
        // 回合数据 - 直接使用标准RoundResult类型
        private RoundResult _lastRoundResult;
        private List<RoundResult> _roundHistory = new List<RoundResult>();
        
        // 统计数据 - 移动到标准位置，这里只保留引用
        private GameStatistics _gameStats = new GameStatistics();

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            InitializeData();
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region 初始化

        private void InitializeData()
        {
            _playerBalance = _initialBalance;
            _currentTableId = _tableId;
            _currentDealerId = _dealerId;
            
            DebugLog("Data store initialized");
        }

        #endregion

        #region 事件订阅

        private void SubscribeToEvents()
        {
            // 游戏流程事件
            GameEvents.OnGameInitialized += OnGameInitialized;
            GameEvents.OnRoundStarted += OnRoundStarted;
            
            // 投注事件 - 直接使用标准BetInfo类型
            GameEvents.OnBetPlaced += OnBetPlaced;
            GameEvents.OnBetCancelled += OnBetCancelled;
            
            // 回合结果事件 - 直接使用标准RoundResult类型
            GameEvents.OnRoundResult += OnRoundResult;
            
            // 连接事件（假设存在）
            // NetworkEvents.OnConnectionEstablished += OnConnectionEstablished;
            // NetworkEvents.OnConnectionLost += OnConnectionLost;
        }

        private void UnsubscribeFromEvents()
        {
            // 游戏流程事件
            GameEvents.OnGameInitialized -= OnGameInitialized;
            GameEvents.OnRoundStarted -= OnRoundStarted;
            
            // 投注事件
            GameEvents.OnBetPlaced -= OnBetPlaced;
            GameEvents.OnBetCancelled -= OnBetCancelled;
            
            // 回合结果事件
            GameEvents.OnRoundResult -= OnRoundResult;
        }

        #endregion

        #region 事件处理 - 直接使用标准类型

        private void OnGameInitialized()
        {
            DebugLog("Game initialized");
        }

        private void OnRoundStarted(RoundInfo roundInfo)
        {
            _currentRoundId = roundInfo.roundId;
            _currentRoundNumber = roundInfo.roundNumber;
            _currentTableId = roundInfo.tableId;
            _currentDealerId = roundInfo.dealerId;
            
            // 清空当前回合数据
            ClearCurrentRoundData();
            
            DebugLog($"Round started: {roundInfo.roundId}");
        }

        private void OnBetPlaced(BetInfo betInfo)
        {
            // 直接使用标准BetInfo，无需转换
            _currentBets[betInfo.betType] = betInfo;
            RecalculateTotalBetAmount();
            
            DebugLog($"Bet placed: {betInfo.betType} - {betInfo.amount}");
        }

        private void OnBetCancelled(BetInfo betInfo)
        {
            // 直接使用标准BetInfo，无需转换
            if (_currentBets.ContainsKey(betInfo.betType))
            {
                _currentBets.Remove(betInfo.betType);
                RecalculateTotalBetAmount();
            }
            
            DebugLog($"Bet cancelled: {betInfo.betId}");
        }

        private void OnRoundResult(RoundResult result)
        {
            // 直接使用标准RoundResult，无需转换
            _lastRoundResult = result;
            _roundHistory.Add(result);
            
            // 更新统计
            UpdateGameStatistics(result);
            
            // 结算投注
            SettleBets(result);
            
            DebugLog($"Round result processed: {result.mainResult}");
        }

        #endregion

        #region 投注数据管理 - 直接使用标准BetInfo类型

        /// <summary>
        /// 添加投注 - 直接接受标准BetInfo类型
        /// </summary>
        public bool AddBet(BetInfo betInfo)
        {
            if (betInfo.amount > _playerBalance)
            {
                DebugLog($"Insufficient balance for bet: {betInfo.amount}");
                return false;
            }

            _currentBets[betInfo.betType] = betInfo;
            RecalculateTotalBetAmount();
            
            DebugLog($"Bet added: {betInfo.betType} - {betInfo.amount}");
            return true;
        }

        /// <summary>
        /// 移除投注
        /// </summary>
        public bool RemoveBet(string betId)
        {
            var betToRemove = _currentBets.Values.FirstOrDefault(b => b.betId == betId);
            if (betToRemove != null)
            {
                _currentBets.Remove(betToRemove.betType);
                RecalculateTotalBetAmount();
                
                DebugLog($"Bet removed: {betId}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 移除指定类型的投注
        /// </summary>
        public bool RemoveBet(BaccaratBetType betType)
        {
            if (_currentBets.ContainsKey(betType))
            {
                var betInfo = _currentBets[betType];
                _currentBets.Remove(betType);
                RecalculateTotalBetAmount();
                
                DebugLog($"Bet removed: {betType}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 重新计算总投注金额
        /// </summary>
        private void RecalculateTotalBetAmount()
        {
            _totalBetAmount = 0m;
            foreach (var bet in _currentBets.Values)
            {
                _totalBetAmount += bet.amount;
            }
            
            DebugLog($"Total bet amount recalculated: {_totalBetAmount}");
        }

        /// <summary>
        /// 获取当前投注总额
        /// </summary>
        public decimal GetTotalBetAmount()
        {
            return _totalBetAmount;
        }

        /// <summary>
        /// 获取当前投注信息
        /// </summary>
        public Dictionary<BaccaratBetType, BetInfo> GetCurrentBets()
        {
            return new Dictionary<BaccaratBetType, BetInfo>(_currentBets);
        }

        /// <summary>
        /// 获取投注历史
        /// </summary>
        public List<BetInfo> GetBetHistory(int count = 0)
        {
            if (count <= 0)
                return new List<BetInfo>(_betHistory);
            
            return _betHistory.Skip(Math.Max(0, _betHistory.Count - count)).ToList();
        }

        /// <summary>
        /// 获取最近N轮结果
        /// </summary>
        public List<RoundResult> GetRecentRounds(int count)
        {
            return _roundHistory.Skip(Math.Max(0, _roundHistory.Count - count)).ToList();
        }

        /// <summary>
        /// 获取回合历史
        /// </summary>
        public List<RoundResult> GetRoundHistory(int count = 0)
        {
            if (count <= 0)
                return new List<RoundResult>(_roundHistory);
            
            return _roundHistory.Skip(Math.Max(0, _roundHistory.Count - count)).ToList();
        }

        /// <summary>
        /// 检查是否可以下注
        /// </summary>
        public bool CanPlaceBet(BaccaratBetType betType, decimal amount)
        {
            // 检查游戏状态
            if (_currentRoundState != RoundState.Betting)
                return false;
            
            // 检查余额
            if (_playerBalance < amount)
                return false;
            
            // 检查是否已有相同类型的投注
            if (_currentBets.ContainsKey(betType))
                return false;
            
            return true;
        }

        #endregion

        #region 游戏状态管理

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

        /// <summary>
        /// 获取当前连接状态
        /// </summary>
        public ConnectionState GetConnectionState()
        {
            return _connectionState;
        }

        /// <summary>
        /// 更新游戏状态
        /// </summary>
        public void UpdateGameState(GameState newState)
        {
            var oldState = _currentGameState;
            _currentGameState = newState;
            
            DebugLog($"Game state changed: {oldState} -> {newState}");
        }

        /// <summary>
        /// 更新回合状态
        /// </summary>
        public void UpdateRoundState(RoundState newState)
        {
            var oldState = _currentRoundState;
            _currentRoundState = newState;
            
            DebugLog($"Round state changed: {oldState} -> {newState}");
        }

        /// <summary>
        /// 更新连接状态
        /// </summary>
        public void UpdateConnectionState(ConnectionState newState)
        {
            var oldState = _connectionState;
            _connectionState = newState;
            
            DebugLog($"Connection state changed: {oldState} -> {newState}");
        }

        /// <summary>
        /// 获取当前回合信息
        /// </summary>
        public RoundInfo GetCurrentRoundInfo()
        {
            return new RoundInfo
            {
                roundId = _currentRoundId ?? "",
                roundNumber = _currentRoundNumber,
                tableId = _currentTableId ?? "",
                dealerId = _currentDealerId ?? ""
            };
        }

        #endregion

        #region 余额管理

        /// <summary>
        /// 获取玩家余额
        /// </summary>
        public decimal GetPlayerBalance()
        {
            return _playerBalance;
        }

        /// <summary>
        /// 更新玩家余额
        /// </summary>
        public void UpdatePlayerBalance(decimal newBalance)
        {
            var oldBalance = _playerBalance;
            _playerBalance = newBalance;
            
            // 触发余额变化事件
            GameEvents.TriggerBalanceChanged(oldBalance, newBalance);
            
            DebugLog($"Balance updated: {oldBalance} -> {newBalance}");
        }

        /// <summary>
        /// 扣除余额
        /// </summary>
        public bool DeductBalance(decimal amount)
        {
            if (_playerBalance >= amount)
            {
                UpdatePlayerBalance(_playerBalance - amount);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 增加余额
        /// </summary>
        public void AddBalance(decimal amount)
        {
            UpdatePlayerBalance(_playerBalance + amount);
        }

        #endregion

        #region 手牌数据管理

        /// <summary>
        /// 获取玩家手牌信息
        /// </summary>
        public HandInfo GetPlayerHand()
        {
            return new HandInfo(_playerCards);
        }

        /// <summary>
        /// 获取庄家手牌信息
        /// </summary>
        public HandInfo GetBankerHand()
        {
            return new HandInfo(_bankerCards);
        }

        /// <summary>
        /// 添加玩家卡牌
        /// </summary>
        public void AddPlayerCard(Card card)
        {
            _playerCards.Add(card);
            _playerTotal = RoundResult.CalculateHandTotal(_playerCards);
            
            DebugLog($"Player card added: {card}, Total: {_playerTotal}");
        }

        /// <summary>
        /// 添加庄家卡牌
        /// </summary>
        public void AddBankerCard(Card card)
        {
            _bankerCards.Add(card);
            _bankerTotal = RoundResult.CalculateHandTotal(_bankerCards);
            
            DebugLog($"Banker card added: {card}, Total: {_bankerTotal}");
        }

        #endregion

        #region 统计数据管理

        /// <summary>
        /// 获取游戏统计
        /// </summary>
        public GameStatistics GetGameStatistics()
        {
            return _gameStats;
        }

        /// <summary>
        /// 更新游戏统计
        /// </summary>
        private void UpdateGameStatistics(RoundResult result)
        {
            _gameStats.totalRounds++;
            
            switch (result.mainResult)
            {
                case BaccaratResult.Player:
                    _gameStats.playerWins++;
                    break;
                case BaccaratResult.Banker:
                    _gameStats.bankerWins++;
                    break;
                case BaccaratResult.Tie:
                    _gameStats.ties++;
                    break;
            }
            
            if (result.hasPlayerPair)
                _gameStats.playerPairs++;
            
            if (result.hasBankerPair)
                _gameStats.bankerPairs++;
                
            // 触发统计更新事件
            GameEvents.TriggerStatisticsUpdated(_gameStats);
        }

        #endregion

        #region 投注结算

        /// <summary>
        /// 结算投注
        /// </summary>
        private void SettleBets(RoundResult result)
        {
            foreach (var bet in _currentBets.Values)
            {
                var settlement = CalculateBetSettlement(bet, result);
                
                // 更新投注状态
                bet.status = BetStatus.Settled;
                bet.isWin = settlement.isWin;
                bet.winAmount = settlement.winAmount;
                
                // 更新余额
                if (settlement.isWin)
                {
                    AddBalance(settlement.winAmount);
                }
                
                // 触发结算事件
                GameEvents.TriggerBetSettled(settlement);
                
                // 添加到历史
                _betHistory.Add(bet);
            }
            
            // 清空当前投注
            _currentBets.Clear();
            _totalBetAmount = 0m;
        }

        /// <summary>
        /// 计算投注结算
        /// </summary>
        private BetSettlement CalculateBetSettlement(BetInfo bet, RoundResult result)
        {
            bool isWin = CheckBetWin(bet.betType, result);
            decimal winAmount = 0m;
            decimal payout = 0m;
            
            if (isWin)
            {
                float odds = bet.betType.GetStandardOdds();
                payout = bet.amount * (decimal)odds;
                winAmount = bet.amount + payout; // 本金 + 奖金
            }
            
            return new BetSettlement
            {
                betId = bet.betId,
                betType = bet.betType,
                betAmount = bet.amount,
                winAmount = winAmount,
                payout = payout,
                isWin = isWin,
                settlementId = Guid.NewGuid().ToString()
            };
        }

        /// <summary>
        /// 检查投注是否中奖
        /// </summary>
        private bool CheckBetWin(BaccaratBetType betType, RoundResult result)
        {
            return betType switch
            {
                BaccaratBetType.Player => result.mainResult == BaccaratResult.Player,
                BaccaratBetType.Banker => result.mainResult == BaccaratResult.Banker,
                BaccaratBetType.Tie => result.mainResult == BaccaratResult.Tie,
                BaccaratBetType.PlayerPair => result.hasPlayerPair,
                BaccaratBetType.BankerPair => result.hasBankerPair,
                BaccaratBetType.Lucky6 => result.isLucky6,
                BaccaratBetType.Dragon7 => result.isDragon7,
                BaccaratBetType.Panda8 => result.isPanda8,
                _ => false
            };
        }

        #endregion

        #region 数据清理

        /// <summary>
        /// 清空当前回合数据
        /// </summary>
        private void ClearCurrentRoundData()
        {
            _playerCards.Clear();
            _bankerCards.Clear();
            _playerTotal = 0;
            _bankerTotal = 0;
            
            DebugLog("Current round data cleared");
        }

        /// <summary>
        /// 清空投注历史
        /// </summary>
        public void ClearBetHistory()
        {
            _betHistory.Clear();
            DebugLog("Bet history cleared");
        }

        /// <summary>
        /// 清空回合历史
        /// </summary>
        public void ClearRoundHistory()
        {
            _roundHistory.Clear();
            DebugLog("Round history cleared");
        }

        /// <summary>
        /// 重置所有数据
        /// </summary>
        public void ResetAllData()
        {
            _playerBalance = _initialBalance;
            _currentBets.Clear();
            _betHistory.Clear();
            _roundHistory.Clear();
            _gameStats = new GameStatistics();
            _totalBetAmount = 0m;
            
            ClearCurrentRoundData();
            
            DebugLog("All data reset");
        }

        #endregion

        #region 调试方法

        private void DebugLog(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[BaccaratDataStore] {message}");
            }
        }

        /// <summary>
        /// 获取调试信息
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Balance: {_playerBalance}, " +
                   $"Game State: {_currentGameState}, " +
                   $"Round State: {_currentRoundState}, " +
                   $"Current Bets: {_currentBets.Count}, " +
                   $"Total Bet Amount: {_totalBetAmount}, " +
                   $"Round: {_currentRoundNumber}";
        }

        #endregion
    }
}
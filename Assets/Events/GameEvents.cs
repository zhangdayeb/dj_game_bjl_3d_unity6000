// Assets/Events/GameEvents.cs
// 游戏事件系统 - 完整版，移除重复定义，统一引用标准数据类型
// 专门处理百家乐游戏的事件定义和触发机制

using System;
using System.Collections.Generic;
using UnityEngine;
using BaccaratGame.Data;   

namespace BaccaratGame.Core.Events
{
    /// <summary>
    /// 游戏事件系统
    /// 定义游戏中的所有事件和相关数据结构
    /// </summary>
    public static class GameEvents
    {
        #region 游戏生命周期事件

        /// <summary>
        /// 游戏初始化完成事件
        /// </summary>
        public static event System.Action OnGameInitialized;

        /// <summary>
        /// 游戏启动事件
        /// </summary>
        public static event System.Action OnGameStarted;

        /// <summary>
        /// 游戏结束事件
        /// </summary>
        public static event System.Action<GameEndReason> OnGameEnded;

        /// <summary>
        /// 游戏状态变化事件
        /// </summary>
        public static event System.Action<GameState, GameState> OnGameStateChanged;

        #endregion

        #region 回合相关事件

        /// <summary>
        /// 回合开始事件
        /// </summary>
        public static event System.Action<RoundInfo> OnRoundStarted;

        /// <summary>
        /// 回合状态变化事件
        /// </summary>
        public static event System.Action<RoundState, RoundState> OnRoundStateChanged;

        /// <summary>
        /// 投注阶段开始事件
        /// </summary>
        public static event System.Action<BettingPhaseInfo> OnBettingPhaseStarted;

        /// <summary>
        /// 投注阶段即将结束事件（倒计时警告）
        /// </summary>
        public static event System.Action<float> OnBettingPhaseEnding;

        /// <summary>
        /// 投注阶段结束事件
        /// </summary>
        public static event System.Action OnBettingPhaseEnded;

        /// <summary>
        /// 发牌阶段开始事件
        /// </summary>
        public static event System.Action<DealingPhaseInfo> OnDealingPhaseStarted;

        /// <summary>
        /// 发牌完成事件
        /// </summary>
        public static event System.Action<DealingCompleteInfo> OnDealingCompleted;

        /// <summary>
        /// 回合结果事件 - 使用标准RoundResult
        /// </summary>
        public static event System.Action<RoundResult> OnRoundResult;

        #endregion

        #region 投注相关事件

        /// <summary>
        /// 投注下注事件 - 使用标准BetInfo
        /// </summary>
        public static event System.Action<BetInfo> OnBetPlaced;

        /// <summary>
        /// 投注取消事件 - 使用标准BetInfo
        /// </summary>
        public static event System.Action<BetInfo> OnBetCancelled;

        /// <summary>
        /// 投注确认事件 - 使用标准BetInfo
        /// </summary>
        public static event System.Action<BetInfo> OnBetConfirmed;

        /// <summary>
        /// 投注结算事件
        /// </summary>
        public static event System.Action<BetSettlement> OnBetSettled;

        /// <summary>
        /// 投注限额检查事件
        /// </summary>
        public static event System.Action<BetLimitCheckResult> OnBetLimitChecked;

        #endregion

        #region 余额和状态事件

        /// <summary>
        /// 余额变化事件
        /// </summary>
        public static event System.Action<decimal, decimal> OnBalanceChanged;

        /// <summary>
        /// 连接状态变化事件
        /// </summary>
        public static event System.Action<ConnectionState> OnConnectionStateChanged;

        #endregion

        #region 卡牌相关事件

        /// <summary>
        /// 卡牌发出事件
        /// </summary>
        public static event System.Action<CardDealtInfo> OnCardDealt;

        /// <summary>
        /// 闲家卡牌揭示事件
        /// </summary>
        public static event System.Action<HandInfo> OnPlayerCardsRevealed;

        /// <summary>
        /// 庄家卡牌揭示事件
        /// </summary>
        public static event System.Action<HandInfo> OnBankerCardsRevealed;

        /// <summary>
        /// 第三张牌规则应用事件
        /// </summary>
        public static event System.Action<ThirdCardRuleResult> OnThirdCardRuleApplied;

        #endregion

        #region 错误和警告事件

        /// <summary>
        /// 游戏错误事件
        /// </summary>
        public static event System.Action<GameError> OnGameError;

        /// <summary>
        /// 游戏警告事件
        /// </summary>
        public static event System.Action<GameWarning> OnGameWarning;

        #endregion

        #region 成就和统计事件

        /// <summary>
        /// 成就解锁事件
        /// </summary>
        public static event System.Action<Achievement> OnAchievementUnlocked;

        /// <summary>
        /// 统计数据更新事件 - 使用标准GameStatistics
        /// </summary>
        public static event System.Action<GameStatistics> OnStatisticsUpdated;

        #endregion

        #region 事件触发方法

        public static void TriggerGameInitialized()
        {
            OnGameInitialized?.Invoke();
        }

        public static void TriggerGameStarted()
        {
            OnGameStarted?.Invoke();
        }

        public static void TriggerGameEnded(GameEndReason reason)
        {
            OnGameEnded?.Invoke(reason);
        }

        public static void TriggerRoundStarted(RoundInfo roundInfo)
        {
            OnRoundStarted?.Invoke(roundInfo);
        }

        public static void TriggerBettingPhaseStarted(BettingPhaseInfo phaseInfo)
        {
            OnBettingPhaseStarted?.Invoke(phaseInfo);
        }

        public static void TriggerBettingPhaseEnding(float timeRemaining)
        {
            OnBettingPhaseEnding?.Invoke(timeRemaining);
        }

        public static void TriggerBettingPhaseEnded()
        {
            OnBettingPhaseEnded?.Invoke();
        }

        public static void TriggerDealingPhaseStarted(DealingPhaseInfo phaseInfo)
        {
            OnDealingPhaseStarted?.Invoke(phaseInfo);
        }

        public static void TriggerDealingCompleted(DealingCompleteInfo dealingInfo)
        {
            OnDealingCompleted?.Invoke(dealingInfo);
        }

        public static void TriggerRoundResult(RoundResult result)
        {
            OnRoundResult?.Invoke(result);
        }

        public static void TriggerBetPlaced(BetInfo betInfo)
        {
            OnBetPlaced?.Invoke(betInfo);
        }

        public static void TriggerBetCancelled(BetInfo betInfo)
        {
            OnBetCancelled?.Invoke(betInfo);
        }

        public static void TriggerBetConfirmed(BetInfo betInfo)
        {
            OnBetConfirmed?.Invoke(betInfo);
        }

        public static void TriggerBetSettled(BetSettlement settlement)
        {
            OnBetSettled?.Invoke(settlement);
        }

        public static void TriggerBetLimitChecked(BetLimitCheckResult result)
        {
            OnBetLimitChecked?.Invoke(result);
        }

        public static void TriggerGameStateChanged(GameState oldState, GameState newState)
        {
            OnGameStateChanged?.Invoke(oldState, newState);
        }

        public static void TriggerRoundStateChanged(RoundState oldState, RoundState newState)
        {
            OnRoundStateChanged?.Invoke(oldState, newState);
        }

        public static void TriggerBalanceChanged(decimal oldBalance, decimal newBalance)
        {
            OnBalanceChanged?.Invoke(oldBalance, newBalance);
        }

        public static void TriggerConnectionStateChanged(ConnectionState state)
        {
            OnConnectionStateChanged?.Invoke(state);
        }

        public static void TriggerCardDealt(CardDealtInfo cardInfo)
        {
            OnCardDealt?.Invoke(cardInfo);
        }

        public static void TriggerPlayerCardsRevealed(HandInfo handInfo)
        {
            OnPlayerCardsRevealed?.Invoke(handInfo);
        }

        public static void TriggerBankerCardsRevealed(HandInfo handInfo)
        {
            OnBankerCardsRevealed?.Invoke(handInfo);
        }

        public static void TriggerThirdCardRuleApplied(ThirdCardRuleResult result)
        {
            OnThirdCardRuleApplied?.Invoke(result);
        }

        public static void TriggerGameError(GameError error)
        {
            OnGameError?.Invoke(error);
        }

        public static void TriggerGameWarning(GameWarning warning)
        {
            OnGameWarning?.Invoke(warning);
        }

        public static void TriggerAchievementUnlocked(Achievement achievement)
        {
            OnAchievementUnlocked?.Invoke(achievement);
        }

        public static void TriggerStatisticsUpdated(GameStatistics statistics)
        {
            OnStatisticsUpdated?.Invoke(statistics);
        }

        /// <summary>
        /// 清理所有事件订阅
        /// </summary>
        public static void ClearAllEvents()
        {
            OnGameInitialized = null;
            OnGameStarted = null;
            OnGameEnded = null;
            OnGameStateChanged = null;
            
            OnRoundStarted = null;
            OnRoundStateChanged = null;
            OnBettingPhaseStarted = null;
            OnBettingPhaseEnding = null;
            OnBettingPhaseEnded = null;
            OnDealingPhaseStarted = null;
            OnDealingCompleted = null;
            OnRoundResult = null;
            
            OnBetPlaced = null;
            OnBetCancelled = null;
            OnBetConfirmed = null;
            OnBetSettled = null;
            OnBetLimitChecked = null;
            
            OnBalanceChanged = null;
            OnConnectionStateChanged = null;
            
            OnCardDealt = null;
            OnPlayerCardsRevealed = null;
            OnBankerCardsRevealed = null;
            OnThirdCardRuleApplied = null;
            
            OnGameError = null;
            OnGameWarning = null;
            OnAchievementUnlocked = null;
            OnStatisticsUpdated = null;
        }

        #endregion
    }
}
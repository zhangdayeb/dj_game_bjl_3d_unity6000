// Assets/Core/Data/Types/BaccaratTypes.cs
// 百家乐业务核心类型定义 - 精简版
// 只保留百家乐游戏特有的业务逻辑类型
// 创建时间: 2025/6/22

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BaccaratGame.Data; // 引用Card类

namespace BaccaratGame.Data
{
    #region 百家乐枚举

    /// <summary>
    /// 百家乐投注类型（与后端API对应）
    /// </summary>
    public enum BaccaratBetType
    {
        Player = 2,      // 闲家
        Lucky6 = 3,      // 幸运6  
        PlayerPair = 4,  // 闲对
        Banker = 6,      // 庄家
        Tie = 7,         // 和局
        BankerPair = 8,  // 庄对
        Dragon7 = 9,     // 龙7
        Panda8 = 10      // 熊8
    }

    /// <summary>
    /// 百家乐游戏结果
    /// </summary>
    public enum BaccaratResult
    {
        None = 0,
        Banker = 1,      // 庄胜
        Player = 2,      // 闲胜  
        Tie = 3,         // 和局
        BankerPair = 4,  // 庄对
        PlayerPair = 5,  // 闲对
        Lucky6 = 6,      // 幸运6
        Dragon7 = 7,     // 龙7
        Panda8 = 8       // 熊8
    }

    /// <summary>
    /// 投注状态
    /// </summary>
    public enum BetStatus
    {
        Pending = 0,    // 待处理
        Confirmed = 1,  // 已确认
        Cancelled = 2,  // 已取消
        Settled = 3     // 已结算
    }

    #endregion

    #region 核心数据结构

    /// <summary>
    /// 投注信息
    /// </summary>
    [Serializable]
    public class BetInfo
    {
        public BaccaratBetType betType = BaccaratBetType.Player;
        public decimal amount = 0m;
        public BetStatus status = BetStatus.Pending;
        public bool isWin = false;
        public float odds = 1.0f;
        public decimal winAmount = 0m;
        public string betId = "";
        public string gameNumber = "";

        public BetInfo()
        {
            betId = Guid.NewGuid().ToString();
        }

        public BetInfo(BaccaratBetType betType, decimal amount, string gameNumber)
        {
            this.betType = betType;
            this.amount = amount;
            this.gameNumber = gameNumber;
            this.betId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 获取投注类型显示名称
        /// </summary>
        public string GetBetTypeName()
        {
            return betType switch
            {
                BaccaratBetType.Player => "闲家",
                BaccaratBetType.Banker => "庄家",
                BaccaratBetType.Tie => "和局",
                BaccaratBetType.PlayerPair => "闲对",
                BaccaratBetType.BankerPair => "庄对",
                BaccaratBetType.Lucky6 => "幸运6",
                BaccaratBetType.Dragon7 => "龙7",
                BaccaratBetType.Panda8 => "熊8",
                _ => "未知"
            };
        }

        /// <summary>
        /// 计算净盈亏
        /// </summary>
        public decimal GetNetProfit()
        {
            return isWin ? winAmount - amount : -amount;
        }
    }

    /// <summary>
    /// 回合结果
    /// </summary>
    [Serializable]
    public class RoundResult
    {
        public string gameNumber = "";
        public List<Card> playerCards = new List<Card>();
        public List<Card> bankerCards = new List<Card>();
        public int playerTotal = 0;
        public int bankerTotal = 0;
        public BaccaratResult mainResult = BaccaratResult.None;
        public bool hasPlayerPair = false;
        public bool hasBankerPair = false;
        public bool isLucky6 = false;
        public bool isDragon7 = false;
        public bool isPanda8 = false;

        /// <summary>
        /// 计算手牌总点数
        /// </summary>
        public static int CalculateHandTotal(List<Card> cards)
        {
            int total = 0;
            foreach (var card in cards)
            {
                total += card.GetBaccaratValue();
            }
            return total % 10; // 百家乐只取个位数
        }

        /// <summary>
        /// 分析游戏结果
        /// </summary>
        public void AnalyzeResult()
        {
            playerTotal = CalculateHandTotal(playerCards);
            bankerTotal = CalculateHandTotal(bankerCards);

            // 检查对子
            hasPlayerPair = playerCards.Count >= 2 && playerCards[0].Rank == playerCards[1].Rank;
            hasBankerPair = bankerCards.Count >= 2 && bankerCards[0].Rank == bankerCards[1].Rank;

            // 确定主要结果
            if (playerTotal > bankerTotal)
                mainResult = BaccaratResult.Player;
            else if (bankerTotal > playerTotal)
                mainResult = BaccaratResult.Banker;
            else
                mainResult = BaccaratResult.Tie;

            // 检查特殊结果
            isLucky6 = (mainResult == BaccaratResult.Banker && bankerTotal == 6);
            isDragon7 = (mainResult == BaccaratResult.Banker && bankerTotal == 7 && bankerCards.Count == 3);
            isPanda8 = (mainResult == BaccaratResult.Player && playerTotal == 8 && playerCards.Count == 3);
        }

        /// <summary>
        /// 获取结果摘要
        /// </summary>
        public string GetResultSummary()
        {
            var resultName = mainResult switch
            {
                BaccaratResult.Banker => "庄胜",
                BaccaratResult.Player => "闲胜",
                BaccaratResult.Tie => "和局",
                _ => "无结果"
            };

            var summary = $"{resultName} ({playerTotal} vs {bankerTotal})";

            var special = new List<string>();
            if (hasPlayerPair) special.Add("闲对");
            if (hasBankerPair) special.Add("庄对");
            if (isLucky6) special.Add("幸运6");
            if (isDragon7) special.Add("龙7");
            if (isPanda8) special.Add("熊8");

            if (special.Count > 0)
                summary += " + " + string.Join(", ", special);

            return summary;
        }
    }

    /// <summary>
    /// 游戏统计数据
    /// </summary>
    [Serializable]
    public class GameStatistics
    {
        public int totalRounds = 0;
        public int playerWins = 0;
        public int bankerWins = 0;
        public int ties = 0;
        public int playerPairs = 0;
        public int bankerPairs = 0;

        /// <summary>
        /// 获取玩家胜率
        /// </summary>
        public float GetPlayerWinRate()
        {
            return totalRounds > 0 ? (float)playerWins / totalRounds : 0f;
        }

        /// <summary>
        /// 获取庄家胜率
        /// </summary>
        public float GetBankerWinRate()
        {
            return totalRounds > 0 ? (float)bankerWins / totalRounds : 0f;
        }

        /// <summary>
        /// 获取和局率
        /// </summary>
        public float GetTieRate()
        {
            return totalRounds > 0 ? (float)ties / totalRounds : 0f;
        }

        /// <summary>
        /// 获取对子出现率
        /// </summary>
        public float GetPairRate()
        {
            return totalRounds > 0 ? (float)(playerPairs + bankerPairs) / totalRounds : 0f;
        }

        /// <summary>
        /// 重置统计数据
        /// </summary>
        public void Reset()
        {
            totalRounds = 0;
            playerWins = 0;
            bankerWins = 0;
            ties = 0;
            playerPairs = 0;
            bankerPairs = 0;
        }

        /// <summary>
        /// 获取统计摘要
        /// </summary>
        public string GetSummary()
        {
            return $"总局数: {totalRounds}, 庄胜: {bankerWins}, 闲胜: {playerWins}, 和局: {ties}";
        }
    }

    #endregion

    #region 免佣相关类型

    /// <summary>
    /// 免佣设置
    /// </summary>
    [Serializable]
    public class ExemptSettings
    {
        public bool isEnabled = false;
        public float exemptRate = 0.05f;       // 免佣率 5%
        public decimal minBetAmount = 100m;     // 最低免佣投注额
        public bool isAvailable = true;        // 是否可用
        public bool onlyForBanker = true;      // 仅限庄家投注
        public float breakEvenPoint = 20f;     // 保本点
    }

    /// <summary>
    /// 免佣统计
    /// </summary>
    [Serializable]
    public class ExemptStatistics
    {
        public int totalExemptGames = 0;
        public decimal totalExemptSavings = 0m;
        public float averageExemptRate = 0f;
        public DateTime lastExemptTime = DateTime.MinValue;
    }

    #endregion

    #region 事件相关数据类型

    /// <summary>
    /// 回合信息 - 事件专用
    /// </summary>
    [Serializable]
    public class RoundInfo
    {
        public string roundId = "";
        public int roundNumber = 0;
        public string tableId = "";
        public string dealerId = "";
        public DateTime startTime = DateTime.Now;
        public float bettingDuration = 30f;

        public RoundInfo() { }

        public RoundInfo(string roundId, int roundNumber, string tableId, string dealerId)
        {
            this.roundId = roundId;
            this.roundNumber = roundNumber;
            this.tableId = tableId;
            this.dealerId = dealerId;
            this.startTime = DateTime.Now;
        }

        /// <summary>
        /// 验证回合信息完整性
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(roundId) &&
                   !string.IsNullOrEmpty(tableId) &&
                   roundNumber > 0;
        }

        public override string ToString()
        {
            return $"Round[{roundId}:{roundNumber}, Table:{tableId}]";
        }
    }

    /// <summary>
    /// 投注结算信息
    /// </summary>
    [Serializable]
    public class BetSettlement
    {
        public string betId = "";
        public string settlementId = "";
        public BaccaratBetType betType = BaccaratBetType.Player;
        public decimal betAmount = 0m;
        public decimal winAmount = 0m;
        public decimal payout = 0m;
        public bool isWin = false;
        public DateTime settlementTime = DateTime.Now;

        public BetSettlement()
        {
            settlementId = Guid.NewGuid().ToString();
            settlementTime = DateTime.Now;
        }

        public BetSettlement(string betId, BaccaratBetType betType, decimal betAmount, bool isWin, decimal winAmount)
        {
            this.betId = betId;
            this.betType = betType;
            this.betAmount = betAmount;
            this.isWin = isWin;
            this.winAmount = winAmount;
            this.payout = isWin ? winAmount - betAmount : 0m;
            this.settlementId = Guid.NewGuid().ToString();
            this.settlementTime = DateTime.Now;
        }

        /// <summary>
        /// 获取净盈亏
        /// </summary>
        public decimal GetNetProfit()
        {
            return isWin ? winAmount - betAmount : -betAmount;
        }

        /// <summary>
        /// 获取投注类型显示名称
        /// </summary>
        public string GetBetTypeName()
        {
            return betType.GetBetTypeName();
        }

        public override string ToString()
        {
            return $"Settlement[{betId}, {betType}, {(isWin ? "WIN" : "LOSE")}, Profit:{GetNetProfit()}]";
        }
    }

    /// <summary>
    /// 手牌信息
    /// </summary>
    [Serializable]
    public class HandInfo
    {
        public List<Card> cards = new List<Card>();
        public int total = 0;
        public bool hasPair = false;

        public HandInfo() { }

        public HandInfo(List<Card> cards)
        {
            this.cards = new List<Card>(cards ?? new List<Card>());
            CalculateTotal();
            CheckPair();
        }

        /// <summary>
        /// 计算手牌总点数
        /// </summary>
        public void CalculateTotal()
        {
            total = 0;
            foreach (var card in cards)
            {
                total += card.GetBaccaratValue();
            }
            total %= 10; // 百家乐只取个位数
        }

        /// <summary>
        /// 检查是否有对子
        /// </summary>
        public void CheckPair()
        {
            hasPair = cards.Count >= 2 && cards[0].Rank == cards[1].Rank;
        }

        /// <summary>
        /// 添加卡牌
        /// </summary>
        public void AddCard(Card card)
        {
            if (card != null)
            {
                cards.Add(card);
                CalculateTotal();
                CheckPair();
            }
        }

        /// <summary>
        /// 清空手牌
        /// </summary>
        public void Clear()
        {
            cards.Clear();
            total = 0;
            hasPair = false;
        }

        /// <summary>
        /// 获取手牌字符串表示
        /// </summary>
        public string GetCardsString()
        {
            return string.Join(" ", cards.Select(c => c.ToString()));
        }

        public override string ToString()
        {
            return $"Hand[{GetCardsString()}, Total:{total}, Pair:{hasPair}]";
        }
    }

    /// <summary>
    /// 发牌信息
    /// </summary>
    [Serializable]
    public class CardDealtInfo
    {
        public Card card;
        public bool isVisible = true;
        public string targetHand = ""; // "player" 或 "banker"
        public int cardIndex = 0;
        public float dealDelay = 0f;

        public CardDealtInfo() { }

        public CardDealtInfo(Card card, bool isVisible, string targetHand)
        {
            this.card = card;
            this.isVisible = isVisible;
            this.targetHand = targetHand;
        }

        public override string ToString()
        {
            return $"CardDealt[{card}, {targetHand}, Visible:{isVisible}]";
        }
    }

    #endregion

    #region 扩展方法

    /// <summary>
    /// 百家乐枚举扩展方法
    /// </summary>
    public static class BaccaratExtensions
    {
        /// <summary>
        /// 获取投注类型的标准赔率
        /// </summary>
        public static float GetStandardOdds(this BaccaratBetType betType)
        {
            return betType switch
            {
                BaccaratBetType.Player => 1.0f,
                BaccaratBetType.Banker => 0.95f, // 扣除5%佣金
                BaccaratBetType.Tie => 8.0f,
                BaccaratBetType.PlayerPair => 11.0f,
                BaccaratBetType.BankerPair => 11.0f,
                BaccaratBetType.Lucky6 => 20.0f,
                BaccaratBetType.Dragon7 => 40.0f,
                BaccaratBetType.Panda8 => 25.0f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// 判断是否为主要投注类型
        /// </summary>
        public static bool IsMainBet(this BaccaratBetType betType)
        {
            return betType == BaccaratBetType.Player ||
                   betType == BaccaratBetType.Banker ||
                   betType == BaccaratBetType.Tie;
        }

        /// <summary>
        /// 获取投注类型显示名称（扩展方法版本）
        /// </summary>
        public static string GetBetTypeName(this BaccaratBetType betType)
        {
            return betType switch
            {
                BaccaratBetType.Player => "闲家",
                BaccaratBetType.Banker => "庄家",
                BaccaratBetType.Tie => "和局",
                BaccaratBetType.PlayerPair => "闲对",
                BaccaratBetType.BankerPair => "庄对",
                BaccaratBetType.Lucky6 => "幸运6",
                BaccaratBetType.Dragon7 => "龙7",
                BaccaratBetType.Panda8 => "熊8",
                _ => "未知"
            };
        }

        /// <summary>
        /// 判断是否为天牌（8或9点）
        /// </summary>
        public static bool IsNatural(this HandInfo handInfo)
        {
            return handInfo.total == 8 || handInfo.total == 9;
        }

        /// <summary>
        /// 获取主要结果
        /// </summary>
        public static BaccaratResult GetResult(this RoundResult roundResult)
        {
            return roundResult.mainResult;
        }

        /// <summary>
        /// 判断是否为天牌
        /// </summary>
        public static bool IsNatural(this RoundResult roundResult)
        {
            return (roundResult.playerTotal == 8 || roundResult.playerTotal == 9) ||
                   (roundResult.bankerTotal == 8 || roundResult.bankerTotal == 9);
        }
    }

    #endregion
}
// Assets/Core/Data/Types/BaccaratTypes.cs
// 百家乐业务核心类型定义 - 激进精简版
// 只保留立即需要的核心业务逻辑类型

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BaccaratGame.Data
{
    #region 卡牌类

    /// <summary>
    /// 卡牌类
    /// </summary>
    [Serializable]
    public class Card
    {
        [SerializeField] private CardSuit suit = CardSuit.Spades;
        [SerializeField] private CardRank rank = CardRank.Ace;

        public CardSuit Suit => suit;
        public CardRank Rank => rank;

        public Card() { }

        public Card(CardSuit suit, CardRank rank)
        {
            this.suit = suit;
            this.rank = rank;
        }

        public Card(int cardId)
        {
            this.suit = (CardSuit)(cardId / 100);
            this.rank = (CardRank)(cardId % 100);
        }

        /// <summary>
        /// 获取百家乐点数
        /// </summary>
        public int GetBaccaratValue()
        {
            return rank switch
            {
                CardRank.Ace => 1,
                CardRank.Two => 2,
                CardRank.Three => 3,
                CardRank.Four => 4,
                CardRank.Five => 5,
                CardRank.Six => 6,
                CardRank.Seven => 7,
                CardRank.Eight => 8,
                CardRank.Nine => 9,
                _ => 0 // 10, J, Q, K
            };
        }

        public override string ToString()
        {
            var suitSymbol = suit switch
            {
                CardSuit.Spades => "♠",
                CardSuit.Hearts => "♥",
                CardSuit.Clubs => "♣",
                CardSuit.Diamonds => "♦",
                _ => "?"
            };

            var rankName = rank switch
            {
                CardRank.Ace => "A",
                CardRank.Jack => "J",
                CardRank.Queen => "Q",
                CardRank.King => "K",
                _ => ((int)rank).ToString()
            };

            return $"{rankName}{suitSymbol}";
        }
    }

    #endregion

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
        Tie = 3          // 和局
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
            return betType.GetBetTypeName();
        }

        /// <summary>
        /// 计算净盈亏
        /// </summary>
        public decimal GetNetProfit()
        {
            return isWin ? winAmount - amount : -amount;
        }

        public override string ToString()
        {
            return $"Bet[{betType}, {amount}, {status}]";
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

            return $"{resultName} ({playerTotal} vs {bankerTotal})";
        }

        public override string ToString()
        {
            return $"Round[{gameNumber}, {mainResult}, {playerTotal}vs{bankerTotal}]";
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
        /// 获取投注类型显示名称
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
        /// 判断是否为主要投注类型
        /// </summary>
        public static bool IsMainBet(this BaccaratBetType betType)
        {
            return betType == BaccaratBetType.Player ||
                   betType == BaccaratBetType.Banker ||
                   betType == BaccaratBetType.Tie;
        }
    }

    #endregion
}
// Assets/Game/Entities/GameEntities.cs
// 游戏实体类合集 - 修复版，统一到Data命名空间
// 移除重复类型定义，避免命名空间循环引用
// 修复时间: 2025/6/23

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BaccaratGame.Data  // 修改：统一到Data命名空间
{
    /// <summary>
    /// 卡牌类 - 修复版
    /// 只包含牌面信息和基本计算功能，统一到Data命名空间避免循环引用
    /// </summary>
    [System.Serializable]
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

    /// <summary>
    /// 玩家类 - 简化版
    /// 只包含基本用户信息，投注功能由 BettingManager 处理
    /// </summary>
    [System.Serializable]
    public class Player
    {
        [SerializeField] private string userId = "";
        [SerializeField] private string nickname = "";
        [SerializeField] private decimal balance = 0m;
        [SerializeField] private string currency = "CNY";

        public string UserId => userId;
        public string Nickname => nickname;
        public decimal Balance => balance;
        public string Currency => currency;

        public Player() { }

        public Player(string userId, string nickname, decimal balance)
        {
            this.userId = userId ?? "";
            this.nickname = nickname ?? "";
            this.balance = Math.Max(0m, balance);
        }

        /// <summary>
        /// 更新余额
        /// </summary>
        public void UpdateBalance(decimal newBalance)
        {
            balance = Math.Max(0m, newBalance);
        }

        /// <summary>
        /// 检查余额是否足够
        /// </summary>
        public bool HasSufficientBalance(decimal amount)
        {
            return balance >= amount;
        }

        public override string ToString()
        {
            return $"{nickname} ({userId}) - {balance:F2} {currency}";
        }
    }

    // 移除：ChipData类型定义（已在其他地方定义，避免重复）
    // 移除：Hand类型定义（使用HandInfo替代，避免重复）

    /// <summary>
    /// 筹码数据类 - 保留此定义，确保与其他地方一致
    /// 注意：如果其他地方已有定义，应该移除此处的定义
    /// </summary>
    [System.Serializable]
    public class ChipData
    {
        public float val;           // 筹码面值
        public string text;         // 显示文本
        public bool enabled = true; // 是否启用

        public ChipData() { }

        public ChipData(float value, string displayText)
        {
            val = value;
            text = displayText;
            enabled = true;
        }

        public override string ToString()
        {
            return $"Chip[{val}, {text}, Enabled:{enabled}]";
        }
    }
}
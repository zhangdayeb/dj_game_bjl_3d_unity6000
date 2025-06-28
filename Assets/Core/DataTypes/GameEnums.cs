// Assets/Core/Data/Types/GameEnums.cs
// 游戏基础枚举定义 - 激进精简版
// 只保留立即需要的枚举类型

using System;

namespace BaccaratGame.Data
{
    #region 游戏状态枚举

    /// <summary>
    /// 游戏状态
    /// </summary>
    public enum GameState
    {
        Initializing = 0,    // 初始化中
        Ready = 1,           // 准备就绪  
        Playing = 2,         // 游戏中
        Disconnected = 4,    // 断开连接
        Ended = 5            // 游戏结束
    }

    /// <summary>
    /// 回合状态
    /// </summary>
    public enum RoundState
    {
        Idle = 0,       // 空闲状态
        Betting = 1,    // 投注阶段
        Dealing = 2,    // 发牌阶段
        Result = 3      // 结果阶段
    }

    /// <summary>
    /// 连接状态
    /// </summary>
    public enum ConnectionState
    {
        Disconnected = 0,  // 断开连接
        Connecting = 1,    // 连接中
        Connected = 2,     // 已连接
        Reconnecting = 3   // 重连中
    }

    #endregion

    #region 卡牌基础枚举

    /// <summary>
    /// 卡牌花色
    /// </summary>
    public enum CardSuit
    {
        Spades = 1,    // 黑桃 ♠
        Hearts = 2,    // 红桃 ♥
        Clubs = 3,     // 梅花 ♣
        Diamonds = 4   // 方块 ♦
    }

    /// <summary>
    /// 卡牌点数
    /// </summary>
    public enum CardRank
    {
        Ace = 1,    // A
        Two = 2,    // 2
        Three = 3,  // 3
        Four = 4,   // 4
        Five = 5,   // 5
        Six = 6,    // 6
        Seven = 7,  // 7
        Eight = 8,  // 8
        Nine = 9,   // 9
        Ten = 10,   // 10
        Jack = 11,  // J
        Queen = 12, // Q
        King = 13   // K
    }

    /// <summary>
    /// 手牌类型
    /// </summary>
    public enum HandType
    {
        Player,  // 闲家
        Banker   // 庄家
    }

    #endregion

    #region 扩展方法

    /// <summary>
    /// 枚举扩展方法
    /// </summary>
    public static class GameEnumExtensions
    {
        /// <summary>
        /// 获取游戏状态的显示名称
        /// </summary>
        public static string GetDisplayName(this GameState state)
        {
            return state switch
            {
                GameState.Initializing => "初始化中",
                GameState.Ready => "准备就绪",
                GameState.Playing => "游戏中",
                GameState.Disconnected => "连接断开",
                GameState.Ended => "游戏结束",
                _ => "未知状态"
            };
        }

        /// <summary>
        /// 获取回合状态的显示名称
        /// </summary>
        public static string GetDisplayName(this RoundState state)
        {
            return state switch
            {
                RoundState.Idle => "空闲",
                RoundState.Betting => "投注阶段",
                RoundState.Dealing => "发牌阶段",
                RoundState.Result => "结果阶段",
                _ => "未知状态"
            };
        }

        /// <summary>
        /// 获取连接状态的显示名称
        /// </summary>
        public static string GetDisplayName(this ConnectionState state)
        {
            return state switch
            {
                ConnectionState.Disconnected => "断开连接",
                ConnectionState.Connecting => "连接中",
                ConnectionState.Connected => "已连接",
                ConnectionState.Reconnecting => "重连中",
                _ => "未知状态"
            };
        }

        /// <summary>
        /// 判断是否为活跃的游戏状态
        /// </summary>
        public static bool IsActive(this GameState state)
        {
            return state == GameState.Ready || state == GameState.Playing;
        }

        /// <summary>
        /// 判断是否为投注状态
        /// </summary>
        public static bool IsBetting(this RoundState state)
        {
            return state == RoundState.Betting;
        }

        /// <summary>
        /// 判断是否已连接
        /// </summary>
        public static bool IsConnected(this ConnectionState state)
        {
            return state == ConnectionState.Connected;
        }
    }

    #endregion
}
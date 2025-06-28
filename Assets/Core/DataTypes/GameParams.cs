// Assets/Core/Data/Types/GameParams.cs
// 游戏参数和配置类型定义 - 激进精简版
// 只保留核心的启动参数和基本信息

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BaccaratGame.Data
{
    #region 游戏启动参数

    /// <summary>
    /// 游戏启动参数
    /// </summary>
    [Serializable]
    public class GameParams
    {
        [Header("必要参数")]
        public string table_id = "1";       // 桌台ID
        public string game_type = "3";       // 游戏类型 (3=百家乐)
        public string user_id = "8";         // 用户ID
        public string token = "9eb5fcdac259fd6cedacad3e04bacf2ed7M3m261WOCWcaAKFFa2Nu"; // 用户令牌

        [Header("可选参数")]
        public string language = "zh";       // 语言设置
        public string currency = "CNY";      // 货币类型

        public GameParams() { }

        public GameParams(string tableId, string gameType, string userId, string userToken)
        {
            table_id = tableId;
            game_type = gameType;
            user_id = userId;
            token = userToken;
        }

        /// <summary>
        /// 从URL查询字符串解析游戏参数
        /// </summary>
        public static GameParams ParseFromUrl()
        {
            var gameParams = new GameParams();
            
            try
            {
                #if UNITY_WEBGL && !UNITY_EDITOR
                string queryString = "";
                if (Application.absoluteURL.Contains("?"))
                {
                    queryString = Application.absoluteURL.Split('?')[1];
                }
                
                if (!string.IsNullOrEmpty(queryString))
                {
                    var parameters = ParseQueryString(queryString);
                    
                    gameParams.table_id = parameters.GetValueOrDefault("table_id", "1");
                    gameParams.game_type = parameters.GetValueOrDefault("game_type", "3");
                    gameParams.user_id = parameters.GetValueOrDefault("user_id", "8");
                    gameParams.token = parameters.GetValueOrDefault("token", "9eb5fcdac259fd6cedacad3e04bacf2ed7M3m261WOCWcaAKFFa2Nu");
                    gameParams.language = parameters.GetValueOrDefault("language", "zh");
                    gameParams.currency = parameters.GetValueOrDefault("currency", "CNY");
                }
                #endif
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GameParams] URL解析失败，使用默认参数: {ex.Message}");
            }
            
            return gameParams;
        }

        private static Dictionary<string, string> ParseQueryString(string queryString)
        {
            var parameters = new Dictionary<string, string>();
            
            var pairs = queryString.Split('&');
            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    var key = Uri.UnescapeDataString(keyValue[0]);
                    var value = Uri.UnescapeDataString(keyValue[1]);
                    parameters[key] = value;
                }
            }
            
            return parameters;
        }

        /// <summary>
        /// 验证参数完整性
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(table_id) &&
                   !string.IsNullOrEmpty(game_type) &&
                   !string.IsNullOrEmpty(user_id) &&
                   !string.IsNullOrEmpty(token);
        }

        public override string ToString()
        {
            var tokenPreview = string.IsNullOrEmpty(token) ? "null" : 
                              token.Length > 8 ? token.Substring(0, 8) + "..." : token;
            
            return $"GameParams[Table:{table_id}, User:{user_id}, Token:{tokenPreview}]";
        }
    }

    #endregion

    #region 用户信息

    /// <summary>
    /// 用户信息
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        public string user_id = "";
        public string username = "";
        public string nickname = "";
        public decimal balance = 0m;
        public string currency = "CNY";

        public UserInfo() { }

        public UserInfo(string userId, string username, decimal balance)
        {
            user_id = userId;
            this.username = username;
            this.balance = balance;
        }

        /// <summary>
        /// 检查是否可以投注指定金额
        /// </summary>
        public bool CanBet(decimal amount)
        {
            return balance >= amount && amount > 0;
        }

        /// <summary>
        /// 更新余额
        /// </summary>
        public void UpdateBalance(decimal newBalance)
        {
            balance = Math.Max(0m, newBalance);
        }

        public override string ToString()
        {
            return $"User[{user_id}:{username}, Balance:{balance}{currency}]";
        }
    }

    #endregion

    #region 桌台信息

    /// <summary>
    /// 桌台信息
    /// </summary>
    [Serializable]
    public class TableInfo
    {
        [Header("基本信息")]
        public int id = 0;
        public string table_name = "";
        public int game_type = 3;

        [Header("投注限额")]
        public decimal min_bet = 10m;
        public decimal max_bet = 10000m;

        [Header("当前状态")]
        public string current_game_number = "";
        public GameState game_state = GameState.Ready;
        public RoundState round_state = RoundState.Idle;
        public int countdown = 0;

        public TableInfo() { }

        public TableInfo(int tableId, string tableName)
        {
            id = tableId;
            table_name = tableName;
        }

        /// <summary>
        /// 检查是否可以投注
        /// </summary>
        public bool CanPlaceBet()
        {
            return round_state == RoundState.Betting && 
                   game_state == GameState.Playing &&
                   countdown > 0;
        }

        /// <summary>
        /// 检查投注金额是否在限额内
        /// </summary>
        public bool IsValidBetAmount(decimal amount)
        {
            return amount >= min_bet && amount <= max_bet;
        }

        /// <summary>
        /// 更新桌台状态
        /// </summary>
        public void UpdateStatus(GameState gameState, RoundState roundState, int newCountdown)
        {
            game_state = gameState;
            round_state = roundState;
            countdown = newCountdown;
        }

        public override string ToString()
        {
            return $"Table[{id}:{table_name}, {game_state}, Countdown:{countdown}]";
        }
    }

    #endregion
}
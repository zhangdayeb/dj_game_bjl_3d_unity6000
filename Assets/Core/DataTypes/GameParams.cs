// Assets/Core/Data/Types/GameParams.cs
// 游戏参数和配置类型定义 - 简化版
// 只保留核心的启动参数和用户信息
// 创建时间: 2025/6/22

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
        public string table_id = "";     // 桌台ID
        public string game_type = "3";   // 游戏类型 (3=百家乐)
        public string user_id = "";      // 用户ID
        public string token = "";        // 用户令牌

        [Header("可选参数")]
        public string language = "zh";   // 语言设置
        public string currency = "CNY";  // 货币类型
        public bool debug_mode = false;  // 调试模式

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
        public static GameParams ParseFromUrl(string queryString = null)
        {
            var gameParams = new GameParams();
            
            if (string.IsNullOrEmpty(queryString))
            {
                #if UNITY_WEBGL && !UNITY_EDITOR
                queryString = GetCurrentUrlQuery();
                #endif
            }
            
            if (string.IsNullOrEmpty(queryString))
                return gameParams;

            if (queryString.StartsWith("?"))
                queryString = queryString.Substring(1);

            var parameters = ParseQueryString(queryString);
            
            gameParams.table_id = parameters.GetValueOrDefault("table_id", "");
            gameParams.game_type = parameters.GetValueOrDefault("game_type", "3");
            gameParams.user_id = parameters.GetValueOrDefault("user_id", "");
            gameParams.token = parameters.GetValueOrDefault("token", "");
            gameParams.language = parameters.GetValueOrDefault("language", "zh");
            gameParams.currency = parameters.GetValueOrDefault("currency", "CNY");
            gameParams.debug_mode = parameters.GetValueOrDefault("debug", "false").ToLower() == "true";

            return gameParams;
        }

        private static string GetCurrentUrlQuery()
        {
            try
            {
                #if UNITY_WEBGL && !UNITY_EDITOR
                return Application.absoluteURL.Split('?').Length > 1 ? 
                       Application.absoluteURL.Split('?')[1] : "";
                #else
                return "";
                #endif
            }
            catch
            {
                return "";
            }
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

        /// <summary>
        /// 是否为百家乐游戏
        /// </summary>
        public bool IsBaccarat()
        {
            return game_type == "3";
        }

        public override string ToString()
        {
            var tokenPreview = string.IsNullOrEmpty(token) ? "null" : 
                              token.Length > 8 ? token.Substring(0, 8) + "..." : token;
            
            return $"GameParams[TableId={table_id}, GameType={game_type}, UserId={user_id}, Token={tokenPreview}]";
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
        [Header("基本信息")]
        public string user_id = "";
        public string username = "";
        public string nickname = "";
        public string avatar_url = "";

        [Header("余额信息")]
        public decimal balance = 0m;
        public string currency = "CNY";
        public decimal available_balance = 0m;

        [Header("权限设置")]
        public bool can_bet = true;
        public bool is_vip = false;
        public string account_status = "active";

        public UserInfo() { }

        public UserInfo(string userId, string username, decimal balance)
        {
            user_id = userId;
            this.username = username;
            this.balance = balance;
            this.available_balance = balance;
        }

        /// <summary>
        /// 检查是否可以投注指定金额
        /// </summary>
        public bool CanBet(decimal amount)
        {
            return can_bet && 
                   account_status == "active" && 
                   available_balance >= amount && 
                   amount > 0;
        }

        /// <summary>
        /// 更新余额
        /// </summary>
        public void UpdateBalance(decimal newBalance)
        {
            balance = newBalance;
            available_balance = newBalance;
        }

        /// <summary>
        /// 验证用户信息完整性
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(user_id) &&
                   !string.IsNullOrEmpty(username) &&
                   balance >= 0;
        }

        public override string ToString()
        {
            return $"UserInfo[{user_id}:{username}, Balance:{balance}{currency}]";
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

        [Header("玩家统计")]
        public int total_players = 0;

        [Header("视频流")]
        public string video_near = "";
        public string video_far = "";

        [Header("投注限额")]
        public decimal min_bet = 10m;
        public decimal max_bet = 10000m;

        [Header("当前状态")]
        public string current_game_number = "";
        public GameState game_state = GameState.Ready;
        public RoundState round_state = RoundState.Idle;
        public int countdown = 0;
        public bool can_bet = false;

        public TableInfo() { }

        public TableInfo(int tableId, string tableName, int gameType)
        {
            id = tableId;
            table_name = tableName;
            game_type = gameType;
        }

        /// <summary>
        /// 获取游戏类型描述
        /// </summary>
        public string GetGameTypeDescription()
        {
            return game_type switch
            {
                2 => "龙虎",
                3 => "百家乐",
                _ => "未知游戏"
            };
        }

        /// <summary>
        /// 检查是否可以投注
        /// </summary>
        public bool CanPlaceBet()
        {
            return can_bet && 
                   round_state == RoundState.Betting && 
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
            can_bet = CanPlaceBet();
        }

        /// <summary>
        /// 验证桌台信息完整性
        /// </summary>
        public bool IsValid()
        {
            return id > 0 &&
                   !string.IsNullOrEmpty(table_name) &&
                   game_type > 0 &&
                   min_bet > 0 &&
                   max_bet > min_bet;
        }

        public override string ToString()
        {
            return $"TableInfo[{id}:{table_name}, {GetGameTypeDescription()}, Players:{total_players}]";
        }
    }

    #endregion
}
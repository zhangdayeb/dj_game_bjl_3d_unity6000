// Assets/Core/Data/Types/NetworkTypes.cs
// 网络数据类型定义 - 精简版，移除重复定义，统一引用标准数据类型
// 包含所有网络消息的数据结构定义
// 创建时间: 2025/6/22

using System;
using System.Collections.Generic;
using UnityEngine;
using BaccaratGame.Data;  // 统一引用标准数据类型

namespace BaccaratGame.Data
{
    #region 基础消息类型

    /// <summary>
    /// 基础消息类
    /// </summary>
    [Serializable]
    public abstract class BaseMessage
    {
        public string messageType = "";     // 消息类型
        public string messageId = "";       // 消息ID
        public DateTime timestamp = DateTime.Now; // 时间戳
        public int version = 1;             // 协议版本
    }

    /// <summary>
    /// 响应消息基类
    /// </summary>
    [Serializable]
    public abstract class BaseResponse : BaseMessage
    {
        public bool success = false;        // 是否成功
        public int errorCode = 0;          // 错误代码
        public string errorMessage = "";   // 错误消息
        
        /// <summary>
        /// 获取完整错误信息
        /// </summary>
        public string GetFullErrorMessage()
        {
            return string.IsNullOrEmpty(errorMessage) ? $"错误代码: {errorCode}" : errorMessage;
        }
    }

    #endregion

    #region 游戏消息类型

    /// <summary>
    /// 倒计时消息
    /// </summary>
    [Serializable]
    public class CountdownMessage : BaseMessage
    {
        public int countdown = 0;           // 当前倒计时（秒）
        public int game_phase = 1;          // 游戏阶段
        public string bureau_number = "";   // 游戏局号
        public int table_id = 0;           // 桌台ID
        public bool can_bet = false;       // 是否可以投注

        public string GetPhaseDescription()
        {
            return game_phase switch
            {
                1 => "投注阶段",
                2 => "发牌阶段",
                3 => "结算阶段",
                _ => "未知阶段"
            };
        }
    }

    /// <summary>
    /// 游戏结果消息
    /// </summary>
    [Serializable]
    public class GameResultMessage : BaseMessage
    {
        public string bureau_number = "";        // 游戏局号
        public int table_id = 0;                // 桌台ID
        public List<int> player_cards = new List<int>();  // 闲家卡牌
        public List<int> banker_cards = new List<int>();  // 庄家卡牌
        public int player_total = 0;            // 闲家总点数
        public int banker_total = 0;            // 庄家总点数
        public int winner = 0;                  // 获胜方 (1=庄, 2=闲, 3=和)
        public bool player_pair = false;        // 是否闲对
        public bool banker_pair = false;        // 是否庄对
        public List<int> special_results = new List<int>(); // 特殊结果列表

        /// <summary>
        /// 转换为RoundResult - 使用标准RoundResult类型
        /// </summary>
        public RoundResult ToRoundResult()
        {
            var result = new RoundResult
            {
                gameNumber = bureau_number,
                playerTotal = player_total,
                bankerTotal = banker_total,
                hasPlayerPair = player_pair,
                hasBankerPair = banker_pair
            };

            // 转换卡牌
            foreach (int cardId in player_cards)
            {
                result.playerCards.Add(new Card(cardId));
            }
            
            foreach (int cardId in banker_cards)
            {
                result.bankerCards.Add(new Card(cardId));
            }

            // 确定主要结果
            result.mainResult = winner switch
            {
                1 => BaccaratResult.Banker,
                2 => BaccaratResult.Player,
                3 => BaccaratResult.Tie,
                _ => BaccaratResult.None
            };

            // 分析特殊结果
            AnalyzeSpecialResults(result, special_results);

            return result;
        }

        /// <summary>
        /// 分析特殊结果
        /// </summary>
        private void AnalyzeSpecialResults(RoundResult result, List<int> specialResults)
        {
            foreach (int specialResult in specialResults)
            {
                switch (specialResult)
                {
                    case 6: // 幸运6
                        result.isLucky6 = true;
                        break;
                    case 7: // 龙7
                        result.isDragon7 = true;
                        break;
                    case 8: // 熊8
                        result.isPanda8 = true;
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 回合开始消息
    /// </summary>
    [Serializable]
    public class RoundStartMessage : BaseMessage
    {
        public string round_id = "";           // 回合ID
        public int round_number = 0;          // 回合序号
        public int table_id = 0;              // 桌台ID
        public string dealer_id = "";         // 荷官ID
        public float betting_duration = 30f;  // 投注时长
        public DateTime start_time = DateTime.Now; // 开始时间
    }

    /// <summary>
    /// 投注消息
    /// </summary>
    [Serializable]
    public class BetMessage : BaseMessage
    {
        public string bet_id = "";            // 投注ID
        public int bet_type = 0;              // 投注类型
        public decimal amount = 0m;           // 投注金额
        public string round_id = "";          // 回合ID
        public string player_id = "";         // 玩家ID

        /// <summary>
        /// 转换为BetInfo - 使用标准BetInfo类型
        /// </summary>
        public BetInfo ToBetInfo()
        {
            return new BetInfo
            {
                betId = bet_id,
                betType = (BaccaratBetType)bet_type,
                amount = amount,
                gameNumber = round_id,
                status = BetStatus.Pending
            };
        }
    }

    /// <summary>
    /// 投注响应消息
    /// </summary>
    [Serializable]
    public class BetResponse : BaseResponse
    {
        public string bet_id = "";            // 投注ID
        public decimal balance = 0m;          // 剩余余额
        public BetStatus status = BetStatus.Pending; // 投注状态
    }

    #endregion

    #region 用户相关消息

    /// <summary>
    /// 登录消息
    /// </summary>
    [Serializable]
    public class LoginMessage : BaseMessage
    {
        public string username = "";          // 用户名
        public string password = "";          // 密码（应该是加密后的）
        public string device_id = "";         // 设备ID
        public string client_version = "";    // 客户端版本
    }

    /// <summary>
    /// 登录响应 - 使用标准UserInfo类型
    /// </summary>
    [Serializable]
    public class LoginResponse : BaseResponse
    {
        public string user_id = "";           // 用户ID
        public string session_token = "";     // 会话令牌
        public decimal balance = 0m;          // 账户余额
        public UserInfo user_info = new UserInfo(); // 用户信息 - 使用标准类型
    }

    /// <summary>
    /// 余额查询响应
    /// </summary>
    [Serializable]
    public class BalanceResponse : BaseResponse
    {
        public decimal balance = 0m;          // 当前余额
        public decimal frozen_balance = 0m;   // 冻结余额
        public DateTime last_updated = DateTime.Now; // 最后更新时间
    }

    #endregion

    #region 桌台相关消息

    /// <summary>
    /// 加入桌台消息
    /// </summary>
    [Serializable]
    public class JoinTableMessage : BaseMessage
    {
        public int table_id = 0;              // 桌台ID
        public string user_id = "";           // 用户ID
    }

    /// <summary>
    /// 加入桌台响应 - 使用标准TableInfo类型
    /// </summary>
    [Serializable]
    public class JoinTableResponse : BaseResponse
    {
        public TableInfo table_info = new TableInfo(); // 桌台信息 - 使用标准类型
        public List<PlayerInfo> players = new List<PlayerInfo>(); // 玩家列表
    }

    /// <summary>
    /// 网络专用玩家信息 - 与标准UserInfo区分
    /// </summary>
    [Serializable]
    public class PlayerInfo
    {
        public string user_id = "";           // 用户ID
        public string username = "";          // 用户名
        public string nickname = "";          // 昵称
        public decimal balance = 0m;          // 余额
        public int seat_number = 0;           // 座位号
        public bool is_ready = false;         // 是否准备
        public DateTime join_time = DateTime.Now; // 加入时间
    }

    #endregion

    #region 历史记录消息

    /// <summary>
    /// 历史记录查询消息
    /// </summary>
    [Serializable]
    public class HistoryQueryMessage : BaseMessage
    {
        public string user_id = "";           // 用户ID
        public DateTime start_date = DateTime.Today; // 开始日期
        public DateTime end_date = DateTime.Now;     // 结束日期
        public int page_number = 1;           // 页码
        public int page_size = 20;            // 每页大小
        public HistoryType history_type = HistoryType.BetHistory; // 历史类型
    }

    /// <summary>
    /// 历史记录响应
    /// </summary>
    [Serializable]
    public class HistoryResponse : BaseResponse
    {
        public List<BetHistoryRecord> bet_records = new List<BetHistoryRecord>(); // 投注记录
        public List<GameHistoryRecord> game_records = new List<GameHistoryRecord>(); // 游戏记录
        public int total_count = 0;           // 总记录数
        public int page_number = 1;           // 当前页
        public int total_pages = 1;           // 总页数
    }

    /// <summary>
    /// 投注历史记录
    /// </summary>
    [Serializable]
    public class BetHistoryRecord
    {
        public string bet_id = "";            // 投注ID
        public string round_id = "";          // 回合ID
        public BaccaratBetType bet_type = BaccaratBetType.Player; // 投注类型
        public decimal bet_amount = 0m;       // 投注金额
        public decimal win_amount = 0m;       // 赢得金额
        public bool is_win = false;           // 是否中奖
        public DateTime bet_time = DateTime.Now; // 投注时间
        public BetStatus status = BetStatus.Settled; // 投注状态
    }

    /// <summary>
    /// 游戏历史记录
    /// </summary>
    [Serializable]
    public class GameHistoryRecord
    {
        public string round_id = "";          // 回合ID
        public int round_number = 0;          // 回合序号
        public BaccaratResult result = BaccaratResult.None; // 游戏结果
        public List<int> player_cards = new List<int>(); // 闲家卡牌
        public List<int> banker_cards = new List<int>(); // 庄家卡牌
        public int player_total = 0;          // 闲家总点数
        public int banker_total = 0;          // 庄家总点数
        public bool has_player_pair = false;  // 是否有闲对
        public bool has_banker_pair = false;  // 是否有庄对
        public DateTime game_time = DateTime.Now; // 游戏时间
    }

    /// <summary>
    /// 历史记录类型
    /// </summary>
    public enum HistoryType
    {
        BetHistory,    // 投注历史
        GameHistory,   // 游戏历史
        WinLossHistory // 输赢历史
    }

    #endregion

    #region 系统消息

    /// <summary>
    /// 心跳消息
    /// </summary>
    [Serializable]
    public class HeartbeatMessage : BaseMessage
    {
        public long client_timestamp = 0;     // 客户端时间戳
    }

    /// <summary>
    /// 心跳响应
    /// </summary>
    [Serializable]
    public class HeartbeatResponse : BaseResponse
    {
        public long server_timestamp = 0;     // 服务器时间戳
        public long client_timestamp = 0;     // 客户端时间戳（回传）
        public int ping = 0;                  // 延迟（毫秒）
    }

    /// <summary>
    /// 系统公告消息
    /// </summary>
    [Serializable]
    public class AnnouncementMessage : BaseMessage
    {
        public string announcement_id = "";   // 公告ID
        public string title = "";             // 标题
        public string content = "";           // 内容
        public AnnouncementType type = AnnouncementType.Info; // 公告类型
        public DateTime start_time = DateTime.Now;           // 开始时间
        public DateTime end_time = DateTime.MaxValue;        // 结束时间
        public bool is_popup = false;         // 是否弹窗显示
    }

    /// <summary>
    /// 公告类型
    /// </summary>
    public enum AnnouncementType
    {
        Info,      // 信息
        Warning,   // 警告
        Maintenance, // 维护
        Event      // 活动
    }

    /// <summary>
    /// 错误消息
    /// </summary>
    [Serializable]
    public class ErrorMessage : BaseMessage
    {
        public int error_code = 0;            // 错误代码
        public string error_message = "";     // 错误消息
        public string error_details = "";     // 错误详情
        public ErrorSeverity severity = ErrorSeverity.Low; // 使用GameEnums中的定义
    }

    #endregion

    #region 网络状态

    /// <summary>
    /// 网络连接状态（与GameEnums.ConnectionState区分）
    /// </summary>
    public enum NetworkConnectionState
    {
        Disconnected,  // 已断开
        Connecting,    // 连接中
        Connected,     // 已连接
        Reconnecting,  // 重连中
        Failed         // 连接失败
    }

    /// <summary>
    /// 连接信息
    /// </summary>
    [Serializable]
    public class NetworkConnectionInfo
    {
        public string server_address = "";    // 服务器地址
        public int server_port = 0;          // 服务器端口
        public NetworkConnectionState state = NetworkConnectionState.Disconnected; // 连接状态
        public DateTime connection_time = DateTime.Now; // 连接时间
        public int retry_count = 0;          // 重试次数
        public int ping = 0;                 // 延迟
        public string connection_id = "";     // 连接ID
    }

    #endregion

    #region 消息转换扩展方法

    /// <summary>
    /// 网络消息转换扩展方法
    /// </summary>
    public static class NetworkMessageExtensions
    {
        /// <summary>
        /// 将网络投注消息转换为标准投注信息
        /// </summary>
        public static BetInfo ToBetInfo(this BetMessage betMessage)
        {
            return new BetInfo
            {
                betId = betMessage.bet_id,
                betType = (BaccaratBetType)betMessage.bet_type,
                amount = betMessage.amount,
                gameNumber = betMessage.round_id,
                status = BetStatus.Pending
            };
        }

        /// <summary>
        /// 将标准投注信息转换为网络投注消息
        /// </summary>
        public static BetMessage ToBetMessage(this BetInfo betInfo)
        {
            return new BetMessage
            {
                bet_id = betInfo.betId,
                bet_type = (int)betInfo.betType,
                amount = betInfo.amount,
                round_id = betInfo.gameNumber,
                messageType = "bet_place"
            };
        }

        /// <summary>
        /// 将网络游戏结果转换为标准回合结果
        /// </summary>
        public static RoundResult ToRoundResult(this GameResultMessage gameResult)
        {
            return gameResult.ToRoundResult();
        }
    }

    #endregion
}
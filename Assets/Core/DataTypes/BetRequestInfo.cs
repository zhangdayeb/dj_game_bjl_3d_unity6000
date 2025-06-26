// Assets/Core/DataTypes/BetRequestInfo.cs
// 投注请求相关数据类型定义
// 用于UI事件系统中的投注请求事件

using System;
using UnityEngine;
using BaccaratGame.Data;

namespace BaccaratGame.Data
{
    /// <summary>
    /// 投注请求信息
    /// 用于UI事件系统，表示用户发起的投注请求
    /// </summary>
    [Serializable]
    public class BetRequestInfo
    {
        [Header("基本投注信息")]
        public string requestId = "";                    // 请求ID，用于跟踪请求
        public BaccaratBetType betType = BaccaratBetType.Player; // 投注类型
        public decimal amount = 0m;                      // 投注金额
        public string userId = "";                       // 用户ID
        public string gameNumber = "";                   // 游戏局号
        
        [Header("请求状态")]
        public BetRequestStatus status = BetRequestStatus.Pending; // 请求状态
        public DateTime requestTime = DateTime.Now;      // 请求时间
        public string errorMessage = "";                 // 错误消息（如果有）
        
        [Header("验证信息")]
        public bool isValidated = false;                 // 是否已验证
        public string validationMessage = "";            // 验证消息
        public decimal userBalance = 0m;                 // 用户余额（验证时使用）
        
        [Header("UI相关")]
        public Vector3 sourcePosition = Vector3.zero;    // 投注发起位置（UI坐标）
        public string sourceComponentId = "";            // 发起投注的UI组件ID
        
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public BetRequestInfo()
        {
            requestId = Guid.NewGuid().ToString();
            requestTime = DateTime.Now;
        }
        
        /// <summary>
        /// 基本构造函数
        /// </summary>
        /// <param name="betType">投注类型</param>
        /// <param name="amount">投注金额</param>
        public BetRequestInfo(BaccaratBetType betType, decimal amount)
        {
            this.requestId = Guid.NewGuid().ToString();
            this.betType = betType;
            this.amount = amount;
            this.requestTime = DateTime.Now;
        }
        
        /// <summary>
        /// 完整构造函数
        /// </summary>
        /// <param name="betType">投注类型</param>
        /// <param name="amount">投注金额</param>
        /// <param name="userId">用户ID</param>
        /// <param name="gameNumber">游戏局号</param>
        public BetRequestInfo(BaccaratBetType betType, decimal amount, string userId, string gameNumber)
        {
            this.requestId = Guid.NewGuid().ToString();
            this.betType = betType;
            this.amount = amount;
            this.userId = userId;
            this.gameNumber = gameNumber;
            this.requestTime = DateTime.Now;
        }
        
        /// <summary>
        /// 验证投注请求
        /// </summary>
        /// <param name="userBalance">用户余额</param>
        /// <param name="minBet">最小投注额</param>
        /// <param name="maxBet">最大投注额</param>
        /// <returns>验证是否成功</returns>
        public bool Validate(decimal userBalance, decimal minBet, decimal maxBet)
        {
            this.userBalance = userBalance;
            
            // 检查投注金额是否有效
            if (amount <= 0)
            {
                validationMessage = "投注金额必须大于0";
                isValidated = false;
                return false;
            }
            
            // 检查是否在投注范围内
            if (amount < minBet)
            {
                validationMessage = $"投注金额不能小于最小投注额 {minBet}";
                isValidated = false;
                return false;
            }
            
            if (amount > maxBet)
            {
                validationMessage = $"投注金额不能大于最大投注额 {maxBet}";
                isValidated = false;
                return false;
            }
            
            // 检查余额是否足够
            if (amount > userBalance)
            {
                validationMessage = $"余额不足，当前余额：{userBalance}";
                isValidated = false;
                return false;
            }
            
            // 验证通过
            validationMessage = "验证通过";
            isValidated = true;
            return true;
        }
        
        /// <summary>
        /// 设置请求状态
        /// </summary>
        /// <param name="status">新状态</param>
        /// <param name="message">状态消息</param>
        public void SetStatus(BetRequestStatus status, string message = "")
        {
            this.status = status;
            if (!string.IsNullOrEmpty(message))
            {
                if (status == BetRequestStatus.Failed || status == BetRequestStatus.ValidationFailed)
                {
                    this.errorMessage = message;
                }
                else
                {
                    this.validationMessage = message;
                }
            }
        }
        
        /// <summary>
        /// 转换为BetInfo对象
        /// </summary>
        /// <returns>BetInfo实例</returns>
        public BetInfo ToBetInfo()
        {
            return new BetInfo(betType, amount, gameNumber)
            {
                betId = requestId,
                status = BetStatus.Pending
            };
        }
        
        /// <summary>
        /// 获取投注类型显示名称
        /// </summary>
        /// <returns>显示名称</returns>
        public string GetBetTypeDisplayName()
        {
            return betType.GetBetTypeName();
        }
        
        /// <summary>
        /// 检查是否可以执行投注
        /// </summary>
        /// <returns>是否可以投注</returns>
        public bool CanExecute()
        {
            return isValidated && 
                   status == BetRequestStatus.Pending && 
                   amount > 0 && 
                   !string.IsNullOrEmpty(userId) && 
                   !string.IsNullOrEmpty(gameNumber);
        }
        
        /// <summary>
        /// 获取请求摘要信息
        /// </summary>
        /// <returns>摘要字符串</returns>
        public string GetSummary()
        {
            return $"{GetBetTypeDisplayName()} {amount}元 [{status}]";
        }
        
        /// <summary>
        /// 克隆投注请求
        /// </summary>
        /// <returns>克隆的实例</returns>
        public BetRequestInfo Clone()
        {
            return new BetRequestInfo
            {
                requestId = Guid.NewGuid().ToString(), // 生成新的请求ID
                betType = this.betType,
                amount = this.amount,
                userId = this.userId,
                gameNumber = this.gameNumber,
                status = BetRequestStatus.Pending, // 重置状态
                requestTime = DateTime.Now, // 更新时间
                errorMessage = "",
                isValidated = false, // 重置验证状态
                validationMessage = "",
                userBalance = this.userBalance,
                sourcePosition = this.sourcePosition,
                sourceComponentId = this.sourceComponentId
            };
        }
        
        public override string ToString()
        {
            return $"BetRequest[{requestId[..8]}, {betType}, {amount}, {status}, Valid:{isValidated}]";
        }
    }
    
    /// <summary>
    /// 投注请求状态枚举
    /// </summary>
    public enum BetRequestStatus
    {
        Pending = 0,            // 待处理
        Validating = 1,         // 验证中
        ValidationFailed = 2,   // 验证失败
        Validated = 3,          // 验证通过
        Processing = 4,         // 处理中
        Completed = 5,          // 完成
        Failed = 6,             // 失败
        Cancelled = 7           // 已取消
    }
    
    /// <summary>
    /// 投注请求状态扩展方法
    /// </summary>
    public static class BetRequestStatusExtensions
    {
        /// <summary>
        /// 获取状态显示名称
        /// </summary>
        public static string GetDisplayName(this BetRequestStatus status)
        {
            return status switch
            {
                BetRequestStatus.Pending => "待处理",
                BetRequestStatus.Validating => "验证中",
                BetRequestStatus.ValidationFailed => "验证失败",
                BetRequestStatus.Validated => "验证通过",
                BetRequestStatus.Processing => "处理中",
                BetRequestStatus.Completed => "完成",
                BetRequestStatus.Failed => "失败",
                BetRequestStatus.Cancelled => "已取消",
                _ => "未知状态"
            };
        }
        
        /// <summary>
        /// 判断是否为最终状态
        /// </summary>
        public static bool IsFinalStatus(this BetRequestStatus status)
        {
            return status == BetRequestStatus.Completed ||
                   status == BetRequestStatus.Failed ||
                   status == BetRequestStatus.Cancelled;
        }
        
        /// <summary>
        /// 判断是否为错误状态
        /// </summary>
        public static bool IsErrorStatus(this BetRequestStatus status)
        {
            return status == BetRequestStatus.ValidationFailed ||
                   status == BetRequestStatus.Failed;
        }
    }
}
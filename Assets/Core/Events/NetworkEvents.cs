// Assets/Core/Events/NetworkEvents.cs
// 简化版网络事件定义 - 只处理3个核心WebSocket事件
using System;
using UnityEngine;

namespace BaccaratGame.Core.Events
{
    /// <summary>
    /// 简化版网络事件定义
    /// 只处理倒计时、开牌信息、开奖信息三个核心事件
    /// </summary>
    public static class NetworkEvents
    {
        #region WebSocket事件

        /// <summary>
        /// 倒计时消息接收事件
        /// </summary>
        public static event Action<string> OnCountdownReceived;

        /// <summary>
        /// 开牌信息消息接收事件
        /// </summary>
        public static event Action<string> OnDealCardsReceived;

        /// <summary>
        /// 开奖信息消息接收事件
        /// </summary>
        public static event Action<string> OnGameResultReceived;

        #endregion

        #region 事件触发方法

        /// <summary>
        /// 触发倒计时消息接收事件
        /// </summary>
        /// <param name="message">消息内容</param>
        public static void TriggerCountdownReceived(string message)
        {
            Debug.Log($"[倒计时] 收到消息: {message}");
            OnCountdownReceived?.Invoke(message);
        }

        /// <summary>
        /// 触发开牌信息消息接收事件
        /// </summary>
        /// <param name="message">消息内容</param>
        public static void TriggerDealCardsReceived(string message)
        {
            Debug.Log($"[开牌信息] 收到消息: {message}");
            OnDealCardsReceived?.Invoke(message);
        }

        /// <summary>
        /// 触发开奖信息消息接收事件
        /// </summary>
        /// <param name="message">消息内容</param>
        public static void TriggerGameResultReceived(string message)
        {
            Debug.Log($"[开奖信息] 收到消息: {message}");
            OnGameResultReceived?.Invoke(message);
        }

        #endregion

        #region 事件清理

        /// <summary>
        /// 清除所有事件订阅
        /// </summary>
        public static void ClearAllSubscriptions()
        {
            OnCountdownReceived = null;
            OnDealCardsReceived = null;
            OnGameResultReceived = null;
        }

        #endregion

        #region 示例使用方法

        /// <summary>
        /// 示例：如何订阅和使用这些事件
        /// </summary>
        public static void ExampleUsage()
        {
            // 订阅事件
            OnCountdownReceived += HandleCountdown;
            OnDealCardsReceived += HandleDealCards;
            OnGameResultReceived += HandleGameResult;
        }

        // 示例事件处理方法
        private static void HandleCountdown(string message)
        {
            // 处理倒计时消息
            Debug.Log($"处理倒计时: {message}");
        }

        private static void HandleDealCards(string message)
        {
            // 处理开牌信息
            Debug.Log($"处理开牌信息: {message}");
        }

        private static void HandleGameResult(string message)
        {
            // 处理开奖信息
            Debug.Log($"处理开奖信息: {message}");
        }

        #endregion
    }
}
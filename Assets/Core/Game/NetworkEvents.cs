// Assets/Core/Events/NetworkEvents.cs
// 网络事件定义 - 处理WebSocket消息事件
// 负责业务实现和事件触发

using System;
using UnityEngine;

namespace BaccaratGame.Core
{
    /// <summary>
    /// 网络事件定义
    /// 处理倒计时、开牌信息、开奖信息三个核心事件
    /// 负责业务逻辑实现
    /// </summary>
    public static class NetworkEvents
    {
        #region WebSocket事件

        /// <summary>
        /// 倒计时消息接收事件
        /// </summary>
        public static event Action<CountdownData> OnCountdownReceived;

        /// <summary>
        /// 开牌信息消息接收事件
        /// </summary>
        public static event Action<string> OnDealCardsReceived;

        /// <summary>
        /// 开奖信息消息接收事件
        /// </summary>
        public static event Action<string> OnGameResultReceived;

        #endregion

        #region 数据结构

        /// <summary>
        /// 倒计时数据结构
        /// </summary>
        [Serializable]
        public class CountdownData
        {
            public int remainingTime;    // 剩余时间（秒）
            public string phase;         // 阶段："betting" 或 "dealing"
            public bool isCountdownEnd;  // 是否倒计时结束
            
            public CountdownData(int time, string gamePhase)
            {
                remainingTime = time;
                phase = gamePhase;
                isCountdownEnd = time <= 0;
            }
        }

        #endregion

        #region 事件触发方法

        /// <summary>
        /// 触发倒计时消息接收事件
        /// </summary>
        /// <param name="message">原始消息内容</param>
        public static void TriggerCountdownReceived(string message)
        {
            Debug.Log($"[NetworkEvents] 收到倒计时消息: {message}");
            
            try
            {
                // 解析倒计时数据
                var countdownData = ParseCountdownMessage(message);
                
                if (countdownData != null)
                {
                    Debug.Log($"[NetworkEvents] 倒计时解析成功 - 剩余时间: {countdownData.remainingTime}秒, 阶段: {countdownData.phase}");
                    OnCountdownReceived?.Invoke(countdownData);
                }
                else
                {
                    Debug.LogWarning("[NetworkEvents] 倒计时消息解析失败");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkEvents] 倒计时消息处理异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 触发开牌信息消息接收事件
        /// </summary>
        /// <param name="message">消息内容</param>
        public static void TriggerDealCardsReceived(string message)
        {
            Debug.Log($"[NetworkEvents] 收到开牌消息: {message}");
            OnDealCardsReceived?.Invoke(message);
        }

        /// <summary>
        /// 触发开奖信息消息接收事件
        /// </summary>
        /// <param name="message">消息内容</param>
        public static void TriggerGameResultReceived(string message)
        {
            Debug.Log($"[NetworkEvents] 收到中奖消息: {message}");
            OnGameResultReceived?.Invoke(message);
        }

        #endregion

        #region 消息解析业务逻辑

        /// <summary>
        /// 解析倒计时消息
        /// 从JSON消息中提取倒计时相关数据
        /// </summary>
        /// <param name="message">原始JSON消息</param>
        /// <returns>解析后的倒计时数据</returns>
        private static CountdownData ParseCountdownMessage(string message)
        {
            try
            {
                // 简单的JSON解析，提取关键字段
                int remainingTime = ExtractIntField(message, "time", 0);
                int countdown = ExtractIntField(message, "countdown", remainingTime);
                
                // 优先使用countdown字段，如果没有则使用time字段
                int finalTime = countdown > 0 ? countdown : remainingTime;
                
                // 判断游戏阶段
                string phase = finalTime > 0 ? "betting" : "dealing";
                
                Debug.Log($"[NetworkEvents] 倒计时解析 - time: {remainingTime}, countdown: {countdown}, 最终时间: {finalTime}, 阶段: {phase}");
                
                return new CountdownData(finalTime, phase);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkEvents] 倒计时消息解析异常: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 从JSON消息中提取整数字段
        /// </summary>
        /// <param name="jsonMessage">JSON消息</param>
        /// <param name="fieldName">字段名</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>提取的整数值</returns>
        private static int ExtractIntField(string jsonMessage, string fieldName, int defaultValue = 0)
        {
            try
            {
                string searchPattern = $"\"{fieldName}\"";
                int fieldIndex = jsonMessage.IndexOf(searchPattern);
                if (fieldIndex == -1) return defaultValue;

                int colonIndex = jsonMessage.IndexOf(":", fieldIndex);
                if (colonIndex == -1) return defaultValue;

                int valueStart = colonIndex + 1;
                while (valueStart < jsonMessage.Length && 
                       (jsonMessage[valueStart] == ' ' || jsonMessage[valueStart] == '"'))
                    valueStart++;

                int valueEnd = valueStart;
                while (valueEnd < jsonMessage.Length && 
                       char.IsDigit(jsonMessage[valueEnd]))
                    valueEnd++;

                if (valueEnd > valueStart)
                {
                    string valueStr = jsonMessage.Substring(valueStart, valueEnd - valueStart);
                    if (int.TryParse(valueStr, out int result))
                    {
                        return result;
                    }
                }
                
                return defaultValue;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[NetworkEvents] 提取字段 {fieldName} 失败: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// 从JSON消息中提取字符串字段
        /// </summary>
        /// <param name="jsonMessage">JSON消息</param>
        /// <param name="fieldName">字段名</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>提取的字符串值</returns>
        private static string ExtractStringField(string jsonMessage, string fieldName, string defaultValue = "")
        {
            try
            {
                string searchPattern = $"\"{fieldName}\"";
                int fieldIndex = jsonMessage.IndexOf(searchPattern);
                if (fieldIndex == -1) return defaultValue;

                int colonIndex = jsonMessage.IndexOf(":", fieldIndex);
                if (colonIndex == -1) return defaultValue;

                int valueStart = colonIndex + 1;
                while (valueStart < jsonMessage.Length && 
                       (jsonMessage[valueStart] == ' ' || jsonMessage[valueStart] == '"'))
                    valueStart++;

                int valueEnd = valueStart;
                while (valueEnd < jsonMessage.Length && 
                       jsonMessage[valueEnd] != '"' && 
                       jsonMessage[valueEnd] != ',' && 
                       jsonMessage[valueEnd] != '}')
                    valueEnd++;

                return jsonMessage.Substring(valueStart, valueEnd - valueStart).Trim();
            }
            catch
            {
                return defaultValue;
            }
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
            
            Debug.Log("[NetworkEvents] 所有事件订阅已清理");
        }

        #endregion

        #region 调试方法

        /// <summary>
        /// 模拟倒计时消息（用于测试）
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void SimulateCountdownMessage(int time)
        {
            string mockMessage = $"{{\"code\":200,\"msg\":\"倒计时信息\",\"data\":{{\"countdown\":{time}}}}}";
            TriggerCountdownReceived(mockMessage);
        }

        /// <summary>
        /// 模拟开牌消息（用于测试）
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void SimulateDealCardsMessage()
        {
            string mockMessage = "{\"code\":200,\"msg\":\"开牌信息\",\"data\":{\"cards\":[]}}";
            TriggerDealCardsReceived(mockMessage);
        }

        /// <summary>
        /// 模拟中奖消息（用于测试）
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void SimulateGameResultMessage()
        {
            string mockMessage = "{\"code\":200,\"msg\":\"中奖信息\",\"data\":{\"result\":\"win\"}}";
            TriggerGameResultReceived(mockMessage);
        }

        #endregion
    }
}
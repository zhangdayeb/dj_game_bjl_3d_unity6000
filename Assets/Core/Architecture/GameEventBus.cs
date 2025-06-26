// Assets/Core/Architecture/GameEventBus.cs
// 游戏事件总线 - 重构版，扩展事件系统功能
// 支持强类型事件分发，事件优先级，统计和调试功能

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BaccaratGame.Core.Events;
using BaccaratGame.Data;

namespace BaccaratGame.Core.Architecture
{
    /// <summary>
    /// 游戏事件总线
    /// 提供强类型事件分发、原始消息处理、事件优先级等功能
    /// </summary>
    public static class GameEventBus
    {
        #region 私有字段

        // 强类型事件订阅
        private static Dictionary<Type, List<Delegate>> _typedSubscribers = new Dictionary<Type, List<Delegate>>();
        
        // 原始消息订阅（兼容现有WebSocket消息）
        private static Dictionary<string, List<Action<string>>> _messageSubscribers = new Dictionary<string, List<Action<string>>>();
        
        // 事件优先级订阅
        private static Dictionary<Type, SortedDictionary<int, List<Delegate>>> _prioritySubscribers = 
            new Dictionary<Type, SortedDictionary<int, List<Delegate>>>();
        
        // 调试和统计
        private static bool _enableDebugLog = false;
        private static EventBusStatistics _statistics = new EventBusStatistics();

        // 事件过滤器和中间件
        private static Dictionary<Type, List<Func<object, bool>>> _eventFilters = new Dictionary<Type, List<Func<object, bool>>>();
        private static Dictionary<Type, List<Func<object, object>>> _eventMiddlewares = new Dictionary<Type, List<Func<object, object>>>();

        #endregion

        #region 强类型事件发布

        /// <summary>
        /// 发布强类型事件
        /// </summary>
        public static void Publish<T>(T eventData) where T : class
        {
            if (eventData == null)
            {
                Debug.LogWarning("[GameEventBus] Cannot publish null event data");
                return;
            }

            var eventType = typeof(T);
            
            if (_enableDebugLog)
            {
                Debug.Log($"[GameEventBus] Publishing event: {eventType.Name}");
            }

            _statistics.totalEventsPublished++;
            _statistics.eventTypeStats[eventType.Name] = _statistics.eventTypeStats.GetValueOrDefault(eventType.Name, 0) + 1;

            // 应用事件过滤器
            if (!ApplyEventFilters(eventData))
            {
                if (_enableDebugLog)
                {
                    Debug.Log($"[GameEventBus] Event filtered out: {eventType.Name}");
                }
                return;
            }

            // 应用事件中间件
            var processedEventData = ApplyEventMiddlewares(eventData);

            // 发布给普通订阅者
            if (_typedSubscribers.TryGetValue(eventType, out var subscribers))
            {
                PublishToSubscribers(subscribers, processedEventData, eventType.Name);
            }

            // 发布给优先级订阅者
            if (_prioritySubscribers.TryGetValue(eventType, out var prioritySubscribers))
            {
                foreach (var priorityLevel in prioritySubscribers.Keys.OrderByDescending(p => p))
                {
                    var prioritySubscriberList = prioritySubscribers[priorityLevel];
                    PublishToSubscribers(prioritySubscriberList, processedEventData, $"{eventType.Name}(Priority:{priorityLevel})");
                }
            }
        }

        /// <summary>
        /// 异步发布事件
        /// </summary>
        public static void PublishAsync<T>(T eventData) where T : class
        {
            if (eventData == null)
            {
                Debug.LogWarning("[GameEventBus] Cannot publish null event data asynchronously");
                return;
            }

            // 在下一帧发布事件
            CoroutineHelper.StartCoroutineStatic(PublishNextFrame(eventData));
        }

        private static System.Collections.IEnumerator PublishNextFrame<T>(T eventData) where T : class
        {
            yield return null;
            Publish(eventData);
        }

        private static void PublishToSubscribers<T>(List<Delegate> subscribers, T eventData, string eventName)
        {
            if (subscribers == null || subscribers.Count == 0)
                return;

            var subscribersCopy = new List<Delegate>(subscribers); // 避免迭代时修改集合

            foreach (var subscriber in subscribersCopy)
            {
                try
                {
                    if (subscriber is Action<T> action)
                    {
                        action.Invoke(eventData);
                        _statistics.successfulNotifications++;
                    }
                    else
                    {
                        Debug.LogWarning($"[GameEventBus] Invalid subscriber type for {eventName}");
                        _statistics.failedNotifications++;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[GameEventBus] Error in event subscriber for {eventName}: {ex.Message}");
                    _statistics.failedNotifications++;
                }
            }
        }

        #endregion

        #region 强类型事件订阅

        /// <summary>
        /// 订阅强类型事件
        /// </summary>
        public static void Subscribe<T>(Action<T> handler) where T : class
        {
            if (handler == null)
            {
                Debug.LogWarning("[GameEventBus] Cannot subscribe with null handler");
                return;
            }

            var eventType = typeof(T);
            
            if (!_typedSubscribers.ContainsKey(eventType))
            {
                _typedSubscribers[eventType] = new List<Delegate>();
            }
            
            _typedSubscribers[eventType].Add(handler);
            _statistics.totalSubscriptions++;
            
            if (_enableDebugLog)
            {
                Debug.Log($"[GameEventBus] Subscribed to event: {eventType.Name}");
            }
        }

        /// <summary>
        /// 订阅带优先级的强类型事件
        /// </summary>
        public static void Subscribe<T>(Action<T> handler, int priority) where T : class
        {
            if (handler == null)
            {
                Debug.LogWarning("[GameEventBus] Cannot subscribe with null handler");
                return;
            }

            var eventType = typeof(T);
            
            if (!_prioritySubscribers.ContainsKey(eventType))
            {
                _prioritySubscribers[eventType] = new SortedDictionary<int, List<Delegate>>();
            }
            
            if (!_prioritySubscribers[eventType].ContainsKey(priority))
            {
                _prioritySubscribers[eventType][priority] = new List<Delegate>();
            }
            
            _prioritySubscribers[eventType][priority].Add(handler);
            _statistics.totalSubscriptions++;
            
            if (_enableDebugLog)
            {
                Debug.Log($"[GameEventBus] Subscribed to event: {eventType.Name} with priority: {priority}");
            }
        }

        /// <summary>
        /// 取消订阅强类型事件
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : class
        {
            if (handler == null)
            {
                Debug.LogWarning("[GameEventBus] Cannot unsubscribe with null handler");
                return;
            }

            var eventType = typeof(T);
            bool unsubscribed = false;
            
            if (_typedSubscribers.TryGetValue(eventType, out var subscribers))
            {
                if (subscribers.Remove(handler))
                {
                    _statistics.totalUnsubscriptions++;
                    unsubscribed = true;
                    
                    if (subscribers.Count == 0)
                    {
                        _typedSubscribers.Remove(eventType);
                    }
                }
            }
            
            // 也检查优先级订阅
            if (_prioritySubscribers.TryGetValue(eventType, out var prioritySubscribers))
            {
                foreach (var priorityLevel in prioritySubscribers.Keys.ToList())
                {
                    if (prioritySubscribers[priorityLevel].Remove(handler))
                    {
                        if (prioritySubscribers[priorityLevel].Count == 0)
                        {
                            prioritySubscribers.Remove(priorityLevel);
                        }
                        _statistics.totalUnsubscriptions++;
                        unsubscribed = true;
                        break;
                    }
                }
                
                if (prioritySubscribers.Count == 0)
                {
                    _prioritySubscribers.Remove(eventType);
                }
            }

            if (unsubscribed && _enableDebugLog)
            {
                Debug.Log($"[GameEventBus] Unsubscribed from event: {eventType.Name}");
            }
        }

        #endregion

        #region 原始消息处理（兼容现有WebSocket）

        /// <summary>
        /// 处理原始WebSocket消息
        /// </summary>
        public static void ProcessRawMessage(string rawMessage)
        {
            if (string.IsNullOrEmpty(rawMessage))
            {
                Debug.LogWarning("[GameEventBus] Received empty message");
                return;
            }

            try
            {
                _statistics.totalRawMessagesProcessed++;
                
                if (_enableDebugLog)
                {
                    var preview = rawMessage.Length > 100 ? rawMessage.Substring(0, 100) + "..." : rawMessage;
                    Debug.Log($"[GameEventBus] Processing raw message: {preview}");
                }

                // 解析消息类型
                var messageType = ParseMessageType(rawMessage);
                if (string.IsNullOrEmpty(messageType))
                {
                    Debug.LogWarning($"[GameEventBus] Cannot parse message type from: {rawMessage.Substring(0, Math.Min(50, rawMessage.Length))}...");
                    _statistics.failedMessageProcessing++;
                    return;
                }

                // 分发给原始消息订阅者
                PublishRawMessage(messageType, rawMessage);
                
                // 尝试转换为强类型事件
                TryConvertToTypedEvent(messageType, rawMessage);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameEventBus] Error processing raw message: {ex.Message}");
                _statistics.failedMessageProcessing++;
            }
        }

        /// <summary>
        /// 订阅原始消息
        /// </summary>
        public static void Subscribe(string messageType, Action<string> handler)
        {
            if (string.IsNullOrEmpty(messageType) || handler == null)
            {
                Debug.LogWarning("[GameEventBus] Invalid subscription parameters");
                return;
            }

            var normalizedType = messageType.ToLower();
            
            if (!_messageSubscribers.ContainsKey(normalizedType))
            {
                _messageSubscribers[normalizedType] = new List<Action<string>>();
            }
            
            _messageSubscribers[normalizedType].Add(handler);
            _statistics.totalSubscriptions++;
            
            if (_enableDebugLog)
            {
                Debug.Log($"[GameEventBus] Subscribed to raw message: {messageType}");
            }
        }

        /// <summary>
        /// 取消订阅原始消息
        /// </summary>
        public static void Unsubscribe(string messageType, Action<string> handler)
        {
            if (string.IsNullOrEmpty(messageType) || handler == null)
            {
                Debug.LogWarning("[GameEventBus] Invalid unsubscription parameters");
                return;
            }

            var normalizedType = messageType.ToLower();
            
            if (_messageSubscribers.TryGetValue(normalizedType, out var subscribers))
            {
                if (subscribers.Remove(handler))
                {
                    _statistics.totalUnsubscriptions++;
                    
                    if (subscribers.Count == 0)
                    {
                        _messageSubscribers.Remove(normalizedType);
                    }
                    
                    if (_enableDebugLog)
                    {
                        Debug.Log($"[GameEventBus] Unsubscribed from raw message: {messageType}");
                    }
                }
            }
        }

        private static void PublishRawMessage(string messageType, string rawMessage)
        {
            var normalizedType = messageType.ToLower();
            
            if (_messageSubscribers.TryGetValue(normalizedType, out var subscribers))
            {
                var subscribersCopy = new List<Action<string>>(subscribers);
                
                foreach (var subscriber in subscribersCopy)
                {
                    try
                    {
                        subscriber.Invoke(rawMessage);
                        _statistics.successfulNotifications++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[GameEventBus] Error in raw message subscriber for {messageType}: {ex.Message}");
                        _statistics.failedNotifications++;
                    }
                }
            }
        }

        private static string ParseMessageType(string rawMessage)
        {
            try
            {
                var message = JsonUtility.FromJson<BaseMessage>(rawMessage);
                return message?.type?.ToLower();
            }
            catch
            {
                return ExtractTypeFromJson(rawMessage);
            }
        }

        private static string ExtractTypeFromJson(string json)
        {
            try
            {
                var typeIndex = json.IndexOf("\"type\"");
                if (typeIndex == -1) return null;

                var colonIndex = json.IndexOf(":", typeIndex);
                if (colonIndex == -1) return null;

                var valueStart = colonIndex + 1;
                while (valueStart < json.Length && (json[valueStart] == ' ' || json[valueStart] == '\t' || json[valueStart] == '"'))
                    valueStart++;

                var valueEnd = valueStart;
                while (valueEnd < json.Length && json[valueEnd] != '"' && json[valueEnd] != ',' && json[valueEnd] != '}')
                    valueEnd++;

                return json.Substring(valueStart, valueEnd - valueStart).Trim().ToLower();
            }
            catch
            {
                return null;
            }
        }

        private static void TryConvertToTypedEvent(string messageType, string rawMessage)
        {
            // 这里可以添加消息类型到强类型事件的转换逻辑
            // 例如：countdown -> CountdownEvent, game_result -> GameResultEvent
            
            try
            {
                switch (messageType.ToLower())
                {
                    case "countdown":
                        var countdownMsg = JsonUtility.FromJson<CountdownMessage>(rawMessage);
                        if (countdownMsg != null)
                        {
                            Publish(countdownMsg);
                        }
                        break;
                        
                    case "game_result":
                        var gameResultMsg = JsonUtility.FromJson<GameResultMessage>(rawMessage);
                        if (gameResultMsg != null)
                        {
                            Publish(gameResultMsg);
                        }
                        break;
                        
                    case "bet_response":
                        var betResponseMsg = JsonUtility.FromJson<BetResponse>(rawMessage);
                        if (betResponseMsg != null)
                        {
                            Publish(betResponseMsg);
                        }
                        break;
                        
                    default:
                        // 未知消息类型，保持原始字符串处理
                        break;
                }
            }
            catch (Exception ex)
            {
                if (_enableDebugLog)
                {
                    Debug.LogWarning($"[GameEventBus] Failed to convert raw message to typed event: {ex.Message}");
                }
            }
        }

        #endregion

        #region 批量事件处理

        /// <summary>
        /// 批量发布事件
        /// </summary>
        public static void PublishBatch<T>(IEnumerable<T> events) where T : class
        {
            if (events == null)
            {
                Debug.LogWarning("[GameEventBus] Cannot publish null events collection");
                return;
            }

            foreach (var eventData in events)
            {
                if (eventData != null)
                {
                    Publish(eventData);
                }
            }
        }

        /// <summary>
        /// 延迟发布事件
        /// </summary>
        public static void PublishDelayed<T>(T eventData, float delaySeconds) where T : class
        {
            if (eventData == null)
            {
                Debug.LogWarning("[GameEventBus] Cannot publish null event data with delay");
                return;
            }

            CoroutineHelper.StartCoroutineStatic(PublishWithDelay(eventData, delaySeconds));
        }

        private static System.Collections.IEnumerator PublishWithDelay<T>(T eventData, float delaySeconds) where T : class
        {
            yield return new UnityEngine.WaitForSeconds(delaySeconds);
            Publish(eventData);
        }

        #endregion

        #region 事件过滤和中间件

        /// <summary>
        /// 添加事件过滤器
        /// </summary>
        public static void AddEventFilter<T>(Func<T, bool> filter) where T : class
        {
            if (filter == null)
            {
                Debug.LogWarning("[GameEventBus] Cannot add null event filter");
                return;
            }

            var eventType = typeof(T);
            
            if (!_eventFilters.ContainsKey(eventType))
            {
                _eventFilters[eventType] = new List<Func<object, bool>>();
            }
            
            _eventFilters[eventType].Add(obj => filter((T)obj));
            
            if (_enableDebugLog)
            {
                Debug.Log($"[GameEventBus] Added event filter for: {eventType.Name}");
            }
        }

        /// <summary>
        /// 添加事件中间件
        /// </summary>
        public static void AddEventMiddleware<T>(Func<T, T> middleware) where T : class
        {
            if (middleware == null)
            {
                Debug.LogWarning("[GameEventBus] Cannot add null event middleware");
                return;
            }

            var eventType = typeof(T);
            
            if (!_eventMiddlewares.ContainsKey(eventType))
            {
                _eventMiddlewares[eventType] = new List<Func<object, object>>();
            }
            
            _eventMiddlewares[eventType].Add(obj => middleware((T)obj));
            
            if (_enableDebugLog)
            {
                Debug.Log($"[GameEventBus] Added event middleware for: {eventType.Name}");
            }
        }

        private static bool ApplyEventFilters<T>(T eventData) where T : class
        {
            var eventType = typeof(T);
            
            if (_eventFilters.TryGetValue(eventType, out var filters))
            {
                foreach (var filter in filters)
                {
                    try
                    {
                        if (!filter(eventData))
                        {
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[GameEventBus] Error in event filter for {eventType.Name}: {ex.Message}");
                    }
                }
            }
            
            return true;
        }

        private static T ApplyEventMiddlewares<T>(T eventData) where T : class
        {
            var eventType = typeof(T);
            var processedData = eventData;
            
            if (_eventMiddlewares.TryGetValue(eventType, out var middlewares))
            {
                foreach (var middleware in middlewares)
                {
                    try
                    {
                        processedData = (T)middleware(processedData);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[GameEventBus] Error in event middleware for {eventType.Name}: {ex.Message}");
                    }
                }
            }
            
            return processedData;
        }

        #endregion

        #region 管理和调试

        /// <summary>
        /// 清除所有订阅
        /// </summary>
        public static void ClearAllSubscriptions()
        {
            var totalSubscriptions = GetTotalSubscriptions();
            
            _typedSubscribers.Clear();
            _messageSubscribers.Clear();
            _prioritySubscribers.Clear();
            _eventFilters.Clear();
            _eventMiddlewares.Clear();
            
            Debug.Log($"[GameEventBus] Cleared all subscriptions: {totalSubscriptions}");
        }

        /// <summary>
        /// 获取统计信息
        /// </summary>
        public static EventBusStatistics GetStatistics()
        {
            _statistics.currentSubscriptions = GetTotalSubscriptions();
            _statistics.messageTypeCount = _messageSubscribers.Count;
            _statistics.eventTypeCount = _typedSubscribers.Count;
            
            return _statistics;
        }

        /// <summary>
        /// 打印状态信息
        /// </summary>
        public static void PrintStatus()
        {
            var stats = GetStatistics();
            
            Debug.Log($"[GameEventBus] ==== Event Bus Status ====");
            Debug.Log($"[GameEventBus] Current Subscriptions: {stats.currentSubscriptions}");
            Debug.Log($"[GameEventBus] Message Types: {stats.messageTypeCount}");
            Debug.Log($"[GameEventBus] Event Types: {stats.eventTypeCount}");
            Debug.Log($"[GameEventBus] Total Events Published: {stats.totalEventsPublished}");
            Debug.Log($"[GameEventBus] Total Raw Messages: {stats.totalRawMessagesProcessed}");
            Debug.Log($"[GameEventBus] Successful Notifications: {stats.successfulNotifications}");
            Debug.Log($"[GameEventBus] Failed Notifications: {stats.failedNotifications}");
            Debug.Log($"[GameEventBus] Failed Message Processing: {stats.failedMessageProcessing}");
        }

        /// <summary>
        /// 设置调试日志
        /// </summary>
        public static void SetDebugLogEnabled(bool enabled)
        {
            _enableDebugLog = enabled;
            Debug.Log($"[GameEventBus] Debug logging {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// 重置统计信息
        /// </summary>
        public static void ResetStatistics()
        {
            _statistics = new EventBusStatistics();
            Debug.Log("[GameEventBus] Statistics reset");
        }

        /// <summary>
        /// 获取事件类型的订阅者数量
        /// </summary>
        public static int GetSubscriberCount<T>() where T : class
        {
            var eventType = typeof(T);
            int count = 0;
            
            if (_typedSubscribers.TryGetValue(eventType, out var subscribers))
            {
                count += subscribers.Count;
            }
            
            if (_prioritySubscribers.TryGetValue(eventType, out var prioritySubscribers))
            {
                foreach (var priorityLevel in prioritySubscribers.Values)
                {
                    count += priorityLevel.Count;
                }
            }
            
            return count;
        }

        private static int GetTotalSubscriptions()
        {
            int total = 0;
            
            foreach (var subscribers in _typedSubscribers.Values)
            {
                total += subscribers.Count;
            }
            
            foreach (var subscribers in _messageSubscribers.Values)
            {
                total += subscribers.Count;
            }
            
            foreach (var priorityGroups in _prioritySubscribers.Values)
            {
                foreach (var subscribers in priorityGroups.Values)
                {
                    total += subscribers.Count;
                }
            }
            
            return total;
        }

        #endregion

        #region 数据结构

        [Serializable]
        private class BaseMessage
        {
            public string type;
        }

        [Serializable]
        public class EventBusStatistics
        {
            public int currentSubscriptions;
            public int totalSubscriptions;
            public int totalUnsubscriptions;
            public int messageTypeCount;
            public int eventTypeCount;
            public int totalEventsPublished;
            public int totalRawMessagesProcessed;
            public int successfulNotifications;
            public int failedNotifications;
            public int failedMessageProcessing;
            public Dictionary<string, int> eventTypeStats = new Dictionary<string, int>();
        }

        #endregion
    }

    #region 辅助类

    /// <summary>
    /// 协程辅助类 - 修复版
    /// </summary>
    public class CoroutineHelper : MonoBehaviour
    {
        private static CoroutineHelper _instance;

        public static CoroutineHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("CoroutineHelper");
                    _instance = go.AddComponent<CoroutineHelper>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>
        /// 静态协程启动方法 - 避免与MonoBehaviour.StartCoroutine冲突
        /// </summary>
        public static Coroutine StartCoroutineStatic(System.Collections.IEnumerator routine)
        {
            if (routine == null)
            {
                Debug.LogWarning("[CoroutineHelper] Cannot start null coroutine");
                return null;
            }

            return Instance.StartCoroutine(routine);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }

    #endregion
}
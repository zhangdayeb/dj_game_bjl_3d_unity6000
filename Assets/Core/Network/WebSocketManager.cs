// Assets/Core/Network/WebSocketManager.cs
// WebSocket管理器 - 简化版库文件
// 传统单例模式，无需手动挂载，直接代码调用

using System;
using System.Collections;
using UnityEngine;
using NativeWebSocket;
using BaccaratGame.Core;
using System.Text;

namespace BaccaratGame.Core
{
    /// <summary>
    /// WebSocket管理器 - 简化版
    /// 作为库文件使用，支持自动连接、重连、心跳和消息处理
    /// </summary>
    public class WebSocketManager : MonoBehaviour
    {
        #region 单例模式

        private static WebSocketManager _instance;
        
        /// <summary>
        /// 获取WebSocketManager单例实例
        /// 第一次访问时自动创建GameObject和组件
        /// </summary>
        public static WebSocketManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    // 自动创建GameObject和组件
                    GameObject go = new GameObject("WebSocketManager");
                    _instance = go.AddComponent<WebSocketManager>();
                    DontDestroyOnLoad(go);
                    Debug.Log("[WebSocketManager] 库实例已自动创建");
                }
                return _instance;
            }
        }

        #endregion

        #region 配置参数

        // WebSocket连接
        private WebSocket _websocket;
        private string _currentUrl = "";
        
        // 连接状态
        private bool _isConnected = false;
        private bool _shouldReconnect = true;
        
        // 重连配置 (5秒间隔，5次重连)
        private const int MAX_RECONNECT_ATTEMPTS = 5;
        private const float RECONNECT_DELAY = 5f;
        private int _reconnectAttempts = 0;
        private Coroutine _reconnectCoroutine;
        
        // 心跳配置 (30秒间隔)
        private const float HEARTBEAT_INTERVAL = 30f;
        private Coroutine _heartbeatCoroutine;

        #endregion

        #region 公共属性

        /// <summary>
        /// 是否已连接到WebSocket服务器
        /// </summary>
        public bool IsConnected => _isConnected && _websocket?.State == WebSocketState.Open;

        /// <summary>
        /// 当前连接的URL
        /// </summary>
        public string CurrentUrl => _currentUrl;

        /// <summary>
        /// 当前重连尝试次数
        /// </summary>
        public int ReconnectAttempts => _reconnectAttempts;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            // 确保单例唯一性
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[WebSocketManager] 单例初始化完成");
            }
            else if (_instance != this)
            {
                Debug.LogWarning("[WebSocketManager] 检测到重复实例，已销毁");
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            // NativeWebSocket需要在主线程处理消息队列
            #if !UNITY_WEBGL || UNITY_EDITOR
            _websocket?.DispatchMessageQueue();
            #endif
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            // 应用暂停时处理连接
            if (pauseStatus && IsConnected)
            {
                Debug.Log("[WebSocketManager] 应用暂停，保持连接");
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            _shouldReconnect = false;
            
            if (_websocket != null)
            {
                _websocket.Close();
                _websocket = null;
            }
            
            Debug.Log("[WebSocketManager] 实例已销毁");
        }

        #endregion

        #region 连接管理

        /// <summary>
        /// 连接到WebSocket服务器
        /// </summary>
        /// <param name="url">WebSocket服务器地址</param>
        public async void Connect(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("[WebSocketManager] URL不能为空");
                return;
            }

            if (_isConnected && _currentUrl == url)
            {
                Debug.Log("[WebSocketManager] 已连接到相同地址，无需重复连接");
                return;
            }

            _currentUrl = url;
            _shouldReconnect = true;
            _reconnectAttempts = 0;

            await ConnectInternal();
        }

        /// <summary>
        /// 断开WebSocket连接
        /// </summary>
        public async void Disconnect()
        {
            _shouldReconnect = false;
            
            StopHeartbeat();
            StopReconnect();
            
            if (_websocket != null)
            {
                try
                {
                    await _websocket.Close();
                    Debug.Log("[WebSocketManager] 主动断开连接");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[WebSocketManager] 断开连接时异常: {ex.Message}");
                }
                finally
                {
                    _websocket = null;
                    _isConnected = false;
                }
            }
        }

        /// <summary>
        /// 内部连接方法
        /// </summary>
        private async System.Threading.Tasks.Task ConnectInternal()
        {
            try
            {
                Debug.Log($"[WebSocketManager] 正在连接: {_currentUrl}");

                // 清理旧连接
                if (_websocket != null)
                {
                    await _websocket.Close();
                    _websocket = null;
                }

                // 创建新连接
                _websocket = new WebSocket(_currentUrl);
                
                // 设置事件处理
                _websocket.OnOpen += OnConnected;
                _websocket.OnMessage += OnMessageReceived;
                _websocket.OnError += OnError;
                _websocket.OnClose += OnDisconnected;

                // 开始连接
                await _websocket.Connect();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketManager] 连接失败: {ex.Message}");
                _isConnected = false;
                
                if (_shouldReconnect)
                {
                    StartReconnect();
                }
            }
        }

        #endregion

        #region WebSocket事件处理

        /// <summary>
        /// WebSocket连接成功
        /// </summary>
        private void OnConnected()
        {
            _isConnected = true;
            _reconnectAttempts = 0;
            
            Debug.Log("[WebSocketManager] ✅ 连接成功");
            
            // 开始心跳
            StartHeartbeat();
        }

        /// <summary>
        /// 收到WebSocket消息
        /// </summary>
        private void OnMessageReceived(byte[] data)
        {
            try
            {
                string message = Encoding.UTF8.GetString(data);
                Debug.Log($"[WebSocketManager] 📨 收到消息: {message}");

                // 解析并分发消息
                ProcessMessage(message);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketManager] 处理消息异常: {ex.Message}");
            }
        }

        /// <summary>
        /// WebSocket连接错误
        /// </summary>
        private void OnError(string error)
        {
            Debug.LogError($"[WebSocketManager] ❌ 连接错误: {error}");
            _isConnected = false;
            
            if (_shouldReconnect)
            {
                StartReconnect();
            }
        }

        /// <summary>
        /// WebSocket连接断开
        /// </summary>
        private void OnDisconnected(WebSocketCloseCode closeCode)
        {
            _isConnected = false;
            
            Debug.LogWarning($"[WebSocketManager] 🔌 连接断开: {closeCode}");
            
            StopHeartbeat();
            
            // 非正常断开时启动重连
            if (_shouldReconnect && closeCode != WebSocketCloseCode.Normal)
            {
                StartReconnect();
            }
        }

        #endregion

        #region 消息处理

        /// <summary>
        /// 处理收到的消息并分发到对应事件
        /// </summary>
        private void ProcessMessage(string message)
        {
            try
            {
                // 简单的JSON解析，提取type字段
                string messageType = ExtractMessageType(message);
                
                if (string.IsNullOrEmpty(messageType))
                {
                    Debug.LogWarning($"[WebSocketManager] 无法解析消息类型: {message}");
                    return;
                }

                // 忽略心跳响应
                if (messageType == "pong")
                {
                    return;
                }

                // 分发到对应的NetworkEvents
                switch (messageType.ToLower())
                {
                    case "countdown":
                        NetworkEvents.TriggerCountdownReceived(message);
                        break;
                        
                    case "deal_cards":
                    case "deal":
                        NetworkEvents.TriggerDealCardsReceived(message);
                        break;
                        
                    case "game_result":
                    case "result":
                        NetworkEvents.TriggerGameResultReceived(message);
                        break;
                        
                    default:
                        Debug.Log($"[WebSocketManager] 未处理的消息类型: {messageType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketManager] 消息处理异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 从JSON消息中提取type字段
        /// </summary>
        private string ExtractMessageType(string jsonMessage)
        {
            try
            {
                // 简单的字符串匹配提取type字段
                int typeIndex = jsonMessage.IndexOf("\"type\"");
                if (typeIndex == -1) return null;

                int colonIndex = jsonMessage.IndexOf(":", typeIndex);
                if (colonIndex == -1) return null;

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
                return null;
            }
        }

        /// <summary>
        /// 发送消息到WebSocket服务器
        /// </summary>
        public async void SendMessage(object data)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[WebSocketManager] 未连接，无法发送消息");
                return;
            }

            try
            {
                string jsonMessage = JsonUtility.ToJson(data);
                Debug.Log($"[WebSocketManager] 📤 发送消息: {jsonMessage}");
                
                await _websocket.SendText(jsonMessage);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketManager] 发送消息失败: {ex.Message}");
            }
        }

        #endregion

        #region 心跳机制

        /// <summary>
        /// 开始心跳检测
        /// </summary>
        private void StartHeartbeat()
        {
            StopHeartbeat();
            _heartbeatCoroutine = StartCoroutine(HeartbeatLoop());
            Debug.Log("[WebSocketManager] 💓 开始心跳检测");
        }

        /// <summary>
        /// 停止心跳检测
        /// </summary>
        private void StopHeartbeat()
        {
            if (_heartbeatCoroutine != null)
            {
                StopCoroutine(_heartbeatCoroutine);
                _heartbeatCoroutine = null;
            }
        }

        /// <summary>
        /// 心跳循环协程
        /// </summary>
        private IEnumerator HeartbeatLoop()
        {
            while (IsConnected)
            {
                yield return new WaitForSeconds(HEARTBEAT_INTERVAL);

                if (IsConnected)
                {
                    // 发送心跳ping
                    var pingData = new { type = "ping", timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() };
                    SendMessage(pingData);
                }
            }
        }

        #endregion

        #region 重连机制

        /// <summary>
        /// 开始重连
        /// </summary>
        private void StartReconnect()
        {
            if (_reconnectCoroutine != null) return;
            
            _reconnectCoroutine = StartCoroutine(ReconnectLoop());
        }

        /// <summary>
        /// 停止重连
        /// </summary>
        private void StopReconnect()
        {
            if (_reconnectCoroutine != null)
            {
                StopCoroutine(_reconnectCoroutine);
                _reconnectCoroutine = null;
            }
        }

        /// <summary>
        /// 重连循环协程
        /// </summary>
        private IEnumerator ReconnectLoop()
        {
            while (_reconnectAttempts < MAX_RECONNECT_ATTEMPTS && _shouldReconnect && !IsConnected)
            {
                _reconnectAttempts++;
                
                Debug.Log($"[WebSocketManager] 🔄 第{_reconnectAttempts}/{MAX_RECONNECT_ATTEMPTS}次重连...");
                
                yield return new WaitForSeconds(RECONNECT_DELAY);
                
                if (!_shouldReconnect) break;
                
                var connectTask = ConnectInternal();
                yield return new WaitUntil(() => connectTask.IsCompleted);
                
                if (IsConnected)
                {
                    Debug.Log("[WebSocketManager] ✅ 重连成功");
                    break;
                }
            }

            if (_reconnectAttempts >= MAX_RECONNECT_ATTEMPTS)
            {
                Debug.LogError("[WebSocketManager] ❌ 重连失败，已达到最大重试次数");
            }

            _reconnectCoroutine = null;
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 手动触发重连
        /// </summary>
        public void Reconnect()
        {
            if (!string.IsNullOrEmpty(_currentUrl))
            {
                _reconnectAttempts = 0;
                Connect(_currentUrl);
            }
        }

        /// <summary>
        /// 获取连接状态信息
        /// </summary>
        public string GetStatusInfo()
        {
            return $"URL: {_currentUrl} | 连接: {IsConnected} | 重连次数: {_reconnectAttempts}/{MAX_RECONNECT_ATTEMPTS}";
        }

        #endregion
    }
}
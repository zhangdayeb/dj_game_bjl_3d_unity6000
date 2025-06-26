// Assets/Core/Network/WebSocketManager.cs
// WebSocket管理器 - Unity内置版本，移除NativeWebSocket依赖
// 使用Unity内置的WebGL JavaScript互操作实现WebSocket功能

using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using System.Runtime.InteropServices;
using BaccaratGame.Core.Architecture;

namespace Core.Network
{
    /// <summary>
    /// WebSocket管理器 - Unity内置版本
    /// 使用Unity WebGL的JavaScript互操作实现WebSocket功能
    /// </summary>
    public class WebSocketManager : MonoBehaviour
    {
        #region JavaScript互操作 - WebGL专用

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void InitWebSocket(string url);
        
        [DllImport("__Internal")]
        private static extern void SendWebSocketMessage(string message);
        
        [DllImport("__Internal")]
        private static extern void CloseWebSocket();
        
        [DllImport("__Internal")]
        private static extern int GetWebSocketState();
#endif

        #endregion

        #region 单例模式

        private static WebSocketManager _instance;
        public static WebSocketManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("WebSocketManager");
                    _instance = go.AddComponent<WebSocketManager>();
                    DontDestroyOnLoad(go);
                    Debug.Log("[WebSocketManager] 单例实例已创建");
                }
                return _instance;
            }
        }

        #endregion

        #region 配置

        [Header("WebSocket配置")]
        [SerializeField] private int _heartbeatInterval = 30; // 心跳间隔（秒）
        [SerializeField] private int _reconnectMaxAttempts = 5; // 最大重连次数
        [SerializeField] private int _reconnectDelay = 3; // 重连延迟（秒）

        #endregion

        #region 私有字段

        // 连接信息
        private string _currentUrl;

        // 连接状态
        private bool _isConnected = false;
        private bool _isConnecting = false;
        private bool _shouldReconnect = true;

        // 重连管理
        private int _reconnectAttempts = 0;
        private Coroutine _reconnectCoroutine;

        // 心跳检测
        private Coroutine _heartbeatCoroutine;

        #endregion

        #region 属性

        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// 当前连接URL
        /// </summary>
        public string CurrentUrl => _currentUrl;

        #endregion

        #region 初始化

        private void Awake()
        {
            // 单例检查
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Initialize()
        {
            Debug.Log("[WebSocketManager] WebSocket管理器已初始化");
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        #endregion

        #region 连接管理

        /// <summary>
        /// 连接WebSocket服务器
        /// </summary>
        /// <param name="url">WebSocket服务器地址</param>
        /// <returns>连接是否成功</returns>
        public async Task<bool> ConnectAsync(string url)
        {
            if (_isConnecting)
            {
                Debug.LogWarning("[WebSocketManager] 正在连接中，请勿重复调用");
                return false;
            }

            if (_isConnected && _currentUrl == url)
            {
                Debug.Log("[WebSocketManager] 已连接到相同地址，无需重复连接");
                return true;
            }

            try
            {
                _isConnecting = true;
                _currentUrl = url;
                _shouldReconnect = true;

                Debug.Log($"[WebSocketManager] ==== 开始连接WebSocket ====");
                Debug.Log($"[WebSocketManager] 连接地址: {url}");

                // 清理之前的连接
                await DisconnectInternal();

                // 创建新连接
#if UNITY_WEBGL && !UNITY_EDITOR
                InitWebSocket(url);
#else
                // 编辑器模式下的模拟连接
                Debug.Log("[WebSocketManager] 编辑器模式下模拟连接成功");
                OnConnected();
#endif

                // 等待连接结果
                var timeout = DateTime.UtcNow.AddSeconds(10);
                while (!_isConnected && _isConnecting && DateTime.UtcNow < timeout)
                {
                    await Task.Delay(100);
                }

                if (_isConnected)
                {
                    _reconnectAttempts = 0;
                    StartHeartbeat();
                    Debug.Log("[WebSocketManager] ==== WebSocket连接成功 ====");
                    return true;
                }
                else
                {
                    throw new Exception("连接超时");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketManager] 连接失败: {ex.Message}");
                return false;
            }
            finally
            {
                _isConnecting = false;
            }
        }

        /// <summary>
        /// 断开WebSocket连接
        /// </summary>
        public async Task DisconnectAsync()
        {
            _shouldReconnect = false;
            await DisconnectInternal();
            Debug.Log("[WebSocketManager] 主动断开连接");
        }

        /// <summary>
        /// 内部断开连接方法
        /// </summary>
        private async Task DisconnectInternal()
        {
            _isConnected = false;
            _isConnecting = false;

            // 停止心跳和重连
            StopHeartbeat();
            StopReconnect();

            // 关闭WebSocket连接
#if UNITY_WEBGL && !UNITY_EDITOR
            CloseWebSocket();
#endif

            await Task.CompletedTask;
        }

        #endregion

        #region 消息处理

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="data">要发送的数据对象</param>
        public async Task<bool> SendMessageAsync(object data)
        {
            if (!_isConnected)
            {
                Debug.LogWarning("[WebSocketManager] 未连接，无法发送消息");
                return false;
            }

            try
            {
                var jsonMessage = JsonUtility.ToJson(data);
                Debug.Log($"[WebSocketManager] ==== 发送消息 ====");
                Debug.Log($"[WebSocketManager] 发送数据: {jsonMessage}");

#if UNITY_WEBGL && !UNITY_EDITOR
                SendWebSocketMessage(jsonMessage);
#else
                // 编辑器模式下模拟发送
                Debug.Log("[WebSocketManager] 编辑器模式下模拟发送消息");
#endif

                Debug.Log("[WebSocketManager] 消息发送成功");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketManager] 发送消息失败: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region JavaScript回调 - 供JS调用

        /// <summary>
        /// JavaScript回调：连接成功
        /// </summary>
        public void OnConnected()
        {
            _isConnected = true;
            _isConnecting = false;
            Debug.Log("[WebSocketManager] WebSocket连接已建立");
        }

        /// <summary>
        /// JavaScript回调：收到消息
        /// </summary>
        /// <param name="message">收到的消息</param>
        public void OnMessageReceived(string message)
        {
            try
            {
                Debug.Log($"[WebSocketManager] ==== 收到消息 ====");
                Debug.Log($"[WebSocketManager] 接收数据: {message}");

                // 检查心跳响应
                if (message.Contains("\"type\":\"pong\"") || message.Contains("\"type\": \"pong\""))
                {
                    Debug.Log("[WebSocketManager] 收到心跳响应");
                    return;
                }

                // 直接将原始消息传递给事件总线处理
                GameEventBus.ProcessRawMessage(message);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketManager] 处理收到的消息时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// JavaScript回调：连接错误
        /// </summary>
        /// <param name="error">错误信息</param>
        public void OnError(string error)
        {
            Debug.LogError($"[WebSocketManager] WebSocket错误: {error}");

            // 如果需要重连，启动重连
            if (_shouldReconnect)
            {
                StartReconnect();
            }
        }

        /// <summary>
        /// JavaScript回调：连接断开
        /// </summary>
        /// <param name="reason">断开原因</param>
        public void OnDisconnected(string reason)
        {
            _isConnected = false;
            Debug.LogWarning($"[WebSocketManager] WebSocket连接已断开: {reason}");

            // 停止心跳
            StopHeartbeat();

            // 如果需要重连，启动重连
            if (_shouldReconnect)
            {
                StartReconnect();
            }
        }

        #endregion

        #region 心跳检测

        /// <summary>
        /// 开始心跳检测
        /// </summary>
        private void StartHeartbeat()
        {
            StopHeartbeat();
            _heartbeatCoroutine = StartCoroutine(HeartbeatLoop());
            Debug.Log($"[WebSocketManager] 开始心跳检测，间隔: {_heartbeatInterval}秒");
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
                Debug.Log("[WebSocketManager] 停止心跳检测");
            }
        }

        /// <summary>
        /// 心跳循环
        /// </summary>
        private IEnumerator HeartbeatLoop()
        {
            while (_isConnected)
            {
                yield return new WaitForSeconds(_heartbeatInterval);

                if (_isConnected)
                {
                    var pingData = new { type = "ping", timestamp = DateTime.UtcNow.Ticks };
                    var task = SendMessageAsync(pingData);
                    yield return new WaitUntil(() => task.IsCompleted);

                    Debug.Log("[WebSocketManager] 发送心跳ping");
                }
            }
        }

        #endregion

        #region 自动重连

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
                Debug.Log("[WebSocketManager] 停止自动重连");
            }
        }

        /// <summary>
        /// 重连循环
        /// </summary>
        private IEnumerator ReconnectLoop()
        {
            while (_reconnectAttempts < _reconnectMaxAttempts && _shouldReconnect && !_isConnected)
            {
                _reconnectAttempts++;

                Debug.Log($"[WebSocketManager] 开始第{_reconnectAttempts}次重连尝试 (最大{_reconnectMaxAttempts}次)");

                yield return new WaitForSeconds(_reconnectDelay);

                if (!_shouldReconnect) break;

                var task = ConnectAsync(_currentUrl);
                yield return new WaitUntil(() => task.IsCompleted);

                if (task.Result)
                {
                    Debug.Log("[WebSocketManager] 重连成功");
                    _reconnectCoroutine = null;
                    yield break;
                }
            }

            if (_reconnectAttempts >= _reconnectMaxAttempts)
            {
                Debug.LogError("[WebSocketManager] 重连失败，已达到最大重试次数");
            }

            _reconnectCoroutine = null;
        }

        #endregion

        #region 清理

        private void Cleanup()
        {
            _shouldReconnect = false;

            StopHeartbeat();
            StopReconnect();

            var task = DisconnectInternal();

            Debug.Log("[WebSocketManager] 资源清理完成");
        }

        #endregion
    }
}
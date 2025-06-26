// Assets/Core/Network/WebSocketManager.cs
// WebSocket管理器 - NativeWebSocket版本
// 使用NativeWebSocket库实现跨平台WebSocket功能

using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using NativeWebSocket;
using BaccaratGame.Core.Architecture;
using System.Text;

namespace Core.Network
{
    /// <summary>
    /// WebSocket管理器 - NativeWebSocket版本
    /// 使用NativeWebSocket库实现高性能跨平台WebSocket功能
    /// </summary>
    public class WebSocketManager : MonoBehaviour
    {
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
        [SerializeField] private int _connectionTimeout = 10; // 连接超时（秒）

        #endregion

        #region 私有字段

        // NativeWebSocket实例
        private WebSocket _websocket;
        
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
        private DateTime _lastPongReceived;

        #endregion

        #region 属性

        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected => _isConnected && _websocket?.State == WebSocketState.Open;

        /// <summary>
        /// 当前连接URL
        /// </summary>
        public string CurrentUrl => _currentUrl;

        /// <summary>
        /// WebSocket连接状态
        /// </summary>
        public WebSocketState ConnectionState => _websocket?.State ?? WebSocketState.Closed;

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
            Debug.Log("[WebSocketManager] NativeWebSocket管理器已初始化");
            _lastPongReceived = DateTime.UtcNow;
        }

        private void Update()
        {
            // NativeWebSocket需要在主线程处理消息队列
            #if !UNITY_WEBGL || UNITY_EDITOR
            _websocket?.DispatchMessageQueue();
            #endif
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

            if (_isConnected && _currentUrl == url && _websocket?.State == WebSocketState.Open)
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
                Debug.Log($"[WebSocketManager] 使用NativeWebSocket库");

                // 清理之前的连接
                await DisconnectInternal();

                // 创建新的WebSocket连接
                _websocket = new WebSocket(url);

                // 设置事件处理器
                SetupWebSocketEvents();

                // 开始连接
                await _websocket.Connect();

                // 等待连接结果
                var startTime = DateTime.UtcNow;
                while (_websocket.State == WebSocketState.Connecting && 
                       DateTime.UtcNow - startTime < TimeSpan.FromSeconds(_connectionTimeout))
                {
                    await Task.Delay(100);
                }

                if (_websocket.State == WebSocketState.Open)
                {
                    _isConnected = true;
                    _reconnectAttempts = 0;
                    _lastPongReceived = DateTime.UtcNow;
                    StartHeartbeat();
                    Debug.Log("[WebSocketManager] ==== WebSocket连接成功 ====");
                    return true;
                }
                else
                {
                    throw new Exception($"连接失败，当前状态: {_websocket.State}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketManager] 连接失败: {ex.Message}");
                _isConnected = false;
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
            if (_websocket != null)
            {
                try
                {
                    if (_websocket.State == WebSocketState.Open || _websocket.State == WebSocketState.Connecting)
                    {
                        await _websocket.Close();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[WebSocketManager] 关闭连接时出现异常: {ex.Message}");
                }
                finally
                {
                    _websocket = null;
                }
            }
        }

        #endregion

        #region WebSocket事件处理

        /// <summary>
        /// 设置WebSocket事件处理器
        /// </summary>
        private void SetupWebSocketEvents()
        {
            if (_websocket == null) return;

            // 连接打开事件
            _websocket.OnOpen += OnWebSocketOpen;

            // 消息接收事件
            _websocket.OnMessage += OnWebSocketMessage;

            // 错误事件
            _websocket.OnError += OnWebSocketError;

            // 连接关闭事件
            _websocket.OnClose += OnWebSocketClose;
        }

        /// <summary>
        /// WebSocket连接打开
        /// </summary>
        private void OnWebSocketOpen()
        {
            _isConnected = true;
            _isConnecting = false;
            Debug.Log("[WebSocketManager] WebSocket连接已建立");
        }

        /// <summary>
        /// WebSocket收到消息
        /// </summary>
        /// <param name="data">收到的消息数据</param>
        private void OnWebSocketMessage(byte[] data)
        {
            try
            {
                var message = Encoding.UTF8.GetString(data);
                Debug.Log($"[WebSocketManager] ==== 收到消息 ====");
                Debug.Log($"[WebSocketManager] 接收数据: {message}");

                // 检查心跳响应
                if (message.Contains("\"type\":\"pong\"") || message.Contains("\"type\": \"pong\""))
                {
                    _lastPongReceived = DateTime.UtcNow;
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
        /// WebSocket连接错误
        /// </summary>
        /// <param name="error">错误信息</param>
        private void OnWebSocketError(string error)
        {
            Debug.LogError($"[WebSocketManager] WebSocket错误: {error}");
            _isConnected = false;

            // 如果需要重连，启动重连
            if (_shouldReconnect)
            {
                StartReconnect();
            }
        }

        /// <summary>
        /// WebSocket连接关闭
        /// </summary>
        /// <param name="closeCode">关闭代码</param>
        private void OnWebSocketClose(WebSocketCloseCode closeCode)
        {
            _isConnected = false;
            Debug.LogWarning($"[WebSocketManager] WebSocket连接已断开: {closeCode}");

            // 停止心跳
            StopHeartbeat();

            // 如果需要重连且不是正常关闭，启动重连
            if (_shouldReconnect && closeCode != WebSocketCloseCode.Normal)
            {
                StartReconnect();
            }
        }

        #endregion

        #region 消息处理

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="data">要发送的数据对象</param>
        public async Task<bool> SendMessageAsync(object data)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[WebSocketManager] 未连接，无法发送消息");
                return false;
            }

            try
            {
                var jsonMessage = JsonUtility.ToJson(data);
                Debug.Log($"[WebSocketManager] ==== 发送消息 ====");
                Debug.Log($"[WebSocketManager] 发送数据: {jsonMessage}");

                await _websocket.SendText(jsonMessage);

                Debug.Log("[WebSocketManager] 消息发送成功");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketManager] 发送消息失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 发送二进制消息
        /// </summary>
        /// <param name="data">要发送的二进制数据</param>
        public async Task<bool> SendBinaryAsync(byte[] data)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[WebSocketManager] 未连接，无法发送二进制消息");
                return false;
            }

            try
            {
                Debug.Log($"[WebSocketManager] 发送二进制数据，长度: {data.Length}");
                await _websocket.Send(data);
                Debug.Log("[WebSocketManager] 二进制消息发送成功");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketManager] 发送二进制消息失败: {ex.Message}");
                return false;
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
            while (IsConnected)
            {
                yield return new WaitForSeconds(_heartbeatInterval);

                if (IsConnected)
                {
                    // 检查是否收到心跳响应
                    var timeSinceLastPong = DateTime.UtcNow - _lastPongReceived;
                    if (timeSinceLastPong.TotalSeconds > _heartbeatInterval * 2)
                    {
                        Debug.LogWarning("[WebSocketManager] 心跳超时，可能连接已断开");
                        _isConnected = false;
                        
                        if (_shouldReconnect)
                        {
                            StartReconnect();
                        }
                        yield break;
                    }

                    // 发送心跳
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
            while (_reconnectAttempts < _reconnectMaxAttempts && _shouldReconnect && !IsConnected)
            {
                _reconnectAttempts++;

                Debug.Log($"[WebSocketManager] 开始第{_reconnectAttempts}次重连尝试 (最大{_reconnectMaxAttempts}次)");

                // 使用指数退避算法计算延迟
                var delay = Mathf.Min(_reconnectDelay * Mathf.Pow(2, _reconnectAttempts - 1), 30f);
                yield return new WaitForSeconds(delay);

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

        #region 公共接口扩展

        /// <summary>
        /// 手动触发重连
        /// </summary>
        public async Task<bool> ReconnectAsync()
        {
            _reconnectAttempts = 0;
            return await ConnectAsync(_currentUrl);
        }

        /// <summary>
        /// 获取连接状态信息
        /// </summary>
        public string GetConnectionInfo()
        {
            return $"URL: {_currentUrl}, State: {ConnectionState}, Connected: {IsConnected}, Attempts: {_reconnectAttempts}";
        }

        /// <summary>
        /// 设置心跳间隔
        /// </summary>
        public void SetHeartbeatInterval(int seconds)
        {
            _heartbeatInterval = seconds;
            if (IsConnected)
            {
                StartHeartbeat(); // 重启心跳以应用新间隔
            }
        }

        /// <summary>
        /// 检查连接健康状态
        /// </summary>
        public bool IsConnectionHealthy()
        {
            if (!IsConnected) return false;
            
            var timeSinceLastPong = DateTime.UtcNow - _lastPongReceived;
            return timeSinceLastPong.TotalSeconds < _heartbeatInterval * 2;
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
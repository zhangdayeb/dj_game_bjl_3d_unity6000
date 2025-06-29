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
    #region WebSocket消息数据结构

    /// <summary>
    /// 带code的业务消息结构
    /// </summary>
    [Serializable]
    public class WSCodeMessage
    {
        public int code;
        public string msg;
        public string user_id;
        public object data;
    }

    /// <summary>
    /// 首次连接验证消息结构
    /// </summary>
    [Serializable]
    public class WSConnectionMessage
    {
        public string user_id;
        public string table_id;
        public string game_type;
        public long timestamp;
    }

    #endregion

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

        // 调试计数器
        private static int _messageCounter = 0;

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

            Debug.Log($"[WebSocketManager] ================ 开始连接 ================");
            Debug.Log($"[WebSocketManager] 目标URL: {url}");

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
                    Debug.Log("[WebSocketManager] 清理旧连接");
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

                Debug.Log("[WebSocketManager] WebSocket实例已创建，开始连接...");
                
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
            
            Debug.Log("[WebSocketManager] ================ 连接成功 ================");
            Debug.Log($"[WebSocketManager] ✅ 连接状态: {_websocket?.State}");
            Debug.Log($"[WebSocketManager] URL: {_currentUrl}");
            
            // 开始心跳
            StartHeartbeat();
            
            // 发送连接成功消息（包含必需参数验证）
            SendConnectionMessage();
        }

        /// <summary>
        /// 收到WebSocket消息
        /// </summary>
        private void OnMessageReceived(byte[] data)
        {
            try
            {
                string message = Encoding.UTF8.GetString(data);
                var messageSize = data.Length;
                
                Debug.Log($"[WebSocketManager] ================ 收到消息 ================");
                Debug.Log($"[WebSocketManager] 📨 消息大小: {messageSize} bytes");
                Debug.Log($"[WebSocketManager] 📨 原始数据: {message}");
                
                // 检查是否是心跳响应
                if (message == "pong")
                {
                    Debug.Log("[WebSocketManager] 💓 收到心跳响应: pong");
                    return;
                }
                
                // 尝试格式化JSON显示
                try
                {
                    if (IsJson(message))
                    {
                        var formattedJson = FormatJson(message);
                        Debug.Log($"[WebSocketManager] 📨 格式化消息:\n{formattedJson}");
                    }
                }
                catch (Exception formatEx)
                {
                    Debug.LogWarning($"[WebSocketManager] JSON格式化失败: {formatEx.Message}");
                }

                // 解析并分发消息
                ProcessMessage(message);
                
                Debug.Log($"[WebSocketManager] ================ 消息处理完成 ================");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketManager] ❌ 处理消息异常: {ex.Message}");
                Debug.LogError($"[WebSocketManager] 异常堆栈: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// WebSocket连接错误
        /// </summary>
        private void OnError(string error)
        {
            Debug.LogError($"[WebSocketManager] ================ 连接错误 ================");
            Debug.LogError($"[WebSocketManager] ❌ 错误信息: {error}");
            Debug.LogError($"[WebSocketManager] URL: {_currentUrl}");
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
            
            Debug.LogWarning($"[WebSocketManager] ================ 连接断开 ================");
            Debug.LogWarning($"[WebSocketManager] 🔌 断开代码: {closeCode}");
            Debug.LogWarning($"[WebSocketManager] URL: {_currentUrl}");
            
            StopHeartbeat();
            
            // 非正常断开时启动重连
            if (_shouldReconnect && closeCode != WebSocketCloseCode.Normal)
            {
                Debug.Log("[WebSocketManager] 检测到非正常断开，启动重连机制");
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
                Debug.Log($"[WebSocketManager] ========== 开始处理消息 ==========");
                
                // 检查是否是连接成功的响应
                if (message.Contains("连接成功") || message.Contains("成功"))
                {
                    Debug.Log("[WebSocketManager] 🎉 收到连接成功响应");
                    return;
                }
                
                // 尝试解析JSON消息
                if (IsJson(message))
                {
                    // 简单的JSON解析，提取code字段
                    var code = ExtractMessageCode(message);
                    var msg = ExtractMessageField(message, "msg");
                    
                    // 🔥 关键修复：解码Unicode字符串
                    var decodedMsg = DecodeUnicodeString(msg);
                    
                    Debug.Log($"[WebSocketManager] 提取的消息code: {code}");
                    Debug.Log($"[WebSocketManager] 提取的消息msg: {msg}");
                    Debug.Log($"[WebSocketManager] 解码后的msg: {decodedMsg}");
                    
                    // 根据后端逻辑分发消息 - 使用解码后的消息进行匹配
                    switch (code)
                    {
                        case 200:
                            if (decodedMsg == "倒计时信息")
                            {
                                Debug.Log("[WebSocketManager] 📊 处理倒计时消息");
                                NetworkEvents.TriggerCountdownReceived(message);
                            }
                            else if (decodedMsg == "开牌信息")
                            {
                                Debug.Log("[WebSocketManager] 🃏 处理开牌消息");
                                NetworkEvents.TriggerDealCardsReceived(message);
                            }
                            else if (decodedMsg == "中奖信息")
                            {
                                Debug.Log("[WebSocketManager] 🎯 处理中奖消息");
                                NetworkEvents.TriggerGameResultReceived(message);
                            }
                            else if (decodedMsg == "成功")
                            {
                                Debug.Log("[WebSocketManager] ✅ 处理成功响应");
                            }
                            else
                            {
                                Debug.Log($"[WebSocketManager] ❓ 未识别的消息类型: {decodedMsg}");
                            }
                            break;
                            
                        default:
                            Debug.Log($"[WebSocketManager] ❓ 未处理的消息code: {code}，msg: {decodedMsg}");
                            break;
                    }
                }
                else
                {
                    Debug.Log($"[WebSocketManager] 📝 收到非JSON消息: {message}");
                }
                
                Debug.Log($"[WebSocketManager] ========== 消息处理完成 ==========");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketManager] ❌ 消息处理异常: {ex.Message}");
                Debug.LogError($"[WebSocketManager] 异常消息: {message}");
            }
        }

        /// <summary>
        /// 解码Unicode字符串（将 \uXXXX 格式转换为中文）
        /// </summary>
        /// <param name="unicodeString">包含Unicode转义序列的字符串</param>
        /// <returns>解码后的字符串</returns>
        private string DecodeUnicodeString(string unicodeString)
        {
            if (string.IsNullOrEmpty(unicodeString))
                return unicodeString;
            
            try
            {
                // 使用正则表达式匹配 \uXXXX 格式的Unicode字符
                string decoded = System.Text.RegularExpressions.Regex.Replace(
                    unicodeString,
                    @"\\u([0-9A-Fa-f]{4})",
                    match => {
                        // 将十六进制字符串转换为字符
                        int code = Convert.ToInt32(match.Groups[1].Value, 16);
                        return char.ConvertFromUtf32(code);
                    }
                );
                
                return decoded;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[WebSocketManager] Unicode解码失败: {ex.Message}，返回原字符串");
                return unicodeString;
            }
        }

        /// <summary>
        /// 从JSON消息中提取code字段
        /// </summary>
        private int ExtractMessageCode(string jsonMessage)
        {
            try
            {
                int codeIndex = jsonMessage.IndexOf("\"code\"");
                if (codeIndex == -1) return -1;

                int colonIndex = jsonMessage.IndexOf(":", codeIndex);
                if (colonIndex == -1) return -1;

                int valueStart = colonIndex + 1;
                while (valueStart < jsonMessage.Length && 
                       (jsonMessage[valueStart] == ' ' || jsonMessage[valueStart] == '"'))
                    valueStart++;

                int valueEnd = valueStart;
                while (valueEnd < jsonMessage.Length && 
                       char.IsDigit(jsonMessage[valueEnd]))
                    valueEnd++;

                if (int.TryParse(jsonMessage.Substring(valueStart, valueEnd - valueStart), out int code))
                {
                    return code;
                }
                
                return -1;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[WebSocketManager] 提取消息code失败: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// 从JSON消息中提取指定字段
        /// </summary>
        private string ExtractMessageField(string jsonMessage, string fieldName)
        {
            try
            {
                string searchPattern = $"\"{fieldName}\"";
                int fieldIndex = jsonMessage.IndexOf(searchPattern);
                if (fieldIndex == -1) return null;

                int colonIndex = jsonMessage.IndexOf(":", fieldIndex);
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
        /// 发送消息到WebSocket服务器（用于业务消息）
        /// </summary>
        public async void SendMessage(object data)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[WebSocketManager] ⚠️ 未连接，无法发送消息");
                return;
            }

            var messageId = ++_messageCounter;

            try
            {
                Debug.Log($"[WebSocketManager] ================ 发送消息#{messageId} ================");
                Debug.Log($"[WebSocketManager] 📤 连接状态: {IsConnected}");
                Debug.Log($"[WebSocketManager] 📤 WebSocket状态: {_websocket?.State}");

                string jsonMessage = JsonUtility.ToJson(data, true);
                var messageSize = Encoding.UTF8.GetByteCount(jsonMessage);
                
                Debug.Log($"[WebSocketManager] 📤 发送数据: {jsonMessage}");
                Debug.Log($"[WebSocketManager] 📤 消息大小: {messageSize} bytes");
                
                await _websocket.SendText(jsonMessage);
                
                Debug.Log($"[WebSocketManager] ✅ 消息#{messageId}发送成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketManager] ❌ 发送消息#{messageId}失败: {ex.Message}");
                Debug.LogError($"[WebSocketManager] 异常堆栈: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// 发送原始消息（不做任何处理）
        /// </summary>
        public async void SendRawMessage(object data)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[WebSocketManager] ⚠️ 未连接，无法发送原始消息");
                return;
            }

            var messageId = ++_messageCounter;

            try
            {
                Debug.Log($"[WebSocketManager] ================ 发送原始消息#{messageId} ================");
                
                string jsonMessage = JsonUtility.ToJson(data, true);
                var messageSize = Encoding.UTF8.GetByteCount(jsonMessage);
                
                Debug.Log($"[WebSocketManager] 📤 原始消息: {jsonMessage}");
                Debug.Log($"[WebSocketManager] 📤 消息大小: {messageSize} bytes");
                
                await _websocket.SendText(jsonMessage);
                
                Debug.Log($"[WebSocketManager] ✅ 原始消息#{messageId}发送成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketManager] ❌ 发送原始消息#{messageId}失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 发送文本消息（用于心跳等特殊情况）
        /// </summary>
        public async void SendTextMessage(string message)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[WebSocketManager] ⚠️ 未连接，无法发送文本消息");
                return;
            }

            var messageId = ++_messageCounter;

            try
            {
                Debug.Log($"[WebSocketManager] ================ 发送文本消息#{messageId} ================");
                Debug.Log($"[WebSocketManager] 📤 文本消息: {message}");
                
                await _websocket.SendText(message);
                
                Debug.Log($"[WebSocketManager] ✅ 文本消息#{messageId}发送成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketManager] ❌ 发送文本消息#{messageId}失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 发送连接验证消息（只在首次连接时需要完整参数）
        /// </summary>
        private void SendConnectionMessage()
        {
            var gameParams = BaccaratGame.Data.GameParams.Instance;
            
            if (!gameParams.IsInitialized)
            {
                Debug.LogWarning("[WebSocketManager] GameParams未初始化，跳过连接验证");
                return;
            }

            // 🔥 修正：首次连接消息需要包含完整参数
            var connectionMessage = new WSConnectionMessage
            {
                user_id = gameParams.user_id,
                table_id = gameParams.table_id,
                game_type = gameParams.game_type,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            Debug.Log("[WebSocketManager] 📡 发送首次连接验证消息");
            SendRawMessage(connectionMessage);
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
                Debug.Log("[WebSocketManager] 💓 停止心跳检测");
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
                    // 🔥 修正：使用专门的心跳发送方法
                    SendHeartbeat();
                }
            }
        }

        /// <summary>
        /// 发送心跳消息（简单字符串，不需要参数）
        /// </summary>
        private async void SendHeartbeat()
        {
            if (!IsConnected) return;

            try
            {
                // 🔥 修正：发送纯字符串 "ping"，不是JSON
                string heartbeatMessage = "ping";
                
                Debug.Log($"[WebSocketManager] 💓 发送心跳: {heartbeatMessage}");
                
                await _websocket.SendText(heartbeatMessage);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketManager] ❌ 发送心跳失败: {ex.Message}");
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
            
            Debug.Log("[WebSocketManager] 🔄 启动重连机制");
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
                Debug.Log("[WebSocketManager] 🔄 停止重连机制");
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
                Debug.Log($"[WebSocketManager] URL: {_currentUrl}");
                
                yield return new WaitForSeconds(RECONNECT_DELAY);
                
                if (!_shouldReconnect) break;
                
                var connectTask = ConnectInternal();
                yield return new WaitUntil(() => connectTask.IsCompleted);
                
                if (IsConnected)
                {
                    Debug.Log("[WebSocketManager] ✅ 重连成功");
                    break;
                }
                else
                {
                    Debug.LogWarning($"[WebSocketManager] ❌ 第{_reconnectAttempts}次重连失败");
                }
            }

            if (_reconnectAttempts >= MAX_RECONNECT_ATTEMPTS)
            {
                Debug.LogError("[WebSocketManager] ❌ 重连失败，已达到最大重试次数");
            }

            _reconnectCoroutine = null;
        }

        #endregion

        #region 便捷发送方法

        /// <summary>
        /// 发送带code的业务消息
        /// </summary>
        /// <param name="code">消息代码</param>
        /// <param name="msg">消息内容</param>
        /// <param name="data">附加数据</param>
        public void SendCodeMessage(int code, string msg = "", object data = null)
        {
            var gameParams = BaccaratGame.Data.GameParams.Instance;
            
            var codeMessage = new WSCodeMessage
            {
                code = code,
                msg = msg,
                user_id = gameParams.user_id,
                data = data
            };

            Debug.Log($"[WebSocketManager] 📋 发送业务消息，code: {code}, msg: {msg}");
            SendRawMessage(codeMessage);
        }

        /// <summary>
        /// 发送自定义消息
        /// </summary>
        /// <param name="messageType">消息类型</param>
        /// <param name="customData">自定义数据</param>
        public void SendCustomMessage(string messageType, object customData = null)
        {
            var customMessage = new 
            {
                type = messageType,
                data = customData,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            Debug.Log($"[WebSocketManager] 🔧 发送自定义消息: {messageType}");
            SendMessage(customMessage);
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
                Debug.Log("[WebSocketManager] 🔄 手动触发重连");
                _reconnectAttempts = 0;
                Connect(_currentUrl);
            }
        }

        /// <summary>
        /// 获取连接状态信息
        /// </summary>
        public string GetStatusInfo()
        {
            var status = $"URL: {_currentUrl} | 连接: {IsConnected} | 重连次数: {_reconnectAttempts}/{MAX_RECONNECT_ATTEMPTS}";
            Debug.Log($"[WebSocketManager] 状态信息: {status}");
            return status;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 判断字符串是否为JSON格式
        /// </summary>
        private bool IsJson(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            
            text = text.Trim();
            return (text.StartsWith("{") && text.EndsWith("}")) || 
                   (text.StartsWith("[") && text.EndsWith("]"));
        }

        /// <summary>
        /// 简单的JSON格式化
        /// </summary>
        private string FormatJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return json;
            
            // 简单的格式化，主要是添加换行
            var formatted = json.Replace(",", ",\n  ")
                              .Replace("{", "{\n  ")
                              .Replace("}", "\n}")
                              .Replace("[", "[\n  ")
                              .Replace("]", "\n]");
            
            return formatted;
        }

        #endregion
    }
}
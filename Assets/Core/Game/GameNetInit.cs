// Assets/Core/Network/GameNetInit.cs
// 游戏网络初始化管理器 - 简化版
// 只负责初始化 WebSocket 连接

using System;
using UnityEngine;
using BaccaratGame.Data;

namespace BaccaratGame.Core
{
    /// <summary>
    /// 游戏网络初始化管理器 - 简化版
    /// 只负责 WebSocket 初始化，HTTP API 由单例自动处理
    /// </summary>
    public class GameNetInit : MonoBehaviour
    {
        #region 配置参数

        [Header("初始化配置")]
        [Tooltip("是否在Start时自动初始化")]
        public bool autoInitializeOnStart = true;

        [Tooltip("是否启用WebSocket连接")]
        public bool enableWebSocket = true;

        #endregion

        #region 私有字段

        private bool _isInitialized = false;

        #endregion

        #region 公共属性

        /// <summary>
        /// 是否已完成初始化
        /// </summary>
        public bool IsInitialized => _isInitialized;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            Debug.Log("[GameNetInit] 网络初始化管理器已创建");
        }

        private void Start()
        {
            if (autoInitializeOnStart)
            {
                InitializeNetwork();
            }
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 初始化网络系统
        /// </summary>
        public void InitializeNetwork()
        {
            if (_isInitialized)
            {
                Debug.Log("[GameNetInit] 网络已初始化，跳过");
                return;
            }

            Debug.Log("[GameNetInit] ==== 开始网络初始化流程 ====");

            try
            {
                // 步骤1：初始化游戏参数
                Debug.Log("[GameNetInit] 步骤1: 初始化游戏参数");
                InitializeGameParams();

                // 步骤2：连接WebSocket（如果启用）
                if (enableWebSocket)
                {
                    Debug.Log("[GameNetInit] 步骤2: 连接WebSocket");
                    ConnectWebSocket();
                }

                _isInitialized = true;
                Debug.Log("[GameNetInit] ==== 网络初始化完成 ====");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameNetInit] ==== 网络初始化失败 ====: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 重新初始化网络系统
        /// </summary>
        public void ReinitializeNetwork()
        {
            Debug.Log("[GameNetInit] 开始重新初始化网络系统");

            // 清理现有资源
            Cleanup();

            // 重新初始化
            InitializeNetwork();
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Cleanup()
        {
            // 断开WebSocket连接
            if (enableWebSocket)
            {
                try
                {
                    WebSocketManager.Instance?.Disconnect();
                    Debug.Log("[GameNetInit] WebSocket连接已断开");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[GameNetInit] WebSocket断开失败: {ex.Message}");
                }
            }

            // 重置状态
            _isInitialized = false;

            Debug.Log("[GameNetInit] 网络初始化资源已清理");
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 初始化游戏参数
        /// </summary>
        private void InitializeGameParams()
        {
            if (!GameParams.Instance.IsInitialized)
            {
                GameParams.Instance.Initialize();
            }

            Debug.Log($"[GameNetInit] 游戏参数初始化完成");
        }

        /// <summary>
        /// 连接WebSocket
        /// </summary>
        private void ConnectWebSocket()
        {
            var websocketUrl = GameParams.Instance.websocketUrl;
            
            if (string.IsNullOrEmpty(websocketUrl))
            {
                Debug.LogWarning("[GameNetInit] WebSocket地址为空，跳过连接");
                return;
            }

            try
            {
                WebSocketManager.Instance.Connect(websocketUrl);
                Debug.Log($"[GameNetInit] WebSocket连接已启动: {websocketUrl}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameNetInit] WebSocket连接失败: {ex.Message}");
                // WebSocket连接失败不影响整体初始化，只记录错误
            }
        }

        #endregion

        #region 调试方法

        /// <summary>
        /// 获取初始化状态信息
        /// </summary>
        /// <returns>状态信息字符串</returns>
        public string GetStatusInfo()
        {
            return $"初始化状态: {(_isInitialized ? "已完成" : "未开始")} | " +
                   $"WebSocket: {(enableWebSocket ? "启用" : "禁用")}";
        }

        [ContextMenu("手动初始化")]
        public void ManualInitialize()
        {
            InitializeNetwork();
        }

        [ContextMenu("重新初始化")]
        public void ManualReinitialize()
        {
            ReinitializeNetwork();
        }

        [ContextMenu("测试WebSocket连接")]
        public void TestWebSocketConnection()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[GameNetInit] 请先初始化网络系统");
                return;
            }

            ConnectWebSocket();
        }

        #endregion
    }
}
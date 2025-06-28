// Assets/Core/Data/Types/GameParams.cs
// 游戏参数管理中心 - 简化版
// 只负责环境检测、参数接收和全局访问

using System;
using UnityEngine;

namespace BaccaratGame.Data
{
    #region 网络配置结构

    /// <summary>
    /// 网络配置结构（对应NetworkConfig.json）
    /// </summary>
    [Serializable]
    public class NetworkConfig
    {
        public ProductionConfig production;
        public DefaultParams defaultParams;
        
        [Serializable]
        public class ProductionConfig
        {
            public HttpConfig http;
            public WebSocketConfig websocket;
        }
        
        [Serializable]
        public class HttpConfig
        {
            public string baseUrl;
            public int timeout;
        }
        
        [Serializable]
        public class WebSocketConfig
        {
            public string url;
            public int reconnectAttempts;
        }
        
        [Serializable]
        public class DefaultParams
        {
            public string table_id;
            public string game_type;
            public string user_id;
            public string token;
            public string language;
            public string currency;
        }
    }

    #endregion

    #region 游戏参数管理器

    /// <summary>
    /// 游戏参数管理器 - 简化版
    /// 只负责参数存储和全局访问
    /// </summary>
    public class GameParams
    {
        #region 单例模式

        private static GameParams _instance;
        public static GameParams Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameParams();
                }
                return _instance;
            }
        }

        #endregion

        #region 核心参数

        public string table_id;
        public string game_type;
        public string user_id;
        public string token;
        public string language;
        public string currency;

        public string httpBaseUrl;
        public string websocketUrl;
        public int httpTimeout;
        public int websocketReconnectAttempts;

        #endregion

        #region 状态属性

        private bool _isInitialized = false;
        private bool _isWebGLMode = false;

        public bool IsInitialized => _isInitialized;
        public bool IsWebGLMode => _isWebGLMode;

        #endregion

        #region 初始化方法

        /// <summary>
        /// 初始化参数系统
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.Log("[GameParams] 已经初始化过，跳过");
                return;
            }

            Debug.Log("[GameParams] 开始参数初始化");

            // 1. 检测环境
            DetectEnvironment();

            // 2. 加载配置文件
            LoadNetworkConfig();

            // 3. 根据环境获取参数
            if (_isWebGLMode)
            {
                LoadParametersFromJS();
            }
            else
            {
                LoadParametersFromConfig();
            }

            _isInitialized = true;
            Debug.Log($"[GameParams] 参数初始化完成: {this}");
        }

        #endregion

        #region 环境检测

        /// <summary>
        /// 检测运行环境
        /// </summary>
        private void DetectEnvironment()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            _isWebGLMode = true;
            Debug.Log("[GameParams] WebGL环境");
            #else
            _isWebGLMode = false;
            Debug.Log("[GameParams] Editor环境");
            #endif
        }

        #endregion

        #region 配置加载

        /// <summary>
        /// 加载NetworkConfig.json配置
        /// </summary>
        private void LoadNetworkConfig()
        {
            try
            {
                var configAsset = Resources.Load<TextAsset>("NetworkConfig");
                if (configAsset != null)
                {
                    var config = JsonUtility.FromJson<NetworkConfig>(configAsset.text);
                    
                    // 加载网络配置
                    httpBaseUrl = config.production.http.baseUrl;
                    websocketUrl = config.production.websocket.url;
                    httpTimeout = config.production.http.timeout;
                    websocketReconnectAttempts = config.production.websocket.reconnectAttempts;
                    
                    Debug.Log("[GameParams] 网络配置加载成功");
                }
                else
                {
                    Debug.LogError("[GameParams] 未找到NetworkConfig.json");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameParams] 配置加载失败: {ex.Message}");
            }
        }

        #endregion

        #region 参数加载

        /// <summary>
        /// 从JS获取参数（WebGL模式）
        /// </summary>
        private void LoadParametersFromJS()
        {
            Debug.Log("[GameParams] 从JS获取参数");

            #if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                // 调用桥接JS中的GetUrlParameters函数
                string paramsJson = Application.ExternalEval("GetUrlParameters()");
                
                if (!string.IsNullOrEmpty(paramsJson))
                {
                    var jsParams = JsonUtility.FromJson<JSGameParams>(paramsJson);
                    
                    table_id = jsParams.table_id ?? "";
                    game_type = jsParams.game_type ?? "";
                    user_id = jsParams.user_id ?? "";
                    token = jsParams.token ?? "";
                    language = jsParams.language ?? "zh";
                    currency = jsParams.currency ?? "CNY";
                    
                    Debug.Log("[GameParams] JS参数获取成功");
                }
                else
                {
                    Debug.LogWarning("[GameParams] JS返回空参数，使用配置默认值");
                    LoadParametersFromConfig();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameParams] JS参数获取失败: {ex.Message}");
                LoadParametersFromConfig();
            }
            #endif
        }

        /// <summary>
        /// 从配置文件加载参数（Editor模式 + 备用）
        /// </summary>
        private void LoadParametersFromConfig()
        {
            Debug.Log("[GameParams] 从配置文件加载参数");

            try
            {
                var configAsset = Resources.Load<TextAsset>("NetworkConfig");
                if (configAsset != null)
                {
                    var config = JsonUtility.FromJson<NetworkConfig>(configAsset.text);
                    var defaultParams = config.defaultParams;
                    
                    table_id = defaultParams.table_id;
                    game_type = defaultParams.game_type;
                    user_id = defaultParams.user_id;
                    token = defaultParams.token;
                    language = defaultParams.language;
                    currency = defaultParams.currency;
                    
                    Debug.Log("[GameParams] 配置参数加载成功");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameParams] 配置参数加载失败: {ex.Message}");
            }
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 获取WebSocket连接URL
        /// </summary>
        public string GetWebSocketUrl()
        {
            return websocketUrl;
        }

        /// <summary>
        /// 获取HTTP API基础URL
        /// </summary>
        public string GetHttpBaseUrl()
        {
            return httpBaseUrl;
        }

        /// <summary>
        /// 获取完整API URL
        /// </summary>
        public string GetApiUrl(string endpoint)
        {
            return httpBaseUrl.TrimEnd('/') + "/" + endpoint.TrimStart('/');
        }

        #endregion

        #region 辅助类型

        /// <summary>
        /// JS传递的参数结构
        /// </summary>
        [Serializable]
        private class JSGameParams
        {
            public string table_id;
            public string game_type;
            public string user_id;
            public string token;
            public string language;
            public string currency;
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            var tokenPreview = string.IsNullOrEmpty(token) ? "null" : 
                              (token.Length > 8 ? token.Substring(0, 8) + "..." : token);
            
            return $"GameParams[Table:{table_id}, User:{user_id}, Token:{tokenPreview}, Env:{(_isWebGLMode ? "WebGL" : "Editor")}]";
        }

        #endregion
    }

    #endregion
}
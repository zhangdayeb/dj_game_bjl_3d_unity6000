// Assets/Core/Network/GameNetInit.cs
// 游戏网络初始化管理器 - 统一管理网络初始化流程
// 挂载到MainCanvas节点，作为游戏启动的网络初始化入口

using System;
using System.Threading.Tasks;
using UnityEngine;
using BaccaratGame.Data;

namespace BaccaratGame.Core
{
    /// <summary>
    /// 游戏网络初始化管理器
    /// 协调所有网络相关的初始化流程
    /// </summary>
    public class GameNetInit : MonoBehaviour
    {
        #region 配置参数

        [Header("初始化配置")]
        [Tooltip("是否在Start时自动初始化")]
        public bool autoInitializeOnStart = true;

        [Tooltip("是否启用重试机制")]
        public bool enableRetry = true;

        [Tooltip("最大重试次数")]
        [Range(0, 5)]
        public int maxRetryAttempts = 3;

        [Tooltip("重试间隔(秒)")]
        [Range(1, 10)]
        public float retryDelay = 2f;

        #endregion

        #region 私有字段

        private GameNetworkApi _networkApi;
        private bool _isInitialized = false;
        private bool _isInitializing = false;
        private int _currentRetryAttempt = 0;

        // 缓存的数据
        private TableInfo _tableInfo;
        private UserInfo _userInfo;

        #endregion

        #region 公共属性

        /// <summary>
        /// 是否已完成初始化
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// 是否正在初始化中
        /// </summary>
        public bool IsInitializing => _isInitializing;

        /// <summary>
        /// 获取台桌信息
        /// </summary>
        public TableInfo TableInfo => _tableInfo;

        /// <summary>
        /// 获取用户信息
        /// </summary>
        public UserInfo UserInfo => _userInfo;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            Debug.Log("[GameNetInit] 网络初始化管理器已创建");
        }

        private async void Start()
        {
            if (autoInitializeOnStart)
            {
                await InitializeNetwork();
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
        /// <returns>初始化任务</returns>
        public async Task InitializeNetwork()
        {
            if (_isInitialized)
            {
                Debug.Log("[GameNetInit] 网络已初始化，跳过");
                return;
            }

            if (_isInitializing)
            {
                Debug.Log("[GameNetInit] 网络正在初始化中，请等待");
                return;
            }

            _isInitializing = true;
            _currentRetryAttempt = 0;

            Debug.Log("[GameNetInit] ==== 开始网络初始化流程 ====");

            try
            {
                await InitializeWithRetry();
                _isInitialized = true;
                Debug.Log("[GameNetInit] ==== 网络初始化完成 ====");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameNetInit] ==== 网络初始化失败 ====: {ex.Message}");
                throw;
            }
            finally
            {
                _isInitializing = false;
            }
        }

        /// <summary>
        /// 重新初始化网络系统
        /// </summary>
        /// <returns>重新初始化任务</returns>
        public async Task ReinitializeNetwork()
        {
            Debug.Log("[GameNetInit] 开始重新初始化网络系统");

            // 清理现有资源
            Cleanup();

            // 重新初始化
            await InitializeNetwork();
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Cleanup()
        {
            if (_networkApi != null)
            {
                _networkApi.Cleanup();
                _networkApi = null;
            }

            // 重置所有状态
            _isInitialized = false;
            _isInitializing = false;
            _currentRetryAttempt = 0;
            _tableInfo = null;
            _userInfo = null;

            Debug.Log("[GameNetInit] 网络初始化资源已清理");
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 带重试的初始化
        /// </summary>
        private async Task InitializeWithRetry()
        {
            Exception lastException = null;

            for (int attempt = 0; attempt <= maxRetryAttempts; attempt++)
            {
                _currentRetryAttempt = attempt;

                // 🔥 关键修改：每次重试前先清理实例（除了第一次）
                if (attempt > 0)
                {
                    Debug.Log($"[GameNetInit] 第{attempt + 1}次重试前，先清理现有实例");
                    CleanupForRetry();
                }

                try
                {
                    await InitializeNetworkInternal();
                    return; // 成功，退出重试循环
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    
                    // 🔥 关键修改：异常时也立即清理，防止实例累积
                    Debug.LogWarning($"[GameNetInit] 第{attempt + 1}次尝试失败，清理实例: {ex.Message}");
                    CleanupForRetry();
                    
                    if (attempt < maxRetryAttempts && enableRetry)
                    {
                        Debug.LogWarning($"[GameNetInit] {retryDelay}秒后进行第{attempt + 2}次重试");
                        await Task.Delay((int)(retryDelay * 1000));
                    }
                    else
                    {
                        Debug.LogError($"[GameNetInit] 初始化失败，已达到最大重试次数: {ex.Message}");
                        break;
                    }
                }
            }

            // 如果到这里，说明所有重试都失败了
            throw lastException ?? new Exception("网络初始化失败");
        }

        /// <summary>
        /// 专门用于重试的清理方法（不重置初始化状态）
        /// </summary>
        private void CleanupForRetry()
        {
            if (_networkApi != null)
            {
                Debug.Log("[GameNetInit] 清理 GameNetworkApi 实例");
                _networkApi.Cleanup();
                _networkApi = null;
            }

            // 注意：不重置 _isInitializing 状态，因为我们还在初始化过程中
            // 只重置数据缓存
            _tableInfo = null;
            _userInfo = null;

            Debug.Log("[GameNetInit] 重试前清理完成");
        }

        /// <summary>
        /// 内部初始化方法
        /// </summary>
        private async Task InitializeNetworkInternal()
        {
            // 步骤1：初始化游戏参数
            Debug.Log("[GameNetInit] 步骤1: 初始化游戏参数");
            InitializeGameParams();

            // 步骤2：创建并初始化API客户端
            Debug.Log("[GameNetInit] 步骤2: 初始化API客户端");
            InitializeApiClients();

            // 步骤3：获取基础数据
            Debug.Log("[GameNetInit] 步骤3: 获取基础数据");
            await FetchBasicData();

            // 步骤4：加载路单iframe（立即可用）
            Debug.Log("[GameNetInit] 步骤4: 加载路单iframe");
            LoadRoadmapIframe();

            // 步骤5：加载视频iframe（需要台桌信息）
            Debug.Log("[GameNetInit] 步骤5: 加载视频iframe");
            LoadVideoIframes();

            // 步骤6：连接WebSocket
            Debug.Log("[GameNetInit] 步骤6: 连接WebSocket");
            ConnectWebSocket();

            // 步骤7：通知初始化完成
            Debug.Log("[GameNetInit] 步骤7: 通知初始化完成");
            NotifyInitializationComplete();
        }

        /// <summary>
        /// 初始化游戏参数
        /// </summary>
        private void InitializeGameParams()
        {
            if (!GameParams.Instance.IsInitialized)
            {
                GameParams.Instance.Initialize();
            }

            Debug.Log($"[GameNetInit] 游戏参数初始化完成: {GameParams.Instance}");
        }

        /// <summary>
        /// 初始化API客户端
        /// </summary>
        private void InitializeApiClients()
        {
            // 🔥 关键修改：创建前先检查并清理
            if (_networkApi != null)
            {
                Debug.Log("[GameNetInit] 发现现有 GameNetworkApi 实例，先清理");
                _networkApi.Cleanup();
                _networkApi = null;
            }

            _networkApi = GameNetworkApi.CreateAndInitialize();
            Debug.Log("[GameNetInit] API客户端初始化完成");
        }

        /// <summary>
        /// 获取基础数据
        /// </summary>
        private async Task FetchBasicData()
        {
            var (tableInfo, userInfo) = await _networkApi.GetBasicData();
            
            _tableInfo = tableInfo;
            _userInfo = userInfo;

            Debug.Log($"[GameNetInit] 基础数据获取完成:");
            Debug.Log($"  - 台桌: {_tableInfo?.table_title} (ID: {_tableInfo?.id})");
            Debug.Log($"  - 用户: {_userInfo?.user_name} (余额: {_userInfo?.money_balance})");
        }

        /// <summary>
        /// 加载路单iframe
        /// </summary>
        private void LoadRoadmapIframe()
        {
            var roadmapUrl = _networkApi.BuildRoadmapIframeUrl();
            
            if (string.IsNullOrEmpty(roadmapUrl))
            {
                Debug.LogWarning("[GameNetInit] 路单URL为空，跳过加载");
                return;
            }

            // 调用JS桥接加载iframe
            var jsCall = $"window.LayeredIframeManager && window.LayeredIframeManager.load('roadmap-layer', '{roadmapUrl}', 'roadmap')";
            
#if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalEval(jsCall);
#else
            Debug.Log($"[GameNetInit] (Editor模式) 路单iframe JS调用: {jsCall}");
#endif

            Debug.Log($"[GameNetInit] 路单iframe加载完成: {roadmapUrl}");
        }

        /// <summary>
        /// 加载视频iframe
        /// </summary>
        private void LoadVideoIframes()
        {
            if (_tableInfo == null)
            {
                Debug.LogWarning("[GameNetInit] 台桌信息为空，跳过视频iframe加载");
                return;
            }

            // 加载远景视频iframe
            LoadVideoIframe(_tableInfo.video_far, "video-far-layer", "远景");

            // 加载近景视频iframe
            LoadVideoIframe(_tableInfo.video_near, "video-near-layer", "近景");
        }

        /// <summary>
        /// 加载单个视频iframe
        /// </summary>
        private void LoadVideoIframe(string baseVideoUrl, string layerId, string videoType)
        {
            if (string.IsNullOrEmpty(baseVideoUrl))
            {
                Debug.LogWarning($"[GameNetInit] {videoType}视频地址为空，跳过加载");
                return;
            }

            var videoUrl = _networkApi.BuildVideoIframeUrl(baseVideoUrl);
            var jsCall = $"window.LayeredIframeManager && window.LayeredIframeManager.load('{layerId}', '{videoUrl}', 'video')";
            
#if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalEval(jsCall);
#else
            Debug.Log($"[GameNetInit] (Editor模式) {videoType}iframe JS调用: {jsCall}");
#endif

            Debug.Log($"[GameNetInit] {videoType}iframe加载完成: {videoUrl}");
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

        /// <summary>
        /// 通知初始化完成
        /// </summary>
        private void NotifyInitializationComplete()
        {
            Debug.Log("[GameNetInit] 网络初始化流程全部完成");
            
            // 可以在这里触发事件通知其他组件
            // 例如：GameEvents.OnNetworkInitialized?.Invoke(_tableInfo, _userInfo);
            
            // 或者更新全局状态
            // 例如：GameState.SetNetworkReady(true);
        }

        #endregion

        #region 调试方法

        /// <summary>
        /// 获取初始化状态信息
        /// </summary>
        /// <returns>状态信息字符串</returns>
        public string GetStatusInfo()
        {
            return $"初始化状态: {(_isInitialized ? "已完成" : _isInitializing ? "进行中" : "未开始")} | " +
                   $"重试次数: {_currentRetryAttempt}/{maxRetryAttempts} | " +
                   $"台桌: {_tableInfo?.table_title ?? "未获取"} | " +
                   $"用户: {_userInfo?.user_name ?? "未获取"}";
        }

        /// <summary>
        /// 手动测试单个步骤
        /// </summary>
        /// <param name="stepName">步骤名称</param>
        [ContextMenu("测试获取台桌信息")]
        public async void TestGetTableInfo()
        {
            if (_networkApi == null)
            {
                InitializeApiClients();
            }

            try
            {
                var tableInfo = await _networkApi.GetTableInfo();
                Debug.Log($"[GameNetInit] 测试结果 - 台桌信息: {tableInfo?.table_title}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameNetInit] 测试失败: {ex.Message}");
            }
        }

        [ContextMenu("测试获取用户信息")]
        public async void TestGetUserInfo()
        {
            if (_networkApi == null)
            {
                InitializeApiClients();
            }

            try
            {
                var userInfo = await _networkApi.GetUserInfo();
                Debug.Log($"[GameNetInit] 测试结果 - 用户信息: {userInfo?.user_name}, 余额: {userInfo?.money_balance}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameNetInit] 测试失败: {ex.Message}");
            }
        }

        #endregion
    }
}
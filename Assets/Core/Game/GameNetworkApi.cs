// Assets/Core/Network/GameNetworkApi.cs
// 游戏网络API集合 - 封装所有网络API调用
// 管理不同baseUrl的HttpClient实例，提供统一的API调用接口

using System;
using System.Threading.Tasks;
using UnityEngine;
using BaccaratGame.Data;

namespace BaccaratGame.Core
{
    
    #region API请求数据结构

    /// <summary>
    /// 获取台桌信息请求数据
    /// </summary>
    [Serializable]
    public class GetTableInfoRequest
    {
        public string tableId;
        public string gameType;
    }

    /// <summary>
    /// 获取用户台桌信息请求数据
    /// </summary>
    [Serializable]
    public class GetUserTableInfoRequest
    {
        public string id;
    }

    /// <summary>
    /// 获取用户信息请求数据
    /// </summary>
    [Serializable]
    public class GetUserInfoRequest
    {
        public string user_id;
    }

    /// <summary>
    /// 提交投注订单请求数据
    /// </summary>
    [Serializable]
    public class OrderRequest
    {
        public int table_id;
        public int game_type;
        public int is_exempt;
        public object[] bet;
    }

    /// <summary>
    /// 获取投注历史请求数据
    /// </summary>
    [Serializable]
    public class GetBettingHistoryRequest
    {
        public string user_id;
        public string table_id;
        public string game_type;
        public int page;
        public int page_size;
        public string start_date;
        public string end_date;
    }

    #endregion

    /// <summary>
    /// 游戏网络API集合
    /// 封装所有游戏相关的网络API调用
    /// </summary>
    public class GameNetworkApi
    {
        #region 私有字段

        private HttpClient _gameHttpClient;    // 游戏接口客户端 (baseUrl)
        private HttpClient _userHttpClient;    // 用户接口客户端 (userUrl)
        private bool _isInitialized = false;

        #endregion

        #region 单例模式

        private static GameNetworkApi _instance;

        /// <summary>
        /// 单例实例
        /// </summary>
        public static GameNetworkApi Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameNetworkApi();
                    _instance.Initialize();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 重置单例实例（用于重新初始化）
        /// </summary>
        public static void ResetInstance()
        {
            if (_instance != null)
            {
                _instance.Cleanup();
                _instance = null;
            }
        }

        #endregion

        #region 公共属性

        /// <summary>
        /// API是否已初始化
        /// </summary>
        public bool IsInitialized => _isInitialized;

        #endregion

        #region 初始化方法

        /// <summary>
        /// 初始化API客户端
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.Log("[GameNetworkApi] 已经初始化过，跳过");
                return;
            }

            // 🔥 关键修改：初始化前先确保清理旧实例
            CleanupHttpClients();

            var gameParams = GameParams.Instance;
            
            if (!gameParams.IsInitialized)
            {
                throw new InvalidOperationException("GameParams未初始化，请先调用GameParams.Instance.Initialize()");
            }

            Debug.Log($"[GameNetworkApi] 开始创建 HttpClient 实例");
            Debug.Log($"  - Game API URL: {gameParams.httpBaseUrl}");
            Debug.Log($"  - User API URL: {gameParams.userUrl}");

            // 创建游戏接口客户端 (baseUrl)
            _gameHttpClient = HttpClient.Create(
                gameParams.httpBaseUrl,
                gameParams.token
            );

            // 创建用户接口客户端 (userUrl)  
            _userHttpClient = HttpClient.Create(
                gameParams.userUrl,
                gameParams.token
            );

            _isInitialized = true;
            Debug.Log("[GameNetworkApi] API客户端初始化完成");
        }

        /// <summary>
        /// 清理 HttpClient 实例（内部方法）
        /// </summary>
        private void CleanupHttpClients()
        {
            if (_gameHttpClient != null)
            {
                Debug.Log("[GameNetworkApi] 销毁现有 Game HttpClient");
                UnityEngine.Object.Destroy(_gameHttpClient.gameObject);
                _gameHttpClient = null;
            }

            if (_userHttpClient != null)
            {
                Debug.Log("[GameNetworkApi] 销毁现有 User HttpClient");
                UnityEngine.Object.Destroy(_userHttpClient.gameObject);
                _userHttpClient = null;
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Cleanup()
        {
            Debug.Log("[GameNetworkApi] 开始清理资源");
            
            CleanupHttpClients();

            _isInitialized = false;
            Debug.Log("[GameNetworkApi] 资源已清理");
        }

        /// <summary>
        /// 强制重新初始化（用于调试）
        /// </summary>
        public void ForceReinitialize()
        {
            Debug.Log("[GameNetworkApi] 强制重新初始化");
            
            Cleanup();
            Initialize();
            
            Debug.Log("[GameNetworkApi] 强制重新初始化完成");
        }

        #endregion

        #region 游戏相关API (baseUrl) - 全部改为POST请求

        /// <summary>
        /// 获取台桌信息 - 修复版本
        /// </summary>
        /// <returns>台桌信息</returns>
        public async Task<string> GetTableInfo()
        {
            EnsureInitialized();

            var gameParams = GameParams.Instance;
            var requestData = new GetUserTableInfoRequest  // 使用这个结构
            {
                id = gameParams.table_id
            };

            Debug.Log($"[GameNetworkApi] 正在获取台桌信息: user_id={gameParams.user_id}, table_id={gameParams.table_id}");

            try
            {
                // 使用用户API客户端，调用 user/table/info 端点
                var result = await _userHttpClient.PostAsync("user/table/info", requestData);
                Debug.Log($"[GameNetworkApi] 台桌信息获取成功");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameNetworkApi] 获取台桌信息失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 获取用户台桌信息 (如果需要的话)
        /// </summary>
        /// <returns>用户台桌相关信息</returns>
        public async Task<string> GetUserTableInfo()
        {
            EnsureInitialized();

            var gameParams = GameParams.Instance;
            var requestData = new GetUserTableInfoRequest
            {
                id = gameParams.table_id
            };

            Debug.Log($"[GameNetworkApi] 正在获取用户台桌信息: user_id={gameParams.user_id}");

            try
            {
                var result = await _gameHttpClient.PostAsync("user/table/info", requestData);
                Debug.Log("[GameNetworkApi] 用户台桌信息获取成功");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameNetworkApi] 获取用户台桌信息失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 提交投注订单
        /// </summary>
        /// <param name="bets">投注数据</param>
        /// <returns>投注结果</returns>
        public async Task<string> Order(object[] bets)
        {
            EnsureInitialized();

            var gameParams = GameParams.Instance;
            var requestData = new OrderRequest
            {
                table_id = int.Parse(gameParams.table_id),
                game_type = int.Parse(gameParams.game_type),
                is_exempt = 0,
                bet = bets
            };

            Debug.Log($"[GameNetworkApi] 正在提交投注订单: table_id={gameParams.table_id}, 投注数量={bets.Length}");

            try
            {
                var result = await _gameHttpClient.PostAsync("bjl/bet/order", requestData);
                Debug.Log("[GameNetworkApi] 投注订单提交成功");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameNetworkApi] 投注订单提交失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 获取投注历史记录
        /// </summary>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>投注历史记录</returns>
        public async Task<string> GetBettingHistory(int page = 1, int pageSize = 20, string startDate = null, string endDate = null)
        {
            EnsureInitialized();

            var gameParams = GameParams.Instance;
            var requestData = new GetBettingHistoryRequest
            {
                user_id = gameParams.user_id,
                table_id = gameParams.table_id,
                game_type = gameParams.game_type,
                page = page,
                page_size = pageSize,
                start_date = startDate,
                end_date = endDate
            };

            Debug.Log($"[GameNetworkApi] 正在获取投注历史: user_id={gameParams.user_id}, page={page}");

            try
            {
                var result = await _gameHttpClient.PostAsync("bjl/bet/history", requestData);
                Debug.Log("[GameNetworkApi] 投注历史获取成功");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameNetworkApi] 获取投注历史失败: {ex.Message}");
                throw;
            }
        }

        #endregion

        // 在 GameNetworkApi.cs 文件的 API请求数据结构 区域添加：

        /// <summary>
        /// 获取台桌统计数据请求
        /// </summary>
        [Serializable]
        public class GetTableBetStatsRequest
        {
            public string table_id;
        }

        // 在 用户相关API (userUrl) 区域添加这个方法：

        /// <summary>
        /// 获取台桌投注统计数据
        /// </summary>
        /// <returns>台桌统计数据</returns>
        public async Task<string> GetTableBetStats()
        {
            EnsureInitialized();

            var gameParams = GameParams.Instance;
            var requestData = new GetTableBetStatsRequest
            {
                table_id = gameParams.table_id
            };

            Debug.Log($"[GameNetworkApi] 正在获取台桌统计数据: table_id={gameParams.table_id}");

            try
            {
                var result = await _userHttpClient.PostAsync("user/table/bet", requestData);
                Debug.Log($"[GameNetworkApi] 台桌统计数据获取成功");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameNetworkApi] 获取台桌统计数据失败: {ex.Message}");
                throw;
            }
        }

        #region 用户相关API (userUrl) - 全部改为POST请求

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns>用户信息</returns>
        public async Task<string> GetUserInfo()
        {
            EnsureInitialized();

            var gameParams = GameParams.Instance;
            var requestData = new GetUserInfoRequest
            {
                user_id = gameParams.user_id
            };

            Debug.Log($"[GameNetworkApi] 正在获取用户信息: user_id={gameParams.user_id}");

            try
            {
                var result = await _userHttpClient.PostAsync("user/user/index", requestData);
                Debug.Log($"[GameNetworkApi] 用户信息获取成功");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameNetworkApi] 获取用户信息失败: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region iframe地址构建

        /// <summary>
        /// 构建路单iframe地址
        /// </summary>
        /// <returns>路单iframe完整地址</returns>
        public string BuildRoadmapIframeUrl()
        {
            var gameParams = GameParams.Instance;
            var lzUrl = gameParams.lzUrl;
            var tableId = gameParams.table_id;
            var userId = gameParams.user_id;
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var url = $"{lzUrl}/bjl/get_table/get_data?tableId={tableId}&user_id={userId}&t={timestamp}";
            
            Debug.Log($"[GameNetworkApi] 路单iframe地址: {url}");
            return url;
        }

        /// <summary>
        /// 构建视频iframe地址
        /// </summary>
        /// <param name="baseVideoUrl">基础视频地址</param>
        /// <returns>视频iframe完整地址</returns>
        public string BuildVideoIframeUrl(string baseVideoUrl)
        {
            if (string.IsNullOrEmpty(baseVideoUrl))
            {
                Debug.LogWarning("[GameNetworkApi] 视频地址为空");
                return "";
            }

            var tableId = GameParams.Instance.table_id;
            var url = $"{baseVideoUrl}{tableId}";
            
            Debug.Log($"[GameNetworkApi] 视频iframe地址: {url}");
            return url;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 确保API已初始化
        /// </summary>
        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("GameNetworkApi未初始化，请先调用Initialize()");
            }
        }

        #endregion

        #region 静态便捷方法

        /// <summary>
        /// 创建并初始化GameNetworkApi实例
        /// </summary>
        /// <returns>已初始化的GameNetworkApi实例</returns>
        public static GameNetworkApi CreateAndInitialize()
        {
            var api = new GameNetworkApi();
            api.Initialize();
            return api;
        }

        #endregion
    }
}

// TODO: 将来可扩展的API方法
// - public async Task<string> GetGameRules();
// - public async Task<string> GetTableList();
// - public async Task<string> GetGameStatistics();
// Assets/Core/Network/GameNetworkApi.cs
// 游戏网络API集合 - 封装所有网络API调用
// 管理不同baseUrl的HttpClient实例，提供统一的API调用接口

using System;
using System.Threading.Tasks;
using UnityEngine;
using BaccaratGame.Data;

namespace BaccaratGame.Core
{
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

        #region 游戏相关API (baseUrl)

        /// <summary>
        /// 获取台桌信息
        /// </summary>
        /// <returns>台桌信息</returns>
        public async Task<TableInfo> GetTableInfo()
        {
            EnsureInitialized();

            var gameParams = GameParams.Instance;
            var queryParams = new
            {
                tableId = gameParams.table_id,
                gameType = gameParams.game_type
            };

            Debug.Log($"[GameNetworkApi] 正在获取台桌信息: tableId={gameParams.table_id}, gameType={gameParams.game_type}");

            try
            {
                var result = await _gameHttpClient.GetAsync<TableInfo>("bjl/get_table/table_info", queryParams);
                Debug.Log($"[GameNetworkApi] 台桌信息获取成功: {result?.table_title}");
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
        public async Task<object> GetUserTableInfo()
        {
            EnsureInitialized();

            var gameParams = GameParams.Instance;
            var queryParams = new
            {
                user_id = gameParams.user_id,
                table_id = gameParams.table_id
            };

            Debug.Log($"[GameNetworkApi] 正在获取用户台桌信息: user_id={gameParams.user_id}");

            try
            {
                var result = await _gameHttpClient.GetAsync<object>("user/table/info", queryParams);
                Debug.Log("[GameNetworkApi] 用户台桌信息获取成功");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameNetworkApi] 获取用户台桌信息失败: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region 用户相关API (userUrl)

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns>用户信息</returns>
        public async Task<UserInfo> GetUserInfo()
        {
            EnsureInitialized();

            var gameParams = GameParams.Instance;
            var queryParams = new
            {
                user_id = gameParams.user_id
            };

            Debug.Log($"[GameNetworkApi] 正在获取用户信息: user_id={gameParams.user_id}");

            try
            {
                var result = await _userHttpClient.GetAsync<UserInfo>("user/user/index", queryParams);
                Debug.Log($"[GameNetworkApi] 用户信息获取成功: {result?.user_name}, 余额: {result?.money_balance}");
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

        #region 便捷方法

        /// <summary>
        /// 获取基础数据（台桌信息和用户信息）
        /// </summary>
        /// <returns>台桌信息和用户信息</returns>
        public async Task<(TableInfo tableInfo, UserInfo userInfo)> GetBasicData()
        {
            EnsureInitialized();

            Debug.Log("[GameNetworkApi] 开始并行获取基础数据");

            try
            {
                // 并行获取台桌信息和用户信息
                var tableInfoTask = GetTableInfo();
                var userInfoTask = GetUserInfo();

                await Task.WhenAll(tableInfoTask, userInfoTask);

                var tableInfo = await tableInfoTask;
                var userInfo = await userInfoTask;

                Debug.Log("[GameNetworkApi] 基础数据获取完成");
                return (tableInfo, userInfo);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameNetworkApi] 获取基础数据失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 安全获取用户信息（失败时返回默认值）
        /// </summary>
        /// <returns>用户信息或默认值</returns>
        public async Task<UserInfo> GetUserInfoSafely()
        {
            try
            {
                return await GetUserInfo();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GameNetworkApi] 获取用户信息失败，使用默认值: {ex.Message}");

                var gameParams = GameParams.Instance;
                return new UserInfo
                {
                    id = int.TryParse(gameParams.user_id, out int userId) ? userId : 0,
                    user_name = "游客",
                    money_balance = 0m,
                    status = 1
                };
            }
        }

        /// <summary>
        /// 安全获取台桌信息（失败时返回默认值）
        /// </summary>
        /// <returns>台桌信息或默认值</returns>
        public async Task<TableInfo> GetTableInfoSafely()
        {
            try
            {
                return await GetTableInfo();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GameNetworkApi] 获取台桌信息失败，使用默认值: {ex.Message}");

                var gameParams = GameParams.Instance;
                return new TableInfo
                {
                    id = int.TryParse(gameParams.table_id, out int tableId) ? tableId : 1,
                    table_title = "百家乐桌台",
                    game_type = int.TryParse(gameParams.game_type, out int gameType) ? gameType : 3,
                    status = 1,
                    run_status = 1
                };
            }
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
// - public async Task<BetResult> PlaceBet(BetRequest bet);
// - public async Task<List<GameHistory>> GetGameHistory();
// - public async Task<decimal> GetUserBalance();
// - public async Task<List<TableInfo>> GetTableList();
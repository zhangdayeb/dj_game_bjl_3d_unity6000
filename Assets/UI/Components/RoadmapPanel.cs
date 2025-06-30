using System;
using UnityEngine;
using BaccaratGame.Data;

namespace BaccaratGame.UI.Components
{
    /// <summary>
    /// 路单面板 - 调整版
    /// 从配置文件lzUrl读取地址，调用前端增强版LoadRoadmapIframe函数
    /// </summary>
    public class RoadmapPanel : MonoBehaviour
    {
        #region 配置参数

        [Header("🔗 iframe配置")]
        [Tooltip("iframe容器ID")]
        public string iframeContainerId = "roadmap-container";
        
        [Tooltip("是否在Start时自动加载")]
        public bool autoLoadOnStart = true;

        #endregion

        #region 私有字段

        private bool _isLoaded = false;
        private string _currentRoadmapUrl = "";

        #endregion

        #region Unity生命周期

        private void Start()
        {
            Debug.Log("[RoadmapPanel] 路单面板组件已启动");
                
            if (autoLoadOnStart)
            {
                // 延迟一点确保GameParams已初始化
                Invoke(nameof(LoadRoadmapIframe), 1f);
            }
        }

        #endregion

        #region 核心功能

        /// <summary>
        /// 加载路单iframe
        /// </summary>
        public void LoadRoadmapIframe()
        {
            try
            {
                Debug.Log("[RoadmapPanel] 开始加载路单iframe");

                // 检查GameParams是否已初始化
                if (!GameParams.Instance.IsInitialized)
                {
                    Debug.LogWarning("[RoadmapPanel] GameParams未初始化，延迟重试");
                    Invoke(nameof(LoadRoadmapIframe), 2f);
                    return;
                }

                // 从配置文件获取lzUrl
                string lzUrl = GameParams.Instance.GetLzUrl();
                
                if (string.IsNullOrEmpty(lzUrl))
                {
                    Debug.LogWarning("[RoadmapPanel] lzUrl为空，无法加载路单");
                    return;
                }

                // 构建完整URL（添加参数）
                string fullUrl = BuildRoadmapUrl(lzUrl);
                
                if (string.IsNullOrEmpty(fullUrl))
                {
                    Debug.LogError("[RoadmapPanel] URL构建失败");
                    return;
                }

                // 调用前端增强版的LoadRoadmapIframe函数
                CallLoadRoadmapIframe(fullUrl);

                // 更新状态
                _currentRoadmapUrl = fullUrl;
                _isLoaded = true;

                Debug.Log($"[RoadmapPanel] 路单iframe加载成功: {fullUrl}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoadmapPanel] 加载路单iframe失败: {ex.Message}");
                _isLoaded = false;
            }
        }

        /// <summary>
        /// 重新加载路单iframe
        /// </summary>
        public void ReloadRoadmapIframe()
        {
            Debug.Log("[RoadmapPanel] 重新加载路单iframe");

            _isLoaded = false;
            _currentRoadmapUrl = "";
            
            LoadRoadmapIframe();
        }

        /// <summary>
        /// 刷新路单iframe（WebSocket触发）
        /// </summary>
        public void RefreshRoadmapIframe()
        {
            try
            {
                // 调用前端的RefreshRoadmapIframe函数
                CallRefreshRoadmapIframe();
                
                Debug.Log("[RoadmapPanel] 路单iframe已刷新");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoadmapPanel] 刷新路单iframe失败: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 构建路单完整URL
        /// </summary>
        /// <param name="baseUrl">基础URL</param>
        /// <returns>完整URL</returns>
        private string BuildRoadmapUrl(string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
                return "";

            try
            {
                // 获取tableId参数
                string tableId = GameParams.Instance.table_id;
                
                if (string.IsNullOrEmpty(tableId))
                {
                    Debug.LogWarning("[RoadmapPanel] tableId为空");
                    return baseUrl; // 返回原URL
                }

                // 构建完整URL（只添加tableId和时间戳）
                string separator = baseUrl.Contains("?") ? "&" : "?";
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                string fullUrl = $"{baseUrl}{separator}tableId={tableId}&t={timestamp}";

                Debug.Log($"[RoadmapPanel] URL构建: {baseUrl} -> {fullUrl}");

                return fullUrl;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoadmapPanel] URL构建失败: {ex.Message}");
                return baseUrl; // 发生错误时返回原URL
            }
        }

        /// <summary>
        /// 调用前端的LoadRoadmapIframe函数
        /// </summary>
        /// <param name="roadmapUrl">路单URL</param>
        private void CallLoadRoadmapIframe(string roadmapUrl)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                // 优先调用前端增强版的LoadRoadmapIframe函数
                string jsFunction = $"window.LoadRoadmapIframe('{iframeContainerId}', '{roadmapUrl}')";
                WebGLJavaScriptBridge.ExecuteJS;
                
                Debug.Log($"[RoadmapPanel] 调用LoadRoadmapIframe成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoadmapPanel] 调用LoadRoadmapIframe失败: {ex.Message}");
                
                // 降级到通用iframe函数
                CallWebGLFunction("loadIframe", $"{iframeContainerId},{roadmapUrl}");
            }
#else
            Debug.Log($"[RoadmapPanel] 编辑器模式 - 模拟调用LoadRoadmapIframe: {iframeContainerId}, {roadmapUrl}");
#endif
        }

        /// <summary>
        /// 调用前端的RefreshRoadmapIframe函数
        /// </summary>
        private void CallRefreshRoadmapIframe()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                // 调用前端增强版的RefreshRoadmapIframe函数
                string jsFunction = "window.RefreshRoadmapIframe()";
                WebGLJavaScriptBridge.ExecuteJS;
                
                Debug.Log($"[RoadmapPanel] 调用RefreshRoadmapIframe成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoadmapPanel] 调用RefreshRoadmapIframe失败: {ex.Message}");
                
                // 降级到通用刷新函数
                CallWebGLFunction("refreshIframe", iframeContainerId);
            }
#else
            Debug.Log($"[RoadmapPanel] 编辑器模式 - 模拟调用RefreshRoadmapIframe");
#endif
        }

        /// <summary>
        /// 调用通用WebGL函数
        /// </summary>
        /// <param name="functionName">函数名</param>
        /// <param name="parameter">参数</param>
        private void CallWebGLFunction(string functionName, string parameter)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                string jsFunction = $"window.{functionName}('{parameter}')";
                WebGLJavaScriptBridge.ExecuteJS;
                
                Debug.Log($"[RoadmapPanel] 调用JS函数成功: {functionName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoadmapPanel] 调用{functionName}失败: {ex.Message}");
            }
#else
            Debug.Log($"[RoadmapPanel] 编辑器模式 - 模拟调用{functionName}: {parameter}");
#endif
        }

        #endregion

        #region 公共属性

        /// <summary>
        /// 是否已加载
        /// </summary>
        public bool IsLoaded => _isLoaded;

        /// <summary>
        /// 当前路单URL
        /// </summary>
        public string CurrentRoadmapUrl => _currentRoadmapUrl;

        #endregion
    }
}
using System;
using UnityEngine;

namespace BaccaratGame.UI.Components
{
    /// <summary>
    /// 路单面板 - 简化版本
    /// 只负责加载iframe，不创建UI界面
    /// </summary>
    public class RoadmapPanel : MonoBehaviour
    {
        #region 序列化字段

        [Header("🔗 iframe配置")]
        [Tooltip("路单完整URL")]
        public string roadmapUrl = "https://h5lzv3.wuming888.com/zh/bjl_xc_big_678.html";
        [Tooltip("桌台ID")]
        public int tableId = 1;
        [Tooltip("用户ID")]
        public int userId = 824;
        [Tooltip("是否自动添加时间戳")]
        public bool autoAddTimestamp = true;

        [Header("🖼️ iframe设置")]
        [Tooltip("iframe容器ID")]
        public string iframeContainerId = "roadmap-container";
        [Tooltip("iframe宽度 (可以是像素或百分比)")]
        public string iframeWidth = "100%";
        [Tooltip("iframe高度 (像素)")]
        public int iframeHeight = 300;

        [Header("🔥 自动加载")]
        [Tooltip("启动时自动加载")]
        public bool autoLoadOnStart = true;
        [Tooltip("延迟加载时间(秒)")]
        public float loadDelay = 0.5f;

        [Header("🐛 调试")]
        public bool enableDebugMode = true;

        #endregion

        #region 私有字段

        private string currentIframeUrl = "";

        #endregion

        #region Unity生命周期

        private void Start()
        {
            if (autoLoadOnStart)
            {
                if (loadDelay > 0)
                {
                    Invoke(nameof(LoadRoadmapIframe), loadDelay);
                }
                else
                {
                    LoadRoadmapIframe();
                }
            }
        }

        #endregion

        #region 🎯 核心功能

        /// <summary>
        /// 加载路单iframe
        /// </summary>
        [ContextMenu("🔄 加载路单iframe")]
        public void LoadRoadmapIframe()
        {
            // 构建完整URL
            string fullUrl = BuildRoadmapUrl();
            currentIframeUrl = fullUrl;
            
            // 使用与VideoController相同的调用方式
            CallWebGLFunction("loadIframe", $"{iframeContainerId},{fullUrl}");
            
            LogDebug($"加载路单iframe: {fullUrl}");
        }

        /// <summary>
        /// 刷新路单
        /// </summary>
        [ContextMenu("🔄 刷新路单")]
        public void RefreshRoadmap()
        {
            LoadRoadmapIframe();
            LogDebug("路单已刷新");
        }

        /// <summary>
        /// 设置桌台ID并刷新
        /// </summary>
        public void SetTableId(int newTableId)
        {
            if (tableId != newTableId)
            {
                tableId = newTableId;
                LoadRoadmapIframe();
                LogDebug($"桌台ID更新为: {newTableId}");
            }
        }

        /// <summary>
        /// 设置用户ID并刷新
        /// </summary>
        public void SetUserId(int newUserId)
        {
            if (userId != newUserId)
            {
                userId = newUserId;
                LoadRoadmapIframe();
                LogDebug($"用户ID更新为: {newUserId}");
            }
        }

        /// <summary>
        /// 设置路单URL并刷新
        /// </summary>
        public void SetRoadmapUrl(string newUrl)
        {
            if (!string.IsNullOrEmpty(newUrl) && roadmapUrl != newUrl)
            {
                roadmapUrl = newUrl;
                LoadRoadmapIframe();
                LogDebug($"路单URL更新为: {newUrl}");
            }
        }

        #endregion

        #region 🔧 辅助方法

        /// <summary>
        /// 构建完整的路单URL
        /// </summary>
        private string BuildRoadmapUrl()
        {
            if (string.IsNullOrEmpty(roadmapUrl))
            {
                LogError("路单URL未设置");
                return "";
            }

            // 移除原有的查询参数
            string baseUrl = roadmapUrl.Split('?')[0];
            
            // 构建新的查询参数
            string queryParams = $"tableId={tableId}&user_id={userId}";
            
            // 添加时间戳（如果启用）
            if (autoAddTimestamp)
            {
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                queryParams += $"&t={timestamp}";
            }
            
            return $"{baseUrl}?{queryParams}";
        }

        /// <summary>
        /// 调用WebGL函数
        /// </summary>
        private void CallWebGLFunction(string functionName, string parameter)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                // 使用与VideoController完全相同的调用方式
                Application.ExternalEval($"window.{functionName}('{parameter}')");
                LogDebug($"WebGL函数调用成功: {functionName}");
            }
            catch (System.Exception e)
            {
                LogError($"WebGL函数调用失败: {e.Message}");
            }
#else
            LogDebug($"模拟WebGL调用: {functionName}({parameter})");
#endif
        }

        #endregion

        #region 🔍 公共接口

        /// <summary>
        /// 获取当前iframe URL
        /// </summary>
        public string GetCurrentUrl()
        {
            return currentIframeUrl;
        }

        /// <summary>
        /// 批量设置参数并刷新
        /// </summary>
        public void SetRoadmapParams(int newTableId, int newUserId, string newUrl = null)
        {
            bool needRefresh = false;

            if (tableId != newTableId)
            {
                tableId = newTableId;
                needRefresh = true;
            }

            if (userId != newUserId)
            {
                userId = newUserId;
                needRefresh = true;
            }

            if (!string.IsNullOrEmpty(newUrl) && roadmapUrl != newUrl)
            {
                roadmapUrl = newUrl;
                needRefresh = true;
            }

            if (needRefresh)
            {
                LoadRoadmapIframe();
                LogDebug($"路单参数批量更新: tableId={tableId}, userId={userId}");
            }
        }

        #endregion

        #region 🐛 调试方法

        /// <summary>
        /// 调试日志
        /// </summary>
        private void LogDebug(string message)
        {
            if (enableDebugMode)
            {
                Debug.Log($"[RoadmapPanel] {message}");
            }
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        private void LogError(string message)
        {
            Debug.LogError($"[RoadmapPanel] ❌ {message}");
        }

        /// <summary>
        /// 显示当前状态
        /// </summary>
        [ContextMenu("📊 显示状态")]
        public void ShowStatus()
        {
            Debug.Log("=== RoadmapPanel 状态 ===");
            Debug.Log($"🔗 基础URL: {roadmapUrl}");
            Debug.Log($"🎯 桌台ID: {tableId}");
            Debug.Log($"👤 用户ID: {userId}");
            Debug.Log($"⏰ 自动时间戳: {autoAddTimestamp}");
            Debug.Log($"🔄 自动加载: {autoLoadOnStart}");
            Debug.Log($"📏 iframe尺寸: {iframeWidth} x {iframeHeight}px");
            Debug.Log($"📦 容器ID: {iframeContainerId}");
            Debug.Log($"🌐 当前完整URL: {currentIframeUrl}");
        }

        /// <summary>
        /// 测试iframe加载
        /// </summary>
        [ContextMenu("🧪 测试iframe加载")]
        public void TestIframeLoad()
        {
            LogDebug("开始测试iframe加载...");
            
            // 测试当前配置
            string testUrl = BuildRoadmapUrl();
            LogDebug($"测试URL: {testUrl}");
            
            // 模拟加载
            LoadRoadmapIframe();
            
            LogDebug("iframe加载测试完成");
        }



        #endregion
    }
}
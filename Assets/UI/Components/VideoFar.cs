using System;
using UnityEngine;
using BaccaratGame.Core;

namespace BaccaratGame.UI.Components
{
    /// <summary>
    /// 远景视频面板 - 核心功能版
    /// 只负责初始化时加载远景视频iframe
    /// </summary>
    public class VideoFar : MonoBehaviour
    {
        #region 配置参数

        [Header("🔗 iframe配置")]
        [Tooltip("iframe容器ID")]
        public string iframeContainerId = "video-far-container";
        
        [Tooltip("是否在Start时自动加载")]
        public bool autoLoadOnStart = true;

        #endregion

        #region API响应数据结构

        [System.Serializable]
        public class ApiResponse
        {
            public int code;
            public string message;
            public TableData data;
        }
        
        [System.Serializable]
        public class TableData
        {
            public string video_far;    // 远景视频地址
            public string video_near;   // 近景视频地址
            // 其他字段根据需要添加
        }

        #endregion

        #region 私有字段

        private bool _isLoaded = false;
        private string _currentVideoUrl = "";

        #endregion

        #region Unity生命周期

        private void Start()
        {
            Debug.Log("[VideoFar] 远景视频组件已启动");
                
            if (autoLoadOnStart)
            {
                // 延迟一点确保GameNetworkApi已初始化
                Invoke(nameof(LoadVideoIframe), 1f);
            }
        }

        #endregion

        #region 核心功能

        /// <summary>
        /// 加载远景视频iframe
        /// </summary>
        public async void LoadVideoIframe()
        {
            try
            {
                Debug.Log("[VideoFar] 开始获取tableInfo以加载远景视频iframe");

                // 检查GameNetworkApi是否可用
                if (GameNetworkApi.Instance == null)
                {
                    Debug.LogWarning("[VideoFar] GameNetworkApi未初始化，延迟重试");
                    Invoke(nameof(LoadVideoIframe), 2f);
                    return;
                }

                // 获取tableInfo接口数据
                string jsonResponse = await GameNetworkApi.Instance.GetTableInfo();
                
                if (string.IsNullOrEmpty(jsonResponse))
                {
                    Debug.LogWarning("[VideoFar] tableInfo接口返回空数据");
                    return;
                }

                // 解析JSON响应
                ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(jsonResponse);
                
                if (apiResponse == null || apiResponse.data == null)
                {
                    Debug.LogWarning("[VideoFar] tableInfo数据结构解析失败");
                    return;
                }

                // 获取video_far地址
                string videoFarUrl = apiResponse.data.video_far;
                
                if (string.IsNullOrEmpty(videoFarUrl))
                {
                    Debug.LogWarning("[VideoFar] video_far地址为空");
                    return;
                }

                // 调用前端加载iframe
                CallLoadVideoIframe(videoFarUrl);

                // 更新状态
                _currentVideoUrl = videoFarUrl;
                _isLoaded = true;

                Debug.Log($"[VideoFar] 远景视频iframe加载成功: {videoFarUrl}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VideoFar] 加载远景视频iframe失败: {ex.Message}");
                _isLoaded = false;
            }
        }

        /// <summary>
        /// 重新加载视频iframe
        /// </summary>
        public void ReloadVideoIframe()
        {
            Debug.Log("[VideoFar] 重新加载远景视频iframe");

            _isLoaded = false;
            _currentVideoUrl = "";
            
            LoadVideoIframe();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 调用前端的LoadVideoIframe函数
        /// </summary>
        /// <param name="videoUrl">视频URL</param>
        private void CallLoadVideoIframe(string videoUrl)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                // 优先调用前端增强版的LoadVideoIframe函数
                string jsFunction = $"window.LoadVideoIframe('{iframeContainerId}', '{videoUrl}')";
                WebGLJavaScriptBridge.ExecuteJS;
                
                Debug.Log($"[VideoFar] 调用LoadVideoIframe成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VideoFar] 调用LoadVideoIframe失败: {ex.Message}");
                
                // 降级到通用iframe函数
                CallWebGLFunction("loadIframe", $"{iframeContainerId},{videoUrl}");
            }
#else
            Debug.Log($"[VideoFar] 编辑器模式 - 模拟调用LoadVideoIframe: {iframeContainerId}, {videoUrl}");
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
                
                Debug.Log($"[VideoFar] 调用JS函数成功: {functionName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VideoFar] 调用{functionName}失败: {ex.Message}");
            }
#else
            Debug.Log($"[VideoFar] 编辑器模式 - 模拟调用{functionName}: {parameter}");
#endif
        }

        #endregion

        #region 公共属性

        /// <summary>
        /// 是否已加载
        /// </summary>
        public bool IsLoaded => _isLoaded;

        /// <summary>
        /// 当前视频URL
        /// </summary>
        public string CurrentVideoUrl => _currentVideoUrl;

        #endregion
    }
}
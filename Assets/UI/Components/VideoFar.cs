using System;
using UnityEngine;

namespace BaccaratGame.UI.Components
{
    /// <summary>
    /// 远景视频面板 - 极简版
    /// 只负责生成 iframe
    /// </summary>
    public class VideoFar : MonoBehaviour
    {
        [Header("🔗 基础配置")]
        [Tooltip("视频URL")]
        public string videoUrl = "https://www.google.com";
        
        [Tooltip("桌台ID")]
        public int tableId = 1;
        
        [Tooltip("用户ID")]
        public int userId = 824;
        
        [Tooltip("iframe容器ID")]
        public string iframeContainerId = "video-far-container";

        /// <summary>
        /// 生成并加载远景视频 iframe
        /// </summary>
        public void LoadVideoIframe()
        {
            string fullUrl = BuildUrl();
            CallWebGLFunction("loadIframe", $"{iframeContainerId},{fullUrl}");
        }

        /// <summary>
        /// 构建完整URL
        /// </summary>
        private string BuildUrl()
        {
            if (string.IsNullOrEmpty(videoUrl)) return "";
            
            string baseUrl = videoUrl.Split('?')[0];
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            return $"{baseUrl}?tableId={tableId}&user_id={userId}&t={timestamp}";
        }

        /// <summary>
        /// 调用WebGL函数
        /// </summary>
        private void CallWebGLFunction(string functionName, string parameter)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalEval($"window.{functionName}('{parameter}')");
#endif
        }
    }
}

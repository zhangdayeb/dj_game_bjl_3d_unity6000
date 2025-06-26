// Assets/Core/Network/HttpClient.cs
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Core.Network
{
    /// <summary>
    /// HTTP客户端 - 简化版，只提供GET和POST功能
    /// 自动携带Token，完整日志打印，按需创建使用
    /// </summary>
    public class HttpClient : MonoBehaviour
    {
        #region 私有字段

        private string _baseUrl;
        private string _authToken;
        private Dictionary<string, string> _defaultHeaders;

        #endregion

        #region 初始化

        private void Awake()
        {
            InitializeHeaders();
        }

        private void InitializeHeaders()
        {
            _defaultHeaders = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"},
                {"Accept", "application/json"},
                {"User-Agent", $"Unity/{Application.unityVersion}"}
            };
            
            Debug.Log("[HttpClient] HTTP客户端已创建");
        }

        #endregion

        #region 配置方法

        /// <summary>
        /// 设置基础URL
        /// </summary>
        /// <param name="baseUrl">API基础地址</param>
        public void SetBaseUrl(string baseUrl)
        {
            _baseUrl = baseUrl?.TrimEnd('/');
            Debug.Log($"[HttpClient] 基础URL设置为: {_baseUrl}");
        }

        /// <summary>
        /// 设置认证Token，会自动添加到所有请求头中
        /// </summary>
        /// <param name="token">认证令牌</param>
        public void SetAuthToken(string token)
        {
            _authToken = token;
            
            if (!string.IsNullOrEmpty(token))
            {
                _defaultHeaders["Authorization"] = $"Bearer {token}";
                Debug.Log("[HttpClient] Token已设置，将自动携带到所有请求中");
            }
            else
            {
                _defaultHeaders.Remove("Authorization");
                Debug.Log("[HttpClient] Token已清除");
            }
        }

        #endregion

        #region HTTP请求方法

        /// <summary>
        /// 发送GET请求
        /// </summary>
        /// <typeparam name="T">响应数据类型</typeparam>
        /// <param name="endpoint">API端点</param>
        /// <param name="queryParams">查询参数对象</param>
        /// <returns>解析后的响应数据</returns>
        public async Task<T> GetAsync<T>(string endpoint, object queryParams = null) where T : class
        {
            var url = BuildUrl(endpoint, queryParams);
            
            Debug.Log($"[HttpClient] ==== GET请求开始 ====");
            Debug.Log($"[HttpClient] 请求URL: {url}");
            Debug.Log($"[HttpClient] 查询参数: {(queryParams != null ? JsonUtility.ToJson(queryParams) : "无")}");
            
            using (var request = UnityWebRequest.Get(url))
            {
                SetHeaders(request);
                var result = await SendRequest<T>(request, "GET");
                
                Debug.Log($"[HttpClient] ==== GET请求完成 ====");
                return result;
            }
        }

        /// <summary>
        /// 发送POST请求
        /// </summary>
        /// <typeparam name="T">响应数据类型</typeparam>
        /// <param name="endpoint">API端点</param>
        /// <param name="data">请求体数据</param>
        /// <returns>解析后的响应数据</returns>
        public async Task<T> PostAsync<T>(string endpoint, object data = null) where T : class
        {
            var url = BuildUrl(endpoint);
            var jsonData = data != null ? JsonUtility.ToJson(data) : "{}";
            
            Debug.Log($"[HttpClient] ==== POST请求开始 ====");
            Debug.Log($"[HttpClient] 请求URL: {url}");
            Debug.Log($"[HttpClient] 请求数据: {jsonData}");
            
            using (var request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                
                SetHeaders(request);
                var result = await SendRequest<T>(request, "POST");
                
                Debug.Log($"[HttpClient] ==== POST请求完成 ====");
                return result;
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 构建完整的请求URL
        /// </summary>
        private string BuildUrl(string endpoint, object queryParams = null)
        {
            if (string.IsNullOrEmpty(_baseUrl))
            {
                throw new InvalidOperationException("基础URL未设置，请先调用SetBaseUrl()");
            }
            
            var url = _baseUrl + "/" + endpoint.TrimStart('/');
            
            if (queryParams != null)
            {
                var queryString = BuildQueryString(queryParams);
                if (!string.IsNullOrEmpty(queryString))
                {
                    url += "?" + queryString;
                }
            }
            
            return url;
        }

        /// <summary>
        /// 构建查询字符串
        /// </summary>
        private string BuildQueryString(object queryParams)
        {
            if (queryParams == null) return "";
            
            try
            {
                var json = JsonUtility.ToJson(queryParams);
                var dict = JsonUtility.FromJson<Dictionary<string, object>>(json);
                
                var queryParts = new List<string>();
                foreach (var kvp in dict)
                {
                    if (kvp.Value != null)
                    {
                        var value = UnityWebRequest.EscapeURL(kvp.Value.ToString());
                        queryParts.Add($"{kvp.Key}={value}");
                    }
                }
                
                return string.Join("&", queryParts);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[HttpClient] 构建查询字符串失败: {ex.Message}");
                return "";
            }
        }

        /// <summary>
        /// 设置请求头
        /// </summary>
        private void SetHeaders(UnityWebRequest request)
        {
            foreach (var header in _defaultHeaders)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
            
            Debug.Log($"[HttpClient] 请求头已设置，包含Token: {(!string.IsNullOrEmpty(_authToken) ? "是" : "否")}");
        }

        /// <summary>
        /// 发送请求并处理响应
        /// </summary>
        private async Task<T> SendRequest<T>(UnityWebRequest request, string method) where T : class
        {
            var startTime = DateTime.UtcNow;
            
            // 发送请求
            var operation = request.SendWebRequest();
            
            // 等待完成
            while (!operation.isDone)
            {
                await Task.Delay(50);
            }
            
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            
            Debug.Log($"[HttpClient] 请求耗时: {duration:F0}ms");
            Debug.Log($"[HttpClient] 响应状态码: {request.responseCode}");
            
            // 检查错误
            if (request.result != UnityWebRequest.Result.Success)
            {
                var errorMsg = $"HTTP {method}请求失败: {request.error} (状态码: {request.responseCode})";
                Debug.LogError($"[HttpClient] {errorMsg}");
                Debug.LogError($"[HttpClient] 错误响应: {request.downloadHandler.text}");
                throw new Exception(errorMsg);
            }
            
            // 获取响应数据
            var responseText = request.downloadHandler.text;
            Debug.Log($"[HttpClient] 响应数据: {responseText}");
            
            // 解析响应
            return ParseResponse<T>(responseText);
        }

        /// <summary>
        /// 解析响应数据
        /// </summary>
        private T ParseResponse<T>(string responseText) where T : class
        {
            if (string.IsNullOrEmpty(responseText))
            {
                Debug.LogWarning("[HttpClient] 响应数据为空");
                return null;
            }
            
            try
            {
                var result = JsonUtility.FromJson<T>(responseText);
                Debug.Log($"[HttpClient] 数据解析成功，类型: {typeof(T).Name}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HttpClient] 解析响应数据失败: {ex.Message}");
                Debug.LogError($"[HttpClient] 原始响应: {responseText}");
                throw new Exception($"解析响应数据失败: {ex.Message}");
            }
        }

        #endregion

        #region 静态便捷方法

        /// <summary>
        /// 创建HttpClient实例的便捷方法
        /// </summary>
        /// <param name="baseUrl">基础URL</param>
        /// <param name="token">认证Token（可选）</param>
        /// <returns>配置好的HttpClient实例</returns>
        public static HttpClient Create(string baseUrl, string token = null)
        {
            var go = new GameObject("HttpClient");
            var client = go.AddComponent<HttpClient>();
            
            client.SetBaseUrl(baseUrl);
            if (!string.IsNullOrEmpty(token))
            {
                client.SetAuthToken(token);
            }
            
            Debug.Log($"[HttpClient] 已创建HttpClient实例: {baseUrl}");
            return client;
        }

        #endregion
    }
}
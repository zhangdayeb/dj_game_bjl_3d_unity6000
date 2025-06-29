// Assets/Core/Network/HttpClient.cs
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BaccaratGame.Core
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
        private static int _requestCounter = 0; // 请求计数器

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
            
            Debug.Log($"[HttpClient] HTTP客户端已创建 - GameObject: {gameObject.name}");
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
            Debug.Log($"[HttpClient][{gameObject.name}] 基础URL设置为: {_baseUrl}");
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
                _defaultHeaders["x-csrf-token"] = $"{token}";
                var tokenPreview = token.Length > 10 ? token.Substring(0, 10) + "..." : token;
                Debug.Log($"[HttpClient][{gameObject.name}] Token已设置: {tokenPreview}");
            }
            else
            {
                _defaultHeaders.Remove("x-csrf-token");
                Debug.Log($"[HttpClient][{gameObject.name}] Token已清除");
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
            var requestId = ++_requestCounter;
            var url = BuildUrl(endpoint, queryParams);
            
            Debug.Log($"[HttpClient][请求#{requestId}] ================ GET请求开始 ================");
            Debug.Log($"[HttpClient][请求#{requestId}] 客户端: {gameObject.name}");
            Debug.Log($"[HttpClient][请求#{requestId}] 完整URL: {url}");
            Debug.Log($"[HttpClient][请求#{requestId}] 端点: {endpoint}");
            
            // 详细打印查询参数
            if (queryParams != null)
            {
                try
                {
                    var queryJson = JsonUtility.ToJson(queryParams, true);
                    Debug.Log($"[HttpClient][请求#{requestId}] 查询参数 (JSON):\n{queryJson}");
                    
                    var queryString = BuildQueryString(queryParams);
                    Debug.Log($"[HttpClient][请求#{requestId}] 查询字符串: {queryString}");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[HttpClient][请求#{requestId}] 查询参数序列化失败: {ex.Message}");
                }
            }
            else
            {
                Debug.Log($"[HttpClient][请求#{requestId}] 查询参数: 无");
            }
            
            using (var request = UnityWebRequest.Get(url))
            {
                SetHeaders(request, requestId);
                var result = await SendRequest<T>(request, "GET", requestId);
                
                Debug.Log($"[HttpClient][请求#{requestId}] ================ GET请求完成 ================");
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
            var requestId = ++_requestCounter;
            var url = BuildUrl(endpoint);
            var jsonData = data != null ? JsonUtility.ToJson(data, true) : "{}";
            
            Debug.Log($"[HttpClient][请求#{requestId}] ================ POST请求开始 ================");
            Debug.Log($"[HttpClient][请求#{requestId}] 客户端: {gameObject.name}");
            Debug.Log($"[HttpClient][请求#{requestId}] 完整URL: {url}");
            Debug.Log($"[HttpClient][请求#{requestId}] 端点: {endpoint}");
            Debug.Log($"[HttpClient][请求#{requestId}] 请求体 (JSON):\n{jsonData}");
            Debug.Log($"[HttpClient][请求#{requestId}] 请求体大小: {Encoding.UTF8.GetByteCount(jsonData)} bytes");
            
            using (var request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                
                SetHeaders(request, requestId);
                var result = await SendRequest<T>(request, "POST", requestId);
                
                Debug.Log($"[HttpClient][请求#{requestId}] ================ POST请求完成 ================");
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
        private void SetHeaders(UnityWebRequest request, int requestId)
        {
            Debug.Log($"[HttpClient][请求#{requestId}] 设置请求头:");
            
            foreach (var header in _defaultHeaders)
            {
                request.SetRequestHeader(header.Key, header.Value);
                
                // 敏感信息脱敏显示
                var displayValue = header.Key == "Authorization" && header.Value.Length > 20 
                    ? header.Value.Substring(0, 20) + "..." 
                    : header.Value;
                    
                Debug.Log($"[HttpClient][请求#{requestId}]   {header.Key}: {displayValue}");
            }
        }

        /// <summary>
        /// 发送请求并处理响应
        /// </summary>
        private async Task<T> SendRequest<T>(UnityWebRequest request, string method, int requestId) where T : class
        {
            var startTime = DateTime.UtcNow;
            
            Debug.Log($"[HttpClient][请求#{requestId}] 开始发送 {method} 请求...");
            
            // 发送请求
            var operation = request.SendWebRequest();
            
            // 等待完成
            while (!operation.isDone)
            {
                await Task.Delay(50);
            }
            
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            
            Debug.Log($"[HttpClient][请求#{requestId}] ========== 响应信息 ==========");
            Debug.Log($"[HttpClient][请求#{requestId}] 请求耗时: {duration:F0}ms");
            Debug.Log($"[HttpClient][请求#{requestId}] 响应状态码: {request.responseCode}");
            Debug.Log($"[HttpClient][请求#{requestId}] 响应结果: {request.result}");
            
            // 打印响应头
            var responseHeaders = request.GetResponseHeaders();
            if (responseHeaders != null && responseHeaders.Count > 0)
            {
                Debug.Log($"[HttpClient][请求#{requestId}] 响应头:");
                foreach (var header in responseHeaders)
                {
                    Debug.Log($"[HttpClient][请求#{requestId}]   {header.Key}: {header.Value}");
                }
            }
            
            // 获取响应数据
            var responseText = request.downloadHandler?.text ?? "";
            var responseSize = Encoding.UTF8.GetByteCount(responseText);
            
            Debug.Log($"[HttpClient][请求#{requestId}] 响应数据大小: {responseSize} bytes");
            
            // 格式化显示响应数据
            if (!string.IsNullOrEmpty(responseText))
            {
                // 如果是JSON，尝试格式化显示
                if (IsJson(responseText))
                {
                    try
                    {
                        // 简单的JSON格式化（添加换行）
                        var formattedJson = FormatJson(responseText);
                        Debug.Log($"[HttpClient][请求#{requestId}] 响应数据 (JSON):\n{formattedJson}");
                    }
                    catch
                    {
                        Debug.Log($"[HttpClient][请求#{requestId}] 响应数据 (原始):\n{responseText}");
                    }
                }
                else
                {
                    Debug.Log($"[HttpClient][请求#{requestId}] 响应数据 (非JSON):\n{responseText}");
                }
            }
            else
            {
                Debug.Log($"[HttpClient][请求#{requestId}] 响应数据: 空");
            }
            
            // 检查错误
            if (request.result != UnityWebRequest.Result.Success)
            {
                var errorMsg = $"HTTP {method}请求失败: {request.error} (状态码: {request.responseCode})";
                Debug.LogError($"[HttpClient][请求#{requestId}] ❌ {errorMsg}");
                Debug.LogError($"[HttpClient][请求#{requestId}] 错误详情: {responseText}");
                throw new Exception(errorMsg);
            }
            
            // 解析响应
            return ParseResponse<T>(responseText, requestId);
        }

        /// <summary>
        /// 解析响应数据
        /// </summary>
        private T ParseResponse<T>(string responseText, int requestId) where T : class
        {
            if (string.IsNullOrEmpty(responseText))
            {
                Debug.LogWarning($"[HttpClient][请求#{requestId}] ⚠️ 响应数据为空");
                return null;
            }
            
            try
            {
                Debug.Log($"[HttpClient][请求#{requestId}] 开始解析为类型: {typeof(T).Name}");
                
                var result = JsonUtility.FromJson<T>(responseText);
                
                Debug.Log($"[HttpClient][请求#{requestId}] ✅ 数据解析成功，类型: {typeof(T).Name}");
                
                // 如果解析后的对象不为空，尝试显示部分信息
                if (result != null)
                {
                    try
                    {
                        var resultJson = JsonUtility.ToJson(result, true);
                        var preview = resultJson.Length > 500 ? resultJson.Substring(0, 500) + "..." : resultJson;
                        Debug.Log($"[HttpClient][请求#{requestId}] 解析后对象预览:\n{preview}");
                    }
                    catch
                    {
                        Debug.Log($"[HttpClient][请求#{requestId}] 解析后对象: {result.GetType().Name} (无法序列化预览)");
                    }
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HttpClient][请求#{requestId}] ❌ 解析响应数据失败: {ex.Message}");
                Debug.LogError($"[HttpClient][请求#{requestId}] 目标类型: {typeof(T).Name}");
                Debug.LogError($"[HttpClient][请求#{requestId}] 原始响应数据:\n{responseText}");
                throw new Exception($"解析响应数据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 判断字符串是否为JSON格式
        /// </summary>
        private bool IsJson(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            
            text = text.Trim();
            return (text.StartsWith("{") && text.EndsWith("}")) || 
                   (text.StartsWith("[") && text.EndsWith("]"));
        }

        /// <summary>
        /// 简单的JSON格式化
        /// </summary>
        private string FormatJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return json;
            
            // 简单的格式化，主要是添加换行
            var formatted = json.Replace(",", ",\n  ")
                              .Replace("{", "{\n  ")
                              .Replace("}", "\n}")
                              .Replace("[", "[\n  ")
                              .Replace("]", "\n]");
            
            return formatted;
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
            // 生成有意义的GameObject名称
            var urlHost = "";
            try
            {
                var uri = new Uri(baseUrl);
                urlHost = uri.Host;
            }
            catch
            {
                urlHost = "unknown";
            }
            
            var instanceCount = FindObjectsByType<HttpClient>(FindObjectsSortMode.None).Length + 1;
            var gameObjectName = $"HttpClient_{urlHost}_{instanceCount}";
            
            var go = new GameObject(gameObjectName);
            var client = go.AddComponent<HttpClient>();
            
            client.SetBaseUrl(baseUrl);
            if (!string.IsNullOrEmpty(token))
            {
                client.SetAuthToken(token);
            }
            
            Debug.Log($"[HttpClient] ✅ 已创建HttpClient实例: {gameObjectName} -> {baseUrl}");
            return client;
        }

        #endregion
    }
}
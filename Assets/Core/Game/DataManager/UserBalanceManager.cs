using System;
using UnityEngine;
using TMPro;
using BaccaratGame.Data;
using BaccaratGame.Core;

namespace BaccaratGame.Managers
{
    /// <summary>
    /// 用户余额管理器 - JSON解析版
    /// </summary>
    public class UserBalanceManager : MonoBehaviour
    {
        [Header("UI组件配置")]
        public TextMeshProUGUI balanceText;
        
        // JSON响应数据结构
        [System.Serializable]
        public class ApiResponse
        {
            public int code;
            public string message;
            public UserData data;
        }
        
        [System.Serializable]
        public class UserData
        {
            public int id;
            public string user_name;
            public string money_balance;  // 注意：这里是字符串
            public string money_freeze;
            public int status;
            public int state;
        }
        
        private void Start()
        {
            InvokeRepeating(nameof(RefreshUserBalance), 0f, 3f);
        }
        
        private async void RefreshUserBalance()
        {
            try
            {
                // 获取原始响应
                var response = await GameNetworkApi.Instance.GetUserInfo();
                
                if (response != null && balanceText != null)
                {
                    // 尝试获取原始JSON字符串
                    string jsonString = GetJsonString(response);
                    
                    if (!string.IsNullOrEmpty(jsonString))
                    {
                        Debug.Log($"[UserBalanceManager] JSON字符串: {jsonString.Substring(0, Math.Min(200, jsonString.Length))}...");
                        
                        // 使用Unity JsonUtility解析
                        ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(jsonString);
                        
                        if (apiResponse != null && apiResponse.data != null)
                        {
                            string balanceStr = apiResponse.data.money_balance;
                            
                            if (decimal.TryParse(balanceStr, out decimal balance))
                            {
                                balanceText.text = balance.ToString("F2");
                                Debug.Log($"[UserBalanceManager] 余额更新成功: {balance:F2}");
                            }
                            else
                            {
                                Debug.LogWarning($"[UserBalanceManager] 无法解析余额: {balanceStr}");
                                balanceText.text = "0.00";
                            }
                        }
                        else
                        {
                            Debug.LogWarning("[UserBalanceManager] JSON解析失败或data为空");
                            balanceText.text = "0.00";
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[UserBalanceManager] 无法获取JSON字符串");
                        balanceText.text = "0.00";
                    }
                }
                else
                {
                    Debug.LogWarning("[UserBalanceManager] response 或 balanceText 为空");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UserBalanceManager] 获取用户信息失败: {ex.Message}");
                balanceText.text = "0.00";
            }
        }
        
        /// <summary>
        /// 从响应对象中获取JSON字符串
        /// </summary>
        private string GetJsonString(object response)
        {
            try
            {
                // 方法1：检查是否有原始JSON属性
                var type = response.GetType();
                var jsonProperty = type.GetProperty("JsonString") ?? type.GetProperty("RawJson") ?? type.GetProperty("OriginalJson");
                if (jsonProperty != null)
                {
                    var jsonValue = jsonProperty.GetValue(response);
                    if (jsonValue != null)
                    {
                        return jsonValue.ToString();
                    }
                }
                
                // 方法2：检查是否有字段
                var jsonField = type.GetField("jsonString") ?? type.GetField("rawJson") ?? type.GetField("originalJson");
                if (jsonField != null)
                {
                    var jsonValue = jsonField.GetValue(response);
                    if (jsonValue != null)
                    {
                        return jsonValue.ToString();
                    }
                }
                
                // 方法3：如果以上都失败，返回null，需要修改API层
                Debug.LogWarning("[UserBalanceManager] 响应对象中没有找到原始JSON字符串");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UserBalanceManager] 获取JSON字符串失败: {ex.Message}");
                return null;
            }
        }
        
        private void OnDestroy()
        {
            CancelInvoke();
        }
    }
}
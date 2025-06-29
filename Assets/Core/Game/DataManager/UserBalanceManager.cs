using System;
using UnityEngine;
using TMPro;
using BaccaratGame.Core;

namespace BaccaratGame.Managers
{
    /// <summary>
    /// 用户余额管理器 - 配合字符串API版本
    /// </summary>
    public class UserBalanceManager : MonoBehaviour
    {
        [Header("UI组件配置")]
        public TextMeshProUGUI balanceText;
        
        // 明确的响应数据结构
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
            public string money_balance;  // 服务器返回字符串
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
                if (balanceText == null) return;

                // 直接获取JSON字符串
                string jsonResponse = await GameNetworkApi.Instance.GetUserInfo();
                
                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    // 直接解析JSON字符串
                    ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(jsonResponse);
                    
                    if (apiResponse != null && apiResponse.data != null)
                    {
                        string balanceStr = apiResponse.data.money_balance;
                        
                        if (decimal.TryParse(balanceStr, out decimal balance))
                        {
                            balanceText.text = balance.ToString("F2");
                            Debug.Log($"[UserBalanceManager] 余额更新: {balance:F2}");
                        }
                        else
                        {
                            balanceText.text = "0.00";
                            Debug.LogWarning($"[UserBalanceManager] 余额解析失败: {balanceStr}");
                        }
                    }
                    else
                    {
                        balanceText.text = "0.00";
                        Debug.LogWarning("[UserBalanceManager] 数据结构解析失败");
                    }
                }
                else
                {
                    balanceText.text = "0.00";
                    Debug.LogWarning("[UserBalanceManager] 响应为空");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UserBalanceManager] 获取用户信息失败: {ex.Message}");
                balanceText.text = "0.00";
            }
        }
        
        private void OnDestroy()
        {
            CancelInvoke();
        }
    }
}
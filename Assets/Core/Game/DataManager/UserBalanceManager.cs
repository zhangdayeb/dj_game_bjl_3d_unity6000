using System;
using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq;  // 使用 JSON 解析替代 dynamic
using BaccaratGame.Data;
using BaccaratGame.Core;

namespace BaccaratGame.Managers
{
    /// <summary>
    /// 用户余额管理器 - 简化版
    /// 每3秒自动刷新用户余额，直接更新UI显示
    /// 挂载节点：GameScene/DataManager
    /// </summary>
    public class UserBalanceManager : MonoBehaviour
    {
        [Header("UI组件配置")]
        public TextMeshProUGUI balanceText;  // TextMeshPro组件
        
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
                    // 将响应转换为JSON字符串，然后解析
                    string jsonString = response.ToString();
                    var jsonObj = JObject.Parse(jsonString);
                    
                    // 获取 data 字段
                    var dataToken = jsonObj["data"];
                    if (dataToken != null)
                    {
                        // 获取 money_balance
                        string balanceStr = dataToken["money_balance"]?.ToString();
                        Debug.Log($"[UserBalanceManager] 获取用户信息: {dataToken["user_name"]}, 原始余额: {balanceStr}");
                        
                        // money_balance 是字符串 "1000.00"
                        decimal balance = 0m;
                        if (decimal.TryParse(balanceStr, out balance))
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
                        Debug.LogWarning("[UserBalanceManager] response.data 为空");
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
            }
        }
        
        private void OnDestroy()
        {
            CancelInvoke();
        }
    }
}
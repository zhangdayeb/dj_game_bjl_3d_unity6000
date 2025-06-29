using System;
using UnityEngine;
using TMPro;  // TextMeshPro支持
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
                // GameNetworkApi 现在返回 object，需要转换
                var response = await GameNetworkApi.Instance.GetUserInfo();
                
                if (response != null && balanceText != null)
                {
                    // 将响应转为 dynamic 来访问 data 字段
                    dynamic responseData = response;
                    var userInfo = responseData.data;
                    
                    if (userInfo != null)
                    {
                        Debug.Log($"[UserBalanceManager] 获取用户信息: {userInfo.user_name}, 原始余额: {userInfo.money_balance}");
                        
                        // money_balance 是字符串 "1000.00"
                        decimal balance = 0m;
                        if (decimal.TryParse(userInfo.money_balance?.ToString(), out balance))
                        {
                            balanceText.text = balance.ToString("F2");
                            Debug.Log($"[UserBalanceManager] 余额更新成功: {balance:F2}");
                        }
                        else
                        {
                            Debug.LogWarning($"[UserBalanceManager] 无法解析余额: {userInfo.money_balance}");
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
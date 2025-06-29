using System;
using UnityEngine;
using TMPro;
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
                    Debug.Log($"[UserBalanceManager] 原始响应类型: {response.GetType()}");
                    
                    // 输出完整的响应内容用于调试
                    string responseStr = response.ToString();
                    Debug.Log($"[UserBalanceManager] 完整响应内容: {responseStr}");
                    
                    // 直接尝试转换为 UserInfo 类型
                    if (response is UserInfo userInfo)
                    {
                        // 如果直接是 UserInfo 类型，直接访问 money_balance 字段
                        decimal balance = userInfo.money_balance;
                        balanceText.text = balance.ToString("F2");
                        Debug.Log($"[UserBalanceManager] 余额更新成功(直接类型): {balance:F2}");
                    }
                    else
                    {
                        // 如果是包装响应，尝试使用字符串解析
                        string balanceStr = ExtractMoneyBalance(responseStr);
                        
                        if (decimal.TryParse(balanceStr, out decimal balance))
                        {
                            balanceText.text = balance.ToString("F2");
                            Debug.Log($"[UserBalanceManager] 余额更新成功(字符串解析): {balance:F2}");
                        }
                        else
                        {
                            Debug.LogWarning($"[UserBalanceManager] 无法解析余额: {balanceStr}");
                            balanceText.text = "0.00";
                        }
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
        
        /// <summary>
        /// 从响应字符串中提取余额
        /// </summary>
        private string ExtractMoneyBalance(string responseStr)
        {
            try
            {
                // 简单的字符串查找方式提取 money_balance
                string searchKey = "\"money_balance\":\"";
                int startIndex = responseStr.IndexOf(searchKey);
                
                if (startIndex >= 0)
                {
                    startIndex += searchKey.Length;
                    int endIndex = responseStr.IndexOf("\"", startIndex);
                    
                    if (endIndex > startIndex)
                    {
                        string result = responseStr.Substring(startIndex, endIndex - startIndex);
                        Debug.Log($"[UserBalanceManager] 提取余额成功: {result}");
                        return result;
                    }
                }
                
                // 如果上面的方法失败，尝试另一种格式（无引号的数字）
                searchKey = "\"money_balance\":";
                startIndex = responseStr.IndexOf(searchKey);
                if (startIndex >= 0)
                {
                    startIndex += searchKey.Length;
                    int endIndex = responseStr.IndexOfAny(new char[] { ',', '}', '\n', '\r' }, startIndex);
                    
                    if (endIndex > startIndex)
                    {
                        string result = responseStr.Substring(startIndex, endIndex - startIndex).Trim().Trim('"');
                        Debug.Log($"[UserBalanceManager] 提取余额成功(格式2): {result}");
                        return result;
                    }
                }
                
                Debug.LogWarning($"[UserBalanceManager] 无法从响应中提取余额");
                Debug.LogWarning($"[UserBalanceManager] 响应前200字符: {responseStr.Substring(0, Math.Min(200, responseStr.Length))}");
                return "0.00";
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UserBalanceManager] 提取余额时出错: {ex.Message}");
                return "0.00";
            }
        }
        
        private void OnDestroy()
        {
            CancelInvoke();
        }
    }
}
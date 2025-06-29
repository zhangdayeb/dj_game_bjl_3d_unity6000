using System;
using UnityEngine;
using UnityEngine.UI;
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
        public Text balanceText;
        
        private void Start()
        {
            InvokeRepeating(nameof(RefreshUserBalance), 0f, 3f);
        }
        
        private async void RefreshUserBalance()
        {
            try
            {
                var userInfo = await GameNetworkApi.Instance.GetUserInfo();
                
                if (balanceText != null && userInfo != null)
                {
                    balanceText.text = userInfo.money_balance.ToString("F2");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"获取用户信息失败: {ex.Message}");
            }
        }
        
        private void OnDestroy()
        {
            CancelInvoke();
        }
    }
}
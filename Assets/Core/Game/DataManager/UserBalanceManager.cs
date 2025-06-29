using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;  // 添加TextMeshPro支持
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
        [Tooltip("可以拖拽 Text 或 TextMeshPro 组件")]
        public Text balanceText;           // 传统Text组件
        public TextMeshProUGUI balanceTextTMP;  // TextMeshPro组件
        
        private void Start()
        {
            InvokeRepeating(nameof(RefreshUserBalance), 0f, 3f);
        }
        
        private async void RefreshUserBalance()
        {
            try
            {
                var userInfo = await GameNetworkApi.Instance.GetUserInfo();
                
                if (userInfo != null)
                {
                    string balanceStr = userInfo.money_balance.ToString("F2");
                    
                    // 更新传统Text组件
                    if (balanceText != null)
                    {
                        balanceText.text = balanceStr;
                    }
                    
                    // 更新TextMeshPro组件
                    if (balanceTextTMP != null)
                    {
                        balanceTextTMP.text = balanceStr;
                    }
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
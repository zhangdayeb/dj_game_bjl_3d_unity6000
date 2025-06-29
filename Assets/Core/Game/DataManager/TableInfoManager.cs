using System;
using UnityEngine;
using TMPro;
using BaccaratGame.Core;

namespace BaccaratGame.Managers
{
    /// <summary>
    /// 台桌信息管理器 - 参考UserBalanceManager写法
    /// </summary>
    public class TableInfoManager : MonoBehaviour
    {
        [Header("UI组件配置")]
        public TextMeshProUGUI gameNumberText;  // TextMeshPro组件
        
        // 明确的响应数据结构
        [System.Serializable]
        public class ApiResponse
        {
            public int code;
            public string message;
            public TableData data;
        }
        
        [System.Serializable]
        public class TableData
        {
            public string lu_zhu_name;
            public string bureau_number;  // 局号字段
            public string right_money_banker_player;
            public string right_money_banker_player_cny;
            public string right_money_tie;
            public string right_money_tie_cny;
            // 可以根据需要添加其他字段
        }
        
        private void Start()
        {
            InvokeRepeating(nameof(RefreshTableInfo), 0f, 3f);
        }
        
        private async void RefreshTableInfo()
        {
            try
            {
                if (gameNumberText == null) return;

                // 直接获取JSON字符串
                string jsonResponse = await GameNetworkApi.Instance.GetTableInfo();
                
                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    // 直接解析JSON字符串
                    ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(jsonResponse);
                    
                    if (apiResponse != null && apiResponse.data != null)
                    {
                        string bureauNumber = apiResponse.data.bureau_number;
                        
                        if (!string.IsNullOrEmpty(bureauNumber))
                        {
                            gameNumberText.text = bureauNumber;
                            Debug.Log($"[TableInfoManager] 局号更新: {bureauNumber}");
                        }
                        else
                        {
                            gameNumberText.text = "未知";
                            Debug.LogWarning($"[TableInfoManager] 局号为空: {bureauNumber}");
                        }
                    }
                    else
                    {
                        gameNumberText.text = "未知";
                        Debug.LogWarning("[TableInfoManager] 数据结构解析失败");
                    }
                }
                else
                {
                    gameNumberText.text = "未知";
                    Debug.LogWarning("[TableInfoManager] 响应为空");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TableInfoManager] 获取台桌信息失败: {ex.Message}");
                gameNumberText.text = "错误";
            }
        }
        
        private void OnDestroy()
        {
            CancelInvoke();
        }
    }
}
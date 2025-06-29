using System;
using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq;  // 使用 JSON 解析替代 dynamic
using BaccaratGame.Data;
using BaccaratGame.Core;

namespace BaccaratGame.Managers
{
    /// <summary>
    /// 台桌信息管理器 - 简化版
    /// 每3秒自动刷新台桌信息，直接更新UI显示
    /// 挂载节点：GameScene/DataManager
    /// </summary>
    public class TableInfoManager : MonoBehaviour
    {
        [Header("UI组件配置")]
        public TextMeshProUGUI gameNumberText;  // TextMeshPro组件
        
        private void Start()
        {
            InvokeRepeating(nameof(RefreshTableInfo), 0f, 3f);
        }
        
        private async void RefreshTableInfo()
        {
            try
            {
                // 获取原始响应
                var response = await GameNetworkApi.Instance.GetTableInfo();
                
                if (response != null && gameNumberText != null)
                {
                    // 将响应转换为JSON字符串，然后解析
                    string jsonString = response.ToString();
                    var jsonObj = JObject.Parse(jsonString);
                    
                    // 获取 data 字段
                    var dataToken = jsonObj["data"];
                    if (dataToken != null)
                    {
                        // 获取 bureau_number
                        string gameNumber = dataToken["bureau_number"]?.ToString() ?? "未知";
                        gameNumberText.text = gameNumber;
                        Debug.Log($"[TableInfoManager] 局号更新成功: {gameNumber}");
                    }
                    else
                    {
                        Debug.LogWarning("[TableInfoManager] response.data 为空");
                    }
                }
                else
                {
                    Debug.LogWarning("[TableInfoManager] response 或 gameNumberText 为空");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TableInfoManager] 获取台桌信息失败: {ex.Message}");
            }
        }
        
        private void OnDestroy()
        {
            CancelInvoke();
        }
    }
}
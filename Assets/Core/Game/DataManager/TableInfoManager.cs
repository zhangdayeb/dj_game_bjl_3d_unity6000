using System;
using UnityEngine;
using TMPro;
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
                    Debug.Log($"[TableInfoManager] 原始响应类型: {response.GetType()}");
                    
                    // 直接尝试转换为 TableInfo 类型
                    if (response is TableInfo tableInfo)
                    {
                        // 如果直接是 TableInfo 类型，检查所有可能的局号字段
                        Debug.Log($"[TableInfoManager] TableInfo对象 - bureau_number: {tableInfo.bureau_number}");
                        Debug.Log($"[TableInfoManager] TableInfo对象 - id: {tableInfo.id}");
                        Debug.Log($"[TableInfoManager] TableInfo对象 - table_title: {tableInfo.table_title}");
                        
                        string gameNumber = "未知";
                        
                        // 尝试不同的字段名
                        if (!string.IsNullOrEmpty(tableInfo.bureau_number?.ToString()))
                        {
                            gameNumber = tableInfo.bureau_number.ToString();
                        }
                        else if (tableInfo.id > 0)
                        {
                            gameNumber = tableInfo.id.ToString();
                        }
                        else
                        {
                            // 如果以上都没有，尝试从字符串中解析
                            string responseStr = response.ToString();
                            gameNumber = ExtractBureauNumber(responseStr);
                        }
                        
                        gameNumberText.text = gameNumber;
                        Debug.Log($"[TableInfoManager] 局号更新成功(直接类型): {gameNumber}");
                    }
                    else
                    {
                        // 如果不是 TableInfo 类型，输出完整内容
                        string responseStr = response.ToString();
                        Debug.Log($"[TableInfoManager] 非TableInfo类型，完整响应: {responseStr}");
                        
                        string gameNumber = ExtractBureauNumber(responseStr);
                        gameNumberText.text = gameNumber;
                        Debug.Log($"[TableInfoManager] 局号更新成功(字符串解析): {gameNumber}");
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
        
        /// <summary>
        /// 从响应字符串中提取局号
        /// </summary>
        private string ExtractBureauNumber(string responseStr)
        {
            try
            {
                // 简单的字符串查找方式提取 bureau_number
                string searchKey = "\"bureau_number\":\"";
                int startIndex = responseStr.IndexOf(searchKey);
                
                if (startIndex >= 0)
                {
                    startIndex += searchKey.Length;
                    int endIndex = responseStr.IndexOf("\"", startIndex);
                    
                    if (endIndex > startIndex)
                    {
                        string result = responseStr.Substring(startIndex, endIndex - startIndex);
                        Debug.Log($"[TableInfoManager] 提取局号成功: {result}");
                        return result;
                    }
                }
                
                // 如果上面的方法失败，尝试另一种格式（无引号的数字）
                searchKey = "\"bureau_number\":";
                startIndex = responseStr.IndexOf(searchKey);
                if (startIndex >= 0)
                {
                    startIndex += searchKey.Length;
                    int endIndex = responseStr.IndexOfAny(new char[] { ',', '}', '\n', '\r' }, startIndex);
                    
                    if (endIndex > startIndex)
                    {
                        string result = responseStr.Substring(startIndex, endIndex - startIndex).Trim().Trim('"');
                        Debug.Log($"[TableInfoManager] 提取局号成功(格式2): {result}");
                        return result;
                    }
                }
                
                Debug.LogWarning($"[TableInfoManager] 无法从响应中提取局号");
                Debug.LogWarning($"[TableInfoManager] 响应前200字符: {responseStr.Substring(0, Math.Min(200, responseStr.Length))}");
                return "未知";
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TableInfoManager] 提取局号时出错: {ex.Message}");
                return "错误";
            }
        }
        
        private void OnDestroy()
        {
            CancelInvoke();
        }
    }
}
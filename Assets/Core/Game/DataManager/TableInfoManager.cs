using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;  // 添加TextMeshPro支持
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
        [Tooltip("可以拖拽 Text 或 TextMeshPro 组件")]
        public Text gameNumberText;           // 传统Text组件
        public TextMeshProUGUI gameNumberTextTMP;  // TextMeshPro组件
        
        private void Start()
        {
            InvokeRepeating(nameof(RefreshTableInfo), 0f, 3f);
        }
        
        private async void RefreshTableInfo()
        {
            try
            {
                var tableInfo = await GameNetworkApi.Instance.GetTableInfo();
                
                if (tableInfo != null)
                {
                    string gameNumber = tableInfo.bureau_number.ToString();
                    
                    // 更新传统Text组件
                    if (gameNumberText != null)
                    {
                        gameNumberText.text = gameNumber;
                    }
                    
                    // 更新TextMeshPro组件
                    if (gameNumberTextTMP != null)
                    {
                        gameNumberTextTMP.text = gameNumber;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"获取台桌信息失败: {ex.Message}");
            }
        }
        
        private void OnDestroy()
        {
            CancelInvoke();
        }
    }
}
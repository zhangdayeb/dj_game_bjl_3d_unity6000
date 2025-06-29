using System;
using UnityEngine;
using UnityEngine.UI;
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
        public Text gameNumberText;
        
        private void Start()
        {
            InvokeRepeating(nameof(RefreshTableInfo), 0f, 3f);
        }
        
        private async void RefreshTableInfo()
        {
            try
            {
                var tableInfo = await GameNetworkApi.Instance.GetTableInfo();
                
                if (gameNumberText != null && tableInfo != null)
                {
                    gameNumberText.text = tableInfo.bureau_number;
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
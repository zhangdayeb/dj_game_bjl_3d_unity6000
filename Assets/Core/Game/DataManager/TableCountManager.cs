using System;
using UnityEngine;
using TMPro;
using BaccaratGame.Core;

namespace BaccaratGame.Managers
{
    /// <summary>
    /// 台桌统计数据管理器
    /// 每3秒请求一次统计数据并更新UI显示
    /// </summary>
    public class TableCountManager : MonoBehaviour
    {
        [Header("UI组件配置")]
        [Tooltip("庄家次数显示")]
        public TextMeshProUGUI zhuangText;
        
        [Tooltip("闲家次数显示")]
        public TextMeshProUGUI xianText;
        
        [Tooltip("和局次数显示")]
        public TextMeshProUGUI heText;
        
        [Tooltip("庄对次数显示")]
        public TextMeshProUGUI zhuangDuiText;
        
        [Tooltip("闲对次数显示")]
        public TextMeshProUGUI xianDuiText;
        
        [Tooltip("总局数显示")]
        public TextMeshProUGUI countText;

        [Header("调试选项")]
        [Tooltip("是否启用详细日志")]
        public bool enableDetailLog = true;

        // API响应数据结构
        [System.Serializable]
        public class ApiResponse
        {
            public int code;
            public string message;
            public TableStatsData data;
        }
        
        [System.Serializable]
        public class TableStatsData
        {
            public int zhuang;          // 庄家次数
            public int xian;            // 闲家次数
            public int he;              // 和局次数
            public int zhuang_dui;      // 庄对次数
            public int xian_dui;        // 闲对次数
            public int xue_num;         // 靴号
            public int count;           // 总局数
            public int count_money;     // 统计金额
        }
        
        private void Start()
        {
            // 立即执行一次，然后每3秒重复执行
            InvokeRepeating(nameof(RefreshTableStats), 0f, 3f);
            
            if (enableDetailLog)
            {
                Debug.Log("[TableCountManager] 开始执行台桌统计数据更新，间隔3秒");
            }
        }
        
        /// <summary>
        /// 刷新台桌统计数据
        /// </summary>
        private async void RefreshTableStats()
        {
            try
            {
                if (enableDetailLog)
                {
                    Debug.Log("[TableCountManager] 开始请求台桌统计数据...");
                }

                // 使用GameNetworkApi获取数据
                string jsonResponse = await GameNetworkApi.Instance.GetTableBetStats();
                
                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    // 解析JSON响应
                    ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(jsonResponse);
                    
                    if (apiResponse != null && apiResponse.data != null)
                    {
                        UpdateUI(apiResponse.data);
                        
                        if (enableDetailLog)
                        {
                            Debug.Log($"[TableCountManager] 统计数据更新成功: " +
                                $"庄{apiResponse.data.zhuang} 闲{apiResponse.data.xian} " +
                                $"和{apiResponse.data.he} 总局{apiResponse.data.count}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[TableCountManager] API响应数据结构解析失败");
                        UpdateUIWithDefault();
                    }
                }
                else
                {
                    Debug.LogWarning("[TableCountManager] API响应为空");
                    UpdateUIWithDefault();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TableCountManager] 获取台桌统计数据失败: {ex.Message}");
                UpdateUIWithDefault();
            }
        }
        
        /// <summary>
        /// 更新UI显示
        /// </summary>
        /// <param name="data">统计数据</param>
        private void UpdateUI(TableStatsData data)
        {
            // 更新各个文本组件
            if (zhuangText != null)
                zhuangText.text = data.zhuang.ToString();
                
            if (xianText != null)
                xianText.text = data.xian.ToString();
                
            if (heText != null)
                heText.text = data.he.ToString();
                
            if (zhuangDuiText != null)
                zhuangDuiText.text = data.zhuang_dui.ToString();
                
            if (xianDuiText != null)
                xianDuiText.text = data.xian_dui.ToString();
                
            if (countText != null)
                countText.text = data.count.ToString();
        }
        
        /// <summary>
        /// 使用默认值更新UI（当获取数据失败时）
        /// </summary>
        private void UpdateUIWithDefault()
        {
            if (zhuangText != null) zhuangText.text = "0";
            if (xianText != null) xianText.text = "0";
            if (heText != null) heText.text = "0";
            if (zhuangDuiText != null) zhuangDuiText.text = "0";
            if (xianDuiText != null) xianDuiText.text = "0";
            if (countText != null) countText.text = "0";
        }
        
        /// <summary>
        /// 手动刷新数据（提供给外部调用）
        /// </summary>
        public void ManualRefresh()
        {
            RefreshTableStats();
        }
        
        /// <summary>
        /// 停止自动刷新
        /// </summary>
        public void StopAutoRefresh()
        {
            CancelInvoke(nameof(RefreshTableStats));
            Debug.Log("[TableCountManager] 已停止自动刷新");
        }
        
        /// <summary>
        /// 重新开始自动刷新
        /// </summary>
        public void StartAutoRefresh()
        {
            StopAutoRefresh();
            InvokeRepeating(nameof(RefreshTableStats), 0f, 3f);
            Debug.Log("[TableCountManager] 已重新开始自动刷新");
        }
        
        private void OnDestroy()
        {
            // 清理定时器
            CancelInvoke();
            
            if (enableDetailLog)
            {
                Debug.Log("[TableCountManager] 组件已销毁，定时器已清理");
            }
        }
        
        #region 调试方法
        
        /// <summary>
        /// 验证UI组件配置（编辑器调试用）
        /// </summary>
        [ContextMenu("验证UI组件配置")]
        private void ValidateUIComponents()
        {
            Debug.Log("=== TableCountManager UI组件验证 ===");
            Debug.Log($"庄家次数文本: {(zhuangText != null ? "✓" : "✗")}");
            Debug.Log($"闲家次数文本: {(xianText != null ? "✓" : "✗")}");
            Debug.Log($"和局次数文本: {(heText != null ? "✓" : "✗")}");
            Debug.Log($"庄对次数文本: {(zhuangDuiText != null ? "✓" : "✗")}");
            Debug.Log($"闲对次数文本: {(xianDuiText != null ? "✓" : "✗")}");
            Debug.Log($"总局数文本: {(countText != null ? "✓" : "✗")}");
        }
        
        #endregion
    }
}
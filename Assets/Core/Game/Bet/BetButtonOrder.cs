// Assets/Scripts/Core/BetButtonOrder.cs
// 投注按钮点击处理器 - 处理8个投注区域的点击事件
// 数据管理完全依赖BetDataManager，只负责点击事件处理
// 创建时间: 2025/6/29

using UnityEngine;
using UnityEngine.UI;
using BaccaratGame.Managers;

namespace BaccaratGame.Core
{
    /// <summary>
    /// 投注按钮点击处理器
    /// 负责处理8个投注区域按钮的点击事件，实现数据累加功能
    /// </summary>
    public class BetButtonOrder : MonoBehaviour
    {
        #region Inspector 配置

        [Header("投注区域按钮引用")]
        [SerializeField] private Button bankerButton;       // 庄按钮 (ButtonZhuang)
        [SerializeField] private Button playerButton;       // 闲按钮 (ButtonXian)
        [SerializeField] private Button tieButton;          // 和按钮 (ButtonHe)
        [SerializeField] private Button bankerPairButton;   // 庄对按钮 (ButtonZhuangdui)
        [SerializeField] private Button playerPairButton;   // 闲对按钮 (ButtonXiandui)
        [SerializeField] private Button long7Button;        // Long7按钮 (ButtonLong7)
        [SerializeField] private Button xiong8Button;       // Xiong8按钮 (ButtonXiong8)
        [SerializeField] private Button superSixButton;     // 幸运6按钮 (ButtonLucky6)

        #endregion

        #region 生命周期

        private void Start()
        {
            SetupButtonEvents();
        }

        private void OnDestroy()
        {
            ClearButtonEvents();
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 设置所有按钮的点击事件
        /// </summary>
        private void SetupButtonEvents()
        {
            // 庄按钮
            if (bankerButton != null)
            {
                bankerButton.onClick.AddListener(OnBankerClick);
                Debug.Log("[BetButtonOrder] 庄按钮事件已绑定");
            }
            else
            {
                Debug.LogWarning("[BetButtonOrder] 庄按钮未配置");
            }

            // 闲按钮
            if (playerButton != null)
            {
                playerButton.onClick.AddListener(OnPlayerClick);
                Debug.Log("[BetButtonOrder] 闲按钮事件已绑定");
            }
            else
            {
                Debug.LogWarning("[BetButtonOrder] 闲按钮未配置");
            }

            // 和按钮
            if (tieButton != null)
            {
                tieButton.onClick.AddListener(OnTieClick);
                Debug.Log("[BetButtonOrder] 和按钮事件已绑定");
            }
            else
            {
                Debug.LogWarning("[BetButtonOrder] 和按钮未配置");
            }

            // 庄对按钮
            if (bankerPairButton != null)
            {
                bankerPairButton.onClick.AddListener(OnBankerPairClick);
                Debug.Log("[BetButtonOrder] 庄对按钮事件已绑定");
            }
            else
            {
                Debug.LogWarning("[BetButtonOrder] 庄对按钮未配置");
            }

            // 闲对按钮
            if (playerPairButton != null)
            {
                playerPairButton.onClick.AddListener(OnPlayerPairClick);
                Debug.Log("[BetButtonOrder] 闲对按钮事件已绑定");
            }
            else
            {
                Debug.LogWarning("[BetButtonOrder] 闲对按钮未配置");
            }

            // Long7按钮
            if (long7Button != null)
            {
                long7Button.onClick.AddListener(OnLong7Click);
                Debug.Log("[BetButtonOrder] Long7按钮事件已绑定");
            }
            else
            {
                Debug.LogWarning("[BetButtonOrder] Long7按钮未配置");
            }

            // Xiong8按钮
            if (xiong8Button != null)
            {
                xiong8Button.onClick.AddListener(OnXiong8Click);
                Debug.Log("[BetButtonOrder] Xiong8按钮事件已绑定");
            }
            else
            {
                Debug.LogWarning("[BetButtonOrder] Xiong8按钮未配置");
            }

            // 幸运6按钮
            if (superSixButton != null)
            {
                superSixButton.onClick.AddListener(OnSuperSixClick);
                Debug.Log("[BetButtonOrder] 幸运6按钮事件已绑定");
            }
            else
            {
                Debug.LogWarning("[BetButtonOrder] 幸运6按钮未配置");
            }

            Debug.Log("[BetButtonOrder] 所有按钮事件设置完成");
        }

        /// <summary>
        /// 清除所有按钮的点击事件
        /// </summary>
        private void ClearButtonEvents()
        {
            if (bankerButton != null) bankerButton.onClick.RemoveListener(OnBankerClick);
            if (playerButton != null) playerButton.onClick.RemoveListener(OnPlayerClick);
            if (tieButton != null) tieButton.onClick.RemoveListener(OnTieClick);
            if (bankerPairButton != null) bankerPairButton.onClick.RemoveListener(OnBankerPairClick);
            if (playerPairButton != null) playerPairButton.onClick.RemoveListener(OnPlayerPairClick);
            if (long7Button != null) long7Button.onClick.RemoveListener(OnLong7Click);
            if (xiong8Button != null) xiong8Button.onClick.RemoveListener(OnXiong8Click);
            if (superSixButton != null) superSixButton.onClick.RemoveListener(OnSuperSixClick);

            Debug.Log("[BetButtonOrder] 所有按钮事件已清除");
        }

        #endregion

        #region 投注区域点击事件

        /// <summary>
        /// 庄按钮点击事件
        /// </summary>
        private void OnBankerClick()
        {
            int currentChip = BetDataManager.Instance.CurrentSelectedChip;
            BetDataManager.Instance.AddBankerAmount(currentChip);
            
            Debug.Log($"[BetButtonOrder] 投注到庄，筹码值：{currentChip}");
            LogAllBetAmounts();
        }

        /// <summary>
        /// 闲按钮点击事件
        /// </summary>
        private void OnPlayerClick()
        {
            int currentChip = BetDataManager.Instance.CurrentSelectedChip;
            BetDataManager.Instance.AddPlayerAmount(currentChip);
            
            Debug.Log($"[BetButtonOrder] 投注到闲，筹码值：{currentChip}");
            LogAllBetAmounts();
        }

        /// <summary>
        /// 和按钮点击事件
        /// </summary>
        private void OnTieClick()
        {
            int currentChip = BetDataManager.Instance.CurrentSelectedChip;
            BetDataManager.Instance.AddTieAmount(currentChip);
            
            Debug.Log($"[BetButtonOrder] 投注到和，筹码值：{currentChip}");
            LogAllBetAmounts();
        }

        /// <summary>
        /// 庄对按钮点击事件
        /// </summary>
        private void OnBankerPairClick()
        {
            int currentChip = BetDataManager.Instance.CurrentSelectedChip;
            BetDataManager.Instance.AddBankerPairAmount(currentChip);
            
            Debug.Log($"[BetButtonOrder] 投注到庄对，筹码值：{currentChip}");
            LogAllBetAmounts();
        }

        /// <summary>
        /// 闲对按钮点击事件
        /// </summary>
        private void OnPlayerPairClick()
        {
            int currentChip = BetDataManager.Instance.CurrentSelectedChip;
            BetDataManager.Instance.AddPlayerPairAmount(currentChip);
            
            Debug.Log($"[BetButtonOrder] 投注到闲对，筹码值：{currentChip}");
            LogAllBetAmounts();
        }

        /// <summary>
        /// Long7按钮点击事件
        /// </summary>
        private void OnLong7Click()
        {
            int currentChip = BetDataManager.Instance.CurrentSelectedChip;
            BetDataManager.Instance.AddLong7Amount(currentChip);
            
            Debug.Log($"[BetButtonOrder] 投注到龙7，筹码值：{currentChip}");
            LogAllBetAmounts();
        }

        /// <summary>
        /// Xiong8按钮点击事件
        /// </summary>
        private void OnXiong8Click()
        {
            int currentChip = BetDataManager.Instance.CurrentSelectedChip;
            BetDataManager.Instance.AddXiong8Amount(currentChip);
            
            Debug.Log($"[BetButtonOrder] 投注到熊8，筹码值：{currentChip}");
            LogAllBetAmounts();
        }

        /// <summary>
        /// 幸运6按钮点击事件
        /// </summary>
        private void OnSuperSixClick()
        {
            int currentChip = BetDataManager.Instance.CurrentSelectedChip;
            BetDataManager.Instance.AddLucky6Amount(currentChip);
            
            Debug.Log($"[BetButtonOrder] 投注到幸运6，筹码值：{currentChip}");
            LogAllBetAmounts();
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 输出所有投注区域的当前金额
        /// </summary>
        private void LogAllBetAmounts()
        {
            var betManager = BetDataManager.Instance;
            
            Debug.Log("=== 当前所有投注区域金额 ===");
            Debug.Log($"庄: {betManager.BankerAmount}");
            Debug.Log($"闲: {betManager.PlayerAmount}");
            Debug.Log($"和: {betManager.TieAmount}");
            Debug.Log($"庄对: {betManager.BankerPairAmount}");
            Debug.Log($"闲对: {betManager.PlayerPairAmount}");
            Debug.Log($"幸运6: {betManager.Lucky6Amount}");
            Debug.Log($"龙7: {betManager.Long7Amount}");
            Debug.Log($"熊8: {betManager.Xiong8Amount}");
            Debug.Log($"总投注金额: {betManager.GetTotalBetAmount()}");
            Debug.Log("==============================");
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 手动清空所有投注（用于测试或重置）
        /// </summary>
        public void ClearAllBets()
        {
            BetDataManager.Instance.ClearAllBets();
            Debug.Log("[BetButtonOrder] 已清空所有投注");
            LogAllBetAmounts();
        }

        /// <summary>
        /// 获取当前选中的筹码值（用于调试）
        /// </summary>
        /// <returns>当前选中筹码值</returns>
        public int GetCurrentChipValue()
        {
            return BetDataManager.Instance.CurrentSelectedChip;
        }

        #endregion

        #region 编辑器辅助

        /// <summary>
        /// 验证配置（编辑器模式下使用）
        /// </summary>
        [ContextMenu("验证按钮配置")]
        public void ValidateButtonConfiguration()
        {
            Debug.Log("=== BetButtonOrder 按钮配置验证 ===");
            
            Debug.Log($"庄按钮: {(bankerButton != null ? "✓" : "✗")}");
            Debug.Log($"闲按钮: {(playerButton != null ? "✓" : "✗")}");
            Debug.Log($"和按钮: {(tieButton != null ? "✓" : "✗")}");
            Debug.Log($"庄对按钮: {(bankerPairButton != null ? "✓" : "✗")}");
            Debug.Log($"闲对按钮: {(playerPairButton != null ? "✓" : "✗")}");
            Debug.Log($"Long7按钮: {(long7Button != null ? "✓" : "✗")}");
            Debug.Log($"Xiong8按钮: {(xiong8Button != null ? "✓" : "✗")}");
            Debug.Log($"幸运6按钮: {(superSixButton != null ? "✓" : "✗")}");
            
            int configuredCount = 0;
            if (bankerButton != null) configuredCount++;
            if (playerButton != null) configuredCount++;
            if (tieButton != null) configuredCount++;
            if (bankerPairButton != null) configuredCount++;
            if (playerPairButton != null) configuredCount++;
            if (long7Button != null) configuredCount++;
            if (xiong8Button != null) configuredCount++;
            if (superSixButton != null) configuredCount++;
            
            Debug.Log($"已配置按钮数量: {configuredCount}/8");
        }

        /// <summary>
        /// 测试所有投注按钮（编辑器模式下使用）
        /// </summary>
        [ContextMenu("测试所有投注按钮")]
        public void TestAllBetButtons()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("请在运行时测试投注按钮功能");
                return;
            }

            Debug.Log("=== 测试所有投注按钮 ===");
            
            // 先清空所有投注
            ClearAllBets();
            
            // 测试每个按钮
            if (bankerButton != null) OnBankerClick();
            if (playerButton != null) OnPlayerClick();
            if (tieButton != null) OnTieClick();
            if (bankerPairButton != null) OnBankerPairClick();
            if (playerPairButton != null) OnPlayerPairClick();
            if (long7Button != null) OnLong7Click();
            if (xiong8Button != null) OnXiong8Click();
            if (superSixButton != null) OnSuperSixClick();
            
            Debug.Log("=== 测试完成 ===");
        }

        #endregion
    }
}
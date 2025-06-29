// Assets/Scripts/Managers/BetDataManager.cs
// 投注数据管理器 - 极简版数据容器
// 单例模式管理筹码选择和投注区域数据
// 创建时间: 2025/6/29

using UnityEngine;

namespace BaccaratGame.Managers
{
    /// <summary>
    /// 投注数据管理器 - 极简版
    /// 纯数据容器，管理筹码选择和8个投注区域的金额
    /// </summary>
    public class BetDataManager
    {
        #region 单例模式

        private static BetDataManager _instance;

        /// <summary>
        /// 获取BetDataManager单例实例
        /// </summary>
        public static BetDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BetDataManager();
                    Debug.Log("[BetDataManager] 单例实例已创建");
                }
                return _instance;
            }
        }

        /// <summary>
        /// 私有构造函数，确保单例模式
        /// </summary>
        private BetDataManager()
        {
            InitializeData();
        }

        #endregion

        #region 筹码数据

        /// <summary>
        /// 可用筹码数组 - 5个筹码
        /// </summary>
        public int[] AvailableChips { get; private set; } = { 1, 5, 10, 20, 50 };

        /// <summary>
        /// 当前选中的筹码值
        /// </summary>
        public int CurrentSelectedChip { get; private set; } = 1;

        #endregion

        #region 投注区域数据 - 8个投注区域

        /// <summary>
        /// 庄投注金额
        /// </summary>
        public decimal BankerAmount { get; private set; } = 0;

        /// <summary>
        /// 闲投注金额
        /// </summary>
        public decimal PlayerAmount { get; private set; } = 0;

        /// <summary>
        /// 和投注金额
        /// </summary>
        public decimal TieAmount { get; private set; } = 0;

        /// <summary>
        /// 庄对投注金额
        /// </summary>
        public decimal BankerPairAmount { get; private set; } = 0;

        /// <summary>
        /// 闲对投注金额
        /// </summary>
        public decimal PlayerPairAmount { get; private set; } = 0;

        /// <summary>
        /// 大投注金额
        /// </summary>
        public decimal BigAmount { get; private set; } = 0;

        /// <summary>
        /// 小投注金额
        /// </summary>
        public decimal SmallAmount { get; private set; } = 0;

        /// <summary>
        /// 超级六投注金额
        /// </summary>
        public decimal SuperSixAmount { get; private set; } = 0;

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData()
        {
            // 默认选中第一个筹码
            CurrentSelectedChip = AvailableChips[0];
            
            // 清空所有投注金额
            ClearAllBets();
            
            Debug.Log($"[BetDataManager] 数据初始化完成，默认筹码: {CurrentSelectedChip}");
        }

        #endregion

        #region 筹码管理方法

        /// <summary>
        /// 设置当前选中的筹码（按索引）
        /// </summary>
        /// <param name="chipIndex">筹码索引 (0-4)</param>
        /// <returns>设置是否成功</returns>
        public bool SetSelectedChipByIndex(int chipIndex)
        {
            if (chipIndex < 0 || chipIndex >= AvailableChips.Length)
            {
                Debug.LogWarning($"[BetDataManager] 无效的筹码索引: {chipIndex}");
                return false;
            }

            CurrentSelectedChip = AvailableChips[chipIndex];
            Debug.Log($"[BetDataManager] 选中筹码: {CurrentSelectedChip} (索引: {chipIndex})");
            return true;
        }

        /// <summary>
        /// 设置当前选中的筹码（按值）
        /// </summary>
        /// <param name="chipValue">筹码值</param>
        /// <returns>设置是否成功</returns>
        public bool SetSelectedChipByValue(int chipValue)
        {
            for (int i = 0; i < AvailableChips.Length; i++)
            {
                if (AvailableChips[i] == chipValue)
                {
                    CurrentSelectedChip = chipValue;
                    Debug.Log($"[BetDataManager] 选中筹码: {CurrentSelectedChip}");
                    return true;
                }
            }

            Debug.LogWarning($"[BetDataManager] 未找到筹码值: {chipValue}");
            return false;
        }

        /// <summary>
        /// 获取当前选中筹码的索引
        /// </summary>
        /// <returns>筹码索引，未找到返回-1</returns>
        public int GetSelectedChipIndex()
        {
            for (int i = 0; i < AvailableChips.Length; i++)
            {
                if (AvailableChips[i] == CurrentSelectedChip)
                {
                    return i;
                }
            }
            return -1;
        }

        #endregion

        #region 投注区域管理方法

        /// <summary>
        /// 设置庄投注金额
        /// </summary>
        /// <param name="amount">投注金额</param>
        public void SetBankerAmount(decimal amount)
        {
            BankerAmount = amount;
            Debug.Log($"[BetDataManager] 庄投注金额: {amount}");
        }

        /// <summary>
        /// 设置闲投注金额
        /// </summary>
        /// <param name="amount">投注金额</param>
        public void SetPlayerAmount(decimal amount)
        {
            PlayerAmount = amount;
            Debug.Log($"[BetDataManager] 闲投注金额: {amount}");
        }

        /// <summary>
        /// 设置和投注金额
        /// </summary>
        /// <param name="amount">投注金额</param>
        public void SetTieAmount(decimal amount)
        {
            TieAmount = amount;
            Debug.Log($"[BetDataManager] 和投注金额: {amount}");
        }

        /// <summary>
        /// 设置庄对投注金额
        /// </summary>
        /// <param name="amount">投注金额</param>
        public void SetBankerPairAmount(decimal amount)
        {
            BankerPairAmount = amount;
            Debug.Log($"[BetDataManager] 庄对投注金额: {amount}");
        }

        /// <summary>
        /// 设置闲对投注金额
        /// </summary>
        /// <param name="amount">投注金额</param>
        public void SetPlayerPairAmount(decimal amount)
        {
            PlayerPairAmount = amount;
            Debug.Log($"[BetDataManager] 闲对投注金额: {amount}");
        }

        /// <summary>
        /// 设置大投注金额
        /// </summary>
        /// <param name="amount">投注金额</param>
        public void SetBigAmount(decimal amount)
        {
            BigAmount = amount;
            Debug.Log($"[BetDataManager] 大投注金额: {amount}");
        }

        /// <summary>
        /// 设置小投注金额
        /// </summary>
        /// <param name="amount">投注金额</param>
        public void SetSmallAmount(decimal amount)
        {
            SmallAmount = amount;
            Debug.Log($"[BetDataManager] 小投注金额: {amount}");
        }

        /// <summary>
        /// 设置超级六投注金额
        /// </summary>
        /// <param name="amount">投注金额</param>
        public void SetSuperSixAmount(decimal amount)
        {
            SuperSixAmount = amount;
            Debug.Log($"[BetDataManager] 超级六投注金额: {amount}");
        }

        /// <summary>
        /// 增加庄投注金额
        /// </summary>
        /// <param name="amount">增加的金额</param>
        public void AddBankerAmount(decimal amount)
        {
            BankerAmount += amount;
            Debug.Log($"[BetDataManager] 庄投注金额增加: +{amount}, 总计: {BankerAmount}");
        }

        /// <summary>
        /// 增加闲投注金额
        /// </summary>
        /// <param name="amount">增加的金额</param>
        public void AddPlayerAmount(decimal amount)
        {
            PlayerAmount += amount;
            Debug.Log($"[BetDataManager] 闲投注金额增加: +{amount}, 总计: {PlayerAmount}");
        }

        /// <summary>
        /// 增加和投注金额
        /// </summary>
        /// <param name="amount">增加的金额</param>
        public void AddTieAmount(decimal amount)
        {
            TieAmount += amount;
            Debug.Log($"[BetDataManager] 和投注金额增加: +{amount}, 总计: {TieAmount}");
        }

        /// <summary>
        /// 增加庄对投注金额
        /// </summary>
        /// <param name="amount">增加的金额</param>
        public void AddBankerPairAmount(decimal amount)
        {
            BankerPairAmount += amount;
            Debug.Log($"[BetDataManager] 庄对投注金额增加: +{amount}, 总计: {BankerPairAmount}");
        }

        /// <summary>
        /// 增加闲对投注金额
        /// </summary>
        /// <param name="amount">增加的金额</param>
        public void AddPlayerPairAmount(decimal amount)
        {
            PlayerPairAmount += amount;
            Debug.Log($"[BetDataManager] 闲对投注金额增加: +{amount}, 总计: {PlayerPairAmount}");
        }

        /// <summary>
        /// 增加大投注金额
        /// </summary>
        /// <param name="amount">增加的金额</param>
        public void AddBigAmount(decimal amount)
        {
            BigAmount += amount;
            Debug.Log($"[BetDataManager] 大投注金额增加: +{amount}, 总计: {BigAmount}");
        }

        /// <summary>
        /// 增加小投注金额
        /// </summary>
        /// <param name="amount">增加的金额</param>
        public void AddSmallAmount(decimal amount)
        {
            SmallAmount += amount;
            Debug.Log($"[BetDataManager] 小投注金额增加: +{amount}, 总计: {SmallAmount}");
        }

        /// <summary>
        /// 增加超级六投注金额
        /// </summary>
        /// <param name="amount">增加的金额</param>
        public void AddSuperSixAmount(decimal amount)
        {
            SuperSixAmount += amount;
            Debug.Log($"[BetDataManager] 超级六投注金额增加: +{amount}, 总计: {SuperSixAmount}");
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 清空所有投注金额
        /// </summary>
        public void ClearAllBets()
        {
            BankerAmount = 0;
            PlayerAmount = 0;
            TieAmount = 0;
            BankerPairAmount = 0;
            PlayerPairAmount = 0;
            BigAmount = 0;
            SmallAmount = 0;
            SuperSixAmount = 0;
            
            Debug.Log("[BetDataManager] 所有投注金额已清空");
        }

        /// <summary>
        /// 获取总投注金额
        /// </summary>
        /// <returns>总投注金额</returns>
        public decimal GetTotalBetAmount()
        {
            decimal total = BankerAmount + PlayerAmount + TieAmount + BankerPairAmount + 
                           PlayerPairAmount + BigAmount + SmallAmount + SuperSixAmount;
            return total;
        }

        /// <summary>
        /// 重置为默认状态
        /// </summary>
        public void ResetToDefault()
        {
            CurrentSelectedChip = AvailableChips[0];
            ClearAllBets();
            Debug.Log("[BetDataManager] 已重置为默认状态");
        }

        /// <summary>
        /// 获取所有投注数据的调试信息
        /// </summary>
        /// <returns>调试信息字符串</returns>
        public string GetDebugInfo()
        {
            return $"[BetDataManager] 当前状态:\n" +
                   $"选中筹码: {CurrentSelectedChip}\n" +
                   $"庄: {BankerAmount}, 闲: {PlayerAmount}, 和: {TieAmount}\n" +
                   $"庄对: {BankerPairAmount}, 闲对: {PlayerPairAmount}\n" +
                   $"大: {BigAmount}, 小: {SmallAmount}, 超级六: {SuperSixAmount}\n" +
                   $"总投注: {GetTotalBetAmount()}";
        }

        #endregion
    }
}
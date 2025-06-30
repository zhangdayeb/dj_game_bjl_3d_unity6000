// Assets/Scripts/Managers/BetDataManager.cs
// 投注数据管理器 - 扩展版数据容器
// 单例模式管理筹码选择和投注区域数据，支持历史记录、撤销、重复投注等功能
// 创建时间: 2025/6/29
// 更新时间: 2025/6/30 - 添加事件系统、投注历史、撤销功能、重复投注功能

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaccaratGame.Managers
{
    /// <summary>
    /// 投注区域类型枚举
    /// </summary>
    public enum BetAreaType
    {
        Banker = 1,      // 庄
        Player = 2,      // 闲
        Tie = 3,         // 和
        BankerPair = 4,  // 庄对
        PlayerPair = 5,  // 闲对
        Lucky6 = 6,      // 幸运6
        Long7 = 7,       // 龙7
        Xiong8 = 8       // 熊8
    }

    /// <summary>
    /// 投注步骤记录
    /// </summary>
    [System.Serializable]
    public class BetStep
    {
        public int stepId;              // 步骤ID
        public BetAreaType areaType;    // 投注区域类型
        public int chipValue;           // 筹码值
        public decimal amount;          // 投注金额
        public DateTime timestamp;      // 时间戳

        public BetStep(int stepId, BetAreaType areaType, int chipValue, decimal amount)
        {
            this.stepId = stepId;
            this.areaType = areaType;
            this.chipValue = chipValue;
            this.amount = amount;
            this.timestamp = DateTime.Now;
        }
    }

    /// <summary>
    /// 投注数据管理器 - 扩展版
    /// 纯数据容器，管理筹码选择和8个投注区域的金额，支持历史记录和事件系统
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

        #region 事件系统

        /// <summary>
        /// 投注金额改变事件
        /// </summary>
        public event Action<BetAreaType> OnBetAmountChanged;

        /// <summary>
        /// 所有投注清空事件
        /// </summary>
        public event Action OnAllBetsCleared;

        /// <summary>
        /// 投注撤销事件
        /// </summary>
        public event Action<BetAreaType> OnBetUndone;

        /// <summary>
        /// 重复投注事件
        /// </summary>
        public event Action OnBetsRepeated;

        /// <summary>
        /// 筹码选择改变事件
        /// </summary>
        public event Action<int> OnChipSelectionChanged;

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
        /// 幸运6投注金额
        /// </summary>
        public decimal Lucky6Amount { get; private set; } = 0;

        /// <summary>
        /// 龙7投注金额
        /// </summary>
        public decimal Long7Amount { get; private set; } = 0;

        /// <summary>
        /// 熊8投注金额
        /// </summary>
        public decimal Xiong8Amount { get; private set; } = 0;

        #endregion

        #region 历史记录和撤销功能

        /// <summary>
        /// 当前局投注历史记录
        /// </summary>
        private List<BetStep> currentBetHistory = new List<BetStep>();

        /// <summary>
        /// 步骤计数器
        /// </summary>
        private int stepCounter = 0;

        /// <summary>
        /// 上一局投注数据备份
        /// </summary>
        private Dictionary<BetAreaType, decimal> lastGameBets = new Dictionary<BetAreaType, decimal>();

        /// <summary>
        /// 获取当前投注历史记录（只读）
        /// </summary>
        public IReadOnlyList<BetStep> CurrentBetHistory => currentBetHistory.AsReadOnly();

        /// <summary>
        /// 获取上一局投注数据（只读）
        /// </summary>
        public IReadOnlyDictionary<BetAreaType, decimal> LastGameBets => lastGameBets;

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
            
            // 初始化上一局数据
            InitializeLastGameBets();
            
            Debug.Log($"[BetDataManager] 数据初始化完成，默认筹码: {CurrentSelectedChip}");
        }

        /// <summary>
        /// 初始化上一局投注数据
        /// </summary>
        private void InitializeLastGameBets()
        {
            lastGameBets.Clear();
            foreach (BetAreaType areaType in Enum.GetValues(typeof(BetAreaType)))
            {
                lastGameBets[areaType] = 0;
            }
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
            OnChipSelectionChanged?.Invoke(CurrentSelectedChip);
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
                    OnChipSelectionChanged?.Invoke(CurrentSelectedChip);
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

        #region 投注区域管理方法 - 获取金额

        /// <summary>
        /// 根据区域类型获取投注金额
        /// </summary>
        /// <param name="areaType">区域类型</param>
        /// <returns>投注金额</returns>
        public decimal GetBetAmount(BetAreaType areaType)
        {
            return areaType switch
            {
                BetAreaType.Banker => BankerAmount,
                BetAreaType.Player => PlayerAmount,
                BetAreaType.Tie => TieAmount,
                BetAreaType.BankerPair => BankerPairAmount,
                BetAreaType.PlayerPair => PlayerPairAmount,
                BetAreaType.Lucky6 => Lucky6Amount,
                BetAreaType.Long7 => Long7Amount,
                BetAreaType.Xiong8 => Xiong8Amount,
                _ => 0
            };
        }

        #endregion

        #region 投注区域管理方法 - 设置金额

        /// <summary>
        /// 设置庄投注金额
        /// </summary>
        /// <param name="amount">投注金额</param>
        public void SetBankerAmount(decimal amount)
        {
            BankerAmount = amount;
            OnBetAmountChanged?.Invoke(BetAreaType.Banker);
            Debug.Log($"[BetDataManager] 庄投注金额: {amount}");
        }

        /// <summary>
        /// 设置闲投注金额
        /// </summary>
        /// <param name="amount">投注金额</param>
        public void SetPlayerAmount(decimal amount)
        {
            PlayerAmount = amount;
            OnBetAmountChanged?.Invoke(BetAreaType.Player);
            Debug.Log($"[BetDataManager] 闲投注金额: {amount}");
        }

        /// <summary>
        /// 设置和投注金额
        /// </summary>
        /// <param name="amount">投注金额</param>
        public void SetTieAmount(decimal amount)
        {
            TieAmount = amount;
            OnBetAmountChanged?.Invoke(BetAreaType.Tie);
            Debug.Log($"[BetDataManager] 和投注金额: {amount}");
        }

        /// <summary>
        /// 设置庄对投注金额
        /// </summary>
        /// <param name="amount">投注金额</param>
        public void SetBankerPairAmount(decimal amount)
        {
            BankerPairAmount = amount;
            OnBetAmountChanged?.Invoke(BetAreaType.BankerPair);
            Debug.Log($"[BetDataManager] 庄对投注金额: {amount}");
        }

        /// <summary>
        /// 设置闲对投注金额
        /// </summary>
        /// <param name="amount">投注金额</param>
        public void SetPlayerPairAmount(decimal amount)
        {
            PlayerPairAmount = amount;
            OnBetAmountChanged?.Invoke(BetAreaType.PlayerPair);
            Debug.Log($"[BetDataManager] 闲对投注金额: {amount}");
        }

        /// <summary>
        /// 设置幸运6投注金额
        /// </summary>
        /// <param name="amount">投注金额</param>
        public void SetLucky6Amount(decimal amount)
        {
            Lucky6Amount = amount;
            OnBetAmountChanged?.Invoke(BetAreaType.Lucky6);
            Debug.Log($"[BetDataManager] 幸运6投注金额: {amount}");
        }

        /// <summary>
        /// 设置龙7投注金额
        /// </summary>
        /// <param name="amount">投注金额</param>
        public void SetLong7Amount(decimal amount)
        {
            Long7Amount = amount;
            OnBetAmountChanged?.Invoke(BetAreaType.Long7);
            Debug.Log($"[BetDataManager] 龙7投注金额: {amount}");
        }

        /// <summary>
        /// 设置熊8投注金额
        /// </summary>
        /// <param name="amount">投注金额</param>
        public void SetXiong8Amount(decimal amount)
        {
            Xiong8Amount = amount;
            OnBetAmountChanged?.Invoke(BetAreaType.Xiong8);
            Debug.Log($"[BetDataManager] 熊8投注金额: {amount}");
        }

        /// <summary>
        /// 根据区域类型设置投注金额
        /// </summary>
        /// <param name="areaType">区域类型</param>
        /// <param name="amount">投注金额</param>
        public void SetBetAmount(BetAreaType areaType, decimal amount)
        {
            switch (areaType)
            {
                case BetAreaType.Banker:
                    SetBankerAmount(amount);
                    break;
                case BetAreaType.Player:
                    SetPlayerAmount(amount);
                    break;
                case BetAreaType.Tie:
                    SetTieAmount(amount);
                    break;
                case BetAreaType.BankerPair:
                    SetBankerPairAmount(amount);
                    break;
                case BetAreaType.PlayerPair:
                    SetPlayerPairAmount(amount);
                    break;
                case BetAreaType.Lucky6:
                    SetLucky6Amount(amount);
                    break;
                case BetAreaType.Long7:
                    SetLong7Amount(amount);
                    break;
                case BetAreaType.Xiong8:
                    SetXiong8Amount(amount);
                    break;
            }
        }

        #endregion

        #region 投注区域管理方法 - 增加金额

        /// <summary>
        /// 增加庄投注金额
        /// </summary>
        /// <param name="amount">增加的金额</param>
        public void AddBankerAmount(decimal amount)
        {
            BankerAmount += amount;
            RecordBetStep(BetAreaType.Banker, (int)amount);
            OnBetAmountChanged?.Invoke(BetAreaType.Banker);
            Debug.Log($"[BetDataManager] 庄投注金额增加: +{amount}, 总计: {BankerAmount}");
        }

        /// <summary>
        /// 增加闲投注金额
        /// </summary>
        /// <param name="amount">增加的金额</param>
        public void AddPlayerAmount(decimal amount)
        {
            PlayerAmount += amount;
            RecordBetStep(BetAreaType.Player, (int)amount);
            OnBetAmountChanged?.Invoke(BetAreaType.Player);
            Debug.Log($"[BetDataManager] 闲投注金额增加: +{amount}, 总计: {PlayerAmount}");
        }

        /// <summary>
        /// 增加和投注金额
        /// </summary>
        /// <param name="amount">增加的金额</param>
        public void AddTieAmount(decimal amount)
        {
            TieAmount += amount;
            RecordBetStep(BetAreaType.Tie, (int)amount);
            OnBetAmountChanged?.Invoke(BetAreaType.Tie);
            Debug.Log($"[BetDataManager] 和投注金额增加: +{amount}, 总计: {TieAmount}");
        }

        /// <summary>
        /// 增加庄对投注金额
        /// </summary>
        /// <param name="amount">增加的金额</param>
        public void AddBankerPairAmount(decimal amount)
        {
            BankerPairAmount += amount;
            RecordBetStep(BetAreaType.BankerPair, (int)amount);
            OnBetAmountChanged?.Invoke(BetAreaType.BankerPair);
            Debug.Log($"[BetDataManager] 庄对投注金额增加: +{amount}, 总计: {BankerPairAmount}");
        }

        /// <summary>
        /// 增加闲对投注金额
        /// </summary>
        /// <param name="amount">增加的金额</param>
        public void AddPlayerPairAmount(decimal amount)
        {
            PlayerPairAmount += amount;
            RecordBetStep(BetAreaType.PlayerPair, (int)amount);
            OnBetAmountChanged?.Invoke(BetAreaType.PlayerPair);
            Debug.Log($"[BetDataManager] 闲对投注金额增加: +{amount}, 总计: {PlayerPairAmount}");
        }

        /// <summary>
        /// 增加幸运6投注金额
        /// </summary>
        /// <param name="amount">增加的金额</param>
        public void AddLucky6Amount(decimal amount)
        {
            Lucky6Amount += amount;
            RecordBetStep(BetAreaType.Lucky6, (int)amount);
            OnBetAmountChanged?.Invoke(BetAreaType.Lucky6);
            Debug.Log($"[BetDataManager] 幸运6投注金额增加: +{amount}, 总计: {Lucky6Amount}");
        }

        /// <summary>
        /// 增加龙7投注金额
        /// </summary>
        /// <param name="amount">增加的金额</param>
        public void AddLong7Amount(decimal amount)
        {
            Long7Amount += amount;
            RecordBetStep(BetAreaType.Long7, (int)amount);
            OnBetAmountChanged?.Invoke(BetAreaType.Long7);
            Debug.Log($"[BetDataManager] 龙7投注金额增加: +{amount}, 总计: {Long7Amount}");
        }

        /// <summary>
        /// 增加熊8投注金额
        /// </summary>
        /// <param name="amount">增加的金额</param>
        public void AddXiong8Amount(decimal amount)
        {
            Xiong8Amount += amount;
            RecordBetStep(BetAreaType.Xiong8, (int)amount);
            OnBetAmountChanged?.Invoke(BetAreaType.Xiong8);
            Debug.Log($"[BetDataManager] 熊8投注金额增加: +{amount}, 总计: {Xiong8Amount}");
        }

        #endregion

        #region 历史记录管理

        /// <summary>
        /// 记录投注步骤
        /// </summary>
        /// <param name="areaType">投注区域</param>
        /// <param name="chipValue">筹码值</param>
        private void RecordBetStep(BetAreaType areaType, int chipValue)
        {
            var betStep = new BetStep(++stepCounter, areaType, chipValue, chipValue);
            currentBetHistory.Add(betStep);
            Debug.Log($"[BetDataManager] 记录投注步骤: {betStep.stepId} - {areaType} - {chipValue}");
        }

        /// <summary>
        /// 撤销上一步投注
        /// </summary>
        /// <returns>是否撤销成功</returns>
        public bool UndoLastBet()
        {
            if (currentBetHistory.Count == 0)
            {
                Debug.LogWarning("[BetDataManager] 没有可撤销的投注步骤");
                return false;
            }

            // 获取最后一步投注
            var lastStep = currentBetHistory[currentBetHistory.Count - 1];
            currentBetHistory.RemoveAt(currentBetHistory.Count - 1);

            // 减少对应区域的投注金额
            var currentAmount = GetBetAmount(lastStep.areaType);
            var newAmount = currentAmount - lastStep.amount;
            SetBetAmount(lastStep.areaType, newAmount);

            OnBetUndone?.Invoke(lastStep.areaType);
            Debug.Log($"[BetDataManager] 撤销投注步骤: {lastStep.stepId} - {lastStep.areaType} - {lastStep.amount}");
            return true;
        }

        /// <summary>
        /// 清空投注历史
        /// </summary>
        public void ClearBetHistory()
        {
            currentBetHistory.Clear();
            stepCounter = 0;
            Debug.Log("[BetDataManager] 投注历史已清空");
        }

        #endregion

        #region 重复投注功能

        /// <summary>
        /// 保存当前投注为上一局数据
        /// </summary>
        public void SaveCurrentGameBets()
        {
            lastGameBets[BetAreaType.Banker] = BankerAmount;
            lastGameBets[BetAreaType.Player] = PlayerAmount;
            lastGameBets[BetAreaType.Tie] = TieAmount;
            lastGameBets[BetAreaType.BankerPair] = BankerPairAmount;
            lastGameBets[BetAreaType.PlayerPair] = PlayerPairAmount;
            lastGameBets[BetAreaType.Lucky6] = Lucky6Amount;
            lastGameBets[BetAreaType.Long7] = Long7Amount;
            lastGameBets[BetAreaType.Xiong8] = Xiong8Amount;

            Debug.Log("[BetDataManager] 当前投注数据已保存为上一局数据");
        }

        /// <summary>
        /// 恢复上一局投注数据
        /// </summary>
        /// <returns>是否恢复成功</returns>
        public bool RestoreLastGameBets()
        {
            if (lastGameBets.Count == 0 || lastGameBets.Values.All(amount => amount == 0))
            {
                Debug.LogWarning("[BetDataManager] 没有可恢复的上一局投注数据");
                return false;
            }

            // 先清空当前投注
            ClearAllBets();

            // 恢复上一局数据
            SetBetAmount(BetAreaType.Banker, lastGameBets[BetAreaType.Banker]);
            SetBetAmount(BetAreaType.Player, lastGameBets[BetAreaType.Player]);
            SetBetAmount(BetAreaType.Tie, lastGameBets[BetAreaType.Tie]);
            SetBetAmount(BetAreaType.BankerPair, lastGameBets[BetAreaType.BankerPair]);
            SetBetAmount(BetAreaType.PlayerPair, lastGameBets[BetAreaType.PlayerPair]);
            SetBetAmount(BetAreaType.Lucky6, lastGameBets[BetAreaType.Lucky6]);
            SetBetAmount(BetAreaType.Long7, lastGameBets[BetAreaType.Long7]);
            SetBetAmount(BetAreaType.Xiong8, lastGameBets[BetAreaType.Xiong8]);

            OnBetsRepeated?.Invoke();
            Debug.Log("[BetDataManager] 上一局投注数据已恢复");
            return true;
        }

        /// <summary>
        /// 检查是否有上一局投注数据
        /// </summary>
        /// <returns>是否有上一局数据</returns>
        public bool HasLastGameBets()
        {
            return lastGameBets.Values.Any(amount => amount > 0);
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
            Lucky6Amount = 0;
            Long7Amount = 0;
            Xiong8Amount = 0;
            
            ClearBetHistory();
            OnAllBetsCleared?.Invoke();
            Debug.Log("[BetDataManager] 所有投注金额已清空");
        }

        /// <summary>
        /// 获取总投注金额
        /// </summary>
        /// <returns>总投注金额</returns>
        public decimal GetTotalBetAmount()
        {
            decimal total = BankerAmount + PlayerAmount + TieAmount + BankerPairAmount + 
                           PlayerPairAmount + Lucky6Amount + Long7Amount + Xiong8Amount;
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
                   $"幸运6: {Lucky6Amount}, 龙7: {Long7Amount}, 熊8: {Xiong8Amount}\n" +
                   $"总投注: {GetTotalBetAmount()}\n" +
                   $"投注步骤数: {currentBetHistory.Count}";
        }

        /// <summary>
        /// 获取指定区域的筹码组合
        /// </summary>
        /// <param name="areaType">区域类型</param>
        /// <returns>筹码值列表</returns>
        public List<int> GetChipCombination(BetAreaType areaType)
        {
            return currentBetHistory
                .Where(step => step.areaType == areaType)
                .Select(step => step.chipValue)
                .ToList();
        }

        #endregion
    }
}
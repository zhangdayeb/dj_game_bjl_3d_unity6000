// Assets/Game/Managers/BettingManager.cs - 合并免佣功能版本
using System.Collections.Generic;
using UnityEngine;
using BaccaratGame.Data;

namespace BaccaratGame.Game.Managers
{
    /// <summary>
    /// 投注管理器 - 包含免佣功能
    /// 负责投注逻辑和免佣设置管理
    /// </summary>
    public class BettingManager : MonoBehaviour
    {
        [Header("投注限制")]
        [SerializeField] private decimal _minBetAmount = 10m;
        [SerializeField] private decimal _maxBetAmount = 10000m;
        [SerializeField] private decimal _maxTotalBetPerRound = 50000m;

        [Header("免佣设置")]
        [SerializeField] private bool _isExemptEnabled = false;
        [SerializeField] private string _exemptStorageKey = "exempt_setting";

        #region 私有字段

        // 当前回合投注数据
        private Dictionary<BaccaratBetType, decimal> _currentBets = new Dictionary<BaccaratBetType, decimal>();

        #endregion

        #region 公共属性

        /// <summary>
        /// 获取总投注金额
        /// </summary>
        public decimal TotalBetAmount 
        { 
            get 
            { 
                decimal total = 0m;
                foreach (var amount in _currentBets.Values)
                {
                    total += amount;
                }
                return total;
            } 
        }

        /// <summary>
        /// 是否有投注
        /// </summary>
        public bool HasBets => _currentBets.Count > 0;

        /// <summary>
        /// 是否启用免佣
        /// </summary>
        public bool IsExemptEnabled => _isExemptEnabled;

        /// <summary>
        /// 最小投注金额
        /// </summary>
        public decimal MinBetAmount => _minBetAmount;

        /// <summary>
        /// 最大投注金额
        /// </summary>
        public decimal MaxBetAmount => _maxBetAmount;

        /// <summary>
        /// 单局最大投注总额
        /// </summary>
        public decimal MaxTotalBetPerRound => _maxTotalBetPerRound;

        #endregion

        #region Unity生命周期

        private void Start()
        {
            LoadExemptPreference();
        }

        #endregion

        #region 投注功能

        /// <summary>
        /// 在指定区域下注
        /// </summary>
        public void PlaceBet(BaccaratBetType betType, decimal amount)
        {
            if (amount <= 0) return;

            // 如果该区域已有投注，累加金额
            if (_currentBets.ContainsKey(betType))
            {
                _currentBets[betType] += amount;
            }
            else
            {
                _currentBets[betType] = amount;
            }

            Debug.Log($"投注: {betType} - {amount} (区域总额: {_currentBets[betType]})");
        }

        /// <summary>
        /// 获取指定区域的投注金额
        /// </summary>
        public decimal GetBetAmount(BaccaratBetType betType)
        {
            return _currentBets.ContainsKey(betType) ? _currentBets[betType] : 0m;
        }

        /// <summary>
        /// 获取所有投注数据
        /// </summary>
        public Dictionary<BaccaratBetType, decimal> GetAllBets()
        {
            return new Dictionary<BaccaratBetType, decimal>(_currentBets);
        }

        /// <summary>
        /// 清空所有投注 (新回合开始时调用)
        /// </summary>
        public void ClearAllBets()
        {
            _currentBets.Clear();
            Debug.Log("所有投注已清空");
        }

        /// <summary>
        /// 倒计时结束时发送订单
        /// </summary>
        public void SubmitBetOrder()
        {
            if (!HasBets)
            {
                Debug.Log("没有投注，无需发送订单");
                return;
            }

            Debug.Log($"发送投注订单 - 总金额: {TotalBetAmount}, 投注项目: {_currentBets.Count}, 免佣: {_isExemptEnabled}");
            
            foreach (var bet in _currentBets)
            {
                Debug.Log($"  {bet.Key}: {bet.Value}");
            }

            // TODO: 触发网络发送事件，包含免佣状态
            // NetworkEvents.TriggerBetOrderSubmit(new BetOrder { 
            //     bets = _currentBets, 
            //     isExempt = _isExemptEnabled 
            // });
        }

        #endregion

        #region 免佣功能

        /// <summary>
        /// 切换免佣状态
        /// </summary>
        public void ToggleExempt()
        {
            _isExemptEnabled = !_isExemptEnabled;
            SaveExemptPreference();
            Debug.Log($"免佣状态: {(_isExemptEnabled ? "开启" : "关闭")}");
        }

        /// <summary>
        /// 设置免佣状态
        /// </summary>
        public void SetExemptEnabled(bool enabled)
        {
            _isExemptEnabled = enabled;
            SaveExemptPreference();
            Debug.Log($"免佣状态: {(_isExemptEnabled ? "开启" : "关闭")}");
        }

        #endregion

        #region 免佣持久化

        /// <summary>
        /// 加载免佣偏好
        /// </summary>
        private void LoadExemptPreference()
        {
            _isExemptEnabled = PlayerPrefs.GetInt(_exemptStorageKey, 0) == 1;
        }

        /// <summary>
        /// 保存免佣偏好
        /// </summary>
        private void SaveExemptPreference()
        {
            PlayerPrefs.SetInt(_exemptStorageKey, _isExemptEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        #endregion
    }
}
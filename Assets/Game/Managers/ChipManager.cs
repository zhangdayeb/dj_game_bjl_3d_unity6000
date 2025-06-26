// Assets/Game/Managers/ChipManager.cs - 最小化版本
using System.Collections.Generic;
using UnityEngine;
using BaccaratGame.Data;

namespace BaccaratGame.Game.Managers
{
    /// <summary>
    /// 筹码管理器 - 最小化版本
    /// 只负责筹码选择和切换
    /// </summary>
    public class ChipManager : MonoBehaviour
    {
        [Header("筹码配置")]
        [SerializeField] private List<ChipData> _availableChips = new List<ChipData>();

        #region 私有字段

        private ChipData _currentChip;
        private int _currentChipIndex = 0;

        #endregion

        #region 核心功能

        /// <summary>
        /// 获取可用筹码列表
        /// </summary>
        public List<ChipData> GetAvailableChips()
        {
            return _availableChips;
        }

        /// <summary>
        /// 设置当前筹码
        /// </summary>
        public void SetCurrentChip(ChipData chipData)
        {
            if (chipData == null) return;

            var index = _availableChips.IndexOf(chipData);
            if (index >= 0)
            {
                _currentChip = chipData;
                _currentChipIndex = index;
            }
        }

        /// <summary>
        /// 选择下一个筹码
        /// </summary>
        public void SelectNextChip()
        {
            if (_availableChips.Count == 0) return;

            _currentChipIndex = (_currentChipIndex + 1) % _availableChips.Count;
            _currentChip = _availableChips[_currentChipIndex];
        }

        /// <summary>
        /// 选择上一个筹码
        /// </summary>
        public void SelectPreviousChip()
        {
            if (_availableChips.Count == 0) return;

            _currentChipIndex = (_currentChipIndex - 1 + _availableChips.Count) % _availableChips.Count;
            _currentChip = _availableChips[_currentChipIndex];
        }

        /// <summary>
        /// 获取当前筹码
        /// </summary>
        public ChipData GetCurrentChip()
        {
            return _currentChip;
        }

        #endregion

        #region Unity生命周期

        private void Start()
        {
            // 设置默认筹码
            if (_availableChips.Count > 0)
            {
                SetCurrentChip(_availableChips[0]);
            }
        }

        #endregion
    }
}
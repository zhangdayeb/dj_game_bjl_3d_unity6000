// Assets/Core/Audio/BaccaratAudioController.cs
// 百家乐音效控制器 - 重构版
// 职责：实现百家乐游戏专用音效播放，通过AudioManager播放具体音效
// 特点：事件驱动，语义化方法，业务逻辑封装

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaccaratGame.Core.Events;
using BaccaratGame.Core.Audio;
using BaccaratGame.Data;

namespace BaccaratGame.Core.Audio
{
    /// <summary>
    /// 百家乐音效控制器
    /// 负责百家乐游戏专用音效的播放控制和业务逻辑
    /// </summary>
    public class BaccaratAudioController : MonoBehaviour
    {
        #region 配置设置

        [Header("🎯 音效开关")]
        [SerializeField] private bool _enableBaccaratAudio = true;
        [SerializeField] private bool _enableBettingAudio = true;
        [SerializeField] private bool _enableCardAudio = true;
        [SerializeField] private bool _enableWinAudio = true;
        [SerializeField] private bool _enableUIAudio = true;

        [Header("🎛️ 音效音量")]
        [SerializeField, Range(0f, 1f)] private float _bettingAudioVolume = 1.0f;
        [SerializeField, Range(0f, 1f)] private float _cardAudioVolume = 0.9f;
        [SerializeField, Range(0f, 1f)] private float _winAudioVolume = 1.0f;
        [SerializeField, Range(0f, 1f)] private float _uiAudioVolume = 0.8f;

        [Header("🐛 调试设置")]
        [SerializeField] private bool _enableDebugLogs = true;

        #endregion

        #region 音效文件名配置

        // 🎯 投注阶段音效文件名
        private const string BETTING_START_SFX = "betting_start";        // "请下注"
        private const string BETTING_END_SFX = "betting_end";            // "停止投注"
        private const string CHIP_CLICK_SFX = "chip_click";              // 筹码点击
        private const string CHIP_SLIDE_SFX = "chip_slide";              // 筹码滑动
        private const string BET_PLACE_SFX = "bet_place";                // 投注区域点击

        // 🃏 发牌阶段音效文件名
        private const string CARD_REVEAL_SFX = "card_reveal";            // 开牌音效

        // 🏆 结果阶段音效文件名
        private const string WIN_SOUND_SFX = "win_sound";                // 中奖音效
        private const string ERROR_SOUND_SFX = "error_sound";            // 错误音效

        // 🔧 基础音效文件名
        private const string BUTTON_CLICK_SFX = "button_click";          // 按钮点击
        private const string CONFIRM_SOUND_SFX = "confirm_sound";        // 确认音效
        private const string CANCEL_SOUND_SFX = "cancel_sound";          // 取消音效
        private const string NOTIFICATION_SFX = "notification_sound";    // 通知音效

        #endregion

        #region 私有字段

        private AudioManager _audioManager;
        private bool _isInitialized = false;

        #endregion

        #region Unity 生命周期

        private void Awake()
        {
            InitializeController();
        }

        private void Start()
        {
            SubscribeToEvents();
            LoadUserSettings();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            SaveUserSettings();
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化控制器
        /// </summary>
        private void InitializeController()
        {
            _audioManager = AudioManager.Instance;
            
            if (_audioManager == null)
            {
                Debug.LogError("[BaccaratAudioController] ❌ AudioManager未找到!");
                return;
            }

            _isInitialized = true;
            DebugLog("🎲 百家乐音效控制器初始化完成");
        }

        /// <summary>
        /// 加载用户设置
        /// </summary>
        private void LoadUserSettings()
        {
            _enableBaccaratAudio = PlayerPrefs.GetInt("BaccaratAudio_Enabled", 1) == 1;
            _enableBettingAudio = PlayerPrefs.GetInt("BaccaratAudio_BettingEnabled", 1) == 1;
            _enableCardAudio = PlayerPrefs.GetInt("BaccaratAudio_CardEnabled", 1) == 1;
            _enableWinAudio = PlayerPrefs.GetInt("BaccaratAudio_WinEnabled", 1) == 1;
            _enableUIAudio = PlayerPrefs.GetInt("BaccaratAudio_UIEnabled", 1) == 1;
            
            _bettingAudioVolume = PlayerPrefs.GetFloat("BaccaratAudio_BettingVolume", 1.0f);
            _cardAudioVolume = PlayerPrefs.GetFloat("BaccaratAudio_CardVolume", 0.9f);
            _winAudioVolume = PlayerPrefs.GetFloat("BaccaratAudio_WinVolume", 1.0f);
            _uiAudioVolume = PlayerPrefs.GetFloat("BaccaratAudio_UIVolume", 0.8f);
            
            DebugLog("💾 用户音效设置已加载");
        }

        /// <summary>
        /// 保存用户设置
        /// </summary>
        private void SaveUserSettings()
        {
            PlayerPrefs.SetInt("BaccaratAudio_Enabled", _enableBaccaratAudio ? 1 : 0);
            PlayerPrefs.SetInt("BaccaratAudio_BettingEnabled", _enableBettingAudio ? 1 : 0);
            PlayerPrefs.SetInt("BaccaratAudio_CardEnabled", _enableCardAudio ? 1 : 0);
            PlayerPrefs.SetInt("BaccaratAudio_WinEnabled", _enableWinAudio ? 1 : 0);
            PlayerPrefs.SetInt("BaccaratAudio_UIEnabled", _enableUIAudio ? 1 : 0);
            
            PlayerPrefs.SetFloat("BaccaratAudio_BettingVolume", _bettingAudioVolume);
            PlayerPrefs.SetFloat("BaccaratAudio_CardVolume", _cardAudioVolume);
            PlayerPrefs.SetFloat("BaccaratAudio_WinVolume", _winAudioVolume);
            PlayerPrefs.SetFloat("BaccaratAudio_UIVolume", _uiAudioVolume);
            
            PlayerPrefs.Save();
            DebugLog("💾 用户音效设置已保存");
        }

        #endregion

        #region 事件订阅

        /// <summary>
        /// 订阅游戏事件
        /// </summary>
        private void SubscribeToEvents()
        {
            // 投注相关事件
            // GameEvents.OnBettingPhaseStarted += OnBettingPhaseStarted;
            // GameEvents.OnBettingPhaseEnded += OnBettingPhaseEnded;
            // GameEvents.OnBetPlaced += OnBetPlaced;
            // GameEvents.OnChipSelected += OnChipSelected;
            
            // 发牌相关事件
            // GameEvents.OnCardRevealed += OnCardRevealed;
            
            // 结果相关事件
            // GameEvents.OnPlayerWin += OnPlayerWin;
            // GameEvents.OnGameError += OnGameError;

            DebugLog("📡 游戏事件订阅完成");
        }

        /// <summary>
        /// 取消事件订阅
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            // 取消投注相关事件
            // GameEvents.OnBettingPhaseStarted -= OnBettingPhaseStarted;
            // GameEvents.OnBettingPhaseEnded -= OnBettingPhaseEnded;
            // GameEvents.OnBetPlaced -= OnBetPlaced;
            // GameEvents.OnChipSelected -= OnChipSelected;
            
            // 取消发牌相关事件
            // GameEvents.OnCardRevealed -= OnCardRevealed;
            
            // 取消结果相关事件
            // GameEvents.OnPlayerWin -= OnPlayerWin;
            // GameEvents.OnGameError -= OnGameError;

            DebugLog("📡 游戏事件取消订阅完成");
        }

        #endregion

        #region 🎯 投注阶段音效

        /// <summary>
        /// 播放"请下注"音效
        /// </summary>
        public void PlayBettingStartSound()
        {
            if (!ShouldPlayBettingAudio()) return;
            
            _audioManager.PlaySoundEffect(BETTING_START_SFX, _bettingAudioVolume);
            DebugLog("🔔 播放投注开始音效: 请下注");
        }

        /// <summary>
        /// 播放"停止投注"音效
        /// </summary>
        public void PlayBettingEndSound()
        {
            if (!ShouldPlayBettingAudio()) return;
            
            _audioManager.PlaySoundEffect(BETTING_END_SFX, _bettingAudioVolume);
            DebugLog("⏰ 播放投注结束音效: 停止投注");
        }

        /// <summary>
        /// 播放筹码点击音效
        /// </summary>
        public void PlayChipClickSound()
        {
            if (!ShouldPlayBettingAudio()) return;
            
            _audioManager.PlaySoundEffect(CHIP_CLICK_SFX, _bettingAudioVolume);
            DebugLog("💰 播放筹码点击音效");
        }

        /// <summary>
        /// 播放筹码滑动音效
        /// </summary>
        public void PlayChipSlideSound()
        {
            if (!ShouldPlayBettingAudio()) return;
            
            _audioManager.PlaySoundEffect(CHIP_SLIDE_SFX, _bettingAudioVolume);
            DebugLog("🎯 播放筹码滑动音效");
        }

        /// <summary>
        /// 播放投注区域点击音效
        /// </summary>
        public void PlayBetPlaceSound()
        {
            if (!ShouldPlayBettingAudio()) return;
            
            _audioManager.PlaySoundEffect(BET_PLACE_SFX, _bettingAudioVolume);
            DebugLog("📍 播放投注区域点击音效");
        }

        #endregion

        #region 🃏 发牌阶段音效

        /// <summary>
        /// 播放开牌音效
        /// </summary>
        public void PlayCardRevealSound()
        {
            if (!ShouldPlayCardAudio()) return;
            
            _audioManager.PlaySoundEffect(CARD_REVEAL_SFX, _cardAudioVolume);
            DebugLog("🎴 播放开牌音效");
        }

        #endregion

        #region 🏆 结果阶段音效

        /// <summary>
        /// 播放中奖音效
        /// </summary>
        /// <param name="winAmount">中奖金额（可选，用于选择不同音效）</param>
        public void PlayWinSound(decimal winAmount = 0)
        {
            if (!ShouldPlayWinAudio()) return;
            
            _audioManager.PlaySoundEffect(WIN_SOUND_SFX, _winAudioVolume);
            
            if (winAmount > 0)
            {
                DebugLog("🎉 播放中奖音效 (金额: " + winAmount + ")");
            }
            else
            {
                DebugLog("🎉 播放中奖音效");
            }
        }

        /// <summary>
        /// 播放错误音效
        /// </summary>
        /// <param name="errorMessage">错误信息（可选）</param>
        public void PlayErrorSound(string errorMessage = "")
        {
            if (!ShouldPlayUIAudio()) return;
            
            _audioManager.PlaySoundEffect(ERROR_SOUND_SFX, _uiAudioVolume);
            
            if (!string.IsNullOrEmpty(errorMessage))
            {
                DebugLog("❌ 播放错误音效: " + errorMessage);
            }
            else
            {
                DebugLog("❌ 播放错误音效");
            }
        }

        #endregion

        #region 🔧 基础音效

        /// <summary>
        /// 播放按钮点击音效
        /// </summary>
        /// <param name="buttonName">按钮名称（可选）</param>
        public void PlayButtonClickSound(string buttonName = "")
        {
            if (!ShouldPlayUIAudio()) return;
            
            _audioManager.PlaySoundEffect(BUTTON_CLICK_SFX, _uiAudioVolume);
            
            if (!string.IsNullOrEmpty(buttonName))
            {
                DebugLog("🖱️ 播放按钮点击音效: " + buttonName);
            }
            else
            {
                DebugLog("🖱️ 播放按钮点击音效");
            }
        }

        /// <summary>
        /// 播放确认音效
        /// </summary>
        public void PlayConfirmSound()
        {
            if (!ShouldPlayUIAudio()) return;
            
            _audioManager.PlaySoundEffect(CONFIRM_SOUND_SFX, _uiAudioVolume);
            DebugLog("✅ 播放确认音效");
        }

        /// <summary>
        /// 播放取消音效
        /// </summary>
        public void PlayCancelSound()
        {
            if (!ShouldPlayUIAudio()) return;
            
            _audioManager.PlaySoundEffect(CANCEL_SOUND_SFX, _uiAudioVolume);
            DebugLog("❌ 播放取消音效");
        }

        /// <summary>
        /// 播放通知音效
        /// </summary>
        /// <param name="notificationMessage">通知信息（可选）</param>
        public void PlayNotificationSound(string notificationMessage = "")
        {
            if (!ShouldPlayUIAudio()) return;
            
            _audioManager.PlaySoundEffect(NOTIFICATION_SFX, _uiAudioVolume);
            
            if (!string.IsNullOrEmpty(notificationMessage))
            {
                DebugLog("📢 播放通知音效: " + notificationMessage);
            }
            else
            {
                DebugLog("📢 播放通知音效");
            }
        }

        #endregion

        #region 🎵 背景音乐控制

        /// <summary>
        /// 播放游戏背景音乐
        /// </summary>
        public void PlayGameBackgroundMusic()
        {
            if (!_isInitialized) return;
            
            _audioManager.PlayBackgroundMusic("main_bgm");
            DebugLog("🎵 播放游戏背景音乐");
        }

        /// <summary>
        /// 暂停背景音乐
        /// </summary>
        public void PauseBackgroundMusic()
        {
            if (!_isInitialized) return;
            
            _audioManager.PauseBGM();
            DebugLog("⏸️ 暂停背景音乐");
        }

        /// <summary>
        /// 恢复背景音乐
        /// </summary>
        public void ResumeBackgroundMusic()
        {
            if (!_isInitialized) return;
            
            _audioManager.ResumeBGM();
            DebugLog("▶️ 恢复背景音乐");
        }

        #endregion

        #region 🎛️ 音效设置控制

        /// <summary>
        /// 设置百家乐音效总开关
        /// </summary>
        /// <param name="enabled">是否启用</param>
        public void SetBaccaratAudioEnabled(bool enabled)
        {
            _enableBaccaratAudio = enabled;
            DebugLog("🎲 百家乐音效" + (enabled ? "启用" : "禁用"));
        }

        /// <summary>
        /// 设置投注音效开关
        /// </summary>
        /// <param name="enabled">是否启用</param>
        public void SetBettingAudioEnabled(bool enabled)
        {
            _enableBettingAudio = enabled;
            DebugLog("🎯 投注音效" + (enabled ? "启用" : "禁用"));
        }

        /// <summary>
        /// 设置发牌音效开关
        /// </summary>
        /// <param name="enabled">是否启用</param>
        public void SetCardAudioEnabled(bool enabled)
        {
            _enableCardAudio = enabled;
            DebugLog("🃏 发牌音效" + (enabled ? "启用" : "禁用"));
        }

        /// <summary>
        /// 设置中奖音效开关
        /// </summary>
        /// <param name="enabled">是否启用</param>
        public void SetWinAudioEnabled(bool enabled)
        {
            _enableWinAudio = enabled;
            DebugLog("🏆 中奖音效" + (enabled ? "启用" : "禁用"));
        }

        /// <summary>
        /// 设置UI音效开关
        /// </summary>
        /// <param name="enabled">是否启用</param>
        public void SetUIAudioEnabled(bool enabled)
        {
            _enableUIAudio = enabled;
            DebugLog("🔧 UI音效" + (enabled ? "启用" : "禁用"));
        }

        #endregion

        #region 条件检查方法

        /// <summary>
        /// 检查是否应该播放投注音效
        /// </summary>
        private bool ShouldPlayBettingAudio()
        {
            return _enableBaccaratAudio && _enableBettingAudio && _isInitialized;
        }

        /// <summary>
        /// 检查是否应该播放发牌音效
        /// </summary>
        private bool ShouldPlayCardAudio()
        {
            return _enableBaccaratAudio && _enableCardAudio && _isInitialized;
        }

        /// <summary>
        /// 检查是否应该播放中奖音效
        /// </summary>
        private bool ShouldPlayWinAudio()
        {
            return _enableBaccaratAudio && _enableWinAudio && _isInitialized;
        }

        /// <summary>
        /// 检查是否应该播放UI音效
        /// </summary>
        private bool ShouldPlayUIAudio()
        {
            return _enableBaccaratAudio && _enableUIAudio && _isInitialized;
        }

        #endregion

        #region 🧪 测试和调试

        /// <summary>
        /// 测试所有百家乐音效
        /// </summary>
        public void TestAllBaccaratSounds()
        {
            if (!_isInitialized)
            {
                DebugLog("❌ 无法测试音效: 控制器未初始化");
                return;
            }
            
            StartCoroutine(TestSoundSequence());
        }

        /// <summary>
        /// 音效测试序列
        /// </summary>
        private IEnumerator TestSoundSequence()
        {
            DebugLog("🧪 开始百家乐音效测试序列");
            
            // 测试投注音效
            PlayBettingStartSound();
            yield return new WaitForSeconds(1f);
            
            PlayChipClickSound();
            yield return new WaitForSeconds(0.5f);
            
            PlayChipSlideSound();
            yield return new WaitForSeconds(0.5f);
            
            PlayBetPlaceSound();
            yield return new WaitForSeconds(1f);
            
            PlayBettingEndSound();
            yield return new WaitForSeconds(1f);
            
            // 测试发牌音效
            PlayCardRevealSound();
            yield return new WaitForSeconds(1f);
            
            // 测试结果音效
            PlayWinSound(100m);
            yield return new WaitForSeconds(1f);
            
            // 测试基础音效
            PlayButtonClickSound("测试按钮");
            yield return new WaitForSeconds(0.5f);
            
            PlayConfirmSound();
            yield return new WaitForSeconds(0.5f);
            
            PlayNotificationSound("测试通知");
            yield return new WaitForSeconds(0.5f);
            
            DebugLog("✅ 百家乐音效测试序列完成");
        }

        /// <summary>
        /// 输出控制器状态信息
        /// </summary>
        public void DebugControllerStatus()
        {
            DebugLog("=== BaccaratAudioController 状态信息 ===");
            DebugLog("初始化状态: " + _isInitialized);
            DebugLog("百家乐音效: " + _enableBaccaratAudio);
            DebugLog("投注音效: " + _enableBettingAudio);
            DebugLog("发牌音效: " + _enableCardAudio);
            DebugLog("中奖音效: " + _enableWinAudio);
            DebugLog("UI音效: " + _enableUIAudio);
            DebugLog("AudioManager状态: " + (_audioManager != null ? "正常" : "缺失"));
        }

        #endregion

        #region 🐛 调试工具

        /// <summary>
        /// 调试日志输出
        /// </summary>
        /// <param name="message">日志信息</param>
        private void DebugLog(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log("[BaccaratAudioController] " + message);
            }
        }

        #endregion

        #region 公开属性

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// 百家乐音效是否启用
        /// </summary>
        public bool IsBaccaratAudioEnabled => _enableBaccaratAudio;

        /// <summary>
        /// 投注音效是否启用
        /// </summary>
        public bool IsBettingAudioEnabled => _enableBettingAudio;

        /// <summary>
        /// 发牌音效是否启用
        /// </summary>
        public bool IsCardAudioEnabled => _enableCardAudio;

        /// <summary>
        /// 中奖音效是否启用
        /// </summary>
        public bool IsWinAudioEnabled => _enableWinAudio;

        /// <summary>
        /// UI音效是否启用
        /// </summary>
        public bool IsUIAudioEnabled => _enableUIAudio;

        #endregion
    }
}
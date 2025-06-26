// Assets/Core/Audio/AudioManager.cs
// 基础音频管理器 - 重构版
// 职责：区分背景音乐和音效，实现播放/停止，暂停/恢复，音量控制
// 特点：无预加载，BGM使用暂停/恢复机制，每次播放都有Debug.Log

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaccaratGame.Data;

namespace BaccaratGame.Core.Audio
{
    /// <summary>
    /// 基础音频管理器
    /// 负责背景音乐和音效的底层播放控制
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region 单例模式

        private static AudioManager _instance;
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<AudioManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("AudioManager");
                        _instance = go.AddComponent<AudioManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region 配置设置

        [Header("🎛️ 音量设置")]
        [SerializeField, Range(0f, 1f)] private float _masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _bgmVolume = 0.6f;
        [SerializeField, Range(0f, 1f)] private float _sfxVolume = 0.8f;

        [Header("📁 资源设置")]
        [SerializeField] private string _bgmResourcePath = "Audio/BGM/";
        [SerializeField] private string _sfxResourcePath = "Audio/SFX/";

        [Header("🐛 调试设置")]
        [SerializeField] private bool _enableDebugLogs = true;
        [SerializeField] private bool _enableAudio = true;

        #endregion

        #region 私有字段

        // 音频源
        private AudioSource _bgmAudioSource;
        private AudioSource _sfxAudioSource;

        // 当前状态
        private string _currentBGM = "";
        private bool _isBGMPaused = false;
        private bool _isInitialized = false;

        // 音频缓存（临时缓存，不做预加载）
        private Dictionary<string, AudioClip> _audioCache = new Dictionary<string, AudioClip>();

        #endregion

        #region Unity 生命周期

        private void Awake()
        {
            // 单例模式处理
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioManager();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void OnDestroy()
        {
            // 清理缓存
            _audioCache.Clear();
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化音频管理器
        /// </summary>
        private void InitializeAudioManager()
        {
            CreateAudioSources();
            _isInitialized = true;
            DebugLog("🎵 AudioManager 初始化完成");
        }

        /// <summary>
        /// 创建音频源
        /// </summary>
        private void CreateAudioSources()
        {
            // 创建背景音乐音频源
            _bgmAudioSource = gameObject.AddComponent<AudioSource>();
            _bgmAudioSource.loop = true;
            _bgmAudioSource.playOnAwake = false;
            _bgmAudioSource.volume = _bgmVolume * _masterVolume;

            // 创建音效音频源
            _sfxAudioSource = gameObject.AddComponent<AudioSource>();
            _sfxAudioSource.loop = false;
            _sfxAudioSource.playOnAwake = false;
            _sfxAudioSource.volume = _sfxVolume * _masterVolume;

            DebugLog("🔧 音频源创建完成 - BGM音量: " + _bgmAudioSource.volume + ", SFX音量: " + _sfxAudioSource.volume);
        }

        #endregion

        #region 🎵 背景音乐管理

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="musicName">音乐文件名（不含扩展名）</param>
        public void PlayBackgroundMusic(string musicName)
        {
            if (!_enableAudio || !_isInitialized || _bgmAudioSource == null)
            {
                DebugLog("❌ 无法播放背景音乐: 音频被禁用或未初始化");
                return;
            }

            // 如果已经在播放相同的音乐，直接返回
            if (_currentBGM == musicName && _bgmAudioSource.isPlaying)
            {
                DebugLog("ℹ️ 背景音乐已在播放: " + musicName);
                return;
            }

            // 加载音频文件
            AudioClip bgmClip = LoadAudioClip(musicName, _bgmResourcePath);
            if (bgmClip == null)
            {
                DebugLog("❌ 背景音乐加载失败: " + musicName);
                return;
            }

            // 停止当前播放的音乐
            if (_bgmAudioSource.isPlaying)
            {
                _bgmAudioSource.Stop();
            }

            // 播放新音乐
            _bgmAudioSource.clip = bgmClip;
            _bgmAudioSource.volume = _bgmVolume * _masterVolume;
            _bgmAudioSource.Play();
            
            _currentBGM = musicName;
            _isBGMPaused = false;

            DebugLog("🎵 播放背景音乐: " + musicName + " (音量: " + _bgmAudioSource.volume.ToString("F2") + ")");
        }

        /// <summary>
        /// 暂停背景音乐
        /// </summary>
        public void PauseBGM()
        {
            if (!_isInitialized || _bgmAudioSource == null)
                return;

            if (_bgmAudioSource.isPlaying)
            {
                _bgmAudioSource.Pause();
                _isBGMPaused = true;
                DebugLog("⏸️ 暂停背景音乐: " + _currentBGM);
            }
        }

        /// <summary>
        /// 恢复背景音乐
        /// </summary>
        public void ResumeBGM()
        {
            if (!_isInitialized || _bgmAudioSource == null)
                return;

            if (_isBGMPaused && !_bgmAudioSource.isPlaying)
            {
                _bgmAudioSource.UnPause();
                _isBGMPaused = false;
                DebugLog("▶️ 恢复背景音乐: " + _currentBGM);
            }
        }

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        public void StopBGM()
        {
            if (!_isInitialized || _bgmAudioSource == null)
                return;

            if (_bgmAudioSource.isPlaying || _isBGMPaused)
            {
                _bgmAudioSource.Stop();
                DebugLog("⏹️ 停止背景音乐: " + _currentBGM);
                _currentBGM = "";
                _isBGMPaused = false;
            }
        }

        #endregion

        #region 🔊 音效管理

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="soundName">音效文件名（不含扩展名）</param>
        /// <param name="volume">音量倍数 (0-1)</param>
        public void PlaySoundEffect(string soundName, float volume = 1f)
        {
            if (!_enableAudio || !_isInitialized || _sfxAudioSource == null)
            {
                DebugLog("❌ 无法播放音效: 音频被禁用或未初始化");
                return;
            }

            // 加载音频文件
            AudioClip sfxClip = LoadAudioClip(soundName, _sfxResourcePath);
            if (sfxClip == null)
            {
                DebugLog("❌ 音效加载失败: " + soundName);
                return;
            }

            // 播放音效
            float finalVolume = _sfxVolume * _masterVolume * Mathf.Clamp01(volume);
            _sfxAudioSource.PlayOneShot(sfxClip, finalVolume);

            DebugLog("🔊 播放音效: " + soundName + " (音量: " + finalVolume.ToString("F2") + ")");
        }

        /// <summary>
        /// 停止所有音效
        /// </summary>
        public void StopAllSoundEffects()
        {
            if (!_isInitialized || _sfxAudioSource == null)
                return;

            _sfxAudioSource.Stop();
            DebugLog("🔇 停止所有音效");
        }

        #endregion

        #region 🎛️ 音量控制

        /// <summary>
        /// 设置主音量
        /// </summary>
        /// <param name="volume">音量 (0-1)</param>
        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
            DebugLog("🎛️ 设置主音量: " + _masterVolume.ToString("F2"));
        }

        /// <summary>
        /// 设置背景音乐音量
        /// </summary>
        /// <param name="volume">音量 (0-1)</param>
        public void SetBGMVolume(float volume)
        {
            _bgmVolume = Mathf.Clamp01(volume);
            UpdateBGMVolume();
            DebugLog("🎵 设置背景音乐音量: " + _bgmVolume.ToString("F2"));
        }

        /// <summary>
        /// 设置音效音量
        /// </summary>
        /// <param name="volume">音量 (0-1)</param>
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            UpdateSFXVolume();
            DebugLog("🔊 设置音效音量: " + _sfxVolume.ToString("F2"));
        }

        /// <summary>
        /// 启用/禁用音频
        /// </summary>
        /// <param name="enabled">是否启用</param>
        public void SetAudioEnabled(bool enabled)
        {
            _enableAudio = enabled;
            
            if (!enabled)
            {
                StopBGM();
                StopAllSoundEffects();
            }
            
            DebugLog("🔊 音频" + (enabled ? "启用" : "禁用"));
        }

        /// <summary>
        /// 更新所有音量
        /// </summary>
        private void UpdateAllVolumes()
        {
            UpdateBGMVolume();
            UpdateSFXVolume();
        }

        /// <summary>
        /// 更新背景音乐音量
        /// </summary>
        private void UpdateBGMVolume()
        {
            if (_bgmAudioSource != null)
            {
                _bgmAudioSource.volume = _bgmVolume * _masterVolume;
            }
        }

        /// <summary>
        /// 更新音效音量
        /// </summary>
        private void UpdateSFXVolume()
        {
            if (_sfxAudioSource != null)
            {
                _sfxAudioSource.volume = _sfxVolume * _masterVolume;
            }
        }

        #endregion

        #region 📁 资源加载

        /// <summary>
        /// 加载音频剪辑（按需加载，不预加载）
        /// </summary>
        /// <param name="clipName">文件名（不含扩展名）</param>
        /// <param name="resourcePath">资源路径</param>
        /// <returns>音频剪辑</returns>
        private AudioClip LoadAudioClip(string clipName, string resourcePath)
        {
            if (string.IsNullOrEmpty(clipName))
                return null;

            // 先检查缓存
            string cacheKey = resourcePath + clipName;
            if (_audioCache.TryGetValue(cacheKey, out AudioClip cachedClip))
            {
                return cachedClip;
            }

            // 从Resources加载
            try
            {
                string fullPath = resourcePath + clipName;
                AudioClip clip = Resources.Load<AudioClip>(fullPath);
                
                if (clip != null)
                {
                    // 添加到临时缓存
                    _audioCache[cacheKey] = clip;
                    DebugLog("📁 音频加载成功: " + fullPath);
                }
                else
                {
                    DebugLog("❌ 音频文件未找到: " + fullPath);
                }

                return clip;
            }
            catch (Exception e)
            {
                DebugLog("❌ 音频加载异常: " + clipName + " - " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// 清理音频缓存
        /// </summary>
        public void ClearAudioCache()
        {
            _audioCache.Clear();
            DebugLog("🧹 音频缓存已清理");
        }

        #endregion

        #region 📊 状态查询

        /// <summary>
        /// 获取当前背景音乐名称
        /// </summary>
        public string GetCurrentBGM()
        {
            return _currentBGM;
        }

        /// <summary>
        /// 检查背景音乐是否正在播放
        /// </summary>
        public bool IsPlayingBGM()
        {
            return _bgmAudioSource != null && _bgmAudioSource.isPlaying;
        }

        /// <summary>
        /// 检查背景音乐是否被暂停
        /// </summary>
        public bool IsBGMPaused()
        {
            return _isBGMPaused;
        }

        /// <summary>
        /// 检查音频是否启用
        /// </summary>
        public bool IsAudioEnabled()
        {
            return _enableAudio;
        }

        /// <summary>
        /// 获取音频管理器状态信息
        /// </summary>
        public Dictionary<string, object> GetAudioManagerStatus()
        {
            return new Dictionary<string, object>
            {
                { "isInitialized", _isInitialized },
                { "isAudioEnabled", _enableAudio },
                { "masterVolume", _masterVolume },
                { "bgmVolume", _bgmVolume },
                { "sfxVolume", _sfxVolume },
                { "currentBGM", _currentBGM },
                { "isPlayingBGM", IsPlayingBGM() },
                { "isBGMPaused", _isBGMPaused },
                { "audioCacheCount", _audioCache.Count }
            };
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
                Debug.Log("[AudioManager] " + message);
            }
        }

        /// <summary>
        /// 输出音频管理器详细状态
        /// </summary>
        public void DebugStatus()
        {
            DebugLog("=== AudioManager 状态信息 ===");
            var status = GetAudioManagerStatus();
            foreach (var kvp in status)
            {
                DebugLog(kvp.Key + ": " + kvp.Value);
            }
        }

        /// <summary>
        /// 测试音频功能
        /// </summary>
        public void TestAudio()
        {
            DebugLog("🧪 开始音频功能测试");
            
            // 测试音效播放
            PlaySoundEffect("test_sfx", 0.5f);
            
            // 测试背景音乐
            if (!string.IsNullOrEmpty(_currentBGM))
            {
                DebugLog("🧪 当前有背景音乐播放: " + _currentBGM);
            }
            else
            {
                DebugLog("🧪 当前无背景音乐播放");
            }
        }

        #endregion

        #region 公开属性

        /// <summary>
        /// 主音量
        /// </summary>
        public float MasterVolume => _masterVolume;

        /// <summary>
        /// 背景音乐音量
        /// </summary>
        public float BGMVolume => _bgmVolume;

        /// <summary>
        /// 音效音量
        /// </summary>
        public float SFXVolume => _sfxVolume;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => _isInitialized;

        #endregion
    }
}
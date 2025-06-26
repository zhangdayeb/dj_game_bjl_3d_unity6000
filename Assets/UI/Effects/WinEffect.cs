// Assets/UI/Effects/WinEffect.cs
// 中奖特效组件 - 完全重写版本
// 职责：处理百家乐游戏中的各种中奖特效动画和视觉反馈
// 特点：支持多级中奖特效、队列处理、粒子系统、音效集成

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BaccaratGame.Data;
using BaccaratGame.Core.Events;

namespace BaccaratGame.UI.Effects
{
    /// <summary>
    /// 中奖特效组件
    /// 负责处理百家乐游戏中所有中奖相关的视觉特效和音效
    /// </summary>
    public class WinEffect : MonoBehaviour
    {
        #region 序列化字段配置

        [Header("🎯 中奖阈值设置")]
        [SerializeField] private decimal smallWinThreshold = 10m;
        [SerializeField] private decimal mediumWinThreshold = 100m;
        [SerializeField] private decimal bigWinThreshold = 1000m;
        [SerializeField] private decimal jackpotThreshold = 10000m;

        [Header("🎨 视觉特效")]
        [SerializeField] private Image flashOverlay;
        [SerializeField] private Text winAmountText;
        [SerializeField] private Text winMessageText;
        [SerializeField] private GameObject winContainer;
        [SerializeField] private Transform effectCenter;

        [Header("✨ 粒子系统")]
        [SerializeField] private ParticleSystem celebrationParticles;
        [SerializeField] private ParticleSystem coinParticles;
        [SerializeField] private ParticleSystem fireworkParticles;
        [SerializeField] private ParticleSystem confettiParticles;
        [SerializeField] private ParticleSystem sparkleParticles;
        [SerializeField] private ParticleSystem glowParticles;

        [Header("🔊 音效设置")]
        [SerializeField] private bool enableSounds = true;
        [SerializeField] private AudioClip smallWinSound;
        [SerializeField] private AudioClip mediumWinSound;
        [SerializeField] private AudioClip bigWinSound;
        [SerializeField] private AudioClip jackpotWinSound;
        [SerializeField] private AudioClip coinDropSound;
        [SerializeField] private AudioClip celebrationSound;
        [SerializeField, Range(0f, 1f)] private float soundVolume = 1.0f;

        [Header("📱 摄像机震动")]
        [SerializeField] private bool enableCameraShake = true;
        [SerializeField] private float smallShakeIntensity = 0.1f;
        [SerializeField] private float mediumShakeIntensity = 0.3f;
        [SerializeField] private float bigShakeIntensity = 0.5f;
        [SerializeField] private float jackpotShakeIntensity = 1.0f;
        [SerializeField] private float shakeDuration = 0.5f;

        [Header("⚡ 动画设置")]
        [SerializeField] private float textAnimationDuration = 2.0f;
        [SerializeField] private float flashDuration = 0.1f;
        [SerializeField] private int flashCount = 3;
        [SerializeField] private AnimationCurve scaleUpCurve;
        [SerializeField] private AnimationCurve fadeInCurve;

        [Header("🎨 颜色配置")]
        [SerializeField] private Color smallWinColor = Color.green;
        [SerializeField] private Color mediumWinColor = Color.blue;
        [SerializeField] private Color bigWinColor = Color.red;
        [SerializeField] private Color jackpotWinColor = Color.yellow;
        [SerializeField] private Color flashColor = Color.white;

        [Header("⏱️ 时间设置")]
        [SerializeField] private float smallWinDuration = 1.5f;
        [SerializeField] private float mediumWinDuration = 2.5f;
        [SerializeField] private float bigWinDuration = 4.0f;
        [SerializeField] private float jackpotWinDuration = 6.0f;

        [Header("🐛 调试设置")]
        [SerializeField] private bool enableDebugMode = true;
        [SerializeField] private bool showEffectBounds = false;

        #endregion

        #region 私有字段

        // 组件引用
        private AudioSource audioSource;
        private Camera mainCamera;
        private Canvas parentCanvas;

        // 特效管理
        private Queue<WinEffectRequest> effectQueue = new Queue<WinEffectRequest>();
        private List<GameObject> activeEffects = new List<GameObject>();
        private bool isPlayingEffect = false;

        // 摄像机震动
        private Vector3 originalCameraPosition;
        private bool isShaking = false;

        // 动画缓存
        private Coroutine currentWinAnimation;
        private Dictionary<WinLevel, WinEffectConfig> winConfigs = new Dictionary<WinLevel, WinEffectConfig>();

        #endregion

        #region 事件定义

        // 中奖特效事件
        public System.Action<decimal, WinLevel> OnWinEffectStarted;
        public System.Action<decimal, WinLevel> OnWinEffectCompleted;
        public System.Action OnAllEffectsCleared;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            InitializeComponent();
            InitializeWinConfigs();
            InitializeAnimationCurves();
        }

        private void Start()
        {
            SetupEventListeners();
            ValidateComponents();
        }

        private void OnDestroy()
        {
            CleanupEventListeners();
            StopAllEffects();
        }

        private void OnDrawGizmosSelected()
        {
            if (showEffectBounds && effectCenter != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(effectCenter.position, 100f);
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponent()
        {
            // 获取音频源
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.volume = soundVolume;
            audioSource.playOnAwake = false;

            // 获取主摄像机
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                originalCameraPosition = mainCamera.transform.position;
            }

            // 获取Canvas
            parentCanvas = GetComponentInParent<Canvas>();

            // 确保特效中心存在
            if (effectCenter == null)
            {
                GameObject centerObj = new GameObject("EffectCenter");
                centerObj.transform.SetParent(transform);
                effectCenter = centerObj.transform;
            }

            if (enableDebugMode)
                Debug.Log("[WinEffect] 组件初始化完成");
        }

        /// <summary>
        /// 初始化中奖配置
        /// </summary>
        private void InitializeWinConfigs()
        {
            winConfigs[WinLevel.Small] = new WinEffectConfig
            {
                duration = smallWinDuration,
                color = smallWinColor,
                shakeIntensity = smallShakeIntensity,
                message = "小奖中奖!",
                sound = smallWinSound,
                useParticles = true,
                useFlash = true
            };

            winConfigs[WinLevel.Medium] = new WinEffectConfig
            {
                duration = mediumWinDuration,
                color = mediumWinColor,
                shakeIntensity = mediumShakeIntensity,
                message = "中奖来了!",
                sound = mediumWinSound,
                useParticles = true,
                useFlash = true
            };

            winConfigs[WinLevel.Big] = new WinEffectConfig
            {
                duration = bigWinDuration,
                color = bigWinColor,
                shakeIntensity = bigShakeIntensity,
                message = "巨额奖金!",
                sound = bigWinSound,
                useParticles = true,
                useFlash = true,
                useFireworks = true
            };

            winConfigs[WinLevel.Jackpot] = new WinEffectConfig
            {
                duration = jackpotWinDuration,
                color = jackpotWinColor,
                shakeIntensity = jackpotShakeIntensity,
                message = "超级大奖!!!",
                sound = jackpotWinSound,
                useParticles = true,
                useFlash = true,
                useFireworks = true,
                useConfetti = true
            };

            if (enableDebugMode)
                Debug.Log("[WinEffect] 中奖配置初始化完成");
        }

        /// <summary>
        /// 初始化动画曲线
        /// </summary>
        private void InitializeAnimationCurves()
        {
            if (scaleUpCurve == null || scaleUpCurve.keys.Length == 0)
            {
                scaleUpCurve = new AnimationCurve();
                scaleUpCurve.AddKey(0f, 0f);
                scaleUpCurve.AddKey(0.2f, 1.2f);
                scaleUpCurve.AddKey(1f, 1f);
            }

            if (fadeInCurve == null || fadeInCurve.keys.Length == 0)
            {
                fadeInCurve = new AnimationCurve();
                fadeInCurve.AddKey(0f, 0f);
                fadeInCurve.AddKey(0.3f, 1f);
                fadeInCurve.AddKey(0.7f, 1f);
                fadeInCurve.AddKey(1f, 0f);
            }

            if (enableDebugMode)
                Debug.Log("[WinEffect] 动画曲线初始化完成");
        }

        #endregion

        #region 事件系统

        /// <summary>
        /// 设置事件监听
        /// </summary>
        private void SetupEventListeners()
        {
            // 这里可以监听游戏事件
            // 示例：
            // GameEventManager.OnPlayerWin += HandlePlayerWin;
            // GameEventManager.OnBankerWin += HandleBankerWin;
            // GameEventManager.OnTieWin += HandleTieWin;
        }

        /// <summary>
        /// 清理事件监听
        /// </summary>
        private void CleanupEventListeners()
        {
            // 取消事件监听
            // GameEventManager.OnPlayerWin -= HandlePlayerWin;
            // GameEventManager.OnBankerWin -= HandleBankerWin;
            // GameEventManager.OnTieWin -= HandleTieWin;
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 播放中奖特效
        /// </summary>
        /// <param name="winAmount">中奖金额</param>
        /// <param name="winType">中奖类型</param>
        /// <param name="position">特效位置</param>
        public void PlayWinEffect(decimal winAmount, string winType = "", Vector3? position = null)
        {
            WinLevel winLevel = GetWinLevel(winAmount);
            Vector3 effectPosition = position ?? (effectCenter != null ? effectCenter.position : transform.position);

            var effectInfo = new WinEffectInfo
            {
                winAmount = winAmount,
                winType = !string.IsNullOrEmpty(winType) ? winType : GetWinTypeFromLevel(winLevel),
                position = effectPosition,
                effectColor = GetWinColor(winLevel),
                duration = GetWinDuration(winLevel)
            };

            var request = new WinEffectRequest
            {
                effectInfo = effectInfo,
                winLevel = winLevel,
                priority = GetEffectPriority(winLevel)
            };

            effectQueue.Enqueue(request);

            if (!isPlayingEffect)
            {
                StartCoroutine(ProcessEffectQueue());
            }

            if (enableDebugMode)
                Debug.Log($"[WinEffect] 添加中奖特效请求: {winAmount} ({winLevel})");
        }

        /// <summary>
        /// 播放百家乐特定中奖特效
        /// </summary>
        /// <param name="result">游戏结果</param>
        /// <param name="winAmount">中奖金额</param>
        public void PlayBaccaratWinEffect(BaccaratResult result, decimal winAmount)
        {
            string winType = result switch
            {
                BaccaratResult.Player => "闲家获胜",
                BaccaratResult.Banker => "庄家获胜", 
                BaccaratResult.Tie => "和局",
                _ => "中奖"
            };

            Color resultColor = result switch
            {
                BaccaratResult.Player => Color.blue,
                BaccaratResult.Banker => Color.red,
                BaccaratResult.Tie => Color.green,
                _ => Color.white
            };

            PlayWinEffect(winAmount, winType);
        }

        /// <summary>
        /// 停止所有特效
        /// </summary>
        public void StopAllEffects()
        {
            StopAllCoroutines();
            ClearAllActiveEffects();
            effectQueue.Clear();
            isPlayingEffect = false;

            // 恢复摄像机位置
            if (mainCamera != null && isShaking)
            {
                mainCamera.transform.position = originalCameraPosition;
                isShaking = false;
            }

            if (enableDebugMode)
                Debug.Log("[WinEffect] 所有特效已停止");
        }

        #endregion

        #region 特效处理

        /// <summary>
        /// 处理特效队列
        /// </summary>
        private IEnumerator ProcessEffectQueue()
        {
            isPlayingEffect = true;

            while (effectQueue.Count > 0)
            {
                WinEffectRequest request = effectQueue.Dequeue();
                yield return StartCoroutine(PlayWinEffectCoroutine(request));
            }

            isPlayingEffect = false;
        }

        /// <summary>
        /// 播放中奖特效协程
        /// </summary>
        private IEnumerator PlayWinEffectCoroutine(WinEffectRequest request)
        {
            WinEffectInfo effectInfo = request.effectInfo;
            WinLevel winLevel = request.winLevel;
            WinEffectConfig config = winConfigs[winLevel];

            // 触发开始事件
            OnWinEffectStarted?.Invoke(effectInfo.winAmount, winLevel);

            // 设置特效位置
            transform.position = effectInfo.position;

            // 并行播放各种特效
            List<Coroutine> effects = new List<Coroutine>();

            // 播放音效
            PlayWinSound(config);

            // 播放摄像机震动
            if (enableCameraShake && config.shakeIntensity > 0)
            {
                effects.Add(StartCoroutine(CameraShakeCoroutine(config.shakeIntensity, shakeDuration)));
            }

            // 播放闪光效果
            if (config.useFlash)
            {
                effects.Add(StartCoroutine(FlashEffectCoroutine(config.color)));
            }

            // 播放粒子特效
            if (config.useParticles)
            {
                PlayParticleEffects(winLevel, effectInfo);
            }

            // 显示中奖文本
            effects.Add(StartCoroutine(ShowWinTextCoroutine(effectInfo, config)));

            // 等待主要特效完成
            yield return new WaitForSeconds(config.duration);

            // 等待所有特效完成
            foreach (var effect in effects)
            {
                if (effect != null)
                {
                    yield return effect;
                }
            }

            // 清理特效
            CleanupEffect(winLevel);

            // 触发完成事件
            OnWinEffectCompleted?.Invoke(effectInfo.winAmount, winLevel);

            if (enableDebugMode)
                Debug.Log($"[WinEffect] 中奖特效播放完成: {effectInfo.winAmount} ({winLevel})");
        }

        /// <summary>
        /// 显示中奖文本协程
        /// </summary>
        private IEnumerator ShowWinTextCoroutine(WinEffectInfo effectInfo, WinEffectConfig config)
        {
            if (winContainer != null)
            {
                winContainer.SetActive(true);
            }

            // 设置中奖金额文本
            if (winAmountText != null)
            {
                winAmountText.text = FormatWinAmount(effectInfo.winAmount);
                winAmountText.color = config.color;
            }

            // 设置中奖消息文本
            if (winMessageText != null)
            {
                winMessageText.text = config.message;
                winMessageText.color = config.color;
            }

            // 播放文本动画
            yield return StartCoroutine(AnimateWinText());

            // 隐藏文本
            if (winContainer != null)
            {
                winContainer.SetActive(false);
            }
        }

        /// <summary>
        /// 文本动画协程
        /// </summary>
        private IEnumerator AnimateWinText()
        {
            if (winContainer == null) yield break;

            Transform textTransform = winContainer.transform;
            Vector3 originalScale = textTransform.localScale;
            Color originalColor = Color.white;

            CanvasGroup canvasGroup = winContainer.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = winContainer.AddComponent<CanvasGroup>();
            }

            float elapsedTime = 0f;
            while (elapsedTime < textAnimationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / textAnimationDuration;

                // 缩放动画
                float scaleValue = scaleUpCurve.Evaluate(t);
                textTransform.localScale = originalScale * scaleValue;

                // 透明度动画
                float alphaValue = fadeInCurve.Evaluate(t);
                canvasGroup.alpha = alphaValue;

                yield return null;
            }

            textTransform.localScale = originalScale;
        }

        /// <summary>
        /// 闪光特效协程
        /// </summary>
        private IEnumerator FlashEffectCoroutine(Color flashColor)
        {
            if (flashOverlay == null) yield break;

            for (int i = 0; i < flashCount; i++)
            {
                // 闪光开启
                flashOverlay.gameObject.SetActive(true);
                flashOverlay.color = flashColor;

                yield return new WaitForSeconds(flashDuration);

                // 闪光关闭
                flashOverlay.gameObject.SetActive(false);

                yield return new WaitForSeconds(flashDuration);
            }
        }

        /// <summary>
        /// 摄像机震动协程
        /// </summary>
        private IEnumerator CameraShakeCoroutine(float intensity, float duration)
        {
            if (mainCamera == null) yield break;

            isShaking = true;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                
                Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * intensity;
                randomOffset.z = 0; // 保持Z轴不变
                
                mainCamera.transform.position = originalCameraPosition + randomOffset;
                
                yield return null;
            }

            // 恢复原始位置
            mainCamera.transform.position = originalCameraPosition;
            isShaking = false;
        }

        #endregion

        #region 粒子特效

        /// <summary>
        /// 播放粒子特效
        /// </summary>
        private void PlayParticleEffects(WinLevel winLevel, WinEffectInfo effectInfo)
        {
            WinEffectConfig config = winConfigs[winLevel];

            // 基础庆祝粒子
            if (celebrationParticles != null)
            {
                ConfigureParticleSystem(celebrationParticles, winLevel, effectInfo.effectColor);
                celebrationParticles.Play();
            }

            // 金币粒子
            if (coinParticles != null)
            {
                ConfigureParticleSystem(coinParticles, winLevel, Color.yellow);
                coinParticles.Play();
            }

            // 发光粒子
            if (glowParticles != null)
            {
                ConfigureParticleSystem(glowParticles, winLevel, effectInfo.effectColor);
                glowParticles.Play();
            }

            // 根据配置播放额外特效
            if (config.useFireworks && fireworkParticles != null)
            {
                ConfigureParticleSystem(fireworkParticles, winLevel, effectInfo.effectColor);
                fireworkParticles.Play();
            }

            if (config.useConfetti && confettiParticles != null)
            {
                ConfigureParticleSystem(confettiParticles, winLevel, Color.white);
                confettiParticles.Play();
            }

            // 闪光粒子
            if (winLevel >= WinLevel.Medium && sparkleParticles != null)
            {
                ConfigureParticleSystem(sparkleParticles, winLevel, Color.white);
                sparkleParticles.Play();
            }
        }

        /// <summary>
        /// 配置粒子系统
        /// </summary>
        private void ConfigureParticleSystem(ParticleSystem particles, WinLevel winLevel, Color color)
        {
            var main = particles.main;
            main.startColor = color;

            // 根据中奖等级调整粒子参数
            switch (winLevel)
            {
                case WinLevel.Small:
                    main.maxParticles = 50;
                    main.startSpeed = 5f;
                    main.startLifetime = 2f;
                    break;
                case WinLevel.Medium:
                    main.maxParticles = 100;
                    main.startSpeed = 8f;
                    main.startLifetime = 3f;
                    break;
                case WinLevel.Big:
                    main.maxParticles = 200;
                    main.startSpeed = 12f;
                    main.startLifetime = 4f;
                    break;
                case WinLevel.Jackpot:
                    main.maxParticles = 500;
                    main.startSpeed = 20f;
                    main.startLifetime = 6f;
                    break;
            }

            // 配置发射器
            var emission = particles.emission;
            emission.rateOverTime = main.maxParticles / 2f;

            // 配置形状
            var shape = particles.shape;
            shape.radius = GetEffectRadius(winLevel);
        }

        /// <summary>
        /// 停止所有粒子系统
        /// </summary>
        private void StopAllParticleSystems()
        {
            ParticleSystem[] allParticles = {
                celebrationParticles, coinParticles, fireworkParticles,
                confettiParticles, sparkleParticles, glowParticles
            };

            foreach (var particle in allParticles)
            {
                if (particle != null)
                {
                    particle.Stop();
                    particle.Clear();
                }
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取中奖等级
        /// </summary>
        private WinLevel GetWinLevel(decimal winAmount)
        {
            if (winAmount >= jackpotThreshold)
                return WinLevel.Jackpot;
            else if (winAmount >= bigWinThreshold)
                return WinLevel.Big;
            else if (winAmount >= mediumWinThreshold)
                return WinLevel.Medium;
            else
                return WinLevel.Small;
        }

        /// <summary>
        /// 获取中奖颜色
        /// </summary>
        private Color GetWinColor(WinLevel winLevel)
        {
            return winLevel switch
            {
                WinLevel.Small => smallWinColor,
                WinLevel.Medium => mediumWinColor,
                WinLevel.Big => bigWinColor,
                WinLevel.Jackpot => jackpotWinColor,
                _ => Color.white
            };
        }

        /// <summary>
        /// 获取中奖持续时间
        /// </summary>
        private float GetWinDuration(WinLevel winLevel)
        {
            return winLevel switch
            {
                WinLevel.Small => smallWinDuration,
                WinLevel.Medium => mediumWinDuration,
                WinLevel.Big => bigWinDuration,
                WinLevel.Jackpot => jackpotWinDuration,
                _ => 1.0f
            };
        }

        /// <summary>
        /// 从等级获取中奖类型
        /// </summary>
        private string GetWinTypeFromLevel(WinLevel winLevel)
        {
            return winLevel switch
            {
                WinLevel.Small => "小奖",
                WinLevel.Medium => "中奖",
                WinLevel.Big => "大奖",
                WinLevel.Jackpot => "超级大奖",
                _ => "中奖"
            };
        }

        /// <summary>
        /// 获取特效优先级
        /// </summary>
        private int GetEffectPriority(WinLevel winLevel)
        {
            return (int)winLevel;
        }

        /// <summary>
        /// 获取特效半径
        /// </summary>
        private float GetEffectRadius(WinLevel winLevel)
        {
            return winLevel switch
            {
                WinLevel.Small => 50f,
                WinLevel.Medium => 80f,
                WinLevel.Big => 120f,
                WinLevel.Jackpot => 200f,
                _ => 50f
            };
        }

        /// <summary>
        /// 格式化中奖金额
        /// </summary>
        private string FormatWinAmount(decimal amount)
        {
            if (amount >= 10000)
                return $"¥{amount / 10000:F1}万";
            else if (amount >= 1000)
                return $"¥{amount / 1000:F1}K";
            else
                return $"¥{amount:F0}";
        }

        /// <summary>
        /// 播放中奖音效
        /// </summary>
        private void PlayWinSound(WinEffectConfig config)
        {
            if (!enableSounds || audioSource == null || config.sound == null) return;

            audioSource.PlayOneShot(config.sound, soundVolume);

            // 大奖额外播放庆祝音效
            if (config.useFireworks && celebrationSound != null)
            {
                StartCoroutine(PlayDelayedSound(celebrationSound, 0.5f));
            }

            // 金币掉落音效
            if (coinDropSound != null)
            {
                StartCoroutine(PlayDelayedSound(coinDropSound, 1.0f));
            }
        }

        /// <summary>
        /// 延迟播放音效
        /// </summary>
        private IEnumerator PlayDelayedSound(AudioClip clip, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip, soundVolume * 0.8f);
            }
        }

        /// <summary>
        /// 清理特效
        /// </summary>
        private void CleanupEffect(WinLevel winLevel)
        {
            // 根据等级决定清理策略
            if (winLevel >= WinLevel.Big)
            {
                // 大奖保留一段时间
                StartCoroutine(DelayedCleanup(2.0f));
            }
            else
            {
                // 小奖立即清理
                StopAllParticleSystems();
            }
        }

        /// <summary>
        /// 延迟清理
        /// </summary>
        private IEnumerator DelayedCleanup(float delay)
        {
            yield return new WaitForSeconds(delay);
            StopAllParticleSystems();
        }

        /// <summary>
        /// 清理所有活动特效
        /// </summary>
        private void ClearAllActiveEffects()
        {
            foreach (GameObject effect in activeEffects)
            {
                if (effect != null)
                {
                    if (Application.isPlaying)
                        Destroy(effect);
                    else
                        DestroyImmediate(effect);
                }
            }

            activeEffects.Clear();
            StopAllParticleSystems();

            if (flashOverlay != null)
            {
                flashOverlay.gameObject.SetActive(false);
            }

            if (winContainer != null)
            {
                winContainer.SetActive(false);
            }

            OnAllEffectsCleared?.Invoke();
        }

        /// <summary>
        /// 验证组件
        /// </summary>
        private void ValidateComponents()
        {
            if (winAmountText == null)
                Debug.LogWarning("[WinEffect] 中奖金额文本组件未设置");

            if (winMessageText == null)
                Debug.LogWarning("[WinEffect] 中奖消息文本组件未设置");

            if (celebrationParticles == null)
                Debug.LogWarning("[WinEffect] 庆祝粒子系统未设置");
        }

        #endregion

        #region 调试方法

        /// <summary>
        /// 显示调试信息
        /// </summary>
        [ContextMenu("显示调试信息")]
        public void ShowDebugInfo()
        {
            Debug.Log("=== WinEffect 调试信息 ===");
            Debug.Log($"队列中的特效请求: {effectQueue.Count}");
            Debug.Log($"正在播放特效: {isPlayingEffect}");
            Debug.Log($"活动特效对象: {activeEffects.Count}");
            Debug.Log($"中奖阈值配置: 小奖{smallWinThreshold}, 中奖{mediumWinThreshold}, 大奖{bigWinThreshold}, 超级大奖{jackpotThreshold}");
            Debug.Log($"摄像机震动状态: {isShaking}");
        }

        /// <summary>
        /// 测试小奖特效
        /// </summary>
        [ContextMenu("测试小奖特效")]
        public void TestSmallWin()
        {
            if (Application.isPlaying)
                PlayWinEffect(50m, "测试小奖");
        }

        /// <summary>
        /// 测试中奖特效
        /// </summary>
        [ContextMenu("测试中奖特效")]
        public void TestMediumWin()
        {
            if (Application.isPlaying)
                PlayWinEffect(300m, "测试中奖");
        }

        /// <summary>
        /// 测试大奖特效
        /// </summary>
        [ContextMenu("测试大奖特效")]
        public void TestBigWin()
        {
            if (Application.isPlaying)
                PlayWinEffect(2000m, "测试大奖");
        }

        /// <summary>
        /// 测试超级大奖特效
        /// </summary>
        [ContextMenu("测试超级大奖特效")]
        public void TestJackpotWin()
        {
            if (Application.isPlaying)
                PlayWinEffect(50000m, "测试超级大奖");
        }

        #endregion
    }

    #region 数据结构

    /// <summary>
    /// 中奖等级
    /// </summary>
    public enum WinLevel
    {
        Small = 1,      // 小奖
        Medium = 2,     // 中奖  
        Big = 3,        // 大奖
        Jackpot = 4     // 超级大奖
    }

    /// <summary>
    /// 中奖特效信息
    /// </summary>
    [System.Serializable]
    public struct WinEffectInfo
    {
        public decimal winAmount;
        public string winType;
        public Vector3 position;
        public Color effectColor;
        public float duration;
    }

    /// <summary>
    /// 中奖特效请求
    /// </summary>
    [System.Serializable]
    public struct WinEffectRequest
    {
        public WinEffectInfo effectInfo;
        public WinLevel winLevel;
        public int priority;
    }

    /// <summary>
    /// 中奖特效配置
    /// </summary>
    [System.Serializable]
    public class WinEffectConfig
    {
        public float duration = 2.0f;
        public Color color = Color.white;
        public float shakeIntensity = 0.1f;
        public string message = "中奖!";
        public AudioClip sound;
        public bool useParticles = true;
        public bool useFlash = true;
        public bool useFireworks = false;
        public bool useConfetti = false;
    }

    #endregion
}
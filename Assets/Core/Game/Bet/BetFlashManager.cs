// Assets/Scripts/Core/BetFlashManager.cs
// 投注区域闪烁管理器 - 专门处理投注区域的闪烁效果
// 负责处理开牌后的区域闪烁提示
// 创建时间: 2025/6/30

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using BaccaratGame.Managers;

namespace BaccaratGame.Core
{
    /// <summary>
    /// 投注区域闪烁配置
    /// </summary>
    [System.Serializable]
    public class BetAreaFlashConfig
    {
        [Header("区域配置")]
        public BetAreaType areaType;                    // 区域类型
        public GameObject areaObject;                   // 投注区域对象（用于闪烁）

        /// <summary>
        /// 验证配置是否完整
        /// </summary>
        public bool IsValid()
        {
            return areaObject != null;
        }

        /// <summary>
        /// 获取调试信息
        /// </summary>
        public string GetDebugInfo()
        {
            return $"{areaType} - Area:{(areaObject != null ? "✓" : "✗")}";
        }
    }

    /// <summary>
    /// 投注区域闪烁管理器
    /// 专门负责处理开牌后的区域闪烁效果
    /// </summary>
    public class BetFlashManager : MonoBehaviour
    {
        #region Inspector 配置

        [Header("投注区域闪烁配置")]
        [SerializeField] private BetAreaFlashConfig[] flashConfigs = new BetAreaFlashConfig[8];

        [Header("闪烁效果配置")]
        [SerializeField] private float flashDuration = 2f;              // 闪烁持续时间
        [SerializeField] private float flashInterval = 0.3f;            // 闪烁间隔
        [SerializeField] private Color flashColor = Color.yellow;       // 闪烁颜色
        [SerializeField] private AnimationCurve flashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 闪烁曲线

        #endregion

        #region 私有字段

        private Dictionary<BetAreaType, BetAreaFlashConfig> flashConfigMap; // 闪烁配置映射
        private Dictionary<BetAreaType, Coroutine> flashCoroutines;         // 闪烁协程映射
        private Dictionary<BetAreaType, Color> originalColors;              // 原始颜色映射
        private bool isInitialized = false;

        #endregion

        #region 生命周期

        private void Awake()
        {
            InitializeFlashManager();
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化闪烁管理器
        /// </summary>
        private void InitializeFlashManager()
        {
            // 初始化映射字典
            flashConfigMap = new Dictionary<BetAreaType, BetAreaFlashConfig>();
            flashCoroutines = new Dictionary<BetAreaType, Coroutine>();
            originalColors = new Dictionary<BetAreaType, Color>();

            // 构建闪烁配置映射
            BuildFlashConfigMap();

            // 验证配置
            ValidateConfiguration();

            isInitialized = true;
            Debug.Log("[BetFlashManager] 闪烁管理器初始化完成");
        }

        /// <summary>
        /// 构建闪烁配置映射
        /// </summary>
        private void BuildFlashConfigMap()
        {
            foreach (var config in flashConfigs)
            {
                if (config != null && config.IsValid())
                {
                    flashConfigMap[config.areaType] = config;
                    
                    // 保存原始颜色
                    SaveOriginalColors(config);
                    
                    Debug.Log($"[BetFlashManager] 闪烁配置已添加: {config.GetDebugInfo()}");
                }
                else
                {
                    Debug.LogWarning($"[BetFlashManager] 闪烁配置无效: {config?.areaType}");
                }
            }
        }

        /// <summary>
        /// 保存原始颜色
        /// </summary>
        /// <param name="config">闪烁配置</param>
        private void SaveOriginalColors(BetAreaFlashConfig config)
        {
            // 保存区域对象的原始颜色
            var image = config.areaObject.GetComponent<Image>();
            if (image != null)
            {
                originalColors[config.areaType] = image.color;
            }
        }

        /// <summary>
        /// 验证配置
        /// </summary>
        private void ValidateConfiguration()
        {
            int validCount = 0;
            foreach (BetAreaType areaType in System.Enum.GetValues(typeof(BetAreaType)))
            {
                if (flashConfigMap.ContainsKey(areaType))
                {
                    validCount++;
                }
                else
                {
                    Debug.LogWarning($"[BetFlashManager] 缺失闪烁配置: {areaType}");
                }
            }

            Debug.Log($"[BetFlashManager] 配置验证完成: {validCount}/8 个区域已配置");
        }

        #endregion

        #region 闪烁控制

        /// <summary>
        /// 开始闪烁指定区域（通过区域ID）
        /// </summary>
        /// <param name="areaId">区域ID</param>
        public void StartFlashArea(int areaId)
        {
            BetAreaType areaType = (BetAreaType)areaId;
            StartFlashArea(areaType);
        }

        /// <summary>
        /// 开始闪烁指定区域
        /// </summary>
        /// <param name="areaType">区域类型</param>
        public void StartFlashArea(BetAreaType areaType)
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[BetFlashManager] 闪烁管理器未初始化");
                return;
            }

            if (!flashConfigMap.ContainsKey(areaType))
            {
                Debug.LogWarning($"[BetFlashManager] 无法闪烁区域: {areaType}");
                return;
            }

            // 停止现有的闪烁
            StopFlashArea(areaType);

            // 开始新的闪烁
            var config = flashConfigMap[areaType];
            flashCoroutines[areaType] = StartCoroutine(FlashAreaCoroutine(config));
            Debug.Log($"[BetFlashManager] 开始闪烁区域: {areaType}");
        }

        /// <summary>
        /// 停止闪烁指定区域
        /// </summary>
        /// <param name="areaType">区域类型</param>
        public void StopFlashArea(BetAreaType areaType)
        {
            if (flashCoroutines.ContainsKey(areaType) && flashCoroutines[areaType] != null)
            {
                StopCoroutine(flashCoroutines[areaType]);
                flashCoroutines[areaType] = null;

                // 恢复原始状态
                RestoreAreaOriginalState(areaType);
                Debug.Log($"[BetFlashManager] 停止闪烁区域: {areaType}");
            }
        }

        /// <summary>
        /// 停止所有闪烁
        /// </summary>
        public void StopAllFlash()
        {
            var areaTypes = new List<BetAreaType>(flashCoroutines.Keys);
            foreach (var areaType in areaTypes)
            {
                StopFlashArea(areaType);
            }
            Debug.Log("[BetFlashManager] 停止所有闪烁");
        }

        /// <summary>
        /// 开始闪烁多个区域
        /// </summary>
        /// <param name="areaIds">区域ID数组</param>
        public void StartFlashAreas(int[] areaIds)
        {
            foreach (int areaId in areaIds)
            {
                StartFlashArea(areaId);
            }
        }

        /// <summary>
        /// 开始闪烁多个区域
        /// </summary>
        /// <param name="areaTypes">区域类型数组</param>
        public void StartFlashAreas(BetAreaType[] areaTypes)
        {
            foreach (var areaType in areaTypes)
            {
                StartFlashArea(areaType);
            }
        }

        #endregion

        #region 闪烁效果实现

        /// <summary>
        /// 闪烁区域协程
        /// </summary>
        /// <param name="config">闪烁配置</param>
        /// <returns>协程</returns>
        private IEnumerator FlashAreaCoroutine(BetAreaFlashConfig config)
        {
            float elapsed = 0f;
            while (elapsed < flashDuration)
            {
                // 计算闪烁强度
                float flashProgress = (elapsed % flashInterval) / flashInterval;
                float intensity = flashCurve.Evaluate(flashProgress);

                // 应用闪烁效果
                ApplyFlashEffect(config, intensity);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 恢复原始状态
            RestoreAreaOriginalState(config.areaType);
            flashCoroutines[config.areaType] = null;
        }

        /// <summary>
        /// 应用闪烁效果
        /// </summary>
        /// <param name="config">闪烁配置</param>
        /// <param name="intensity">闪烁强度</param>
        private void ApplyFlashEffect(BetAreaFlashConfig config, float intensity)
        {
            // 区域对象闪烁效果
            var image = config.areaObject.GetComponent<Image>();
            if (image != null && originalColors.ContainsKey(config.areaType))
            {
                Color originalColor = originalColors[config.areaType];
                Color flashedColor = Color.Lerp(originalColor, flashColor, intensity);
                image.color = flashedColor;
            }
        }

        /// <summary>
        /// 恢复区域原始状态
        /// </summary>
        /// <param name="areaType">区域类型</param>
        private void RestoreAreaOriginalState(BetAreaType areaType)
        {
            if (!flashConfigMap.ContainsKey(areaType)) return;

            var config = flashConfigMap[areaType];
            
            // 恢复区域对象颜色
            var image = config.areaObject.GetComponent<Image>();
            if (image != null && originalColors.ContainsKey(areaType))
            {
                image.color = originalColors[areaType];
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置闪烁配置（运行时动态配置）
        /// </summary>
        /// <param name="areaType">区域类型</param>
        /// <param name="config">闪烁配置</param>
        public void SetFlashConfig(BetAreaType areaType, BetAreaFlashConfig config)
        {
            if (config != null && config.IsValid())
            {
                flashConfigMap[areaType] = config;
                SaveOriginalColors(config);
                
                Debug.Log($"[BetFlashManager] 运行时设置闪烁配置: {config.GetDebugInfo()}");
            }
        }

        /// <summary>
        /// 获取闪烁配置
        /// </summary>
        /// <param name="areaType">区域类型</param>
        /// <returns>闪烁配置</returns>
        public BetAreaFlashConfig GetFlashConfig(BetAreaType areaType)
        {
            return flashConfigMap.ContainsKey(areaType) ? flashConfigMap[areaType] : null;
        }

        /// <summary>
        /// 设置闪烁参数
        /// </summary>
        /// <param name="duration">闪烁持续时间</param>
        /// <param name="interval">闪烁间隔</param>
        /// <param name="color">闪烁颜色</param>
        public void SetFlashParameters(float duration, float interval, Color color)
        {
            flashDuration = duration;
            flashInterval = interval;
            flashColor = color;
            
            Debug.Log($"[BetFlashManager] 闪烁参数已更新: 持续时间={duration}, 间隔={interval}");
        }

        /// <summary>
        /// 检查指定区域是否正在闪烁
        /// </summary>
        /// <param name="areaType">区域类型</param>
        /// <returns>是否正在闪烁</returns>
        public bool IsFlashing(BetAreaType areaType)
        {
            return flashCoroutines.ContainsKey(areaType) && flashCoroutines[areaType] != null;
        }

        /// <summary>
        /// 获取当前闪烁区域数量
        /// </summary>
        /// <returns>闪烁区域数量</returns>
        public int GetActiveFlashCount()
        {
            int count = 0;
            foreach (var coroutine in flashCoroutines.Values)
            {
                if (coroutine != null) count++;
            }
            return count;
        }

        #endregion

        #region 调试和编辑器辅助

        /// <summary>
        /// 验证所有闪烁配置
        /// </summary>
        [ContextMenu("验证闪烁配置")]
        public void ValidateAllFlashConfigurations()
        {
            Debug.Log("=== BetFlashManager 闪烁配置验证 ===");
            
            foreach (var config in flashConfigs)
            {
                if (config != null)
                {
                    Debug.Log(config.GetDebugInfo());
                }
                else
                {
                    Debug.LogWarning("发现null的闪烁配置");
                }
            }

            Debug.Log($"已初始化闪烁配置数量: {flashConfigMap?.Count ?? 0}/8");
        }

        /// <summary>
        /// 测试所有区域闪烁效果
        /// </summary>
        [ContextMenu("测试闪烁效果")]
        public void TestFlashEffects()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("请在运行时测试闪烁效果");
                return;
            }

            Debug.Log("=== 测试所有区域闪烁效果 ===");
            
            foreach (BetAreaType areaType in flashConfigMap.Keys)
            {
                StartFlashArea(areaType);
            }

            // 5秒后停止所有闪烁
            StartCoroutine(StopFlashAfterDelay(5f));
        }

        /// <summary>
        /// 延迟停止闪烁
        /// </summary>
        /// <param name="delay">延迟时间</param>
        /// <returns>协程</returns>
        private IEnumerator StopFlashAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            StopAllFlash();
            Debug.Log("停止所有闪烁效果");
        }

        /// <summary>
        /// 获取闪烁管理器状态信息
        /// </summary>
        /// <returns>状态信息</returns>
        public string GetStatusInfo()
        {
            return $"[BetFlashManager] 状态:\n" +
                   $"已初始化: {isInitialized}\n" +
                   $"配置区域数: {flashConfigMap?.Count ?? 0}/8\n" +
                   $"活跃闪烁数: {GetActiveFlashCount()}";
        }

        #endregion
    }
}
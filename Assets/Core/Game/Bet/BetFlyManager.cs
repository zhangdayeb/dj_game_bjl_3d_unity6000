// Assets/Scripts/Core/BetFlyManager.cs
// 筹码飞行管理器 - 专门处理筹码飞行动画效果
// 负责从选中筹码位置飞行到投注区域的视觉特效
// 创建时间: 2025/6/30

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using BaccaratGame.Managers;

namespace BaccaratGame.Core
{
    /// <summary>
    /// 筹码飞行配置
    /// </summary>
    [System.Serializable]
    public class ChipFlyConfig
    {
        [Header("区域配置")]
        public BetAreaType areaType;                    // 区域类型
        public Transform targetTransform;               // 目标位置Transform

        /// <summary>
        /// 验证配置是否完整
        /// </summary>
        public bool IsValid()
        {
            return targetTransform != null;
        }

        /// <summary>
        /// 获取调试信息
        /// </summary>
        public string GetDebugInfo()
        {
            return $"{areaType} - Target:{(targetTransform != null ? targetTransform.name : "null")}";
        }
    }

    /// <summary>
    /// 筹码飞行管理器
    /// 专门负责筹码从选中位置飞行到投注区域的动画效果
    /// </summary>
    public class BetFlyManager : MonoBehaviour
    {
        #region Inspector 配置

        [Header("飞行源点配置")]
        [SerializeField] private Transform chipSelectionArea;           // 筹码选择区域
        [SerializeField] private Transform[] chipButtons = new Transform[5]; // 5个筹码按钮位置

        [Header("飞行目标配置")]
        [SerializeField] private ChipFlyConfig[] flyConfigs = new ChipFlyConfig[8]; // 8个投注区域配置

        [Header("飞行参数配置")]
        [SerializeField] private float flyDuration = 0.8f;              // 飞行持续时间
        [SerializeField] private float flyHeight = 100f;                // 飞行高度（抛物线）
        [SerializeField] private AnimationCurve flyCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 飞行曲线
        [SerializeField] private bool enableRotation = true;            // 是否启用旋转
        [SerializeField] private float rotationSpeed = 360f;            // 旋转速度
        [SerializeField] private bool enableScaling = true;             // 是否启用缩放
        [SerializeField] private float maxScale = 1.2f;                 // 最大缩放

        [Header("视觉效果配置")]
        [SerializeField] private bool enableTrail = false;              // 是否启用拖尾
        [SerializeField] private bool enableGlow = false;               // 是否启用发光
        [SerializeField] private LayerMask flyLayer = -1;               // 飞行层级

        #endregion

        #region 私有字段

        private Dictionary<BetAreaType, Transform> targetMap;           // 目标位置映射
        private Dictionary<int, Sprite> chipSpritesMap;                // 筹码图片映射
        private Canvas uiCanvas;                                       // UI画布
        private bool isInitialized = false;

        #endregion

        #region 生命周期

        private void Awake()
        {
            InitializeFlyManager();
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化飞行管理器
        /// </summary>
        private void InitializeFlyManager()
        {
            // 初始化映射字典
            targetMap = new Dictionary<BetAreaType, Transform>();
            chipSpritesMap = new Dictionary<int, Sprite>();

            // 查找UI画布
            FindUICanvas();

            // 加载筹码图片
            LoadChipSprites();

            // 构建目标位置映射
            BuildTargetMap();

            // 验证配置
            ValidateConfiguration();

            isInitialized = true;
            Debug.Log("[BetFlyManager] 飞行管理器初始化完成");
        }

        /// <summary>
        /// 查找UI画布
        /// </summary>
        private void FindUICanvas()
        {
            // 优先查找父级的Canvas
            uiCanvas = GetComponentInParent<Canvas>();
            
            if (uiCanvas == null)
            {
                // 如果没找到，查找场景中的Canvas
                uiCanvas = FindObjectOfType<Canvas>();
            }

            if (uiCanvas == null)
            {
                Debug.LogError("[BetFlyManager] 未找到UI Canvas，筹码飞行效果无法正常工作");
            }
            else
            {
                Debug.Log($"[BetFlyManager] 找到UI Canvas: {uiCanvas.name}");
            }
        }

        /// <summary>
        /// 加载筹码图片
        /// </summary>
        private void LoadChipSprites()
        {
            var availableChips = BetDataManager.Instance.AvailableChips;
            
            for (int i = 0; i < availableChips.Length; i++)
            {
                int chipValue = availableChips[i];
                
                // 使用与BetDisplayManager相同的路径规则
                string spritePath = $"Images/chips/B_{chipValue:D2}";
                
                Sprite chipSprite = Resources.Load<Sprite>(spritePath);
                if (chipSprite != null)
                {
                    chipSpritesMap[chipValue] = chipSprite;
                    Debug.Log($"[BetFlyManager] 成功加载筹码图片: {spritePath}");
                }
                else
                {
                    // 尝试其他命名方式
                    string[] alternativePaths = {
                        $"Images/chips/B_{chipValue}",
                        $"Images/chips/B_{chipValue}K",
                        $"Images/chips/B_{chipValue}M"
                    };
                    
                    foreach (string altPath in alternativePaths)
                    {
                        chipSprite = Resources.Load<Sprite>(altPath);
                        if (chipSprite != null)
                        {
                            chipSpritesMap[chipValue] = chipSprite;
                            Debug.Log($"[BetFlyManager] 使用备选路径加载筹码图片: {altPath}");
                            break;
                        }
                    }
                }
            }
            
            Debug.Log($"[BetFlyManager] 筹码图片加载完成，共加载 {chipSpritesMap.Count}/{availableChips.Length} 个图片");
        }

        /// <summary>
        /// 构建目标位置映射
        /// </summary>
        private void BuildTargetMap()
        {
            foreach (var config in flyConfigs)
            {
                if (config != null && config.IsValid())
                {
                    targetMap[config.areaType] = config.targetTransform;
                    Debug.Log($"[BetFlyManager] 目标映射已添加: {config.GetDebugInfo()}");
                }
                else
                {
                    Debug.LogWarning($"[BetFlyManager] 飞行配置无效: {config?.areaType}");
                }
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
                if (targetMap.ContainsKey(areaType))
                {
                    validCount++;
                }
                else
                {
                    Debug.LogWarning($"[BetFlyManager] 缺失飞行目标配置: {areaType}");
                }
            }

            Debug.Log($"[BetFlyManager] 配置验证完成: {validCount}/8 个区域已配置");

            // 验证筹码选择区域
            if (chipSelectionArea == null)
            {
                Debug.LogWarning("[BetFlyManager] 筹码选择区域未配置");
            }

            // 验证筹码按钮
            int chipButtonCount = 0;
            for (int i = 0; i < chipButtons.Length; i++)
            {
                if (chipButtons[i] != null) chipButtonCount++;
            }
            Debug.Log($"[BetFlyManager] 筹码按钮配置: {chipButtonCount}/5");
        }

        #endregion

        #region 飞行控制

        /// <summary>
        /// 开始筹码飞行
        /// </summary>
        /// <param name="targetArea">目标区域</param>
        /// <param name="chipValue">筹码值</param>
        public void StartChipFly(BetAreaType targetArea, int chipValue)
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[BetFlyManager] 飞行管理器未初始化");
                return;
            }

            if (!targetMap.ContainsKey(targetArea))
            {
                Debug.LogWarning($"[BetFlyManager] 未找到目标区域: {targetArea}");
                return;
            }

            // 获取飞行起点和终点
            Vector3 startPos = GetChipSourcePosition(chipValue);
            Vector3 endPos = GetTargetPosition(targetArea);

            if (startPos == Vector3.zero || endPos == Vector3.zero)
            {
                Debug.LogWarning("[BetFlyManager] 无法获取有效的飞行起点或终点");
                return;
            }

            // 创建飞行筹码并开始动画
            StartCoroutine(ChipFlyCoroutine(startPos, endPos, chipValue));
        }

        /// <summary>
        /// 获取筹码源位置
        /// </summary>
        /// <param name="chipValue">筹码值</param>
        /// <returns>世界坐标位置</returns>
        private Vector3 GetChipSourcePosition(int chipValue)
        {
            // 根据筹码值找到对应的筹码按钮位置
            var availableChips = BetDataManager.Instance.AvailableChips;
            int chipIndex = System.Array.IndexOf(availableChips, chipValue);

            if (chipIndex >= 0 && chipIndex < chipButtons.Length && chipButtons[chipIndex] != null)
            {
                return chipButtons[chipIndex].position;
            }

            // 如果没有找到具体的筹码按钮，使用筹码选择区域的中心
            if (chipSelectionArea != null)
            {
                return chipSelectionArea.position;
            }

            Debug.LogWarning($"[BetFlyManager] 无法找到筹码 {chipValue} 的源位置");
            return Vector3.zero;
        }

        /// <summary>
        /// 获取目标位置
        /// </summary>
        /// <param name="targetArea">目标区域</param>
        /// <returns>世界坐标位置</returns>
        private Vector3 GetTargetPosition(BetAreaType targetArea)
        {
            if (targetMap.ContainsKey(targetArea))
            {
                return targetMap[targetArea].position;
            }

            Debug.LogWarning($"[BetFlyManager] 无法找到目标区域 {targetArea} 的位置");
            return Vector3.zero;
        }

        #endregion

        #region 飞行动画

        /// <summary>
        /// 筹码飞行协程
        /// </summary>
        /// <param name="startPos">起始位置</param>
        /// <param name="endPos">结束位置</param>
        /// <param name="chipValue">筹码值</param>
        /// <returns>协程</returns>
        private IEnumerator ChipFlyCoroutine(Vector3 startPos, Vector3 endPos, int chipValue)
        {
            // 创建飞行筹码
            GameObject flyingChip = CreateFlyingChip(chipValue);
            if (flyingChip == null)
            {
                Debug.LogError("[BetFlyManager] 创建飞行筹码失败");
                yield break;
            }

            RectTransform chipRect = flyingChip.GetComponent<RectTransform>();
            Vector3 originalScale = chipRect.localScale;

            // 设置初始位置
            chipRect.position = startPos;

            float elapsed = 0f;
            while (elapsed < flyDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / flyDuration;
                float curveValue = flyCurve.Evaluate(progress);

                // 计算位置（抛物线轨迹）
                Vector3 currentPos = CalculateParabolicPosition(startPos, endPos, curveValue);
                chipRect.position = currentPos;

                // 旋转效果
                if (enableRotation)
                {
                    float rotation = progress * rotationSpeed;
                    chipRect.rotation = Quaternion.Euler(0, 0, rotation);
                }

                // 缩放效果
                if (enableScaling)
                {
                    float scale = Mathf.Lerp(1f, maxScale, Mathf.Sin(progress * Mathf.PI));
                    chipRect.localScale = originalScale * scale;
                }

                yield return null;
            }

            // 确保最终位置正确
            chipRect.position = endPos;

            // 销毁飞行筹码
            Destroy(flyingChip);

            Debug.Log($"[BetFlyManager] 筹码飞行完成: {chipValue} -> 目标位置");
        }

        /// <summary>
        /// 计算抛物线位置
        /// </summary>
        /// <param name="start">起始位置</param>
        /// <param name="end">结束位置</param>
        /// <param name="progress">进度(0-1)</param>
        /// <returns>当前位置</returns>
        private Vector3 CalculateParabolicPosition(Vector3 start, Vector3 end, float progress)
        {
            // 线性插值X和Y
            Vector3 linearPos = Vector3.Lerp(start, end, progress);

            // 添加抛物线高度
            float height = flyHeight * Mathf.Sin(progress * Mathf.PI);
            linearPos.y += height;

            return linearPos;
        }

        /// <summary>
        /// 创建飞行筹码
        /// </summary>
        /// <param name="chipValue">筹码值</param>
        /// <returns>飞行筹码GameObject</returns>
        private GameObject CreateFlyingChip(int chipValue)
        {
            if (uiCanvas == null)
            {
                Debug.LogError("[BetFlyManager] UI Canvas 未找到，无法创建飞行筹码");
                return null;
            }

            // 创建筹码对象
            GameObject flyingChip = new GameObject($"FlyingChip_{chipValue}");
            flyingChip.transform.SetParent(uiCanvas.transform, false);

            // 添加Image组件
            Image chipImage = flyingChip.AddComponent<Image>();

            // 设置筹码图片
            if (chipSpritesMap.ContainsKey(chipValue))
            {
                chipImage.sprite = chipSpritesMap[chipValue];
            }
            else
            {
                // 使用颜色块作为备选
                chipImage.color = GetChipColor(chipValue);
                Debug.LogWarning($"[BetFlyManager] 筹码值 {chipValue} 没有对应图片，使用颜色块");
            }

            // 设置大小（使用较小尺寸，模拟飞行中的筹码）
            RectTransform chipRect = flyingChip.GetComponent<RectTransform>();
            chipRect.sizeDelta = new Vector2(50f, 50f); // 飞行筹码比正常筹码小一些

            // 保持图片比例
            chipImage.preserveAspect = true;

            // 设置层级，确保在最上层
            chipRect.SetAsLastSibling();

            return flyingChip;
        }

        /// <summary>
        /// 获取筹码颜色
        /// </summary>
        /// <param name="chipValue">筹码值</param>
        /// <returns>筹码颜色</returns>
        private Color GetChipColor(int chipValue)
        {
            return chipValue switch
            {
                1 => Color.white,
                5 => Color.red,
                10 => Color.blue,
                20 => Color.green,
                50 => Color.yellow,
                _ => Color.gray
            };
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置飞行参数
        /// </summary>
        /// <param name="duration">飞行时间</param>
        /// <param name="height">飞行高度</param>
        /// <param name="enableRotation">是否旋转</param>
        public void SetFlyParameters(float duration, float height, bool enableRotation)
        {
            flyDuration = duration;
            flyHeight = height;
            this.enableRotation = enableRotation;
            
            Debug.Log($"[BetFlyManager] 飞行参数已更新: 时间={duration}, 高度={height}, 旋转={enableRotation}");
        }

        /// <summary>
        /// 设置筹码源位置
        /// </summary>
        /// <param name="chipIndex">筹码索引</param>
        /// <param name="position">位置Transform</param>
        public void SetChipSourcePosition(int chipIndex, Transform position)
        {
            if (chipIndex >= 0 && chipIndex < chipButtons.Length)
            {
                chipButtons[chipIndex] = position;
                Debug.Log($"[BetFlyManager] 设置筹码 {chipIndex} 源位置: {position?.name}");
            }
        }

        /// <summary>
        /// 设置目标位置
        /// </summary>
        /// <param name="areaType">区域类型</param>
        /// <param name="target">目标Transform</param>
        public void SetTargetPosition(BetAreaType areaType, Transform target)
        {
            targetMap[areaType] = target;
            Debug.Log($"[BetFlyManager] 设置目标位置: {areaType} -> {target?.name}");
        }

        #endregion

        #region 调试和编辑器辅助

        /// <summary>
        /// 验证所有飞行配置
        /// </summary>
        [ContextMenu("验证飞行配置")]
        public void ValidateAllFlyConfigurations()
        {
            Debug.Log("=== BetFlyManager 飞行配置验证 ===");
            
            Debug.Log($"筹码选择区域: {(chipSelectionArea != null ? chipSelectionArea.name : "未配置")}");
            
            for (int i = 0; i < chipButtons.Length; i++)
            {
                Debug.Log($"筹码按钮 {i}: {(chipButtons[i] != null ? chipButtons[i].name : "未配置")}");
            }
            
            foreach (var config in flyConfigs)
            {
                if (config != null)
                {
                    Debug.Log(config.GetDebugInfo());
                }
                else
                {
                    Debug.LogWarning("发现null的飞行配置");
                }
            }

            Debug.Log($"已初始化目标数量: {targetMap?.Count ?? 0}/8");
        }

        /// <summary>
        /// 测试筹码飞行
        /// </summary>
        [ContextMenu("测试筹码飞行")]
        public void TestChipFly()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("请在运行时测试筹码飞行");
                return;
            }

            Debug.Log("=== 测试筹码飞行 ===");
            
            // 测试飞到庄家区域
            StartChipFly(BetAreaType.Banker, 5);
        }

        /// <summary>
        /// 获取飞行管理器状态信息
        /// </summary>
        /// <returns>状态信息</returns>
        public string GetStatusInfo()
        {
            return $"[BetFlyManager] 状态:\n" +
                   $"已初始化: {isInitialized}\n" +
                   $"UI画布: {(uiCanvas != null ? uiCanvas.name : "未找到")}\n" +
                   $"配置目标数: {targetMap?.Count ?? 0}/8\n" +
                   $"已加载筹码图片数: {chipSpritesMap?.Count ?? 0}";
        }

        #endregion
    }
}
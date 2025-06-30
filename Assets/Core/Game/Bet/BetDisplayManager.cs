// Assets/Scripts/Core/BetDisplayManager.cs
// 投注显示管理器 - UI显示控制器
// 负责管理所有投注区域的金额显示、筹码显示和闪烁效果
// 创建时间: 2025/6/30

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using BaccaratGame.Managers;

namespace BaccaratGame.Core
{
    /// <summary>
    /// 投注区域显示组件
    /// </summary>
    [System.Serializable]
    public class BetAreaDisplay
    {
        [Header("区域配置")]
        public BetAreaType areaType;                    // 区域类型

        [Header("UI组件引用")]
        public TextMeshProUGUI moneyText;               // 金额显示文本
        public Transform chipsContainer;                // 筹码容器
        public GameObject areaObject;                   // 投注区域对象（用于闪烁）

        /// <summary>
        /// 验证配置是否完整
        /// </summary>
        public bool IsValid()
        {
            return moneyText != null && chipsContainer != null;
        }

        /// <summary>
        /// 获取调试信息
        /// </summary>
        public string GetDebugInfo()
        {
            return $"{areaType} - Text:{(moneyText != null ? "✓" : "✗")} Chips:{(chipsContainer != null ? "✓" : "✗")} Area:{(areaObject != null ? "✓" : "✗")}";
        }
    }

    /// <summary>
    /// 投注显示管理器
    /// 负责管理所有投注区域的UI显示更新和闪烁效果
    /// </summary>
    public class BetDisplayManager : MonoBehaviour
    {
        #region Inspector 配置

        [Header("投注区域显示配置")]
        [SerializeField] private BetAreaDisplay[] betAreaDisplays = new BetAreaDisplay[8];

        [Header("显示格式配置")]
        [SerializeField] private string moneyFormat = "¥{0}";           // 金额显示格式
        [SerializeField] private Color normalTextColor = Color.white;    // 正常文本颜色
        [SerializeField] private Color highlightTextColor = Color.yellow; // 高亮文本颜色

        [Header("筹码显示配置")]
        [SerializeField] private float chipSize = 30f;                  // 筹码图标大小
        [SerializeField] private float chipSpacing = 5f;                // 筹码间距
        [SerializeField] private int maxChipsPerRow = 5;                // 每行最大筹码数
        [SerializeField] private bool enableChipAnimation = true;        // 是否启用筹码动画

        [Header("闪烁效果配置")]
        [SerializeField] private float flashDuration = 2f;              // 闪烁持续时间
        [SerializeField] private float flashInterval = 0.3f;            // 闪烁间隔
        [SerializeField] private Color flashColor = Color.yellow;       // 闪烁颜色
        [SerializeField] private AnimationCurve flashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 闪烁曲线

        #endregion

        #region 私有字段

        private Dictionary<BetAreaType, BetAreaDisplay> areaDisplayMap;  // 区域显示映射
        private Dictionary<BetAreaType, List<GameObject>> chipObjects;   // 筹码对象映射
        private Dictionary<BetAreaType, Coroutine> flashCoroutines;      // 闪烁协程映射
        private Dictionary<int, Sprite> chipSpritesMap;                  // 筹码图片映射
        private bool isInitialized = false;

        #endregion

        #region 生命周期

        private void Awake()
        {
            InitializeDisplayManager();
        }

        private void Start()
        {
            SubscribeToEvents();
            RefreshAllDisplays();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化显示管理器
        /// </summary>
        private void InitializeDisplayManager()
        {
            // 初始化映射字典
            areaDisplayMap = new Dictionary<BetAreaType, BetAreaDisplay>();
            chipObjects = new Dictionary<BetAreaType, List<GameObject>>();
            flashCoroutines = new Dictionary<BetAreaType, Coroutine>();
            chipSpritesMap = new Dictionary<int, Sprite>();

            // 加载筹码图片资源
            LoadChipSprites();

            // 构建区域显示映射
            BuildAreaDisplayMap();

            // 验证配置
            ValidateConfiguration();

            isInitialized = true;
            Debug.Log("[BetDisplayManager] 显示管理器初始化完成");
        }

        /// <summary>
        /// 加载筹码图片资源
        /// </summary>
        private void LoadChipSprites()
        {
            var availableChips = BetDataManager.Instance.AvailableChips;
            
            for (int i = 0; i < availableChips.Length; i++)
            {
                int chipValue = availableChips[i];
                
                // 构建资源路径，参考ChipSelectionManager的命名规则
                string spritePath = $"Images/chips/B_{chipValue}"; // 假设筹码图片命名为 B_1, B_5, B_10 等
                
                Sprite chipSprite = Resources.Load<Sprite>(spritePath);
                if (chipSprite != null)
                {
                    chipSpritesMap[chipValue] = chipSprite;
                    Debug.Log($"[BetDisplayManager] 成功加载筹码图片: {spritePath}");
                }
                else
                {
                    Debug.LogWarning($"[BetDisplayManager] 未找到筹码图片: {spritePath}");
                    // 如果找不到图片，可以尝试其他可能的命名方式
                    string alternativePath = $"Images/chips/chip_{chipValue}";
                    chipSprite = Resources.Load<Sprite>(alternativePath);
                    if (chipSprite != null)
                    {
                        chipSpritesMap[chipValue] = chipSprite;
                        Debug.Log($"[BetDisplayManager] 使用备选路径加载筹码图片: {alternativePath}");
                    }
                }
            }
            
            Debug.Log($"[BetDisplayManager] 筹码图片加载完成，共加载 {chipSpritesMap.Count}/{availableChips.Length} 个图片");
        }

        /// <summary>
        /// 构建区域显示映射
        /// </summary>
        private void BuildAreaDisplayMap()
        {
            foreach (var display in betAreaDisplays)
            {
                if (display != null && display.IsValid())
                {
                    areaDisplayMap[display.areaType] = display;
                    chipObjects[display.areaType] = new List<GameObject>();
                    
                    // 初始化筹码容器
                    InitializeChipsContainer(display);
                    
                    Debug.Log($"[BetDisplayManager] 区域映射已添加: {display.GetDebugInfo()}");
                }
                else
                {
                    Debug.LogWarning($"[BetDisplayManager] 区域显示配置无效: {display?.areaType}");
                }
            }
        }

        /// <summary>
        /// 初始化筹码容器
        /// </summary>
        /// <param name="display">区域显示配置</param>
        private void InitializeChipsContainer(BetAreaDisplay display)
        {
            if (display.chipsContainer == null) return;

            // 清空现有的筹码对象（包括默认的Image）
            ClearContainerChildren(display.chipsContainer);

            // 设置容器布局（如果没有Layout组件的话）
            SetupContainerLayout(display.chipsContainer);
        }

        /// <summary>
        /// 清空容器的所有子对象
        /// </summary>
        /// <param name="container">容器Transform</param>
        private void ClearContainerChildren(Transform container)
        {
            // 在编辑器模式下使用DestroyImmediate，运行时使用Destroy
            if (Application.isPlaying)
            {
                foreach (Transform child in container)
                {
                    Destroy(child.gameObject);
                }
            }
            else
            {
                // 编辑器模式下的清理
                for (int i = container.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(container.GetChild(i).gameObject);
                }
            }
            
            Debug.Log($"[BetDisplayManager] 已清空容器 {container.name} 的所有子对象");
        }

        /// <summary>
        /// 设置容器布局
        /// </summary>
        /// <param name="container">容器Transform</param>
        private void SetupContainerLayout(Transform container)
        {
            var layoutGroup = container.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = container.gameObject.AddComponent<HorizontalLayoutGroup>();
                layoutGroup.spacing = chipSpacing;
                layoutGroup.childControlWidth = false;
                layoutGroup.childControlHeight = false;
                layoutGroup.childForceExpandWidth = false;
                layoutGroup.childForceExpandHeight = false;
                layoutGroup.childAlignment = TextAnchor.MiddleLeft;
                
                Debug.Log($"[BetDisplayManager] 为容器 {container.name} 添加了HorizontalLayoutGroup");
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
                if (areaDisplayMap.ContainsKey(areaType))
                {
                    validCount++;
                }
                else
                {
                    Debug.LogWarning($"[BetDisplayManager] 缺失区域配置: {areaType}");
                }
            }

            Debug.Log($"[BetDisplayManager] 配置验证完成: {validCount}/8 个区域已配置");
        }

        #endregion

        #region 事件订阅

        /// <summary>
        /// 订阅BetDataManager事件
        /// </summary>
        private void SubscribeToEvents()
        {
            var betManager = BetDataManager.Instance;
            betManager.OnBetAmountChanged += OnBetAmountChanged;
            betManager.OnAllBetsCleared += OnAllBetsCleared;
            betManager.OnBetUndone += OnBetUndone;
            betManager.OnBetsRepeated += OnBetsRepeated;

            Debug.Log("[BetDisplayManager] 事件订阅完成");
        }

        /// <summary>
        /// 取消订阅BetDataManager事件
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            var betManager = BetDataManager.Instance;
            if (betManager != null)
            {
                betManager.OnBetAmountChanged -= OnBetAmountChanged;
                betManager.OnAllBetsCleared -= OnAllBetsCleared;
                betManager.OnBetUndone -= OnBetUndone;
                betManager.OnBetsRepeated -= OnBetsRepeated;
            }

            Debug.Log("[BetDisplayManager] 事件取消订阅完成");
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 投注金额改变事件处理
        /// </summary>
        /// <param name="areaType">区域类型</param>
        private void OnBetAmountChanged(BetAreaType areaType)
        {
            RefreshBetDisplay(areaType);
            Debug.Log($"[BetDisplayManager] 刷新区域显示: {areaType}");
        }

        /// <summary>
        /// 所有投注清空事件处理
        /// </summary>
        private void OnAllBetsCleared()
        {
            RefreshAllDisplays();
            Debug.Log("[BetDisplayManager] 刷新所有区域显示");
        }

        /// <summary>
        /// 投注撤销事件处理
        /// </summary>
        /// <param name="areaType">区域类型</param>
        private void OnBetUndone(BetAreaType areaType)
        {
            RefreshBetDisplay(areaType);
            Debug.Log($"[BetDisplayManager] 撤销后刷新区域显示: {areaType}");
        }

        /// <summary>
        /// 重复投注事件处理
        /// </summary>
        private void OnBetsRepeated()
        {
            RefreshAllDisplays();
            Debug.Log("[BetDisplayManager] 重复投注后刷新所有区域显示");
        }

        #endregion

        #region 显示更新

        /// <summary>
        /// 刷新指定区域的显示
        /// </summary>
        /// <param name="areaType">区域类型</param>
        public void RefreshBetDisplay(BetAreaType areaType)
        {
            if (!isInitialized || !areaDisplayMap.ContainsKey(areaType))
            {
                Debug.LogWarning($"[BetDisplayManager] 无法刷新区域显示: {areaType}");
                return;
            }

            var display = areaDisplayMap[areaType];
            var betManager = BetDataManager.Instance;

            // 更新金额显示
            UpdateMoneyDisplay(display, betManager.GetBetAmount(areaType));

            // 更新筹码显示
            UpdateChipsDisplay(display, betManager.GetChipCombination(areaType));
        }

        /// <summary>
        /// 刷新所有区域的显示
        /// </summary>
        public void RefreshAllDisplays()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[BetDisplayManager] 显示管理器未初始化");
                return;
            }

            foreach (BetAreaType areaType in areaDisplayMap.Keys)
            {
                RefreshBetDisplay(areaType);
            }

            Debug.Log("[BetDisplayManager] 所有区域显示已刷新");
        }

        /// <summary>
        /// 更新金额显示
        /// </summary>
        /// <param name="display">区域显示配置</param>
        /// <param name="amount">金额</param>
        private void UpdateMoneyDisplay(BetAreaDisplay display, decimal amount)
        {
            if (display.moneyText == null) return;

            if (amount > 0)
            {
                display.moneyText.text = string.Format(moneyFormat, amount);
                display.moneyText.color = normalTextColor;
                display.moneyText.gameObject.SetActive(true);
            }
            else
            {
                display.moneyText.text = "";
                display.moneyText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 更新筹码显示
        /// </summary>
        /// <param name="display">区域显示配置</param>
        /// <param name="chips">筹码列表</param>
        private void UpdateChipsDisplay(BetAreaDisplay display, List<int> chips)
        {
            if (display.chipsContainer == null) return;

            // 清空现有筹码对象
            ClearChipObjects(display.areaType);

            // 如果没有筹码，隐藏容器
            if (chips.Count == 0)
            {
                display.chipsContainer.gameObject.SetActive(false);
                return;
            }

            // 显示容器并创建筹码对象
            display.chipsContainer.gameObject.SetActive(true);
            CreateChipObjects(display, chips);
        }

        /// <summary>
        /// 清空筹码对象
        /// </summary>
        /// <param name="areaType">区域类型</param>
        private void ClearChipObjects(BetAreaType areaType)
        {
            if (!chipObjects.ContainsKey(areaType)) return;

            foreach (var chipObj in chipObjects[areaType])
            {
                if (chipObj != null)
                {
                    Destroy(chipObj);
                }
            }

            chipObjects[areaType].Clear();
        }

        /// <summary>
        /// 创建筹码对象
        /// </summary>
        /// <param name="display">区域显示配置</param>
        /// <param name="chips">筹码列表</param>
        private void CreateChipObjects(BetAreaDisplay display, List<int> chips)
        {
            for (int i = 0; i < chips.Count; i++)
            {
                var chipValue = chips[i];
                GameObject chipObj = CreateChipImageObject(display.chipsContainer, chipValue);
                if (chipObj != null)
                {
                    chipObjects[display.areaType].Add(chipObj);
                    
                    // 设置堆叠效果（稍微错开位置）
                    ApplyStackingEffect(chipObj, i);
                    
                    // 添加动画效果
                    if (enableChipAnimation)
                    {
                        StartCoroutine(AnimateChipAppear(chipObj));
                    }
                }
            }
        }

        /// <summary>
        /// 创建筹码图片对象
        /// </summary>
        /// <param name="parent">父容器</param>
        /// <param name="chipValue">筹码值</param>
        /// <returns>筹码对象</returns>
        private GameObject CreateChipImageObject(Transform parent, int chipValue)
        {
            // 创建基础对象
            GameObject chipObj = new GameObject($"Chip_{chipValue}");
            chipObj.transform.SetParent(parent);

            // 添加Image组件
            Image chipImage = chipObj.AddComponent<Image>();
            
            // 设置筹码图片
            if (chipSpritesMap.ContainsKey(chipValue))
            {
                chipImage.sprite = chipSpritesMap[chipValue];
            }
            else
            {
                // 如果没有找到对应的图片，使用颜色块作为备选
                chipImage.color = GetChipColor(chipValue);
                Debug.LogWarning($"[BetDisplayManager] 筹码值 {chipValue} 没有对应的图片，使用颜色块");
            }

            // 设置大小
            RectTransform rectTransform = chipObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(chipSize, chipSize);

            // 保持图片比例
            chipImage.preserveAspect = true;

            return chipObj;
        }

        /// <summary>
        /// 应用筹码堆叠效果
        /// </summary>
        /// <param name="chipObj">筹码对象</param>
        /// <param name="stackIndex">堆叠索引</param>
        private void ApplyStackingEffect(GameObject chipObj, int stackIndex)
        {
            RectTransform rectTransform = chipObj.GetComponent<RectTransform>();
            if (rectTransform == null) return;

            // 计算堆叠偏移
            float offsetX = stackIndex * (chipSize * 0.1f); // 每个筹码稍微向右偏移
            float offsetY = stackIndex * (chipSize * 0.05f); // 每个筹码稍微向上偏移，营造堆叠效果
            
            // 应用偏移
            rectTransform.anchoredPosition = new Vector2(offsetX, offsetY);
            
            // 设置渲染顺序，后面的筹码在上层
            rectTransform.SetSiblingIndex(stackIndex);
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

        #region 动画效果

        /// <summary>
        /// 筹码出现动画
        /// </summary>
        /// <param name="chipObj">筹码对象</param>
        /// <returns>协程</returns>
        private IEnumerator AnimateChipAppear(GameObject chipObj)
        {
            if (chipObj == null) yield break;

            RectTransform rectTransform = chipObj.GetComponent<RectTransform>();
            Vector3 originalScale = rectTransform.localScale;

            // 从0缩放到原始大小
            rectTransform.localScale = Vector3.zero;
            
            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float scale = flashCurve.Evaluate(progress);
                
                rectTransform.localScale = originalScale * scale;
                yield return null;
            }

            rectTransform.localScale = originalScale;
        }

        #endregion

        #region 闪烁效果

        /// <summary>
        /// 开始闪烁指定区域
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
            if (!areaDisplayMap.ContainsKey(areaType))
            {
                Debug.LogWarning($"[BetDisplayManager] 无法闪烁区域: {areaType}");
                return;
            }

            // 停止现有的闪烁
            StopFlashArea(areaType);

            // 开始新的闪烁
            var display = areaDisplayMap[areaType];
            if (display.areaObject != null)
            {
                flashCoroutines[areaType] = StartCoroutine(FlashAreaCoroutine(display));
                Debug.Log($"[BetDisplayManager] 开始闪烁区域: {areaType}");
            }
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
                Debug.Log($"[BetDisplayManager] 停止闪烁区域: {areaType}");
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
        }

        /// <summary>
        /// 闪烁区域协程
        /// </summary>
        /// <param name="display">区域显示配置</param>
        /// <returns>协程</returns>
        private IEnumerator FlashAreaCoroutine(BetAreaDisplay display)
        {
            var image = display.areaObject.GetComponent<Image>();
            var originalColor = image != null ? image.color : Color.white;

            float elapsed = 0f;
            while (elapsed < flashDuration)
            {
                // 计算闪烁强度
                float flashProgress = (elapsed % flashInterval) / flashInterval;
                float intensity = flashCurve.Evaluate(flashProgress);

                // 应用闪烁效果
                ApplyFlashEffect(display, intensity, originalColor);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 恢复原始状态
            RestoreAreaOriginalState(display.areaType);
            flashCoroutines[display.areaType] = null;
        }

        /// <summary>
        /// 应用闪烁效果
        /// </summary>
        /// <param name="display">区域显示配置</param>
        /// <param name="intensity">闪烁强度</param>
        /// <param name="originalColor">原始颜色</param>
        private void ApplyFlashEffect(BetAreaDisplay display, float intensity, Color originalColor)
        {
            var image = display.areaObject.GetComponent<Image>();
            if (image != null)
            {
                Color flashedColor = Color.Lerp(originalColor, flashColor, intensity);
                image.color = flashedColor;
            }

            // 文字高亮效果
            if (display.moneyText != null)
            {
                Color textColor = Color.Lerp(normalTextColor, highlightTextColor, intensity);
                display.moneyText.color = textColor;
            }
        }

        /// <summary>
        /// 恢复区域原始状态
        /// </summary>
        /// <param name="areaType">区域类型</param>
        private void RestoreAreaOriginalState(BetAreaType areaType)
        {
            if (!areaDisplayMap.ContainsKey(areaType)) return;

            var display = areaDisplayMap[areaType];
            
            // 恢复区域对象颜色
            var image = display.areaObject?.GetComponent<Image>();
            if (image != null)
            {
                // 这里应该恢复到原始颜色，暂时设为白色
                image.color = Color.white;
            }

            // 恢复文字颜色
            if (display.moneyText != null)
            {
                display.moneyText.color = normalTextColor;
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置区域显示配置（运行时动态配置）
        /// </summary>
        /// <param name="areaType">区域类型</param>
        /// <param name="display">显示配置</param>
        public void SetAreaDisplay(BetAreaType areaType, BetAreaDisplay display)
        {
            if (display != null && display.IsValid())
            {
                areaDisplayMap[areaType] = display;
                chipObjects[areaType] = new List<GameObject>();
                InitializeChipsContainer(display);
                
                Debug.Log($"[BetDisplayManager] 运行时设置区域显示: {display.GetDebugInfo()}");
            }
        }

        /// <summary>
        /// 获取区域显示配置
        /// </summary>
        /// <param name="areaType">区域类型</param>
        /// <returns>显示配置</returns>
        public BetAreaDisplay GetAreaDisplay(BetAreaType areaType)
        {
            return areaDisplayMap.ContainsKey(areaType) ? areaDisplayMap[areaType] : null;
        }

        /// <summary>
        /// 设置金额显示格式
        /// </summary>
        /// <param name="format">格式字符串</param>
        public void SetMoneyFormat(string format)
        {
            moneyFormat = format;
            RefreshAllDisplays();
        }

        /// <summary>
        /// 手动触发显示刷新
        /// </summary>
        public void ForceRefreshAllDisplays()
        {
            RefreshAllDisplays();
        }

        #endregion

        #region 调试和编辑器辅助

        /// <summary>
        /// 验证所有区域配置
        /// </summary>
        [ContextMenu("验证区域配置")]
        public void ValidateAllAreaConfigurations()
        {
            Debug.Log("=== BetDisplayManager 区域配置验证 ===");
            
            foreach (var display in betAreaDisplays)
            {
                if (display != null)
                {
                    Debug.Log(display.GetDebugInfo());
                }
                else
                {
                    Debug.LogWarning("发现null的区域显示配置");
                }
            }

            Debug.Log($"已初始化区域数量: {areaDisplayMap?.Count ?? 0}/8");
        }

        /// <summary>
        /// 测试闪烁效果
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
            
            foreach (BetAreaType areaType in areaDisplayMap.Keys)
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
        /// 获取显示管理器状态信息
        /// </summary>
        /// <returns>状态信息</returns>
        public string GetStatusInfo()
        {
            int activeFlashCount = 0;
            if (flashCoroutines != null)
            {
                foreach (var coroutine in flashCoroutines.Values)
                {
                    if (coroutine != null) activeFlashCount++;
                }
            }

            return $"[BetDisplayManager] 状态:\n" +
                   $"已初始化: {isInitialized}\n" +
                   $"配置区域数: {areaDisplayMap?.Count ?? 0}/8\n" +
                   $"活跃闪烁数: {activeFlashCount}";
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取第一个有效的区域显示配置（用于获取筹码图片等共用资源）
        /// </summary>
        /// <returns>区域显示配置</returns>
        private BetAreaDisplay GetFirstValidDisplay()
        {
            if (areaDisplayMap == null || areaDisplayMap.Count == 0) return null;
            
            foreach (var display in areaDisplayMap.Values)
            {
                if (display != null) return display;
            }
            return null;
        }

        #endregion
    }
}
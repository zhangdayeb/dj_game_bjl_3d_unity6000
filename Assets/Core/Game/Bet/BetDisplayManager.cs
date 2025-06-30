// Assets/Scripts/Core/BetDisplayManager.cs
// 投注显示管理器 - UI显示控制器
// 负责管理所有投注区域的金额显示和筹码显示
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
            return $"{areaType} - Text:{(moneyText != null ? "✓" : "✗")} Chips:{(chipsContainer != null ? "✓" : "✗")}";
        }
    }

    /// <summary>
    /// 投注显示管理器
    /// 负责管理所有投注区域的UI显示更新
    /// </summary>
    public class BetDisplayManager : MonoBehaviour
    {
        #region Inspector 配置

        [Header("投注区域显示配置")]
        [SerializeField] private BetAreaDisplay[] betAreaDisplays = new BetAreaDisplay[8];

        [Header("筹码显示配置")]
        [SerializeField] private bool enableChipAnimation = true;        // 是否启用筹码动画

        #endregion

        #region 私有字段

        private Dictionary<BetAreaType, BetAreaDisplay> areaDisplayMap;  // 区域显示映射
        private Dictionary<BetAreaType, List<GameObject>> chipObjects;   // 筹码对象映射
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
                
                // 根据截图中的路径结构：Assets/Resources/Images/chips/B_01, B_1K 等
                string spritePath = $"Images/chips/B_{chipValue:D2}"; // 先尝试两位数格式 B_01, B_05, B_10, B_20, B_50
                
                Sprite chipSprite = Resources.Load<Sprite>(spritePath);
                if (chipSprite != null)
                {
                    chipSpritesMap[chipValue] = chipSprite;
                    Debug.Log($"[BetDisplayManager] 成功加载筹码图片: {spritePath}");
                }
                else
                {
                    // 尝试其他可能的命名方式
                    string[] alternativePaths = {
                        $"Images/chips/B_{chipValue}", // B_1, B_5 等
                        $"Images/chips/B_{chipValue}K", // B_1K, B_5K 等 (如果是千)
                        $"Images/chips/B_{chipValue}M"  // B_1M, B_5M 等 (如果是万)
                    };
                    
                    foreach (string altPath in alternativePaths)
                    {
                        chipSprite = Resources.Load<Sprite>(altPath);
                        if (chipSprite != null)
                        {
                            chipSpritesMap[chipValue] = chipSprite;
                            Debug.Log($"[BetDisplayManager] 使用备选路径加载筹码图片: {altPath}");
                            break;
                        }
                    }
                    
                    if (chipSprite == null)
                    {
                        Debug.LogWarning($"[BetDisplayManager] 未找到筹码图片: 尝试了多个路径但都失败");
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

            // 设置容器布局为垂直布局
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
        /// 设置容器布局为垂直布局
        /// </summary>
        /// <param name="container">容器Transform</param>
        private void SetupContainerLayout(Transform container)
        {
            // 移除可能存在的水平布局组件
            var horizontalLayout = container.GetComponent<HorizontalLayoutGroup>();
            if (horizontalLayout != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(horizontalLayout);
                }
                else
                {
                    DestroyImmediate(horizontalLayout);
                }
            }

            // 添加垂直布局组件
            var verticalLayout = container.GetComponent<VerticalLayoutGroup>();
            if (verticalLayout == null)
            {
                verticalLayout = container.gameObject.AddComponent<VerticalLayoutGroup>();
                verticalLayout.spacing = 2f; // 筹码间距很小，营造堆叠效果
                verticalLayout.childControlWidth = false;
                verticalLayout.childControlHeight = false;
                verticalLayout.childForceExpandWidth = false;
                verticalLayout.childForceExpandHeight = false;
                verticalLayout.childAlignment = TextAnchor.LowerCenter; // 从底部开始堆叠
                verticalLayout.reverseArrangement = false; // 第一个筹码在底部
                
                Debug.Log($"[BetDisplayManager] 为容器 {container.name} 添加了VerticalLayoutGroup");
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
                display.moneyText.text = amount.ToString(); // 直接显示数字，不加任何符号
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
                GameObject chipObj = CreateChipImageObject(display.chipsContainer, chipValue, display.areaType);
                if (chipObj != null)
                {
                    chipObjects[display.areaType].Add(chipObj);
                    
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
        /// <param name="areaType">区域类型</param>
        /// <returns>筹码对象</returns>
        private GameObject CreateChipImageObject(Transform parent, int chipValue, BetAreaType areaType)
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

            // 根据区域类型设置筹码大小
            float chipSize = GetChipSizeForArea(areaType);
            RectTransform rectTransform = chipObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(chipSize, chipSize);

            // 保持图片比例
            chipImage.preserveAspect = true;

            return chipObj;
        }

        /// <summary>
        /// 根据区域类型获取筹码大小
        /// </summary>
        /// <param name="areaType">区域类型</param>
        /// <returns>筹码大小</returns>
        private float GetChipSizeForArea(BetAreaType areaType)
        {
            // Banker, Player, Tie 区域使用大筹码 65x65
            if (areaType == BetAreaType.Banker || areaType == BetAreaType.Player || areaType == BetAreaType.Tie)
            {
                return 65f;
            }
            // 其他区域使用小筹码 35x35
            else
            {
                return 35f;
            }
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
                float scale = Mathf.SmoothStep(0f, 1f, progress);
                
                rectTransform.localScale = originalScale * scale;
                yield return null;
            }

            rectTransform.localScale = originalScale;
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
        /// 获取显示管理器状态信息
        /// </summary>
        /// <returns>状态信息</returns>
        public string GetStatusInfo()
        {
            return $"[BetDisplayManager] 状态:\n" +
                   $"已初始化: {isInitialized}\n" +
                   $"配置区域数: {areaDisplayMap?.Count ?? 0}/8\n" +
                   $"已加载筹码图片数: {chipSpritesMap?.Count ?? 0}";
        }

        #endregion
    }
}
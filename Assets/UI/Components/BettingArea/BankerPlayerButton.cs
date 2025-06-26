// Assets/UI/Components/BettingArea/BankerPlayerButton.cs
// 庄闲和投注按钮组件 - 手动控制版本
// 优化UI创建逻辑，支持完整的三种按钮类型
// 创建时间: 2025/6/27

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BaccaratGame.Data;

namespace BaccaratGame.UI.Components
{
    /// <summary>
    /// 庄闲和投注按钮组件 - 手动控制版本
    /// 支持庄、闲、和三种按钮类型，手动创建UI界面
    /// </summary>
    public class BankerPlayerButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        #region 序列化字段

        [Header("按钮配置")]
        public BaccaratBetType betType = BaccaratBetType.Player;
        
        [Header("按钮布局")]
        public Vector2 buttonSize = new Vector2(200, 80);
        public Vector2 buttonPosition = Vector2.zero;

        [Header("字体设置")]
        public int titleFontSize = 24;
        public int oddsFontSize = 16;
        public int numberFontSize = 14;

        [Header("调试设置")]
        public bool enableDebugMode = false;

        #endregion

        #region 按钮类型配置

        /// <summary>
        /// 按钮类型配置数据
        /// </summary>
        [Serializable]
        public class BetTypeConfig
        {
            public BaccaratBetType betType;
            public string displayTitle;
            public string odds;
            public Color buttonColor;
            public Color textColor;
            public Color numberColor;
        }

        /// <summary>
        /// 预定义的按钮配置
        /// </summary>
        private static readonly BetTypeConfig[] BetTypeConfigs = new BetTypeConfig[]
        {
            new BetTypeConfig
            {
                betType = BaccaratBetType.Player,
                displayTitle = "闲",
                odds = "1:1",
                buttonColor = new Color(0.2f, 0.4f, 1f, 1f),      // 蓝色
                textColor = Color.white,
                numberColor = Color.yellow
            },
            new BetTypeConfig
            {
                betType = BaccaratBetType.Tie,
                displayTitle = "和",
                odds = "1:8",
                buttonColor = new Color(0.2f, 0.8f, 0.2f, 1f),    // 绿色
                textColor = Color.white,
                numberColor = Color.yellow
            },
            new BetTypeConfig
            {
                betType = BaccaratBetType.Banker,
                displayTitle = "庄",
                odds = "1:0.95",
                buttonColor = new Color(1f, 0.2f, 0.2f, 1f),      // 红色
                textColor = Color.white,
                numberColor = Color.yellow
            }
        };

        #endregion

        #region 私有字段

        private RectTransform rectTransform;
        private Canvas parentCanvas;
        private bool buttonUICreated = false;
        private bool isInteractable = true;
        private int currentPlayerCount = 0;
        private decimal currentAmount = 0m;
        
        // UI组件引用
        private Image backgroundImage;
        private Button button;
        private Text titleText;
        private Text oddsText;
        private Text playerCountText;
        private Text amountText;

        // 当前配置
        private BetTypeConfig currentConfig;

        // 动画协程引用
        private Coroutine currentAnimation;

        #endregion

        #region 事件定义

        // 事件回调
        public System.Action OnButtonClicked;
        public System.Action<BaccaratBetType> OnBetTypeSelected;
        public System.Action<BaccaratBetType, int, decimal> OnBetDataUpdated;

        #endregion

        #region 生命周期

        private void Awake()
        {
            InitializeComponent();
        }

        private void Start()
        {
            // 直接创建UI以查看布局效果
            CreateButtonUI();
            
            // 根据按钮类型显示对应的测试数据
            switch (betType)
            {
                case BaccaratBetType.Player:
                    UpdateDisplay(26, 844m);
                    break;
                case BaccaratBetType.Tie:
                    UpdateDisplay(8, 255m);
                    break;
                case BaccaratBetType.Banker:
                    UpdateDisplay(38, 735m);
                    break;
            }
            
            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] {betType} UI布局已创建");
        }

        private void OnDestroy()
        {
            // 清理动画协程
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
                currentAnimation = null;
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponent()
        {
            // 确保有RectTransform
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            // 设置按钮大小和位置
            rectTransform.sizeDelta = buttonSize;
            rectTransform.anchoredPosition = buttonPosition;

            // 查找父Canvas
            parentCanvas = GetComponentInParent<Canvas>();

            // 获取当前配置
            currentConfig = GetBetTypeConfig(betType);

            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] {betType} 组件初始化完成");
        }

        /// <summary>
        /// 获取投注类型配置
        /// </summary>
        private BetTypeConfig GetBetTypeConfig(BaccaratBetType type)
        {
            foreach (var config in BetTypeConfigs)
            {
                if (config.betType == type)
                    return config;
            }
            return BetTypeConfigs[0]; // 默认返回第一个配置
        }

        /// <summary>
        /// 更新按钮配置
        /// </summary>
        private void UpdateButtonConfig()
        {
            currentConfig = GetBetTypeConfig(betType);
            
            if (buttonUICreated)
            {
                UpdateVisualConfig();
            }
        }

        #endregion

        #region 手动控制的UI创建

        /// <summary>
        /// 手动创建按钮UI界面
        /// </summary>
        [ContextMenu("创建按钮UI")]
        public void CreateButtonUI()
        {
            if (buttonUICreated)
            {
                if (enableDebugMode)
                    Debug.Log($"[BankerPlayerButton] {betType} UI已存在，跳过创建");
                return;
            }

            try
            {
                // 确保组件已初始化
                if (rectTransform == null)
                    InitializeComponent();

                // 创建UI组件（只在未创建时创建）
                CreateBackgroundImage();
                CreateButtonComponent();
                CreateTextComponents();
                
                // 应用配置
                UpdateVisualConfig();
                
                // 初始化显示数据
                UpdateDisplay(0, 0m);

                buttonUICreated = true;
                
                if (enableDebugMode)
                    Debug.Log($"[BankerPlayerButton] {betType} 按钮UI创建完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BankerPlayerButton] {betType} 创建按钮UI时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建背景图片组件
        /// </summary>
        private void CreateBackgroundImage()
        {
            // 检查是否已存在
            backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
                backgroundImage.sprite = CreateRoundedRectSprite();
            }

            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] {betType} 背景图片组件准备完成");
        }

        /// <summary>
        /// 创建按钮组件
        /// </summary>
        private void CreateButtonComponent()
        {
            // 检查是否已存在
            button = GetComponent<Button>();
            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
                button.onClick.AddListener(HandleButtonClick);
            }
            else
            {
                // 清除旧的监听器，重新添加
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(HandleButtonClick);
            }

            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] {betType} 按钮组件准备完成");
        }

        /// <summary>
        /// 创建文本组件
        /// </summary>
        private void CreateTextComponents()
        {
            // 创建标题文本（投注类型）
            CreateTitleText();
            
            // 创建赔率文本
            CreateOddsText();
            
            // 创建投注人数文本
            CreatePlayerCountText();
            
            // 创建投注金额文本
            CreateAmountText();

            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] {betType} 文本组件创建完成");
        }

        /// <summary>
        /// 创建标题文本
        /// </summary>
        private void CreateTitleText()
        {
            GameObject titleObj = FindOrCreateChild("TitleText");
            titleText = GetOrAddComponent<Text>(titleObj);
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.6f);
            titleRect.anchorMax = new Vector2(0.6f, 0.9f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            titleRect.localScale = Vector3.one;

            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = titleFontSize;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
        }

        /// <summary>
        /// 创建赔率文本
        /// </summary>
        private void CreateOddsText()
        {
            GameObject oddsObj = FindOrCreateChild("OddsText");
            oddsText = GetOrAddComponent<Text>(oddsObj);
            
            RectTransform oddsRect = oddsObj.GetComponent<RectTransform>();
            oddsRect.anchorMin = new Vector2(0.6f, 0.6f);
            oddsRect.anchorMax = new Vector2(0.9f, 0.9f);
            oddsRect.offsetMin = Vector2.zero;
            oddsRect.offsetMax = Vector2.zero;
            oddsRect.localScale = Vector3.one;

            oddsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            oddsText.fontSize = oddsFontSize;
            oddsText.fontStyle = FontStyle.Normal;
            oddsText.alignment = TextAnchor.MiddleCenter;
        }

        /// <summary>
        /// 创建投注人数文本
        /// </summary>
        private void CreatePlayerCountText()
        {
            GameObject countObj = FindOrCreateChild("PlayerCountText");
            playerCountText = GetOrAddComponent<Text>(countObj);
            
            RectTransform countRect = countObj.GetComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0.1f, 0.3f);
            countRect.anchorMax = new Vector2(0.5f, 0.6f);
            countRect.offsetMin = Vector2.zero;
            countRect.offsetMax = Vector2.zero;
            countRect.localScale = Vector3.one;

            playerCountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            playerCountText.fontSize = numberFontSize;
            playerCountText.fontStyle = FontStyle.Normal;
            playerCountText.alignment = TextAnchor.MiddleLeft;
        }

        /// <summary>
        /// 创建投注金额文本
        /// </summary>
        private void CreateAmountText()
        {
            GameObject amountObj = FindOrCreateChild("AmountText");
            amountText = GetOrAddComponent<Text>(amountObj);
            
            RectTransform amountRect = amountObj.GetComponent<RectTransform>();
            amountRect.anchorMin = new Vector2(0.5f, 0.3f);
            amountRect.anchorMax = new Vector2(0.9f, 0.6f);
            amountRect.offsetMin = Vector2.zero;
            amountRect.offsetMax = Vector2.zero;
            amountRect.localScale = Vector3.one;

            amountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            amountText.fontSize = numberFontSize;
            amountText.fontStyle = FontStyle.Bold;
            amountText.alignment = TextAnchor.MiddleRight;
        }

        /// <summary>
        /// 查找或创建子对象
        /// </summary>
        private GameObject FindOrCreateChild(string childName)
        {
            Transform child = transform.Find(childName);
            if (child == null)
            {
                GameObject childObj = new GameObject(childName);
                childObj.transform.SetParent(transform);
                return childObj;
            }
            return child.gameObject;
        }

        /// <summary>
        /// 获取或添加组件
        /// </summary>
        private T GetOrAddComponent<T>(GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component == null)
            {
                component = obj.AddComponent<T>();
            }
            return component;
        }

        #endregion

        #region 视觉配置更新

        /// <summary>
        /// 更新视觉配置
        /// </summary>
        private void UpdateVisualConfig()
        {
            if (currentConfig == null) return;

            // 更新背景颜色
            if (backgroundImage != null)
            {
                backgroundImage.color = currentConfig.buttonColor;
            }

            // 更新按钮颜色状态
            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = currentConfig.buttonColor;
                colors.highlightedColor = Color.Lerp(currentConfig.buttonColor, Color.white, 0.2f);
                colors.pressedColor = Color.Lerp(currentConfig.buttonColor, Color.black, 0.2f);
                colors.disabledColor = Color.Lerp(currentConfig.buttonColor, Color.gray, 0.5f);
                colors.colorMultiplier = 1f;
                colors.fadeDuration = 0.1f;
                button.colors = colors;
            }

            // 更新文本内容和颜色
            if (titleText != null)
            {
                titleText.text = currentConfig.displayTitle;
                titleText.color = currentConfig.textColor;
            }

            if (oddsText != null)
            {
                oddsText.text = currentConfig.odds;
                oddsText.color = currentConfig.textColor;
            }

            if (playerCountText != null)
            {
                playerCountText.color = currentConfig.numberColor;
            }

            if (amountText != null)
            {
                amountText.color = currentConfig.numberColor;
            }

            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] {betType} 视觉配置更新完成");
        }

        #endregion

        #region 按钮控制逻辑

        /// <summary>
        /// 更新显示数据
        /// </summary>
        public void UpdateDisplay(int playerCount, decimal amount)
        {
            currentPlayerCount = playerCount;
            currentAmount = amount;
            
            // 更新人数显示
            if (playerCountText != null)
            {
                playerCountText.text = playerCount > 0 ? $"👥{playerCount}" : "";
            }
            
            // 更新金额显示
            if (amountText != null)
            {
                amountText.text = amount > 0 ? $"¥{FormatAmount(amount)}" : "";
            }
            
            // 触发数据更新事件
            OnBetDataUpdated?.Invoke(betType, playerCount, amount);
            
            // 添加数据变化动画
            if (playerCount > 0 || amount > 0)
            {
                TriggerUpdateAnimation();
            }
            
            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] {betType} 更新显示: {playerCount}人, ¥{amount}");
        }

        /// <summary>
        /// 设置交互状态
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
            
            if (button != null)
                button.interactable = interactable;
            
            // 更新视觉状态
            UpdateInteractableState();
            
            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] {betType} 设置交互状态: {interactable}");
        }

        /// <summary>
        /// 设置按钮类型
        /// </summary>
        public void SetBetType(BaccaratBetType newBetType)
        {
            if (betType == newBetType) return;

            betType = newBetType;
            UpdateButtonConfig();
            
            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] 按钮类型更新为: {betType}");
        }

        /// <summary>
        /// 重置显示数据
        /// </summary>
        public void ResetDisplay()
        {
            UpdateDisplay(0, 0m);
        }

        /// <summary>
        /// 设置按钮高亮
        /// </summary>
        public void SetHighlight(bool highlight)
        {
            if (backgroundImage != null && currentConfig != null)
            {
                Color targetColor = highlight ? 
                    Color.Lerp(currentConfig.buttonColor, Color.white, 0.3f) : 
                    currentConfig.buttonColor;
                backgroundImage.color = targetColor;
            }
        }

        #endregion

        #region 动画系统

        /// <summary>
        /// 缩放动画协程
        /// </summary>
        private IEnumerator ScaleAnimation(Vector3 targetScale, float duration)
        {
            Vector3 startScale = transform.localScale;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsedTime / duration);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            transform.localScale = targetScale;
        }

        /// <summary>
        /// 点击动画
        /// </summary>
        private IEnumerator ClickAnimationCoroutine()
        {
            // 缩小
            yield return StartCoroutine(ScaleAnimation(Vector3.one * 0.95f, 0.1f));
            // 恢复
            yield return StartCoroutine(ScaleAnimation(Vector3.one, 0.1f));
        }

        /// <summary>
        /// 数据更新闪烁动画
        /// </summary>
        private IEnumerator FlashAnimation()
        {
            if (amountText == null) yield break;

            Color originalColor = amountText.color;
            Color flashColor = Color.white;
            
            // 闪烁效果
            for (int i = 0; i < 2; i++)
            {
                amountText.color = flashColor;
                yield return new WaitForSeconds(0.1f);
                amountText.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }
        }

        /// <summary>
        /// 触发点击动画
        /// </summary>
        public void TriggerClickAnimation()
        {
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);
            
            currentAnimation = StartCoroutine(ClickAnimationCoroutine());
        }

        /// <summary>
        /// 触发数据更新动画
        /// </summary>
        private void TriggerUpdateAnimation()
        {
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);
            
            currentAnimation = StartCoroutine(FlashAnimation());
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 处理按钮点击
        /// </summary>
        private void HandleButtonClick()
        {
            if (!isInteractable) return;
            
            // 播放点击动画
            TriggerClickAnimation();
            
            // 触发回调
            OnButtonClicked?.Invoke();
            OnBetTypeSelected?.Invoke(betType);
            
            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] {betType} 按钮被点击");
        }

        /// <summary>
        /// 处理指针点击事件
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            // 由Button组件处理，这里可以添加额外逻辑
        }

        /// <summary>
        /// 鼠标进入事件
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isInteractable) return;
            
            // 轻微放大
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);
            
            currentAnimation = StartCoroutine(ScaleAnimation(Vector3.one * 1.05f, 0.2f));
        }

        /// <summary>
        /// 鼠标离开事件
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isInteractable) return;
            
            // 恢复大小
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);
            
            currentAnimation = StartCoroutine(ScaleAnimation(Vector3.one, 0.2f));
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 格式化金额显示
        /// </summary>
        private string FormatAmount(decimal amount)
        {
            if (amount >= 10000)
                return $"{amount / 10000:F1}万";
            else if (amount >= 1000)
                return $"{amount / 1000:F1}K";
            else
                return amount.ToString("F0");
        }

        /// <summary>
        /// 更新交互状态的视觉效果
        /// </summary>
        private void UpdateInteractableState()
        {
            float alpha = isInteractable ? 1f : 0.5f;
            
            if (titleText != null)
            {
                Color color = titleText.color;
                color.a = alpha;
                titleText.color = color;
            }
            
            if (oddsText != null)
            {
                Color color = oddsText.color;
                color.a = alpha;
                oddsText.color = color;
            }
            
            if (playerCountText != null)
            {
                Color color = playerCountText.color;
                color.a = alpha;
                playerCountText.color = color;
            }
            
            if (amountText != null)
            {
                Color color = amountText.color;
                color.a = alpha;
                amountText.color = color;
            }
        }

        /// <summary>
        /// 创建圆角矩形Sprite
        /// </summary>
        private Sprite CreateRoundedRectSprite()
        {
            int width = 64;
            int height = 64;
            int cornerRadius = 8;
            
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bool isInside = IsInsideRoundedRect(x, y, width, height, cornerRadius);
                    Color pixelColor = isInside ? Color.white : Color.clear;
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(cornerRadius, cornerRadius, cornerRadius, cornerRadius));
        }

        /// <summary>
        /// 判断点是否在圆角矩形内
        /// </summary>
        private bool IsInsideRoundedRect(int x, int y, int width, int height, int radius)
        {
            int cornerX = Mathf.Clamp(x, radius, width - radius);
            int cornerY = Mathf.Clamp(y, radius, height - radius);
            
            int dx = x - cornerX;
            int dy = y - cornerY;
            
            return dx * dx + dy * dy <= radius * radius;
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 获取当前投注类型
        /// </summary>
        public BaccaratBetType GetBetType()
        {
            return betType;
        }

        /// <summary>
        /// 获取当前投注数据
        /// </summary>
        public (int playerCount, decimal amount) GetCurrentData()
        {
            return (currentPlayerCount, currentAmount);
        }

        /// <summary>
        /// 获取按钮是否已创建UI
        /// </summary>
        public bool IsUICreated()
        {
            return buttonUICreated;
        }

        /// <summary>
        /// 强制重新创建UI
        /// </summary>
        [ContextMenu("重新创建UI")]
        public void RecreateUI()
        {
            ClearUI();
            CreateButtonUI();
        }

        /// <summary>
        /// 清除UI组件
        /// </summary>
        [ContextMenu("清除UI")]
        public void ClearUI()
        {
            // 清除子对象
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
            
            // 清除动态添加的组件
            var componentsToRemove = new System.Type[] { typeof(Image), typeof(Button) };
            foreach (var componentType in componentsToRemove)
            {
                Component component = GetComponent(componentType);
                if (component != null)
                {
                    if (Application.isPlaying)
                        Destroy(component);
                    else
                        DestroyImmediate(component);
                }
            }
            
            // 清空引用
            titleText = null;
            oddsText = null;
            playerCountText = null;
            amountText = null;
            backgroundImage = null;
            button = null;
            
            buttonUICreated = false;
            
            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] {betType} UI已清除");
        }

        #endregion

        #region 调试方法

        /// <summary>
        /// 显示组件状态
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowComponentStatus()
        {
            Debug.Log($"=== BankerPlayerButton {betType} 组件状态 ===");
            Debug.Log($"按钮UI已创建: {buttonUICreated}");
            Debug.Log($"是否可交互: {isInteractable}");
            Debug.Log($"当前配置: {currentConfig?.displayTitle} - {currentConfig?.odds}");
            Debug.Log($"背景图片: {(backgroundImage != null ? "✓" : "✗")}");
            Debug.Log($"按钮组件: {(button != null ? "✓" : "✗")}");
            Debug.Log($"标题文本: {(titleText != null ? "✓" : "✗")} - {titleText?.text}");
            Debug.Log($"赔率文本: {(oddsText != null ? "✓" : "✗")} - {oddsText?.text}");
            Debug.Log($"人数文本: {(playerCountText != null ? "✓" : "✗")} - {playerCountText?.text}");
            Debug.Log($"金额文本: {(amountText != null ? "✓" : "✗")} - {amountText?.text}");
            Debug.Log($"当前投注人数: {currentPlayerCount}");
            Debug.Log($"当前投注金额: {currentAmount}");
        }

        /// <summary>
        /// 测试按钮功能
        /// </summary>
        [ContextMenu("测试按钮功能")]
        public void TestButtonFunctions()
        {
            Debug.Log($"[BankerPlayerButton] {betType} 开始测试按钮功能");
            
            // 如果UI未创建，先创建
            if (!buttonUICreated)
            {
                CreateButtonUI();
            }
            
            // 测试数据更新
            UpdateDisplay(5, 1000m);
            
            // 测试交互状态
            SetInteractable(false);
            SetInteractable(true);
            
            // 测试点击动画
            TriggerClickAnimation();
            
            Debug.Log($"[BankerPlayerButton] {betType} 按钮功能测试完成");
        }

        #endregion
    }
}
// Assets/UI/Components/BettingArea/BankerPlayerButton.cs
// 庄闲和投注按钮组件 - 启动即显示版本
// 自动创建并立即显示投注按钮界面
// 创建时间: 2025/6/26

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BaccaratGame.Data;

namespace BaccaratGame.UI.Components
{
    /// <summary>
    /// 庄闲和投注按钮组件 - 启动即显示版本
    /// 组件启动时立即创建并显示投注按钮界面
    /// </summary>
    public class BankerPlayerButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        #region 序列化字段

        [Header("自动显示设置")]
        public bool autoCreateAndShow = false;
        public bool showOnAwake = false;
        public bool immediateDisplay = false;

        [Header("按钮配置")]
        public BaccaratBetType betType = BaccaratBetType.Player;
        public string displayTitle = "闲";
        public string odds = "1:1";
        
        [Header("按钮布局")]
        public Vector2 buttonSize = new Vector2(150, 100);
        public Vector2 buttonPosition = Vector2.zero;

        [Header("UI样式")]
        public Color normalColor = new Color(0.2f, 0.6f, 1f, 1f);
        public Color highlightColor = new Color(0.3f, 0.7f, 1f, 1f);
        public Color pressedColor = new Color(0.1f, 0.5f, 0.9f, 1f);
        public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        public Color textColor = Color.white;
        public Color numberColor = Color.yellow;
        
        [Header("字体设置")]
        public int titleFontSize = 18;
        public int oddsFontSize = 14;
        public int numberFontSize = 12;

        [Header("现有组件引用 (可选)")]
        public Text titleText;
        public Text oddsText;
        public Text playerCountText;
        public Text amountText;
        public Image backgroundImage;
        public Button button;

        [Header("调试设置")]
        public bool enableDebugMode = true;

        #endregion

        #region 动画协程

        /// <summary>
        /// 缩放动画协程
        /// </summary>
        private System.Collections.IEnumerator ScaleAnimation(Vector3 targetScale, float duration)
        {
            Vector3 startScale = transform.localScale;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                t = Mathf.SmoothStep(0, 1, t); // 平滑插值
                
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            transform.localScale = targetScale;
        }

        /// <summary>
        /// 点击动画协程
        /// </summary>
        private System.Collections.IEnumerator ClickAnimationCoroutine()
        {
            // 缩小
            yield return StartCoroutine(ScaleAnimation(Vector3.one * 0.95f, 0.1f));
            // 恢复
            yield return StartCoroutine(ScaleAnimation(Vector3.one, 0.1f));
        }

        /// <summary>
        /// 闪烁动画协程
        /// </summary>
        private System.Collections.IEnumerator FlashAnimation(GameObject target)
        {
            Image[] images = target.GetComponentsInChildren<Image>();
            Text[] texts = target.GetComponentsInChildren<Text>();
            
            // 存储原始透明度
            float[] originalImageAlphas = new float[images.Length];
            float[] originalTextAlphas = new float[texts.Length];
            
            for (int i = 0; i < images.Length; i++)
                originalImageAlphas[i] = images[i].color.a;
            for (int i = 0; i < texts.Length; i++)
                originalTextAlphas[i] = texts[i].color.a;

            // 闪烁效果
            float duration = 0.2f;
            float elapsedTime = 0;

            // 变暗
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0.5f, elapsedTime / duration);
                
                for (int i = 0; i < images.Length; i++)
                {
                    Color color = images[i].color;
                    color.a = originalImageAlphas[i] * alpha;
                    images[i].color = color;
                }
                
                for (int i = 0; i < texts.Length; i++)
                {
                    Color color = texts[i].color;
                    color.a = originalTextAlphas[i] * alpha;
                    texts[i].color = color;
                }
                
                yield return null;
            }

            // 恢复
            elapsedTime = 0;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0.5f, 1f, elapsedTime / duration);
                
                for (int i = 0; i < images.Length; i++)
                {
                    Color color = images[i].color;
                    color.a = originalImageAlphas[i] * alpha;
                    images[i].color = color;
                }
                
                for (int i = 0; i < texts.Length; i++)
                {
                    Color color = texts[i].color;
                    color.a = originalTextAlphas[i] * alpha;
                    texts[i].color = color;
                }
                
                yield return null;
            }

            // 确保恢复到原始状态
            for (int i = 0; i < images.Length; i++)
            {
                Color color = images[i].color;
                color.a = originalImageAlphas[i];
                images[i].color = color;
            }
            
            for (int i = 0; i < texts.Length; i++)
            {
                Color color = texts[i].color;
                color.a = originalTextAlphas[i];
                texts[i].color = color;
            }
        }

        #endregion

        #region 私有字段

        private RectTransform rectTransform;
        private Canvas parentCanvas;
        private bool buttonUICreated = false;
        private bool isInteractable = true;
        private int currentPlayerCount = 0;
        private decimal currentAmount = 0m;
        
        // UI组件引用
        private GameObject mainButton;
        private GameObject titleArea;
        private GameObject oddsArea;
        private GameObject statsArea;

        #endregion

        #region 事件定义

        // 事件回调
        public System.Action OnButtonClicked;
        public System.Action<BaccaratBetType> OnBetTypeSelected;

        #endregion

        #region 生命周期

        private void Awake()
        {
            InitializeComponent();
            
            if (showOnAwake)
            {
                CreateAndShowButton();
            }
        }

        private void Start()
        {
            if (!buttonUICreated && autoCreateAndShow)
            {
                CreateAndShowButton();
            }
            
            SetupExistingComponents();
            UpdateInitialDisplay();
        }

        private void OnValidate()
        {
            // 在编辑器中实时预览
            if (Application.isEditor && !Application.isPlaying)
            {
                if (immediateDisplay)
                {
                    InitializeComponent();
                    CreateAndShowButton();
                }
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
            if (parentCanvas == null)
            {
                CreateCanvasIfNeeded();
            }

            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] {betType} 组件初始化完成");
        }

        /// <summary>
        /// 如需要则创建Canvas
        /// </summary>
        private void CreateCanvasIfNeeded()
        {
            GameObject canvasObj = new GameObject("BettingCanvas");
            canvasObj.transform.SetParent(transform.parent);
            
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
            
            // 将BankerPlayerButton移到Canvas下
            transform.SetParent(canvasObj.transform);
            
            parentCanvas = canvas;
            
            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] {betType} 创建了新的Canvas");
        }

        #endregion

        #region 按钮界面创建

        /// <summary>
        /// 创建并显示按钮界面
        /// </summary>
        [ContextMenu("创建并显示按钮界面")]
        public void CreateAndShowButton()
        {
            if (buttonUICreated) return;

            try
            {
                // 确保组件已初始化
                if (rectTransform == null)
                    InitializeComponent();

                // 创建主按钮
                CreateMainButton();
                
                // 创建标题区域
                CreateTitleArea();
                
                // 创建赔率区域
                CreateOddsArea();
                
                // 创建统计区域
                CreateStatsArea();
                
                // 添加动画效果
                AddButtonAnimation();

                buttonUICreated = true;
                
                if (enableDebugMode)
                    Debug.Log($"[BankerPlayerButton] {betType} 按钮界面创建完成并已显示");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BankerPlayerButton] {betType} 创建按钮界面时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建主按钮
        /// </summary>
        private void CreateMainButton()
        {
            // 使用当前GameObject作为主按钮
            mainButton = gameObject;

            // 添加背景图片
            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
                backgroundImage.color = GetBetTypeColor();
                backgroundImage.sprite = CreateRoundedRectSprite();
            }

            // 添加Button组件
            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
                
                // 设置按钮颜色状态
                ColorBlock colors = button.colors;
                colors.normalColor = GetBetTypeColor();
                colors.highlightedColor = highlightColor;
                colors.pressedColor = pressedColor;
                colors.disabledColor = disabledColor;
                colors.colorMultiplier = 1f;
                colors.fadeDuration = 0.1f;
                button.colors = colors;
                
                // 设置点击事件
                button.onClick.AddListener(() => OnButtonClicked?.Invoke());
            }

            // 添加阴影效果
            Shadow shadow = gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.3f);
            shadow.effectDistance = new Vector2(3, -3);

            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] {betType} 主按钮创建完成");
        }

        /// <summary>
        /// 创建标题区域
        /// </summary>
        private void CreateTitleArea()
        {
            GameObject titleAreaObj = new GameObject("TitleArea");
            titleAreaObj.transform.SetParent(transform);

            RectTransform titleAreaRect = titleAreaObj.AddComponent<RectTransform>();
            titleAreaRect.anchorMin = new Vector2(0, 0.6f);
            titleAreaRect.anchorMax = new Vector2(1, 1);
            titleAreaRect.offsetMin = new Vector2(5, 0);
            titleAreaRect.offsetMax = new Vector2(-5, -5);
            titleAreaRect.localScale = Vector3.one;

            titleArea = titleAreaObj;

            // 创建标题文本
            CreateTitleText(titleAreaObj);

            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] {betType} 标题区域创建完成");
        }

        /// <summary>
        /// 创建标题文本
        /// </summary>
        private void CreateTitleText(GameObject parent)
        {
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(parent.transform);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            titleRect.localScale = Vector3.one;

            if (titleText == null)
            {
                titleText = titleObj.AddComponent<Text>();
                titleText.text = displayTitle;
                titleText.color = textColor;
                titleText.alignment = TextAnchor.MiddleCenter;
                titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                titleText.fontSize = titleFontSize;
                titleText.fontStyle = FontStyle.Bold;
            }
        }

        /// <summary>
        /// 创建赔率区域
        /// </summary>
        private void CreateOddsArea()
        {
            GameObject oddsAreaObj = new GameObject("OddsArea");
            oddsAreaObj.transform.SetParent(transform);

            RectTransform oddsAreaRect = oddsAreaObj.AddComponent<RectTransform>();
            oddsAreaRect.anchorMin = new Vector2(0, 0.4f);
            oddsAreaRect.anchorMax = new Vector2(1, 0.6f);
            oddsAreaRect.offsetMin = new Vector2(5, 0);
            oddsAreaRect.offsetMax = new Vector2(-5, 0);
            oddsAreaRect.localScale = Vector3.one;

            oddsArea = oddsAreaObj;

            // 创建赔率文本
            CreateOddsText(oddsAreaObj);

            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] {betType} 赔率区域创建完成");
        }

        /// <summary>
        /// 创建赔率文本
        /// </summary>
        private void CreateOddsText(GameObject parent)
        {
            GameObject oddsObj = new GameObject("OddsText");
            oddsObj.transform.SetParent(parent.transform);

            RectTransform oddsRect = oddsObj.AddComponent<RectTransform>();
            oddsRect.anchorMin = Vector2.zero;
            oddsRect.anchorMax = Vector2.one;
            oddsRect.offsetMin = Vector2.zero;
            oddsRect.offsetMax = Vector2.zero;
            oddsRect.localScale = Vector3.one;

            if (oddsText == null)
            {
                oddsText = oddsObj.AddComponent<Text>();
                oddsText.text = odds;
                oddsText.color = numberColor;
                oddsText.alignment = TextAnchor.MiddleCenter;
                oddsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                oddsText.fontSize = oddsFontSize;
                oddsText.fontStyle = FontStyle.Normal;
            }
        }

        /// <summary>
        /// 创建统计区域
        /// </summary>
        private void CreateStatsArea()
        {
            GameObject statsAreaObj = new GameObject("StatsArea");
            statsAreaObj.transform.SetParent(transform);

            RectTransform statsAreaRect = statsAreaObj.AddComponent<RectTransform>();
            statsAreaRect.anchorMin = new Vector2(0, 0);
            statsAreaRect.anchorMax = new Vector2(1, 0.4f);
            statsAreaRect.offsetMin = new Vector2(5, 5);
            statsAreaRect.offsetMax = new Vector2(-5, 0);
            statsAreaRect.localScale = Vector3.one;

            statsArea = statsAreaObj;

            // 创建投注人数文本
            CreatePlayerCountText(statsAreaObj);
            
            // 创建投注金额文本
            CreateAmountText(statsAreaObj);

            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] {betType} 统计区域创建完成");
        }

        /// <summary>
        /// 创建投注人数文本
        /// </summary>
        private void CreatePlayerCountText(GameObject parent)
        {
            GameObject countObj = new GameObject("PlayerCountText");
            countObj.transform.SetParent(parent.transform);

            RectTransform countRect = countObj.AddComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0, 0.5f);
            countRect.anchorMax = new Vector2(1, 1);
            countRect.offsetMin = Vector2.zero;
            countRect.offsetMax = Vector2.zero;
            countRect.localScale = Vector3.one;

            if (playerCountText == null)
            {
                playerCountText = countObj.AddComponent<Text>();
                playerCountText.text = "";
                playerCountText.color = textColor;
                playerCountText.alignment = TextAnchor.MiddleCenter;
                playerCountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                playerCountText.fontSize = numberFontSize;
                playerCountText.fontStyle = FontStyle.Normal;
            }
        }

        /// <summary>
        /// 创建投注金额文本
        /// </summary>
        private void CreateAmountText(GameObject parent)
        {
            GameObject amountObj = new GameObject("AmountText");
            amountObj.transform.SetParent(parent.transform);

            RectTransform amountRect = amountObj.AddComponent<RectTransform>();
            amountRect.anchorMin = new Vector2(0, 0);
            amountRect.anchorMax = new Vector2(1, 0.5f);
            amountRect.offsetMin = Vector2.zero;
            amountRect.offsetMax = Vector2.zero;
            amountRect.localScale = Vector3.one;

            if (amountText == null)
            {
                amountText = amountObj.AddComponent<Text>();
                amountText.text = "";
                amountText.color = numberColor;
                amountText.alignment = TextAnchor.MiddleCenter;
                amountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                amountText.fontSize = numberFontSize;
                amountText.fontStyle = FontStyle.Bold;
            }
        }

        /// <summary>
        /// 添加按钮动画
        /// </summary>
        private void AddButtonAnimation()
        {
            // 简化动画处理，不使用AnimatorController
            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] {betType} 动画组件准备完成");
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
            // 计算到最近角的距离
            int cornerX = Mathf.Clamp(x, radius, width - radius);
            int cornerY = Mathf.Clamp(y, radius, height - radius);
            
            int dx = x - cornerX;
            int dy = y - cornerY;
            
            return dx * dx + dy * dy <= radius * radius;
        }

        #endregion

        #region 现有组件设置

        /// <summary>
        /// 设置现有组件
        /// </summary>
        private void SetupExistingComponents()
        {
            // 如果有现有的组件引用，设置它们
            if (button != null && !buttonUICreated)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnButtonClicked?.Invoke());
            }

            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] {betType} 现有组件设置完成");
        }

        /// <summary>
        /// 更新初始显示
        /// </summary>
        private void UpdateInitialDisplay()
        {
            // 设置按钮标题和赔率
            if (titleText != null) 
                titleText.text = displayTitle;
            if (oddsText != null) 
                oddsText.text = odds;
            
            // 初始化显示数据
            UpdateDisplay(0, 0m);
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
                playerCountText.text = playerCount > 0 ? $"{playerCount}人" : "";
            }
            
            // 更新金额显示
            if (amountText != null)
            {
                amountText.text = amount > 0 ? FormatAmount(amount) : "";
            }
            
            // 添加数据变化动画
            if (playerCount > 0 || amount > 0)
            {
                TriggerUpdateAnimation();
            }
            
            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] {betType} 更新显示: {playerCount}人, {amount}");
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
            UpdateVisualState();
            
            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] {betType} 设置交互状态: {interactable}");
        }

        /// <summary>
        /// 设置按钮配置
        /// </summary>
        public void SetButtonConfig(BaccaratBetType newBetType, string title, string newOdds)
        {
            betType = newBetType;
            displayTitle = title;
            odds = newOdds;
            
            // 更新显示
            if (titleText != null) titleText.text = title;
            if (oddsText != null) oddsText.text = newOdds;
            
            // 更新颜色
            if (backgroundImage != null && button != null)
            {
                Color newColor = GetBetTypeColor();
                backgroundImage.color = newColor;
                
                ColorBlock colors = button.colors;
                colors.normalColor = newColor;
                button.colors = colors;
            }
            
            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] 配置更新: {betType} - {title} - {newOdds}");
        }

        /// <summary>
        /// 触发点击动画
        /// </summary>
        public void TriggerClickAnimation()
        {
            // 使用协程替代LeanTween
            StartCoroutine(ClickAnimationCoroutine());
        }

        /// <summary>
        /// 触发数据更新动画
        /// </summary>
        private void TriggerUpdateAnimation()
        {
            if (statsArea != null)
            {
                // 使用协程实现闪烁动画
                StartCoroutine(FlashAnimation(statsArea));
            }
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 处理指针点击事件
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isInteractable)
            {
                if (enableDebugMode)
                    Debug.Log($"[BankerPlayerButton] {betType} 按钮不可交互，忽略点击");
                return;
            }
            
            // 播放点击动画
            TriggerClickAnimation();
            
            // 触发回调
            OnButtonClicked?.Invoke();
            OnBetTypeSelected?.Invoke(betType);
            
            if (enableDebugMode)
                Debug.Log($"[BankerPlayerButton] {betType} 按钮被点击");
        }

        /// <summary>
        /// 鼠标进入事件
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isInteractable) return;
            
            // 悬停效果
            if (backgroundImage != null)
            {
                backgroundImage.color = highlightColor;
            }
            
            // 轻微放大 - 使用协程替代LeanTween
            StartCoroutine(ScaleAnimation(Vector3.one * 1.05f, 0.2f));
        }

        /// <summary>
        /// 鼠标离开事件
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isInteractable) return;
            
            // 恢复颜色
            if (backgroundImage != null)
            {
                backgroundImage.color = GetBetTypeColor();
            }
            
            // 恢复大小
            StartCoroutine(ScaleAnimation(Vector3.one, 0.2f));
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取投注类型对应的颜色
        /// </summary>
        private Color GetBetTypeColor()
        {
            return betType switch
            {
                BaccaratBetType.Banker => new Color(1f, 0.2f, 0.2f, 1f), // 庄-红色
                BaccaratBetType.Player => new Color(0.2f, 0.2f, 1f, 1f), // 闲-蓝色
                BaccaratBetType.Tie => new Color(0.2f, 1f, 0.2f, 1f),    // 和-绿色
                _ => normalColor
            };
        }

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
        /// 更新视觉状态
        /// </summary>
        private void UpdateVisualState()
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = isInteractable ? GetBetTypeColor() : disabledColor;
            }
            
            // 更新文本透明度
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

        #endregion

        #region 公共接口

        /// <summary>
        /// 强制显示按钮
        /// </summary>
        [ContextMenu("强制显示按钮")]
        public void ForceShowButton()
        {
            buttonUICreated = false;
            CreateAndShowButton();
        }

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
            if (backgroundImage != null)
            {
                Color targetColor = highlight ? highlightColor : GetBetTypeColor();
                backgroundImage.color = targetColor;
            }
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
            Debug.Log($"自动创建并显示: {autoCreateAndShow}");
            Debug.Log($"启动时显示: {showOnAwake}");
            Debug.Log($"立即显示: {immediateDisplay}");
            Debug.Log($"按钮UI已创建: {buttonUICreated}");
            Debug.Log($"是否可交互: {isInteractable}");
            Debug.Log($"父Canvas: {(parentCanvas != null ? "✓" : "✗")}");
            Debug.Log($"主按钮: {(mainButton != null ? "✓" : "✗")}");
            Debug.Log($"标题文本: {(titleText != null ? "✓" : "✗")} - {titleText?.text}");
            Debug.Log($"赔率文本: {(oddsText != null ? "✓" : "✗")} - {oddsText?.text}");
            Debug.Log($"人数文本: {(playerCountText != null ? "✓" : "✗")} - {playerCountText?.text}");
            Debug.Log($"金额文本: {(amountText != null ? "✓" : "✗")} - {amountText?.text}");
            Debug.Log($"背景图片: {(backgroundImage != null ? "✓" : "✗")}");
            Debug.Log($"按钮组件: {(button != null ? "✓" : "✗")}");
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
            
            // 测试数据更新
            UpdateDisplay(5, 1000m);
            System.Threading.Thread.Sleep(500);
            
            // 测试交互状态
            SetInteractable(false);
            System.Threading.Thread.Sleep(500);
            SetInteractable(true);
            
            // 测试点击动画
            TriggerClickAnimation();
            
            Debug.Log($"[BankerPlayerButton] {betType} 按钮功能测试完成");
        }

        /// <summary>
        /// 删除所有创建的UI
        /// </summary>
        [ContextMenu("删除所有UI")]
        public void ClearAllUI()
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
            
            // 清除组件
            if (button != null && button != GetComponent<Button>())
            {
                if (Application.isPlaying)
                    Destroy(button);
                else
                    DestroyImmediate(button);
            }
            
            if (backgroundImage != null && backgroundImage != GetComponent<Image>())
            {
                if (Application.isPlaying)
                    Destroy(backgroundImage);
                else
                    DestroyImmediate(backgroundImage);
            }
            
            // 清空引用
            titleText = null;
            oddsText = null;
            playerCountText = null;
            amountText = null;
            backgroundImage = null;
            button = null;
            
            buttonUICreated = false;
            
            Debug.Log($"[BankerPlayerButton] {betType} 所有UI已删除");
        }

        #endregion
    }
}
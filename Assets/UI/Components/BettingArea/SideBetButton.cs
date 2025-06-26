// Assets/UI/Components/BettingArea/SideBetButton.cs
// 边注投注按钮组件 - 启动即显示版本
// 自动创建并立即显示边注按钮界面
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
    /// 边注投注按钮组件 - 启动即显示版本
    /// 龙7、庄对、幸运6、闲对、熊8等边注按钮
    /// 组件启动时立即创建并显示边注按钮界面
    /// </summary>
    public class SideBetButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        #region 序列化字段

        [Header("自动显示设置")]
        public bool autoCreateAndShow = false;
        public bool showOnAwake = false;
        public bool immediateDisplay = false;

        [Header("按钮配置")]
        public BaccaratBetType betType = BaccaratBetType.Dragon7;
        public string displayTitle = "龙7";
        public string odds = "1:40";
        
        [Header("按钮布局")]
        public Vector2 buttonSize = new Vector2(80, 60);
        public Vector2 buttonPosition = Vector2.zero;

        [Header("UI样式")]
        public Color normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        public Color betColor = new Color(0.2f, 0.8f, 0.2f, 1f);
        public Color highlightColor = new Color(1f, 1f, 0.8f, 1f);
        public Color pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        public Color indicatorColor = Color.green;
        public Color textColor = Color.black;
        public Color oddsColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        
        [Header("字体设置")]
        public int titleFontSize = 12;
        public int oddsFontSize = 10;

        [Header("现有组件引用 (可选)")]
        public Text titleText;
        public Text oddsText;
        public Image backgroundImage;
        public Image betIndicator;
        public Button button;

        [Header("调试设置")]
        public bool enableDebugMode = true;

        #endregion

        #region 私有字段

        private RectTransform rectTransform;
        private Canvas parentCanvas;
        private bool buttonUICreated = false;
        private bool isInteractable = true;
        private bool hasBet = false;
        
        // UI组件引用
        private GameObject mainButton;
        private GameObject titleArea;
        private GameObject oddsArea;
        private GameObject indicatorArea;

        #endregion

        #region 事件定义

        // 事件回调
        public System.Action OnButtonClicked;
        public System.Action<BaccaratBetType> OnSideBetSelected;
        public System.Action<BaccaratBetType, bool> OnBetStateChanged;

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
                Debug.Log($"[SideBetButton] {betType} 组件初始化完成");
        }

        /// <summary>
        /// 如需要则创建Canvas
        /// </summary>
        private void CreateCanvasIfNeeded()
        {
            GameObject canvasObj = new GameObject("SideBetCanvas");
            canvasObj.transform.SetParent(transform.parent);
            
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 35;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
            
            // 将SideBetButton移到Canvas下
            transform.SetParent(canvasObj.transform);
            
            parentCanvas = canvas;
            
            if (enableDebugMode)
                Debug.Log($"[SideBetButton] {betType} 创建了新的Canvas");
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
                
                // 创建投注指示器
                CreateBetIndicator();

                buttonUICreated = true;
                
                if (enableDebugMode)
                    Debug.Log($"[SideBetButton] {betType} 按钮界面创建完成并已显示");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SideBetButton] {betType} 创建按钮界面时出错: {ex.Message}");
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
                backgroundImage.color = normalColor;
                backgroundImage.sprite = CreateRoundedRectSprite();
            }

            // 添加Button组件
            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
                
                // 设置按钮颜色状态
                ColorBlock colors = button.colors;
                colors.normalColor = normalColor;
                colors.highlightedColor = highlightColor;
                colors.pressedColor = pressedColor;
                colors.disabledColor = disabledColor;
                colors.colorMultiplier = 1f;
                colors.fadeDuration = 0.1f;
                button.colors = colors;
                
                // 设置点击事件
                button.onClick.AddListener(() => OnButtonClicked?.Invoke());
            }

            // 添加边框效果
            Outline outline = gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.5f);
            outline.effectDistance = new Vector2(1, -1);

            if (enableDebugMode)
                Debug.Log($"[SideBetButton] {betType} 主按钮创建完成");
        }

        /// <summary>
        /// 创建标题区域
        /// </summary>
        private void CreateTitleArea()
        {
            GameObject titleAreaObj = new GameObject("TitleArea");
            titleAreaObj.transform.SetParent(transform);

            RectTransform titleAreaRect = titleAreaObj.AddComponent<RectTransform>();
            titleAreaRect.anchorMin = new Vector2(0, 0.5f);
            titleAreaRect.anchorMax = new Vector2(1, 1);
            titleAreaRect.offsetMin = new Vector2(2, 0);
            titleAreaRect.offsetMax = new Vector2(-2, -2);
            titleAreaRect.localScale = Vector3.one;

            titleArea = titleAreaObj;

            // 创建标题文本
            CreateTitleText(titleAreaObj);

            if (enableDebugMode)
                Debug.Log($"[SideBetButton] {betType} 标题区域创建完成");
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
            oddsAreaRect.anchorMin = new Vector2(0, 0);
            oddsAreaRect.anchorMax = new Vector2(1, 0.5f);
            oddsAreaRect.offsetMin = new Vector2(2, 2);
            oddsAreaRect.offsetMax = new Vector2(-2, 0);
            oddsAreaRect.localScale = Vector3.one;

            oddsArea = oddsAreaObj;

            // 创建赔率文本
            CreateOddsText(oddsAreaObj);

            if (enableDebugMode)
                Debug.Log($"[SideBetButton] {betType} 赔率区域创建完成");
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
                oddsText.color = oddsColor;
                oddsText.alignment = TextAnchor.MiddleCenter;
                oddsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                oddsText.fontSize = oddsFontSize;
                oddsText.fontStyle = FontStyle.Normal;
            }
        }

        /// <summary>
        /// 创建投注指示器
        /// </summary>
        private void CreateBetIndicator()
        {
            GameObject indicatorObj = new GameObject("BetIndicator");
            indicatorObj.transform.SetParent(transform);

            RectTransform indicatorRect = indicatorObj.AddComponent<RectTransform>();
            indicatorRect.anchorMin = new Vector2(1, 1);
            indicatorRect.anchorMax = new Vector2(1, 1);
            indicatorRect.sizeDelta = new Vector2(12, 12);
            indicatorRect.anchoredPosition = new Vector2(-6, -6);
            indicatorRect.localScale = Vector3.one;

            if (betIndicator == null)
            {
                betIndicator = indicatorObj.AddComponent<Image>();
                betIndicator.color = indicatorColor;
                betIndicator.sprite = CreateCircleSprite();
            }

            indicatorArea = indicatorObj;

            // 默认隐藏指示器
            indicatorObj.SetActive(false);

            if (enableDebugMode)
                Debug.Log($"[SideBetButton] {betType} 投注指示器创建完成");
        }

        /// <summary>
        /// 创建圆角矩形Sprite
        /// </summary>
        private Sprite CreateRoundedRectSprite()
        {
            int width = 32;
            int height = 32;
            int cornerRadius = 4;
            
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
        /// 创建圆形Sprite
        /// </summary>
        private Sprite CreateCircleSprite()
        {
            int size = 16;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 1;
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    Color pixelColor = distance <= radius ? Color.white : Color.clear;
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
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
                Debug.Log($"[SideBetButton] {betType} 现有组件设置完成");
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
            
            // 初始化显示状态
            UpdateDisplay(false);
        }

        #endregion

        #region 按钮控制逻辑

        /// <summary>
        /// 更新显示状态
        /// </summary>
        public void UpdateDisplay(bool hasBet)
        {
            this.hasBet = hasBet;
            
            // 更新背景颜色
            if (backgroundImage != null)
            {
                backgroundImage.color = hasBet ? betColor : normalColor;
            }
            
            // 更新投注指示器
            if (betIndicator != null)
            {
                betIndicator.gameObject.SetActive(hasBet);
                if (hasBet)
                {
                    StartCoroutine(IndicatorPulseEffect());
                }
            }
            
            // 更新按钮颜色
            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = hasBet ? betColor : normalColor;
                button.colors = colors;
            }
            
            // 触发状态变化事件
            OnBetStateChanged?.Invoke(betType, hasBet);
            
            if (enableDebugMode)
                Debug.Log($"[SideBetButton] {betType} 更新显示状态: 有投注={hasBet}");
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
                Debug.Log($"[SideBetButton] {betType} 设置交互状态: {interactable}");
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
            
            if (enableDebugMode)
                Debug.Log($"[SideBetButton] 配置更新: {betType} - {title} - {newOdds}");
        }

        /// <summary>
        /// 触发特殊效果
        /// </summary>
        public void TriggerSpecialEffect()
        {
            StartCoroutine(SpecialGlowEffect());
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
                    Debug.Log($"[SideBetButton] {betType} 按钮不可交互，忽略点击");
                return;
            }
            
            // 播放点击动画
            StartCoroutine(ClickAnimation());
            
            // 触发回调
            OnButtonClicked?.Invoke();
            OnSideBetSelected?.Invoke(betType);
            
            if (enableDebugMode)
                Debug.Log($"[SideBetButton] {betType} 按钮被点击");
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
            
            // 轻微放大
            StartCoroutine(ScaleAnimation(Vector3.one * 1.1f, 0.2f));
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
                backgroundImage.color = hasBet ? betColor : normalColor;
            }
            
            // 恢复大小
            StartCoroutine(ScaleAnimation(Vector3.one, 0.2f));
        }

        #endregion

        #region 动画效果

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
                float t = elapsedTime / duration;
                t = Mathf.SmoothStep(0, 1, t);
                
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            transform.localScale = targetScale;
        }

        /// <summary>
        /// 点击动画协程
        /// </summary>
        private IEnumerator ClickAnimation()
        {
            // 快速缩小
            yield return StartCoroutine(ScaleAnimation(Vector3.one * 0.9f, 0.1f));
            // 弹回
            yield return StartCoroutine(ScaleAnimation(Vector3.one, 0.1f));
        }

        /// <summary>
        /// 指示器脉冲效果
        /// </summary>
        private IEnumerator IndicatorPulseEffect()
        {
            if (betIndicator == null) yield break;
            
            Vector3 originalScale = betIndicator.transform.localScale;
            
            while (hasBet && betIndicator.gameObject.activeInHierarchy)
            {
                // 放大
                yield return StartCoroutine(ScaleIndicator(originalScale * 1.3f, 0.5f));
                // 缩小
                yield return StartCoroutine(ScaleIndicator(originalScale, 0.5f));
                
                yield return new WaitForSeconds(0.5f);
            }
            
            betIndicator.transform.localScale = originalScale;
        }

        /// <summary>
        /// 指示器缩放动画
        /// </summary>
        private IEnumerator ScaleIndicator(Vector3 targetScale, float duration)
        {
            if (betIndicator == null) yield break;
            
            Vector3 startScale = betIndicator.transform.localScale;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                t = Mathf.SmoothStep(0, 1, t);
                
                betIndicator.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            betIndicator.transform.localScale = targetScale;
        }

        /// <summary>
        /// 特殊发光效果
        /// </summary>
        private IEnumerator SpecialGlowEffect()
        {
            if (backgroundImage == null) yield break;
            
            Color originalColor = backgroundImage.color;
            Color glowColor = Color.white;
            
            // 发光
            float duration = 0.3f;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                
                backgroundImage.color = Color.Lerp(originalColor, glowColor, t);
                yield return null;
            }

            // 恢复
            elapsedTime = 0;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                
                backgroundImage.color = Color.Lerp(glowColor, originalColor, t);
                yield return null;
            }

            backgroundImage.color = originalColor;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 更新视觉状态
        /// </summary>
        private void UpdateVisualState()
        {
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
            
            if (backgroundImage != null)
            {
                Color color = backgroundImage.color;
                color.a = isInteractable ? 1f : 0.7f;
                backgroundImage.color = color;
            }
        }

        /// <summary>
        /// 获取边注类型对应的颜色
        /// </summary>
        private Color GetSideBetColor()
        {
            return betType switch
            {
                BaccaratBetType.Dragon7 => new Color(1f, 0.2f, 0.2f, 1f),      // 龙7-红色
                BaccaratBetType.BankerPair => new Color(1f, 0.6f, 0.2f, 1f),   // 庄对-橙色
                BaccaratBetType.PlayerPair => new Color(0.2f, 0.6f, 1f, 1f),   // 闲对-蓝色
                BaccaratBetType.Lucky6 => new Color(1f, 0.8f, 0.2f, 1f),       // 幸运6-金色
                BaccaratBetType.Panda8 => new Color(0.6f, 0.2f, 1f, 1f),       // 熊8-紫色
                _ => normalColor
            };
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
        /// 获取当前投注状态
        /// </summary>
        public bool HasBet()
        {
            return hasBet;
        }

        /// <summary>
        /// 切换投注状态
        /// </summary>
        public void ToggleBetState()
        {
            UpdateDisplay(!hasBet);
        }

        /// <summary>
        /// 重置投注状态
        /// </summary>
        public void ResetBetState()
        {
            UpdateDisplay(false);
        }

        /// <summary>
        /// 设置按钮颜色主题
        /// </summary>
        public void SetColorTheme(Color normal, Color bet, Color highlight)
        {
            normalColor = normal;
            betColor = bet;
            highlightColor = highlight;
            
            // 应用新颜色
            UpdateDisplay(hasBet);
        }

        #endregion

        #region 调试方法

        /// <summary>
        /// 显示组件状态
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowComponentStatus()
        {
            Debug.Log($"=== SideBetButton {betType} 组件状态 ===");
            Debug.Log($"自动创建并显示: {autoCreateAndShow}");
            Debug.Log($"启动时显示: {showOnAwake}");
            Debug.Log($"立即显示: {immediateDisplay}");
            Debug.Log($"按钮UI已创建: {buttonUICreated}");
            Debug.Log($"是否可交互: {isInteractable}");
            Debug.Log($"是否有投注: {hasBet}");
            Debug.Log($"父Canvas: {(parentCanvas != null ? "✓" : "✗")}");
            Debug.Log($"主按钮: {(mainButton != null ? "✓" : "✗")}");
            Debug.Log($"标题文本: {(titleText != null ? "✓" : "✗")} - {titleText?.text}");
            Debug.Log($"赔率文本: {(oddsText != null ? "✓" : "✗")} - {oddsText?.text}");
            Debug.Log($"背景图片: {(backgroundImage != null ? "✓" : "✗")}");
            Debug.Log($"投注指示器: {(betIndicator != null ? "✓" : "✗")}");
            Debug.Log($"按钮组件: {(button != null ? "✓" : "✗")}");
            Debug.Log($"显示标题: {displayTitle}");
            Debug.Log($"赔率: {odds}");
        }

        /// <summary>
        /// 测试所有功能
        /// </summary>
        [ContextMenu("测试所有功能")]
        public void TestAllFunctions()
        {
            Debug.Log($"[SideBetButton] {betType} 开始测试所有功能");
            
            // 测试投注状态切换
            ToggleBetState();
            System.Threading.Thread.Sleep(500);
            
            // 测试交互状态
            SetInteractable(false);
            System.Threading.Thread.Sleep(500);
            SetInteractable(true);
            
            // 测试特殊效果
            TriggerSpecialEffect();
            
            Debug.Log($"[SideBetButton] {betType} 功能测试完成");
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
            backgroundImage = null;
            betIndicator = null;
            button = null;
            
            buttonUICreated = false;
            
            Debug.Log($"[SideBetButton] {betType} 所有UI已删除");
        }

        #endregion
    }
}
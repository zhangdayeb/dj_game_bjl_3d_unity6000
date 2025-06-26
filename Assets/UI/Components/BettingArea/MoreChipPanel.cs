// Assets/UI/Components/BettingArea/MoreChipPanel.cs
// 更多筹码选择面板组件 - 启动即显示版本
// 自动创建并立即显示筹码选择面板界面
// 创建时间: 2025/6/26

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BaccaratGame.UI.Components
{
    /// <summary>
    /// 更多筹码选择面板组件 - 启动即显示版本
    /// 弹出式面板显示所有可用筹码选项
    /// 组件启动时立即创建并显示筹码选择面板界面
    /// </summary>
    public class MoreChipPanel : MonoBehaviour
    {
        #region 序列化字段

        [Header("自动显示设置")]
        public bool autoCreateAndShow = false;
        public bool showOnAwake = false; // 默认不在启动时显示，因为是弹出面板
        public bool immediateDisplay = false;

        [Header("面板布局")]
        public Vector2 panelSize = new Vector2(400, 300);
        public Vector2 panelPosition = Vector2.zero;
        public Color panelBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        public Color headerColor = new Color(0.2f, 0.6f, 1f, 1f);

        [Header("筹码配置")]
        public decimal[] allChipValues = { 25m, 100m, 500m, 1000m, 5000m, 10000m, 25000m, 50000m };
        public string[] chipDisplayTexts = { "25", "100", "500", "1K", "5K", "10K", "25K", "50K" };
        public Color[] chipColors = { 
            Color.green, Color.blue, Color.red, Color.yellow, 
            Color.magenta, Color.cyan, new Color(1f, 0.5f, 0f), new Color(0.5f, 0f, 1f) 
        };
        
        [Header("按钮布局")]
        public Vector2 buttonSize = new Vector2(80, 80);
        public Vector2 spacing = new Vector2(10, 10);
        public int columnsPerRow = 4;
        
        [Header("动画配置")]
        public bool enableAnimation = true;
        public float animationDuration = 0.3f;
        
        [Header("现有组件引用 (可选)")]
        public GameObject panelRoot;
        public Transform chipContainer;
        public Button closeButton;
        public Button backgroundButton;
        public Text titleText;
        public GridLayoutGroup gridLayout;

        [Header("调试设置")]
        public bool enableDebugMode = true;

        #endregion

        #region 私有字段

        private RectTransform rectTransform;
        private Canvas parentCanvas;
        private bool panelUICreated = false;
        private bool isVisible = false;
        private Coroutine animationCoroutine;
        
        // UI组件引用
        private GameObject mainPanel;
        private GameObject headerPanel;
        private GameObject contentPanel;
        private GameObject maskPanel;
        private List<Button> chipButtons = new List<Button>();

        #endregion

        #region 事件定义

        // 事件回调
        public System.Action<decimal> OnChipSelected;
        public System.Action OnPanelClosed;

        #endregion

        #region 生命周期

        private void Awake()
        {
            InitializeComponent();
            
            if (showOnAwake)
            {
                CreateAndShowPanel();
            }
            else if (autoCreateAndShow)
            {
                CreateAndShowPanel();
                Hide(false); // 创建但不显示
            }
        }

        private void Start()
        {
            if (!panelUICreated && autoCreateAndShow)
            {
                CreateAndShowPanel();
                Hide(false); // 创建但不显示
            }
            
            SetupExistingComponents();
        }

        private void OnValidate()
        {
            // 在编辑器中实时预览
            if (Application.isEditor && !Application.isPlaying)
            {
                if (immediateDisplay)
                {
                    InitializeComponent();
                    CreateAndShowPanel();
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

            // 设置为全屏
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            // 查找父Canvas
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                CreateCanvasIfNeeded();
            }

            if (enableDebugMode)
                Debug.Log("[MoreChipPanel] 组件初始化完成");
        }

        /// <summary>
        /// 如需要则创建Canvas
        /// </summary>
        private void CreateCanvasIfNeeded()
        {
            GameObject canvasObj = new GameObject("ChipPanelCanvas");
            canvasObj.transform.SetParent(transform.parent);
            
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // 最高层级
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
            
            // 将MoreChipPanel移到Canvas下
            transform.SetParent(canvasObj.transform);
            
            parentCanvas = canvas;
            
            if (enableDebugMode)
                Debug.Log("[MoreChipPanel] 创建了新的Canvas");
        }

        #endregion

        #region 面板界面创建

        /// <summary>
        /// 创建并显示面板界面
        /// </summary>
        [ContextMenu("创建并显示面板界面")]
        public void CreateAndShowPanel()
        {
            if (panelUICreated) return;

            try
            {
                // 确保组件已初始化
                if (rectTransform == null)
                    InitializeComponent();

                // 创建背景遮罩
                CreateMaskPanel();
                
                // 创建主面板
                CreateMainPanel();
                
                // 创建标题栏
                CreateHeaderPanel();
                
                // 创建内容区域
                CreateContentPanel();
                
                // 生成筹码按钮
                GenerateChipButtons();

                panelUICreated = true;
                
                if (enableDebugMode)
                    Debug.Log("[MoreChipPanel] 面板界面创建完成并已显示");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MoreChipPanel] 创建面板界面时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建背景遮罩
        /// </summary>
        private void CreateMaskPanel()
        {
            GameObject maskObj = new GameObject("BackgroundMask");
            maskObj.transform.SetParent(transform);

            RectTransform maskRect = maskObj.AddComponent<RectTransform>();
            maskRect.anchorMin = Vector2.zero;
            maskRect.anchorMax = Vector2.one;
            maskRect.offsetMin = Vector2.zero;
            maskRect.offsetMax = Vector2.zero;
            maskRect.localScale = Vector3.one;

            Image maskImage = maskObj.AddComponent<Image>();
            maskImage.color = new Color(0, 0, 0, 0.5f);

            // 添加背景按钮用于关闭面板
            if (backgroundButton == null)
            {
                backgroundButton = maskObj.AddComponent<Button>();
                backgroundButton.onClick.AddListener(() => Hide());
            }

            maskPanel = maskObj;

            if (enableDebugMode)
                Debug.Log("[MoreChipPanel] 背景遮罩创建完成");
        }

        /// <summary>
        /// 创建主面板
        /// </summary>
        private void CreateMainPanel()
        {
            GameObject panelObj = new GameObject("MainPanel");
            panelObj.transform.SetParent(transform);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.sizeDelta = panelSize;
            panelRect.anchoredPosition = panelPosition;
            panelRect.localScale = Vector3.one;

            // 添加背景
            Image panelBackground = panelObj.AddComponent<Image>();
            panelBackground.color = panelBackgroundColor;
            panelBackground.sprite = CreateRoundedRectSprite();

            // 添加阴影效果
            Shadow shadow = panelObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(5, -5);

            mainPanel = panelObj;

            // 更新引用
            if (panelRoot == null) panelRoot = panelObj;

            if (enableDebugMode)
                Debug.Log("[MoreChipPanel] 主面板创建完成");
        }

        /// <summary>
        /// 创建标题栏
        /// </summary>
        private void CreateHeaderPanel()
        {
            GameObject headerObj = new GameObject("HeaderPanel");
            headerObj.transform.SetParent(mainPanel.transform);

            RectTransform headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.sizeDelta = new Vector2(0, 50);
            headerRect.anchoredPosition = new Vector2(0, -25);
            headerRect.localScale = Vector3.one;

            // 添加背景
            Image headerBackground = headerObj.AddComponent<Image>();
            headerBackground.color = headerColor;
            headerBackground.sprite = CreateSolidSprite(Color.white);

            headerPanel = headerObj;

            // 创建标题文本
            CreateTitleText(headerObj);
            
            // 创建关闭按钮
            CreateCloseButton(headerObj);

            if (enableDebugMode)
                Debug.Log("[MoreChipPanel] 标题栏创建完成");
        }

        /// <summary>
        /// 创建标题文本
        /// </summary>
        private void CreateTitleText(GameObject parent)
        {
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(parent.transform);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = new Vector2(15, 0);
            titleRect.offsetMax = new Vector2(-50, 0);
            titleRect.localScale = Vector3.one;

            if (titleText == null)
            {
                titleText = titleObj.AddComponent<Text>();
                titleText.text = "选择筹码";
                titleText.color = Color.white;
                titleText.alignment = TextAnchor.MiddleLeft;
                titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                titleText.fontSize = 16;
                titleText.fontStyle = FontStyle.Bold;
            }
        }

        /// <summary>
        /// 创建关闭按钮
        /// </summary>
        private void CreateCloseButton(GameObject parent)
        {
            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(parent.transform);

            RectTransform closeRect = closeObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 0.5f);
            closeRect.anchorMax = new Vector2(1, 0.5f);
            closeRect.sizeDelta = new Vector2(30, 30);
            closeRect.anchoredPosition = new Vector2(-15, 0);
            closeRect.localScale = Vector3.one;

            Image closeImage = closeObj.AddComponent<Image>();
            closeImage.color = new Color(1f, 1f, 1f, 0.3f);
            closeImage.sprite = CreateSolidSprite(Color.white);

            if (closeButton == null)
            {
                closeButton = closeObj.AddComponent<Button>();
                ColorBlock colors = closeButton.colors;
                colors.normalColor = new Color(1f, 1f, 1f, 0.3f);
                colors.highlightedColor = new Color(1f, 1f, 1f, 0.5f);
                colors.pressedColor = new Color(1f, 1f, 1f, 0.7f);
                closeButton.colors = colors;
                
                closeButton.onClick.AddListener(() => Hide());
            }

            // 添加关闭图标文本
            GameObject closeTextObj = new GameObject("CloseText");
            closeTextObj.transform.SetParent(closeObj.transform);

            RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.offsetMin = Vector2.zero;
            closeTextRect.offsetMax = Vector2.zero;
            closeTextRect.localScale = Vector3.one;

            Text closeText = closeTextObj.AddComponent<Text>();
            closeText.text = "×";
            closeText.color = Color.white;
            closeText.alignment = TextAnchor.MiddleCenter;
            closeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            closeText.fontSize = 18;
            closeText.fontStyle = FontStyle.Bold;
        }

        /// <summary>
        /// 创建内容区域
        /// </summary>
        private void CreateContentPanel()
        {
            GameObject contentObj = new GameObject("ContentPanel");
            contentObj.transform.SetParent(mainPanel.transform);

            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(10, 10);
            contentRect.offsetMax = new Vector2(-10, -60);
            contentRect.localScale = Vector3.one;

            contentPanel = contentObj;

            // 创建滚动视图
            CreateScrollView(contentObj);

            if (enableDebugMode)
                Debug.Log("[MoreChipPanel] 内容区域创建完成");
        }

        /// <summary>
        /// 创建滚动视图
        /// </summary>
        private void CreateScrollView(GameObject parent)
        {
            // 创建滚动视图
            GameObject scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(parent.transform);

            RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            scrollRect.localScale = Vector3.one;

            ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            // 创建筹码容器
            CreateChipContainer(scrollObj, scroll);
        }

        /// <summary>
        /// 创建筹码容器
        /// </summary>
        private void CreateChipContainer(GameObject scrollView, ScrollRect scroll)
        {
            GameObject containerObj = new GameObject("ChipContainer");
            containerObj.transform.SetParent(scrollView.transform);

            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 1);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.pivot = new Vector2(0.5f, 1);
            containerRect.sizeDelta = new Vector2(0, 200);
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.localScale = Vector3.one;

            scroll.content = containerRect;

            // 添加网格布局
            if (gridLayout == null)
            {
                gridLayout = containerObj.AddComponent<GridLayoutGroup>();
                gridLayout.cellSize = buttonSize;
                gridLayout.spacing = spacing;
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = columnsPerRow;
                gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
                gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
                gridLayout.childAlignment = TextAnchor.MiddleCenter;
            }

            // 添加内容尺寸自适应
            ContentSizeFitter fitter = containerObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            chipContainer = containerObj.transform;

            if (enableDebugMode)
                Debug.Log("[MoreChipPanel] 筹码容器创建完成");
        }

        /// <summary>
        /// 生成筹码按钮
        /// </summary>
        private void GenerateChipButtons()
        {
            if (chipContainer == null || allChipValues == null) return;

            // 清除现有按钮
            ClearExistingButtons();

            for (int i = 0; i < allChipValues.Length; i++)
            {
                string displayText = i < chipDisplayTexts.Length ? chipDisplayTexts[i] : allChipValues[i].ToString();
                Color buttonColor = i < chipColors.Length ? chipColors[i] : Color.white;
                
                CreateChipButton(allChipValues[i], displayText, buttonColor);
            }

            if (enableDebugMode)
                Debug.Log($"[MoreChipPanel] 生成了 {allChipValues.Length} 个筹码按钮");
        }

        /// <summary>
        /// 创建筹码按钮
        /// </summary>
        private void CreateChipButton(decimal value, string displayText, Color buttonColor)
        {
            GameObject buttonObj = new GameObject($"ChipButton_{value}");
            buttonObj.transform.SetParent(chipContainer);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.localScale = Vector3.one;

            // 添加背景
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = buttonColor;
            buttonImage.sprite = CreateCircleSprite();

            // 添加Button组件
            Button button = buttonObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = buttonColor;
            colors.highlightedColor = Color.Lerp(buttonColor, Color.white, 0.3f);
            colors.pressedColor = Color.Lerp(buttonColor, Color.black, 0.2f);
            colors.disabledColor = Color.gray;
            button.colors = colors;

            // 设置点击事件
            button.onClick.AddListener(() => OnChipButtonClicked(value));

            // 添加文本
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textRect.localScale = Vector3.one;

            Text chipText = textObj.AddComponent<Text>();
            chipText.text = displayText;
            chipText.color = GetContrastColor(buttonColor);
            chipText.alignment = TextAnchor.MiddleCenter;
            chipText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            chipText.fontSize = 12;
            chipText.fontStyle = FontStyle.Bold;

            // 添加边框效果
            Outline outline = buttonObj.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.5f);
            outline.effectDistance = new Vector2(1, -1);

            chipButtons.Add(button);
        }

        /// <summary>
        /// 创建圆形Sprite
        /// </summary>
        private Sprite CreateCircleSprite()
        {
            int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 2;
            
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
        /// 创建纯色Sprite
        /// </summary>
        private Sprite CreateSolidSprite(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
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

        /// <summary>
        /// 获取对比色
        /// </summary>
        private Color GetContrastColor(Color backgroundColor)
        {
            float brightness = (backgroundColor.r * 0.299f + backgroundColor.g * 0.587f + backgroundColor.b * 0.114f);
            return brightness > 0.5f ? Color.black : Color.white;
        }

        #endregion

        #region 现有组件设置

        /// <summary>
        /// 设置现有组件
        /// </summary>
        private void SetupExistingComponents()
        {
            // 如果有现有的组件引用，设置它们
            if (closeButton != null && !panelUICreated)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => Hide());
            }

            if (backgroundButton != null && !panelUICreated)
            {
                backgroundButton.onClick.RemoveAllListeners();
                backgroundButton.onClick.AddListener(() => Hide());
            }

            if (enableDebugMode)
                Debug.Log("[MoreChipPanel] 现有组件设置完成");
        }

        #endregion

        #region 面板控制逻辑

        /// <summary>
        /// 显示面板
        /// </summary>
        public void Show(bool animated = true)
        {
            if (isVisible) return;

            // 确保面板已创建
            if (!panelUICreated)
            {
                CreateAndShowPanel();
            }

            isVisible = true;

            if (panelRoot != null)
                panelRoot.SetActive(true);

            if (animated && enableAnimation)
            {
                PlayShowAnimation();
            }

            if (enableDebugMode)
                Debug.Log("[MoreChipPanel] 面板显示");
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void Hide(bool animated = true)
        {
            if (!isVisible) return;

            isVisible = false;

            if (animated && enableAnimation)
            {
                PlayHideAnimation();
            }
            else
            {
                if (panelRoot != null)
                    panelRoot.SetActive(false);
            }

            // 触发关闭事件
            OnPanelClosed?.Invoke();

            if (enableDebugMode)
                Debug.Log("[MoreChipPanel] 面板隐藏");
        }

        /// <summary>
        /// 切换面板显示
        /// </summary>
        public void Toggle()
        {
            if (isVisible)
                Hide();
            else
                Show();
        }

        /// <summary>
        /// 处理筹码按钮点击
        /// </summary>
        private void OnChipButtonClicked(decimal value)
        {
            OnChipSelected?.Invoke(value);
            Hide();

            if (enableDebugMode)
                Debug.Log($"[MoreChipPanel] 选择筹码: {value}");
        }

        /// <summary>
        /// 清除现有按钮
        /// </summary>
        private void ClearExistingButtons()
        {
            foreach (var button in chipButtons)
            {
                if (button != null && button.gameObject != null)
                {
                    if (Application.isPlaying)
                        Destroy(button.gameObject);
                    else
                        DestroyImmediate(button.gameObject);
                }
            }
            chipButtons.Clear();
        }

        #endregion

        #region 动画效果

        /// <summary>
        /// 播放显示动画
        /// </summary>
        private void PlayShowAnimation()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(ShowAnimation());
        }

        /// <summary>
        /// 播放隐藏动画
        /// </summary>
        private void PlayHideAnimation()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(HideAnimation());
        }

        /// <summary>
        /// 显示动画协程
        /// </summary>
        private IEnumerator ShowAnimation()
        {
            if (mainPanel == null) yield break;

            mainPanel.transform.localScale = Vector3.zero;
            
            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                t = Mathf.SmoothStep(0, 1, t);
                
                mainPanel.transform.localScale = Vector3.one * t;
                yield return null;
            }

            mainPanel.transform.localScale = Vector3.one;
            animationCoroutine = null;
        }

        /// <summary>
        /// 隐藏动画协程
        /// </summary>
        private IEnumerator HideAnimation()
        {
            if (mainPanel == null) yield break;

            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                float scale = Mathf.SmoothStep(1, 0, t);
                
                mainPanel.transform.localScale = Vector3.one * scale;
                yield return null;
            }

            if (panelRoot != null)
                panelRoot.SetActive(false);
            mainPanel.transform.localScale = Vector3.one;
            animationCoroutine = null;
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 强制显示面板
        /// </summary>
        [ContextMenu("强制显示面板")]
        public void ForceShowPanel()
        {
            panelUICreated = false;
            CreateAndShowPanel();
            Show(false);
        }

        /// <summary>
        /// 设置筹码配置
        /// </summary>
        public void SetChipConfig(decimal[] values, string[] displayTexts = null, Color[] colors = null)
        {
            allChipValues = values;
            chipDisplayTexts = displayTexts ?? new string[0];
            chipColors = colors ?? new Color[0];

            // 重新生成按钮
            if (panelUICreated)
            {
                GenerateChipButtons();
            }

            if (enableDebugMode)
                Debug.Log($"[MoreChipPanel] 筹码配置更新，共 {values.Length} 个筹码");
        }

        /// <summary>
        /// 设置布局参数
        /// </summary>
        public void SetLayoutParams(Vector2 newButtonSize, Vector2 newSpacing, int columns)
        {
            buttonSize = newButtonSize;
            spacing = newSpacing;
            columnsPerRow = columns;

            if (gridLayout != null)
            {
                gridLayout.cellSize = buttonSize;
                gridLayout.spacing = spacing;
                gridLayout.constraintCount = columns;
            }

            if (enableDebugMode)
                Debug.Log($"[MoreChipPanel] 布局参数更新: 大小={buttonSize}, 间距={spacing}, 列数={columns}");
        }

        /// <summary>
        /// 获取是否可见
        /// </summary>
        public bool IsVisible()
        {
            return isVisible;
        }

        /// <summary>
        /// 获取筹码按钮数量
        /// </summary>
        public int GetChipButtonCount()
        {
            return chipButtons.Count;
        }

        #endregion

        #region 调试方法

        /// <summary>
        /// 显示组件状态
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowComponentStatus()
        {
            Debug.Log("=== MoreChipPanel 组件状态 ===");
            Debug.Log($"自动创建并显示: {autoCreateAndShow}");
            Debug.Log($"启动时显示: {showOnAwake}");
            Debug.Log($"立即显示: {immediateDisplay}");
            Debug.Log($"面板UI已创建: {panelUICreated}");
            Debug.Log($"是否可见: {isVisible}");
            Debug.Log($"父Canvas: {(parentCanvas != null ? "✓" : "✗")}");
            Debug.Log($"主面板: {(mainPanel != null ? "✓" : "✗")}");
            Debug.Log($"筹码容器: {(chipContainer != null ? "✓" : "✗")}");
            Debug.Log($"网格布局: {(gridLayout != null ? "✓" : "✗")}");
            Debug.Log($"筹码按钮数量: {chipButtons.Count}");
            Debug.Log($"筹码值配置: [{string.Join(", ", allChipValues)}]");
            Debug.Log($"布局参数: 大小={buttonSize}, 间距={spacing}, 列数={columnsPerRow}");
            Debug.Log($"启用动画: {enableAnimation}, 动画时长: {animationDuration}");
        }

        /// <summary>
        /// 测试所有功能
        /// </summary>
        [ContextMenu("测试所有功能")]
        public void TestAllFunctions()
        {
            Debug.Log("[MoreChipPanel] 开始测试所有功能");

            // 测试显示隐藏
            Show();
            System.Threading.Thread.Sleep(1000);
            Hide();
            
            // 测试筹码配置
            decimal[] testValues = { 50m, 200m, 1000m };
            string[] testTexts = { "50", "200", "1K" };
            SetChipConfig(testValues, testTexts);

            Debug.Log("[MoreChipPanel] 功能测试完成");
        }

        /// <summary>
        /// 删除所有创建的UI
        /// </summary>
        [ContextMenu("删除所有UI")]
        public void ClearAllUI()
        {
            // 清除筹码按钮
            ClearExistingButtons();

            // 清除面板对象
            if (mainPanel != null)
            {
                if (Application.isPlaying)
                    Destroy(mainPanel);
                else
                    DestroyImmediate(mainPanel);
                mainPanel = null;
            }

            if (maskPanel != null)
            {
                if (Application.isPlaying)
                    Destroy(maskPanel);
                else
                    DestroyImmediate(maskPanel);
                maskPanel = null;
            }

            // 清空引用
            panelRoot = null;
            chipContainer = null;
            closeButton = null;
            backgroundButton = null;
            titleText = null;
            gridLayout = null;

            panelUICreated = false;
            isVisible = false;

            Debug.Log("[MoreChipPanel] 所有UI已删除");
        }

        #endregion
    }
}
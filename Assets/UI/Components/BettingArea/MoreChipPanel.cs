// Assets/UI/Components/BettingArea/MoreChipPanel.cs
// 筹码配置面板组件 - 重写优化版本
// 只显示图片筹码，选中效果明显
// 创建时间: 2025/6/27

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BaccaratGame.UI.Components
{
    /// <summary>
    /// 筹码配置面板组件 - 优化版本
    /// 只显示图片筹码，最多选择5个，选中效果明显
    /// </summary>
    public class MoreChipPanel : MonoBehaviour
    {
        #region 序列化字段

        [Header("自动显示设置")]
        public bool autoCreateAndShow = false;
        public bool showOnAwake = false;
        public bool immediateDisplay = false;

        [Header("面板布局")]
        public Vector2 panelSize = new Vector2(480, 360);
        public Vector2 panelPosition = Vector2.zero;
        public Color panelBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        public Color maskColor = new Color(0f, 0f, 0f, 0.5f);
        public Color headerColor = new Color(0.2f, 0.6f, 1f, 1f);

        [Header("所有可用筹码")]
        public int[] allAvailableChips = { 
            1, 5, 10, 20, 50, 100, 500, 1000, 5000, 10000, 20000, 50000, 100000, 200000, 500000, 1000000 
        };
        
        [Header("默认选择的筹码")]
        public int[] defaultSelectedChips = { 1, 10, 50, 100, 500 };
        
        [Header("选择限制")]
        public int maxSelectionCount = 5;
        
        [Header("按钮布局")]
        public Vector2 buttonSize = new Vector2(65, 65);
        public Vector2 spacing = new Vector2(12, 12);
        public int columnsPerRow = 5;
        public float headerHeight = 50f;
        public float paddingTop = 80f;
        public float paddingBottom = 50f;
        public float paddingSides = 15f;
        
        [Header("选中效果")]
        public Color selectedBorderColor = new Color(0.2f, 0.8f, 0.2f, 1f);
        public Color hoverBorderColor = new Color(1f, 1f, 0.8f, 0.8f);
        public float selectedScale = 1.1f;
        public float hoverScale = 1.05f;
        public float borderWidth = 3f;
        
        [Header("字体设置")]
        public int titleFontSize = 18;
        public Color titleColor = Color.white;
        
        [Header("动画配置")]
        public bool enableAnimation = true;
        public float animationDuration = 0.3f;
        public float scaleAnimationDuration = 0.2f;
        
        [Header("资源路径")]
        public string chipImagePath = "Images/chips/";
        
        [Header("调试设置")]
        public bool enableDebugMode = true;

        #endregion

        #region 事件定义

        public System.Action<int[]> OnChipsSelected;
        public System.Action OnPanelClosed;

        #endregion

        #region 私有字段

        private RectTransform rectTransform;
        private Canvas parentCanvas;
        private bool panelUICreated = false;
        private bool isVisible = false;
        private Coroutine animationCoroutine;
        
        // UI组件引用
        private GameObject panelRoot;
        private GameObject mainPanel;
        private GameObject maskPanel;
        private Transform chipContainer;
        private Text statusText;
        private Button confirmButton;
        private Button resetButton;
        private Button closeButton;
        private ScrollRect scrollView;
        private GridLayoutGroup gridLayout;
        
        // 筹码选择状态
        private List<int> currentSelectedChips = new List<int>();
        private Dictionary<int, ChipButtonData> chipButtonDataMap = new Dictionary<int, ChipButtonData>();

        #endregion

        #region 数据结构

        /// <summary>
        /// 筹码按钮数据
        /// </summary>
        private class ChipButtonData
        {
            public GameObject buttonObject;
            public Button button;
            public Image chipImage;
            public Image borderImage;
            public RectTransform rectTransform;
            public int chipValue;
            public bool isSelected;
        }

        #endregion

        #region 生命周期

        private void Awake()
        {
            InitializeComponent();
        }

        private void Start()
        {
            // 只在明确需要时才自动显示
            if (autoCreateAndShow && showOnAwake)
            {
                Show();
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponent()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                CreateCanvasIfNeeded();
            }

            InitializeSelection();

            if (enableDebugMode)
                Debug.Log("[MoreChipPanel] 组件初始化完成");
        }

        /// <summary>
        /// 初始化选择状态
        /// </summary>
        private void InitializeSelection()
        {
            currentSelectedChips.Clear();
            if (defaultSelectedChips != null)
            {
                foreach (int chip in defaultSelectedChips)
                {
                    if (currentSelectedChips.Count < maxSelectionCount)
                    {
                        currentSelectedChips.Add(chip);
                    }
                }
            }

            if (enableDebugMode)
                Debug.Log($"[MoreChipPanel] 初始化选择: [{string.Join(", ", currentSelectedChips)}]");
        }

        /// <summary>
        /// 创建Canvas
        /// </summary>
        private void CreateCanvasIfNeeded()
        {
            GameObject canvasObj = new GameObject("MoreChipPanelCanvas");
            canvasObj.transform.SetParent(transform.parent);
            
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            transform.SetParent(canvasObj.transform);
            parentCanvas = canvas;
        }

        #endregion

        #region 面板创建

        /// <summary>
        /// 创建面板UI
        /// </summary>
        private void CreatePanelUI()
        {
            if (panelUICreated) return;

            try
            {
                CreateRootPanel();
                CreateMaskBackground();
                CreateMainPanel();
                CreateHeaderPanel();
                CreateContentArea();
                CreateChipGrid();
                CreateBottomPanel();
                GenerateChipButtons();
                
                panelUICreated = true;
                panelRoot.SetActive(false); // 默认隐藏

                if (enableDebugMode)
                    Debug.Log("[MoreChipPanel] 面板UI创建完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MoreChipPanel] 创建面板UI时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建根面板
        /// </summary>
        private void CreateRootPanel()
        {
            GameObject rootObj = new GameObject("MoreChipPanelRoot");
            rootObj.transform.SetParent(parentCanvas.transform);

            RectTransform rootRect = rootObj.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            rootRect.localScale = Vector3.one;

            panelRoot = rootObj;
        }

        /// <summary>
        /// 创建遮罩背景
        /// </summary>
        private void CreateMaskBackground()
        {
            GameObject maskObj = new GameObject("MaskBackground");
            maskObj.transform.SetParent(panelRoot.transform);

            RectTransform maskRect = maskObj.AddComponent<RectTransform>();
            maskRect.anchorMin = Vector2.zero;
            maskRect.anchorMax = Vector2.one;
            maskRect.offsetMin = Vector2.zero;
            maskRect.offsetMax = Vector2.zero;
            maskRect.localScale = Vector3.one;

            Image maskImage = maskObj.AddComponent<Image>();
            maskImage.color = maskColor;

            Button maskButton = maskObj.AddComponent<Button>();
            maskButton.onClick.AddListener(() => Hide());

            maskPanel = maskObj;
        }

        /// <summary>
        /// 创建主面板
        /// </summary>
        private void CreateMainPanel()
        {
            GameObject mainObj = new GameObject("MainPanel");
            mainObj.transform.SetParent(panelRoot.transform);

            RectTransform mainRect = mainObj.AddComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0.5f, 0.5f);
            mainRect.anchorMax = new Vector2(0.5f, 0.5f);
            mainRect.pivot = new Vector2(0.5f, 0.5f);
            mainRect.sizeDelta = panelSize;
            mainRect.anchoredPosition = panelPosition;
            mainRect.localScale = Vector3.one;

            Image mainImage = mainObj.AddComponent<Image>();
            mainImage.color = panelBackgroundColor;

            Shadow shadow = mainObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(3, -3);

            mainPanel = mainObj;
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
            headerRect.pivot = new Vector2(0.5f, 1);
            headerRect.sizeDelta = new Vector2(0, headerHeight);
            headerRect.anchoredPosition = Vector2.zero;
            headerRect.localScale = Vector3.one;

            Image headerImage = headerObj.AddComponent<Image>();
            headerImage.color = headerColor;

            // 标题文本
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(headerObj.transform);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = new Vector2(20, 0);
            titleRect.offsetMax = new Vector2(-60, 0);
            titleRect.localScale = Vector3.one;

            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "选择筹码 (最多5个)";
            titleText.font = GetDefaultFont();
            titleText.fontSize = titleFontSize;
            titleText.color = titleColor;
            titleText.alignment = TextAnchor.MiddleLeft;
            titleText.fontStyle = FontStyle.Bold;

            // 关闭按钮
            CreateCloseButton(headerObj);
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
            closeRect.pivot = new Vector2(1, 0.5f);
            closeRect.sizeDelta = new Vector2(40, 40);
            closeRect.anchoredPosition = new Vector2(-10, 0);
            closeRect.localScale = Vector3.one;

            Image closeImage = closeObj.AddComponent<Image>();
            closeImage.color = new Color(1f, 0.3f, 0.3f, 1f);

            Button closeBtn = closeObj.AddComponent<Button>();
            closeBtn.onClick.AddListener(() => Hide());

            // X文字
            GameObject xTextObj = new GameObject("XText");
            xTextObj.transform.SetParent(closeObj.transform);

            RectTransform xTextRect = xTextObj.AddComponent<RectTransform>();
            xTextRect.anchorMin = Vector2.zero;
            xTextRect.anchorMax = Vector2.one;
            xTextRect.offsetMin = Vector2.zero;
            xTextRect.offsetMax = Vector2.zero;
            xTextRect.localScale = Vector3.one;

            Text xText = xTextObj.AddComponent<Text>();
            xText.text = "×";
            xText.font = GetDefaultFont();
            xText.fontSize = 20;
            xText.color = Color.white;
            xText.alignment = TextAnchor.MiddleCenter;
            xText.fontStyle = FontStyle.Bold;

            closeButton = closeBtn;
        }

        /// <summary>
        /// 创建内容区域
        /// </summary>
        private void CreateContentArea()
        {
            GameObject contentObj = new GameObject("ContentArea");
            contentObj.transform.SetParent(mainPanel.transform);

            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(paddingSides, paddingBottom);
            contentRect.offsetMax = new Vector2(-paddingSides, -paddingTop);
            contentRect.localScale = Vector3.one;
        }

        /// <summary>
        /// 创建筹码网格
        /// </summary>
        private void CreateChipGrid()
        {
            GameObject contentArea = mainPanel.transform.Find("ContentArea").gameObject;
            
            GameObject scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(contentArea.transform);

            RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = new Vector2(0, 50);
            scrollRect.offsetMax = Vector2.zero;
            scrollRect.localScale = Vector3.one;

            // 滚动背景
            Image scrollBg = scrollObj.AddComponent<Image>();
            scrollBg.color = new Color(0.05f, 0.05f, 0.05f, 0.8f);

            // 滚动组件
            ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;

            // Viewport
            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollObj.transform);

            RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(5, 5);
            viewportRect.offsetMax = new Vector2(-15, -5);
            viewportRect.localScale = Vector3.one;

            Image viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.color = Color.clear;
            Mask viewportMask = viewportObj.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            scroll.viewport = viewportRect;

            // 筹码容器
            GameObject gridObj = new GameObject("ChipGrid");
            gridObj.transform.SetParent(viewportObj.transform);

            RectTransform gridRect = gridObj.AddComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0, 1);
            gridRect.anchorMax = new Vector2(1, 1);
            gridRect.pivot = new Vector2(0.5f, 1);
            gridRect.anchoredPosition = Vector2.zero;
            gridRect.localScale = Vector3.one;

            scroll.content = gridRect;

            // 网格布局
            gridLayout = gridObj.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = buttonSize;
            gridLayout.spacing = spacing;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = columnsPerRow;
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            gridLayout.padding = new RectOffset(8, 8, 8, 8);

            // 内容自适应
            ContentSizeFitter fitter = gridObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            chipContainer = gridObj.transform;
            scrollView = scroll;
        }

        /// <summary>
        /// 创建底部面板
        /// </summary>
        private void CreateBottomPanel()
        {
            GameObject contentArea = mainPanel.transform.Find("ContentArea").gameObject;
            
            GameObject bottomObj = new GameObject("BottomPanel");
            bottomObj.transform.SetParent(contentArea.transform);

            RectTransform bottomRect = bottomObj.AddComponent<RectTransform>();
            bottomRect.anchorMin = new Vector2(0, 0);
            bottomRect.anchorMax = new Vector2(1, 0);
            bottomRect.pivot = new Vector2(0.5f, 0);
            bottomRect.sizeDelta = new Vector2(0, 40);
            bottomRect.anchoredPosition = Vector2.zero;
            bottomRect.localScale = Vector3.one;

            // 状态文本
            GameObject statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(bottomObj.transform);

            RectTransform statusRect = statusObj.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0, 0);
            statusRect.anchorMax = new Vector2(0.5f, 1);
            statusRect.offsetMin = Vector2.zero;
            statusRect.offsetMax = Vector2.zero;
            statusRect.localScale = Vector3.one;

            Text statusTextComp = statusObj.AddComponent<Text>();
            statusTextComp.text = "已选择: 0/5";
            statusTextComp.font = GetDefaultFont();
            statusTextComp.fontSize = 14;
            statusTextComp.color = Color.white;
            statusTextComp.alignment = TextAnchor.MiddleLeft;
            statusText = statusTextComp;

            // 重置按钮
            CreateResetButton(bottomObj);
            
            // 确认按钮
            CreateConfirmButton(bottomObj);
        }

        /// <summary>
        /// 创建重置按钮
        /// </summary>
        private void CreateResetButton(GameObject parent)
        {
            GameObject resetObj = new GameObject("ResetButton");
            resetObj.transform.SetParent(parent.transform);

            RectTransform resetRect = resetObj.AddComponent<RectTransform>();
            resetRect.anchorMin = new Vector2(0.5f, 0);
            resetRect.anchorMax = new Vector2(0.75f, 1);
            resetRect.offsetMin = new Vector2(5, 5);
            resetRect.offsetMax = new Vector2(-5, -5);
            resetRect.localScale = Vector3.one;

            Image resetImage = resetObj.AddComponent<Image>();
            resetImage.color = new Color(0.8f, 0.4f, 0.2f, 1f);

            Button resetBtn = resetObj.AddComponent<Button>();
            resetBtn.onClick.AddListener(ResetSelection);

            GameObject resetTextObj = new GameObject("Text");
            resetTextObj.transform.SetParent(resetObj.transform);

            RectTransform resetTextRect = resetTextObj.AddComponent<RectTransform>();
            resetTextRect.anchorMin = Vector2.zero;
            resetTextRect.anchorMax = Vector2.one;
            resetTextRect.offsetMin = Vector2.zero;
            resetTextRect.offsetMax = Vector2.zero;
            resetTextRect.localScale = Vector3.one;

            Text resetText = resetTextObj.AddComponent<Text>();
            resetText.text = "重置";
            resetText.font = GetDefaultFont();
            resetText.fontSize = 12;
            resetText.color = Color.white;
            resetText.alignment = TextAnchor.MiddleCenter;
            resetText.fontStyle = FontStyle.Bold;

            resetButton = resetBtn;
        }

        /// <summary>
        /// 创建确认按钮
        /// </summary>
        private void CreateConfirmButton(GameObject parent)
        {
            GameObject confirmObj = new GameObject("ConfirmButton");
            confirmObj.transform.SetParent(parent.transform);

            RectTransform confirmRect = confirmObj.AddComponent<RectTransform>();
            confirmRect.anchorMin = new Vector2(0.75f, 0);
            confirmRect.anchorMax = new Vector2(1, 1);
            confirmRect.offsetMin = new Vector2(5, 5);
            confirmRect.offsetMax = new Vector2(-5, -5);
            confirmRect.localScale = Vector3.one;

            Image confirmImage = confirmObj.AddComponent<Image>();
            confirmImage.color = new Color(0.2f, 0.8f, 0.2f, 1f);

            Button confirmBtn = confirmObj.AddComponent<Button>();
            confirmBtn.onClick.AddListener(ConfirmSelection);

            GameObject confirmTextObj = new GameObject("Text");
            confirmTextObj.transform.SetParent(confirmObj.transform);

            RectTransform confirmTextRect = confirmTextObj.AddComponent<RectTransform>();
            confirmTextRect.anchorMin = Vector2.zero;
            confirmTextRect.anchorMax = Vector2.one;
            confirmTextRect.offsetMin = Vector2.zero;
            confirmTextRect.offsetMax = Vector2.zero;
            confirmTextRect.localScale = Vector3.one;

            Text confirmText = confirmTextObj.AddComponent<Text>();
            confirmText.text = "确认";
            confirmText.font = GetDefaultFont();
            confirmText.fontSize = 12;
            confirmText.color = Color.white;
            confirmText.alignment = TextAnchor.MiddleCenter;
            confirmText.fontStyle = FontStyle.Bold;

            confirmButton = confirmBtn;
        }

        #endregion

        #region 筹码按钮生成

        /// <summary>
        /// 生成筹码按钮
        /// </summary>
        private void GenerateChipButtons()
        {
            if (chipContainer == null || allAvailableChips == null) return;

            ClearAllChipButtons();

            int successCount = 0;
            foreach (int chipValue in allAvailableChips)
            {
                if (CreateChipButton(chipValue))
                {
                    successCount++;
                }
            }

            if (enableDebugMode)
                Debug.Log($"[MoreChipPanel] 成功创建 {successCount}/{allAvailableChips.Length} 个筹码按钮");

            // 更新选中状态
            UpdateAllSelectionStates();
            UpdateStatusText();
        }

        /// <summary>
        /// 创建单个筹码按钮
        /// </summary>
        private bool CreateChipButton(int chipValue)
        {
            // 1. 尝试加载图片
            string imageName = GenerateChipImageName(chipValue);
            Sprite chipSprite = LoadChipSprite(imageName);
            
            if (chipSprite == null)
            {
                if (enableDebugMode)
                    Debug.Log($"[MoreChipPanel] 跳过筹码 {chipValue}，图片加载失败: {imageName}");
                return false;
            }

            // 2. 创建按钮对象
            GameObject buttonObj = new GameObject($"ChipButton_{chipValue}");
            buttonObj.transform.SetParent(chipContainer);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.localScale = Vector3.one;

            // 3. 创建筹码图片
            Image chipImage = buttonObj.AddComponent<Image>();
            chipImage.sprite = chipSprite;
            chipImage.color = Color.white;
            chipImage.preserveAspect = true;

            // 4. 添加Button组件
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = chipImage;
            
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.disabledColor = Color.gray;
            button.colors = colors;

            // 5. 创建选中边框
            Image borderImage = CreateSelectionBorder(buttonObj);

            // 6. 设置点击事件
            button.onClick.AddListener(() => ToggleChipSelection(chipValue));

            // 7. 添加悬停效果
            AddHoverEffect(buttonObj, chipValue);

            // 8. 存储数据
            ChipButtonData buttonData = new ChipButtonData
            {
                buttonObject = buttonObj,
                button = button,
                chipImage = chipImage,
                borderImage = borderImage,
                rectTransform = buttonRect,
                chipValue = chipValue,
                isSelected = false
            };

            chipButtonDataMap[chipValue] = buttonData;

            if (enableDebugMode)
                Debug.Log($"[MoreChipPanel] 成功创建筹码按钮: {chipValue} - {imageName}");

            return true;
        }

        /// <summary>
        /// 创建选中边框
        /// </summary>
        private Image CreateSelectionBorder(GameObject parent)
        {
            GameObject borderObj = new GameObject("SelectionBorder");
            borderObj.transform.SetParent(parent.transform);

            RectTransform borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-borderWidth, -borderWidth);
            borderRect.offsetMax = new Vector2(borderWidth, borderWidth);
            borderRect.localScale = Vector3.one;

            Image borderImage = borderObj.AddComponent<Image>();
            borderImage.color = selectedBorderColor;
            borderImage.sprite = CreateBorderSprite();
            borderImage.enabled = false; // 默认隐藏

            return borderImage;
        }

        /// <summary>
        /// 添加悬停效果
        /// </summary>
        private void AddHoverEffect(GameObject buttonObj, int chipValue)
        {
            EventTrigger trigger = buttonObj.AddComponent<EventTrigger>();
            
            // 鼠标进入
            EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((data) => OnChipHoverEnter(chipValue));
            trigger.triggers.Add(pointerEnter);
            
            // 鼠标离开
            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) => OnChipHoverExit(chipValue));
            trigger.triggers.Add(pointerExit);
        }

        #endregion

        #region 图片加载

        /// <summary>
        /// 生成筹码图片名称
        /// </summary>
        private string GenerateChipImageName(int chipValue)
        {
            switch (chipValue)
            {
                case 1: return "B_01";
                case 5: return "B_05";
                case 10: return "B_10";
                case 20: return "B_20";
                case 50: return "B_50";
                case 100: return "B_100";
                case 500: return "B_500";
                case 1000: return "B_1K";
                case 5000: return "B_5K";
                case 10000: return "B_10K";
                case 20000: return "B_20K";
                case 50000: return "B_50K";
                case 100000: return "B_100K";
                case 200000: return "B_200K";
                case 500000: return "B_500K";
                case 1000000: return "B_1000K";
                default: return $"B_{chipValue}";
            }
        }

        /// <summary>
        /// 加载筹码图片
        /// </summary>
        private Sprite LoadChipSprite(string imageName)
        {
            try
            {
                string fullPath = chipImagePath + imageName;
                Sprite sprite = Resources.Load<Sprite>(fullPath);
                
                if (sprite != null && enableDebugMode)
                    Debug.Log($"[MoreChipPanel] 成功加载图片: {fullPath}");
                
                return sprite;
            }
            catch (Exception ex)
            {
                if (enableDebugMode)
                    Debug.Log($"[MoreChipPanel] 加载图片失败: {chipImagePath + imageName} - {ex.Message}");
                return null;
            }
        }

        #endregion

        #region 选择逻辑

        /// <summary>
        /// 切换筹码选择状态
        /// </summary>
        private void ToggleChipSelection(int chipValue)
        {
            if (currentSelectedChips.Contains(chipValue))
            {
                // 取消选择
                currentSelectedChips.Remove(chipValue);
                if (enableDebugMode)
                    Debug.Log($"[MoreChipPanel] 取消选择: {chipValue}");
            }
            else
            {
                // 添加选择
                if (currentSelectedChips.Count < maxSelectionCount)
                {
                    currentSelectedChips.Add(chipValue);
                    if (enableDebugMode)
                        Debug.Log($"[MoreChipPanel] 选择: {chipValue}");
                }
                else
                {
                    if (enableDebugMode)
                        Debug.Log($"[MoreChipPanel] 已达到最大选择数量: {maxSelectionCount}");
                    return;
                }
            }

            UpdateChipSelectionState(chipValue);
            UpdateStatusText();
        }

        /// <summary>
        /// 更新筹码选择状态
        /// </summary>
        private void UpdateChipSelectionState(int chipValue)
        {
            if (!chipButtonDataMap.ContainsKey(chipValue)) return;

            ChipButtonData data = chipButtonDataMap[chipValue];
            bool isSelected = currentSelectedChips.Contains(chipValue);
            data.isSelected = isSelected;

            // 更新边框
            data.borderImage.enabled = isSelected;
            
            // 更新缩放
            float targetScale = isSelected ? selectedScale : 1f;
            StartCoroutine(AnimateScale(data.rectTransform, targetScale));
        }

        /// <summary>
        /// 更新所有选择状态
        /// </summary>
        private void UpdateAllSelectionStates()
        {
            foreach (var kvp in chipButtonDataMap)
            {
                UpdateChipSelectionState(kvp.Key);
            }
        }

        /// <summary>
        /// 悬停进入
        /// </summary>
        private void OnChipHoverEnter(int chipValue)
        {
            if (!chipButtonDataMap.ContainsKey(chipValue)) return;

            ChipButtonData data = chipButtonDataMap[chipValue];
            
            if (!data.isSelected)
            {
                // 显示悬停边框
                data.borderImage.color = hoverBorderColor;
                data.borderImage.enabled = true;
                
                // 悬停缩放
                StartCoroutine(AnimateScale(data.rectTransform, hoverScale));
            }
        }

        /// <summary>
        /// 悬停离开
        /// </summary>
        private void OnChipHoverExit(int chipValue)
        {
            if (!chipButtonDataMap.ContainsKey(chipValue)) return;

            ChipButtonData data = chipButtonDataMap[chipValue];
            
            if (!data.isSelected)
            {
                // 隐藏悬停边框
                data.borderImage.enabled = false;
                
                // 恢复原始大小
                StartCoroutine(AnimateScale(data.rectTransform, 1f));
            }
            else
            {
                // 恢复选中边框颜色
                data.borderImage.color = selectedBorderColor;
            }
        }

        /// <summary>
        /// 重置选择
        /// </summary>
        private void ResetSelection()
        {
            currentSelectedChips.Clear();
            InitializeSelection();
            UpdateAllSelectionStates();
            UpdateStatusText();

            if (enableDebugMode)
                Debug.Log("[MoreChipPanel] 重置到默认选择");
        }

        /// <summary>
        /// 确认选择
        /// </summary>
        private void ConfirmSelection()
        {
            if (currentSelectedChips.Count == 0)
            {
                if (enableDebugMode)
                    Debug.Log("[MoreChipPanel] 没有选择任何筹码");
                return;
            }

            var sortedChips = currentSelectedChips.OrderBy(x => x).ToArray();
            OnChipsSelected?.Invoke(sortedChips);
            Hide();

            if (enableDebugMode)
                Debug.Log($"[MoreChipPanel] 确认选择: [{string.Join(", ", sortedChips)}]");
        }

        /// <summary>
        /// 更新状态文本
        /// </summary>
        private void UpdateStatusText()
        {
            if (statusText != null)
            {
                statusText.text = $"已选择: {currentSelectedChips.Count}/{maxSelectionCount}";
                
                if (currentSelectedChips.Count > 0)
                {
                    var sortedChips = currentSelectedChips.OrderBy(x => x).ToArray();
                    statusText.text += $" [{string.Join(", ", sortedChips)}]";
                }
            }
        }

        #endregion

        #region 显示隐藏

        /// <summary>
        /// 显示面板
        /// </summary>
        public void Show()
        {
            if (!panelUICreated)
            {
                CreatePanelUI();
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
                isVisible = true;

                // 确保选择状态正确
                UpdateAllSelectionStates();
                UpdateStatusText();

                if (enableAnimation)
                {
                    PlayShowAnimation();
                }

                if (enableDebugMode)
                    Debug.Log("[MoreChipPanel] 面板显示");
            }
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void Hide()
        {
            if (!isVisible) return;

            isVisible = false;

            if (enableAnimation)
            {
                PlayHideAnimation();
            }
            else
            {
                if (panelRoot != null)
                    panelRoot.SetActive(false);
            }

            OnPanelClosed?.Invoke();

            if (enableDebugMode)
                Debug.Log("[MoreChipPanel] 面板隐藏");
        }

        /// <summary>
        /// 切换显示
        /// </summary>
        public void Toggle()
        {
            if (isVisible)
                Hide();
            else
                Show();
        }

        #endregion

        #region 动画

        /// <summary>
        /// 播放显示动画
        /// </summary>
        private void PlayShowAnimation()
        {
            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);
            animationCoroutine = StartCoroutine(ShowAnimation());
        }

        /// <summary>
        /// 播放隐藏动画
        /// </summary>
        private void PlayHideAnimation()
        {
            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);
            animationCoroutine = StartCoroutine(HideAnimation());
        }

        /// <summary>
        /// 显示动画
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
        /// 隐藏动画
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

        /// <summary>
        /// 缩放动画
        /// </summary>
        private IEnumerator AnimateScale(RectTransform target, float targetScale)
        {
            if (target == null) yield break;

            Vector3 startScale = target.localScale;
            Vector3 endScale = Vector3.one * targetScale;
            
            float elapsed = 0f;
            while (elapsed < scaleAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / scaleAnimationDuration;
                t = Mathf.SmoothStep(0, 1, t);
                
                target.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }

            target.localScale = endScale;
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 创建边框Sprite
        /// </summary>
        private Sprite CreateBorderSprite()
        {
            int size = 64;
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float outerRadius = size / 2f - 1;
            float innerRadius = size / 2f - 6;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    if (distance <= outerRadius && distance >= innerRadius)
                    {
                        pixels[y * size + x] = Color.white;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// 获取默认字体
        /// </summary>
        private Font GetDefaultFont()
        {
            try
            {
                Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (font != null) return font;
            }
            catch { }
            
            try
            {
                Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                if (font != null) return font;
            }
            catch { }
            
            try
            {
                Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
                if (fonts != null && fonts.Length > 0)
                    return fonts[0];
            }
            catch { }
            
            return Font.CreateDynamicFontFromOSFont("Arial", 16);
        }

        /// <summary>
        /// 清除所有筹码按钮
        /// </summary>
        private void ClearAllChipButtons()
        {
            foreach (var kvp in chipButtonDataMap)
            {
                if (kvp.Value.buttonObject != null)
                {
                    if (Application.isPlaying)
                        Destroy(kvp.Value.buttonObject);
                    else
                        DestroyImmediate(kvp.Value.buttonObject);
                }
            }
            chipButtonDataMap.Clear();
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 强制显示面板
        /// </summary>
        [ContextMenu("强制显示面板")]
        public void ForceShow()
        {
            panelUICreated = false;
            Show();
        }

        /// <summary>
        /// 设置选中筹码
        /// </summary>
        public void SetSelectedChips(int[] chips)
        {
            currentSelectedChips.Clear();
            if (chips != null)
            {
                foreach (int chip in chips)
                {
                    if (currentSelectedChips.Count < maxSelectionCount)
                        currentSelectedChips.Add(chip);
                }
            }
            
            if (panelUICreated)
            {
                UpdateAllSelectionStates();
                UpdateStatusText();
            }
        }

        /// <summary>
        /// 获取选中筹码
        /// </summary>
        public int[] GetSelectedChips()
        {
            return currentSelectedChips.OrderBy(x => x).ToArray();
        }

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool IsVisible()
        {
            return isVisible;
        }

        #endregion

        #region 调试方法

        /// <summary>
        /// 显示状态
        /// </summary>
        [ContextMenu("显示状态")]
        public void ShowStatus()
        {
            Debug.Log("=== MoreChipPanel 状态 ===");
            Debug.Log($"面板已创建: {panelUICreated}");
            Debug.Log($"是否可见: {isVisible}");
            Debug.Log($"筹码按钮数量: {chipButtonDataMap.Count}");
            Debug.Log($"当前选择: [{string.Join(", ", currentSelectedChips)}]");
            Debug.Log($"图片路径: {chipImagePath}");
        }

        /// <summary>
        /// 清除UI
        /// </summary>
        [ContextMenu("清除UI")]
        public void ClearUI()
        {
            ClearAllChipButtons();
            
            if (panelRoot != null)
            {
                if (Application.isPlaying)
                    Destroy(panelRoot);
                else
                    DestroyImmediate(panelRoot);
                panelRoot = null;
            }
            
            panelUICreated = false;
            isVisible = false;
            
            Debug.Log("[MoreChipPanel] UI已清除");
        }

        #endregion
    }
}
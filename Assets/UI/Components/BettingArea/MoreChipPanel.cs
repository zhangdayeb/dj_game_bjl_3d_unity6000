// Assets/UI/Components/BettingArea/MoreChipPanel.cs
// 筹码配置面板组件 - 修复边框覆盖问题版本
// 修复选中边框覆盖筹码图片的问题，移除悬停效果
// 修复时间: 2025/6/27

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
    /// 筹码配置面板组件 - 修复版本
    /// 专业黑色系风格，修复边框覆盖问题
    /// </summary>
    public class MoreChipPanel : MonoBehaviour
    {
        #region 序列化字段

        [Header("自动显示设置")]
        public bool autoCreateAndShow = false;
        public bool showOnAwake = false;

        [Header("面板尺寸和位置")]
        public Vector2 panelSize = new Vector2(650, 500);
        
        [Header("专业黑色系风格")]
        public Color panelBackgroundColor = new Color(0.06f, 0.06f, 0.06f, 0.98f);  // 深黑背景
        public Color maskColor = new Color(0f, 0f, 0f, 0.75f);                      // 遮罩
        public Color headerColor = new Color(0.1f, 0.1f, 0.1f, 1f);                // 头部背景
        public Color contentBackgroundColor = new Color(0.08f, 0.08f, 0.08f, 1f);   // 内容背景
        public Color borderColor = new Color(0.3f, 0.3f, 0.3f, 1f);                // 边框

        [Header("筹码配置 - 与您的图片文件完全对应")]
        public int[] allAvailableChips = { 1, 5, 10, 20, 50, 100, 500, 1000, 5000, 10000, 20000, 50000, 100000 };
        
        [Header("默认选择")]
        public int[] defaultSelectedChips = { 5, 10, 20, 50, 100 };
        public int maxSelectionCount = 5;
        
        [Header("网格布局")]
        public Vector2 buttonSize = new Vector2(75, 75);
        public Vector2 spacing = new Vector2(12, 12);
        public int columnsPerRow = 6;
        public float topPadding = 20f;
        public float sidePadding = 25f;
        
        [Header("选中效果 - 优化版")]
        public Color selectedBorderColor = new Color(0f, 1f, 0.6f, 1f);            // 亮绿色
        public Color chipNormalColor = new Color(1f, 1f, 1f, 1f);                  // 正常白色
        public Color chipSelectedColor = new Color(1f, 1f, 1f, 1f);                // 选中白色
        public float selectedScale = 1.08f;                                        // 轻微缩放
        public float borderWidth = 3f;                                             // 边框宽度
        
        [Header("文字设置")]
        public int titleFontSize = 22;
        public int statusFontSize = 16;
        public int buttonFontSize = 14;
        public Color titleColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        public Color textColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        public Color selectedTextColor = new Color(1f, 0.9f, 0.5f, 1f);
        
        [Header("动画设置")]
        public bool enableAnimation = true;
        public float animationDuration = 0.3f;
        public float scaleAnimationDuration = 0.2f;
        
        [Header("调试设置")]
        public bool enableDebugMode = true;

        #endregion

        #region 事件定义

        public System.Action<int[]> OnChipsSelected;
        public System.Action OnPanelClosed;

        #endregion

        #region 私有字段

        private Canvas parentCanvas;
        private bool panelUICreated = false;
        private bool isVisible = false;
        private Coroutine animationCoroutine;
        
        // UI组件
        private GameObject panelRoot;
        private GameObject mainPanel;
        private Transform chipContainer;
        private Text statusText;
        private Button confirmButton;
        private Button resetButton;
        private ScrollRect scrollView;
        private GridLayoutGroup gridLayout;
        
        // 筹码数据
        private List<int> currentSelectedChips = new List<int>();
        private Dictionary<int, ChipButtonData> chipButtonDataMap = new Dictionary<int, ChipButtonData>();

        #endregion

        #region 数据结构

        private class ChipButtonData
        {
            public GameObject buttonObject;
            public Button button;
            public Image chipImage;
            public Outline outline;             // 改用Outline替代边框
            public RectTransform rectTransform;
            public int chipValue;
            public bool isSelected;
        }

        #endregion

        #region 生命周期

        private void Awake()
        {
            // 强制重置数组为默认值
            allAvailableChips = new int[] { 1, 5, 10, 20, 50, 100, 500, 1000, 5000, 10000, 20000, 50000, 100000 };
            
            InitializeComponent();
        }

        private void Start()
        {
            if (autoCreateAndShow && showOnAwake)
            {
                Show();
            }
        }

        private void OnDestroy()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
        }

        #endregion

        #region 初始化

        private void InitializeComponent()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                CreateCanvasIfNeeded();
            }

            InitializeSelection();

            if (enableDebugMode)
                Debug.Log("[MoreChipPanel] 组件初始化完成");
        }

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

        private void CreateCanvasIfNeeded()
        {
            GameObject canvasObj = new GameObject("MoreChipPanelCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            transform.SetParent(canvasObj.transform, false);
            parentCanvas = canvas;
        }

        #endregion

        #region 面板创建

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
                CreateBottomPanel();
                GenerateChipButtons();
                
                panelUICreated = true;
                panelRoot.SetActive(false);

                if (enableDebugMode)
                    Debug.Log("[MoreChipPanel] 专业黑色系面板创建完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MoreChipPanel] 创建面板失败: {ex.Message}");
            }
        }

        private void CreateRootPanel()
        {
            panelRoot = new GameObject("MoreChipPanelRoot");
            panelRoot.transform.SetParent(parentCanvas.transform, false);

            RectTransform rootRect = panelRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.sizeDelta = Vector2.zero;
            rootRect.anchoredPosition = Vector2.zero;
        }

        private void CreateMaskBackground()
        {
            GameObject maskObj = new GameObject("MaskBackground");
            maskObj.transform.SetParent(panelRoot.transform, false);

            RectTransform maskRect = maskObj.AddComponent<RectTransform>();
            maskRect.anchorMin = Vector2.zero;
            maskRect.anchorMax = Vector2.one;
            maskRect.sizeDelta = Vector2.zero;
            maskRect.anchoredPosition = Vector2.zero;

            Image maskImage = maskObj.AddComponent<Image>();
            maskImage.color = maskColor;

            Button maskButton = maskObj.AddComponent<Button>();
            maskButton.onClick.AddListener(Hide);
        }

        private void CreateMainPanel()
        {
            mainPanel = new GameObject("MainPanel");
            mainPanel.transform.SetParent(panelRoot.transform, false);

            RectTransform mainRect = mainPanel.AddComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0.5f, 0.5f);
            mainRect.anchorMax = new Vector2(0.5f, 0.5f);
            mainRect.sizeDelta = panelSize;
            mainRect.anchoredPosition = Vector2.zero; // 确保居中

            Image mainImage = mainPanel.AddComponent<Image>();
            mainImage.color = panelBackgroundColor;

            // 添加边框效果
            Outline outline = mainPanel.AddComponent<Outline>();
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(2, -2);

            // 添加阴影
            Shadow shadow = mainPanel.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(4, -4);
        }

        private void CreateHeaderPanel()
        {
            GameObject headerObj = new GameObject("HeaderPanel");
            headerObj.transform.SetParent(mainPanel.transform, false);

            RectTransform headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.sizeDelta = new Vector2(0, 60);
            headerRect.anchoredPosition = new Vector2(0, -30);

            Image headerImage = headerObj.AddComponent<Image>();
            headerImage.color = headerColor;

            CreateTitleText(headerObj);
            CreateCloseButton(headerObj);
        }

        private void CreateTitleText(GameObject parent)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent.transform, false);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0);
            titleRect.anchorMax = new Vector2(0.8f, 1);
            titleRect.sizeDelta = Vector2.zero;
            titleRect.anchoredPosition = new Vector2(0, 0);

            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "选择筹码 (最多5个)";
            titleText.font = GetDefaultFont();
            titleText.fontSize = titleFontSize;
            titleText.color = titleColor;
            titleText.alignment = TextAnchor.MiddleLeft;
            titleText.fontStyle = FontStyle.Bold;
            
            // 添加左边距
            titleRect.offsetMin = new Vector2(25, 0);
        }

        private void CreateCloseButton(GameObject parent)
        {
            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(parent.transform, false);

            RectTransform closeRect = closeObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 0.5f);
            closeRect.anchorMax = new Vector2(1, 0.5f);
            closeRect.sizeDelta = new Vector2(45, 45);
            closeRect.anchoredPosition = new Vector2(-35, 0);

            Image closeImage = closeObj.AddComponent<Image>();
            closeImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);

            Button closeBtn = closeObj.AddComponent<Button>();
            closeBtn.onClick.AddListener(Hide);

            CreateButtonText(closeObj, "×", 28, Color.white);
        }

        private void CreateContentArea()
        {
            GameObject contentObj = new GameObject("ContentArea");
            contentObj.transform.SetParent(mainPanel.transform, false);

            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(0, 80);  // 底部留空间
            contentRect.offsetMax = new Vector2(0, -60); // 顶部留空间

            Image contentImage = contentObj.AddComponent<Image>();
            contentImage.color = contentBackgroundColor;

            CreateScrollView(contentObj);
        }

        private void CreateScrollView(GameObject parent)
        {
            GameObject scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(parent.transform, false);

            RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.sizeDelta = Vector2.zero;
            scrollRect.anchoredPosition = Vector2.zero;

            scrollView = scrollObj.AddComponent<ScrollRect>();
            scrollView.horizontal = false;
            scrollView.vertical = true;
            scrollView.movementType = ScrollRect.MovementType.Clamped;

            CreateChipGrid(scrollObj);
        }

        private void CreateChipGrid(GameObject parent)
        {
            GameObject gridObj = new GameObject("ChipGrid");
            gridObj.transform.SetParent(parent.transform, false);

            RectTransform gridRect = gridObj.AddComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0, 1);
            gridRect.anchorMax = new Vector2(1, 1);
            gridRect.pivot = new Vector2(0.5f, 1);
            gridRect.anchoredPosition = Vector2.zero;

            gridLayout = gridObj.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = buttonSize;
            gridLayout.spacing = spacing;
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = columnsPerRow;
            gridLayout.padding = new RectOffset((int)sidePadding, (int)sidePadding, (int)topPadding, 0);

            ContentSizeFitter contentSizeFitter = gridObj.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollView.content = gridRect;
            chipContainer = gridObj.transform;
        }

        private void CreateBottomPanel()
        {
            GameObject bottomObj = new GameObject("BottomPanel");
            bottomObj.transform.SetParent(mainPanel.transform, false);

            RectTransform bottomRect = bottomObj.AddComponent<RectTransform>();
            bottomRect.anchorMin = new Vector2(0, 0);
            bottomRect.anchorMax = new Vector2(1, 0);
            bottomRect.sizeDelta = new Vector2(0, 80);
            bottomRect.anchoredPosition = new Vector2(0, 40);

            Image bottomImage = bottomObj.AddComponent<Image>();
            bottomImage.color = headerColor;

            CreateStatusText(bottomObj);
            CreateActionButtons(bottomObj);
        }

        private void CreateStatusText(GameObject parent)
        {
            GameObject statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(parent.transform, false);

            RectTransform statusRect = statusObj.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0, 0);
            statusRect.anchorMax = new Vector2(0.5f, 1);
            statusRect.sizeDelta = Vector2.zero;
            statusRect.anchoredPosition = Vector2.zero;
            statusRect.offsetMin = new Vector2(25, 0);

            statusText = statusObj.AddComponent<Text>();
            statusText.font = GetDefaultFont();
            statusText.fontSize = statusFontSize;
            statusText.color = textColor;
            statusText.alignment = TextAnchor.MiddleLeft;
        }

        private void CreateActionButtons(GameObject parent)
        {
            // 重置按钮
            GameObject resetObj = new GameObject("ResetButton");
            resetObj.transform.SetParent(parent.transform, false);

            RectTransform resetRect = resetObj.AddComponent<RectTransform>();
            resetRect.anchorMin = new Vector2(0.5f, 0.5f);
            resetRect.anchorMax = new Vector2(0.5f, 0.5f);
            resetRect.sizeDelta = new Vector2(100, 50);
            resetRect.anchoredPosition = new Vector2(-60, 0);

            Image resetImage = resetObj.AddComponent<Image>();
            resetImage.color = new Color(0.4f, 0.4f, 0.4f, 1f);

            resetButton = resetObj.AddComponent<Button>();
            resetButton.onClick.AddListener(ResetSelection);

            CreateButtonText(resetObj, "重置", buttonFontSize, Color.white);

            // 确认按钮
            GameObject confirmObj = new GameObject("ConfirmButton");
            confirmObj.transform.SetParent(parent.transform, false);

            RectTransform confirmRect = confirmObj.AddComponent<RectTransform>();
            confirmRect.anchorMin = new Vector2(0.5f, 0.5f);
            confirmRect.anchorMax = new Vector2(0.5f, 0.5f);
            confirmRect.sizeDelta = new Vector2(100, 50);
            confirmRect.anchoredPosition = new Vector2(60, 0);

            Image confirmImage = confirmObj.AddComponent<Image>();
            confirmImage.color = new Color(1f, 0.7f, 0f, 1f); // 金色

            confirmButton = confirmObj.AddComponent<Button>();
            confirmButton.onClick.AddListener(ConfirmSelection);

            CreateButtonText(confirmObj, "确认", buttonFontSize, Color.white);
        }

        private void CreateButtonText(GameObject parent, string text, int fontSize, Color color)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parent.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = text;
            buttonText.font = GetDefaultFont();
            buttonText.fontSize = fontSize;
            buttonText.color = color;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.fontStyle = FontStyle.Bold;
        }

        #endregion

        #region 筹码按钮生成

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

            UpdateAllSelectionStates();
            UpdateStatusText();
        }

        private bool CreateChipButton(int chipValue)
        {
            // 1. 创建按钮对象
            GameObject buttonObj = new GameObject($"ChipButton_{chipValue}");
            buttonObj.transform.SetParent(chipContainer, false);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = buttonSize;

            // 2. 加载筹码图片
            string imageName = GetChipImageName(chipValue);
            Sprite chipSprite = LoadChipSprite(imageName);

            // 3. 创建筹码图片
            Image chipImage = buttonObj.AddComponent<Image>();
            if (chipSprite != null)
            {
                chipImage.sprite = chipSprite;
                chipImage.color = chipNormalColor;
                chipImage.preserveAspect = true;
            }
            else
            {
                // 使用备用纯色
                chipImage.color = GetFallbackColor(chipValue);
                chipImage.sprite = CreateSolidSprite();
                
                // 添加数值文字
                CreateChipValueText(buttonObj, chipValue);
            }

            // 4. 添加Button组件
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = chipImage;
            
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.disabledColor = Color.gray;
            button.colors = colors;

            // 5. 设置点击事件 (移除悬停效果)
            button.onClick.AddListener(() => ToggleChipSelection(chipValue));

            // 6. 保存数据
            ChipButtonData buttonData = new ChipButtonData
            {
                buttonObject = buttonObj,
                button = button,
                chipImage = chipImage,
                outline = null,  // 初始没有outline
                rectTransform = buttonRect,
                chipValue = chipValue,
                isSelected = false
            };

            chipButtonDataMap[chipValue] = buttonData;

            if (enableDebugMode)
            {
                string resourceType = chipSprite != null ? "图片" : "纯色";
                Debug.Log($"[MoreChipPanel] ✅ 创建筹码: {chipValue} ({resourceType})");
            }

            return true;
        }

        /// <summary>
        /// 获取筹码图片名称 - 完全匹配您的文件命名
        /// </summary>
        private string GetChipImageName(int chipValue)
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
                case 1000000: return "B_1M";
                case 10000000: return "B_10M";
                case 20000000: return "B_20M";
                case 50000000: return "B_50M";
                case 100000000: return "B_100M";
                case 200000000: return "B_200M";
                case 500000000: return "B_500M";
                case 1000000000: return "B_1000M";
                default: return $"B_{chipValue}";
            }
        }

        private Sprite LoadChipSprite(string imageName)
        {
            try
            {
                string[] paths = { "Images/chips/", "Images/" };
                string[] extensions = { ".png", "", ".jpg", ".jpeg" };
                
                foreach (string path in paths)
                {
                    foreach (string ext in extensions)
                    {
                        string fullPath = path + imageName + ext;
                        Sprite sprite = Resources.Load<Sprite>(fullPath);
                        
                        if (sprite != null)
                        {
                            if (enableDebugMode)
                                Debug.Log($"[MoreChipPanel] ✅ 成功加载: {fullPath}");
                            return sprite;
                        }
                    }
                }
                
                if (enableDebugMode)
                    Debug.Log($"[MoreChipPanel] ⚠️ 图片未找到，使用纯色: {imageName}");
                
                return null;
            }
            catch (Exception ex)
            {
                if (enableDebugMode)
                    Debug.LogError($"[MoreChipPanel] ❌ 加载异常: {imageName} - {ex.Message}");
                return null;
            }
        }

        private Color GetFallbackColor(int chipValue)
        {
            // 根据筹码数值生成颜色
            Color[] colors = {
                new Color(0.9f, 0.1f, 0.1f, 1f), // 红
                new Color(0.2f, 0.8f, 0.2f, 1f), // 绿
                new Color(0.1f, 0.4f, 0.9f, 1f), // 蓝
                new Color(0.9f, 0.5f, 0.1f, 1f), // 橙
                new Color(0.7f, 0.2f, 0.8f, 1f), // 紫
                new Color(0.9f, 0.9f, 0.1f, 1f), // 黄
                new Color(0.1f, 0.8f, 0.8f, 1f), // 青
                new Color(0.8f, 0.3f, 0.5f, 1f)  // 粉
            };
            
            int index = Array.IndexOf(allAvailableChips, chipValue) % colors.Length;
            return index >= 0 ? colors[index] : colors[0];
        }

        private Sprite CreateSolidSprite()
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }

        private void CreateChipValueText(GameObject parent, int chipValue)
        {
            GameObject textObj = new GameObject("ValueText");
            textObj.transform.SetParent(parent.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            Text valueText = textObj.AddComponent<Text>();
            valueText.text = FormatChipValue(chipValue);
            valueText.font = GetDefaultFont();
            valueText.fontSize = buttonFontSize;
            valueText.color = Color.white;
            valueText.alignment = TextAnchor.MiddleCenter;
            valueText.fontStyle = FontStyle.Bold;

            // 添加阴影
            Shadow textShadow = textObj.AddComponent<Shadow>();
            textShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            textShadow.effectDistance = new Vector2(1, -1);
        }

        private string FormatChipValue(int value)
        {
            if (value >= 1000000000) return $"{value / 1000000000}B";
            if (value >= 1000000) return $"{value / 1000000}M";
            if (value >= 1000) return $"{value / 1000}K";
            return value.ToString();
        }

        private void ClearAllChipButtons()
        {
            foreach (var pair in chipButtonDataMap)
            {
                if (pair.Value.buttonObject != null)
                {
                    if (Application.isPlaying)
                        Destroy(pair.Value.buttonObject);
                    else
                        DestroyImmediate(pair.Value.buttonObject);
                }
            }
            chipButtonDataMap.Clear();
        }

        private Font GetDefaultFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        #endregion

        #region 选择逻辑

        private void ToggleChipSelection(int chipValue)
        {
            if (currentSelectedChips.Contains(chipValue))
            {
                currentSelectedChips.Remove(chipValue);
                if (enableDebugMode)
                    Debug.Log($"[MoreChipPanel] 取消选择: {chipValue}");
            }
            else
            {
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

        private void UpdateChipSelectionState(int chipValue)
        {
            if (!chipButtonDataMap.ContainsKey(chipValue)) return;

            ChipButtonData data = chipButtonDataMap[chipValue];
            bool isSelected = currentSelectedChips.Contains(chipValue);
            data.isSelected = isSelected;

            // 使用Outline替代边框，避免覆盖图片
            if (isSelected)
            {
                // 添加outline
                if (data.outline == null)
                {
                    data.outline = data.buttonObject.AddComponent<Outline>();
                    data.outline.effectColor = selectedBorderColor;
                    data.outline.effectDistance = new Vector2(borderWidth, -borderWidth);
                }
            }
            else
            {
                // 移除outline
                if (data.outline != null)
                {
                    if (Application.isPlaying)
                        Destroy(data.outline);
                    else
                        DestroyImmediate(data.outline);
                    data.outline = null;
                }
            }
            
            // 缩放效果
            float targetScale = isSelected ? selectedScale : 1f;
            if (enableAnimation)
            {
                StartCoroutine(AnimateScale(data.rectTransform, targetScale));
            }
            else
            {
                data.rectTransform.localScale = Vector3.one * targetScale;
            }

            // 颜色调整
            data.chipImage.color = isSelected ? chipSelectedColor : chipNormalColor;
        }

        private void UpdateAllSelectionStates()
        {
            foreach (var pair in chipButtonDataMap)
            {
                UpdateChipSelectionState(pair.Key);
            }
        }

        private void ResetSelection()
        {
            currentSelectedChips.Clear();
            foreach (int chip in defaultSelectedChips)
            {
                if (currentSelectedChips.Count < maxSelectionCount)
                {
                    currentSelectedChips.Add(chip);
                }
            }
            
            UpdateAllSelectionStates();
            UpdateStatusText();

            if (enableDebugMode)
                Debug.Log($"[MoreChipPanel] 重置选择: [{string.Join(", ", currentSelectedChips)}]");
        }

        private void ConfirmSelection()
        {
            if (currentSelectedChips.Count == 0)
            {
                if (enableDebugMode)
                    Debug.Log("[MoreChipPanel] 没有选择筹码");
                return;
            }

            var sortedChips = currentSelectedChips.OrderBy(x => x).ToArray();
            OnChipsSelected?.Invoke(sortedChips);
            Hide();

            if (enableDebugMode)
                Debug.Log($"[MoreChipPanel] 确认选择: [{string.Join(", ", sortedChips)}]");
        }

        private void UpdateStatusText()
        {
            if (statusText != null)
            {
                statusText.text = $"已选择: {currentSelectedChips.Count}/{maxSelectionCount}";
                
                if (currentSelectedChips.Count > 0)
                {
                    var sortedChips = currentSelectedChips.OrderBy(x => x).ToArray();
                    var formattedChips = sortedChips.Select(FormatChipValue);
                    statusText.text += $"\n[{string.Join(", ", formattedChips)}]";
                }
            }
        }

        #endregion

        #region 动画效果

        private IEnumerator AnimateScale(RectTransform target, float targetScale)
        {
            Vector3 startScale = target.localScale;
            Vector3 endScale = Vector3.one * targetScale;
            float elapsed = 0f;

            while (elapsed < scaleAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / scaleAnimationDuration;
                target.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }

            target.localScale = endScale;
        }

        private IEnumerator ShowAnimationCoroutine()
        {
            mainPanel.transform.localScale = Vector3.zero;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                float scale = Mathf.Lerp(0f, 1f, t);
                mainPanel.transform.localScale = Vector3.one * scale;
                yield return null;
            }

            mainPanel.transform.localScale = Vector3.one;
            animationCoroutine = null;
        }

        private IEnumerator HideAnimationCoroutine()
        {
            mainPanel.transform.localScale = Vector3.one;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                float scale = Mathf.Lerp(1f, 0f, t);
                mainPanel.transform.localScale = Vector3.one * scale;
                yield return null;
            }

            if (panelRoot != null)
                panelRoot.SetActive(false);
            animationCoroutine = null;
        }

        #endregion

        #region 显示隐藏

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

                UpdateAllSelectionStates();
                UpdateStatusText();

                if (enableAnimation)
                {
                    if (animationCoroutine != null)
                        StopCoroutine(animationCoroutine);
                    animationCoroutine = StartCoroutine(ShowAnimationCoroutine());
                }

                if (enableDebugMode)
                    Debug.Log("[MoreChipPanel] 面板显示");
            }
        }

        public void Hide()
        {
            if (!isVisible) return;

            isVisible = false;

            if (enableAnimation)
            {
                if (animationCoroutine != null)
                    StopCoroutine(animationCoroutine);
                animationCoroutine = StartCoroutine(HideAnimationCoroutine());
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

        #endregion

        #region 公共接口

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

        public int[] GetSelectedChips()
        {
            return currentSelectedChips.OrderBy(x => x).ToArray();
        }

        public bool IsVisible()
        {
            return isVisible;
        }

        #endregion

        #region 调试方法

        [ContextMenu("显示状态")]
        public void ShowStatus()
        {
            Debug.Log("=== MoreChipPanel 状态 ===");
            Debug.Log($"面板已创建: {panelUICreated}");
            Debug.Log($"是否可见: {isVisible}");
            Debug.Log($"筹码按钮数量: {chipButtonDataMap.Count}");
            Debug.Log($"当前选择: [{string.Join(", ", currentSelectedChips)}]");
            
            int imageCount = 0;
            int colorCount = 0;
            foreach (var data in chipButtonDataMap.Values)
            {
                if (data.chipImage.sprite != null && data.chipImage.sprite.name != "")
                    imageCount++;
                else
                    colorCount++;
            }
            Debug.Log($"图片筹码: {imageCount}, 纯色筹码: {colorCount}");
        }

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
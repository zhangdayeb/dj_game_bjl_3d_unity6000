// Assets/UI/Components/VideoOverlay/Set/MoreChipPanel.cs
// 简化版筹码配置面板组件 - 仅用于UI生成
// 挂载到节点上自动创建筹码选择面板UI
// 创建时间: 2025/6/28

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace BaccaratGame.UI.Components.VideoOverlay
{
    /// <summary>
    /// 简化版筹码配置面板组件
    /// 挂载到节点上自动创建UI，包含筹码选择和网格布局
    /// </summary>
    public class MoreChipPanel : MonoBehaviour
    {
        #region 配置参数

        [Header("面板配置")]
        public Vector2 panelSize = new Vector2(600, 450);
        public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        public Color headerColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        public Color titleColor = Color.white;
        public int fontSize = 14;
        
        [Header("遮罩层设置")]
        public Color maskColor = new Color(0, 0, 0, 0.5f);
        
        [Header("筹码配置")]
        public int[] allChips = { 1, 5, 10, 20, 50, 100, 500, 1000, 5000, 10000 };
        public int[] defaultSelected = { 5, 10, 20, 50, 100 };
        public int maxSelection = 5;
        
        [Header("网格布局")]
        public Vector2 chipSize = new Vector2(70, 70);
        public Vector2 chipSpacing = new Vector2(10, 10);
        public int columnsPerRow = 5;
        
        [Header("选中效果")]
        public Color selectedBorderColor = new Color(0f, 1f, 0.6f, 1f);
        public float borderWidth = 3f;

        #endregion

        #region 私有字段

        private bool uiCreated = false;
        private GameObject maskLayer;
        private GameObject chipPanel;
        private Canvas uiCanvas;
        
        // UI组件引用
        private Transform chipContainer;
        private Text statusText;
        private Button confirmButton;
        private Button resetButton;
        
        // 筹码数据
        private List<int> selectedChips = new List<int>();
        private Dictionary<int, ChipData> chipDataMap = new Dictionary<int, ChipData>();

        #endregion

        #region 数据结构

        private class ChipData
        {
            public GameObject chipObject;
            public Button button;
            public Image chipImage;
            public Outline outline;
            public int value;
            public bool isSelected;
        }

        #endregion

        #region 生命周期

        private void Awake()
        {
            CreateUI();
            InitializeSelection();
        }

        #endregion

        #region UI创建

        /// <summary>
        /// 创建完整的UI系统
        /// </summary>
        private void CreateUI()
        {
            if (uiCreated) return;

            CreateCanvas();
            CreateMaskLayer();
            CreateChipPanel();
            CreatePanelHeader();
            CreateChipGrid();
            CreatePanelFooter();
            
            uiCreated = true;
        }

        /// <summary>
        /// 创建Canvas
        /// </summary>
        private void CreateCanvas()
        {
            uiCanvas = GetComponentInParent<Canvas>();
            if (uiCanvas == null)
            {
                GameObject canvasObj = new GameObject("ChipPanelCanvas");
                canvasObj.transform.SetParent(transform.parent);
                
                uiCanvas = canvasObj.AddComponent<Canvas>();
                uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                uiCanvas.sortingOrder = 2500; // 确保在最上层
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                
                canvasObj.AddComponent<GraphicRaycaster>();
                
                transform.SetParent(canvasObj.transform);
            }

            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = gameObject.AddComponent<RectTransform>();
                
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// 创建遮罩层
        /// </summary>
        private void CreateMaskLayer()
        {
            maskLayer = new GameObject("MaskLayer");
            maskLayer.transform.SetParent(transform);

            RectTransform maskRect = maskLayer.AddComponent<RectTransform>();
            maskRect.anchorMin = Vector2.zero;
            maskRect.anchorMax = Vector2.one;
            maskRect.offsetMin = Vector2.zero;
            maskRect.offsetMax = Vector2.zero;

            Image maskImage = maskLayer.AddComponent<Image>();
            maskImage.color = maskColor;
            maskImage.sprite = CreateSimpleSprite();

            Button maskButton = maskLayer.AddComponent<Button>();
            maskButton.onClick.AddListener(HidePanel);
            
            ColorBlock colors = maskButton.colors;
            colors.normalColor = Color.clear;
            colors.highlightedColor = Color.clear;
            colors.pressedColor = Color.clear;
            colors.disabledColor = Color.clear;
            maskButton.colors = colors;
        }

        /// <summary>
        /// 创建筹码面板
        /// </summary>
        private void CreateChipPanel()
        {
            chipPanel = new GameObject("ChipPanel");
            chipPanel.transform.SetParent(transform);

            RectTransform panelRect = chipPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f); // 居中
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = panelSize;
            panelRect.anchoredPosition = Vector2.zero;

            Image panelBg = chipPanel.AddComponent<Image>();
            panelBg.color = backgroundColor;
            panelBg.sprite = CreateSimpleSprite();

            // 添加边框效果
            Outline outline = chipPanel.AddComponent<Outline>();
            outline.effectColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            outline.effectDistance = new Vector2(2, -2);
        }

        /// <summary>
        /// 创建面板头部
        /// </summary>
        private void CreatePanelHeader()
        {
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(chipPanel.transform);

            RectTransform headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.85f);
            headerRect.anchorMax = new Vector2(1, 1f);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;

            Image headerBg = headerObj.AddComponent<Image>();
            headerBg.color = headerColor;
            headerBg.sprite = CreateSimpleSprite();

            // 创建标题
            CreateTitle(headerObj);
            
            // 创建关闭按钮
            CreateCloseButton(headerObj);
        }

        /// <summary>
        /// 创建标题
        /// </summary>
        private void CreateTitle(GameObject parent)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent.transform);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0);
            titleRect.anchorMax = new Vector2(0.8f, 1f);
            titleRect.offsetMin = new Vector2(15, 0);
            titleRect.offsetMax = Vector2.zero;

            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = $"🪙 选择筹码 (最多{maxSelection}个)";
            titleText.color = titleColor;
            titleText.alignment = TextAnchor.MiddleLeft;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = fontSize + 4;
            titleText.fontStyle = FontStyle.Bold;
        }

        /// <summary>
        /// 创建关闭按钮
        /// </summary>
        private void CreateCloseButton(GameObject parent)
        {
            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(parent.transform);

            RectTransform closeRect = closeObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.85f, 0.1f);
            closeRect.anchorMax = new Vector2(0.95f, 0.9f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;

            Button closeBtn = closeObj.AddComponent<Button>();
            
            Image closeImage = closeObj.AddComponent<Image>();
            closeImage.color = Color.red;
            closeImage.sprite = CreateSimpleSprite();

            closeBtn.onClick.AddListener(HidePanel);

            // 关闭按钮文字
            GameObject closeTextObj = new GameObject("Text");
            closeTextObj.transform.SetParent(closeObj.transform);

            RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.offsetMin = Vector2.zero;
            closeTextRect.offsetMax = Vector2.zero;

            Text closeText = closeTextObj.AddComponent<Text>();
            closeText.text = "✕";
            closeText.color = Color.white;
            closeText.alignment = TextAnchor.MiddleCenter;
            closeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            closeText.fontSize = fontSize + 2;
            closeText.fontStyle = FontStyle.Bold;
        }

        /// <summary>
        /// 创建筹码网格
        /// </summary>
        private void CreateChipGrid()
        {
            GameObject gridObj = new GameObject("ChipGrid");
            gridObj.transform.SetParent(chipPanel.transform);

            RectTransform gridRect = gridObj.AddComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0, 0.2f);
            gridRect.anchorMax = new Vector2(1, 0.85f);
            gridRect.offsetMin = new Vector2(15, 0);
            gridRect.offsetMax = new Vector2(-15, 0);

            // 添加网格布局
            GridLayoutGroup gridLayout = gridObj.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = chipSize;
            gridLayout.spacing = chipSpacing;
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = columnsPerRow;
            gridLayout.padding = new RectOffset(10, 10, 10, 10);

            chipContainer = gridObj.transform;
            
            // 创建所有筹码按钮
            CreateChipButtons();
        }

        /// <summary>
        /// 创建面板底部
        /// </summary>
        private void CreatePanelFooter()
        {
            GameObject footerObj = new GameObject("Footer");
            footerObj.transform.SetParent(chipPanel.transform);

            RectTransform footerRect = footerObj.AddComponent<RectTransform>();
            footerRect.anchorMin = new Vector2(0, 0);
            footerRect.anchorMax = new Vector2(1, 0.2f);
            footerRect.offsetMin = Vector2.zero;
            footerRect.offsetMax = Vector2.zero;

            Image footerBg = footerObj.AddComponent<Image>();
            footerBg.color = headerColor;
            footerBg.sprite = CreateSimpleSprite();

            // 状态文字
            CreateStatusText(footerObj);
            
            // 操作按钮
            CreateActionButtons(footerObj);
        }

        /// <summary>
        /// 创建状态文字
        /// </summary>
        private void CreateStatusText(GameObject parent)
        {
            GameObject statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(parent.transform);

            RectTransform statusRect = statusObj.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0, 0);
            statusRect.anchorMax = new Vector2(0.5f, 1f);
            statusRect.offsetMin = new Vector2(15, 0);
            statusRect.offsetMax = Vector2.zero;

            statusText = statusObj.AddComponent<Text>();
            statusText.text = $"已选择: 0/{maxSelection}";
            statusText.color = Color.white;
            statusText.alignment = TextAnchor.MiddleLeft;
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.fontSize = fontSize;
        }

        /// <summary>
        /// 创建操作按钮
        /// </summary>
        private void CreateActionButtons(GameObject parent)
        {
            // 重置按钮
            GameObject resetObj = new GameObject("ResetButton");
            resetObj.transform.SetParent(parent.transform);

            RectTransform resetRect = resetObj.AddComponent<RectTransform>();
            resetRect.anchorMin = new Vector2(0.55f, 0.2f);
            resetRect.anchorMax = new Vector2(0.75f, 0.8f);
            resetRect.offsetMin = Vector2.zero;
            resetRect.offsetMax = Vector2.zero;

            resetButton = resetObj.AddComponent<Button>();
            
            Image resetImage = resetObj.AddComponent<Image>();
            resetImage.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            resetImage.sprite = CreateSimpleSprite();

            resetButton.onClick.AddListener(ResetSelection);

            CreateButtonText(resetObj, "重置");

            // 确认按钮
            GameObject confirmObj = new GameObject("ConfirmButton");
            confirmObj.transform.SetParent(parent.transform);

            RectTransform confirmRect = confirmObj.AddComponent<RectTransform>();
            confirmRect.anchorMin = new Vector2(0.78f, 0.2f);
            confirmRect.anchorMax = new Vector2(0.98f, 0.8f);
            confirmRect.offsetMin = Vector2.zero;
            confirmRect.offsetMax = Vector2.zero;

            confirmButton = confirmObj.AddComponent<Button>();
            
            Image confirmImage = confirmObj.AddComponent<Image>();
            confirmImage.color = new Color(0.2f, 0.8f, 0.2f, 1f);
            confirmImage.sprite = CreateSimpleSprite();

            confirmButton.onClick.AddListener(ConfirmSelection);

            CreateButtonText(confirmObj, "确认");
        }

        /// <summary>
        /// 创建按钮文字
        /// </summary>
        private void CreateButtonText(GameObject parent, string text)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parent.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = text;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = fontSize;
            buttonText.fontStyle = FontStyle.Bold;
        }

        /// <summary>
        /// 创建简单背景
        /// </summary>
        private Sprite CreateSimpleSprite()
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }

        #endregion

        #region 筹码按钮创建

        /// <summary>
        /// 创建所有筹码按钮
        /// </summary>
        private void CreateChipButtons()
        {
            foreach (int chipValue in allChips)
            {
                CreateSingleChipButton(chipValue);
            }
            
            UpdateAllSelectionStates();
            UpdateStatusText();
        }

        /// <summary>
        /// 创建单个筹码按钮
        /// </summary>
        private void CreateSingleChipButton(int chipValue)
        {
            GameObject chipObj = new GameObject($"Chip_{chipValue}");
            chipObj.transform.SetParent(chipContainer);

            RectTransform chipRect = chipObj.AddComponent<RectTransform>();
            chipRect.sizeDelta = chipSize;

            // 筹码图片
            Image chipImage = chipObj.AddComponent<Image>();
            
            // 尝试加载筹码图片
            Sprite chipSprite = LoadChipSprite(chipValue);
            if (chipSprite != null)
            {
                chipImage.sprite = chipSprite;
                chipImage.color = Color.white; // 使用原图颜色
                chipImage.preserveAspect = true;
            }
            else
            {
                // 如果没有图片，使用纯色背景
                chipImage.color = GetChipColor(chipValue);
                chipImage.sprite = CreateSimpleSprite();
                // 添加数值文字
                CreateChipValueText(chipObj, chipValue);
            }

            // 按钮组件
            Button chipButton = chipObj.AddComponent<Button>();
            chipButton.targetGraphic = chipImage;
            chipButton.onClick.AddListener(() => ToggleChipSelection(chipValue));

            // 保存数据
            ChipData chipData = new ChipData
            {
                chipObject = chipObj,
                button = chipButton,
                chipImage = chipImage,
                outline = null,
                value = chipValue,
                isSelected = false
            };

            chipDataMap[chipValue] = chipData;
        }

        /// <summary>
        /// 格式化筹码数值
        /// </summary>
        private string FormatChipValue(int value)
        {
            if (value >= 1000000) return $"{value / 1000000}M";
            if (value >= 1000) return $"{value / 1000}K";
            return value.ToString();
        }

        /// <summary>
        /// 创建筹码数值文字 (仅在没有图片时使用)
        /// </summary>
        private void CreateChipValueText(GameObject parent, int chipValue)
        {
            GameObject textObj = new GameObject("ValueText");
            textObj.transform.SetParent(parent.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text valueText = textObj.AddComponent<Text>();
            valueText.text = FormatChipValue(chipValue);
            valueText.color = Color.white;
            valueText.alignment = TextAnchor.MiddleCenter;
            valueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            valueText.fontSize = fontSize - 2;
            valueText.fontStyle = FontStyle.Bold;

            // 添加阴影
            Shadow textShadow = textObj.AddComponent<Shadow>();
            textShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            textShadow.effectDistance = new Vector2(1, -1);
        }

        /// <summary>
        /// 加载筹码图片
        /// </summary>
        private Sprite LoadChipSprite(int chipValue)
        {
            try
            {
                // 根据筹码数值获取对应的图片名称
                string imageName = GetChipImageName(chipValue);
                
                // 尝试从Resources/Images/chips/路径加载
                string resourcePath = $"Images/chips/{imageName}";
                Sprite sprite = Resources.Load<Sprite>(resourcePath);
                
                if (sprite != null)
                {
                    Debug.Log($"[MoreChipPanel] ✅ 成功加载筹码图片: {resourcePath}");
                    return sprite;
                }
                else
                {
                    Debug.Log($"[MoreChipPanel] ⚠️ 筹码图片未找到: {resourcePath}，使用纯色代替");
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MoreChipPanel] ❌ 加载筹码图片失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取筹码图片名称 - 匹配你的文件命名
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
                case 500000: return "B_500K";
                case 1000000: return "B_1M";
                case 5000000: return "B_5M";
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

        /// <summary>
        /// 获取筹码颜色 (当没有图片时使用)
        /// </summary>
        private Color GetChipColor(int chipValue)
        {
            Color[] colors = {
                new Color(0.9f, 0.1f, 0.1f, 1f), // 红
                new Color(0.2f, 0.8f, 0.2f, 1f), // 绿
                new Color(0.1f, 0.4f, 0.9f, 1f), // 蓝
                new Color(0.9f, 0.5f, 0.1f, 1f), // 橙
                new Color(0.7f, 0.2f, 0.8f, 1f), // 紫
                new Color(0.9f, 0.9f, 0.1f, 1f), // 黄
                new Color(0.1f, 0.8f, 0.8f, 1f), // 青
                new Color(0.8f, 0.3f, 0.5f, 1f), // 粉
                new Color(0.5f, 0.9f, 0.3f, 1f), // 浅绿
                new Color(0.9f, 0.3f, 0.9f, 1f)  // 粉紫
            };
            
            int index = System.Array.IndexOf(allChips, chipValue) % colors.Length;
            return index >= 0 ? colors[index] : colors[0];
        }

        #endregion

        #region 选择逻辑

        /// <summary>
        /// 初始化选择
        /// </summary>
        private void InitializeSelection()
        {
            selectedChips.Clear();
            foreach (int chip in defaultSelected)
            {
                if (selectedChips.Count < maxSelection)
                {
                    selectedChips.Add(chip);
                }
            }
        }

        /// <summary>
        /// 切换筹码选择
        /// </summary>
        private void ToggleChipSelection(int chipValue)
        {
            if (selectedChips.Contains(chipValue))
            {
                selectedChips.Remove(chipValue);
                Debug.Log($"[MoreChipPanel] 取消选择筹码: {chipValue}");
            }
            else
            {
                if (selectedChips.Count < maxSelection)
                {
                    selectedChips.Add(chipValue);
                    Debug.Log($"[MoreChipPanel] 选择筹码: {chipValue}");
                }
                else
                {
                    Debug.Log($"[MoreChipPanel] 已达到最大选择数量: {maxSelection}");
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
            if (!chipDataMap.ContainsKey(chipValue)) return;

            ChipData data = chipDataMap[chipValue];
            bool isSelected = selectedChips.Contains(chipValue);
            data.isSelected = isSelected;

            // 使用Outline显示选中状态
            if (isSelected)
            {
                if (data.outline == null)
                {
                    data.outline = data.chipObject.AddComponent<Outline>();
                    data.outline.effectColor = selectedBorderColor;
                    data.outline.effectDistance = new Vector2(borderWidth, -borderWidth);
                }
            }
            else
            {
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
            float targetScale = isSelected ? 1.1f : 1f;
            data.chipObject.transform.localScale = Vector3.one * targetScale;
        }

        /// <summary>
        /// 更新所有选择状态
        /// </summary>
        private void UpdateAllSelectionStates()
        {
            foreach (var pair in chipDataMap)
            {
                UpdateChipSelectionState(pair.Key);
            }
        }

        /// <summary>
        /// 重置选择
        /// </summary>
        private void ResetSelection()
        {
            selectedChips.Clear();
            foreach (int chip in defaultSelected)
            {
                if (selectedChips.Count < maxSelection)
                {
                    selectedChips.Add(chip);
                }
            }
            
            UpdateAllSelectionStates();
            UpdateStatusText();
            Debug.Log("[MoreChipPanel] 重置为默认选择");
        }

        /// <summary>
        /// 确认选择
        /// </summary>
        private void ConfirmSelection()
        {
            if (selectedChips.Count == 0)
            {
                Debug.Log("[MoreChipPanel] 没有选择任何筹码");
                return;
            }

            var sortedChips = selectedChips.ToArray();
            System.Array.Sort(sortedChips);
            
            Debug.Log($"[MoreChipPanel] 确认选择筹码: [{string.Join(", ", sortedChips)}]");
            HidePanel();
        }

        /// <summary>
        /// 更新状态文字
        /// </summary>
        private void UpdateStatusText()
        {
            if (statusText != null)
            {
                statusText.text = $"已选择: {selectedChips.Count}/{maxSelection}";
                
                if (selectedChips.Count > 0)
                {
                    var sortedChips = selectedChips.ToArray();
                    System.Array.Sort(sortedChips);
                    var formattedChips = new string[sortedChips.Length];
                    for (int i = 0; i < sortedChips.Length; i++)
                    {
                        formattedChips[i] = FormatChipValue(sortedChips[i]);
                    }
                    statusText.text += $"\n[{string.Join(", ", formattedChips)}]";
                }
            }
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void HidePanel()
        {
            if (maskLayer != null) maskLayer.SetActive(false);
            if (chipPanel != null) chipPanel.SetActive(false);
            Debug.Log("[MoreChipPanel] 面板已隐藏");
        }

        /// <summary>
        /// 显示面板
        /// </summary>
        public void ShowPanel()
        {
            if (maskLayer != null) maskLayer.SetActive(true);
            if (chipPanel != null) chipPanel.SetActive(true);
            UpdateAllSelectionStates();
            UpdateStatusText();
            Debug.Log("[MoreChipPanel] 面板已显示");
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 切换面板显示状态
        /// </summary>
        public void TogglePanel()
        {
            if (maskLayer != null && maskLayer.activeInHierarchy)
                HidePanel();
            else
                ShowPanel();
        }

        /// <summary>
        /// 设置选择的筹码
        /// </summary>
        public void SetSelectedChips(int[] chips)
        {
            selectedChips.Clear();
            if (chips != null)
            {
                foreach (int chip in chips)
                {
                    if (selectedChips.Count < maxSelection)
                        selectedChips.Add(chip);
                }
            }
            
            UpdateAllSelectionStates();
            UpdateStatusText();
        }

        /// <summary>
        /// 获取选择的筹码
        /// </summary>
        public int[] GetSelectedChips()
        {
            var sortedChips = selectedChips.ToArray();
            System.Array.Sort(sortedChips);
            return sortedChips;
        }

        #endregion

        #region 编辑器辅助

        /// <summary>
        /// 重新创建UI
        /// </summary>
        [ContextMenu("重新创建UI")]
        public void RecreateUI()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                    Destroy(transform.GetChild(i).gameObject);
                else
                    DestroyImmediate(transform.GetChild(i).gameObject);
            }

            chipDataMap.Clear();
            uiCreated = false;
            CreateUI();
            InitializeSelection();
        }

        /// <summary>
        /// 显示组件状态
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowStatus()
        {
            Debug.Log($"[MoreChipPanel] UI已创建: {uiCreated}");
            Debug.Log($"[MoreChipPanel] 遮罩层: {(maskLayer != null ? "✓" : "✗")}");
            Debug.Log($"[MoreChipPanel] 筹码面板: {(chipPanel != null ? "✓" : "✗")}");
            Debug.Log($"[MoreChipPanel] 筹码数量: {chipDataMap.Count}");
            Debug.Log($"[MoreChipPanel] 已选择: [{string.Join(", ", selectedChips)}]");
        }

        #endregion
    }
}
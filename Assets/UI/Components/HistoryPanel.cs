// Assets/UI/Components/VideoOverlay/Set/HistoryPanel.cs
// 简化版历史记录面板组件 - 仅用于UI生成
// 挂载到节点上自动创建历史记录面板UI
// 创建时间: 2025/6/28

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace BaccaratGame.UI.Components.VideoOverlay
{
    /// <summary>
    /// 简化版历史记录面板组件
    /// 挂载到节点上自动创建UI，包含标题、列表和滚动功能
    /// </summary>
    public class HistoryPanel : MonoBehaviour
    {
        #region 配置参数

        [Header("面板配置")]
        public Vector2 panelSize = new Vector2(400, 500);
        public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        public Color titleColor = Color.white;
        public Color buttonColor = new Color(0.2f, 0.6f, 1f, 1f);
        public int fontSize = 14;
        
        [Header("遮罩层设置")]
        public Color maskColor = new Color(0, 0, 0, 0.3f);
        
        [Header("历史记录样式")]
        public Color recordBgColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        public Color winColor = Color.green;
        public Color loseColor = Color.red;
        public Color tieColor = Color.yellow;

        #endregion

        #region 私有字段

        private bool uiCreated = false;
        private GameObject maskLayer;
        private GameObject historyPanel;
        private Canvas uiCanvas;
        
        // UI组件引用
        private ScrollRect scrollView;
        private Transform contentParent;
        private Text titleText;
        private Button closeButton;
        
        // 模拟数据
        private List<HistoryRecord> mockData = new List<HistoryRecord>();

        #endregion

        #region 生命周期

        private void Awake()
        {
            CreateUI();
            InitializeMockData();
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
            CreateHistoryPanel();
            CreatePanelHeader();
            CreateScrollArea();
            
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
                GameObject canvasObj = new GameObject("HistoryPanelCanvas");
                canvasObj.transform.SetParent(transform.parent);
                
                uiCanvas = canvasObj.AddComponent<Canvas>();
                uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                uiCanvas.sortingOrder = 2000; // 确保在最上层
                
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
        /// 创建历史面板
        /// </summary>
        private void CreateHistoryPanel()
        {
            historyPanel = new GameObject("HistoryPanel");
            historyPanel.transform.SetParent(transform);

            RectTransform panelRect = historyPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f); // 居中
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = panelSize;
            panelRect.anchoredPosition = Vector2.zero;

            Image panelBg = historyPanel.AddComponent<Image>();
            panelBg.color = backgroundColor;
            panelBg.sprite = CreateSimpleSprite();
        }

        /// <summary>
        /// 创建面板头部
        /// </summary>
        private void CreatePanelHeader()
        {
            // 创建标题
            CreateTitle();
            
            // 创建关闭按钮
            CreateCloseButton();
        }

        /// <summary>
        /// 创建标题
        /// </summary>
        private void CreateTitle()
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(historyPanel.transform);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.9f);
            titleRect.anchorMax = new Vector2(0.8f, 1f);
            titleRect.offsetMin = new Vector2(15, 0);
            titleRect.offsetMax = new Vector2(0, -5);

            titleText = titleObj.AddComponent<Text>();
            titleText.text = "📋 投注历史";
            titleText.color = titleColor;
            titleText.alignment = TextAnchor.MiddleLeft;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = fontSize + 6;
            titleText.fontStyle = FontStyle.Bold;
        }

        /// <summary>
        /// 创建关闭按钮
        /// </summary>
        private void CreateCloseButton()
        {
            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(historyPanel.transform);

            RectTransform closeRect = closeObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.85f, 0.9f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.offsetMin = new Vector2(0, 0);
            closeRect.offsetMax = new Vector2(-10, -5);

            closeButton = closeObj.AddComponent<Button>();
            
            Image closeImage = closeObj.AddComponent<Image>();
            closeImage.color = Color.red;
            closeImage.sprite = CreateSimpleSprite();

            closeButton.onClick.AddListener(HidePanel);

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
        /// 创建滚动区域
        /// </summary>
        private void CreateScrollArea()
        {
            GameObject scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(historyPanel.transform);

            RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 0.9f);
            scrollRect.offsetMin = new Vector2(10, 10);
            scrollRect.offsetMax = new Vector2(-10, -5);

            scrollView = scrollObj.AddComponent<ScrollRect>();
            scrollView.horizontal = false;
            scrollView.vertical = true;

            // 创建Viewport
            CreateViewport(scrollObj);
            
            // 创建Content
            CreateContent();
        }

        /// <summary>
        /// 创建视口
        /// </summary>
        private void CreateViewport(GameObject parent)
        {
            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(parent.transform);

            RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            Image viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.color = new Color(0.05f, 0.05f, 0.05f, 1f);
            viewportImage.sprite = CreateSimpleSprite();

            Mask viewportMask = viewportObj.AddComponent<Mask>();
            viewportMask.showMaskGraphic = true;

            scrollView.viewport = viewportRect;
        }

        /// <summary>
        /// 创建内容区域
        /// </summary>
        private void CreateContent()
        {
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(scrollView.viewport);

            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);
            contentRect.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 5f;
            layoutGroup.padding = new RectOffset(5, 5, 5, 5);
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;

            ContentSizeFitter sizeFitter = contentObj.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollView.content = contentRect;
            contentParent = contentObj.transform;
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

        #region 数据处理

        /// <summary>
        /// 初始化模拟数据
        /// </summary>
        private void InitializeMockData()
        {
            mockData.Clear();
            
            string[] betTypes = { "庄", "闲", "和", "庄对", "闲对" };
            string[] results = { "胜", "负", "胜", "负", "胜", "负", "胜" };
            int[] amounts = { 100, 500, 1000, 200, 300, 800, 1500 };
            
            for (int i = 0; i < 15; i++)
            {
                var record = new HistoryRecord
                {
                    gameNumber = $"T{System.DateTime.Now:yyMMdd}{(i + 1):D3}",
                    betType = betTypes[i % betTypes.Length],
                    betAmount = amounts[i % amounts.Length],
                    result = results[i % results.Length],
                    winAmount = results[i % results.Length] == "胜" ? amounts[i % amounts.Length] * 2 : 0,
                    gameTime = System.DateTime.Now.AddMinutes(-i * 5).ToString("HH:mm:ss")
                };
                
                mockData.Add(record);
            }
            
            // 自动加载数据到UI
            LoadDataToUI();
        }

        /// <summary>
        /// 加载数据到UI
        /// </summary>
        private void LoadDataToUI()
        {
            // 清除现有项目
            for (int i = contentParent.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                    Destroy(contentParent.GetChild(i).gameObject);
                else
                    DestroyImmediate(contentParent.GetChild(i).gameObject);
            }

            // 创建历史记录项
            foreach (var record in mockData)
            {
                CreateHistoryItem(record);
            }
        }

        /// <summary>
        /// 创建历史记录项
        /// </summary>
        private void CreateHistoryItem(HistoryRecord record)
        {
            GameObject itemObj = new GameObject("HistoryItem");
            itemObj.transform.SetParent(contentParent);

            RectTransform itemRect = itemObj.AddComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(0, 60);

            // 背景
            Image itemBg = itemObj.AddComponent<Image>();
            itemBg.color = recordBgColor;
            itemBg.sprite = CreateSimpleSprite();

            // 创建文本信息
            CreateItemTexts(itemObj, record);
        }

        /// <summary>
        /// 创建记录项文本
        /// </summary>
        private void CreateItemTexts(GameObject parent, HistoryRecord record)
        {
            // 第一行：局号 | 投注类型 | 结果
            CreateItemText(parent, "Info1", 
                $"局号: {record.gameNumber}  类型: {record.betType}  结果: {record.result}",
                new Vector2(0, 0.5f), new Vector2(1, 1f), GetResultColor(record.result));
            
            // 第二行：投注金额 | 赢得金额 | 时间
            string amountInfo = $"投注: ¥{record.betAmount}  赢得: ¥{record.winAmount}  时间: {record.gameTime}";
            CreateItemText(parent, "Info2", amountInfo,
                new Vector2(0, 0f), new Vector2(1, 0.5f), Color.gray);
        }

        /// <summary>
        /// 创建记录项文本
        /// </summary>
        private void CreateItemText(GameObject parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = anchorMin;
            textRect.anchorMax = anchorMax;
            textRect.offsetMin = new Vector2(10, 2);
            textRect.offsetMax = new Vector2(-10, -2);

            Text recordText = textObj.AddComponent<Text>();
            recordText.text = text;
            recordText.color = color;
            recordText.alignment = TextAnchor.MiddleLeft;
            recordText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            recordText.fontSize = fontSize - 2;
        }

        /// <summary>
        /// 获取结果颜色
        /// </summary>
        private Color GetResultColor(string result)
        {
            return result switch
            {
                "胜" => winColor,
                "负" => loseColor,
                "和" => tieColor,
                _ => Color.white
            };
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void HidePanel()
        {
            if (maskLayer != null) maskLayer.SetActive(false);
            if (historyPanel != null) historyPanel.SetActive(false);
            Debug.Log("[HistoryPanel] 面板已隐藏");
        }

        /// <summary>
        /// 显示面板
        /// </summary>
        public void ShowPanel()
        {
            if (maskLayer != null) maskLayer.SetActive(true);
            if (historyPanel != null) historyPanel.SetActive(true);
            Debug.Log("[HistoryPanel] 面板已显示");
        }

        /// <summary>
        /// 打开面板 (外部调用)
        /// </summary>
        public void OpenPanel()
        {
            ShowPanel();
        }

        /// <summary>
        /// 关闭面板 (外部调用)
        /// </summary>
        public void ClosePanel()
        {
            HidePanel();
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
        /// 刷新数据
        /// </summary>
        public void RefreshData()
        {
            LoadDataToUI();
            Debug.Log("[HistoryPanel] 数据已刷新");
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

            uiCreated = false;
            CreateUI();
            InitializeMockData();
        }

        /// <summary>
        /// 显示组件状态
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowStatus()
        {
            Debug.Log($"[HistoryPanel] UI已创建: {uiCreated}");
            Debug.Log($"[HistoryPanel] 遮罩层: {(maskLayer != null ? "✓" : "✗")}");
            Debug.Log($"[HistoryPanel] 历史面板: {(historyPanel != null ? "✓" : "✗")}");
            Debug.Log($"[HistoryPanel] 滚动视图: {(scrollView != null ? "✓" : "✗")}");
            Debug.Log($"[HistoryPanel] 数据条数: {mockData.Count}");
        }

        #endregion
    }

    #region 数据类型
    
    /// <summary>
    /// 历史记录数据
    /// </summary>
    [System.Serializable]
    public class HistoryRecord
    {
        public string gameNumber;     // 局号
        public string betType;        // 投注类型
        public int betAmount;         // 投注金额
        public string result;         // 结果 (胜/负/和)
        public int winAmount;         // 赢得金额
        public string gameTime;       // 游戏时间
    }
    
    #endregion
}
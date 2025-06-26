// Assets/UI/Components/Panels/HistoryPanel.cs
// 历史记录面板组件 - 运行时自动生成UI版本
// 完整的历史记录面板，包含标题栏、内容区域、滚动列表等
// 创建时间: 2025/6/26

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BaccaratGame.UI.Components
{
    /// <summary>
    /// 历史记录面板组件 - 自动生成UI版本
    /// 运行时自动创建完整的历史记录面板界面
    /// </summary>
    public class HistoryPanel : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text titleText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button refreshButton;
        [SerializeField] private ScrollRect scrollView;
        [SerializeField] private Transform contentParent;
        [SerializeField] private Text emptyMessageText;
        
        [Header("面板配置")]
        [SerializeField] private Vector2 panelSize = new Vector2(400, 500);
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.9f);
        [SerializeField] private Color titleColor = Color.white;
        [SerializeField] private Color buttonColor = new Color(0.2f, 0.4f, 0.8f);
        [SerializeField] private string panelTitle = "投注历史";
        
        [Header("历史记录配置")]
        [SerializeField] private Color recordBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        [SerializeField] private Color winRecordColor = Color.green;
        [SerializeField] private Color loseRecordColor = Color.red;
        [SerializeField] private Color tieRecordColor = Color.yellow;
        [SerializeField] private int maxDisplayRecords = 50;
        
        [Header("动画配置")]
        [SerializeField] private bool enableShowAnimation = true;
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private bool enableRecordAnimation = true;
        [SerializeField] private float recordAnimationDelay = 0.1f;
        
        // 状态变量
        private bool isPanelOpen = false;
        private List<GameObject> historyItems = new List<GameObject>();
        private List<HistoryRecord> mockHistoryData = new List<HistoryRecord>();
        private bool isAnimating = false;
        
        #region Unity生命周期
        
        private void Awake()
        {
            CreateHistoryPanelUI();
            InitializeMockData();
        }
        
        private void Start()
        {
            // 默认隐藏面板
            HidePanel(false);
            
            // 开始演示
            StartCoroutine(DemoCoroutine());
        }
        
        #endregion
        
        #region UI创建
        
        /// <summary>
        /// 创建历史记录面板UI
        /// </summary>
        private void CreateHistoryPanelUI()
        {
            // 确保有RectTransform组件
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }
            
            // 设置为全屏
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // 创建面板根对象
            CreatePanelRoot();
            
            // 创建背景遮罩
            CreateBackgroundMask();
            
            // 创建主面板
            CreateMainPanel();
            
            // 创建标题栏
            CreateTitleBar();
            
            // 创建内容区域
            CreateContentArea();
            
            Debug.Log("[HistoryPanel] UI创建完成");
        }
        
        /// <summary>
        /// 创建面板根对象
        /// </summary>
        private void CreatePanelRoot()
        {
            if (panelRoot == null)
            {
                panelRoot = new GameObject("PanelRoot");
                panelRoot.transform.SetParent(transform);
                
                RectTransform panelRect = panelRoot.AddComponent<RectTransform>();
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
            }
        }
        
        /// <summary>
        /// 创建背景遮罩
        /// </summary>
        private void CreateBackgroundMask()
        {
            GameObject maskObj = new GameObject("BackgroundMask");
            maskObj.transform.SetParent(panelRoot.transform);
            
            RectTransform maskRect = maskObj.AddComponent<RectTransform>();
            maskRect.anchorMin = Vector2.zero;
            maskRect.anchorMax = Vector2.one;
            maskRect.offsetMin = Vector2.zero;
            maskRect.offsetMax = Vector2.zero;
            
            Image maskImage = maskObj.AddComponent<Image>();
            maskImage.color = new Color(0, 0, 0, 0.5f);
            
            // 点击背景关闭面板
            Button maskButton = maskObj.AddComponent<Button>();
            maskButton.onClick.AddListener(() => HidePanel());
        }
        
        /// <summary>
        /// 创建主面板
        /// </summary>
        private void CreateMainPanel()
        {
            GameObject mainPanelObj = new GameObject("MainPanel");
            mainPanelObj.transform.SetParent(panelRoot.transform);
            
            RectTransform mainRect = mainPanelObj.AddComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0.5f, 0.5f);
            mainRect.anchorMax = new Vector2(0.5f, 0.5f);
            mainRect.sizeDelta = panelSize;
            mainRect.anchoredPosition = Vector2.zero;
            
            backgroundImage = mainPanelObj.AddComponent<Image>();
            backgroundImage.color = backgroundColor;
            backgroundImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
            backgroundImage.type = Image.Type.Sliced;
        }
        
        /// <summary>
        /// 创建标题栏
        /// </summary>
        private void CreateTitleBar()
        {
            GameObject titleBarObj = new GameObject("TitleBar");
            titleBarObj.transform.SetParent(backgroundImage.transform);
            
            RectTransform titleRect = titleBarObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.9f);
            titleRect.anchorMax = new Vector2(1, 1f);
            titleRect.offsetMin = new Vector2(10, 0);
            titleRect.offsetMax = new Vector2(-10, -5);
            
            // 创建标题文字
            CreateTitleText(titleBarObj);
            
            // 创建关闭按钮
            CreateCloseButton(titleBarObj);
            
            // 创建刷新按钮
            CreateRefreshButton(titleBarObj);
        }
        
        /// <summary>
        /// 创建标题文字
        /// </summary>
        private void CreateTitleText(GameObject parent)
        {
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(parent.transform);
            
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0);
            titleRect.anchorMax = new Vector2(0.7f, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            titleText = titleObj.AddComponent<Text>();
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 20;
            titleText.color = titleColor;
            titleText.text = panelTitle;
            titleText.alignment = TextAnchor.MiddleLeft;
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
            closeRect.anchorMin = new Vector2(0.85f, 0);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
            
            closeButton = closeObj.AddComponent<Button>();
            Image closeImage = closeObj.AddComponent<Image>();
            closeImage.color = Color.red;
            closeImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            
            // 添加关闭文字
            GameObject closeTextObj = new GameObject("CloseText");
            closeTextObj.transform.SetParent(closeObj.transform);
            
            RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.offsetMin = Vector2.zero;
            closeTextRect.offsetMax = Vector2.zero;
            
            Text closeText = closeTextObj.AddComponent<Text>();
            closeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            closeText.fontSize = 16;
            closeText.color = Color.white;
            closeText.text = "×";
            closeText.alignment = TextAnchor.MiddleCenter;
            closeText.fontStyle = FontStyle.Bold;
            
            closeButton.onClick.AddListener(() => HidePanel());
        }
        
        /// <summary>
        /// 创建刷新按钮
        /// </summary>
        private void CreateRefreshButton(GameObject parent)
        {
            GameObject refreshObj = new GameObject("RefreshButton");
            refreshObj.transform.SetParent(parent.transform);
            
            RectTransform refreshRect = refreshObj.AddComponent<RectTransform>();
            refreshRect.anchorMin = new Vector2(0.7f, 0);
            refreshRect.anchorMax = new Vector2(0.85f, 1f);
            refreshRect.offsetMin = Vector2.zero;
            refreshRect.offsetMax = Vector2.zero;
            
            refreshButton = refreshObj.AddComponent<Button>();
            Image refreshImage = refreshObj.AddComponent<Image>();
            refreshImage.color = buttonColor;
            refreshImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            
            // 添加刷新文字
            GameObject refreshTextObj = new GameObject("RefreshText");
            refreshTextObj.transform.SetParent(refreshObj.transform);
            
            RectTransform refreshTextRect = refreshTextObj.AddComponent<RectTransform>();
            refreshTextRect.anchorMin = Vector2.zero;
            refreshTextRect.anchorMax = Vector2.one;
            refreshTextRect.offsetMin = Vector2.zero;
            refreshTextRect.offsetMax = Vector2.zero;
            
            Text refreshText = refreshTextObj.AddComponent<Text>();
            refreshText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            refreshText.fontSize = 12;
            refreshText.color = Color.white;
            refreshText.text = "刷新";
            refreshText.alignment = TextAnchor.MiddleCenter;
            refreshText.fontStyle = FontStyle.Bold;
            
            refreshButton.onClick.AddListener(() => RefreshData());
        }
        
        /// <summary>
        /// 创建内容区域
        /// </summary>
        private void CreateContentArea()
        {
            GameObject contentAreaObj = new GameObject("ContentArea");
            contentAreaObj.transform.SetParent(backgroundImage.transform);
            
            RectTransform contentRect = contentAreaObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 0.9f);
            contentRect.offsetMin = new Vector2(10, 10);
            contentRect.offsetMax = new Vector2(-10, -5);
            
            // 创建滚动视图
            CreateScrollView(contentAreaObj);
        }
        
        /// <summary>
        /// 创建滚动视图
        /// </summary>
        private void CreateScrollView(GameObject parent)
        {
            GameObject scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(parent.transform);
            
            RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            
            // 添加ScrollRect组件
            scrollView = scrollObj.AddComponent<ScrollRect>();
            scrollView.horizontal = false;
            scrollView.vertical = true;
            
            // 创建Viewport
            CreateViewport(scrollObj);
            
            // 创建Content
            CreateContent();
        }
        
        /// <summary>
        /// 创建Viewport
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
            viewportImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
            viewportImage.type = Image.Type.Sliced;
            
            Mask viewportMask = viewportObj.AddComponent<Mask>();
            viewportMask.showMaskGraphic = true;
            
            scrollView.viewport = viewportRect;
        }
        
        /// <summary>
        /// 创建Content
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
            
            // 添加布局组件
            VerticalLayoutGroup layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 5f;
            layoutGroup.padding = new RectOffset(5, 5, 5, 5);
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
            
            // 添加内容适配器
            ContentSizeFitter sizeFitter = contentObj.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scrollView.content = contentRect;
            contentParent = contentObj.transform;
        }
        
        #endregion
        
        #region 数据管理
        
        /// <summary>
        /// 初始化模拟数据
        /// </summary>
        private void InitializeMockData()
        {
            mockHistoryData.Clear();
            
            // 创建模拟历史记录
            string[] betTypes = { "庄", "闲", "和", "庄对", "闲对" };
            string[] results = { "胜", "负", "胜", "负", "胜", "负", "胜" };
            decimal[] amounts = { 100m, 500m, 1000m, 200m, 300m, 800m, 1500m };
            
            for (int i = 0; i < 20; i++)
            {
                var record = new HistoryRecord
                {
                    gameNumber = $"T250626{(i + 1):D3}",
                    betType = betTypes[i % betTypes.Length],
                    betAmount = amounts[i % amounts.Length],
                    result = results[i % results.Length],
                    winAmount = results[i % results.Length] == "胜" ? amounts[i % amounts.Length] * 2 : 0,
                    gameTime = DateTime.Now.AddMinutes(-i * 3)
                };
                
                mockHistoryData.Add(record);
            }
        }
        
        /// <summary>
        /// 刷新数据
        /// </summary>
        public void RefreshData()
        {
            ClearHistoryItems();
            
            if (mockHistoryData.Count == 0)
            {
                ShowEmptyMessage();
            }
            else
            {
                StartCoroutine(LoadHistoryItemsWithAnimation());
            }
        }
        
        /// <summary>
        /// 带动画加载历史项目
        /// </summary>
        private IEnumerator LoadHistoryItemsWithAnimation()
        {
            for (int i = 0; i < mockHistoryData.Count && i < maxDisplayRecords; i++)
            {
                CreateHistoryItem(mockHistoryData[i]);
                
                if (enableRecordAnimation)
                {
                    yield return new WaitForSeconds(recordAnimationDelay);
                }
            }
        }
        
        #endregion
        
        #region 历史记录项创建
        
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
            itemBg.color = recordBackgroundColor;
            itemBg.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
            itemBg.type = Image.Type.Sliced;
            
            // 创建文本信息
            CreateRecordTexts(itemObj, record);
            
            historyItems.Add(itemObj);
        }
        
        /// <summary>
        /// 创建记录文本
        /// </summary>
        private void CreateRecordTexts(GameObject parent, HistoryRecord record)
        {
            // 局号
            CreateRecordText(parent, "GameNumber", record.gameNumber, new Vector2(0, 0.7f), new Vector2(0.3f, 1f), 12);
            
            // 投注类型
            CreateRecordText(parent, "BetType", record.betType, new Vector2(0.3f, 0.7f), new Vector2(0.5f, 1f), 12);
            
            // 投注金额
            CreateRecordText(parent, "BetAmount", record.betAmount.ToString(), new Vector2(0.5f, 0.7f), new Vector2(0.7f, 1f), 12);
            
            // 结果
            Color resultColor = GetResultColor(record.result);
            CreateRecordText(parent, "Result", record.result, new Vector2(0.7f, 0.7f), new Vector2(1f, 1f), 12, resultColor);
            
            // 赢得金额
            CreateRecordText(parent, "WinAmount", record.winAmount > 0 ? $"+{record.winAmount}" : "0", 
                new Vector2(0, 0f), new Vector2(0.5f, 0.3f), 12, record.winAmount > 0 ? winRecordColor : Color.gray);
            
            // 时间
            CreateRecordText(parent, "Time", record.gameTime.ToString("HH:mm:ss"), 
                new Vector2(0.5f, 0f), new Vector2(1f, 0.3f), 10, Color.gray);
        }
        
        /// <summary>
        /// 创建记录文本
        /// </summary>
        private void CreateRecordText(GameObject parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, int fontSize, Color? color = null)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = anchorMin;
            textRect.anchorMax = anchorMax;
            textRect.offsetMin = new Vector2(5, 2);
            textRect.offsetMax = new Vector2(-5, -2);
            
            Text recordText = textObj.AddComponent<Text>();
            recordText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            recordText.fontSize = fontSize;
            recordText.color = color ?? Color.white;
            recordText.text = text;
            recordText.alignment = TextAnchor.MiddleCenter;
        }
        
        /// <summary>
        /// 获取结果颜色
        /// </summary>
        private Color GetResultColor(string result)
        {
            return result switch
            {
                "胜" => winRecordColor,
                "负" => loseRecordColor,
                "和" => tieRecordColor,
                _ => Color.white
            };
        }
        
        #endregion
        
        #region 面板控制
        
        /// <summary>
        /// 显示面板
        /// </summary>
        public void ShowPanel()
        {
            if (isPanelOpen || isAnimating) return;
            
            isPanelOpen = true;
            panelRoot.SetActive(true);
            
            RefreshData();
            
            if (enableShowAnimation)
            {
                StartCoroutine(ShowAnimation());
            }
        }
        
        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void HidePanel(bool useAnimation = true)
        {
            if (!isPanelOpen || isAnimating) return;
            
            isPanelOpen = false;
            
            if (useAnimation && enableShowAnimation)
            {
                StartCoroutine(HideAnimation());
            }
            else
            {
                panelRoot.SetActive(false);
            }
        }
        
        /// <summary>
        /// 切换面板显示状态
        /// </summary>
        public void TogglePanel()
        {
            if (isPanelOpen)
                HidePanel();
            else
                ShowPanel();
        }
        
        /// <summary>
        /// 获取面板状态
        /// </summary>
        public bool IsOpen()
        {
            return isPanelOpen;
        }
        
        #endregion
        
        #region 动画
        
        /// <summary>
        /// 显示动画
        /// </summary>
        private IEnumerator ShowAnimation()
        {
            isAnimating = true;
            
            Transform mainPanel = backgroundImage.transform;
            Vector3 originalScale = mainPanel.localScale;
            mainPanel.localScale = Vector3.zero;
            
            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                mainPanel.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
                yield return null;
            }
            
            mainPanel.localScale = originalScale;
            isAnimating = false;
        }
        
        /// <summary>
        /// 隐藏动画
        /// </summary>
        private IEnumerator HideAnimation()
        {
            isAnimating = true;
            
            Transform mainPanel = backgroundImage.transform;
            Vector3 originalScale = mainPanel.localScale;
            
            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                mainPanel.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
                yield return null;
            }
            
            panelRoot.SetActive(false);
            mainPanel.localScale = originalScale;
            isAnimating = false;
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 清除历史项目
        /// </summary>
        private void ClearHistoryItems()
        {
            foreach (var item in historyItems)
            {
                if (item != null)
                    DestroyImmediate(item);
            }
            historyItems.Clear();
        }
        
        /// <summary>
        /// 显示空消息
        /// </summary>
        private void ShowEmptyMessage()
        {
            GameObject emptyObj = new GameObject("EmptyMessage");
            emptyObj.transform.SetParent(contentParent);
            
            RectTransform emptyRect = emptyObj.AddComponent<RectTransform>();
            emptyRect.sizeDelta = new Vector2(0, 100);
            
            emptyMessageText = emptyObj.AddComponent<Text>();
            emptyMessageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            emptyMessageText.fontSize = 16;
            emptyMessageText.color = Color.gray;
            emptyMessageText.text = "暂无历史记录";
            emptyMessageText.alignment = TextAnchor.MiddleCenter;
            
            historyItems.Add(emptyObj);
        }
        
        /// <summary>
        /// 演示协程
        /// </summary>
        private IEnumerator DemoCoroutine()
        {
            yield return new WaitForSeconds(3f);
            
            // 自动显示面板演示
            ShowPanel();
            yield return new WaitForSeconds(8f);
            
            // 隐藏面板
            HidePanel();
            yield return new WaitForSeconds(5f);
            
            // 重复演示
            StartCoroutine(DemoCoroutine());
        }
        
        #endregion
        
        #region 公共接口
        
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
        public decimal betAmount;     // 投注金额
        public string result;         // 结果 (胜/负/和)
        public decimal winAmount;     // 赢得金额
        public DateTime gameTime;     // 游戏时间
    }
    
    #endregion
}
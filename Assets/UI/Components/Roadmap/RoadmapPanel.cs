// Assets/UI/Components/Roadmap/RoadmapPanel.cs
// 露珠面板组件 - 启动即显示版本(简化iframe版)
// 自动创建并立即显示iframe露珠
// 创建时间: 2025/6/26

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BaccaratGame.Data;

namespace BaccaratGame.UI.Components
{
    /// <summary>
    /// 露珠面板 - 启动即显示版本(简化iframe版)
    /// 组件启动时立即创建并显示iframe露珠
    /// </summary>
    public class RoadmapPanel : MonoBehaviour
    {
        #region 序列化字段

        [Header("自动显示设置")]
        public bool autoCreateAndShow = false;
        public bool showOnAwake = false;
        public bool immediateDisplay = false;
        
        [Header("面板布局")]
        public Vector2 panelSize = new Vector2(600, 400);
        public Vector2 panelPosition = new Vector2(300, 0);
        
        [Header("露珠配置")]
        public string roadmapBaseUrl = "https://example.com/roadmap";
        public string iframeContainerId = "roadmap-container";
        public int roadmapHeight = 400;
        public int gameType = 3; // 百家乐固定为3
        public int tableId = 1;
        
        [Header("UI样式")]
        public Color panelBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        public Color headerColor = new Color(0.2f, 0.6f, 1f, 1f);
        public Color textColor = Color.white;
        
        [Header("现有组件引用 (可选)")]
        public GameObject roadmapPanel;

        [Header("调试设置")]
        public bool enableDebugMode = true;

        #endregion

        #region 私有字段

        private RectTransform rectTransform;
        private Canvas parentCanvas;
        private bool roadmapUICreated = false;
        private bool isPanelOpen = false;
        private string currentIframeUrl = "";
        
        // UI组件引用
        private GameObject mainPanel;
        private Button refreshButton;
        private Button closeButton;
        private TextMeshProUGUI titleText;
        private GameObject iframeContainer;

        #endregion

        #region 生命周期

        private void Awake()
        {
            InitializeComponent();
            
            if (showOnAwake)
            {
                CreateAndShowRoadmap();
            }
        }

        private void Start()
        {
            if (!roadmapUICreated && autoCreateAndShow)
            {
                CreateAndShowRoadmap();
            }
            
            SetupExistingComponents();
            LoadRoadmap();
        }

        private void OnValidate()
        {
            // 在编辑器中实时预览
            if (Application.isEditor && !Application.isPlaying)
            {
                if (immediateDisplay)
                {
                    InitializeComponent();
                    CreateAndShowRoadmap();
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

            // 查找父Canvas
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                CreateCanvasIfNeeded();
            }

            if (enableDebugMode)
                Debug.Log("[RoadmapPanel] 组件初始化完成");
        }

        /// <summary>
        /// 如需要则创建Canvas
        /// </summary>
        private void CreateCanvasIfNeeded()
        {
            GameObject canvasObj = new GameObject("RoadmapCanvas");
            canvasObj.transform.SetParent(transform.parent);
            
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
            
            // 将RoadmapPanel移到Canvas下
            transform.SetParent(canvasObj.transform);
            
            parentCanvas = canvas;
            
            if (enableDebugMode)
                Debug.Log("[RoadmapPanel] 创建了新的Canvas");
        }

        #endregion

        #region 露珠界面创建

        /// <summary>
        /// 创建并显示露珠界面
        /// </summary>
        [ContextMenu("创建并显示露珠界面")]
        public void CreateAndShowRoadmap()
        {
            if (roadmapUICreated) return;

            try
            {
                // 确保组件已初始化
                if (rectTransform == null)
                    InitializeComponent();

                // 创建主面板
                CreateMainPanel();
                
                // 创建标题栏
                CreateHeader();
                
                // 创建iframe容器
                CreateIframeContainer();

                roadmapUICreated = true;
                isPanelOpen = true;
                
                if (enableDebugMode)
                    Debug.Log("[RoadmapPanel] 露珠界面创建完成并已显示");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoadmapPanel] 创建露珠界面时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建主面板
        /// </summary>
        private void CreateMainPanel()
        {
            // 创建主面板
            GameObject panelObj = new GameObject("RoadmapMainPanel");
            panelObj.transform.SetParent(transform);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.sizeDelta = panelSize;
            panelRect.anchoredPosition = panelPosition;
            panelRect.localScale = Vector3.one;

            // 添加背景
            Image panelBackground = panelObj.AddComponent<Image>();
            panelBackground.color = panelBackgroundColor;
            panelBackground.sprite = CreateSolidSprite(Color.white);

            // 添加阴影效果
            Shadow shadow = panelObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(5, -5);

            mainPanel = panelObj;

            // 更新引用
            if (roadmapPanel == null) roadmapPanel = panelObj;

            if (enableDebugMode)
                Debug.Log("[RoadmapPanel] 主面板创建完成");
        }

        /// <summary>
        /// 创建标题栏
        /// </summary>
        private void CreateHeader()
        {
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(mainPanel.transform);

            RectTransform headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.sizeDelta = new Vector2(0, 40);
            headerRect.anchoredPosition = new Vector2(0, -20);
            headerRect.localScale = Vector3.one;

            // 添加背景
            Image headerBackground = headerObj.AddComponent<Image>();
            headerBackground.color = headerColor;
            headerBackground.sprite = CreateSolidSprite(Color.white);

            // 创建标题文本
            CreateTitleText(headerObj);
            
            // 创建按钮
            CreateHeaderButtons(headerObj);

            if (enableDebugMode)
                Debug.Log("[RoadmapPanel] 标题栏创建完成");
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
            titleRect.offsetMax = new Vector2(-80, 0);
            titleRect.localScale = Vector3.one;

            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = $"露珠路单 - 桌台 {tableId}";
            titleText.color = textColor;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.fontSize = 14;
            titleText.fontStyle = FontStyles.Bold;
        }

        /// <summary>
        /// 创建标题栏按钮
        /// </summary>
        private void CreateHeaderButtons(GameObject parent)
        {
            // 刷新按钮
            refreshButton = CreateHeaderButton(parent, "RefreshButton", "刷新", new Vector2(-70, 0), RefreshRoadmap);
            
            // 关闭按钮
            closeButton = CreateHeaderButton(parent, "CloseButton", "×", new Vector2(-20, 0), ClosePanel);
        }

        /// <summary>
        /// 创建标题栏按钮
        /// </summary>
        private Button CreateHeaderButton(GameObject parent, string buttonName, string buttonText, Vector2 position, UnityEngine.Events.UnityAction onClick)
        {
            GameObject buttonObj = new GameObject(buttonName);
            buttonObj.transform.SetParent(parent.transform);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1, 0.5f);
            buttonRect.anchorMax = new Vector2(1, 0.5f);
            buttonRect.sizeDelta = new Vector2(buttonName.Contains("Refresh") ? 40 : 25, 25);
            buttonRect.anchoredPosition = position;
            buttonRect.localScale = Vector3.one;

            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(1f, 1f, 1f, 0.3f);
            buttonImage.sprite = CreateSolidSprite(Color.white);

            Button button = buttonObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0.3f);
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.5f);
            colors.pressedColor = new Color(1f, 1f, 1f, 0.7f);
            button.colors = colors;

            button.onClick.AddListener(onClick);

            // 添加按钮文本
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textRect.localScale = Vector3.one;

            TextMeshProUGUI buttonTextComponent = textObj.AddComponent<TextMeshProUGUI>();
            buttonTextComponent.text = buttonText;
            buttonTextComponent.color = Color.white;
            buttonTextComponent.alignment = TextAlignmentOptions.Center;
            buttonTextComponent.fontSize = 12;
            buttonTextComponent.fontStyle = FontStyles.Bold;

            return button;
        }

        /// <summary>
        /// 创建iframe容器
        /// </summary>
        private void CreateIframeContainer()
        {
            GameObject containerObj = new GameObject("IframeContainer");
            containerObj.transform.SetParent(mainPanel.transform);

            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 0);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.offsetMin = new Vector2(5, 5);
            containerRect.offsetMax = new Vector2(-5, -45);
            containerRect.localScale = Vector3.one;

            // 添加背景(iframe占位)
            Image containerBackground = containerObj.AddComponent<Image>();
            containerBackground.color = Color.black;
            containerBackground.sprite = CreateSolidSprite(Color.white);

            iframeContainer = containerObj;

            // 创建占位文本
            CreatePlaceholderText(containerObj);

            if (enableDebugMode)
                Debug.Log("[RoadmapPanel] iframe容器创建完成");
        }

        /// <summary>
        /// 创建占位文本
        /// </summary>
        private void CreatePlaceholderText(GameObject parent)
        {
            GameObject textObj = new GameObject("PlaceholderText");
            textObj.transform.SetParent(parent.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textRect.localScale = Vector3.one;

            TextMeshProUGUI placeholderText = textObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = "露珠路单 iframe\n(WebGL环境中将显示实际路单)";
            placeholderText.color = Color.white;
            placeholderText.alignment = TextAlignmentOptions.Center;
            placeholderText.fontSize = 16;
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

        #endregion

        #region 现有组件设置

        /// <summary>
        /// 设置现有组件
        /// </summary>
        private void SetupExistingComponents()
        {
            // 如果有现有的roadmapPanel引用，确保它是激活的
            if (roadmapPanel != null && !roadmapUICreated)
            {
                isPanelOpen = roadmapPanel.activeSelf;
            }

            if (enableDebugMode)
                Debug.Log("[RoadmapPanel] 现有组件设置完成");
        }

        #endregion

        #region 露珠控制逻辑

        /// <summary>
        /// 加载露珠
        /// </summary>
        public void LoadRoadmap()
        {
            if (string.IsNullOrEmpty(roadmapBaseUrl))
            {
                Debug.LogWarning("[RoadmapPanel] 露珠基础URL未设置");
                return;
            }
            
            // 构建完整URL
            string fullUrl = BuildRoadmapUrl();
            currentIframeUrl = fullUrl;
            
            // 调用WebGL函数加载iframe，宽度100%，高度固定
            CallWebGLFunction("loadRoadmapIframe", $"{fullUrl},100%,{roadmapHeight},{iframeContainerId}");
            
            if (enableDebugMode)
                Debug.Log($"[RoadmapPanel] 加载露珠iframe: {fullUrl}");
        }

        /// <summary>
        /// 刷新露珠
        /// </summary>
        public void RefreshRoadmap()
        {
            LoadRoadmap();
            
            if (enableDebugMode)
                Debug.Log("[RoadmapPanel] 露珠已刷新");
        }

        /// <summary>
        /// 关闭面板
        /// </summary>
        public void ClosePanel()
        {
            if (mainPanel != null)
            {
                mainPanel.SetActive(false);
                isPanelOpen = false;
            }
            
            if (enableDebugMode)
                Debug.Log("[RoadmapPanel] 面板已关闭");
        }

        /// <summary>
        /// 显示面板
        /// </summary>
        public void ShowPanel()
        {
            if (mainPanel != null)
            {
                mainPanel.SetActive(true);
                isPanelOpen = true;
            }
            
            if (enableDebugMode)
                Debug.Log("[RoadmapPanel] 面板已显示");
        }

        /// <summary>
        /// 切换面板显示
        /// </summary>
        public void TogglePanel()
        {
            if (isPanelOpen)
                ClosePanel();
            else
                ShowPanel();
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 构建露珠URL
        /// </summary>
        private string BuildRoadmapUrl()
        {
            string separator = roadmapBaseUrl.Contains("?") ? "&" : "?";
            return $"{roadmapBaseUrl}{separator}gametype={gameType}&tableid={tableId}";
        }

        /// <summary>
        /// 调用WebGL函数
        /// </summary>
        private void CallWebGLFunction(string functionName, string parameter)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                Application.ExternalEval($"window.{functionName}('{parameter}')");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[RoadmapPanel] WebGL函数调用失败: {e.Message}");
            }
#else
            if (enableDebugMode)
                Debug.Log($"[RoadmapPanel] 模拟WebGL调用: {functionName}({parameter})");
#endif
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 强制显示露珠面板
        /// </summary>
        [ContextMenu("强制显示露珠面板")]
        public void ForceShowRoadmap()
        {
            roadmapUICreated = false;
            CreateAndShowRoadmap();
        }

        /// <summary>
        /// 设置桌台ID
        /// </summary>
        public void SetTableId(int newTableId)
        {
            if (tableId != newTableId)
            {
                tableId = newTableId;
                
                // 更新标题
                if (titleText != null)
                    titleText.text = $"露珠路单 - 桌台 {newTableId}";
                
                LoadRoadmap();
                
                if (enableDebugMode)
                    Debug.Log($"[RoadmapPanel] 桌台ID更新: {newTableId}");
            }
        }

        /// <summary>
        /// 设置游戏类型
        /// </summary>
        public void SetGameType(int newGameType)
        {
            if (gameType != newGameType)
            {
                gameType = newGameType;
                LoadRoadmap();
                
                if (enableDebugMode)
                    Debug.Log($"[RoadmapPanel] 游戏类型更新: {newGameType}");
            }
        }

        /// <summary>
        /// 设置露珠基础URL
        /// </summary>
        public void SetRoadmapUrl(string baseUrl)
        {
            if (roadmapBaseUrl != baseUrl)
            {
                roadmapBaseUrl = baseUrl;
                LoadRoadmap();
                
                if (enableDebugMode)
                    Debug.Log($"[RoadmapPanel] 基础URL更新: {baseUrl}");
            }
        }

        /// <summary>
        /// 批量设置参数
        /// </summary>
        public void SetTableInfo(TableInfo tableInfo)
        {
            if (tableInfo == null) return;
            
            bool needRefresh = false;
            
            if (tableId != tableInfo.id)
            {
                tableId = tableInfo.id;
                
                // 更新标题
                if (titleText != null)
                    titleText.text = $"露珠路单 - 桌台 {tableInfo.id}";
                
                needRefresh = true;
            }
            
            if (needRefresh)
            {
                LoadRoadmap();
            }
        }

        /// <summary>
        /// 获取当前iframe URL
        /// </summary>
        public string GetCurrentUrl()
        {
            return currentIframeUrl;
        }

        /// <summary>
        /// 获取面板状态
        /// </summary>
        public bool IsOpen()
        {
            return isPanelOpen;
        }

        #endregion

        #region 调试方法

        /// <summary>
        /// 显示组件状态
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowComponentStatus()
        {
            Debug.Log("=== RoadmapPanel 组件状态 ===");
            Debug.Log($"自动创建并显示: {autoCreateAndShow}");
            Debug.Log($"启动时显示: {showOnAwake}");
            Debug.Log($"立即显示: {immediateDisplay}");
            Debug.Log($"露珠UI已创建: {roadmapUICreated}");
            Debug.Log($"面板是否打开: {isPanelOpen}");
            Debug.Log($"父Canvas: {(parentCanvas != null ? "✓" : "✗")}");
            Debug.Log($"主面板: {(mainPanel != null ? "✓" : "✗")} - {(mainPanel?.activeInHierarchy == true ? "显示" : "隐藏")}");
            Debug.Log($"iframe容器: {(iframeContainer != null ? "✓" : "✗")}");
            Debug.Log($"刷新按钮: {(refreshButton != null ? "✓" : "✗")}");
            Debug.Log($"关闭按钮: {(closeButton != null ? "✓" : "✗")}");
            Debug.Log($"当前URL: {currentIframeUrl}");
            Debug.Log($"桌台ID: {tableId}");
            Debug.Log($"游戏类型: {gameType}");
            Debug.Log($"iframe容器ID: {iframeContainerId}");
        }

        /// <summary>
        /// 测试所有功能
        /// </summary>
        [ContextMenu("测试所有功能")]
        public void TestAllFunctions()
        {
            Debug.Log("[RoadmapPanel] 开始测试所有功能");
            
            RefreshRoadmap();
            System.Threading.Thread.Sleep(500);
            SetTableId(999);
            System.Threading.Thread.Sleep(500);
            
            Debug.Log("[RoadmapPanel] 功能测试完成");
        }

        /// <summary>
        /// 删除所有创建的UI
        /// </summary>
        [ContextMenu("删除所有UI")]
        public void ClearAllUI()
        {
            if (mainPanel != null)
            {
                if (Application.isPlaying)
                    Destroy(mainPanel);
                else
                    DestroyImmediate(mainPanel);
                mainPanel = null;
            }
            
            // 清空引用
            roadmapPanel = null;
            refreshButton = null;
            closeButton = null;
            titleText = null;
            iframeContainer = null;
            
            roadmapUICreated = false;
            isPanelOpen = false;
            
            Debug.Log("[RoadmapPanel] 所有UI已删除");
        }

        #endregion
    }
}
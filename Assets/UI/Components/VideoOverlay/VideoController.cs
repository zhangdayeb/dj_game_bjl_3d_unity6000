// Assets/UI/Components/VideoOverlay/VideoController.cs
// 视频控制器组件 - 启动即显示版本
// 自动创建并立即显示视频播放器UI
// 创建时间: 2025/6/26

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BaccaratGame.Data;

namespace BaccaratGame.UI.Components
{
    /// <summary>
    /// 视频控制器 - 启动即显示版本
    /// 组件启动时立即创建并显示视频播放器界面
    /// </summary>
    public class VideoController : MonoBehaviour
    {
        #region 序列化字段

        [Header("自动显示设置")]
        public bool autoCreateAndShow = false;
        public bool showOnAwake = false;
        public bool immediateDisplay = false;
        
        [Header("视频播放器布局")]
        public Vector2 videoPlayerSize = new Vector2(800, 450);
        public Vector2 videoPlayerPosition = new Vector2(0, 100);
        public Vector2 controlPanelSize = new Vector2(300, 100);
        public Vector2 controlPanelPosition = new Vector2(0, -200);
        
        [Header("视频配置")]
        public string nearVideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ";
        public string farVideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ";
        public bool autoStartBothVideos = true;
        public int videoWidth = 800;
        public int videoHeight = 450;
        
        [Header("iframe配置")]
        public string nearIframeId = "video-near";
        public string farIframeId = "video-far";
        
        [Header("UI样式")]
        public Color activeButtonColor = new Color(1f, 0.8f, 0f, 1f);
        public Color normalButtonColor = Color.white;
        public Color connectedColor = Color.green;
        public Color disconnectedColor = Color.red;
        public Color videoBackgroundColor = Color.black;
        public Color controlPanelColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        [Header("现有组件引用 (可选)")]
        public GameObject nearVideoContainer;
        public GameObject farVideoContainer;
        public Button nearViewButton;
        public Button farViewButton;
        public Button refreshButton;
        public GameObject loadingPanel;
        public TextMeshProUGUI statusText;
        public Image connectionIndicator;

        [Header("调试设置")]
        public bool enableDebugMode = true;

        #endregion

        #region 私有字段

        private RectTransform rectTransform;
        private Canvas parentCanvas;
        private bool videoUICreated = false;
        private VideoViewType currentView = VideoViewType.Near;
        private bool isNearVideoLoaded = false;
        private bool isFarVideoLoaded = false;
        
        // UI组件引用
        private GameObject videoPlayerPanel;
        private GameObject controlPanel;
        private RawImage videoDisplay;
        private Button switchViewButton;
        private Button fullscreenButton;
        private Slider volumeSlider;
        private Text currentViewLabel;

        #endregion

        #region 枚举定义

        public enum VideoViewType
        {
            Near,   // 近景
            Far     // 远景
        }
        
        public enum VideoStatus
        {
            Loading,
            Connected,
            Disconnected,
            Error
        }

        #endregion

        #region 生命周期

        private void Awake()
        {
            InitializeComponent();
            
            if (showOnAwake)
            {
                CreateAndShowVideoPlayer();
            }
        }

        private void Start()
        {
            if (!videoUICreated && autoCreateAndShow)
            {
                CreateAndShowVideoPlayer();
            }
            
            SetupExistingComponents();
            
            if (autoStartBothVideos)
            {
                LoadBothVideos();
            }
        }

        private void OnValidate()
        {
            // 在编辑器中实时预览
            if (Application.isEditor && !Application.isPlaying)
            {
                if (immediateDisplay)
                {
                    InitializeComponent();
                    CreateAndShowVideoPlayer();
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
                Debug.Log("[VideoController] 组件初始化完成");
        }

        /// <summary>
        /// 如需要则创建Canvas
        /// </summary>
        private void CreateCanvasIfNeeded()
        {
            GameObject canvasObj = new GameObject("VideoControllerCanvas");
            canvasObj.transform.SetParent(transform.parent);
            
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
            
            // 将VideoController移到Canvas下
            transform.SetParent(canvasObj.transform);
            
            parentCanvas = canvas;
            
            if (enableDebugMode)
                Debug.Log("[VideoController] 创建了新的Canvas");
        }

        #endregion

        #region 视频播放器创建

        /// <summary>
        /// 创建并显示视频播放器
        /// </summary>
        [ContextMenu("创建并显示视频播放器")]
        public void CreateAndShowVideoPlayer()
        {
            if (videoUICreated) return;

            try
            {
                // 确保组件已初始化
                if (rectTransform == null)
                    InitializeComponent();

                // 创建视频播放器面板
                CreateVideoPlayerPanel();
                
                // 创建控制面板
                CreateControlPanel();
                
                // 创建状态指示器
                CreateStatusIndicators();

                videoUICreated = true;
                
                if (enableDebugMode)
                    Debug.Log("[VideoController] 视频播放器UI创建完成并已显示");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VideoController] 创建视频播放器时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建视频播放器面板
        /// </summary>
        private void CreateVideoPlayerPanel()
        {
            // 创建主面板
            GameObject panelObj = new GameObject("VideoPlayerPanel");
            panelObj.transform.SetParent(transform);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.sizeDelta = videoPlayerSize;
            panelRect.anchoredPosition = videoPlayerPosition;
            panelRect.localScale = Vector3.one;

            // 添加背景
            Image panelBackground = panelObj.AddComponent<Image>();
            panelBackground.color = videoBackgroundColor;
            panelBackground.sprite = CreateSolidSprite(Color.white);

            videoPlayerPanel = panelObj;

            // 创建视频显示区域
            CreateVideoDisplay(panelObj);
            
            // 创建近景容器
            CreateVideoContainer(panelObj, "NearVideoContainer", nearIframeId);
            
            // 创建远景容器
            CreateVideoContainer(panelObj, "FarVideoContainer", farIframeId);

            if (enableDebugMode)
                Debug.Log("[VideoController] 视频播放器面板创建完成");
        }

        /// <summary>
        /// 创建视频显示区域
        /// </summary>
        private void CreateVideoDisplay(GameObject parent)
        {
            GameObject displayObj = new GameObject("VideoDisplay");
            displayObj.transform.SetParent(parent.transform);

            RectTransform displayRect = displayObj.AddComponent<RectTransform>();
            displayRect.anchorMin = Vector2.zero;
            displayRect.anchorMax = Vector2.one;
            displayRect.offsetMin = new Vector2(10, 10);
            displayRect.offsetMax = new Vector2(-10, -10);
            displayRect.localScale = Vector3.one;

            // 添加RawImage用于显示视频
            videoDisplay = displayObj.AddComponent<RawImage>();
            videoDisplay.color = Color.black;

            // 添加占位文本
            CreateVideoPlaceholderText(displayObj);
        }

        /// <summary>
        /// 创建视频占位文本
        /// </summary>
        private void CreateVideoPlaceholderText(GameObject parent)
        {
            GameObject textObj = new GameObject("PlaceholderText");
            textObj.transform.SetParent(parent.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textRect.localScale = Vector3.one;

            Text placeholderText = textObj.AddComponent<Text>();
            placeholderText.text = "视频播放器\n(WebGL环境中将显示实际视频)";
            placeholderText.color = Color.white;
            placeholderText.alignment = TextAnchor.MiddleCenter;
            placeholderText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            placeholderText.fontSize = 16;
        }

        /// <summary>
        /// 创建视频容器
        /// </summary>
        private void CreateVideoContainer(GameObject parent, string containerName, string iframeId)
        {
            GameObject containerObj = new GameObject(containerName);
            containerObj.transform.SetParent(parent.transform);

            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            containerRect.localScale = Vector3.one;

            // 根据容器名称分配引用
            if (containerName.Contains("Near"))
            {
                if (nearVideoContainer == null) nearVideoContainer = containerObj;
            }
            else if (containerName.Contains("Far"))
            {
                if (farVideoContainer == null) farVideoContainer = containerObj;
                containerObj.SetActive(false); // 默认隐藏远景
            }

            if (enableDebugMode)
                Debug.Log($"[VideoController] 创建视频容器: {containerName}");
        }

        /// <summary>
        /// 创建控制面板
        /// </summary>
        private void CreateControlPanel()
        {
            // 创建控制面板
            GameObject panelObj = new GameObject("ControlPanel");
            panelObj.transform.SetParent(transform);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.sizeDelta = controlPanelSize;
            panelRect.anchoredPosition = controlPanelPosition;
            panelRect.localScale = Vector3.one;

            // 添加背景
            Image panelBackground = panelObj.AddComponent<Image>();
            panelBackground.color = controlPanelColor;
            panelBackground.sprite = CreateSolidSprite(Color.white);

            controlPanel = panelObj;

            // 创建控制按钮
            CreateControlButtons(panelObj);
            
            // 创建当前视角标签
            CreateCurrentViewLabel(panelObj);

            if (enableDebugMode)
                Debug.Log("[VideoController] 控制面板创建完成");
        }

        /// <summary>
        /// 创建控制按钮
        /// </summary>
        private void CreateControlButtons(GameObject parent)
        {
            // 近景按钮
            nearViewButton = CreateControlButton(parent, "NearViewButton", "近景", 
                new Vector2(-120, 20), () => SwitchToView(VideoViewType.Near));

            // 远景按钮
            farViewButton = CreateControlButton(parent, "FarViewButton", "远景", 
                new Vector2(-40, 20), () => SwitchToView(VideoViewType.Far));

            // 刷新按钮
            refreshButton = CreateControlButton(parent, "RefreshButton", "刷新", 
                new Vector2(40, 20), () => RefreshCurrentVideo());

            // 全屏按钮
            fullscreenButton = CreateControlButton(parent, "FullscreenButton", "全屏", 
                new Vector2(120, 20), () => ToggleFullscreen());

            // 切换视角按钮
            switchViewButton = CreateControlButton(parent, "SwitchViewButton", "切换", 
                new Vector2(0, -20), () => SwitchView());

            // 创建音量滑块
            CreateVolumeSlider(parent);
        }

        /// <summary>
        /// 创建控制按钮
        /// </summary>
        private Button CreateControlButton(GameObject parent, string buttonName, string buttonText, Vector2 position, UnityEngine.Events.UnityAction onClick)
        {
            GameObject buttonObj = new GameObject(buttonName);
            buttonObj.transform.SetParent(parent.transform);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(70, 30);
            buttonRect.anchoredPosition = position;
            buttonRect.localScale = Vector3.one;

            // 添加背景
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = normalButtonColor;
            buttonImage.sprite = CreateSolidSprite(Color.white);

            // 添加Button组件
            Button button = buttonObj.AddComponent<Button>();
            
            // 设置颜色状态
            ColorBlock colors = button.colors;
            colors.normalColor = normalButtonColor;
            colors.highlightedColor = activeButtonColor;
            colors.pressedColor = new Color(activeButtonColor.r * 0.8f, activeButtonColor.g * 0.8f, activeButtonColor.b * 0.8f, 1f);
            button.colors = colors;

            // 添加点击事件
            button.onClick.AddListener(onClick);

            // 创建按钮文本
            CreateButtonText(buttonObj, buttonText);

            return button;
        }

        /// <summary>
        /// 创建按钮文本
        /// </summary>
        private void CreateButtonText(GameObject buttonObj, string text)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textRect.localScale = Vector3.one;

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = text;
            buttonText.color = Color.black;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = 12;
            buttonText.fontStyle = FontStyle.Bold;
        }

        /// <summary>
        /// 创建音量滑块
        /// </summary>
        private void CreateVolumeSlider(GameObject parent)
        {
            GameObject sliderObj = new GameObject("VolumeSlider");
            sliderObj.transform.SetParent(parent.transform);

            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(120, 20);
            sliderRect.anchoredPosition = new Vector2(0, -50);
            sliderRect.localScale = Vector3.one;

            volumeSlider = sliderObj.AddComponent<Slider>();
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = 0.8f;

            // 创建滑块背景
            Image sliderBackground = sliderObj.AddComponent<Image>();
            sliderBackground.color = Color.gray;
            sliderBackground.sprite = CreateSolidSprite(Color.white);

            // 创建滑块Handle
            CreateSliderHandle(sliderObj);

            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        /// <summary>
        /// 创建滑块Handle
        /// </summary>
        private void CreateSliderHandle(GameObject sliderObj)
        {
            GameObject handleAreaObj = new GameObject("Handle Slide Area");
            handleAreaObj.transform.SetParent(sliderObj.transform);

            RectTransform handleAreaRect = handleAreaObj.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(10, 0);
            handleAreaRect.offsetMax = new Vector2(-10, 0);

            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(handleAreaObj.transform);

            RectTransform handleRect = handleObj.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 20);

            Image handleImage = handleObj.AddComponent<Image>();
            handleImage.color = activeButtonColor;
            handleImage.sprite = CreateSolidSprite(Color.white);

            volumeSlider.handleRect = handleRect;
            volumeSlider.targetGraphic = handleImage;
        }

        /// <summary>
        /// 创建当前视角标签
        /// </summary>
        private void CreateCurrentViewLabel(GameObject parent)
        {
            GameObject labelObj = new GameObject("CurrentViewLabel");
            labelObj.transform.SetParent(parent.transform);

            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(200, 25);
            labelRect.anchoredPosition = new Vector2(0, 50);
            labelRect.localScale = Vector3.one;

            currentViewLabel = labelObj.AddComponent<Text>();
            currentViewLabel.text = "当前视角: 近景";
            currentViewLabel.color = Color.white;
            currentViewLabel.alignment = TextAnchor.MiddleCenter;
            currentViewLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            currentViewLabel.fontSize = 14;
            currentViewLabel.fontStyle = FontStyle.Bold;
        }

        /// <summary>
        /// 创建状态指示器
        /// </summary>
        private void CreateStatusIndicators()
        {
            // 创建状态面板
            GameObject statusPanelObj = new GameObject("StatusPanel");
            statusPanelObj.transform.SetParent(transform);

            RectTransform statusRect = statusPanelObj.AddComponent<RectTransform>();
            statusRect.sizeDelta = new Vector2(200, 60);
            statusRect.anchoredPosition = new Vector2(300, 200);
            statusRect.localScale = Vector3.one;

            // 状态面板背景
            Image statusBackground = statusPanelObj.AddComponent<Image>();
            statusBackground.color = new Color(0, 0, 0, 0.7f);
            statusBackground.sprite = CreateSolidSprite(Color.white);

            // 创建状态文本
            CreateStatusText(statusPanelObj);
            
            // 创建连接指示器
            CreateConnectionIndicator(statusPanelObj);

            if (enableDebugMode)
                Debug.Log("[VideoController] 状态指示器创建完成");
        }

        /// <summary>
        /// 创建状态文本
        /// </summary>
        private void CreateStatusText(GameObject parent)
        {
            GameObject textObj = new GameObject("StatusText");
            textObj.transform.SetParent(parent.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0.5f);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = new Vector2(5, 0);
            textRect.offsetMax = new Vector2(-30, -5);
            textRect.localScale = Vector3.one;

            if (statusText == null)
            {
                statusText = textObj.AddComponent<TextMeshProUGUI>();
                statusText.text = "视频初始化中...";
                statusText.color = Color.white;
                statusText.alignment = TextAlignmentOptions.MidlineLeft;
                statusText.fontSize = 12;
            }
        }

        /// <summary>
        /// 创建连接指示器
        /// </summary>
        private void CreateConnectionIndicator(GameObject parent)
        {
            GameObject indicatorObj = new GameObject("ConnectionIndicator");
            indicatorObj.transform.SetParent(parent.transform);

            RectTransform indicatorRect = indicatorObj.AddComponent<RectTransform>();
            indicatorRect.sizeDelta = new Vector2(20, 20);
            indicatorRect.anchoredPosition = new Vector2(-15, 0);
            indicatorRect.localScale = Vector3.one;

            if (connectionIndicator == null)
            {
                connectionIndicator = indicatorObj.AddComponent<Image>();
                connectionIndicator.color = Color.yellow;
                connectionIndicator.sprite = CreateCircleSprite();
            }
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
        /// 创建圆形Sprite
        /// </summary>
        private Sprite CreateCircleSprite()
        {
            int size = 32;
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

        #endregion

        #region 现有组件设置

        /// <summary>
        /// 设置现有组件
        /// </summary>
        private void SetupExistingComponents()
        {
            // 如果有现有的按钮引用，设置它们的事件
            if (nearViewButton != null && !videoUICreated)
            {
                nearViewButton.onClick.RemoveAllListeners();
                nearViewButton.onClick.AddListener(() => SwitchToView(VideoViewType.Near));
            }

            if (farViewButton != null && !videoUICreated)
            {
                farViewButton.onClick.RemoveAllListeners();
                farViewButton.onClick.AddListener(() => SwitchToView(VideoViewType.Far));
            }

            if (refreshButton != null && !videoUICreated)
            {
                refreshButton.onClick.RemoveAllListeners();
                refreshButton.onClick.AddListener(() => RefreshCurrentVideo());
            }

            if (enableDebugMode)
                Debug.Log("[VideoController] 现有组件设置完成");
        }

        #endregion

        #region 视频控制逻辑

        /// <summary>
        /// 加载双路视频
        /// </summary>
        private void LoadBothVideos()
        {
            if (string.IsNullOrEmpty(nearVideoUrl) || string.IsNullOrEmpty(farVideoUrl))
            {
                UpdateStatusText("视频URL未设置");
                UpdateConnectionIndicator(VideoStatus.Error);
                return;
            }
            
            UpdateConnectionIndicator(VideoStatus.Loading);
            UpdateStatusText("加载视频中...");
            
            // 调用WebGL函数加载视频
            CallWebGLFunction("loadDualVideos", 
                $"{nearVideoUrl},{farVideoUrl},{videoWidth},{videoHeight},{nearIframeId},{farIframeId}");
            
            // 模拟加载完成
            isNearVideoLoaded = true;
            isFarVideoLoaded = true;
            
            UpdateStatusText($"双路视频加载完成 - 当前: {GetViewName(currentView)}");
            UpdateConnectionIndicator(VideoStatus.Connected);

            if (enableDebugMode)
                Debug.Log("[VideoController] 双路视频加载完成");
        }

        /// <summary>
        /// 切换到指定视角
        /// </summary>
        public void SwitchToView(VideoViewType viewType)
        {
            if (currentView == viewType) return;

            currentView = viewType;
            
            // 设置容器可见性
            SetVideoContainerVisibility(VideoViewType.Near, viewType == VideoViewType.Near);
            SetVideoContainerVisibility(VideoViewType.Far, viewType == VideoViewType.Far);
            
            // 更新按钮状态
            UpdateButtonStates();
            
            // 更新状态显示
            UpdateStatusText($"切换到{GetViewName(viewType)}视角");
            if (currentViewLabel != null)
            {
                currentViewLabel.text = $"当前视角: {GetViewName(viewType)}";
            }

            if (enableDebugMode)
                Debug.Log($"[VideoController] 切换到{GetViewName(viewType)}视角");
        }

        /// <summary>
        /// 切换视角
        /// </summary>
        public void SwitchView()
        {
            VideoViewType newView = currentView == VideoViewType.Near ? VideoViewType.Far : VideoViewType.Near;
            SwitchToView(newView);
        }

        /// <summary>
        /// 刷新当前视频
        /// </summary>
        public void RefreshCurrentVideo()
        {
            UpdateStatusText("刷新视频中...");
            UpdateConnectionIndicator(VideoStatus.Loading);
            
            // 刷新当前视频
            string currentUrl = currentView == VideoViewType.Near ? nearVideoUrl : farVideoUrl;
            string currentIframeId = currentView == VideoViewType.Near ? nearIframeId : farIframeId;
            
            CallWebGLFunction("refreshVideo", $"{currentIframeId},{currentUrl}");
            
            UpdateStatusText($"{GetViewName(currentView)}视频已刷新");
            UpdateConnectionIndicator(VideoStatus.Connected);

            if (enableDebugMode)
                Debug.Log($"[VideoController] 刷新{GetViewName(currentView)}视频");
        }

        /// <summary>
        /// 切换全屏模式
        /// </summary>
        public void ToggleFullscreen()
        {
            string currentIframeId = currentView == VideoViewType.Near ? nearIframeId : farIframeId;
            CallWebGLFunction("toggleFullscreen", currentIframeId);

            if (enableDebugMode)
                Debug.Log("[VideoController] 切换全屏模式");
        }

        /// <summary>
        /// 音量改变处理
        /// </summary>
        private void OnVolumeChanged(float volume)
        {
            CallWebGLFunction("setVolume", volume.ToString("F2"));

            if (enableDebugMode)
                Debug.Log($"[VideoController] 音量设置为: {volume:F2}");
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 设置视频容器可见性
        /// </summary>
        private void SetVideoContainerVisibility(VideoViewType viewType, bool visible)
        {
            GameObject container = viewType == VideoViewType.Near ? nearVideoContainer : farVideoContainer;
            
            if (container != null)
            {
                container.SetActive(visible);
            }
            
            // 通过WebGL控制iframe显示
            string iframeId = viewType == VideoViewType.Near ? nearIframeId : farIframeId;
            CallWebGLFunction("setVideoVisibility", $"{iframeId},{(visible ? "1" : "0")}");
        }

        /// <summary>
        /// 更新按钮状态
        /// </summary>
        private void UpdateButtonStates()
        {
            // 更新近景按钮
            if (nearViewButton != null)
            {
                var colors = nearViewButton.colors;
                colors.normalColor = currentView == VideoViewType.Near ? activeButtonColor : normalButtonColor;
                nearViewButton.colors = colors;
            }
            
            // 更新远景按钮
            if (farViewButton != null)
            {
                var colors = farViewButton.colors;
                colors.normalColor = currentView == VideoViewType.Far ? activeButtonColor : normalButtonColor;
                farViewButton.colors = colors;
            }
        }

        /// <summary>
        /// 更新状态文本
        /// </summary>
        private void UpdateStatusText(string status)
        {
            if (statusText != null)
            {
                statusText.text = status;
            }
        }

        /// <summary>
        /// 更新连接指示器
        /// </summary>
        private void UpdateConnectionIndicator(VideoStatus status)
        {
            if (connectionIndicator == null) return;
            
            connectionIndicator.color = status switch
            {
                VideoStatus.Loading => Color.yellow,
                VideoStatus.Connected => connectedColor,
                VideoStatus.Disconnected => disconnectedColor,
                VideoStatus.Error => disconnectedColor,
                _ => Color.yellow
            };
        }

        /// <summary>
        /// 获取视角名称
        /// </summary>
        private string GetViewName(VideoViewType viewType)
        {
            return viewType == VideoViewType.Near ? "近景" : "远景";
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
                Debug.LogError($"[VideoController] WebGL函数调用失败: {e.Message}");
            }
#else
            if (enableDebugMode)
                Debug.Log($"[VideoController] 模拟WebGL调用: {functionName}({parameter})");
#endif
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 强制显示视频播放器
        /// </summary>
        [ContextMenu("强制显示视频播放器")]
        public void ForceShowVideoPlayer()
        {
            videoUICreated = false;
            CreateAndShowVideoPlayer();
        }

        /// <summary>
        /// 获取当前视角
        /// </summary>
        public VideoViewType GetCurrentView()
        {
            return currentView;
        }

        /// <summary>
        /// 检查视频是否已加载
        /// </summary>
        public bool AreVideosLoaded()
        {
            return isNearVideoLoaded && isFarVideoLoaded;
        }

        /// <summary>
        /// 设置视频URL
        /// </summary>
        public void SetVideoUrls(string nearUrl, string farUrl)
        {
            nearVideoUrl = nearUrl;
            farVideoUrl = farUrl;
            
            if (autoStartBothVideos)
            {
                LoadBothVideos();
            }

            if (enableDebugMode)
                Debug.Log($"[VideoController] 视频URL已更新: Near={nearUrl}, Far={farUrl}");
        }

        /// <summary>
        /// 设置面板可见性
        /// </summary>
        public void SetPanelVisible(bool videoPlayerVisible, bool controlPanelVisible)
        {
            if (videoPlayerPanel != null)
                videoPlayerPanel.SetActive(videoPlayerVisible);
                
            if (controlPanel != null)
                controlPanel.SetActive(controlPanelVisible);
        }

        #endregion

        #region 调试方法

        /// <summary>
        /// 显示组件状态
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowComponentStatus()
        {
            Debug.Log("=== VideoController 组件状态 ===");
            Debug.Log($"自动创建并显示: {autoCreateAndShow}");
            Debug.Log($"启动时显示: {showOnAwake}");
            Debug.Log($"立即显示: {immediateDisplay}");
            Debug.Log($"视频UI已创建: {videoUICreated}");
            Debug.Log($"父Canvas: {(parentCanvas != null ? "✓" : "✗")}");
            Debug.Log($"当前视角: {GetViewName(currentView)}");
            Debug.Log($"近景视频已加载: {isNearVideoLoaded}");
            Debug.Log($"远景视频已加载: {isFarVideoLoaded}");
            Debug.Log($"视频播放器面板: {(videoPlayerPanel != null ? "✓" : "✗")} - {(videoPlayerPanel?.activeInHierarchy == true ? "显示" : "隐藏")}");
            Debug.Log($"控制面板: {(controlPanel != null ? "✓" : "✗")} - {(controlPanel?.activeInHierarchy == true ? "显示" : "隐藏")}");
            Debug.Log($"近景按钮: {(nearViewButton != null ? "✓" : "✗")}");
            Debug.Log($"远景按钮: {(farViewButton != null ? "✓" : "✗")}");
            Debug.Log($"刷新按钮: {(refreshButton != null ? "✓" : "✗")}");
            Debug.Log($"状态文本: {(statusText != null ? "✓" : "✗")}");
            Debug.Log($"连接指示器: {(connectionIndicator != null ? "✓" : "✗")}");
            Debug.Log($"近景视频URL: {nearVideoUrl}");
            Debug.Log($"远景视频URL: {farVideoUrl}");
        }

        /// <summary>
        /// 测试所有功能
        /// </summary>
        [ContextMenu("测试所有功能")]
        public void TestAllFunctions()
        {
            Debug.Log("[VideoController] 开始测试所有功能");
            
            SwitchToView(VideoViewType.Near);
            System.Threading.Thread.Sleep(500);
            SwitchToView(VideoViewType.Far);
            System.Threading.Thread.Sleep(500);
            RefreshCurrentVideo();
            
            Debug.Log("[VideoController] 功能测试完成");
        }

        /// <summary>
        /// 删除所有创建的UI
        /// </summary>
        [ContextMenu("删除所有UI")]
        public void ClearAllUI()
        {
            if (videoPlayerPanel != null)
            {
                if (Application.isPlaying)
                    Destroy(videoPlayerPanel);
                else
                    DestroyImmediate(videoPlayerPanel);
                videoPlayerPanel = null;
            }
            
            if (controlPanel != null)
            {
                if (Application.isPlaying)
                    Destroy(controlPanel);
                else
                    DestroyImmediate(controlPanel);
                controlPanel = null;
            }
            
            // 清空引用
            nearVideoContainer = null;
            farVideoContainer = null;
            nearViewButton = null;
            farViewButton = null;
            refreshButton = null;
            statusText = null;
            connectionIndicator = null;
            
            videoUICreated = false;
            
            Debug.Log("[VideoController] 所有UI已删除");
        }

        #endregion
    }
}
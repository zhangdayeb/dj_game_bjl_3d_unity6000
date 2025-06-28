// Assets/UI/Components/VideoOverlay/Set/AudioController.cs
// 简化版音频控制组件 - 仅用于UI生成
// 挂载到节点上自动创建音频控制面板
// 创建时间: 2025/6/28

using UnityEngine;
using UnityEngine.UI;

namespace BaccaratGame.UI.Components.VideoOverlay
{
    /// <summary>
    /// 简化版音频控制组件
    /// 挂载到节点上自动创建UI，包含音效和背景音乐控制
    /// </summary>
    public class AudioController : MonoBehaviour
    {
        #region 配置参数

        [Header("面板配置")]
        public Vector2 panelSize = new Vector2(300, 200);
        public Vector2 panelPosition = new Vector2(-200, -100); // 相对右上角偏移
        
        [Header("UI样式")]
        public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        public Color buttonEnabledColor = new Color(0.2f, 0.8f, 0.2f, 1f);
        public Color buttonDisabledColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        public Color sliderColor = new Color(0.2f, 0.6f, 1f, 1f);
        public Color textColor = Color.white;
        public int fontSize = 14;
        
        [Header("遮罩层设置")]
        public Color maskColor = new Color(0, 0, 0, 0.3f);

        #endregion

        #region 私有字段

        private bool uiCreated = false;
        private GameObject maskLayer;
        private GameObject audioPanel;
        private Canvas uiCanvas;
        
        // UI组件引用
        private Button musicToggleButton;
        private Button soundToggleButton;
        private Slider musicVolumeSlider;
        private Slider soundVolumeSlider;
        private Text musicVolumeText;
        private Text soundVolumeText;
        
        // 状态
        private bool isMusicEnabled = true;
        private bool isSoundEnabled = true;
        private float musicVolume = 0.6f;
        private float soundVolume = 0.8f;

        #endregion

        #region 生命周期

        private void Awake()
        {
            CreateUI();
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
            CreateAudioPanel();
            CreateAudioControls();
            
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
                GameObject canvasObj = new GameObject("AudioControlCanvas");
                canvasObj.transform.SetParent(transform.parent);
                
                uiCanvas = canvasObj.AddComponent<Canvas>();
                uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                uiCanvas.sortingOrder = 1500; // 确保在最上层
                
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
        /// 创建音频控制面板
        /// </summary>
        private void CreateAudioPanel()
        {
            audioPanel = new GameObject("AudioPanel");
            audioPanel.transform.SetParent(transform);

            RectTransform panelRect = audioPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 1); // 右上角锚点
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(1, 1);
            panelRect.sizeDelta = panelSize;
            panelRect.anchoredPosition = panelPosition;

            Image panelBg = audioPanel.AddComponent<Image>();
            panelBg.color = backgroundColor;
            panelBg.sprite = CreateSimpleSprite();
        }

        /// <summary>
        /// 创建音频控制组件
        /// </summary>
        private void CreateAudioControls()
        {
            CreateTitle();
            CreateMusicControls();
            CreateSoundControls();
        }

        /// <summary>
        /// 创建标题
        /// </summary>
        private void CreateTitle()
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(audioPanel.transform);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.8f);
            titleRect.anchorMax = new Vector2(1, 1f);
            titleRect.offsetMin = new Vector2(10, 0);
            titleRect.offsetMax = new Vector2(-10, -5);

            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "🎵 音频控制";
            titleText.color = textColor;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = fontSize + 4;
            titleText.fontStyle = FontStyle.Bold;
        }

        /// <summary>
        /// 创建音乐控制区域
        /// </summary>
        private void CreateMusicControls()
        {
            // 音乐开关按钮
            CreateMusicToggle();
            
            // 音乐音量滑条
            CreateMusicVolumeSlider();
            
            // 音乐音量文字
            CreateMusicVolumeText();
        }

        /// <summary>
        /// 创建音乐开关按钮
        /// </summary>
        private void CreateMusicToggle()
        {
            GameObject buttonObj = new GameObject("MusicToggle");
            buttonObj.transform.SetParent(audioPanel.transform);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.05f, 0.55f);
            buttonRect.anchorMax = new Vector2(0.35f, 0.75f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;

            musicToggleButton = buttonObj.AddComponent<Button>();
            
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = buttonEnabledColor;
            buttonImage.sprite = CreateSimpleSprite();

            musicToggleButton.onClick.AddListener(() => OnMusicToggle());

            // 按钮文字
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = "🎵 音乐";
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = fontSize;
            buttonText.fontStyle = FontStyle.Bold;
        }

        /// <summary>
        /// 创建音乐音量滑条
        /// </summary>
        private void CreateMusicVolumeSlider()
        {
            GameObject sliderObj = new GameObject("MusicVolumeSlider");
            sliderObj.transform.SetParent(audioPanel.transform);

            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.4f, 0.57f);
            sliderRect.anchorMax = new Vector2(0.8f, 0.73f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;

            musicVolumeSlider = sliderObj.AddComponent<Slider>();
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.value = musicVolume;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

            CreateSliderComponents(sliderObj, musicVolumeSlider);
        }

        /// <summary>
        /// 创建音乐音量文字
        /// </summary>
        private void CreateMusicVolumeText()
        {
            GameObject textObj = new GameObject("MusicVolumeText");
            textObj.transform.SetParent(audioPanel.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.82f, 0.55f);
            textRect.anchorMax = new Vector2(0.98f, 0.75f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            musicVolumeText = textObj.AddComponent<Text>();
            musicVolumeText.text = "60%";
            musicVolumeText.color = textColor;
            musicVolumeText.alignment = TextAnchor.MiddleCenter;
            musicVolumeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            musicVolumeText.fontSize = fontSize - 2;
        }

        /// <summary>
        /// 创建音效控制区域
        /// </summary>
        private void CreateSoundControls()
        {
            // 音效开关按钮
            CreateSoundToggle();
            
            // 音效音量滑条
            CreateSoundVolumeSlider();
            
            // 音效音量文字
            CreateSoundVolumeText();
        }

        /// <summary>
        /// 创建音效开关按钮
        /// </summary>
        private void CreateSoundToggle()
        {
            GameObject buttonObj = new GameObject("SoundToggle");
            buttonObj.transform.SetParent(audioPanel.transform);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.05f, 0.25f);
            buttonRect.anchorMax = new Vector2(0.35f, 0.45f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;

            soundToggleButton = buttonObj.AddComponent<Button>();
            
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = buttonEnabledColor;
            buttonImage.sprite = CreateSimpleSprite();

            soundToggleButton.onClick.AddListener(() => OnSoundToggle());

            // 按钮文字
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = "🔊 音效";
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = fontSize;
            buttonText.fontStyle = FontStyle.Bold;
        }

        /// <summary>
        /// 创建音效音量滑条
        /// </summary>
        private void CreateSoundVolumeSlider()
        {
            GameObject sliderObj = new GameObject("SoundVolumeSlider");
            sliderObj.transform.SetParent(audioPanel.transform);

            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.4f, 0.27f);
            sliderRect.anchorMax = new Vector2(0.8f, 0.43f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;

            soundVolumeSlider = sliderObj.AddComponent<Slider>();
            soundVolumeSlider.minValue = 0f;
            soundVolumeSlider.maxValue = 1f;
            soundVolumeSlider.value = soundVolume;
            soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeChanged);

            CreateSliderComponents(sliderObj, soundVolumeSlider);
        }

        /// <summary>
        /// 创建音效音量文字
        /// </summary>
        private void CreateSoundVolumeText()
        {
            GameObject textObj = new GameObject("SoundVolumeText");
            textObj.transform.SetParent(audioPanel.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.82f, 0.25f);
            textRect.anchorMax = new Vector2(0.98f, 0.45f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            soundVolumeText = textObj.AddComponent<Text>();
            soundVolumeText.text = "80%";
            soundVolumeText.color = textColor;
            soundVolumeText.alignment = TextAnchor.MiddleCenter;
            soundVolumeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            soundVolumeText.fontSize = fontSize - 2;
        }

        /// <summary>
        /// 创建滑条组件
        /// </summary>
        private void CreateSliderComponents(GameObject sliderObj, Slider slider)
        {
            // 背景
            GameObject backgroundObj = new GameObject("Background");
            backgroundObj.transform.SetParent(sliderObj.transform);

            RectTransform bgRect = backgroundObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            Image bgImage = backgroundObj.AddComponent<Image>();
            bgImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            bgImage.sprite = CreateSimpleSprite();

            // 填充区域
            GameObject fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform);

            RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform);

            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.color = sliderColor;
            fillImage.sprite = CreateSimpleSprite();

            // 手柄区域
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
            handleImage.color = Color.white;
            handleImage.sprite = CreateSimpleSprite();

            // 设置滑条引用
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
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

        #region 事件处理

        /// <summary>
        /// 音乐开关点击
        /// </summary>
        private void OnMusicToggle()
        {
            isMusicEnabled = !isMusicEnabled;
            
            Image buttonImage = musicToggleButton.GetComponent<Image>();
            buttonImage.color = isMusicEnabled ? buttonEnabledColor : buttonDisabledColor;
            
            Debug.Log($"[AudioController] 音乐{(isMusicEnabled ? "开启" : "关闭")}");
        }

        /// <summary>
        /// 音效开关点击
        /// </summary>
        private void OnSoundToggle()
        {
            isSoundEnabled = !isSoundEnabled;
            
            Image buttonImage = soundToggleButton.GetComponent<Image>();
            buttonImage.color = isSoundEnabled ? buttonEnabledColor : buttonDisabledColor;
            
            Debug.Log($"[AudioController] 音效{(isSoundEnabled ? "开启" : "关闭")}");
        }

        /// <summary>
        /// 音乐音量改变
        /// </summary>
        private void OnMusicVolumeChanged(float value)
        {
            musicVolume = value;
            musicVolumeText.text = $"{(int)(value * 100)}%";
            Debug.Log($"[AudioController] 音乐音量: {(int)(value * 100)}%");
        }

        /// <summary>
        /// 音效音量改变
        /// </summary>
        private void OnSoundVolumeChanged(float value)
        {
            soundVolume = value;
            soundVolumeText.text = $"{(int)(value * 100)}%";
            Debug.Log($"[AudioController] 音效音量: {(int)(value * 100)}%");
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void HidePanel()
        {
            if (maskLayer != null) maskLayer.SetActive(false);
            if (audioPanel != null) audioPanel.SetActive(false);
            Debug.Log("[AudioController] 面板已隐藏");
        }

        /// <summary>
        /// 显示面板
        /// </summary>
        public void ShowPanel()
        {
            if (maskLayer != null) maskLayer.SetActive(true);
            if (audioPanel != null) audioPanel.SetActive(true);
            Debug.Log("[AudioController] 面板已显示");
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
        /// 更新面板位置
        /// </summary>
        public void UpdatePosition(Vector2 newPosition)
        {
            panelPosition = newPosition;
            if (audioPanel != null)
            {
                RectTransform panelRect = audioPanel.GetComponent<RectTransform>();
                panelRect.anchoredPosition = newPosition;
            }
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
        }

        /// <summary>
        /// 显示组件状态
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowStatus()
        {
            Debug.Log($"[AudioController] UI已创建: {uiCreated}");
            Debug.Log($"[AudioController] 遮罩层: {(maskLayer != null ? "✓" : "✗")}");
            Debug.Log($"[AudioController] 音频面板: {(audioPanel != null ? "✓" : "✗")}");
            Debug.Log($"[AudioController] 音乐状态: {(isMusicEnabled ? "开启" : "关闭")} - 音量: {(int)(musicVolume * 100)}%");
            Debug.Log($"[AudioController] 音效状态: {(isSoundEnabled ? "开启" : "关闭")} - 音量: {(int)(soundVolume * 100)}%");
        }

        #endregion
    }
}
// Assets/UI/Components/VideoOverlay/Set/AudioController.cs
// 音频控制组件 - 持久化显示版本
// 完整的音频控制面板，包含音乐、音效开关和音量控制
// 特点：执行后UI依然可见，支持编辑器预览和持久显示
// 创建时间: 2025/6/26

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BaccaratGame.UI.Components
{
    /// <summary>
    /// 音频控制组件 - 持久化显示版本
    /// 立即创建并持久显示UI，不依赖运行状态
    /// </summary>
    public class AudioController : MonoBehaviour
    {
        #region 序列化字段

        [Header("自动显示设置")]
        [SerializeField] private bool autoCreateAndShow = true;         // 自动创建并显示
        [SerializeField] private bool showOnAwake = true;               // 启动时显示
        [SerializeField] private bool immediateDisplay = true;          // 立即显示
        [SerializeField] private bool enableDebugMode = true;           // 启用调试模式

        [Header("音频源")]
        [SerializeField] private AudioSource backgroundMusicSource;     // 背景音乐源
        [SerializeField] private AudioSource soundEffectSource;         // 音效源
        [SerializeField] private AudioSource uiSoundSource;             // UI音效源

        [Header("UI组件引用")]
        [SerializeField] private GameObject controlPanel;               // 控制面板
        [SerializeField] private Image backgroundImage;                 // 背景图片
        [SerializeField] private Text titleText;                        // 标题文字
        [SerializeField] private Button musicToggleButton;              // 音乐开关按钮
        [SerializeField] private Button soundToggleButton;              // 音效开关按钮
        [SerializeField] private Slider musicVolumeSlider;              // 音乐音量滑条
        [SerializeField] private Slider soundVolumeSlider;              // 音效音量滑条
        [SerializeField] private Text musicVolumeText;                  // 音乐音量文字
        [SerializeField] private Text soundVolumeText;                  // 音效音量文字
        [SerializeField] private Image musicIcon;                       // 音乐图标
        [SerializeField] private Image soundIcon;                       // 音效图标

        [Header("布局配置")]
        [SerializeField] private Vector2 panelSize = new Vector2(320, 240);        // 面板大小
        [SerializeField] private Vector2 panelPosition = new Vector2(-160, 120);   // 面板位置

        [Header("显示配置")]
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.9f); // 背景颜色
        [SerializeField] private Color enabledColor = Color.green;                 // 启用状态颜色
        [SerializeField] private Color disabledColor = Color.red;                  // 禁用状态颜色
        [SerializeField] private Color sliderColor = new Color(0.2f, 0.4f, 0.8f); // 滑条颜色
        [SerializeField] private Color textColor = Color.white;                    // 文字颜色
        [SerializeField] private int titleFontSize = 18;                           // 标题字体大小
        [SerializeField] private int labelFontSize = 14;                           // 标签字体大小
        [SerializeField] private int volumeFontSize = 12;                          // 音量字体大小

        [Header("音频设置")]
        [SerializeField, Range(0f, 1f)] private float defaultMusicVolume = 0.6f;  // 默认音乐音量
        [SerializeField, Range(0f, 1f)] private float defaultSoundVolume = 0.8f;  // 默认音效音量
        [SerializeField] private bool autoPlayMusic = true;                       // 自动播放音乐
        [SerializeField] private bool enableVolumeText = true;                     // 启用音量文字显示
        [SerializeField] private bool saveSettingsAutomatically = true;           // 自动保存设置

        [Header("动画配置")]
        [SerializeField] private bool enableButtonAnimations = true;      // 启用按钮动画
        [SerializeField] private bool enableSliderAnimations = true;      // 启用滑条动画
        [SerializeField] private float animationDuration = 0.2f;          // 动画持续时间

        [Header("演示配置")]
        [SerializeField] private bool enableAutoDemo = true;              // 启用自动演示
        [SerializeField] private float demoInterval = 5f;                 // 演示间隔

        #endregion

        #region 私有变量

        // UI对象引用
        private GameObject audioPanel;                   // 音频面板
        private RectTransform audioRect;                 // 面板RectTransform
        private Canvas parentCanvas;                     // 父Canvas

        // 状态变量
        private bool isMusicEnabled = true;              // 音乐启用状态
        private bool isSoundEnabled = true;              // 音效启用状态
        private float currentMusicVolume;                // 当前音乐音量
        private float currentSoundVolume;                // 当前音效音量
        private bool isPanelVisible = true;              // 面板可见状态
        private bool audioUICreated = false;             // UI是否已创建
        private Vector3 originalScale;                   // 原始缩放

        // 音效字典
        private Dictionary<string, AudioClip> soundEffects = new Dictionary<string, AudioClip>();

        // 存储键名
        private const string MUSIC_ENABLED_KEY = "AudioController_MusicEnabled";
        private const string SOUND_ENABLED_KEY = "AudioController_SoundEnabled";
        private const string MUSIC_VOLUME_KEY = "AudioController_MusicVolume";
        private const string SOUND_VOLUME_KEY = "AudioController_SoundVolume";

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            if (enableDebugMode)
                Debug.Log("[AudioController] Awake - 开始初始化");

            // 立即创建UI以确保持久显示
            if (autoCreateAndShow)
            {
                CreateAndShowAudioControl();
            }

            // 初始化音频源
            InitializeAudioSources();

            // 加载设置
            LoadSettings();
        }

        private void Start()
        {
            if (enableDebugMode)
                Debug.Log("[AudioController] Start - 开始设置");

            // 确保UI已创建
            if (!audioUICreated && showOnAwake)
            {
                CreateAndShowAudioControl();
            }

            // 更新UI显示
            UpdateUI();

            // 自动播放音乐
            if (autoPlayMusic)
            {
                StartBackgroundMusic();
            }

            // 开始演示
            if (enableAutoDemo)
            {
                StartCoroutine(DemoCoroutine());
            }
        }

        private void OnValidate()
        {
            // 在编辑器中实时更新
            if (Application.isPlaying && audioUICreated)
            {
                UpdateDisplayProperties();
            }
        }

        private void OnDestroy()
        {
            if (saveSettingsAutomatically)
            {
                SaveSettings();
            }
        }

        #endregion

        #region UI创建

        /// <summary>
        /// 创建并显示音频控制器
        /// </summary>
        private void CreateAndShowAudioControl()
        {
            if (audioUICreated)
            {
                if (enableDebugMode)
                    Debug.Log("[AudioController] 音频UI已存在，跳过创建");
                return;
            }

            // 查找或创建Canvas
            FindOrCreateCanvas();

            // 创建音频面板
            CreateAudioPanel();

            // 创建UI元素
            CreateAudioUI();

            audioUICreated = true;

            if (enableDebugMode)
                Debug.Log("[AudioController] 音频UI创建并显示完成");
        }

        /// <summary>
        /// 查找或创建Canvas
        /// </summary>
        private void FindOrCreateCanvas()
        {
            // 首先尝试在父级中查找Canvas
            parentCanvas = GetComponentInParent<Canvas>();

            if (parentCanvas == null)
            {
                // 查找场景中的Canvas
                parentCanvas = FindObjectOfType<Canvas>();
            }

            if (parentCanvas == null)
            {
                // 创建新的Canvas
                GameObject canvasObj = new GameObject("AudioControlCanvas");
                parentCanvas = canvasObj.AddComponent<Canvas>();
                parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                parentCanvas.sortingOrder = 200; // 高层级确保音频控制在顶层

                // 添加CanvasScaler
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                // 添加GraphicRaycaster
                canvasObj.AddComponent<GraphicRaycaster>();

                if (enableDebugMode)
                    Debug.Log("[AudioController] 创建新Canvas用于音频控制");
            }
        }

        /// <summary>
        /// 创建音频面板
        /// </summary>
        private void CreateAudioPanel()
        {
            // 创建主面板
            audioPanel = new GameObject("AudioPanel");
            audioPanel.transform.SetParent(parentCanvas.transform);

            // 设置RectTransform
            audioRect = audioPanel.AddComponent<RectTransform>();
            audioRect.anchorMin = new Vector2(1, 1);           // 右上角锚点
            audioRect.anchorMax = new Vector2(1, 1);
            audioRect.sizeDelta = panelSize;
            audioRect.anchoredPosition = panelPosition;

            originalScale = audioRect.localScale;

            if (enableDebugMode)
                Debug.Log($"[AudioController] 音频面板已创建 - 大小:{panelSize}, 位置:{panelPosition}");
        }

        /// <summary>
        /// 创建音频UI元素
        /// </summary>
        private void CreateAudioUI()
        {
            // 创建控制面板
            CreateControlPanel();

            // 创建背景
            CreateBackground();

            // 创建标题
            CreateTitle();

            // 创建音乐控制区域
            CreateMusicControls();

            // 创建音效控制区域
            CreateSoundControls();

            // 初始化显示
            UpdateDisplayProperties();
        }

        /// <summary>
        /// 创建控制面板
        /// </summary>
        private void CreateControlPanel()
        {
            controlPanel = new GameObject("ControlPanel");
            controlPanel.transform.SetParent(audioPanel.transform);

            RectTransform panelRect = controlPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// 创建背景
        /// </summary>
        private void CreateBackground()
        {
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(controlPanel.transform);

            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            backgroundImage = bgObj.AddComponent<Image>();
            backgroundImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
            backgroundImage.color = backgroundColor;
            backgroundImage.type = Image.Type.Sliced;
        }

        /// <summary>
        /// 创建标题
        /// </summary>
        private void CreateTitle()
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(controlPanel.transform);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.8f);
            titleRect.anchorMax = new Vector2(1, 1f);
            titleRect.offsetMin = new Vector2(10, 0);
            titleRect.offsetMax = new Vector2(-10, -5);

            titleText = titleObj.AddComponent<Text>();
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = titleFontSize;
            titleText.color = textColor;
            titleText.text = "音频控制";
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.fontStyle = FontStyle.Bold;
        }

        /// <summary>
        /// 创建音乐控制区域
        /// </summary>
        private void CreateMusicControls()
        {
            // 创建音乐区域容器
            GameObject musicArea = new GameObject("MusicArea");
            musicArea.transform.SetParent(controlPanel.transform);

            RectTransform musicRect = musicArea.AddComponent<RectTransform>();
            musicRect.anchorMin = new Vector2(0, 0.5f);
            musicRect.anchorMax = new Vector2(1, 0.8f);
            musicRect.offsetMin = new Vector2(10, 5);
            musicRect.offsetMax = new Vector2(-10, -5);

            // 创建音乐开关按钮
            CreateMusicToggleButton(musicArea);

            // 创建音乐音量滑条
            CreateMusicVolumeSlider(musicArea);

            // 创建音乐音量文字
            if (enableVolumeText)
                CreateMusicVolumeText(musicArea);
        }

        /// <summary>
        /// 创建音乐开关按钮
        /// </summary>
        private void CreateMusicToggleButton(GameObject parent)
        {
            GameObject buttonObj = new GameObject("MusicToggleButton");
            buttonObj.transform.SetParent(parent.transform);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0, 0);
            buttonRect.anchorMax = new Vector2(0.3f, 1);
            buttonRect.offsetMin = new Vector2(0, 0);
            buttonRect.offsetMax = new Vector2(-5, 0);

            // 添加Button组件
            musicToggleButton = buttonObj.AddComponent<Button>();

            // 添加Image组件作为按钮背景
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Button.psd");
            buttonImage.type = Image.Type.Sliced;
            buttonImage.color = enabledColor;

            // 设置按钮事件
            musicToggleButton.onClick.AddListener(ToggleMusic);

            // 创建按钮文字
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = labelFontSize;
            buttonText.color = Color.white;
            buttonText.text = "音乐";
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.fontStyle = FontStyle.Bold;
        }

        /// <summary>
        /// 创建音乐音量滑条
        /// </summary>
        private void CreateMusicVolumeSlider(GameObject parent)
        {
            GameObject sliderObj = new GameObject("MusicVolumeSlider");
            sliderObj.transform.SetParent(parent.transform);

            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.35f, 0.2f);
            sliderRect.anchorMax = new Vector2(0.8f, 0.8f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;

            // 添加Slider组件
            musicVolumeSlider = sliderObj.AddComponent<Slider>();
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.value = defaultMusicVolume;
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

            // 创建滑条背景
            GameObject backgroundObj = new GameObject("Background");
            backgroundObj.transform.SetParent(sliderObj.transform);

            RectTransform bgRect = backgroundObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            Image bgImage = backgroundObj.AddComponent<Image>();
            bgImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            bgImage.type = Image.Type.Sliced;

            // 创建滑条填充
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
            fillImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            fillImage.color = sliderColor;
            fillImage.type = Image.Type.Sliced;

            // 创建滑条手柄
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
            handleImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            handleImage.color = Color.white;

            // 设置Slider组件的引用
            musicVolumeSlider.fillRect = fillRect;
            musicVolumeSlider.handleRect = handleRect;
            musicVolumeSlider.targetGraphic = handleImage;
        }

        /// <summary>
        /// 创建音乐音量文字
        /// </summary>
        private void CreateMusicVolumeText(GameObject parent)
        {
            GameObject textObj = new GameObject("MusicVolumeText");
            textObj.transform.SetParent(parent.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.85f, 0);
            textRect.anchorMax = new Vector2(1f, 1);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            musicVolumeText = textObj.AddComponent<Text>();
            musicVolumeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            musicVolumeText.fontSize = volumeFontSize;
            musicVolumeText.color = textColor;
            musicVolumeText.text = "60%";
            musicVolumeText.alignment = TextAnchor.MiddleCenter;
        }

        /// <summary>
        /// 创建音效控制区域
        /// </summary>
        private void CreateSoundControls()
        {
            // 创建音效区域容器
            GameObject soundArea = new GameObject("SoundArea");
            soundArea.transform.SetParent(controlPanel.transform);

            RectTransform soundRect = soundArea.AddComponent<RectTransform>();
            soundRect.anchorMin = new Vector2(0, 0.2f);
            soundRect.anchorMax = new Vector2(1, 0.5f);
            soundRect.offsetMin = new Vector2(10, 5);
            soundRect.offsetMax = new Vector2(-10, -5);

            // 创建音效开关按钮
            CreateSoundToggleButton(soundArea);

            // 创建音效音量滑条
            CreateSoundVolumeSlider(soundArea);

            // 创建音效音量文字
            if (enableVolumeText)
                CreateSoundVolumeText(soundArea);
        }

        /// <summary>
        /// 创建音效开关按钮
        /// </summary>
        private void CreateSoundToggleButton(GameObject parent)
        {
            GameObject buttonObj = new GameObject("SoundToggleButton");
            buttonObj.transform.SetParent(parent.transform);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0, 0);
            buttonRect.anchorMax = new Vector2(0.3f, 1);
            buttonRect.offsetMin = new Vector2(0, 0);
            buttonRect.offsetMax = new Vector2(-5, 0);

            // 添加Button组件
            soundToggleButton = buttonObj.AddComponent<Button>();

            // 添加Image组件作为按钮背景
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Button.psd");
            buttonImage.type = Image.Type.Sliced;
            buttonImage.color = enabledColor;

            // 设置按钮事件
            soundToggleButton.onClick.AddListener(ToggleSound);

            // 创建按钮文字
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = labelFontSize;
            buttonText.color = Color.white;
            buttonText.text = "音效";
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.fontStyle = FontStyle.Bold;
        }

        /// <summary>
        /// 创建音效音量滑条
        /// </summary>
        private void CreateSoundVolumeSlider(GameObject parent)
        {
            GameObject sliderObj = new GameObject("SoundVolumeSlider");
            sliderObj.transform.SetParent(parent.transform);

            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.35f, 0.2f);
            sliderRect.anchorMax = new Vector2(0.8f, 0.8f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;

            // 添加Slider组件
            soundVolumeSlider = sliderObj.AddComponent<Slider>();
            soundVolumeSlider.minValue = 0f;
            soundVolumeSlider.maxValue = 1f;
            soundVolumeSlider.value = defaultSoundVolume;
            soundVolumeSlider.onValueChanged.AddListener(SetSoundVolume);

            // 创建滑条背景
            GameObject backgroundObj = new GameObject("Background");
            backgroundObj.transform.SetParent(sliderObj.transform);

            RectTransform bgRect = backgroundObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            Image bgImage = backgroundObj.AddComponent<Image>();
            bgImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            bgImage.type = Image.Type.Sliced;

            // 创建滑条填充
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
            fillImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            fillImage.color = sliderColor;
            fillImage.type = Image.Type.Sliced;

            // 创建滑条手柄
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
            handleImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            handleImage.color = Color.white;

            // 设置Slider组件的引用
            soundVolumeSlider.fillRect = fillRect;
            soundVolumeSlider.handleRect = handleRect;
            soundVolumeSlider.targetGraphic = handleImage;
        }

        /// <summary>
        /// 创建音效音量文字
        /// </summary>
        private void CreateSoundVolumeText(GameObject parent)
        {
            GameObject textObj = new GameObject("SoundVolumeText");
            textObj.transform.SetParent(parent.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.85f, 0);
            textRect.anchorMax = new Vector2(1f, 1);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            soundVolumeText = textObj.AddComponent<Text>();
            soundVolumeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            soundVolumeText.fontSize = volumeFontSize;
            soundVolumeText.color = textColor;
            soundVolumeText.text = "80%";
            soundVolumeText.alignment = TextAnchor.MiddleCenter;
        }

        #endregion

        #region 音频源初始化

        /// <summary>
        /// 初始化音频源
        /// </summary>
        private void InitializeAudioSources()
        {
            // 创建背景音乐源
            if (backgroundMusicSource == null)
            {
                GameObject bgmObj = new GameObject("BackgroundMusicSource");
                bgmObj.transform.SetParent(transform);
                backgroundMusicSource = bgmObj.AddComponent<AudioSource>();
                backgroundMusicSource.loop = true;
                backgroundMusicSource.playOnAwake = false;
                backgroundMusicSource.volume = defaultMusicVolume;
            }

            // 创建音效源
            if (soundEffectSource == null)
            {
                GameObject sfxObj = new GameObject("SoundEffectSource");
                sfxObj.transform.SetParent(transform);
                soundEffectSource = sfxObj.AddComponent<AudioSource>();
                soundEffectSource.loop = false;
                soundEffectSource.playOnAwake = false;
                soundEffectSource.volume = defaultSoundVolume;
            }

            // 创建UI音效源
            if (uiSoundSource == null)
            {
                GameObject uiObj = new GameObject("UISoundSource");
                uiObj.transform.SetParent(transform);
                uiSoundSource = uiObj.AddComponent<AudioSource>();
                uiSoundSource.loop = false;
                uiSoundSource.playOnAwake = false;
                uiSoundSource.volume = defaultSoundVolume * 0.8f;
            }

            // 设置初始音量
            currentMusicVolume = defaultMusicVolume;
            currentSoundVolume = defaultSoundVolume;

            if (enableDebugMode)
                Debug.Log("[AudioController] 音频源初始化完成");
        }

        #endregion

        #region UI更新和显示

        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void UpdateUI()
        {
            if (!audioUICreated) return;

            // 更新按钮颜色
            if (musicToggleButton != null)
            {
                Image musicButtonImage = musicToggleButton.GetComponent<Image>();
                if (musicButtonImage != null)
                    musicButtonImage.color = isMusicEnabled ? enabledColor : disabledColor;
            }

            if (soundToggleButton != null)
            {
                Image soundButtonImage = soundToggleButton.GetComponent<Image>();
                if (soundButtonImage != null)
                    soundButtonImage.color = isSoundEnabled ? enabledColor : disabledColor;
            }

            // 更新滑条值
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = currentMusicVolume;

            if (soundVolumeSlider != null)
                soundVolumeSlider.value = currentSoundVolume;

            // 更新音量文字
            UpdateVolumeText();
        }

        /// <summary>
        /// 更新音量文字显示
        /// </summary>
        private void UpdateVolumeText()
        {
            if (enableVolumeText)
            {
                if (musicVolumeText != null)
                    musicVolumeText.text = $"{(int)(currentMusicVolume * 100)}%";

                if (soundVolumeText != null)
                    soundVolumeText.text = $"{(int)(currentSoundVolume * 100)}%";
            }
        }

        /// <summary>
        /// 更新显示属性
        /// </summary>
        private void UpdateDisplayProperties()
        {
            if (!audioUICreated) return;

            // 更新面板大小和位置
            if (audioRect != null)
            {
                audioRect.sizeDelta = panelSize;
                audioRect.anchoredPosition = panelPosition;
            }

            // 更新背景颜色
            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
            }

            // 更新标题样式
            if (titleText != null)
            {
                titleText.fontSize = titleFontSize;
                titleText.color = textColor;
            }

            // 更新音量文字样式
            if (musicVolumeText != null)
            {
                musicVolumeText.fontSize = volumeFontSize;
                musicVolumeText.color = textColor;
            }

            if (soundVolumeText != null)
            {
                soundVolumeText.fontSize = volumeFontSize;
                soundVolumeText.color = textColor;
            }
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 切换音乐开关
        /// </summary>
        public void ToggleMusic()
        {
            isMusicEnabled = !isMusicEnabled;

            if (isMusicEnabled)
            {
                if (backgroundMusicSource != null && backgroundMusicSource.clip != null)
                {
                    backgroundMusicSource.Play();
                }
            }
            else
            {
                if (backgroundMusicSource != null)
                {
                    backgroundMusicSource.Pause();
                }
            }

            UpdateUI();
            
            if (saveSettingsAutomatically)
                SaveSettings();
            
            PlayUIClick();

            if (enableDebugMode)
                Debug.Log($"[AudioController] 音乐{(isMusicEnabled ? "开启" : "关闭")}");
        }

        /// <summary>
        /// 切换音效开关
        /// </summary>
        public void ToggleSound()
        {
            isSoundEnabled = !isSoundEnabled;

            UpdateUI();
            
            if (saveSettingsAutomatically)
                SaveSettings();
            
            PlayUIClick();

            if (enableDebugMode)
                Debug.Log($"[AudioController] 音效{(isSoundEnabled ? "开启" : "关闭")}");
        }

        /// <summary>
        /// 设置音乐音量
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            currentMusicVolume = Mathf.Clamp01(volume);

            if (backgroundMusicSource != null && isMusicEnabled)
            {
                backgroundMusicSource.volume = currentMusicVolume;
            }

            UpdateVolumeText();
            
            if (saveSettingsAutomatically)
                SaveSettings();

            if (enableDebugMode)
                Debug.Log($"[AudioController] 音乐音量设置为: {(int)(currentMusicVolume * 100)}%");
        }

        /// <summary>
        /// 设置音效音量
        /// </summary>
        public void SetSoundVolume(float volume)
        {
            currentSoundVolume = Mathf.Clamp01(volume);

            if (soundEffectSource != null)
            {
                soundEffectSource.volume = currentSoundVolume;
            }

            if (uiSoundSource != null)
            {
                uiSoundSource.volume = currentSoundVolume * 0.8f;
            }

            UpdateVolumeText();
            
            if (saveSettingsAutomatically)
                SaveSettings();
            
            PlayUIClick();

            if (enableDebugMode)
                Debug.Log($"[AudioController] 音效音量设置为: {(int)(currentSoundVolume * 100)}%");
        }

        /// <summary>
        /// 显示/隐藏控制面板
        /// </summary>
        public void TogglePanel()
        {
            isPanelVisible = !isPanelVisible;
            
            if (audioPanel != null)
            {
                audioPanel.SetActive(isPanelVisible);
            }

            if (enableDebugMode)
                Debug.Log($"[AudioController] 面板{(isPanelVisible ? "显示" : "隐藏")}");
        }

        /// <summary>
        /// 显示/隐藏音频控制器
        /// </summary>
        /// <param name="show">是否显示</param>
        public void ShowAudioControl(bool show)
        {
            isPanelVisible = show;
            
            if (audioPanel != null)
            {
                audioPanel.SetActive(show);
            }

            if (enableDebugMode)
                Debug.Log($"[AudioController] 控制器{(show ? "显示" : "隐藏")}");
        }

        #endregion

        #region 音效播放

        /// <summary>
        /// 开始播放背景音乐
        /// </summary>
        private void StartBackgroundMusic()
        {
            if (backgroundMusicSource != null && isMusicEnabled)
            {
                // 尝试加载默认背景音乐
                AudioClip bgmClip = Resources.Load<AudioClip>("Audio/BGM/background_music");
                if (bgmClip != null)
                {
                    backgroundMusicSource.clip = bgmClip;
                    backgroundMusicSource.Play();
                    
                    if (enableDebugMode)
                        Debug.Log("[AudioController] 背景音乐开始播放");
                }
                else
                {
                    if (enableDebugMode)
                        Debug.LogWarning("[AudioController] 未找到背景音乐文件");
                }
            }
        }

        /// <summary>
        /// 播放UI点击音效
        /// </summary>
        public void PlayUIClick()
        {
            if (isSoundEnabled && uiSoundSource != null)
            {
                // 生成简单的点击音效
                StartCoroutine(GenerateClickSound());
            }
        }

        /// <summary>
        /// 播放按钮音效
        /// </summary>
        public void PlayButtonSound()
        {
            PlayUIClick();
        }

        /// <summary>
        /// 播放筹码音效
        /// </summary>
        public void PlayChipSound()
        {
            if (isSoundEnabled && soundEffectSource != null)
            {
                StartCoroutine(GenerateChipSound());
            }
        }

        /// <summary>
        /// 播放卡牌音效
        /// </summary>
        public void PlayCardSound()
        {
            if (isSoundEnabled && soundEffectSource != null)
            {
                StartCoroutine(GenerateCardSound());
            }
        }

        /// <summary>
        /// 生成点击音效
        /// </summary>
        private IEnumerator GenerateClickSound()
        {
            if (uiSoundSource != null)
            {
                uiSoundSource.pitch = 1.2f;
                
                // 尝试加载音效文件，如果没有则使用代码生成
                AudioClip clickClip = Resources.Load<AudioClip>("Audio/UI/click");
                if (clickClip != null)
                {
                    uiSoundSource.PlayOneShot(clickClip);
                }
                else
                {
                    // 使用Unity内置的音效
                    AudioClip defaultClip = Resources.GetBuiltinResource<AudioClip>("Audio/Beep.wav");
                    if (defaultClip != null)
                    {
                        uiSoundSource.PlayOneShot(defaultClip);
                    }
                }
                
                yield return new WaitForSeconds(0.1f);
                uiSoundSource.pitch = 1f;
            }
        }

        /// <summary>
        /// 生成筹码音效
        /// </summary>
        private IEnumerator GenerateChipSound()
        {
            if (soundEffectSource != null)
            {
                soundEffectSource.pitch = 0.8f;
                
                // 尝试加载筹码音效
                AudioClip chipClip = Resources.Load<AudioClip>("Audio/SFX/chip_sound");
                if (chipClip != null)
                {
                    soundEffectSource.PlayOneShot(chipClip);
                }
                
                yield return new WaitForSeconds(0.15f);
                soundEffectSource.pitch = 1f;
            }
        }

        /// <summary>
        /// 生成卡牌音效
        /// </summary>
        private IEnumerator GenerateCardSound()
        {
            if (soundEffectSource != null)
            {
                soundEffectSource.pitch = 1.1f;
                
                // 尝试加载卡牌音效
                AudioClip cardClip = Resources.Load<AudioClip>("Audio/SFX/card_sound");
                if (cardClip != null)
                {
                    soundEffectSource.PlayOneShot(cardClip);
                }
                
                yield return new WaitForSeconds(0.2f);
                soundEffectSource.pitch = 1f;
            }
        }

        #endregion

        #region 设置和配置

        /// <summary>
        /// 设置位置
        /// </summary>
        public void SetPosition(Vector2 position)
        {
            panelPosition = position;
            if (audioRect != null)
                audioRect.anchoredPosition = position;
        }

        /// <summary>
        /// 设置大小
        /// </summary>
        public void SetSize(Vector2 size)
        {
            panelSize = size;
            if (audioRect != null)
                audioRect.sizeDelta = size;
        }

        /// <summary>
        /// 设置字体大小
        /// </summary>
        public void SetFontSize(int titleSize, int labelSize, int volumeSize)
        {
            titleFontSize = titleSize;
            labelFontSize = labelSize;
            volumeFontSize = volumeSize;

            if (titleText != null)
                titleText.fontSize = titleSize;
            if (musicVolumeText != null)
                musicVolumeText.fontSize = volumeSize;
            if (soundVolumeText != null)
                soundVolumeText.fontSize = volumeSize;
        }

        #endregion

        #region 状态查询

        /// <summary>
        /// 获取音乐启用状态
        /// </summary>
        public bool IsMusicEnabled()
        {
            return isMusicEnabled;
        }

        /// <summary>
        /// 获取音效启用状态
        /// </summary>
        public bool IsSoundEnabled()
        {
            return isSoundEnabled;
        }

        /// <summary>
        /// 获取音乐音量
        /// </summary>
        public float GetMusicVolume()
        {
            return currentMusicVolume;
        }

        /// <summary>
        /// 获取音效音量
        /// </summary>
        public float GetSoundVolume()
        {
            return currentSoundVolume;
        }

        /// <summary>
        /// 获取面板可见状态
        /// </summary>
        public bool IsPanelVisible()
        {
            return isPanelVisible;
        }

        #endregion

        #region 设置保存和加载

        /// <summary>
        /// 加载设置
        /// </summary>
        private void LoadSettings()
        {
            isMusicEnabled = PlayerPrefs.GetInt(MUSIC_ENABLED_KEY, 1) == 1;
            isSoundEnabled = PlayerPrefs.GetInt(SOUND_ENABLED_KEY, 1) == 1;
            currentMusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, defaultMusicVolume);
            currentSoundVolume = PlayerPrefs.GetFloat(SOUND_VOLUME_KEY, defaultSoundVolume);

            if (enableDebugMode)
                Debug.Log($"[AudioController] 设置已加载 - 音乐:{isMusicEnabled}, 音效:{isSoundEnabled}, 音乐音量:{currentMusicVolume:F2}, 音效音量:{currentSoundVolume:F2}");
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        private void SaveSettings()
        {
            PlayerPrefs.SetInt(MUSIC_ENABLED_KEY, isMusicEnabled ? 1 : 0);
            PlayerPrefs.SetInt(SOUND_ENABLED_KEY, isSoundEnabled ? 1 : 0);
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, currentMusicVolume);
            PlayerPrefs.SetFloat(SOUND_VOLUME_KEY, currentSoundVolume);
            PlayerPrefs.Save();

            if (enableDebugMode)
                Debug.Log("[AudioController] 设置已保存");
        }

        #endregion

        #region 演示功能

        /// <summary>
        /// 演示协程
        /// </summary>
        private IEnumerator DemoCoroutine()
        {
            yield return new WaitForSeconds(2f);

            // 演示音效播放
            PlayButtonSound();
            yield return new WaitForSeconds(1f);

            PlayChipSound();
            yield return new WaitForSeconds(1f);

            PlayCardSound();
            yield return new WaitForSeconds(2f);

            // 演示音量调节
            for (float volume = 0.6f; volume <= 1f; volume += 0.1f)
            {
                SetMusicVolume(volume);
                yield return new WaitForSeconds(0.5f);
            }

            yield return new WaitForSeconds(1f);

            // 演示开关切换
            ToggleMusic();
            yield return new WaitForSeconds(1f);
            ToggleMusic();
            yield return new WaitForSeconds(1f);

            ToggleSound();
            yield return new WaitForSeconds(1f);
            ToggleSound();

            yield return new WaitForSeconds(demoInterval);

            // 重复演示
            if (enableAutoDemo)
            {
                StartCoroutine(DemoCoroutine());
            }
        }

        #endregion

        #region 调试方法

        /// <summary>
        /// 强制显示音频控制器
        /// </summary>
        [ContextMenu("强制显示音频控制器")]
        public void ForceShowAudioControl()
        {
            audioUICreated = false;
            CreateAndShowAudioControl();
        }

        /// <summary>
        /// 显示组件状态
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowComponentStatus()
        {
            Debug.Log("=== AudioController 组件状态 ===");
            Debug.Log($"自动创建并显示: {autoCreateAndShow}");
            Debug.Log($"启动时显示: {showOnAwake}");
            Debug.Log($"立即显示: {immediateDisplay}");
            Debug.Log($"音频UI已创建: {audioUICreated}");
            Debug.Log($"父Canvas: {(parentCanvas != null ? "✓" : "✗")}");
            Debug.Log($"音频面板: {(audioPanel != null ? "✓" : "✗")} - {(audioPanel?.activeInHierarchy == true ? "显示" : "隐藏")}");
            Debug.Log($"控制面板: {(controlPanel != null ? "✓" : "✗")}");
            Debug.Log($"音乐按钮: {(musicToggleButton != null ? "✓" : "✗")}");
            Debug.Log($"音效按钮: {(soundToggleButton != null ? "✓" : "✗")}");
            Debug.Log($"音乐滑条: {(musicVolumeSlider != null ? "✓" : "✗")}");
            Debug.Log($"音效滑条: {(soundVolumeSlider != null ? "✓" : "✗")}");
            Debug.Log($"背景音乐源: {(backgroundMusicSource != null ? "✓" : "✗")}");
            Debug.Log($"音效源: {(soundEffectSource != null ? "✓" : "✗")}");
            Debug.Log($"UI音效源: {(uiSoundSource != null ? "✓" : "✗")}");
            Debug.Log($"音乐状态: {(isMusicEnabled ? "开启" : "关闭")} - 音量: {currentMusicVolume:F2}");
            Debug.Log($"音效状态: {(isSoundEnabled ? "开启" : "关闭")} - 音量: {currentSoundVolume:F2}");
        }

        /// <summary>
        /// 测试音乐切换
        /// </summary>
        [ContextMenu("测试音乐切换")]
        public void TestMusicToggle()
        {
            ToggleMusic();
            Debug.Log($"[AudioController] 测试音乐切换 - 当前状态: {(isMusicEnabled ? "开启" : "关闭")}");
        }

        /// <summary>
        /// 测试音效切换
        /// </summary>
        [ContextMenu("测试音效切换")]
        public void TestSoundToggle()
        {
            ToggleSound();
            Debug.Log($"[AudioController] 测试音效切换 - 当前状态: {(isSoundEnabled ? "开启" : "关闭")}");
        }

        /// <summary>
        /// 测试音量调节
        /// </summary>
        [ContextMenu("测试音量调节")]
        public void TestVolumeAdjustment()
        {
            float randomMusicVolume = UnityEngine.Random.Range(0f, 1f);
            float randomSoundVolume = UnityEngine.Random.Range(0f, 1f);
            
            SetMusicVolume(randomMusicVolume);
            SetSoundVolume(randomSoundVolume);
            
            Debug.Log($"[AudioController] 测试音量调节 - 音乐: {randomMusicVolume:F2}, 音效: {randomSoundVolume:F2}");
        }

        /// <summary>
        /// 测试音效播放
        /// </summary>
        [ContextMenu("测试音效播放")]
        public void TestSoundEffects()
        {
            StartCoroutine(TestSoundEffectsCoroutine());
        }

        /// <summary>
        /// 测试音效播放协程
        /// </summary>
        private IEnumerator TestSoundEffectsCoroutine()
        {
            Debug.Log("[AudioController] 开始测试音效播放");
            
            PlayUIClick();
            yield return new WaitForSeconds(0.5f);
            
            PlayChipSound();
            yield return new WaitForSeconds(0.5f);
            
            PlayCardSound();
            yield return new WaitForSeconds(0.5f);
            
            Debug.Log("[AudioController] 音效播放测试完成");
        }

        #endregion
    }
}
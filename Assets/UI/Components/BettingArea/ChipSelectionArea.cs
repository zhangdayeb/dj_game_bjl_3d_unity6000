// Assets/UI/Components/BettingArea/ChipSelectionArea.cs
// 筹码选择区域组件 - 完整自创建版本
// 功能：自动创建UI、持久显示、防重复、逻辑绑定
// 修改时间: 2025/6/27

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BaccaratGame.UI.Components
{
    /// <summary>
    /// 筹码选择区域组件 - 完整自创建版本
    /// 从空GameObject开始创建完整的筹码选择UI
    /// </summary>
    public class ChipSelectionArea : MonoBehaviour
    {
        #region 序列化字段

        [Header("🔥 自动显示设置")]
        [Tooltip("是否自动创建并显示UI")]
        public bool autoCreateAndShow = true;
        [Tooltip("启动时显示")]
        public bool showOnAwake = true;
        [Tooltip("立即显示")]
        public bool immediateDisplay = true;
        [Tooltip("是否持久显示")]
        public bool persistentDisplay = true;

        [Header("📐 布局设置")]
        [Tooltip("筹码栏高度")]
        public float chipBarHeight = 100f;
        [Tooltip("筹码按钮大小")]
        public Vector2 chipSize = new Vector2(80f, 80f);
        [Tooltip("侧边按钮大小（更多、续压）")]
        public Vector2 sideButtonSize = new Vector2(60f, 80f);
        [Tooltip("按钮间距")]
        public float spacing = 10f;
        [Tooltip("左边距")]
        public int paddingLeft = 20;
        [Tooltip("右边距")]
        public int paddingRight = 20;
        [Tooltip("上边距")]
        public int paddingTop = 10;
        [Tooltip("下边距")]
        public int paddingBottom = 10;

        [Header("📍 按钮布局")]
        [Tooltip("按钮位置")]
        public Vector2 buttonPosition = Vector2.zero;
        [Tooltip("自动居中")]
        public bool autoCenter = true;

        [Header("🎨 UI样式")]
        [Tooltip("普通状态颜色")]
        public Color normalColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        [Tooltip("高亮状态颜色")]
        public Color highlightColor = new Color(1f, 1f, 1f, 1f);
        [Tooltip("按下状态颜色")]
        public Color pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        [Tooltip("选中状态颜色")]
        public Color selectedColor = new Color(1f, 0.8f, 0.2f, 1f);
        [Tooltip("禁用状态颜色")]
        public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        [Tooltip("背景颜色")]
        public Color backgroundColor = new Color(0f, 0f, 0f, 0.8f);
        [Tooltip("按钮文字颜色")]
        public Color textColor = Color.white;
        [Tooltip("数字颜色")]
        public Color numberColor = Color.yellow;

        [Header("📝 字体设置")]
        [Tooltip("文字大小")]
        public int fontSize = 16;
        [Tooltip("数字文字大小")]
        public int numberFontSize = 18;
        [Tooltip("字体样式")]
        public FontStyle fontStyle = FontStyle.Bold;

        [Header("🎬 动画设置")]
        [Tooltip("启用选择动画")]
        public bool enableSelectionAnimation = true;
        [Tooltip("选中时的缩放比例")]
        public float selectedScale = 1.15f;
        [Tooltip("缩放动画时间")]
        public float animationDuration = 0.2f;
        [Tooltip("动画曲线")]
        public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("💰 筹码配置")]
        [Tooltip("5个默认筹码数值")]
        public int[] chipValues = { 2, 10, 20, 50, 100 };
        [Tooltip("对应的图片文件名（不含扩展名）")]
        public string[] chipImageNames = { "B_01", "B_10", "B_20", "B_50", "B_100" };

        [Header("🔗 资源路径")]
        [Tooltip("筹码图片资源路径")]
        public string chipImagePath = "Images/chips/";
        [Tooltip("手动指定筹码图片数组（可选，优先级最高）")]
        public Sprite[] manualChipSprites;

        [Header("🎵 音效设置")]
        [Tooltip("启用按钮音效")]
        public bool enableButtonSound = true;
        [Tooltip("点击音效")]
        public AudioClip clickSound;
        [Tooltip("选择音效")]
        public AudioClip selectSound;

        [Header("🔧 高级设置")]
        [Tooltip("按钮可交互")]
        public bool buttonsInteractable = true;
        [Tooltip("显示按钮边框")]
        public bool showButtonBorder = false;
        [Tooltip("边框颜色")]
        public Color borderColor = Color.white;
        [Tooltip("边框宽度")]
        public float borderWidth = 2f;

        [Header("🐛 调试")]
        public bool enableDebugMode = true;

        #endregion

        #region 私有字段

        // 单例防重复
        private static ChipSelectionArea instance;
        private static bool isQuitting = false;

        // 状态标志
        private bool isInitialized = false;
        private bool isUICreated = false;
        private bool isCreatingUI = false;

        // UI组件引用
        private RectTransform rectTransform;
        private HorizontalLayoutGroup layoutGroup;
        private Image backgroundImage;

        // 按钮引用 - 按规划顺序
        private Button moreButton;        // 左侧：更多按钮
        private Button[] chipButtons;     // 中间：5个筹码按钮
        private Button rebetButton;       // 右侧：续压按钮

        // 选择状态
        private int selectedChipIndex = -1;

        // 预定义颜色
        private readonly Color[] chipColors = {
            new Color(0.2f, 0.6f, 1f, 1f),    // 蓝色 - 2
            new Color(0f, 0.8f, 0.4f, 1f),    // 绿色 - 10
            new Color(0f, 0.8f, 0.8f, 1f),    // 青色 - 20
            new Color(0.3f, 0.5f, 1f, 1f),    // 蓝色 - 50
            new Color(0.6f, 0.6f, 0.6f, 1f)   // 灰色 - 100
        };

        #endregion

        #region 事件定义

        [System.Serializable]
        public class ChipEvent : UnityEngine.Events.UnityEvent<int> { }

        [Header("📡 事件")]
        public ChipEvent OnChipSelected;
        public UnityEngine.Events.UnityEvent OnMoreButtonClicked;
        public UnityEngine.Events.UnityEvent OnRebetButtonClicked;

        #endregion

        #region 单例模式

        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static ChipSelectionArea Instance
        {
            get
            {
                if (isQuitting) return null;
                
                if (instance == null)
                {
                    instance = FindObjectOfType<ChipSelectionArea>();
                    if (instance == null)
                    {
                        LogStaticDebug("未找到现有实例，需要手动创建ChipSelectionArea GameObject");
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// 确保单例唯一性
        /// </summary>
        private void EnsureSingleton()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                LogDebug("设置为单例实例并标记为DontDestroyOnLoad");
            }
            else if (instance != this)
            {
                LogDebug("发现重复实例，销毁当前对象");
                Destroy(gameObject);
            }
        }

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            LogDebug("Awake - 开始初始化");
            
            // 确保单例
            EnsureSingleton();
            if (instance != this) return;

            // 基础初始化
            InitializeComponent();
            
            LogDebug("Awake完成");
        }

        private void Start()
        {
            if (instance != this) return;
            
            LogDebug("Start - 开始");

            // 自动创建UI
            if (autoCreateAndShow)
            {
                CreateChipSelectionUI();
            }

            LogDebug("Start完成");
        }

        private void OnApplicationQuit()
        {
            isQuitting = true;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
            LogDebug("OnDestroy");
        }

        #endregion

        #region 🔧 初始化

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponent()
        {
            if (isInitialized) return;

            try
            {
                // 获取或添加RectTransform
                rectTransform = GetComponent<RectTransform>();
                if (rectTransform == null)
                {
                    rectTransform = gameObject.AddComponent<RectTransform>();
                }

                // 设置为屏幕底部锚点 - 宽度100%，高度固定
                rectTransform.anchorMin = new Vector2(0f, 0f);
                rectTransform.anchorMax = new Vector2(1f, 0f);
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = new Vector2(0f, chipBarHeight); // 宽度0表示使用锚点拉伸，高度使用chipBarHeight

                isInitialized = true;
                LogDebug("组件初始化完成");
            }
            catch (Exception ex)
            {
                LogError($"初始化失败: {ex.Message}");
            }
        }

        #endregion

        #region 🎨 UI创建

        /// <summary>
        /// 创建筹码选择UI
        /// </summary>
        public void CreateChipSelectionUI()
        {
            if (isUICreated || isCreatingUI) 
            {
                LogDebug("UI已创建或正在创建，跳过");
                return;
            }

            isCreatingUI = true;

            try
            {
                LogDebug("开始创建筹码选择UI");

                // 第1步：创建背景
                CreateBackground();

                // 第2步：设置水平布局
                SetupHorizontalLayout();

                // 第3步：按顺序创建按钮
                CreateButtonsInOrder();

                isUICreated = true;
                LogDebug("UI创建完成");

                // 确保持久显示
                if (persistentDisplay)
                {
                    EnsurePersistentDisplay();
                }
            }
            catch (Exception ex)
            {
                LogError($"创建UI失败: {ex.Message}");
                isUICreated = false;
            }
            finally
            {
                isCreatingUI = false;
            }
        }

        /// <summary>
        /// 创建背景
        /// </summary>
        private void CreateBackground()
        {
            backgroundImage = gameObject.GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
            }

            backgroundImage.color = backgroundColor;
            backgroundImage.sprite = CreateSolidSprite(Color.white);

            LogDebug("背景创建完成");
        }

        /// <summary>
        /// 设置水平布局
        /// </summary>
        private void SetupHorizontalLayout()
        {
            layoutGroup = gameObject.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            // 配置布局参数
            layoutGroup.spacing = spacing;
            layoutGroup.padding = new RectOffset(paddingLeft, paddingRight, paddingTop, paddingBottom);
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            LogDebug("水平布局设置完成");
        }

        /// <summary>
        /// 按顺序创建所有按钮
        /// </summary>
        private void CreateButtonsInOrder()
        {
            LogDebug("开始按顺序创建按钮...");

            // 1. 创建左侧"更多"按钮
            CreateMoreButton();

            // 2. 创建中间5个筹码按钮
            CreateChipButtons();

            // 3. 创建右侧"续压"按钮
            CreateRebetButton();

            LogDebug("所有按钮创建完成");
        }

        /// <summary>
        /// 创建更多按钮
        /// </summary>
        private void CreateMoreButton()
        {
            GameObject moreObj = CreateButton("MoreChipPanel", sideButtonSize);
            moreButton = moreObj.GetComponent<Button>();

            // 设置按钮样式
            SetButtonStyle(moreButton, new Color(0.3f, 0.3f, 0.3f, 1f));

            // 创建文字
            CreateButtonText(moreObj, "...", fontSize);

            // 设置事件
            moreButton.onClick.AddListener(() => {
                LogDebug("更多按钮被点击");
                PlaySound(clickSound); // 播放点击音效
                OnMoreButtonClicked?.Invoke();
            });

            LogDebug("更多按钮创建完成");
        }

        /// <summary>
        /// 创建筹码按钮
        /// </summary>
        private void CreateChipButtons()
        {
            chipButtons = new Button[chipValues.Length];

            for (int i = 0; i < chipValues.Length; i++)
            {
                int value = chipValues[i];
                
                // 优先使用手动配置的图片名称，否则自动生成
                string imageName = "";
                if (i < chipImageNames.Length && !string.IsNullOrEmpty(chipImageNames[i]))
                {
                    imageName = chipImageNames[i];
                }
                else
                {
                    imageName = GenerateChipImageName(value);
                }

                GameObject chipObj = CreateButton($"Chip{i + 1}", chipSize);
                chipButtons[i] = chipObj.GetComponent<Button>();

                // 设置按钮样式和颜色
                Color chipColor = i < chipColors.Length ? chipColors[i] : Color.gray;
                SetButtonStyle(chipButtons[i], chipColor);

                // 尝试加载图片，失败则创建文字
                if (!LoadChipImage(chipObj, imageName))
                {
                    CreateButtonText(chipObj, value.ToString(), fontSize);
                    LogDebug($"筹码 {value} 使用文字显示（图片加载失败）");
                }
                else
                {
                    LogDebug($"筹码 {value} 使用图片显示：{imageName}");
                }

                // 设置事件（使用闭包变量）
                int chipIndex = i;
                chipButtons[i].onClick.AddListener(() => {
                    SelectChip(chipIndex);
                    PlaySound(selectSound); // 播放选择音效
                });

                LogDebug($"筹码按钮 {value} 创建完成");
            }

            LogDebug($"所有筹码按钮创建完成，共{chipButtons.Length}个");
        }

        /// <summary>
        /// 创建续压按钮
        /// </summary>
        private void CreateRebetButton()
        {
            GameObject rebetObj = CreateButton("xuyaChip", sideButtonSize);
            rebetButton = rebetObj.GetComponent<Button>();

            // 设置按钮样式
            SetButtonStyle(rebetButton, new Color(0.3f, 0.3f, 0.3f, 1f));

            // 创建文字
            CreateButtonText(rebetObj, "续压", fontSize);

            // 设置事件
            rebetButton.onClick.AddListener(() => {
                LogDebug("续压按钮被点击");
                OnRebetButtonClicked?.Invoke();
            });

            LogDebug("续压按钮创建完成");
        }

        #endregion

        #region 🔧 UI工具方法

        /// <summary>
        /// 创建按钮GameObject
        /// </summary>
        private GameObject CreateButton(string name, Vector2 size)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(transform);

            // 设置RectTransform
            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;

            // 添加Image组件（按钮需要）
            Image image = buttonObj.AddComponent<Image>();
            image.sprite = CreateSolidSprite(Color.white);

            // 添加Button组件
            Button button = buttonObj.AddComponent<Button>();

            return buttonObj;
        }

        /// <summary>
        /// 设置按钮样式
        /// </summary>
        private void SetButtonStyle(Button button, Color normalColor)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = highlightColor;
            colors.pressedColor = pressedColor;
            colors.selectedColor = selectedColor;
            colors.disabledColor = disabledColor;
            button.colors = colors;
            
            // 设置交互性
            button.interactable = buttonsInteractable;
            
            // 添加边框（如果启用）
            if (showButtonBorder)
            {
                AddButtonBorder(button.gameObject);
            }
        }

        /// <summary>
        /// 创建按钮文字
        /// </summary>
        private Text CreateButtonText(GameObject buttonObj, string text, int textSize)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);

            // 设置文字RectTransform
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            textRect.localScale = Vector3.one;

            // 设置Text组件
            Text textComp = textObj.AddComponent<Text>();
            textComp.text = text;
            
            // 获取默认字体
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
            {
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            textComp.font = defaultFont;
            
            textComp.fontSize = textSize;
            textComp.color = textColor;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.fontStyle = fontStyle;

            return textComp;
        }

        /// <summary>
        /// 添加按钮边框
        /// </summary>
        private void AddButtonBorder(GameObject buttonObj)
        {
            GameObject borderObj = new GameObject("Border");
            borderObj.transform.SetParent(buttonObj.transform);
            borderObj.transform.SetAsFirstSibling(); // 放在最底层

            RectTransform borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.sizeDelta = Vector2.zero;
            borderRect.anchoredPosition = Vector2.zero;
            borderRect.localScale = Vector3.one;

            Image borderImage = borderObj.AddComponent<Image>();
            borderImage.color = borderColor;
            borderImage.sprite = CreateSolidSprite(Color.white);
            
            // 创建边框效果（通过调整sizeDelta）
            borderRect.sizeDelta = new Vector2(borderWidth * 2, borderWidth * 2);
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (!enableButtonSound || clip == null) return;
            
            // 简单的音效播放，可以根据需要使用AudioSource
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        }

        /// <summary>
        /// 生成筹码图片文件名
        /// </summary>
        private string GenerateChipImageName(int chipValue)
        {
            // 根据筹码数值生成对应的图片文件名
            // 规则：B_01.png, B_10.png, B_20.png, B_50.png, B_100.png
            switch (chipValue)
            {
                case 1: return "B_01";
                case 2: return "B_01";  // 使用B_01作为2的图片
                case 5: return "B_05";
                case 10: return "B_10";
                case 20: return "B_20";
                case 50: return "B_50";
                case 100: return "B_100";
                case 200: return "B_200";
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
        private bool LoadChipImage(GameObject buttonObj, string imageName)
        {
            if (string.IsNullOrEmpty(imageName)) return false;

            try
            {
                string fullPath = chipImagePath + imageName;
                Sprite sprite = Resources.Load<Sprite>(fullPath);
                
                if (sprite != null)
                {
                    Image image = buttonObj.GetComponent<Image>();
                    if (image != null)
                    {
                        image.sprite = sprite;
                        LogDebug($"成功加载筹码图片: {fullPath}");
                        return true;
                    }
                }
                else
                {
                    LogDebug($"未找到筹码图片: {fullPath}");
                }
            }
            catch (Exception ex)
            {
                LogDebug($"加载筹码图片失败: {ex.Message}");
            }

            return false;
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

        #region 🎯 筹码选择逻辑

        /// <summary>
        /// 选择筹码
        /// </summary>
        private void SelectChip(int chipIndex)
        {
            if (chipButtons == null || chipIndex < 0 || chipIndex >= chipButtons.Length) return;

            // 取消之前选择
            if (selectedChipIndex >= 0 && selectedChipIndex < chipButtons.Length)
            {
                SetChipSelected(selectedChipIndex, false);
            }

            // 设置新选择
            selectedChipIndex = chipIndex;
            SetChipSelected(chipIndex, true);

            int chipValue = chipIndex < chipValues.Length ? chipValues[chipIndex] : 0;
            LogDebug($"选择筹码: {chipValue}");

            // 触发事件
            OnChipSelected?.Invoke(chipValue);
        }

        /// <summary>
        /// 设置筹码选中状态
        /// </summary>
        private void SetChipSelected(int chipIndex, bool selected)
        {
            if (chipButtons == null || chipIndex < 0 || chipIndex >= chipButtons.Length) return;
            if (chipButtons[chipIndex] == null) return;

            Transform buttonTransform = chipButtons[chipIndex].transform;
            Vector3 targetScale = selected ? Vector3.one * selectedScale : Vector3.one;
            
            if (Application.isPlaying)
            {
                StartCoroutine(ScaleAnimation(buttonTransform, targetScale, animationDuration));
            }
            else
            {
                buttonTransform.localScale = targetScale;
            }
        }

        /// <summary>
        /// 缩放动画
        /// </summary>
        private IEnumerator ScaleAnimation(Transform target, Vector3 targetScale, float duration)
        {
            if (target == null) yield break;

            Vector3 startScale = target.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                target.localScale = Vector3.Lerp(startScale, targetScale, progress);
                yield return null;
            }

            target.localScale = targetScale;
        }

        #endregion

        #region 🔥 持久显示

        /// <summary>
        /// 确保持久显示
        /// </summary>
        private void EnsurePersistentDisplay()
        {
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }

            if (!enabled)
            {
                enabled = true;
            }

            LogDebug("持久显示已确保");
        }

        #endregion

        #region 🔧 公共方法

        /// <summary>
        /// 手动创建UI
        /// </summary>
        [ContextMenu("🎨 创建UI")]
        public void ManualCreateUI()
        {
            CreateChipSelectionUI();
        }

        /// <summary>
        /// 清除UI
        /// </summary>
        [ContextMenu("🗑️ 清除UI")]
        public void ClearUI()
        {
            if (!isUICreated) return;

            // 销毁所有子对象
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }

            // 重置状态
            isUICreated = false;
            isCreatingUI = false;
            selectedChipIndex = -1;
            moreButton = null;
            chipButtons = null;
            rebetButton = null;

            LogDebug("UI已清除");
        }

        /// <summary>
        /// 重新创建UI
        /// </summary>
        [ContextMenu("🔄 重新创建UI")]
        public void RecreateUI()
        {
            ClearUI();
            CreateChipSelectionUI();
        }

        /// <summary>
        /// 获取选中筹码值
        /// </summary>
        public int GetSelectedChipValue()
        {
            if (selectedChipIndex >= 0 && selectedChipIndex < chipValues.Length)
            {
                return chipValues[selectedChipIndex];
            }
            return -1;
        }

        /// <summary>
        /// 通过值选择筹码
        /// </summary>
        public void SelectChipByValue(int value)
        {
            for (int i = 0; i < chipValues.Length; i++)
            {
                if (chipValues[i] == value)
                {
                    SelectChip(i);
                    return;
                }
            }
            LogDebug($"未找到值为 {value} 的筹码");
        }

        /// <summary>
        /// 清除选择
        /// </summary>
        public void ClearSelection()
        {
            if (selectedChipIndex >= 0 && selectedChipIndex < chipButtons.Length)
            {
                SetChipSelected(selectedChipIndex, false);
            }
            selectedChipIndex = -1;
            LogDebug("已清除筹码选择");
        }

        /// <summary>
        /// 设置筹码数值配置
        /// </summary>
        public void SetChipValues(int[] newValues)
        {
            if (newValues != null && newValues.Length > 0)
            {
                chipValues = newValues;
                LogDebug($"筹码数值已更新: [{string.Join(", ", chipValues)}]");
                
                // 如果UI已创建，重新创建以应用新配置
                if (isUICreated)
                {
                    RecreateUI();
                }
            }
        }

        #endregion

        #region 🐛 调试方法

        /// <summary>
        /// 调试日志
        /// </summary>
        private void LogDebug(string message)
        {
            if (enableDebugMode)
            {
                Debug.Log($"[ChipSelectionArea] {message}");
            }
        }

        /// <summary>
        /// 静态调试日志
        /// </summary>
        private static void LogStaticDebug(string message)
        {
            Debug.Log($"[ChipSelectionArea-Static] {message}");
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        private void LogError(string message)
        {
            Debug.LogError($"[ChipSelectionArea] ❌ {message}");
        }

        /// <summary>
        /// 显示状态
        /// </summary>
        [ContextMenu("📊 显示状态")]
        public void ShowStatus()
        {
            Debug.Log("=== ChipSelectionArea 状态 ===");
            Debug.Log($"🔧 已初始化: {isInitialized}");
            Debug.Log($"🎨 UI已创建: {isUICreated}");
            Debug.Log($"🔥 自动创建: {autoCreateAndShow}");
            Debug.Log($"💎 持久显示: {persistentDisplay}");
            Debug.Log($"🎯 选中筹码: {GetSelectedChipValue()}");
            Debug.Log($"📋 筹码配置: [{string.Join(", ", chipValues)}]");
            Debug.Log($"🏠 单例实例: {(instance == this ? "是" : "否")}");
            
            // 检查按钮状态
            Debug.Log($"更多按钮: {(moreButton != null ? "✓" : "✗")}");
            Debug.Log($"筹码按钮: {(chipButtons != null ? chipButtons.Length : 0)}个");
            Debug.Log($"续压按钮: {(rebetButton != null ? "✓" : "✗")}");
        }

        /// <summary>
        /// 测试所有按钮功能
        /// </summary>
        [ContextMenu("🧪 测试按钮功能")]
        public void TestButtonFunctions()
        {
            LogDebug("开始测试按钮功能...");

            // 测试更多按钮
            if (moreButton != null)
            {
                LogDebug("测试更多按钮点击");
                moreButton.onClick.Invoke();
            }

            // 测试筹码按钮
            if (chipButtons != null)
            {
                for (int i = 0; i < chipButtons.Length; i++)
                {
                    if (chipButtons[i] != null)
                    {
                        LogDebug($"测试筹码按钮{i+1}点击");
                        chipButtons[i].onClick.Invoke();
                        break; // 只测试第一个
                    }
                }
            }

            // 测试续压按钮
            if (rebetButton != null)
            {
                LogDebug("测试续压按钮点击");
                rebetButton.onClick.Invoke();
            }

            LogDebug("按钮功能测试完成");
        }

        #endregion
    }
}
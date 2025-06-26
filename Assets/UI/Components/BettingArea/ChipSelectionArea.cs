// Assets/UI/Components/BettingArea/ChipSelectionArea.cs
// 筹码选择区域组件 - 正确布局实现
// 布局：[更多...] [2] [10] [20] [40] [100] [续压]
// 创建时间: 2025/6/26

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BaccaratGame.UI.Components
{
    /// <summary>
    /// 筹码选择区域组件 - 正确布局版本
    /// 严格按照规划实现：左侧更多按钮 + 中间5个筹码 + 右侧续压按钮
    /// </summary>
    public class ChipSelectionArea : MonoBehaviour
    {
        #region 序列化字段

        [Header("🔥 显示控制")]
        [Tooltip("是否自动创建并显示UI")]
        public bool autoCreateAndShow = true;
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

        [Header("🎨 外观设置")]
        [Tooltip("背景颜色")]
        public Color backgroundColor = new Color(0f, 0f, 0f, 0.8f);
        [Tooltip("按钮文字颜色")]
        public Color textColor = Color.white;
        [Tooltip("文字大小")]
        public int fontSize = 16;

        [Header("💰 筹码配置")]
        [Tooltip("5个默认筹码数值")]
        public int[] chipValues = { 2, 10, 20, 50, 100 };
        [Tooltip("对应的图片文件名（不含扩展名）")]
        public string[] chipImageNames = { "B_01", "B_10", "B_20", "B_50", "B_100" };

        [Header("🔗 资源路径")]
        [Tooltip("筹码图片资源路径")]
        public string chipImagePath = "Images/chips/";

        [Header("🐛 调试")]
        public bool enableDebugMode = true;

        #endregion

        #region 私有字段

        // 防重复创建
        private static HashSet<ChipSelectionArea> instances = new HashSet<ChipSelectionArea>();
        private int instanceId;
        private static int idCounter = 0;

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

        #endregion

        #region 事件定义

        [System.Serializable]
        public class ChipEvent : UnityEngine.Events.UnityEvent<int> { }

        [Header("📡 事件")]
        public ChipEvent OnChipSelected;
        public UnityEngine.Events.UnityEvent OnMoreButtonClicked;
        public UnityEngine.Events.UnityEvent OnRebetButtonClicked;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            instanceId = ++idCounter;
            instances.Add(this);

            LogDebug($"Awake - 实例{instanceId}开始初始化");

            // 检查重复实例
            if (CheckForDuplicates())
            {
                LogDebug($"发现重复实例，销毁实例{instanceId}");
                return;
            }

            // 基础初始化
            InitializeComponent();

            LogDebug($"Awake完成 - 实例{instanceId}");
        }

        private void Start()
        {
            if (!IsValidInstance()) return;

            LogDebug($"Start - 实例{instanceId}");

            // 自动创建UI
            if (autoCreateAndShow)
            {
                CreateChipSelectionUI();
            }

            LogDebug($"Start完成 - 实例{instanceId}");
        }

        private void OnDestroy()
        {
            LogDebug($"OnDestroy - 实例{instanceId}");
            instances.Remove(this);
        }

        #endregion

        #region 🔧 初始化和重复检查

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

                // 设置为屏幕底部锚点
                rectTransform.anchorMin = new Vector2(0f, 0f);
                rectTransform.anchorMax = new Vector2(1f, 0f);
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = new Vector2(0f, chipBarHeight);

                isInitialized = true;
                LogDebug($"组件初始化完成 - 实例{instanceId}");
            }
            catch (Exception ex)
            {
                LogError($"初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查重复实例
        /// </summary>
        private bool CheckForDuplicates()
        {
            foreach (var instance in instances)
            {
                if (instance != null && instance != this && 
                    instance.gameObject.name == this.gameObject.name &&
                    instance.isUICreated)
                {
                    // 销毁当前重复实例
                    if (Application.isPlaying)
                        Destroy(this.gameObject);
                    else
                        DestroyImmediate(this.gameObject);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查实例有效性
        /// </summary>
        private bool IsValidInstance()
        {
            return this != null && gameObject != null && instances.Contains(this);
        }

        #endregion

        #region 🎨 UI创建 - 按规划实现

        /// <summary>
        /// 创建筹码选择UI - 严格按照规划
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
                LogDebug($"开始创建筹码选择UI - 实例{instanceId}");

                // 第1步：创建背景
                CreateBackground();

                // 第2步：设置水平布局
                SetupHorizontalLayout();

                // 第3步：按顺序创建按钮
                CreateButtonsInOrder();

                isUICreated = true;
                LogDebug($"UI创建完成 - 实例{instanceId}");

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

            // 🔥 修复：运行时创建RectOffset，避免构造函数错误
            RectOffset layoutPadding = new RectOffset(paddingLeft, paddingRight, paddingTop, paddingBottom);

            // 配置布局参数
            layoutGroup.spacing = spacing;
            layoutGroup.padding = layoutPadding;
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
        /// 创建左侧更多按钮
        /// </summary>
        private void CreateMoreButton()
        {
            GameObject buttonObj = CreateButtonObject("MoreButton");
            moreButton = SetupButtonComponent(buttonObj, sideButtonSize);

            // 设置更多按钮样式
            SetButtonStyle(moreButton, new Color(0.3f, 0.3f, 0.3f, 1f));
            
            // 添加文字
            CreateButtonText(buttonObj, "...", fontSize + 2);

            // 添加点击事件
            moreButton.onClick.AddListener(() => {
                LogDebug("更多按钮被点击");
                OnMoreButtonClicked?.Invoke();
            });

            LogDebug("更多按钮创建完成");
        }

        /// <summary>
        /// 创建中间5个筹码按钮
        /// </summary>
        private void CreateChipButtons()
        {
            chipButtons = new Button[chipValues.Length];

            for (int i = 0; i < chipValues.Length; i++)
            {
                int value = chipValues[i];
                string imageName = i < chipImageNames.Length ? chipImageNames[i] : "B_01";

                // 创建筹码按钮
                GameObject buttonObj = CreateButtonObject($"ChipButton_{value}");
                Button button = SetupButtonComponent(buttonObj, chipSize);

                // 🔥 先设置默认样式和文字，确保按钮可见
                SetButtonStyle(button, GetChipColor(i));
                CreateButtonText(buttonObj, value.ToString(), fontSize);

                // 尝试加载筹码图片（如果成功则覆盖默认样式）
                bool imageLoaded = LoadChipImage(buttonObj, imageName);
                
                if (imageLoaded)
                {
                    // 图片加载成功，隐藏文字或调整透明度
                    Text buttonText = buttonObj.GetComponentInChildren<Text>();
                    if (buttonText != null)
                    {
                        buttonText.color = new Color(buttonText.color.r, buttonText.color.g, buttonText.color.b, 0.8f);
                    }
                }

                // 添加点击事件
                int chipIndex = i; // 闭包变量
                button.onClick.AddListener(() => {
                    SelectChip(chipIndex);
                });

                chipButtons[i] = button;
                LogDebug($"筹码按钮 {value} 创建完成 (图片加载: {(imageLoaded ? "成功" : "失败")})");
            }

            LogDebug($"所有筹码按钮创建完成，共{chipButtons.Length}个");
        }

        /// <summary>
        /// 创建右侧续压按钮
        /// </summary>
        private void CreateRebetButton()
        {
            GameObject buttonObj = CreateButtonObject("RebetButton");
            rebetButton = SetupButtonComponent(buttonObj, sideButtonSize);

            // 设置续压按钮样式
            SetButtonStyle(rebetButton, new Color(1f, 0.8f, 0f, 1f)); // 金色
            
            // 添加文字
            CreateButtonText(buttonObj, "续压", fontSize);

            // 添加点击事件
            rebetButton.onClick.AddListener(() => {
                LogDebug("续压按钮被点击");
                OnRebetButtonClicked?.Invoke();
            });

            LogDebug("续压按钮创建完成");
        }

        #endregion

        #region 🔨 按钮创建辅助方法

        /// <summary>
        /// 创建按钮GameObject
        /// </summary>
        private GameObject CreateButtonObject(string name)
        {
            string uniqueName = $"{name}_{instanceId}";
            
            GameObject buttonObj = new GameObject(uniqueName);
            buttonObj.transform.SetParent(transform);
            
            RectTransform rectTrans = buttonObj.AddComponent<RectTransform>();
            rectTrans.localScale = Vector3.one;
            
            return buttonObj;
        }

        /// <summary>
        /// 设置按钮组件
        /// </summary>
        private Button SetupButtonComponent(GameObject buttonObj, Vector2 size)
        {
            // 设置大小
            RectTransform rectTrans = buttonObj.GetComponent<RectTransform>();
            rectTrans.sizeDelta = size;

            // 添加Image组件
            Image image = buttonObj.AddComponent<Image>();
            image.color = Color.white;

            // 添加Button组件
            Button button = buttonObj.AddComponent<Button>();
            
            return button;
        }

        /// <summary>
        /// 设置按钮样式
        /// </summary>
        private void SetButtonStyle(Button button, Color normalColor)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = Color.Lerp(normalColor, Color.white, 0.2f);
            colors.pressedColor = Color.Lerp(normalColor, Color.gray, 0.3f);
            colors.selectedColor = Color.Lerp(normalColor, Color.yellow, 0.4f);
            button.colors = colors;
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
            
            // 🔥 修复：使用LegacyRuntime.ttf替代Arial.ttf
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
            {
                // 如果LegacyRuntime也没有，尝试使用Arial
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            textComp.font = defaultFont;
            
            textComp.fontSize = textSize;
            textComp.color = textColor;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.fontStyle = FontStyle.Bold;

            return textComp;
        }

        /// <summary>
        /// 加载筹码图片
        /// </summary>
        private bool LoadChipImage(GameObject buttonObj, string imageName)
        {
            try
            {
                LogDebug($"尝试加载图片: {imageName}");
                
                string fullPath = chipImagePath + imageName;
                Sprite sprite = Resources.Load<Sprite>(fullPath);
                
                if (sprite == null)
                {
                    LogDebug($"图片未找到: {fullPath}，尝试PNG格式");
                    // 尝试带.png扩展名
                    sprite = Resources.Load<Sprite>(fullPath + ".png");
                }
                
                if (sprite == null)
                {
                    LogDebug($"尝试不带路径加载: {imageName}");
                    // 尝试直接加载图片名
                    sprite = Resources.Load<Sprite>("Images/chips/" + imageName);
                }

                if (sprite != null)
                {
                    Image buttonImage = buttonObj.GetComponent<Image>();
                    buttonImage.sprite = sprite;
                    buttonImage.type = Image.Type.Simple;
                    buttonImage.preserveAspect = true;
                    
                    LogDebug($"✅ 成功加载图片: {imageName}");
                    return true;
                }
                else
                {
                    LogDebug($"❌ 图片加载失败: {imageName}，将使用文字显示");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogError($"加载图片异常 {imageName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取筹码默认颜色
        /// </summary>
        private Color GetChipColor(int index)
        {
            Color[] colors = {
                new Color(0f, 0.8f, 0f, 1f),     // 绿色 - 2
                new Color(0f, 0.5f, 1f, 1f),     // 蓝色 - 10  
                new Color(1f, 0.5f, 0f, 1f),     // 橙色 - 20
                new Color(0.8f, 0f, 0.8f, 1f),   // 紫色 - 40
                new Color(1f, 0f, 0f, 1f)        // 红色 - 100
            };
            
            return index < colors.Length ? colors[index] : Color.gray;
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
            if (chipIndex < 0 || chipIndex >= chipButtons.Length) return;

            // 取消之前选择
            if (selectedChipIndex >= 0 && selectedChipIndex < chipButtons.Length)
            {
                SetChipSelected(selectedChipIndex, false);
            }

            // 设置新选择
            selectedChipIndex = chipIndex;
            SetChipSelected(chipIndex, true);

            int chipValue = chipValues[chipIndex];
            LogDebug($"选择筹码: {chipValue}");

            // 触发事件
            OnChipSelected?.Invoke(chipValue);
        }

        /// <summary>
        /// 设置筹码选中状态
        /// </summary>
        private void SetChipSelected(int chipIndex, bool selected)
        {
            if (chipIndex < 0 || chipIndex >= chipButtons.Length) return;
            if (chipButtons[chipIndex] == null) return;

            Transform buttonTransform = chipButtons[chipIndex].transform;
            Vector3 targetScale = selected ? Vector3.one * 1.15f : Vector3.one;
            
            if (Application.isPlaying)
            {
                StartCoroutine(ScaleAnimation(buttonTransform, targetScale, 0.2f));
            }
            else
            {
                buttonTransform.localScale = targetScale;
            }
        }

        /// <summary>
        /// 缩放动画
        /// </summary>
        private System.Collections.IEnumerator ScaleAnimation(Transform target, Vector3 targetScale, float duration)
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

        #endregion

        #region 🐛 调试方法

        /// <summary>
        /// 调试日志
        /// </summary>
        private void LogDebug(string message)
        {
            if (enableDebugMode)
            {
                Debug.Log($"[ChipSelectionArea-{instanceId}] {message}");
            }
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        private void LogError(string message)
        {
            Debug.LogError($"[ChipSelectionArea-{instanceId}] ❌ {message}");
        }

        /// <summary>
        /// 显示状态
        /// </summary>
        [ContextMenu("📊 显示状态")]
        public void ShowStatus()
        {
            Debug.Log($"=== ChipSelectionArea-{instanceId} 状态 ===");
            Debug.Log($"🆔 实例ID: {instanceId}");
            Debug.Log($"🔧 已初始化: {isInitialized}");
            Debug.Log($"🎨 UI已创建: {isUICreated}");
            Debug.Log($"🔥 自动创建: {autoCreateAndShow}");
            Debug.Log($"💎 持久显示: {persistentDisplay}");
            Debug.Log($"🎯 选中筹码: {GetSelectedChipValue()}");
            Debug.Log($"📋 筹码配置: [{string.Join(", ", chipValues)}]");
            Debug.Log($"📊 活动实例数: {instances.Count}");
            
            // 检查按钮状态
            Debug.Log($"更多按钮: {(moreButton != null ? "✓" : "✗")}");
            Debug.Log($"筹码按钮: {(chipButtons != null ? chipButtons.Length : 0)}个");
            Debug.Log($"续压按钮: {(rebetButton != null ? "✓" : "✗")}");
        }

        #endregion
    }
}
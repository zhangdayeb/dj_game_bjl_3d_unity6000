// Assets/UI/Components/VideoOverlay/StatusDisplay.cs
// 状态显示组件 - 持久显示版本
// 显示游戏状态文字，支持不同状态的颜色变化和动画效果
// 特点：执行后UI依然可见，支持编辑器预览
// 创建时间: 2025/6/26

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace BaccaratGame.UI.Components
{
    /// <summary>
    /// 状态显示组件 - 持久显示版本
    /// 立即创建并持久显示UI，不依赖运行状态
    /// </summary>
    public class StatusDisplay : MonoBehaviour
    {
        #region 序列化字段

        [Header("自动显示设置")]
        [SerializeField] private bool autoCreateAndShow = false;         // 自动创建并显示
        [SerializeField] private bool showOnAwake = false;               // 启动时显示
        [SerializeField] private bool immediateDisplay = false;          // 立即显示

        [Header("UI组件引用")]
        [SerializeField] private Text statusText;                       // 状态文字
        [SerializeField] private Image backgroundImage;                 // 背景图片
        [SerializeField] private Image iconImage;                       // 状态图标

        [Header("布局配置")]
        [SerializeField] private Vector2 panelSize = new Vector2(180, 50);          // 面板大小
        [SerializeField] private Vector2 panelPosition = new Vector2(0, 0);         // 面板位置

        [Header("显示配置")]
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);  // 背景颜色
        [SerializeField] private Color normalTextColor = Color.white;               // 普通文字颜色
        [SerializeField] private Color warningTextColor = Color.yellow;             // 警告文字颜色
        [SerializeField] private Color errorTextColor = Color.red;                  // 错误文字颜色
        [SerializeField] private Color successTextColor = Color.green;              // 成功文字颜色
        [SerializeField] private int fontSize = 20;                                 // 字体大小
        [SerializeField] private FontStyle fontStyle = FontStyle.Bold;              // 字体样式

        [Header("动画配置")]
        [SerializeField] private bool enableFlashAnimation = true;      // 启用闪烁动画
        [SerializeField] private float flashDuration = 0.5f;           // 闪烁持续时间
        [SerializeField] private bool enableTypewriter = true;          // 启用打字机效果
        [SerializeField] private float typewriterSpeed = 0.05f;         // 打字机速度
        [SerializeField] private bool enablePulseEffect = true;         // 启用脉冲效果
        [SerializeField] private float pulseSpeed = 2f;                 // 脉冲速度

        [Header("状态配置")]
        [SerializeField] private string defaultStatus = "准备中";       // 默认状态
        [SerializeField] private StatusType defaultStatusType = StatusType.Normal; // 默认状态类型
        [SerializeField] private bool showStatusIcon = true;            // 显示状态图标
        [SerializeField] private bool autoDemo = false;                 // 自动演示

        [Header("调试设置")]
        [SerializeField] private bool enableDebugMode = true;           // 启用调试模式
        [SerializeField] private bool showTestData = true;              // 显示测试数据

        #endregion

        #region 私有字段

        private RectTransform rectTransform;
        private Canvas parentCanvas;
        private bool statusUICreated = false;

        // 状态管理
        private string currentStatus = "";
        private StatusType currentStatusType = StatusType.Normal;
        private bool isVisible = true;
        private bool isAnimating = false;

        // UI面板引用
        private GameObject statusPanel;
        private RectTransform statusRect;

        // 动画相关
        private float pulseTimer = 0f;
        private Vector3 originalScale;
        private Coroutine currentAnimation;

        // 状态历史
        private System.Collections.Generic.List<string> statusHistory = new System.Collections.Generic.List<string>();

        #endregion

        #region 枚举定义

        /// <summary>
        /// 状态类型枚举
        /// </summary>
        public enum StatusType
        {
            Normal,   // 普通状态 - 白色
            Warning,  // 警告状态 - 黄色
            Error,    // 错误状态 - 红色
            Success,  // 成功状态 - 绿色
            Info      // 信息状态 - 蓝色
        }

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            InitializeComponent();

            if (showOnAwake)
            {
                CreateAndShowStatus();
            }
        }

        private void Start()
        {
            if (!statusUICreated && autoCreateAndShow)
            {
                CreateAndShowStatus();
            }

            SetupExistingComponents();

            if (showTestData)
            {
                SetStatus(defaultStatus, defaultStatusType);
            }

            if (autoDemo)
            {
                StartStatusDemo();
            }
        }

        private void Update()
        {
            UpdateAnimations();
        }

        private void OnValidate()
        {
            // 编辑器中实时预览
            if (Application.isEditor && !Application.isPlaying)
            {
                if (immediateDisplay)
                {
                    InitializeComponent();
                    CreateAndShowStatus();
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
                Debug.Log("[StatusDisplay] 组件初始化完成");
        }

        /// <summary>
        /// 如需要则创建Canvas
        /// </summary>
        private void CreateCanvasIfNeeded()
        {
            GameObject canvasObj = new GameObject("StatusDisplayCanvas");
            canvasObj.transform.SetParent(transform.parent);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            parentCanvas = canvas;
            transform.SetParent(canvasObj.transform);

            if (enableDebugMode)
                Debug.Log("[StatusDisplay] 创建了新的Canvas");
        }

        /// <summary>
        /// 设置现有组件
        /// </summary>
        private void SetupExistingComponents()
        {
            // 如果手动赋值了组件，进行初始化设置
            if (statusText != null)
            {
                statusText.color = normalTextColor;
                statusText.fontSize = fontSize;
                statusText.fontStyle = fontStyle;
                statusText.alignment = TextAnchor.MiddleLeft;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
                backgroundImage.type = Image.Type.Sliced;
            }

            if (iconImage != null)
            {
                iconImage.color = normalTextColor;
                iconImage.type = Image.Type.Simple;
            }
        }

        #endregion

        #region UI创建

        /// <summary>
        /// 创建并显示状态显示器
        /// </summary>
        private void CreateAndShowStatus()
        {
            if (statusUICreated)
            {
                if (enableDebugMode)
                    Debug.Log("[StatusDisplay] 状态UI已存在，跳过创建");
                return;
            }

            CreateStatusPanel();
            CreateStatusUI();
            
            statusUICreated = true;

            if (enableDebugMode)
                Debug.Log("[StatusDisplay] 状态UI创建并显示完成");
        }

        /// <summary>
        /// 创建状态面板
        /// </summary>
        private void CreateStatusPanel()
        {
            // 创建主面板
            statusPanel = new GameObject("StatusPanel");
            statusPanel.transform.SetParent(transform);

            // 设置RectTransform
            statusRect = statusPanel.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.5f, 0.5f);
            statusRect.anchorMax = new Vector2(0.5f, 0.5f);
            statusRect.sizeDelta = panelSize;
            statusRect.anchoredPosition = panelPosition;

            originalScale = statusRect.localScale;

            if (enableDebugMode)
                Debug.Log($"[StatusDisplay] 状态面板已创建 - 大小:{panelSize}, 位置:{panelPosition}");
        }

        /// <summary>
        /// 创建状态UI元素
        /// </summary>
        private void CreateStatusUI()
        {
            // 创建背景
            CreateBackground();

            // 创建状态图标
            if (showStatusIcon)
                CreateStatusIcon();

            // 创建状态文字
            CreateStatusText();

            // 初始化显示
            UpdateStatusDisplay();
        }

        /// <summary>
        /// 创建背景
        /// </summary>
        private void CreateBackground()
        {
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(statusPanel.transform);

            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            backgroundImage = bgObj.AddComponent<Image>();
            backgroundImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
            backgroundImage.color = backgroundColor;
            backgroundImage.type = Image.Type.Sliced;

            if (enableDebugMode)
                Debug.Log("[StatusDisplay] 背景已创建");
        }

        /// <summary>
        /// 创建状态图标
        /// </summary>
        private void CreateStatusIcon()
        {
            GameObject iconObj = new GameObject("StatusIcon");
            iconObj.transform.SetParent(statusPanel.transform);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0f);
            iconRect.anchorMax = new Vector2(0.25f, 1f);
            iconRect.offsetMin = new Vector2(8, 8);
            iconRect.offsetMax = new Vector2(-4, -8);

            iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            iconImage.color = normalTextColor;
            iconImage.type = Image.Type.Simple;

            if (enableDebugMode)
                Debug.Log("[StatusDisplay] 状态图标已创建");
        }

        /// <summary>
        /// 创建状态文字
        /// </summary>
        private void CreateStatusText()
        {
            GameObject textObj = new GameObject("StatusText");
            textObj.transform.SetParent(statusPanel.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            if (showStatusIcon)
            {
                textRect.anchorMin = new Vector2(0.25f, 0f);
                textRect.anchorMax = new Vector2(1f, 1f);
                textRect.offsetMin = new Vector2(4, 0);
                textRect.offsetMax = new Vector2(-8, 0);
            }
            else
            {
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(8, 0);
                textRect.offsetMax = new Vector2(-8, 0);
            }

            statusText = textObj.AddComponent<Text>();
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.fontSize = fontSize;
            statusText.fontStyle = fontStyle;
            statusText.color = normalTextColor;
            statusText.text = defaultStatus;
            statusText.alignment = TextAnchor.MiddleLeft;

            if (enableDebugMode)
                Debug.Log("[StatusDisplay] 状态文字已创建");
        }

        #endregion

        #region 状态管理

        /// <summary>
        /// 设置状态
        /// </summary>
        /// <param name="status">状态文本</param>
        /// <param name="statusType">状态类型</param>
        /// <param name="useAnimation">是否使用动画</param>
        public void SetStatus(string status, StatusType statusType = StatusType.Normal, bool useAnimation = true)
        {
            // 记录状态历史
            if (!string.IsNullOrEmpty(status))
            {
                statusHistory.Add($"[{System.DateTime.Now:HH:mm:ss}] {status}");
                if (statusHistory.Count > 10) // 保留最近10条
                    statusHistory.RemoveAt(0);
            }

            currentStatus = status;
            currentStatusType = statusType;

            // 停止当前动画
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
                isAnimating = false;
            }

            if (useAnimation && enableTypewriter && !string.IsNullOrEmpty(status))
            {
                currentAnimation = StartCoroutine(TypewriterEffect(status));
            }
            else
            {
                UpdateStatusDisplay();
            }

            // 根据状态类型播放闪烁动画
            if (useAnimation && enableFlashAnimation && statusType != StatusType.Normal)
            {
                currentAnimation = StartCoroutine(FlashAnimation());
            }

            if (enableDebugMode)
                Debug.Log($"[StatusDisplay] 设置状态: {status} ({statusType})");
        }

        /// <summary>
        /// 快速设置常用状态
        /// </summary>
        public void SetWaitingStatus(string message = "等待开始")
        {
            SetStatus(message, StatusType.Normal);
        }

        public void SetBettingStatus(string message = "投注阶段")
        {
            SetStatus(message, StatusType.Success);
        }

        public void SetDealingStatus(string message = "发牌中")
        {
            SetStatus(message, StatusType.Warning);
        }

        public void SetResultStatus(string message = "开牌结果")
        {
            SetStatus(message, StatusType.Info);
        }

        public void SetConnectedStatus(string message = "连接成功")
        {
            SetStatus(message, StatusType.Success);
        }

        public void SetErrorStatus(string message = "连接错误")
        {
            SetStatus(message, StatusType.Error);
        }

        public void SetLoadingStatus(string message = "加载中...")
        {
            SetStatus(message, StatusType.Warning);
        }

        /// <summary>
        /// 更新状态显示
        /// </summary>
        private void UpdateStatusDisplay()
        {
            if (statusText != null)
            {
                statusText.text = currentStatus;
                statusText.color = GetStatusColor(currentStatusType);
            }

            if (iconImage != null)
            {
                iconImage.color = GetStatusColor(currentStatusType);
            }
        }

        /// <summary>
        /// 获取状态颜色
        /// </summary>
        private Color GetStatusColor(StatusType statusType)
        {
            return statusType switch
            {
                StatusType.Warning => warningTextColor,
                StatusType.Error => errorTextColor,
                StatusType.Success => successTextColor,
                StatusType.Info => Color.cyan,
                _ => normalTextColor
            };
        }

        #endregion

        #region 动画效果

        /// <summary>
        /// 更新动画效果
        /// </summary>
        private void UpdateAnimations()
        {
            // 脉冲效果
            if (enablePulseEffect && isAnimating)
            {
                UpdatePulseEffect();
            }
        }

        /// <summary>
        /// 更新脉冲效果
        /// </summary>
        private void UpdatePulseEffect()
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float scale = 1f + Mathf.Sin(pulseTimer) * 0.05f;
            
            if (statusRect != null)
            {
                statusRect.localScale = originalScale * scale;
            }
        }

        /// <summary>
        /// 打字机效果
        /// </summary>
        private IEnumerator TypewriterEffect(string text)
        {
            if (statusText == null) yield break;

            isAnimating = true;
            statusText.text = "";
            statusText.color = GetStatusColor(currentStatusType);

            for (int i = 0; i <= text.Length; i++)
            {
                statusText.text = text.Substring(0, i);
                yield return new WaitForSeconds(typewriterSpeed);
            }

            isAnimating = false;
        }

        /// <summary>
        /// 闪烁动画
        /// </summary>
        private IEnumerator FlashAnimation()
        {
            if (backgroundImage == null) yield break;

            Color originalColor = backgroundColor;
            Color flashColor = GetStatusColor(currentStatusType);
            flashColor.a = 0.3f;

            for (int i = 0; i < 3; i++)
            {
                backgroundImage.color = flashColor;
                yield return new WaitForSeconds(flashDuration * 0.5f);

                backgroundImage.color = originalColor;
                yield return new WaitForSeconds(flashDuration * 0.5f);
            }
        }

        /// <summary>
        /// 重置动画效果
        /// </summary>
        private void ResetAnimations()
        {
            if (statusRect != null)
            {
                statusRect.localScale = originalScale;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
            }

            pulseTimer = 0f;
            isAnimating = false;
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 显示状态面板
        /// </summary>
        public void Show()
        {
            if (statusPanel != null)
                statusPanel.SetActive(true);
            isVisible = true;

            if (enableDebugMode)
                Debug.Log("[StatusDisplay] 显示状态面板");
        }

        /// <summary>
        /// 隐藏状态面板
        /// </summary>
        public void Hide()
        {
            if (statusPanel != null)
                statusPanel.SetActive(false);
            isVisible = false;

            if (enableDebugMode)
                Debug.Log("[StatusDisplay] 隐藏状态面板");
        }

        /// <summary>
        /// 切换显示状态
        /// </summary>
        public void Toggle()
        {
            if (isVisible)
                Hide();
            else
                Show();
        }

        /// <summary>
        /// 获取当前状态
        /// </summary>
        public string GetCurrentStatus()
        {
            return currentStatus;
        }

        /// <summary>
        /// 获取当前状态类型
        /// </summary>
        public StatusType GetCurrentStatusType()
        {
            return currentStatusType;
        }

        /// <summary>
        /// 清除状态
        /// </summary>
        public void ClearStatus()
        {
            SetStatus("", StatusType.Normal, false);
        }

        /// <summary>
        /// 获取状态历史
        /// </summary>
        public System.Collections.Generic.List<string> GetStatusHistory()
        {
            return new System.Collections.Generic.List<string>(statusHistory);
        }

        /// <summary>
        /// 设置面板位置
        /// </summary>
        public void SetPosition(Vector2 position)
        {
            panelPosition = position;
            if (statusRect != null)
                statusRect.anchoredPosition = position;
        }

        /// <summary>
        /// 设置面板大小
        /// </summary>
        public void SetSize(Vector2 size)
        {
            panelSize = size;
            if (statusRect != null)
                statusRect.sizeDelta = size;
        }

        /// <summary>
        /// 设置字体大小
        /// </summary>
        public void SetFontSize(int size)
        {
            fontSize = size;
            if (statusText != null)
                statusText.fontSize = size;
        }

        #endregion

        #region 演示和测试

        /// <summary>
        /// 开始状态演示
        /// </summary>
        private void StartStatusDemo()
        {
            StartCoroutine(StatusDemoSequence());
        }

        /// <summary>
        /// 状态演示序列
        /// </summary>
        private IEnumerator StatusDemoSequence()
        {
            yield return new WaitForSeconds(1f);

            SetWaitingStatus();
            yield return new WaitForSeconds(2f);

            SetBettingStatus();
            yield return new WaitForSeconds(3f);

            SetDealingStatus();
            yield return new WaitForSeconds(2f);

            SetResultStatus();
            yield return new WaitForSeconds(2f);

            SetConnectedStatus();
            yield return new WaitForSeconds(1f);

            SetErrorStatus();
            yield return new WaitForSeconds(2f);

            SetLoadingStatus();
            yield return new WaitForSeconds(2f);

            // 重复演示
            if (autoDemo)
            {
                StartStatusDemo();
            }
        }

        #endregion

        #region 调试方法

        /// <summary>
        /// 强制显示状态显示器
        /// </summary>
        [ContextMenu("强制显示状态显示器")]
        public void ForceShowStatus()
        {
            statusUICreated = false;
            CreateAndShowStatus();
        }

        /// <summary>
        /// 显示组件状态
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowComponentStatus()
        {
            Debug.Log("=== StatusDisplay 组件状态 ===");
            Debug.Log($"自动创建并显示: {autoCreateAndShow}");
            Debug.Log($"启动时显示: {showOnAwake}");
            Debug.Log($"立即显示: {immediateDisplay}");
            Debug.Log($"状态UI已创建: {statusUICreated}");
            Debug.Log($"父Canvas: {(parentCanvas != null ? "✓" : "✗")}");
            Debug.Log($"状态面板: {(statusPanel != null ? "✓" : "✗")} - {(statusPanel?.activeInHierarchy == true ? "显示" : "隐藏")}");
            Debug.Log($"状态文字: {(statusText != null ? "✓" : "✗")}");
            Debug.Log($"背景图片: {(backgroundImage != null ? "✓" : "✗")}");
            Debug.Log($"状态图标: {(iconImage != null ? "✓" : "✗")}");
            Debug.Log($"当前状态: {currentStatus} ({currentStatusType})");
            Debug.Log($"可见性: {isVisible}");
            Debug.Log($"动画中: {isAnimating}");
            Debug.Log($"状态历史: {statusHistory.Count}条记录");
        }

        /// <summary>
        /// 测试所有状态
        /// </summary>
        [ContextMenu("测试所有状态")]
        public void TestAllStatuses()
        {
            Debug.Log("[StatusDisplay] 开始测试所有状态");

            SetWaitingStatus("测试等待状态");
            System.Threading.Thread.Sleep(100);
            SetBettingStatus("测试投注状态");
            System.Threading.Thread.Sleep(100);
            SetDealingStatus("测试发牌状态");
            System.Threading.Thread.Sleep(100);
            SetResultStatus("测试结果状态");
            System.Threading.Thread.Sleep(100);
            SetConnectedStatus("测试连接状态");
            System.Threading.Thread.Sleep(100);
            SetErrorStatus("测试错误状态");

            Debug.Log("[StatusDisplay] 状态测试完成");
        }

        /// <summary>
        /// 开始演示模式
        /// </summary>
        [ContextMenu("开始演示模式")]
        public void StartDemo()
        {
            autoDemo = true;
            StartStatusDemo();
        }

        /// <summary>
        /// 停止演示模式
        /// </summary>
        [ContextMenu("停止演示模式")]
        public void StopDemo()
        {
            autoDemo = false;
            StopAllCoroutines();
        }

        /// <summary>
        /// 删除所有UI
        /// </summary>
        [ContextMenu("删除所有UI")]
        public void ClearAllUI()
        {
            if (statusPanel != null)
            {
                if (Application.isPlaying)
                    Destroy(statusPanel);
                else
                    DestroyImmediate(statusPanel);
                statusPanel = null;
            }

            // 清空引用
            statusText = null;
            backgroundImage = null;
            iconImage = null;
            statusRect = null;

            statusUICreated = false;

            Debug.Log("[StatusDisplay] 所有UI已删除");
        }

        /// <summary>
        /// 重新初始化
        /// </summary>
        [ContextMenu("重新初始化")]
        public void Reinitialize()
        {
            ClearAllUI();
            InitializeComponent();
            CreateAndShowStatus();
        }

        /// <summary>
        /// 打印状态历史
        /// </summary>
        [ContextMenu("打印状态历史")]
        public void PrintStatusHistory()
        {
            Debug.Log("=== 状态历史记录 ===");
            foreach (string record in statusHistory)
            {
                Debug.Log(record);
            }
        }

        #endregion
    }
}
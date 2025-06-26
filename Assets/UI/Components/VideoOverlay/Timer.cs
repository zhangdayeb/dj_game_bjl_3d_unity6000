// Assets/UI/Components/VideoOverlay/Timer.cs
// 倒计时组件 - 持久显示版本
// 圆形背景 + 倒计时数字，立即创建并持久显示UI
// 特点：执行后UI依然可见，支持编辑器预览
// 创建时间: 2025/6/26

using UnityEngine;
using UnityEngine.UI;

namespace BaccaratGame.UI.Components
{
    /// <summary>
    /// 倒计时组件 - 持久显示版本
    /// 立即创建并持久显示UI，不依赖运行状态
    /// </summary>
    public class Timer : MonoBehaviour
    {
        #region 序列化字段

        [Header("自动显示设置")]
        [SerializeField] private bool autoCreateAndShow = false;      // 自动创建并显示
        [SerializeField] private bool showOnAwake = false;            // 启动时显示
        [SerializeField] private bool immediateDisplay = false;       // 立即显示

        [Header("UI组件引用")]
        [SerializeField] private Text timerText;                     // 倒计时文字
        [SerializeField] private Image circleBackground;             // 圆形背景
        [SerializeField] private Image progressRing;                 // 进度环

        [Header("布局配置")]
        [SerializeField] private Vector2 timerSize = new Vector2(80, 80);          // 计时器大小
        [SerializeField] private Vector2 timerPosition = new Vector2(0, 0);        // 计时器位置

        [Header("视觉配置")]
        [SerializeField] private Color circleColor = new Color(0, 0, 0, 0.7f);     // 圆形背景颜色
        [SerializeField] private Color textColor = Color.white;                     // 文字颜色
        [SerializeField] private Color progressColor = new Color(1f, 0.8f, 0f, 1f); // 进度环颜色
        [SerializeField] private Color warningColor = Color.red;                    // 警告颜色 (时间不足时)
        [SerializeField] private int fontSize = 32;                                 // 字体大小
        [SerializeField] private FontStyle fontStyle = FontStyle.Bold;              // 字体样式

        [Header("计时器配置")]
        [SerializeField] private float defaultTime = 30f;           // 默认倒计时时间
        [SerializeField] private float warningThreshold = 10f;      // 警告阈值
        [SerializeField] private bool showProgressRing = true;      // 显示进度环
        [SerializeField] private bool autoRestart = false;          // 自动重启

        [Header("动画效果")]
        [SerializeField] private bool enablePulseEffect = true;     // 启用脉冲效果
        [SerializeField] private bool enableWarningFlash = true;    // 启用警告闪烁
        [SerializeField] private float pulseSpeed = 2f;             // 脉冲速度
        [SerializeField] private float flashSpeed = 4f;             // 闪烁速度

        [Header("调试设置")]
        [SerializeField] private bool enableDebugMode = true;       // 启用调试模式
        [SerializeField] private bool showTestData = true;          // 显示测试数据

        #endregion

        #region 私有字段

        private RectTransform rectTransform;
        private Canvas parentCanvas;
        private bool timerUICreated = false;

        // 计时器状态
        private float currentTime;
        private float totalTime;
        private bool isActive = false;
        private bool isPaused = false;
        private bool isWarning = false;

        // UI面板引用
        private GameObject timerPanel;
        private RectTransform timerRect;

        // 动画相关
        private float pulseTimer = 0f;
        private float flashTimer = 0f;
        private Vector3 originalScale;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            InitializeComponent();

            if (showOnAwake)
            {
                CreateAndShowTimer();
            }
        }

        private void Start()
        {
            if (!timerUICreated && autoCreateAndShow)
            {
                CreateAndShowTimer();
            }

            SetupExistingComponents();

            if (showTestData)
            {
                StartTimer(defaultTime);
            }
        }

        private void Update()
        {
            UpdateTimer();
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
                    CreateAndShowTimer();
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
                Debug.Log("[Timer] 组件初始化完成");
        }

        /// <summary>
        /// 如需要则创建Canvas
        /// </summary>
        private void CreateCanvasIfNeeded()
        {
            GameObject canvasObj = new GameObject("TimerCanvas");
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
                Debug.Log("[Timer] 创建了新的Canvas");
        }

        /// <summary>
        /// 设置现有组件
        /// </summary>
        private void SetupExistingComponents()
        {
            // 如果手动赋值了组件，进行初始化设置
            if (timerText != null)
            {
                timerText.color = textColor;
                timerText.fontSize = fontSize;
                timerText.fontStyle = fontStyle;
                timerText.alignment = TextAnchor.MiddleCenter;
            }

            if (circleBackground != null)
            {
                circleBackground.color = circleColor;
                circleBackground.type = Image.Type.Filled;
            }

            if (progressRing != null)
            {
                progressRing.color = progressColor;
                progressRing.type = Image.Type.Filled;
                progressRing.fillMethod = Image.FillMethod.Radial360;
                progressRing.fillOrigin = 2; // Top
                progressRing.fillClockwise = false; // 逆时针
            }
        }

        #endregion

        #region UI创建

        /// <summary>
        /// 创建并显示计时器
        /// </summary>
        private void CreateAndShowTimer()
        {
            if (timerUICreated)
            {
                if (enableDebugMode)
                    Debug.Log("[Timer] 计时器UI已存在，跳过创建");
                return;
            }

            CreateTimerPanel();
            CreateTimerUI();
            
            timerUICreated = true;

            if (enableDebugMode)
                Debug.Log("[Timer] 计时器UI创建并显示完成");
        }

        /// <summary>
        /// 创建计时器面板
        /// </summary>
        private void CreateTimerPanel()
        {
            // 创建主面板
            timerPanel = new GameObject("TimerPanel");
            timerPanel.transform.SetParent(transform);

            // 设置RectTransform
            timerRect = timerPanel.AddComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0.5f, 0.5f);
            timerRect.anchorMax = new Vector2(0.5f, 0.5f);
            timerRect.sizeDelta = timerSize;
            timerRect.anchoredPosition = timerPosition;

            originalScale = timerRect.localScale;

            if (enableDebugMode)
                Debug.Log($"[Timer] 计时器面板已创建 - 大小:{timerSize}, 位置:{timerPosition}");
        }

        /// <summary>
        /// 创建计时器UI元素
        /// </summary>
        private void CreateTimerUI()
        {
            // 创建圆形背景
            CreateCircleBackground();

            // 创建进度环
            if (showProgressRing)
                CreateProgressRing();

            // 创建倒计时文字
            CreateTimerText();

            // 初始化显示
            UpdateTimerDisplay();
        }

        /// <summary>
        /// 创建圆形背景
        /// </summary>
        private void CreateCircleBackground()
        {
            GameObject bgObj = new GameObject("CircleBackground");
            bgObj.transform.SetParent(timerPanel.transform);

            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            circleBackground = bgObj.AddComponent<Image>();
            circleBackground.sprite = CreateCircleSprite();
            circleBackground.color = circleColor;
            circleBackground.type = Image.Type.Simple;

            if (enableDebugMode)
                Debug.Log("[Timer] 圆形背景已创建");
        }

        /// <summary>
        /// 创建进度环
        /// </summary>
        private void CreateProgressRing()
        {
            GameObject ringObj = new GameObject("ProgressRing");
            ringObj.transform.SetParent(timerPanel.transform);

            RectTransform ringRect = ringObj.AddComponent<RectTransform>();
            ringRect.anchorMin = Vector2.zero;
            ringRect.anchorMax = Vector2.one;
            ringRect.offsetMin = new Vector2(4, 4);
            ringRect.offsetMax = new Vector2(-4, -4);

            progressRing = ringObj.AddComponent<Image>();
            progressRing.sprite = CreateRingSprite();
            progressRing.color = progressColor;
            progressRing.type = Image.Type.Filled;
            progressRing.fillMethod = Image.FillMethod.Radial360;
            progressRing.fillOrigin = 2; // Top
            progressRing.fillClockwise = false;
            progressRing.fillAmount = 1f;

            if (enableDebugMode)
                Debug.Log("[Timer] 进度环已创建");
        }

        /// <summary>
        /// 创建倒计时文字
        /// </summary>
        private void CreateTimerText()
        {
            GameObject textObj = new GameObject("TimerText");
            textObj.transform.SetParent(timerPanel.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            timerText = textObj.AddComponent<Text>();
            timerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            timerText.fontSize = fontSize;
            timerText.fontStyle = fontStyle;
            timerText.color = textColor;
            timerText.alignment = TextAnchor.MiddleCenter;
            timerText.text = "30";

            if (enableDebugMode)
                Debug.Log("[Timer] 倒计时文字已创建");
        }

        #endregion

        #region 计时器逻辑

        /// <summary>
        /// 开始计时器
        /// </summary>
        public void StartTimer(float time)
        {
            totalTime = time;
            currentTime = time;
            isActive = true;
            isPaused = false;
            isWarning = false;

            UpdateTimerDisplay();

            if (enableDebugMode)
                Debug.Log($"[Timer] 开始计时: {time}秒");
        }

        /// <summary>
        /// 暂停计时器
        /// </summary>
        public void PauseTimer()
        {
            isPaused = true;

            if (enableDebugMode)
                Debug.Log("[Timer] 计时器已暂停");
        }

        /// <summary>
        /// 恢复计时器
        /// </summary>
        public void ResumeTimer()
        {
            isPaused = false;

            if (enableDebugMode)
                Debug.Log("[Timer] 计时器已恢复");
        }

        /// <summary>
        /// 停止计时器
        /// </summary>
        public void StopTimer()
        {
            isActive = false;
            isPaused = false;
            currentTime = 0f;
            isWarning = false;

            UpdateTimerDisplay();

            if (enableDebugMode)
                Debug.Log("[Timer] 计时器已停止");
        }

        /// <summary>
        /// 重置计时器
        /// </summary>
        public void ResetTimer()
        {
            currentTime = totalTime;
            isWarning = false;

            UpdateTimerDisplay();

            if (enableDebugMode)
                Debug.Log("[Timer] 计时器已重置");
        }

        /// <summary>
        /// 更新计时器
        /// </summary>
        private void UpdateTimer()
        {
            if (!isActive || isPaused) return;

            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;

                // 检查警告状态
                if (currentTime <= warningThreshold && !isWarning)
                {
                    isWarning = true;
                    OnWarningStarted();
                }

                UpdateTimerDisplay();
            }
            else
            {
                currentTime = 0;
                OnTimerFinished();
            }
        }

        /// <summary>
        /// 更新计时器显示
        /// </summary>
        private void UpdateTimerDisplay()
        {
            if (timerText != null)
            {
                int seconds = Mathf.CeilToInt(currentTime);
                timerText.text = seconds.ToString();

                // 警告状态时改变颜色
                timerText.color = isWarning ? warningColor : textColor;
            }

            if (progressRing != null && showProgressRing)
            {
                float progress = totalTime > 0 ? currentTime / totalTime : 0f;
                progressRing.fillAmount = progress;

                // 警告状态时改变颜色
                progressRing.color = isWarning ? warningColor : progressColor;
            }
        }

        #endregion

        #region 动画效果

        /// <summary>
        /// 更新动画效果
        /// </summary>
        private void UpdateAnimations()
        {
            if (!isActive) return;

            // 脉冲效果
            if (enablePulseEffect && isWarning)
            {
                UpdatePulseEffect();
            }

            // 警告闪烁
            if (enableWarningFlash && isWarning)
            {
                UpdateFlashEffect();
            }
        }

        /// <summary>
        /// 更新脉冲效果
        /// </summary>
        private void UpdatePulseEffect()
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float scale = 1f + Mathf.Sin(pulseTimer) * 0.1f;
            
            if (timerRect != null)
            {
                timerRect.localScale = originalScale * scale;
            }
        }

        /// <summary>
        /// 更新闪烁效果
        /// </summary>
        private void UpdateFlashEffect()
        {
            flashTimer += Time.deltaTime * flashSpeed;
            float alpha = 0.5f + Mathf.Sin(flashTimer) * 0.5f;

            if (circleBackground != null)
            {
                Color bgColor = circleColor;
                bgColor.a = alpha;
                circleBackground.color = bgColor;
            }
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 计时器完成时
        /// </summary>
        private void OnTimerFinished()
        {
            isActive = false;
            isWarning = false;

            if (enableDebugMode)
                Debug.Log("[Timer] 计时器完成");

            // 重置动画效果
            ResetAnimations();

            // 自动重启
            if (autoRestart)
            {
                StartTimer(defaultTime);
            }
        }

        /// <summary>
        /// 警告开始时
        /// </summary>
        private void OnWarningStarted()
        {
            if (enableDebugMode)
                Debug.Log("[Timer] 进入警告状态");
        }

        /// <summary>
        /// 重置动画效果
        /// </summary>
        private void ResetAnimations()
        {
            if (timerRect != null)
            {
                timerRect.localScale = originalScale;
            }

            if (circleBackground != null)
            {
                circleBackground.color = circleColor;
            }

            pulseTimer = 0f;
            flashTimer = 0f;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 创建圆形Sprite
        /// </summary>
        private Sprite CreateCircleSprite()
        {
            // 使用Unity内置的Knob sprite作为圆形
            return Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
        }

        /// <summary>
        /// 创建环形Sprite
        /// </summary>
        private Sprite CreateRingSprite()
        {
            // 使用Unity内置的Knob sprite作为环形
            return Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 强制显示计时器
        /// </summary>
        [ContextMenu("强制显示计时器")]
        public void ForceShowTimer()
        {
            timerUICreated = false;
            CreateAndShowTimer();
        }

        /// <summary>
        /// 获取当前时间
        /// </summary>
        public float GetCurrentTime()
        {
            return currentTime;
        }

        /// <summary>
        /// 获取总时间
        /// </summary>
        public float GetTotalTime()
        {
            return totalTime;
        }

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsActive()
        {
            return isActive;
        }

        /// <summary>
        /// 是否已暂停
        /// </summary>
        public bool IsPaused()
        {
            return isPaused;
        }

        /// <summary>
        /// 是否处于警告状态
        /// </summary>
        public bool IsWarning()
        {
            return isWarning;
        }

        /// <summary>
        /// 设置计时器可见性
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (timerPanel != null)
                timerPanel.SetActive(visible);
        }

        /// <summary>
        /// 设置计时器位置
        /// </summary>
        public void SetPosition(Vector2 position)
        {
            timerPosition = position;
            if (timerRect != null)
                timerRect.anchoredPosition = position;
        }

        /// <summary>
        /// 设置计时器大小
        /// </summary>
        public void SetSize(Vector2 size)
        {
            timerSize = size;
            if (timerRect != null)
                timerRect.sizeDelta = size;
        }

        #endregion

        #region 调试方法

        /// <summary>
        /// 显示组件状态
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowComponentStatus()
        {
            Debug.Log("=== Timer 组件状态 ===");
            Debug.Log($"自动创建并显示: {autoCreateAndShow}");
            Debug.Log($"启动时显示: {showOnAwake}");
            Debug.Log($"立即显示: {immediateDisplay}");
            Debug.Log($"计时器UI已创建: {timerUICreated}");
            Debug.Log($"父Canvas: {(parentCanvas != null ? "✓" : "✗")}");
            Debug.Log($"计时器面板: {(timerPanel != null ? "✓" : "✗")} - {(timerPanel?.activeInHierarchy == true ? "显示" : "隐藏")}");
            Debug.Log($"倒计时文字: {(timerText != null ? "✓" : "✗")}");
            Debug.Log($"圆形背景: {(circleBackground != null ? "✓" : "✗")}");
            Debug.Log($"进度环: {(progressRing != null ? "✓" : "✗")}");
            Debug.Log($"当前时间: {currentTime:F1}秒");
            Debug.Log($"总时间: {totalTime:F1}秒");
            Debug.Log($"运行状态: {(isActive ? "运行中" : "已停止")}");
            Debug.Log($"暂停状态: {(isPaused ? "已暂停" : "正常")}");
            Debug.Log($"警告状态: {(isWarning ? "警告中" : "正常")}");
        }

        /// <summary>
        /// 测试计时器功能
        /// </summary>
        [ContextMenu("测试计时器功能")]
        public void TestTimerFunctions()
        {
            Debug.Log("[Timer] 开始测试计时器功能");

            StartTimer(15f);
            Debug.Log("1. 开始15秒倒计时");

            System.Threading.Thread.Sleep(100);
            PauseTimer();
            Debug.Log("2. 暂停计时器");

            System.Threading.Thread.Sleep(100);
            ResumeTimer();
            Debug.Log("3. 恢复计时器");

            Debug.Log("[Timer] 计时器功能测试完成");
        }

        /// <summary>
        /// 删除所有UI
        /// </summary>
        [ContextMenu("删除所有UI")]
        public void ClearAllUI()
        {
            if (timerPanel != null)
            {
                if (Application.isPlaying)
                    Destroy(timerPanel);
                else
                    DestroyImmediate(timerPanel);
                timerPanel = null;
            }

            // 清空引用
            timerText = null;
            circleBackground = null;
            progressRing = null;
            timerRect = null;

            timerUICreated = false;

            Debug.Log("[Timer] 所有UI已删除");
        }

        /// <summary>
        /// 重新初始化
        /// </summary>
        [ContextMenu("重新初始化")]
        public void Reinitialize()
        {
            ClearAllUI();
            InitializeComponent();
            CreateAndShowTimer();
        }

        #endregion
    }
}
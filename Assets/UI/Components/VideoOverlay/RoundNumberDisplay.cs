// Assets/UI/Components/VideoOverlay/RoundNumberDisplay.cs
// 局号显示组件 - 持久显示版本
// 显示当前游戏局号和桌台信息，支持动态更新和动画效果
// 特点：执行后UI依然可见，支持编辑器预览和持久显示
// 创建时间: 2025/6/26

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace BaccaratGame.UI.Components
{
    /// <summary>
    /// 局号显示组件 - 持久显示版本
    /// 立即创建并持久显示UI，不依赖运行状态
    /// </summary>
    public class RoundNumberDisplay : MonoBehaviour
    {
        #region 序列化字段

        [Header("自动显示设置")]
        [SerializeField] private bool autoCreateAndShow = false;         // 自动创建并显示
        [SerializeField] private bool showOnAwake = false;               // 启动时显示
        [SerializeField] private bool immediateDisplay = false;          // 立即显示
        [SerializeField] private bool enableDebugMode = true;           // 启用调试模式

        [Header("UI组件引用")]
        [SerializeField] private Text roundNumberText;                  // 局号文字
        [SerializeField] private Text tableNameText;                    // 桌台名称文字
        [SerializeField] private Text roundLabelText;                   // 局号标签
        [SerializeField] private Text tableLabelText;                   // 桌台标签
        [SerializeField] private Image backgroundImage;                 // 背景图片

        [Header("布局配置")]
        [SerializeField] private Vector2 panelSize = new Vector2(200, 80);          // 面板大小
        [SerializeField] private Vector2 panelPosition = new Vector2(100, -100);    // 面板位置

        [Header("显示配置")]
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);  // 背景颜色
        [SerializeField] private Color labelColor = Color.gray;                     // 标签颜色
        [SerializeField] private Color valueColor = Color.white;                    // 数值颜色
        [SerializeField] private Color highlightColor = Color.yellow;               // 高亮颜色
        [SerializeField] private int labelFontSize = 14;                            // 标签字体大小
        [SerializeField] private int valueFontSize = 18;                            // 数值字体大小

        [Header("动画配置")]
        [SerializeField] private bool enableUpdateAnimation = true;      // 启用更新动画
        [SerializeField] private float highlightDuration = 1f;          // 高亮持续时间
        [SerializeField] private bool enableNumberAnimation = true;      // 启用数字动画
        [SerializeField] private float numberAnimationSpeed = 0.05f;     // 数字动画速度
        [SerializeField] private bool enablePulseEffect = true;          // 启用脉冲效果
        [SerializeField] private float pulseSpeed = 2f;                  // 脉冲速度

        [Header("演示配置")]
        [SerializeField] private bool autoDemo = true;                   // 自动演示
        [SerializeField] private float demoInterval = 4f;               // 演示间隔

        [Header("默认信息")]
        [SerializeField] private string defaultRoundNumber = "T250626001";  // 默认局号
        [SerializeField] private string defaultTableName = "百家乐桌台A";    // 默认桌台名称

        #endregion

        #region 私有变量

        // UI对象引用
        private GameObject roundPanel;                   // 局号面板
        private RectTransform roundRect;                 // 面板RectTransform
        private Canvas parentCanvas;                     // 父Canvas

        // 状态变量
        private string currentRoundNumber = "";          // 当前局号
        private string currentTableName = "";           // 当前桌台名称
        private bool isAnimating = false;                // 是否正在动画
        private bool roundUICreated = false;             // UI是否已创建
        private Vector3 originalScale;                   // 原始缩放

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            if (enableDebugMode)
                Debug.Log("[RoundNumberDisplay] Awake - 开始初始化");

            // 立即创建UI以确保持久显示
            if (autoCreateAndShow)
            {
                CreateAndShowRoundDisplay();
            }
        }

        private void Start()
        {
            if (enableDebugMode)
                Debug.Log("[RoundNumberDisplay] Start - 开始设置");

            // 确保UI已创建
            if (!roundUICreated && showOnAwake)
            {
                CreateAndShowRoundDisplay();
            }

            // 设置默认信息
            SetDefaultInfo();

            // 开始演示
            if (autoDemo)
            {
                StartRoundDemo();
            }
        }

        private void OnValidate()
        {
            // 在编辑器中实时更新
            if (Application.isPlaying && roundUICreated)
            {
                UpdateDisplayProperties();
            }
        }

        #endregion

        #region UI创建

        /// <summary>
        /// 创建并显示局号显示器
        /// </summary>
        private void CreateAndShowRoundDisplay()
        {
            if (roundUICreated)
            {
                if (enableDebugMode)
                    Debug.Log("[RoundNumberDisplay] 局号UI已存在，跳过创建");
                return;
            }

            // 查找或创建Canvas
            FindOrCreateCanvas();

            // 创建局号面板
            CreateRoundPanel();

            // 创建UI元素
            CreateRoundUI();

            roundUICreated = true;

            if (enableDebugMode)
                Debug.Log("[RoundNumberDisplay] 局号UI创建并显示完成");
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
                GameObject canvasObj = new GameObject("RoundDisplayCanvas");
                parentCanvas = canvasObj.AddComponent<Canvas>();
                parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                parentCanvas.sortingOrder = 100;

                // 添加GraphicRaycaster
                canvasObj.AddComponent<GraphicRaycaster>();

                if (enableDebugMode)
                    Debug.Log("[RoundNumberDisplay] 创建新Canvas用于局号显示");
            }
        }

        /// <summary>
        /// 创建局号面板
        /// </summary>
        private void CreateRoundPanel()
        {
            // 创建主面板
            roundPanel = new GameObject("RoundPanel");
            roundPanel.transform.SetParent(parentCanvas.transform);

            // 设置RectTransform
            roundRect = roundPanel.AddComponent<RectTransform>();
            roundRect.anchorMin = new Vector2(0, 1);       // 左上角锚点
            roundRect.anchorMax = new Vector2(0, 1);
            roundRect.sizeDelta = panelSize;
            roundRect.anchoredPosition = panelPosition;

            originalScale = roundRect.localScale;

            if (enableDebugMode)
                Debug.Log($"[RoundNumberDisplay] 局号面板已创建 - 大小:{panelSize}, 位置:{panelPosition}");
        }

        /// <summary>
        /// 创建局号UI元素
        /// </summary>
        private void CreateRoundUI()
        {
            // 创建背景
            CreateBackground();

            // 创建局号区域（上半部分）
            CreateRoundSection();

            // 创建桌台区域（下半部分）
            CreateTableSection();

            // 初始化显示
            UpdateDisplayProperties();
        }

        /// <summary>
        /// 创建背景
        /// </summary>
        private void CreateBackground()
        {
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(roundPanel.transform);

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
        /// 创建局号区域
        /// </summary>
        private void CreateRoundSection()
        {
            // 创建局号标签
            CreateRoundLabel();

            // 创建局号数值
            CreateRoundNumber();
        }

        /// <summary>
        /// 创建局号标签
        /// </summary>
        private void CreateRoundLabel()
        {
            GameObject labelObj = new GameObject("RoundLabel");
            labelObj.transform.SetParent(roundPanel.transform);

            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0.5f);
            labelRect.anchorMax = new Vector2(0.4f, 1f);
            labelRect.offsetMin = new Vector2(8, 2);
            labelRect.offsetMax = new Vector2(-2, -2);

            roundLabelText = labelObj.AddComponent<Text>();
            roundLabelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            roundLabelText.fontSize = labelFontSize;
            roundLabelText.color = labelColor;
            roundLabelText.text = "局号";
            roundLabelText.alignment = TextAnchor.MiddleLeft;
        }

        /// <summary>
        /// 创建局号数值
        /// </summary>
        private void CreateRoundNumber()
        {
            GameObject numberObj = new GameObject("RoundNumber");
            numberObj.transform.SetParent(roundPanel.transform);

            RectTransform numberRect = numberObj.AddComponent<RectTransform>();
            numberRect.anchorMin = new Vector2(0.4f, 0.5f);
            numberRect.anchorMax = new Vector2(1f, 1f);
            numberRect.offsetMin = new Vector2(2, 2);
            numberRect.offsetMax = new Vector2(-8, -2);

            roundNumberText = numberObj.AddComponent<Text>();
            roundNumberText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            roundNumberText.fontSize = valueFontSize;
            roundNumberText.color = valueColor;
            roundNumberText.text = defaultRoundNumber;
            roundNumberText.alignment = TextAnchor.MiddleRight;
            roundNumberText.fontStyle = FontStyle.Bold;
        }

        /// <summary>
        /// 创建桌台区域
        /// </summary>
        private void CreateTableSection()
        {
            // 创建桌台标签
            CreateTableLabel();

            // 创建桌台名称
            CreateTableName();
        }

        /// <summary>
        /// 创建桌台标签
        /// </summary>
        private void CreateTableLabel()
        {
            GameObject labelObj = new GameObject("TableLabel");
            labelObj.transform.SetParent(roundPanel.transform);

            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0f);
            labelRect.anchorMax = new Vector2(0.4f, 0.5f);
            labelRect.offsetMin = new Vector2(8, 2);
            labelRect.offsetMax = new Vector2(-2, -2);

            tableLabelText = labelObj.AddComponent<Text>();
            tableLabelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tableLabelText.fontSize = labelFontSize;
            tableLabelText.color = labelColor;
            tableLabelText.text = "桌台";
            tableLabelText.alignment = TextAnchor.MiddleLeft;
        }

        /// <summary>
        /// 创建桌台名称
        /// </summary>
        private void CreateTableName()
        {
            GameObject nameObj = new GameObject("TableName");
            nameObj.transform.SetParent(roundPanel.transform);

            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.4f, 0f);
            nameRect.anchorMax = new Vector2(1f, 0.5f);
            nameRect.offsetMin = new Vector2(2, 2);
            nameRect.offsetMax = new Vector2(-8, -2);

            tableNameText = nameObj.AddComponent<Text>();
            tableNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tableNameText.fontSize = valueFontSize;
            tableNameText.color = valueColor;
            tableNameText.text = defaultTableName;
            tableNameText.alignment = TextAnchor.MiddleRight;
            tableNameText.fontStyle = FontStyle.Bold;
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 更新局号
        /// </summary>
        /// <param name="roundNumber">局号</param>
        /// <param name="useAnimation">是否使用动画</param>
        public void UpdateRoundNumber(string roundNumber, bool useAnimation = true)
        {
            if (string.IsNullOrEmpty(roundNumber)) return;

            // 确保UI已创建
            if (!roundUICreated)
            {
                CreateAndShowRoundDisplay();
            }

            string previousRound = currentRoundNumber;
            currentRoundNumber = roundNumber;

            if (useAnimation && enableNumberAnimation && !isAnimating && !string.IsNullOrEmpty(previousRound))
            {
                StartCoroutine(AnimateRoundNumberChange(previousRound, roundNumber));
            }
            else
            {
                UpdateRoundNumberDisplay();
            }

            // 播放高亮动画
            if (useAnimation && enableUpdateAnimation)
            {
                StartCoroutine(HighlightAnimation(roundNumberText));
            }

            if (enableDebugMode)
                Debug.Log($"[RoundNumberDisplay] 局号更新: {roundNumber}");
        }

        /// <summary>
        /// 更新桌台名称
        /// </summary>
        /// <param name="tableName">桌台名称</param>
        /// <param name="useAnimation">是否使用动画</param>
        public void UpdateTableName(string tableName, bool useAnimation = true)
        {
            if (string.IsNullOrEmpty(tableName)) return;

            // 确保UI已创建
            if (!roundUICreated)
            {
                CreateAndShowRoundDisplay();
            }

            currentTableName = tableName;

            if (tableNameText != null)
            {
                tableNameText.text = tableName;
            }

            // 播放高亮动画
            if (useAnimation && enableUpdateAnimation)
            {
                StartCoroutine(HighlightAnimation(tableNameText));
            }

            if (enableDebugMode)
                Debug.Log($"[RoundNumberDisplay] 桌台名称更新: {tableName}");
        }

        /// <summary>
        /// 更新局号（数字类型）
        /// </summary>
        /// <param name="roundNumber">局号数字</param>
        /// <param name="useAnimation">是否使用动画</param>
        public void UpdateRoundNumber(int roundNumber, bool useAnimation = true)
        {
            UpdateRoundNumber($"T250626{roundNumber:D3}", useAnimation);
        }

        /// <summary>
        /// 批量更新信息
        /// </summary>
        /// <param name="roundNumber">局号</param>
        /// <param name="tableName">桌台名称</param>
        /// <param name="useAnimation">是否使用动画</param>
        public void UpdateGameInfo(string roundNumber, string tableName, bool useAnimation = true)
        {
            UpdateRoundNumber(roundNumber, useAnimation);
            UpdateTableName(tableName, useAnimation);
        }

        /// <summary>
        /// 获取当前局号
        /// </summary>
        public string GetCurrentRoundNumber()
        {
            return currentRoundNumber;
        }

        /// <summary>
        /// 获取当前桌台名称
        /// </summary>
        public string GetCurrentTableName()
        {
            return currentTableName;
        }

        /// <summary>
        /// 显示/隐藏局号显示器
        /// </summary>
        /// <param name="show">是否显示</param>
        public void ShowRoundDisplay(bool show)
        {
            if (roundPanel != null)
            {
                roundPanel.SetActive(show);
                
                if (enableDebugMode)
                    Debug.Log($"[RoundNumberDisplay] 显示状态: {(show ? "显示" : "隐藏")}");
            }
        }

        /// <summary>
        /// 清除显示
        /// </summary>
        public void ClearDisplay()
        {
            currentRoundNumber = "";
            currentTableName = "";

            if (roundNumberText != null)
                roundNumberText.text = "";
            if (tableNameText != null)
                tableNameText.text = "";

            if (enableDebugMode)
                Debug.Log("[RoundNumberDisplay] 显示已清除");
        }

        #endregion

        #region 设置和配置

        /// <summary>
        /// 设置默认信息
        /// </summary>
        private void SetDefaultInfo()
        {
            if (!string.IsNullOrEmpty(defaultRoundNumber))
            {
                UpdateRoundNumber(defaultRoundNumber, false);
            }

            if (!string.IsNullOrEmpty(defaultTableName))
            {
                UpdateTableName(defaultTableName, false);
            }
        }

        /// <summary>
        /// 更新显示属性
        /// </summary>
        private void UpdateDisplayProperties()
        {
            if (!roundUICreated) return;

            // 更新面板大小和位置
            if (roundRect != null)
            {
                roundRect.sizeDelta = panelSize;
                roundRect.anchoredPosition = panelPosition;
            }

            // 更新背景颜色
            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
            }

            // 更新标签样式
            if (roundLabelText != null)
            {
                roundLabelText.fontSize = labelFontSize;
                roundLabelText.color = labelColor;
            }

            if (tableLabelText != null)
            {
                tableLabelText.fontSize = labelFontSize;
                tableLabelText.color = labelColor;
            }

            // 更新数值样式
            if (roundNumberText != null)
            {
                roundNumberText.fontSize = valueFontSize;
                roundNumberText.color = valueColor;
            }

            if (tableNameText != null)
            {
                tableNameText.fontSize = valueFontSize;
                tableNameText.color = valueColor;
            }
        }

        /// <summary>
        /// 设置位置
        /// </summary>
        public void SetPosition(Vector2 position)
        {
            panelPosition = position;
            if (roundRect != null)
                roundRect.anchoredPosition = position;
        }

        /// <summary>
        /// 设置大小
        /// </summary>
        public void SetSize(Vector2 size)
        {
            panelSize = size;
            if (roundRect != null)
                roundRect.sizeDelta = size;
        }

        /// <summary>
        /// 设置字体大小
        /// </summary>
        public void SetFontSize(int labelSize, int valueSize)
        {
            labelFontSize = labelSize;
            valueFontSize = valueSize;

            if (roundLabelText != null)
                roundLabelText.fontSize = labelSize;
            if (tableLabelText != null)
                tableLabelText.fontSize = labelSize;
            if (roundNumberText != null)
                roundNumberText.fontSize = valueSize;
            if (tableNameText != null)
                tableNameText.fontSize = valueSize;
        }

        #endregion

        #region 动画效果

        /// <summary>
        /// 更新局号显示
        /// </summary>
        private void UpdateRoundNumberDisplay()
        {
            if (roundNumberText != null)
            {
                roundNumberText.text = currentRoundNumber;
            }
        }

        /// <summary>
        /// 局号变化动画
        /// </summary>
        private IEnumerator AnimateRoundNumberChange(string fromNumber, string toNumber)
        {
            if (roundNumberText == null) yield break;

            isAnimating = true;

            // 尝试提取数字部分进行数字动画
            if (TryExtractNumber(fromNumber, out int fromNum) && TryExtractNumber(toNumber, out int toNum))
            {
                string prefix = fromNumber.Substring(0, fromNumber.Length - fromNum.ToString().Length);

                // 数字递增动画
                for (int i = fromNum; i <= toNum; i++)
                {
                    roundNumberText.text = prefix + i.ToString().PadLeft(3, '0');
                    yield return new WaitForSeconds(numberAnimationSpeed);
                }
            }
            else
            {
                // 直接显示目标文本
                roundNumberText.text = toNumber;
            }

            isAnimating = false;
        }

        /// <summary>
        /// 高亮动画
        /// </summary>
        private IEnumerator HighlightAnimation(Text targetText)
        {
            if (targetText == null) yield break;

            Color originalColor = targetText.color;

            // 闪烁高亮
            float elapsed = 0f;
            while (elapsed < highlightDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.PingPong(elapsed * 4f, 1f);
                targetText.color = Color.Lerp(originalColor, highlightColor, t);
                yield return null;
            }

            // 恢复原色
            targetText.color = originalColor;
        }

        /// <summary>
        /// 脉冲效果
        /// </summary>
        private IEnumerator PulseEffect()
        {
            if (roundRect == null) yield break;

            while (enablePulseEffect && roundPanel != null && roundPanel.activeInHierarchy)
            {
                float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.05f;
                roundRect.localScale = originalScale * scale;
                yield return null;
            }

            if (roundRect != null)
                roundRect.localScale = originalScale;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 尝试从文本中提取数字
        /// </summary>
        private bool TryExtractNumber(string text, out int number)
        {
            number = 0;
            if (string.IsNullOrEmpty(text)) return false;

            // 提取末尾的数字
            for (int i = text.Length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(text[i]))
                {
                    string numberPart = text.Substring(i + 1);
                    return int.TryParse(numberPart, out number);
                }
            }

            return int.TryParse(text, out number);
        }

        /// <summary>
        /// 开始局号演示（测试用）
        /// </summary>
        private void StartRoundDemo()
        {
            if (autoDemo)
            {
                StartCoroutine(RoundDemoCoroutine());
            }
        }

        /// <summary>
        /// 局号演示协程（测试用）
        /// </summary>
        private IEnumerator RoundDemoCoroutine()
        {
            yield return new WaitForSeconds(2f);

            // 模拟局号递增
            for (int i = 2; i <= 5; i++)
            {
                UpdateRoundNumber(i);
                yield return new WaitForSeconds(demoInterval);
            }

            // 切换桌台
            UpdateTableName("百家乐桌台B");
            yield return new WaitForSeconds(3f);

            UpdateTableName("VIP桌台1");
            yield return new WaitForSeconds(3f);

            UpdateTableName("百家乐桌台A");
            yield return new WaitForSeconds(2f);

            // 重复演示
            if (autoDemo)
            {
                StartRoundDemo();
            }
        }

        #endregion

        #region 调试方法

        /// <summary>
        /// 强制显示局号显示器
        /// </summary>
        [ContextMenu("强制显示局号显示器")]
        public void ForceShowRoundDisplay()
        {
            roundUICreated = false;
            CreateAndShowRoundDisplay();
        }

        /// <summary>
        /// 显示组件状态
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowComponentStatus()
        {
            Debug.Log("=== RoundNumberDisplay 组件状态 ===");
            Debug.Log($"自动创建并显示: {autoCreateAndShow}");
            Debug.Log($"启动时显示: {showOnAwake}");
            Debug.Log($"立即显示: {immediateDisplay}");
            Debug.Log($"局号UI已创建: {roundUICreated}");
            Debug.Log($"父Canvas: {(parentCanvas != null ? "✓" : "✗")}");
            Debug.Log($"局号面板: {(roundPanel != null ? "✓" : "✗")} - {(roundPanel?.activeInHierarchy == true ? "显示" : "隐藏")}");
            Debug.Log($"局号文字: {(roundNumberText != null ? "✓" : "✗")}");
            Debug.Log($"桌台文字: {(tableNameText != null ? "✓" : "✗")}");
            Debug.Log($"背景图片: {(backgroundImage != null ? "✓" : "✗")}");
            Debug.Log($"当前局号: {currentRoundNumber}");
            Debug.Log($"当前桌台: {currentTableName}");
        }

        /// <summary>
        /// 测试局号更新
        /// </summary>
        [ContextMenu("测试局号更新")]
        public void TestRoundUpdate()
        {
            int testRound = UnityEngine.Random.Range(100, 999);
            UpdateRoundNumber($"T250626{testRound:D3}");
        }

        /// <summary>
        /// 测试桌台更新
        /// </summary>
        [ContextMenu("测试桌台更新")]
        public void TestTableUpdate()
        {
            string[] tables = { "百家乐桌台A", "百家乐桌台B", "VIP桌台1", "VIP桌台2", "经典桌台" };
            string randomTable = tables[UnityEngine.Random.Range(0, tables.Length)];
            UpdateTableName(randomTable);
        }

        /// <summary>
        /// 启动脉冲效果
        /// </summary>
        [ContextMenu("启动脉冲效果")]
        public void StartPulse()
        {
            if (enablePulseEffect)
            {
                StartCoroutine(PulseEffect());
            }
        }

        #endregion
    }
}
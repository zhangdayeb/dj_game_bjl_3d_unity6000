// Assets/UI/Components/VideoOverlay/BalanceDisplay.cs
// 余额显示组件 - 持久化显示版本
// 显示格式化的余额数字，支持货币符号和动画效果
// 特点：执行后UI依然可见，支持编辑器预览和持久显示
// 创建时间: 2025/6/26

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace BaccaratGame.UI.Components
{
    /// <summary>
    /// 余额显示组件 - 持久化显示版本
    /// 立即创建并持久显示UI，不依赖运行状态
    /// </summary>
    public class BalanceDisplay : MonoBehaviour
    {
        #region 序列化字段

        [Header("自动显示设置")]
        [SerializeField] private bool autoCreateAndShow = false;         // 自动创建并显示
        [SerializeField] private bool showOnAwake = false;               // 启动时显示
        [SerializeField] private bool immediateDisplay = false;          // 立即显示
        [SerializeField] private bool enableDebugMode = true;           // 启用调试模式

        [Header("UI组件引用")]
        [SerializeField] private Text balanceText;                      // 余额文字
        [SerializeField] private Text labelText;                        // 标签文字
        [SerializeField] private Image backgroundImage;                 // 背景图片
        [SerializeField] private Image iconImage;                       // 余额图标

        [Header("布局配置")]
        [SerializeField] private Vector2 panelSize = new Vector2(160, 70);          // 面板大小
        [SerializeField] private Vector2 panelPosition = new Vector2(-100, -100);   // 面板位置

        [Header("显示配置")]
        [SerializeField] private string labelText_str = "余额";                     // 标签文字
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);  // 背景颜色
        [SerializeField] private Color labelColor = Color.gray;                     // 标签颜色
        [SerializeField] private Color balanceColor = Color.white;                  // 余额颜色
        [SerializeField] private Color increaseColor = Color.green;                 // 增加时颜色
        [SerializeField] private Color decreaseColor = Color.red;                   // 减少时颜色
        [SerializeField] private Color warningColor = Color.gray;                 // 警告颜色
        [SerializeField] private int labelFontSize = 14;                            // 标签字体大小
        [SerializeField] private int balanceFontSize = 20;                          // 余额字体大小

        [Header("动画配置")]
        [SerializeField] private bool enableChangeAnimation = true;      // 启用变化动画
        [SerializeField] private bool enableNumberCountUp = true;        // 启用数字递增动画
        [SerializeField] private bool enableFlashEffect = true;          // 启用闪烁效果
        [SerializeField] private bool enablePulseEffect = true;          // 启用脉冲效果
        [SerializeField] private float animationDuration = 1f;          // 动画持续时间
        [SerializeField] private float countUpSpeed = 0.05f;            // 数字递增速度
        [SerializeField] private float pulseSpeed = 2f;                 // 脉冲速度

        [Header("余额配置")]
        [SerializeField] private decimal defaultBalance = 10000m;        // 默认余额
        [SerializeField] private decimal warningThreshold = 1000m;       // 余额警告阈值
        [SerializeField] private bool showCurrencySymbol = true;         // 显示货币符号
        [SerializeField] private string currencySymbol = "¥";            // 货币符号
        [SerializeField] private bool useSmartFormat = true;             // 使用智能格式化

        [Header("演示配置")]
        [SerializeField] private bool autoDemo = true;                   // 自动演示
        [SerializeField] private float demoInterval = 3f;               // 演示间隔

        #endregion

        #region 私有变量

        // UI对象引用
        private GameObject balancePanel;                 // 余额面板
        private RectTransform balanceRect;               // 面板RectTransform
        private Canvas parentCanvas;                     // 父Canvas

        // 状态变量
        private decimal currentBalance = 10000m;         // 当前余额
        private decimal previousBalance = 10000m;        // 之前余额
        private bool isAnimating = false;                // 是否正在动画
        private bool balanceUICreated = false;           // UI是否已创建
        private Vector3 originalScale;                   // 原始缩放

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            if (enableDebugMode)
                Debug.Log("[BalanceDisplay] Awake - 开始初始化");

            // 立即创建UI以确保持久显示
            if (autoCreateAndShow)
            {
                CreateAndShowBalanceDisplay();
            }
        }

        private void Start()
        {
            if (enableDebugMode)
                Debug.Log("[BalanceDisplay] Start - 开始设置");

            // 确保UI已创建
            if (!balanceUICreated && showOnAwake)
            {
                CreateAndShowBalanceDisplay();
            }

            // 设置默认余额
            SetDefaultBalance();

            // 开始演示
            if (autoDemo)
            {
                StartBalanceDemo();
            }
        }

        private void OnValidate()
        {
            // 在编辑器中实时更新
            if (Application.isPlaying && balanceUICreated)
            {
                UpdateDisplayProperties();
            }
        }

        #endregion

        #region UI创建

        /// <summary>
        /// 创建并显示余额显示器
        /// </summary>
        private void CreateAndShowBalanceDisplay()
        {
            if (balanceUICreated)
            {
                if (enableDebugMode)
                    Debug.Log("[BalanceDisplay] 余额UI已存在，跳过创建");
                return;
            }

            // 查找或创建Canvas
            FindOrCreateCanvas();

            // 创建余额面板
            CreateBalancePanel();

            // 创建UI元素
            CreateBalanceUI();

            balanceUICreated = true;

            if (enableDebugMode)
                Debug.Log("[BalanceDisplay] 余额UI创建并显示完成");
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
                GameObject canvasObj = new GameObject("BalanceDisplayCanvas");
                parentCanvas = canvasObj.AddComponent<Canvas>();
                parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                parentCanvas.sortingOrder = 100;

                // 添加CanvasScaler
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                // 添加GraphicRaycaster
                canvasObj.AddComponent<GraphicRaycaster>();

                if (enableDebugMode)
                    Debug.Log("[BalanceDisplay] 创建新Canvas用于余额显示");
            }
        }

        /// <summary>
        /// 创建余额面板
        /// </summary>
        private void CreateBalancePanel()
        {
            // 创建主面板
            balancePanel = new GameObject("BalancePanel");
            balancePanel.transform.SetParent(parentCanvas.transform);

            // 设置RectTransform
            balanceRect = balancePanel.AddComponent<RectTransform>();
            balanceRect.anchorMin = new Vector2(1, 1);         // 右上角锚点
            balanceRect.anchorMax = new Vector2(1, 1);
            balanceRect.sizeDelta = panelSize;
            balanceRect.anchoredPosition = panelPosition;

            originalScale = balanceRect.localScale;

            if (enableDebugMode)
                Debug.Log($"[BalanceDisplay] 余额面板已创建 - 大小:{panelSize}, 位置:{panelPosition}");
        }

        /// <summary>
        /// 创建余额UI元素
        /// </summary>
        private void CreateBalanceUI()
        {
            // 创建背景
            CreateBackground();

            // 创建余额图标（可选）
            if (showCurrencySymbol)
                CreateBalanceIcon();

            // 创建标签
            CreateLabel();

            // 创建余额文字
            CreateBalanceText();

            // 初始化显示
            UpdateDisplayProperties();
        }

        /// <summary>
        /// 创建背景
        /// </summary>
        private void CreateBackground()
        {
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(balancePanel.transform);

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
        /// 创建余额图标
        /// </summary>
        private void CreateBalanceIcon()
        {
            GameObject iconObj = new GameObject("BalanceIcon");
            iconObj.transform.SetParent(balancePanel.transform);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0.5f);
            iconRect.anchorMax = new Vector2(0.15f, 1f);
            iconRect.offsetMin = new Vector2(5, 2);
            iconRect.offsetMax = new Vector2(-2, -2);

            iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            iconImage.color = balanceColor;
            iconImage.type = Image.Type.Simple;
        }

        /// <summary>
        /// 创建标签
        /// </summary>
        private void CreateLabel()
        {
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(balancePanel.transform);

            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0.6f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(5, 2);
            labelRect.offsetMax = new Vector2(-5, -2);

            labelText = labelObj.AddComponent<Text>();
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = labelFontSize;
            labelText.color = labelColor;
            labelText.text = labelText_str;
            labelText.alignment = TextAnchor.LowerLeft;
        }

        /// <summary>
        /// 创建余额文字
        /// </summary>
        private void CreateBalanceText()
        {
            GameObject balanceObj = new GameObject("BalanceText");
            balanceObj.transform.SetParent(balancePanel.transform);

            RectTransform balanceTextRect = balanceObj.AddComponent<RectTransform>();
            balanceTextRect.anchorMin = new Vector2(0, 0f);
            balanceTextRect.anchorMax = new Vector2(1, 0.6f);
            balanceTextRect.offsetMin = new Vector2(5, 0);
            balanceTextRect.offsetMax = new Vector2(-5, 0);

            balanceText = balanceObj.AddComponent<Text>();
            balanceText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            balanceText.fontSize = balanceFontSize;
            balanceText.color = balanceColor;
            balanceText.text = FormatBalance(currentBalance);
            balanceText.alignment = TextAnchor.UpperLeft;
            balanceText.fontStyle = FontStyle.Bold;
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 设置余额
        /// </summary>
        /// <param name="balance">余额金额</param>
        /// <param name="useAnimation">是否使用动画</param>
        public void SetBalance(decimal balance, bool useAnimation = true)
        {
            // 确保UI已创建
            if (!balanceUICreated)
            {
                CreateAndShowBalanceDisplay();
            }

            previousBalance = currentBalance;
            currentBalance = balance;

            if (useAnimation && enableChangeAnimation && !isAnimating)
            {
                if (enableNumberCountUp && Mathf.Abs((float)(balance - previousBalance)) > 0.01f)
                {
                    StartCoroutine(AnimateBalanceCountUp(previousBalance, balance));
                }
                else
                {
                    StartCoroutine(AnimateBalanceChange());
                }
            }
            else
            {
                UpdateBalanceDisplay();
            }

            if (enableDebugMode)
                Debug.Log($"[BalanceDisplay] 余额更新: {previousBalance} → {balance}");
        }

        /// <summary>
        /// 增加余额
        /// </summary>
        /// <param name="amount">增加金额</param>
        /// <param name="useAnimation">是否使用动画</param>
        public void AddBalance(decimal amount, bool useAnimation = true)
        {
            SetBalance(currentBalance + amount, useAnimation);
        }

        /// <summary>
        /// 减少余额
        /// </summary>
        /// <param name="amount">减少金额</param>
        /// <param name="useAnimation">是否使用动画</param>
        public void SubtractBalance(decimal amount, bool useAnimation = true)
        {
            SetBalance(currentBalance - amount, useAnimation);
        }

        /// <summary>
        /// 获取当前余额
        /// </summary>
        public decimal GetCurrentBalance()
        {
            return currentBalance;
        }

        /// <summary>
        /// 设置标签文字
        /// </summary>
        /// <param name="text">标签文字</param>
        public void SetLabelText(string text)
        {
            labelText_str = text;
            if (labelText != null)
                labelText.text = text;

            if (enableDebugMode)
                Debug.Log($"[BalanceDisplay] 标签文字更新: {text}");
        }

        /// <summary>
        /// 检查余额是否足够
        /// </summary>
        /// <param name="amount">需要的金额</param>
        public bool HasSufficientBalance(decimal amount)
        {
            return currentBalance >= amount;
        }

        /// <summary>
        /// 显示/隐藏余额显示器
        /// </summary>
        /// <param name="show">是否显示</param>
        public void ShowBalanceDisplay(bool show)
        {
            if (balancePanel != null)
            {
                balancePanel.SetActive(show);

                if (enableDebugMode)
                    Debug.Log($"[BalanceDisplay] 显示状态: {(show ? "显示" : "隐藏")}");
            }
        }

        /// <summary>
        /// 清除显示
        /// </summary>
        public void ClearDisplay()
        {
            currentBalance = 0m;
            previousBalance = 0m;

            if (balanceText != null)
                balanceText.text = FormatBalance(0m);

            if (enableDebugMode)
                Debug.Log("[BalanceDisplay] 显示已清除");
        }

        #endregion

        #region 设置和配置

        /// <summary>
        /// 设置默认余额
        /// </summary>
        private void SetDefaultBalance()
        {
            if (defaultBalance > 0)
            {
                SetBalance(defaultBalance, false);
            }
        }

        /// <summary>
        /// 更新显示属性
        /// </summary>
        private void UpdateDisplayProperties()
        {
            if (!balanceUICreated) return;

            // 更新面板大小和位置
            if (balanceRect != null)
            {
                balanceRect.sizeDelta = panelSize;
                balanceRect.anchoredPosition = panelPosition;
            }

            // 更新背景颜色
            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
            }

            // 更新标签样式
            if (labelText != null)
            {
                labelText.fontSize = labelFontSize;
                labelText.color = labelColor;
                labelText.text = labelText_str;
            }

            // 更新余额样式
            if (balanceText != null)
            {
                balanceText.fontSize = balanceFontSize;
                balanceText.color = GetBalanceColor();
                balanceText.text = FormatBalance(currentBalance);
            }

            // 更新图标颜色
            if (iconImage != null)
            {
                iconImage.color = GetBalanceColor();
            }
        }

        /// <summary>
        /// 获取余额显示颜色
        /// </summary>
        private Color GetBalanceColor()
        {
            if (currentBalance <= warningThreshold)
                return warningColor;
            
            return balanceColor;
        }

        /// <summary>
        /// 设置位置
        /// </summary>
        public void SetPosition(Vector2 position)
        {
            panelPosition = position;
            if (balanceRect != null)
                balanceRect.anchoredPosition = position;
        }

        /// <summary>
        /// 设置大小
        /// </summary>
        public void SetSize(Vector2 size)
        {
            panelSize = size;
            if (balanceRect != null)
                balanceRect.sizeDelta = size;
        }

        /// <summary>
        /// 设置字体大小
        /// </summary>
        public void SetFontSize(int labelSize, int balanceSize)
        {
            labelFontSize = labelSize;
            balanceFontSize = balanceSize;

            if (labelText != null)
                labelText.fontSize = labelSize;
            if (balanceText != null)
                balanceText.fontSize = balanceSize;
        }

        #endregion

        #region 格式化和显示

        /// <summary>
        /// 更新余额显示
        /// </summary>
        private void UpdateBalanceDisplay()
        {
            if (balanceText != null)
            {
                balanceText.text = FormatBalance(currentBalance);
                balanceText.color = GetBalanceColor();
            }

            if (iconImage != null)
            {
                iconImage.color = GetBalanceColor();
            }
        }

        /// <summary>
        /// 格式化余额显示
        /// </summary>
        /// <param name="amount">金额</param>
        /// <returns>格式化后的字符串</returns>
        private string FormatBalance(decimal amount)
        {
            string formattedAmount = "";

            if (useSmartFormat)
            {
                // 智能格式化：超过万元显示为万元单位
                if (amount >= 10000)
                {
                    formattedAmount = $"{(amount / 10000m):F1}万";
                }
                else if (amount >= 1000)
                {
                    formattedAmount = $"{(amount / 1000m):F1}K";
                }
                else
                {
                    formattedAmount = $"{amount:F0}";
                }
            }
            else
            {
                // 标准格式化
                formattedAmount = amount.ToString("N0");
            }

            // 添加货币符号
            if (showCurrencySymbol)
            {
                return currencySymbol + formattedAmount;
            }

            return formattedAmount;
        }

        #endregion

        #region 动画效果

        /// <summary>
        /// 余额变化动画
        /// </summary>
        private IEnumerator AnimateBalanceChange()
        {
            if (balanceText == null) yield break;

            isAnimating = true;
            decimal difference = currentBalance - previousBalance;

            // 设置颜色表示增减
            Color targetColor = difference > 0 ? increaseColor : (difference < 0 ? decreaseColor : balanceColor);
            Color originalColor = GetBalanceColor();

            // 闪烁效果
            if (enableFlashEffect)
            {
                float elapsed = 0f;
                while (elapsed < animationDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / animationDuration;

                    // 颜色插值
                    if (t < 0.5f)
                    {
                        // 前半段：变为目标颜色
                        balanceText.color = Color.Lerp(originalColor, targetColor, t * 2f);
                    }
                    else
                    {
                        // 后半段：回到原色
                        balanceText.color = Color.Lerp(targetColor, originalColor, (t - 0.5f) * 2f);
                    }

                    yield return null;
                }
            }

            // 确保回到正确颜色
            balanceText.color = GetBalanceColor();
            UpdateBalanceDisplay();
            isAnimating = false;
        }

        /// <summary>
        /// 余额数字递增动画
        /// </summary>
        private IEnumerator AnimateBalanceCountUp(decimal fromAmount, decimal toAmount)
        {
            if (balanceText == null) yield break;

            isAnimating = true;
            decimal difference = toAmount - fromAmount;
            decimal step = difference / (decimal)(animationDuration / countUpSpeed);

            decimal currentAmount = fromAmount;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += countUpSpeed;
                currentAmount += step;

                // 确保不超过目标值
                if ((difference > 0 && currentAmount > toAmount) || (difference < 0 && currentAmount < toAmount))
                {
                    currentAmount = toAmount;
                }

                balanceText.text = FormatBalance(currentAmount);
                yield return new WaitForSeconds(countUpSpeed);

                if (Mathf.Abs((float)(currentAmount - toAmount)) < 0.01f)
                    break;
            }

            // 确保显示最终值
            balanceText.text = FormatBalance(toAmount);
            balanceText.color = GetBalanceColor();
            isAnimating = false;
        }

        /// <summary>
        /// 脉冲效果
        /// </summary>
        private IEnumerator PulseEffect()
        {
            if (balanceRect == null) yield break;

            while (enablePulseEffect && balancePanel != null && balancePanel.activeInHierarchy)
            {
                float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.05f;
                balanceRect.localScale = originalScale * scale;
                yield return null;
            }

            if (balanceRect != null)
                balanceRect.localScale = originalScale;
        }

        #endregion

        #region 演示和测试

        /// <summary>
        /// 开始余额演示
        /// </summary>
        private void StartBalanceDemo()
        {
            if (autoDemo)
            {
                StartCoroutine(BalanceDemoCoroutine());
            }
        }

        /// <summary>
        /// 余额演示协程
        /// </summary>
        private IEnumerator BalanceDemoCoroutine()
        {
            yield return new WaitForSeconds(2f);

            // 模拟投注减少余额
            SubtractBalance(500m);
            yield return new WaitForSeconds(demoInterval);

            // 模拟中奖增加余额
            AddBalance(1200m);
            yield return new WaitForSeconds(demoInterval);

            // 继续演示
            SubtractBalance(300m);
            yield return new WaitForSeconds(demoInterval);

            AddBalance(800m);
            yield return new WaitForSeconds(demoInterval);

            // 测试大额变化
            AddBalance(15000m);
            yield return new WaitForSeconds(demoInterval);

            SubtractBalance(8000m);
            yield return new WaitForSeconds(demoInterval);

            // 重复演示
            if (autoDemo)
            {
                StartBalanceDemo();
            }
        }

        #endregion

        #region 调试方法

        /// <summary>
        /// 强制显示余额显示器
        /// </summary>
        [ContextMenu("强制显示余额显示器")]
        public void ForceShowBalanceDisplay()
        {
            balanceUICreated = false;
            CreateAndShowBalanceDisplay();
        }

        /// <summary>
        /// 显示组件状态
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowComponentStatus()
        {
            Debug.Log("=== BalanceDisplay 组件状态 ===");
            Debug.Log($"自动创建并显示: {autoCreateAndShow}");
            Debug.Log($"启动时显示: {showOnAwake}");
            Debug.Log($"立即显示: {immediateDisplay}");
            Debug.Log($"余额UI已创建: {balanceUICreated}");
            Debug.Log($"父Canvas: {(parentCanvas != null ? "✓" : "✗")}");
            Debug.Log($"余额面板: {(balancePanel != null ? "✓" : "✗")} - {(balancePanel?.activeInHierarchy == true ? "显示" : "隐藏")}");
            Debug.Log($"余额文字: {(balanceText != null ? "✓" : "✗")}");
            Debug.Log($"标签文字: {(labelText != null ? "✓" : "✗")}");
            Debug.Log($"背景图片: {(backgroundImage != null ? "✓" : "✗")}");
            Debug.Log($"当前余额: {currentBalance:C}");
            Debug.Log($"格式化显示: {FormatBalance(currentBalance)}");
        }

        /// <summary>
        /// 测试余额增加
        /// </summary>
        [ContextMenu("测试余额增加")]
        public void TestAddBalance()
        {
            decimal amount = (decimal)(UnityEngine.Random.Range(100, 2000));
            AddBalance(amount);
            Debug.Log($"[BalanceDisplay] 测试增加余额: +{amount}");
        }

        /// <summary>
        /// 测试余额减少
        /// </summary>
        [ContextMenu("测试余额减少")]
        public void TestSubtractBalance()
        {
            decimal amount = (decimal)(UnityEngine.Random.Range(50, 1000));
            SubtractBalance(amount);
            Debug.Log($"[BalanceDisplay] 测试减少余额: -{amount}");
        }

        /// <summary>
        /// 测试大额余额
        /// </summary>
        [ContextMenu("测试大额余额")]
        public void TestLargeBalance()
        {
            decimal largeAmount = (decimal)(UnityEngine.Random.Range(50000, 500000));
            SetBalance(largeAmount);
            Debug.Log($"[BalanceDisplay] 测试大额余额: {largeAmount}");
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

        /// <summary>
        /// 测试余额警告
        /// </summary>
        [ContextMenu("测试余额警告")]
        public void TestLowBalance()
        {
            SetBalance(warningThreshold - 100m);
            Debug.Log($"[BalanceDisplay] 测试低余额警告: {warningThreshold - 100m}");
        }

        #endregion
    }
}
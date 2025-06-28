// Assets/UI/Components/VideoOverlay/Set/ButtonList.cs
// 功能按钮列表组件 - 启动即显示版本
// 自动创建并立即显示按钮UI
// 创建时间: 2025/6/26

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using BaccaratGame.Core.Events;
using BaccaratGame.Data;

namespace BaccaratGame.UI.Components.VideoOverlay
{
    /// <summary>
    /// 功能按钮列表组件 - 启动即显示版本
    /// 组件启动时立即创建并显示所有按钮
    /// </summary>
    public class ButtonList : MonoBehaviour
    {
        #region 序列化字段

        [Header("自动显示设置")]
        public bool autoCreateAndShow = false;
        public bool showOnAwake = false;
        public bool immediateDisplay = false;
        
        [Header("按钮布局")]
        public Vector2 buttonSize = new Vector2(120, 40);
        public float buttonSpacing = 15f;
        public Vector2 startPosition = new Vector2(-250, -150);
        public bool horizontalLayout = true;
        
        [Header("按钮样式")]
        public Color buttonColor = new Color(0.2f, 0.6f, 1f, 1f);
        public Color textColor = Color.white;
        public Color hoverColor = new Color(0.3f, 0.7f, 1f, 1f);
        public int fontSize = 14;

        [Header("现有按钮引用 (可选)")]
        public Button historyButton;
        public Button rechargeButton;
        public Button withdrawButton;
        public Button customerServiceButton;
        public Button backButton;

        [Header("组件引用")]
        public BaccaratGame.UI.Components.HistoryPanel historyPanel;

        [Header("链接配置")]
        public string rechargeUrl = "https://example.com/recharge";
        public string withdrawUrl = "https://example.com/withdraw";
        public string customerServiceUrl = "https://example.com/service";
        public string backUrl = "https://www.google.com";

        [Header("调试设置")]
        public bool enableDebugMode = true;

        #endregion

        #region 私有字段

        private RectTransform rectTransform;
        private Canvas parentCanvas;
        private bool buttonsCreated = false;

        #endregion

        #region 事件定义

        [Header("事件回调")]
        public UnityEvent OnHistoryClicked;
        public UnityEvent OnRechargeClicked;
        public UnityEvent OnWithdrawClicked;
        public UnityEvent OnCustomerServiceClicked;
        public UnityEvent OnBackClicked;

        #endregion

        #region 生命周期

        private void Awake()
        {
            InitializeComponent();
            
            if (showOnAwake)
            {
                CreateAndShowButtons();
            }
        }

        private void Start()
        {
            if (!buttonsCreated && autoCreateAndShow)
            {
                CreateAndShowButtons();
            }
            
            SetupExistingButtons();
        }

        private void OnValidate()
        {
            // 在编辑器中实时预览
            if (Application.isEditor && !Application.isPlaying)
            {
                if (immediateDisplay)
                {
                    InitializeComponent();
                    CreateAndShowButtons();
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
                // 尝试创建Canvas
                CreateCanvasIfNeeded();
            }

            if (enableDebugMode)
                Debug.Log("[ButtonList] 组件初始化完成");
        }

        /// <summary>
        /// 如需要则创建Canvas
        /// </summary>
        private void CreateCanvasIfNeeded()
        {
            GameObject canvasObj = new GameObject("ButtonListCanvas");
            canvasObj.transform.SetParent(transform.parent);
            
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
            
            // 将ButtonList移到Canvas下
            transform.SetParent(canvasObj.transform);
            
            parentCanvas = canvas;
            
            if (enableDebugMode)
                Debug.Log("[ButtonList] 创建了新的Canvas");
        }

        #endregion

        #region 按钮创建

        /// <summary>
        /// 创建并显示所有按钮
        /// </summary>
        [ContextMenu("创建并显示按钮")]
        public void CreateAndShowButtons()
        {
            if (buttonsCreated) return;

            try
            {
                // 确保组件已初始化
                if (rectTransform == null)
                    InitializeComponent();

                // 创建按钮数据
                var buttonData = new[]
                {
                    new { name = "history", text = "历史记录", url = "", color = new Color(0.5f, 0.7f, 1f, 1f) },
                    new { name = "recharge", text = "充值", url = rechargeUrl, color = new Color(0.2f, 0.8f, 0.2f, 1f) },
                    new { name = "withdraw", text = "提现", url = withdrawUrl, color = new Color(0.8f, 0.4f, 0.2f, 1f) },
                    new { name = "customerservice", text = "客服", url = customerServiceUrl, color = new Color(0.6f, 0.2f, 0.8f, 1f) },
                    new { name = "back", text = "返回", url = backUrl, color = new Color(0.8f, 0.2f, 0.2f, 1f) }
                };

                // 创建每个按钮
                for (int i = 0; i < buttonData.Length; i++)
                {
                    var data = buttonData[i];
                    Vector2 buttonPos = CalculateButtonPosition(i);
                    Button button = CreateSingleButton(data.name, data.text, data.url, data.color, buttonPos);
                    
                    // 分配到对应的引用
                    AssignButtonReference(data.name, button);
                }

                buttonsCreated = true;
                
                if (enableDebugMode)
                    Debug.Log("[ButtonList] 所有按钮创建完成并已显示");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ButtonList] 创建按钮时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 计算按钮位置
        /// </summary>
        private Vector2 CalculateButtonPosition(int index)
        {
            if (horizontalLayout)
            {
                float x = startPosition.x + (index * (buttonSize.x + buttonSpacing));
                return new Vector2(x, startPosition.y);
            }
            else
            {
                float y = startPosition.y - (index * (buttonSize.y + buttonSpacing));
                return new Vector2(startPosition.x, y);
            }
        }

        /// <summary>
        /// 创建单个按钮
        /// </summary>
        private Button CreateSingleButton(string buttonName, string displayText, string url, Color btnColor, Vector2 position)
        {
            // 创建按钮GameObject
            GameObject buttonObj = new GameObject(buttonName + "Button");
            buttonObj.transform.SetParent(transform);

            // 设置RectTransform
            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = buttonSize;
            buttonRect.anchoredPosition = position;
            buttonRect.localScale = Vector3.one;

            // 添加背景图片
            Image backgroundImage = buttonObj.AddComponent<Image>();
            backgroundImage.color = btnColor;
            backgroundImage.sprite = CreateButtonSprite();

            // 添加Button组件
            Button button = buttonObj.AddComponent<Button>();
            
            // 设置按钮颜色状态
            ColorBlock colors = button.colors;
            colors.normalColor = btnColor;
            colors.highlightedColor = hoverColor;
            colors.pressedColor = new Color(btnColor.r * 0.8f, btnColor.g * 0.8f, btnColor.b * 0.8f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            button.colors = colors;

            // 创建文本
            CreateButtonText(buttonObj, displayText);

            // 设置按钮点击事件
            button.onClick.AddListener(() => HandleButtonClick(buttonName, url));

            // 立即激活显示
            buttonObj.SetActive(true);

            if (enableDebugMode)
                Debug.Log($"[ButtonList] 创建按钮: {buttonName} 位置: {position}");

            return button;
        }

        /// <summary>
        /// 创建按钮Sprite
        /// </summary>
        private Sprite CreateButtonSprite()
        {
            // 创建简单的按钮背景纹理
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// 创建按钮文本
        /// </summary>
        private void CreateButtonText(GameObject buttonObj, string displayText)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textRect.localScale = Vector3.one;

            Text text = textObj.AddComponent<Text>();
            text.text = displayText;
            text.color = textColor;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
        }

        /// <summary>
        /// 分配按钮引用
        /// </summary>
        private void AssignButtonReference(string buttonName, Button button)
        {
            switch (buttonName.ToLower())
            {
                case "history":
                    if (historyButton == null) historyButton = button;
                    break;
                case "recharge":
                    if (rechargeButton == null) rechargeButton = button;
                    break;
                case "withdraw":
                    if (withdrawButton == null) withdrawButton = button;
                    break;
                case "customerservice":
                    if (customerServiceButton == null) customerServiceButton = button;
                    break;
                case "back":
                    if (backButton == null) backButton = button;
                    break;
            }
        }

        #endregion

        #region 现有按钮设置

        /// <summary>
        /// 设置现有按钮
        /// </summary>
        private void SetupExistingButtons()
        {
            // 查找现有按钮（如果没有手动设置且没有自动创建）
            if (!buttonsCreated)
            {
                if (historyButton == null) historyButton = FindButtonByName("HistoryButton", "History");
                if (rechargeButton == null) rechargeButton = FindButtonByName("RechargeButton", "Recharge");
                if (withdrawButton == null) withdrawButton = FindButtonByName("WithdrawButton", "Withdraw");
                if (customerServiceButton == null) customerServiceButton = FindButtonByName("CustomerServiceButton", "CustomerService");
                if (backButton == null) backButton = FindButtonByName("BackButton", "Back");
            }

            // 设置按钮回调
            if (historyButton != null)
            {
                historyButton.onClick.RemoveAllListeners();
                historyButton.onClick.AddListener(() => HandleButtonClick("history", ""));
            }

            if (rechargeButton != null)
            {
                rechargeButton.onClick.RemoveAllListeners();
                rechargeButton.onClick.AddListener(() => HandleButtonClick("recharge", rechargeUrl));
            }

            if (withdrawButton != null)
            {
                withdrawButton.onClick.RemoveAllListeners();
                withdrawButton.onClick.AddListener(() => HandleButtonClick("withdraw", withdrawUrl));
            }

            if (customerServiceButton != null)
            {
                customerServiceButton.onClick.RemoveAllListeners();
                customerServiceButton.onClick.AddListener(() => HandleButtonClick("customerservice", customerServiceUrl));
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(() => HandleButtonClick("back", backUrl));
            }

            // 查找历史面板组件
            if (historyPanel == null)
                historyPanel = FindFirstObjectByType<BaccaratGame.UI.Components.HistoryPanel>();

            if (enableDebugMode)
                Debug.Log("[ButtonList] 现有按钮设置完成");
        }

        /// <summary>
        /// 按名称查找按钮
        /// </summary>
        private Button FindButtonByName(params string[] names)
        {
            foreach (string name in names)
            {
                Transform found = transform.Find(name);
                if (found != null)
                {
                    Button button = found.GetComponent<Button>();
                    if (button != null)
                        return button;
                }
            }
            return null;
        }

        #endregion

        #region 按钮处理

        /// <summary>
        /// 处理按钮点击
        /// </summary>
        private void HandleButtonClick(string buttonName, string url)
        {
            if (enableDebugMode)
                Debug.Log($"[ButtonList] 按钮点击: {buttonName}");

            switch (buttonName.ToLower())
            {
                case "history":
                    HandleHistoryClick();
                    break;
                case "recharge":
                    HandleRechargeClick(url);
                    break;
                case "withdraw":
                    HandleWithdrawClick(url);
                    break;
                case "customerservice":
                    HandleCustomerServiceClick(url);
                    break;
                case "back":
                    HandleBackClick(url);
                    break;
            }
        }

        private void HandleHistoryClick()
        {
            OnHistoryClicked?.Invoke();

            if (historyPanel != null)
            {
                historyPanel.OpenPanel();
            }
            else
            {
                UIEvents.TriggerPanelShown(UIPanel.BetHistory);
            }

            if (enableDebugMode)
                Debug.Log("[ButtonList] 显示历史记录");
        }

        private void HandleRechargeClick(string url)
        {
            OnRechargeClicked?.Invoke();
            OpenUrl(url, "充值");
        }

        private void HandleWithdrawClick(string url)
        {
            OnWithdrawClicked?.Invoke();
            OpenUrl(url, "提现");
        }

        private void HandleCustomerServiceClick(string url)
        {
            OnCustomerServiceClicked?.Invoke();
            OpenUrl(url, "客服");
        }

        private void HandleBackClick(string url)
        {
            OnBackClicked?.Invoke();
            OpenUrl(url, "返回");
        }

        /// <summary>
        /// 打开URL
        /// </summary>
        private void OpenUrl(string url, string description)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    Debug.LogWarning($"[ButtonList] {description}链接为空");
                    return;
                }

                Application.OpenURL(url);
                
                if (enableDebugMode)
                    Debug.Log($"[ButtonList] 打开{description}链接: {url}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ButtonList] 打开{description}链接失败: {ex.Message}");
            }
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 强制显示按钮
        /// </summary>
        [ContextMenu("强制显示按钮")]
        public void ForceShowButtons()
        {
            buttonsCreated = false;
            CreateAndShowButtons();
        }

        /// <summary>
        /// 设置按钮可见性
        /// </summary>
        public void SetButtonVisible(string buttonName, bool visible)
        {
            Button button = GetButtonByName(buttonName);
            if (button != null)
            {
                button.gameObject.SetActive(visible);
            }
        }

        /// <summary>
        /// 设置按钮启用状态
        /// </summary>
        public void SetButtonEnabled(string buttonName, bool enabled)
        {
            Button button = GetButtonByName(buttonName);
            if (button != null)
            {
                button.interactable = enabled;
            }
        }

        /// <summary>
        /// 根据名称获取按钮
        /// </summary>
        private Button GetButtonByName(string buttonName)
        {
            switch (buttonName.ToLower())
            {
                case "history": return historyButton;
                case "recharge": return rechargeButton;
                case "withdraw": return withdrawButton;
                case "customerservice": return customerServiceButton;
                case "back": return backButton;
                default: return null;
            }
        }

        /// <summary>
        /// 更新链接配置
        /// </summary>
        public void UpdateUrls(string newRechargeUrl, string newWithdrawUrl, string newCustomerServiceUrl, string newBackUrl)
        {
            rechargeUrl = newRechargeUrl;
            withdrawUrl = newWithdrawUrl;
            customerServiceUrl = newCustomerServiceUrl;
            backUrl = newBackUrl;

            if (enableDebugMode)
                Debug.Log("[ButtonList] 链接配置已更新");
        }

        /// <summary>
        /// 重新定位所有按钮
        /// </summary>
        [ContextMenu("重新定位按钮")]
        public void RepositionButtons()
        {
            var buttons = new[] { historyButton, rechargeButton, withdrawButton, customerServiceButton, backButton };
            
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null)
                {
                    RectTransform buttonRect = buttons[i].GetComponent<RectTransform>();
                    if (buttonRect != null)
                    {
                        buttonRect.anchoredPosition = CalculateButtonPosition(i);
                    }
                }
            }
        }

        #endregion

        #region 调试方法

        /// <summary>
        /// 显示组件状态
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowComponentStatus()
        {
            Debug.Log("=== ButtonList 组件状态 ===");
            Debug.Log($"自动创建并显示: {autoCreateAndShow}");
            Debug.Log($"启动时显示: {showOnAwake}");
            Debug.Log($"立即显示: {immediateDisplay}");
            Debug.Log($"按钮已创建: {buttonsCreated}");
            Debug.Log($"父Canvas: {(parentCanvas != null ? "✓" : "✗")}");
            Debug.Log($"历史记录按钮: {(historyButton != null ? "✓" : "✗")} - {(historyButton?.gameObject.activeInHierarchy == true ? "显示" : "隐藏")}");
            Debug.Log($"充值按钮: {(rechargeButton != null ? "✓" : "✗")} - {(rechargeButton?.gameObject.activeInHierarchy == true ? "显示" : "隐藏")}");
            Debug.Log($"提现按钮: {(withdrawButton != null ? "✓" : "✗")} - {(withdrawButton?.gameObject.activeInHierarchy == true ? "显示" : "隐藏")}");
            Debug.Log($"客服按钮: {(customerServiceButton != null ? "✓" : "✗")} - {(customerServiceButton?.gameObject.activeInHierarchy == true ? "显示" : "隐藏")}");
            Debug.Log($"返回按钮: {(backButton != null ? "✓" : "✗")} - {(backButton?.gameObject.activeInHierarchy == true ? "显示" : "隐藏")}");
            Debug.Log($"按钮布局: {(horizontalLayout ? "水平" : "垂直")}");
            Debug.Log($"起始位置: {startPosition}");
        }

        /// <summary>
        /// 测试所有按钮
        /// </summary>
        [ContextMenu("测试所有按钮")]
        public void TestAllButtons()
        {
            Debug.Log("[ButtonList] 开始测试所有按钮");
            
            if (historyButton != null) HandleButtonClick("history", "");
            if (rechargeButton != null) HandleButtonClick("recharge", rechargeUrl);
            if (withdrawButton != null) HandleButtonClick("withdraw", withdrawUrl);
            if (customerServiceButton != null) HandleButtonClick("customerservice", customerServiceUrl);
            if (backButton != null) HandleButtonClick("back", backUrl);
            
            Debug.Log("[ButtonList] 按钮测试完成");
        }

        /// <summary>
        /// 删除所有创建的按钮
        /// </summary>
        [ContextMenu("删除所有按钮")]
        public void ClearAllButtons()
        {
            var buttons = new[] { historyButton, rechargeButton, withdrawButton, customerServiceButton, backButton };
            
            foreach (var button in buttons)
            {
                if (button != null && button.transform.parent == transform)
                {
                    if (Application.isPlaying)
                        Destroy(button.gameObject);
                    else
                        DestroyImmediate(button.gameObject);
                }
            }
            
            historyButton = null;
            rechargeButton = null;
            withdrawButton = null;
            customerServiceButton = null;
            backButton = null;
            
            buttonsCreated = false;
            
            Debug.Log("[ButtonList] 所有按钮已删除");
        }

        #endregion
    }
}
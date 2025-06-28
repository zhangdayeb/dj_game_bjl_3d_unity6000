// Assets/UI/Components/VideoOverlay/Set/WinEffect.cs
// 简洁优雅的中奖展示UI
// 挂载到节点上自动创建美观的中奖展示界面
// 创建时间: 2025/6/28

using UnityEngine;
using UnityEngine.UI;

namespace BaccaratGame.UI.Components.VideoOverlay
{
    /// <summary>
    /// 简洁优雅的中奖展示UI组件
    /// 专注于美观的页面布局，无复杂特效
    /// </summary>
    public class WinEffect : MonoBehaviour
    {
        #region 配置参数

        [Header("面板配置")]
        public Vector2 panelSize = new Vector2(480, 320);
        public Color panelBackgroundColor = new Color(0.12f, 0.12f, 0.15f, 0.95f);
        public Color accentColor = new Color(1f, 0.84f, 0.2f, 1f); // 金色
        
        [Header("遮罩层设置")]
        public Color maskColor = new Color(0, 0, 0, 0.6f);
        
        [Header("文字配置")]
        public Color primaryTextColor = Color.white;
        public Color secondaryTextColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        public int baseFontSize = 18;

        #endregion

        #region 私有字段

        private bool uiCreated = false;
        private GameObject maskLayer;
        private GameObject winPanel;
        private Canvas uiCanvas;
        
        // UI组件引用
        private Text winAmountText;
        private Text congratsText;
        private Button continueButton;
        private Button closeButton;

        #endregion

        #region 生命周期

        private void Awake()
        {
            CreateUI();
            ShowSampleWin();
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
            CreateWinPanel();
            CreateWinContent();
            
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
                GameObject canvasObj = new GameObject("WinEffectCanvas");
                canvasObj.transform.SetParent(transform.parent);
                
                uiCanvas = canvasObj.AddComponent<Canvas>();
                uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                uiCanvas.sortingOrder = 5000;
                
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
        /// 创建中奖面板
        /// </summary>
        private void CreateWinPanel()
        {
            winPanel = new GameObject("WinPanel");
            winPanel.transform.SetParent(transform);

            RectTransform panelRect = winPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = panelSize;
            panelRect.anchoredPosition = Vector2.zero;

            // 主背景
            Image panelBg = winPanel.AddComponent<Image>();
            panelBg.color = panelBackgroundColor;
            panelBg.sprite = CreateRoundedSprite();

            // 顶部装饰条
            CreateTopAccent();
            
            // 添加微妙阴影
            Shadow shadow = winPanel.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            shadow.effectDistance = new Vector2(0, -4);
        }

        /// <summary>
        /// 创建顶部装饰条
        /// </summary>
        private void CreateTopAccent()
        {
            GameObject accentObj = new GameObject("TopAccent");
            accentObj.transform.SetParent(winPanel.transform);

            RectTransform accentRect = accentObj.AddComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0, 0.85f);
            accentRect.anchorMax = new Vector2(1, 0.95f);
            accentRect.offsetMin = Vector2.zero;
            accentRect.offsetMax = Vector2.zero;

            Image accentImage = accentObj.AddComponent<Image>();
            accentImage.color = accentColor;
            accentImage.sprite = CreateSimpleSprite();
        }

        /// <summary>
        /// 创建中奖内容
        /// </summary>
        private void CreateWinContent()
        {
            CreateCongratulationsText();
            CreateWinAmountDisplay();
            CreateBottomButtons();
        }

        /// <summary>
        /// 创建祝贺文字
        /// </summary>
        private void CreateCongratulationsText()
        {
            GameObject congratsObj = new GameObject("CongratulationsText");
            congratsObj.transform.SetParent(winPanel.transform);

            RectTransform congratsRect = congratsObj.AddComponent<RectTransform>();
            congratsRect.anchorMin = new Vector2(0.1f, 0.65f);
            congratsRect.anchorMax = new Vector2(0.9f, 0.85f);
            congratsRect.offsetMin = Vector2.zero;
            congratsRect.offsetMax = Vector2.zero;

            congratsText = congratsObj.AddComponent<Text>();
            congratsText.text = "恭喜中奖！";
            congratsText.color = primaryTextColor;
            congratsText.alignment = TextAnchor.MiddleCenter;
            congratsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            congratsText.fontSize = baseFontSize + 6;
            congratsText.fontStyle = FontStyle.Bold;

            // 添加文字阴影
            Shadow textShadow = congratsObj.AddComponent<Shadow>();
            textShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            textShadow.effectDistance = new Vector2(1, -1);
        }

        /// <summary>
        /// 创建中奖金额显示
        /// </summary>
        private void CreateWinAmountDisplay()
        {
            // 金额背景框
            GameObject amountBgObj = new GameObject("AmountBackground");
            amountBgObj.transform.SetParent(winPanel.transform);

            RectTransform amountBgRect = amountBgObj.AddComponent<RectTransform>();
            amountBgRect.anchorMin = new Vector2(0.15f, 0.35f);
            amountBgRect.anchorMax = new Vector2(0.85f, 0.6f);
            amountBgRect.offsetMin = Vector2.zero;
            amountBgRect.offsetMax = Vector2.zero;

            Image amountBgImage = amountBgObj.AddComponent<Image>();
            amountBgImage.color = new Color(0.08f, 0.08f, 0.1f, 0.8f);
            amountBgImage.sprite = CreateRoundedSprite();

            // 金额文字
            GameObject amountObj = new GameObject("WinAmount");
            amountObj.transform.SetParent(amountBgObj.transform);

            RectTransform amountRect = amountObj.AddComponent<RectTransform>();
            amountRect.anchorMin = Vector2.zero;
            amountRect.anchorMax = Vector2.one;
            amountRect.offsetMin = new Vector2(10, 0);
            amountRect.offsetMax = new Vector2(-10, 0);

            winAmountText = amountObj.AddComponent<Text>();
            winAmountText.text = "¥ 88,888";
            winAmountText.color = accentColor;
            winAmountText.alignment = TextAnchor.MiddleCenter;
            winAmountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            winAmountText.fontSize = baseFontSize + 14;
            winAmountText.fontStyle = FontStyle.Bold;

            // 金额文字发光效果
            Outline amountOutline = amountObj.AddComponent<Outline>();
            amountOutline.effectColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.3f);
            amountOutline.effectDistance = new Vector2(2, -2);
        }

        /// <summary>
        /// 创建底部按钮
        /// </summary>
        private void CreateBottomButtons()
        {
            // 按钮容器
            GameObject buttonsContainer = new GameObject("ButtonsContainer");
            buttonsContainer.transform.SetParent(winPanel.transform);

            RectTransform buttonsRect = buttonsContainer.AddComponent<RectTransform>();
            buttonsRect.anchorMin = new Vector2(0.1f, 0.08f);
            buttonsRect.anchorMax = new Vector2(0.9f, 0.28f);
            buttonsRect.offsetMin = Vector2.zero;
            buttonsRect.offsetMax = Vector2.zero;

            // 水平布局组
            HorizontalLayoutGroup layoutGroup = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 20;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = true;

            // 继续按钮
            continueButton = CreateStyledButton(buttonsContainer, "继续游戏", accentColor, primaryTextColor);
            continueButton.onClick.AddListener(() => {
                HidePanel();
                Debug.Log("[WinEffect] 继续游戏");
            });

            // 关闭按钮
            closeButton = CreateStyledButton(buttonsContainer, "关闭", 
                new Color(0.3f, 0.3f, 0.3f, 1f), secondaryTextColor);
            closeButton.onClick.AddListener(() => {
                HidePanel();
                Debug.Log("[WinEffect] 关闭面板");
            });
        }

        /// <summary>
        /// 创建样式化按钮
        /// </summary>
        private Button CreateStyledButton(GameObject parent, string text, Color bgColor, Color textColor)
        {
            GameObject buttonObj = new GameObject($"Button_{text}");
            buttonObj.transform.SetParent(parent.transform);

            Button button = buttonObj.AddComponent<Button>();
            
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = bgColor;
            buttonImage.sprite = CreateRoundedSprite();

            // 按钮状态颜色
            ColorBlock colors = button.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = new Color(bgColor.r * 1.1f, bgColor.g * 1.1f, bgColor.b * 1.1f, bgColor.a);
            colors.pressedColor = new Color(bgColor.r * 0.9f, bgColor.g * 0.9f, bgColor.b * 0.9f, bgColor.a);
            colors.disabledColor = new Color(bgColor.r * 0.5f, bgColor.g * 0.5f, bgColor.b * 0.5f, bgColor.a);
            button.colors = colors;

            // 按钮文字
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = text;
            buttonText.color = textColor;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = baseFontSize;
            buttonText.fontStyle = FontStyle.Bold;

            return button;
        }

        /// <summary>
        /// 创建简单背景Sprite
        /// </summary>
        private Sprite CreateSimpleSprite()
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// 创建圆角背景Sprite（简化版）
        /// </summary>
        private Sprite CreateRoundedSprite()
        {
            // 简化实现，实际项目中可以使用更复杂的圆角纹理
            return CreateSimpleSprite();
        }

        #endregion

        #region 中奖展示逻辑

        /// <summary>
        /// 显示示例中奖
        /// </summary>
        private void ShowSampleWin()
        {
            UpdateWinDisplay(88888);
        }

        /// <summary>
        /// 更新中奖显示
        /// </summary>
        private void UpdateWinDisplay(int amount)
        {
            if (winAmountText != null)
            {
                winAmountText.text = FormatWinAmount(amount);
            }

            if (congratsText != null)
            {
                congratsText.text = "恭喜中奖！";
            }

            Debug.Log($"[WinEffect] 显示中奖金额: ¥{amount}");
        }

        /// <summary>
        /// 格式化中奖金额
        /// </summary>
        private string FormatWinAmount(int amount)
        {
            if (amount >= 10000)
                return $"¥ {amount / 10000:F1}万";
            else if (amount >= 1000)
                return $"¥ {amount:N0}";
            else
                return $"¥ {amount}";
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void HidePanel()
        {
            gameObject.SetActive(false);
            Debug.Log("[WinEffect] 面板已隐藏");
        }

        /// <summary>
        /// 显示面板
        /// </summary>
        public void ShowPanel()
        {
            gameObject.SetActive(true);
            Debug.Log("[WinEffect] 面板已显示");
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 显示中奖效果
        /// </summary>
        public void ShowWinEffect(int winAmount)
        {
            UpdateWinDisplay(winAmount);
            ShowPanel();
        }

        /// <summary>
        /// 切换面板显示状态
        /// </summary>
        public void TogglePanel()
        {
            if (gameObject.activeInHierarchy)
                HidePanel();
            else
                ShowPanel();
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
            ShowSampleWin();
        }

        /// <summary>
        /// 测试不同金额
        /// </summary>
        [ContextMenu("测试小额中奖")]
        public void TestSmallWin()
        {
            ShowWinEffect(288);
        }

        [ContextMenu("测试中等中奖")]
        public void TestMediumWin()
        {
            ShowWinEffect(8888);
        }

        [ContextMenu("测试大额中奖")]
        public void TestBigWin()
        {
            ShowWinEffect(66666);
        }

        /// <summary>
        /// 显示组件状态
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowStatus()
        {
            Debug.Log($"[WinEffect] UI已创建: {uiCreated}");
            Debug.Log($"[WinEffect] 遮罩层: {(maskLayer != null ? "✓" : "✗")}");
            Debug.Log($"[WinEffect] 中奖面板: {(winPanel != null ? "✓" : "✗")}");
            Debug.Log($"[WinEffect] 当前状态: {(gameObject.activeInHierarchy ? "显示" : "隐藏")}");
        }

        #endregion
    }
}
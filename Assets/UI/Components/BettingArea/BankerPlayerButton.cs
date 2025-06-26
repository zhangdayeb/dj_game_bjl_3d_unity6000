// Assets/UI/Components/BettingArea/BankerPlayerButton.cs
// 庄闲和投注按钮组合组件 - 重新设计简化版本
// 一个组件管理三个按钮，确保正确显示
// 创建时间: 2025/6/27

using System;
using UnityEngine;
using UnityEngine.UI;
using BaccaratGame.Data;

namespace BaccaratGame.UI.Components
{
    /// <summary>
    /// 庄闲和投注按钮组合组件
    /// 简化版本，确保三个按钮正确显示
    /// </summary>
    public class BankerPlayerButton : MonoBehaviour
    {
        #region 序列化字段

        [Header("整体布局")]
        public Vector2 containerSize = new Vector2(800, 120);
        
        [Header("字体设置")]
        public int titleFontSize = 24;
        public int oddsFontSize = 16;
        public int numberFontSize = 14;

        [Header("调试设置")]
        public bool enableDebugMode = true;

        #endregion

        #region 私有字段

        private RectTransform rectTransform;
        private bool buttonsCreated = false;
        
        // 三个按钮的引用
        private GameObject playerButton;  // 闲
        private GameObject bankerButton;  // 庄
        private GameObject tieButton;     // 和
        
        // 测试数据
        private readonly int[] testPlayerCounts = { 26, 38, 8 };
        private readonly decimal[] testAmounts = { 844m, 735m, 255m };

        #endregion

        #region 事件定义

        public System.Action<BaccaratBetType> OnBetTypeSelected;

        #endregion

        #region 生命周期

        private void Awake()
        {
            InitializeComponent();
        }

        private void Start()
        {
            CreateAllButtons();
            ApplyTestData();
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponent()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            // 设置容器大小
            rectTransform.sizeDelta = containerSize;

            Debug.Log("[BankerPlayerButton] 组件初始化完成");
        }

        #endregion

        #region 按钮创建

        /// <summary>
        /// 创建所有按钮
        /// </summary>
        [ContextMenu("创建所有按钮")]
        public void CreateAllButtons()
        {
            if (buttonsCreated)
            {
                Debug.Log("[BankerPlayerButton] 按钮已存在，先清除");
                ClearAllButtons();
            }

            Debug.Log("[BankerPlayerButton] 开始创建三个按钮");

            try
            {
                // 创建闲按钮（左边，蓝色）
                playerButton = CreatePlayerButton();
                
                // 创建庄按钮（右边，红色）
                bankerButton = CreateBankerButton();
                
                // 创建和按钮（中间，绿色）
                tieButton = CreateTieButton();

                buttonsCreated = true;
                Debug.Log("[BankerPlayerButton] 三个按钮创建完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BankerPlayerButton] 创建按钮时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建闲按钮（左边）
        /// </summary>
        private GameObject CreatePlayerButton()
        {
            GameObject button = CreateBaseButton("PlayerButton", 0f, 0.45f);
            
            // 设置蓝色渐变背景
            SetButtonGradient(button, new Color(0.4f, 0.7f, 1f), new Color(0.1f, 0.4f, 0.9f));
            
            // 添加文本
            AddButtonTexts(button, "闲", "1:1", true);
            
            // 添加点击事件
            Button buttonComponent = button.GetComponent<Button>();
            buttonComponent.onClick.AddListener(() => OnBetTypeSelected?.Invoke(BaccaratBetType.Player));
            
            Debug.Log("[BankerPlayerButton] 闲按钮创建完成");
            return button;
        }

        /// <summary>
        /// 创建庄按钮（右边）
        /// </summary>
        private GameObject CreateBankerButton()
        {
            GameObject button = CreateBaseButton("BankerButton", 0.55f, 0.45f);
            
            // 设置红色渐变背景
            SetButtonGradient(button, new Color(1f, 0.5f, 0.5f), new Color(0.9f, 0.2f, 0.2f));
            
            // 添加文本
            AddButtonTexts(button, "庄", "1:0.95", true);
            
            // 添加点击事件
            Button buttonComponent = button.GetComponent<Button>();
            buttonComponent.onClick.AddListener(() => OnBetTypeSelected?.Invoke(BaccaratBetType.Banker));
            
            Debug.Log("[BankerPlayerButton] 庄按钮创建完成");
            return button;
        }

        /// <summary>
        /// 创建和按钮（中间）
        /// </summary>
        private GameObject CreateTieButton()
        {
            GameObject button = CreateBaseButton("TieButton", 0.425f, 0.15f);
            
            // 设置绿色渐变背景
            SetButtonGradient(button, new Color(0.5f, 1f, 0.5f), new Color(0.3f, 0.8f, 0.3f));
            
            // 添加文本（不显示投注信息）
            AddButtonTexts(button, "和", "1:8", false);
            
            // 添加点击事件
            Button buttonComponent = button.GetComponent<Button>();
            buttonComponent.onClick.AddListener(() => OnBetTypeSelected?.Invoke(BaccaratBetType.Tie));
            
            Debug.Log("[BankerPlayerButton] 和按钮创建完成");
            return button;
        }

        /// <summary>
        /// 创建基础按钮结构
        /// </summary>
        private GameObject CreateBaseButton(string name, float xPos, float width)
        {
            // 创建按钮GameObject
            GameObject button = new GameObject(name);
            button.transform.SetParent(transform);

            // 设置RectTransform
            RectTransform rect = button.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(xPos, 0f);
            rect.anchorMax = new Vector2(xPos + width, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;

            // 添加Button组件
            Button buttonComp = button.AddComponent<Button>();
            
            // 添加Image组件（用于背景）
            Image image = button.AddComponent<Image>();
            buttonComp.targetGraphic = image;
            
            // 设置按钮状态颜色
            ColorBlock colors = buttonComp.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.disabledColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            buttonComp.colors = colors;

            Debug.Log($"[BankerPlayerButton] 基础按钮 {name} 创建完成，位置: {xPos}, 宽度: {width}");
            return button;
        }

        /// <summary>
        /// 设置按钮渐变背景
        /// </summary>
        private void SetButtonGradient(GameObject button, Color topColor, Color bottomColor)
        {
            // 创建渐变纹理
            Texture2D gradientTexture = CreateGradientTexture(topColor, bottomColor);
            
            // 替换Image为RawImage以显示渐变
            Image oldImage = button.GetComponent<Image>();
            if (oldImage != null)
            {
                DestroyImmediate(oldImage);
            }
            
            RawImage rawImage = button.AddComponent<RawImage>();
            rawImage.texture = gradientTexture;
            
            // 更新Button的目标图形
            Button buttonComp = button.GetComponent<Button>();
            buttonComp.targetGraphic = rawImage;
            
            Debug.Log($"[BankerPlayerButton] 按钮 {button.name} 渐变背景设置完成");
        }

        /// <summary>
        /// 添加按钮文本
        /// </summary>
        private void AddButtonTexts(GameObject button, string title, string odds, bool showBetInfo)
        {
            // 创建标题文本（左上角）
            CreateText(button, "TitleText", title, titleFontSize, FontStyle.Bold, 
                new Vector2(0.05f, 0.6f), new Vector2(0.5f, 0.95f), Color.white);

            // 创建赔率文本（右上角）
            CreateText(button, "OddsText", odds, oddsFontSize, FontStyle.Normal,
                new Vector2(0.5f, 0.6f), new Vector2(0.95f, 0.95f), Color.white);

            if (showBetInfo)
            {
                // 创建投注人数文本（左下角）
                CreateText(button, "PlayerCountText", "", numberFontSize, FontStyle.Normal,
                    new Vector2(0.05f, 0.05f), new Vector2(0.5f, 0.4f), Color.yellow);

                // 创建投注金额文本（右下角）
                CreateText(button, "AmountText", "", numberFontSize, FontStyle.Bold,
                    new Vector2(0.5f, 0.05f), new Vector2(0.95f, 0.4f), Color.yellow);
            }

            Debug.Log($"[BankerPlayerButton] 按钮 {button.name} 文本添加完成");
        }

        /// <summary>
        /// 创建文本组件
        /// </summary>
        private GameObject CreateText(GameObject parent, string name, string text, int fontSize, 
            FontStyle fontStyle, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = anchorMin;
            textRect.anchorMax = anchorMax;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textRect.localScale = Vector3.one;

            Text textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = fontSize;
            textComponent.fontStyle = fontStyle;
            textComponent.color = color;
            textComponent.alignment = TextAnchor.MiddleCenter;

            return textObj;
        }

        /// <summary>
        /// 创建渐变纹理
        /// </summary>
        private Texture2D CreateGradientTexture(Color topColor, Color bottomColor)
        {
            int width = 64;
            int height = 64;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            
            for (int y = 0; y < height; y++)
            {
                Color color = Color.Lerp(bottomColor, topColor, (float)y / height);
                for (int x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }
            
            texture.Apply();
            return texture;
        }

        #endregion

        #region 数据更新

        /// <summary>
        /// 应用测试数据
        /// </summary>
        [ContextMenu("应用测试数据")]
        public void ApplyTestData()
        {
            if (!buttonsCreated)
            {
                Debug.Log("[BankerPlayerButton] 按钮未创建，跳过测试数据");
                return;
            }

            // 更新闲按钮数据
            UpdateButtonData(playerButton, testPlayerCounts[0], testAmounts[0]);
            
            // 更新庄按钮数据
            UpdateButtonData(bankerButton, testPlayerCounts[1], testAmounts[1]);
            
            // 和按钮不显示投注数据

            Debug.Log("[BankerPlayerButton] 测试数据应用完成");
        }

        /// <summary>
        /// 更新单个按钮的数据
        /// </summary>
        private void UpdateButtonData(GameObject button, int playerCount, decimal amount)
        {
            if (button == null) return;

            // 查找并更新人数文本
            Transform playerCountObj = button.transform.Find("PlayerCountText");
            if (playerCountObj != null)
            {
                Text playerCountText = playerCountObj.GetComponent<Text>();
                if (playerCountText != null)
                {
                    playerCountText.text = playerCount > 0 ? $"👥{playerCount}" : "";
                }
            }

            // 查找并更新金额文本
            Transform amountObj = button.transform.Find("AmountText");
            if (amountObj != null)
            {
                Text amountText = amountObj.GetComponent<Text>();
                if (amountText != null)
                {
                    amountText.text = amount > 0 ? $"¥{FormatAmount(amount)}" : "";
                }
            }

            Debug.Log($"[BankerPlayerButton] 按钮 {button.name} 数据更新: {playerCount}人, ¥{amount}");
        }

        /// <summary>
        /// 格式化金额显示
        /// </summary>
        private string FormatAmount(decimal amount)
        {
            if (amount >= 10000)
                return $"{amount / 10000:F1}万";
            else if (amount >= 1000)
                return $"{amount / 1000:F1}K";
            else
                return amount.ToString("F0");
        }

        #endregion

        #region 清理和重建

        /// <summary>
        /// 清除所有按钮
        /// </summary>
        [ContextMenu("清除所有按钮")]
        public void ClearAllButtons()
        {
            // 清除所有子对象
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
            
            // 重置引用
            playerButton = null;
            bankerButton = null;
            tieButton = null;
            buttonsCreated = false;
            
            Debug.Log("[BankerPlayerButton] 所有按钮已清除");
        }

        /// <summary>
        /// 重新创建所有按钮
        /// </summary>
        [ContextMenu("重新创建所有按钮")]
        public void RecreateAllButtons()
        {
            ClearAllButtons();
            CreateAllButtons();
            ApplyTestData();
        }

        #endregion

        #region 调试方法

        /// <summary>
        /// 显示组件状态
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowComponentStatus()
        {
            Debug.Log("=== BankerPlayerButton 组件状态 ===");
            Debug.Log($"按钮已创建: {buttonsCreated}");
            Debug.Log($"容器大小: {containerSize}");
            Debug.Log($"子对象数量: {transform.childCount}");
            Debug.Log($"闲按钮: {(playerButton != null ? "✓" : "✗")}");
            Debug.Log($"庄按钮: {(bankerButton != null ? "✓" : "✗")}");
            Debug.Log($"和按钮: {(tieButton != null ? "✓" : "✗")}");
            
            // 显示子对象名称
            for (int i = 0; i < transform.childCount; i++)
            {
                Debug.Log($"子对象 {i}: {transform.GetChild(i).name}");
            }
        }

        #endregion
    }
}
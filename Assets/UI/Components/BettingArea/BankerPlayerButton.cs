// Assets/UI/Components/BettingArea/BankerPlayerButton.cs
// 庄闲和投注按钮组合组件 - 简化版本
// 庄闲和三个按钮，庄闲显示投注信息和图标
// 创建时间: 2025/6/27

using System;
using UnityEngine;
using UnityEngine.UI;
using BaccaratGame.Data;

namespace BaccaratGame.UI.Components
{
    /// <summary>
    /// 庄闲和投注按钮组合组件
    /// 庄闲和三个按钮，庄闲显示投注信息
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

        [Header("PNG背景图片")]
        public Sprite playerButtonSprite;    // 闲按钮背景图
        public Sprite bankerButtonSprite;    // 庄按钮背景图
        public Sprite tieButtonSprite;       // 和按钮背景图

        [Header("投注信息图标")]
        public Sprite playerCountIcon;       // 投注人数图标 (👥)
        public Sprite amountIcon;           // 投注金额图标 (¥)

        [Header("Resources路径设置")]
        public string spritePath = "Images/BettingButtons/"; // 背景图片路径
        public string iconPath = "Images/Icons/";            // 图标路径

        [Header("调试设置")]
        public bool enableDebugMode = true;

        #endregion

        #region 私有字段

        private RectTransform rectTransform;
        private bool buttonsCreated = false;
        
        // 三个按钮的引用
        private ButtonData playerButtonData;  // 闲
        private ButtonData bankerButtonData;  // 庄
        private ButtonData tieButtonData;     // 和
        
        // 测试数据
        private readonly int[] testPlayerCounts = { 26, 38, 8 };
        private readonly decimal[] testAmounts = { 844m, 735m, 255m };

        #endregion

        #region 按钮数据结构

        /// <summary>
        /// 单个按钮的数据结构
        /// </summary>
        private class ButtonData
        {
            public GameObject gameObject;
            public Image backgroundImage;
            public Button button;
            public Text titleText;
            public Text oddsText;
            public Image playerCountIcon;
            public Text playerCountText;
            public Image amountIcon;
            public Text amountText;
            public BaccaratBetType betType;
            public int currentPlayerCount = 0;
            public decimal currentAmount = 0m;
        }

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
            // 自动加载图片资源
            LoadSpritesFromResources();
            
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

        #region 资源加载

        /// <summary>
        /// 从Resources文件夹自动加载图片
        /// </summary>
        [ContextMenu("从Resources加载图片")]
        public void LoadSpritesFromResources()
        {
            try
            {
                // 加载背景图片
                if (playerButtonSprite == null)
                {
                    playerButtonSprite = Resources.Load<Sprite>(spritePath + "player_button_normal");
                    if (playerButtonSprite != null)
                        Debug.Log("[BankerPlayerButton] 成功加载 player_button_normal");
                }

                if (bankerButtonSprite == null)
                {
                    bankerButtonSprite = Resources.Load<Sprite>(spritePath + "banker_button_normal");
                    if (bankerButtonSprite != null)
                        Debug.Log("[BankerPlayerButton] 成功加载 banker_button_normal");
                }

                if (tieButtonSprite == null)
                {
                    tieButtonSprite = Resources.Load<Sprite>(spritePath + "tie_button_normal");
                    if (tieButtonSprite != null)
                        Debug.Log("[BankerPlayerButton] 成功加载 tie_button_normal");
                }

                // 加载图标
                if (playerCountIcon == null)
                {
                    playerCountIcon = Resources.Load<Sprite>(iconPath + "player_count_icon");
                    if (playerCountIcon == null)
                        Debug.Log("[BankerPlayerButton] 未找到 player_count_icon，将使用文字显示");
                }

                if (amountIcon == null)
                {
                    amountIcon = Resources.Load<Sprite>(iconPath + "amount_icon");
                    if (amountIcon == null)
                        Debug.Log("[BankerPlayerButton] 未找到 amount_icon，将使用文字显示");
                }

                Debug.Log("[BankerPlayerButton] Resources图片加载完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BankerPlayerButton] 从Resources加载图片时出错: {ex.Message}");
            }
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
                playerButtonData = CreateButton("PlayerButton", 0f, 0.45f, BaccaratBetType.Player, 
                    "闲", "1:1", playerButtonSprite, true);
                
                // 创建庄按钮（右边，红色）
                bankerButtonData = CreateButton("BankerButton", 0.55f, 0.45f, BaccaratBetType.Banker, 
                    "庄", "1:0.95", bankerButtonSprite, true);
                
                // 创建和按钮（中间，绿色）
                tieButtonData = CreateButton("TieButton", 0.425f, 0.15f, BaccaratBetType.Tie, 
                    "和", "1:8", tieButtonSprite, false);

                buttonsCreated = true;
                Debug.Log("[BankerPlayerButton] 三个按钮创建完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BankerPlayerButton] 创建按钮时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建单个按钮
        /// </summary>
        private ButtonData CreateButton(string name, float xPos, float width, BaccaratBetType betType,
            string title, string odds, Sprite backgroundSprite, bool showBetInfo)
        {
            ButtonData data = new ButtonData();
            data.betType = betType;

            // 创建按钮GameObject
            data.gameObject = new GameObject(name);
            data.gameObject.transform.SetParent(transform);

            // 设置RectTransform
            RectTransform rect = data.gameObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(xPos, 0f);
            rect.anchorMax = new Vector2(xPos + width, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;

            // 创建背景图片
            data.backgroundImage = data.gameObject.AddComponent<Image>();
            if (backgroundSprite != null)
            {
                data.backgroundImage.sprite = backgroundSprite;
                data.backgroundImage.type = Image.Type.Sliced;
            }
            else
            {
                // 如果没有图片，使用默认颜色
                data.backgroundImage.color = GetDefaultColor(betType);
            }

            // 创建Button组件
            data.button = data.gameObject.AddComponent<Button>();
            data.button.targetGraphic = data.backgroundImage;
            
            // 设置按钮点击事件
            data.button.onClick.AddListener(() => OnBetTypeSelected?.Invoke(betType));

            // 创建文本和图标
            CreateButtonContent(data, title, odds, showBetInfo);

            Debug.Log($"[BankerPlayerButton] 按钮 {name} 创建完成");
            return data;
        }

        /// <summary>
        /// 创建按钮内容（文本和图标）
        /// </summary>
        private void CreateButtonContent(ButtonData data, string title, string odds, bool showBetInfo)
        {
            // 创建标题文本（左上角）
            data.titleText = CreateText(data.gameObject, "TitleText", title, titleFontSize, FontStyle.Bold, 
                new Vector2(0.05f, 0.6f), new Vector2(0.5f, 0.95f), Color.white);

            // 创建赔率文本（右上角）
            data.oddsText = CreateText(data.gameObject, "OddsText", odds, oddsFontSize, FontStyle.Normal,
                new Vector2(0.5f, 0.6f), new Vector2(0.95f, 0.95f), Color.white);

            if (showBetInfo)
            {
                // 创建投注人数区域（左下角）
                CreatePlayerCountArea(data);
                
                // 创建投注金额区域（右下角）
                CreateAmountArea(data);
            }

            Debug.Log($"[BankerPlayerButton] 按钮内容创建完成: {data.gameObject.name}");
        }

        /// <summary>
        /// 创建投注人数区域（图标+文字）
        /// </summary>
        private void CreatePlayerCountArea(ButtonData data)
        {
            // 创建容器
            GameObject container = new GameObject("PlayerCountArea");
            container.transform.SetParent(data.gameObject.transform);

            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.05f, 0.05f);
            containerRect.anchorMax = new Vector2(0.5f, 0.45f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            containerRect.localScale = Vector3.one;

            // 创建图标
            if (playerCountIcon != null)
            {
                GameObject iconObj = new GameObject("PlayerCountIcon");
                iconObj.transform.SetParent(container.transform);

                RectTransform iconRect = iconObj.AddComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0f, 0f);
                iconRect.anchorMax = new Vector2(0.3f, 1f);
                iconRect.offsetMin = Vector2.zero;
                iconRect.offsetMax = Vector2.zero;
                iconRect.localScale = Vector3.one;

                data.playerCountIcon = iconObj.AddComponent<Image>();
                data.playerCountIcon.sprite = playerCountIcon;
                data.playerCountIcon.preserveAspect = true;
            }

            // 创建文字
            GameObject textObj = new GameObject("PlayerCountText");
            textObj.transform.SetParent(container.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(playerCountIcon != null ? 0.3f : 0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textRect.localScale = Vector3.one;

            data.playerCountText = textObj.AddComponent<Text>();
            data.playerCountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            data.playerCountText.fontSize = numberFontSize;
            data.playerCountText.fontStyle = FontStyle.Normal;
            data.playerCountText.color = Color.yellow;
            data.playerCountText.alignment = TextAnchor.MiddleLeft;
        }

        /// <summary>
        /// 创建投注金额区域（图标+文字）
        /// </summary>
        private void CreateAmountArea(ButtonData data)
        {
            // 创建容器
            GameObject container = new GameObject("AmountArea");
            container.transform.SetParent(data.gameObject.transform);

            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.05f);
            containerRect.anchorMax = new Vector2(0.95f, 0.45f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            containerRect.localScale = Vector3.one;

            // 创建图标
            if (amountIcon != null)
            {
                GameObject iconObj = new GameObject("AmountIcon");
                iconObj.transform.SetParent(container.transform);

                RectTransform iconRect = iconObj.AddComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0f, 0f);
                iconRect.anchorMax = new Vector2(0.3f, 1f);
                iconRect.offsetMin = Vector2.zero;
                iconRect.offsetMax = Vector2.zero;
                iconRect.localScale = Vector3.one;

                data.amountIcon = iconObj.AddComponent<Image>();
                data.amountIcon.sprite = amountIcon;
                data.amountIcon.preserveAspect = true;
            }

            // 创建文字
            GameObject textObj = new GameObject("AmountText");
            textObj.transform.SetParent(container.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(amountIcon != null ? 0.3f : 0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textRect.localScale = Vector3.one;

            data.amountText = textObj.AddComponent<Text>();
            data.amountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            data.amountText.fontSize = numberFontSize;
            data.amountText.fontStyle = FontStyle.Bold;
            data.amountText.color = Color.yellow;
            data.amountText.alignment = TextAnchor.MiddleRight;
        }

        /// <summary>
        /// 创建文本组件
        /// </summary>
        private Text CreateText(GameObject parent, string name, string text, int fontSize, 
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

            return textComponent;
        }

        /// <summary>
        /// 获取默认颜色（当没有PNG图片时使用）
        /// </summary>
        private Color GetDefaultColor(BaccaratBetType betType)
        {
            return betType switch
            {
                BaccaratBetType.Player => new Color(0.2f, 0.4f, 1f, 1f), // 蓝色
                BaccaratBetType.Banker => new Color(1f, 0.2f, 0.2f, 1f), // 红色
                BaccaratBetType.Tie => new Color(0.2f, 0.8f, 0.2f, 1f),    // 绿色
                _ => Color.gray
            };
        }

        #endregion

        #region 数据更新

        /// <summary>
        /// 应用测试数据
        /// </summary>
        [ContextMenu("应用测试数据")]
        public void ApplyTestData()
        {
            if (!buttonsCreated) return;

            // 更新庄闲按钮数据
            UpdateButtonData(playerButtonData, testPlayerCounts[0], testAmounts[0]);
            UpdateButtonData(bankerButtonData, testPlayerCounts[1], testAmounts[1]);
            // 和按钮不显示投注数据

            Debug.Log("[BankerPlayerButton] 测试数据应用完成");
        }

        /// <summary>
        /// 更新按钮数据
        /// </summary>
        public void UpdateButtonData(BaccaratBetType betType, int playerCount, decimal amount)
        {
            ButtonData data = GetButtonData(betType);
            if (data == null) return;

            UpdateButtonData(data, playerCount, amount);
        }

        /// <summary>
        /// 更新单个按钮数据
        /// </summary>
        private void UpdateButtonData(ButtonData data, int playerCount, decimal amount)
        {
            if (data == null) return;

            data.currentPlayerCount = playerCount;
            data.currentAmount = amount;

            // 更新人数显示
            if (data.playerCountText != null)
            {
                if (playerCountIcon != null)
                {
                    // 有图标时只显示数字
                    data.playerCountText.text = playerCount > 0 ? playerCount.ToString() : "";
                }
                else
                {
                    // 没有图标时显示图标+数字
                    data.playerCountText.text = playerCount > 0 ? $"👥{playerCount}" : "";
                }
            }

            // 更新金额显示
            if (data.amountText != null)
            {
                if (amountIcon != null)
                {
                    // 有图标时只显示金额
                    data.amountText.text = amount > 0 ? FormatAmount(amount) : "";
                }
                else
                {
                    // 没有图标时显示符号+金额
                    data.amountText.text = amount > 0 ? $"¥{FormatAmount(amount)}" : "";
                }
            }

            Debug.Log($"[BankerPlayerButton] 按钮数据更新: {data.betType} - {playerCount}人, ¥{amount}");
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

        #region 辅助方法

        /// <summary>
        /// 根据类型获取按钮数据
        /// </summary>
        private ButtonData GetButtonData(BaccaratBetType betType)
        {
            return betType switch
            {
                BaccaratBetType.Player => playerButtonData,
                BaccaratBetType.Banker => bankerButtonData,
                BaccaratBetType.Tie => tieButtonData,
                _ => null
            };
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
            playerButtonData = null;
            bankerButtonData = null;
            tieButtonData = null;
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
            Debug.Log($"闲按钮: {(playerButtonData != null ? "✓" : "✗")}");
            Debug.Log($"庄按钮: {(bankerButtonData != null ? "✓" : "✗")}");
            Debug.Log($"和按钮: {(tieButtonData != null ? "✓" : "✗")}");
            Debug.Log($"人数图标: {(playerCountIcon != null ? "✓" : "✗")}");
            Debug.Log($"金额图标: {(amountIcon != null ? "✓" : "✗")}");
        }

        #endregion
    }
}
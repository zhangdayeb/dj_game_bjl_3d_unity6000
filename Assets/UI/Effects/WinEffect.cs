// Assets/UI/Components/VideoOverlay/Set/WinEffect.cs
// 简化版中奖特效组件 - 仅用于UI生成
// 挂载到节点上自动创建中奖展示UI
// 创建时间: 2025/6/28

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace BaccaratGame.UI.Components.VideoOverlay
{
    /// <summary>
    /// 简化版中奖特效组件
    /// 挂载到节点上自动创建UI，包含不同等级的中奖展示
    /// </summary>
    public class WinEffect : MonoBehaviour
    {
        #region 配置参数

        [Header("面板配置")]
        public Vector2 panelSize = new Vector2(500, 350);
        public Color backgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.95f);
        public Color titleColor = Color.white;
        public int fontSize = 16;
        
        [Header("遮罩层设置")]
        public Color maskColor = new Color(0, 0, 0, 0.4f);
        
        [Header("中奖等级配置")]
        public Color smallWinColor = new Color(0.2f, 0.8f, 0.2f, 1f);  // 绿色
        public Color mediumWinColor = new Color(0.2f, 0.6f, 1f, 1f);   // 蓝色
        public Color bigWinColor = new Color(1f, 0.6f, 0.2f, 1f);      // 橙色
        public Color jackpotWinColor = new Color(1f, 0.8f, 0.2f, 1f);  // 金色
        
        [Header("中奖阈值")]
        public int smallWinThreshold = 10;
        public int mediumWinThreshold = 100;
        public int bigWinThreshold = 1000;
        public int jackpotWinThreshold = 10000;

        #endregion

        #region 私有字段

        private bool uiCreated = false;
        private GameObject maskLayer;
        private GameObject winPanel;
        private Canvas uiCanvas;
        
        // UI组件引用
        private Text winAmountText;
        private Text winMessageText;
        private Image flashOverlay;
        
        // 中奖等级数据
        private readonly string[] winLevelNames = { "小奖", "中奖", "大奖", "超级大奖" };
        private readonly int[] winAmounts = { 50, 300, 2000, 50000 };
        private Color[] winColors;

        #endregion

        #region 生命周期

        private void Awake()
        {
            InitializeWinColors();
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
            CreateWinDisplay();
            CreateWinButtons();
            
            uiCreated = true;
        }

        /// <summary>
        /// 初始化中奖颜色
        /// </summary>
        private void InitializeWinColors()
        {
            winColors = new Color[] { smallWinColor, mediumWinColor, bigWinColor, jackpotWinColor };
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
                uiCanvas.sortingOrder = 4000; // 确保在最上层
                
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
            panelRect.anchorMin = new Vector2(0.5f, 0.5f); // 居中
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = panelSize;
            panelRect.anchoredPosition = Vector2.zero;

            Image panelBg = winPanel.AddComponent<Image>();
            panelBg.color = backgroundColor;
            panelBg.sprite = CreateSimpleSprite();

            // 添加发光边框
            Outline outline = winPanel.AddComponent<Outline>();
            outline.effectColor = jackpotWinColor;
            outline.effectDistance = new Vector2(3, -3);

            // 添加阴影
            Shadow shadow = winPanel.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(5, -5);
        }

        /// <summary>
        /// 创建中奖显示区域
        /// </summary>
        private void CreateWinDisplay()
        {
            // 创建闪光层
            CreateFlashOverlay();
            
            // 创建标题
            CreateTitle();
            
            // 创建中奖金额显示
            CreateWinAmountDisplay();
            
            // 创建中奖消息显示
            CreateWinMessageDisplay();
        }

        /// <summary>
        /// 创建闪光覆盖层
        /// </summary>
        private void CreateFlashOverlay()
        {
            GameObject flashObj = new GameObject("FlashOverlay");
            flashObj.transform.SetParent(winPanel.transform);

            RectTransform flashRect = flashObj.AddComponent<RectTransform>();
            flashRect.anchorMin = Vector2.zero;
            flashRect.anchorMax = Vector2.one;
            flashRect.offsetMin = Vector2.zero;
            flashRect.offsetMax = Vector2.zero;

            flashOverlay = flashObj.AddComponent<Image>();
            flashOverlay.color = new Color(1f, 1f, 1f, 0.3f);
            flashOverlay.sprite = CreateSimpleSprite();
            
            // 默认隐藏
            flashObj.SetActive(false);
        }

        /// <summary>
        /// 创建标题
        /// </summary>
        private void CreateTitle()
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(winPanel.transform);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.8f);
            titleRect.anchorMax = new Vector2(1, 1f);
            titleRect.offsetMin = new Vector2(15, 0);
            titleRect.offsetMax = new Vector2(-15, -5);

            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "🎉 中奖特效展示 🎉";
            titleText.color = titleColor;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = fontSize + 4;
            titleText.fontStyle = FontStyle.Bold;
        }

        /// <summary>
        /// 创建中奖金额显示
        /// </summary>
        private void CreateWinAmountDisplay()
        {
            GameObject amountObj = new GameObject("WinAmount");
            amountObj.transform.SetParent(winPanel.transform);

            RectTransform amountRect = amountObj.AddComponent<RectTransform>();
            amountRect.anchorMin = new Vector2(0, 0.5f);
            amountRect.anchorMax = new Vector2(1, 0.8f);
            amountRect.offsetMin = new Vector2(20, 0);
            amountRect.offsetMax = new Vector2(-20, 0);

            winAmountText = amountObj.AddComponent<Text>();
            winAmountText.text = "¥50,000";
            winAmountText.color = jackpotWinColor;
            winAmountText.alignment = TextAnchor.MiddleCenter;
            winAmountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            winAmountText.fontSize = fontSize + 12;
            winAmountText.fontStyle = FontStyle.Bold;

            // 添加发光效果
            Outline amountOutline = amountObj.AddComponent<Outline>();
            amountOutline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            amountOutline.effectDistance = new Vector2(2, -2);
        }

        /// <summary>
        /// 创建中奖消息显示
        /// </summary>
        private void CreateWinMessageDisplay()
        {
            GameObject messageObj = new GameObject("WinMessage");
            messageObj.transform.SetParent(winPanel.transform);

            RectTransform messageRect = messageObj.AddComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0, 0.35f);
            messageRect.anchorMax = new Vector2(1, 0.5f);
            messageRect.offsetMin = new Vector2(20, 0);
            messageRect.offsetMax = new Vector2(-20, 0);

            winMessageText = messageObj.AddComponent<Text>();
            winMessageText.text = "🏆 超级大奖!!! 🏆";
            winMessageText.color = jackpotWinColor;
            winMessageText.alignment = TextAnchor.MiddleCenter;
            winMessageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            winMessageText.fontSize = fontSize + 2;
            winMessageText.fontStyle = FontStyle.Bold;

            // 添加阴影
            Shadow messageShadow = messageObj.AddComponent<Shadow>();
            messageShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            messageShadow.effectDistance = new Vector2(1, -1);
        }

        /// <summary>
        /// 创建中奖按钮
        /// </summary>
        private void CreateWinButtons()
        {
            GameObject buttonsObj = new GameObject("WinButtons");
            buttonsObj.transform.SetParent(winPanel.transform);

            RectTransform buttonsRect = buttonsObj.AddComponent<RectTransform>();
            buttonsRect.anchorMin = new Vector2(0, 0.05f);
            buttonsRect.anchorMax = new Vector2(1, 0.35f);
            buttonsRect.offsetMin = new Vector2(20, 0);
            buttonsRect.offsetMax = new Vector2(-20, 0);

            // 添加网格布局
            GridLayoutGroup gridLayout = buttonsObj.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(100, 50);
            gridLayout.spacing = new Vector2(10, 10);
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.MiddleCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 2;

            // 创建中奖等级按钮
            for (int i = 0; i < winLevelNames.Length; i++)
            {
                CreateWinLevelButton(buttonsObj, i);
            }
        }

        /// <summary>
        /// 创建中奖等级按钮
        /// </summary>
        private void CreateWinLevelButton(GameObject parent, int levelIndex)
        {
            GameObject buttonObj = new GameObject($"WinButton_{winLevelNames[levelIndex]}");
            buttonObj.transform.SetParent(parent.transform);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(100, 50);

            Button winButton = buttonObj.AddComponent<Button>();
            
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = winColors[levelIndex];
            buttonImage.sprite = CreateSimpleSprite();

            winButton.onClick.AddListener(() => ShowWinLevel(levelIndex));

            // 按钮文字
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = winLevelNames[levelIndex];
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = fontSize - 2;
            buttonText.fontStyle = FontStyle.Bold;

            // 添加阴影
            Shadow textShadow = textObj.AddComponent<Shadow>();
            textShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            textShadow.effectDistance = new Vector2(1, -1);
        }

        /// <summary>
        /// 创建简单背景
        /// </summary>
        private Sprite CreateSimpleSprite()
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }

        #endregion

        #region 中奖展示逻辑

        /// <summary>
        /// 显示示例中奖
        /// </summary>
        private void ShowSampleWin()
        {
            // 默认显示超级大奖
            ShowWinLevel(3);
        }

        /// <summary>
        /// 显示指定等级的中奖
        /// </summary>
        private void ShowWinLevel(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= winLevelNames.Length) return;

            string levelName = winLevelNames[levelIndex];
            int amount = winAmounts[levelIndex];
            Color levelColor = winColors[levelIndex];

            // 更新显示
            if (winAmountText != null)
            {
                winAmountText.text = FormatWinAmount(amount);
                winAmountText.color = levelColor;
            }

            if (winMessageText != null)
            {
                string message = GetWinMessage(levelIndex);
                winMessageText.text = message;
                winMessageText.color = levelColor;
            }

            // 更新边框颜色
            Outline panelOutline = winPanel.GetComponent<Outline>();
            if (panelOutline != null)
            {
                panelOutline.effectColor = levelColor;
            }

            // 播放闪光效果
            StartCoroutine(PlayFlashEffect(levelColor));

            Debug.Log($"[WinEffect] 显示{levelName}: ¥{amount}");
        }

        /// <summary>
        /// 获取中奖消息
        /// </summary>
        private string GetWinMessage(int levelIndex)
        {
            return levelIndex switch
            {
                0 => "🎉 小奖中奖! 🎉",
                1 => "🎊 中奖来了! 🎊", 
                2 => "🔥 巨额奖金! 🔥",
                3 => "🏆 超级大奖!!! 🏆",
                _ => "🎉 中奖了! 🎉"
            };
        }

        /// <summary>
        /// 格式化中奖金额
        /// </summary>
        private string FormatWinAmount(int amount)
        {
            if (amount >= 10000)
                return $"¥{amount / 10000:F1}万";
            else if (amount >= 1000)
                return $"¥{amount / 1000:F1}K";
            else
                return $"¥{amount}";
        }

        /// <summary>
        /// 播放闪光特效
        /// </summary>
        private System.Collections.IEnumerator PlayFlashEffect(Color flashColor)
        {
            if (flashOverlay == null) yield break;

            // 设置闪光颜色
            Color originalColor = flashColor;
            originalColor.a = 0.5f;
            flashOverlay.color = originalColor;

            // 闪光3次
            for (int i = 0; i < 3; i++)
            {
                flashOverlay.gameObject.SetActive(true);
                yield return new WaitForSeconds(0.1f);
                
                flashOverlay.gameObject.SetActive(false);
                yield return new WaitForSeconds(0.1f);
            }
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void HidePanel()
        {
            if (maskLayer != null) maskLayer.SetActive(false);
            if (winPanel != null) winPanel.SetActive(false);
            Debug.Log("[WinEffect] 面板已隐藏");
        }

        /// <summary>
        /// 显示面板
        /// </summary>
        public void ShowPanel()
        {
            if (maskLayer != null) maskLayer.SetActive(true);
            if (winPanel != null) winPanel.SetActive(true);
            Debug.Log("[WinEffect] 面板已显示");
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 切换面板显示状态
        /// </summary>
        public void TogglePanel()
        {
            if (maskLayer != null && maskLayer.activeInHierarchy)
                HidePanel();
            else
                ShowPanel();
        }

        /// <summary>
        /// 播放中奖特效 (简化版本，仅更新显示)
        /// </summary>
        public void PlayWinEffect(int winAmount, string winType = "")
        {
            int levelIndex = GetWinLevelIndex(winAmount);
            ShowWinLevel(levelIndex);
        }

        /// <summary>
        /// 根据金额获取等级索引
        /// </summary>
        private int GetWinLevelIndex(int amount)
        {
            if (amount >= jackpotWinThreshold) return 3;
            if (amount >= bigWinThreshold) return 2;
            if (amount >= mediumWinThreshold) return 1;
            return 0;
        }

        /// <summary>
        /// 测试小奖
        /// </summary>
        public void TestSmallWin()
        {
            PlayWinEffect(50);
        }

        /// <summary>
        /// 测试中奖
        /// </summary>
        public void TestMediumWin()
        {
            PlayWinEffect(300);
        }

        /// <summary>
        /// 测试大奖
        /// </summary>
        public void TestBigWin()
        {
            PlayWinEffect(2000);
        }

        /// <summary>
        /// 测试超级大奖
        /// </summary>
        public void TestJackpotWin()
        {
            PlayWinEffect(50000);
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
        /// 显示组件状态
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowStatus()
        {
            Debug.Log($"[WinEffect] UI已创建: {uiCreated}");
            Debug.Log($"[WinEffect] 遮罩层: {(maskLayer != null ? "✓" : "✗")}");
            Debug.Log($"[WinEffect] 中奖面板: {(winPanel != null ? "✓" : "✗")}");
            Debug.Log($"[WinEffect] 中奖等级数: {winLevelNames.Length}");
        }

        /// <summary>
        /// 测试所有等级
        /// </summary>
        [ContextMenu("测试所有等级")]
        public void TestAllLevels()
        {
            StartCoroutine(TestAllLevelsCoroutine());
        }

        /// <summary>
        /// 测试所有等级协程
        /// </summary>
        private System.Collections.IEnumerator TestAllLevelsCoroutine()
        {
            for (int i = 0; i < winLevelNames.Length; i++)
            {
                ShowWinLevel(i);
                yield return new WaitForSeconds(2f);
            }
        }

        #endregion
    }
}
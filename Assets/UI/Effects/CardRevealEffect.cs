// Assets/UI/Components/VideoOverlay/Set/CardRevealEffect.cs
// 简化版卡牌显示特效组件 - 仅用于UI生成
// 挂载到节点上自动创建卡牌展示UI
// 创建时间: 2025/6/28

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace BaccaratGame.UI.Components.VideoOverlay
{
    /// <summary>
    /// 简化版卡牌显示特效组件
    /// 挂载到节点上自动创建UI，包含庄家和闲家卡牌区域
    /// </summary>
    public class CardRevealEffect : MonoBehaviour
    {
        #region 配置参数

        [Header("卡牌区域配置")]
        public Vector2 cardAreaSize = new Vector2(400, 150);
        public Vector2 playerAreaPosition = new Vector2(-200, -100); // 相对屏幕中心
        public Vector2 bankerAreaPosition = new Vector2(-200, 100);
        
        [Header("卡牌设置")]
        public Vector2 cardSize = new Vector2(80, 112);
        public float cardSpacing = 10f;
        public Color cardBackColor = new Color(0.2f, 0.4f, 0.8f, 1f);
        public Color playerAreaColor = new Color(0.1f, 0.3f, 0.1f, 0.8f);
        public Color bankerAreaColor = new Color(0.3f, 0.1f, 0.1f, 0.8f);
        
        [Header("UI样式")]
        public Color titleColor = Color.white;
        public int fontSize = 16;
        
        [Header("遮罩层设置")]
        public Color maskColor = new Color(0, 0, 0, 0.3f);

        #endregion

        #region 私有字段

        private bool uiCreated = false;
        private GameObject maskLayer;
        private GameObject cardPanel;
        private Canvas uiCanvas;
        
        // 卡牌区域引用
        private Transform playerCardArea;
        private Transform bankerCardArea;
        
        // 卡牌对象列表
        private List<GameObject> playerCards = new List<GameObject>();
        private List<GameObject> bankerCards = new List<GameObject>();

        #endregion

        #region 生命周期

        private void Awake()
        {
            CreateUI();
            CreateSampleCards();
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
            CreateCardPanel();
            CreateCardAreas();
            
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
                GameObject canvasObj = new GameObject("CardRevealCanvas");
                canvasObj.transform.SetParent(transform.parent);
                
                uiCanvas = canvasObj.AddComponent<Canvas>();
                uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                uiCanvas.sortingOrder = 3000; // 确保在最上层
                
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
        /// 创建卡牌面板
        /// </summary>
        private void CreateCardPanel()
        {
            cardPanel = new GameObject("CardPanel");
            cardPanel.transform.SetParent(transform);

            RectTransform panelRect = cardPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f); // 居中
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(500, 350);
            panelRect.anchoredPosition = Vector2.zero;

            Image panelBg = cardPanel.AddComponent<Image>();
            panelBg.color = new Color(0.05f, 0.05f, 0.05f, 0.9f);
            panelBg.sprite = CreateSimpleSprite();

            // 添加边框
            Outline outline = cardPanel.AddComponent<Outline>();
            outline.effectColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            outline.effectDistance = new Vector2(2, -2);
        }

        /// <summary>
        /// 创建卡牌区域
        /// </summary>
        private void CreateCardAreas()
        {
            // 庄家区域
            CreateBankerArea();
            
            // 闲家区域
            CreatePlayerArea();
        }

        /// <summary>
        /// 创建庄家区域
        /// </summary>
        private void CreateBankerArea()
        {
            GameObject bankerAreaObj = new GameObject("BankerArea");
            bankerAreaObj.transform.SetParent(cardPanel.transform);

            RectTransform bankerRect = bankerAreaObj.AddComponent<RectTransform>();
            bankerRect.anchorMin = new Vector2(0, 0.55f);
            bankerRect.anchorMax = new Vector2(1, 0.95f);
            bankerRect.offsetMin = new Vector2(20, 0);
            bankerRect.offsetMax = new Vector2(-20, -10);

            Image bankerBg = bankerAreaObj.AddComponent<Image>();
            bankerBg.color = bankerAreaColor;
            bankerBg.sprite = CreateSimpleSprite();

            // 庄家标题
            CreateAreaTitle(bankerAreaObj, "🏛️ 庄家", new Vector2(0, 0.7f), new Vector2(0.3f, 1f));

            // 庄家卡牌容器
            bankerCardArea = CreateCardContainer(bankerAreaObj, "BankerCards").transform;
        }

        /// <summary>
        /// 创建闲家区域
        /// </summary>
        private void CreatePlayerArea()
        {
            GameObject playerAreaObj = new GameObject("PlayerArea");
            playerAreaObj.transform.SetParent(cardPanel.transform);

            RectTransform playerRect = playerAreaObj.AddComponent<RectTransform>();
            playerRect.anchorMin = new Vector2(0, 0.05f);
            playerRect.anchorMax = new Vector2(1, 0.45f);
            playerRect.offsetMin = new Vector2(20, 10);
            playerRect.offsetMax = new Vector2(-20, 0);

            Image playerBg = playerAreaObj.AddComponent<Image>();
            playerBg.color = playerAreaColor;
            playerBg.sprite = CreateSimpleSprite();

            // 闲家标题
            CreateAreaTitle(playerAreaObj, "👤 闲家", new Vector2(0, 0.7f), new Vector2(0.3f, 1f));

            // 闲家卡牌容器
            playerCardArea = CreateCardContainer(playerAreaObj, "PlayerCards").transform;
        }

        /// <summary>
        /// 创建区域标题
        /// </summary>
        private void CreateAreaTitle(GameObject parent, string titleText, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent.transform);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = anchorMin;
            titleRect.anchorMax = anchorMax;
            titleRect.offsetMin = new Vector2(10, 0);
            titleRect.offsetMax = new Vector2(0, 0);

            Text title = titleObj.AddComponent<Text>();
            title.text = titleText;
            title.color = titleColor;
            title.alignment = TextAnchor.MiddleLeft;
            title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            title.fontSize = fontSize;
            title.fontStyle = FontStyle.Bold;
        }

        /// <summary>
        /// 创建卡牌容器
        /// </summary>
        private GameObject CreateCardContainer(GameObject parent, string containerName)
        {
            GameObject containerObj = new GameObject(containerName);
            containerObj.transform.SetParent(parent.transform);

            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.3f, 0);
            containerRect.anchorMax = new Vector2(1f, 0.7f);
            containerRect.offsetMin = new Vector2(10, 10);
            containerRect.offsetMax = new Vector2(-10, -10);

            // 添加水平布局组件
            HorizontalLayoutGroup layoutGroup = containerObj.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = cardSpacing;
            layoutGroup.padding = new RectOffset(5, 5, 5, 5);
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = false;
            layoutGroup.childForceExpandHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childAlignment = TextAnchor.MiddleLeft;

            return containerObj;
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

        #region 卡牌创建

        /// <summary>
        /// 创建示例卡牌
        /// </summary>
        private void CreateSampleCards()
        {
            // 为庄家创建3张示例卡牌
            for (int i = 0; i < 3; i++)
            {
                CreateSampleCard(bankerCardArea, $"庄{i+1}", bankerCards);
            }

            // 为闲家创建3张示例卡牌
            for (int i = 0; i < 3; i++)
            {
                CreateSampleCard(playerCardArea, $"闲{i+1}", playerCards);
            }
        }

        /// <summary>
        /// 创建示例卡牌
        /// </summary>
        private void CreateSampleCard(Transform parent, string cardName, List<GameObject> cardList)
        {
            GameObject cardObj = new GameObject($"Card_{cardName}");
            cardObj.transform.SetParent(parent);

            RectTransform cardRect = cardObj.AddComponent<RectTransform>();
            cardRect.sizeDelta = cardSize;

            // 卡牌背景
            Image cardImage = cardObj.AddComponent<Image>();
            cardImage.color = cardBackColor;
            cardImage.sprite = CreateSimpleSprite();

            // 添加卡牌边框
            Outline cardOutline = cardObj.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            cardOutline.effectDistance = new Vector2(1, -1);

            // 添加阴影
            Shadow cardShadow = cardObj.AddComponent<Shadow>();
            cardShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            cardShadow.effectDistance = new Vector2(2, -2);

            // 卡牌文字
            CreateCardText(cardObj, cardName);

            // 添加到列表
            cardList.Add(cardObj);

            // 添加点击事件
            Button cardButton = cardObj.AddComponent<Button>();
            cardButton.onClick.AddListener(() => OnCardClick(cardName));
        }

        /// <summary>
        /// 创建卡牌文字
        /// </summary>
        private void CreateCardText(GameObject parent, string cardText)
        {
            GameObject textObj = new GameObject("CardText");
            textObj.transform.SetParent(parent.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text text = textObj.AddComponent<Text>();
            text.text = cardText;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize - 2;
            text.fontStyle = FontStyle.Bold;

            // 文字阴影
            Shadow textShadow = textObj.AddComponent<Shadow>();
            textShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            textShadow.effectDistance = new Vector2(1, -1);
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 卡牌点击事件
        /// </summary>
        private void OnCardClick(string cardName)
        {
            Debug.Log($"[CardRevealEffect] 点击了卡牌: {cardName}");
            
            // 这里可以添加卡牌点击的视觉反馈
            // 比如改变颜色、播放动画等
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void HidePanel()
        {
            if (maskLayer != null) maskLayer.SetActive(false);
            if (cardPanel != null) cardPanel.SetActive(false);
            Debug.Log("[CardRevealEffect] 面板已隐藏");
        }

        /// <summary>
        /// 显示面板
        /// </summary>
        public void ShowPanel()
        {
            if (maskLayer != null) maskLayer.SetActive(true);
            if (cardPanel != null) cardPanel.SetActive(true);
            Debug.Log("[CardRevealEffect] 面板已显示");
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
        /// 清除所有卡牌
        /// </summary>
        public void ClearAllCards()
        {
            // 清除庄家卡牌
            foreach (GameObject card in bankerCards)
            {
                if (card != null)
                {
                    if (Application.isPlaying)
                        Destroy(card);
                    else
                        DestroyImmediate(card);
                }
            }
            bankerCards.Clear();

            // 清除闲家卡牌
            foreach (GameObject card in playerCards)
            {
                if (card != null)
                {
                    if (Application.isPlaying)
                        Destroy(card);
                    else
                        DestroyImmediate(card);
                }
            }
            playerCards.Clear();

            Debug.Log("[CardRevealEffect] 所有卡牌已清除");
        }

        /// <summary>
        /// 重新创建示例卡牌
        /// </summary>
        public void RecreateCards()
        {
            ClearAllCards();
            CreateSampleCards();
        }

        /// <summary>
        /// 添加庄家卡牌
        /// </summary>
        public void AddBankerCard(string cardName = "")
        {
            if (string.IsNullOrEmpty(cardName))
                cardName = $"庄{bankerCards.Count + 1}";
            
            CreateSampleCard(bankerCardArea, cardName, bankerCards);
        }

        /// <summary>
        /// 添加闲家卡牌
        /// </summary>
        public void AddPlayerCard(string cardName = "")
        {
            if (string.IsNullOrEmpty(cardName))
                cardName = $"闲{playerCards.Count + 1}";
            
            CreateSampleCard(playerCardArea, cardName, playerCards);
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

            bankerCards.Clear();
            playerCards.Clear();
            uiCreated = false;
            CreateUI();
            CreateSampleCards();
        }

        /// <summary>
        /// 显示组件状态
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowStatus()
        {
            Debug.Log($"[CardRevealEffect] UI已创建: {uiCreated}");
            Debug.Log($"[CardRevealEffect] 遮罩层: {(maskLayer != null ? "✓" : "✗")}");
            Debug.Log($"[CardRevealEffect] 卡牌面板: {(cardPanel != null ? "✓" : "✗")}");
            Debug.Log($"[CardRevealEffect] 庄家卡牌: {bankerCards.Count}张");
            Debug.Log($"[CardRevealEffect] 闲家卡牌: {playerCards.Count}张");
        }

        /// <summary>
        /// 测试添加卡牌
        /// </summary>
        [ContextMenu("测试添加卡牌")]
        public void TestAddCards()
        {
            if (Application.isPlaying)
            {
                AddBankerCard("测试庄");
                AddPlayerCard("测试闲");
            }
        }

        #endregion
    }
}
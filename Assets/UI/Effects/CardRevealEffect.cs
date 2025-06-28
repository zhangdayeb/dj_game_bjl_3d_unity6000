using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace BaccaratGame.UI.Components.VideoOverlay
{
    /// <summary>
    /// 简单开牌显示组件 - 先确保基础功能正常
    /// </summary>
    public class CardRevealEffect : MonoBehaviour
    {
        #region 配置参数

        [Header("🎨 基础配置")]
        public Vector2 cardSize = new Vector2(160, 224); // 放大扑克牌
        public float cardSpacing = 10f; // 减小间距
        
        [Header("🎯 测试配置")]
        public bool showOnStart = true;
        public float showDelay = 1f;

        #endregion

        #region 私有字段

        private GameObject mainPanel;
        private Transform bankerCardArea;
        private Transform playerCardArea;
        private Text resultText;
        
        private List<GameObject> bankerCards = new List<GameObject>();
        private List<GameObject> playerCards = new List<GameObject>();
        
        // 扑克牌映射
        private Dictionary<string, string> cardMapping = new Dictionary<string, string>();

        #endregion

        #region 初始化

        private void Awake()
        {
            InitCardMapping();
            CreateSimpleUI();
            
            if (showOnStart && Application.isPlaying)
            {
                Invoke(nameof(ShowTestCards), showDelay);
            }
        }

        private void InitCardMapping()
        {
            // 黑桃 (Spades) -> 01-13
            for (int i = 1; i <= 13; i++)
            {
                cardMapping[$"s{i}"] = $"Images/poker/{i:D2}";
            }
            
            // 红桃 (Hearts) -> h1-h13
            for (int i = 1; i <= 13; i++)
            {
                cardMapping[$"h{i}"] = $"Images/poker/h{i}";
            }
            
            // 方块 (Diamonds/f) -> f1-f13
            for (int i = 1; i <= 13; i++)
            {
                cardMapping[$"f{i}"] = $"Images/poker/f{i}";
            }
            
            // 梅花 (Clubs) -> m1-m13
            for (int i = 1; i <= 13; i++)
            {
                cardMapping[$"m{i}"] = $"Images/poker/m{i}";
            }
            
            // 卡背
            cardMapping["back"] = "Images/poker/00";
        }

        #endregion

        #region UI创建

        private void CreateSimpleUI()
        {
            CreateCanvas();
            CreateMainPanel();
            CreateBankerArea();
            CreatePlayerArea();
            CreateResultArea();
        }

        private void CreateCanvas()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("CardRevealCanvas");
                canvasObj.transform.SetParent(transform.parent);
                
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 9999;
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasObj.AddComponent<GraphicRaycaster>();
                transform.SetParent(canvasObj.transform);
            }

            RectTransform rect = GetComponent<RectTransform>();
            if (rect == null) rect = gameObject.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void CreateMainPanel()
        {
            mainPanel = new GameObject("CardRevealPanel");
            mainPanel.transform.SetParent(transform);

            RectTransform panelRect = mainPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(800, 500);
            panelRect.anchoredPosition = Vector2.zero;

            // 简单黑色背景
            Image panelBg = mainPanel.AddComponent<Image>();
            panelBg.color = new Color(0, 0, 0, 0.9f);
            panelBg.sprite = CreateSimpleSprite();

            // 白色边框
            Outline border = mainPanel.AddComponent<Outline>();
            border.effectColor = Color.white;
            border.effectDistance = new Vector2(2, -2);
        }

        private void CreateBankerArea()
        {
            GameObject bankerArea = new GameObject("BankerArea");
            bankerArea.transform.SetParent(mainPanel.transform);

            RectTransform bankerRect = bankerArea.AddComponent<RectTransform>();
            bankerRect.anchorMin = new Vector2(0.05f, 0.6f);
            bankerRect.anchorMax = new Vector2(0.95f, 0.9f);
            bankerRect.offsetMin = Vector2.zero;
            bankerRect.offsetMax = Vector2.zero;

            // 红色背景
            Image bankerBg = bankerArea.AddComponent<Image>();
            bankerBg.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
            bankerBg.sprite = CreateSimpleSprite();

            // 标题
            CreateLabel(bankerArea, "庄家", new Vector2(0.02f, 0.7f), new Vector2(0.3f, 1f));

            // 卡牌容器
            bankerCardArea = CreateCardContainer(bankerArea, "BankerCards");
        }

        private void CreatePlayerArea()
        {
            GameObject playerArea = new GameObject("PlayerArea");
            playerArea.transform.SetParent(mainPanel.transform);

            RectTransform playerRect = playerArea.AddComponent<RectTransform>();
            playerRect.anchorMin = new Vector2(0.05f, 0.3f);
            playerRect.anchorMax = new Vector2(0.95f, 0.6f);
            playerRect.offsetMin = Vector2.zero;
            playerRect.offsetMax = Vector2.zero;

            // 蓝色背景
            Image playerBg = playerArea.AddComponent<Image>();
            playerBg.color = new Color(0.2f, 0.2f, 0.8f, 0.8f);
            playerBg.sprite = CreateSimpleSprite();

            // 标题
            CreateLabel(playerArea, "闲家", new Vector2(0.02f, 0.7f), new Vector2(0.3f, 1f));

            // 卡牌容器
            playerCardArea = CreateCardContainer(playerArea, "PlayerCards");
        }

        private void CreateResultArea()
        {
            GameObject resultArea = new GameObject("ResultArea");
            resultArea.transform.SetParent(mainPanel.transform);

            RectTransform resultRect = resultArea.AddComponent<RectTransform>();
            resultRect.anchorMin = new Vector2(0.1f, 0.05f);
            resultRect.anchorMax = new Vector2(0.9f, 0.25f);
            resultRect.offsetMin = Vector2.zero;
            resultRect.offsetMax = Vector2.zero;

            // 结果背景
            Image resultBg = resultArea.AddComponent<Image>();
            resultBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            resultBg.sprite = CreateSimpleSprite();

            // 结果文字
            GameObject resultTextObj = new GameObject("ResultText");
            resultTextObj.transform.SetParent(resultArea.transform);

            RectTransform resultTextRect = resultTextObj.AddComponent<RectTransform>();
            resultTextRect.anchorMin = Vector2.zero;
            resultTextRect.anchorMax = Vector2.one;
            resultTextRect.offsetMin = Vector2.zero;
            resultTextRect.offsetMax = Vector2.zero;

            resultText = resultTextObj.AddComponent<Text>();
            resultText.text = "结果显示区域";
            resultText.color = Color.yellow;
            resultText.alignment = TextAnchor.MiddleCenter;
            resultText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            resultText.fontSize = 24;
            resultText.fontStyle = FontStyle.Bold;
        }

        private void CreateLabel(GameObject parent, string text, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(parent.transform);

            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = anchorMin;
            labelRect.anchorMax = anchorMax;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            Text label = labelObj.AddComponent<Text>();
            label.text = text;
            label.color = Color.white;
            label.alignment = TextAnchor.MiddleCenter;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 18;
            label.fontStyle = FontStyle.Bold;
        }

        private Transform CreateCardContainer(GameObject parent, string name)
        {
            GameObject container = new GameObject(name);
            container.transform.SetParent(parent.transform);

            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.3f, 0f);
            containerRect.anchorMax = new Vector2(1f, 0.7f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = cardSpacing;
            layout.childControlHeight = true;
            layout.childControlWidth = false;
            layout.childForceExpandHeight = true;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.padding = new RectOffset(10, 10, 5, 5);

            return container.transform;
        }

        #endregion

        #region 卡牌创建

        private GameObject CreateSimpleCard(Transform parent, string cardKey)
        {
            GameObject cardObj = new GameObject($"Card_{cardKey}");
            cardObj.transform.SetParent(parent);

            RectTransform cardRect = cardObj.AddComponent<RectTransform>();
            cardRect.sizeDelta = cardSize;

            Image cardImg = cardObj.AddComponent<Image>();
            cardImg.preserveAspect = true;
            
            // 加载扑克牌图片
            Sprite cardSprite = LoadCardSprite(cardKey);
            cardImg.sprite = cardSprite;

            // 简单边框
            Outline cardBorder = cardObj.AddComponent<Outline>();
            cardBorder.effectColor = Color.white;
            cardBorder.effectDistance = new Vector2(1, -1);

            return cardObj;
        }

        private Sprite LoadCardSprite(string cardKey)
        {
            if (cardMapping.TryGetValue(cardKey, out string path))
            {
                Sprite sprite = Resources.Load<Sprite>(path);
                if (sprite != null)
                {
                    return sprite;
                }
            }

            // 创建备用卡牌
            return CreateBackupCard(cardKey);
        }

        private Sprite CreateBackupCard(string cardKey)
        {
            int width = 160; // 增大备用卡牌尺寸
            int height = 224;
            Texture2D tex = new Texture2D(width, height);
            
            // 根据卡牌类型选择颜色
            Color cardColor;
            if (cardKey == "back")
            {
                cardColor = new Color(0.2f, 0.3f, 0.7f, 1f); // 蓝色卡背
            }
            else if (cardKey.StartsWith("h") || cardKey.StartsWith("f"))
            {
                cardColor = new Color(0.9f, 0.1f, 0.1f, 1f); // 红色花色
            }
            else
            {
                cardColor = new Color(0.1f, 0.1f, 0.1f, 1f); // 黑色花色
            }
            
            // 填充颜色
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bool isBorder = x < 3 || x >= width - 3 || y < 3 || y >= height - 3;
                    if (isBorder)
                    {
                        tex.SetPixel(x, y, Color.white); // 白色边框
                    }
                    else
                    {
                        tex.SetPixel(x, y, cardColor); // 卡牌颜色
                    }
                }
            }
            
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), Vector2.one * 0.5f);
        }

        private Sprite CreateSimpleSprite()
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
        }

        #endregion

        #region 公共接口

        public void ShowCards(List<string> playerCardKeys, List<string> bankerCardKeys, string result)
        {
            ClearCards();
            
            // 显示闲家卡牌
            foreach (string cardKey in playerCardKeys)
            {
                GameObject card = CreateSimpleCard(playerCardArea, cardKey);
                playerCards.Add(card);
            }

            // 显示庄家卡牌
            foreach (string cardKey in bankerCardKeys)
            {
                GameObject card = CreateSimpleCard(bankerCardArea, cardKey);
                bankerCards.Add(card);
            }

            // 显示结果
            if (resultText != null)
            {
                resultText.text = result;
            }
        }

        public void ShowTestCards()
        {
            List<string> playerCards = new List<string> { "h2", "s10", "f5" }; // 红桃2, 黑桃10, 方块5
            List<string> bankerCards = new List<string> { "f2", "h13" };       // 方块2, 红桃K
            
            ShowCards(playerCards, bankerCards, "🏛️ 庄家获胜！");
        }

        private void ClearCards()
        {
            foreach (GameObject card in bankerCards)
                if (card != null) Destroy(card);
            foreach (GameObject card in playerCards)
                if (card != null) Destroy(card);

            bankerCards.Clear();
            playerCards.Clear();
        }

        [ContextMenu("测试显示卡牌")]
        public void TestShowCards()
        {
            ShowTestCards();
        }

        [ContextMenu("清理卡牌")]
        public void TestClearCards()
        {
            ClearCards();
        }

        #endregion
    }
}
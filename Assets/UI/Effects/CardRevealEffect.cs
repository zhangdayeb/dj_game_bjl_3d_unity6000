using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace BaccaratGame.UI.Components.VideoOverlay
{
    /// <summary>
    /// 简化版开牌特效 - 专注于视觉效果
    /// </summary>
    public class CardRevealEffect : MonoBehaviour
    {
        #region 配置参数

        [Header("🎨 视觉配置")]
        public Vector2 cardSize = new Vector2(120, 168); // 调整为合适的卡牌尺寸
        public float cardSpacing = 20f;
        public Color panelBgColor = new Color(0, 0, 0, 0.85f); // 黑色半透明
        public Color maskColor = new Color(0, 0, 0, 0.6f);
        
        [Header("✨ 动画配置")]
        public float cardFlipDuration = 0.6f;
        public float cardRevealDelay = 0.4f;
        public float resultShowDelay = 1.5f;
        
        [Header("🏆 结果显示")]
        public Color winnerTextColor = new Color(1f, 0.9f, 0.3f, 1f); // 金黄色
        public int resultFontSize = 28;

        #endregion

        #region 默认测试数据

        /// <summary>
        /// 显示默认卡牌 - 用于测试，使用 f2 卡牌
        /// </summary>
        private void ShowDefaultCards()
        {
            List<string> playerCards = new List<string> { "h2", "s10", "f5" }; // 红桃2, 黑桃10, 方块5
            List<string> bankerCards = new List<string> { "f2", "h13" };       // 测试用f2, 红桃K
            ShowCards(playerCards, bankerCards, "🏛️ 庄家获胜！");
        }

        #endregion

        #region 私有字段

        private GameObject maskLayer;
        private GameObject cardPanel;
        private Transform bankerCardArea;
        private Transform playerCardArea;
        private Text resultText;
        
        private List<GameObject> bankerCards = new List<GameObject>();
        private List<GameObject> playerCards = new List<GameObject>();
        
        // 扑克牌图片映射
        private Dictionary<string, string> cardMapping = new Dictionary<string, string>();

        #endregion

        #region 初始化

        private void Awake()
        {
            InitCardMapping();
            CreateUI();
            
            // 显示默认测试数据
            if (Application.isPlaying)
            {
                Invoke(nameof(ShowDefaultCards), 1f);
            }
        }

        /// <summary>
        /// 初始化卡牌图片映射
        /// </summary>
        private void InitCardMapping()
        {
            // 红桃 (Hearts) -> h01-h13
            for (int i = 1; i <= 13; i++)
                cardMapping[$"h{i}"] = $"Images/poker/h{i:D2}";
            
            // 方块 (Diamonds) -> f01-f13  
            for (int i = 1; i <= 13; i++)
                cardMapping[$"d{i}"] = $"Images/poker/f{i:D2}";
            
            // 梅花 (Clubs) -> m01-m13
            for (int i = 1; i <= 13; i++)
                cardMapping[$"c{i}"] = $"Images/poker/m{i:D2}";
            
            // 黑桃 (Spades) -> 01-13
            for (int i = 1; i <= 13; i++)
                cardMapping[$"s{i}"] = $"Images/poker/{i:D2}";
            
            // 卡背
            cardMapping["back"] = "Images/poker/00";
        }

        #endregion

        #region UI创建

        /// <summary>
        /// 创建UI界面
        /// </summary>
        private void CreateUI()
        {
            CreateCanvas();
            CreateMask();
            CreateMainPanel();
            CreateCardAreas();
            CreateResultDisplay();
        }

        /// <summary>
        /// 创建Canvas
        /// </summary>
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

        /// <summary>
        /// 创建遮罩层
        /// </summary>
        private void CreateMask()
        {
            maskLayer = new GameObject("Mask");
            maskLayer.transform.SetParent(transform);

            RectTransform maskRect = maskLayer.AddComponent<RectTransform>();
            maskRect.anchorMin = Vector2.zero;
            maskRect.anchorMax = Vector2.one;
            maskRect.offsetMin = Vector2.zero;
            maskRect.offsetMax = Vector2.zero;

            Image maskImg = maskLayer.AddComponent<Image>();
            maskImg.color = maskColor;
            maskImg.sprite = CreatePixelSprite();

            Button maskBtn = maskLayer.AddComponent<Button>();
            maskBtn.onClick.AddListener(Hide);
        }

        /// <summary>
        /// 创建主面板
        /// </summary>
        private void CreateMainPanel()
        {
            cardPanel = new GameObject("CardPanel");
            cardPanel.transform.SetParent(transform);

            RectTransform panelRect = cardPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(800, 500);
            panelRect.anchoredPosition = Vector2.zero;

            // 背景
            Image panelBg = cardPanel.AddComponent<Image>();
            panelBg.color = panelBgColor;
            panelBg.sprite = CreatePixelSprite();

            // 豪华边框
            Outline border = cardPanel.AddComponent<Outline>();
            border.effectColor = new Color(0.8f, 0.6f, 0.2f, 1f);
            border.effectDistance = new Vector2(4, -4);

            // 阴影
            Shadow shadow = cardPanel.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.6f);
            shadow.effectDistance = new Vector2(8, -8);
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
            GameObject bankerArea = new GameObject("BankerArea");
            bankerArea.transform.SetParent(cardPanel.transform);

            RectTransform bankerRect = bankerArea.AddComponent<RectTransform>();
            bankerRect.anchorMin = new Vector2(0.05f, 0.55f);
            bankerRect.anchorMax = new Vector2(0.95f, 0.85f);
            bankerRect.offsetMin = Vector2.zero;
            bankerRect.offsetMax = Vector2.zero;

            // 背景 - 红色半透明
            Image bankerBg = bankerArea.AddComponent<Image>();
            bankerBg.color = new Color(0.6f, 0.1f, 0.1f, 0.8f); // 红色背景
            bankerBg.sprite = CreatePixelSprite();

            // 边框
            Outline bankerOutline = bankerArea.AddComponent<Outline>();
            bankerOutline.effectColor = new Color(1f, 0.3f, 0.3f, 0.9f); // 红色边框
            bankerOutline.effectDistance = new Vector2(2, -2);

            // 标题
            CreateLabel(bankerArea, "🏛️ 庄家", new Vector2(0.02f, 0.7f), new Vector2(0.98f, 1f));

            // 卡牌容器
            bankerCardArea = CreateCardContainer(bankerArea, "BankerCards");
        }

        /// <summary>
        /// 创建闲家区域
        /// </summary>
        private void CreatePlayerArea()
        {
            GameObject playerArea = new GameObject("PlayerArea");
            playerArea.transform.SetParent(cardPanel.transform);

            RectTransform playerRect = playerArea.AddComponent<RectTransform>();
            playerRect.anchorMin = new Vector2(0.05f, 0.15f);
            playerRect.anchorMax = new Vector2(0.95f, 0.45f);
            playerRect.offsetMin = Vector2.zero;
            playerRect.offsetMax = Vector2.zero;

            // 背景 - 蓝色半透明
            Image playerBg = playerArea.AddComponent<Image>();
            playerBg.color = new Color(0.1f, 0.1f, 0.6f, 0.8f); // 蓝色背景
            playerBg.sprite = CreatePixelSprite();

            // 边框
            Outline playerOutline = playerArea.AddComponent<Outline>();
            playerOutline.effectColor = new Color(0.3f, 0.3f, 1f, 0.9f); // 蓝色边框
            playerOutline.effectDistance = new Vector2(2, -2);

            // 标题
            CreateLabel(playerArea, "👤 闲家", new Vector2(0.02f, 0.7f), new Vector2(0.98f, 1f));

            // 卡牌容器
            playerCardArea = CreateCardContainer(playerArea, "PlayerCards");
        }

        /// <summary>
        /// 创建标签
        /// </summary>
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

            // 文字发光
            Outline outline = labelObj.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.8f);
            outline.effectDistance = new Vector2(2, -2);
        }

        /// <summary>
        /// 创建卡牌容器
        /// </summary>
        private Transform CreateCardContainer(GameObject parent, string name)
        {
            GameObject container = new GameObject(name);
            container.transform.SetParent(parent.transform);

            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.05f, 0.05f);
            containerRect.anchorMax = new Vector2(0.95f, 0.65f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = cardSpacing;
            layout.childControlHeight = true;
            layout.childControlWidth = false;
            layout.childForceExpandHeight = true;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.padding = new RectOffset(10, 10, 10, 10);

            return container.transform;
        }

        /// <summary>
        /// 创建结果显示
        /// </summary>
        private void CreateResultDisplay()
        {
            GameObject resultObj = new GameObject("ResultDisplay");
            resultObj.transform.SetParent(cardPanel.transform);

            RectTransform resultRect = resultObj.AddComponent<RectTransform>();
            resultRect.anchorMin = new Vector2(0.1f, 0.02f);
            resultRect.anchorMax = new Vector2(0.9f, 0.12f);
            resultRect.offsetMin = Vector2.zero;
            resultRect.offsetMax = Vector2.zero;

            resultText = resultObj.AddComponent<Text>();
            resultText.text = "";
            resultText.color = winnerTextColor;
            resultText.alignment = TextAnchor.MiddleCenter;
            resultText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            resultText.fontSize = resultFontSize;
            resultText.fontStyle = FontStyle.Bold;

            // 发光效果
            Outline resultOutline = resultObj.AddComponent<Outline>();
            resultOutline.effectColor = new Color(0.3f, 0.2f, 0.1f, 1f);
            resultOutline.effectDistance = new Vector2(3, -3);

            // 阴影
            Shadow resultShadow = resultObj.AddComponent<Shadow>();
            resultShadow.effectColor = new Color(0, 0, 0, 0.8f);
            resultShadow.effectDistance = new Vector2(2, -2);

            resultObj.SetActive(false);
        }

        #endregion

        #region 卡牌创建与动画

        /// <summary>
        /// 创建卡牌
        /// </summary>
        private GameObject CreateCard(Transform parent, string cardKey, bool isBack = true)
        {
            GameObject cardObj = new GameObject($"Card_{cardKey}");
            cardObj.transform.SetParent(parent);

            RectTransform cardRect = cardObj.AddComponent<RectTransform>();
            cardRect.sizeDelta = cardSize;

            Image cardImg = cardObj.AddComponent<Image>();
            cardImg.preserveAspect = true;
            
            // 初始显示卡背或卡面
            string spriteKey = isBack ? "back" : cardKey;
            cardImg.sprite = LoadCardSprite(spriteKey);

            // 移除文字显示 - 不再添加卡牌文字

            // 卡牌边框 - 更精致的边框
            Outline cardBorder = cardObj.AddComponent<Outline>();
            cardBorder.effectColor = new Color(1f, 1f, 1f, 0.6f); // 白色边框
            cardBorder.effectDistance = new Vector2(2, -2);

            // 柔和阴影
            Shadow cardShadow = cardObj.AddComponent<Shadow>();
            cardShadow.effectColor = new Color(0, 0, 0, 0.4f);
            cardShadow.effectDistance = new Vector2(3, -3);

            // 保存卡牌数据
            CardInfo cardInfo = cardObj.AddComponent<CardInfo>();
            cardInfo.cardKey = cardKey;
            cardInfo.isRevealed = !isBack;

            return cardObj;
        }

        /// <summary>
        /// 翻牌动画
        /// </summary>
        private IEnumerator FlipCard(GameObject card, string targetCardKey)
        {
            Image cardImg = card.GetComponent<Image>();
            Transform cardTransform = card.transform;
            CardInfo cardInfo = card.GetComponent<CardInfo>();

            if (cardImg == null || cardInfo == null) yield break;

            Vector3 originalScale = cardTransform.localScale;
            float halfDuration = cardFlipDuration * 0.5f;

            // 第一阶段：翻转到中间（隐藏）
            float time = 0f;
            while (time < halfDuration)
            {
                time += Time.deltaTime;
                float progress = time / halfDuration;
                
                Vector3 scale = new Vector3(
                    originalScale.x * (1f - progress),
                    originalScale.y,
                    originalScale.z
                );
                cardTransform.localScale = scale;
                
                yield return null;
            }

            // 切换到目标卡牌
            cardImg.sprite = LoadCardSprite(targetCardKey);
            cardInfo.cardKey = targetCardKey;
            cardInfo.isRevealed = true;
            
            // 移除文字更新 - 不再使用文字显示

            // 第二阶段：翻转回来（显示）
            time = 0f;
            while (time < halfDuration)
            {
                time += Time.deltaTime;
                float progress = time / halfDuration;
                
                Vector3 scale = new Vector3(
                    originalScale.x * progress,
                    originalScale.y,
                    originalScale.z
                );
                cardTransform.localScale = scale;
                
                yield return null;
            }

            cardTransform.localScale = originalScale;

            // 发光效果
            StartCoroutine(CardGlow(card));
        }

        /// <summary>
        /// 卡牌发光效果
        /// </summary>
        private IEnumerator CardGlow(GameObject card)
        {
            Outline outline = card.GetComponent<Outline>();
            if (outline == null) yield break;

            Color originalColor = outline.effectColor;
            Color glowColor = new Color(1f, 0.8f, 0.2f, 1f);
            
            float duration = 1f;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float intensity = Mathf.Sin(time * Mathf.PI * 4f) * 0.5f + 0.5f;
                outline.effectColor = Color.Lerp(originalColor, glowColor, intensity * 0.8f);
                yield return null;
            }

            outline.effectColor = originalColor;
        }

        /// <summary>
        /// 结果文字动画
        /// </summary>
        private IEnumerator ShowResultAnimation(string result)
        {
            if (resultText == null) yield break;

            resultText.text = result;
            resultText.gameObject.SetActive(true);

            // 从小放大
            Transform resultTransform = resultText.transform;
            resultTransform.localScale = Vector3.zero;

            float duration = 0.8f;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float progress = time / duration;
                float scale = Mathf.SmoothStep(0f, 1.2f, progress);
                resultTransform.localScale = Vector3.one * scale;
                yield return null;
            }

            // 回到正常大小
            time = 0f;
            while (time < 0.3f)
            {
                time += Time.deltaTime;
                float progress = time / 0.3f;
                float scale = Mathf.Lerp(1.2f, 1f, progress);
                resultTransform.localScale = Vector3.one * scale;
                yield return null;
            }

            resultTransform.localScale = Vector3.one;

            // 持续发光
            StartCoroutine(ResultGlow());
        }

        /// <summary>
        /// 结果文字发光
        /// </summary>
        private IEnumerator ResultGlow()
        {
            Outline outline = resultText.GetComponent<Outline>();
            if (outline == null) yield break;

            Color originalColor = outline.effectColor;
            Color glowColor = new Color(1f, 0.6f, 0f, 1f);

            while (resultText.gameObject.activeInHierarchy)
            {
                float intensity = Mathf.Sin(Time.time * 3f) * 0.5f + 0.5f;
                outline.effectColor = Color.Lerp(originalColor, glowColor, intensity);
                yield return null;
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 加载卡牌图片
        /// </summary>
        private Sprite LoadCardSprite(string cardKey)
        {
            // 尝试加载真实扑克牌图片
            if (cardMapping.TryGetValue(cardKey, out string path))
            {
                Sprite sprite = Resources.Load<Sprite>(path);
                if (sprite != null) 
                {
                    Debug.Log($"[CardReveal] 成功加载扑克牌: {path}");
                    return sprite;
                }
                else
                {
                    Debug.LogWarning($"[CardReveal] 扑克牌图片未找到: {path}");
                }
            }

            // 如果图片加载失败，使用精美的备用卡牌
            return CreateBeautifulCard(cardKey);
        }

        /// <summary>
        /// 获取卡牌颜色
        /// </summary>
        private Color GetCardColor(string cardKey)
        {
            if (cardKey == "back") return new Color(0.1f, 0.2f, 0.6f, 1f); // 深蓝色卡背
            
            // 红色花色：红桃(h)、方块(d)
            if (cardKey.StartsWith("h") || cardKey.StartsWith("d")) 
                return new Color(0.8f, 0.1f, 0.1f, 1f); // 红色
            
            // 黑色花色：梅花(c)、黑桃(s)
            return new Color(0.1f, 0.1f, 0.1f, 1f); // 黑色
        }

        /// <summary>
        /// 创建像素图片
        /// </summary>
        private Sprite CreatePixelSprite()
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
        }

        /// <summary>
        /// 创建精美的备用卡牌
        /// </summary>
        private Sprite CreateBeautifulCard(string cardKey)
        {
            int width = 120;  // 调整备用卡牌尺寸
            int height = 168;
            Texture2D tex = new Texture2D(width, height);
            
            Color cardColor;
            Color borderColor;
            
            if (cardKey == "back")
            {
                // 卡背 - 深蓝渐变
                cardColor = new Color(0.1f, 0.2f, 0.4f, 1f);
                borderColor = new Color(0.3f, 0.4f, 0.6f, 1f);
            }
            else
            {
                // 所有卡牌使用白底
                cardColor = new Color(0.95f, 0.95f, 0.95f, 1f);
                borderColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            }
            
            // 绘制卡牌
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // 边框检测
                    bool isBorder = x < 4 || x >= width - 4 || y < 4 || y >= height - 4;
                    // 圆角检测
                    bool isCorner = (x < 10 && y < 10) || (x >= width - 10 && y < 10) || 
                                   (x < 10 && y >= height - 10) || (x >= width - 10 && y >= height - 10);
                    
                    if (isCorner)
                    {
                        // 圆角透明
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else if (isBorder)
                    {
                        // 边框颜色
                        tex.SetPixel(x, y, borderColor);
                    }
                    else
                    {
                        // 内部颜色
                        tex.SetPixel(x, y, cardColor);
                    }
                }
            }
            
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), Vector2.one * 0.5f);
        }

        /// <summary>
        /// 清理卡牌
        /// </summary>
        private void ClearCards()
        {
            foreach (GameObject card in bankerCards)
                if (card != null) Destroy(card);
            foreach (GameObject card in playerCards)
                if (card != null) Destroy(card);

            bankerCards.Clear();
            playerCards.Clear();

            if (resultText != null)
                resultText.gameObject.SetActive(false);
        }

        /// <summary>
        /// 创建卡牌文字
        /// </summary>
        private void CreateCardText(GameObject cardObj, string text)
        {
            GameObject textObj = new GameObject("CardText");
            textObj.transform.SetParent(cardObj.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text cardText = textObj.AddComponent<Text>();
            cardText.text = text;
            
            // 根据卡牌类型设置文字颜色
            if (text == "?")
            {
                cardText.color = new Color(0.8f, 0.8f, 1f, 1f); // 卡背白色文字
            }
            else if (text.Contains("♥") || text.Contains("♦"))
            {
                cardText.color = new Color(0.8f, 0.1f, 0.1f, 1f); // 红色花色
            }
            else
            {
                cardText.color = new Color(0.1f, 0.1f, 0.1f, 1f); // 黑色花色
            }
            
            cardText.alignment = TextAnchor.MiddleCenter;
            cardText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            cardText.fontSize = 32; // 再次增大字体匹配大卡牌
            cardText.fontStyle = FontStyle.Bold;

            // 文字阴影
            Shadow textShadow = textObj.AddComponent<Shadow>();
            textShadow.effectColor = new Color(1f, 1f, 1f, 0.5f);
            textShadow.effectDistance = new Vector2(1, -1);
        }

        /// <summary>
        /// 获取卡牌显示文字
        /// </summary>
        private string GetCardDisplayText(string cardKey)
        {
            if (cardKey.Length < 2) return "?";

            string suit = cardKey.Substring(0, 1);
            string rank = cardKey.Substring(1);

            // 花色符号
            string suitSymbol = suit switch
            {
                "h" => "♥", // 红桃
                "d" => "♦", // 方块
                "c" => "♣", // 梅花
                "s" => "♠", // 黑桃
                _ => "?"
            };

            // 点数转换
            string rankText = rank switch
            {
                "1" => "A",
                "11" => "J",
                "12" => "Q",
                "13" => "K",
                _ => rank
            };

            return $"{suitSymbol}{rankText}";
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 显示开牌特效
        /// </summary>
        public void ShowCards(List<string> playerCardKeys, List<string> bankerCardKeys, string result)
        {
            ClearCards();
            
            // 显示界面
            if (maskLayer != null) maskLayer.SetActive(true);
            if (cardPanel != null) cardPanel.SetActive(true);

            StartCoroutine(PlayRevealSequence(playerCardKeys, bankerCardKeys, result));
        }

        /// <summary>
        /// 开牌序列
        /// </summary>
        private IEnumerator PlayRevealSequence(List<string> playerCardKeys, List<string> bankerCardKeys, string result)
        {
            // 创建所有卡牌（卡背状态）
            for (int i = 0; i < playerCardKeys.Count; i++)
            {
                GameObject card = CreateCard(playerCardArea, playerCardKeys[i], true);
                playerCards.Add(card);
            }

            for (int i = 0; i < bankerCardKeys.Count; i++)
            {
                GameObject card = CreateCard(bankerCardArea, bankerCardKeys[i], true);
                bankerCards.Add(card);
            }

            yield return new WaitForSeconds(0.5f);

            // 逐张翻牌
            for (int i = 0; i < Mathf.Max(playerCards.Count, bankerCards.Count); i++)
            {
                if (i < playerCards.Count)
                {
                    StartCoroutine(FlipCard(playerCards[i], playerCardKeys[i]));
                }

                yield return new WaitForSeconds(cardRevealDelay * 0.5f);

                if (i < bankerCards.Count)
                {
                    StartCoroutine(FlipCard(bankerCards[i], bankerCardKeys[i]));
                }

                yield return new WaitForSeconds(cardRevealDelay);
            }

            // 显示结果
            yield return new WaitForSeconds(resultShowDelay);
            StartCoroutine(ShowResultAnimation(result));
        }

        /// <summary>
        /// 隐藏界面
        /// </summary>
        public void Hide()
        {
            if (maskLayer != null) maskLayer.SetActive(false);
            if (cardPanel != null) cardPanel.SetActive(false);
        }

        /// <summary>
        /// 快速测试
        /// </summary>
        [ContextMenu("测试开牌")]
        public void TestReveal()
        {
            List<string> playerCards = new List<string> { "h1", "s10" };
            List<string> bankerCards = new List<string> { "d5", "c13", "h7" };
            ShowCards(playerCards, bankerCards, "🏛️ 庄家获胜！");
        }

        #endregion
    }

    /// <summary>
    /// 卡牌信息组件
    /// </summary>
    public class CardInfo : MonoBehaviour
    {
        public string cardKey = "";
        public bool isRevealed = false;
    }
}
// Assets/UI/Effects/CardRevealEffect.cs
// 卡牌显示特效组件 - 完全重写版本
// 职责：处理百家乐游戏中的卡牌发牌、翻牌、发光等视觉特效
// 特点：支持队列处理、多种动画效果、事件驱动

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BaccaratGame.Core.Events;
using BaccaratGame.Data;

namespace BaccaratGame.UI.Effects
{
    /// <summary>
    /// 卡牌显示特效组件
    /// 负责处理百家乐游戏中所有卡牌相关的视觉特效
    /// </summary>
    public class CardRevealEffect : MonoBehaviour
    {
        #region 序列化字段配置

        [Header("🎯 卡牌区域设置")]
        [SerializeField] private Transform playerCardArea;
        [SerializeField] private Transform bankerCardArea;
        [SerializeField] private Transform deckArea; // 牌堆位置

        [Header("🃏 卡牌资源")]
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Sprite cardBackSprite;
        [SerializeField] private Sprite[] cardSprites = new Sprite[52]; // 52张牌的正面图片

        [Header("⚡ 动画设置")]
        [SerializeField] private float dealDuration = 0.8f;
        [SerializeField] private float flipDuration = 0.5f;
        [SerializeField] private float glowDuration = 1.0f;
        [SerializeField] private AnimationCurve dealCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        [SerializeField] private AnimationCurve flipCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

        [Header("🎨 视觉效果")]
        [SerializeField] private Color playerGlowColor = Color.blue;
        [SerializeField] private Color bankerGlowColor = Color.red;
        [SerializeField] private Color winningGlowColor = Color.yellow;
        [SerializeField] private float glowIntensity = 2.0f;

        [Header("🔊 音效设置")]
        [SerializeField] private bool enableSounds = true;
        [SerializeField] private AudioClip dealSound;
        [SerializeField] private AudioClip flipSound;
        [SerializeField] private AudioClip winSound;
        [SerializeField] private float soundVolume = 1.0f;

        [Header("✨ 粒子特效")]
        [SerializeField] private ParticleSystem dealParticles;
        [SerializeField] private ParticleSystem flipParticles;
        [SerializeField] private ParticleSystem winParticles;

        [Header("🏗️ 布局配置")]
        [SerializeField] private Vector2 cardSize = new Vector2(120, 168);
        [SerializeField] private float cardSpacing = 15f;
        [SerializeField] private Vector3 cardStackOffset = new Vector3(2, -2, 0);

        [Header("🐛 调试设置")]
        [SerializeField] private bool enableDebugMode = true;
        [SerializeField] private bool showCardPaths = false;

        #endregion

        #region 私有字段

        // 组件引用
        private AudioSource audioSource;
        private Canvas parentCanvas;

        // 卡牌管理
        private List<GameObject> activeCards = new List<GameObject>();
        private Dictionary<HandType, List<GameObject>> handCards = new Dictionary<HandType, List<GameObject>>();
        private Queue<CardRevealRequest> revealQueue = new Queue<CardRevealRequest>();

        // 状态管理
        private bool isProcessingQueue = false;
        private int totalCardsDealt = 0;
        private Dictionary<string, Sprite> cardSpriteCache = new Dictionary<string, Sprite>();

        // 位置缓存
        private Dictionary<HandType, List<Vector3>> cardPositions = new Dictionary<HandType, List<Vector3>>();
        private Vector3 deckPosition;

        #endregion

        #region 事件定义

        // 卡牌特效事件
        public System.Action<GameObject> OnCardDealt;
        public System.Action<GameObject, Card> OnCardRevealed;
        public System.Action<HandType> OnHandComplete;
        public System.Action OnAllCardsCleared;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            InitializeComponent();
            InitializeCardCache();
            InitializePositions();
            InitializeAnimationCurves();
        }

        private void Start()
        {
            SetupEventListeners();
            ValidateSetup();
        }

        private void OnDestroy()
        {
            CleanupEventListeners();
            ClearAllCards();
        }

        private void OnDrawGizmos()
        {
            if (showCardPaths && Application.isPlaying)
            {
                DrawDebugPaths();
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponent()
        {
            // 获取音频源
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.volume = soundVolume;
            audioSource.playOnAwake = false;

            // 获取Canvas
            parentCanvas = GetComponentInParent<Canvas>();

            // 初始化卡牌字典
            handCards[HandType.Player] = new List<GameObject>();
            handCards[HandType.Banker] = new List<GameObject>();

            if (enableDebugMode)
                Debug.Log("[CardRevealEffect] 组件初始化完成");
        }

        /// <summary>
        /// 初始化卡牌缓存
        /// </summary>
        private void InitializeCardCache()
        {
            if (cardSprites == null || cardSprites.Length == 0)
            {
                Debug.LogWarning("[CardRevealEffect] 卡牌图片数组为空，将使用默认卡背");
                return;
            }

            // 按标准扑克牌顺序缓存卡牌图片
            CardSuit[] suits = { CardSuit.Spades, CardSuit.Hearts, CardSuit.Diamonds, CardSuit.Clubs };
            CardRank[] ranks = { 
                CardRank.Ace, CardRank.Two, CardRank.Three, CardRank.Four, CardRank.Five, CardRank.Six, 
                CardRank.Seven, CardRank.Eight, CardRank.Nine, CardRank.Ten, CardRank.Jack, CardRank.Queen, CardRank.King 
            };

            int spriteIndex = 0;
            foreach (CardSuit suit in suits)
            {
                foreach (CardRank rank in ranks)
                {
                    if (spriteIndex < cardSprites.Length && cardSprites[spriteIndex] != null)
                    {
                        string cardKey = $"{rank}_{suit}";
                        cardSpriteCache[cardKey] = cardSprites[spriteIndex];
                    }
                    spriteIndex++;
                }
            }

            if (enableDebugMode)
                Debug.Log($"[CardRevealEffect] 卡牌缓存初始化完成，共 {cardSpriteCache.Count} 张卡牌");
        }

        /// <summary>
        /// 初始化动画曲线
        /// </summary>
        private void InitializeAnimationCurves()
        {
            // 如果动画曲线为空或没有关键帧，则创建默认曲线
            if (dealCurve == null || dealCurve.keys.Length == 0)
            {
                // 创建缓出动画曲线 (EaseOut效果)
                dealCurve = new AnimationCurve();
                dealCurve.AddKey(0f, 0f);
                dealCurve.AddKey(0.3f, 0.8f);
                dealCurve.AddKey(1f, 1f);
            }

            if (flipCurve == null || flipCurve.keys.Length == 0)
            {
                // 创建缓入缓出动画曲线 (EaseInOut效果)
                flipCurve = new AnimationCurve();
                flipCurve.AddKey(0f, 0f);
                flipCurve.AddKey(0.5f, 0.5f);
                flipCurve.AddKey(1f, 1f);
            }

            if (enableDebugMode)
                Debug.Log("[CardRevealEffect] 动画曲线初始化完成");
        }

        /// <summary>
        /// 初始化位置
        /// </summary>
        private void InitializePositions()
        {
            // 设置牌堆位置
            if (deckArea != null)
            {
                deckPosition = deckArea.position;
            }
            else
            {
                deckPosition = new Vector3(-500, 0, 0); // 默认位置
            }

            // 计算卡牌位置
            CalculateCardPositions();
        }

        /// <summary>
        /// 计算卡牌位置
        /// </summary>
        private void CalculateCardPositions()
        {
            cardPositions[HandType.Player] = CalculateHandPositions(playerCardArea);
            cardPositions[HandType.Banker] = CalculateHandPositions(bankerCardArea);
        }

        /// <summary>
        /// 计算手牌位置
        /// </summary>
        private List<Vector3> CalculateHandPositions(Transform handArea)
        {
            List<Vector3> positions = new List<Vector3>();
            
            if (handArea == null) return positions;

            Vector3 basePosition = handArea.position;
            
            // 最多3张牌的位置
            for (int i = 0; i < 3; i++)
            {
                Vector3 cardPosition = basePosition + new Vector3(i * cardSpacing, 0, 0);
                positions.Add(cardPosition);
            }

            return positions;
        }

        #endregion

        #region 事件系统

        /// <summary>
        /// 设置事件监听
        /// </summary>
        private void SetupEventListeners()
        {
            // 这里可以监听游戏事件
            // 示例：
            // GameEventManager.OnCardDealt += HandleCardDealt;
            // GameEventManager.OnRoundStart += HandleRoundStart;
            // GameEventManager.OnRoundEnd += HandleRoundEnd;
        }

        /// <summary>
        /// 清理事件监听
        /// </summary>
        private void CleanupEventListeners()
        {
            // 取消事件监听
            // GameEventManager.OnCardDealt -= HandleCardDealt;
            // GameEventManager.OnRoundStart -= HandleRoundStart;
            // GameEventManager.OnRoundEnd -= HandleRoundEnd;
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 播放发牌特效
        /// </summary>
        /// <param name="card">卡牌数据</param>
        /// <param name="handType">手牌类型</param>
        /// <param name="revealImmediately">是否立即翻牌</param>
        public void PlayDealEffect(Card card, HandType handType, bool revealImmediately = false)
        {
            var request = new CardRevealRequest
            {
                card = card,
                handType = handType,
                revealImmediately = revealImmediately,
                effectType = CardEffectType.Deal
            };

            revealQueue.Enqueue(request);

            if (!isProcessingQueue)
            {
                StartCoroutine(ProcessRevealQueue());
            }

            if (enableDebugMode)
                Debug.Log($"[CardRevealEffect] 添加发牌请求: {card.Suit}_{card.Rank} -> {handType}");
        }

        /// <summary>
        /// 播放翻牌特效
        /// </summary>
        /// <param name="handType">手牌类型</param>
        /// <param name="cardIndex">卡牌索引</param>
        /// <param name="card">卡牌数据</param>
        public void PlayFlipEffect(HandType handType, int cardIndex, Card card)
        {
            if (handCards.ContainsKey(handType) && cardIndex < handCards[handType].Count)
            {
                GameObject cardObj = handCards[handType][cardIndex];
                if (cardObj != null)
                {
                    StartCoroutine(FlipCardCoroutine(cardObj, card));
                }
            }
        }

        /// <summary>
        /// 播放发光特效
        /// </summary>
        /// <param name="handType">手牌类型</param>
        /// <param name="glowColor">发光颜色</param>
        public void PlayGlowEffect(HandType handType, Color? glowColor = null)
        {
            Color finalColor = glowColor ?? (handType == HandType.Player ? playerGlowColor : bankerGlowColor);

            if (handCards.ContainsKey(handType))
            {
                foreach (GameObject card in handCards[handType])
                {
                    if (card != null)
                    {
                        StartCoroutine(GlowCardCoroutine(card, finalColor));
                    }
                }
            }
        }

        /// <summary>
        /// 播放获胜特效
        /// </summary>
        /// <param name="winningHand">获胜手牌</param>
        public void PlayWinEffect(HandType winningHand)
        {
            PlayGlowEffect(winningHand, winningGlowColor);
            PlaySound(winSound);

            if (winParticles != null)
            {
                Transform targetArea = winningHand == HandType.Player ? playerCardArea : bankerCardArea;
                winParticles.transform.position = targetArea.position;
                winParticles.Play();
            }

            if (enableDebugMode)
                Debug.Log($"[CardRevealEffect] 播放获胜特效: {winningHand}");
        }

        /// <summary>
        /// 清除所有卡牌
        /// </summary>
        public void ClearAllCards()
        {
            StopAllCoroutines();

            foreach (GameObject card in activeCards)
            {
                if (card != null)
                {
                    if (Application.isPlaying)
                        Destroy(card);
                    else
                        DestroyImmediate(card);
                }
            }

            activeCards.Clear();
            handCards[HandType.Player].Clear();
            handCards[HandType.Banker].Clear();
            revealQueue.Clear();
            isProcessingQueue = false;
            totalCardsDealt = 0;

            OnAllCardsCleared?.Invoke();

            if (enableDebugMode)
                Debug.Log("[CardRevealEffect] 所有卡牌已清除");
        }

        /// <summary>
        /// 清除指定手牌
        /// </summary>
        /// <param name="handType">手牌类型</param>
        public void ClearHandCards(HandType handType)
        {
            if (!handCards.ContainsKey(handType)) return;

            foreach (GameObject card in handCards[handType])
            {
                if (card != null)
                {
                    activeCards.Remove(card);
                    if (Application.isPlaying)
                        Destroy(card);
                    else
                        DestroyImmediate(card);
                }
            }

            handCards[handType].Clear();

            if (enableDebugMode)
                Debug.Log($"[CardRevealEffect] {handType} 手牌已清除");
        }

        #endregion

        #region 协程动画

        /// <summary>
        /// 处理发牌队列
        /// </summary>
        private IEnumerator ProcessRevealQueue()
        {
            isProcessingQueue = true;

            while (revealQueue.Count > 0)
            {
                CardRevealRequest request = revealQueue.Dequeue();
                yield return StartCoroutine(ExecuteCardRequest(request));

                // 卡牌间隔时间
                yield return new WaitForSeconds(0.3f);
            }

            isProcessingQueue = false;
        }

        /// <summary>
        /// 执行卡牌请求
        /// </summary>
        private IEnumerator ExecuteCardRequest(CardRevealRequest request)
        {
            switch (request.effectType)
            {
                case CardEffectType.Deal:
                    yield return StartCoroutine(DealCardCoroutine(request));
                    break;
                case CardEffectType.Flip:
                    // 翻牌逻辑
                    break;
                case CardEffectType.Glow:
                    // 发光逻辑
                    break;
            }
        }

        /// <summary>
        /// 发牌协程
        /// </summary>
        private IEnumerator DealCardCoroutine(CardRevealRequest request)
        {
            // 创建卡牌对象
            GameObject cardObj = CreateCardObject(request.card, request.handType);
            if (cardObj == null) yield break;

            // 设置初始位置（牌堆）
            cardObj.transform.position = deckPosition;

            // 计算目标位置
            Vector3 targetPosition = GetTargetPosition(request.handType, handCards[request.handType].Count);

            // 播放发牌音效
            PlaySound(dealSound);

            // 播放粒子特效
            if (dealParticles != null)
            {
                dealParticles.transform.position = deckPosition;
                dealParticles.Play();
            }

            // 执行发牌动画
            yield return StartCoroutine(AnimateCardMovement(cardObj, targetPosition, dealDuration, dealCurve));

            // 添加到手牌列表
            handCards[request.handType].Add(cardObj);
            activeCards.Add(cardObj);
            totalCardsDealt++;

            // 触发事件
            OnCardDealt?.Invoke(cardObj);

            // 如果需要立即翻牌
            if (request.revealImmediately)
            {
                yield return new WaitForSeconds(0.2f);
                yield return StartCoroutine(FlipCardCoroutine(cardObj, request.card));
            }

            if (enableDebugMode)
                Debug.Log($"[CardRevealEffect] 发牌完成: {request.card.Suit}_{request.card.Rank}");
        }

        /// <summary>
        /// 翻牌协程
        /// </summary>
        private IEnumerator FlipCardCoroutine(GameObject cardObj, Card card)
        {
            if (cardObj == null) yield break;

            Image cardImage = cardObj.GetComponent<Image>();
            if (cardImage == null) yield break;

            // 播放翻牌音效
            PlaySound(flipSound);

            // 播放粒子特效
            if (flipParticles != null)
            {
                flipParticles.transform.position = cardObj.transform.position;
                flipParticles.Play();
            }

            // 翻牌动画：先缩小到0
            float halfFlipTime = flipDuration / 2f;
            Vector3 originalScale = cardObj.transform.localScale;

            // 缩小阶段
            float elapsedTime = 0f;
            while (elapsedTime < halfFlipTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / halfFlipTime;
                float scaleX = Mathf.Lerp(1f, 0f, flipCurve.Evaluate(t));
                cardObj.transform.localScale = new Vector3(scaleX, originalScale.y, originalScale.z);
                yield return null;
            }

            // 更换卡牌图片
            Sprite cardSprite = GetCardSprite(card);
            if (cardSprite != null)
            {
                cardImage.sprite = cardSprite;
            }

            // 放大阶段
            elapsedTime = 0f;
            while (elapsedTime < halfFlipTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / halfFlipTime;
                float scaleX = Mathf.Lerp(0f, 1f, flipCurve.Evaluate(t));
                cardObj.transform.localScale = new Vector3(scaleX, originalScale.y, originalScale.z);
                yield return null;
            }

            cardObj.transform.localScale = originalScale;

            // 触发事件
            OnCardRevealed?.Invoke(cardObj, card);

            if (enableDebugMode)
                Debug.Log($"[CardRevealEffect] 翻牌完成: {card.Suit}_{card.Rank}");
        }

        /// <summary>
        /// 发光协程
        /// </summary>
        private IEnumerator GlowCardCoroutine(GameObject cardObj, Color glowColor)
        {
            if (cardObj == null) yield break;

            // 添加发光效果组件
            Outline outline = cardObj.GetComponent<Outline>();
            if (outline == null)
            {
                outline = cardObj.AddComponent<Outline>();
            }

            outline.effectColor = glowColor;
            outline.effectDistance = Vector2.one * glowIntensity;

            // 发光动画
            float elapsedTime = 0f;
            while (elapsedTime < glowDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.PingPong(elapsedTime * 2f, 1f);
                Color currentColor = glowColor;
                currentColor.a = alpha;
                outline.effectColor = currentColor;
                yield return null;
            }

            // 移除发光效果
            if (outline != null)
            {
                Destroy(outline);
            }
        }

        /// <summary>
        /// 卡牌移动动画
        /// </summary>
        private IEnumerator AnimateCardMovement(GameObject cardObj, Vector3 targetPosition, float duration, AnimationCurve curve)
        {
            Vector3 startPosition = cardObj.transform.position;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                float curveValue = curve.Evaluate(t);
                
                cardObj.transform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);
                yield return null;
            }

            cardObj.transform.position = targetPosition;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 创建卡牌对象
        /// </summary>
        private GameObject CreateCardObject(Card card, HandType handType)
        {
            GameObject prefab = cardPrefab != null ? cardPrefab : CreateDefaultCardPrefab();
            if (prefab == null) return null;

            Transform parentArea = handType == HandType.Player ? playerCardArea : bankerCardArea;
            GameObject cardObj = Instantiate(prefab, parentArea);

            // 设置卡牌名称
            cardObj.name = $"Card_{card.Suit}_{card.Rank}_{handType}";

            // 设置卡背
            Image cardImage = cardObj.GetComponent<Image>();
            if (cardImage != null)
            {
                cardImage.sprite = cardBackSprite;
            }

            // 设置尺寸
            RectTransform rectTransform = cardObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = cardSize;
            }

            return cardObj;
        }

        /// <summary>
        /// 创建默认卡牌预制体
        /// </summary>
        private GameObject CreateDefaultCardPrefab()
        {
            GameObject card = new GameObject("DefaultCard");
            
            RectTransform rectTransform = card.AddComponent<RectTransform>();
            rectTransform.sizeDelta = cardSize;

            Image cardImage = card.AddComponent<Image>();
            cardImage.sprite = cardBackSprite;

            // 添加阴影效果
            Shadow shadow = card.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(2, -2);

            return card;
        }

        /// <summary>
        /// 获取卡牌图片
        /// </summary>
        private Sprite GetCardSprite(Card card)
        {
            string cardKey = $"{card.Rank}_{card.Suit}";
            return cardSpriteCache.ContainsKey(cardKey) ? cardSpriteCache[cardKey] : cardBackSprite;
        }

        /// <summary>
        /// 获取目标位置
        /// </summary>
        private Vector3 GetTargetPosition(HandType handType, int cardIndex)
        {
            if (cardPositions.ContainsKey(handType) && cardIndex < cardPositions[handType].Count)
            {
                return cardPositions[handType][cardIndex] + cardStackOffset * cardIndex;
            }

            // 默认位置
            Transform targetArea = handType == HandType.Player ? playerCardArea : bankerCardArea;
            return targetArea.position + new Vector3(cardIndex * cardSpacing, 0, 0);
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (enableSounds && audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip, soundVolume);
            }
        }

        /// <summary>
        /// 验证设置
        /// </summary>
        private void ValidateSetup()
        {
            if (playerCardArea == null)
                Debug.LogWarning("[CardRevealEffect] Player卡牌区域未设置");

            if (bankerCardArea == null)
                Debug.LogWarning("[CardRevealEffect] Banker卡牌区域未设置");

            if (cardBackSprite == null)
                Debug.LogWarning("[CardRevealEffect] 卡背图片未设置");

            if (cardSprites == null || cardSprites.Length == 0)
                Debug.LogWarning("[CardRevealEffect] 卡牌图片数组为空");
        }

        /// <summary>
        /// 绘制调试路径
        /// </summary>
        private void DrawDebugPaths()
        {
            if (deckArea != null && playerCardArea != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(deckArea.position, playerCardArea.position);
            }

            if (deckArea != null && bankerCardArea != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(deckArea.position, bankerCardArea.position);
            }
        }

        #endregion

        #region 调试方法

        /// <summary>
        /// 显示调试信息
        /// </summary>
        [ContextMenu("显示调试信息")]
        public void ShowDebugInfo()
        {
            Debug.Log("=== CardRevealEffect 调试信息 ===");
            Debug.Log($"活动卡牌数量: {activeCards.Count}");
            Debug.Log($"Player手牌数量: {handCards[HandType.Player].Count}");
            Debug.Log($"Banker手牌数量: {handCards[HandType.Banker].Count}");
            Debug.Log($"队列中的请求: {revealQueue.Count}");
            Debug.Log($"正在处理队列: {isProcessingQueue}");
            Debug.Log($"总发牌数: {totalCardsDealt}");
            Debug.Log($"缓存的卡牌图片: {cardSpriteCache.Count}");
        }

        /// <summary>
        /// 测试发牌
        /// </summary>
        [ContextMenu("测试发牌")]
        public void TestDealCard()
        {
            if (Application.isPlaying)
            {
                Card testCard = new Card(CardSuit.Hearts, CardRank.Ace);
                PlayDealEffect(testCard, HandType.Player, false);
            }
        }

        /// <summary>
        /// 测试翻牌
        /// </summary>
        [ContextMenu("测试翻牌")]
        public void TestFlipCard()
        {
            if (Application.isPlaying && handCards[HandType.Player].Count > 0)
            {
                Card testCard = new Card(CardSuit.Hearts, CardRank.Ace);
                PlayFlipEffect(HandType.Player, 0, testCard);
            }
        }

        #endregion
    }

    #region 数据结构

    /// <summary>
    /// 卡牌显示请求
    /// </summary>
    [System.Serializable]
    public struct CardRevealRequest
    {
        public Card card;
        public HandType handType;
        public bool revealImmediately;
        public CardEffectType effectType;
    }

    /// <summary>
    /// 卡牌特效类型
    /// </summary>
    public enum CardEffectType
    {
        Deal,    // 发牌
        Flip,    // 翻牌
        Glow,    // 发光
        Remove   // 移除
    }

    /// <summary>
    /// 手牌类型
    /// </summary>
    public enum HandType
    {
        Player,  // 玩家
        Banker   // 庄家
    }

    #endregion
}
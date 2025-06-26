// Assets/UI/Components/BettingArea/SideBetButton.cs
// 边注投注按钮组件 - 按照BankerPlayerButton模式重写
// 创建5个边注按钮：龙7、庄对、幸运6、闲对、熊8
// 创建时间: 2025/6/27

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BaccaratGame.Data;

namespace BaccaratGame.UI.Components
{
    /// <summary>
    /// 边注投注按钮组件
    /// 管理5个边注按钮：龙7、庄对、幸运6、闲对、熊8
    /// </summary>
    public class SideBetButton : MonoBehaviour
    {
        #region 序列化字段

        [Header("容器设置")]
        public Vector2 containerSize = new Vector2(750, 200);
        public float cornerRadius = 10f;
        
        [Header("按钮布局")]
        public Vector2 buttonSize = new Vector2(80, 60);
        public float buttonSpacing = 10f;
        public Vector2 startPosition = new Vector2(0f, 0f);
        
        [Header("图片资源路径")]
        public string spritePath = "Images/BettingButtons/";
        
        [Header("手动分配的Sprite (可选)")]
        public Sprite dragon7Sprite;
        public Sprite bankerPairSprite;
        public Sprite lucky6Sprite;
        public Sprite playerPairSprite;
        public Sprite panda8Sprite;
        
        [Header("调试设置")]
        public bool enableDebugMode = false;

        #endregion

        #region 私有字段

        private RectTransform rectTransform;
        private bool buttonsCreated = false;
        
        // 按钮数据结构
        private SideBetButtonData dragon7ButtonData;
        private SideBetButtonData bankerPairButtonData;
        private SideBetButtonData lucky6ButtonData;
        private SideBetButtonData playerPairButtonData;
        private SideBetButtonData panda8ButtonData;

        /// <summary>
        /// 边注按钮数据结构
        /// </summary>
        [Serializable]
        public class SideBetButtonData
        {
            public GameObject buttonObject;
            public Button button;
            public Image backgroundImage;
            public BaccaratBetType betType;
            public int currentPlayerCount = 0;
            public decimal currentAmount = 0m;
        }

        #endregion

        #region 事件定义

        public System.Action<BaccaratBetType> OnSideBetSelected;

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

            Debug.Log("[SideBetButton] 组件初始化完成");
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
                // 加载龙7图片
                if (dragon7Sprite == null)
                {
                    dragon7Sprite = Resources.Load<Sprite>(spritePath + "long7_normal");
                    if (dragon7Sprite != null)
                        Debug.Log("[SideBetButton] 成功加载 long7_normal");
                }

                // 加载庄对图片
                if (bankerPairSprite == null)
                {
                    bankerPairSprite = Resources.Load<Sprite>(spritePath + "zhuangdui_normal");
                    if (bankerPairSprite != null)
                        Debug.Log("[SideBetButton] 成功加载 zhuangdui_normal");
                }

                // 加载幸运6图片
                if (lucky6Sprite == null)
                {
                    lucky6Sprite = Resources.Load<Sprite>(spritePath + "lucky6_normal");
                    if (lucky6Sprite != null)
                        Debug.Log("[SideBetButton] 成功加载 lucky6_normal");
                }

                // 加载闲对图片
                if (playerPairSprite == null)
                {
                    playerPairSprite = Resources.Load<Sprite>(spritePath + "xiandui_normal");
                    if (playerPairSprite != null)
                        Debug.Log("[SideBetButton] 成功加载 xiandui_normal");
                }

                // 加载熊8图片
                if (panda8Sprite == null)
                {
                    panda8Sprite = Resources.Load<Sprite>(spritePath + "xiong8_normal");
                    if (panda8Sprite != null)
                        Debug.Log("[SideBetButton] 成功加载 xiong8_normal");
                }

                Debug.Log("[SideBetButton] Resources图片加载完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SideBetButton] 从Resources加载图片时出错: {ex.Message}");
            }
        }

        #endregion

        #region 按钮创建

        /// <summary>
        /// 创建所有边注按钮
        /// </summary>
        [ContextMenu("创建所有按钮")]
        public void CreateAllButtons()
        {
            if (buttonsCreated)
            {
                Debug.Log("[SideBetButton] 按钮已存在，先清除");
                ClearAllButtons();
            }

            Debug.Log("[SideBetButton] 开始创建5个边注按钮");

            try
            {
                // 计算按钮位置（水平排列）
                float totalWidth = 5 * buttonSize.x + 4 * buttonSpacing;
                float startX = -totalWidth / 2 + buttonSize.x / 2;

                // 创建龙7按钮
                dragon7ButtonData = CreateButton("Dragon7Button", 
                    startX + 0 * (buttonSize.x + buttonSpacing), 0f, 
                    BaccaratBetType.Dragon7, dragon7Sprite);
                
                // 创建庄对按钮
                bankerPairButtonData = CreateButton("BankerPairButton", 
                    startX + 1 * (buttonSize.x + buttonSpacing), 0f, 
                    BaccaratBetType.BankerPair, bankerPairSprite);
                
                // 创建幸运6按钮
                lucky6ButtonData = CreateButton("Lucky6Button", 
                    startX + 2 * (buttonSize.x + buttonSpacing), 0f, 
                    BaccaratBetType.Lucky6, lucky6Sprite);
                
                // 创建闲对按钮
                playerPairButtonData = CreateButton("PlayerPairButton", 
                    startX + 3 * (buttonSize.x + buttonSpacing), 0f, 
                    BaccaratBetType.PlayerPair, playerPairSprite);
                
                // 创建熊8按钮
                panda8ButtonData = CreateButton("Panda8Button", 
                    startX + 4 * (buttonSize.x + buttonSpacing), 0f, 
                    BaccaratBetType.Panda8, panda8Sprite);

                buttonsCreated = true;
                Debug.Log("[SideBetButton] 5个边注按钮创建完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SideBetButton] 创建按钮时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建单个按钮
        /// </summary>
        private SideBetButtonData CreateButton(string buttonName, float posX, float posY, 
            BaccaratBetType betType, Sprite backgroundSprite)
        {
            // 创建按钮数据
            SideBetButtonData buttonData = new SideBetButtonData();
            buttonData.betType = betType;

            try
            {
                // 创建按钮GameObject
                GameObject buttonObj = new GameObject(buttonName);
                buttonObj.transform.SetParent(transform);
                buttonData.buttonObject = buttonObj;

                // 设置RectTransform
                RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
                buttonRect.sizeDelta = buttonSize;
                buttonRect.anchoredPosition = new Vector2(posX, posY);
                buttonRect.localScale = Vector3.one;

                // 添加背景图片
                Image backgroundImage = buttonObj.AddComponent<Image>();
                if (backgroundSprite != null)
                {
                    backgroundImage.sprite = backgroundSprite;
                    backgroundImage.color = Color.white; // 保持原图颜色
                }
                else
                {
                    // 使用默认颜色
                    backgroundImage.color = Color.gray;
                }
                buttonData.backgroundImage = backgroundImage;

                // 添加Button组件
                Button button = buttonObj.AddComponent<Button>();
                
                // 设置按钮颜色状态
                ColorBlock colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);  // 轻微提亮
                colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);      // 轻微变暗
                colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                colors.colorMultiplier = 1f;
                colors.fadeDuration = 0.1f;
                button.colors = colors;
                
                buttonData.button = button;

                // 设置按钮点击事件
                button.onClick.AddListener(() => OnButtonClicked(betType));

                // 添加边框效果（可选）
                Outline outline = buttonObj.AddComponent<Outline>();
                outline.effectColor = new Color(0, 0, 0, 0.3f);
                outline.effectDistance = new Vector2(1, -1);

                if (enableDebugMode)
                    Debug.Log($"[SideBetButton] 创建按钮: {buttonName} - {betType}");

                return buttonData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SideBetButton] 创建按钮 {buttonName} 时出错: {ex.Message}");
                return buttonData;
            }
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 处理按钮点击事件
        /// </summary>
        private void OnButtonClicked(BaccaratBetType betType)
        {
            if (enableDebugMode)
                Debug.Log($"[SideBetButton] 边注按钮被点击: {betType}");

            // 触发事件
            OnSideBetSelected?.Invoke(betType);
        }

        #endregion

        #region 数据更新

        /// <summary>
        /// 更新按钮数据
        /// </summary>
        public void UpdateButtonData(BaccaratBetType betType, int playerCount, decimal amount)
        {
            SideBetButtonData buttonData = GetButtonData(betType);
            if (buttonData == null) return;

            buttonData.currentPlayerCount = playerCount;
            buttonData.currentAmount = amount;

            if (enableDebugMode)
                Debug.Log($"[SideBetButton] 更新按钮数据: {betType} - {playerCount}人 - ¥{amount}");
        }

        /// <summary>
        /// 获取按钮数据
        /// </summary>
        private SideBetButtonData GetButtonData(BaccaratBetType betType)
        {
            return betType switch
            {
                BaccaratBetType.Dragon7 => dragon7ButtonData,
                BaccaratBetType.BankerPair => bankerPairButtonData,
                BaccaratBetType.Lucky6 => lucky6ButtonData,
                BaccaratBetType.PlayerPair => playerPairButtonData,
                BaccaratBetType.Panda8 => panda8ButtonData,
                _ => null
            };
        }

        /// <summary>
        /// 应用测试数据
        /// </summary>
        [ContextMenu("应用测试数据")]
        public void ApplyTestData()
        {
            if (!buttonsCreated) return;

            // 设置一些测试数据
            UpdateButtonData(BaccaratBetType.Dragon7, 3, 1200m);
            UpdateButtonData(BaccaratBetType.BankerPair, 5, 800m);
            UpdateButtonData(BaccaratBetType.Lucky6, 2, 500m);
            UpdateButtonData(BaccaratBetType.PlayerPair, 4, 950m);
            UpdateButtonData(BaccaratBetType.Panda8, 1, 300m);

            if (enableDebugMode)
                Debug.Log("[SideBetButton] 测试数据应用完成");
        }

        #endregion

        #region 清理

        /// <summary>
        /// 清除所有按钮
        /// </summary>
        [ContextMenu("清除所有按钮")]
        public void ClearAllButtons()
        {
            if (dragon7ButtonData?.buttonObject != null)
            {
                if (Application.isPlaying)
                    Destroy(dragon7ButtonData.buttonObject);
                else
                    DestroyImmediate(dragon7ButtonData.buttonObject);
            }

            if (bankerPairButtonData?.buttonObject != null)
            {
                if (Application.isPlaying)
                    Destroy(bankerPairButtonData.buttonObject);
                else
                    DestroyImmediate(bankerPairButtonData.buttonObject);
            }

            if (lucky6ButtonData?.buttonObject != null)
            {
                if (Application.isPlaying)
                    Destroy(lucky6ButtonData.buttonObject);
                else
                    DestroyImmediate(lucky6ButtonData.buttonObject);
            }

            if (playerPairButtonData?.buttonObject != null)
            {
                if (Application.isPlaying)
                    Destroy(playerPairButtonData.buttonObject);
                else
                    DestroyImmediate(playerPairButtonData.buttonObject);
            }

            if (panda8ButtonData?.buttonObject != null)
            {
                if (Application.isPlaying)
                    Destroy(panda8ButtonData.buttonObject);
                else
                    DestroyImmediate(panda8ButtonData.buttonObject);
            }

            // 清空引用
            dragon7ButtonData = null;
            bankerPairButtonData = null;
            lucky6ButtonData = null;
            playerPairButtonData = null;
            panda8ButtonData = null;

            buttonsCreated = false;

            Debug.Log("[SideBetButton] 所有按钮已清除");
        }

        #endregion

        #region 调试方法

        /// <summary>
        /// 显示组件状态
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowComponentStatus()
        {
            Debug.Log("=== SideBetButton 组件状态 ===");
            Debug.Log($"按钮已创建: {buttonsCreated}");
            Debug.Log($"龙7按钮: {(dragon7ButtonData != null ? "✓" : "✗")}");
            Debug.Log($"庄对按钮: {(bankerPairButtonData != null ? "✓" : "✗")}");
            Debug.Log($"幸运6按钮: {(lucky6ButtonData != null ? "✓" : "✗")}");
            Debug.Log($"闲对按钮: {(playerPairButtonData != null ? "✓" : "✗")}");
            Debug.Log($"熊8按钮: {(panda8ButtonData != null ? "✓" : "✗")}");
        }

        /// <summary>
        /// 测试所有功能
        /// </summary>
        [ContextMenu("测试所有功能")]
        public void TestAllFunctions()
        {
            Debug.Log("[SideBetButton] 开始测试所有功能");
            
            // 重新创建按钮
            CreateAllButtons();
            
            // 显示状态
            ShowComponentStatus();
            
            Debug.Log("[SideBetButton] 功能测试完成");
        }

        #endregion
    }
}
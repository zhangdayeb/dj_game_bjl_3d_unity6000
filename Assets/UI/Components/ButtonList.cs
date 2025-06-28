// Assets/UI/Components/VideoOverlay/Set/ButtonList.cs
// 带遮罩层的垂直按钮列表组件
// 挂载后持久显示，点击遮罩层隐藏
// 创建时间: 2025/6/28

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BaccaratGame.UI.Components.VideoOverlay
{
    /// <summary>
    /// 带遮罩层的垂直按钮列表组件
    /// 自动创建UI并持久显示，支持遮罩层隐藏
    /// </summary>
    public class ButtonList : MonoBehaviour
    {
        #region 配置参数

        [Header("按钮配置")]
        public Vector2 buttonSize = new Vector2(150, 50);
        public float buttonSpacing = 10f;
        public Vector2 panelPosition = new Vector2(400, 200); // 相对屏幕右上角的偏移
        
        [Header("按钮样式")]
        public Color[] buttonColors = {
            new Color(0.2f, 0.6f, 1f, 0.9f),   // 历史记录 - 蓝色
            new Color(0.3f, 0.7f, 0.3f, 0.9f), // 充值 - 绿色
            new Color(1f, 0.6f, 0.2f, 0.9f),   // 提现 - 橙色
            new Color(0.6f, 0.2f, 0.8f, 0.9f)  // 客服 - 紫色
        };
        
        public Color textColor = Color.white;
        public int fontSize = 16;

        [Header("遮罩层设置")]
        public Color maskColor = new Color(0, 0, 0, 0.3f);

        #endregion

        #region 私有字段

        private bool uiCreated = false;
        private GameObject maskLayer;
        private GameObject buttonPanel;
        private Canvas uiCanvas;
        
        private readonly string[] buttonTexts = { "历史记录", "充值", "提现", "客服" };
        private readonly string[] buttonIcons = { "📋", "💰", "💸", "🎧" };

        #endregion

        #region 生命周期

        private void Awake()
        {
            CreateUI();
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
            CreateButtonPanel();
            CreateButtons();
            
            uiCreated = true;
        }

        /// <summary>
        /// 创建Canvas
        /// </summary>
        private void CreateCanvas()
        {
            // 检查是否在Canvas下
            uiCanvas = GetComponentInParent<Canvas>();
            if (uiCanvas == null)
            {
                // 创建新的Canvas
                GameObject canvasObj = new GameObject("ButtonListCanvas");
                canvasObj.transform.SetParent(transform.parent);
                
                uiCanvas = canvasObj.AddComponent<Canvas>();
                uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                uiCanvas.sortingOrder = 1000; // 确保在最上层
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                
                canvasObj.AddComponent<GraphicRaycaster>();
                
                // 移动ButtonList到Canvas下
                transform.SetParent(canvasObj.transform);
            }

            // 设置ButtonList的RectTransform
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

            // 设置为全屏覆盖
            RectTransform maskRect = maskLayer.AddComponent<RectTransform>();
            maskRect.anchorMin = Vector2.zero;
            maskRect.anchorMax = Vector2.one;
            maskRect.offsetMin = Vector2.zero;
            maskRect.offsetMax = Vector2.zero;

            // 添加背景图片（半透明遮罩）
            Image maskImage = maskLayer.AddComponent<Image>();
            maskImage.color = maskColor;
            maskImage.sprite = CreateSimpleSprite();

            // 添加按钮组件用于点击检测
            Button maskButton = maskLayer.AddComponent<Button>();
            maskButton.onClick.AddListener(HidePanel);
            
            // 设置按钮颜色为透明
            ColorBlock colors = maskButton.colors;
            colors.normalColor = Color.clear;
            colors.highlightedColor = Color.clear;
            colors.pressedColor = Color.clear;
            colors.disabledColor = Color.clear;
            maskButton.colors = colors;
        }

        /// <summary>
        /// 创建按钮面板
        /// </summary>
        private void CreateButtonPanel()
        {
            buttonPanel = new GameObject("ButtonPanel");
            buttonPanel.transform.SetParent(transform);

            // 设置面板位置（右上角）
            RectTransform panelRect = buttonPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 1); // 右上角锚点
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(1, 1);
            
            // 计算面板大小
            float panelHeight = (buttonSize.y + buttonSpacing) * buttonTexts.Length - buttonSpacing + 20; // 20为padding
            panelRect.sizeDelta = new Vector2(buttonSize.x + 20, panelHeight);
            panelRect.anchoredPosition = new Vector2(-panelPosition.x, -panelPosition.y);

            // 添加背景
            Image panelBg = buttonPanel.AddComponent<Image>();
            panelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            panelBg.sprite = CreateSimpleSprite();

            // 添加圆角效果（可选）
            // 可以考虑使用Mask组件实现圆角
        }

        /// <summary>
        /// 创建所有按钮
        /// </summary>
        private void CreateButtons()
        {
            for (int i = 0; i < buttonTexts.Length; i++)
            {
                CreateSingleButton(i);
            }
        }

        /// <summary>
        /// 创建单个按钮
        /// </summary>
        private void CreateSingleButton(int index)
        {
            GameObject buttonObj = new GameObject($"Button_{buttonTexts[index]}");
            buttonObj.transform.SetParent(buttonPanel.transform);

            // 设置按钮位置
            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = buttonSize;
            buttonRect.anchorMin = new Vector2(0.5f, 1f); // 顶部中心锚点
            buttonRect.anchorMax = new Vector2(0.5f, 1f);
            buttonRect.pivot = new Vector2(0.5f, 1f);
            
            float yPos = -10 - (index * (buttonSize.y + buttonSpacing)); // 10为顶部padding
            buttonRect.anchoredPosition = new Vector2(0, yPos);

            // 添加按钮背景
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = GetButtonColor(index);
            buttonImage.sprite = CreateSimpleSprite();

            // 添加Button组件
            Button button = buttonObj.AddComponent<Button>();
            SetupButtonColors(button, GetButtonColor(index));
            
            // 添加点击事件（暂时只打印日志）
            button.onClick.AddListener(() => OnButtonClick(buttonTexts[index]));

            // 创建按钮文本
            CreateButtonText(buttonObj, buttonTexts[index], buttonIcons[index]);

            buttonObj.SetActive(true);
        }

        /// <summary>
        /// 获取按钮颜色
        /// </summary>
        private Color GetButtonColor(int index)
        {
            return index < buttonColors.Length ? buttonColors[index] : Color.gray;
        }

        /// <summary>
        /// 设置按钮颜色状态
        /// </summary>
        private void SetupButtonColors(Button button, Color baseColor)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = baseColor;
            colors.highlightedColor = new Color(baseColor.r * 1.1f, baseColor.g * 1.1f, baseColor.b * 1.1f, baseColor.a);
            colors.pressedColor = new Color(baseColor.r * 0.9f, baseColor.g * 0.9f, baseColor.b * 0.9f, baseColor.a);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            button.colors = colors;
        }

        /// <summary>
        /// 创建按钮文本
        /// </summary>
        private void CreateButtonText(GameObject buttonObj, string displayText, string icon)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text text = textObj.AddComponent<Text>();
            text.text = $"{icon} {displayText}";
            text.color = textColor;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
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

        #region 事件处理

        /// <summary>
        /// 按钮点击事件
        /// </summary>
        private void OnButtonClick(string buttonName)
        {
            Debug.Log($"[ButtonList] 点击了按钮: {buttonName}");
            
            // 这里可以根据按钮名称执行相应的功能
            switch (buttonName)
            {
                case "历史记录":
                    // 显示历史记录
                    break;
                case "充值":
                    // 充值功能
                    break;
                case "提现":
                    // 提现功能
                    break;
                case "客服":
                    // 客服功能
                    break;
            }
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void HidePanel()
        {
            if (maskLayer != null) maskLayer.SetActive(false);
            if (buttonPanel != null) buttonPanel.SetActive(false);
            Debug.Log("[ButtonList] 面板已隐藏");
        }

        /// <summary>
        /// 显示面板
        /// </summary>
        public void ShowPanel()
        {
            if (maskLayer != null) maskLayer.SetActive(true);
            if (buttonPanel != null) buttonPanel.SetActive(true);
            Debug.Log("[ButtonList] 面板已显示");
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
        /// 更新按钮位置
        /// </summary>
        public void UpdatePosition(Vector2 newPosition)
        {
            panelPosition = newPosition;
            if (buttonPanel != null)
            {
                RectTransform panelRect = buttonPanel.GetComponent<RectTransform>();
                panelRect.anchoredPosition = new Vector2(-panelPosition.x, -panelPosition.y);
            }
        }

        #endregion

        #region 编辑器辅助

        /// <summary>
        /// 重新创建UI
        /// </summary>
        [ContextMenu("重新创建UI")]
        public void RecreateUI()
        {
            // 清理现有UI
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                    Destroy(transform.GetChild(i).gameObject);
                else
                    DestroyImmediate(transform.GetChild(i).gameObject);
            }

            uiCreated = false;
            CreateUI();
        }

        /// <summary>
        /// 显示组件状态
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowStatus()
        {
            Debug.Log($"[ButtonList] UI已创建: {uiCreated}");
            Debug.Log($"[ButtonList] 遮罩层: {(maskLayer != null ? "✓" : "✗")}");
            Debug.Log($"[ButtonList] 按钮面板: {(buttonPanel != null ? "✓" : "✗")}");
            Debug.Log($"[ButtonList] 按钮数量: {(buttonPanel != null ? buttonPanel.transform.childCount : 0)}");
            Debug.Log($"[ButtonList] 当前显示状态: {gameObject.activeInHierarchy}");
        }

        #endregion
    }
}
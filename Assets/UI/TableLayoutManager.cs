// Assets/UI/Managers/TableLayoutManager.cs
// 游戏桌台布局管理器
// 职责：管理VideoOverlay、BettingArea、Roadmap三个主要区域的布局
// 特点：自上而下布局，Effects浮动在BettingArea上方，响应式设计
// 创建时间: 2025/6/26

using UnityEngine;
using UnityEngine.UI;

namespace BaccaratGame.UI.Managers
{
    /// <summary>
    /// 游戏桌台布局管理器
    /// 负责协调VideoOverlay、BettingArea、Roadmap三个主要区域的位置和大小
    /// Effects区域浮动在BettingArea上方，默认隐藏，开牌/中奖时显示
    /// </summary>
    public class TableLayoutManager : MonoBehaviour
    {
        #region 序列化字段

        [Header("🎯 区域引用")]
        [SerializeField] private RectTransform videoOverlay;
        [SerializeField] private RectTransform bettingArea;
        [SerializeField] private RectTransform roadmap;
        [SerializeField] private RectTransform effects;

        [Header("📐 布局配置")]
        [SerializeField] private Vector2 referenceResolution = new Vector2(750, 1334); // 参考分辨率
        [SerializeField] private float videoHeightRatio = 0.375f;    // 视频区域高度比例 (500/1334)
        [SerializeField] private float bettingHeightRatio = 0.375f;  // 投注区域高度比例 (500/1334)
        [SerializeField] private float roadmapHeightRatio = 0.25f;   // 路单区域高度比例 (334/1334)
        
        [Header("🔧 间距设置")]
        [SerializeField] private float sectionPadding = 5f;          // 区域间间距
        [SerializeField] private float screenPadding = 10f;          // 屏幕边缘间距
        
        [Header("✨ Effects 设置")]
        [SerializeField] private bool effectsFloatAboveBetting = true; // Effects是否浮动在投注区域上方
        [SerializeField] private float effectsOffsetY = 0f;           // Effects垂直偏移
        [SerializeField] private Vector2 effectsSize = new Vector2(400, 300); // Effects默认大小
        
        [Header("🖼️ 视觉设置")]
        [SerializeField] private bool showSectionBorders = true;     // 显示区域边框（调试用）
        [SerializeField] private Color borderColor = Color.cyan;     // 边框颜色
        [SerializeField] private float borderWidth = 2f;             // 边框宽度
        
        [Header("🔧 调试设置")]
        [SerializeField] private bool enableDebugMode = true;        // 启用调试模式
        [SerializeField] private bool autoLayoutOnStart = true;      // 启动时自动布局
        [SerializeField] private bool autoLayoutOnResolutionChange = true; // 分辨率变化时自动重新布局

        #endregion

        #region 私有字段

        private Canvas parentCanvas;
        private RectTransform canvasRect;
        private Vector2 currentScreenSize;
        private bool isLayoutInitialized = false;

        // 区域尺寸缓存
        private Vector2 videoSize;
        private Vector2 bettingSize;
        private Vector2 roadmapSize;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            if (autoLayoutOnStart)
            {
                RefreshLayout();
            }
        }

        private void Update()
        {
            if (autoLayoutOnResolutionChange)
            {
                CheckResolutionChange();
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化组件引用
        /// </summary>
        private void InitializeComponents()
        {
            // 获取Canvas组件
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                canvasRect = parentCanvas.GetComponent<RectTransform>();
            }

            // 自动查找子对象（如果没有手动赋值）
            if (videoOverlay == null)
                videoOverlay = transform.Find("VideoOverlay")?.GetComponent<RectTransform>();
            
            if (bettingArea == null)
                bettingArea = transform.Find("BettingArea")?.GetComponent<RectTransform>();
            
            if (roadmap == null)
                roadmap = transform.Find("Roadmap")?.GetComponent<RectTransform>();
            
            if (effects == null)
                effects = transform.Find("Effects")?.GetComponent<RectTransform>();

            // 记录当前屏幕尺寸
            currentScreenSize = new Vector2(Screen.width, Screen.height);

            if (enableDebugMode)
            {
                Debug.Log("[TableLayoutManager] 组件初始化完成");
                LogComponentStatus();
            }
        }

        #endregion

        #region 布局管理

        /// <summary>
        /// 刷新整体布局
        /// </summary>
        [ContextMenu("刷新布局")]
        public void RefreshLayout()
        {
            if (!ValidateComponents())
            {
                if (enableDebugMode)
                    Debug.LogWarning("[TableLayoutManager] 组件验证失败，无法执行布局");
                return;
            }

            CalculateLayout();
            ApplyLayout();
            SetupEffectsArea();
            
            if (showSectionBorders)
                CreateSectionBorders();

            isLayoutInitialized = true;

            if (enableDebugMode)
            {
                Debug.Log("[TableLayoutManager] 布局刷新完成");
                LogLayoutInfo();
            }
        }

        /// <summary>
        /// 计算布局尺寸
        /// </summary>
        private void CalculateLayout()
        {
            Vector2 canvasSize = canvasRect != null ? canvasRect.sizeDelta : currentScreenSize;
            
            // 计算可用高度（减去屏幕边距和区域间距）
            float availableHeight = canvasSize.y - (screenPadding * 2) - (sectionPadding * 2);
            float availableWidth = canvasSize.x - (screenPadding * 2);

            // 根据比例计算各区域高度
            float videoHeight = availableHeight * videoHeightRatio;
            float bettingHeight = availableHeight * bettingHeightRatio;
            float roadmapHeight = availableHeight * roadmapHeightRatio;

            // 设置区域尺寸
            videoSize = new Vector2(availableWidth, videoHeight);
            bettingSize = new Vector2(availableWidth, bettingHeight);
            roadmapSize = new Vector2(availableWidth, roadmapHeight);

            if (enableDebugMode)
            {
                Debug.Log($"[TableLayoutManager] 计算布局 - Canvas:{canvasSize}, 可用:{availableWidth}x{availableHeight}");
                Debug.Log($"Video:{videoSize}, Betting:{bettingSize}, Roadmap:{roadmapSize}");
            }
        }

        /// <summary>
        /// 应用布局到各个区域
        /// </summary>
        private void ApplyLayout()
        {
            Vector2 canvasSize = canvasRect != null ? canvasRect.sizeDelta : currentScreenSize;
            float startY = (canvasSize.y / 2) - screenPadding;

            // 1. VideoOverlay - 顶部
            if (videoOverlay != null)
            {
                SetupRectTransform(videoOverlay, videoSize, 
                    new Vector2(0, startY - videoSize.y / 2));
            }

            // 2. BettingArea - 中部
            if (bettingArea != null)
            {
                float bettingY = startY - videoSize.y - sectionPadding - bettingSize.y / 2;
                SetupRectTransform(bettingArea, bettingSize, 
                    new Vector2(0, bettingY));
            }

            // 3. Roadmap - 底部
            if (roadmap != null)
            {
                float roadmapY = startY - videoSize.y - bettingSize.y - (sectionPadding * 2) - roadmapSize.y / 2;
                SetupRectTransform(roadmap, roadmapSize, 
                    new Vector2(0, roadmapY));
            }
        }

        /// <summary>
        /// 设置RectTransform属性
        /// </summary>
        private void SetupRectTransform(RectTransform rect, Vector2 size, Vector2 position)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        /// <summary>
        /// 设置Effects区域
        /// </summary>
        private void SetupEffectsArea()
        {
            if (effects == null) return;

            if (effectsFloatAboveBetting && bettingArea != null)
            {
                // Effects浮动在BettingArea上方
                Vector2 bettingPos = bettingArea.anchoredPosition;
                Vector2 effectsPos = new Vector2(bettingPos.x, bettingPos.y + effectsOffsetY);
                
                SetupRectTransform(effects, effectsSize, effectsPos);
                
                // 确保Effects在正确的层级
                effects.SetAsLastSibling();
            }
            else
            {
                // Effects居中显示
                SetupRectTransform(effects, effectsSize, Vector2.zero);
            }

            // 默认隐藏Effects
            effects.gameObject.SetActive(false);

            if (enableDebugMode)
                Debug.Log($"[TableLayoutManager] Effects设置完成 - 位置:{effects.anchoredPosition}, 大小:{effects.sizeDelta}");
        }

        #endregion

        #region Effects管理

        /// <summary>
        /// 显示特效区域
        /// </summary>
        public void ShowEffects()
        {
            if (effects != null)
            {
                effects.gameObject.SetActive(true);
                effects.SetAsLastSibling(); // 确保在最上层
                
                if (enableDebugMode)
                    Debug.Log("[TableLayoutManager] 显示Effects区域");
            }
        }

        /// <summary>
        /// 隐藏特效区域
        /// </summary>
        public void HideEffects()
        {
            if (effects != null)
            {
                effects.gameObject.SetActive(false);
                
                if (enableDebugMode)
                    Debug.Log("[TableLayoutManager] 隐藏Effects区域");
            }
        }

        /// <summary>
        /// 切换特效区域显示状态
        /// </summary>
        public void ToggleEffects()
        {
            if (effects != null)
            {
                if (effects.gameObject.activeInHierarchy)
                    HideEffects();
                else
                    ShowEffects();
            }
        }

        #endregion

        #region 分辨率适配

        /// <summary>
        /// 检查分辨率变化
        /// </summary>
        private void CheckResolutionChange()
        {
            Vector2 newSize = new Vector2(Screen.width, Screen.height);
            
            if (Vector2.Distance(currentScreenSize, newSize) > 1f)
            {
                currentScreenSize = newSize;
                OnResolutionChanged();
            }
        }

        /// <summary>
        /// 分辨率变化处理
        /// </summary>
        private void OnResolutionChanged()
        {
            if (enableDebugMode)
                Debug.Log($"[TableLayoutManager] 分辨率变化: {currentScreenSize}");
            
            RefreshLayout();
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 设置区域高度比例
        /// </summary>
        public void SetHeightRatios(float video, float betting, float roadmap)
        {
            videoHeightRatio = Mathf.Clamp01(video);
            bettingHeightRatio = Mathf.Clamp01(betting);
            roadmapHeightRatio = Mathf.Clamp01(roadmap);
            
            // 确保总比例不超过1
            float totalRatio = videoHeightRatio + bettingHeightRatio + roadmapHeightRatio;
            if (totalRatio > 1f)
            {
                videoHeightRatio /= totalRatio;
                bettingHeightRatio /= totalRatio;
                roadmapHeightRatio /= totalRatio;
            }
            
            if (isLayoutInitialized)
                RefreshLayout();
        }

        /// <summary>
        /// 获取区域信息
        /// </summary>
        public RectTransform GetAreaTransform(string areaName)
        {
            switch (areaName.ToLower())
            {
                case "video":
                case "videooverlay":
                    return videoOverlay;
                case "betting":
                case "bettingarea":
                    return bettingArea;
                case "roadmap":
                    return roadmap;
                case "effects":
                    return effects;
                default:
                    return null;
            }
        }

        /// <summary>
        /// 获取区域大小
        /// </summary>
        public Vector2 GetAreaSize(string areaName)
        {
            RectTransform area = GetAreaTransform(areaName);
            return area != null ? area.sizeDelta : Vector2.zero;
        }

        #endregion

        #region 验证和调试

        /// <summary>
        /// 验证组件引用
        /// </summary>
        private bool ValidateComponents()
        {
            bool isValid = true;
            
            if (videoOverlay == null)
            {
                Debug.LogWarning("[TableLayoutManager] VideoOverlay引用缺失");
                isValid = false;
            }
            
            if (bettingArea == null)
            {
                Debug.LogWarning("[TableLayoutManager] BettingArea引用缺失");
                isValid = false;
            }
            
            if (roadmap == null)
            {
                Debug.LogWarning("[TableLayoutManager] Roadmap引用缺失");
                isValid = false;
            }
            
            if (canvasRect == null)
            {
                Debug.LogWarning("[TableLayoutManager] Canvas引用缺失");
                isValid = false;
            }
            
            return isValid;
        }

        /// <summary>
        /// 创建区域边框（调试用）
        /// </summary>
        private void CreateSectionBorders()
        {
            CreateBorderForArea(videoOverlay, "Video");
            CreateBorderForArea(bettingArea, "Betting");
            CreateBorderForArea(roadmap, "Roadmap");
            if (effects != null && effects.gameObject.activeInHierarchy)
                CreateBorderForArea(effects, "Effects");
        }

        /// <summary>
        /// 为指定区域创建边框
        /// </summary>
        private void CreateBorderForArea(RectTransform area, string areaName)
        {
            if (area == null) return;

            GameObject borderObj = area.Find($"{areaName}Border")?.gameObject;
            if (borderObj == null)
            {
                borderObj = new GameObject($"{areaName}Border");
                borderObj.transform.SetParent(area);
            }

            RectTransform borderRect = borderObj.GetComponent<RectTransform>();
            if (borderRect == null)
                borderRect = borderObj.AddComponent<RectTransform>();

            Image borderImage = borderObj.GetComponent<Image>();
            if (borderImage == null)
                borderImage = borderObj.AddComponent<Image>();

            // 设置边框样式
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;

            borderImage.color = new Color(borderColor.r, borderColor.g, borderColor.b, 0.3f);
            borderImage.type = Image.Type.Sliced;

            // 创建边框线条
            CreateBorderOutline(borderObj.transform, borderColor, borderWidth);
        }

        /// <summary>
        /// 创建边框轮廓
        /// </summary>
        private void CreateBorderOutline(Transform parent, Color color, float width)
        {
            // 这里可以添加更详细的边框绘制逻辑
            // 简化版本：直接使用Image组件的边框效果
        }

        /// <summary>
        /// 输出组件状态
        /// </summary>
        private void LogComponentStatus()
        {
            Debug.Log("=== TableLayoutManager 组件状态 ===");
            Debug.Log($"VideoOverlay: {(videoOverlay != null ? "✓" : "✗")}");
            Debug.Log($"BettingArea: {(bettingArea != null ? "✓" : "✗")}");
            Debug.Log($"Roadmap: {(roadmap != null ? "✓" : "✗")}");
            Debug.Log($"Effects: {(effects != null ? "✓" : "✗")}");
            Debug.Log($"ParentCanvas: {(parentCanvas != null ? "✓" : "✗")}");
        }

        /// <summary>
        /// 输出布局信息
        /// </summary>
        private void LogLayoutInfo()
        {
            Debug.Log("=== TableLayoutManager 布局信息 ===");
            Debug.Log($"屏幕尺寸: {currentScreenSize}");
            Debug.Log($"高度比例: Video={videoHeightRatio:P1}, Betting={bettingHeightRatio:P1}, Roadmap={roadmapHeightRatio:P1}");
            Debug.Log($"区域尺寸: Video={videoSize}, Betting={bettingSize}, Roadmap={roadmapSize}");
            
            if (videoOverlay != null)
                Debug.Log($"Video位置: {videoOverlay.anchoredPosition}");
            if (bettingArea != null)
                Debug.Log($"Betting位置: {bettingArea.anchoredPosition}");
            if (roadmap != null)
                Debug.Log($"Roadmap位置: {roadmap.anchoredPosition}");
            if (effects != null)
                Debug.Log($"Effects位置: {effects.anchoredPosition}, 激活: {effects.gameObject.activeInHierarchy}");
        }

        #endregion

        #region 编辑器方法

        [ContextMenu("重新初始化")]
        public void Reinitialize()
        {
            InitializeComponents();
            RefreshLayout();
        }

        [ContextMenu("显示调试信息")]
        public void ShowDebugInfo()
        {
            LogComponentStatus();
            LogLayoutInfo();
        }

        [ContextMenu("测试显示Effects")]
        public void TestShowEffects()
        {
            ShowEffects();
        }

        [ContextMenu("测试隐藏Effects")]
        public void TestHideEffects()
        {
            HideEffects();
        }

        [ContextMenu("重置为默认比例")]
        public void ResetToDefaultRatios()
        {
            SetHeightRatios(0.375f, 0.375f, 0.25f);
        }

        #endregion
    }
}
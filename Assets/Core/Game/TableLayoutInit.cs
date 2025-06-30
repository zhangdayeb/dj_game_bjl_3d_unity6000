using UnityEngine;
using UnityEngine.UI;

namespace BaccaratGame.Core
{
    /// <summary>
    /// TableLayout 动态布局管理组件
    /// 负责根据屏幕宽度动态调整子布局的高度
    /// </summary>
    public class TableLayoutInit : MonoBehaviour
    {
        [Header("布局配置")]
        [SerializeField] private float bottomHeightRatio = 0.35f; // 底部区域高度系数
        [SerializeField] private float middleFixedHeight = 200f; // 中间区域固定高度
        
        [Header("子布局对象")]
        [SerializeField] private RectTransform topLayout; // 顶部布局 (VideoOverlay)
        [SerializeField] private RectTransform middleLayout; // 中间布局 (BettingAndChipArea)
        [SerializeField] private RectTransform bottomLayout; // 底部布局
        
        private RectTransform tableRectTransform;
        private Canvas parentCanvas;
        private Vector2 lastScreenSize;
        
        void Start()
        {
            InitializeComponents();
            UpdateLayout();
        }
        
        void Update()
        {
            // 检测屏幕尺寸变化
            Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
            if (lastScreenSize != currentScreenSize)
            {
                lastScreenSize = currentScreenSize;
                UpdateLayout();
            }
        }
        
        /// <summary>
        /// 初始化组件引用
        /// </summary>
        private void InitializeComponents()
        {
            tableRectTransform = GetComponent<RectTransform>();
            parentCanvas = GetComponentInParent<Canvas>();
            
            // 如果未手动设置子布局，尝试自动获取
            if (topLayout == null || middleLayout == null || bottomLayout == null)
            {
                AutoAssignChildLayouts();
            }
            
            lastScreenSize = new Vector2(Screen.width, Screen.height);
        }
        
        /// <summary>
        /// 自动分配子布局
        /// </summary>
        private void AutoAssignChildLayouts()
        {
            if (transform.childCount >= 3)
            {
                topLayout = transform.GetChild(0) as RectTransform;
                middleLayout = transform.GetChild(1) as RectTransform;
                bottomLayout = transform.GetChild(2) as RectTransform;
                
                Debug.Log($"[TableLayoutInit] 自动分配子布局: " +
                         $"顶部={topLayout?.name}, 中间={middleLayout?.name}, 底部={bottomLayout?.name}");
            }
            else
            {
                Debug.LogError("[TableLayoutInit] TableLayout 需要至少3个子对象");
            }
        }
        
        /// <summary>
        /// 更新布局
        /// </summary>
        public void UpdateLayout()
        {
            if (!ValidateComponents()) return;
            
            // 获取当前画布的实际宽度
            float canvasWidth = GetCanvasWidth();
            
            // 计算各区域高度
            float bottomHeight = canvasWidth * bottomHeightRatio;
            float totalHeight = tableRectTransform.rect.height;
            float topHeight = totalHeight - middleFixedHeight - bottomHeight;
            
            // 确保顶部高度不为负数
            if (topHeight < 0)
            {
                topHeight = 0;
                bottomHeight = totalHeight - middleFixedHeight;
            }
            
            // 应用布局
            ApplyLayout(topHeight, middleFixedHeight, bottomHeight, totalHeight);
            
            Debug.Log($"[TableLayoutInit] 布局更新 - 画布宽度: {canvasWidth:F2}, " +
                     $"顶部: {topHeight:F2}, 中间: {middleFixedHeight:F2}, 底部: {bottomHeight:F2}");
        }
        
        /// <summary>
        /// 获取画布宽度
        /// </summary>
        private float GetCanvasWidth()
        {
            if (parentCanvas != null)
            {
                RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
                if (canvasRect != null)
                {
                    return canvasRect.rect.width;
                }
            }
            
            // 如果没有找到Canvas，使用屏幕宽度
            return Screen.width;
        }
        
        /// <summary>
        /// 应用布局设置
        /// </summary>
        private void ApplyLayout(float topHeight, float middleHeight, float bottomHeight, float totalHeight)
        {
            float currentY = totalHeight / 2f; // 从顶部开始
            
            // 设置顶部布局
            SetLayoutRect(topLayout, 0, currentY - topHeight / 2f, 0, topHeight);
            currentY -= topHeight;
            
            // 设置中间布局
            SetLayoutRect(middleLayout, 0, currentY - middleHeight / 2f, 0, middleHeight);
            currentY -= middleHeight;
            
            // 设置底部布局
            SetLayoutRect(bottomLayout, 0, currentY - bottomHeight / 2f, 0, bottomHeight);
        }
        
        /// <summary>
        /// 设置RectTransform的位置和尺寸
        /// </summary>
        private void SetLayoutRect(RectTransform rectTransform, float anchoredX, float anchoredY, float width, float height)
        {
            if (rectTransform == null) return;
            
            // 设置锚点为拉伸模式
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.anchorMax = new Vector2(1, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            
            // 设置位置和尺寸
            rectTransform.anchoredPosition = new Vector2(anchoredX, anchoredY);
            rectTransform.sizeDelta = new Vector2(width, height);
        }
        
        /// <summary>
        /// 验证组件完整性
        /// </summary>
        private bool ValidateComponents()
        {
            if (tableRectTransform == null)
            {
                Debug.LogError("[TableLayoutInit] TableLayout RectTransform 未找到");
                return false;
            }
            
            if (topLayout == null || middleLayout == null || bottomLayout == null)
            {
                Debug.LogError("[TableLayoutInit] 子布局对象未完整设置");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 设置底部高度系数
        /// </summary>
        public void SetBottomHeightRatio(float ratio)
        {
            bottomHeightRatio = Mathf.Clamp(ratio, 0.1f, 0.8f);
            UpdateLayout();
        }
        
        /// <summary>
        /// 设置中间固定高度
        /// </summary>
        public void SetMiddleFixedHeight(float height)
        {
            middleFixedHeight = Mathf.Max(height, 0);
            UpdateLayout();
        }
        
        /// <summary>
        /// 强制刷新布局
        /// </summary>
        public void RefreshLayout()
        {
            UpdateLayout();
        }
        
        /// <summary>
        /// 在Inspector中显示调试信息
        /// </summary>
        void OnValidate()
        {
            if (Application.isPlaying)
            {
                UpdateLayout();
            }
        }
    }
}
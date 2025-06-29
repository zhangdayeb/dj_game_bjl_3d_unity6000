// Assets/Core/UI/EventsUIManager.cs
// UI事件管理器 - 处理网络事件与UI组件的联动
// 挂载到空白GameObject上，通过拖拽设置UI组件引用

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BaccaratGame.Core
{
    /// <summary>
    /// UI事件管理器
    /// 订阅NetworkEvents的事件，控制UI组件的显示和隐藏
    /// 需要挂载到空白GameObject上
    /// </summary>
    public class EventsUIManager : MonoBehaviour
    {
        #region 配置参数

        [Header("⏰ 时间组件")]
        [Tooltip("拖拽整个Timer节点")]
        public GameObject timerNode;

        [Header("📊 状态组件")]
        [Tooltip("拖拽整个StatusDisplay节点")]
        public GameObject statusNode;

        [Header("🃏 开牌组件")]
        [Tooltip("拖拽开牌相关组件节点")]
        public GameObject dealCardsNode;

        [Header("🎯 中奖组件")]
        [Tooltip("拖拽中奖相关组件节点")]
        public GameObject winResultNode;

        [Header("⚙️ 自动隐藏配置")]
        [Tooltip("状态组件显示时长（秒）")]
        public float statusDisplayDuration = 5f;
        
        [Tooltip("开牌组件显示时长（秒）")]
        public float dealCardsDisplayDuration = 5f;
        
        [Tooltip("中奖组件显示时长（秒）")]
        public float winResultDisplayDuration = 5f;

        #endregion

        #region 私有字段

        // UI组件引用
        private Text timerText;          // 倒计时文本
        private Text statusText;         // 状态文本
        
        // 协程引用
        private Coroutine hideStatusCoroutine;
        private Coroutine hideDealCardsCoroutine;
        private Coroutine hideWinResultCoroutine;
        
        // 状态管理
        private bool isTimerActive = false;
        private string currentGamePhase = "";

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            Debug.Log("[EventsUIManager] UI事件管理器初始化");
            InitializeUIReferences();
        }

        private void OnEnable()
        {
            // 订阅网络事件
            NetworkEvents.OnCountdownReceived += HandleCountdownReceived;
            NetworkEvents.OnDealCardsReceived += HandleDealCardsReceived;
            NetworkEvents.OnGameResultReceived += HandleGameResultReceived;
            
            Debug.Log("[EventsUIManager] 网络事件订阅完成");
        }

        private void OnDisable()
        {
            // 取消订阅网络事件
            NetworkEvents.OnCountdownReceived -= HandleCountdownReceived;
            NetworkEvents.OnDealCardsReceived -= HandleDealCardsReceived;
            NetworkEvents.OnGameResultReceived -= HandleGameResultReceived;
            
            Debug.Log("[EventsUIManager] 网络事件订阅已取消");
        }

        private void Start()
        {
            // 初始化UI状态
            InitializeUIState();
        }

        #endregion

        #region 初始化方法

        /// <summary>
        /// 初始化UI组件引用
        /// </summary>
        private void InitializeUIReferences()
        {
            // 查找Timer节点中的Text组件
            if (timerNode != null)
            {
                timerText = timerNode.GetComponentInChildren<Text>();
                if (timerText == null)
                {
                    Debug.LogWarning("[EventsUIManager] Timer节点中未找到Text组件");
                }
            }
            
            // 查找Status节点中的Text组件
            if (statusNode != null)
            {
                statusText = statusNode.GetComponentInChildren<Text>();
                if (statusText == null)
                {
                    Debug.LogWarning("[EventsUIManager] Status节点中未找到Text组件");
                }
            }
            
            Debug.Log("[EventsUIManager] UI组件引用初始化完成");
        }

        /// <summary>
        /// 初始化UI状态
        /// </summary>
        private void InitializeUIState()
        {
            // 初始状态：隐藏所有组件
            SetNodeActive(timerNode, false);
            SetNodeActive(statusNode, false);
            SetNodeActive(dealCardsNode, false);
            SetNodeActive(winResultNode, false);
            
            Debug.Log("[EventsUIManager] UI初始状态设置完成");
        }

        #endregion

        #region 网络事件处理

        /// <summary>
        /// 处理倒计时消息
        /// </summary>
        /// <param name="countdownData">倒计时数据</param>
        private void HandleCountdownReceived(NetworkEvents.CountdownData countdownData)
        {
            Debug.Log($"[EventsUIManager] 处理倒计时消息 - 时间: {countdownData.remainingTime}, 阶段: {countdownData.phase}");
            
            currentGamePhase = countdownData.phase;
            
            if (countdownData.isCountdownEnd)
            {
                // 倒计时结束：隐藏Timer，显示Status（开牌中）
                HandleCountdownEnd();
            }
            else
            {
                // 倒计时进行中：显示Timer和Status（投注中）
                HandleCountdownProgress(countdownData.remainingTime);
            }
        }

        /// <summary>
        /// 处理开牌消息
        /// </summary>
        /// <param name="message">开牌消息</param>
        private void HandleDealCardsReceived(string message)
        {
            Debug.Log($"[EventsUIManager] 处理开牌消息: {message}");
            
            // 显示开牌组件
            ShowDealCardsComponent();
        }

        /// <summary>
        /// 处理中奖消息
        /// </summary>
        /// <param name="message">中奖消息</param>
        private void HandleGameResultReceived(string message)
        {
            Debug.Log($"[EventsUIManager] 处理中奖消息: {message}");
            
            // 显示中奖组件
            ShowWinResultComponent();
        }

        #endregion

        #region 倒计时处理逻辑

        /// <summary>
        /// 处理倒计时进行中
        /// </summary>
        /// <param name="remainingTime">剩余时间</param>
        private void HandleCountdownProgress(int remainingTime)
        {
            // 显示Timer组件并更新倒计时
            SetNodeActive(timerNode, true);
            UpdateTimerDisplay(remainingTime);
            isTimerActive = true;
            
            // 显示Status组件（投注中）并设置自动隐藏
            ShowStatusWithText("投注中");
        }

        /// <summary>
        /// 处理倒计时结束
        /// </summary>
        private void HandleCountdownEnd()
        {
            // 隐藏Timer组件
            SetNodeActive(timerNode, false);
            isTimerActive = false;
            
            // 显示Status组件（开牌中）并设置自动隐藏
            ShowStatusWithText("开牌中");
        }

        /// <summary>
        /// 更新倒计时显示
        /// </summary>
        /// <param name="remainingTime">剩余时间</param>
        private void UpdateTimerDisplay(int remainingTime)
        {
            if (timerText != null)
            {
                // 格式化时间显示：mm:ss
                int minutes = remainingTime / 60;
                int seconds = remainingTime % 60;
                timerText.text = $"{minutes:D2}:{seconds:D2}";
                
                Debug.Log($"[EventsUIManager] 更新倒计时显示: {timerText.text}");
            }
        }

        #endregion

        #region 状态组件控制

        /// <summary>
        /// 显示状态组件并设置文本
        /// </summary>
        /// <param name="statusMessage">状态文本</param>
        private void ShowStatusWithText(string statusMessage)
        {
            // 取消之前的隐藏协程
            if (hideStatusCoroutine != null)
            {
                StopCoroutine(hideStatusCoroutine);
                hideStatusCoroutine = null;
            }
            
            // 显示状态组件并更新文本
            SetNodeActive(statusNode, true);
            if (statusText != null)
            {
                statusText.text = statusMessage;
            }
            
            // 启动自动隐藏协程
            hideStatusCoroutine = StartCoroutine(HideStatusAfterDelay());
            
            Debug.Log($"[EventsUIManager] 显示状态: {statusMessage}，{statusDisplayDuration}秒后自动隐藏");
        }

        /// <summary>
        /// 延时隐藏状态组件
        /// </summary>
        private IEnumerator HideStatusAfterDelay()
        {
            yield return new WaitForSeconds(statusDisplayDuration);
            
            SetNodeActive(statusNode, false);
            hideStatusCoroutine = null;
            
            Debug.Log("[EventsUIManager] 状态组件已自动隐藏");
        }

        #endregion

        #region 开牌组件控制

        /// <summary>
        /// 显示开牌组件
        /// </summary>
        private void ShowDealCardsComponent()
        {
            // 取消之前的隐藏协程
            if (hideDealCardsCoroutine != null)
            {
                StopCoroutine(hideDealCardsCoroutine);
                hideDealCardsCoroutine = null;
            }
            
            // 显示开牌组件
            SetNodeActive(dealCardsNode, true);
            
            // 启动自动隐藏协程
            hideDealCardsCoroutine = StartCoroutine(HideDealCardsAfterDelay());
            
            Debug.Log($"[EventsUIManager] 显示开牌组件，{dealCardsDisplayDuration}秒后自动隐藏");
        }

        /// <summary>
        /// 延时隐藏开牌组件
        /// </summary>
        private IEnumerator HideDealCardsAfterDelay()
        {
            yield return new WaitForSeconds(dealCardsDisplayDuration);
            
            SetNodeActive(dealCardsNode, false);
            hideDealCardsCoroutine = null;
            
            Debug.Log("[EventsUIManager] 开牌组件已自动隐藏");
        }

        #endregion

        #region 中奖组件控制

        /// <summary>
        /// 显示中奖组件
        /// </summary>
        private void ShowWinResultComponent()
        {
            // 取消之前的隐藏协程
            if (hideWinResultCoroutine != null)
            {
                StopCoroutine(hideWinResultCoroutine);
                hideWinResultCoroutine = null;
            }
            
            // 显示中奖组件
            SetNodeActive(winResultNode, true);
            
            // 启动自动隐藏协程
            hideWinResultCoroutine = StartCoroutine(HideWinResultAfterDelay());
            
            Debug.Log($"[EventsUIManager] 显示中奖组件，{winResultDisplayDuration}秒后自动隐藏");
        }

        /// <summary>
        /// 延时隐藏中奖组件
        /// </summary>
        private IEnumerator HideWinResultAfterDelay()
        {
            yield return new WaitForSeconds(winResultDisplayDuration);
            
            SetNodeActive(winResultNode, false);
            hideWinResultCoroutine = null;
            
            Debug.Log("[EventsUIManager] 中奖组件已自动隐藏");
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 安全设置节点激活状态
        /// </summary>
        /// <param name="node">目标节点</param>
        /// <param name="active">激活状态</param>
        private void SetNodeActive(GameObject node, bool active)
        {
            if (node != null)
            {
                node.SetActive(active);
            }
            else
            {
                Debug.LogWarning($"[EventsUIManager] 尝试设置空节点的激活状态: {active}");
            }
        }

        #endregion

        #region 公共接口（用于调试）

        /// <summary>
        /// 手动显示倒计时（用于测试）
        /// </summary>
        [ContextMenu("测试显示倒计时")]
        public void TestShowCountdown()
        {
            var testData = new NetworkEvents.CountdownData(30, "betting");
            HandleCountdownReceived(testData);
        }

        /// <summary>
        /// 手动显示开牌（用于测试）
        /// </summary>
        [ContextMenu("测试显示开牌")]
        public void TestShowDealCards()
        {
            HandleDealCardsReceived("测试开牌消息");
        }

        /// <summary>
        /// 手动显示中奖（用于测试）
        /// </summary>
        [ContextMenu("测试显示中奖")]
        public void TestShowWinResult()
        {
            HandleGameResultReceived("测试中奖消息");
        }

        /// <summary>
        /// 获取组件状态信息
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowComponentStatus()
        {
            Debug.Log($"[EventsUIManager] 组件状态检查:");
            Debug.Log($"  Timer节点: {(timerNode != null ? "✓" : "✗")} - 激活: {(timerNode?.activeInHierarchy ?? false)}");
            Debug.Log($"  Status节点: {(statusNode != null ? "✓" : "✗")} - 激活: {(statusNode?.activeInHierarchy ?? false)}");
            Debug.Log($"  开牌节点: {(dealCardsNode != null ? "✓" : "✗")} - 激活: {(dealCardsNode?.activeInHierarchy ?? false)}");
            Debug.Log($"  中奖节点: {(winResultNode != null ? "✓" : "✗")} - 激活: {(winResultNode?.activeInHierarchy ?? false)}");
            Debug.Log($"  当前游戏阶段: {currentGamePhase}");
            Debug.Log($"  Timer激活状态: {isTimerActive}");
        }

        #endregion
    }
}
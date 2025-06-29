// Assets/Core/UI/EventsUIManager.cs
// UI事件管理器 - TextMeshPro版本，完全响应网络数据

using System.Collections;
using UnityEngine;
using TMPro; // TextMeshPro命名空间

namespace BaccaratGame.Core
{
    /// <summary>
    /// UI事件管理器 - 简化版
    /// 订阅NetworkEvents的事件，控制UI组件的显示和隐藏
    /// 只支持TextMeshPro组件，完全响应网络数据
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

        // TextMeshPro UI组件引用
        private TextMeshProUGUI timerTextMeshPro;          // 倒计时文本
        private TextMeshProUGUI statusTextMeshPro;         // 状态文本
        private TextMeshProUGUI dealCardsTextMeshPro;      // 开牌信息文本
        private TextMeshProUGUI winResultTextMeshPro;      // 中奖信息文本
        
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
            
            // 停止所有协程
            StopAllCoroutines();
            
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
            // 查找Timer节点中的TextMeshPro组件
            if (timerNode != null)
            {
                timerTextMeshPro = timerNode.GetComponentInChildren<TextMeshProUGUI>();
                if (timerTextMeshPro == null)
                {
                    Debug.LogWarning("[EventsUIManager] Timer节点中未找到TextMeshPro组件");
                }
                else
                {
                    Debug.Log("[EventsUIManager] ✅ 找到Timer的TextMeshPro组件");
                }
            }
            
            // 查找Status节点中的TextMeshPro组件
            if (statusNode != null)
            {
                statusTextMeshPro = statusNode.GetComponentInChildren<TextMeshProUGUI>();
                if (statusTextMeshPro == null)
                {
                    Debug.LogWarning("[EventsUIManager] Status节点中未找到TextMeshPro组件");
                }
                else
                {
                    Debug.Log("[EventsUIManager] ✅ 找到Status的TextMeshPro组件");
                }
            }
            
            // 查找开牌节点中的TextMeshPro组件
            if (dealCardsNode != null)
            {
                dealCardsTextMeshPro = dealCardsNode.GetComponentInChildren<TextMeshProUGUI>();
                if (dealCardsTextMeshPro == null)
                {
                    Debug.LogWarning("[EventsUIManager] 开牌节点中未找到TextMeshPro组件");
                }
                else
                {
                    Debug.Log("[EventsUIManager] ✅ 找到开牌的TextMeshPro组件");
                }
            }
            
            // 查找中奖节点中的TextMeshPro组件
            if (winResultNode != null)
            {
                winResultTextMeshPro = winResultNode.GetComponentInChildren<TextMeshProUGUI>();
                if (winResultTextMeshPro == null)
                {
                    Debug.LogWarning("[EventsUIManager] 中奖节点中未找到TextMeshPro组件");
                }
                else
                {
                    Debug.Log("[EventsUIManager] ✅ 找到中奖的TextMeshPro组件");
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
            Debug.Log($"[EventsUIManager] 处理倒计时消息 - 时间: {countdownData.remainingTime}, 阶段: {countdownData.phase}, 局号: {countdownData.bureauNumber}");
            
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
        /// <param name="dealCardsData">开牌数据</param>
        private void HandleDealCardsReceived(NetworkEvents.DealCardsData dealCardsData)
        {
            Debug.Log($"[EventsUIManager] 处理开牌消息 - 庄家: {dealCardsData.zhuangPoint}点, 闲家: {dealCardsData.xianPoint}点, 局号: {dealCardsData.bureauNumber}");
            
            // 显示开牌组件并更新信息
            ShowDealCardsComponent(dealCardsData);
        }

        /// <summary>
        /// 处理中奖消息
        /// </summary>
        /// <param name="gameResultData">游戏结果数据</param>
        private void HandleGameResultReceived(NetworkEvents.GameResultData gameResultData)
        {
            Debug.Log($"[EventsUIManager] 处理中奖消息 - 结果: {gameResultData.result}, 金额: {gameResultData.winAmount}, 局号: {gameResultData.bureauNumber}");
            
            // 显示中奖组件并更新信息
            ShowWinResultComponent(gameResultData);
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
            if (timerTextMeshPro != null)
            {
                // 确保时间不为负数
                remainingTime = Mathf.Max(0, remainingTime);
                
                // 格式化时间显示：mm:ss
                int minutes = remainingTime / 60;
                int seconds = remainingTime % 60;
                timerTextMeshPro.text = $"{minutes:D2}:{seconds:D2}";
                
                Debug.Log($"[EventsUIManager] 更新倒计时显示: {timerTextMeshPro.text}");
            }
            else
            {
                Debug.LogWarning("[EventsUIManager] timerTextMeshPro为空，无法更新倒计时显示");
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
            if (statusTextMeshPro != null)
            {
                statusTextMeshPro.text = statusMessage;
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
        /// <param name="dealCardsData">开牌数据</param>
        private void ShowDealCardsComponent(NetworkEvents.DealCardsData dealCardsData)
        {
            // 取消之前的隐藏协程
            if (hideDealCardsCoroutine != null)
            {
                StopCoroutine(hideDealCardsCoroutine);
                hideDealCardsCoroutine = null;
            }
            
            // 显示开牌组件
            SetNodeActive(dealCardsNode, true);
            
            // 更新开牌信息文本
            if (dealCardsTextMeshPro != null)
            {
                string dealCardsInfo = FormatDealCardsInfo(dealCardsData);
                dealCardsTextMeshPro.text = dealCardsInfo;
            }
            
            // 启动自动隐藏协程
            hideDealCardsCoroutine = StartCoroutine(HideDealCardsAfterDelay());
            
            Debug.Log($"[EventsUIManager] 显示开牌组件，{dealCardsDisplayDuration}秒后自动隐藏");
        }

        /// <summary>
        /// 格式化开牌信息显示
        /// </summary>
        /// <param name="dealCardsData">开牌数据</param>
        /// <returns>格式化后的文本</returns>
        private string FormatDealCardsInfo(NetworkEvents.DealCardsData dealCardsData)
        {
            string info = $"开牌结果\n";
            info += $"庄家: {dealCardsData.zhuangPoint}点 ({dealCardsData.zhuangCount}张)\n";
            info += $"闲家: {dealCardsData.xianPoint}点 ({dealCardsData.xianCount}张)\n";
            
            if (!string.IsNullOrEmpty(dealCardsData.zhuangString))
            {
                info += $"庄家牌面: {dealCardsData.zhuangString}\n";
            }
            
            if (!string.IsNullOrEmpty(dealCardsData.xianString))
            {
                info += $"闲家牌面: {dealCardsData.xianString}\n";
            }
            
            if (dealCardsData.lucky > 0)
            {
                info += $"幸运数字: {dealCardsData.lucky}";
            }
            
            if (!string.IsNullOrEmpty(dealCardsData.bureauNumber))
            {
                info += $"\n局号: {dealCardsData.bureauNumber}";
            }
            
            return info;
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
        /// <param name="gameResultData">游戏结果数据</param>
        private void ShowWinResultComponent(NetworkEvents.GameResultData gameResultData)
        {
            // 取消之前的隐藏协程
            if (hideWinResultCoroutine != null)
            {
                StopCoroutine(hideWinResultCoroutine);
                hideWinResultCoroutine = null;
            }
            
            // 显示中奖组件
            SetNodeActive(winResultNode, true);
            
            // 更新中奖信息文本
            if (winResultTextMeshPro != null)
            {
                string winResultInfo = FormatWinResultInfo(gameResultData);
                winResultTextMeshPro.text = winResultInfo;
            }
            
            // 启动自动隐藏协程
            hideWinResultCoroutine = StartCoroutine(HideWinResultAfterDelay());
            
            Debug.Log($"[EventsUIManager] 显示中奖组件，{winResultDisplayDuration}秒后自动隐藏");
        }

        /// <summary>
        /// 格式化中奖信息显示
        /// </summary>
        /// <param name="gameResultData">游戏结果数据</param>
        /// <returns>格式化后的文本</returns>
        private string FormatWinResultInfo(NetworkEvents.GameResultData gameResultData)
        {
            string info = $"游戏结果\n";
            info += $"结果: {gameResultData.result}\n";
            
            if (gameResultData.winAmount > 0)
            {
                info += $"中奖金额: {gameResultData.winAmount:F2}\n";
            }
            
            if (gameResultData.betAmount > 0)
            {
                info += $"投注金额: {gameResultData.betAmount:F2}\n";
            }
            
            if (gameResultData.betType > 0)
            {
                info += $"投注类型: {gameResultData.betType}\n";
            }
            
            if (!string.IsNullOrEmpty(gameResultData.bureauNumber))
            {
                info += $"局号: {gameResultData.bureauNumber}";
            }
            
            return info;
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
            var testData = new NetworkEvents.CountdownData(30, "betting", "TEST001");
            HandleCountdownReceived(testData);
        }

        /// <summary>
        /// 手动显示开牌（用于测试）
        /// </summary>
        [ContextMenu("测试显示开牌")]
        public void TestShowDealCards()
        {
            var testData = new NetworkEvents.DealCardsData
            {
                zhuangPoint = 8,
                xianPoint = 5,
                zhuangCount = 2,
                xianCount = 2,
                zhuangString = "梅花3-梅花5-",
                xianString = "红桃10-黑桃5-",
                lucky = 8,
                bureauNumber = "TEST001"
            };
            HandleDealCardsReceived(testData);
        }

        /// <summary>
        /// 手动显示中奖（用于测试）
        /// </summary>
        [ContextMenu("测试显示中奖")]
        public void TestShowWinResult()
        {
            var testData = new NetworkEvents.GameResultData
            {
                result = "win",
                winAmount = 100.0f,
                betAmount = 50.0f,
                betType = 1,
                bureauNumber = "TEST001"
            };
            HandleGameResultReceived(testData);
        }

        /// <summary>
        /// 获取组件状态信息
        /// </summary>
        [ContextMenu("显示组件状态")]
        public void ShowComponentStatus()
        {
            Debug.Log($"[EventsUIManager] 组件状态检查:");
            Debug.Log($"  Timer节点: {(timerNode != null ? "✓" : "✗")} - 激活: {(timerNode?.activeInHierarchy ?? false)}");
            Debug.Log($"  Timer TextMeshPro: {(timerTextMeshPro != null ? "✓" : "✗")}");
            Debug.Log($"  Status TextMeshPro: {(statusTextMeshPro != null ? "✓" : "✗")}");
            Debug.Log($"  开牌 TextMeshPro: {(dealCardsTextMeshPro != null ? "✓" : "✗")}");
            Debug.Log($"  中奖 TextMeshPro: {(winResultTextMeshPro != null ? "✓" : "✗")}");
            Debug.Log($"  当前游戏阶段: {currentGamePhase}");
            Debug.Log($"  Timer激活状态: {isTimerActive}");
        }

        #endregion
    }
}
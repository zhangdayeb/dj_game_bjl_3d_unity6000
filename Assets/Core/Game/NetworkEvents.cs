// Assets/Core/Events/NetworkEvents.cs
// 网络事件定义 - 处理WebSocket消息事件
// 负责业务实现和事件触发

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BaccaratGame.Core
{
    /// <summary>
    /// 网络事件定义
    /// 处理倒计时、开牌信息、开奖信息三个核心事件
    /// 负责业务逻辑实现
    /// </summary>
    public static class NetworkEvents
    {
        #region WebSocket事件

        /// <summary>
        /// 倒计时消息接收事件
        /// </summary>
        public static event Action<CountdownData> OnCountdownReceived;

        /// <summary>
        /// 开牌信息消息接收事件
        /// </summary>
        public static event Action<DealCardsData> OnDealCardsReceived;

        /// <summary>
        /// 开奖信息消息接收事件
        /// </summary>
        public static event Action<GameResultData> OnGameResultReceived;

        #endregion

        #region 数据结构

        /// <summary>
        /// 倒计时数据结构
        /// </summary>
        [Serializable]
        public class CountdownData
        {
            public int remainingTime;    // 剩余时间（秒）
            public string phase;         // 阶段："betting" 或 "dealing"
            public bool isCountdownEnd;  // 是否倒计时结束
            public string bureauNumber;  // 局号
            public int countdownTime;    // 总倒计时时间
            
            public CountdownData(int time, string gamePhase, string bureau = "", int totalTime = 30)
            {
                remainingTime = time;
                phase = gamePhase;
                isCountdownEnd = time <= 0;
                bureauNumber = bureau;
                countdownTime = totalTime;
            }
        }

        /// <summary>
        /// 开牌信息数据结构
        /// </summary>
        [Serializable]
        public class DealCardsData
        {
            public int zhuangPoint;         // 庄家点数
            public int xianPoint;           // 闲家点数
            public int zhuangCount;         // 庄家牌数
            public int xianCount;           // 闲家牌数
            public int zhuangDui;           // 庄对
            public int xianDui;             // 闲对
            public int lucky;               // 幸运数字
            public int luckySize;           // 幸运大小
            public string zhuangString;     // 庄家牌面描述
            public string xianString;       // 闲家牌面描述
            public Dictionary<string, string> zhuangCards;  // 庄家牌面信息
            public Dictionary<string, string> xianCards;    // 闲家牌面信息
            public int[] winArray;          // 中奖数组
            public string bureauNumber;     // 局号
            
            public DealCardsData()
            {
                zhuangCards = new Dictionary<string, string>();
                xianCards = new Dictionary<string, string>();
                winArray = new int[0];
            }
        }

        /// <summary>
        /// 游戏结果数据结构
        /// </summary>
        [Serializable]
        public class GameResultData
        {
            public string result;           // 游戏结果
            public float winAmount;         // 中奖金额
            public float betAmount;         // 投注金额
            public int betType;             // 投注类型
            public string bureauNumber;     // 局号
            public Dictionary<string, object> additionalData; // 额外数据
            
            public GameResultData()
            {
                additionalData = new Dictionary<string, object>();
            }
        }

        #endregion

        #region 事件触发方法

        /// <summary>
        /// 触发倒计时消息接收事件
        /// </summary>
        /// <param name="message">原始消息内容</param>
        public static void TriggerCountdownReceived(string message)
        {
            Debug.Log($"[NetworkEvents] 收到倒计时消息: {message}");
            
            try
            {
                // 解析倒计时数据
                var countdownData = ParseCountdownMessage(message);
                
                if (countdownData != null)
                {
                    Debug.Log($"[NetworkEvents] 倒计时解析成功 - 剩余时间: {countdownData.remainingTime}秒, 阶段: {countdownData.phase}, 局号: {countdownData.bureauNumber}");
                    OnCountdownReceived?.Invoke(countdownData);
                }
                else
                {
                    Debug.LogWarning("[NetworkEvents] 倒计时消息解析失败");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkEvents] 倒计时消息处理异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 触发开牌信息消息接收事件
        /// </summary>
        /// <param name="message">消息内容</param>
        public static void TriggerDealCardsReceived(string message)
        {
            Debug.Log($"[NetworkEvents] 收到开牌消息: {message}");
            
            try
            {
                // 解析开牌数据
                var dealCardsData = ParseDealCardsMessage(message);
                
                if (dealCardsData != null)
                {
                    Debug.Log($"[NetworkEvents] 开牌解析成功 - 庄家: {dealCardsData.zhuangPoint}点, 闲家: {dealCardsData.xianPoint}点, 局号: {dealCardsData.bureauNumber}");
                    OnDealCardsReceived?.Invoke(dealCardsData);
                }
                else
                {
                    Debug.LogWarning("[NetworkEvents] 开牌消息解析失败");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkEvents] 开牌消息处理异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 触发开奖信息消息接收事件
        /// </summary>
        /// <param name="message">消息内容</param>
        public static void TriggerGameResultReceived(string message)
        {
            Debug.Log($"[NetworkEvents] 收到中奖消息: {message}");
            
            try
            {
                // 解析中奖数据
                var gameResultData = ParseGameResultMessage(message);
                
                if (gameResultData != null)
                {
                    Debug.Log($"[NetworkEvents] 中奖解析成功 - 结果: {gameResultData.result}, 金额: {gameResultData.winAmount}, 局号: {gameResultData.bureauNumber}");
                    OnGameResultReceived?.Invoke(gameResultData);
                }
                else
                {
                    Debug.LogWarning("[NetworkEvents] 中奖消息解析失败");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkEvents] 中奖消息处理异常: {ex.Message}");
            }
        }

        #endregion

        #region 消息解析业务逻辑

        /// <summary>
        /// 解析倒计时消息
        /// 从JSON消息中提取倒计时相关数据
        /// </summary>
        /// <param name="message">原始JSON消息</param>
        /// <returns>解析后的倒计时数据</returns>
        private static CountdownData ParseCountdownMessage(string message)
        {
            try
            {
                // 使用Unity的JsonUtility来解析（需要先构造简化的JSON结构）
                // 先尝试手动解析关键字段
                
                // 查找 data.table_run_info 部分
                string tableRunInfoSection = ExtractJsonSection(message, "\"table_run_info\":{", "}}");
                if (string.IsNullOrEmpty(tableRunInfoSection))
                {
                    Debug.LogError("[NetworkEvents] 无法找到 table_run_info 部分");
                    return null;
                }
                
                // 从 table_run_info 中提取字段
                int endTime = ExtractIntFromSection(tableRunInfoSection, "\"end_time\":", 0);
                int countdownTime = ExtractIntFromSection(tableRunInfoSection, "\"countdown_time\":", 30);
                string bureauNumber = ExtractStringFromSection(tableRunInfoSection, "\"bureau_number\":\"", "\"");
                
                // 判断游戏阶段
                string phase = endTime > 0 ? "betting" : "dealing";
                
                Debug.Log($"[NetworkEvents] 倒计时解析详情 - end_time: {endTime}, countdown_time: {countdownTime}, 阶段: {phase}, 局号: {bureauNumber}");
                
                return new CountdownData(endTime, phase, bureauNumber, countdownTime);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkEvents] 倒计时消息解析异常: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 解析开牌消息
        /// 从JSON消息中提取开牌相关数据
        /// </summary>
        /// <param name="message">原始JSON消息</param>
        /// <returns>解析后的开牌数据</returns>
        private static DealCardsData ParseDealCardsMessage(string message)
        {
            try
            {
                var dealCardsData = new DealCardsData();
                
                // 提取顶层局号
                dealCardsData.bureauNumber = ExtractStringFromSection(message, "\"bureau_number\":\"", "\"");
                
                // 查找 result_info.result 部分
                string resultSection = ExtractJsonSection(message, "\"result\":{", "}");
                if (string.IsNullOrEmpty(resultSection))
                {
                    Debug.LogError("[NetworkEvents] 无法找到 result 部分");
                    return null;
                }
                
                // 从 result 部分提取数据
                dealCardsData.zhuangPoint = ExtractIntFromSection(resultSection, "\"zhuang_point\":", 0);
                dealCardsData.xianPoint = ExtractIntFromSection(resultSection, "\"xian_point\":", 0);
                dealCardsData.zhuangCount = ExtractIntFromSection(resultSection, "\"zhuang_count\":", 0);
                dealCardsData.xianCount = ExtractIntFromSection(resultSection, "\"xian_count\":", 0);
                dealCardsData.zhuangDui = ExtractIntFromSection(resultSection, "\"zhuang_dui\":", 0);
                dealCardsData.xianDui = ExtractIntFromSection(resultSection, "\"xian_dui\":", 0);
                dealCardsData.lucky = ExtractIntFromSection(resultSection, "\"lucky\":", 0);
                dealCardsData.luckySize = ExtractIntFromSection(resultSection, "\"luckySize\":", 0);
                
                // 提取牌面描述字符串
                dealCardsData.zhuangString = ExtractStringFromSection(resultSection, "\"zhuang_string\":\"", "\"");
                dealCardsData.xianString = ExtractStringFromSection(resultSection, "\"xian_string\":\"", "\"");
                
                // 提取中奖数组
                string winArraySection = ExtractJsonSection(resultSection, "\"win_array\":[", "]");
                if (!string.IsNullOrEmpty(winArraySection))
                {
                    ParseWinArray(winArraySection, out dealCardsData.winArray);
                }
                
                // 查找并解析牌面信息
                string infoSection = ExtractJsonSection(message, "\"info\":{", "}");
                if (!string.IsNullOrEmpty(infoSection))
                {
                    // 解析庄家牌面
                    string zhuangSection = ExtractJsonSection(infoSection, "\"zhuang\":{", "}");
                    if (!string.IsNullOrEmpty(zhuangSection))
                    {
                        ParseCardDictionary(zhuangSection, dealCardsData.zhuangCards);
                    }
                    
                    // 解析闲家牌面
                    string xianSection = ExtractJsonSection(infoSection, "\"xian\":{", "}");
                    if (!string.IsNullOrEmpty(xianSection))
                    {
                        ParseCardDictionary(xianSection, dealCardsData.xianCards);
                    }
                }
                
                Debug.Log($"[NetworkEvents] 开牌解析详情 - 庄: {dealCardsData.zhuangPoint}点({dealCardsData.zhuangCount}张), " +
                         $"闲: {dealCardsData.xianPoint}点({dealCardsData.xianCount}张), 幸运: {dealCardsData.lucky}, 局号: {dealCardsData.bureauNumber}");
                
                return dealCardsData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkEvents] 开牌消息解析异常: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 解析游戏结果消息
        /// 从JSON消息中提取中奖相关数据
        /// </summary>
        /// <param name="message">原始JSON消息</param>
        /// <returns>解析后的游戏结果数据</returns>
        private static GameResultData ParseGameResultMessage(string message)
        {
            try
            {
                var gameResultData = new GameResultData();
                
                // 提取局号
                gameResultData.bureauNumber = ExtractStringFromSection(message, "\"bureau_number\":\"", "\"");
                
                // 查找 result_info 部分
                string resultInfoSection = ExtractJsonSection(message, "\"result_info\":{", "}");
                if (!string.IsNullOrEmpty(resultInfoSection))
                {
                    // 从 result_info 中提取 money 字段
                    gameResultData.winAmount = ExtractFloatFromSection(resultInfoSection, "\"money\":", 0f);
                    Debug.Log($"[NetworkEvents] 从 result_info.money 提取中奖金额: {gameResultData.winAmount}");
                }
                
                // 如果没有在 result_info 中找到，尝试其他可能的字段位置
                if (gameResultData.winAmount <= 0)
                {
                    gameResultData.winAmount = ExtractFloatFromSection(message, "\"money\":", 0f);
                    gameResultData.winAmount = ExtractFloatFromSection(message, "\"win_amount\":", gameResultData.winAmount);
                }
                
                // 提取其他可能的字段
                gameResultData.result = ExtractStringFromSection(message, "\"result\":\"", "\"");
                gameResultData.betAmount = ExtractFloatFromSection(message, "\"bet_amount\":", 0f);
                gameResultData.betType = ExtractIntFromSection(message, "\"bet_type\":", 0);
                
                // 如果没有明确的结果字段，根据金额判断
                if (string.IsNullOrEmpty(gameResultData.result))
                {
                    gameResultData.result = gameResultData.winAmount > 0 ? "win" : "lose";
                }
                
                Debug.Log($"[NetworkEvents] 中奖解析详情 - 结果: {gameResultData.result}, " +
                         $"中奖金额: {gameResultData.winAmount}, 投注金额: {gameResultData.betAmount}, 局号: {gameResultData.bureauNumber}");
                
                return gameResultData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkEvents] 中奖消息解析异常: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region 辅助解析方法

        /// <summary>
        /// 提取JSON中两个标记之间的内容
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <param name="startMarker">开始标记</param>
        /// <param name="endMarker">结束标记</param>
        /// <returns>提取的内容</returns>
        private static string ExtractJsonSection(string json, string startMarker, string endMarker)
        {
            try
            {
                int startIndex = json.IndexOf(startMarker);
                if (startIndex == -1) return "";
                
                startIndex += startMarker.Length;
                
                // 对于嵌套的大括号，需要计算平衡
                if (endMarker.Contains("}"))
                {
                    int braceCount = 1;
                    int currentIndex = startIndex;
                    
                    while (currentIndex < json.Length && braceCount > 0)
                    {
                        if (json[currentIndex] == '{')
                            braceCount++;
                        else if (json[currentIndex] == '}')
                            braceCount--;
                        
                        currentIndex++;
                    }
                    
                    if (braceCount == 0)
                    {
                        return json.Substring(startIndex, currentIndex - startIndex - 1);
                    }
                }
                else
                {
                    int endIndex = json.IndexOf(endMarker, startIndex);
                    if (endIndex != -1)
                    {
                        return json.Substring(startIndex, endIndex - startIndex);
                    }
                }
                
                return "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 从指定区域提取整数值
        /// </summary>
        private static int ExtractIntFromSection(string section, string fieldMarker, int defaultValue)
        {
            try
            {
                int startIndex = section.IndexOf(fieldMarker);
                if (startIndex == -1) return defaultValue;
                
                startIndex += fieldMarker.Length;
                
                // 跳过空格
                while (startIndex < section.Length && section[startIndex] == ' ')
                    startIndex++;
                
                int endIndex = startIndex;
                while (endIndex < section.Length && 
                       (char.IsDigit(section[endIndex]) || section[endIndex] == '-'))
                    endIndex++;
                
                if (endIndex > startIndex)
                {
                    string valueStr = section.Substring(startIndex, endIndex - startIndex);
                    if (int.TryParse(valueStr, out int result))
                        return result;
                }
                
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 从指定区域提取浮点数值
        /// </summary>
        private static float ExtractFloatFromSection(string section, string fieldMarker, float defaultValue)
        {
            try
            {
                int startIndex = section.IndexOf(fieldMarker);
                if (startIndex == -1) return defaultValue;
                
                startIndex += fieldMarker.Length;
                
                // 跳过空格
                while (startIndex < section.Length && section[startIndex] == ' ')
                    startIndex++;
                
                int endIndex = startIndex;
                while (endIndex < section.Length && 
                       (char.IsDigit(section[endIndex]) || section[endIndex] == '.' || section[endIndex] == '-'))
                    endIndex++;
                
                if (endIndex > startIndex)
                {
                    string valueStr = section.Substring(startIndex, endIndex - startIndex);
                    if (float.TryParse(valueStr, out float result))
                        return result;
                }
                
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 从指定区域提取字符串值
        /// </summary>
        private static string ExtractStringFromSection(string section, string startMarker, string endMarker)
        {
            try
            {
                int startIndex = section.IndexOf(startMarker);
                if (startIndex == -1) return "";
                
                startIndex += startMarker.Length;
                int endIndex = section.IndexOf(endMarker, startIndex);
                
                if (endIndex != -1)
                {
                    return section.Substring(startIndex, endIndex - startIndex);
                }
                
                return "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 解析中奖数组
        /// </summary>
        private static void ParseWinArray(string arraySection, out int[] winArray)
        {
            try
            {
                var matches = System.Text.RegularExpressions.Regex.Matches(arraySection, @"\d+");
                winArray = new int[matches.Count];
                
                for (int i = 0; i < matches.Count; i++)
                {
                    int.TryParse(matches[i].Value, out winArray[i]);
                }
            }
            catch
            {
                winArray = new int[0];
            }
        }

        /// <summary>
        /// 解析牌面字典
        /// </summary>
        private static void ParseCardDictionary(string cardSection, Dictionary<string, string> cardDict)
        {
            try
            {
                // 解析形如 "1":"m3.png","2":"m5.png" 的字符串
                var matches = System.Text.RegularExpressions.Regex.Matches(cardSection, @"""(\d+)"":""([^""]+)""");
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    if (match.Groups.Count >= 3)
                    {
                        cardDict[match.Groups[1].Value] = match.Groups[2].Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[NetworkEvents] 解析牌面字典失败: {ex.Message}");
            }
        }

        #endregion

        #region 事件清理

        /// <summary>
        /// 清除所有事件订阅
        /// </summary>
        public static void ClearAllSubscriptions()
        {
            OnCountdownReceived = null;
            OnDealCardsReceived = null;
            OnGameResultReceived = null;
            
            Debug.Log("[NetworkEvents] 所有事件订阅已清理");
        }

        #endregion

        #region 调试方法

        /// <summary>
        /// 模拟倒计时消息（用于测试）
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void SimulateCountdownMessage(int time)
        {
            string mockMessage = $"{{\"code\":200,\"msg\":\"倒计时信息\",\"data\":{{\"table_run_info\":{{\"end_time\":{time},\"countdown_time\":30,\"bureau_number\":\"TEST001\"}}}}}}";
            TriggerCountdownReceived(mockMessage);
        }

        /// <summary>
        /// 模拟开牌消息（用于测试）
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void SimulateDealCardsMessage()
        {
            string mockMessage = @"{""code"":200,""msg"":""开牌信息"",""data"":{""result_info"":{""result"":{""luckySize"":2,""size"":0,""zhuang_point"":8,""xian_point"":5,""zhuang_dui"":0,""xian_dui"":0,""lucky"":8,""zhuang_count"":2,""xian_count"":2,""zhuang_string"":""梅花3-梅花5-"",""xian_string"":""红桃10-黑桃5-"",""win_array"":[8]},""info"":{""zhuang"":{""1"":""m3.png"",""2"":""m5.png""},""xian"":{""4"":""r10.png"",""5"":""h5.png""}}},""bureau_number"":""TEST002""}}";
            TriggerDealCardsReceived(mockMessage);
        }

        /// <summary>
        /// 模拟中奖消息（用于测试）
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void SimulateGameResultMessage()
        {
            string mockMessage = "{\"code\":200,\"msg\":\"中奖信息\",\"data\":{\"result_info\":{\"money\":150.5},\"bureau_number\":\"TEST003\"}}";
            TriggerGameResultReceived(mockMessage);
        }

        #endregion
    }
}
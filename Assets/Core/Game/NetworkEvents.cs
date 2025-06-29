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
            
            public CountdownData(int time, string gamePhase, string bureau = "")
            {
                remainingTime = time;
                phase = gamePhase;
                isCountdownEnd = time <= 0;
                bureauNumber = bureau;
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
                // 提取局号
                string bureauNumber = ExtractStringField(message, "bureau_number");
                
                // 从嵌套路径提取倒计时：data.table_run_info.end_time
                int endTime = ExtractNestedIntField(message, "data", "table_run_info", "end_time");
                
                // 如果没有找到end_time，尝试其他可能的字段
                if (endTime <= 0)
                {
                    endTime = ExtractIntField(message, "countdown_time", 0);
                }
                if (endTime <= 0)
                {
                    endTime = ExtractIntField(message, "time", 0);
                }
                if (endTime <= 0)
                {
                    endTime = ExtractIntField(message, "countdown", 0);
                }
                
                // 判断游戏阶段
                string phase = endTime > 0 ? "betting" : "dealing";
                
                Debug.Log($"[NetworkEvents] 倒计时解析 - end_time: {endTime}, 阶段: {phase}, 局号: {bureauNumber}");
                
                return new CountdownData(endTime, phase, bureauNumber);
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
                
                // 提取局号
                dealCardsData.bureauNumber = ExtractStringField(message, "bureau_number");
                
                // 提取result部分的数据
                dealCardsData.zhuangPoint = ExtractNestedIntField(message, "data", "result_info", "result", "zhuang_point");
                dealCardsData.xianPoint = ExtractNestedIntField(message, "data", "result_info", "result", "xian_point");
                dealCardsData.zhuangCount = ExtractNestedIntField(message, "data", "result_info", "result", "zhuang_count");
                dealCardsData.xianCount = ExtractNestedIntField(message, "data", "result_info", "result", "xian_count");
                dealCardsData.zhuangDui = ExtractNestedIntField(message, "data", "result_info", "result", "zhuang_dui");
                dealCardsData.xianDui = ExtractNestedIntField(message, "data", "result_info", "result", "xian_dui");
                dealCardsData.lucky = ExtractNestedIntField(message, "data", "result_info", "result", "lucky");
                dealCardsData.luckySize = ExtractNestedIntField(message, "data", "result_info", "result", "luckySize");
                
                // 提取牌面描述
                dealCardsData.zhuangString = ExtractNestedStringField(message, "data", "result_info", "result", "zhuang_string");
                dealCardsData.xianString = ExtractNestedStringField(message, "data", "result_info", "result", "xian_string");
                
                // 提取中奖数组 - 简化处理，只取第一个值
                string winArrayStr = ExtractNestedStringField(message, "data", "result_info", "result", "win_array");
                if (!string.IsNullOrEmpty(winArrayStr))
                {
                    // 简单解析数组，查找数字
                    var matches = System.Text.RegularExpressions.Regex.Matches(winArrayStr, @"\d+");
                    if (matches.Count > 0)
                    {
                        dealCardsData.winArray = new int[matches.Count];
                        for (int i = 0; i < matches.Count; i++)
                        {
                            int.TryParse(matches[i].Value, out dealCardsData.winArray[i]);
                        }
                    }
                }
                
                // 提取牌面信息 - 庄家牌
                string zhuangCardsSection = ExtractSectionBetween(message, "\"zhuang\":{", "}");
                if (!string.IsNullOrEmpty(zhuangCardsSection))
                {
                    ParseCardDictionary(zhuangCardsSection, dealCardsData.zhuangCards);
                }
                
                // 提取牌面信息 - 闲家牌
                string xianCardsSection = ExtractSectionBetween(message, "\"xian\":{", "}");
                if (!string.IsNullOrEmpty(xianCardsSection))
                {
                    ParseCardDictionary(xianCardsSection, dealCardsData.xianCards);
                }
                
                Debug.Log($"[NetworkEvents] 开牌解析详情 - 庄: {dealCardsData.zhuangPoint}点({dealCardsData.zhuangCount}张), " +
                         $"闲: {dealCardsData.xianPoint}点({dealCardsData.xianCount}张), 幸运: {dealCardsData.lucky}");
                
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
                gameResultData.bureauNumber = ExtractStringField(message, "bureau_number");
                
                // 提取基本结果信息
                gameResultData.result = ExtractStringField(message, "result", "unknown");
                
                // 提取金额信息（如果有的话）
                gameResultData.winAmount = ExtractFloatField(message, "win_amount", 0f);
                gameResultData.betAmount = ExtractFloatField(message, "bet_amount", 0f);
                gameResultData.betType = ExtractIntField(message, "bet_type", 0);
                
                Debug.Log($"[NetworkEvents] 中奖解析详情 - 结果: {gameResultData.result}, " +
                         $"中奖金额: {gameResultData.winAmount}, 投注金额: {gameResultData.betAmount}");
                
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
        /// 从JSON消息中提取整数字段
        /// </summary>
        private static int ExtractIntField(string jsonMessage, string fieldName, int defaultValue = 0)
        {
            try
            {
                string searchPattern = $"\"{fieldName}\"";
                int fieldIndex = jsonMessage.IndexOf(searchPattern);
                if (fieldIndex == -1) return defaultValue;

                int colonIndex = jsonMessage.IndexOf(":", fieldIndex);
                if (colonIndex == -1) return defaultValue;

                int valueStart = colonIndex + 1;
                while (valueStart < jsonMessage.Length && 
                       (jsonMessage[valueStart] == ' ' || jsonMessage[valueStart] == '"'))
                    valueStart++;

                int valueEnd = valueStart;
                while (valueEnd < jsonMessage.Length && 
                       (char.IsDigit(jsonMessage[valueEnd]) || jsonMessage[valueEnd] == '-'))
                    valueEnd++;

                if (valueEnd > valueStart)
                {
                    string valueStr = jsonMessage.Substring(valueStart, valueEnd - valueStart);
                    if (int.TryParse(valueStr, out int result))
                    {
                        return result;
                    }
                }
                
                return defaultValue;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[NetworkEvents] 提取字段 {fieldName} 失败: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// 从JSON消息中提取浮点数字段
        /// </summary>
        private static float ExtractFloatField(string jsonMessage, string fieldName, float defaultValue = 0f)
        {
            try
            {
                string searchPattern = $"\"{fieldName}\"";
                int fieldIndex = jsonMessage.IndexOf(searchPattern);
                if (fieldIndex == -1) return defaultValue;

                int colonIndex = jsonMessage.IndexOf(":", fieldIndex);
                if (colonIndex == -1) return defaultValue;

                int valueStart = colonIndex + 1;
                while (valueStart < jsonMessage.Length && 
                       (jsonMessage[valueStart] == ' ' || jsonMessage[valueStart] == '"'))
                    valueStart++;

                int valueEnd = valueStart;
                while (valueEnd < jsonMessage.Length && 
                       (char.IsDigit(jsonMessage[valueEnd]) || jsonMessage[valueEnd] == '.' || jsonMessage[valueEnd] == '-'))
                    valueEnd++;

                if (valueEnd > valueStart)
                {
                    string valueStr = jsonMessage.Substring(valueStart, valueEnd - valueStart);
                    if (float.TryParse(valueStr, out float result))
                    {
                        return result;
                    }
                }
                
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 从JSON消息中提取字符串字段
        /// </summary>
        private static string ExtractStringField(string jsonMessage, string fieldName, string defaultValue = "")
        {
            try
            {
                string searchPattern = $"\"{fieldName}\"";
                int fieldIndex = jsonMessage.IndexOf(searchPattern);
                if (fieldIndex == -1) return defaultValue;

                int colonIndex = jsonMessage.IndexOf(":", fieldIndex);
                if (colonIndex == -1) return defaultValue;

                int valueStart = colonIndex + 1;
                while (valueStart < jsonMessage.Length && 
                       (jsonMessage[valueStart] == ' ' || jsonMessage[valueStart] == '"'))
                    valueStart++;

                int valueEnd = valueStart;
                while (valueEnd < jsonMessage.Length && 
                       jsonMessage[valueEnd] != '"' && 
                       jsonMessage[valueEnd] != ',' && 
                       jsonMessage[valueEnd] != '}')
                    valueEnd++;

                return jsonMessage.Substring(valueStart, valueEnd - valueStart).Trim();
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 从嵌套JSON路径中提取整数字段
        /// </summary>
        private static int ExtractNestedIntField(string jsonMessage, params string[] path)
        {
            try
            {
                string currentSection = jsonMessage;
                
                // 逐级查找嵌套路径
                for (int i = 0; i < path.Length - 1; i++)
                {
                    string sectionStart = $"\"{path[i]}\":{{";
                    int startIndex = currentSection.IndexOf(sectionStart);
                    if (startIndex == -1) return 0;
                    
                    startIndex += sectionStart.Length - 1; // 保留开始的{
                    
                    // 找到匹配的结束括号
                    int braceCount = 1;
                    int endIndex = startIndex + 1;
                    while (endIndex < currentSection.Length && braceCount > 0)
                    {
                        if (currentSection[endIndex] == '{') braceCount++;
                        else if (currentSection[endIndex] == '}') braceCount--;
                        endIndex++;
                    }
                    
                    if (braceCount == 0)
                    {
                        currentSection = currentSection.Substring(startIndex, endIndex - startIndex);
                    }
                    else
                    {
                        return 0;
                    }
                }
                
                // 在最终的section中查找目标字段
                return ExtractIntField(currentSection, path[path.Length - 1], 0);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 从嵌套JSON路径中提取字符串字段
        /// </summary>
        private static string ExtractNestedStringField(string jsonMessage, params string[] path)
        {
            try
            {
                string currentSection = jsonMessage;
                
                // 逐级查找嵌套路径
                for (int i = 0; i < path.Length - 1; i++)
                {
                    string sectionStart = $"\"{path[i]}\":{{";
                    int startIndex = currentSection.IndexOf(sectionStart);
                    if (startIndex == -1) return "";
                    
                    startIndex += sectionStart.Length - 1; // 保留开始的{
                    
                    // 找到匹配的结束括号
                    int braceCount = 1;
                    int endIndex = startIndex + 1;
                    while (endIndex < currentSection.Length && braceCount > 0)
                    {
                        if (currentSection[endIndex] == '{') braceCount++;
                        else if (currentSection[endIndex] == '}') braceCount--;
                        endIndex++;
                    }
                    
                    if (braceCount == 0)
                    {
                        currentSection = currentSection.Substring(startIndex, endIndex - startIndex);
                    }
                    else
                    {
                        return "";
                    }
                }
                
                // 在最终的section中查找目标字段
                return ExtractStringField(currentSection, path[path.Length - 1], "");
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 提取两个标记之间的内容
        /// </summary>
        private static string ExtractSectionBetween(string text, string startMarker, string endMarker)
        {
            try
            {
                int startIndex = text.IndexOf(startMarker);
                if (startIndex == -1) return "";
                
                startIndex += startMarker.Length;
                int endIndex = text.IndexOf(endMarker, startIndex);
                if (endIndex == -1) return "";
                
                return text.Substring(startIndex, endIndex - startIndex);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 解析牌面字典
        /// </summary>
        private static void ParseCardDictionary(string cardSection, Dictionary<string, string> cardDict)
        {
            try
            {
                // 简单的键值对解析
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
            string mockMessage = $"{{\"code\":200,\"msg\":\"倒计时信息\",\"data\":{{\"table_run_info\":{{\"end_time\":{time}}}}},\"bureau_number\":\"2025062920116947\"}}";
            TriggerCountdownReceived(mockMessage);
        }

        /// <summary>
        /// 模拟开牌消息（用于测试）
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void SimulateDealCardsMessage()
        {
            string mockMessage = @"{""code"":200,""msg"":""开牌信息"",""data"":{""result_info"":{""result"":{""luckySize"":2,""size"":0,""zhuang_point"":8,""xian_point"":5,""zhuang_dui"":0,""xian_dui"":0,""lucky"":8,""zhuang_count"":2,""xian_count"":2,""zhuang_string"":""梅花3-梅花5-"",""xian_string"":""红桃10-黑桃5-"",""win_array"":[8]},""info"":{""zhuang"":{""1"":""m3.png"",""2"":""m5.png""},""xian"":{""4"":""r10.png"",""5"":""h5.png""}}},""bureau_number"":""2025062920116948""}}";
            TriggerDealCardsReceived(mockMessage);
        }

        /// <summary>
        /// 模拟中奖消息（用于测试）
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void SimulateGameResultMessage()
        {
            string mockMessage = "{\"code\":200,\"msg\":\"中奖信息\",\"data\":{\"result\":\"win\",\"win_amount\":100.0,\"bet_amount\":50.0},\"bureau_number\":\"2025062920116948\"}";
            TriggerGameResultReceived(mockMessage);
        }

        #endregion
    }
}
// Assets/Core/Data/Types/EventDataTypes.cs
// 事件专用数据类型定义 - 精简版
// 只包含GameEvents和NetworkEvents需要的新增数据结构
// 移除所有与现有文件重复的定义
// 创建时间: 2025/6/22

using System;
using System.Collections.Generic;
using UnityEngine;
using BaccaratGame.Data; 

namespace BaccaratGame.Data
{
    #region 游戏事件专用数据类型（新增）

    /// <summary>
    /// 投注阶段信息
    /// </summary>
    [Serializable]
    public class BettingPhaseInfo
    {
        public string roundId = "";
        public float duration = 30f;
        public DateTime startTime = DateTime.Now;
        public bool allowBetting = true;
        
        public BettingPhaseInfo() { }
        
        public BettingPhaseInfo(string roundId, float duration)
        {
            this.roundId = roundId;
            this.duration = duration;
            this.startTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"BettingPhase[{roundId}, Duration:{duration}s, CanBet:{allowBetting}]";
        }
    }

    /// <summary>
    /// 发牌阶段信息
    /// </summary>
    [Serializable]
    public class DealingPhaseInfo
    {
        public string roundId = "";
        public float estimatedDuration = 10f;
        public int expectedCardCount = 4;
        public DateTime startTime = DateTime.Now;
        
        public DealingPhaseInfo() { }
        
        public DealingPhaseInfo(string roundId, int expectedCardCount)
        {
            this.roundId = roundId;
            this.expectedCardCount = expectedCardCount;
            this.startTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"DealingPhase[{roundId}, Cards:{expectedCardCount}, Duration:{estimatedDuration}s]";
        }
    }

    /// <summary>
    /// 发牌完成信息
    /// </summary>
    [Serializable]
    public class DealingCompleteInfo
    {
        public string roundId = "";
        public HandInfo playerHand = new HandInfo();
        public HandInfo bankerHand = new HandInfo();
        public float dealingDuration = 0f;
        public bool isComplete = false;
        
        public DealingCompleteInfo() { }
        
        public DealingCompleteInfo(string roundId, HandInfo playerHand, HandInfo bankerHand)
        {
            this.roundId = roundId;
            this.playerHand = playerHand;
            this.bankerHand = bankerHand;
            this.isComplete = true;
        }
        
        public override string ToString()
        {
            return $"DealingComplete[{roundId}, Player:{playerHand.total}, Banker:{bankerHand.total}]";
        }
    }

    /// <summary>
    /// 投注限额检查结果
    /// </summary>
    [Serializable]
    public class BetLimitCheckResult
    {
        public bool isValid = false;
        public decimal maxAllowedBet = 0m;
        public decimal minAllowedBet = 0m;
        public string errorMessage = "";
        public string limitType = ""; // 使用字符串避免枚举重复
        
        public BetLimitCheckResult() { }
        
        public BetLimitCheckResult(bool isValid, decimal minBet, decimal maxBet)
        {
            this.isValid = isValid;
            this.minAllowedBet = minBet;
            this.maxAllowedBet = maxBet;
        }
        
        public override string ToString()
        {
            return $"BetLimitCheck[Valid:{isValid}, Range:{minAllowedBet}-{maxAllowedBet}]";
        }
    }

    /// <summary>
    /// 第三张牌规则结果
    /// </summary>
    [Serializable]
    public class ThirdCardRuleResult
    {
        public bool playerNeedsThirdCard = false;
        public bool bankerNeedsThirdCard = false;
        public Card playerThirdCard = null;
        public Card bankerThirdCard = null;
        public string ruleDescription = "";
        
        public ThirdCardRuleResult() { }
        
        public ThirdCardRuleResult(bool playerNeeds, bool bankerNeeds, string description)
        {
            this.playerNeedsThirdCard = playerNeeds;
            this.bankerNeedsThirdCard = bankerNeeds;
            this.ruleDescription = description;
        }
        
        public override string ToString()
        {
            return $"ThirdCardRule[Player:{playerNeedsThirdCard}, Banker:{bankerNeedsThirdCard}]";
        }
    }

    /// <summary>
    /// 游戏错误
    /// </summary>
    [Serializable]
    public class GameError
    {
        public string errorCode = "";
        public string message = "";
        public ErrorSeverity severity = ErrorSeverity.Low; // 使用GameEnums中的定义
        public DateTime timestamp = DateTime.Now;
        public string context = "";
        
        public GameError() { }
        
        public GameError(string code, string message, ErrorSeverity severity = ErrorSeverity.Low)
        {
            this.errorCode = code;
            this.message = message;
            this.severity = severity;
            this.timestamp = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"GameError[{errorCode}:{severity}] {message}";
        }
    }

    /// <summary>
    /// 游戏警告
    /// </summary>
    [Serializable]
    public class GameWarning
    {
        public string warningCode = "";
        public string message = "";
        public string warningType = "Information"; // 使用字符串避免枚举重复
        public DateTime timestamp = DateTime.Now;
        
        public GameWarning() { }
        
        public GameWarning(string code, string message, string type = "Information")
        {
            this.warningCode = code;
            this.message = message;
            this.warningType = type;
            this.timestamp = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"GameWarning[{warningCode}:{warningType}] {message}";
        }
    }

    /// <summary>
    /// 成就
    /// </summary>
    [Serializable]
    public class Achievement
    {
        public string achievementId = "";
        public string name = "";
        public string description = "";
        public string type = "FirstWin"; // 使用字符串避免枚举重复
        public DateTime unlockedTime = DateTime.Now;
        public int points = 0;
        
        public Achievement() { }
        
        public Achievement(string id, string name, string description, int points = 0)
        {
            this.achievementId = id;
            this.name = name;
            this.description = description;
            this.points = points;
            this.unlockedTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"Achievement[{achievementId}:{name}, Points:{points}]";
        }
    }

    #endregion

    #region 网络事件专用数据类型（新增，但不包含枚举）

    /// <summary>
    /// 连接失败信息
    /// </summary>
    [Serializable]
    public class ConnectionFailureInfo
    {
        public string errorCode = "";
        public string errorMessage = "";
        public string reason = "NetworkUnavailable"; // 使用字符串避免枚举重复
        public DateTime failureTime = DateTime.Now;
        public int retryCount = 0;
        public float nextRetryDelay = 5f;
        
        public ConnectionFailureInfo() { }
        
        public ConnectionFailureInfo(string errorCode, string message, string reason)
        {
            this.errorCode = errorCode;
            this.errorMessage = message;
            this.reason = reason;
            this.failureTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"ConnectionFailure[{reason}] {errorMessage}";
        }
    }

    /// <summary>
    /// 断开连接信息
    /// </summary>
    [Serializable]
    public class DisconnectionInfo
    {
        public string connectionId = "";
        public string reason = "NetworkLost"; // 使用字符串避免枚举重复
        public DateTime disconnectTime = DateTime.Now;
        public TimeSpan connectionDuration = TimeSpan.Zero;
        public bool wasExpected = false;
        public string details = "";
        
        public DisconnectionInfo() { }
        
        public DisconnectionInfo(string connectionId, string reason, bool expected = false)
        {
            this.connectionId = connectionId;
            this.reason = reason;
            this.wasExpected = expected;
            this.disconnectTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"Disconnection[{reason}, Expected:{wasExpected}] {details}";
        }
    }

    /// <summary>
    /// 重连信息
    /// </summary>
    [Serializable]
    public class ReconnectionInfo
    {
        public string connectionId = "";
        public int attemptNumber = 0;
        public int maxAttempts = 5;
        public float delay = 0f;
        public DateTime attemptTime = DateTime.Now;
        public string strategy = "ExponentialBackoff"; // 使用字符串避免枚举重复
        
        public ReconnectionInfo() { }
        
        public ReconnectionInfo(string connectionId, int attempt, int maxAttempts)
        {
            this.connectionId = connectionId;
            this.attemptNumber = attempt;
            this.maxAttempts = maxAttempts;
            this.attemptTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"Reconnection[Attempt {attemptNumber}/{maxAttempts}, Strategy:{strategy}]";
        }
    }

    /// <summary>
    /// 超时信息
    /// </summary>
    [Serializable]
    public class TimeoutInfo
    {
        public string operationId = "";
        public string timeoutType = "Connection"; // 使用字符串避免枚举重复
        public float timeoutDuration = 0f;
        public DateTime timeoutTime = DateTime.Now;
        public string context = "";
        
        public TimeoutInfo() { }
        
        public TimeoutInfo(string operationId, string type, float duration)
        {
            this.operationId = operationId;
            this.timeoutType = type;
            this.timeoutDuration = duration;
            this.timeoutTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"Timeout[{timeoutType}, Duration:{timeoutDuration}s] {context}";
        }
    }

    /// <summary>
    /// WebSocket消息信息
    /// </summary>
    [Serializable]
    public class WebSocketMessageInfo
    {
        public string messageId = "";
        public string messageType = "";
        public string content = "";
        public DateTime timestamp = DateTime.Now;
        public string direction = "Sent"; // 使用字符串避免枚举重复
        public int messageSize = 0;
        public string priority = "Normal"; // 使用字符串避免枚举重复
        
        public WebSocketMessageInfo() 
        {
            messageId = Guid.NewGuid().ToString();
        }
        
        public WebSocketMessageInfo(string messageType, string content, string direction)
        {
            this.messageId = Guid.NewGuid().ToString();
            this.messageType = messageType;
            this.content = content;
            this.direction = direction;
            this.messageSize = content?.Length ?? 0;
            this.timestamp = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"WSMessage[{messageType}, {direction}, Size:{messageSize}]";
        }
    }

    /// <summary>
    /// WebSocket错误信息
    /// </summary>
    [Serializable]
    public class WebSocketErrorInfo
    {
        public string errorCode = "";
        public string errorMessage = "";
        public string errorType = "ConnectionError"; // 使用字符串避免枚举重复
        public DateTime errorTime = DateTime.Now;
        public string stackTrace = "";
        
        public WebSocketErrorInfo() { }
        
        public WebSocketErrorInfo(string code, string message, string type)
        {
            this.errorCode = code;
            this.errorMessage = message;
            this.errorType = type;
            this.errorTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"WSError[{errorType}] {errorMessage}";
        }
    }

    /// <summary>
    /// WebSocket状态信息
    /// </summary>
    [Serializable]
    public class WebSocketStateInfo
    {
        public string oldState = "Closed"; // 使用字符串避免枚举重复
        public string newState = "Connecting"; // 使用字符串避免枚举重复
        public DateTime stateChangeTime = DateTime.Now;
        public string reason = "";
        
        public WebSocketStateInfo() { }
        
        public WebSocketStateInfo(string oldState, string newState, string reason = "")
        {
            this.oldState = oldState;
            this.newState = newState;
            this.reason = reason;
            this.stateChangeTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"WSState[{oldState} -> {newState}] {reason}";
        }
    }

    /// <summary>
    /// 心跳信息
    /// </summary>
    [Serializable]
    public class HeartbeatInfo
    {
        public string heartbeatId = "";
        public DateTime timestamp = DateTime.Now;
        public float roundTripTime = 0f;
        public int sequenceNumber = 0;
        public bool isResponse = false;
        
        public HeartbeatInfo() 
        {
            heartbeatId = Guid.NewGuid().ToString();
        }
        
        public HeartbeatInfo(int sequenceNumber, bool isResponse = false)
        {
            this.heartbeatId = Guid.NewGuid().ToString();
            this.sequenceNumber = sequenceNumber;
            this.isResponse = isResponse;
            this.timestamp = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"Heartbeat[{sequenceNumber}, RTT:{roundTripTime}ms, Response:{isResponse}]";
        }
    }

    /// <summary>
    /// HTTP请求信息
    /// </summary>
    [Serializable]
    public class HttpRequestInfo
    {
        public string requestId = "";
        public string url = "";
        public string method = "GET"; // 使用字符串避免枚举重复
        public Dictionary<string, string> headers = new Dictionary<string, string>();
        public string body = "";
        public DateTime startTime = DateTime.Now;
        public float timeout = 30f;
        
        public HttpRequestInfo() 
        {
            requestId = Guid.NewGuid().ToString();
        }
        
        public HttpRequestInfo(string url, string method)
        {
            this.requestId = Guid.NewGuid().ToString();
            this.url = url;
            this.method = method;
            this.startTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"HttpRequest[{method} {url}, Timeout:{timeout}s]";
        }
    }

    /// <summary>
    /// HTTP响应信息
    /// </summary>
    [Serializable]
    public class HttpResponseInfo
    {
        public string requestId = "";
        public int statusCode = 200;
        public string statusText = "";
        public Dictionary<string, string> headers = new Dictionary<string, string>();
        public string body = "";
        public DateTime responseTime = DateTime.Now;
        public float duration = 0f;
        public long contentLength = 0;
        
        public HttpResponseInfo() { }
        
        public HttpResponseInfo(string requestId, int statusCode, string body)
        {
            this.requestId = requestId;
            this.statusCode = statusCode;
            this.body = body;
            this.contentLength = body?.Length ?? 0;
            this.responseTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"HttpResponse[{statusCode}, Size:{contentLength}, Duration:{duration}ms]";
        }
    }

    /// <summary>
    /// HTTP错误信息
    /// </summary>
    [Serializable]
    public class HttpErrorInfo
    {
        public string requestId = "";
        public string errorCode = "";
        public string errorMessage = "";
        public string errorType = "NetworkError"; // 使用字符串避免枚举重复
        public DateTime errorTime = DateTime.Now;
        public int statusCode = 0;
        
        public HttpErrorInfo() { }
        
        public HttpErrorInfo(string requestId, string errorCode, string message, string type)
        {
            this.requestId = requestId;
            this.errorCode = errorCode;
            this.errorMessage = message;
            this.errorType = type;
            this.errorTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"HttpError[{errorType}] {errorMessage}";
        }
    }

    /// <summary>
    /// HTTP超时信息
    /// </summary>
    [Serializable]
    public class HttpTimeoutInfo
    {
        public string requestId = "";
        public float timeoutDuration = 0f;
        public DateTime timeoutTime = DateTime.Now;
        public string url = "";
        public string method = "GET"; // 使用字符串避免枚举重复
        
        public HttpTimeoutInfo() { }
        
        public HttpTimeoutInfo(string requestId, string url, string method, float timeout)
        {
            this.requestId = requestId;
            this.url = url;
            this.method = method;
            this.timeoutDuration = timeout;
            this.timeoutTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"HttpTimeout[{method} {url}, Duration:{timeoutDuration}s]";
        }
    }

    /// <summary>
    /// 上传进度信息
    /// </summary>
    [Serializable]
    public class UploadProgressInfo
    {
        public string uploadId = "";
        public long bytesUploaded = 0;
        public long totalBytes = 0;
        public float progressPercentage = 0f;
        public float uploadSpeed = 0f; // bytes per second
        public TimeSpan estimatedTimeRemaining = TimeSpan.Zero;
        
        public UploadProgressInfo() 
        {
            uploadId = Guid.NewGuid().ToString();
        }
        
        public UploadProgressInfo(long bytesUploaded, long totalBytes)
        {
            this.uploadId = Guid.NewGuid().ToString();
            this.bytesUploaded = bytesUploaded;
            this.totalBytes = totalBytes;
            this.progressPercentage = totalBytes > 0 ? (float)bytesUploaded / totalBytes * 100f : 0f;
        }
        
        public override string ToString()
        {
            return $"Upload[{progressPercentage:F1}%, {bytesUploaded}/{totalBytes} bytes]";
        }
    }

    /// <summary>
    /// 下载进度信息
    /// </summary>
    [Serializable]
    public class DownloadProgressInfo
    {
        public string downloadId = "";
        public long bytesDownloaded = 0;
        public long totalBytes = 0;
        public float progressPercentage = 0f;
        public float downloadSpeed = 0f; // bytes per second
        public TimeSpan estimatedTimeRemaining = TimeSpan.Zero;
        
        public DownloadProgressInfo() 
        {
            downloadId = Guid.NewGuid().ToString();
        }
        
        public DownloadProgressInfo(long bytesDownloaded, long totalBytes)
        {
            this.downloadId = Guid.NewGuid().ToString();
            this.bytesDownloaded = bytesDownloaded;
            this.totalBytes = totalBytes;
            this.progressPercentage = totalBytes > 0 ? (float)bytesDownloaded / totalBytes * 100f : 0f;
        }
        
        public override string ToString()
        {
            return $"Download[{progressPercentage:F1}%, {bytesDownloaded}/{totalBytes} bytes]";
        }
    }

    #endregion

    #region 游戏消息专用数据类型（新增）

    /// <summary>
    /// 游戏消息信息
    /// </summary>
    [Serializable]
    public class GameMessageInfo
    {
        public string messageId = "";
        public string messageType = "RoundStart"; // 使用字符串避免枚举重复
        public string content = "";
        public DateTime timestamp = DateTime.Now;
        public string senderId = "";
        public string priority = "Normal"; // 使用字符串避免枚举重复
        
        public GameMessageInfo() 
        {
            messageId = Guid.NewGuid().ToString();
        }
        
        public GameMessageInfo(string type, string content)
        {
            this.messageId = Guid.NewGuid().ToString();
            this.messageType = type;
            this.content = content;
            this.timestamp = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"GameMessage[{messageType}] {content}";
        }
    }

    /// <summary>
    /// 投注消息信息
    /// </summary>
    [Serializable]
    public class BetMessageInfo
    {
        public string betId = "";
        public BaccaratBetType betType = BaccaratBetType.Player; // 使用现有枚举
        public decimal amount = 0m;
        public DateTime timestamp = DateTime.Now;
        public string tableId = "";
        public string roundId = "";
        
        public BetMessageInfo() 
        {
            betId = Guid.NewGuid().ToString();
        }
        
        public BetMessageInfo(BaccaratBetType betType, decimal amount, string tableId, string roundId)
        {
            this.betId = Guid.NewGuid().ToString();
            this.betType = betType;
            this.amount = amount;
            this.tableId = tableId;
            this.roundId = roundId;
            this.timestamp = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"BetMessage[{betType}, {amount}, Table:{tableId}]";
        }
    }

    /// <summary>
    /// 投注确认消息信息
    /// </summary>
    [Serializable]
    public class BetConfirmationMessageInfo
    {
        public string betId = "";
        public string status = "Pending"; // 使用字符串避免枚举重复
        public string message = "";
        public DateTime confirmationTime = DateTime.Now;
        public decimal confirmedAmount = 0m;
        
        public BetConfirmationMessageInfo() { }
        
        public BetConfirmationMessageInfo(string betId, string status, decimal amount)
        {
            this.betId = betId;
            this.status = status;
            this.confirmedAmount = amount;
            this.confirmationTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"BetConfirmation[{betId}, {status}, Amount:{confirmedAmount}]";
        }
    }

    /// <summary>
    /// 回合开始消息信息
    /// </summary>
    [Serializable]
    public class RoundStartMessageInfo
    {
        public string roundId = "";
        public string tableId = "";
        public DateTime startTime = DateTime.Now;
        public float bettingDuration = 30f;
        public string dealerId = "";
        public int roundNumber = 0;
        
        public RoundStartMessageInfo() { }
        
        public RoundStartMessageInfo(string roundId, string tableId, int roundNumber)
        {
            this.roundId = roundId;
            this.tableId = tableId;
            this.roundNumber = roundNumber;
            this.startTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"RoundStart[{roundId}, Round:{roundNumber}, Table:{tableId}]";
        }
    }

    /// <summary>
    /// 回合结果消息信息
    /// </summary>
    [Serializable]
    public class RoundResultMessageInfo
    {
        public string roundId = "";
        public BaccaratResult result = BaccaratResult.None; // 使用现有枚举
        public List<Card> playerCards = new List<Card>();
        public List<Card> bankerCards = new List<Card>();
        public int playerTotal = 0;
        public int bankerTotal = 0;
        public DateTime resultTime = DateTime.Now;
        
        public RoundResultMessageInfo() { }
        
        public RoundResultMessageInfo(string roundId, BaccaratResult result)
        {
            this.roundId = roundId;
            this.result = result;
            this.resultTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"RoundResult[{roundId}, {result}, Player:{playerTotal} vs Banker:{bankerTotal}]";
        }
    }

    /// <summary>
    /// 发牌消息信息
    /// </summary>
    [Serializable]
    public class DealCardsMessageInfo
    {
        public string roundId = "";
        public List<Card> cards = new List<Card>();
        public HandType handType = HandType.Player; // 使用现有枚举
        public int dealSequence = 0;
        public DateTime dealTime = DateTime.Now;
        public bool isVisible = true;
        
        public DealCardsMessageInfo() { }
        
        public DealCardsMessageInfo(string roundId, List<Card> cards, HandType handType)
        {
            this.roundId = roundId;
            this.cards = new List<Card>(cards ?? new List<Card>());
            this.handType = handType;
            this.dealTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"DealCards[{roundId}, {handType}, Cards:{cards.Count}]";
        }
    }

    /// <summary>
    /// 系统通知信息
    /// </summary>
    [Serializable]
    public class SystemNotificationInfo
    {
        public string notificationId = "";
        public string notificationType = "Information"; // 使用字符串避免枚举重复
        public string title = "";
        public string message = "";
        public DateTime timestamp = DateTime.Now;
        public string priority = "Normal"; // 使用字符串避免枚举重复
        public TimeSpan displayDuration = TimeSpan.FromSeconds(5);
        
        public SystemNotificationInfo() 
        {
            notificationId = Guid.NewGuid().ToString();
        }
        
        public SystemNotificationInfo(string title, string message, string type)
        {
            this.notificationId = Guid.NewGuid().ToString();
            this.title = title;
            this.message = message;
            this.notificationType = type;
            this.timestamp = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"SystemNotification[{notificationType}] {title}: {message}";
        }
    }

    #endregion

    #region 认证相关数据类型（新增）

    /// <summary>
    /// 认证信息
    /// </summary>
    [Serializable]
    public class AuthenticationInfo
    {
        public string userId = "";
        public string token = "";
        public DateTime authTime = DateTime.Now;
        public TimeSpan tokenExpiry = TimeSpan.FromHours(24);
        public string authType = "Guest"; // 使用字符串避免枚举重复
        public string deviceId = "";
        
        public AuthenticationInfo() { }
        
        public AuthenticationInfo(string userId, string token, string type)
        {
            this.userId = userId;
            this.token = token;
            this.authType = type;
            this.authTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"Authentication[{userId}, {authType}, Expires:{authTime.Add(tokenExpiry)}]";
        }
    }

    /// <summary>
    /// 认证失败信息
    /// </summary>
    [Serializable]
    public class AuthenticationFailureInfo
    {
        public string errorCode = "";
        public string errorMessage = "";
        public string reason = "InvalidCredentials"; // 使用字符串避免枚举重复
        public DateTime failureTime = DateTime.Now;
        public int attemptCount = 0;
        public bool isBlocked = false;
        
        public AuthenticationFailureInfo() { }
        
        public AuthenticationFailureInfo(string errorCode, string message, string reason)
        {
            this.errorCode = errorCode;
            this.errorMessage = message;
            this.reason = reason;
            this.failureTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"AuthFailure[{reason}] {errorMessage}";
        }
    }

    /// <summary>
    /// Token刷新信息
    /// </summary>
    [Serializable]
    public class TokenRefreshInfo
    {
        public string oldToken = "";
        public string newToken = "";
        public DateTime refreshTime = DateTime.Now;
        public TimeSpan newExpiry = TimeSpan.FromHours(24);
        public bool isAutoRefresh = false;
        
        public TokenRefreshInfo() { }
        
        public TokenRefreshInfo(string oldToken, string newToken, bool isAuto = false)
        {
            this.oldToken = oldToken;
            this.newToken = newToken;
            this.isAutoRefresh = isAuto;
            this.refreshTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"TokenRefresh[Auto:{isAutoRefresh}, Time:{refreshTime}]";
        }
    }

    /// <summary>
    /// 会话过期信息
    /// </summary>
    [Serializable]
    public class SessionExpiredInfo
    {
        public string sessionId = "";
        public DateTime expiryTime = DateTime.Now;
        public TimeSpan sessionDuration = TimeSpan.Zero;
        public string reason = "Timeout"; // 使用字符串避免枚举重复
        public bool canRenew = false;
        
        public SessionExpiredInfo() { }
        
        public SessionExpiredInfo(string sessionId, string reason, bool canRenew = false)
        {
            this.sessionId = sessionId;
            this.reason = reason;
            this.canRenew = canRenew;
            this.expiryTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"SessionExpired[{reason}, CanRenew:{canRenew}]";
        }
    }

    /// <summary>
    /// 安全验证信息
    /// </summary>
    [Serializable]
    public class SecurityVerificationInfo
    {
        public string verificationId = "";
        public string verificationType = "TwoFactor"; // 使用字符串避免枚举重复
        public string challenge = "";
        public DateTime challengeTime = DateTime.Now;
        public TimeSpan timeLimit = TimeSpan.FromMinutes(5);
        
        public SecurityVerificationInfo() 
        {
            verificationId = Guid.NewGuid().ToString();
        }
        
        public SecurityVerificationInfo(string type, string challenge)
        {
            this.verificationId = Guid.NewGuid().ToString();
            this.verificationType = type;
            this.challenge = challenge;
            this.challengeTime = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"SecurityVerification[{verificationType}, Challenge:{challenge}]";
        }
    }

    #endregion
}
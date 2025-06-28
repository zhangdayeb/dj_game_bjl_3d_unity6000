// Assets/Core/Events/NetworkEvents.cs
// 网络相关事件定义 - 完整版，移除重复定义，统一引用标准数据类型
// 统一网络事件管理，支持WebSocket和HTTP通信

using System;
using System.Collections.Generic;
using UnityEngine;
using BaccaratGame.Data;    

namespace BaccaratGame.Core.Events
{
    /// <summary>
    /// 网络相关事件定义
    /// 包含连接管理、消息处理、错误处理等所有网络事件
    /// </summary>
    public static class NetworkEvents
    {
        #region 连接生命周期事件

        /// <summary>
        /// 开始连接事件
        /// </summary>
        public static event Action<NetworkConnectionInfo> OnConnectionStarted;

        /// <summary>
        /// 连接成功事件
        /// </summary>
        public static event Action<NetworkConnectionInfo> OnConnectionEstablished;

        /// <summary>
        /// 连接失败事件
        /// </summary>
        public static event Action<ConnectionFailureInfo> OnConnectionFailed;

        /// <summary>
        /// 连接断开事件
        /// </summary>
        public static event Action<DisconnectionInfo> OnConnectionLost;

        /// <summary>
        /// 重连开始事件
        /// </summary>
        public static event Action<ReconnectionInfo> OnReconnectionStarted;

        /// <summary>
        /// 重连成功事件
        /// </summary>
        public static event Action<ReconnectionInfo> OnReconnectionSucceeded;

        /// <summary>
        /// 重连失败事件
        /// </summary>
        public static event Action<ReconnectionInfo> OnReconnectionFailed;

        /// <summary>
        /// 连接超时事件
        /// </summary>
        public static event Action<TimeoutInfo> OnConnectionTimeout;

        #endregion

        #region WebSocket事件

        /// <summary>
        /// WebSocket消息接收事件
        /// </summary>
        public static event Action<WebSocketMessageInfo> OnWebSocketMessageReceived;

        /// <summary>
        /// WebSocket消息发送事件
        /// </summary>
        public static event Action<WebSocketMessageInfo> OnWebSocketMessageSent;

        /// <summary>
        /// WebSocket错误事件
        /// </summary>
        public static event Action<WebSocketErrorInfo> OnWebSocketError;

        /// <summary>
        /// WebSocket状态变化事件
        /// </summary>
        public static event Action<WebSocketStateInfo> OnWebSocketStateChanged;

        /// <summary>
        /// 心跳包事件
        /// </summary>
        public static event Action<HeartbeatInfo> OnHeartbeatSent;

        /// <summary>
        /// 心跳响应事件
        /// </summary>
        public static event Action<HeartbeatInfo> OnHeartbeatReceived;

        #endregion

        #region HTTP请求事件

        /// <summary>
        /// HTTP请求开始事件
        /// </summary>
        public static event Action<HttpRequestInfo> OnHttpRequestStarted;

        /// <summary>
        /// HTTP请求成功事件
        /// </summary>
        public static event Action<HttpResponseInfo> OnHttpRequestSucceeded;

        /// <summary>
        /// HTTP请求失败事件
        /// </summary>
        public static event Action<HttpErrorInfo> OnHttpRequestFailed;

        /// <summary>
        /// HTTP请求超时事件
        /// </summary>
        public static event Action<HttpTimeoutInfo> OnHttpRequestTimeout;

        /// <summary>
        /// 文件上传进度事件
        /// </summary>
        public static event Action<UploadProgressInfo> OnUploadProgress;

        /// <summary>
        /// 文件下载进度事件
        /// </summary>
        public static event Action<DownloadProgressInfo> OnDownloadProgress;

        #endregion

        #region 游戏消息事件

        /// <summary>
        /// 游戏消息接收事件
        /// </summary>
        public static event Action<GameMessageInfo> OnGameMessageReceived;

        /// <summary>
        /// 投注消息发送事件
        /// </summary>
        public static event Action<BetMessageInfo> OnBetMessageSent;

        /// <summary>
        /// 投注确认消息接收事件
        /// </summary>
        public static event Action<BetConfirmationMessageInfo> OnBetConfirmationReceived;

        /// <summary>
        /// 回合开始消息接收事件
        /// </summary>
        public static event Action<RoundStartMessageInfo> OnRoundStartMessageReceived;

        /// <summary>
        /// 回合结果消息接收事件
        /// </summary>
        public static event Action<RoundResultMessageInfo> OnRoundResultMessageReceived;

        /// <summary>
        /// 发牌消息接收事件
        /// </summary>
        public static event Action<DealCardsMessageInfo> OnDealCardsMessageReceived;

        /// <summary>
        /// 系统通知消息接收事件
        /// </summary>
        public static event Action<SystemNotificationInfo> OnSystemNotificationReceived;

        #endregion

        #region 认证和安全事件

        /// <summary>
        /// 认证请求事件
        /// </summary>
        public static event Action<AuthenticationInfo> OnAuthenticationRequested;

        /// <summary>
        /// 认证成功事件
        /// </summary>
        public static event Action<AuthenticationInfo> OnAuthenticationSucceeded;

        /// <summary>
        /// 认证失败事件
        /// </summary>
        public static event Action<AuthenticationFailureInfo> OnAuthenticationFailed;

        /// <summary>
        /// Token刷新事件
        /// </summary>
        public static event Action<TokenRefreshInfo> OnTokenRefreshed;

        /// <summary>
        /// 会话过期事件
        /// </summary>
        public static event Action<SessionExpiredInfo> OnSessionExpired;

        /// <summary>
        /// 安全验证事件
        /// </summary>
        public static event Action<SecurityVerificationInfo> OnSecurityVerificationRequired;

        #endregion

        #region 事件触发方法

        public static void TriggerConnectionStarted(NetworkConnectionInfo info)
        {
            OnConnectionStarted?.Invoke(info);
        }

        public static void TriggerConnectionEstablished(NetworkConnectionInfo info)
        {
            OnConnectionEstablished?.Invoke(info);
        }

        public static void TriggerConnectionFailed(ConnectionFailureInfo info)
        {
            OnConnectionFailed?.Invoke(info);
        }

        public static void TriggerConnectionLost(DisconnectionInfo info)
        {
            OnConnectionLost?.Invoke(info);
        }

        public static void TriggerReconnectionStarted(ReconnectionInfo info)
        {
            OnReconnectionStarted?.Invoke(info);
        }

        public static void TriggerReconnectionSucceeded(ReconnectionInfo info)
        {
            OnReconnectionSucceeded?.Invoke(info);
        }

        public static void TriggerReconnectionFailed(ReconnectionInfo info)
        {
            OnReconnectionFailed?.Invoke(info);
        }

        public static void TriggerConnectionTimeout(TimeoutInfo info)
        {
            OnConnectionTimeout?.Invoke(info);
        }

        public static void TriggerWebSocketMessageReceived(WebSocketMessageInfo info)
        {
            OnWebSocketMessageReceived?.Invoke(info);
        }

        public static void TriggerWebSocketMessageSent(WebSocketMessageInfo info)
        {
            OnWebSocketMessageSent?.Invoke(info);
        }

        public static void TriggerWebSocketError(WebSocketErrorInfo info)
        {
            OnWebSocketError?.Invoke(info);
        }

        public static void TriggerWebSocketStateChanged(WebSocketStateInfo info)
        {
            OnWebSocketStateChanged?.Invoke(info);
        }

        public static void TriggerHeartbeatSent(HeartbeatInfo info)
        {
            OnHeartbeatSent?.Invoke(info);
        }

        public static void TriggerHeartbeatReceived(HeartbeatInfo info)
        {
            OnHeartbeatReceived?.Invoke(info);
        }

        public static void TriggerHttpRequestStarted(HttpRequestInfo info)
        {
            OnHttpRequestStarted?.Invoke(info);
        }

        public static void TriggerHttpRequestSucceeded(HttpResponseInfo info)
        {
            OnHttpRequestSucceeded?.Invoke(info);
        }

        public static void TriggerHttpRequestFailed(HttpErrorInfo info)
        {
            OnHttpRequestFailed?.Invoke(info);
        }

        public static void TriggerHttpRequestTimeout(HttpTimeoutInfo info)
        {
            OnHttpRequestTimeout?.Invoke(info);
        }

        public static void TriggerUploadProgress(UploadProgressInfo info)
        {
            OnUploadProgress?.Invoke(info);
        }

        public static void TriggerDownloadProgress(DownloadProgressInfo info)
        {
            OnDownloadProgress?.Invoke(info);
        }

        public static void TriggerGameMessageReceived(GameMessageInfo info)
        {
            OnGameMessageReceived?.Invoke(info);
        }

        public static void TriggerBetMessageSent(BetMessageInfo info)
        {
            OnBetMessageSent?.Invoke(info);
        }

        public static void TriggerBetConfirmationReceived(BetConfirmationMessageInfo info)
        {
            OnBetConfirmationReceived?.Invoke(info);
        }

        public static void TriggerRoundStartMessageReceived(RoundStartMessageInfo info)
        {
            OnRoundStartMessageReceived?.Invoke(info);
        }

        public static void TriggerRoundResultMessageReceived(RoundResultMessageInfo info)
        {
            OnRoundResultMessageReceived?.Invoke(info);
        }

        public static void TriggerDealCardsMessageReceived(DealCardsMessageInfo info)
        {
            OnDealCardsMessageReceived?.Invoke(info);
        }

        public static void TriggerSystemNotificationReceived(SystemNotificationInfo info)
        {
            OnSystemNotificationReceived?.Invoke(info);
        }

        public static void TriggerAuthenticationRequested(AuthenticationInfo info)
        {
            OnAuthenticationRequested?.Invoke(info);
        }

        public static void TriggerAuthenticationSucceeded(AuthenticationInfo info)
        {
            OnAuthenticationSucceeded?.Invoke(info);
        }

        public static void TriggerAuthenticationFailed(AuthenticationFailureInfo info)
        {
            OnAuthenticationFailed?.Invoke(info);
        }

        public static void TriggerTokenRefreshed(TokenRefreshInfo info)
        {
            OnTokenRefreshed?.Invoke(info);
        }

        public static void TriggerSessionExpired(SessionExpiredInfo info)
        {
            OnSessionExpired?.Invoke(info);
        }

        public static void TriggerSecurityVerificationRequired(SecurityVerificationInfo info)
        {
            OnSecurityVerificationRequired?.Invoke(info);
        }

        #endregion

        #region 事件清理

        /// <summary>
        /// 清除所有网络事件订阅
        /// </summary>
        public static void ClearAllSubscriptions()
        {
            OnConnectionStarted = null;
            OnConnectionEstablished = null;
            OnConnectionFailed = null;
            OnConnectionLost = null;
            OnReconnectionStarted = null;
            OnReconnectionSucceeded = null;
            OnReconnectionFailed = null;
            OnConnectionTimeout = null;
            
            OnWebSocketMessageReceived = null;
            OnWebSocketMessageSent = null;
            OnWebSocketError = null;
            OnWebSocketStateChanged = null;
            OnHeartbeatSent = null;
            OnHeartbeatReceived = null;
            
            OnHttpRequestStarted = null;
            OnHttpRequestSucceeded = null;
            OnHttpRequestFailed = null;
            OnHttpRequestTimeout = null;
            OnUploadProgress = null;
            OnDownloadProgress = null;
            
            OnGameMessageReceived = null;
            OnBetMessageSent = null;
            OnBetConfirmationReceived = null;
            OnRoundStartMessageReceived = null;
            OnRoundResultMessageReceived = null;
            OnDealCardsMessageReceived = null;
            OnSystemNotificationReceived = null;
            
            OnAuthenticationRequested = null;
            OnAuthenticationSucceeded = null;
            OnAuthenticationFailed = null;
            OnTokenRefreshed = null;
            OnSessionExpired = null;
            OnSecurityVerificationRequired = null;
        }

        #endregion
    }
}
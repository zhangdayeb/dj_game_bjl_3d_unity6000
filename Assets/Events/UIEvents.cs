// Assets/Core/Events/UIEvents.cs
// UI相关事件定义 - 完整版，移除重复定义，统一引用标准数据类型
// 解耦UI组件间的直接依赖，实现响应式UI更新

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BaccaratGame.Data;   

namespace BaccaratGame.Core.Events
{
    /// <summary>
    /// UI相关事件定义
    /// 包含界面交互、更新、动画、状态变化等所有UI事件
    /// </summary>
    public static class UIEvents
    {
        #region 界面生命周期事件

        /// <summary>
        /// UI初始化完成事件
        /// </summary>
        public static event Action OnUIInitialized;

        /// <summary>
        /// 界面显示事件
        /// </summary>
        public static event Action<UIPanel> OnPanelShown;

        /// <summary>
        /// 界面隐藏事件
        /// </summary>
        public static event Action<UIPanel> OnPanelHidden;

        /// <summary>
        /// 界面切换事件
        /// </summary>
        public static event Action<UIPanel, UIPanel> OnPanelSwitched; // 参数: 旧界面, 新界面

        /// <summary>
        /// 模态对话框显示事件
        /// </summary>
        public static event Action<ModalDialog> OnModalShown;

        /// <summary>
        /// 模态对话框关闭事件
        /// </summary>
        public static event Action<ModalDialog> OnModalClosed;

        #endregion

        #region 投注UI事件

        /// <summary>
        /// 投注按钮点击事件
        /// </summary>
        public static event Action<BetButtonClickInfo> OnBetButtonClicked;

        /// <summary>
        /// 筹码选择事件
        /// </summary>
        public static event Action<ChipSelectionInfo> OnChipSelected;

        /// <summary>
        /// 投注区域高亮事件
        /// </summary>
        public static event Action<BetAreaHighlightInfo> OnBetAreaHighlighted;

        /// <summary>
        /// 投注金额输入事件
        /// </summary>
        public static event Action<BetAmountInputInfo> OnBetAmountInput;

        /// <summary>
        /// 投注确认UI显示事件
        /// </summary>
        public static event Action<BetConfirmationInfo> OnBetConfirmationShown;

        /// <summary>
        /// 投注历史更新事件
        /// </summary>
        public static event Action<BetHistoryInfo> OnBetHistoryUpdated;

        // ========== 新增：缺失的投注相关事件 ==========
        /// <summary>
        /// 投注放置事件
        /// </summary>
        public static event Action<BetInfo> OnBetPlaced;

        /// <summary>
        /// 投注结算事件
        /// </summary>
        public static event Action<BetSettlement> OnBetSettled;

        /// <summary>
        /// 投注请求事件
        /// </summary>
        public static event Action<BetRequestInfo> OnBetRequested;

        #endregion

        #region 游戏状态UI事件

        /// <summary>
        /// 余额显示更新事件
        /// </summary>
        public static event Action<BalanceDisplayInfo> OnBalanceDisplayUpdated;

        /// <summary>
        /// 倒计时显示更新事件
        /// </summary>
        public static event Action<CountdownDisplayInfo> OnCountdownDisplayUpdated;

        /// <summary>
        /// 游戏状态指示器更新事件
        /// </summary>
        public static event Action<GameStatusInfo> OnGameStatusUpdated;

        /// <summary>
        /// 连接状态指示器更新事件
        /// </summary>
        public static event Action<ConnectionStatusInfo> OnConnectionStatusUpdated;

        /// <summary>
        /// 回合信息显示更新事件
        /// </summary>
        public static event Action<RoundDisplayInfo> OnRoundDisplayUpdated;

        // ========== 新增：缺失的游戏状态事件 ==========
        /// <summary>
        /// 游戏状态变化事件
        /// </summary>
        public static event Action<GameState> OnGameStateChanged;

        /// <summary>
        /// 回合状态变化事件
        /// </summary>
        public static event Action<RoundState> OnRoundStateChanged;

        /// <summary>
        /// 回合完成事件
        /// </summary>
        public static event Action<RoundResult> OnRoundCompleted;

        /// <summary>
        /// 余额更新事件
        /// </summary>
        public static event Action<decimal> OnBalanceUpdated;

        /// <summary>
        /// 倒计时更新事件
        /// </summary>
        public static event Action<int> OnCountdownUpdated;

        /// <summary>
        /// 连接状态变化事件
        /// </summary>
        public static event Action<ConnectionState> OnConnectionStateChanged;

        #endregion

        #region 卡牌UI事件

        /// <summary>
        /// 卡牌动画开始事件
        /// </summary>
        public static event Action<CardAnimationInfo> OnCardAnimationStarted;

        /// <summary>
        /// 卡牌动画完成事件
        /// </summary>
        public static event Action<CardAnimationInfo> OnCardAnimationCompleted;

        /// <summary>
        /// 卡牌翻转事件
        /// </summary>
        public static event Action<CardFlipInfo> OnCardFlipped;

        /// <summary>
        /// 手牌总点数显示更新事件
        /// </summary>
        public static event Action<HandTotalDisplayInfo> OnHandTotalDisplayUpdated;

        /// <summary>
        /// 卡牌发光效果事件
        /// </summary>
        public static event Action<CardGlowInfo> OnCardGlowTriggered;

        // ========== 新增：缺失的卡牌事件 ==========
        /// <summary>
        /// 卡牌发牌事件
        /// </summary>
        public static event Action<Card, HandType> OnCardDealt;

        /// <summary>
        /// 卡牌动画请求事件
        /// </summary>
        public static event Action<CardAnimationInfo> OnCardAnimationRequested;

        #endregion

        #region 动画和特效事件

        /// <summary>
        /// 筹码动画开始事件
        /// </summary>
        public static event Action<ChipAnimationInfo> OnChipAnimationStarted;

        /// <summary>
        /// 筹码动画完成事件
        /// </summary>
        public static event Action<ChipAnimationInfo> OnChipAnimationCompleted;

        /// <summary>
        /// 获胜特效触发事件
        /// </summary>
        public static event Action<WinEffectInfo> OnWinEffectTriggered;

        /// <summary>
        /// 闪光特效触发事件
        /// </summary>
        public static event Action<FlashEffectInfo> OnFlashEffectTriggered;

        /// <summary>
        /// 粒子特效触发事件
        /// </summary>
        public static event Action<ParticleEffectInfo> OnParticleEffectTriggered;

        /// <summary>
        /// 倒计时动画触发事件
        /// </summary>
        public static event Action<CountdownAnimationInfo> OnCountdownAnimationTriggered;

        // ========== 新增：缺失的特效事件 ==========
        /// <summary>
        /// 筹码滑动请求事件
        /// </summary>
        public static event Action<ChipAnimationInfo> OnChipSlideRequested;

        /// <summary>
        /// 中奖特效请求事件
        /// </summary>
        public static event Action<WinEffectInfo> OnWinEffectRequested;

        /// <summary>
        /// 闪光特效请求事件
        /// </summary>
        public static event Action<FlashEffectInfo> OnFlashEffectRequested;

        #endregion

        #region 交互事件

        /// <summary>
        /// 按钮hover事件
        /// </summary>
        public static event Action<ButtonHoverInfo> OnButtonHovered;

        /// <summary>
        /// 按钮点击事件
        /// </summary>
        public static event Action<ButtonClickInfo> OnButtonClicked;

        /// <summary>
        /// 滑动操作事件
        /// </summary>
        public static event Action<SwipeInfo> OnUISwipe;

        /// <summary>
        /// 拖拽操作事件
        /// </summary>
        public static event Action<DragInfo> OnUIDrag;

        /// <summary>
        /// 双击事件
        /// </summary>
        public static event Action<DoubleClickInfo> OnUIDoubleClick;

        /// <summary>
        /// 长按事件
        /// </summary>
        public static event Action<LongPressInfo> OnUILongPress;

        #endregion

        #region 音效和反馈事件

        /// <summary>
        /// UI音效播放请求事件
        /// </summary>
        public static event Action<UISoundInfo> OnUISoundRequested;

        /// <summary>
        /// 触觉反馈请求事件
        /// </summary>
        public static event Action<HapticFeedbackInfo> OnHapticFeedbackRequested;

        /// <summary>
        /// Toast消息显示事件
        /// </summary>
        public static event Action<ToastInfo> OnToastRequested;

        /// <summary>
        /// 通知显示事件
        /// </summary>
        public static event Action<NotificationInfo> OnNotificationRequested;

        #endregion

        #region 布局和适配事件

        /// <summary>
        /// 屏幕方向变化事件
        /// </summary>
        public static event Action<ScreenOrientationInfo> OnScreenOrientationChanged;

        /// <summary>
        /// 分辨率变化事件
        /// </summary>
        public static event Action<ResolutionInfo> OnResolutionChanged;

        /// <summary>
        /// 安全区域变化事件
        /// </summary>
        public static event Action<SafeAreaInfo> OnSafeAreaChanged;

        /// <summary>
        /// UI缩放变化事件
        /// </summary>
        public static event Action<UIScaleInfo> OnUIScaleChanged;

        #endregion

        #region 事件触发方法

        public static void TriggerUIInitialized()
        {
            OnUIInitialized?.Invoke();
        }

        public static void TriggerPanelShown(UIPanel panel)
        {
            OnPanelShown?.Invoke(panel);
        }

        public static void TriggerPanelHidden(UIPanel panel)
        {
            OnPanelHidden?.Invoke(panel);
        }

        public static void TriggerPanelSwitched(UIPanel oldPanel, UIPanel newPanel)
        {
            OnPanelSwitched?.Invoke(oldPanel, newPanel);
        }

        public static void TriggerModalShown(ModalDialog modal)
        {
            OnModalShown?.Invoke(modal);
        }

        public static void TriggerModalClosed(ModalDialog modal)
        {
            OnModalClosed?.Invoke(modal);
        }

        public static void TriggerBetButtonClicked(BetButtonClickInfo info)
        {
            OnBetButtonClicked?.Invoke(info);
        }

        public static void TriggerChipSelected(ChipSelectionInfo info)
        {
            OnChipSelected?.Invoke(info);
        }

        public static void TriggerBetAreaHighlighted(BetAreaHighlightInfo info)
        {
            OnBetAreaHighlighted?.Invoke(info);
        }

        public static void TriggerBetAmountInput(BetAmountInputInfo info)
        {
            OnBetAmountInput?.Invoke(info);
        }

        public static void TriggerBetConfirmationShown(BetConfirmationInfo info)
        {
            OnBetConfirmationShown?.Invoke(info);
        }

        public static void TriggerBetHistoryUpdated(BetHistoryInfo info)
        {
            OnBetHistoryUpdated?.Invoke(info);
        }

        // ========== 新增：缺失事件的触发方法 ==========
        public static void TriggerBetPlaced(BetInfo betInfo)
        {
            OnBetPlaced?.Invoke(betInfo);
        }

        public static void TriggerBetSettled(BetSettlement settlement)
        {
            OnBetSettled?.Invoke(settlement);
        }

        public static void TriggerBetRequested(BetRequestInfo requestInfo)
        {
            OnBetRequested?.Invoke(requestInfo);
        }

        public static void TriggerGameStateChanged(GameState newState)
        {
            OnGameStateChanged?.Invoke(newState);
        }

        public static void TriggerRoundStateChanged(RoundState newState)
        {
            OnRoundStateChanged?.Invoke(newState);
        }

        public static void TriggerRoundCompleted(RoundResult result)
        {
            OnRoundCompleted?.Invoke(result);
        }

        public static void TriggerBalanceUpdated(decimal newBalance)
        {
            OnBalanceUpdated?.Invoke(newBalance);
        }

        public static void TriggerCountdownUpdated(int seconds)
        {
            OnCountdownUpdated?.Invoke(seconds);
        }

        public static void TriggerConnectionStateChanged(ConnectionState newState)
        {
            OnConnectionStateChanged?.Invoke(newState);
        }

        public static void TriggerCardDealt(Card card, HandType handType)
        {
            OnCardDealt?.Invoke(card, handType);
        }

        public static void TriggerCardAnimationRequested(CardAnimationInfo animInfo)
        {
            OnCardAnimationRequested?.Invoke(animInfo);
        }

        public static void TriggerChipSlideRequested(ChipAnimationInfo animInfo)
        {
            OnChipSlideRequested?.Invoke(animInfo);
        }

        public static void TriggerWinEffectRequested(WinEffectInfo effectInfo)
        {
            OnWinEffectRequested?.Invoke(effectInfo);
        }

        public static void TriggerFlashEffectRequested(FlashEffectInfo flashInfo)
        {
            OnFlashEffectRequested?.Invoke(flashInfo);
        }

        // ========== 原有触发方法继续 ==========
        public static void TriggerBalanceDisplayUpdated(BalanceDisplayInfo info)
        {
            OnBalanceDisplayUpdated?.Invoke(info);
        }

        public static void TriggerCountdownDisplayUpdated(CountdownDisplayInfo info)
        {
            OnCountdownDisplayUpdated?.Invoke(info);
        }

        public static void TriggerGameStatusUpdated(GameStatusInfo info)
        {
            OnGameStatusUpdated?.Invoke(info);
        }

        public static void TriggerConnectionStatusUpdated(ConnectionStatusInfo info)
        {
            OnConnectionStatusUpdated?.Invoke(info);
        }

        public static void TriggerRoundDisplayUpdated(RoundDisplayInfo info)
        {
            OnRoundDisplayUpdated?.Invoke(info);
        }

        public static void TriggerCardAnimationStarted(CardAnimationInfo info)
        {
            OnCardAnimationStarted?.Invoke(info);
        }

        public static void TriggerCardAnimationCompleted(CardAnimationInfo info)
        {
            OnCardAnimationCompleted?.Invoke(info);
        }

        public static void TriggerCardFlipped(CardFlipInfo info)
        {
            OnCardFlipped?.Invoke(info);
        }

        public static void TriggerHandTotalDisplayUpdated(HandTotalDisplayInfo info)
        {
            OnHandTotalDisplayUpdated?.Invoke(info);
        }

        public static void TriggerCardGlowTriggered(CardGlowInfo info)
        {
            OnCardGlowTriggered?.Invoke(info);
        }

        public static void TriggerChipAnimationStarted(ChipAnimationInfo info)
        {
            OnChipAnimationStarted?.Invoke(info);
        }

        public static void TriggerChipAnimationCompleted(ChipAnimationInfo info)
        {
            OnChipAnimationCompleted?.Invoke(info);
        }

        public static void TriggerWinEffectTriggered(WinEffectInfo info)
        {
            OnWinEffectTriggered?.Invoke(info);
        }

        public static void TriggerFlashEffectTriggered(FlashEffectInfo info)
        {
            OnFlashEffectTriggered?.Invoke(info);
        }

        public static void TriggerParticleEffectTriggered(ParticleEffectInfo info)
        {
            OnParticleEffectTriggered?.Invoke(info);
        }

        public static void TriggerCountdownAnimationTriggered(CountdownAnimationInfo info)
        {
            OnCountdownAnimationTriggered?.Invoke(info);
        }

        public static void TriggerButtonHovered(ButtonHoverInfo info)
        {
            OnButtonHovered?.Invoke(info);
        }

        public static void TriggerButtonClicked(ButtonClickInfo info)
        {
            OnButtonClicked?.Invoke(info);
        }

        public static void TriggerUISwipe(SwipeInfo info)
        {
            OnUISwipe?.Invoke(info);
        }

        public static void TriggerUIDrag(DragInfo info)
        {
            OnUIDrag?.Invoke(info);
        }

        public static void TriggerUIDoubleClick(DoubleClickInfo info)
        {
            OnUIDoubleClick?.Invoke(info);
        }

        public static void TriggerUILongPress(LongPressInfo info)
        {
            OnUILongPress?.Invoke(info);
        }

        public static void TriggerUISoundRequested(UISoundInfo info)
        {
            OnUISoundRequested?.Invoke(info);
        }

        public static void TriggerHapticFeedbackRequested(HapticFeedbackInfo info)
        {
            OnHapticFeedbackRequested?.Invoke(info);
        }

        public static void TriggerToastRequested(ToastInfo info)
        {
            OnToastRequested?.Invoke(info);
        }

        public static void TriggerNotificationRequested(NotificationInfo info)
        {
            OnNotificationRequested?.Invoke(info);
        }

        public static void TriggerScreenOrientationChanged(ScreenOrientationInfo info)
        {
            OnScreenOrientationChanged?.Invoke(info);
        }

        public static void TriggerResolutionChanged(ResolutionInfo info)
        {
            OnResolutionChanged?.Invoke(info);
        }

        public static void TriggerSafeAreaChanged(SafeAreaInfo info)
        {
            OnSafeAreaChanged?.Invoke(info);
        }

        public static void TriggerUIScaleChanged(UIScaleInfo info)
        {
            OnUIScaleChanged?.Invoke(info);
        }

        #endregion

        #region 事件清理

        /// <summary>
        /// 清除所有UI事件订阅
        /// </summary>
        public static void ClearAllSubscriptions()
        {
            OnUIInitialized = null;
            OnPanelShown = null;
            OnPanelHidden = null;
            OnPanelSwitched = null;
            OnModalShown = null;
            OnModalClosed = null;
            
            OnBetButtonClicked = null;
            OnChipSelected = null;
            OnBetAreaHighlighted = null;
            OnBetAmountInput = null;
            OnBetConfirmationShown = null;
            OnBetHistoryUpdated = null;
            
            // 新增事件清理
            OnBetPlaced = null;
            OnBetSettled = null;
            OnBetRequested = null;
            
            OnBalanceDisplayUpdated = null;
            OnCountdownDisplayUpdated = null;
            OnGameStatusUpdated = null;
            OnConnectionStatusUpdated = null;
            OnRoundDisplayUpdated = null;
            
            // 新增事件清理
            OnGameStateChanged = null;
            OnRoundStateChanged = null;
            OnRoundCompleted = null;
            OnBalanceUpdated = null;
            OnCountdownUpdated = null;
            OnConnectionStateChanged = null;
            
            OnCardAnimationStarted = null;
            OnCardAnimationCompleted = null;
            OnCardFlipped = null;
            OnHandTotalDisplayUpdated = null;
            OnCardGlowTriggered = null;
            
            // 新增事件清理
            OnCardDealt = null;
            OnCardAnimationRequested = null;
            
            OnChipAnimationStarted = null;
            OnChipAnimationCompleted = null;
            OnWinEffectTriggered = null;
            OnFlashEffectTriggered = null;
            OnParticleEffectTriggered = null;
            OnCountdownAnimationTriggered = null;
            
            // 新增事件清理
            OnChipSlideRequested = null;
            OnWinEffectRequested = null;
            OnFlashEffectRequested = null;
            
            OnButtonHovered = null;
            OnButtonClicked = null;
            OnUISwipe = null;
            OnUIDrag = null;
            OnUIDoubleClick = null;
            OnUILongPress = null;
            
            OnUISoundRequested = null;
            OnHapticFeedbackRequested = null;
            OnToastRequested = null;
            OnNotificationRequested = null;
            
            OnScreenOrientationChanged = null;
            OnResolutionChanged = null;
            OnSafeAreaChanged = null;
            OnUIScaleChanged = null;
        }

        #endregion
    }
}
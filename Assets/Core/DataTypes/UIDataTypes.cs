// Assets/Core/Data/Types/UIDataTypes.cs
// UI专用数据类型定义 - 精简版
// 只包含UIEvents需要的新增数据结构
// 移除所有与现有文件重复的定义，使用字符串替代重复枚举
// 创建时间: 2025/6/22

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using BaccaratGame.Data;      

namespace BaccaratGame.Data
{
    #region UI界面相关数据类型（新增）

    /// <summary>
    /// 模态对话框信息
    /// </summary>
    [Serializable]
    public class ModalDialog
    {
        public string dialogId = "";
        public string dialogType = "Information"; // 使用字符串避免枚举重复
        public string title = "";
        public string message = "";
        public List<string> buttonTexts = new List<string>();
        public System.Action<int> onButtonClicked;
        
        public ModalDialog() 
        {
            dialogId = Guid.NewGuid().ToString();
        }
        
        public ModalDialog(string title, string message, string type = "Information")
        {
            this.dialogId = Guid.NewGuid().ToString();
            this.title = title;
            this.message = message;
            this.dialogType = type;
        }
        
        public override string ToString()
        {
            return $"ModalDialog[{dialogType}] {title}: {message}";
        }
    }

    #endregion

    #region 投注UI相关数据类型（新增）

    /// <summary>
    /// 投注按钮点击信息
    /// </summary>
    [Serializable]
    public class BetButtonClickInfo
    {
        public string buttonId = "";
        public BaccaratBetType betType = BaccaratBetType.Player; // 使用现有枚举
        public decimal chipValue = 0m;
        public Vector2 clickPosition = Vector2.zero;
        public DateTime timestamp = DateTime.Now;
        
        public BetButtonClickInfo() { }
        
        public BetButtonClickInfo(string buttonId, BaccaratBetType betType, decimal chipValue)
        {
            this.buttonId = buttonId;
            this.betType = betType;
            this.chipValue = chipValue;
            this.timestamp = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"BetButtonClick[{betType}, Value:{chipValue}, Button:{buttonId}]";
        }
    }

    /// <summary>
    /// 筹码选择信息
    /// </summary>
    [Serializable]
    public class ChipSelectionInfo
    {
        public string chipId = "";
        public decimal value = 0m;
        public Color chipColor = Color.white;
        public bool isSelected = false;
        public Vector2 position = Vector2.zero;
        
        public ChipSelectionInfo() { }
        
        public ChipSelectionInfo(string chipId, decimal value, bool isSelected)
        {
            this.chipId = chipId;
            this.value = value;
            this.isSelected = isSelected;
        }
        
        public override string ToString()
        {
            return $"ChipSelection[{chipId}, Value:{value}, Selected:{isSelected}]";
        }
    }

    /// <summary>
    /// 投注区域高亮信息
    /// </summary>
    [Serializable]
    public class BetAreaHighlightInfo
    {
        public BaccaratBetType betType = BaccaratBetType.Player; // 使用现有枚举
        public bool isHighlighted = false;
        public Color highlightColor = Color.yellow;
        public float highlightIntensity = 1f;
        public float duration = 1f;
        
        public BetAreaHighlightInfo() { }
        
        public BetAreaHighlightInfo(BaccaratBetType betType, bool isHighlighted)
        {
            this.betType = betType;
            this.isHighlighted = isHighlighted;
        }
        
        public override string ToString()
        {
            return $"BetAreaHighlight[{betType}, Highlighted:{isHighlighted}]";
        }
    }

    /// <summary>
    /// 投注金额输入信息
    /// </summary>
    [Serializable]
    public class BetAmountInputInfo
    {
        public decimal amount = 0m;
        public BaccaratBetType betType = BaccaratBetType.Player; // 使用现有枚举
        public bool isValid = false;
        public string validationMessage = "";
        
        public BetAmountInputInfo() { }
        
        public BetAmountInputInfo(decimal amount, BaccaratBetType betType, bool isValid)
        {
            this.amount = amount;
            this.betType = betType;
            this.isValid = isValid;
        }
        
        public override string ToString()
        {
            return $"BetAmountInput[{betType}, Amount:{amount}, Valid:{isValid}]";
        }
    }

    /// <summary>
    /// 投注确认信息
    /// </summary>
    [Serializable]
    public class BetConfirmationInfo
    {
        public List<BetInfo> bets = new List<BetInfo>(); // 使用现有类型
        public decimal totalAmount = 0m;
        public float timeRemaining = 0f;
        public bool canConfirm = false;
        
        public BetConfirmationInfo() { }
        
        public BetConfirmationInfo(List<BetInfo> bets, float timeRemaining)
        {
            this.bets = new List<BetInfo>(bets ?? new List<BetInfo>());
            this.timeRemaining = timeRemaining;
            this.totalAmount = this.bets.Sum(b => b.amount);
        }
        
        public override string ToString()
        {
            return $"BetConfirmation[Bets:{bets.Count}, Total:{totalAmount}, Time:{timeRemaining}s]";
        }
    }

    /// <summary>
    /// 投注历史信息
    /// </summary>
    [Serializable]
    public class BetHistoryInfo
    {
        public List<BetInfo> recentBets = new List<BetInfo>(); // 使用现有类型
        public int totalBets = 0;
        public decimal totalAmount = 0m;
        public int pageIndex = 0;
        public int totalPages = 0;
        
        public BetHistoryInfo() { }
        
        public BetHistoryInfo(List<BetInfo> bets, int totalBets, int pageIndex, int totalPages)
        {
            this.recentBets = new List<BetInfo>(bets ?? new List<BetInfo>());
            this.totalBets = totalBets;
            this.pageIndex = pageIndex;
            this.totalPages = totalPages;
            this.totalAmount = this.recentBets.Sum(b => b.amount);
        }
        
        public override string ToString()
        {
            return $"BetHistory[Page:{pageIndex+1}/{totalPages}, Bets:{totalBets}, Amount:{totalAmount}]";
        }
    }

    #endregion

    #region 游戏状态UI相关数据类型（新增）

    /// <summary>
    /// 余额显示信息
    /// </summary>
    [Serializable]
    public class BalanceDisplayInfo
    {
        public decimal currentBalance = 0m;
        public decimal previousBalance = 0m;
        public decimal changeAmount = 0m;
        public bool isIncrease = false;
        public bool showAnimation = false;
        
        public BalanceDisplayInfo() { }
        
        public BalanceDisplayInfo(decimal currentBalance, decimal previousBalance)
        {
            this.currentBalance = currentBalance;
            this.previousBalance = previousBalance;
            this.changeAmount = currentBalance - previousBalance;
            this.isIncrease = changeAmount > 0;
            this.showAnimation = changeAmount != 0;
        }
        
        public override string ToString()
        {
            return $"BalanceDisplay[Current:{currentBalance}, Change:{changeAmount:+#;-#;0}]";
        }
    }

    /// <summary>
    /// 倒计时显示信息
    /// </summary>
    [Serializable]
    public class CountdownDisplayInfo
    {
        public float totalTime = 0f;
        public float remainingTime = 0f;
        public string countdownType = "BettingPhase"; // 使用字符串避免枚举重复
        public bool isWarning = false;
        public bool showMilliseconds = false;
        
        public CountdownDisplayInfo() { }
        
        public CountdownDisplayInfo(float totalTime, float remainingTime, string type)
        {
            this.totalTime = totalTime;
            this.remainingTime = remainingTime;
            this.countdownType = type;
            this.isWarning = remainingTime < totalTime * 0.2f; // Warning at 20% remaining
        }
        
        public override string ToString()
        {
            return $"CountdownDisplay[{countdownType}, {remainingTime:F1}s/{totalTime:F1}s, Warning:{isWarning}]";
        }
    }

    /// <summary>
    /// 游戏状态信息
    /// </summary>
    [Serializable]
    public class GameStatusInfo
    {
        public GameState currentState = GameState.Ready; // 使用现有枚举
        public string statusText = "";
        public Color statusColor = Color.white;
        public bool showIcon = false;
        public string iconName = "";
        
        public GameStatusInfo() { }
        
        public GameStatusInfo(GameState state, string statusText)
        {
            this.currentState = state;
            this.statusText = statusText;
        }
        
        public override string ToString()
        {
            return $"GameStatus[{currentState}] {statusText}";
        }
    }

    /// <summary>
    /// 连接状态信息
    /// </summary>
    [Serializable]
    public class ConnectionStatusInfo
    {
        public ConnectionState state = ConnectionState.Disconnected; // 使用现有枚举
        public int ping = 0;
        public float signalStrength = 0f;
        public string statusText = "";
        public Color statusColor = Color.white;
        
        public ConnectionStatusInfo() { }
        
        public ConnectionStatusInfo(ConnectionState state, int ping)
        {
            this.state = state;
            this.ping = ping;
            this.statusText = state.GetDisplayName();
        }
        
        public override string ToString()
        {
            return $"ConnectionStatus[{state}, Ping:{ping}ms]";
        }
    }

    /// <summary>
    /// 回合显示信息
    /// </summary>
    [Serializable]
    public class RoundDisplayInfo
    {
        public int roundNumber = 0;
        public string tableId = "";
        public string dealerName = "";
        public TimeSpan roundDuration = TimeSpan.Zero;
        public RoundState currentState = RoundState.Idle; // 使用现有枚举
        
        public RoundDisplayInfo() { }
        
        public RoundDisplayInfo(int roundNumber, string tableId, RoundState state)
        {
            this.roundNumber = roundNumber;
            this.tableId = tableId;
            this.currentState = state;
        }
        
        public override string ToString()
        {
            return $"RoundDisplay[Round:{roundNumber}, Table:{tableId}, State:{currentState}]";
        }
    }

    #endregion

    #region 卡牌UI相关数据类型（新增）

    /// <summary>
    /// 卡牌动画信息
    /// </summary>
    [Serializable]
    public class CardAnimationInfo
    {
        public string cardId = "";
        public string animationType = "Deal"; // 使用字符串避免枚举重复
        public Vector3 startPosition = Vector3.zero;
        public Vector3 endPosition = Vector3.zero;
        public float duration = 0f;
        public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public bool playSound = true;
        
        public CardAnimationInfo() { }
        
        public CardAnimationInfo(string cardId, string type, Vector3 start, Vector3 end, float duration)
        {
            this.cardId = cardId;
            this.animationType = type;
            this.startPosition = start;
            this.endPosition = end;
            this.duration = duration;
        }
        
        public override string ToString()
        {
            return $"CardAnimation[{cardId}, {animationType}, Duration:{duration}s]";
        }
    }

    /// <summary>
    /// 卡牌翻转信息
    /// </summary>
    [Serializable]
    public class CardFlipInfo
    {
        public string cardId = "";
        public Card card = null; // 使用现有类型
        public bool isRevealing = false;
        public float flipDuration = 0.5f;
        public bool playSound = true;
        
        public CardFlipInfo() { }
        
        public CardFlipInfo(string cardId, Card card, bool isRevealing)
        {
            this.cardId = cardId;
            this.card = card;
            this.isRevealing = isRevealing;
        }
        
        public override string ToString()
        {
            return $"CardFlip[{cardId}, Card:{card}, Revealing:{isRevealing}]";
        }
    }

    /// <summary>
    /// 手牌总点数显示信息
    /// </summary>
    [Serializable]
    public class HandTotalDisplayInfo
    {
        public HandType handType = HandType.Player; // 使用现有枚举
        public int total = 0;
        public int previousTotal = 0;
        public bool isNatural = false;
        public bool showAnimation = false;
        public Color textColor = Color.white;
        
        public HandTotalDisplayInfo() { }
        
        public HandTotalDisplayInfo(HandType handType, int total, bool isNatural)
        {
            this.handType = handType;
            this.total = total;
            this.isNatural = isNatural;
        }
        
        public override string ToString()
        {
            return $"HandTotalDisplay[{handType}, Total:{total}, Natural:{isNatural}]";
        }
    }

    /// <summary>
    /// 卡牌发光信息
    /// </summary>
    [Serializable]
    public class CardGlowInfo
    {
        public string cardId = "";
        public Color glowColor = Color.yellow;
        public float intensity = 1f;
        public float duration = 1f;
        public bool isPulsing = false;
        
        public CardGlowInfo() { }
        
        public CardGlowInfo(string cardId, Color glowColor, float intensity, float duration)
        {
            this.cardId = cardId;
            this.glowColor = glowColor;
            this.intensity = intensity;
            this.duration = duration;
        }
        
        public override string ToString()
        {
            return $"CardGlow[{cardId}, Color:{glowColor}, Intensity:{intensity}]";
        }
    }

    #endregion

    #region 动画和特效相关数据类型（新增）

    /// <summary>
    /// 筹码动画信息
    /// </summary>
    [Serializable]
    public class ChipAnimationInfo
    {
        public string chipId = "";
        public string animationType = "Place"; // 使用字符串避免枚举重复
        public Vector3 startPosition = Vector3.zero;
        public Vector3 endPosition = Vector3.zero;
        public float duration = 0f;
        public decimal value = 0m;
        public bool playSound = true;
        
        public ChipAnimationInfo() { }
        
        public ChipAnimationInfo(string chipId, string type, Vector3 start, Vector3 end, decimal value)
        {
            this.chipId = chipId;
            this.animationType = type;
            this.startPosition = start;
            this.endPosition = end;
            this.value = value;
        }
        
        public override string ToString()
        {
            return $"ChipAnimation[{chipId}, {animationType}, Value:{value}]";
        }
    }

    /// <summary>
    /// 获胜特效信息
    /// </summary>
    [Serializable]
    public class WinEffectInfo
    {
        public string winType = "Small"; // 使用字符串避免枚举重复
        public decimal winAmount = 0m;
        public Vector3 position = Vector3.zero;
        public float duration = 2f;
        public bool playSound = true;
        public Color effectColor = Color.yellow;
        
        public WinEffectInfo() { }
        
        public WinEffectInfo(string winType, decimal winAmount, Vector3 position)
        {
            this.winType = winType;
            this.winAmount = winAmount;
            this.position = position;
        }
        
        public override string ToString()
        {
            return $"WinEffect[{winType}, Amount:{winAmount}, Duration:{duration}s]";
        }
    }

    /// <summary>
    /// 闪光特效信息
    /// </summary>
    [Serializable]
    public class FlashEffectInfo
    {
        public string targetId = "";
        public Color flashColor = Color.white;
        public float intensity = 1f;
        public float duration = 0.5f;
        public int flashCount = 3;
        
        public FlashEffectInfo() { }
        
        public FlashEffectInfo(string targetId, Color flashColor, float duration, int flashCount)
        {
            this.targetId = targetId;
            this.flashColor = flashColor;
            this.duration = duration;
            this.flashCount = flashCount;
        }
        
        public override string ToString()
        {
            return $"FlashEffect[{targetId}, Color:{flashColor}, Count:{flashCount}]";
        }
    }

    /// <summary>
    /// 粒子特效信息
    /// </summary>
    [Serializable]
    public class ParticleEffectInfo
    {
        public string effectType = "Celebration"; // 使用字符串避免枚举重复
        public Vector3 position = Vector3.zero;
        public Color color = Color.white;
        public float intensity = 1f;
        public float duration = 2f;
        public int particleCount = 50;
        
        public ParticleEffectInfo() { }
        
        public ParticleEffectInfo(string effectType, Vector3 position, Color color)
        {
            this.effectType = effectType;
            this.position = position;
            this.color = color;
        }
        
        public override string ToString()
        {
            return $"ParticleEffect[{effectType}, Count:{particleCount}, Duration:{duration}s]";
        }
    }

    /// <summary>
    /// 倒计时动画信息
    /// </summary>
    [Serializable]
    public class CountdownAnimationInfo
    {
        public float remainingTime = 0f;
        public bool isWarning = false;
        public bool isCritical = false;
        public string animationType = "Scale"; // 使用字符串避免枚举重复
        public Color textColor = Color.white;
        public float scale = 1f;
        
        public CountdownAnimationInfo() { }
        
        public CountdownAnimationInfo(float remainingTime, bool isWarning, string type)
        {
            this.remainingTime = remainingTime;
            this.isWarning = isWarning;
            this.animationType = type;
            this.isCritical = remainingTime < 5f;
        }
        
        public override string ToString()
        {
            return $"CountdownAnimation[{animationType}, Time:{remainingTime:F1}s, Critical:{isCritical}]";
        }
    }

    #endregion

    #region 交互相关数据类型（新增）

    /// <summary>
    /// 按钮hover信息
    /// </summary>
    [Serializable]
    public class ButtonHoverInfo
    {
        public string buttonId = "";
        public bool isHovering = false;
        public Vector2 mousePosition = Vector2.zero;
        public float hoverDuration = 0f;
        
        public ButtonHoverInfo() { }
        
        public ButtonHoverInfo(string buttonId, bool isHovering, Vector2 mousePosition)
        {
            this.buttonId = buttonId;
            this.isHovering = isHovering;
            this.mousePosition = mousePosition;
        }
        
        public override string ToString()
        {
            return $"ButtonHover[{buttonId}, Hovering:{isHovering}, Duration:{hoverDuration:F1}s]";
        }
    }

    /// <summary>
    /// 按钮点击信息
    /// </summary>
    [Serializable]
    public class ButtonClickInfo
    {
        public string buttonId = "";
        public string buttonType = "Action"; // 使用字符串避免枚举重复
        public Vector2 clickPosition = Vector2.zero;
        public DateTime timestamp = DateTime.Now;
        public bool isDoubleClick = false;
        
        public ButtonClickInfo() { }
        
        public ButtonClickInfo(string buttonId, string buttonType, Vector2 clickPosition)
        {
            this.buttonId = buttonId;
            this.buttonType = buttonType;
            this.clickPosition = clickPosition;
            this.timestamp = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"ButtonClick[{buttonId}, Type:{buttonType}, DoubleClick:{isDoubleClick}]";
        }
    }

    /// <summary>
    /// 滑动信息
    /// </summary>
    [Serializable]
    public class SwipeInfo
    {
        public Vector2 startPosition = Vector2.zero;
        public Vector2 endPosition = Vector2.zero;
        public Vector2 direction = Vector2.zero;
        public float distance = 0f;
        public float duration = 0f;
        public string swipeDirection = "None"; // 使用字符串避免枚举重复
        
        public SwipeInfo() { }
        
        public SwipeInfo(Vector2 startPos, Vector2 endPos, float duration)
        {
            this.startPosition = startPos;
            this.endPosition = endPos;
            this.duration = duration;
            this.direction = (endPos - startPos).normalized;
            this.distance = Vector2.Distance(startPos, endPos);
            
            // Determine swipe direction
            Vector2 delta = endPos - startPos;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                swipeDirection = delta.x > 0 ? "Right" : "Left";
            }
            else
            {
                swipeDirection = delta.y > 0 ? "Up" : "Down";
            }
        }
        
        public override string ToString()
        {
            return $"Swipe[{swipeDirection}, Distance:{distance:F1}, Duration:{duration:F1}s]";
        }
    }

    /// <summary>
    /// 拖拽信息
    /// </summary>
    [Serializable]
    public class DragInfo
    {
        public string draggedObjectId = "";
        public Vector2 startPosition = Vector2.zero;
        public Vector2 currentPosition = Vector2.zero;
        public Vector2 deltaPosition = Vector2.zero;
        public bool isDragging = false;
        
        public DragInfo() { }
        
        public DragInfo(string objectId, Vector2 startPos, Vector2 currentPos, bool isDragging)
        {
            this.draggedObjectId = objectId;
            this.startPosition = startPos;
            this.currentPosition = currentPos;
            this.deltaPosition = currentPos - startPos;
            this.isDragging = isDragging;
        }
        
        public override string ToString()
        {
            return $"Drag[{draggedObjectId}, Delta:{deltaPosition}, Dragging:{isDragging}]";
        }
    }

    /// <summary>
    /// 双击信息
    /// </summary>
    [Serializable]
    public class DoubleClickInfo
    {
        public Vector2 position = Vector2.zero;
        public float timeBetweenClicks = 0f;
        public string targetId = "";
        
        public DoubleClickInfo() { }
        
        public DoubleClickInfo(Vector2 position, float timeBetween, string targetId)
        {
            this.position = position;
            this.timeBetweenClicks = timeBetween;
            this.targetId = targetId;
        }
        
        public override string ToString()
        {
            return $"DoubleClick[{targetId}, Interval:{timeBetweenClicks:F2}s]";
        }
    }

    /// <summary>
    /// 长按信息
    /// </summary>
    [Serializable]
    public class LongPressInfo
    {
        public Vector2 position = Vector2.zero;
        public float pressDuration = 0f;
        public string targetId = "";
        public bool isComplete = false;
        
        public LongPressInfo() { }
        
        public LongPressInfo(Vector2 position, float duration, string targetId, bool isComplete)
        {
            this.position = position;
            this.pressDuration = duration;
            this.targetId = targetId;
            this.isComplete = isComplete;
        }
        
        public override string ToString()
        {
            return $"LongPress[{targetId}, Duration:{pressDuration:F1}s, Complete:{isComplete}]";
        }
    }

    #endregion

    #region 音效和反馈相关数据类型（新增）

    /// <summary>
    /// UI音效信息
    /// </summary>
    [Serializable]
    public class UISoundInfo
    {
        public string soundType = "ButtonClick"; // 使用字符串避免枚举重复
        public string soundId = "";
        public float volume = 1f;
        public float pitch = 1f;
        public bool loop = false;
        
        public UISoundInfo() { }
        
        public UISoundInfo(string soundType, float volume = 1f, float pitch = 1f)
        {
            this.soundType = soundType;
            this.volume = volume;
            this.pitch = pitch;
        }
        
        public override string ToString()
        {
            return $"UISound[{soundType}, Volume:{volume:F1}, Pitch:{pitch:F1}]";
        }
    }

    /// <summary>
    /// 触觉反馈信息
    /// </summary>
    [Serializable]
    public class HapticFeedbackInfo
    {
        public string hapticType = "Light"; // 使用字符串避免枚举重复
        public float intensity = 1f;
        public float duration = 0.1f;
        
        public HapticFeedbackInfo() { }
        
        public HapticFeedbackInfo(string hapticType, float intensity = 1f, float duration = 0.1f)
        {
            this.hapticType = hapticType;
            this.intensity = intensity;
            this.duration = duration;
        }
        
        public override string ToString()
        {
            return $"HapticFeedback[{hapticType}, Intensity:{intensity:F1}, Duration:{duration:F1}s]";
        }
    }

    /// <summary>
    /// Toast信息
    /// </summary>
    [Serializable]
    public class ToastInfo
    {
        public string message = "";
        public string toastType = "Info"; // 使用字符串避免枚举重复
        public float duration = 3f;
        public Vector2 position = Vector2.zero;
        public Color backgroundColor = Color.black;
        public Color textColor = Color.white;
        
        public ToastInfo() { }
        
        public ToastInfo(string message, string toastType = "Info", float duration = 3f)
        {
            this.message = message;
            this.toastType = toastType;
            this.duration = duration;
        }
        
        public override string ToString()
        {
            return $"Toast[{toastType}] {message}";
        }
    }

    /// <summary>
    /// 通知信息
    /// </summary>
    [Serializable]
    public class NotificationInfo
    {
        public string title = "";
        public string message = "";
        public string notificationType = "Info"; // 使用字符串避免枚举重复
        public float duration = 5f;
        public bool isAutoDismiss = true;
        public System.Action onClicked;
        
        public NotificationInfo() { }
        
        public NotificationInfo(string title, string message, string type = "Info")
        {
            this.title = title;
            this.message = message;
            this.notificationType = type;
        }
        
        public override string ToString()
        {
            return $"Notification[{notificationType}] {title}: {message}";
        }
    }

    #endregion

    #region 布局和适配相关数据类型（新增）

    /// <summary>
    /// 屏幕方向信息
    /// </summary>
    [Serializable]
    public class ScreenOrientationInfo
    {
        public ScreenOrientation oldOrientation = ScreenOrientation.Portrait;
        public ScreenOrientation newOrientation = ScreenOrientation.Portrait;
        public float transitionDuration = 0.5f;
        
        public ScreenOrientationInfo() { }
        
        public ScreenOrientationInfo(ScreenOrientation oldOrientation, ScreenOrientation newOrientation)
        {
            this.oldOrientation = oldOrientation;
            this.newOrientation = newOrientation;
        }
        
        public override string ToString()
        {
            return $"ScreenOrientation[{oldOrientation} -> {newOrientation}]";
        }
    }

    /// <summary>
    /// 分辨率信息
    /// </summary>
    [Serializable]
    public class ResolutionInfo
    {
        public Vector2Int oldResolution = Vector2Int.zero;
        public Vector2Int newResolution = Vector2Int.zero;
        public float aspectRatio = 0f;
        public bool isFullscreen = false;
        
        public ResolutionInfo() { }
        
        public ResolutionInfo(Vector2Int oldRes, Vector2Int newRes, bool isFullscreen)
        {
            this.oldResolution = oldRes;
            this.newResolution = newRes;
            this.isFullscreen = isFullscreen;
            this.aspectRatio = newRes.y != 0 ? (float)newRes.x / newRes.y : 0f;
        }
        
        public override string ToString()
        {
            return $"Resolution[{oldResolution} -> {newResolution}, Ratio:{aspectRatio:F2}, Fullscreen:{isFullscreen}]";
        }
    }

    /// <summary>
    /// 安全区域信息
    /// </summary>
    [Serializable]
    public class SafeAreaInfo
    {
        public Rect safeArea = Rect.zero;
        public Vector4 padding = Vector4.zero;
        public bool hasNotch = false;
        public bool hasHomeIndicator = false;
        
        public SafeAreaInfo() { }
        
        public SafeAreaInfo(Rect safeArea, bool hasNotch, bool hasHomeIndicator)
        {
            this.safeArea = safeArea;
            this.hasNotch = hasNotch;
            this.hasHomeIndicator = hasHomeIndicator;
        }
        
        public override string ToString()
        {
            return $"SafeArea[{safeArea}, Notch:{hasNotch}, HomeIndicator:{hasHomeIndicator}]";
        }
    }

    /// <summary>
    /// UI缩放信息
    /// </summary>
    [Serializable]
    public class UIScaleInfo
    {
        public float oldScale = 1f;
        public float newScale = 1f;
        public Vector2 referenceResolution = new Vector2(1920, 1080);
        public CanvasScaler.ScreenMatchMode matchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        
        public UIScaleInfo() { }
        
        public UIScaleInfo(float oldScale, float newScale, Vector2 referenceResolution)
        {
            this.oldScale = oldScale;
            this.newScale = newScale;
            this.referenceResolution = referenceResolution;
        }
        
        public override string ToString()
        {
            return $"UIScale[{oldScale:F2} -> {newScale:F2}, Ref:{referenceResolution}]";
        }
    }

    #endregion

    #region UI专用枚举（新增，仅在UIEvents中使用的）

    /// <summary>
    /// UI面板类型
    /// </summary>
    public enum UIPanel
    {
        MainMenu,   // 主菜单
        GameTable,  // 游戏桌台
        Settings,   // 设置
        BetHistory, // 投注历史
        Statistics, // 统计
        Profile,    // 个人资料
        Help,       // 帮助
        Loading     // 加载界面
    }

    #endregion
}
// Assets/Core/Data/Types/BaccaratTypes.cs
// 百家乐核心数据类型定义
// 只包含核心业务数据结构，枚举统一放到GameEnums.cs

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BaccaratGame.Data
{
    #region 台桌信息

    /// <summary>
    /// 台桌信息 - 对应数据库 ntp_dianji_table
    /// </summary>
    [Serializable]
    public class TableInfo
    {
        [Header("基本信息")]
        public int id = 0;                          // 台桌ID
        public string table_title = "";            // 台桌名字
        public string table_describe = "";         // 台桌描述
        public int game_type = 3;                  // 游戏类型：3=百家乐
        public int list_order = 0;                 // 显示排序
        public string bureau_number = "";           // 局号信息

        [Header("荷官信息")]
        public string he_guan_name = "";           // 荷官名称
        public string he_guan_head_img = "";       // 荷官头像
        public string he_guan_describe = "";       // 荷官描述

        [Header("视频地址")]
        public string video_near = "";             // 近景视频地址
        public string video_far = "";              // 远景视频地址

        [Header("游戏状态")]
        public int status = 1;                     // 台桌状态
        public int run_status = 1;                 // 运行状态：1投注 2开牌 3洗牌等
        public int game_play_staus = 0;            // 游戏进行状态
        public int countdown_time = 0;             // 倒计时
        public int start_time = 0;                 // 当前局倒计时开始时间戳
        public int wash_status = 0;                // 洗牌状态：1在洗牌

        [Header("投注限红")]
        public int bjl_xian_hong_xian_min = 10;    // 闲最小投注
        public int bjl_xian_hong_xian_max = 10000; // 闲最大投注
        public int bjl_xian_hong_zhuang_min = 10;  // 庄最小投注
        public int bjl_xian_hong_zhuang_max = 10000; // 庄最大投注
        public int bjl_xian_hong_he_min = 10;      // 和最小投注
        public int bjl_xian_hong_he_max = 10000;   // 和最大投注
        public int bjl_xian_hong_xian_dui_min = 10; // 闲对最小投注
        public int bjl_xian_hong_xian_dui_max = 10000; // 闲对最大投注
        public int bjl_xian_hong_zhuang_dui_min = 10; // 庄对最小投注
        public int bjl_xian_hong_zhuang_dui_max = 10000; // 庄对最大投注

        [Header("特殊投注限红")]
        public int bjl_xian_hong_lucky6_min = 10;  // 幸运6最小投注
        public int bjl_xian_hong_lucky6_max = 10000; // 幸运6最大投注
        public int bjl_xian_hong_long7_min = 10;   // 龙7最小投注
        public int bjl_xian_hong_long7_max = 10000; // 龙7最大投注
        public int bjl_xian_hong_xiong8_min = 10;  // 熊8最小投注
        public int bjl_xian_hong_xiong8_max = 10000; // 熊8最大投注

        [Header("其他配置")]
        public int is_table_xian_hong = 1;         // 是否开启台桌限红：0不是 1是
        public string lu_zhu_name = "";            // 露珠台桌名称
        public string remark = "";                 // 备注信息

        public TableInfo() { }

        public TableInfo(int tableId, string tableName)
        {
            id = tableId;
            table_title = tableName;
        }

        /// <summary>
        /// 检查是否可以投注
        /// </summary>
        public bool CanPlaceBet()
        {
            return status == 1 && run_status == 1 && countdown_time > 0;
        }

        /// <summary>
        /// 检查投注金额是否在限额内
        /// </summary>
        public bool IsValidBetAmount(int betType, decimal amount)
        {
            // 根据投注类型检查限额
            var (min, max) = GetBetLimits(betType);
            return amount >= min && amount <= max;
        }

        /// <summary>
        /// 获取指定投注类型的限额
        /// </summary>
        public (int min, int max) GetBetLimits(int betType)
        {
            return betType switch
            {
                2 => (bjl_xian_hong_xian_min, bjl_xian_hong_xian_max),     // 闲家
                6 => (bjl_xian_hong_zhuang_min, bjl_xian_hong_zhuang_max), // 庄家
                7 => (bjl_xian_hong_he_min, bjl_xian_hong_he_max),         // 和局
                4 => (bjl_xian_hong_xian_dui_min, bjl_xian_hong_xian_dui_max), // 闲对
                8 => (bjl_xian_hong_zhuang_dui_min, bjl_xian_hong_zhuang_dui_max), // 庄对
                3 => (bjl_xian_hong_lucky6_min, bjl_xian_hong_lucky6_max), // 幸运6
                9 => (bjl_xian_hong_long7_min, bjl_xian_hong_long7_max),   // 龙7
                10 => (bjl_xian_hong_xiong8_min, bjl_xian_hong_xiong8_max), // 熊8
                _ => (10, 10000) // 默认限额
            };
        }

        public override string ToString()
        {
            return $"Table[{id}:{table_title}, Status:{status}, RunStatus:{run_status}, Countdown:{countdown_time}]";
        }
    }

    #endregion

    #region 用户信息

    /// <summary>
    /// 用户信息 - 对应数据库 ntp_common_user
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        [Header("基本信息")]
        public int id = 0;                         // 用户ID
        public string user_name = "";              // 账号
        public string nickname = "";               // 昵称
        public int type = 2;                       // 账号类型：1代理 2会员
        public int vip_grade = 0;                  // 会员等级

        [Header("账户状态")]
        public int status = 1;                     // 账号状态：1正常 0冻结
        public int state = 0;                      // 是否在线：1在线 0下线
        public int is_real_name = 0;               // 是否实名：1已实名 0未实名
        public int is_fictitious = 0;              // 是否虚拟账号：1是 0否 2试玩帐号

        [Header("资金信息")]
        public decimal money_balance = 0m;         // 可用余额
        public decimal money_freeze = 0m;          // 冻结金额
        public decimal money_total_recharge = 0m;  // 累积充值
        public decimal money_total_withdraw = 0m;  // 累计提现
        public decimal rebate_balance = 0m;        // 洗码费余额
        public decimal rebate_total = 0m;          // 累计洗码费

        [Header("代理信息")]
        public int agent_id = 0;                   // 上级代理ID
        public int agent_id_1 = 0;                 // 上级代理（三级分销）
        public int agent_id_2 = 0;                 // 上上级代理（三级分销）
        public int agent_id_3 = 0;                 // 上上上级代理（三级分销）
        public decimal agent_rate = 0m;            // 分销比例（%）

        [Header("其他信息")]
        public string phone = "";                  // 手机号
        public int points = 0;                     // 积分
        public string remarks = "";                // 用户备注
        public string invitation_code = "";        // 邀请码
        public DateTime create_time = DateTime.Now; // 创建时间

        public UserInfo() { }

        public UserInfo(int userId, string userName, decimal balance)
        {
            id = userId;
            user_name = userName;
            money_balance = balance;
        }

        /// <summary>
        /// 检查是否可以投注指定金额
        /// </summary>
        public bool CanBet(decimal amount)
        {
            return money_balance >= amount && amount > 0 && status == 1 && !IsVirtual();
        }

        /// <summary>
        /// 是否为虚拟/试玩账号
        /// </summary>
        public bool IsVirtual()
        {
            return is_fictitious == 1 || is_fictitious == 2;
        }

        /// <summary>
        /// 更新余额
        /// </summary>
        public void UpdateBalance(decimal newBalance)
        {
            money_balance = Math.Max(0m, newBalance);
        }

        /// <summary>
        /// 扣除投注金额
        /// </summary>
        public bool DeductBet(decimal amount)
        {
            if (CanBet(amount))
            {
                money_balance -= amount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 添加奖金
        /// </summary>
        public void AddWinnings(decimal amount)
        {
            money_balance += Math.Max(0m, amount);
        }

        public override string ToString()
        {
            return $"User[{id}:{user_name}, Balance:{money_balance}, Status:{status}, Online:{state == 1}]";
        }
    }

    #endregion

    #region 历史投注信息

    /// <summary>
    /// 用户历史投注信息 - 对应数据库 ntp_dianji_records
    /// </summary>
    [Serializable]
    public class HistoryBetInfo
    {
        [Header("基本信息")]
        public long id = 0;                        // 注单号
        public long user_id = 0;                   // 用户ID
        public long lu_zhu_id = 0;                 // 游戏记录ID
        public string table_id = "";               // 牌桌NUM
        public int game_type = 3;                  // 游戏类型

        [Header("投注信息")]
        public decimal bet_amt = 0m;               // 下注金额
        public decimal before_amt = 0m;            // 下注前金额
        public decimal end_amt = 0m;               // 下注后金额
        public int game_peilv_id = 0;              // 投注类型ID
        public string game_peilv = "";             // 当前玩的时候赔率

        [Header("结算信息")]
        public decimal win_amt = 0m;               // 会员总赢
        public decimal delta_amt = 0m;             // 变化金额
        public string result = "";                 // 游戏结果
        public int close_status = 1;               // 结束状态：1待开牌 2已结算 3台面作废 4修改结果

        [Header("洗码信息")]
        public long shuffling_num = 0;             // 洗码量
        public decimal shuffling_rate = 0m;        // 洗码率
        public decimal shuffling_amt = 0m;         // 洗码费
        public int agent_status = 0;               // 代理计算状态：1洗码结算 9全部结算

        [Header("游戏详情")]
        public string detail = "";                 // 投注明细
        public string xue_number = "";             // 靴号
        public string pu_number = "";              // 铺号

        [Header("其他信息")]
        public int is_exempt = 0;                  // 是否免佣：0不是 1是
        public decimal deposit_amt = 0m;           // 牛牛押金
        public DateTime created_at = DateTime.Now; // 创建时间
        public DateTime updated_at = DateTime.Now; // 更新时间

        public HistoryBetInfo() { }

        public override string ToString()
        {
            return $"HistoryBet[{id}, User:{user_id}, Amount:{bet_amt}, Win:{win_amt}, Status:{close_status}]";
        }
    }

    #endregion

    #region 投注订单信息

    /// <summary>
    /// 投注订单信息 - 当前局投注（提交前的临时订单）
    /// </summary>
    [Serializable]
    public class OrderInfo
    {
        [Header("订单基本信息")]
        public string order_id = "";               // 临时订单ID
        public long user_id = 0;                   // 用户ID
        public string table_id = "";               // 台桌ID
        public int game_type = 3;                  // 游戏类型

        [Header("投注信息")]
        public int game_peilv_id = 0;              // 投注类型ID
        public decimal bet_amt = 0m;               // 投注金额
        public string game_peilv = "";             // 当前赔率
        public DateTime create_time = DateTime.Now; // 下注时间

        [Header("订单状态")]
        public int order_status = 0;               // 订单状态：0待提交 1已提交 2已取消
        public bool is_confirmed = false;          // 是否已确认

        [Header("游戏信息")]
        public string xue_number = "";             // 靴号
        public string pu_number = "";              // 铺号
        public long lu_zhu_id = 0;                 // 游戏记录ID（提交后获得）

        [Header("金额信息")]
        public decimal before_amt = 0m;            // 下注前金额
        public decimal after_amt = 0m;             // 预计下注后金额

        [Header("其他信息")]
        public string client_ip = "";              // 客户端IP
        public string user_agent = "";             // 用户代理
        public string remark = "";                 // 备注

        public OrderInfo()
        {
            order_id = GenerateOrderId();
        }

        public OrderInfo(long userId, string tableId, int betTypeId, decimal amount)
        {
            order_id = GenerateOrderId();
            user_id = userId;
            table_id = tableId;
            game_peilv_id = betTypeId;
            bet_amt = amount;
        }

        /// <summary>
        /// 生成订单ID
        /// </summary>
        private string GenerateOrderId()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var random = UnityEngine.Random.Range(1000, 9999);
            return $"BO{timestamp}{random}";
        }

        public override string ToString()
        {
            return $"Order[{order_id}, User:{user_id}, Amount:{bet_amt}, Status:{order_status}]";
        }
    }

    #endregion
}
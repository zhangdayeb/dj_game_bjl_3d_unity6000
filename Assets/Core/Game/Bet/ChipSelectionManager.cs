// Assets/Scripts/Core/ChipSelectionManager.cs
// 筹码选择管理器 - 处理筹码选中效果和数据管理
// 挂载到 ChipSelectionArea 节点上
// 创建时间: 2025/6/29

using UnityEngine;
using UnityEngine.UI;
using BaccaratGame.Managers;

namespace BaccaratGame.Core
{
    /// <summary>
    /// 筹码选择管理器 - 纯UI控制器
    /// 负责处理筹码按钮的选中效果，数据管理交给BetDataManager
    /// </summary>
    public class ChipSelectionManager : MonoBehaviour
    {
        #region Inspector 配置

        [Header("筹码按钮引用")]
        [SerializeField] private Button[] chipButtons = new Button[5];
        
        [Header("选中效果配置")]
        [SerializeField] private Color selectedBorderColor = new Color(0f, 1f, 0.6f, 1f); // 绿色边框
        [SerializeField] private float selectedScale = 1.1f; // 选中时的缩放比例
        [SerializeField] private float borderWidth = 3f; // 边框宽度

        #endregion

        #region 私有字段

        private Outline[] chipOutlines; // 存储每个筹码的Outline组件

        #endregion

        #region 公共属性

        /// <summary>
        /// 获取当前选中的筹码值（从BetDataManager读取）
        /// </summary>
        public int CurrentSelectedChipValue => BetDataManager.Instance.CurrentSelectedChip;

        /// <summary>
        /// 获取当前选中的筹码索引（从BetDataManager读取）
        /// </summary>
        public int CurrentSelectedIndex => BetDataManager.Instance.GetSelectedChipIndex();

        /// <summary>
        /// 获取可用筹码数组（从BetDataManager读取）
        /// </summary>
        public int[] AvailableChips => BetDataManager.Instance.AvailableChips;

        #endregion

        #region 生命周期

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            SetupChipButtons();
            // 从BetDataManager获取默认选中的筹码索引并应用UI效果
            int defaultIndex = BetDataManager.Instance.GetSelectedChipIndex();
            ApplyChipSelectionUI(defaultIndex);
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponents()
        {
            // 验证配置
            if (chipButtons == null || chipButtons.Length == 0)
            {
                Debug.LogError("[ChipSelectionManager] 未配置筹码按钮引用！");
                return;
            }

            var availableChips = BetDataManager.Instance.AvailableChips;
            if (availableChips.Length != chipButtons.Length)
            {
                Debug.LogError($"[ChipSelectionManager] 筹码按钮数量({chipButtons.Length})与BetDataManager中筹码数量({availableChips.Length})不匹配！");
                return;
            }

            // 初始化Outline数组
            chipOutlines = new Outline[chipButtons.Length];

            Debug.Log($"[ChipSelectionManager] 初始化完成，共{chipButtons.Length}个筹码按钮");
        }

        /// <summary>
        /// 设置筹码按钮事件
        /// </summary>
        private void SetupChipButtons()
        {
            var availableChips = BetDataManager.Instance.AvailableChips;
            
            for (int i = 0; i < chipButtons.Length; i++)
            {
                if (chipButtons[i] == null)
                {
                    Debug.LogWarning($"[ChipSelectionManager] 筹码按钮{i}为空，跳过设置");
                    continue;
                }

                // 保存索引用于点击事件
                int index = i;
                chipButtons[i].onClick.RemoveAllListeners(); // 清除可能存在的监听器
                chipButtons[i].onClick.AddListener(() => OnChipButtonClicked(index));

                Debug.Log($"[ChipSelectionManager] 设置筹码按钮{i}点击事件，对应值：{availableChips[i]}");
            }
        }

        #endregion

        #region 筹码选择逻辑

        /// <summary>
        /// 筹码按钮点击事件
        /// </summary>
        /// <param name="chipIndex">点击的筹码索引</param>
        private void OnChipButtonClicked(int chipIndex)
        {
            if (chipIndex < 0 || chipIndex >= chipButtons.Length)
            {
                Debug.LogWarning($"[ChipSelectionManager] 无效的筹码索引：{chipIndex}");
                return;
            }

            // 更新BetDataManager中的数据
            bool success = BetDataManager.Instance.SetSelectedChipByIndex(chipIndex);
            if (success)
            {
                // 更新UI效果
                SelectChipByIndex(chipIndex);
                
                var availableChips = BetDataManager.Instance.AvailableChips;
                Debug.Log($"[ChipSelectionManager] 玩家选择筹码{chipIndex}，值：{availableChips[chipIndex]}");
            }
        }

        /// <summary>
        /// 按索引选择筹码（纯UI效果，不修改数据）
        /// </summary>
        /// <param name="chipIndex">要选择的筹码索引</param>
        public void SelectChipByIndex(int chipIndex)
        {
            if (chipIndex < 0 || chipIndex >= chipButtons.Length)
            {
                Debug.LogWarning($"[ChipSelectionManager] 选择筹码失败，无效索引：{chipIndex}");
                return;
            }

            // 获取当前选中的索引（从BetDataManager）
            int currentIndex = BetDataManager.Instance.GetSelectedChipIndex();
            
            // 如果已经选中同一个筹码，不需要重复操作
            if (currentIndex == chipIndex)
            {
                Debug.Log($"[ChipSelectionManager] 筹码{chipIndex}已经被选中");
                return;
            }

            // 应用UI选中效果
            ApplyChipSelectionUI(chipIndex);

            Debug.Log($"[ChipSelectionManager] UI选中筹码{chipIndex}");
        }

        /// <summary>
        /// 应用筹码选中的UI效果
        /// </summary>
        /// <param name="chipIndex">要选择的筹码索引</param>
        private void ApplyChipSelectionUI(int chipIndex)
        {
            // 清除所有筹码的选中效果
            for (int i = 0; i < chipButtons.Length; i++)
            {
                ClearChipSelection(i);
            }

            // 应用新选中筹码的效果
            ApplyChipSelection(chipIndex);

            // 更新显示
            UpdateChipDisplay();
        }

        /// <summary>
        /// 按筹码值选择筹码
        /// </summary>
        /// <param name="chipValue">要选择的筹码值</param>
        public void SelectChipByValue(int chipValue)
        {
            // 先更新BetDataManager中的数据
            bool success = BetDataManager.Instance.SetSelectedChipByValue(chipValue);
            if (success)
            {
                // 获取对应的索引并更新UI
                int chipIndex = BetDataManager.Instance.GetSelectedChipIndex();
                ApplyChipSelectionUI(chipIndex);
                Debug.Log($"[ChipSelectionManager] 按值选择筹码：{chipValue}");
            }
            else
            {
                Debug.LogWarning($"[ChipSelectionManager] 未找到值为{chipValue}的筹码");
            }
        }

        #endregion

        #region 视觉效果

        /// <summary>
        /// 应用筹码选中效果
        /// </summary>
        /// <param name="chipIndex">筹码索引</param>
        private void ApplyChipSelection(int chipIndex)
        {
            if (chipIndex < 0 || chipIndex >= chipButtons.Length || chipButtons[chipIndex] == null)
                return;

            GameObject chipObject = chipButtons[chipIndex].gameObject;

            // 添加边框效果
            if (chipOutlines[chipIndex] == null)
            {
                chipOutlines[chipIndex] = chipObject.AddComponent<Outline>();
            }

            Outline outline = chipOutlines[chipIndex];
            outline.effectColor = selectedBorderColor;
            outline.effectDistance = new Vector2(borderWidth, -borderWidth);
            outline.enabled = true;

            // 添加缩放效果
            chipObject.transform.localScale = Vector3.one * selectedScale;

            Debug.Log($"[ChipSelectionManager] 应用选中效果到筹码{chipIndex}");
        }

        /// <summary>
        /// 清除筹码选中效果
        /// </summary>
        /// <param name="chipIndex">筹码索引</param>
        private void ClearChipSelection(int chipIndex)
        {
            if (chipIndex < 0 || chipIndex >= chipButtons.Length || chipButtons[chipIndex] == null)
                return;

            GameObject chipObject = chipButtons[chipIndex].gameObject;

            // 移除边框效果
            if (chipOutlines[chipIndex] != null)
            {
                chipOutlines[chipIndex].enabled = false;
            }

            // 恢复原始缩放
            chipObject.transform.localScale = Vector3.one;

            Debug.Log($"[ChipSelectionManager] 清除筹码{chipIndex}的选中效果");
        }

        /// <summary>
        /// 更新筹码值显示
        /// </summary>
        private void UpdateChipDisplay()
        {
            Debug.Log($"[ChipSelectionManager] 当前选中筹码值：{CurrentSelectedChipValue}");
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 清除所有选择
        /// </summary>
        public void ClearAllSelection()
        {
            // 清除所有UI效果
            for (int i = 0; i < chipButtons.Length; i++)
            {
                ClearChipSelection(i);
            }
            
            Debug.Log("[ChipSelectionManager] 清除所有筹码选择UI效果");
        }

        /// <summary>
        /// 重置为默认选择（第一个筹码）
        /// </summary>
        public void ResetToDefault()
        {
            // 重置BetDataManager中的数据
            BetDataManager.Instance.ResetToDefault();
            
            // 应用UI效果
            ApplyChipSelectionUI(0);
            
            Debug.Log("[ChipSelectionManager] 重置为默认选择");
        }

        /// <summary>
        /// 获取所有筹码值
        /// </summary>
        /// <returns>筹码值数组</returns>
        public int[] GetAllChipValues()
        {
            return (int[])BetDataManager.Instance.AvailableChips.Clone();
        }

        #endregion

        #region 编辑器辅助

        /// <summary>
        /// 验证配置（编辑器模式下使用）
        /// </summary>
        [ContextMenu("验证配置")]
        public void ValidateConfiguration()
        {
            Debug.Log("=== ChipSelectionManager 配置验证 ===");
            
            Debug.Log($"筹码按钮数量: {(chipButtons != null ? chipButtons.Length : 0)}");
            
            var availableChips = BetDataManager.Instance.AvailableChips;
            Debug.Log($"BetDataManager筹码数量: {availableChips.Length}");
            Debug.Log($"BetDataManager筹码数值: [{string.Join(", ", availableChips)}]");
            Debug.Log($"当前选中筹码: {BetDataManager.Instance.CurrentSelectedChip}");
            
            if (chipButtons != null)
            {
                for (int i = 0; i < chipButtons.Length; i++)
                {
                    Debug.Log($"筹码按钮{i}: {(chipButtons[i] != null ? chipButtons[i].name : "未配置")}");
                }
            }
        }

        /// <summary>
        /// 测试选择筹码（编辑器模式下使用）
        /// </summary>
        [ContextMenu("测试选择筹码")]
        public void TestChipSelection()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("请在运行时测试筹码选择功能");
                return;
            }

            Debug.Log("=== 测试筹码选择 ===");
            var availableChips = BetDataManager.Instance.AvailableChips;
            
            for (int i = 0; i < chipButtons.Length; i++)
            {
                SelectChipByValue(availableChips[i]);
                Debug.Log($"测试选择筹码{i}，当前显示值：{CurrentSelectedChipValue}");
            }
        }

        #endregion
    }
}
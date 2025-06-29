// Assets/Scripts/Core/ChipSelectionManager.cs
// 筹码选择管理器 - 处理筹码选中效果和数据管理
// 挂载到 ChipSelectionArea 节点上
// 创建时间: 2025/6/29

using UnityEngine;
using UnityEngine.UI;

namespace BaccaratGame.Core
{
    /// <summary>
    /// 筹码选择管理器
    /// 负责处理筹码按钮的选中效果、数据存储和显示更新
    /// </summary>
    public class ChipSelectionManager : MonoBehaviour
    {
        #region Inspector 配置

        [Header("筹码按钮引用")]
        [SerializeField] private Button[] chipButtons = new Button[5];
        

        
        [Header("筹码数值配置")]
        [SerializeField] private int[] chipValues = { 1, 5, 10, 20, 50 };
        
        [Header("选中效果配置")]
        [SerializeField] private Color selectedBorderColor = new Color(0f, 1f, 0.6f, 1f); // 绿色边框
        [SerializeField] private float selectedScale = 1.1f; // 选中时的缩放比例
        [SerializeField] private float borderWidth = 3f; // 边框宽度

        #endregion

        #region 私有字段

        private int currentSelectedIndex = 0; // 当前选中的筹码索引
        private Outline[] chipOutlines; // 存储每个筹码的Outline组件

        #endregion

        #region 公共属性

        /// <summary>
        /// 获取当前选中的筹码值
        /// </summary>
        public int CurrentSelectedChipValue
        {
            get
            {
                if (currentSelectedIndex >= 0 && currentSelectedIndex < chipValues.Length)
                    return chipValues[currentSelectedIndex];
                return 0;
            }
        }

        /// <summary>
        /// 获取当前选中的筹码索引
        /// </summary>
        public int CurrentSelectedIndex => currentSelectedIndex;

        #endregion

        #region 生命周期

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            SetupChipButtons();
            SelectChipByIndex(0); // 默认选中第一个筹码
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

            if (chipValues == null || chipValues.Length != chipButtons.Length)
            {
                Debug.LogError("[ChipSelectionManager] 筹码数值配置与按钮数量不匹配！");
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

                Debug.Log($"[ChipSelectionManager] 设置筹码按钮{i}点击事件，对应值：{chipValues[i]}");
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

            SelectChipByIndex(chipIndex);
            Debug.Log($"[ChipSelectionManager] 玩家选择筹码{chipIndex}，值：{chipValues[chipIndex]}");
        }

        /// <summary>
        /// 按索引选择筹码
        /// </summary>
        /// <param name="chipIndex">要选择的筹码索引</param>
        public void SelectChipByIndex(int chipIndex)
        {
            if (chipIndex < 0 || chipIndex >= chipButtons.Length)
            {
                Debug.LogWarning($"[ChipSelectionManager] 选择筹码失败，无效索引：{chipIndex}");
                return;
            }

            // 如果已经选中同一个筹码，不需要重复操作
            if (currentSelectedIndex == chipIndex)
            {
                Debug.Log($"[ChipSelectionManager] 筹码{chipIndex}已经被选中");
                return;
            }

            // 取消之前选中的筹码效果
            ClearChipSelection(currentSelectedIndex);

            // 设置新选中的筹码
            currentSelectedIndex = chipIndex;
            ApplyChipSelection(currentSelectedIndex);

            // 更新显示
            UpdateChipDisplay();

            Debug.Log($"[ChipSelectionManager] 选中筹码{chipIndex}，值：{chipValues[chipIndex]}");
        }

        /// <summary>
        /// 按筹码值选择筹码
        /// </summary>
        /// <param name="chipValue">要选择的筹码值</param>
        public void SelectChipByValue(int chipValue)
        {
            for (int i = 0; i < chipValues.Length; i++)
            {
                if (chipValues[i] == chipValue)
                {
                    SelectChipByIndex(i);
                    return;
                }
            }

            Debug.LogWarning($"[ChipSelectionManager] 未找到值为{chipValue}的筹码");
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
            ClearChipSelection(currentSelectedIndex);
            currentSelectedIndex = -1;
            
            Debug.Log("[ChipSelectionManager] 清除所有筹码选择");
        }

        /// <summary>
        /// 重置为默认选择（第一个筹码）
        /// </summary>
        public void ResetToDefault()
        {
            SelectChipByIndex(0);
            Debug.Log("[ChipSelectionManager] 重置为默认选择");
        }

        /// <summary>
        /// 获取所有筹码值
        /// </summary>
        /// <returns>筹码值数组</returns>
        public int[] GetAllChipValues()
        {
            return (int[])chipValues.Clone();
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
            Debug.Log($"筹码数值数量: {(chipValues != null ? chipValues.Length : 0)}");
            
            if (chipButtons != null)
            {
                for (int i = 0; i < chipButtons.Length; i++)
                {
                    Debug.Log($"筹码按钮{i}: {(chipButtons[i] != null ? chipButtons[i].name : "未配置")}");
                }
            }

            if (chipValues != null)
            {
                Debug.Log($"筹码数值: [{string.Join(", ", chipValues)}]");
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
            for (int i = 0; i < chipButtons.Length; i++)
            {
                SelectChipByIndex(i);
                Debug.Log($"测试选择筹码{i}，当前显示值：{CurrentSelectedChipValue}");
            }
        }

        #endregion
    }
}
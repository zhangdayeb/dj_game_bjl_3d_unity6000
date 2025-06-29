using UnityEngine;
using UnityEngine.UI;

namespace BaccaratGame.Core
{
    /// <summary>
    /// 通用点击控制组件
    /// 点击按钮控制目标GameObject的显示/隐藏
    /// </summary>
    public class CommonClick : MonoBehaviour
    {
        [Header("组件引用")]
        [SerializeField] private Button targetButton;      // 目标按钮
        [SerializeField] private GameObject targetPanel;   // 被控制的面板

        private void Start()
        {
            // 绑定按钮点击事件
            if (targetButton != null)
            {
                targetButton.onClick.AddListener(OnButtonClick);
            }
            else
            {
                Debug.LogError("[CommonClick] 按钮组件未分配！");
            }
        }

        private void OnDestroy()
        {
            // 解绑事件
            if (targetButton != null)
            {
                targetButton.onClick.RemoveListener(OnButtonClick);
            }
        }

        /// <summary>
        /// 按钮点击事件
        /// </summary>
        private void OnButtonClick()
        {
            if (targetPanel != null)
            {
                // 切换显示状态
                bool isActive = targetPanel.activeSelf;
                targetPanel.SetActive(!isActive);

                Debug.Log($"[CommonClick] 面板 {targetPanel.name} 状态切换为: {!isActive}");
            }
            else
            {
                Debug.LogError("[CommonClick] 目标面板未分配！");
            }
        }

        /// <summary>
        /// 公共方法：显示面板
        /// </summary>
        public void ShowPanel()
        {
            if (targetPanel != null)
            {
                targetPanel.SetActive(true);
            }
        }

        /// <summary>
        /// 公共方法：隐藏面板
        /// </summary>
        public void HidePanel()
        {
            if (targetPanel != null)
            {
                targetPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 公共方法：切换面板状态
        /// </summary>
        public void TogglePanel()
        {
            if (targetPanel != null)
            {
                targetPanel.SetActive(!targetPanel.activeSelf);
            }
        }
    }
}
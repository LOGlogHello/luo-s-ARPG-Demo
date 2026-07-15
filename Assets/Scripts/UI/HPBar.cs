using Luo.Character;
using Luo.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Luo.UI
{
    /// <summary>
    /// HP 面板 UI 组件
    /// 监听 HealthChangedEvent，自动更新血量条和文本
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class HPBar : MonoBehaviour
    {
        [Header("绑定目标")]
        [Tooltip("要显示哪个角色的血量信息")]
        public CharacterUnit targetCharacter;

        [Header("UI 组件引用（自动查找）")]
        [Tooltip("血量条 Slider（UI 组件）")]
        public Slider hpSlider; // 如果未拖拽，会在 Start 中尝试自动查找

        [Tooltip("血量文本（TextMeshPro）")]
        public TextMeshProUGUI hpText; // 如果未拖拽，会在 Start 中尝试自动查找

        [Header("UI 样式")]
        [Tooltip("血量文本格式，{0}=当前血量，{1}=最大血量")]
        public string textFormat = "{0}/{1}";

        private void Start()
        {
            // 自动查找 UI 组件（如果未手动拖拽）
            if (hpSlider == null)
                hpSlider = GetComponentInChildren<Slider>();

            if (hpText == null)
                hpText = GetComponentInChildren<TextMeshProUGUI>();

            // 如果目标角色已设置，立即刷新一次
            if (targetCharacter != null)
                UpdateUI(targetCharacter.CurrentHealth, targetCharacter.MaxHealth);
        }

        private void OnEnable()
        {
            // 订阅血量变化事件
            EventManager.AddListener<HealthChangedEvent>(OnHealthChanged);
        }

        private void OnDisable()
        {
            // 取消订阅，避免内存泄漏
            EventManager.RemoveListener<HealthChangedEvent>(OnHealthChanged);
        }

        /// <summary>
        /// 血量变化事件回调
        /// </summary>
        private void OnHealthChanged(HealthChangedEvent evt)
        {
            // 只处理绑定的目标角色
            if (evt.Character != targetCharacter)
                return;

            UpdateUI(evt.CurrentHealth, evt.MaxHealth);
        }

        /// <summary>
        /// 更新 UI 显示
        /// </summary>
        private void UpdateUI(float currentHealth, float maxHealth)
        {
            if (hpSlider != null)
            {
                // 归一化值（0~1）
                float normalizedValue = maxHealth > 0 ? currentHealth / maxHealth : 0f;
                hpSlider.value = normalizedValue;
            }

            if (hpText != null)
            {
                // 格式："89/100"
                hpText.text = string.Format(textFormat, currentHealth, maxHealth);
            }
        }

        /// <summary>
        /// 外部调用：设置目标角色并立即刷新
        /// </summary>
        public void SetTarget(CharacterUnit character)
        {
            targetCharacter = character;
            if (character != null)
                UpdateUI(character.CurrentHealth, character.MaxHealth);
        }

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器辅助：自动绑定 UI 组件
        /// </summary>
        private void Reset()
        {
            hpSlider = GetComponentInChildren<Slider>();
            hpText = GetComponentInChildren<TextMeshProUGUI>();
        }
#endif
    }
}
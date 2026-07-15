using Luo.Character;
using Luo.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Luo.UI
{
    /// <summary>
    /// 敌人头顶血条（屏幕空间 UI）
    /// 将敌人世界坐标投影到屏幕坐标，始终面向屏幕
    /// </summary>
    public class EnemyHPBar_Screen : MonoBehaviour
    {
        [Header("绑定目标")]
        public CharacterUnit targetCharacter;

        [Header("UI 组件")]
        public Slider hpSlider;
        public TextMeshProUGUI hpText;
        public RectTransform rectTransform;

        [Header("偏移（像素）")]
        public Vector2 screenOffset = new Vector2(0f, 50f);

        [Header("超出屏幕时隐藏")]
        public bool hideWhenOffscreen = true;

        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;
            rectTransform = GetComponent<RectTransform>();

            if (hpSlider == null)
                hpSlider = GetComponentInChildren<Slider>();
            if (hpText == null)
                hpText = GetComponentInChildren<TextMeshProUGUI>();

            if (targetCharacter != null)
                UpdateUI(targetCharacter.CurrentHealth, targetCharacter.MaxHealth);
        }

        private void Update()
        {
            if (targetCharacter == null || _mainCamera == null)
                return;

            // 将世界坐标转换为屏幕坐标
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(
                targetCharacter.transform.position + Vector3.up * 2f // 头顶偏移
            );

            // 判断是否在屏幕外
            bool isOffscreen = screenPos.x < 0 || screenPos.x > Screen.width ||
                               screenPos.y < 0 || screenPos.y > Screen.height ||
                               screenPos.z < 0; // 在摄像机后面

            if (hideWhenOffscreen && isOffscreen)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            // 设置 UI 位置
            rectTransform.position = screenPos + (Vector3)screenOffset;
        }

        private void OnEnable()
        {
            EventManager.AddListener<HealthChangedEvent>(OnHealthChanged);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<HealthChangedEvent>(OnHealthChanged);
        }

        private void OnHealthChanged(HealthChangedEvent evt)
        {
            if (evt.Character != targetCharacter)
                return;
            UpdateUI(evt.CurrentHealth, evt.MaxHealth);
        }

        private void UpdateUI(float currentHealth, float maxHealth)
        {
            if (hpSlider != null)
                hpSlider.value = maxHealth > 0 ? currentHealth / maxHealth : 0f;

            if (hpText != null)
                hpText.text = $"{currentHealth}/{maxHealth}";
        }

        public void SetTarget(CharacterUnit character)
        {
            targetCharacter = character;
            if (character != null)
                UpdateUI(character.CurrentHealth, character.MaxHealth);
        }
    }
}
using Branded.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.UI
{
    // HUD health bar. A pale trail shows the chunk just lost, then catches up (same feel as the enemy bars).
    public class PlayerHealthBar : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("health")] HealthComponent _healthComponent; // empty = the Player-tagged object
        [SerializeField, FormerlySerializedAs("fill")] RectTransform _fillComponent;       // stretched; its right anchor follows the value
        [SerializeField, FormerlySerializedAs("trail")] RectTransform _trailComponent;
        [SerializeField, FormerlySerializedAs("label")] TMP_Text _labelComponent;
        [SerializeField, FormerlySerializedAs("trailDelay")] float _trailDelay = 0.35f;
        [SerializeField, FormerlySerializedAs("trailSpeed")] float _trailSpeed = 1f; // bar fraction per second

        float _value = 1f;
        float _trailValue = 1f;
        float _trailTimer;

        void Awake()
        {
            if (_healthComponent) return;
            var player = GameObject.FindWithTag("Player");
            if (player) _healthComponent = player.GetComponent<HealthComponent>();
        }

        void OnEnable()
        {
            if (!_healthComponent) return;
            _healthComponent.HealthChanged += OnHealthChanged;
            OnHealthChanged(_healthComponent.CurrentHealth, _healthComponent.MaxHealth); // catch up on anything missed while hidden
        }

        void OnDisable()
        {
            if (!_healthComponent) return;
            _healthComponent.HealthChanged -= OnHealthChanged;
        }

        void Start()
        {
            if (!_healthComponent) return;
            OnHealthChanged(_healthComponent.CurrentHealth, _healthComponent.MaxHealth);
            _trailValue = _value;
            SetBar(_trailComponent, _trailValue);
        }

        void OnHealthChanged(float current, float max)
        {
            _value = max > 0f ? current / max : 0f;
            _trailTimer = _trailDelay;
            SetBar(_fillComponent, _value);
            if (!_labelComponent) return;
            _labelComponent.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }

        void Update()
        {
            if (_trailValue <= _value) _trailValue = _value;
            else if (_trailTimer > 0f) _trailTimer -= Time.unscaledDeltaTime;
            else _trailValue = Mathf.MoveTowards(_trailValue, _value, _trailSpeed * Time.unscaledDeltaTime);
            SetBar(_trailComponent, _trailValue);
        }

        static void SetBar(RectTransform bar, float value) => bar.anchorMax = new Vector2(Mathf.Clamp01(value), 1f);
    }
}

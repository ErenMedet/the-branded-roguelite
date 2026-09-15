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

        BarTrail _trail;

        void Awake()
        {
            _trail = new BarTrail(_trailDelay, _trailSpeed);
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
            _trail.Snap();
            SetBar(_trailComponent, _trail.Value);
        }

        void OnHealthChanged(float current, float max)
        {
            float value = max > 0f ? current / max : 0f;
            _trail.SetTarget(value);
            SetBar(_fillComponent, value);
            if (!_labelComponent) return;
            _labelComponent.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }

        // Unscaled, so the trail keeps moving during hitstop.
        void Update() => SetBar(_trailComponent, _trail.Tick(Time.unscaledDeltaTime));

        static void SetBar(RectTransform bar, float value) => bar.anchorMax = new Vector2(Mathf.Clamp01(value), 1f);
    }
}
